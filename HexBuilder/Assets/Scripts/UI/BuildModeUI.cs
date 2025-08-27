using UnityEngine;
using HexBuilder.Systems.Buildings;

namespace HexBuilder.UI
{
    public class BuildModeUI : MonoBehaviour
    {
        public BuildingPlacer placer;
        public BuildPanelAnimator buildPanel;   

        public void OnSelectModeClick()
        {
            if (placer) placer.ExitBuildMode();
            if (buildPanel) buildPanel.Hide();  
        }

       
        public void OnBuildModeClick()
        {
            if (buildPanel) buildPanel.Show();  
            
            if (placer) placer.EnterBuildMode(null); 
        }

        
        public void OnPickBuilding(BuildingType type)
        {
            if (placer && type) placer.EnterBuildMode(type);
            if (buildPanel) buildPanel.Show();
        }
    }
}
