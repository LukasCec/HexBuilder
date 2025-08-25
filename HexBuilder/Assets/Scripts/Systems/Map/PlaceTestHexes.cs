using UnityEngine;
using System.Collections.Generic;
using HexBuilder.Systems.Map;

public class PlaceTestHexes : MonoBehaviour
{
    [Header("References")]
    public GameObject tilePrefab;   
    public Transform mapRoot;      
    public float yHeight = 0f;

    [Header("Hex radius (pointy-top math)")]
    public bool overrideOuterRadius = true;
    public float outerRadius = 0.5f;

    [Header("Rotation")]
    [Tooltip("Otočí každú dlaždicu okolo osi Y po inštancovaní (Riešenie A = 30°).")]
    public float rotationOverrideY = 30f;

    [Header("What to place")]
    public bool useManualCoords = true;
    public List<HexCoords> manualCoords = new List<HexCoords>()
    {
        new HexCoords(0, 0),
        new HexCoords(1, 0),   // E
        new HexCoords(1,-1),   // NE
        new HexCoords(0,-1),   // NW
        new HexCoords(-1,0),   // W
        new HexCoords(-1,1),   // SW
        new HexCoords(0, 1),   // SE
    };
    public int gridRadius = 2;

    [ContextMenu("Place Test Hexes")]
    public void PlaceNow()
    {
        if (overrideOuterRadius) HexMetrics.OuterRadius = outerRadius;

        if (!tilePrefab) { Debug.LogError("tilePrefab nie je priradený!"); return; }
        if (!mapRoot) { Debug.LogError("mapRoot nie je priradený!"); return; }

        
        for (int i = mapRoot.childCount - 1; i >= 0; i--)
            DestroyImmediate(mapRoot.GetChild(i).gameObject);

        var coordsToPlace = new List<HexCoords>();
        if (useManualCoords) coordsToPlace.AddRange(manualCoords);
        else
        {
            for (int q = -gridRadius; q <= gridRadius; q++)
                for (int r = -gridRadius; r <= gridRadius; r++)
                    if (Mathf.Abs(q + r) <= gridRadius)
                        coordsToPlace.Add(new HexCoords(q, r));
        }

        Quaternion rot = Quaternion.Euler(0f, rotationOverrideY, 0f);

        foreach (var c in coordsToPlace)
        {
            Vector3 pos = HexMetrics.AxialToWorld(c.q, c.r, yHeight);
            var go = Instantiate(tilePrefab, pos, rot, mapRoot);
            go.name = $"Hex_{c.q}_{c.r}";
           
            go.transform.localScale = Vector3.one;
        }

        Debug.Log($"Placed {coordsToPlace.Count} hexes with Y-rotation {rotationOverrideY}° and OuterRadius {HexMetrics.OuterRadius}.");
    }
}
