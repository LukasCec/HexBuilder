using UnityEngine;
using System;

namespace HexBuilder.Systems.Map
{
    
    [Serializable]
    public struct HexCoords : IEquatable<HexCoords>
    {
        public int q;
        public int r; 

        public HexCoords(int q, int r)
        {
            this.q = q; this.r = r;
        }

        public override string ToString() => $"({q},{r})";

       
        public bool Equals(HexCoords other) => q == other.q && r == other.r;
        public override bool Equals(object obj) => obj is HexCoords hc && Equals(hc);
        public override int GetHashCode() => (q * 397) ^ r;

        public static HexCoords operator +(HexCoords a, HexCoords b) => new HexCoords(a.q + b.q, a.r + b.r);
        public static HexCoords operator -(HexCoords a, HexCoords b) => new HexCoords(a.q - b.q, a.r - b.r);

      
        public static readonly HexCoords[] Directions = new HexCoords[]
        {
            new HexCoords(+1, 0),   // E
            new HexCoords(+1, -1),  // NE
            new HexCoords(0, -1),   // NW
            new HexCoords(-1, 0),   // W
            new HexCoords(-1, +1),  // SW
            new HexCoords(0, +1),   // SE
        };

        public HexCoords Neighbor(int dir)
        {
            dir = Mathf.FloorToInt(Mathf.Repeat(dir, 6));
            return this + Directions[dir];
        }

        public static int Distance(HexCoords a, HexCoords b)
        {
           
            var ac = HexMetrics.AxialToCube(a.q, a.r);
            var bc = HexMetrics.AxialToCube(b.q, b.r);
            int dx = Mathf.Abs(Mathf.RoundToInt(ac.x - bc.x));
            int dy = Mathf.Abs(Mathf.RoundToInt(ac.y - bc.y));
            int dz = Mathf.Abs(Mathf.RoundToInt(ac.z - bc.z));
            return Mathf.Max(dx, Mathf.Max(dy, dz));
        }

       
        public Vector3 ToWorld(float y = 0f) => HexMetrics.AxialToWorld(q, r, y);

       
        public static HexCoords FromWorld(Vector3 worldPos)
        {
            var ar = HexMetrics.WorldToAxial(worldPos);
            return new HexCoords(ar.x, ar.y);
        }
    }
}
