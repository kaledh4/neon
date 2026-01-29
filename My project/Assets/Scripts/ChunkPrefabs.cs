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

        public static void ApplyMicroVariation(Transform t, Renderer r, int seed)
        {
            var rng = new System.Random(seed);
            t.Rotate(0f, (float)(rng.NextDouble() * 12f - 6f), 0f);
            float scale = 0.9f + (float)rng.NextDouble() * 0.2f;
            t.localScale *= scale;
            t.position += Vector3.up * (float)(rng.NextDouble() * 0.1f - 0.05f);

            if (r != null && r.material.HasProperty("_Color"))
            {
                float tint = 0.95f + (float)rng.NextDouble() * 0.1f;
                r.material.color *= tint;
            }
        }

        public static void ApplyRoleColor(Renderer r, float saturationMultiplier)
        {
            if (r == null) return;
            Color c = r.material.color;
            Color.RGBToHSV(c, out float h, out float s, out float v);
            s *= saturationMultiplier;
            r.material.color = Color.HSVToRGB(h, s, v);
            if (r.material.IsKeywordEnabled("_EMISSION"))
            {
                r.material.SetColor("_EmissionColor", r.material.GetColor("_EmissionColor") * saturationMultiplier);
            }
        }

        public static Light SpawnAccentLight(Transform parent, Vector3 localPos, Color color)
        {
            var go = new GameObject("AccentLight");
            go.transform.SetParent(parent);
            go.transform.localPosition = localPos + Vector3.up * 2.5f;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 10f;
            light.intensity = 1.0f;
            light.color = color;
            go.AddComponent<NeonSplash.V0_1.NeonPulse>().baseIntensity = 1.0f;
            return light;
        }

        public static void CreateFakeReflection(GameObject source, Transform parent)
        {
            GameObject refl = GameObject.Instantiate(source, parent);
            refl.name = source.name + "_Reflection";
            refl.transform.localScale = new Vector3(source.transform.localScale.x, -source.transform.localScale.y * 0.5f, source.transform.localScale.z);
            refl.transform.localPosition = new Vector3(source.transform.localPosition.x, -0.5f, source.transform.localPosition.z);
            
            Renderer r = refl.GetComponent<Renderer>();
            if (r != null)
            {
                Material m = new Material(r.material);
                Color c = m.color;
                c.a = 0.2f;
                m.color = c;
                m.SetFloat("_Smoothness", 0f);
                r.material = m;
            }
            // Remove colliders from reflections
            foreach (var col in refl.GetComponentsInChildren<Collider>()) Object.Destroy(col);
        }

        public static bool IsPrimaryPath(Vector3 pos) => Mathf.Abs(pos.z) < 15f; // Center corridor

        public static void AdjustForFlow(GameObject obj, Vector3 pos)
        {
            if (IsPrimaryPath(pos))
                obj.transform.localScale *= 0.7f; // Clear traffic paths
            else
                obj.transform.localScale *= 1.3f; // Denser surroundings
        }

        public static GameObject CreateBase(Vector3 position, ColorPalette palette, bool isBlue, float size = 100f, Material customFloor = null)
        {
            GameObject group = new GameObject(isBlue ? "BaseBlue" : "BaseRed");
            group.transform.position = position;
            var state = group.AddComponent<NeonSplash.V0_1.ChunkStateController>();

            Color teamColor = isBlue ? new Color(0, 0.4f, 1f) : new Color(1f, 0.2f, 0f);
            Color accentColor = isBlue ? palette.Primary : palette.Secondary;

            // Floor (INTERESTING TEAM TILES)
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.SetParent(group.transform);
            floor.transform.localPosition = new Vector3(0, -0.25f, 0);
            floor.transform.localScale = new Vector3(size, 0.5f, size);
            Renderer r = floor.GetComponent<Renderer>();
            
            if (customFloor != null) r.material = customFloor;
            else r.material = GetTileMaterial(teamColor);

            r.material.mainTextureScale = new Vector2(size / 5f, size / 5f);
            ApplyRoleColor(r, 0.6f); 

            // Accent light
            state.lights.Add(SpawnAccentLight(group.transform, Vector3.zero, teamColor));

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

        private static Texture2D _grassTexture;
        private static Material GetGrassMaterial()
        {
            if (_grassTexture == null)
            {
                int size = 256;
                _grassTexture = new Texture2D(size, size);
                Color darkGreen = new Color(0.05f, 0.2f, 0.05f); // Deep forest green
                Color lightGreen = new Color(0.1f, 0.35f, 0.1f); // Vibrant grass green
                
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        // Layered Perlin noise for organic look
                        float noise1 = Mathf.PerlinNoise(x * 0.05f, y * 0.05f);
                        float noise2 = Mathf.PerlinNoise(x * 0.2f, y * 0.2f) * 0.3f;
                        float noise3 = Mathf.PerlinNoise(x * 0.8f, y * 0.8f) * 0.1f; // Detail grit
                        
                        float blend = Mathf.Clamp01(noise1 + noise2 + noise3);
                        _grassTexture.SetPixel(x, y, Color.Lerp(darkGreen, lightGreen, blend));
                    }
                }
                _grassTexture.Apply();
            }
            Material mat = new Material(GetAppropriateShader());
            mat.mainTexture = _grassTexture;
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", _grassTexture); // URP support
            
            // Adjust smoothness to look like matte grass, not plastic
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.1f);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.1f);
            
            return mat;
        }

        public static GameObject CreateTrees(Vector3 position, ColorPalette palette, bool mirror, float size = 100f, float floorHeightOffset = 0f, Material customFloor = null)
        {
            GameObject group = new GameObject("Trees_Neon_Complex");
            group.transform.position = position;
            var state = group.AddComponent<NeonSplash.V0_1.ChunkStateController>();

            // 1. IMPROVED GRASS FLOOR
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.SetParent(group.transform);
            // Apply Floor Height Offset Here
            floor.transform.localPosition = new Vector3(0, -0.25f + floorHeightOffset, 0);
            floor.transform.localScale = new Vector3(size, 0.5f, size);
            Renderer rFloor = floor.GetComponent<Renderer>();
            
            if (customFloor != null) rFloor.material = customFloor;
            else rFloor.material = GetGrassMaterial(); 
            
            // 2. CLEARED PRIMITIVE TREES & PYLONS
            // As requested: Old yellow lines (Pylons) removed to keep it clean for FBX trees.
            // FBX trees will be spawned by MapGeneratorV2.

            state.lights.Add(SpawnAccentLight(group.transform, Vector3.zero, new Color(0, 1, 0.2f)));

            if (mirror) group.transform.localScale = new Vector3(-1, 1, 1);
            return group;
        }

        public static GameObject CreateGarden(Vector3 position, ColorPalette palette, bool mirror, float size = 100f, float floorHeightOffset = 0f, Material customFloor = null)
        {
            GameObject group = new GameObject("Garden_VIP_Deck");
            group.transform.position = position;
            var state = group.AddComponent<NeonSplash.V0_1.ChunkStateController>();

            // Wood Floor
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.SetParent(group.transform);
            floor.transform.localPosition = new Vector3(0, -0.25f + floorHeightOffset, 0);
            floor.transform.localScale = new Vector3(size, 0.5f, size);
            Renderer rendG = floor.GetComponent<Renderer>();
            
            if (customFloor != null) rendG.material = customFloor;
            else rendG.material = GetWoodMaterial();

            rendG.material.mainTextureScale = new Vector2(size / 8f, size / 8f);
            ApplyRoleColor(rendG, 0.6f);

            // 25 Items (Chairs, Umbrellas, Lights) ...
            for (int i = 0; i < 25; i++)
            {
                Vector3 randomPos = new Vector3(Random.Range(-size*0.4f, size*0.4f), 0, Random.Range(-size*0.4f, size*0.4f));
                // Lift items slightly if floor is raised, though usually they parent to group at Y=0.
                // If floor is moved down, items float. 
                // We should probably move items with floor offset too OR assume items are on Y=0 relative to group.
                // Ideally, if floor moves, items should move? 
                // The user asked to "LOWER FLOOR". If we lower floor, items at Y=0 will float.
                // So we must offset items too.
                
                float itemYOffset = floorHeightOffset;

                GameObject item;
                if (i < 10) // 10 Lounge Chairs
                {
                    item = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    item.transform.SetParent(group.transform);
                    item.transform.localPosition = randomPos + Vector3.up * (0.5f + itemYOffset);
                    item.transform.localScale = new Vector3(4, 0.5f, 2);
                    var rC = item.GetComponent<Renderer>();
                    rC.material = GetNeonMaterial(palette.Primary, 0.5f);
                    ApplyRoleColor(rC, 0.8f);
                }
                else if (i < 15) // 5 Umbrellas
                {
                    item = new GameObject("Umbrella_Tall");
                    item.transform.SetParent(group.transform);
                    item.transform.localPosition = randomPos + Vector3.up * itemYOffset;
                    
                    float hBias = 1f + Mathf.PerlinNoise(randomPos.x * 0.5f, randomPos.z * 0.5f) * 0.5f;

                    GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    pole.transform.SetParent(item.transform);
                    pole.transform.localPosition = Vector3.up * 3 * hBias;
                    pole.transform.localScale = new Vector3(0.2f, 3 * hBias, 0.2f);
                    pole.GetComponent<Renderer>().material = GetMat(new Color(0.05f, 0.05f, 0.05f));
                    
                    GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    top.transform.SetParent(item.transform);
                    top.transform.localPosition = Vector3.up * 6 * hBias;
                    top.transform.localScale = new Vector3(7, 0.15f, 7);
                    var rU = top.GetComponent<Renderer>();
                    rU.material = GetNeonMaterial(palette.Secondary, 1.5f);
                    ApplyRoleColor(rU, 1.0f);
                    
                    var pulse = top.AddComponent<NeonSplash.V0_1.NeonPulse>();
                    pulse.role = NeonSplash.V0_1.NeonPulse.PulseType.Building;
                }
                else // 10 Floating Neon Orbs
                {
                    item = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    item.transform.SetParent(group.transform);
                    item.transform.localPosition = randomPos + Vector3.up * (3f + itemYOffset);
                    item.transform.localScale = Vector3.one * 1.5f;
                    var rO = item.GetComponent<Renderer>();
                    rO.material = GetNeonMaterial(palette.Shooting, 3f);
                    ApplyRoleColor(rO, 1.2f);
                    var pulse = item.AddComponent<NeonSplash.V0_1.NeonPulse>();
                    pulse.role = NeonSplash.V0_1.NeonPulse.PulseType.Objective;
                }
                AdjustForFlow(item, randomPos);
                ApplyMicroVariation(item.transform, item.GetComponent<Renderer>(), (int)(position.z + i));
            }

            state.lights.Add(SpawnAccentLight(group.transform, Vector3.zero, palette.Primary));
            return group;
        }

        public static GameObject CreateTikiBar(Vector3 position, ColorPalette palette, bool mirror, float size = 100f, float floorHeightOffset = 0f, Material customFloor = null)
        {
            GameObject group = new GameObject("TikiBar_Neon_Deluxe");
            group.transform.position = position;
            var state = group.AddComponent<NeonSplash.V0_1.ChunkStateController>();

            // Wood Floor
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.SetParent(group.transform);
            floor.transform.localPosition = new Vector3(0, -0.25f + floorHeightOffset, 0);
            floor.transform.localScale = new Vector3(size, 0.5f, size);
            Renderer rendB = floor.GetComponent<Renderer>();
            
            if (customFloor != null) rendB.material = customFloor;
            else rendB.material = GetWoodMaterial();

            rendB.material.mainTextureScale = new Vector2(size / 8f, size / 8f);
            ApplyRoleColor(rendB, 0.6f);

            float itemYOffset = floorHeightOffset;

            // 25 Items (Bar stuff, stools, lights)
            for (int i = 0; i < 25; i++)
            {
                Vector3 randomPos = new Vector3(Random.Range(-size*0.4f, size*0.4f), 0, Random.Range(-size*0.4f, size*0.4f));
                GameObject item;
                if (i == 0) // Main Bar Table
                {
                    item = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    item.transform.SetParent(group.transform);
                    item.transform.localPosition = new Vector3(0, 1.5f + itemYOffset, 0);
                    item.transform.localScale = new Vector3(15, 3, 4);
                    item.GetComponent<Renderer>().material = GetMat(new Color(0.3f, 0.2f, 0.1f));
                }
                else if (i < 12) // 11 Stools
                {
                    item = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    item.transform.SetParent(group.transform);
                    item.transform.localPosition = randomPos + Vector3.up * (1f + itemYOffset);
                    item.transform.localScale = new Vector3(1.5f, 1f, 1.5f);
                    var rS = item.GetComponent<Renderer>();
                    rS.material = GetNeonMaterial(palette.Primary, 1.2f);
                    ApplyRoleColor(rS, 1.0f);
                }
                else // 13 Neon Pillars/Lights
                {
                    item = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    item.transform.SetParent(group.transform);
                    item.transform.localPosition = randomPos + Vector3.up * (4 + itemYOffset);
                    item.transform.localScale = new Vector3(1, 4, 1);
                    var rP = item.GetComponent<Renderer>();
                    rP.material = GetNeonMaterial(palette.Shooting, 3f);
                    ApplyRoleColor(rP, 1.2f);
                }
                AdjustForFlow(item, randomPos);
                ApplyMicroVariation(item.transform, item.GetComponent<Renderer>(), (int)(position.x - position.z + i));
            }

            state.lights.Add(SpawnAccentLight(group.transform, Vector3.zero, palette.Shooting));
            return group;
        }

        public static GameObject CreateHotTub(Vector3 position, ColorPalette palette, bool mirror, float size = 100f, float floorHeightOffset = 0f, Material customFloor = null)
        {
            GameObject group = new GameObject("HotTub_Ultra_VIP");
            group.transform.position = position;
            var state = group.AddComponent<NeonSplash.V0_1.ChunkStateController>();

            // Wood Floor
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.SetParent(group.transform);
            floor.transform.localPosition = new Vector3(0, -0.25f + floorHeightOffset, 0);
            floor.transform.localScale = new Vector3(size, 0.5f, size);
            Renderer rendTub = floor.GetComponent<Renderer>();

            if (customFloor != null) rendTub.material = customFloor;
            else rendTub.material = GetWoodMaterial();

            rendTub.material.mainTextureScale = new Vector2(size / 8f, size / 8f);
            ApplyRoleColor(rendTub, 0.6f);

            float itemYOffset = floorHeightOffset;

            // 25 Items (Tubs, candles, towels, orbs)
            for (int i = 0; i < 25; i++)
            {
                Vector3 randomPos = new Vector3(Random.Range(-size*0.4f, size*0.4f), 0, Random.Range(-size*0.4f, size*0.4f));
                GameObject item;
                if (i < 5) // 5 Tubs
                {
                    item = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    item.transform.SetParent(group.transform);
                    item.transform.localPosition = randomPos + Vector3.up * (0.5f + itemYOffset);
                    item.transform.localScale = new Vector3(8, 0.5f, 8);
                    var rT = item.GetComponent<Renderer>();
                    rT.material = GetNeonMaterial(palette.Secondary, 1.5f);
                    ApplyRoleColor(rT, 1.0f);
                    item.AddComponent<NeonSplash.V0_1.NeonPulse>().amplitude = 0.1f;
                }
                else // 20 Candles/Orbs
                {
                    item = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    item.transform.SetParent(group.transform);
                    item.transform.localPosition = randomPos + Vector3.up * (1f + itemYOffset);
                    item.transform.localScale = Vector3.one * 0.8f;
                    var rO = item.GetComponent<Renderer>();
                    rO.material = GetNeonMaterial(palette.Primary, 5f);
                    ApplyRoleColor(rO, 1.4f); 
                }
                AdjustForFlow(item, randomPos);
                ApplyMicroVariation(item.transform, item.GetComponent<Renderer>(), (int)(position.x * position.z + i));
            }

            state.lights.Add(SpawnAccentLight(group.transform, Vector3.zero, palette.Secondary));
            return group;
        }

        public static GameObject CreatePool(Vector3 position, ColorPalette palette, bool isLeftHalf, float size = 100f, float floorHeightOffset = 0f, Material customFloor = null)
        {
            GameObject group = new GameObject(isLeftHalf ? "PoolLeft" : "PoolRight");
            group.transform.position = position;
            var state = group.AddComponent<NeonSplash.V0_1.ChunkStateController>();

            // Pool Border (BLUISH TILES)
            GameObject deck = GameObject.CreatePrimitive(PrimitiveType.Cube);
            deck.transform.SetParent(group.transform);
            deck.transform.localPosition = new Vector3(0, -0.15f + floorHeightOffset, 0);
            deck.transform.localScale = new Vector3(size, 0.3f, size);
            Renderer rendD = deck.GetComponent<Renderer>();
            
            if (customFloor != null) rendD.material = customFloor;
            else rendD.material = GetTileMaterial(new Color(0, 0.3f, 0.6f));

            rendD.material.mainTextureScale = new Vector2(size/4f, size/4f);
            ApplyRoleColor(rendD, 0.6f);

            // Glowing Water (System 6 - Water as Light source)
            GameObject water = GameObject.CreatePrimitive(PrimitiveType.Cube);
            water.transform.SetParent(group.transform);
            water.transform.localPosition = new Vector3(0, 0.2f, 0);
            water.transform.localScale = new Vector3(size * 0.95f, 0.15f, size * 0.95f);
            var rendW = water.GetComponent<Renderer>();
            rendW.material = GetNeonMaterial(palette.Primary, 3.0f);
            ApplyRoleColor(rendW, 0.8f);

            // River Lights (System 6)
            for (int i = 1; i <= 3; i++)
            {
                GameObject lightObj = new GameObject("WaterGlowLight");
                lightObj.transform.SetParent(group.transform);
                lightObj.transform.localPosition = new Vector3((i-2) * 25f, -2f, 0);
                var l = lightObj.AddComponent<Light>();
                l.type = LightType.Point;
                l.intensity = 2.0f;
                l.range = 30f;
                l.color = palette.Primary;
                var pulse = lightObj.AddComponent<NeonSplash.V0_1.NeonPulse>();
                pulse.role = NeonSplash.V0_1.NeonPulse.PulseType.Building;
                state.lights.Add(l);
            }

            // Fake Reflections
            for (int i = 0; i < 5; i++)
            {
                GameObject dummy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                dummy.transform.SetParent(group.transform);
                dummy.transform.localPosition = new Vector3(Random.Range(-size*0.3f, size*0.3f), 2f, Random.Range(-size*0.3f, size*0.3f));
                dummy.GetComponent<Renderer>().material = GetNeonMaterial(palette.Shooting, 2f);
                CreateFakeReflection(dummy, group.transform);
            }

            state.lights.Add(SpawnAccentLight(group.transform, Vector3.forward * 20f, palette.Primary));
            return group;
        }

        public static GameObject CreateBridge(Vector3 start, Vector3 end, ColorPalette palette) => null;
    }
}
