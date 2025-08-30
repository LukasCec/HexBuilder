// Assets/Scripts/Systems/Workers/WorkerJob.cs
using UnityEngine;
using HexBuilder.Systems.Buildings;

namespace HexBuilder.Systems.Workers
{
    public enum JobStatus { Pending, Claimed, Done, Invalid }

    [System.Serializable]
    public class PickupDeliverJob
    {
        public string resourceId;          // "wood"/"stone"/"water"
        public int amount;                 // požadované množstvo
        public BuildingBehaviour source;   // producent (má buffer)
        public WarehouseBehaviour dest;    // sklad (cie¾)
        public JobStatus status = JobStatus.Pending;

        // QoL pre UI
        public string DebugLabel =>
            $"{resourceId} x{amount} from {source?.name ?? "?"} to {dest?.name ?? "?"}";
    }
}
