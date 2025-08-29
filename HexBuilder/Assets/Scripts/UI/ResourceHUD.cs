using UnityEngine;
using TMPro;
using HexBuilder.Systems.Resources;

namespace HexBuilder.UI
{
    public class ResourceHUD : MonoBehaviour
    {
        [Header("Refs")]
        public ResourceInventory inventory;

        [Header("UI")]
        public TMP_Text woodText;
        public TMP_Text stoneText;
        public TMP_Text waterText;

        void Awake()
        {
            if (!inventory) inventory = FindObjectOfType<ResourceInventory>();
        }

        void OnEnable()
        {

            if (inventory) inventory.OnChanged += Refresh;
            Refresh();
        }

        void OnDisable()
        {
            if(inventory) inventory.OnChanged -= Refresh;
        }

        void Start()
        {

            Refresh();
        }

        public void Refresh()
        {
            if (!inventory) return;

            if (woodText) woodText.text = $"{inventory.wood}/{inventory.maxWood}";
            if (stoneText) stoneText.text = $"{inventory.stone}/{inventory.maxStone}";
            if (waterText) waterText.text = $"{inventory.water}/{inventory.maxWater}";
        }
    }
}
