# Neon Lighting Setup

This project uses a **Global Neon Registry**. You do not check "Material.Emission" in the inspector. You configure a **Channel**.

## 1. Concepts

*   **Registry**: The brain. Holds the list of Channels.
*   **Channel**: A named data stream (e.g., "Bar_Sign", "Cyber_Grid", "Warning_Light"). Defines Color, Pulse Speed, Intensity.
*   **Binder**: The script on the object (`NeonBinder.cs`). Listens to a Channel and updates the `MeshRenderer` or `Light`.

## 2. Setting Up a New Light Type

### Step A: Create the Channel
1.  Go to `Ascets/_Game/Data/NeonChannels/` (create folder if needed).
2.  Right Click > `Architecture` > `Neon Channel`.
3.  Name the file `Channel_CyberPink`.
4.  In Inspector:
    *   **Channel Name**: `CyberPink` (Must match what you put in the binder!)
    *   **Base Color**: Pink (HDR).
    *   **Pulsing**: Checked.
    *   **Pulse Speed**: 2.

### Step B: Register It
1.  Find the `NeonRegistry` object in your scene (or create one).
2.  Add your new `Channel_CyberPink` asset to the **Defined Channels** list.

### Step C: Apply to Objects
1.  Ideally, just name your object `Neon_CyberPink_Sign`.
2.  The `AssetSemanticResolver` will:
    *   Add `NeonBinder`.
    *   Set `ChannelID` to `CyberPink` (it parses the middle word!).
3.  **Manual Way**: Add component `NeonBinder`, type `CyberPink` into "Channel ID".

## 3. Controlling Logic (Day/Night)
To turn off all lights, you don't find 1000 objects. You write one script:
```csharp
NeonRegistry.Instance.GetChannel("CyberPink").intensity = 0;
```
Done.
