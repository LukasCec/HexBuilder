using UnityEngine;

namespace HexBuilder.Systems.Map
{
    [RequireComponent(typeof(HexTile))]
    public class HexTileHover : MonoBehaviour
    {
        // ==== GLOBAL SUSPEND (nové) ===========================================
        // Keď je aspoň jeden BeginGlobalSuspend(), hover sa úplne vypne.
        static int _suspendCounter = 0;
        public static bool GlobalSuspend => _suspendCounter > 0;
        public static void BeginGlobalSuspend() { _suspendCounter++; }
        public static void EndGlobalSuspend() { if (_suspendCounter > 0) _suspendCounter--; }
        // =====================================================================

        [Header("Lift on hover")]
        public float liftHeight = 0.2f;
        public float liftSpeed = 10f;
        public float returnSpeed = 10f;

        [Header("Outline")]
        public OutlineOverlay overlayPrefab;
        public Transform overlayParent;
        public Color hoverColor = new Color(1f, 0.9f, 0.4f, 1f);
        public Color buildableColor = new Color(0.3f, 1f, 0.3f, 1f);
        public Color blockedColor = new Color(1f, 0.3f, 0.3f, 1f);
        [Range(1.0f, 1.2f)] public float radiusScale = 1.06f;
        public float lineWidth = 0.035f;

        OutlineOverlay overlayInst;
        float baseY;
        float targetY;
        bool hovered;
        bool canLift = true;

        HexTile tile;

        // FOLLOW bez parentovania
        HexBuilder.Systems.Buildings.BuildingInstance occ;
        float occBaseY;
        bool occActive;

        bool pendingSnapBack = false;

        void Awake()
        {
            tile = GetComponent<HexTile>();

            baseY = transform.position.y;
            targetY = baseY;

            if (!overlayParent)
            {
                var gen = FindObjectOfType<HexMapGenerator>();
                if (gen) overlayParent = gen.mapRoot;
            }

            // vodu nezdvíhame (má WaterBob)
            if (GetComponent<WaterBob>() != null)
                canLift = false;
        }

        void EnsureOverlay()
        {
            if (overlayInst) return;
            if (!overlayPrefab)
            {
                Debug.LogWarning("[HexTileHover] overlayPrefab not assigned.", this);
                return;
            }

            Transform parent = overlayParent ? overlayParent : transform.parent;
            overlayInst = Instantiate(overlayPrefab, parent);
            overlayInst.radiusScale = radiusScale;
            overlayInst.SetWidth(lineWidth);
            overlayInst.UpdatePoints();

            SyncOverlayTransform();
        }

        void SyncOverlayTransform()
        {
            if (!overlayInst) return;

            var pos = transform.position;
            pos.y = baseY; // outline ostáva na zemi
            overlayInst.transform.position = pos;

            var rot = overlayInst.transform.rotation.eulerAngles;
            rot.y = transform.rotation.eulerAngles.y;
            overlayInst.transform.rotation = Quaternion.Euler(rot.x, rot.y, rot.z);
            overlayInst.transform.localScale = Vector3.one;
        }

        public void SetHovered(bool isHovered, bool? buildable = null)
        {
            if (GlobalSuspend) return; // keď je globálne vypnuté, ignoruj

            if (hovered == isHovered) return;
            hovered = isHovered;

            if (isHovered)
            {
                EnsureOverlay();
                if (overlayInst)
                {
                    var c = buildable.HasValue ? (buildable.Value ? buildableColor : blockedColor) : hoverColor;
                    overlayInst.SetColor(c);
                    overlayInst.gameObject.SetActive(true);
                    SyncOverlayTransform();
                }

                if (canLift)
                {
                    // zachyť aktuálneho occupanta a jeho pôvodný Y
                    occ = tile ? tile.occupant : null;
                    if (occ != null)
                    {
                        occBaseY = occ.transform.position.y;
                        occActive = true;
                    }
                    else occActive = false;

                    pendingSnapBack = false;
                    targetY = baseY + liftHeight;
                }
            }
            else
            {
                if (overlayInst) overlayInst.gameObject.SetActive(false);

                if (canLift)
                {
                    targetY = baseY;
                    pendingSnapBack = true; // occupant vraciame až keď dosadneme
                }
            }
        }

        void Update()
        {
            if (GlobalSuspend)
            {
                // okamžite vypni vizuály a vráť occupanta
                if (overlayInst && overlayInst.gameObject.activeSelf) overlayInst.gameObject.SetActive(false);
                if (occActive && occ != null)
                {
                    var op = occ.transform.position; op.y = occBaseY; occ.transform.position = op;
                    occActive = false; occ = null; pendingSnapBack = false;
                }
                hovered = false;
                targetY = baseY;
                return;
            }

            // zdvih/vrat dlaždicu
            if (canLift)
            {
                var p = transform.position;
                float speed = hovered ? liftSpeed : returnSpeed;
                p.y = Mathf.Lerp(p.y, targetY, Time.deltaTime * speed);
                transform.position = p;

                // posuň aj occupanta rovnakým Δy (bez parentovania)
                if (occActive && occ != null)
                {
                    float dy = transform.position.y - baseY;
                    var op = occ.transform.position;
                    op.y = occBaseY + dy;
                    occ.transform.position = op;
                }

                // keď sme sa vrátili späť, snapni occupanta
                if (!hovered && pendingSnapBack && Mathf.Abs(transform.position.y - baseY) < 0.001f)
                {
                    if (occActive && occ != null)
                    {
                        var op = occ.transform.position;
                        op.y = occBaseY;
                        occ.transform.position = op;
                    }
                    occActive = false;
                    occ = null;
                    pendingSnapBack = false;
                }
            }

            if (overlayInst && overlayInst.gameObject.activeSelf)
                SyncOverlayTransform();
        }

        public void ResetBaseY(float newBaseY)
        {
            baseY = newBaseY;
            targetY = baseY;
            var p = transform.position; p.y = baseY; transform.position = p;

            if (occActive && occ != null)
            {
                var op = occ.transform.position;
                op.y = occBaseY;
                occ.transform.position = op;
                occActive = false;
                occ = null;
                pendingSnapBack = false;
            }

            SyncOverlayTransform();
        }

        void OnDisable()
        {
            if (occActive && occ != null)
            {
                var op = occ.transform.position;
                op.y = occBaseY;
                occ.transform.position = op;
            }
            occActive = false;
            occ = null;

            pendingSnapBack = false;
            if (hovered) { hovered = false; targetY = baseY; }
        }
    }
}
