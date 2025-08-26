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

        void OnEnable()
        {
            if (!inventory) inventory = FindObjectOfType<ResourceInventory>();
            if (inventory != null) inventory.OnChanged += UpdateUI;
            UpdateUI();
        }

        void OnDisable()
        {
            if (inventory != null) inventory.OnChanged -= UpdateUI;
        }

        void Start()
        {
            
            UpdateUI();
        }

        public void UpdateUI()
        {
            if (!inventory) return;
            if (woodText) woodText.text = inventory.wood.ToString();
            if (stoneText) stoneText.text = inventory.stone.ToString();
            if (waterText) waterText.text = inventory.water.ToString();
        }
    }
}
