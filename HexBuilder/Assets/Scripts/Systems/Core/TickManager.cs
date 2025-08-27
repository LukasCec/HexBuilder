using System;
using UnityEngine;

namespace HexBuilder.Systems.Core
{
    
    public class TickManager : MonoBehaviour
    {
        [Tooltip("Sekundy medzi tickmi")]
        public float tickInterval = 1f;

        [Tooltip("Pozastavi tiky")]
        public bool paused = false;

        public event Action OnTick;

        float acc;

        void Update()
        {
            if (paused) return;
            acc += Time.deltaTime;
            if (acc >= tickInterval)
            {
                acc -= tickInterval;
                OnTick?.Invoke();
            }
        }
    }
}
