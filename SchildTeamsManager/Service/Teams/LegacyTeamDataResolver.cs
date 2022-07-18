using SchildTeamsManager.Model;
using System.Collections.Generic;
using System.Linq;

namespace SchildTeamsManager.Service.Teams
{
    public class LegacyTeamDataResolver : DefaultTeamDataResolver
    {
        private static readonly string[] sekundarstufeII = new[] { "EF", "Q1", "Q2" };

        protected static bool isSekII(IEnumerable<Grade> grades)
        {
            if(!grades.Any())
            {
                return false;
            }

            return sekundarstufeII.Contains(grades.First().Name);
        }

        public override string ResolveAlias(Tuition tuition, short year)
        {
            if (isSekII(tuition.Grades))
            {
                return $"{tuition.Name}-{CollapsedGradeList(tuition.Grades, '-')}-{year}{(year % 100) + 1}".ToLower();
            }
            else if (tuition.SchildId != null)
            {
                return $"{tuition.Subject}-{CollapsedGradeList(tuition.Grades, '-')}-{year}{(year % 100) + 1}".ToLower();
            }
            else
            {
                return $"{tuition.Subject}-{CollapsedGradeList(tuition.Grades, '-')}-{year}{(year % 100) + 1}".ToLower();
            }
        }

    }
}
