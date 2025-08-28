using System.Collections.Generic;
using UnityEngine;

namespace HexBuilder.Systems.Buildings
{
    [CreateAssetMenu(menuName = "HexBuilder/Building Type Registry", fileName = "BuildingTypeRegistry")]
    public class BuildingTypeRegistry : ScriptableObject
    {
        [System.Serializable]
        public class Entry
        {
            public string id;             
            public BuildingType type;    
        }

        public List<Entry> entries = new List<Entry>();
        Dictionary<string, BuildingType> cache;

        void OnEnable()
        {
            BuildCache();
        }

        void BuildCache()
        {
            cache = new Dictionary<string, BuildingType>();
            foreach (var e in entries)
            {
                if (!string.IsNullOrWhiteSpace(e.id) && e.type != null)
                    cache[e.id] = e.type;
            }
        }

        public BuildingType GetById(string id)
        {
            if (cache == null) BuildCache();
            return (id != null && cache.TryGetValue(id, out var t)) ? t : null;
        }
    }
}
