using System;
using System.Collections.Generic;
using UnityEngine;
using HexBuilder.Systems.Resources;

namespace HexBuilder.Systems.Workers
{
    public class WorkerManager : MonoBehaviour
    {
        public static WorkerManager Instance { get; private set; }

        [Header("Refs")]
        public ResourceInventory inventory;

        [Header("Targets & weights")]
        public int targetWood = 120;
        public int targetStone = 90;
        public int targetWater = 60;

        [Tooltip("Vplyv backlogu (pending jobov) na prioritu.")]
        public float backlogWeight = 0.5f;

        [Tooltip("Vyhladenie potrieb (0..1, vyööie = hladöie a pomalöie).")]
        [Range(0f, 1f)] public float emaSmoothing = 0.2f;

        [Header("Quotas & fairness")]
        [Tooltip("Hard limit na aktÌvnych workerov pre 1 resource.")]
        public int maxWorkersPerResource = 999;
        [Tooltip("Minim·lny poËet workerov pre resource (ak existuj˙ joby).")]
        public int minWorkersPerResource = 1;
        [Tooltip("Koæko nov˝ch claimov mÙûe dan˝ resource dostaù za tick (anti-stampede).")]
        public int maxNewClaimsPerResourcePerTick = 3;

        [Header("Stickiness / cooldown")]
        [Tooltip("Po claimnutÌ jobu na resource dostane worker cooldown (ticky), poËas ktorÈho preferuje rovnak˝ resource.")]
        public int workerStickinessTicks = 20;

    
        readonly Dictionary<string, float> emaNeed = new Dictionary<string, float> {
            { "wood", 0f }, { "stone", 0f }, { "water", 0f }
        };
        readonly Dictionary<string, int> activeWorkers = new Dictionary<string, int> {
            { "wood", 0 }, { "stone", 0 }, { "water", 0 }
        };
        readonly Dictionary<string, int> newClaimsThisTick = new Dictionary<string, int> {
            { "wood", 0 }, { "stone", 0 }, { "water", 0 }
        };

      
        readonly Dictionary<int, (string res, int untilTick)> stickyByWorkerId = new();
        int tickCounter = 0;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (!inventory) inventory = FindObjectOfType<ResourceInventory>();
        }

        void LateUpdate()
        {
       
            tickCounter++;
            newClaimsThisTick["wood"] = 0;
            newClaimsThisTick["stone"] = 0;
            newClaimsThisTick["water"] = 0;
        }

        // ---- Public API for WorkerAgent ----
        public PickupDeliverJob TryClaimJobFor(WorkerAgent w)
        {
            if (JobBoard.Instance == null) return null;

   
            var weights = ComputeWeights();

       
            var order = RankResourcesForWorker(w, weights);

        
            foreach (var res in order)
            {
                if (!QuotaAllows(res)) continue;

                var job = JobBoard.Instance.TryClaimJob(j => j.resourceId == res);
                if (job != null)
                {
                    activeWorkers[res]++;
                    newClaimsThisTick[res]++;
                  
                    stickyByWorkerId[w.GetInstanceID()] = (res, tickCounter + workerStickinessTicks);
                    return job;
                }
            }


            return null;
        }

        public void NotifyRelease(WorkerAgent w, string resId)
        {
            if (!string.IsNullOrEmpty(resId) && activeWorkers.ContainsKey(resId))
                activeWorkers[resId] = Mathf.Max(0, activeWorkers[resId] - 1);
        }


        Dictionary<string, float> ComputeWeights()
        {
            int w = inventory ? inventory.wood : 0;
            int s = inventory ? inventory.stone : 0;
            int wa = inventory ? inventory.water : 0;

        
            float needWood = Mathf.Max(0, targetWood - w);
            float needStone = Mathf.Max(0, targetStone - s);
            float needWater = Mathf.Max(0, targetWater - wa);

      
            int jbW = JobBoard.Instance?.CountPending("wood") ?? 0;
            int jbS = JobBoard.Instance?.CountPending("stone") ?? 0;
            int jbWa = JobBoard.Instance?.CountPending("water") ?? 0;

        
            float scoreW = needWood + backlogWeight * jbW;
            float scoreS = needStone + backlogWeight * jbS;
            float scoreWa = needWater + backlogWeight * jbWa;

        
            emaNeed["wood"] = Mathf.Lerp(scoreW, emaNeed["wood"], emaSmoothing);
            emaNeed["stone"] = Mathf.Lerp(scoreS, emaNeed["stone"], emaSmoothing);
            emaNeed["water"] = Mathf.Lerp(scoreWa, emaNeed["water"], emaSmoothing);

         
            float sum = emaNeed["wood"] + emaNeed["stone"] + emaNeed["water"];
            if (sum < 0.0001f) sum = 1f;

            return new Dictionary<string, float> {
                { "wood",  emaNeed["wood"]  / sum },
                { "stone", emaNeed["stone"] / sum },
                { "water", emaNeed["water"] / sum },
            };
        }

        List<string> RankResourcesForWorker(WorkerAgent w, Dictionary<string, float> weights)
        {
           
            var ordered = new List<(string id, float w)> {
                ("wood", weights["wood"]),
                ("stone", weights["stone"]),
                ("water", weights["water"])
            };
            ordered.Sort((a, b) => b.w.CompareTo(a.w));

            
            if (stickyByWorkerId.TryGetValue(w.GetInstanceID(), out var stick))
            {
                if (tickCounter <= stick.untilTick)
                {
                   
                    ordered.Sort((a, b) =>
                    {
                        if (a.id == stick.res && b.id != stick.res) return -1;
                        if (b.id == stick.res && a.id != stick.res) return 1;
                        return b.w.CompareTo(a.w);
                    });
                }
            }

            var resOnly = new List<string>();
            foreach (var x in ordered) resOnly.Add(x.id);
            return resOnly;
        }

        bool QuotaAllows(string resId)
        {
            int pending = JobBoard.Instance?.CountPending(resId) ?? 0;
            if (pending <= 0) return false; 

             if (newClaimsThisTick[resId] >= maxNewClaimsPerResourcePerTick)
                return false;

            
            if (activeWorkers[resId] >= maxWorkersPerResource)
                return false;

      
            return true;
        }
    }
}
