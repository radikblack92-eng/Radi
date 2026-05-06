using UnityEngine;

namespace AntiStressLab.Slime
{
    public static class GridMeshBuilder
    {
        public static Mesh Build(int gridResolution, float size)
        {
            // gridResolution = number of quads per side
            int vertsPerSide = gridResolution + 1;
            int vertCount = vertsPerSide * vertsPerSide;
            int triCount = gridResolution * gridResolution * 2;

            var vertices = new Vector3[vertCount];
            var uvs = new Vector2[vertCount];
            var triangles = new int[triCount * 3];

            float half = size * 0.5f;
            float step = size / gridResolution;

            int v = 0;
            for (int y = 0; y < vertsPerSide; y++)
            {
                for (int x = 0; x < vertsPerSide; x++)
                {
                    float px = -half + x * step;
                    float py = -half + y * step;
                    vertices[v] = new Vector3(px, 0f, py);
                    uvs[v] = new Vector2((float)x / gridResolution, (float)y / gridResolution);
                    v++;
                }
            }

            int t = 0;
            for (int y = 0; y < gridResolution; y++)
            {
                for (int x = 0; x < gridResolution; x++)
                {
                    int i0 = y * vertsPerSide + x;
                    int i1 = i0 + 1;
                    int i2 = i0 + vertsPerSide;
                    int i3 = i2 + 1;

                    // Winding order for +Y normal (up)
                    triangles[t++] = i0;
                    triangles[t++] = i2;
                    triangles[t++] = i1;

                    triangles[t++] = i1;
                    triangles[t++] = i2;
                    triangles[t++] = i3;
                }
            }

            var mesh = new Mesh
            {
                name = "SlimeGrid",
                vertices = vertices,
                uv = uvs,
                triangles = triangles
            };

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.MarkDynamic();
            return mesh;
        }
    }
}

