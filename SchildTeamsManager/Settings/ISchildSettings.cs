using System;

namespace SchildTeamsManager.Settings
{
    public interface ISchildSettings
    {
        bool OnlyVisibleEntities { get; set; }

        int[] StudentFilter { get; set; }

        string ConnectionString { get; set; }
    }
}
