using UnityEngine;
using HexBuilder.Systems.Map;
using HexBuilder.Systems.Resources;

namespace HexBuilder.Systems.Buildings
{
    public class BuildingInstance : MonoBehaviour
    {
        public BuildingType type;
        public HexCoords coords; 
        public HexTile tile;     

        public void Bind(BuildingType t, HexTile onTile)
        {
            type = t;
            tile = onTile;
            coords = onTile.coords;
            onTile.occupant = this;
        }

        public void Unbind()
        {
            if (tile && tile.occupant == this)
                tile.occupant = null;
            tile = null;
        }


        public void Demolish(ResourceInventory inventory, float refundPercent = 0.5f)
        {
            if (!type) { Destroy(gameObject); return; }

            refundPercent = Mathf.Clamp01(refundPercent);

            int woodRefund = Mathf.RoundToInt(type.costWood * refundPercent);
            int stoneRefund = Mathf.RoundToInt(type.costStone * refundPercent);

            if (inventory)
            {
                if (woodRefund > 0) inventory.Add("wood", woodRefund);
                if (stoneRefund > 0) inventory.Add("stone", stoneRefund);
            }

            Unbind();
            Destroy(gameObject);
        }

    }
}
