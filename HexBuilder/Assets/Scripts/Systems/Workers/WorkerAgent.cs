// Assets/Scripts/Systems/Workers/WorkerAgent.cs
using System.Collections.Generic;
using UnityEngine;
using HexBuilder.Systems.Map;
using HexBuilder.Systems.Buildings;
using HexBuilder.Systems.Pathfinding;
using HexBuilder.Systems.Workers;
using HexBuilder.Systems.Resources;

namespace HexBuilder.Systems.Workers
{
    public class WorkerAgent : MonoBehaviour
    {
        public string workerName = "Worker";
        public int carryCapacity = 6;
        public float moveSpeed = 3.0f; 
        public float arriveThreshold = 0.05f;

       
        public enum State { Idle, ToSource, Loading, ToDest, Unloading }
        public State CurrentState { get; private set; } = State.Idle;

       
        public string carryingId = null;
        public int carryingAmount = 0;

       
        PickupDeliverJob job;

      
        List<HexCoords> path = new List<HexCoords>();
        int pathIndex = 0;

       
        MapGenerationProfile profile;

        void Start()
        {
            var gen = FindObjectOfType<HexMapGenerator>();
            if (gen) profile = gen.profile;
        }

        void Update()
        {
            switch (CurrentState)
            {
                case State.Idle:
                    TryClaimJob();
                    break;

                case State.ToSource:
                    StepAlongPath();
                    if (ReachedTarget(job?.source?.GetCoords() ?? default)) EnterLoading();
                    break;

                case State.Loading:
                    DoLoad();
                    break;

                case State.ToDest:
                    StepAlongPath();
                    if (ReachedTarget(job?.dest?.GetCoords() ?? default)) EnterUnloading();
                    break;

                case State.Unloading:
                    DoUnload();
                    break;
            }
        }



        
        void TryClaimJob()
        {
            if (WorkerManager.Instance == null) return;
            var j = WorkerManager.Instance.TryClaimJobFor(this);
            if (j == null) return;

            job = j;
            carryingId = null;
            carryingAmount = 0;

            if (!PlanPathTo(job.source)) { AbortJob(); return; }
            CurrentState = State.ToSource;
        }

        void EnterLoading()
        {
            CurrentState = State.Loading;
        }

        void DoLoad()
        {
            if (job == null || job.source == null) { AbortJob(); return; }

            int canTake = Mathf.Min(carryCapacity, job.amount);
            int got = job.source.TakeFromOutput(job.resourceId, canTake);
            if (got <= 0)
            {
                // nič nie je – job už nemá zmysel
                CompleteOrInvalidate();
                return;
            }

            carryingId = job.resourceId;
            carryingAmount = got;
            job.amount -= got;

            if (!PlanPathTo(job.dest)) { AbortJob(); return; }
            CurrentState = State.ToDest;
        }

        void EnterUnloading()
        {
            CurrentState = State.Unloading;
        }

        void DoUnload()
        {
            if (job == null || job.dest == null) { AbortJob(); return; }
            if (string.IsNullOrEmpty(carryingId) || carryingAmount <= 0) { CompleteOrInvalidate(); return; }

           
            var inv = job.dest.GetComponentInParent<ResourceInventory>();
            if (!inv) inv = FindObjectOfType<ResourceInventory>();
            if (!inv) { AbortJob(); return; }

            inv.Add(carryingId, carryingAmount);
            carryingId = null; carryingAmount = 0;

            if (WorkerManager.Instance != null && job != null)
                WorkerManager.Instance.NotifyRelease(this, job.resourceId);

            if (job.amount > 0)
            {
               
                if (!PlanPathTo(job.source)) { AbortJob(); return; }
                CurrentState = State.ToSource;
            }
            else
            {
                JobBoard.Instance.CompleteJob(job);
                job = null;
                CurrentState = State.Idle;
            }
        }

        void CompleteOrInvalidate()
        {
            if (job != null && WorkerManager.Instance != null)
                WorkerManager.Instance.NotifyRelease(this, job.resourceId);

            if (job == null) { CurrentState = State.Idle; return; }
            if (job.amount <= 0) JobBoard.Instance.CompleteJob(job);
            else JobBoard.Instance.InvalidateJob(job);
            job = null; CurrentState = State.Idle;
        }


        void AbortJob()
        {
            if (job != null)
            {
                if (JobBoard.Instance != null) JobBoard.Instance.InvalidateJob(job);
                if (WorkerManager.Instance != null) WorkerManager.Instance.NotifyRelease(this, job.resourceId);
            }
            job = null;
            carryingId = null; carryingAmount = 0;
            CurrentState = State.Idle;
        }

        // ------------- Movement -------------
        bool PlanPathTo(BuildingBehaviour target)
        {
            if (target == null || target.GetComponent<BuildingInstance>() == null) return false;
            var inst = target.GetComponent<BuildingInstance>();
            var myTile = FindClosestTileUnderMe();
            if (myTile == null) return false;

            var start = myTile.coords;
            var goal = inst.coords;

            path = HexPathfinder.FindPath(start, goal, profile);
            pathIndex = 0;
            return path != null && path.Count > 0;
        }

        void StepAlongPath()
        {
            if (path == null || pathIndex >= path.Count) return;

            var curCoords = path[pathIndex];
            var curTile = GetTile(curCoords);
            if (curTile == null) { pathIndex++; return; }

            var targetPos = curTile.transform.position;
            var pos = transform.position;
            float step = moveSpeed * Time.deltaTime;

            transform.position = Vector3.MoveTowards(pos, targetPos, step);

            if (Vector3.Distance(transform.position, targetPos) <= arriveThreshold)
                pathIndex++;
        }

        bool ReachedTarget(HexCoords target)
        {
            if (path == null || path.Count == 0) return false;
            return pathIndex >= path.Count;
        }

        HexTile FindClosestTileUnderMe()
        {
           
            HexTile best = null;
            float bestDist = float.MaxValue;
            foreach (var kv in HexMapGenerator.TileIndexByKey)
            {
                var t = kv.Value;
                if (t == null) continue;
                float d = (t.transform.position - transform.position).sqrMagnitude;
                if (d < bestDist) { best = t; bestDist = d; }
            }
            return best;
        }

        HexTile GetTile(HexCoords c)
        {
            if (HexMapGenerator.TileIndexByKey.TryGetValue($"{c.q},{c.r}", out var t1)) return t1;
            if (HexMapGenerator.TileIndex.TryGetValue(c, out var t2)) return t2;
            return null;
        }

        // ------------- Info pre UI -------------
        public string GetStatusText()
        {
            switch (CurrentState)
            {
                case State.Idle: return "Idle";
                case State.ToSource: return $"To Source ({job?.resourceId})";
                case State.Loading: return $"Loading ({job?.resourceId})";
                case State.ToDest: return $"To Warehouse ({job?.resourceId})";
                case State.Unloading: return $"Unloading ({job?.resourceId})";
                default: return "Unknown";
            }
        }

        public int GetCarryingAmount() => carryingAmount;
        public string GetCarryingId() => carryingId ?? "-";
        public string GetJobLabel() => job?.DebugLabel ?? "-";
    }
}
