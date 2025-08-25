using UnityEngine;

namespace HexBuilder.Systems.Map
{
    
    public class WaterBob : MonoBehaviour
    {
        [Header("Wave range (relative to baseY)")]
        public float minOffsetY = -0.12f;
        public float maxOffsetY = -0.05f;

        [Header("Speed (seconds scale)")]
        [Tooltip("Min a max n·sobiteæ Ëasu pre Perlin; kaûd· inötancia si vyûre r˝chlosù medzi nimi.")]
        public float speedMin = 0.15f;
        public float speedMax = 0.28f;

        [Header("Randomness")]
        [Tooltip("Kaûd˝ tile dostane n·hodn˙ f·zu, aby sa neh˝bali rovnako.")]
        public float phaseSeed;  

        float baseY;              
        float speed;             

        bool initialized;

        void Awake()
        {
           
            baseY = transform.position.y;

          
            phaseSeed = UnityEngine.Random.Range(0f, 10000f);
            speed = UnityEngine.Random.Range(speedMin, speedMax);

            initialized = true;
        }

        public void Initialize(float baseYOverride, float minY, float maxY, float spdMin = 0.08f, float spdMax = 0.18f)
        {
            baseY = baseYOverride;
            minOffsetY = minY;
            maxOffsetY = maxY;
            speedMin = spdMin;
            speedMax = spdMax;
            phaseSeed = UnityEngine.Random.Range(0f, 10000f);
            speed = UnityEngine.Random.Range(speedMin, speedMax);
            initialized = true;
        }

        void Update()
        {
            if (!initialized) return;

           
            float t = Mathf.PerlinNoise(phaseSeed, Time.time * speed);

           
            float yOffset = Mathf.Lerp(minOffsetY, maxOffsetY, t);

          
            var p = transform.position;
            p.y = baseY + yOffset;
            transform.position = p;
        }
    }
}
