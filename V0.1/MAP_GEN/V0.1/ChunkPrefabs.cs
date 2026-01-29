using UnityEngine;
using System.Collections.Generic;

namespace NeonSplash
{
    public static class ChunkPrefabs
    {
        private static Shader _cachedShader;
        private static Shader GetAppropriateShader()
        {
            if (_cachedShader != null) return _cachedShader;

            // Try URP/Lit first, then Standard
            _cachedShader = Shader.Find("Universal Render Pipeline/Lit");
            if (_cachedShader == null) _cachedShader = Shader.Find("Standard");
            if (_cachedShader == null) _cachedShader = Shader.Find("Public/Lit"); // Sometimes used in newer versions
            
            return _cachedShader;
        }

        private static Material GetNeonMaterial(Color color, float intensity = 1.0f)
        {
            Material mat = new Material(GetAppropriateShader());
            mat.color = color;
            
            // Replicate the neon glow
            mat.EnableKeyword("_EMISSION");
            
            // URP uses _EmissionColor, Built-in uses _EmissionColor
            // But URP needs the keyword _EMISSION enabled and the color set
            mat.SetColor("_EmissionColor", color * intensity);
            
            // For URP specifically
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

        public static GameObject CreateBase(Vector3 position, ColorPalette palette, bool isBlue)
        {
            GameObject group = new GameObject(isBlue ? "BaseBlue" : "BaseRed");
            group.transform.position = position;

            Color teamColor = isBlue ? new Color(0, 0.66f, 1) : new Color(1, 0.2f, 0);
            Color glowColor = isBlue ? new Color(0, 0.53f, 1) : new Color(1, 0.33f, 0);

            // Floor
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.SetParent(group.transform);
            floor.transform.localPosition = new Vector3(0, -0.25f, 0);
            floor.transform.localScale = new Vector3(10, 0.5f, 10);
            floor.GetComponent<Renderer>().material = GetNeonMaterial(teamColor, 0.4f);

            // Pillar
            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.transform.SetParent(group.transform);
            pillar.transform.localPosition = new Vector3(0, 3, 0);
            pillar.transform.localScale = new Vector3(3, 3, 3); // Cylinder is 2 units high by default
            pillar.GetComponent<Renderer>().material = GetNeonMaterial(teamColor, 1.0f);

            // Floating Ring (simplified as a flat cylinder or sphere)
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.transform.SetParent(group.transform);
            ring.transform.localPosition = new Vector3(0, 5, 0);
            ring.transform.localScale = new Vector3(5, 0.1f, 5);
            ring.GetComponent<Renderer>().material = GetNeonMaterial(glowColor, 2.0f);

            // Corner posts
            Vector2[] corners = { new Vector2(-4, -4), new Vector2(-4, 4), new Vector2(4, -4), new Vector2(4, 4) };
            foreach (var c in corners)
            {
                GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.transform.SetParent(group.transform);
                post.transform.localPosition = new Vector3(c.x, 1.5f, c.y);
                post.transform.localScale = new Vector3(0.4f, 1.5f, 0.4f);
                post.GetComponent<Renderer>().material = GetMat(new Color(0.13f, 0.13f, 0.13f));

                GameObject light = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                light.transform.SetParent(group.transform);
                light.transform.localPosition = new Vector3(c.x, 3.2f, c.y);
                light.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                light.GetComponent<Renderer>().material = GetNeonMaterial(glowColor, 2.0f);
            }

            return group;
        }

        public static GameObject CreateTrees(Vector3 position, ColorPalette palette, bool mirror)
        {
            GameObject group = new GameObject("Trees");
            group.transform.position = position;

            // Ground
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.SetParent(group.transform);
            floor.transform.localPosition = new Vector3(0, -0.15f, 0);
            floor.transform.localScale = new Vector3(10, 0.3f, 10);
            floor.GetComponent<Renderer>().material = GetNeonMaterial(new Color(0.06f, 0.06f, 0.13f), 0.05f);

            Vector2[] treePos = { new Vector2(-3, -2), new Vector2(2, 3), new Vector2(-1, 1), new Vector2(3, -3) };
            for (int i = 0; i < treePos.Length; i++)
            {
                float height = 3 + (i % 2);
                GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                trunk.transform.SetParent(group.transform);
                trunk.transform.localPosition = new Vector3(treePos[i].x, height / 2, treePos[i].y);
                trunk.transform.localScale = new Vector3(0.4f, height / 2, 0.4f);
                trunk.GetComponent<Renderer>().material = GetNeonMaterial(palette.Secondary, 0.3f);

                // Fronds (using cubes or cones)
                for (int j = 0; j < 4; j++)
                {
                    GameObject frond = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    frond.transform.SetParent(group.transform);
                    float angle = (j / 4f) * Mathf.PI * 2f;
                    frond.transform.localPosition = new Vector3(
                        treePos[i].x + Mathf.Cos(angle) * 0.5f,
                        height + 0.5f,
                        treePos[i].y + Mathf.Sin(angle) * 0.5f
                    );
                    frond.transform.localScale = new Vector3(0.8f, 0.2f, 1.5f);
                    frond.transform.rotation = Quaternion.Euler(20, angle * Mathf.Rad2Deg, 0);
                    frond.GetComponent<Renderer>().material = GetNeonMaterial(palette.Primary, 0.5f);
                }
            }

            if (mirror) group.transform.localScale = new Vector3(-1, 1, 1);
            return group;
        }

        public static GameObject CreateGarden(Vector3 position, ColorPalette palette, bool mirror)
        {
            GameObject group = new GameObject("Garden");
            group.transform.position = position;

            // Grass
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.SetParent(group.transform);
            floor.transform.localPosition = new Vector3(0, -0.15f, 0);
            floor.transform.localScale = new Vector3(10, 0.3f, 10);
            floor.GetComponent<Renderer>().material = GetNeonMaterial(new Color(0, 0.13f, 0.06f), 0.2f);

            float[] xPos = { -2.5f, 2.5f };
            foreach (float x in xPos)
            {
                GameObject planter = GameObject.CreatePrimitive(PrimitiveType.Cube);
                planter.transform.SetParent(group.transform);
                planter.transform.localPosition = new Vector3(x, 0.3f, 0);
                planter.transform.localScale = new Vector3(3, 0.6f, 6);
                planter.GetComponent<Renderer>().material = GetMat(new Color(0.2f, 0.2f, 0.2f));

                for (int i = 0; i < 5; i++)
                {
                    GameObject flower = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    flower.transform.SetParent(group.transform);
                    flower.transform.localPosition = new Vector3(x + (Random.value - 0.5f) * 2f, 0.8f, (i - 2) * 1.2f);
                    flower.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                    flower.GetComponent<Renderer>().material = GetMat(i % 2 == 0 ? palette.Primary : palette.Secondary);

                    GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    stem.transform.SetParent(group.transform);
                    stem.transform.localPosition = new Vector3(flower.transform.localPosition.x, 0.5f, flower.transform.localPosition.z);
                    stem.transform.localScale = new Vector3(0.06f, 0.2f, 0.06f);
                    stem.GetComponent<Renderer>().material = GetMat(Color.green);
                }
            }

            // Path
            GameObject path = GameObject.CreatePrimitive(PrimitiveType.Cube);
            path.transform.SetParent(group.transform);
            path.transform.localPosition = new Vector3(0, 0.05f, 0);
            path.transform.localScale = new Vector3(2, 0.05f, 8);
            path.GetComponent<Renderer>().material = GetNeonMaterial(palette.Primary, 0.2f);

            if (mirror) group.transform.localScale = new Vector3(-1, 1, 1);
            return group;
        }

        public static GameObject CreateTikiBar(Vector3 position, ColorPalette palette, bool mirror)
        {
            GameObject group = new GameObject("TikiBar");
            group.transform.position = position;

            // Deck
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.SetParent(group.transform);
            floor.transform.localPosition = new Vector3(0, -0.2f, 0);
            floor.transform.localScale = new Vector3(10, 0.4f, 10);
            floor.GetComponent<Renderer>().material = GetMat(new Color(0.26f, 0.13f, 0));

            // Counter
            GameObject counter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            counter.transform.SetParent(group.transform);
            counter.transform.localPosition = new Vector3(0, 0.6f, -2);
            counter.transform.localScale = new Vector3(6, 1.2f, 1.5f);
            counter.GetComponent<Renderer>().material = GetNeonMaterial(new Color(0.33f, 0.2f, 0.06f), 0.1f);

            // Sign
            GameObject signFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            signFrame.transform.SetParent(group.transform);
            signFrame.transform.localPosition = new Vector3(0, 2.5f, -4);
            signFrame.transform.localScale = new Vector3(4, 1.5f, 0.1f);
            signFrame.GetComponent<Renderer>().material = GetMat(Color.black);

            GameObject signGlow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            signGlow.transform.SetParent(group.transform);
            signGlow.transform.localPosition = new Vector3(0, 2.5f, -3.9f);
            signGlow.transform.localScale = new Vector3(3.5f, 1, 0.2f);
            signGlow.GetComponent<Renderer>().material = GetNeonMaterial(palette.Primary, 1.5f);

            for (int i = -2; i <= 2; i++)
            {
                GameObject stool = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stool.transform.SetParent(group.transform);
                stool.transform.localPosition = new Vector3(i * 1.2f, 0.4f, -0.5f);
                stool.transform.localScale = new Vector3(0.6f, 0.4f, 0.6f);
                stool.GetComponent<Renderer>().material = GetNeonMaterial(palette.Secondary, 0.3f);
            }

            if (mirror) group.transform.localScale = new Vector3(-1, 1, 1);
            return group;
        }

        public static GameObject CreateHotTub(Vector3 position, ColorPalette palette, bool mirror)
        {
            GameObject group = new GameObject("HotTub");
            group.transform.position = position;

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.SetParent(group.transform);
            floor.transform.localPosition = new Vector3(0, -0.15f, 0);
            floor.transform.localScale = new Vector3(10, 0.3f, 10);
            floor.GetComponent<Renderer>().material = GetMat(new Color(0.2f, 0.2f, 0.26f));

            GameObject tub = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tub.transform.SetParent(group.transform);
            tub.transform.localPosition = new Vector3(0, 0.6f, 0);
            tub.transform.localScale = new Vector3(6, 0.6f, 6);
            tub.GetComponent<Renderer>().material = GetMat(new Color(0.13f, 0.13f, 0.2f));

            GameObject water = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            water.transform.SetParent(group.transform);
            water.transform.localPosition = new Vector3(0, 1.1f, 0);
            water.transform.localScale = new Vector3(5.6f, 0.05f, 5.6f);
            water.GetComponent<Renderer>().material = GetNeonMaterial(palette.Primary, 0.8f);

            if (mirror) group.transform.localScale = new Vector3(-1, 1, 1);
            return group;
        }

        public static GameObject CreatePool(Vector3 position, ColorPalette palette, bool isLeftHalf)
        {
            GameObject group = new GameObject(isLeftHalf ? "PoolLeft" : "PoolRight");
            group.transform.position = position;

            GameObject deck = GameObject.CreatePrimitive(PrimitiveType.Cube);
            deck.transform.SetParent(group.transform);
            deck.transform.localPosition = new Vector3(0, -0.15f, 0);
            deck.transform.localScale = new Vector3(10, 0.3f, 10);
            deck.GetComponent<Renderer>().material = GetMat(new Color(0.26f, 0.33f, 0.4f));

            float basinOffset = isLeftHalf ? 1 : -1;
            GameObject basin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            basin.transform.SetParent(group.transform);
            basin.transform.localPosition = new Vector3(basinOffset, -0.5f, 0);
            basin.transform.localScale = new Vector3(8, 1.5f, 8);
            basin.GetComponent<Renderer>().material = GetMat(new Color(0.06f, 0.13f, 0.2f));

            GameObject water = GameObject.CreatePrimitive(PrimitiveType.Cube);
            water.transform.SetParent(group.transform);
            water.transform.localPosition = new Vector3(basinOffset, 0.15f, 0);
            water.transform.localScale = new Vector3(7.5f, 0.1f, 7.5f);
            water.GetComponent<Renderer>().material = GetNeonMaterial(palette.Primary, 0.7f);

            return group;
        }

        public static GameObject CreateBridge(Vector3 start, Vector3 end, ColorPalette palette)
        {
            GameObject group = new GameObject("Bridge");
            
            Vector3 diff = end - start;
            float length = diff.magnitude;
            
            group.transform.position = start + diff * 0.5f;
            group.transform.position += new Vector3(0, 0.15f, 0);
            group.transform.rotation = Quaternion.LookRotation(diff);
            group.transform.Rotate(0, 90, 0); // Correct for horizontal alignment

            GameObject deck = GameObject.CreatePrimitive(PrimitiveType.Cube);
            deck.transform.SetParent(group.transform);
            deck.transform.localScale = new Vector3(length, 0.3f, 2.5f);
            deck.GetComponent<Renderer>().material = GetNeonMaterial(palette.Primary, 0.3f);

            // Rails
            GameObject rail1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail1.transform.SetParent(group.transform);
            rail1.transform.localPosition = new Vector3(0, 0.5f, 1.1f);
            rail1.transform.localScale = new Vector3(length, 0.8f, 0.15f);
            rail1.GetComponent<Renderer>().material = GetNeonMaterial(palette.Primary, 1.0f);

            GameObject rail2 = GameObject.Instantiate(rail1, group.transform);
            rail2.transform.localPosition = new Vector3(0, 0.5f, -1.1f);

            return group;
        }
    }
}
