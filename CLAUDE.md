# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a **subway/metro network simulation game** built in Unity. Players draw transit lines connecting stations, deploy trains, and manage passenger flow to prevent station overcrowding. The core fail condition is stations exceeding passenger capacity for 30 seconds (overflow timer).

**Unity Version**: 6000.3.11f1 (Unity 6)

## Development Setup

This is a standard Unity project — open in Unity Hub using Unity 6000.3.11f1. No separate build scripts exist; use the Unity Editor build pipeline directly.

**Code formatting**: The project uses `dotnet-csharpier` for C# formatting.

```powershell
dotnet csharpier .
```

**Testing**: Unity Test Framework (`com.unity.test-framework` 1.6.0) is included but no tests are written yet.

## Permissions & Restrictions

Per `.claude/settings.json`:
- Do **not** edit `.unity`, `.csproj`, `.sln`, or `package-lock.json` files
- Do **not** read or modify `Library/`, `Temp/`, or `Logs/` directories
- Do **not** run destructive `rm` commands

## Architecture

### Scene Flow

```
MainTitleScene → MenuScene → Scene (main gameplay)
```

Dev scenes (`Jina.unity`, `Jisu.unity`, `Sky.unity`) are per-developer sandboxes.

### Manager Hierarchy

All managers live in `Assets/Scripts/` and communicate via direct references (no DI container):

| Manager | Responsibility |
|---|---|
| `GameManager` | Central game state, game-over handling, UI coordination |
| `AssetManager` | Asset economy: day counter, weekly reward system, unlockable assets |
| `MouseInput` | Routes all input to the correct manager based on current `Mode` enum |
| `LineManager` | Line creation, editing, deletion |
| `TrainManager` | Train/carriage spawning and management |
| `StationManager` | Random station spawning over time |
| `PassengerManager` | Passenger destination assignment |

**Finding managers in scenes**: Managers are found by tag (e.g., `"GameManager"`, `"PassengerManager"`), not by type or reference. Tags are set in the Inspector.

### Input State Machine

`MouseInput.cs` is the central input router. Its `Mode` enum controls what happens on click/drag:

```
None | NewLine | ExtendLine | EditLine | NewTrain | HighTrain | InterchangeStation | Carriage
```

Mode transitions are triggered by UI buttons and passed through `MouseInput`.

### Station Overflow & Game Over

- Each station tracks a 30-second overflow timer (`StationTimerUI.cs`)
- `Station.OnTimeOver` is a **static event** — `GameManager` subscribes to it
- When fired, GameManager triggers the game-over sequence

### Train Pathfinding

`Train.cs` (~731 lines) contains all pathfinding and movement logic:

- **`BFSDistance()`** — finds shortest path distance to destination station across all connected lines
- **`BFS()`** — reachability check (used for transfer planning)
- **`CanBoard()`** — validates if a waiting passenger can board this train based on reachability
- **`FindTransferStation()`** — determines optimal transfer point for multi-line journeys

Circular vs. linear line topology changes train behavior: linear lines reverse direction at endpoints; circular lines circulate continuously.

### Carriage Following System

Trains record position history as they move. Carriages interpolate along this history to create a smooth "snake" following effect. This is why `Train.cs` maintains a position history list.

### Weekly Reward System

`AssetManager` tracks days elapsed. Every 7 days, gameplay halts and a random asset selection UI appears (handled in `GameManager`). Unlockable assets: interchange stations, additional trains, carriages, high-speed trains.

### Global Static State

- `Score` — static class, `Score.score` (int) tracks current score
- `Colors` — static class, array of 7 colors for the 7 maximum transit lines
- `AssetManager.dayCount` — static day counter

### Key Data Types

- `StationType` enum: `Circle`, `Square`, `Triangle` — determines which passengers can alight
- `PassengerState` enum: `Waiting` → `OnTrain` → `Arrived`
- `TrainDirection` enum: `Forward` / `Backward`

## Code Conventions

- **Language**: C# with Unity MonoBehaviour patterns
- **Comments and debug logs are written in Korean** (역=station, 열차=train, 승객=passenger, 환승=transfer)
- PascalCase for classes and public members; camelCase for private members
- Private field prefix is inconsistent (`_variable` and plain `camelCase` both appear)
- Tags used for cross-object identification — check `CompareTag()` calls before adding new tags
- `Resources.Load()` used for runtime sprite loading from `Assets/Resources/`

## Key Packages

- `com.unity.render-pipelines.universal` 17.3.0 — URP rendering pipeline
- `com.unity.inputsystem` 1.19.0 — New Input System (used in `MouseInput.cs`)
- `com.unity.2d.sprite` — 2D sprite support
- `com.unity.timeline` 1.8.11 — used for cutscenes/credit animations
