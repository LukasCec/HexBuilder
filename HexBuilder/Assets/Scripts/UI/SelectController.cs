using UnityEngine;
using HexBuilder.Systems.Buildings;
using HexBuilder.Systems.Map;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine.EventSystems;
using HexBuilder.Systems.Workers;

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

        [Tooltip("Maska pre jednotky (Layer \"Units\").")]
        public LayerMask unitsMask;


        void Awake()
        {
            if (!cam) cam = Camera.main;
            if (buildingsMask == 0) buildingsMask = LayerMask.GetMask("Buildings");
            if (tilesMask == 0) tilesMask = LayerMask.GetMask("Tiles");
            if (unitsMask == 0) unitsMask = LayerMask.GetMask("Units");

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

            if (Physics.Raycast(ray, out var hitU, maxRayDistance, unitsMask, QueryTriggerInteraction.Ignore))
            {
                var w = hitU.collider.GetComponentInParent<WorkerAgent>();
                if (w)
                {
                    infoPanel?.Show(w);     
                    return;
                }
            }


            if (Physics.Raycast(ray, out var hitB, maxRayDistance, buildingsMask, QueryTriggerInteraction.Ignore))
            {
                var inst = hitB.collider.GetComponentInParent<BuildingInstance>();
                if (inst != null)
                {
                    infoPanel?.Show(inst);
                    return;
                }
            }

            
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
