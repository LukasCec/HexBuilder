using UnityEngine;
using HexBuilder.Systems.Save;
using HexBuilder.Systems.Buildings;
using HexBuilder.Systems.Map;

namespace HexBuilder.Bootstrap
{
    public class GameLoader : MonoBehaviour
    {
        [Header("Refs")]
        public HexMapGenerator generator;          
        public BuildingTypeRegistry registry;     
        public Transform buildingsParent;           

        [Header("Fallback")]
        public bool generateNewIfNoLoad = true;

        void Start()
        {
           
            int slot = SaveSystem.pendingLoadSlot;
            SaveSystem.pendingLoadSlot = 0;

            if (slot > 0)
            {
                SaveSystem.LoadIntoCurrentScene(slot, registry, buildingsParent);
            }
            else if (generateNewIfNoLoad && generator != null)
            {
                generator.Generate(); 
            }
        }
    }
}
