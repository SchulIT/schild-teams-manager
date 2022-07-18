using NaturalSort.Extension;
using SchildTeamsManager.Extension;
using SchildTeamsManager.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SchildTeamsManager.Service.Teams
{
    public class DefaultTeamDataResolver : ITeamDataResolver
    {
        protected static string CollapsedGradeList(IEnumerable<Grade> grades, char delimiter)
        {
            var gradeNames = grades.Select(g => g.Name.WithoutStartingZero()).ToList();
            var sortedGradeNames = gradeNames.OrderBy(x => x, StringComparison.OrdinalIgnoreCase.WithNaturalSort()).ToList();
            var jgst = sortedGradeNames.Select(x => x.StartsWith("10") ? "10" : x[..1]).Distinct().ToArray();

            if(jgst.Length == 1)
            {
                var gradeSuffix = sortedGradeNames.Select(x => x.Substring(jgst.First().Length, 1)).ToArray();
                return $"{jgst.First()}{String.Join("", gradeSuffix)}";
            }

            return String.Join(delimiter, sortedGradeNames);
        }

        public virtual string ResolveAlias(Tuition tuition, short year)
        {
            if (tuition.SchildId != null)
            {
                return $"{tuition.Name}-{CollapsedGradeList(tuition.Grades, '-')}-{year}{(year % 100) + 1}".ToLower();
            }
            else
            {
                return $"{tuition.Subject}-{CollapsedGradeList(tuition.Grades, '-')}-{year}{(year % 100) + 1}".ToLower();
            }
        }

        public string ResolveDisplayName(Tuition tuition, short year)
        {
            if (tuition.SchildId != null)
            {
                return $"{CollapsedGradeList(tuition.Grades, '-')} {tuition.Name} ({year}/{(year % 100) + 1})";
            }
            else
            {
                return $"{CollapsedGradeList(tuition.Grades, '-')} {tuition.Subject} ({year}/{(year % 100) + 1})";
            }
        }

        public string ResolveAlias(Grade grade, short year)
        {
            return $"ordinariat-{grade.Name.WithoutStartingZero().ToLower()}-{year}{(year % 100) + 1}".ToLower();
        }

        public string ResolveDisplayName(Grade grade, short year)
        {
            return $"Ordinariat {grade.Name.WithoutStartingZero()} ({year}/{(year % 100) + 1})";
        }
    }
}
