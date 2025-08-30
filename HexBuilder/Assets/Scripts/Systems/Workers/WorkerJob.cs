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
        public int amount;                 // po�adovan� mno�stvo
        public BuildingBehaviour source;   // producent (m� buffer)
        public WarehouseBehaviour dest;    // sklad (cie�)
        public JobStatus status = JobStatus.Pending;

        // QoL pre UI
        public string DebugLabel =>
            $"{resourceId} x{amount} from {source?.name ?? "?"} to {dest?.name ?? "?"}";
    }
}
