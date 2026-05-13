using UnityEngine;

namespace AntiStressLab.Slime
{
    [CreateAssetMenu(menuName = "AntiStress Lab/Slime Settings", fileName = "SlimeSettings")]
    public sealed class SlimeSettings : ScriptableObject
    {
        public enum MaterialMode
        {
            SlimePlane = 0,
            ClayBlob3D = 1
        }

        [Header("Mode")]
        public MaterialMode mode = MaterialMode.ClayBlob3D;

        [Header("Mesh (SlimePlane)")]
        [Min(8)] public int gridResolution = 40; // number of quads per side
        [Min(0.05f)] public float size = 1.2f;   // world size (square)

        [Header("Mesh (ClayBlob3D)")]
        [Range(0, 6)] public int sphereSubdivisions = 5;
        [Min(0.05f)] public float sphereRadius = 0.55f;

        [Header("Simulation (SlimePlane)")]
        [Range(0.01f, 3f)] public float stiffness = 0.55f;      // spring back strength
        [Range(0f, 2f)] public float damping = 0.20f;           // velocity damping
        [Range(0f, 2f)] public float neighborTension = 0.35f;   // smoothing / cohesion
        [Range(0.01f, 0.5f)] public float maxVertexSpeed = 0.18f;

        [Header("Simulation (ClayBlob3D)")]
        [Range(0f, 4f)] public float clayRelaxation = 0.2f;      // self-smooth speed (lower = holds shape longer)
        [Range(0f, 2f)] public float claySmoothing = 0.14f;      // laplacian blend toward neighbors (high = erodes dents)
        [Range(0.01f, 0.5f)] public float clayMaxStep = 0.045f;  // max vertex move per relax step
        [Range(0f, 1f)] public float clayVolumePreserve = 0.22f; // bulge outward when indenting (clay feel)
        [Range(0f, 1f)] public float clayGrabTangentRatio = 0.52f; // 0 = raw drag, 1 = mostly surface slide
        [Range(0.05f, 1f)] public float clayPostTouchSmoothMult = 0.12f; // smoothing multiplier right after touch (lower = crisper)
        [Range(0.1f, 2.5f)] public float clayPostTouchHoldSeconds = 0.75f; // how long weak-smoothing window lasts after sculpt

        [Header("Rendering (ClayBlob3D)")]
        [Range(0f, 1f)] public float clayGlossiness = 0.12f;
        [Range(0f, 0.3f)] public float clayMetallic = 0f;

        [Header("Interaction")]
        [Min(0.01f)] public float deformRadius = 0.19f;
        [Min(0.001f)] public float tapDentStrength = 0.12f;    // dent / push
        [Min(0.001f)] public float dragPullStrength = 0.28f;   // grab / stretch
        [Min(0.001f)] public float dragDepth = 0.10f;          // legacy plane pull depth

        [Header("Rendering")]
        public Color initialColor = new(0.9f, 0.72f, 0.62f, 1f);
        [Range(1, 10)] public int normalsUpdateEveryNFrames = 2;

        [Header("Audio")]
        [Range(0f, 1f)] public float interactionVolume = 0.25f;
        [Range(0.02f, 0.5f)] public float minSoundIntervalSeconds = 0.07f;
    }
}

