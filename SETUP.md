# Steiner Blocks — Unity 6 Project Setup

This guide walks through setting up the modernized Steiner Blocks project in Unity 6.

## Prerequisites

- **Unity 6** (6000.0 or later) installed via Unity Hub
- Unity modules installed:
  - WebGL Build Support
  - Android Build Support (for Quest / Android XR)
  - visionOS Build Support (for Apple Vision Pro)

## Step 1: Open in Unity 6

1. Open Unity Hub → **Add** → select this repository folder
2. Set the Unity version to **6000.0** or later
3. Unity will prompt to upgrade the project — accept the upgrade
4. Unity will import packages from `Packages/manifest.json` (XRI, OpenXR, AR Foundation, etc.)

> **Note:** The old `Assets/HoloToolkit/` and `Assets/Scripts/` folders contain the legacy Unity 5.5 code.
> They will produce compile errors. Delete them after confirming the new scripts work:
> - Delete `Assets/HoloToolkit/`
> - Delete `Assets/HoloToolkit-Tests/`
> - Delete `Assets/Scripts/` (the old 8 scripts)
> - Delete `Assets/WindowsStoreApp/` (old UWP build output)

## Step 2: Move Resources

The existing resources need to be accessible to the new code:

### Block Textures
The block textures at `Assets/Resources/Textures/` are already in the right place.
The new `BlockController.cs` loads them via `Resources.Load<Texture>("Textures/block_XXX")`.

### Block Patterns (.blocks files)
Move the `.blocks` pattern files so they can be loaded as TextAssets:

```
Assets/Resources/Blocks/
├── 001.blocks    (rename from .blocks to .blocks.txt or use a custom importer)
├── 002.blocks
├── ...
└── my.blocks
```

> **Important:** Unity's `Resources.Load<TextAsset>()` requires files to have a recognized
> text extension. Either:
> - Rename `.blocks` → `.json` and update the code, OR
> - Create a custom ScriptedImporter for `.blocks` files, OR
> - Copy the files to `StreamingAssets/Blocks/` and load via `Application.streamingAssetsPath`

The simplest approach: copy `.blocks` files to `Assets/StreamingAssets/Blocks/` and update
`BlockFileIO.LoadFromResources()` to use `StreamingAssets` path instead.

### Block 3D Model
The block mesh at `Assets/Resources/block-beveled.fbx` is already accessible.

### Audio
Audio files at `Assets/Audio/` can stay where they are. Assign them to AudioSource components in the scene.

### Materials
The block material at `Assets/Resources/Materials/block.mat` is already accessible.

## Step 3: Create the Block Prefab

1. Create a new empty GameObject
2. Add components:
   - **MeshFilter** → assign `block-beveled` mesh
   - **MeshRenderer** → assign `block` material
   - **BoxCollider** (for mouse/XR raycasting)
   - **AudioSource** → assign `Click.wav`
   - **BlockController** (from `Assets/SteinerBlocks/Scripts/Game/`)
3. Save as prefab at `Assets/SteinerBlocks/Prefabs/Block.prefab`

## Step 4: Set Up the Scene

Create a new scene `Assets/Scenes/Main.unity` with this hierarchy:

```
Main Scene
│
├── GameManager                          [GameManager.cs, SelectionManager.cs]
│
├── Main Camera                          [Camera, AudioListener]
│   (Position: 0, 0.5, -1  |  Rotation: 15, 0, 0)
│
├── Directional Light
│
├── SlideshowGrid                        [BlockGridController.cs]
│   │  blockPrefab = Block.prefab
│   │  scaleFactor = 1.0
│   │  isEditable = false
│   └── Grid                             (empty child — blocks spawn here)
│
├── LocalGrid                            [BlockGridController.cs]
│   │  blockPrefab = Block.prefab
│   │  scaleFactor = 1.5
│   │  isEditable = true
│   └── Grid                             (empty child — blocks spawn here)
│
├── SelectionHighlight                   [SelectionHighlight.cs]
│   ├── FocusHighlight                   (4 arrow meshes, white material)
│   └── SelectionHighlight               (highlight frame mesh, yellow material)
│
├── DesktopInput                         [DesktopInputHandler.cs]
│
└── XRInput                              [XRInputHandler.cs]  (disabled for now)
```

### Wire up references in the Inspector:

**GameManager:**
- `slideshowGrid` → SlideshowGrid
- `localGrid` → LocalGrid
- `selectionManager` → GameManager (the SelectionManager component on same object)
- `selectionHighlight` → SelectionHighlight

**BlockGridController (on SlideshowGrid):**
- `blockPrefab` → Block.prefab
- `gridParent` → SlideshowGrid/Grid

**BlockGridController (on LocalGrid):**
- `blockPrefab` → Block.prefab
- `gridParent` → LocalGrid/Grid

**SelectionManager (on GameManager):**
- `ActiveGrid` → set at runtime (when showing local blocks)

**DesktopInputHandler:**
- `selectionManager` → GameManager's SelectionManager component
- `gameManager` → GameManager

## Step 5: Test in Editor

1. Press **Play**
2. The slideshow should start automatically after 3 seconds
3. After 30 seconds, local blocks appear
4. **Mouse hover** over blocks to see focus highlight
5. **Left click** to select a block (it pops up and scales)
6. **Arrow keys / WASD** to rotate the selected block
7. **R** to turn, **F** to flip
8. **Left click** or **Escape** to deselect
9. **1/2** to show/hide slideshow, **3/4** to show/hide local blocks
10. **Space** to pause/resume slideshow

## Step 6: WebGL Build

1. **File → Build Profiles → WebGL**
2. Set compression to Brotli
3. Disable the `XRInputHandler` component in the scene
4. Build and deploy to any static web server

## Phase 2: XR Setup (Future)

When ready to add XR support:

### Quest (OpenXR)
1. **Edit → Project Settings → XR Plug-in Management** → enable OpenXR
2. Add **Meta Quest Touch Pro** interaction profile
3. Add **XR Origin** to the scene (replaces Main Camera for XR)
4. Add XRRayInteractor to hand/controller objects
5. Add XRGrabInteractable to the block prefab
6. Enable and configure `XRInputHandler.cs`

### Apple Vision Pro (visionOS + PolySpatial)
1. Enable **PolySpatial** in Package Manager
2. Add **VisionOS Volume Camera** to the scene
3. Configure **PolySpatial XR** settings
4. Use gaze-and-pinch input via XRGazeInteractor

### Android XR
1. Enable **Android XR** build target
2. OpenXR configuration similar to Quest
3. Hand tracking enabled by default on Galaxy XR

## Script Migration Reference

| Old Script (Unity 5.5 + HoloToolkit) | New Script (Unity 6) | Key Changes |
|---------------------------------------|----------------------|-------------|
| `Globals.cs` | `GameManager.cs` | Removed WSA WorldAnchors, singleton pattern preserved |
| `BlockBehaviors.cs` | `BlockController.cs` | Removed IFocusable/IInputClickHandler, event-driven |
| `BlockIO.cs` | `BlockGridController.cs` + `BlockFileIO.cs` | Split into grid management and file I/O. Cross-platform file access |
| `BlockIO.BlockDataList` | `BlockGridData.cs` | JsonUtility instead of DataContractJsonSerializer |
| `TappedHandler.cs` | `DesktopInputHandler.cs` + `SelectionManager.cs` | Input separated from selection logic |
| `SelectionHighlightCommands.cs` | `SelectionHighlight.cs` | `FindChild()` → `Find()`, event-driven |
| `SpinBlock.cs` | `BlockSpinner.cs` | Minimal change, namespaced |
| `BlocksParentBehaviors.cs` | (Integrated into `BlockGridController`) | `SetVisible()` replaces `OnShow()`/`OnHide()` |
| `MenuInput.cs` | (Removed — was empty) | N/A |

## File Structure

```
Assets/
├── SteinerBlocks/
│   ├── Scripts/
│   │   ├── Core/
│   │   │   └── BlockGridData.cs         # Data model + JSON serialization
│   │   ├── Game/
│   │   │   ├── GameManager.cs           # App state, slideshow, singleton
│   │   │   ├── BlockController.cs       # Per-block behavior & animation
│   │   │   ├── BlockGridController.cs   # Grid creation & management
│   │   │   ├── SelectionManager.cs      # Focus/selection state machine
│   │   │   ├── SelectionHighlight.cs    # Visual highlight follower
│   │   │   └── BlockSpinner.cs          # Auto-rotate decoration
│   │   ├── Input/
│   │   │   ├── DesktopInputHandler.cs   # Mouse/keyboard (Editor + web)
│   │   │   └── XRInputHandler.cs        # XRI stub (Phase 2)
│   │   └── Persistence/
│   │       └── BlockFileIO.cs           # Cross-platform file I/O
│   └── Prefabs/
│       └── Block.prefab                 # (create in Editor)
├── Resources/
│   ├── Textures/                        # Existing block textures
│   ├── Materials/                       # Existing materials
│   └── block-beveled.fbx               # Existing block mesh
├── Audio/                               # Existing audio files
├── Scenes/
│   └── Main.unity                       # (create in Editor)
└── Packages/
    └── manifest.json                    # Unity 6 package dependencies
```
