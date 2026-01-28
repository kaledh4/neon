using UnityEngine;
using System.Collections.Generic;
using Visual;

namespace NeonSplash
{
    public static class ChunkPrefabs
    {
        private static Shader _cachedShader;
        private static Shader GetAppropriateShader()
        {
            if (_cachedShader != null) return _cachedShader;
            _cachedShader = Shader.Find("Universal Render Pipeline/Lit");
            if (_cachedShader == null) _cachedShader = Shader.Find("Standard");
            return _cachedShader;
        }

        private static Material GetNeonMaterial(Color color, float intensity = 1.0f)
        {
            Material mat = new Material(GetAppropriateShader());
            mat.color = color;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * intensity);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            return mat;
        }

        private static Material GetMat(Color color)
        {
            Material mat = new Material(GetAppropriateShader());
            mat.color = color;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            return mat;
        }

        public static GameObject CreateBase(Vector3 position, ColorPalette palette, bool isBlue, float size = 100f)
        {
            GameObject group = new GameObject(isBlue ? "BaseBlue" : "BaseRed");
            group.transform.position = position;

            Color teamColor = isBlue ? new Color(0, 0.4f, 1f) : new Color(1f, 0.2f, 0f);
            Color accentColor = isBlue ? palette.Primary : palette.Secondary;

            // Floor
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.SetParent(group.transform);
            floor.transform.localPosition = new Vector3(0, -0.25f, 0);
            floor.transform.localScale = new Vector3(size, 0.5f, size);
            floor.GetComponent<Renderer>().material = GetNeonMaterial(new Color(0.05f, 0.05f, 0.07f), 0.1f);

            // Team Pedestal
            GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestal.transform.SetParent(group.transform);
            pedestal.transform.localPosition = new Vector3(0, 0.5f, 0);
            pedestal.transform.localScale = new Vector3(8, 0.5f, 8);
            pedestal.GetComponent<Renderer>().material = GetNeonMaterial(teamColor, 2.0f);

            // Giant Neon Text (Legacy TextMesh for compatibility)
            GameObject textObj = new GameObject("TeamText");
            textObj.transform.SetParent(group.transform);
            textObj.transform.localPosition = new Vector3(0, 5, 0);
            textObj.transform.localScale = Vector3.one * 1.5f;
            var tm = textObj.AddComponent<TextMesh>();
            tm.text = isBlue ? "BLUE NEON" : "RED NEON";
            tm.fontSize = 120;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.color = teamColor;

            // Lanterns around base
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90f * Mathf.Deg2Rad;
                Vector3 lPos = new Vector3(Mathf.Cos(angle) * 15f, 2f, Mathf.Sin(angle) * 15f);
                GameObject lantern = GameObject.CreatePrimitive(PrimitiveType.Cube);
                lantern.transform.SetParent(group.transform);
                lantern.transform.localPosition = lPos;
                lantern.transform.localScale = new Vector3(1, 2, 1);
                lantern.GetComponent<Renderer>().material = GetNeonMaterial(accentColor, 4.0f);
            }

            return group;
        }

        public static GameObject CreateTrees(Vector3 position, ColorPalette palette, bool mirror, float size = 100f)
        {
            GameObject group = new GameObject("Trees_Neon");
            group.transform.position = position;

            // Base Floor (Visible & Tinted)
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.SetParent(group.transform);
            floor.transform.localPosition = new Vector3(0, -0.25f, 0);
            floor.transform.localScale = new Vector3(size, 0.5f, size);
            floor.GetComponent<Renderer>().material = GetNeonMaterial(new Color(0.1f, 0.1f, 0.15f), 0.3f);

            for (int i = 0; i < 6; i++)
            {
                float x = (i % 2 == 0 ? 1 : -1) * (15 + i * 5);
                float z = (i < 3 ? 1 : -1) * (20 + i * 2);
                Vector3 treePos = new Vector3(x, 0, z);
                
                // Abstract Neon Tree
                GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                trunk.transform.SetParent(group.transform);
                trunk.transform.localPosition = treePos + Vector3.up * 4;
                trunk.transform.localScale = new Vector3(1, 4, 1);
                trunk.GetComponent<Renderer>().material = GetMat(new Color(0.1f, 0.1f, 0.1f));

                GameObject leaves = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                leaves.transform.SetParent(group.transform);
                leaves.transform.localPosition = treePos + Vector3.up * 9;
                leaves.transform.localScale = new Vector3(6, 6, 6);
                leaves.GetComponent<Renderer>().material = GetNeonMaterial(palette.Secondary, 0.8f);
            }

            if (mirror) group.transform.localScale = new Vector3(-1, 1, 1);
            return group;
        }

        public static GameObject CreateGarden(Vector3 position, ColorPalette palette, bool mirror, float size = 100f)
        {
            GameObject group = new GameObject("Garden_Chairs");
            group.transform.position = position;

            // Base Floor (Visible & Tinted)
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.SetParent(group.transform);
            floor.transform.localPosition = new Vector3(0, -0.25f, 0);
            floor.transform.localScale = new Vector3(size, 0.5f, size);
            floor.GetComponent<Renderer>().material = GetNeonMaterial(new Color(0.1f, 0.1f, 0.15f), 0.3f);

            // Lounge Chairs
            for (int i = 0; i < 4; i++)
            {
                float z = -20 + i * 15;
                GameObject chair = GameObject.CreatePrimitive(PrimitiveType.Cube);
                chair.transform.SetParent(group.transform);
                chair.transform.localPosition = new Vector3(10, 0.5f, z);
                chair.transform.localScale = new Vector3(4, 0.5f, 2);
                chair.GetComponent<Renderer>().material = GetNeonMaterial(palette.Primary, 0.5f);
            }

            return group;
        }

        public static GameObject CreateTikiBar(Vector3 position, ColorPalette palette, bool mirror, float size = 100f)
        {
            GameObject group = new GameObject("TikiBar_Neon");
            group.transform.position = position;

            // Base Floor (Visible & Tinted)
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.SetParent(group.transform);
            floor.transform.localPosition = new Vector3(0, -0.25f, 0);
            floor.transform.localScale = new Vector3(size, 0.5f, size);
            floor.GetComponent<Renderer>().material = GetNeonMaterial(new Color(0.1f, 0.1f, 0.15f), 0.3f);

            // Bar Table
            GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bar.transform.SetParent(group.transform);
            bar.transform.localPosition = new Vector3(0, 1.5f, 0);
            bar.transform.localScale = new Vector3(10, 3, 2);
            bar.GetComponent<Renderer>().material = GetMat(new Color(0.3f, 0.2f, 0.1f));

            // Neon Sign at bar
            GameObject sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sign.transform.SetParent(group.transform);
            sign.transform.localPosition = new Vector3(0, 5, 0);
            sign.transform.localScale = new Vector3(8, 1, 0.2f);
            sign.GetComponent<Renderer>().material = GetNeonMaterial(palette.Shooting, 3.0f);

            return group;
        }

        public static GameObject CreateHotTub(Vector3 position, ColorPalette palette, bool mirror, float size = 100f)
        {
            GameObject group = new GameObject("HotTub_VIP");
            group.transform.position = position;

            // Base Floor (Visible & Tinted)
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.SetParent(group.transform);
            floor.transform.localPosition = new Vector3(0, -0.25f, 0);
            floor.transform.localScale = new Vector3(size, 0.5f, size);
            floor.GetComponent<Renderer>().material = GetNeonMaterial(new Color(0.1f, 0.1f, 0.15f), 0.3f);

            GameObject tub = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tub.transform.SetParent(group.transform);
            tub.transform.localPosition = new Vector3(0, 0.5f, 0);
            tub.transform.localScale = new Vector3(10, 0.5f, 10);
            tub.GetComponent<Renderer>().material = GetNeonMaterial(palette.Secondary, 1.5f);

            return group;
        }

        public static GameObject CreatePool(Vector3 position, ColorPalette palette, bool isLeftHalf, float size = 100f)
        {
            GameObject group = new GameObject(isLeftHalf ? "PoolLeft" : "PoolRight");
            group.transform.position = position;

            // Pool Border
            GameObject deck = GameObject.CreatePrimitive(PrimitiveType.Cube);
            deck.transform.SetParent(group.transform);
            deck.transform.localPosition = new Vector3(0, -0.15f, 0);
            deck.transform.localScale = new Vector3(size, 0.3f, size);
            deck.GetComponent<Renderer>().material = GetNeonMaterial(new Color(0.1f, 0.1f, 0.15f), 0.2f);

            // Glowing Water
            GameObject water = GameObject.CreatePrimitive(PrimitiveType.Cube);
            water.transform.SetParent(group.transform);
            water.transform.localPosition = new Vector3(0, 0.2f, 0);
            water.transform.localScale = new Vector3(size * 0.9f, 0.1f, size * 0.9f);
            water.GetComponent<Renderer>().material = GetNeonMaterial(palette.Primary, 1.2f);

            return group;
        }

        public static GameObject CreateBridge(Vector3 start, Vector3 end, ColorPalette palette) => null;
    }
}
