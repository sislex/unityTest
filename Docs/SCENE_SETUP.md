# Sci-Fi Research Complex: Practical Setup

Этот документ дополняет скрипты в `Assets/Scripts` и помогает собрать рабочую сцену.

## 1) Готовые скрипты
- `Assets/Scripts/PlayerMovement.cs` — FPS движение, бег, прыжок, обзор мышью, плавное движение камеры.
- `Assets/Scripts/LocationTrigger.cs` — триггер входа в локацию, сообщение UI.
- `Assets/Scripts/MainMenu.cs` — стартовое меню, запуск игры, переключение UI.
- `Assets/Scripts/UIManager.cs` — HUD и popup "Вы вошли в: ...".
- `Assets/Scripts/NPCPatrol.cs` — патруль NPC на `NavMeshAgent`.
- `Assets/Scripts/LocationZoneTemplate.cs` — автопривязка данных локации в триггер и подпись.

## 2) Минимальный иерархический шаблон
- `Environment`
- `Player`
- `Gameplay`
  - `MainEntrance`
  - `Laboratory`
  - `Warehouse`
  - `ControlCenter`
  - `Hangar`
- `UI_Root`
- `GameManager`

## 3) Как сделать prefab локации
1. Создайте пустой объект `LocationZone_Template`.
2. Внутри создайте:
   - `Geo` (стены/пол)
   - `Sign` (TextMeshPro 3D)
   - `Trigger` (Cube с `BoxCollider (Is Trigger)` + `LocationTrigger`)
3. На корень добавьте `LocationZoneTemplate`.
4. Привяжите в `LocationZoneTemplate`:
   - `Location Trigger` -> `Trigger`
   - `World Sign Text` -> TMP Text из `Sign`
5. Укажите `Location Name` и `Location Subtitle`.
6. Перетащите объект в `Assets/Prefabs/` чтобы создать prefab.
7. Разместите 5 копий prefab в сцене и задайте им уникальные имена зон.

## 4) NPC подготовка
1. Создайте NPC объект (например Capsule + материал).
2. Добавьте `NavMeshAgent` и `NPCPatrol`.
3. Создайте дочерний объект `Waypoints` и 3-6 пустых точек.
4. Заполните массив `Waypoints` в `NPCPatrol`.
5. После Bake NavMesh NPC начнет патруль.

## 5) Definition of Done
- Меню появляется при старте, кнопка Start запускает игру.
- Игрок управляется: WASD, Shift, Space, Mouse.
- HUD показывает подсказки и текущую локацию.
- В каждой из 5 локаций срабатывает popup "Вы вошли в: ...".
- На сцене есть `Directional Light`, Skybox, и baked NavMesh.
- В сцене хотя бы один NPC с `NavMeshAgent` и `NPCPatrol`.

