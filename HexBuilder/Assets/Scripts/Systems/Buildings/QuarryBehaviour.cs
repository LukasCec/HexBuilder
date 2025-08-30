using UnityEngine;
using HexBuilder.Systems.Map;

namespace HexBuilder.Systems.Buildings
{
    public class QuarryBehaviour : BuildingBehaviour
    {
        [Header("Production (per tick)")]
        public int baseStone = 0;
        public int stonePerAdjacentRock = 1;
        public int maxPerTick = 4;

        protected override void OnTick()
        {
            if (inventory == null || instance == null || profile == null) return;

            int adjStone = 0;
            var ns = GetNeighborsRing1();
            for (int i = 0; i < ns.Length; i++)
            {
                var t = ns[i];
                if (t != null && t.terrain != null)
                {
                    bool isStone = false;

                    if (profile.stone && t.terrain == profile.stone) isStone = true;

                    if (!isStone && profile.stone != null)
                    {
                        var tt = t.terrain as TerrainType;
                        if (tt != null && !string.IsNullOrEmpty(tt.id) &&
                            !string.IsNullOrEmpty(profile.stone.id) &&
                            tt.id == profile.stone.id)
                        {
                            isStone = true;
                        }
                    }

                    if (!isStone && profile.stone)
                    {
                        if (t.terrain.name == profile.stone.name ||
                            t.terrain.name == profile.stone.displayName)
                            isStone = true;
                    }

                    if (isStone) adjStone++;
                }
            }

            int amount = Mathf.Clamp(baseStone + adjStone * stonePerAdjacentRock, 0, maxPerTick);
            if (amount > 0)
                AddToOutput("stone", amount);
        }
    }
}
