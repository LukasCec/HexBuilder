using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using HexBuilder.Systems.Save;

namespace HexBuilder.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Slot labels (1..3)")]
        public TMP_Text slot1Label;
        public TMP_Text slot2Label;
        public TMP_Text slot3Label;

        [Header("Scene names")]
        public string gameSceneName = "Game";

        void Start()
        {
            Refresh();
        }

        void SetSlotLabel(int slot, TMP_Text label)
        {
            if (!label) return;

            if (SaveSystem.HasSave(slot))
            {
                var peek = SaveSystem.Peek(slot);
                var dt = System.DateTimeOffset.FromUnixTimeSeconds(peek.savedAtUnix).LocalDateTime;
                label.text = $"Slot {slot}: (saved {dt:yyyy-MM-dd HH:mm})";
            }
            else
            {
                label.text = $"Slot {slot}: <empty>";
            }
        }

        public void Refresh()
        {
            SetSlotLabel(1, slot1Label);
            SetSlotLabel(2, slot2Label);
            SetSlotLabel(3, slot3Label);
        }

       
        public void OnNewGame()
        {
            SaveSystem.pendingLoadSlot = 0; 
            SceneManager.LoadScene(gameSceneName);
        }

        public void OnLoadSlot(int slot)
        {
            if (!SaveSystem.HasSave(slot)) return;
            SaveSystem.pendingLoadSlot = slot;
            SceneManager.LoadScene(gameSceneName);
        }

        public void OnDeleteSlot(int slot)
        {
            SaveSystem.Delete(slot);
            Refresh();
        }

        
        public void OnExit() => Application.Quit();
    }
}
