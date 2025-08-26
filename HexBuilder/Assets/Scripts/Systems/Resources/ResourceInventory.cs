using System;
using UnityEngine;

namespace HexBuilder.Systems.Resources
{
    
    public class ResourceInventory : MonoBehaviour
    {
        [Header("Starting resources")]
        public int wood = 50;
        public int stone = 30;
        public int water = 0;   // zatia¾ ho len zobrazujeme (náklady máme wood/stone)

        public event Action OnChanged;

        // --- API ---
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
            NotifyChanged();
        }

        public void Add(string id, int amount)
        {
            if (amount == 0) return;
            switch (id.ToLowerInvariant())
            {
                case "wood": wood += amount; break;
                case "stone": stone += amount; break;
                case "water": water += amount; break;
                default:
                    Debug.LogWarning($"[Inventory] Unknown resource id '{id}'. Use wood/stone/water.");
                    return;
            }
            NotifyChanged();
        }

        public void NotifyChanged() => OnChanged?.Invoke();
    }
}
