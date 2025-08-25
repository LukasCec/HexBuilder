using UnityEngine;
using System;

namespace HexBuilder.Systems.Map
{
  
    public static class HexMetrics
    {
       
        public static float OuterRadius = 0.5f;
      
        public static float InnerRadius => OuterRadius * 0.86602540378f;

       
        public static float CenterStepX => Mathf.Sqrt(3f) * OuterRadius;
       
        public static float CenterStepZ => 1.5f * OuterRadius;

       
        public static Vector3 AxialToWorld(int q, int r, float y = 0f)
        {
            float x = CenterStepX * (q + r * 0.5f);
            float z = CenterStepZ * r;
            return new Vector3(x, y, z);
        }

       
        public static Vector2Int WorldToAxial(Vector3 worldPos)
        {
          
            float size = OuterRadius;
            float qf = (worldPos.x * Mathf.Sqrt(3f) / 3f - worldPos.z / 3f) / size;
            float rf = (2f / 3f * worldPos.z) / size;

           
            var cube = FractionalAxialToCube(qf, rf);
            var rounded = CubeRound(cube);
            return new Vector2Int(rounded.q, rounded.r);
        }

       
        public struct Cube
        {
            public float x, y, z; 
            public Cube(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        }

        public struct Axial
        {
            public int q, r;
            public Axial(int q, int r) { this.q = q; this.r = r; }
        }

        public static Cube AxialToCube(int q, int r)
        {
           
            return new Cube(q, -q - r, r);
        }

        public static Axial CubeToAxial(Cube c)
        {
           
            return new Axial(Mathf.RoundToInt(c.x), Mathf.RoundToInt(c.z));
        }

        public static Cube FractionalAxialToCube(float qf, float rf)
        {
            float xf = qf;
            float zf = rf;
            float yf = -xf - zf;
            return new Cube(xf, yf, zf);
        }

      
        public static Axial CubeRound(Cube frac)
        {
            int rx = Mathf.RoundToInt(frac.x);
            int ry = Mathf.RoundToInt(frac.y);
            int rz = Mathf.RoundToInt(frac.z);

            float dx = Mathf.Abs(rx - frac.x);
            float dy = Mathf.Abs(ry - frac.y);
            float dz = Mathf.Abs(rz - frac.z);

            if (dx > dy && dx > dz)
            {
                rx = -ry - rz;
            }
            else if (dy > dz)
            {
                ry = -rx - rz;
            }
            else
            {
                rz = -rx - ry;
            }

            return new Axial(rx, rz); 
        }
    }
}
