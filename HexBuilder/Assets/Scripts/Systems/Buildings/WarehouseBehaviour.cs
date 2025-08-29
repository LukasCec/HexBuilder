using UnityEngine;
using HexBuilder.Systems.Resources;

namespace HexBuilder.Systems.Buildings
{
    public class WarehouseBehaviour : BuildingBehaviour
    {
        [Header("Cap bonuses (while active)")]
        public int addMaxWood = 50;
        public int addMaxStone = 50;
        public int addMaxWater = 0;

        bool applied = false;

        protected override void OnTick()
        {
            // Warehouse niè neprodukuje per tick; robí len cap bonus.
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
    }
}
