using SchildTeamsManager.Model;

namespace SchildTeamsManager.Service.Teams
{
    public interface ITeamDataResolver
    {
        public string ResolveAlias(Tuition tuition, short year);

        public string ResolveDisplayName(Tuition tuition, short year);

        public string ResolveAlias(Grade grade, short year);

        public string ResolveDisplayName(Grade grade, short year);
    }
}
