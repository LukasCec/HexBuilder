using UnityEngine;
using HexBuilder.Systems.Map;
using HexBuilder.Systems.Resources;
using HexBuilder.Systems.Core;

namespace HexBuilder.Systems.Buildings
{
    /// Base pre akéko¾vek „živé“ správanie budovy.
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

            // ak by profil nebol vyriešený, skús to ešte raz
            if (profile == null)
            {
                var gen = FindObjectOfType<HexBuilder.Systems.Map.HexMapGenerator>();
                if (gen) profile = gen.profile;
                if (profile == null) return;
            }

            OnTick();
        }

        protected abstract void OnTick();

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
            // 1) string index "q,r"
            if (HexBuilder.Systems.Map.HexMapGenerator.TileIndexByKey.TryGetValue($"{c.q},{c.r}", out var t1))
                return t1;

            // 2) value-type index
            if (HexBuilder.Systems.Map.HexMapGenerator.TileIndex.TryGetValue(c, out var t2))
                return t2;

            // 3) pomalý fallback – prebehni existujúce HexTile v scéne
            //    (6 susedov × O(N) je úplne v pohode)
            var all = Object.FindObjectsOfType<HexBuilder.Systems.Map.HexTile>();
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null && all[i].coords.q == c.q && all[i].coords.r == c.r)
                    return all[i];
            }
            return null;
        }



        /// Vráti 6 susedov (mimo mapy môže vraca null).
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
