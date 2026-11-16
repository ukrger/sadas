# Validation — prototype 0.8

## Executed
- **46 actual production-core checks compiled and executed with C# Roslyn / .NET 8.** Previous file/inventory/crafting/needs checks plus EnvironmentLayout: 7-tile normal window, stranded-player coverage, overlapping windows, fixed capacity and unique indices across teleports, deterministic placement, station/ruin/track clearances and ruin orientation. A failing tree-vs-ruin clearance check exposed a placement defect; the rule was corrected and all checks passed.
- **Blender 4.3.2:** birch, pine and overgrown house generated, exported and reimported. Checked mesh/triangle counts for all three LODs, unit axis markers, height and vertex wind masks. House doorway and missing roof quadrant checked with geometry rays.
- All 69 C# files syntax-parsed with tree-sitter. Python generators syntax-checked. Menu/assembly JSON validated.
- URP 14 shader-source signatures inspected for UniversalFragmentPBR, InputData, GetShadowCoord and ApplyShadowBias. This is an API/source inspection, not shader compilation.
- Blender assembly preview generated from exported meshes. Package/download hashes checked at publication.

## Provided, not executed here
- Unity tests from earlier releases, plus route projection, track normals/bounds, transformed vegetation bounds and ShaderUtil.ShaderHasError for the new foliage shader.

## Not verified
No Unity installation is available. C# Unity compilation, HLSL compilation, scene generation, native mesh-collider cooking, GPU instancing, shader lighting/shadows/depth, actual runtime tile retention, save/load integration, NavMesh behavior, Windows builds and performance have not been run. The source reduces renderer/collider component counts, but this does not establish an FPS improvement. Use Frame Debugger, CPU/GPU Profiler and a target-PC Development Build.

SummerRouteEnvironment.png and individual model previews are Blender renders, not Unity screenshots. The layout preview is illustrative; it does not show runtime grass, wind, camera stacking or performance. Vegetation is a technical geometry/material pass, not final photorealistic art.
