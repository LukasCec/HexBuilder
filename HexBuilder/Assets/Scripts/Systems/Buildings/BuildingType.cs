using UnityEngine;
using System.Collections.Generic;

namespace HexBuilder.Systems.Buildings
{
    [CreateAssetMenu(menuName = "HexBuilder/Building Type", fileName = "NewBuildingType")]
    public class BuildingType : ScriptableObject
    {
        [Header("ID")]
        [Tooltip("Jedineènı string (napr. 'lumber', 'well', 'quarry'). Pouíva sa v savoch.")]
        public string id = "unset";

        [Header("Basics")]
        public string displayName = "Building";
        public GameObject prefab;

        [TextArea(2, 5)]
        public string description = "";

        [Tooltip("Na akıch terénoch je povolené stava.")]
        public List<HexBuilder.Systems.Map.TerrainType> allowedTerrains = new();

        [Header("Placement")]
        [Tooltip("Vertikálny posun pri poloení (napr. +0.05 nad dladicu).")]
        public float yOffset = 0.0f;
        [Tooltip("Lokálny posun voèi stredu dladice.")]
        public Vector3 localOffset = Vector3.zero;
        [Tooltip("Default rotácia ghostu/budovy (Y sa dá toèi klávesmi).")]
        public Vector3 defaultRotationEuler = Vector3.zero;

        [Header("Cost (optional)")]
        public int costWood = 0;
        public int costStone = 0;
    }
}
