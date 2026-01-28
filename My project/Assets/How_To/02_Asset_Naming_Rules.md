# Asset Naming Rules

**"Scripts are Smart, Models are Dumb."**
Your 3D models should just be geometry. The **Name** tells the engine what they *are*.

## The Golden List

| Prefix | Category | What happens automatically? |
| :--- | :--- | :--- |
| **`Tree_*`** | Vegetation | Adds `WindSway`, tag=`Nature`, Layer=`Obstacle` |
| **`Bush_*`** | Vegetation | Adds `WindSway`, tag=`Nature` (Smaller sway) |
| **`Floor_*`** | Ground | Layer=`Ground`, Static for NavMesh |
| **`Wall_*`** | Structure | Layer=`Obstacle`, Occlusion Static |
| **`Neon_*`** | Lighting | Adds `NeonBinder` (Connects to Global Neon Registry, SEE: 03_Neon_Setup) |
| **`Int_*`** | Interactable | Adds `InteractionTrigger`, Layer=`Interactable` |
| **`Prop_*`** | Decoration | Basic collider, Lightweight physics if small |

## Usage Examples

*   `Tree_Oak_Large_01.fbx` -> Correct. System knows it is a tree.
*   `Cyber_Tree_01.fbx` -> **WRONG**. System ignores `Cyber_`. Rename to `Tree_Cyber_01`.
*   `Neon_Bar_Sign.fbx` -> Correct. Will hook into "Bar" or "Default" neon channel.
*   `Table_Metal.fbx` -> **WRONG**. Use `Prop_Table_Metal`.

## What if I want a new rule?
If you need `Water_*`, tell the Programmer. They will add one line to `AssetSemanticResolver.cs`.
**DO NOT** manually attach scripts to 500 water tiles.
