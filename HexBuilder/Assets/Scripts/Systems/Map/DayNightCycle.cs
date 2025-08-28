using UnityEngine;

namespace HexBuilder.Visual
{
    [ExecuteAlways]
    public class DayNightCycle : MonoBehaviour
    {
        [Header("Sun / Directional Light")]
        public Light sun;                           // priraÔ tvoj Directional Light

        [Header("Timing")]
        [Tooltip("DÂûka celÈho dÚa v sekund·ch (svitanie->noc->svitanie).")]
        public float dayLengthSeconds = 300f;
        [Range(0f, 1f)]
        [Tooltip("0 = svitanie, 0.25 = poludnie, 0.5 = s˙mrak, 0.75 = polnoc, 1 = sp‰ù svitanie")]
        public float timeOfDay = 0.25f;

        [Header("Sun Look & Feel")]
        public AnimationCurve sunIntensity = new AnimationCurve(
            new Keyframe(0.00f, 0.0f),    // svitanie
            new Keyframe(0.15f, 0.7f),
            new Keyframe(0.25f, 1.0f),    // poludnie
            new Keyframe(0.35f, 0.7f),
            new Keyframe(0.50f, 0.0f),    // s˙mrak
            new Keyframe(0.75f, 0.0f),    // noc
            new Keyframe(1.00f, 0.0f)
        );

        public Gradient sunColor = new Gradient
        {
            colorKeys = new[]
            {
                new GradientColorKey(new Color(1.0f, 0.65f, 0.45f), 0.00f), // svitanie
                new GradientColorKey(new Color(1.0f, 0.95f, 0.85f), 0.20f),
                new GradientColorKey(new Color(1.0f, 1.0f,  1.0f),  0.25f), // poludnie
                new GradientColorKey(new Color(1.0f, 0.9f,  0.8f),  0.35f),
                new GradientColorKey(new Color(0.9f, 0.4f,  0.2f),  0.50f), // s˙mrak
                new GradientColorKey(new Color(0.1f, 0.15f, 0.2f),  0.75f), // noc
                new GradientColorKey(new Color(1.0f, 0.65f, 0.45f), 1.00f)
            },
            alphaKeys = new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            }
        };

        [Header("Ambient / Sky")]
        [Tooltip("Ambient svetlo poËas dÚa/noci.")]
        public Gradient ambientColor = new Gradient
        {
            colorKeys = new[]
            {
                new GradientColorKey(new Color(0.25f,0.3f,0.35f), 0.00f), // svitanie chladnÈ
                new GradientColorKey(new Color(0.6f, 0.65f,0.7f ), 0.25f), // deÚ
                new GradientColorKey(new Color(0.2f, 0.22f,0.25f), 0.75f), // noc
                new GradientColorKey(new Color(0.25f,0.3f,0.35f),  1.00f)
            },
            alphaKeys = new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            }
        };

        [Tooltip("VoliteænÈ: skybox materi·l (procedur·lny/PBR). Ak vyplnenÈ, bude sa meniù exposure.")]
        public Material skyboxMaterial;
        [Tooltip("Ako sa menÌ exposure skyboxu v priebehu dÚa.")]
        public AnimationCurve skyboxExposure = new AnimationCurve(
            new Keyframe(0.00f, 0.8f),
            new Keyframe(0.15f, 1.0f),
            new Keyframe(0.25f, 1.2f),
            new Keyframe(0.50f, 0.6f),
            new Keyframe(0.75f, 0.25f),
            new Keyframe(1.00f, 0.8f)
        );

        [Header("Rotation")]
        [Tooltip("Lok·lny n·klon slnka (X) ñ napr. 45 pre krajöie tiene.")]
        public float sunTilt = 45f;
        [Tooltip("Svet vych·dza zo smeru (svitanie = 0%) a toËÌ sa dookola.")]
        public Vector3 sunAxis = Vector3.right; // rotuj okolo X; tilt si dobieha rot·cia parentu (niûöie)

        public bool playInEditMode = true;

        void Reset()
        {
            if (!sun) sun = GetComponent<Light>();
        }

        void Update()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && !playInEditMode)
                return;
#endif
            if (sun == null) return;

            // 1) posun Ëasu
            if (Application.isPlaying && dayLengthSeconds > 0.01f)
            {
                timeOfDay += Time.deltaTime / dayLengthSeconds;
                timeOfDay -= Mathf.Floor(timeOfDay); // wrap 0..1
            }

            // 2) rot·cia slnka
            // 0.0 -> svitanie (nÌzko nad horizontom), 0.25 -> poludnie, 0.5 -> s˙mrak, 0.75 -> polnoc
            float angle = timeOfDay * 360f;
            // z·kladn· rot·cia okolo X (sunAxis), plus jemn˝ tilt aby svetlo nebolo ˙plne kolmÈ
            Quaternion q = Quaternion.AngleAxis(sunTilt, Vector3.forward) * Quaternion.AngleAxis(angle, sunAxis);
            sun.transform.rotation = q;

            // 3) vizu·ly (farba, intenzita, ambient, skybox)
            float intensity = sunIntensity.Evaluate(timeOfDay);
            sun.intensity = intensity;
            sun.color = sunColor.Evaluate(timeOfDay);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = ambientColor.Evaluate(timeOfDay);

            if (skyboxMaterial)
            {
                // beûnÈ property pre Procedural Skybox: _Exposure
                float exp = skyboxExposure.Evaluate(timeOfDay);
                skyboxMaterial.SetFloat("_Exposure", exp);
                RenderSettings.skybox = skyboxMaterial;
                // Pri zmene materi·lu v runtime:
                DynamicGI.UpdateEnvironment();
            }
        }

        // --- Save/Load API ---
        public float GetTimeOfDay() => timeOfDay;
        public void SetTimeOfDay(float t)
        {
            timeOfDay = Mathf.Repeat(t, 1f);
            // okamûite ÑprepoËÌtaùì jednu frame logiku
#if UNITY_EDITOR
            if (!Application.isPlaying && !playInEditMode) return;
#endif
            if (sun != null)
            {
                float angle = timeOfDay * 360f;
                Quaternion q = Quaternion.AngleAxis(sunTilt, Vector3.forward) * Quaternion.AngleAxis(angle, sunAxis);
                sun.transform.rotation = q;

                sun.intensity = sunIntensity.Evaluate(timeOfDay);
                sun.color = sunColor.Evaluate(timeOfDay);
            }
            RenderSettings.ambientLight = ambientColor.Evaluate(timeOfDay);
            if (skyboxMaterial)
            {
                skyboxMaterial.SetFloat("_Exposure", skyboxExposure.Evaluate(timeOfDay));
                RenderSettings.skybox = skyboxMaterial;
                DynamicGI.UpdateEnvironment();
            }
        }
    }
}
