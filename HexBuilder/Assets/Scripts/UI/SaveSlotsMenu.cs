using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using HexBuilder.Systems.Save;

namespace HexBuilder.UI
{
    public class SaveSlotsMenu : MonoBehaviour
    {
        [System.Serializable]
        public class SlotWidgets
        {
            public TMP_Text label;
            public Button loadButton;
            public Button deleteButton;
        }

        public string gameSceneName = "Main";   
        public SlotWidgets[] slots = new SlotWidgets[3];

        void OnEnable() { Refresh(); }

        public void Refresh()
        {
            for (int i = 0; i < 3; i++)
            {
                var data = SaveSystem.Peek(i + 1);
                bool has = data != null;
                if (slots[i].label)
                {
                    if (has)
                    {
                        var dt = System.DateTimeOffset.FromUnixTimeSeconds(data.savedAtUnix).LocalDateTime;
                        slots[i].label.text = $"Slot {i + 1}: {dt:g}  |  buildings: {data.buildings.Count}";
                    }
                    else slots[i].label.text = $"Slot {i + 1}: Empty";
                }
                if (slots[i].loadButton) slots[i].loadButton.interactable = has;
                if (slots[i].deleteButton) slots[i].deleteButton.interactable = has;
            }
        }

        public void OnLoadSlot(int slot)
        {
            SaveSystem.pendingLoadSlot = slot;
            SceneManager.LoadScene(gameSceneName);
        }

        public void OnDeleteSlot(int slot)
        {
            SaveSystem.Delete(slot);
            Refresh();
        }

        public void OnNewGame()
        {
            SaveSystem.pendingLoadSlot = 0; 
            SceneManager.LoadScene(gameSceneName);
        }
    }
}
