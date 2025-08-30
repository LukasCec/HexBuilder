// Assets/Scripts/Systems/Workers/JobBoard.cs
using System.Collections.Generic;
using UnityEngine;

namespace HexBuilder.Systems.Workers
{
    public class JobBoard : MonoBehaviour
    {
        public static JobBoard Instance { get; private set; }

        [Tooltip("Max jobs kept to avoid unbounded growth.")]
        public int maxJobs = 200;

        readonly List<PickupDeliverJob> jobs = new List<PickupDeliverJob>();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void PostOrUpdateJob(PickupDeliverJob job)
        {
            if (job == null) return;
            // jednoduché: ak je podobný job, len zvýšime amount
            foreach (var j in jobs)
            {
                if (j.status == JobStatus.Pending &&
                    j.source == job.source &&
                    j.dest == job.dest &&
                    j.resourceId == job.resourceId)
                {
                    j.amount += job.amount;
                    return;
                }
            }
            if (jobs.Count < maxJobs) jobs.Add(job);
        }

        public PickupDeliverJob TryClaimJob(System.Predicate<PickupDeliverJob> filter = null)
        {
            for (int i = 0; i < jobs.Count; i++)
            {
                var j = jobs[i];
                if (j.status != JobStatus.Pending) continue;
                if (filter != null && !filter(j)) continue;
                j.status = JobStatus.Claimed;
                return j;
            }
            return null;
        }

        public int CountPending(string resourceId)
        {
            int n = 0;
            for (int i = 0; i < jobs.Count; i++)
            {
                var j = jobs[i];
                if (j.status == JobStatus.Pending && j.resourceId == resourceId)
                    n += j.amount; 
            }
            return n;
        }

        public void CompleteJob(PickupDeliverJob job)
        {
            if (job == null) return;
            job.status = JobStatus.Done;
        }

        public void InvalidateJob(PickupDeliverJob job)
        {
            if (job == null) return;
            job.status = JobStatus.Invalid;
        }

        public IReadOnlyList<PickupDeliverJob> GetAllJobs() => jobs;
    }
}
