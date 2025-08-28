using UnityEngine;
using System;
using System.Collections.Generic;

namespace HexBuilder.Systems.Map
{
    public class HexMapGenerator : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Base hex tile prefab (Prefabs/Tiles/Tile_Hex_Base)")]
        public GameObject tilePrefab;

        [Tooltip("Parent for all tiles (/_Bootstrap/MapRoot)")]
        public Transform mapRoot;

        [Tooltip("Parent for props (/_Bootstrap/MapRoot/DecorRoot). If null, uses mapRoot.")]
        public Transform decorRoot;

        [Tooltip("Profile with sizes, noise and terrain thresholds")]
        public MapGenerationProfile profile;

        [Header("Tile rotation (pointy-top)")]
        [Tooltip("Rotate each tile around Y when instantiating. Use 30 for pointy-top if your model is flat-top.")]
        public float rotationY = 30f;

        public int lastSeed { get; private set; } = 0;

        
        public static readonly Dictionary<HexCoords, HexTile> TileIndex = new();
       
        public static readonly Dictionary<string, HexTile> TileIndexByKey = new();

        public static string Key(HexCoords c) => $"{c.q},{c.r}";
        public static string Key(int q, int r) => $"{q},{r}";

        static Vector2 SeedToNoiseOffset(int s)
        {
            float x = Mathf.Abs(Mathf.Sin((s * 0.000123f) + 12.9898f)) * 43758.5453f;
            float y = Mathf.Abs(Mathf.Sin((s * 0.000173f) + 78.2330f)) * 12345.6789f;
            return new Vector2(x, y);
        }

        // ----------------- PUBLIC API -----------------

        [ContextMenu("Generate Map")]
        public void Generate()
        {
            if (!profile) { Debug.LogError("[Gen] Missing profile"); return; }
            int seed = profile.useRandomSeed
                ? (Environment.TickCount ^ DateTime.Now.Ticks.GetHashCode())
                : profile.seed;

            GenerateInternal(seed);
        }

       
        public void GenerateFromSeed(int seed)
        {
            if (!profile) { Debug.LogError("[GenFromSeed] Missing profile"); return; }
            GenerateInternal(seed);
        }

        // ----------------- INTERNAL CORE -----------------

        void GenerateInternal(int seed)
        {
            if (!tilePrefab) { Debug.LogError("[Gen] Missing tilePrefab"); return; }
            if (!mapRoot) { Debug.LogError("[Gen] Missing mapRoot"); return; }
            if (!decorRoot) decorRoot = mapRoot;

           
            if (decorRoot)
            {
                for (int i = decorRoot.childCount - 1; i >= 0; i--)
                    DestroyImmediate(decorRoot.GetChild(i).gameObject);
            }

            if (mapRoot)
            {
                for (int i = mapRoot.childCount - 1; i >= 0; i--)
                {
                    var child = mapRoot.GetChild(i);
                    if (decorRoot && child == decorRoot) continue;
                    DestroyImmediate(child.gameObject);
                }
            }

           
            TileIndex.Clear();
            TileIndexByKey.Clear();

            
            HexMetrics.OuterRadius = profile.outerRadius;

          
            lastSeed = seed;
            UnityEngine.Random.InitState(seed);
            Vector2 seedOffset = SeedToNoiseOffset(seed);

           
            Quaternion rot = Quaternion.Euler(0f, rotationY, 0f);
            int qOffset = profile.width / 2;
            int rOffset = profile.height / 2;

            int waterCnt = 0, grassCnt = 0, forestCnt = 0, stoneCnt = 0;
            float minH = 1f, maxH = 0f;

           
            for (int r = 0; r < profile.height; r++)
            {
                for (int q = 0; q < profile.width; q++)
                {
                    int qAx = q - qOffset;
                    int rAx = r - rOffset;
                    var coords = new HexCoords(qAx, rAx);

                    Vector3 pos = HexMetrics.AxialToWorld(coords.q, coords.r, profile.tileY);

                    var go = Instantiate(tilePrefab, pos, rot, mapRoot);
                    go.name = $"Hex_{coords.q}_{coords.r}";
                    go.transform.localScale = Vector3.one;

                    var tile = go.GetComponent<HexTile>();
                    if (!tile) tile = go.AddComponent<HexTile>();
                    tile.coords = coords;
                    tile.meshRenderer = go.GetComponentInChildren<MeshRenderer>();

                   
                    TileIndex[coords] = tile;
                    TileIndexByKey[Key(coords)] = tile;

                   
                    float h = profile.FBm(coords.q + seedOffset.x, coords.r + seedOffset.y);
                    minH = Mathf.Min(minH, h);
                    maxH = Mathf.Max(maxH, h);

                   
                    TerrainType tt;
                    if (h <= profile.waterThreshold) tt = profile.water;
                    else if (h >= profile.stoneThreshold) tt = profile.stone;
                    else if (h < profile.forestThreshold) tt = profile.grass;
                    else tt = profile.forest;

                    tile.ApplyTerrain(tt);

                   
                    if (tt != null && Mathf.Abs(tt.heightOffsetY) > 0.0001f)
                    {
                        var p = go.transform.position;
                        p.y += tt.heightOffsetY;
                        go.transform.position = p;
                    }

                  
                    if (tt == profile.water)
                    {
                        var wb = go.GetComponent<WaterBob>() ?? go.AddComponent<WaterBob>();
                        float baseY = go.transform.position.y;
                        wb.Initialize(baseY, -0.12f, -0.05f, 0.08f, 0.18f);
                    }

                    
                    if (tt == profile.water) waterCnt++;
                    else if (tt == profile.stone) stoneCnt++;
                    else if (tt == profile.forest) forestCnt++;
                    else if (tt == profile.grass) grassCnt++;

                    // props
                    if (tt && tt.propPrefab && UnityEngine.Random.value < tt.propSpawnChance)
                    {
                        var finalPos = go.transform.position;
                        var prop = Instantiate(tt.propPrefab, finalPos, Quaternion.identity, decorRoot);

                        float yj = UnityEngine.Random.Range(tt.propHeightJitter.x, tt.propHeightJitter.y);
                        float sj = UnityEngine.Random.Range(tt.propScaleJitter.x, tt.propScaleJitter.y);
                        float yr = UnityEngine.Random.Range(tt.propYawJitter.x, tt.propYawJitter.y);

                        prop.transform.position += new Vector3(0f, yj, 0f);
                        prop.transform.localScale *= sj;
                        prop.transform.rotation = Quaternion.Euler(0f, yr, 0f);
                    }
                }
            }

            int totalTiles = profile.width * profile.height;
            Debug.Log($"[Gen] Done. Seed={lastSeed}, tiles={totalTiles}, R={HexMetrics.OuterRadius}, rotY={rotationY}° | " +
                      $"noise min={minH:F2} max={maxH:F2} | water={waterCnt} grass={grassCnt} forest={forestCnt} stone={stoneCnt}");
            Debug.Log($"[Gen] TileIndex.Count={TileIndex.Count}, TileIndexByKey.Count={TileIndexByKey.Count}");

           
            bool byKey = TileIndexByKey.ContainsKey("0,0");
            bool byVal = TileIndex.ContainsKey(new HexCoords(0, 0));
            Debug.Log($"[Gen] Index check (0,0): byKey={byKey} byVal={byVal}");
        }
    }
}
