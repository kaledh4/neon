# Step-by-Step Integration Guide

Use this guide to start from zero.

## Phase 1: The Unity Project
1.  Open **Unity Hub**.
2.  Click **New Project**.
3.  Select **3D (URP)** or **Universal 3D**.
4.  Name it `MapGen_System`.
5.  Click **Create**.
6.  Wait for Unity to open.

## Phase 2: Installing Dependencies
1.  Inside Unity, go to `Window > Package Manager`.
2.  Change the dropdown from "Packages: In Project" to **"Unity Registry"**.
3.  Search for **"Input System"**.
4.  Click **Install**.
5.  **Click YES** when it asks to restart the Editor.

## Phase 3: Bringing in the Architecture
1.  Close Unity.
2.  Navigate to your Desktop folder: `MAP_GEN/Unity_Architecture_Implementation`.
3.  **COPY** the `Assets` folder.
4.  Navigate to your new Unity Project folder (e.g., `Documents/Unity Projects/MapGen_System`).
5.  **PASTE** the `Assets` folder.
    *   *System asks to Merge?* -> **YES.**
    *   *System asks to Replace?* -> **YES.** (Safe, we are overriding empty defaults).
6.  Open Unity again.

## Phase 4: Validation
1.  Look at the **Console** window.
2.  You should see a message: `[URPBootstrap] ...`.
    *   If it says "Auto-assigned URP Asset", it worked.
    *   If it says "CRITICAL: No URP Asset", go to `Project Settings > Graphics` and assign the URP Asset manually.
3.  Go to `Assets/Scripts/Visual/NeonRegistry.cs`. If it compiles without red errors, you are good.

## Phase 5: Your First Test
1.  Create a cube in the scene.
2.  Name it `Tree_Test_01`.
3.  Look at the Inspector. Use the context menu on `AssetSemanticResolver` (or just check if components appeared - note: default resolver logic in this code base assumes you have the `FoliageSway` script, if you don't, it might just log).
4.  Rename it to `Neon_Cube`.
5.  It should automatically get the `NeonBinder` component.

**You are now ready to work.**
