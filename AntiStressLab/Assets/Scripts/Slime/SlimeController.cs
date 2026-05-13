using UnityEngine;

namespace AntiStressLab.Slime
{
    /// <summary>
    /// Owns the slime object (mesh, renderer, collider) and exposes an API
    /// for interaction (tap/drag), reset, and color changes.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public sealed class SlimeController : MonoBehaviour
    {
        private SlimeSettings _settings;
        private SlimeSimulator _slimeSim;
        private ClaySimulator _claySim;
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
            if (!gameObject.TryGetComponent(out _meshFilter)) _meshFilter = gameObject.AddComponent<MeshFilter>();
            if (!gameObject.TryGetComponent(out _meshRenderer)) _meshRenderer = gameObject.AddComponent<MeshRenderer>();
            if (!gameObject.TryGetComponent(out _meshCollider)) _meshCollider = gameObject.AddComponent<MeshCollider>();

            // Extra safety for Unity Editor edge cases (domain reload / Safe Mode transitions)
            _meshFilter ??= gameObject.GetComponent<MeshFilter>();
            _meshRenderer ??= gameObject.GetComponent<MeshRenderer>();
            _meshCollider ??= gameObject.GetComponent<MeshCollider>();

            if (_meshFilter == null || _meshRenderer == null || _meshCollider == null)
            {
                Debug.LogError("SlimeController: missing required mesh components. Initialization aborted.");
                return;
            }

            // Mesh
            Mesh mesh = _settings.mode == SlimeSettings.MaterialMode.ClayBlob3D
                ? IcoSphereBuilder.Build(_settings.sphereRadius, _settings.sphereSubdivisions)
                : GridMeshBuilder.Build(_settings.gridResolution, _settings.size);
            _meshFilter.sharedMesh = mesh;
            _meshCollider.sharedMesh = mesh;
            _meshCollider.convex = false;

            // Material
            _material = new Material(Shader.Find("Standard"));
            _material.color = _settings.initialColor;
            if (_settings.mode == SlimeSettings.MaterialMode.ClayBlob3D)
            {
                _material.SetFloat("_Glossiness", _settings.clayGlossiness);
                _material.SetFloat("_Metallic", _settings.clayMetallic);
            }
            else
            {
                _material.SetFloat("_Glossiness", 0.65f);
                _material.SetFloat("_Metallic", 0.05f);
            }
            _meshRenderer.sharedMaterial = _material;

            // Simulation state
            _slimeSim = null;
            _claySim = null;
            if (_settings.mode == SlimeSettings.MaterialMode.ClayBlob3D)
                _claySim = new ClaySimulator(mesh, _settings);
            else
                _slimeSim = new SlimeSimulator(mesh, _settings);

            // Place a subtle base plane under it (for depth cues)
            CreateBase();
        }

        private void CreateBase()
        {
            var baseGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
            baseGo.name = "Base";
            baseGo.transform.SetParent(transform, worldPositionStays: false);
            baseGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            float baseY = _settings.mode == SlimeSettings.MaterialMode.ClayBlob3D ? -(_settings.sphereRadius + 0.25f) : -0.18f;
            float baseScale = _settings.mode == SlimeSettings.MaterialMode.ClayBlob3D ? (_settings.sphereRadius * 3.2f) : (_settings.size * 1.2f);
            baseGo.transform.localPosition = new Vector3(0f, baseY, 0f);
            baseGo.transform.localScale = Vector3.one * baseScale;
            Destroy(baseGo.GetComponent<Collider>());

            var mr = baseGo.GetComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.52f, 0.40f, 0.30f, 1f);
            mat.SetFloat("_Glossiness", 0.1f);
            mr.sharedMaterial = mat;
        }

        private void Update()
        {
            if (_slimeSim == null && _claySim == null) return;

            _slimeSim?.Step(Time.deltaTime);
            _claySim?.Step(Time.deltaTime);

            var mesh = _meshFilter.sharedMesh;
            int cadence = _claySim != null ? 1 : Mathf.Max(1, _settings.normalsUpdateEveryNFrames);
            bool updateNormals = (_frame++ % cadence) == 0;
            _slimeSim?.ApplyToMesh(mesh, updateNormals);
            _claySim?.ApplyToMesh(mesh, updateNormals);

            // Keep collider usable for raycasts; updating every frame can be expensive,
            // so we only refresh when there is recent interaction or at a small cadence.
            if ((_slimeSim != null && _slimeSim.ColliderDirty) || (_claySim != null && _claySim.ColliderDirty))
            {
                _meshCollider.sharedMesh = null;
                _meshCollider.sharedMesh = mesh;
                if (_slimeSim != null) _slimeSim.ColliderDirty = false;
                if (_claySim != null) _claySim.ColliderDirty = false;
            }
        }

        public void ResetSlime()
        {
            _slimeSim?.Reset();
            _claySim?.Reset();
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
            Vector3 local = transform.InverseTransformPoint(worldPoint);
            if (_claySim != null)
            {
                // Without hit normal we approximate using radial direction.
                Vector3 n = (local.sqrMagnitude > 0.0001f) ? local.normalized : Vector3.up;
                _claySim.Indent(local, n, _settings.deformRadius, _settings.tapDentStrength);
                return;
            }
            _slimeSim?.AddDent(local, _settings.deformRadius, _settings.tapDentStrength);
        }

        public void DragDeform(Vector3 worldPoint)
        {
            Vector3 local = transform.InverseTransformPoint(worldPoint);
            if (_claySim != null)
            {
                // Legacy drag without delta uses downward pull for safety.
                _claySim.Grab(local, new Vector3(0f, -0.02f, 0f), _settings.deformRadius, _settings.dragPullStrength);
                return;
            }
            _slimeSim?.AddPull(local, _settings.deformRadius, _settings.dragPullStrength, _settings.dragDepth);
        }

        public void ClayIndent(Vector3 worldPoint, Vector3 worldNormal)
        {
            if (_claySim == null) return;
            Vector3 local = transform.InverseTransformPoint(worldPoint);
            Vector3 n = transform.InverseTransformDirection(worldNormal).normalized;
            _claySim.Indent(local, n, _settings.deformRadius, _settings.tapDentStrength);
        }

        public void ClayGrabDelta(Vector3 worldPoint, Vector3 worldDelta)
        {
            if (_claySim == null) return;
            Vector3 local = transform.InverseTransformPoint(worldPoint);
            Vector3 deltaLocal = transform.InverseTransformVector(worldDelta);
            _claySim.Grab(local, deltaLocal, _settings.deformRadius, _settings.dragPullStrength);
        }
    }
}

