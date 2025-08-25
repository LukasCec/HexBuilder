using UnityEngine;
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

        [ContextMenu("Generate Map")]
        public void Generate()
        {
           
            if (!profile) { Debug.LogError("[Gen] Missing profile"); return; }
            if (!tilePrefab) { Debug.LogError("[Gen] Missing tilePrefab"); return; }
            if (!mapRoot) { Debug.LogError("[Gen] Missing mapRoot"); return; }
            if (!decorRoot) decorRoot = mapRoot;

          
            if (decorRoot)
            {
                for (int i = decorRoot.childCount - 1; i >= 0; i--)
                {
                    var child = decorRoot.GetChild(i);
                    if (child) DestroyImmediate(child.gameObject);
                }
            }

          
            if (mapRoot)
            {
                for (int i = mapRoot.childCount - 1; i >= 0; i--)
                {
                    var child = mapRoot.GetChild(i);
                    if (!child) continue;
                    if (decorRoot && child == decorRoot) continue; 
                    DestroyImmediate(child.gameObject);
                }
            }

           
            HexMetrics.OuterRadius = profile.outerRadius;

            
            int seed = profile.useRandomSeed ? Random.Range(int.MinValue / 4, int.MaxValue / 4) : profile.seed;
            Random.InitState(seed);

            
            Quaternion rot = Quaternion.Euler(0f, rotationY, 0f);
            int total = 0;

           
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

                    
                    float h = profile.FBm(coords.q, coords.r);
                    minH = Mathf.Min(minH, h);
                    maxH = Mathf.Max(maxH, h);

                    
                    TerrainType tt;
                    if (h <= profile.waterThreshold) tt = profile.water;
                    else if (h >= profile.stoneThreshold) tt = profile.stone;
                    else if (h < profile.forestThreshold) tt = profile.grass;
                    else tt = profile.forest;

                    tile.ApplyTerrain(tt);
                    if (tt == profile.water) waterCnt++;
                    else if (tt == profile.stone) stoneCnt++;
                    else if (tt == profile.forest) forestCnt++;
                    else if (tt == profile.grass) grassCnt++;

                   
                    if (tt && tt.propPrefab && Random.value < tt.propSpawnChance)
                    {
                        var prop = Instantiate(tt.propPrefab, pos, Quaternion.identity, decorRoot);

                      
                        float yj = Random.Range(tt.propHeightJitter.x, tt.propHeightJitter.y);
                        float sj = Random.Range(tt.propScaleJitter.x, tt.propScaleJitter.y);
                        float yr = Random.Range(tt.propYawJitter.x, tt.propYawJitter.y);

                        prop.transform.position += new Vector3(0f, yj, 0f);
                        prop.transform.localScale *= sj;
                        prop.transform.rotation = Quaternion.Euler(0f, yr, 0f);
                    }

                    total++;
                }
            }

            int totala = profile.width * profile.height;
            Debug.Log($"[Gen] Done. Seed={seed}, tiles={totala}, R={HexMetrics.OuterRadius}, rotY={rotationY}° " +
                      $"| noise min={minH:F2} max={maxH:F2} " +
                      $"| water={waterCnt} grass={grassCnt} forest={forestCnt} stone={stoneCnt}");
        }
    }
}
