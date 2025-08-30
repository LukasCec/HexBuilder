using UnityEngine;
using HexBuilder.Systems.Resources;
using System.Collections.Generic;
using HexBuilder.Systems.Map;
using HexBuilder.Systems.Workers;


namespace HexBuilder.Systems.Buildings
{
    public class WarehouseBehaviour : BuildingBehaviour
    {
        [Header("Cap bonuses (while active)")]
        public int addMaxWood = 50;
        public int addMaxStone = 50;
        public int addMaxWater = 0;

        [Header("Workers mode")]
        public bool useWorkers = true;
        [Tooltip("Max resource units to generate as jobs per tick (sum).")]
        public int jobsPerTick = 10;

        [Header("Logistics")]
        [Tooltip("Koæko hexov od skladu vie obsl˙ûiù.")]
        public int serviceRadius = 3;
        [Tooltip("Max. poËet jednotiek (spolu) ktor˝ stiahne za 1 tick.")]
        public int pullPerTick = 8;

        bool applied = false;

        protected override void OnTick()
        {
            
            if (useWorkers)
            {
                PostJobsFromNeighborhood();
            }
            else
            {
                if (!inventory || instance == null || instance.tile == null) return;
                int budget = Mathf.Max(0, pullPerTick);
                if (budget == 0) return;

                var tiles = GetTilesInRange(instance.tile.coords, serviceRadius);
                foreach (var t in tiles)
                {
                    if (t == null || t.occupant == null) continue;
                    var beh = t.occupant.GetComponent<BuildingBehaviour>();
                    if (beh == null || beh == this) continue;

                    budget -= PullFrom(beh, "wood", budget);
                    if (budget <= 0) break;
                    budget -= PullFrom(beh, "stone", budget);
                    if (budget <= 0) break;
                    budget -= PullFrom(beh, "water", budget);
                    if (budget <= 0) break;
                }
            }

           
        }

        void PostJobsFromNeighborhood()
        {
            if (JobBoard.Instance == null) return;

            int budget = Mathf.Max(0, jobsPerTick);
            if (budget == 0) return;

            var tiles = GetTilesInRange(instance.tile.coords, serviceRadius);
            foreach (var t in tiles)
            {
                if (t == null || t.occupant == null) continue;
                var beh = t.occupant.GetComponent<BuildingBehaviour>();
                if (beh == null || beh == this) continue;

                // poradie: wood -> stone -> water
                budget -= PostJobsFor(beh, "wood", budget);
                if (budget <= 0) break;
                budget -= PostJobsFor(beh, "stone", budget);
                if (budget <= 0) break;
                budget -= PostJobsFor(beh, "water", budget);
                if (budget <= 0) break;
            }
        }

        int PostJobsFor(BuildingBehaviour producer, string id, int budget)
        {
            int avail = producer.GetBufferAmount(id);
            if (avail <= 0 || budget <= 0) return 0;

            int toPost = Mathf.Min(avail, budget);
            var job = new HexBuilder.Systems.Workers.PickupDeliverJob
            {
                resourceId = id,
                amount = toPost,
                source = producer,
                dest = this,
                status = HexBuilder.Systems.Workers.JobStatus.Pending
            };
            HexBuilder.Systems.Workers.JobBoard.Instance.PostOrUpdateJob(job);
            return toPost;
        }

        int PullFrom(BuildingBehaviour producer, string id, int budget)
        {
            if (budget <= 0) return 0;
            int avail = producer.GetBufferAmount(id);
            if (avail <= 0) return 0;

            int toTake = Mathf.Min(avail, budget);
            int got = producer.TakeFromOutput(id, toTake);
            if (got > 0)
            {
                inventory.Add(id, got); 
            }
            return got;
        }

        void Apply()
        {
            if (applied) return;
            if (inventory == null) inventory = FindObjectOfType<ResourceInventory>();
            if (inventory == null) return;

            inventory.AddCaps(addMaxWood, addMaxStone, addMaxWater);
            applied = true;
        }

        void Remove()
        {
            if (!applied) return;
            if (inventory == null) inventory = FindObjectOfType<ResourceInventory>();
            if (inventory == null) { applied = false; return; }

            inventory.AddCaps(-addMaxWood, -addMaxStone, -addMaxWater);
            applied = false;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Apply();
        }

        protected override void OnDisable()
        {
            Remove();
            base.OnDisable();
        }

        protected override void OnDestroy()
        {
            Remove();
            base.OnDestroy();
        }

        List<HexTile> GetTilesInRange(HexCoords center, int range)
        {
            var result = new List<HexTile>();
            var visited = new HashSet<string>();
            var queue = new Queue<(HexCoords c, int d)>();

            queue.Enqueue((center, 0));
            visited.Add($"{center.q},{center.r}");

            while (queue.Count > 0)
            {
                var (c, d) = queue.Dequeue();
                var tile = GetTileAt(c);
                result.Add(tile);

                if (d >= range) continue;

                for (int dir = 0; dir < 6; dir++)
                {
                    var n = c.Neighbor(dir);
                    string key = $"{n.q},{n.r}";
                    if (visited.Contains(key)) continue;
                    visited.Add(key);
                    queue.Enqueue((n, d + 1));
                }
            }

            return result;
        }
    }
}
