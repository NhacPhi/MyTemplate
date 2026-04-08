# Context Discovery Agent — Walkthrough

## What Was Done

Executed the **`@context-discovery-agent`** skill to scan the Unity project and produce a structured `ContextReport`.

## Discovery Results

| Property | Value |
|---|---|
| **Unity Version** | `2022.3.14f1` (LTS) |
| **Unity Major** | 2022 |
| **Render Pipeline** | **URP** 14.0.9 |
| **SRP Asset** | `Assets/Settings/UniversalRenderPipelineGlobalSettings.asset` |

### Package Status

| Package | Installed | Version |
|---|---|---|
| `com.unity.addressables` | ✅ | 1.21.21 |
| `com.unity.render-pipelines.universal` | ✅ | 14.0.9 |
| `com.unity.textmeshpro` | ✅ | 3.0.6 |
| `com.unity.timeline` | ✅ | 1.7.6 |
| `com.unity.nuget.newtonsoft-json` | ✅ | 3.2.1 |
| `com.cysharp.unitask` | ✅ | git |
| `jp.hadashikick.vcontainer` | ✅ | 1.16.8 |
| `com.unity.burst` | ❌ | — |
| `com.unity.collections` | ❌ | — |
| `com.unity.inputsystem` | ❌ | — |
| `com.unity.cinemachine` | ❌ | — |
| `com.unity.entities` | ❌ | — |
| `com.unity.netcode.gameobjects` | ❌ | — |

### Feature Flags

| Feature | Available |
|---|---|
| Awaitable API (Unity 6+) | ❌ |
| Burst Compiler | ❌ |
| Jobs System | ❌ |
| ECS (Entities) | ❌ |
| New Input System | ❌ |
| Addressables | ✅ |

### Third-Party Dependencies

| Library | Installed |
|---|---|
| UniTask | ✅ |
| VContainer (DI) | ✅ |
| Newtonsoft JSON | ✅ |

## Warnings

> [!WARNING]
> - **Unity 2022.3 LTS** — `Awaitable` API is NOT available (requires Unity 6+). Use `UniTask` instead.
> - **Burst not installed** — `@job-system-burst` skill cannot be used.
> - **Input System not installed** — Project uses legacy `UnityEngine.Input`. Do NOT generate `InputAction` code.
> - Several skills in the skill pack have `requirements.unity_version >= 6.0` — these need adjustment for this 2022.3 project.

## Output File

The full ContextReport JSON was saved to:

[context.json](file:///d:/UnityUdeme/MyTemplate/MyTemplate/.agent/context.json)

All other skills should read this file before generating code to ensure correct API usage.
