# Godot port 0.1 — executed validation

Engine: Godot **4.6.3 stable official 7d41c59c4**, Linux. Model source/export: Blender 4.3.2. This report distinguishes executed checks from pending release criteria.

## Passed in the actual engine
- **73 main checks** run against production GDScript and the instantiated main scene, not a reimplementation. Inventory/crafting/ammo conservation, ownership/capacities, deterministic bounded chunk window, ADS transition rules, actual scene setup, four stations/eight enemies/ten chunks, GLB LOD0 instantiation, physics support and movement, save/load/checksum/backup and invalid-state rejection passed.
- A longer physics test initially exposed drift on the moving railcar. The final local-deck attachment plus swept horizontal collision kept the passenger's local horizontal position within 0.05 m over six seconds through acceleration/curve at cruise speed. The measurement is in validation/MainChecks.txt. Train jump inherited velocity, landed back on the moving train, and a 0.30 m step was crossed without jumping.
- Real pooled projectiles: eight pellets applied the expected damage to a board; a newly created wall blocked a bullet. NavMesh was actually generated. The full enemy AI encounter/door chase is still a manual acceptance task.
- Simulated RMB input entered ADS, narrowed FOV and aligned each pistol/shotgun/rifle marker with the camera axis. Releasing aim and reloading restored/blocked ADS as expected. A save after a large train teleport restored the passenger without a launch impulse.
- The inventory uses a distinct SubViewport/World3D. A real physics ray through its camera selected a 3D bottle and consumed the actual inventory item, restoring thirst. Opening the bag did not pause the world; closing reset FOV. Pause froze train simulation.
- **6 additional long-route checks:** separate profile/station distances, distant station NavMesh, production train-tick integration to the first 2800 m stop at roughly six simulated minutes, fuel/wear consumption, relay departure gate and valid departure/save contract. This is schedule simulation, **not a real-time full playthrough**.
- Final main checks completed without SCRIPT ERROR, ERROR, WARNING or failed assertions. Audio initialization is intentionally skipped in headless mode.

## Rendering executed
- Main scene and diagnostic capture ran with **Vulkan 1.4.305 / Forward+**, using **Mesa llvmpipe (LLVM 19.1.7, 256 bits)** software rendering. Medium day/overview/train view, actual RMB aim, 3D inventory and High night were captured. High enabled SSAO and volumetric fog. The capture script completed; final capture log contains no shader/runtime errors.
- An earlier run fell back from Chromium SwiftShader to OpenGL Compatibility; that run exposed reversed terrain faces. Winding/normals were corrected. Final previews were replaced by the Forward+ captures.
- These are **actual Godot images**, not Blender/concept images. The overview uses a diagnostic camera position. The capture tool grants inventory for visualization; it does not prove all loot was collected through normal play.
- The renderer is software, not a representative gaming GPU. **No target-PC FPS, 1% low, VRAM budget or performance improvement is established.** A displayed FPS counter/headless --fixed-fps is not a benchmark.

## Asset/package checks
- 23 original BLEND files exported to GLB. GLB magic/version/length, manifest sizes and nonempty source LOD0/1/2 meshes verified.
- Godot imported all model GLBs, procedural PNGs, WAVs, shaders and scripts. Import settings enable generated mesh LOD and shadow meshes. Native scene tests instantiated the actual imported resources.
- ZIP integrity and public GitHub-download SHA-256 checked at publication. .godot cache, local user saves, test-driver binaries and development dependencies are excluded from the package.

## Still required
Windows standalone export/run, full manual short/long playthrough, all combat/navigation/doorway cases, moving-train walking near every obstacle, multiplayer is not implemented, window resizing/DPI/fullscreen matrix, audio playback quality on a real audio device, controller support, shader appearance/driver compatibility across target GPUs, extended save corruption/failure/rollback testing and representative hardware profiling.

Art is prototype quality: static first-person hands, simple rigid-segment enemy animation, repeated stations/terrain and procedural materials. No optical scope/zeroing, drag-and-drop bag, final reload animations, ragdolls, final recorded sound, floating origin or additive asset unloading. The corridor has limited transverse physical coverage. Code/features and rendered previews do not establish final UE4-level visuals or a completed commercial game. See README_RU.md and docs/PERFORMANCE_RU.md.

Reproduce:

    godot --headless --editor --path . --import
    godot --headless --path . --fixed-fps 60 --script res://tests/run_checks.gd
    godot --headless --path . --fixed-fps 60 --script res://tests/long_route_check.gd -- --long-route

On a graphics-capable machine:

    godot --path . --script res://tools/capture.gd


## Windows export follow-up
Official Godot 4.6.3 Windows x86_64 release templates were downloaded from the official Godot release archive; ZIP entry CRCs/version and template SHA-256 recorded during extraction. Windows Desktop release export succeeded without errors/warnings. TrainSummer.exe is a PE32+ x86_64 GUI executable with embedded GDPC payload; no external PCK is required.

The embedded payload in the actual exported EXE booted with Linux Godot, then passed all 73 main + 6 long-route checks using external test scripts against the exported production scenes/bytecode/assets. This verifies packaging and runtime data, **not native execution of the PE binary on Windows**. No Windows/Wine environment is installed. Windows graphics/audio/hardware performance remain unverified. The ZIP includes a Compatibility launcher and engine/third-party license notices.
