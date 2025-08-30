using System.Collections.Generic;
using UnityEngine;
using HexBuilder.Systems.Map;

namespace HexBuilder.Systems.Pathfinding
{
    public static class HexPathfinder
    {
       
        static bool IsWalkable(HexTile t, MapGenerationProfile profile)
        {
            if (t == null || t.terrain == null) return false;
            if (profile != null && profile.water != null && t.terrain == profile.water)
                return false;
            return true;
        }

        public static List<HexCoords> FindPath(HexCoords start, HexCoords goal, MapGenerationProfile profile)
        {
            if (start.Equals(goal))
                return new List<HexCoords> { start };

            var q = new Queue<HexCoords>();
            var cameFrom = new Dictionary<string, HexCoords>();
            var visited = new HashSet<string>();

            string Key(HexCoords c) => $"{c.q},{c.r}";

            q.Enqueue(start);
            visited.Add(Key(start));

            while (q.Count > 0)
            {
                var cur = q.Dequeue();
                if (cur.Equals(goal)) break;

                for (int d = 0; d < 6; d++)
                {
                    var n = cur.Neighbor(d);
                    var ntile = GetTile(n);
                    if (!IsWalkable(ntile, profile)) continue;

                    var key = Key(n);
                    if (visited.Contains(key)) continue;
                    visited.Add(key);
                    cameFrom[key] = cur;
                    q.Enqueue(n);
                }
            }

        
            var path = new List<HexCoords>();
            var gk = $"{goal.q},{goal.r}";
            if (!cameFrom.ContainsKey(gk))
            {
                return path;
            }

            var cur2 = goal;
            while (!cur2.Equals(start))
            {
                path.Add(cur2);
                cur2 = cameFrom[$"{cur2.q},{cur2.r}"];
            }
            path.Add(start);
            path.Reverse();
            return path;
        }

        static HexTile GetTile(HexCoords c)
        {
            if (HexMapGenerator.TileIndexByKey.TryGetValue($"{c.q},{c.r}", out var t1)) return t1;
            if (HexMapGenerator.TileIndex.TryGetValue(c, out var t2)) return t2;
            return null;
        }
    }
}
