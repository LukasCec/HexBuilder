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
        public OutlineOverlay overlayPrefab;           // OutlineOverlay.prefab
        public Transform overlayParent;                // nastav na /_Bootstrap/MapRoot (alebo nechaj pr·zdne)
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

        void Awake()
        {
            baseY = transform.position.y;
            targetY = baseY;

            if (!overlayParent)
            {
                var gen = FindObjectOfType<HexMapGenerator>();
                if (gen) overlayParent = gen.mapRoot;
            }

            // vodu nech·me nezdvÌhaù (m· WaterBob)
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

            // D‘LEéIT…: inötancuj pod overlayParent (napr. MapRoot), nie pod tile
            Transform parent = overlayParent ? overlayParent : transform.parent;
            overlayInst = Instantiate(overlayPrefab, parent);

            // nastav tvar/öt˝l
            overlayInst.radiusScale = radiusScale;
            overlayInst.SetWidth(lineWidth);
            overlayInst.UpdatePoints();

            // prvotnÈ umiestnenie a rot·cia zodpovedaj˙ca tile
            SyncOverlayTransform();
        }

        void SyncOverlayTransform()
        {
            if (!overlayInst) return;

            // drû outline v strede tile, ale na zemi (baseY), s rovnakou Y-rot·ciou ako tile
            var pos = transform.position;
            pos.y = baseY;  // nezodvihne sa s tile
            overlayInst.transform.position = pos;

            var rot = overlayInst.transform.rotation.eulerAngles;
            rot.y = transform.rotation.eulerAngles.y;   // rovnakÈ natoËenie
            overlayInst.transform.rotation = Quaternion.Euler(rot.x, rot.y, rot.z);
            overlayInst.transform.localScale = Vector3.one; // bez dedenia scale
        }

        public void SetHovered(bool isHovered, bool? buildable = null)
        {
            hovered = isHovered;

            if (isHovered)
            {
                EnsureOverlay();
                if (overlayInst)
                {
                    var c = buildable.HasValue ? (buildable.Value ? buildableColor : blockedColor) : hoverColor;
                    overlayInst.SetColor(c);
                    overlayInst.gameObject.SetActive(true);
                    SyncOverlayTransform(); // hneÔ zos˙ladiù polohu/rot·ciu
                }
            }
            else
            {
                if (overlayInst) overlayInst.gameObject.SetActive(false);
            }

            if (canLift)
                targetY = baseY + (hovered ? liftHeight : 0f);
        }

        void Update()
        {
            // zdvih/vrat
            if (canLift)
            {
                var p = transform.position;
                float speed = hovered ? liftSpeed : returnSpeed;
                p.y = Mathf.Lerp(p.y, targetY, Time.deltaTime * speed);
                transform.position = p;
            }

            // outline drû na zemi a v strede tile aj poËas pohybu
            if (overlayInst && overlayInst.gameObject.activeSelf)
            {
                SyncOverlayTransform();
            }
        }

        // ak by sa poËas regener·cie menil base Y, mÙûeö zavolaù
        public void ResetBaseY(float newBaseY)
        {
            baseY = newBaseY;
            targetY = baseY;
            var p = transform.position; p.y = baseY; transform.position = p;
            SyncOverlayTransform();
        }
    }
}
