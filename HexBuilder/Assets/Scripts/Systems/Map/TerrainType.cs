using UnityEngine;

namespace HexBuilder.Systems.Map
{
    [CreateAssetMenu(fileName = "TerrainType", menuName = "HexBuilder/Terrain Type", order = 0)]
    public class TerrainType : ScriptableObject
    {
        public string id = "forest";
        public string displayName = "Terrain";
        public Material material;         
        public bool buildable = true;      
        [Range(0, 10)] public int movementCost = 1;

        [Header("Visual")]
        [Tooltip("Lokálny y-offset pre tento terén (záporné = nižšie).")]
        public float heightOffsetY = 0f;

        [Header("Base yields per tile (optional)")]
        public int yieldWood = 0;
        public int yieldStone = 0;
        public int yieldWater = 0;        

        [Header("Decoration (optional)")]
        public GameObject propPrefab;      
        [Range(0f, 1f)] public float propSpawnChance = 0.0f;
        public Vector2 propHeightJitter = new Vector2(0f, 0.05f);
        public Vector2 propScaleJitter = new Vector2(0.9f, 1.1f);
        public Vector2 propYawJitter = new Vector2(0f, 360f);
    }
}
