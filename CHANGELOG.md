# Changelog
## 0.4.4

### Added
NPC race spawning: Configurable Human, Elf, or Random natural spawns, plus biome-specific spawn commands.

### Fixed
Guard behavior: Stationary guards periodically face random directions without leaving their posts.
Food consumption: Hungry patrolling NPCs seek dropped food, consume it, and resume patrolling.

## 0.4.3

### Added
- Added naturally spawning Norsemen in the Deep North biome.
- Added `DeepNorth_Norseman_RS` for manual spawning.
- Added Deep North equipment sets featuring Ashlands-tier warrior and mage gear.
- Added automatic migration of missing biome equipment configurations into existing YAML files while preserving user customizations.

### Fixed
- Fixed patrol behavior conflicting with NPC job execution, causing NPCs to pace in one location.
- Fixed Norsemen failing to fire crossbows due to reload handling.

### Cleanup
- Removed obsolete resurrection debug logging.
- Removed temporary diagnostic logging.

## 0.4.2 

### Bug Fixes

- Fixed NPC not moving when set to patrol.
- Fixed NPC health regeneration. (Eating food still gives an instant health boost)

### Technical

- Adjusted call to update base.updateAI which controlled countdowns, which influences path finding for NPCs and health regeneration.

---
## 0.4.1 - Persistence Fixes

### Bug Fixes

- Fixed equipped armor and weapon appearances not persisting correctly after Norsemen unload and reload.
- Fixed resurrected Norsemen disappearing after leaving load distance and returning.
- Fixed resurrected Norsemen disappearing after logging out and reloading the world.

### Technical

- Added persistent ZDO handling for Norsemen to ensure resurrected NPCs remain saved in the world.
- Added persistent equipment visual state for equipped chest, leg, helmet, and hand items.

---

## 0.4.0 - Norsemen Continued

### Valheim 1.0 Compatibility

- Updated VikingNPC / Norsemen for compatibility with Valheim 1.0.
- Updated code affected by Valheim API changes.
- Updated equipment and item handling for the current Valheim version.
- Updated NPC interaction and behavior code where required for Valheim 1.0.
- Updated version checking to use the current plugin version.

### Compatibility

- Retained the original `RustyMods.Norsemen` plugin GUID for compatibility.
- Retained existing Norsemen prefab IDs.
- Retained the existing `RustyMods.Norsemen.cfg` configuration filename.
- Retained existing configuration directory and synchronization identifiers.
- Existing VikingNPC / Norsemen configurations should continue to be recognized.

### Project

- Renamed the maintained plugin to **Norsemen Continued**.
- Continued version numbering from the original VikingNPC / Norsemen project.
- Updated project metadata for continued maintenance by **SephrinMods**.
- Removed obsolete development/debugging code encountered during the Valheim 1.0 update.

### Credits

VikingNPC / Norsemen was originally created by **RustyMods**.

**Norsemen Continued** is maintained by **SephrinMods** and is published with the original author's permission.

---

## Previous Releases

Versions **0.3.4 and earlier** were released by RustyMods as part of the original VikingNPC / Norsemen project.

See the original VikingNPC project for the historical changelog.
