using System;
using UnityEngine;

namespace HexBuilder.Systems.Resources
{
    public class ResourceInventory : MonoBehaviour
    {
        [Header("Starting resources")]
        public int wood = 50;
        public int stone = 30;
        public int water = 0;

        [Header("Caps")]
        public int maxWood = 100;
        public int maxStone = 100;
        public int maxWater = 100;

        public event Action OnChanged;

        
        public bool CanAfford(HexBuilder.Systems.Buildings.BuildingType t)
        {
            if (t == null) return false;
            return wood >= t.costWood && stone >= t.costStone;
        }

        public void Pay(HexBuilder.Systems.Buildings.BuildingType t)
        {
            if (t == null) return;
            wood -= t.costWood;
            stone -= t.costStone;
            ClampToCaps();
            NotifyChanged();
        }

       
        public bool CanAffordMove(HexBuilder.Systems.Buildings.BuildingType t, float factor = 0.2f)
        {
            if (t == null) return false;
            int w = Mathf.CeilToInt(t.costWood * factor);
            int s = Mathf.CeilToInt(t.costStone * factor);
            return wood >= w && stone >= s;
        }

        public void PayMove(HexBuilder.Systems.Buildings.BuildingType t, float factor = 0.2f)
        {
            if (t == null) return;
            int w = Mathf.CeilToInt(t.costWood * factor);
            int s = Mathf.CeilToInt(t.costStone * factor);
            wood -= w;
            stone -= s;
            ClampToCaps();
            NotifyChanged();
        }

       
        public void Add(string id, int amount)
        {
            if (amount == 0) return;
            switch (id.ToLowerInvariant())
            {
                case "wood": wood = Mathf.Clamp(wood + amount, 0, maxWood); break;
                case "stone": stone = Mathf.Clamp(stone + amount, 0, maxStone); break;
                case "water": water = Mathf.Clamp(water + amount, 0, maxWater); break;
                default:
                    Debug.LogWarning($"[Inventory] Unknown resource id '{id}'. Use wood/stone/water.");
                    return;
            }
            NotifyChanged();
        }

        public int GetAmount(string id)
        {
            switch (id.ToLowerInvariant())
            {
                case "wood": return wood;
                case "stone": return stone;
                case "water": return water;
                default: return 0;
            }
        }

        public void SetAmount(string id, int value)
        {
            switch (id.ToLowerInvariant())
            {
                case "wood": wood = Mathf.Clamp(value, 0, maxWood); break;
                case "stone": stone = Mathf.Clamp(value, 0, maxStone); break;
                case "water": water = Mathf.Clamp(value, 0, maxWater); break;
            }
            NotifyChanged();
        }

        public (int wood, int stone, int water) GetAll() => (wood, stone, water);

        public void SetAll(int wood, int stone, int water)
        {
            SetAmount("wood", wood);
            SetAmount("stone", stone);
            SetAmount("water", water);
        }

       
        public void AddCaps(int addWood, int addStone, int addWater)
        {
            maxWood = Mathf.Max(0, maxWood + addWood);
            maxStone = Mathf.Max(0, maxStone + addStone);
            maxWater = Mathf.Max(0, maxWater + addWater);
            ClampToCaps();
            NotifyChanged();
        }

        void ClampToCaps()
        {
            wood = Mathf.Clamp(wood, 0, maxWood);
            stone = Mathf.Clamp(stone, 0, maxStone);
            water = Mathf.Clamp(water, 0, maxWater);
        }

       
        public bool TryConsume(string id, int amount)
        {
            if (amount <= 0) return true;
            switch (id.ToLowerInvariant())
            {
                case "wood":
                    if (wood >= amount) { wood -= amount; NotifyChanged(); return true; }
                    return false;
                case "stone":
                    if (stone >= amount) { stone -= amount; NotifyChanged(); return true; }
                    return false;
                case "water":
                    if (water >= amount) { water -= amount; NotifyChanged(); return true; }
                    return false;
                default:
                    Debug.LogWarning($"[Inventory] Unknown resource id '{id}'.");
                    return false;
            }
        }

       
        public bool TryConsume(int needWood, int needStone, int needWater, out string missing)
        {
            missing = null;
            if (needWood > 0 && wood < needWood) { missing = "wood"; return false; }
            if (needStone > 0 && stone < needStone) { missing = "stone"; return false; }
            if (needWater > 0 && water < needWater) { missing = "water"; return false; }

            if (needWood > 0) wood -= needWood;
            if (needStone > 0) stone -= needStone;
            if (needWater > 0) water -= needWater;
            NotifyChanged();
            return true;
        }

        public void NotifyChanged() => OnChanged?.Invoke();
    }
}
