using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using HexBuilder.Systems.Map;
using HexBuilder.Systems.Resources;

namespace HexBuilder.Systems.Buildings
{
    public class BuildingPlacer : MonoBehaviour
    {
        public enum Mode { None, Build, Move }

        [Header("Refs")]
        public Camera cam;
        public LayerMask tilesMask;
        public Transform buildingsParent;
        public ResourceInventory inventory;

        [Header("Ghost")]
        public float invalidTintAlpha = 0.5f;
        public Color validTint = new Color(0.4f, 1f, 0.4f, 0.6f);
        public Color invalidTint = new Color(1f, 0.3f, 0.3f, 0.6f);


        public bool buildMode => currentMode == Mode.Build;
        public BuildingType activeType { get; private set; }


        public bool moveMode => currentMode == Mode.Move;
        public float moveCostRate = 0.2f;
        BuildingInstance movingInstance;

 
        Mode currentMode = Mode.None;
        GameObject ghost;
        Renderer[] ghostRenderers;
        MaterialPropertyBlock mpb;
        float ghostYaw;

        void Start()
        {
            if (!cam) cam = Camera.main;
            if (tilesMask == 0) tilesMask = LayerMask.GetMask("Tiles");
            if (!inventory) inventory = FindObjectOfType<ResourceInventory>();

            if (!buildingsParent)
            {
                var gen = FindObjectOfType<HexMapGenerator>();
                if (gen) buildingsParent = gen.mapRoot;
            }
        }

        void Update()
        {
            if (currentMode == Mode.None) return;
            if (!cam) cam = Camera.main;

            HexTile tile = GetTileUnderMouse(out Vector3 hitPos);
            bool hasTile = tile != null;

           
            bool tileValid = hasTile && IsValidPlacement(tile);
            bool canPay = true;

            if (currentMode == Mode.Build && activeType != null && inventory != null)
            {
                canPay = inventory.CanAfford(activeType);
            }
            else if (currentMode == Mode.Move && movingInstance != null && inventory != null)
            {
                canPay = inventory.CanAffordMove(movingInstance.type, moveCostRate);
            }

            bool valid = tileValid && canPay;

         
            EnsureGhost();
            if (ghost)
            {
                var refType = (currentMode == Mode.Build) ? activeType : movingInstance?.type;
                Vector3 basePos = hasTile ? tile.transform.position : hitPos;

                if (refType != null)
                {
                    basePos += refType.localOffset;
                    basePos.y = (hasTile ? tile.transform.position.y : 0f) + refType.yOffset;
                }

                ghost.transform.position = basePos;

                Vector3 baseEuler = refType ? refType.defaultRotationEuler : Vector3.zero;
                ghost.transform.rotation = Quaternion.Euler(baseEuler + new Vector3(0f, ghostYaw, 0f));
                TintGhost(valid ? validTint : invalidTint);
            }

          
            if (KeyDown(KeyCode.Q)) ghostYaw -= 60f;
            if (KeyDown(KeyCode.E)) ghostYaw += 60f;

          
            if (LeftClickDown() && valid)
            {
                if (currentMode == Mode.Build)
                    PlaceNew(tile);
                else if (currentMode == Mode.Move)
                    MoveExisting(tile);
            }

          
            if (KeyDown(KeyCode.Escape))
            {
                ExitBuildMode();
                ExitMoveMode();
            }
        }

        // ---------- Public API ----------
        public void EnterBuildMode(BuildingType type)
        {
            currentMode = Mode.Build;
            activeType = type;
            movingInstance = null;
            ghostYaw = 0f;
            EnsureGhost(true);
        }

        public void ExitBuildMode()
        {
            if (currentMode != Mode.Build) return;
            currentMode = Mode.None;
            activeType = null;
            DestroyGhost();
        }

        public void EnterMoveMode(BuildingInstance inst)
        {
            if (inst == null || inst.type == null) return;
            currentMode = Mode.Move;
            movingInstance = inst;
            activeType = null;
            
            ghostYaw = inst.transform.rotation.eulerAngles.y - inst.type.defaultRotationEuler.y;
            EnsureGhost(true);
        }

        public void ExitMoveMode()
        {
            if (currentMode != Mode.Move) return;
            currentMode = Mode.None;
            movingInstance = null;
            DestroyGhost();
        }

        // ---------- Internals ----------
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

            var refType = (currentMode == Mode.Build) ? activeType : movingInstance?.type;
            if (refType == null) return false;

           
            if (currentMode == Mode.Move && movingInstance != null && tile == movingInstance.tile)
                return false;

            
            if (tile.IsOccupied) return false;

            var terr = tile.terrain;
            if (!terr || !terr.buildable) return false;

      
            if (refType.allowedTerrains != null && refType.allowedTerrains.Count > 0)
            {
                if (!refType.allowedTerrains.Contains(terr)) return false;
            }

            return true;
        }

        void PlaceNew(HexTile tile)
        {
            if (!tile || !activeType) return;

            if (inventory && !inventory.CanAfford(activeType))
            {
                Debug.Log("[Build] Not enough resources.");
                return;
            }

            var pos = tile.transform.position + activeType.localOffset;
            pos.y += activeType.yOffset;
            var rot = Quaternion.Euler(activeType.defaultRotationEuler + new Vector3(0f, ghostYaw, 0f));

            inventory?.Pay(activeType);

            var go = Instantiate(activeType.prefab, pos, rot, buildingsParent);
            var inst = go.GetComponent<BuildingInstance>();
            if (!inst) inst = go.AddComponent<BuildingInstance>();
            inst.Bind(activeType, tile);
        }

        void MoveExisting(HexTile targetTile)
        {
            if (!targetTile || movingInstance == null || movingInstance.type == null) return;

            if (inventory && !inventory.CanAffordMove(movingInstance.type, moveCostRate))
            {
                Debug.Log("[Move] Not enough resources for relocation.");
                return;
            }

   
            var oldTile = movingInstance.tile;
            if (oldTile != null && oldTile.occupant == movingInstance)
                oldTile.occupant = null;

           
            var type = movingInstance.type;
            var pos = targetTile.transform.position + type.localOffset;
            pos.y += type.yOffset;

            var rot = Quaternion.Euler(type.defaultRotationEuler + new Vector3(0f, ghostYaw, 0f));
            movingInstance.transform.SetPositionAndRotation(pos, rot);

            movingInstance.Bind(type, targetTile);

            inventory?.PayMove(type, moveCostRate);

           
            ExitMoveMode();
        }

        void EnsureGhost(bool forceRecreate = false)
        {
            if (forceRecreate) DestroyGhost();
            if (ghost) { if (ghostRenderers == null) ghostRenderers = ghost.GetComponentsInChildren<Renderer>(true); return; }

            var refType = (currentMode == Mode.Build) ? activeType : movingInstance?.type;
            if (refType == null) return;

            ghost = Instantiate(refType.prefab, buildingsParent ? buildingsParent : null);
            ghost.name = currentMode == Mode.Build ? $"_GHOST_BUILD_{refType.displayName}" : $"_GHOST_MOVE_{refType.displayName}";
            ghostRenderers = ghost.GetComponentsInChildren<Renderer>(true);

            // vypni kolidery
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
                int baseColorId = r.sharedMaterial.HasProperty("_BaseColor")
                    ? Shader.PropertyToID("_BaseColor")
                    : Shader.PropertyToID("_Color");

                r.GetPropertyBlock(mpb);
                mpb.SetColor(baseColorId, c);
                mpb.SetFloat("_Surface", 1);
                mpb.SetFloat("_ZWrite", 0);
                r.SetPropertyBlock(mpb);
            }
        }


        public bool HandleEscapeFromModes()
        {
           
            if (moveMode)
            {


                ExitMoveMode();               
                return true;               
            }

            
            if (buildMode)
            {
                ExitBuildMode();
                return true;              
            }

            
            return false;
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
            return k switch
            {
                KeyCode.Q => kb.qKey.wasPressedThisFrame,
                KeyCode.E => kb.eKey.wasPressedThisFrame,
                KeyCode.R => kb.rKey.wasPressedThisFrame,
                KeyCode.Escape => kb.escapeKey.wasPressedThisFrame,
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
