# Scene Definition of Done Checklist

## Core Scene
- [ ] На сцене размещены 5 локаций: `Main Entrance`, `Laboratory`, `Warehouse`, `Control Center`, `Hangar`.
- [ ] Для каждой локации есть геометрия: пол, стены, декор, указатель.
- [ ] Объекты уровня сгруппированы по родителям и имеют понятные имена.

## Player
- [ ] У игрока есть `CharacterController` и `PlayerMovement`.
- [ ] `Main Camera` вложена в игрока и назначена в поле `cameraRoot`.
- [ ] Работают: WASD, Shift, Space, Mouse.

## UI
- [ ] Есть `MainMenuPanel` (название, Start, описание управления).
- [ ] Есть `HUDPanel` (текущая локация + подсказки).
- [ ] Есть `LocationPopup` с `CanvasGroup`.
- [ ] `MainMenu` и `UIManager` имеют все назначенные ссылки.

## Locations
- [ ] На каждой локации есть объект-триггер с `BoxCollider (Is Trigger)` и `LocationTrigger`.
- [ ] У игрока установлен tag `Player`.
- [ ] Вход в локацию показывает текст `Вы вошли в: [название]`.

## Lighting and Navigation
- [ ] Настроен `Directional Light`.
- [ ] Назначен Skybox в Lighting Settings.
- [ ] Проложен NavMesh и проверен проход NPC.
- [ ] На NPC добавлены `NavMeshAgent` и `NPCPatrol`.

