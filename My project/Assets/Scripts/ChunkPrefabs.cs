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

        private static Texture2D _woodTexture;
        private static Material GetWoodMaterial()
        {
            if (_woodTexture == null)
            {
                int size = 256;
                _woodTexture = new Texture2D(size, size);
                Color brown1 = new Color(0.15f, 0.1f, 0.05f);
                Color brown2 = new Color(0.25f, 0.18f, 0.1f);
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float grain = Mathf.PerlinNoise(x * 0.1f, y * 5.0f);
                        float plank = (x % 64 < 2) ? 0.3f : 1.0f;
                        _woodTexture.SetPixel(x, y, Color.Lerp(brown1, brown2, grain) * plank);
                    }
                }
                _woodTexture.Apply();
            }
            Material mat = new Material(GetAppropriateShader());
            mat.mainTexture = _woodTexture;
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", _woodTexture);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.4f);
            return mat;
        }

        private static Material GetTileMaterial(Color baseColor)
        {
            int size = 128;
            Texture2D tex = new Texture2D(size, size);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool border = (x < 4 || y < 4 || x > size - 5 || y > size - 5);
                    tex.SetPixel(x, y, border ? baseColor * 0.5f : baseColor);
                }
            }
            tex.Apply();
            Material mat = new Material(GetAppropriateShader());
            mat.mainTexture = tex;
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", baseColor * 0.2f);
            return mat;
        }

        public static GameObject CreateBase(Vector3 position, ColorPalette palette, bool isBlue, float size = 100f)
        {
            GameObject group = new GameObject(isBlue ? "BaseBlue" : "BaseRed");
            group.transform.position = position;

            Color teamColor = isBlue ? new Color(0, 0.4f, 1f) : new Color(1f, 0.2f, 0f);
            Color accentColor = isBlue ? palette.Primary : palette.Secondary;

            // Floor (INTERESTING TEAM TILES)
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.SetParent(group.transform);
            floor.transform.localPosition = new Vector3(0, -0.25f, 0);
            floor.transform.localScale = new Vector3(size, 0.5f, size);
            Renderer r = floor.GetComponent<Renderer>();
            r.material = GetTileMaterial(teamColor);
            r.material.mainTextureScale = new Vector2(size / 5f, size / 5f);

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
            GameObject group = new GameObject("Trees_Neon_Complex");
            group.transform.position = position;

            // Base Floor (GREEN NEON STYLE)
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.SetParent(group.transform);
            floor.transform.localPosition = new Vector3(0, -0.25f, 0);
            floor.transform.localScale = new Vector3(size, 0.5f, size);
            floor.GetComponent<Renderer>().material = GetNeonMaterial(new Color(0, 0.5f, 0.1f), 0.3f);

            // 25 Decoration Items
            for (int i = 0; i < 25; i++)
            {
                Vector3 randomPos = new Vector3(Random.Range(-size*0.4f, size*0.4f), 0, Random.Range(-size*0.4f, size*0.4f));
                if (i < 15) // 15 Trees
                {
                    GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    trunk.transform.SetParent(group.transform);
                    trunk.transform.localPosition = randomPos + Vector3.up * 4;
                    trunk.transform.localScale = new Vector3(1, 4, 1);
                    trunk.GetComponent<Renderer>().material = GetMat(new Color(0.1f, 0.1f, 0.1f));

                    GameObject leaves = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    leaves.transform.SetParent(group.transform);
                    leaves.transform.localPosition = randomPos + Vector3.up * 9;
                    leaves.transform.localScale = new Vector3(6, 6, 6);
                    leaves.GetComponent<Renderer>().material = GetNeonMaterial(new Color(0, 1, 0.2f), 0.8f);
                }
                else // 10 Neon Flowers/Lights
                {
                    GameObject flower = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    flower.transform.SetParent(group.transform);
                    flower.transform.localPosition = randomPos + Vector3.up * 0.5f;
                    flower.transform.localScale = Vector3.one * 1.5f;
                    flower.GetComponent<Renderer>().material = GetNeonMaterial(palette.Secondary, 2.0f);
                }
            }

            if (mirror) group.transform.localScale = new Vector3(-1, 1, 1);
            return group;
        }

        public static GameObject CreateGarden(Vector3 position, ColorPalette palette, bool mirror, float size = 100f)
        {
            GameObject group = new GameObject("Garden_VIP_Deck");
            group.transform.position = position;

            // Wood Floor
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.SetParent(group.transform);
            floor.transform.localPosition = new Vector3(0, -0.25f, 0);
            floor.transform.localScale = new Vector3(size, 0.5f, size);
            Renderer rendG = floor.GetComponent<Renderer>();
            rendG.material = GetWoodMaterial();
            rendG.material.mainTextureScale = new Vector2(size / 8f, size / 8f);

            // 25 Items (Chairs, Umbrellas, Lights)
            for (int i = 0; i < 25; i++)
            {
                Vector3 randomPos = new Vector3(Random.Range(-size*0.4f, size*0.4f), 0, Random.Range(-size*0.4f, size*0.4f));
                if (i < 10) // 10 Lounge Chairs
                {
                    GameObject chair = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    chair.transform.SetParent(group.transform);
                    chair.transform.localPosition = randomPos + Vector3.up * 0.5f;
                    chair.transform.localScale = new Vector3(4, 0.5f, 2);
                    chair.GetComponent<Renderer>().material = GetNeonMaterial(palette.Primary, 0.5f);
                }
                else if (i < 15) // 5 Umbrellas
                {
                    GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    pole.transform.SetParent(group.transform);
                    pole.transform.localPosition = randomPos + Vector3.up * 3;
                    pole.transform.localScale = new Vector3(0.2f, 3, 0.2f);
                    
                    GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    top.transform.SetParent(group.transform);
                    top.transform.localPosition = randomPos + Vector3.up * 6;
                    top.transform.localScale = new Vector3(6, 0.1f, 6);
                    top.GetComponent<Renderer>().material = GetNeonMaterial(palette.Secondary, 0.8f);
                }
                else // 10 Floating Neon Orbs
                {
                    GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    orb.transform.SetParent(group.transform);
                    orb.transform.localPosition = randomPos + Vector3.up * 2;
                    orb.transform.localScale = Vector3.one * 1.2f;
                    orb.GetComponent<Renderer>().material = GetNeonMaterial(palette.Shooting, 1.5f);
                }
            }

            return group;
        }

        public static GameObject CreateTikiBar(Vector3 position, ColorPalette palette, bool mirror, float size = 100f)
        {
            GameObject group = new GameObject("TikiBar_Neon_Deluxe");
            group.transform.position = position;

            // Wood Floor
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.SetParent(group.transform);
            floor.transform.localPosition = new Vector3(0, -0.25f, 0);
            floor.transform.localScale = new Vector3(size, 0.5f, size);
            Renderer rendB = floor.GetComponent<Renderer>();
            rendB.material = GetWoodMaterial();
            rendB.material.mainTextureScale = new Vector2(size / 8f, size / 8f);

            // 25 Items (Bar stuff, stools, lights)
            for (int i = 0; i < 25; i++)
            {
                Vector3 randomPos = new Vector3(Random.Range(-size*0.4f, size*0.4f), 0, Random.Range(-size*0.4f, size*0.4f));
                if (i == 0) // Main Bar Table
                {
                    GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    bar.transform.SetParent(group.transform);
                    bar.transform.localPosition = new Vector3(0, 1.5f, 0);
                    bar.transform.localScale = new Vector3(15, 3, 4);
                    bar.GetComponent<Renderer>().material = GetMat(new Color(0.3f, 0.2f, 0.1f));
                }
                else if (i < 12) // 11 Stools
                {
                    GameObject stool = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    stool.transform.SetParent(group.transform);
                    stool.transform.localPosition = randomPos + Vector3.up * 1f;
                    stool.transform.localScale = new Vector3(1.5f, 1f, 1.5f);
                    stool.GetComponent<Renderer>().material = GetNeonMaterial(palette.Primary, 1.2f);
                }
                else // 13 Neon Pillars/Lights
                {
                    GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    pillar.transform.SetParent(group.transform);
                    pillar.transform.localPosition = randomPos + Vector3.up * 4;
                    pillar.transform.localScale = new Vector3(1, 4, 1);
                    pillar.GetComponent<Renderer>().material = GetNeonMaterial(palette.Shooting, 3f);
                }
            }

            return group;
        }

        public static GameObject CreateHotTub(Vector3 position, ColorPalette palette, bool mirror, float size = 100f)
        {
            GameObject group = new GameObject("HotTub_Ultra_VIP");
            group.transform.position = position;

            // Wood Floor
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.SetParent(group.transform);
            floor.transform.localPosition = new Vector3(0, -0.25f, 0);
            floor.transform.localScale = new Vector3(size, 0.5f, size);
            Renderer rendTub = floor.GetComponent<Renderer>();
            rendTub.material = GetWoodMaterial();
            rendTub.material.mainTextureScale = new Vector2(size / 8f, size / 8f);

            // 25 Items (Tubs, candles, towels, orbs)
            for (int i = 0; i < 25; i++)
            {
                Vector3 randomPos = new Vector3(Random.Range(-size*0.4f, size*0.4f), 0, Random.Range(-size*0.4f, size*0.4f));
                if (i < 5) // 5 Tubs
                {
                    GameObject tub = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    tub.transform.SetParent(group.transform);
                    tub.transform.localPosition = randomPos + Vector3.up * 0.5f;
                    tub.transform.localScale = new Vector3(8, 0.5f, 8);
                    tub.GetComponent<Renderer>().material = GetNeonMaterial(palette.Secondary, 1.5f);
                }
                else // 20 Candles/Orbs
                {
                    GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    orb.transform.SetParent(group.transform);
                    orb.transform.localPosition = randomPos + Vector3.up * 1f;
                    orb.transform.localScale = Vector3.one * 0.8f;
                    orb.GetComponent<Renderer>().material = GetNeonMaterial(palette.Primary, 5f);
                }
            }

            return group;
        }

        public static GameObject CreatePool(Vector3 position, ColorPalette palette, bool isLeftHalf, float size = 100f)
        {
            GameObject group = new GameObject(isLeftHalf ? "PoolLeft" : "PoolRight");
            group.transform.position = position;

            // Pool Border (BLUISH TILES)
            GameObject deck = GameObject.CreatePrimitive(PrimitiveType.Cube);
            deck.transform.SetParent(group.transform);
            deck.transform.localPosition = new Vector3(0, -0.15f, 0);
            deck.transform.localScale = new Vector3(size, 0.3f, size);
            Renderer rendD = deck.GetComponent<Renderer>();
            rendD.material = GetTileMaterial(new Color(0, 0.3f, 0.6f));
            rendD.material.mainTextureScale = new Vector2(size/4f, size/4f);

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
