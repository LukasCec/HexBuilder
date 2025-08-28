using UnityEngine;
using UnityEngine.SceneManagement;
using HexBuilder.Systems.Save;
using HexBuilder.Systems.Buildings;
using HexBuilder.Systems.Map;

namespace HexBuilder.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("UI")]
        public GameObject panel;

        [Header("Scene names")]
        public string mainMenuSceneName = "MainMenu";

        [Header("Save/Load deps")]
        public BuildingTypeRegistry registry;
        public HexMapGenerator generator;       
        public Transform buildingsParent;


        [Header("Gameplay refs")]
        public BuildingPlacer placer;

        void Awake()
        {
            if (panel) panel.SetActive(false);
            if (!buildingsParent && generator) buildingsParent = generator.mapRoot;
        }

        void Update()
        {
            if (EscapePressed())
            {
                if (placer != null && placer.HandleEscapeFromModes())
                    return;

               
                if (panel) panel.SetActive(!panel.activeSelf);
                Time.timeScale = panel && panel.activeSelf ? 0f : 1f;
            }
        }

       
        public void OnContinue()
        {
            if (panel) panel.SetActive(false);
            Time.timeScale = 1f;
        }

        public void OnSaveSlot(int slot)
        {
            SaveSystem.Save(slot);
        }

        public void OnLoadSlot(int slot)
        {
            SaveSystem.LoadIntoCurrentScene(slot, registry, buildingsParent);
            
        }

        public void OnExitToMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }

       
        bool EscapePressed()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = UnityEngine.InputSystem.Keyboard.current;
            return kb != null && kb.escapeKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Escape);
#endif
        }
    }
}
