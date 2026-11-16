# Validation — prototype 0.9

## Executed in this environment
- **93 actual production-core checks compiled and executed with Roslyn / .NET 8.** Atomic file rotation/checksum, inventory, repair/crafting/needs, layout, independent ammunition for three firearms, single-shell loading, mission gating/idempotence, timed departure prerequisites, timer persistence, completion, save migration 1/2 → 3, ADS interpolation/accuracy/sensitivity/movement/reset rules and procedural PBR channel/periodicity/normal checks passed.
- The migration checks compile the **actual SaveData and SaveMigration sources**. A small Tools/CoreChecks-only Vector3 placeholder supplies position-field types. No Unity vector operations, JsonUtility serialization or engine behavior is emulated/verified by these checks.
- **92 C# files syntax-parsed** with tree-sitter, no parse errors. This is not a Unity API/type check or full Unity compilation.
- **Blender 4.3.2** generated/exported the six new models; reimport validators checked decreasing LOD triangle counts, mesh/material presence, unit orthogonal semantic axes and extents. StationZombie: 11-bone rig, armature modifiers, weighted vertex groups and separate baked Idle/Walk actions verified on reimport. The previous pistol/ammo/mutant also reimported successfully. The original FuelCan was upgraded to the shared three-LOD semantic-axis pipeline and is now used as the actual station pickup, preserving its save ID.
- Blender aiming previews used the exported pistol/shotgun/rifle meshes. The rear markers at eye relief .23 m and chosen front-sight top points projected to x=.5/y=.5 with positive depth for all three. This checks geometry/alignment in Blender, **not Unity runtime ADS transitions**.
- **21 RGBA PNG maps at 256²** exported by the compiled production SurfacePattern through the .NET tool. PNGs decoded/checked; procedural albedo/normal/metallic-smoothness data are original. Unity texture import/compression/lighting is not verified here.
- Blender generator Python syntax, localization JSON and asmdef JSON validated.
- Selected official URP 14 source signatures inspected: SetupRenderPasses, Dispose, RTHandle ReAllocateIfNeeded, Blit.hlsl and renderer feature data APIs. Source inspection is not C#/HLSL compilation.
- ZIP integrity and public GitHub-download SHA-256 checks performed as the last publication step; see the downloadable archive checksum manifest.

## Model batch: actual reimported triangle counts
| Model | LOD0 / LOD1 / LOD2 |
|---|---|
| PumpShotgun | 2568 / 356 / 216 |
| PlayerHands | 1672 / 376 / 112 |
| FieldRadio | 2364 / 300 / 216 |
| AutomaticRifle | 2904 / 316 / 252 |
| StationZombie | 5508 / 1572 / 540 |
| RailSignal | 1728 / 272 / 188 |
| FuelCan (revised) | 1032 / 124 / 76 |

## Provided but NOT run here
Unity EditMode tests include precise sight-pose position/orientation, scene reference validation, legacy/new save migration, train departure gating and earlier route/inventory/environment checks. PlayMode tests include wall occlusion, pause/resume, complete pellet damage and atomic pellet-pool reservation. BuildTools supplies scene generation/validation, Windows Development Build and optional station occlusion baking. These require an installed Unity editor and the stated packages.

## Material limitations
**No Unity installation is available.** Unity C# compilation, HLSL compilation, FBX/texture import in Unity, generated scene execution, aim transitions/weapon clipping/grips, camera stacking, menus, save/load integration/rollback, physics/collider cooking, AI/NavMesh activation, shadows/depth/sun shafts, occlusion bake, Windows build, full playthrough and CPU/GPU performance are not verified. No FPS improvement has been measured.

All Previews PNGs are Blender renders of prototype assets. Aim previews are illustrative of the chosen geometry, not gameplay screenshots or evidence of hit accuracy in Unity. Hands are static geometry following weapon pivots; zombie animation uses rigid segment weights. Materials, audio and lighting remain prototype art, not photorealistic final production work.

Stations are activation-culled, not unloaded; all authored station assets stay in memory. Pooled tree trunks/ruins are not dynamically baked into station NavMesh. Long-route floating origin, full skeletal weapon reload/hand animations, optical scopes, sight zeroing, final art/recorded audio and target-hardware optimization remain incomplete. See Docs/BUILD_AND_TEST_RU.md and Docs/ROADMAP_RU.md for the engine validation gates.
