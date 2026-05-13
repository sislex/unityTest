# Preset Layout (координаты)

Ниже готовая раскладка для быстрого старта. Эти значения уже встроены в `Tools/Research Complex/Build Location Layout`.

| Локация | Центр (X,Y,Z) | Размер оболочки (X,Y,Z) |
|---|---|---|
| Главный вход | `(0, 0, 0)` | `(22, 4, 14)` |
| Лаборатория | `(35, 0, 0)` | `(26, 4, 18)` |
| Склад | `(-35, 0, 0)` | `(30, 4, 18)` |
| Центр управления | `(0, 0, 32)` | `(24, 4, 18)` |
| Ангар | `(0, 0, -34)` | `(34, 5, 22)` |

## Что делает автоматический билдер
- Создает/обновляет корень `Gameplay` и 5 локаций.
- Для каждой локации создает `Floor`, `Shell`, `Props`, `Sign`, `LocationTrigger`.
- Скрывает рендер у `LocationTrigger`, но оставляет `BoxCollider (Is Trigger)`.
- Назначает `LocationTrigger` + `LocationZoneTemplate` с именем и подписью.
- Проверяет/добавляет `Directional Light` и `NavMeshSurface` на `Environment`.

## Как использовать
1. Откройте `Assets/Scenes/SampleScene.unity`.
2. В Unity выберите `Tools > Research Complex > Build Location Layout`.
3. Для NPC prefab выберите `Tools > Research Complex > Create NPC Prefab`.
4. Проверьте ссылки UI и объекта игрока (см. `Docs/SCENE_SETUP.md`).

