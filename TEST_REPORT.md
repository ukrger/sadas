# Validation — prototype 0.7

## Executed
- **36 checks compiled and executed with C# Roslyn / .NET 8.** Actual production sources: AtomicFileStore, InventoryModel, NeedsModel, InventoryState, WeaponSlot, CraftingRules. Checks cover atomic writes/backups/checksums, resource/ammo transactions, capacity, restore validation, both crafting recipes, failed crafts without spending, new supplies, needs restoration, pause, saturation, starvation timing and frame subdivision.
- **Blender 4.3.2:** water bottle, canned food and repair kit generated/exported and reimported from FBX. Checked LOD mesh/triangle counts, unit markers, height and upper surfaces. Inventory layout rendered from exported models as an illustrative Blender preview.
- All 63 C# source files syntax-parsed with tree-sitter. This does not compile Unity APIs.
- URP 14 source inspected for camera depth and shadow-resolution APIs. Removed assignment to read-only clearDepth; replaced the inaccessible runtime shadow-resolution setter with Editor-generated pipeline presets and runtime copies.
- Menu dictionaries and assembly-definition JSON validated. Package integrity/download hashes checked when publishing.

## Provided, not executed here
- Unity EditMode tests from prior versions, plus crafting, repair-kit use and schema-1 migration preservation/rejection cases.
- Existing Unity PlayMode projectile/pause tests.

## Not verified
Unity is absent. No Unity compilation, scene generation, camera-stack execution, pointer picking, train/platform interaction, runtime migration, complete save round trip, AI/animation/audio integration, Windows build or FPS measurement was possible. In particular, verify Forward renderer camera stacking, cursor handling across pause/resume, save/load while inventory is open, and migration of a real 0.6 save before relying on the feature.

InventoryClothLayout.png is a Blender assembly preview, not an in-game screenshot. The current physical inventory uses fixed category slots and actual meshes; drag-and-drop, hands and cloth physics are not implemented.
