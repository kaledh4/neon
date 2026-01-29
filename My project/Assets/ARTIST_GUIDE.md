# 🎨 Procedural Map Designer Guide

Welcome to the **Neon Splash Procedural Engine**. This document outlines how to populate the game world with assets using the **MapGeneratorV2** tool. Your goal is to take the procedural grayboxing and turn it into a "finished looking game" by configuring spawn rules and creating optimized seeds.

---

## 🛠️ 1. Mission Control: The Global Scatter System
We have implemented a "Mission Control" system that allows you to scatter props across the entire map without writing code.

### How to Use:
1.  Select the **MapGenerator** object in the Hierarchy.
2.  Locate the section **"Mission Control - Global Scatter"** in the Inspector.
3.  Click **`+`** to add a new **Scatter Rule**.

### Rule Configuration:
*   **Name**: Give it a descriptive name (e.g., "Neon Crates", "Palm Trees").
*   **Prefab**: Drag and drop your `.fbx` or Prefab here.
*   **Total Count**: How many should exist in the *entire world* (e.g., 50).
*   **Excluded Types**: A list of chunk IDs where this object *should NOT* spawn.
    *   *Example*: Add "BaseStart" and "BaseEnd" to prevent cluttering the player spawn points.
*   **Scale Multiplier**: Adjusts the size of the spawned objects (Default: 1.0).
*   **Height Offset**: Lifts the object up/down. Essential if your model's pivot is in the center instead of the bottom.

---

## 🏗️ 2. Chunk Types
The map is built from a sequence of "Chunks". When setting up **Exclusions**, use these exact IDs:

| Chunk ID | Description |
| :--- | :--- |
| `BaseStart` | Team Blue's spawn area. Keep clear of obstacles. |
| `BaseEnd` | Team Red's spawn area. Keep clear. |
| `PoolStart` / `PoolEnd` | The central objective zone containing the pool. |
| `Trees` | Dense nature chunks. |
| `TikiBar` | Social zones with bar structures. |
| `HotTub` | VIP areas with tub geometry. |
| `Garden` | Open lounge areas. |

---

## 📏 3. Technical Specs for 3D Assets
To ensure your models spawn correctly in the procedural logic:

1.  **Pivot Point**: MUST be at the **bottom center** of the mesh (Y=0). If the pivot is in the center, the object will spawn half-underground.
2.  **Scale**: 1 Unit = 1 Meter.
    *   *Reference*: The character is approx 2 units tall.
    *   *Walls*: The fence height is approx 4 units.
3.  **Colliders**:
    *   Small props (cans, bottles) should have **No Collider** or a Trigger.
    *   Large props (crates, rocks) must have a **Box/Mesh Collider** so players don't walk through them.
4.  **Materials**:
    *   The system supports standard Unity materials.
    *   For the "Neon" look, use standard shaders with **Emission** enabled and set to high intensity.

---

## 🔄 4. Workflow: Finding the "Golden Seed"
Part of the design process is curating the randomness.

1.  **Iterate**: Change the `Seed` value in the MapGenerator (e.g., 12345 -> 555).
2.  **Generate**: Right-click the component title or use the context menu to **"Generate World"** (or press Play).
3.  **Evaluate**: Fly through the level.
    *   *Are paths blocked?* -> Reduce `Total Count` or add exclusions.
    *   *Too empty?* -> Increase `Total Count` or add new Scatter Rules.
4.  **Lock It**: Once you find a seed that feels perfect (good flow, beautiful random arrangement), note it down. That becomes a "Production Seed".

---

## 📝 Request List for Artist
Based on the current logic, we need the following assets to populate the rules:

*   [ ] **Neon Crates / Boxes** (Cover objects)
*   [ ] **Palm Tree Variants** (Tall, Short, curved)
*   [ ] **Lounge Chairs** (For Garden/Pool areas)
*   [ ] **Floating Lanterns** (To scatter at height)
*   [ ] **Bar Stools** (For Tiki sections)
