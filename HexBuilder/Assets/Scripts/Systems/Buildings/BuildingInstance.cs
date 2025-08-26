using UnityEngine;
using HexBuilder.Systems.Map;

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
    }
}
