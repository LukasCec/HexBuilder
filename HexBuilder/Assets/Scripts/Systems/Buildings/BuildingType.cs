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

        [Tooltip("Na ak˝ch terÈnoch je povolenÈ stavaù.")]
        public List<HexBuilder.Systems.Map.TerrainType> allowedTerrains = new();

        [Header("Placement")]
        [Tooltip("Vertik·lny posun pri poloûenÌ (napr. +0.05 nad dlaûdicu).")]
        public float yOffset = 0.0f;
        [Tooltip("Lok·lny posun voËi stredu dlaûdice.")]
        public Vector3 localOffset = Vector3.zero;
        [Tooltip("Default rot·cia ghostu/budovy (Y sa d· toËiù kl·vesmi).")]
        public Vector3 defaultRotationEuler = Vector3.zero;

        [Header("Cost (optional)")]
        public int costWood = 0;
        public int costStone = 0;
    }
}
