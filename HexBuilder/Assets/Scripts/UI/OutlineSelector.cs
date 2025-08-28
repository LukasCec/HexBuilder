using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using HexBuilder.Systems.Buildings;

namespace HexBuilder.UI
{
   
    public class OutlineSelector : MonoBehaviour
    {
        public Camera cam;

       
        [Tooltip("Index URP Rendering Layer pre outline (napr. 6 = 'Outline').")]
        [Range(0, 7)] public int outlineLayerIndex = 6;

        BuildingInstance current;

        void Start()
        {
            if (!cam) cam = Camera.main;
        }

        void Update()
        {
            if (LeftClickDown())
            {
                Ray ray = cam.ScreenPointToRay(GetMousePos());
                if (Physics.Raycast(ray, out var hit, 1000f, ~0, QueryTriggerInteraction.Ignore))
                {
                    var inst = hit.collider.GetComponentInParent<BuildingInstance>();
                    SetSelected(inst);
                }
            }
        }

        public void SetSelected(BuildingInstance inst)
        {
          
            if (current) SetOutlineOn(current, false);

            current = inst;

            if (current) SetOutlineOn(current, true);
        }

        void SetOutlineOn(BuildingInstance inst, bool on)
        {
            if (!inst) return;

            uint bit = 1u << outlineLayerIndex;
            var renderers = inst.GetComponentsInChildren<Renderer>(true);

            foreach (var r in renderers)
            {
                if (!r) continue;
                uint mask = r.renderingLayerMask;

                if (on) mask |= bit;    
                else mask &= ~bit;    

                r.renderingLayerMask = mask;
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
