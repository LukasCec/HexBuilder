using UnityEngine;
using HexBuilder.Systems.Buildings;

namespace HexBuilder.UI
{
    public class SelectButton : MonoBehaviour
    {
        public BuildingPlacer placer;   

       
        public void HandleClick()
        {
            if (placer) placer.ExitBuildMode(); 
        }
    }
}
