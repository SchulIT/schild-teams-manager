using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchildTeamsManager.Service.MicrosoftGraph
{
    public class MicrosoftGraph : IMicrosoftGraph
    {
        public bool IsAuthenticated { get; private set; } = false;

        public string TenantName { get; private set; } = string.Empty;

        public Dictionary<string, Model.User> Users { get; } = new Dictionary<string, Model.User>();

        #region Event

        public event AuthenticationStateChangedEventHandler? AuthenticationStateChanged;

        private void RaiseAuthenticationStateChanged(bool isAuthenticated)
        {
            AuthenticationStateChanged?.Invoke(this, new AuthenticationStateChangedEventArgs(isAuthenticated));
        }

        #endregion

        #region Services

        private IConfidentialClientApplication? app;
        private GraphServiceClient? graphClient;

        #endregion

        public async Task AuthenticateAsync(string tenantId, string clientId, string clientSecret)
        {
            RaiseAuthenticationStateChanged(false);

            app = ConfidentialClientApplicationBuilder
                    .Create(clientId)
                    .WithClientSecret(clientSecret)
                    .WithAuthority(new Uri("https://login.microsoftonline.com/" + tenantId))
                    .Build();

            var result = await app.AcquireTokenForClient(new string[] { "https://graph.microsoft.com/.default" })
                .ExecuteAsync();

            graphClient = new GraphServiceClient(new DelegateAuthenticationProvider(requestMessage =>
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.AccessToken);
                return Task.CompletedTask;
            }));

            var org = await graphClient.Organization.Request().GetAsync();
            if (org.Any())
            {
                var tenant = org.First();
                TenantName = tenant.DisplayName;
            }

            IsAuthenticated = true;
            RaiseAuthenticationStateChanged(true);
        }

        private Task<List<string>> GetUserIds(IEnumerable<string> emailAddresses)
        {
            return Task.Run(() =>
            {
                var userIds = new List<string>();

                foreach(var email in emailAddresses)
                {
                    if(Users.ContainsKey(email))
                    {
                        userIds.Add(Users[email].Id);
                    }
                }

                return userIds;
            });
        }

        public async Task<Model.Team> CreateTeamAsync(string displayName, string alias, IEnumerable<string> owners, IEnumerable<string> members, string template)
        {
            if (!IsAuthenticated || graphClient == null)
            {
                throw new NotAuthenticatedException();
            }

            // Test if group exists
            var groups = await graphClient.Groups
                .Request()
                .Filter($"mailNickname eq '{alias}'")
                .Top(1)
                .GetAsync();

            var group = groups.FirstOrDefault();

            

            if (group == null)
            {
                var educationClass = new EducationClass
                {
                    DisplayName = displayName,
                    MailNickname = alias
                };

                var createdClass = await graphClient.Education.Classes
                    .Request()
                    .AddAsync(educationClass);

                await Task.Delay(30 * 1000);

                group = await graphClient.Groups[createdClass.Id]
                    .Request()
                    .GetAsync();
            }

            var ownersIds = await GetUserIds(owners);
            var membersIds = await GetUserIds(members);

            if (ownersIds.Any()) {
                var tasks = new List<Task>();
                foreach(var owner in ownersIds)
                {
                    var directoryObject = new DirectoryObject
                    {
                        Id = owner
                    };
                    tasks.Add(graphClient.Groups[group.Id].Owners.References.Request().AddAsync(directoryObject));
                }

                try
                {
                    await Task.WhenAll(tasks);
                }
                catch { }
            }

            if (membersIds.Any()) {
                var tasks = new List<Task>();
                foreach (var member in membersIds)
                {
                    var directoryObject = new DirectoryObject
                    {
                        Id = member
                    };
                    tasks.Add(graphClient.Groups[group.Id].Members.References.Request().AddAsync(directoryObject));
                }

                try
                {
                    await Task.WhenAll(tasks);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Write(ex.Message);
                }
            }

            // Team anlegen, falls noch nicht vorhanden
            try
            {
                var existingTeam = await graphClient.Teams[group.Id]
                    .Request()
                    .GetAsync();

                return ToTeam(group);
            }
            catch
            {
            }

            await Task.Delay(10 * 1000);


            // Create Team
            var team = new Team
            {
                AdditionalData = new Dictionary<string, object>()
                    {
                        {"template@odata.bind", "https://graph.microsoft.com/v1.0/teamsTemplates('educationClass')"},
                        {"group@odata.bind", $"https://graph.microsoft.com/v1.0/groups('{group.Id}')"}
                    }
            };

            var result = await graphClient.Teams
                .Request()
                .AddAsync(team);

            return ToTeam(group);
        }

        public async Task<Model.Team> GetTeamMembersAsync(Model.Team team)
        {
            if (!IsAuthenticated || graphClient == null)
            {
                throw new NotAuthenticatedException();
            }

            // Query owners
            var owners = await graphClient.Groups[team.GroupId]
                .Owners
                .Request()
                .GetAsync();

            while (owners.Any())
            {
                foreach (var owner in owners)
                {
                    if (Users.ContainsKey(owner.Id))
                    {
                        team.Owners.Add(Users[owner.Id]);
                    }
                }

                if (owners.NextPageRequest == null)
                {
                    break;
                }

                owners = await owners.NextPageRequest.GetAsync();
            }

            // Query members
            var members = await graphClient.Groups[team.GroupId]
                .Members
                .Request()
                .GetAsync();

            while (members.Any())
            {
                foreach (var member in members)
                {
                    if (Users.ContainsKey(member.Id))
                    {
                        team.Members.Add(Users[member.Id]);
                    }
                }

                if (members.NextPageRequest == null)
                {
                    break;
                }

                members = await members.NextPageRequest.GetAsync();
            }

            return team;
        }

        public async Task<Model.Team?> GetTeamAsync(string alias)
        {
            if (!IsAuthenticated || graphClient == null)
            {
                throw new NotAuthenticatedException();
            }

            var result = await graphClient.Groups.Request()
                .Filter($"mailNickname eq '{alias}') and resourceProvisioningOptions/Any(x:x eq 'Team')")
                .GetAsync();

            if(!result.Any())
            {
                return null;
            }

            var team = ToTeam(result.First());

            return await GetTeamMembersAsync(team);
        }

        public async Task<List<Model.Team>> GetGradeTeamsAsync(short year)
        {
            if(!IsAuthenticated || graphClient == null)
            {
                return new List<Model.Team>();
            }

            var teams = new List<Model.Team>();

            var result = await graphClient.Groups.Request()
                .Filter($"startsWith(mailNickname, 'ordinariat') and resourceProvisioningOptions/Any(x:x eq 'Team')")
                .GetAsync();

            while (result.Any())
            {
                teams.AddRange(result.Select(x => ToTeam(x)));

                if (result.NextPageRequest == null)
                {
                    break;
                }

                result = await result.NextPageRequest.GetAsync();
            }

            return teams.Where(x => x.EmailAddress.EndsWith(year + "" + (year % 100 + 1))).ToList();
        }

        public async Task<List<Model.Team>> GetTeamsAsync()
        {
            if(!IsAuthenticated || graphClient == null)
            {
                return new List<Model.Team>();
            }

            List<Model.Team> teams = new List<Model.Team>();

            var result = await graphClient.Groups.Request().Filter("resourceProvisioningOptions/Any(x:x eq 'Team')").GetAsync();
            while(result.Any())
            {
                teams.AddRange(result.Select(x => new Model.Team {
                    DisplayName = x.DisplayName, 
                    GroupId = x.Id, 
                    EmailAddress = x.MailNickname, 
                    IsArchived = x.IsArchived != null && x.IsArchived.Value
                }));

                if(result.NextPageRequest == null)
                {
                    break;
                }

                result = await result.NextPageRequest.GetAsync();
            }

            return teams;
        }

        public async Task<List<Model.Team>> GetTeamsAsync(short year)
        {
            var teams = await GetTeamsAsync();
            return teams.Where(x => x.EmailAddress.EndsWith($"{year}{(year % 100) + 1}")).ToList();
        }

        public Task LogoutAsync()
        {
            try
            {
                app = null;
                graphClient = null;
                Users.Clear();

                return Task.CompletedTask;
            }
            finally
            {
                TenantName = string.Empty;
                IsAuthenticated = false;
                RaiseAuthenticationStateChanged(false);
            }
        }

        private static Model.Team ToTeam(Group group)
        {
            return new Model.Team
            {
                DisplayName = group.DisplayName,
                GroupId = group.Id,
                EmailAddress = group.MailNickname,
                IsArchived = group.IsArchived != null && group.IsArchived.Value
            };
        }

        public async Task LoadUsersAsync()
        {
            if (!IsAuthenticated || graphClient == null)
            {
                return;
            }

            var result = await graphClient.Users.Request().GetAsync();

            Users.Clear();

            while (result.Any())
            {
                foreach (var user in result)
                {
                    if (!Users.ContainsKey(user.UserPrincipalName))
                    {
                        Users.Add(user.UserPrincipalName, new Model.User
                        {
                            FirstName = user.GivenName,
                            LastName = user.Surname,
                            Id = user.Id,
                            Email = user.UserPrincipalName
                        });
                    }
                }

                if (result.NextPageRequest == null)
                {
                    break;
                }

                result = await result.NextPageRequest.GetAsync();
            }
        }

        public async Task<Dictionary<string, bool?>> GetTeamsArchiveStatus(IEnumerable<Model.Team> teams)
        {
            if (!IsAuthenticated || graphClient == null)
            {
                throw new NotAuthenticatedException();
            }

            var batchRequestContent = new BatchRequestContent();
            var requestIds = new Dictionary<string, Model.Team>();

            foreach(var team in teams)
            {
                var requestId = batchRequestContent.AddBatchRequestStep(graphClient.Teams[team.GroupId].Request());
                requestIds.Add(requestId, team);
            }

            var batchResponse = await graphClient.Batch.Request().PostAsync(batchRequestContent).ConfigureAwait(false);
            var result = new Dictionary<string, bool?>();

            foreach(var kv in requestIds)
            {
                var response = await batchResponse.GetResponseByIdAsync<Team>(kv.Key).ConfigureAwait(false);
                result.Add(kv.Value.GroupId, response.IsArchived);
            }

            return result;
        }

        public Task ArchiveTeam(Model.Team team)
        {
            if (!IsAuthenticated || graphClient == null)
            {
                throw new NotAuthenticatedException();
            }

            return graphClient.Teams[team.GroupId].Archive().Request().PostAsync();
        }

        public Task UnarchiveTeam(Model.Team team)
        {
            if (!IsAuthenticated || graphClient == null)
            {
                throw new NotAuthenticatedException();
            }

            return graphClient.Teams[team.GroupId].Unarchive().Request().PostAsync();
        }
    }
}
