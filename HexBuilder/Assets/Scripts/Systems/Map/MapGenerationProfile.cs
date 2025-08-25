using UnityEngine;

namespace HexBuilder.Systems.Map
{
    [CreateAssetMenu(fileName = "MapGenerationProfile", menuName = "HexBuilder/Map Generation Profile", order = 10)]
    public class MapGenerationProfile : ScriptableObject
    {
        [Header("Size (tiles in axial space)")]
        public int width = 20;    
        public int height = 20;    

        [Header("Random")]
        public int seed = 12345;
        public bool useRandomSeed = true;

        [Header("Hex geometry")]
        public float outerRadius = 0.5f;  
        public float tileY = 0.0f;        

        [Header("Noise (height/biome map)")]
        public float noiseScale = 0.12f;
        [Range(1, 8)] public int octaves = 3;
        public float persistence = 0.5f;
        public float lacunarity = 2.0f;
        public Vector2 noiseOffset = Vector2.zero;

        [Header("Thresholds (0..1)")]
        [Tooltip("<= waterThreshold => Water")]
        [Range(0f, 1f)] public float waterThreshold = 0.30f;
        [Tooltip("> water && < forestThreshold => Grass")]
        [Range(0f, 1f)] public float forestThreshold = 0.60f;
        [Tooltip(">= stoneThreshold => Stone (inak Grass/Forest)")]
        [Range(0f, 1f)] public float stoneThreshold = 0.80f;

        [Header("Terrain types (assign in Inspector)")]
        public TerrainType grass;
        public TerrainType forest;
        public TerrainType stone;
        public TerrainType water;

        
        public float FBm(float x, float y)
        {
            float amp = 1f;
            float freq = 1f;
            float sum = 0f;
            for (int i = 0; i < octaves; i++)
            {
                sum += amp * Mathf.PerlinNoise(noiseOffset.x + x * noiseScale * freq,
                                               noiseOffset.y + y * noiseScale * freq);
                amp *= persistence;
                freq *= lacunarity;
            }
           
            return Mathf.InverseLerp(0f, (1f - Mathf.Pow(persistence, octaves)) / (1f - persistence), sum);
        }
    }
}
