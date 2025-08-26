using UnityEngine;
using HexBuilder.Systems.Buildings;

namespace HexBuilder.UI
{
    public class BuildButton : MonoBehaviour
    {
        public BuildingPlacer placer;      
        public BuildingType type;          

       
        public void HandleClick()
        {
            if (placer && type) placer.EnterBuildMode(type);
        }
    }
}
