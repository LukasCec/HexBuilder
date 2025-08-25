using UnityEngine;

namespace HexBuilder.Systems.Map
{
    public static class HexMetrics
    {
        // Vonkajší polomer (stred -> vrchol)
        public static float OuterRadius = 0.5f;

        // Vnútornı polomer (stred -> hrana) = R * sqrt(3)/2
        public static float InnerRadius => OuterRadius * 0.8660254037844386f; // sqrt(3)/2

        // -------------------------------
        //  POINTY-TOP KONVERZIE
        // -------------------------------

        // Axial (q,r) -> World (x,z)
        // x = R*sqrt(3) * (q + r/2)
        // z = R*3/2 * r
        public static Vector3 AxialToWorld(int q, int r, float y = 0f)
        {
            float x = OuterRadius * 1.7320508075688772f * (q + r * 0.5f); // sqrt(3)
            float z = OuterRadius * 1.5f * r;
            return new Vector3(x, y, z);
        }

        // World (x,z) -> Axial (q,r) s presnım cube-zaokrúhlením
        public static Vector2Int WorldToAxial(Vector3 world)
        {
            // analytická inverzia k AxialToWorld (pointy-top)
            float qf = (0.5773502691896258f * world.x - 0.3333333333333333f * world.z) / OuterRadius; // (sqrt(3)/3*x - 1/3*z)/R
            float rf = (0.6666666666666666f * world.z) / OuterRadius;                                  // (2/3*z)/R
            return RoundAxial(qf, rf);
        }

        // -------------------------------
        //  POMOCNÉ (kompatibilita s HexCoords)
        // -------------------------------

        // Axial -> Cube (x,y,z) kde x+y+z = 0
        public static Vector3Int AxialToCube(int q, int r)
        {
            int x = q;
            int z = r;
            int y = -x - z;
            return new Vector3Int(x, y, z);
        }

        // Cube -> Axial
        public static Vector2Int CubeToAxial(Vector3Int cube)
        {
            return new Vector2Int(cube.x, cube.z);
        }

        // Zaokrúhlenie cube súradníc (float) na najbliší platnı hex
        public static Vector3Int CubeRound(float x, float y, float z)
        {
            int rx = Mathf.RoundToInt(x);
            int ry = Mathf.RoundToInt(y);
            int rz = Mathf.RoundToInt(z);

            float x_diff = Mathf.Abs(rx - x);
            float y_diff = Mathf.Abs(ry - y);
            float z_diff = Mathf.Abs(rz - z);

            if (x_diff > y_diff && x_diff > z_diff)
                rx = -ry - rz;
            else if (y_diff > z_diff)
                ry = -rx - rz;
            else
                rz = -rx - ry;

            return new Vector3Int(rx, ry, rz);
        }

        // Zaokrúhlenie axial (float) pomocou cube-roundingu
        public static Vector2Int RoundAxial(float qf, float rf)
        {
            // preveï na cube, zaokrúhli, spä na axial
            var cube = CubeRound(qf, -qf - rf, rf);
            return CubeToAxial(cube);
        }
    }
}
