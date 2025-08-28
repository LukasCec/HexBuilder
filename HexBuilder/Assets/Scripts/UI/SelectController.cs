using UnityEngine;
using HexBuilder.Systems.Buildings;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

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
        [Tooltip("Voliteæne obmedz raycast na konkrÈtne vrstvy (napr. Buildings/Tiles). 0 = vöetko.")]
        public LayerMask hitMask = 0;

        void Awake()
        {
            if (!cam) cam = Camera.main;
        }

        void Update()
        {
            if (LeftClickDown())
            {
                Vector2 mpos = GetMousePos();
                if (cam == null) cam = Camera.main;
                if (cam == null) return;

                Ray ray = cam.ScreenPointToRay(mpos);
                RaycastHit hit;

                bool didHit = (hitMask.value == 0)
                    ? Physics.Raycast(ray, out hit, maxRayDistance)
                    : Physics.Raycast(ray, out hit, maxRayDistance, hitMask, QueryTriggerInteraction.Ignore);

                if (didHit)
                {
                    var inst = hit.collider.GetComponentInParent<BuildingInstance>();
                    if (inst != null && infoPanel != null)
                    {
                        infoPanel.Show(inst);
                    }
                }
            }
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
