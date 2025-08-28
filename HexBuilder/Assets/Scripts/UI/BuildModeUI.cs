using UnityEngine;
using HexBuilder.Systems.Buildings;

namespace HexBuilder.UI
{
    public class BuildModeUI : MonoBehaviour
    {
        public BuildingPlacer placer;
        public BuildPanelAnimator buildPanel;
        public SelectController selectController;
       
        public void OnSelectModeClick()
        {
            if (placer) placer.ExitBuildMode();
            if (buildPanel) buildPanel.Hide();
            if (selectController) selectController.enabled = true;
        }


        public void OnBuildModeClick()
        {
            if (buildPanel) buildPanel.Show();
            if (placer) placer.EnterBuildMode(null);
            if (selectController) selectController.enabled = false;
        }


        public void OnPickBuilding(BuildingType type)
        {
            if (placer && type) placer.EnterBuildMode(type);
            if (buildPanel) buildPanel.Show();
        }
    }
}
