using UnityEngine;

namespace HexBuilder.Systems.Map
{
    public class HexTile : MonoBehaviour
    {
        public HexCoords coords;
        public TerrainType terrain;
        [HideInInspector] public MeshRenderer meshRenderer;

        void Awake()
        {
            if (!meshRenderer) meshRenderer = GetComponentInChildren<MeshRenderer>();
        }

        public void ApplyTerrain(TerrainType t)
        {
            terrain = t;
            if (!meshRenderer) meshRenderer = GetComponentInChildren<MeshRenderer>();
            if (meshRenderer && t && t.material) meshRenderer.sharedMaterial = t.material;
        }

        [System.NonSerialized] public HexBuilder.Systems.Buildings.BuildingInstance occupant;
        public bool IsOccupied => occupant != null;
    }
}
