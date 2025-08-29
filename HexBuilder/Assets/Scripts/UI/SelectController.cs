using UnityEngine;
using HexBuilder.Systems.Buildings;
using HexBuilder.Systems.Map;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine.EventSystems;

namespace HexBuilder.UI
{
    public class SelectController : MonoBehaviour
    {
        [Header("Refs")]
        public Camera cam;
        public BuildingInfoPanel infoPanel;

        [Header("Raycast")]
        [Tooltip("Max. vzdialenosù raycastu pri kliku.")]
        public float maxRayDistance = 2000f;

        [Tooltip("Maska pre budovy (Layer \"Buildings\"). Ak nenech·ö, doplnÌ sa automaticky.")]
        public LayerMask buildingsMask;

        [Tooltip("Maska pre dlaûdice (Layer \"Tiles\"). Ak nenech·ö, doplnÌ sa automaticky.")]
        public LayerMask tilesMask;

        void Awake()
        {
            if (!cam) cam = Camera.main;
            if (buildingsMask == 0) buildingsMask = LayerMask.GetMask("Buildings");
            if (tilesMask == 0) tilesMask = LayerMask.GetMask("Tiles");
        }

        void Update()
        {
            if (!LeftClickDown()) return;
            if (!cam) cam = Camera.main;
            if (!cam) return;

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Vector2 mpos = GetMousePos();
            Ray ray = cam.ScreenPointToRay(mpos);

            // 1) Najprv sk˙sime trafiù budovu (Buildings).
            if (Physics.Raycast(ray, out var hitB, maxRayDistance, buildingsMask, QueryTriggerInteraction.Ignore))
            {
                var inst = hitB.collider.GetComponentInParent<BuildingInstance>();
                if (inst != null)
                {
                    infoPanel?.Show(inst);
                    return;
                }
            }

            // 2) Fallback: sk˙sime trafiù dlaûdicu (Tiles) a z nej occupant budovu.
            if (Physics.Raycast(ray, out var hitT, maxRayDistance, tilesMask, QueryTriggerInteraction.Ignore))
            {
                var tile = hitT.collider.GetComponentInParent<HexTile>();
                if (tile != null && tile.occupant != null)
                {
                    infoPanel?.Show(tile.occupant);
                    return;
                }
            }

           
            infoPanel?.Hide();
        }

        bool LeftClickDown()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
            return Input.GetMouseButtonDown(0);
#endif
        }

        Vector2 GetMousePos()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
#else
            return Input.mousePosition;
#endif
        }
    }
}
