using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace HexBuilder.Systems.Map
{
    /// <summary>
    /// PlynulÈ RTS ovl·danie: pan (RMB), rotate (MMB), zoom (scroll).
    /// Funguje s New Input System aj star˝m Input.
    /// Rig: CameraRig (tento skript) -> Pivot (rot·cia, vzdialenosù) -> MainCamera
    /// </summary>
    public class RTSCameraController : MonoBehaviour
    {
        [Header("Rig References")]
        public Transform pivot;            // /_Bootstrap/CameraRig/Pivot
        public Camera cam;                 // /_Bootstrap/CameraRig/Pivot/MainCamera

        [Header("Pan (RMB drag)")]
        public float panSpeed = 1.2f;      // z·kladn· r˝chlosù posunu
        public float panDistanceFactor = 0.04f; // ËÌm Ôalej si, t˝m r˝chlejöÌ pan
        public float panSmoothTime = 0.08f;

        [Header("Rotate (MMB drag)")]
        public float rotateSensitivity = 0.25f;    // ∞ na pixel
        public float rotateSmoothTime = 0.06f;
        public float minPitch = 20f;
        public float maxPitch = 80f;

        [Header("Zoom (scroll)")]
        public float minDistance = 6f;
        public float maxDistance = 60f;
        public float zoomSpeed = 6f;          // ÑakÈ veækÈ krokyì
        public float zoomSmoothTime = 0.08f;

        [Header("Picking plane")]
        [Tooltip("Rovina, po ktorej sa pan-uje (zvyËajne tileY = 0). Ak nech·ö -9999, naËÌta sa z MapGenerationProfile.")]
        public float planeY = -9999f;

        [Header("Misc")]
        public bool invertZoom = false;
        public bool debugRays = false;

        // internÈ stavy
        Vector3 targetRigPos, rigVel;
        float targetYaw, yawVel;
        float targetPitch, pitchVel;
        float targetDistance, distVel;

        // panovanie ñ pam‰t· si bod na rovine
        bool rmbDown;
        Vector3 panLastHit;

        void Reset()
        {
            // predvyplÚ referencie
            if (!pivot && transform.childCount > 0) pivot = transform.GetChild(0);
            if (!cam) cam = Camera.main;
        }

        void Start()
        {
            if (!pivot)
            {
                Debug.LogError("[RTSCamera] Missing Pivot reference."); enabled = false; return;
            }
            if (!cam)
            {
                cam = Camera.main;
                if (!cam) { Debug.LogError("[RTSCamera] Missing Camera reference."); enabled = false; return; }
            }

            // planeY z profilu (ak nie je nastavenÈ)
            if (planeY < -9000f)
            {
                var gen = FindObjectOfType<HexMapGenerator>();
                if (gen && gen.profile != null) planeY = gen.profile.tileY;
                else planeY = 0f;
            }

            // inicializ·cia cieæov z aktu·lnej pÛzy
            targetRigPos = transform.position;

            // infer yaw/pitch
            var e = pivot.rotation.eulerAngles;
            targetYaw = e.y;
            targetPitch = e.x;

            // infer distance z lok·lnej pozÌcie kamery
            targetDistance = Mathf.Max(1f, -cam.transform.localPosition.z);
            targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);
        }

        void Update()
        {
            if (!cam) cam = Camera.main;
            if (!cam || !pivot) return;

            // --- INPUT STAVY ---
            bool rmb = IsRightMouseHeld();
            bool mmb = IsMiddleMouseHeld();
            Vector2 mouseDelta = GetMouseDelta();
            float scroll = GetScroll(); // kladnÈ = nahor (ötandardne priblÌûiù)

            // --- PAN (RMB) ---
            if (rmb && TryGetPlanePoint(out Vector3 hit))
            {
                if (!rmbDown)
                {
                    rmbDown = true;
                    panLastHit = hit;
                }
                Vector3 delta = panLastHit - hit; // presun rig-u o to, Ëo "uöiel" kurzor po rovine
                float distanceFactor = 1f + targetDistance * panDistanceFactor;
                targetRigPos += delta * panSpeed * distanceFactor;
                panLastHit = hit;

                if (debugRays)
                {
                    Debug.DrawLine(hit + Vector3.up * 0.01f, hit + Vector3.up * 1.0f, Color.green, 0f, false);
                }
            }
            else
            {
                rmbDown = false;
            }

            // --- ROTATE (MMB) ---
            if (mmb)
            {
                targetYaw += mouseDelta.x * rotateSensitivity;
                targetPitch -= mouseDelta.y * rotateSensitivity;
                targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);
            }

            // --- ZOOM (scroll) ---
            if (Mathf.Abs(scroll) > 0.0001f)
            {
                float dir = invertZoom ? -1f : 1f;
                // New Input System d·va ~120 krok; star˝ Input ±0.1 ñ normalizujme
                float norm = Mathf.Sign(scroll) * Mathf.Log(1f + Mathf.Abs(scroll)) * 5f;
                targetDistance = Mathf.Clamp(targetDistance - norm * dir * zoomSpeed, minDistance, maxDistance);
            }

            // --- SMOOTH APLIK¡CIE ---
            // pozÌcia rig-u
            transform.position = Vector3.SmoothDamp(transform.position, targetRigPos, ref rigVel, panSmoothTime);

            // rot·cia (pitch/yaw na pivot)
            float yaw = Mathf.SmoothDampAngle(pivot.eulerAngles.y, targetYaw, ref yawVel, rotateSmoothTime);
            float pitch = Mathf.SmoothDampAngle(pivot.eulerAngles.x, targetPitch, ref pitchVel, rotateSmoothTime);
            pivot.rotation = Quaternion.Euler(pitch, yaw, 0f);

            // vzdialenosù kamery
            float d = Mathf.SmoothDamp(-cam.transform.localPosition.z, targetDistance, ref distVel, zoomSmoothTime);
            cam.transform.localPosition = new Vector3(0f, 0f, -d);
        }

        // ---------- Helpers: input ----------
        bool IsRightMouseHeld()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null) return Mouse.current.rightButton.isPressed;
#endif
            return Input.GetMouseButton(1);
        }
        bool IsMiddleMouseHeld()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null) return Mouse.current.middleButton.isPressed;
#endif
            return Input.GetMouseButton(2);
        }
        Vector2 GetMouseDelta()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null) return Mouse.current.delta.ReadValue();
#endif
            return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * 10f; // pribliûne ako delta
        }
        float GetScroll()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null) return Mouse.current.scroll.ReadValue().y / 120f; // normaliz·cia
#endif
            return Input.GetAxis("Mouse ScrollWheel"); // typicky ±0.1
        }

        // ---------- Helpers: plane pick ----------
        bool TryGetPlanePoint(out Vector3 hit)
        {
            var ray = cam.ScreenPointToRay(GetMousePosition());
            var plane = new Plane(Vector3.up, new Vector3(0f, planeY, 0f));
            if (plane.Raycast(ray, out float enter))
            {
                hit = ray.GetPoint(enter);
                if (debugRays) Debug.DrawRay(ray.origin, ray.direction * enter, Color.cyan);
                return true;
            }
            hit = default;
            return false;
        }
        Vector2 GetMousePosition()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null) return Mouse.current.position.ReadValue();
#endif
            return Input.mousePosition;
        }
    }
}
