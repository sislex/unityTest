# Unity Sci-Fi Research Complex (Учебный прототип)

Учебная сцена Unity 3D с FPS-персонажем, UI-меню, триггерами локаций и базовой подготовкой под NPC на NavMesh.

## Что уже добавлено
- Runtime-скрипты в `Assets/Scripts/`.
- Папки структуры: `Assets/Scripts`, `Assets/Scenes`, `Assets/Prefabs`, `Assets/Materials`, `Assets/UI`, `Assets/Models`.
- Практическая документация:
  - `Docs/SCENE_SETUP.md`
  - `Docs/SCENE_DOD_CHECKLIST.md`

## Скрипты
- `PlayerMovement.cs`
- `LocationTrigger.cs`
- `MainMenu.cs`
- `UIManager.cs`
- `NPCPatrol.cs`
- `LocationZoneTemplate.cs`

## Быстрый запуск
1. Откройте `Assets/Scenes/SampleScene.unity`.
2. Либо соберите геометрию вручную по `Docs/SCENE_SETUP.md`, либо запустите авто-билдер:
   - `Tools > Research Complex > Build Location Layout`
   - `Tools > Research Complex > Create NPC Prefab`
3. Запустите Play и проверьте чеклист из `Docs/SCENE_DOD_CHECKLIST.md`.

## Layout preset
- Координаты и размеры 5 локаций: `Docs/LOCATION_LAYOUT_PRESET.md`.

## Примечание
Текущий проект ориентирован на Unity `6000.4.6f1` (см. `ProjectSettings/ProjectVersion.txt`).


