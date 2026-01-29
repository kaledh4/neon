# Quick Start: Where do files go?

Stop. Before you do anything, read this.

## 1. Folder Structure
Organize your `Assets` folder like this to keep the Auto-Import system happy.

```text
Assets/
  ├── _Game/                  <-- YOUR CONTENT GOES HERE
  │   ├── Models/             <-- FBX / OBJ files
  │   │   ├── Environment/
  │   │   ├── Props/
  │   │   └── Characters/
  │   ├── Materials/          <-- Materials (Standard or Shader Graphs)
  │   ├── Prefabs/            <-- ONLY for complex assembled objects
  │   └── Scenes/
  │
  ├── Scripts/                <-- The Logic (Do not touch unless coding)
  │   ├── Core/
  │   ├── Visual/
  │   └── World/
```

## 2. Typical Workflow

1.  **Get the FBX**: You have a `CyberTree.fbx`.
2.  **Rename It**: Rename it **OUTSIDE** Unity or immediately after import to `Tree_CyberOak_01`.
3.  **Place It**: Drag `Tree_CyberOak_01` into `Assets/_Game/Models/Environment/`.
4.  **Check Import**: The `FBXImportPostprocessor` will see `Tree_*` and might auto-set settings (e.g., Generate Colliders).
5.  **Use It**: Drag it into the Scene.
6.  **Play**: The `AssetSemanticResolver` wakes up, sees `Tree_`, and adds the `FoliageSway` script automatically.

## 3. Where do Neon textures go?
If you have a texture that is meant to glow:
1.  Put it in `Assets/_Game/Textures/Neon/`.
2.  Create a Material.
3.  Name the **Object** using that material `Neon_Sign_Open`.
4.  The system handles the rest.
