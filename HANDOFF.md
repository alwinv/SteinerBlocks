# Session Handoff — Rhythm Wheel App

## What Was Built

A single-file web app: `rhythm-wheel.html`

**Concept:** An ancient-artifact-themed interactive rhythm sequencer styled as a rotating bronze percussion wheel from 3rd century BCE Mesopotamia.

### Features
- **Canvas-rendered wheel** with a stone ring, rusted iron arm sweeping clockwise, and placeable bronze bells
- **4 pitch tones** (I–IV): frequencies 390, 585, 878, 1317 Hz — rendered as metallic cowbells using Web Audio API (two detuned square oscillators through bandpass + highpass filters)
- **Divisions:** 8 / 16 / 32 steps (buttons labeled VIII / XVI / XXXII)
- **Tempo:** BPM slider (40–220), default 90
- **Interaction:**
  - Left-click on ring → place bell at nearest step (using selected tone)
  - Left-click existing bell → remove it
  - Right-click existing bell → cycle its pitch
  - "Clear" button removes all bells
- **Visual details:** worn stone texture, Islamic geometric interlacing on the ring, 8-pointed star at center, ancient crack lines, aged patina/verdigris on bells, rust flecks on arm, animated glow on bell strike

### Aesthetic / Design Language
- Dark warm background (`#0d0902`), gold/amber palette
- Font: Cinzel (Google Fonts) — classical Roman lettering
- Subtitle in Arabic: *طارة الإيقاع* ("rhythm wheel")
- Flavor text placard at bottom: "Artifact No. 1847-C · Circa 3rd Century BCE · Mesopotamia"
- Everything fits in one self-contained HTML file (no build step, no dependencies beyond Google Fonts + Web Audio)

---

## Repository Context

This file was built in the **SteinerBlocks** repo (`alwinv/SteinerBlocks`) as a standalone experiment, then the decision was made to move it to **Artifacts-Playground** (`alwinv/Artifacts-Playground`) — a repo dedicated to single-file interactive web artifacts.

The file to copy over is: `rhythm-wheel.html` (root of SteinerBlocks repo).

---

## Continuing in the New Session

1. Copy `rhythm-wheel.html` into the Artifacts-Playground repo
2. Commit it on the appropriate `claude/` branch
3. Any further feature work should continue from there

### Potential Next Steps (if the user wants them)
- Save/load patterns via URL hash or localStorage
- Additional tones / pitch sets
- Variable bell size/volume per step (velocity)
- Mobile touch support improvements
- Additional rhythm presets (pre-loaded patterns)
- Export as audio

---

## Files of Note in SteinerBlocks (for context only)
- `ARCHITECTURE_RESEARCH.md` — research on porting the original Unity/HoloLens block-arrangement game to web + XR platforms (Quest 3, Vision Pro, Android XR). Not directly related to the rhythm wheel.
- `rhythm-wheel.html` — the artifact to migrate
