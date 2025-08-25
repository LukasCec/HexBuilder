using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace HexBuilder.Systems.Map
{
    public class TileHoverController : MonoBehaviour
    {
        public Camera cam;                  // priraï, alebo nechaj prázdne -> Camera.main
        public LayerMask tilesMask;         // nastav na Layer "Tiles"
        public float maxDistance = 1000f;

        HexTileHover current;

        void Start()
        {
            if (!cam) cam = Camera.main;
            if (tilesMask == 0) tilesMask = LayerMask.GetMask("Tiles");
        }

        void Update()
        {
            if (!cam) cam = Camera.main;
            if (!cam) return;

            Vector2 mousePos;
#if ENABLE_INPUT_SYSTEM
            mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : (Vector2)Input.mousePosition;
#else
            mousePos = Input.mousePosition;
#endif

            var ray = cam.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out var hit, maxDistance, tilesMask, QueryTriggerInteraction.Ignore))
            {
                var hover = hit.collider.GetComponentInParent<HexTileHover>();
                if (hover != current)
                {
                    if (current) current.SetHovered(false);
                    current = hover;
                    if (current)
                    {
                        // volite¾ná farba pod¾a buildability
                        bool buildable = current.GetComponent<HexTile>()?.terrain?.buildable ?? true;
                        current.SetHovered(true, buildable);
                    }
                }
            }
            else
            {
                if (current) current.SetHovered(false);
                current = null;
            }
        }
    }
}
