using System;
using System.Collections.Generic;

namespace HexBuilder.Systems.Save
{
    [Serializable]
    public class SaveResources
    {
        public int wood;
        public int stone;
        public int water;
    }

    [Serializable]
    public class SaveBuilding
    {
        public string typeId; 
        public int q;
        public int r;
        public float yaw;
    }

    [Serializable]
    public class SaveGame
    {
        public long savedAtUnix;
        public int seed;
        public SaveResources resources = new SaveResources();
        public List<SaveBuilding> buildings = new List<SaveBuilding>();
        public SaveDayNight dayNight = new SaveDayNight();
    }

    [System.Serializable]
    public class SaveDayNight
    {
        public float timeOfDay; 
    }

    
}
