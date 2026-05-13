using System.Collections.Generic;
using UnityEngine;

namespace AntiStressLab.Slime
{
    /// <summary>
    /// Plastic (clay-like) deformation for an arbitrary mesh:
    /// - Keeps sculpted shape (no spring-back to rest)
    /// - Laplacian relaxation with softer response right after interaction
    /// - Smooth brush falloff, light volume redistribution on indent
    /// - Grab blends tangential motion for a sticky stretch feel
    /// </summary>
    public sealed class ClaySimulator
    {
        private readonly SlimeSettings _s;
        private readonly Vector3[] _rest;
        private readonly Vector3[] _pos;
        private readonly Vector3[] _tmp;
        private readonly int[][] _neighbors;

        private float _recentInteractionTtl;

        public bool ColliderDirty { get; set; }

        public ClaySimulator(Mesh mesh, SlimeSettings settings)
        {
            _s = settings;
            _pos = mesh.vertices;
            _rest = (Vector3[])_pos.Clone();
            _tmp = new Vector3[_pos.Length];
            _neighbors = BuildNeighbors(mesh, _pos.Length);
        }

        public void Reset()
        {
            for (int i = 0; i < _pos.Length; i++) _pos[i] = _rest[i];
            _recentInteractionTtl = Mathf.Max(0.2f, _s.clayPostTouchHoldSeconds);
            ColliderDirty = true;
        }

        public void Step(float dt)
        {
            if (dt <= 0f) return;
            dt = Mathf.Min(dt, 1f / 30f);

            float relax = Mathf.Max(0f, _s.clayRelaxation);
            float smooth = Mathf.Clamp01(_s.claySmoothing);
            float maxStep = Mathf.Max(0.001f, _s.clayMaxStep);

            if (relax <= 0f || smooth <= 0f)
            {
                if (_recentInteractionTtl > 0f)
                {
                    _recentInteractionTtl -= dt;
                    ColliderDirty = true;
                }
                return;
            }

            // Right after sculpting, relax less aggressively so folds stay crisp longer.
            float hold = Mathf.Max(0.08f, _s.clayPostTouchHoldSeconds);
            float touch01 = Mathf.Clamp01(_recentInteractionTtl / hold);
            float smoothMult = Mathf.Lerp(Mathf.Clamp01(_s.clayPostTouchSmoothMult), 1f, 1f - touch01);
            float effectiveSmooth = smooth * smoothMult;

            float a = 1f - Mathf.Exp(-relax * dt);

            for (int i = 0; i < _pos.Length; i++)
            {
                var neigh = _neighbors[i];
                if (neigh.Length == 0)
                {
                    _tmp[i] = _pos[i];
                    continue;
                }

                Vector3 avg = Vector3.zero;
                for (int n = 0; n < neigh.Length; n++) avg += _pos[neigh[n]];
                avg /= neigh.Length;

                Vector3 p = _pos[i];
                Vector3 target = Vector3.Lerp(p, avg, effectiveSmooth);

                Vector3 delta = (target - p) * a;
                float mag = delta.magnitude;
                if (mag > maxStep) delta *= maxStep / mag;

                _tmp[i] = p + delta;
            }

            for (int i = 0; i < _pos.Length; i++) _pos[i] = _tmp[i];

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

        public void Indent(Vector3 localPoint, Vector3 localNormal, float radius, float strength)
        {
            if (radius <= 0f || strength <= 0f) return;
            float r2 = radius * radius;
            Vector3 dir = -localNormal.normalized;
            float vol = Mathf.Clamp01(_s.clayVolumePreserve);

            for (int i = 0; i < _pos.Length; i++)
            {
                Vector3 p = _pos[i];
                float d2 = (p - localPoint).sqrMagnitude;
                if (d2 > r2) continue;

                float w = SmoothFalloff(d2, r2);
                Vector3 inward = dir * (strength * w);

                Vector3 bulge = Vector3.zero;
                if (vol > 0f)
                {
                    Vector3 fromCenter = p;
                    if (fromCenter.sqrMagnitude > 1e-8f)
                    {
                        Vector3 outDir = fromCenter.normalized;
                        bulge = outDir * (strength * w * vol * 0.18f);
                    }
                }

                _pos[i] = p + inward + bulge;
            }

            _recentInteractionTtl = Mathf.Max(_recentInteractionTtl, _s.clayPostTouchHoldSeconds);
            ColliderDirty = true;
        }

        public void Grab(Vector3 localPoint, Vector3 localDelta, float radius, float strength)
        {
            if (radius <= 0f || strength <= 0f) return;
            float r2 = radius * radius;
            float tRatio = Mathf.Clamp01(_s.clayGrabTangentRatio);

            for (int i = 0; i < _pos.Length; i++)
            {
                Vector3 p = _pos[i];
                float d2 = (p - localPoint).sqrMagnitude;
                if (d2 > r2) continue;

                float w = SmoothFalloff(d2, r2);
                Vector3 radial = p.sqrMagnitude > 1e-8f ? p.normalized : Vector3.up;
                Vector3 tang = localDelta - Vector3.Dot(localDelta, radial) * radial;
                Vector3 blended = Vector3.Lerp(localDelta, tang + radial * (Vector3.Dot(localDelta, radial) * 0.12f), tRatio);
                _pos[i] = p + blended * (strength * w);
            }

            _recentInteractionTtl = Mathf.Max(_recentInteractionTtl, _s.clayPostTouchHoldSeconds);
            ColliderDirty = true;
        }

        /// <summary>
        /// Smooth 1 at center -> 0 at radius (C1 falloff).
        /// </summary>
        private static float SmoothFalloff(float d2, float r2)
        {
            float t = Mathf.Sqrt(d2 / r2);
            float u = 1f - Mathf.Clamp01(t);
            return u * u * (3f - 2f * u);
        }

        private static int[][] BuildNeighbors(Mesh mesh, int vertexCount)
        {
            var sets = new HashSet<int>[vertexCount];
            for (int i = 0; i < vertexCount; i++) sets[i] = new HashSet<int>();

            var tris = mesh.triangles;
            for (int i = 0; i < tris.Length; i += 3)
            {
                int a = tris[i];
                int b = tris[i + 1];
                int c = tris[i + 2];

                sets[a].Add(b); sets[a].Add(c);
                sets[b].Add(a); sets[b].Add(c);
                sets[c].Add(a); sets[c].Add(b);
            }

            var neigh = new int[vertexCount][];
            for (int i = 0; i < vertexCount; i++)
            {
                neigh[i] = new int[sets[i].Count];
                sets[i].CopyTo(neigh[i]);
            }
            return neigh;
        }
    }
}
