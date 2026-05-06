using UnityEngine;

namespace AntiStressLab.Slime
{
    /// <summary>
    /// Lightweight soft-ish deformation for a grid mesh:
    /// - Each vertex has rest position, current position, velocity
    /// - Springs pull vertices back to rest (stiffness) with damping
    /// - Neighbor tension smooths deformation across the grid
    /// 
    /// This is not a true FEM/soft-body, but produces convincing slime for an MVP and is mobile-friendly.
    /// </summary>
    public sealed class SlimeSimulator
    {
        private readonly SlimeSettings _s;
        private readonly int _gridResolution;
        private readonly int _vertsPerSide;

        private readonly Vector3[] _rest;
        private readonly Vector3[] _pos;
        private readonly Vector3[] _vel;

        // Scratch buffers to avoid allocations
        private readonly Vector3[] _tmp;

        private float _recentInteractionTtl;

        public bool ColliderDirty { get; set; }

        public SlimeSimulator(Mesh mesh, SlimeSettings settings)
        {
            _s = settings;
            _gridResolution = settings.gridResolution;
            _vertsPerSide = _gridResolution + 1;

            _pos = mesh.vertices;
            _rest = (Vector3[])_pos.Clone();
            _vel = new Vector3[_pos.Length];
            _tmp = new Vector3[_pos.Length];
        }

        public void Reset()
        {
            for (int i = 0; i < _pos.Length; i++)
            {
                _pos[i] = _rest[i];
                _vel[i] = Vector3.zero;
            }
            _recentInteractionTtl = 0.2f;
            ColliderDirty = true;
        }

        public void Step(float dt)
        {
            if (dt <= 0f) return;

            // Clamp dt for stability on slow frames
            dt = Mathf.Min(dt, 1f / 30f);

            float k = _s.stiffness;
            float damp = _s.damping;
            float tension = _s.neighborTension;
            float maxSpeed = Mathf.Max(0.0001f, _s.maxVertexSpeed);

            // Neighbor smoothing pass into _tmp
            if (tension > 0f)
            {
                for (int y = 0; y < _vertsPerSide; y++)
                {
                    for (int x = 0; x < _vertsPerSide; x++)
                    {
                        int i = Index(x, y);
                        Vector3 sum = _pos[i];
                        int count = 1;

                        if (x > 0) { sum += _pos[Index(x - 1, y)]; count++; }
                        if (x < _vertsPerSide - 1) { sum += _pos[Index(x + 1, y)]; count++; }
                        if (y > 0) { sum += _pos[Index(x, y - 1)]; count++; }
                        if (y < _vertsPerSide - 1) { sum += _pos[Index(x, y + 1)]; count++; }

                        Vector3 avg = sum / count;
                        _tmp[i] = Vector3.Lerp(_pos[i], avg, tension);
                    }
                }
            }
            else
            {
                for (int i = 0; i < _pos.Length; i++) _tmp[i] = _pos[i];
            }

            // Spring back + integrate
            for (int i = 0; i < _pos.Length; i++)
            {
                Vector3 p = _tmp[i];
                Vector3 r = _rest[i];

                Vector3 accel = (r - p) * k;
                Vector3 v = _vel[i];

                v += accel * dt;
                v *= Mathf.Clamp01(1f - damp * dt);

                // Speed clamp to avoid spikes on mobile and keep stable visuals
                float sp = v.magnitude;
                if (sp > maxSpeed) v = v * (maxSpeed / sp);

                p += v * dt;

                _pos[i] = p;
                _vel[i] = v;
            }

            if (_recentInteractionTtl > 0f)
            {
                _recentInteractionTtl -= dt;
                if (_recentInteractionTtl < 0f) _recentInteractionTtl = 0f;
                ColliderDirty = true;
            }
        }

        public void ApplyToMesh(Mesh mesh, bool updateNormals)
        {
            mesh.vertices = _pos;
            if (updateNormals) mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }

        public void AddDent(Vector3 localPoint, float radius, float strength)
        {
            if (radius <= 0f || strength <= 0f) return;
            float r2 = radius * radius;

            for (int i = 0; i < _pos.Length; i++)
            {
                Vector3 p = _pos[i];
                Vector2 dxz = new Vector2(p.x - localPoint.x, p.z - localPoint.z);
                float d2 = dxz.sqrMagnitude;
                if (d2 > r2) continue;

                float t = 1f - Mathf.Sqrt(d2 / r2); // 1 at center -> 0 at edge
                float dent = strength * t;
                _pos[i].y -= dent;
                _vel[i].y -= dent * 6f;
            }

            _recentInteractionTtl = 0.12f;
            ColliderDirty = true;
        }

        public void AddPull(Vector3 localPoint, float radius, float strength, float depth)
        {
            if (radius <= 0f || strength <= 0f) return;
            float r2 = radius * radius;

            // Pull target slightly below the surface to feel "stretchy"
            Vector3 target = new Vector3(localPoint.x, -Mathf.Abs(depth), localPoint.z);

            for (int i = 0; i < _pos.Length; i++)
            {
                Vector3 p = _pos[i];
                Vector2 dxz = new Vector2(p.x - localPoint.x, p.z - localPoint.z);
                float d2 = dxz.sqrMagnitude;
                if (d2 > r2) continue;

                float t = 1f - Mathf.Sqrt(d2 / r2);
                Vector3 to = (target - p);

                // Pull more on Y, less on XZ for a clay-like feel.
                Vector3 impulse = new Vector3(to.x * 0.35f, to.y * 1.2f, to.z * 0.35f) * (strength * t);

                _vel[i] += impulse;
            }

            _recentInteractionTtl = 0.12f;
            ColliderDirty = true;
        }

        private int Index(int x, int y) => y * _vertsPerSide + x;
    }
}

