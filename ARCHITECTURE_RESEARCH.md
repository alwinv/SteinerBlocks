# Steiner Blocks — Cross-Platform Game Architecture Research

## Current State

The existing project is a **Unity/C# HoloLens app** built with:
- Unity + HoloToolkit (now deprecated, replaced by MRTK/XRI)
- Windows-specific APIs (`Windows.Storage`, `UnityEngine.VR.WSA`)
- JSON-based block data format (grid of rotation vectors representing block orientations)
- Grid sizes up to 50x20 blocks, each with Euler rotation data (`{"r":"0,90,180"}`)
- Voice commands, air-tap gestures, gaze-based selection
- Local file I/O for save/load, slideshow of preset patterns

The game concept — selecting, rotating, and arranging textured blocks on a grid to create patterns — is well-suited for both 2D (flat screen) and 3D (spatial) interaction.

---

## Target Platforms

| Platform | Display | Input | Browser/Runtime |
|----------|---------|-------|-----------------|
| **Web (desktop/mobile)** | 2D flat screen | Mouse/touch | Any modern browser |
| **Meta Quest 3/3S** | Stereoscopic 3D MR | Controllers + hand tracking | Quest Browser (Chromium) or native |
| **Apple Vision Pro** | Stereoscopic 3D MR | Gaze + pinch | Safari or native (visionOS) |
| **Android XR (Samsung Galaxy XR)** | Stereoscopic 3D MR | Hand tracking (primary) | Chrome or native |

---

## Approach Comparison

### Option A: Unity 6 (Recommended)

**Single engine targeting all platforms natively.**

| Target | Build Type | Status |
|--------|-----------|--------|
| Web (2D) | WebGL / Unity Web | Stable, ships with Unity |
| Web (XR) | WebGL + [WebXR Export plugin](https://github.com/De-Panther/unity-webxr-export) | Community-maintained, works on Quest/Android XR/desktop VR |
| Meta Quest | Native Android (OpenXR) | Mature, first-class support |
| Apple Vision Pro | Native visionOS (requires PolySpatial for immersive MR) | Supported in Unity 6 |
| Android XR | Native Android XR | GA in Unity 6 since Oct 2025 |

**Pros:**
- Existing project is already Unity/C# — significant code and asset reuse
- Single codebase, single editor for all platforms
- Unity 6 has first-class support for Quest, visionOS, and Android XR
- XR Interaction Toolkit (XRI) + XR Hands provides unified hand tracking across platforms
- AR Foundation provides cross-platform plane detection, passthrough, scene understanding
- Largest ecosystem of XR tools, tutorials, and community support
- Owlchemy Labs reported porting to Android XR "in about a week" using Unity 6
- PolySpatial enables visionOS Volumes and Spaces (unique to Apple's paradigm)

**Cons:**
- WebGL/Web builds have performance limitations (no threading, limited GPU)
- WebXR Export plugin is community-maintained (not official Unity)
- PolySpatial is an additional package with its own learning curve
- Unity licensing costs (free tier available for <$200K revenue)
- Web builds produce large download sizes compared to native web frameworks

**Architecture with Unity 6:**
```
┌─────────────────────────────────────────────────┐
│                  Unity 6 Project                │
│                                                 │
│  ┌───────────────────────────────────────────┐  │
│  │         Shared Game Logic (C#)            │  │
│  │  - Block grid model & rotation math       │  │
│  │  - Challenge generation & validation      │  │
│  │  - Score/stats tracking                   │  │
│  │  - JSON serialization (existing format)   │  │
│  └───────────────────────────────────────────┘  │
│                                                 │
│  ┌──────────┐ ┌──────────┐ ┌───────────────┐   │
│  │ 2D UI    │ │ 3D Scene │ │ XR Interaction│   │
│  │ (Canvas/ │ │ (blocks, │ │ (XRI + XR     │   │
│  │  UGUI)   │ │  camera) │ │  Hands)       │   │
│  └──────────┘ └──────────┘ └───────────────┘   │
│                                                 │
│  Build Targets:                                 │
│  ├── WebGL (2D web) ──────────── browsers       │
│  ├── WebGL + WebXR Export ────── Quest/AVP/AXR  │
│  ├── Android (Quest) ─────────── Meta Quest     │
│  ├── visionOS + PolySpatial ──── Vision Pro     │
│  └── Android XR ──────────────── Galaxy XR      │
└─────────────────────────────────────────────────┘
         │
         │  REST API (HTTPS/JSON)
         ▼
┌─────────────────────────────────┐
│     Backend Service (Cloud)     │
│  - Daily challenges             │
│  - Player profiles & stats      │
│  - Leaderboards                 │
└─────────────────────────────────┘
```

---

### Option B: WebXR (Three.js or Babylon.js)

**Single web codebase, runs in browsers on all platforms.**

| Target | How | Status |
|--------|-----|--------|
| Web (2D) | Standard web page | Works everywhere |
| Meta Quest | Quest Browser (WebXR) | Full VR + AR support |
| Apple Vision Pro | Safari (WebXR) | VR only (immersive-ar NOT supported) |
| Android XR | Chrome (WebXR) | Full VR + AR support |

**Pros:**
- True "write once, run everywhere" — single deployment via URL
- No app store approval, instant updates for all users
- TypeScript/JavaScript ecosystem (wider developer pool)
- Three.js and Babylon.js both have mature WebXR support
- Meta's Reality Accelerator Toolkit simplifies MR features with Three.js
- No licensing costs (all open source)
- Smallest barrier to entry for users (just open a link)

**Cons:**
- **No immersive-ar (passthrough MR) on Apple Vision Pro** — only immersive-vr works in Safari WebXR
- Performance ceiling lower than native (no foveated rendering, no Application SpaceWarp)
- No access to platform-specific features (visionOS Volumes/Spaces, Quest system keyboard, etc.)
- Requires rebuilding all game logic in JavaScript/TypeScript (no C# reuse)
- Hand tracking API differences across platforms require platform-specific code
- Limited offline capability compared to native apps

**Framework comparison:**

| Feature | Three.js | Babylon.js |
|---------|----------|------------|
| Community size | Larger (most popular web 3D lib) | Smaller but very active |
| WebXR support | Good (via WebXRManager) | Excellent (WebXR Experience Helper) |
| Physics | Via cannon.js/ammo.js | Built-in (Havok or Ammo) |
| GUI system | Via external libs | Built-in (Babylon GUI) |
| Learning curve | Lower | Moderate |
| TypeScript | Supported | Native TypeScript |
| Best for | Lightweight, custom experiences | Full-featured game development |

**For this project, Babylon.js would be the stronger choice** due to its built-in GUI system, native TypeScript support, and more complete WebXR integration including the Experience Helper.

---

### Option C: Godot 4.5+

**Open-source engine with growing XR support.**

| Target | How | Status |
|--------|-----|--------|
| Web (2D) | HTML5 export | Stable |
| Web (XR) | WebXR export | Supported (immersive-vr + immersive-ar) |
| Meta Quest | OpenXR (universal APK) | Mature |
| Apple Vision Pro | Native visionOS | Being built into engine (Apple contributing directly) |
| Android XR | OpenXR | Should work via universal OpenXR APK |

**Pros:**
- Completely free and open source (MIT license)
- Apple is directly contributing visionOS support to the engine
- Universal OpenXR APK works across headsets
- GDScript is approachable; C# also supported
- WebXR export supports both VR and AR modes
- Active, growing community

**Cons:**
- visionOS support is very new and still being built (immersive XR via plugin)
- Android XR specific testing/documentation is limited
- Smaller XR ecosystem compared to Unity (fewer tutorials, plugins, assets)
- No existing codebase to reuse (current project is Unity)
- WebXR examples and documentation have gaps (reported by users in 2025)
- Would require full rewrite of the existing game

---

### Option D: Hybrid (Web + Native)

**Web framework for 2D/WebXR + Unity for native XR apps.**

Use Babylon.js or Three.js for the web experience (both flat 2D and WebXR), and Unity for native Quest/visionOS/Android XR builds. Share game state through the backend API.

**Pros:**
- Best possible web experience (fast load, small bundle)
- Best possible native XR experience (full platform features)
- Each platform gets optimal treatment

**Cons:**
- Two separate codebases to maintain
- Game logic must be implemented twice (TypeScript + C#)
- Higher development cost and complexity
- Risk of feature drift between platforms

---

## Recommendation

### **Primary Recommendation: Option A — Unity 6**

Given that:
1. The existing project is already built in Unity with C# scripts and Unity assets
2. Unity 6 now has production-ready support for **all four target platforms** (Web, Quest, visionOS, Android XR)
3. The XR Interaction Toolkit provides a unified abstraction across input methods
4. The game's complexity (grid of rotating blocks) is well within WebGL performance limits
5. The JSON data format can be shared between all builds and the backend API

**This is the lowest-risk, highest-reuse path.**

### Migration path from current project:
1. Upgrade to Unity 6 (from the older Unity version with HoloToolkit)
2. Replace HoloToolkit with XR Interaction Toolkit (XRI) + XR Hands
3. Replace `UnityEngine.VR.WSA` APIs with AR Foundation equivalents
4. Replace `Windows.Storage` file I/O with `UnityEngine.Networking` (HTTP) for cloud save/load
5. Create a 2D UI mode (Canvas/UGUI) for the web build alongside the existing 3D mode
6. Add WebXR Export package for browser-based XR

### If web performance or distribution is the top priority:
Consider **Option B (Babylon.js)** for the web version, and keep Unity for native XR. This is the hybrid approach (Option D) but driven by web-first priorities. The block rotation game is simple enough that maintaining two implementations is feasible.

---

## Backend Service Architecture

### Recommended: Serverless (AWS or Firebase)

A daily challenges game with profile stats is an ideal fit for serverless:
- **Spiky usage patterns** — traffic spikes when daily challenge drops, quiet overnight
- **Simple data model** — challenges, player profiles, stats, leaderboards
- **Low operational overhead** — no servers to manage
- **Pay-per-use pricing** — cost-effective for indie/small-scale games

### Option 1: AWS Serverless (Most Flexible)

```
┌──────────────┐     ┌───────────────┐     ┌──────────────┐
│  API Gateway  │────▶│  Lambda       │────▶│  DynamoDB    │
│  (REST/HTTP)  │     │  Functions    │     │  (NoSQL)     │
└──────────────┘     └───────────────┘     └──────────────┘
                            │
                     ┌──────┴──────┐
                     │  CloudWatch │
                     │  (Monitoring)│
                     └─────────────┘
```

- **API Gateway**: REST endpoints for challenges, profiles, stats
- **Lambda**: Node.js/Python functions for business logic
- **DynamoDB**: Player profiles, daily challenge definitions, stats/scores
- **EventBridge**: Scheduled daily challenge generation (cron)
- **S3**: Static asset storage (block textures, challenge images)

### Option 2: Firebase (Simplest to Start)

```
┌──────────────┐     ┌───────────────┐     ┌──────────────┐
│  Firebase     │────▶│  Cloud        │────▶│  Firestore   │
│  Hosting      │     │  Functions    │     │  (NoSQL)     │
└──────────────┘     └───────────────┘     └──────────────┘
                                                  │
                                           ┌──────┴──────┐
                                           │  Firebase   │
                                           │  Auth       │
                                           └─────────────┘
```

- **Firestore**: Real-time database for challenges and profiles
- **Cloud Functions**: Challenge generation, stat computation
- **Firebase Auth**: Google/Apple/anonymous sign-in (works well with Unity's Firebase SDK)
- **Cloud Scheduler**: Daily challenge generation trigger

### Option 3: Supabase (Open-Source Alternative)

- PostgreSQL-based (relational, with good JSON support)
- Built-in Auth, real-time subscriptions, edge functions
- Self-hostable if needed
- Good REST API auto-generated from database schema

### Recommended API Endpoints

```
GET  /api/challenges/daily          → Today's challenge
GET  /api/challenges/daily/{date}   → Challenge for specific date
POST /api/challenges/daily/submit   → Submit solution for scoring

GET  /api/profiles/{userId}         → Player profile & lifetime stats
PUT  /api/profiles/{userId}         → Update profile
GET  /api/profiles/{userId}/stats   → Detailed play statistics

GET  /api/leaderboard/daily         → Today's leaderboard
GET  /api/leaderboard/alltime       → All-time leaderboard
```

### Daily Challenge Data Model

The existing `.blocks` JSON format can be extended for challenges:

```json
{
  "challengeId": "2026-02-23",
  "type": "match-pattern",
  "targetPattern": {
    "BlockDataArray": [{"r": "0,90,0"}, {"r": "0,0,180"}, ...],
    "Name": "daily-2026-02-23",
    "Size": "10,10"
  },
  "startingState": {
    "BlockDataArray": [{"r": "0,0,0"}, {"r": "0,0,0"}, ...],
    "Name": "start",
    "Size": "10,10"
  },
  "maxMoves": 25,
  "timeLimit": 300,
  "difficulty": "medium"
}
```

### Backend Recommendation

**Start with Firebase** for fastest time-to-market:
- Firebase SDK integrates directly with Unity (official plugin)
- Firebase Auth handles cross-platform sign-in seamlessly
- Firestore's real-time sync means leaderboards update live
- Cloud Functions + Cloud Scheduler handle daily challenge generation
- Free tier is generous enough for development and early users

**Migrate to AWS** if you need more control, custom infrastructure, or scale beyond Firebase's pricing model.

---

## Development Roadmap (Suggested Phases)

### Phase 1: Core Game Modernization
- Upgrade project to Unity 6
- Replace HoloToolkit → XR Interaction Toolkit
- Replace Windows-specific APIs → cross-platform alternatives
- Get the block grid + rotation mechanics working in Unity Editor

### Phase 2: Multi-Platform Builds
- WebGL build (2D web version)
- Quest build (native OpenXR)
- Test WebXR Export for browser-based XR

### Phase 3: Backend Service
- Set up Firebase (Auth + Firestore + Functions)
- Implement daily challenge generation
- Implement profile/stats saving
- Integrate Unity game with backend API

### Phase 4: Additional Platforms
- visionOS build (with PolySpatial for spatial features)
- Android XR build
- Platform-specific input tuning (gaze+pinch for AVP, hand-first for AXR)

### Phase 5: Polish & Launch
- Daily challenge content pipeline
- Leaderboards
- Social features (share patterns)
- App store submissions (Meta, Apple, Google)

---

## Key References

### WebXR Platform Support
- [WebXR on Android XR — Android Developers](https://developer.android.com/develop/xr/web)
- [visionOS 2 Enables WebXR by Default — Road to VR](https://www.roadtovr.com/visionos-2-webxr-support-vision-pro/)
- [WebXR Device API — Can I Use](https://caniuse.com/webxr)
- [WebXR + Vision Pro Cross-Compatibility — UploadVR](https://www.uploadvr.com/webxr-vision-pro-cross-compatible-quest/)

### Unity XR
- [Unity 6 Android XR Support — Unity News](https://unity.com/news/unity-powers-launch-titles-for-samsung-galaxy-xr-and-accelerates-the-android-xr-ecosystem-)
- [Getting Started with Unity and Android XR — Android Developers Blog](https://android-developers.googleblog.com/2025/10/getting-started-with-unity-and-android.html)
- [Unity visionOS Documentation](https://docs.unity3d.com/6000.3/Documentation/Manual/visionOS.html)
- [Unity WebXR Export Plugin — GitHub](https://github.com/De-Panther/unity-webxr-export)

### Godot XR
- [Godot XR Update Feb 2025](https://godotengine.org/article/godot-xr-update-feb-2025/)
- [GodotVision (visionOS)](https://godot.vision/)

### Web Frameworks
- [Babylon.js WebXR Documentation](https://doc.babylonjs.com/features/featuresDeepDive/webXR/introToWebXR)
- [Three.js WebXR Resources](https://threejsresources.com/vr/blog/best-vr-headsets-with-webxr-support-for-three-js-developers-2026)
- [Meta Reality Accelerator Toolkit for Three.js](https://immersiveweb.dev/)

### Backend
- [AWS Serverless Game Backend Architecture](https://docs.aws.amazon.com/wellarchitected/latest/games-industry-lens/serverless-backend.html)
- [Serverless Best Practices for APIs 2025](https://devtechtoday.com/navigating-the-future-of-api-development-best-practices-for-serverless-frameworks-in-2025)
