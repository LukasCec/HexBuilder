using UnityEngine;
using TMPro;
using UnityEngine.UI;
using HexBuilder.Systems.Buildings;
using HexBuilder.Systems.Map;
using UnityEngine.EventSystems;
using HexBuilder.Systems.Workers;

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

            if (beh != null)
            {
                var buf = beh.GetOutputSnapshot();
                int bw = buf.TryGetValue("wood", out var w) ? w : 0;
                int bs = buf.TryGetValue("stone", out var s) ? s : 0;
                int ba = buf.TryGetValue("water", out var a) ? a : 0;

                string bufLine = "Buffer: ";
                var parts = new System.Collections.Generic.List<string>();
                if (bw > 0) parts.Add($"{bw} wood");
                if (bs > 0) parts.Add($"{bs} stone");
                if (ba > 0) parts.Add($"{ba} water");
                if (parts.Count == 0) bufLine += "-";
                else bufLine += string.Join(", ", parts);

               
                if (prodText) prodText.text = prodText.text + "\n" + bufLine;
            }

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

        public void OnClickDemolish()
        {
            if (!current) return;
            current.Demolish(inventory, refundPercent);
            Hide();
        }

        public void Show(WorkerAgent w)
        {
            current = null; 
            if (!w)
            {
                Hide();
                return;
            }

            
            if (nameText) nameText.text = w.workerName;

           
            var t = FindClosestTile(w.transform.position);
            if (coordsText) coordsText.text = t ? $"X:{t.coords.q} Y:{t.coords.r}" : "X:- Y:-";

           
            if (descText) descText.text = "Hauls resources between producers and warehouses.";

            
            string cargo = (w.GetCarryingAmount() > 0)
                ? $"{w.GetCarryingAmount()} {w.GetCarryingId()}"
                : "-";
            if (prodText) prodText.text = $"Cargo: {cargo}";

           
            if (statusText) statusText.text = $"Status: {w.GetStatusText()} | Job: {w.GetJobLabel()}";

            
            if (demolishButton) demolishButton.interactable = false;
           

            root.SetActive(true);
        }

        HexBuilder.Systems.Map.HexTile FindClosestTile(Vector3 pos)
        {
            HexBuilder.Systems.Map.HexTile best = null;
            float bestD = float.MaxValue;
            foreach (var kv in HexBuilder.Systems.Map.HexMapGenerator.TileIndexByKey)
            {
                var tile = kv.Value;
                if (!tile) continue;
                float d = (tile.transform.position - pos).sqrMagnitude;
                if (d < bestD) { bestD = d; best = tile; }
            }
            return best;
        }


        public void OnMoveClick()
        {
            if (!current) return;

            if (placer == null)
                placer = FindObjectOfType<BuildingPlacer>();
            if (placer == null)
            {
                Debug.LogWarning("[InfoPanel] BuildingPlacer nebol nájdený.");
                return;
            }

            placer.EnterMoveMode(current);
            Hide();
        }
    }
}
