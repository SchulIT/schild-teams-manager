using SchulIT.SchildExport.Models;

namespace SchildTeamsManager.Service.Schild
{
    public interface INameResolver
    {
        public string ResolveName(Tuition tuition);
    }
}
