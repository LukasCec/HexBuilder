using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using HexBuilder.Systems.Map;

namespace HexBuilder.Systems.Buildings
{
    
    public class BuildingPlacer : MonoBehaviour
    {
        [Header("Refs")]
        public Camera cam;
        public LayerMask tilesMask;       
        public Transform buildingsParent; 

        [Header("Ghost")]
        public float invalidTintAlpha = 0.5f;
        public Color validTint = new Color(0.4f, 1f, 0.4f, 0.6f);
        public Color invalidTint = new Color(1f, 0.3f, 0.3f, 0.6f);

       
        public bool buildMode { get; private set; }
        public BuildingType activeType { get; private set; }
        GameObject ghost;
        Renderer[] ghostRenderers;
        MaterialPropertyBlock mpb;
        float ghostYaw;

        void Start()
        {
            if (!cam) cam = Camera.main;
            if (tilesMask == 0) tilesMask = LayerMask.GetMask("Tiles");

            if (!buildingsParent)
            {
                var gen = FindObjectOfType<HexMapGenerator>();
                if (gen) buildingsParent = gen.mapRoot;
            }
        }

        void Update()
        {
            if (!buildMode || activeType == null) return;
            if (!cam) cam = Camera.main;

          
            HexTile tile = GetTileUnderMouse(out Vector3 hitPos);
            bool valid = tile && IsValidPlacement(tile);

          
            EnsureGhost();
            if (ghost)
            {
                Vector3 basePos = tile ? tile.transform.position : hitPos;
                basePos += activeType.localOffset;
                basePos.y = (tile ? tile.transform.position.y : 0f) + activeType.yOffset;
                ghost.transform.position = basePos;
                ghost.transform.rotation = Quaternion.Euler(activeType.defaultRotationEuler + new Vector3(0f, ghostYaw, 0f));
                TintGhost(valid ? validTint : invalidTint);
            }

           
            if (KeyDown(KeyCode.Q)) ghostYaw -= 60f;
            if (KeyDown(KeyCode.E)) ghostYaw += 60f;

          
            if (LeftClickDown() && valid)
                Place(tile);

          
            if (KeyDown(KeyCode.Escape))
                ExitBuildMode();
        }

        // ---------- Public API (UI hooky) ----------
        public void EnterBuildMode(BuildingType type)
        {
            activeType = type;
            buildMode = true;
            ghostYaw = 0f;
            EnsureGhost(true);
        }

        public void ExitBuildMode()
        {
            buildMode = false;
            activeType = null;
            DestroyGhost();
        }

       
        HexTile GetTileUnderMouse(out Vector3 planeHit)
        {
            var ray = cam.ScreenPointToRay(GetMousePos());
            if (Physics.Raycast(ray, out var hit, 1000f, tilesMask, QueryTriggerInteraction.Ignore))
            {
                planeHit = hit.point;
                return hit.collider.GetComponentInParent<HexTile>();
            }
            planeHit = Vector3.zero;
            return null;
        }

        bool IsValidPlacement(HexTile tile)
        {
            if (tile == null) return false;
            if (tile.IsOccupied) return false;
            var terr = tile.terrain;
            if (!terr || !terr.buildable) return false;

          
            if (activeType.allowedTerrains != null && activeType.allowedTerrains.Count > 0)
            {
                if (!activeType.allowedTerrains.Contains(terr)) return false;
            }
            return true;
        }

        void Place(HexTile tile)
        {
            if (!tile || !activeType) return;

            var pos = tile.transform.position + activeType.localOffset;
            pos.y += activeType.yOffset;
            var rot = Quaternion.Euler(activeType.defaultRotationEuler + new Vector3(0f, ghostYaw, 0f));

            var go = Instantiate(activeType.prefab, pos, rot, buildingsParent);
            var inst = go.GetComponent<BuildingInstance>();
            if (!inst) inst = go.AddComponent<BuildingInstance>();
            inst.Bind(activeType, tile);

           

           
           
        }

        void EnsureGhost(bool forceRecreate = false)
        {
            if (forceRecreate) DestroyGhost();
            if (ghost || activeType == null) { if (ghostRenderers == null && ghost) ghostRenderers = ghost.GetComponentsInChildren<Renderer>(true); return; }

            ghost = Instantiate(activeType.prefab, buildingsParent ? buildingsParent : null);
            ghost.name = $"_GHOST_{activeType.displayName}";
            ghostRenderers = ghost.GetComponentsInChildren<Renderer>(true);

           
            foreach (var col in ghost.GetComponentsInChildren<Collider>(true))
                col.enabled = false;

           
            TintGhost(invalidTint);
        }

        void DestroyGhost()
        {
            if (ghost) Destroy(ghost);
            ghost = null;
            ghostRenderers = null;
        }

        void TintGhost(Color c)
        {
            if (ghostRenderers == null) return;
            mpb ??= new MaterialPropertyBlock();
            foreach (var r in ghostRenderers)
            {
               
                int baseColorId = r.sharedMaterial.HasProperty("_BaseColor") ? Shader.PropertyToID("_BaseColor")
                                                                              : Shader.PropertyToID("_Color");
                r.GetPropertyBlock(mpb);
                mpb.SetColor(baseColorId, c);
                mpb.SetFloat("_Surface", 1);   
                mpb.SetFloat("_ZWrite", 0);
                r.SetPropertyBlock(mpb);
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
        bool KeyDown(KeyCode k)
        {
#if ENABLE_INPUT_SYSTEM
    var kb = UnityEngine.InputSystem.Keyboard.current;
    if (kb == null) return false;

    // pridaj si ïalšie klávesy pod¾a potreby
    return k switch
    {
        KeyCode.Q       => kb.qKey.wasPressedThisFrame,
        KeyCode.E       => kb.eKey.wasPressedThisFrame,
        KeyCode.R       => kb.rKey.wasPressedThisFrame,
        KeyCode.Escape  => kb.escapeKey.wasPressedThisFrame,
        KeyCode.LeftShift=> kb.leftShiftKey.wasPressedThisFrame,
        KeyCode.RightShift=>kb.rightShiftKey.wasPressedThisFrame,
        _ => false
    };
#else
            return Input.GetKeyDown(k);
#endif
        }
        Vector2 GetMousePos()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null) return Mouse.current.position.ReadValue();
#endif
            return Input.mousePosition;
        }
    }
}
