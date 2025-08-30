using UnityEngine;
using HexBuilder.Systems.Map;
using HexBuilder.Systems.Resources;
using HexBuilder.Systems.Core;
using System.Collections.Generic;

namespace HexBuilder.Systems.Buildings
{
    [RequireComponent(typeof(BuildingInstance))]
    public abstract class BuildingBehaviour : MonoBehaviour
    {
        protected BuildingInstance instance;
        protected ResourceInventory inventory;
        protected TickManager ticks;
        protected MapGenerationProfile profile;

        // ===== Periodic Upkeep (every N ticks; 0 = disabled) =====
        [Header("Upkeep (every N ticks; 0 = disabled)")]
        [Tooltip("Consumes 1 wood each N ticks (0 = none).")]
        public int upkeepWoodEveryNTicks = 0;
        [Tooltip("Consumes 1 stone each N ticks (0 = none).")]
        public int upkeepStoneEveryNTicks = 0;
        [Tooltip("Consumes 1 water each N ticks (0 = none).")]
        public int upkeepWaterEveryNTicks = 0;

        int upkeepTickCounter = 0;
        public bool PausedForUpkeep { get; private set; }
        public string PauseReason { get; private set; }

        [Header("Output buffer (auto-filled by producers)")]
        [SerializeField] private Dictionary<string, int> outputBuffer = new Dictionary<string, int>();

        public HexCoords GetCoords() => instance != null ? instance.coords : default;
        public BuildingInstance GetInstance() => instance;

        protected virtual void Awake() { TryResolveRefs(); }
        protected virtual void OnEnable() { TryResolveRefs(); if (ticks) ticks.OnTick += OnTickSafe; }
        protected virtual void Start() { TryResolveRefs(); }
        protected virtual void OnDisable() { if (ticks) ticks.OnTick -= OnTickSafe; }
        protected virtual void OnDestroy() { if (ticks) ticks.OnTick -= OnTickSafe; }

        void OnTickSafe()
        {
            if (instance == null || instance.tile == null || inventory == null)
                return;

            if (profile == null)
            {
                var gen = FindObjectOfType<HexMapGenerator>();
                if (gen) profile = gen.profile;
                if (profile == null) return;
            }

            upkeepTickCounter++;
            if (!HandleUpkeepThisTick())
                return; // paused this tick

            OnTick();
        }

        bool HandleUpkeepThisTick()
        {
            int needWood = (upkeepWoodEveryNTicks > 0 && (upkeepTickCounter % upkeepWoodEveryNTicks == 0)) ? 1 : 0;
            int needStone = (upkeepStoneEveryNTicks > 0 && (upkeepTickCounter % upkeepStoneEveryNTicks == 0)) ? 1 : 0;
            int needWater = (upkeepWaterEveryNTicks > 0 && (upkeepTickCounter % upkeepWaterEveryNTicks == 0)) ? 1 : 0;

            if (needWood == 0 && needStone == 0 && needWater == 0)
            {
                PausedForUpkeep = false;
                PauseReason = null;
                return true;
            }

            if (needWood > 0 && inventory.GetAmount("wood") < needWood) { Pause("No wood"); return false; }
            if (needStone > 0 && inventory.GetAmount("stone") < needStone) { Pause("No stone"); return false; }
            if (needWater > 0 && inventory.GetAmount("water") < needWater) { Pause("No water"); return false; }

            if (needWood > 0) inventory.TryConsume("wood", needWood);
            if (needStone > 0) inventory.TryConsume("stone", needStone);
            if (needWater > 0) inventory.TryConsume("water", needWater);

            PausedForUpkeep = false;
            PauseReason = null;
            return true;
        }

        void Pause(string reason)
        {
            PausedForUpkeep = true;
            PauseReason = reason;
        }

        protected abstract void OnTick();


        protected void AddToOutput(string id, int amount)
        {
            if (amount <= 0 || string.IsNullOrEmpty(id)) return;
            if (!outputBuffer.TryGetValue(id, out int cur)) cur = 0;
            outputBuffer[id] = cur + amount;
        }

        public int GetBufferAmount(string id)
        {
            if (string.IsNullOrEmpty(id)) return 0;
            return outputBuffer.TryGetValue(id, out int v) ? v : 0;
        }

        public int TakeFromOutput(string id, int maxAmount)
        {
            if (string.IsNullOrEmpty(id) || maxAmount <= 0) return 0;
            if (!outputBuffer.TryGetValue(id, out int have) || have <= 0) return 0;
            int take = Mathf.Min(have, maxAmount);
            have -= take;
            if (have > 0) outputBuffer[id] = have;
            else outputBuffer.Remove(id);
            return take;
        }

        public bool HasAnyOutput()
        {
            foreach (var kv in outputBuffer)
                if (kv.Value > 0) return true;
            return false;
        }

        public Dictionary<string, int> GetOutputSnapshot()
        {
            // len pre UI (read-only snapshot)
            var copy = new Dictionary<string, int>();
            foreach (var kv in outputBuffer) copy[kv.Key] = kv.Value;
            return copy;
        }


        // --- helpers ---
        protected void TryResolveRefs()
        {
            if (!instance) instance = GetComponent<BuildingInstance>();
            if (!inventory) inventory = FindObjectOfType<ResourceInventory>();
            if (!ticks) ticks = FindObjectOfType<TickManager>();
            if (!profile)
            {
                var gen = FindObjectOfType<HexMapGenerator>();
                if (gen) profile = gen.profile;
            }
        }

        protected HexTile GetTileAt(HexCoords c)
        {
            if (HexMapGenerator.TileIndexByKey.TryGetValue($"{c.q},{c.r}", out var t1))
                return t1;
            if (HexMapGenerator.TileIndex.TryGetValue(c, out var t2))
                return t2;

            var all = Object.FindObjectsOfType<HexTile>();
            for (int i = 0; i < all.Length; i++)
                if (all[i] && all[i].coords.q == c.q && all[i].coords.r == c.r)
                    return all[i];
            return null;
        }

        protected HexTile[] GetNeighborsRing1()
        {
            var res = new HexTile[6];
            if (instance == null || instance.tile == null) return res;

            var center = instance.tile.coords;
            for (int d = 0; d < 6; d++)
            {
                var n = center.Neighbor(d);
                res[d] = GetTileAt(n);
            }
            return res;
        }

        // Pre UI
        public HexTile[] GetNeighborsForUI() => GetNeighborsRing1();
        public MapGenerationProfile GetProfile() => profile;
    }
}
