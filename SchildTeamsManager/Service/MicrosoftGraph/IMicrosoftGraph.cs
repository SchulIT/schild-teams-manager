using SchildTeamsManager.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SchildTeamsManager.Service.MicrosoftGraph
{
    public interface IMicrosoftGraph
    {
        event AuthenticationStateChangedEventHandler AuthenticationStateChanged;

        bool IsAuthenticated { get; }

        string TenantName { get; }

        Dictionary<string, User> Users { get; }

        Task AuthenticateAsync(string tenantId, string clientId, string clientSecret);

        Task LogoutAsync();

        public Task LoadUsersAsync();

        public Task<Team?> GetTeamAsync(string alias);

        public Task<List<Team>> GetGradeTeamsAsync(int year);

        public Task<List<Team>> GetTeamsAsync(int year);

        public Task ArchiveTeam(Team team);

        public Task UnarchiveTeam(Team team);

        public Task RemoveTeamAsync(Team team);

        public Task<Dictionary<string, bool?>> GetTeamsArchiveStatus(IEnumerable<Team> teams);

        public Task<Team> CreateTeamAsync(string displayName, string alias, IEnumerable<string> owners, IEnumerable<string> members, string template);
    }
}
