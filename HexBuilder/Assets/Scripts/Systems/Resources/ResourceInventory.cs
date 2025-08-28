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

        public event Action OnChanged;


        public bool CanAffordMove(HexBuilder.Systems.Buildings.BuildingType t, float rate = 0.2f)
        {
            if (t == null) return false;
            int w = Mathf.CeilToInt(t.costWood * rate);
            int s = Mathf.CeilToInt(t.costStone * rate);
            return wood >= w && stone >= s;
        }

        
        public void PayMove(HexBuilder.Systems.Buildings.BuildingType t, float rate = 0.2f)
        {
            if (t == null) return;
            wood -= Mathf.CeilToInt(t.costWood * rate);
            stone -= Mathf.CeilToInt(t.costStone * rate);
            NotifyChanged();
        }

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

        public (int wood, int stone, int water) GetAll()
        {
            int wood = GetAmount("wood");  
            int stone = GetAmount("stone");
            int water = GetAmount("water");
            return (wood, stone, water);
        }

        public void SetAll(int wood, int stone, int water)
        {
            SetAmount("wood", wood);  
            SetAmount("stone", stone);
            SetAmount("water", water);
        }


        public int GetAmount(string id)
        {
            switch (id.ToLowerInvariant())
            {
                case "wood": return wood;
                case "stone": return stone;
                case "water": return water;
                default:
                    Debug.LogWarning($"[Inventory] Unknown resource id '{id}'. Use wood/stone/water.");
                    return 0;
            }
        }

        public void SetAmount(string id, int value)
        {
            switch (id.ToLowerInvariant())
            {
                case "wood": wood = value; break;
                case "stone": stone = value; break;
                case "water": water = value; break;
                default:
                    Debug.LogWarning($"[Inventory] Unknown resource id '{id}'. Use wood/stone/water.");
                    return;
            }
            NotifyChanged();
        }




        public void NotifyChanged() => OnChanged?.Invoke();
    }
}
