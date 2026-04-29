using System.Collections;
using System.Collections.Generic;
using HollowGround.Grid;
using UnityEngine;

namespace HollowGround.Roads
{
    public class RoadVisualizer : MonoBehaviour
    {
        private readonly Dictionary<Vector2Int, GameObject> _tiles = new();
        private readonly Dictionary<Vector2Int, int> _tileMasks = new();
        private Material _roadMaterial;
        private Texture2D _dirtTexture;

        private const float TileY = 0.012f;
        private const float PathHalf = 0.7f;
        private const float CellHalf = 1.0f;
        private const float AppearDuration = 1.5f;
        private const int CornerSegments = 14;
        private const float UVScale = 0.5f;

        public void RebuildVisuals(HashSet<Vector2Int> roadCells)
        {
            var toRemove = new List<Vector2Int>();
            foreach (var kvp in _tiles)
                if (!roadCells.Contains(kvp.Key))
                    toRemove.Add(kvp.Key);

            foreach (var key in toRemove)
                DestroyTile(key);

            foreach (var cell in roadCells)
            {
                int mask = ComputeMask(cell, roadCells);
                if (!_tiles.ContainsKey(cell))
                    CreateTile(cell, mask);
                else if (_tileMasks[cell] != mask)
                    RefreshMesh(cell, mask);
            }
        }

        public void FadeOutAndRemove(HashSet<Vector2Int> cells)
        {
            foreach (var cell in cells)
            {
                if (_tiles.TryGetValue(cell, out var go) && go != null)
                    StartCoroutine(FadeOutTile(go, cell));
            }
        }

        private int ComputeMask(Vector2Int cell, HashSet<Vector2Int> roadCells)
        {
            int mask = 0;
            if (roadCells.Contains(new Vector2Int(cell.x, cell.y + 1))) mask |= 1;
            if (roadCells.Contains(new Vector2Int(cell.x, cell.y - 1))) mask |= 2;
            if (roadCells.Contains(new Vector2Int(cell.x + 1, cell.y))) mask |= 4;
            if (roadCells.Contains(new Vector2Int(cell.x - 1, cell.y))) mask |= 8;
            return mask;
        }

        private void DestroyTile(Vector2Int cell)
        {
            if (!_tiles.TryGetValue(cell, out var go)) return;
            if (go != null)
            {
                var mf = go.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null) Destroy(mf.sharedMesh);
                Destroy(go);
            }
            _tiles.Remove(cell);
            _tileMasks.Remove(cell);
        }

        private void CreateTile(Vector2Int cell, int mask)
        {
            EnsureMaterial();
            Vector3 worldPos = GridSystem.Instance.GetWorldPosition(cell.x, cell.y);
            var go = new GameObject("RoadTile");
            go.transform.SetParent(transform, false);
            go.transform.position = new Vector3(worldPos.x, TileY, worldPos.z);
            go.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            mf.sharedMesh = BuildMesh(mask, cell);
            mr.sharedMaterial = _roadMaterial;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;

            _tiles[cell] = go;
            _tileMasks[cell] = mask;
            StartCoroutine(AnimateTileIn(go.transform));
        }

        private void RefreshMesh(Vector2Int cell, int mask)
        {
            var mf = _tiles[cell].GetComponent<MeshFilter>();
            if (mf == null) return;
            if (mf.sharedMesh != null) Destroy(mf.sharedMesh);
            mf.sharedMesh = BuildMesh(mask, cell);
            _tileMasks[cell] = mask;
        }

        private Mesh BuildMesh(int mask, Vector2Int cell)
        {
            bool n = (mask & 1) != 0;
            bool s = (mask & 2) != 0;
            bool e = (mask & 4) != 0;
            bool w = (mask & 8) != 0;
            int count = (n ? 1 : 0) + (s ? 1 : 0) + (e ? 1 : 0) + (w ? 1 : 0);
            bool opposite = (n && s && !e && !w) || (e && w && !n && !s);

            Vector3 worldPos = GridSystem.Instance.GetWorldPosition(cell.x, cell.y);
            var wo = new Vector2(worldPos.x, worldPos.z);

            if (count == 2 && !opposite)
            {
                Vector2 p0, p2;
                if      (n && e) { p0 = new Vector2(0,       CellHalf);  p2 = new Vector2(CellHalf,  0); }
                else if (n && w) { p0 = new Vector2(0,       CellHalf);  p2 = new Vector2(-CellHalf, 0); }
                else if (s && e) { p0 = new Vector2(0,      -CellHalf);  p2 = new Vector2(CellHalf,  0); }
                else             { p0 = new Vector2(0,      -CellHalf);  p2 = new Vector2(-CellHalf, 0); }
                return BuildBezierRibbon(p0, Vector2.zero, p2, wo);
            }

            var verts = new List<Vector3>();
            var tris  = new List<int>();
            var uvs   = new List<Vector2>();

            if (count == 1)
            {
                if      (n) { AddQuad(verts, tris, uvs, -PathHalf, PathHalf, -PathHalf,  CellHalf, wo); AddEllipsoidCap(verts, tris, uvs,  0,        -PathHalf, PathHalf, PathHalf, 180, 360, wo, CornerSegments); }
                else if (s) { AddQuad(verts, tris, uvs, -PathHalf, PathHalf, -CellHalf,  PathHalf, wo); AddEllipsoidCap(verts, tris, uvs,  0,         PathHalf, PathHalf, PathHalf,   0, 180, wo, CornerSegments); }
                else if (e) { AddQuad(verts, tris, uvs, -PathHalf, CellHalf, -PathHalf,  PathHalf, wo); AddEllipsoidCap(verts, tris, uvs, -PathHalf,  0,        PathHalf, PathHalf,  90, 270, wo, CornerSegments); }
                else        { AddQuad(verts, tris, uvs, -CellHalf, PathHalf, -PathHalf,  PathHalf, wo); AddEllipsoidCap(verts, tris, uvs,  PathHalf,  0,        PathHalf, PathHalf, -90,  90, wo, CornerSegments); }
                return ToMesh(verts, tris, uvs);
            }

            AddQuad(verts, tris, uvs, -PathHalf, PathHalf, -PathHalf,  PathHalf, wo);
            if (n) AddQuad(verts, tris, uvs, -PathHalf,  PathHalf,  PathHalf,  CellHalf, wo);
            if (s) AddQuad(verts, tris, uvs, -PathHalf,  PathHalf, -CellHalf, -PathHalf, wo);
            if (e) AddQuad(verts, tris, uvs,  PathHalf,  CellHalf, -PathHalf,  PathHalf, wo);
            if (w) AddQuad(verts, tris, uvs, -CellHalf, -PathHalf, -PathHalf,  PathHalf, wo);

            return ToMesh(verts, tris, uvs);
        }

        private static void AddEllipsoidCap(List<Vector3> verts, List<int> tris, List<Vector2> uvs,
            float cx, float cz, float aX, float aZ, float startDeg, float endDeg, Vector2 wo, int segs = 10)
        {
            int ci = verts.Count;
            verts.Add(new Vector3(cx, 0f, cz));
            uvs.Add(new Vector2((cx + wo.x) / UVScale, (cz + wo.y) / UVScale));

            for (int i = 0; i <= segs; i++)
            {
                float a  = Mathf.Lerp(startDeg, endDeg, i / (float)segs) * Mathf.Deg2Rad;
                float px = cx + aX * Mathf.Cos(a);
                float pz = cz + aZ * Mathf.Sin(a);
                verts.Add(new Vector3(px, 0f, pz));
                uvs.Add(new Vector2((px + wo.x) / UVScale, (pz + wo.y) / UVScale));
            }

            for (int i = 0; i < segs; i++)
            {
                tris.Add(ci);
                tris.Add(ci + 2 + i);
                tris.Add(ci + 1 + i);
            }
        }

        private Mesh BuildBezierRibbon(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 wo)
        {
            var verts = new List<Vector3>();
            var tris  = new List<int>();
            var uvs   = new List<Vector2>();

            for (int i = 0; i <= CornerSegments; i++)
            {
                float t = (float)i / CornerSegments;
                float u = 1f - t;

                Vector2 center  = u * u * p0 + 2f * u * t * p1 + t * t * p2;
                Vector2 tangent = (2f * (1f - t) * (p1 - p0) + 2f * t * (p2 - p1)).normalized;
                Vector2 perp    = new Vector2(-tangent.y, tangent.x);

                Vector3 left  = new Vector3(center.x - PathHalf * perp.x, 0f, center.y - PathHalf * perp.y);
                Vector3 right = new Vector3(center.x + PathHalf * perp.x, 0f, center.y + PathHalf * perp.y);

                verts.Add(left);
                verts.Add(right);
                uvs.Add(new Vector2((left.x  + wo.x) / UVScale, (left.z  + wo.y) / UVScale));
                uvs.Add(new Vector2((right.x + wo.x) / UVScale, (right.z + wo.y) / UVScale));

                if (i > 0)
                {
                    int b = (i - 1) * 2;
                    tris.Add(b);     tris.Add(b + 1); tris.Add(b + 2);
                    tris.Add(b + 1); tris.Add(b + 3); tris.Add(b + 2);
                }
            }

            return ToMesh(verts, tris, uvs);
        }

        private static Mesh ToMesh(List<Vector3> verts, List<int> tris, List<Vector2> uvs)
        {
            var mesh = new Mesh { name = "RoadMesh" };
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AddQuad(List<Vector3> verts, List<int> tris, List<Vector2> uvs,
            float xMin, float xMax, float zMin, float zMax, Vector2 wo)
        {
            int b = verts.Count;
            verts.Add(new Vector3(xMin, 0f, zMin));
            verts.Add(new Vector3(xMax, 0f, zMin));
            verts.Add(new Vector3(xMax, 0f, zMax));
            verts.Add(new Vector3(xMin, 0f, zMax));
            tris.Add(b); tris.Add(b + 2); tris.Add(b + 1);
            tris.Add(b); tris.Add(b + 3); tris.Add(b + 2);
            uvs.Add(new Vector2((xMin + wo.x) / UVScale, (zMin + wo.y) / UVScale));
            uvs.Add(new Vector2((xMax + wo.x) / UVScale, (zMin + wo.y) / UVScale));
            uvs.Add(new Vector2((xMax + wo.x) / UVScale, (zMax + wo.y) / UVScale));
            uvs.Add(new Vector2((xMin + wo.x) / UVScale, (zMax + wo.y) / UVScale));
        }

        private void EnsureMaterial()
        {
            if (_roadMaterial != null) return;

            _dirtTexture = BuildDirtTexture();

            _roadMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            _roadMaterial.SetColor("_BaseColor", Color.white);
            _roadMaterial.SetTexture("_BaseMap", _dirtTexture);
            _roadMaterial.SetFloat("_Smoothness", 0f);
            _roadMaterial.SetFloat("_Metallic", 0f);
            _roadMaterial.SetFloat("_Cull", 0f);
            _roadMaterial.renderQueue = 2001;
        }

        private static Texture2D BuildDirtTexture()
        {
            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGB24, true);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = x / (float)size;
                    float py = y / (float)size;

                    float n1 = Mathf.PerlinNoise(px * 3.1f + 0.3f, py * 3.1f + 0.7f);
                    float n2 = Mathf.PerlinNoise(px * 9.7f + 2.1f, py * 9.7f + 3.6f);
                    float n3 = Mathf.PerlinNoise(px * 23f  + 5.4f, py * 23f  + 8.1f);
                    float n  = n1 * 0.55f + n2 * 0.33f + n3 * 0.12f;

                    float r = Mathf.Clamp01(0.41f + (n - 0.5f) * 0.16f);
                    float g = Mathf.Clamp01(0.32f + (n - 0.5f) * 0.11f);
                    float b = Mathf.Clamp01(0.19f + (n - 0.5f) * 0.07f);
                    tex.SetPixel(x, y, new Color(r, g, b));
                }
            }

            tex.Apply();
            return tex;
        }

        private IEnumerator AnimateTileIn(Transform t)
        {
            float elapsed = 0f;
            while (elapsed < AppearDuration)
            {
                if (t == null) yield break;
                elapsed += Time.deltaTime;
                float p = elapsed / AppearDuration;
                p = p * p * (3f - 2f * p);
                float s = Mathf.Lerp(0.01f, 1f, p);
                t.localScale = new Vector3(s, s, s);
                yield return null;
            }
            if (t != null) t.localScale = Vector3.one;
        }

        private void OnDestroy()
        {
            foreach (var kvp in _tiles)
            {
                if (kvp.Value == null) continue;
                var mf = kvp.Value.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null) Destroy(mf.sharedMesh);
            }
            if (_roadMaterial != null) Destroy(_roadMaterial);
            if (_dirtTexture != null) Destroy(_dirtTexture);
        }

        private IEnumerator FadeOutTile(GameObject go, Vector2Int cell)
        {
            float elapsed = 0f;
            Vector3 startScale = go.transform.localScale;

            while (elapsed < 2f)
            {
                if (go == null) { _tiles.Remove(cell); _tileMasks.Remove(cell); yield break; }
                elapsed += Time.deltaTime;
                float t = 1f - Mathf.Clamp01(elapsed / 2f);
                go.transform.localScale = startScale * (t * t);
                yield return null;
            }

            if (go != null)
            {
                var mf = go.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null) Destroy(mf.sharedMesh);
                Destroy(go);
            }
            _tiles.Remove(cell);
            _tileMasks.Remove(cell);
        }
    }
}
