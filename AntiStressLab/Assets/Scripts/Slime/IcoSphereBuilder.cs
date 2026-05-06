using System.Collections.Generic;
using UnityEngine;

namespace AntiStressLab.Slime
{
    public static class IcoSphereBuilder
    {
        public static Mesh Build(float radius, int subdivisions)
        {
            subdivisions = Mathf.Clamp(subdivisions, 0, 6);
            radius = Mathf.Max(0.001f, radius);

            var vertices = new List<Vector3>(12);
            var triangles = new List<int>(60);

            float t = (1f + Mathf.Sqrt(5f)) / 2f;

            // 12 vertices of an icosahedron
            vertices.Add(new Vector3(-1,  t,  0));
            vertices.Add(new Vector3( 1,  t,  0));
            vertices.Add(new Vector3(-1, -t,  0));
            vertices.Add(new Vector3( 1, -t,  0));

            vertices.Add(new Vector3( 0, -1,  t));
            vertices.Add(new Vector3( 0,  1,  t));
            vertices.Add(new Vector3( 0, -1, -t));
            vertices.Add(new Vector3( 0,  1, -t));

            vertices.Add(new Vector3( t,  0, -1));
            vertices.Add(new Vector3( t,  0,  1));
            vertices.Add(new Vector3(-t,  0, -1));
            vertices.Add(new Vector3(-t,  0,  1));

            // 20 faces
            int[] faces =
            {
                0,11,5,  0,5,1,  0,1,7,  0,7,10, 0,10,11,
                1,5,9,   5,11,4, 11,10,2, 10,7,6, 7,1,8,
                3,9,4,   3,4,2,  3,2,6,  3,6,8,  3,8,9,
                4,9,5,   2,4,11, 6,2,10, 8,6,7,  9,8,1
            };
            triangles.AddRange(faces);

            // Subdivide
            var midpointCache = new Dictionary<long, int>(triangles.Count);
            for (int s = 0; s < subdivisions; s++)
            {
                var newTris = new List<int>(triangles.Count * 4);
                for (int i = 0; i < triangles.Count; i += 3)
                {
                    int a = triangles[i];
                    int b = triangles[i + 1];
                    int c = triangles[i + 2];

                    int ab = GetMidpoint(a, b, vertices, midpointCache);
                    int bc = GetMidpoint(b, c, vertices, midpointCache);
                    int ca = GetMidpoint(c, a, vertices, midpointCache);

                    newTris.Add(a);  newTris.Add(ab); newTris.Add(ca);
                    newTris.Add(b);  newTris.Add(bc); newTris.Add(ab);
                    newTris.Add(c);  newTris.Add(ca); newTris.Add(bc);
                    newTris.Add(ab); newTris.Add(bc); newTris.Add(ca);
                }
                triangles = newTris;
            }

            // Normalize to radius
            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i] = vertices[i].normalized * radius;
            }

            var mesh = new Mesh
            {
                name = "ClayBlob",
                vertices = vertices.ToArray(),
                triangles = triangles.ToArray()
            };

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.MarkDynamic();
            return mesh;
        }

        private static int GetMidpoint(int i0, int i1, List<Vector3> verts, Dictionary<long, int> cache)
        {
            long key = i0 < i1 ? ((long)i0 << 32) + i1 : ((long)i1 << 32) + i0;
            if (cache.TryGetValue(key, out int idx)) return idx;

            Vector3 mid = (verts[i0] + verts[i1]) * 0.5f;
            int newIndex = verts.Count;
            verts.Add(mid);
            cache[key] = newIndex;
            return newIndex;
        }
    }
}

