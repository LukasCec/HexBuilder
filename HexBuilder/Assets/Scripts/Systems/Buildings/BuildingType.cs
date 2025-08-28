using UnityEngine;
using System.Collections.Generic;

namespace HexBuilder.Systems.Buildings
{
    [CreateAssetMenu(menuName = "HexBuilder/Building Type", fileName = "NewBuildingType")]
    public class BuildingType : ScriptableObject
    {
        [Header("Basics")]
        public string displayName = "Building";
        public GameObject prefab;

        [TextArea(2, 5)]
        public string description = "";

        [Tooltip("Na ak�ch ter�noch je povolen� stava�.")]
        public List<HexBuilder.Systems.Map.TerrainType> allowedTerrains = new();

        [Header("Placement")]
        [Tooltip("Vertik�lny posun pri polo�en� (napr. +0.05 nad dla�dicu).")]
        public float yOffset = 0.0f;
        [Tooltip("Lok�lny posun vo�i stredu dla�dice.")]
        public Vector3 localOffset = Vector3.zero;
        [Tooltip("Default rot�cia ghostu/budovy (Y sa d� to�i� kl�vesmi).")]
        public Vector3 defaultRotationEuler = Vector3.zero;

        [Header("Cost (optional)")]
        public int costWood = 0;
        public int costStone = 0;
    }
}
