# Validation — prototype 0.6

## Executed
- **20 core checks compiled and executed with C# Roslyn / .NET 8.** Sources under test are the actual AtomicFileStore and InventoryModel used by the game, plus InventoryState and WeaponSlot. Checks cover UTF-8 writes, repeated replacement and backups, temporary-file cleanup, SHA-256 known vector, finite/range checks, resource transactions, negative values, weapon ownership, magazine exhaustion, reload conservation, inventory round trip, rejection without partial mutation, capacity and medkits.
- **Blender 4.3.2:** all three new generators ran and exported FBX/BLEND. FBX was reimported; LOD mesh counts/triangles and unit markers checked. Mutant skin bindings, 11 bones and exported Idle/Walk actions checked.
- C# syntax parsed with tree-sitter (53 source files including the standalone check program). This is not Unity API compilation.
- Menu dictionaries checked for matching keys and valid JSON. Assembly-definition JSON checked.
- ZIP integrity and published download hashes checked during packaging.

## Provided, not executed here
- Unity EditMode tests for route, stops, repairs, healing, resources, ownership, restored crate/inventory state, reload and localization catalogs.
- Unity PlayMode tests for wall occlusion of bullet damage and projectile suspension during pause.

## Not verified
Unity is not installed in this environment. No Unity compilation, Editor scene generation, NavMesh build, AI/animation/audio integration, settings on actual graphics hardware, complete session round trip, Windows build or performance measurement was possible. Standalone core checks and Blender renders do not establish those behaviors. In particular, verify the Generic animation/avatar import and the carving of destroyed door barriers in Unity 2022.3 before expanding content.
