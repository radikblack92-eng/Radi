using UnityEngine;

namespace AntiStressLab.Slime
{
    /// <summary>
    /// Owns the slime object (mesh, renderer, collider) and exposes an API
    /// for interaction (tap/drag), reset, and color changes.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SlimeController : MonoBehaviour
    {
        private SlimeSettings _settings;
        private SlimeSimulator _sim;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private MeshCollider _meshCollider;
        private Material _material;

        private int _frame;

        public void ApplySettings(SlimeSettings settings) => _settings = settings;

        public void Initialize()
        {
            if (_settings == null)
            {
                _settings = ScriptableObject.CreateInstance<SlimeSettings>();
            }

            // Components
            _meshFilter = gameObject.GetComponent<MeshFilter>() ?? gameObject.AddComponent<MeshFilter>();
            _meshRenderer = gameObject.GetComponent<MeshRenderer>() ?? gameObject.AddComponent<MeshRenderer>();
            _meshCollider = gameObject.GetComponent<MeshCollider>() ?? gameObject.AddComponent<MeshCollider>();

            // Mesh
            var mesh = GridMeshBuilder.Build(_settings.gridResolution, _settings.size);
            _meshFilter.sharedMesh = mesh;
            _meshCollider.sharedMesh = mesh;
            _meshCollider.convex = false;

            // Material
            _material = new Material(Shader.Find("Standard"));
            _material.color = _settings.initialColor;
            _material.SetFloat("_Glossiness", 0.65f);
            _material.SetFloat("_Metallic", 0.05f);
            _meshRenderer.sharedMaterial = _material;

            // Simulation state
            _sim = new SlimeSimulator(mesh, _settings);

            // Place a subtle base plane under it (for depth cues)
            CreateBase();
        }

        private void CreateBase()
        {
            var baseGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
            baseGo.name = "Base";
            baseGo.transform.SetParent(transform, worldPositionStays: false);
            baseGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            baseGo.transform.localPosition = new Vector3(0f, -0.18f, 0f);
            baseGo.transform.localScale = Vector3.one * (_settings.size * 1.2f);
            Destroy(baseGo.GetComponent<Collider>());

            var mr = baseGo.GetComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.09f, 0.09f, 0.10f, 1f);
            mat.SetFloat("_Glossiness", 0.2f);
            mr.sharedMaterial = mat;
        }

        private void Update()
        {
            if (_sim == null) return;

            _sim.Step(Time.deltaTime);

            var mesh = _meshFilter.sharedMesh;
            _sim.ApplyToMesh(mesh, updateNormals: (_frame++ % Mathf.Max(1, _settings.normalsUpdateEveryNFrames) == 0));

            // Keep collider usable for raycasts; updating every frame can be expensive,
            // so we only refresh when there is recent interaction or at a small cadence.
            if (_sim.ColliderDirty)
            {
                _meshCollider.sharedMesh = null;
                _meshCollider.sharedMesh = mesh;
                _sim.ColliderDirty = false;
            }
        }

        public void ResetSlime()
        {
            _sim?.Reset();
            _meshCollider.sharedMesh = _meshFilter.sharedMesh;
        }

        public void SetColor(Color c)
        {
            if (_material != null) _material.color = c;
        }

        public Color GetColor()
        {
            return _material != null ? _material.color : Color.white;
        }

        public bool Raycast(Ray ray, out RaycastHit hit)
        {
            if (_meshCollider == null)
            {
                hit = default;
                return false;
            }
            return _meshCollider.Raycast(ray, out hit, 100f);
        }

        public void TapDeform(Vector3 worldPoint)
        {
            if (_sim == null) return;
            Vector3 local = transform.InverseTransformPoint(worldPoint);
            _sim.AddDent(local, _settings.deformRadius, _settings.tapDentStrength);
        }

        public void DragDeform(Vector3 worldPoint)
        {
            if (_sim == null) return;
            Vector3 local = transform.InverseTransformPoint(worldPoint);
            _sim.AddPull(local, _settings.deformRadius, _settings.dragPullStrength, _settings.dragDepth);
        }
    }
}

