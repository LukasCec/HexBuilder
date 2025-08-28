using System.IO;
using UnityEngine;
using HexBuilder.Systems.Buildings;
using HexBuilder.Systems.Map;
using HexBuilder.Systems.Resources;
using System.Collections.Generic;

namespace HexBuilder.Systems.Save
{
    public static class SaveSystem
    {
        
        public static int pendingLoadSlot = 0;

        static string Dir => Application.persistentDataPath;
        static string FilePath(int slot) => Path.Combine(Dir, $"save{slot}.json");

        public static bool HasSave(int slot) => File.Exists(FilePath(slot));
        public static void Delete(int slot) { var p = FilePath(slot); if (File.Exists(p)) File.Delete(p); }

        public static void Save(int slot)
        {
            var gen = Object.FindObjectOfType<HexMapGenerator>();
            var inv = Object.FindObjectOfType<ResourceInventory>();
            var buildings = Object.FindObjectsOfType<BuildingInstance>();

            var data = new SaveGame
            {
                savedAtUnix = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                seed = gen ? gen.lastSeed : 0
            };

            if (inv != null)
            {
                var (w, s, wa) = inv.GetAll();
                data.resources.wood = w;
                data.resources.stone = s;
                data.resources.water = wa;
            }

            foreach (var b in buildings)
            {
                if (!b || b.type == null) continue;
                data.buildings.Add(new SaveBuilding
                {
                    typeId = b.type.id,
                    q = b.coords.q,
                    r = b.coords.r,
                    yaw = b.transform.rotation.eulerAngles.y
                });
            }

            Directory.CreateDirectory(Dir);
            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(FilePath(slot), json);
            Debug.Log($"[Save] Wrote {data.buildings.Count} buildings to {FilePath(slot)}");
        }

        public static SaveGame Peek(int slot)
        {
            var p = FilePath(slot);
            if (!File.Exists(p)) return null;
            var json = File.ReadAllText(p);
            return JsonUtility.FromJson<SaveGame>(json);
        }

        public static void LoadIntoCurrentScene(int slot, BuildingTypeRegistry registry, Transform buildingsParent = null)
        {
            var p = FilePath(slot);
            if (!File.Exists(p))
            {
                Debug.LogWarning($"[Load] Slot {slot} neexistuje.");
                return;
            }

            var json = File.ReadAllText(p);
            var data = JsonUtility.FromJson<SaveGame>(json);

            var gen = Object.FindObjectOfType<HexMapGenerator>();
            if (gen == null) { Debug.LogError("[Load] HexMapGenerator nenájdený."); return; }

           
            gen.GenerateFromSeed(data.seed);

           
            var exist = Object.FindObjectsOfType<BuildingInstance>();
            foreach (var e in exist) Object.Destroy(e.gameObject);

           
            if (!registry) { Debug.LogError("[Load] Missing BuildingTypeRegistry"); return; }
            if (!buildingsParent) buildingsParent = gen.mapRoot;

            foreach (var sb in data.buildings)
            {
                var type = registry.GetById(sb.typeId);
                if (!type) { Debug.LogWarning($"[Load] Unknown typeId '{sb.typeId}'"); continue; }

                var key = HexMapGenerator.Key(sb.q, sb.r);
                if (!HexMapGenerator.TileIndexByKey.TryGetValue(key, out var tile))
                {
                    Debug.LogWarning($"[Load] Tile {key} nenájdený.");
                    continue;
                }

                var pos = tile.transform.position + type.localOffset;
                pos.y += type.yOffset;
                var rot = Quaternion.Euler(type.defaultRotationEuler + new Vector3(0f, sb.yaw, 0f));

                var go = Object.Instantiate(type.prefab, pos, rot, buildingsParent);
                var inst = go.GetComponent<BuildingInstance>();
                if (!inst) inst = go.AddComponent<BuildingInstance>();
                inst.Bind(type, tile);
            }

           
            var inv = Object.FindObjectOfType<ResourceInventory>();
            if (inv != null) inv.SetAll(data.resources.wood, data.resources.stone, data.resources.water);

            Debug.Log($"[Load] Loaded slot {slot}: buildings={data.buildings.Count}, seed={data.seed}");
        }
    }
}
