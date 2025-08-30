using UnityEngine;
using HexBuilder.Systems.Map;

namespace HexBuilder.Systems.Buildings
{
    public class WellBehaviour : BuildingBehaviour
    {
        [Header("Production (per tick)")]
        public int baseWater = 0;
        public int waterPerAdjacentWater = 1;
        public int maxPerTick = 3;

        protected override void OnTick()
        {
            if (inventory == null || instance == null || profile == null) return;

            int adjWater = 0;
            var ns = GetNeighborsRing1();
            for (int i = 0; i < ns.Length; i++)
            {
                var t = ns[i];
                if (t != null && t.terrain != null)
                {
                    bool isWater = false;

                    if (profile.water && t.terrain == profile.water) isWater = true;

                    if (!isWater && profile.water != null)
                    {
                        var tt = t.terrain as TerrainType;
                        if (tt != null && !string.IsNullOrEmpty(tt.id) &&
                            !string.IsNullOrEmpty(profile.water.id) &&
                            tt.id == profile.water.id)
                        {
                            isWater = true;
                        }
                    }

                    if (!isWater && profile.water)
                    {
                        if (t.terrain.name == profile.water.name ||
                            t.terrain.name == profile.water.displayName)
                            isWater = true;
                    }

                    if (isWater) adjWater++;
                }
            }

            int amount = Mathf.Clamp(baseWater + adjWater * waterPerAdjacentWater, 0, maxPerTick);
            if (amount > 0)
                AddToOutput("water", amount);
        }
    }
}
