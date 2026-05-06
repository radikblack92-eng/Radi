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
        [Range(0, 6)] public int sphereSubdivisions = 4;
        [Min(0.05f)] public float sphereRadius = 0.55f;

        [Header("Simulation (SlimePlane)")]
        [Range(0.01f, 3f)] public float stiffness = 0.55f;      // spring back strength
        [Range(0f, 2f)] public float damping = 0.20f;           // velocity damping
        [Range(0f, 2f)] public float neighborTension = 0.35f;   // smoothing / cohesion
        [Range(0.01f, 0.5f)] public float maxVertexSpeed = 0.18f;

        [Header("Simulation (ClayBlob3D)")]
        [Range(0f, 4f)] public float clayRelaxation = 0.65f;     // how quickly it self-smooths
        [Range(0f, 2f)] public float claySmoothing = 0.55f;      // laplacian smoothing amount
        [Range(0.01f, 0.5f)] public float clayMaxStep = 0.08f;   // stability clamp

        [Header("Interaction")]
        [Min(0.01f)] public float deformRadius = 0.17f;
        [Min(0.001f)] public float tapDentStrength = 0.10f;    // dent / push
        [Min(0.001f)] public float dragPullStrength = 0.20f;   // grab / stretch
        [Min(0.001f)] public float dragDepth = 0.10f;          // legacy plane pull depth

        [Header("Rendering")]
        public Color initialColor = new(0.35f, 0.85f, 0.75f, 1f);
        [Range(1, 10)] public int normalsUpdateEveryNFrames = 2;

        [Header("Audio")]
        [Range(0f, 1f)] public float interactionVolume = 0.25f;
        [Range(0.02f, 0.5f)] public float minSoundIntervalSeconds = 0.07f;
    }
}

