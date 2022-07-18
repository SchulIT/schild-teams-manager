using SchulIT.SchildExport.Models;

namespace SchildTeamsManager.Service.Schild
{
    public class DefaultNameResolver : INameResolver
    {
        public string ResolveName(Tuition tuition)
        {
            if (tuition.StudyGroupRef.Id == null) // Klassenunterricht
            {
                return $"{tuition.SubjectRef.Abbreviation}-{tuition.StudyGroupRef.Name}";
            }

            return tuition.StudyGroupRef.Name;
        }
    }
}
