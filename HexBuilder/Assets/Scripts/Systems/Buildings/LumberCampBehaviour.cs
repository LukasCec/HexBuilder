using UnityEngine;
using HexBuilder.Systems.Map;

namespace HexBuilder.Systems.Buildings
{
    public class LumberCampBehaviour : BuildingBehaviour
    {
        [Header("Production (per tick)")]
        public int baseWood = 0;
        public int woodPerAdjacentForest = 1;
        public int maxPerTick = 6;

        public bool debugNeighbors = true;

        protected override void OnTick()
        {
            if (inventory == null || instance == null || profile == null) return;

            int adjForest = 0;

            //if (debugNeighbors)
                //Debug.Log($"[Lumber] center {instance.coords.q},{instance.coords.r}");

            var ns = GetNeighborsRing1();
            for (int i = 0; i < ns.Length; i++)
            {
                var t = ns[i];
                string terrName = (t && t.terrain) ? t.terrain.name : "null";
                //if (debugNeighbors)
                    //Debug.Log($"  dir {i}: key={instance.coords.Neighbor(i).q},{instance.coords.Neighbor(i).r} -> {(t ? "HIT" : "MISS")}  terr={terrName}");

                if (t != null && t.terrain != null)
                {
                    bool isForest = false;

                   
                    if (profile.forest && t.terrain == profile.forest) isForest = true;

                   
                    if (!isForest && profile.forest != null)
                    {
                        var tt = t.terrain as TerrainType;         
                        if (tt != null && !string.IsNullOrEmpty(tt.id) &&
                            !string.IsNullOrEmpty(profile.forest.id) &&
                            tt.id == profile.forest.id)
                        {
                            isForest = true;
                        }
                    }

                   
                    if (!isForest && profile.forest)
                    {
                        if (t.terrain.name == profile.forest.name ||
                            t.terrain.name == profile.forest.displayName)
                            isForest = true;
                    }

                    if (isForest) adjForest++;
                }
            }

            int amount = Mathf.Clamp(baseWood + adjForest * woodPerAdjacentForest, 0, maxPerTick);
            if (amount > 0)
            {
                AddToOutput("wood", amount);
                //if (debugNeighbors) Debug.Log($"[Lumber] +{amount} wood (adjForest={adjForest})");
            }
        }
    }
}
