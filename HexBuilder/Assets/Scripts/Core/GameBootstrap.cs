using UnityEngine;
using HexBuilder.Systems.Map;
using HexBuilder.Systems.Save;
using HexBuilder.Systems.Buildings;

namespace HexBuilder.Systems.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        public HexMapGenerator generator;
        public BuildingTypeRegistry registry;

        void Start()
        {
            if (!generator) generator = FindObjectOfType<HexMapGenerator>();

            if (SaveSystem.pendingLoadSlot > 0)
            {
                SaveSystem.LoadIntoCurrentScene(SaveSystem.pendingLoadSlot, registry, generator ? generator.mapRoot : null);
                SaveSystem.pendingLoadSlot = 0; 
            }
            else
            {
               
                if (generator) generator.Generate();
            }
        }
    }
}
