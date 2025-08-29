using UnityEngine;
using TMPro;
using UnityEngine.UI;
using HexBuilder.Systems.Buildings;

namespace HexBuilder.UI
{
    public class BuildingInfoPanel : MonoBehaviour
    {
        [Header("Root to toggle")]
        public GameObject root;

        [Header("Texts")]
        public TMP_Text nameText;
        public TMP_Text coordsText;
        public TMP_Text prodText;
        public TMP_Text descText;
        public TMP_Text statusText;

        [Header("Actions")]
        public Button demolishButton;
        public BuildingPlacer placer;

        [Range(0f, 1f)]
        public float refundPercent = 0.5f;
        public HexBuilder.Systems.Resources.ResourceInventory inventory;

        BuildingInstance current;

        void Awake()
        {
            if (!root) root = gameObject;
            Hide();

            if (demolishButton)
                demolishButton.onClick.AddListener(OnClickDemolish);

            if (!inventory)
                inventory = FindObjectOfType<HexBuilder.Systems.Resources.ResourceInventory>();

        }

        public void Show(BuildingInstance inst)
        {
            current = inst;
            if (!inst || inst.type == null)
            {
                Hide();
                return;
            }

           
            if (nameText) nameText.text = inst.type.displayName;

           
            if (coordsText) coordsText.text = $"X:{inst.coords.q} Y:{inst.coords.r}";

            
            if (descText)
            {
                var d = string.IsNullOrWhiteSpace(inst.type.description) ? "-" : inst.type.description;
                descText.text = d;
            }

           
            string prod = "Production: -";
            var beh = inst.GetComponent<BuildingBehaviour>();

            if (beh is LumberCampBehaviour lumber)
            {
                var ns = lumber.GetNeighborsForUI();
                var profile = lumber.GetProfile();

                int adj = 0;
                foreach (var t in ns)
                    if (t && t.terrain == profile.forest)
                        adj++;

                int amount = Mathf.Clamp(lumber.baseWood + adj * lumber.woodPerAdjacentForest, 0, lumber.maxPerTick);
                prod = $"+{amount} wood / tick";
            }
            else if (beh is QuarryBehaviour quarry)
            {
                var ns = quarry.GetNeighborsForUI();
                var profile = quarry.GetProfile();

                int adj = 0;
                foreach (var t in ns)
                    if (t && t.terrain == profile.stone)
                        adj++;

                int amount = Mathf.Clamp(quarry.baseStone + adj * quarry.stonePerAdjacentRock, 0, quarry.maxPerTick);
                prod = $"+{amount} stone / tick";
            }
            else if (beh is WellBehaviour well)
            {
                var ns = well.GetNeighborsForUI();
                var profile = well.GetProfile();

                int adj = 0;
                foreach (var t in ns)
                    if (t && t.terrain == profile.water)
                        adj++;

                int amount = Mathf.Clamp(well.baseWater + adj * well.waterPerAdjacentWater, 0, well.maxPerTick);
                prod = $"+{amount} water / tick";
            }

            if (prodText) prodText.text = prod;

            if (statusText)
            {
                if (beh != null && beh.PausedForUpkeep)
                {
                    string reason = beh.PauseReason;
                    statusText.text = $"Status: Paused (No {reason})";
                }
                else
                {
                    statusText.text = "Status: Running";
                }
            }

                if (demolishButton) demolishButton.interactable = true;

            root.SetActive(true);
        }

        public void Hide()
        {
            current = null;
            if (root) root.SetActive(false);
        }

        void OnClickDemolish()
        {
            if (!current) return;

            current.Demolish(inventory, refundPercent);
            Hide();
        }
        public void OnMoveClick()
        {
            if (placer != null && current != null)
            {
                placer.EnterMoveMode(current);
                
            }
        }
    }
}
