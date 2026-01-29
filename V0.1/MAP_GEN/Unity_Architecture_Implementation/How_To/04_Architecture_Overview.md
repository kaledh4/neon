# Architecture Overview

**Goal**: Build a system that extracts Meaning from Metadata (Names), not References.

## Why are we doing this?
In standard Unity development, you attach scripts to objects.
*   Tree -> Drag `Wind.cs` onto it.
*   Sign -> Drag `Glow.cs` onto it.

When we have 5,000 objects and we want to change the wind system, we have to touch 5,000 objects. **That is technical debt.**

## The New Way (Semantic Architecture)
1.  **Identity is Name**: `Tree_Oak_01` tells the system "I am a tree".
2.  **Resolvers are Smart**: The `AssetSemanticResolver` scans the scene. It sees `Tree_` and attaches the centralized wind system listener.
3.  **Visuals are Dumb**: The FBX has no scripts. It is just a shape.

## Key Systems

### 1. Asset Semantic Resolver (`Core`)
The brain. It validates names and ensures every object has the components it implies it should have.

### 2. Neon Global (`Visual`)
Allows us to change the color of every "Exit Sign" in the game from one inspector window. No more searching through scenes.

### 3. Parallel Maps (`World`)
We render multiple "layers" of reality. This is managed by `WorldBinder`, which clones or manipulates the root geometry to create visual variants (Spirit World, Cyber World) without needing duplicate scenes.
