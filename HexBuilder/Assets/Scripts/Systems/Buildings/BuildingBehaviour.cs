using UnityEngine;
using HexBuilder.Systems.Map;
using HexBuilder.Systems.Resources;
using HexBuilder.Systems.Core;

namespace HexBuilder.Systems.Buildings
{
    
    [RequireComponent(typeof(BuildingInstance))]
    public abstract class BuildingBehaviour : MonoBehaviour
    {
        protected BuildingInstance instance;
        protected ResourceInventory inventory;
        protected TickManager ticks;
        protected MapGenerationProfile profile;

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
                var gen = FindObjectOfType<HexBuilder.Systems.Map.HexMapGenerator>();
                if (gen) profile = gen.profile;
                if (profile == null) return;
            }

            OnTick();
        }

        protected abstract void OnTick();

        

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
           
            if (HexBuilder.Systems.Map.HexMapGenerator.TileIndexByKey.TryGetValue($"{c.q},{c.r}", out var t1))
                return t1;

            
            if (HexBuilder.Systems.Map.HexMapGenerator.TileIndex.TryGetValue(c, out var t2))
                return t2;

            
            var all = Object.FindObjectsOfType<HexBuilder.Systems.Map.HexTile>();
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null && all[i].coords.q == c.q && all[i].coords.r == c.r)
                    return all[i];
            }
            return null;
        }
        public HexBuilder.Systems.Map.HexTile[] GetNeighborsForUI()
        {
            return GetNeighborsRing1();
        }

        
        public HexBuilder.Systems.Map.MapGenerationProfile GetProfile()
        {
            return profile;
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
    }
}
