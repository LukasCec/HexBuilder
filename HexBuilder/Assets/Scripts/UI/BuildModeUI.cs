using UnityEngine;
using HexBuilder.Systems.Buildings;

namespace HexBuilder.UI
{
    public class BuildModeUI : MonoBehaviour
    {
        public BuildingPlacer placer;

        public void OnSelectModeClick()
        {
            if (placer) placer.ExitBuildMode();
        }

        public void OnBuildModeClick(BuildingType type)
        {
            if (placer && type) placer.EnterBuildMode(type);
        }
    }
}
