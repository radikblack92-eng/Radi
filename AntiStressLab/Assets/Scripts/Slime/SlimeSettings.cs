using UnityEngine;

namespace AntiStressLab.Slime
{
    [CreateAssetMenu(menuName = "AntiStress Lab/Slime Settings", fileName = "SlimeSettings")]
    public sealed class SlimeSettings : ScriptableObject
    {
        [Header("Mesh")]
        [Min(8)] public int gridResolution = 40; // number of quads per side
        [Min(0.05f)] public float size = 1.2f;   // world size (square)

        [Header("Simulation")]
        [Range(0.01f, 3f)] public float stiffness = 0.55f;      // spring back strength
        [Range(0f, 2f)] public float damping = 0.20f;           // velocity damping
        [Range(0f, 2f)] public float neighborTension = 0.35f;   // smoothing / cohesion
        [Range(0.01f, 0.5f)] public float maxVertexSpeed = 0.18f;

        [Header("Interaction")]
        [Min(0.01f)] public float deformRadius = 0.17f;
        [Min(0.001f)] public float tapDentStrength = 0.10f;
        [Min(0.001f)] public float dragPullStrength = 0.12f;
        [Min(0.001f)] public float dragDepth = 0.10f; // how far into slime we "pull"

        [Header("Rendering")]
        public Color initialColor = new(0.35f, 0.85f, 0.75f, 1f);
        [Range(1, 10)] public int normalsUpdateEveryNFrames = 2;

        [Header("Audio")]
        [Range(0f, 1f)] public float interactionVolume = 0.25f;
        [Range(0.02f, 0.5f)] public float minSoundIntervalSeconds = 0.07f;
    }
}

