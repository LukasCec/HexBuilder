using UnityEngine;

namespace HexBuilder.Systems.Map
{
    [RequireComponent(typeof(LineRenderer))]
    public class OutlineOverlay : MonoBehaviour
    {
        [Header("Shape")]
        [Tooltip("Scale > 1.0 = o trochu väèší než tile.")]
        public float radiusScale = 1.06f;

        [Tooltip("Výška nad povrchom, aby neblikalo so zemou.")]
        public float yOffset = 0.05f;

        [Tooltip("Uhol prvého rohu v stupòoch. Pre pointy-top zaèni 0; ak by nesedelo, skús 30 alebo -30.")]
        public float startAngleDeg = 0f;

        [Header("Style")]
        public Color color = new Color(1f, 1f, 1f, 0.95f);
        public float lineWidth = 0.035f;

        LineRenderer lr;

        void Awake()
        {
            lr = GetComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.loop = true;
            lr.positionCount = 7;
            lr.startWidth = lr.endWidth = lineWidth;
            lr.startColor = lr.endColor = color;
            UpdatePoints();
        }

        public void SetColor(Color c)
        {
            color = c;
            if (!lr) lr = GetComponent<LineRenderer>();
            lr.startColor = lr.endColor = c;
        }

        public void SetWidth(float w)
        {
            lineWidth = w;
            if (!lr) lr = GetComponent<LineRenderer>();
            lr.startWidth = lr.endWidth = w;
        }

        public void UpdatePoints()
        {
            if (!lr) lr = GetComponent<LineRenderer>();

            float R = HexMetrics.OuterRadius * radiusScale;
            for (int i = 0; i < 6; i++)
            {
                float angleDeg = startAngleDeg + 60f * i;
                float rad = angleDeg * Mathf.Deg2Rad;
                float x = R * Mathf.Cos(rad);
                float z = R * Mathf.Sin(rad);
                lr.SetPosition(i, new Vector3(x, yOffset, z));
            }
            lr.SetPosition(6, lr.GetPosition(0));
        }
    }
}
