using UnityEngine;

namespace HexBuilder.Systems.Map
{
    [RequireComponent(typeof(HexTile))]
    public class HexTileHover : MonoBehaviour
    {
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

        // --- NEW: refs na tile a prÌpadnÈho ìoccupantaî (budovu) ---
        HexTile tile;
        HexBuilder.Systems.Buildings.BuildingInstance hoveredOccupant;
        Transform originalOccupantParent;

        bool pendingDetach = false;

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

            // vodu nezdvÌhame (m· WaterBob)
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
            pos.y = baseY; // outline ost·va na zemi
            overlayInst.transform.position = pos;

            var rot = overlayInst.transform.rotation.eulerAngles;
            rot.y = transform.rotation.eulerAngles.y;
            overlayInst.transform.rotation = Quaternion.Euler(rot.x, rot.y, rot.z);
            overlayInst.transform.localScale = Vector3.one;
        }

        public void SetHovered(bool isHovered, bool? buildable = null)
        {
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
                    AttachOccupant();            // pripn˙ù hneÔ
                    pendingDetach = false;       // ruöÌme prÌpadnÈ Ëakanie na odpojenie
                    targetY = baseY + liftHeight;
                }
            }
            else
            {
                if (overlayInst) overlayInst.gameObject.SetActive(false);

                if (canLift)
                {
                    targetY = baseY;
                    // NEodpojuj teraz ñ poËk·me, k˝m dlaûdica dosadne v Update()
                    pendingDetach = true;
                }
            }
        }

        void Update()
        {
            if (canLift)
            {
                var p = transform.position;
                float speed = hovered ? liftSpeed : returnSpeed;
                p.y = Mathf.Lerp(p.y, targetY, Time.deltaTime * speed);
                transform.position = p;

                
                if (!hovered && pendingDetach && Mathf.Abs(transform.position.y - baseY) < 0.001f)
                {
                    DetachOccupant();
                    pendingDetach = false;
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
            SyncOverlayTransform();
        }

        // --- NEW: helpery na pripnutie/odpojenie occupanta (budovy) ---
        void AttachOccupant()
        {
            if (!tile) tile = GetComponent<HexTile>();
            var occ = tile ? tile.occupant : null;
            if (!occ) return;
            if (hoveredOccupant == occ) return;

            hoveredOccupant = occ;
            originalOccupantParent = occ.transform.parent;
            occ.transform.SetParent(transform, worldPositionStays: true);
        }

        void DetachOccupant()
        {
            if (!hoveredOccupant) return;
            hoveredOccupant.transform.SetParent(originalOccupantParent, worldPositionStays: true);
            hoveredOccupant = null;
            originalOccupantParent = null;
        }

        void OnDisable()
        {
            pendingDetach = false;
            if (hovered) { hovered = false; targetY = baseY; }
            DetachOccupant();
        }
    }
}
