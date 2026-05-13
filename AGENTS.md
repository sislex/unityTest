# Руководство для AGENTS

## Текущее состояние проекта (важно)
- Это свежий шаблон проекта Unity 6 URP, а не готовая игровая логика.
- Версия Unity зафиксирована в `ProjectSettings/ProjectVersion.txt` (`6000.4.6f1`); используйте ту же версию редактора, чтобы избежать лишних изменений сериализации ассетов.
- В build settings сейчас только одна сцена: `Assets/Scenes/SampleScene.unity`.
- Runtime C#-скриптов под `Assets` пока нет (есть только шаблонные readme/editor-скрипты в `Assets/TutorialInfo/Scripts`).

## Архитектура и поток конфигурации
- Рендер-пайплайн: Universal RP, назначен глобально в `ProjectSettings/GraphicsSettings.asset` через `m_CustomRenderPipeline`.
- Quality-уровни разделяют pipeline-ассеты по платформам в `ProjectSettings/QualitySettings.asset`:
  - `Mobile` -> `Assets/Settings/Mobile_RPAsset.asset`
  - `PC` -> `Assets/Settings/PC_RPAsset.asset`
- Постобработка на уровне сцены: объект `Global Volume` в `Assets/Scenes/SampleScene.unity`, профиль `Assets/Settings/SampleSceneProfile.asset`.
- Input System включен через `ProjectSettings/EditorBuildSettings.asset` (`m_configObjects`), где привязан `Assets/InputSystem_Actions.inputactions`.
- В `Assets/InputSystem_Actions.inputactions` есть карты `Player` и `UI` с биндингами для Keyboard&Mouse, Gamepad, Touch, Joystick и XR.

## Зависимости и точки интеграции
- Пакеты управляются через UPM в `Packages/manifest.json`.
- Ключевые уже подключенные пакеты: `com.unity.inputsystem`, `com.unity.render-pipelines.universal`, `com.unity.ai.navigation`, `com.unity.test-framework`, `com.unity.visualscripting`.
- Фактическая фиксация версий и транзитивных зависимостей — `Packages/packages-lock.json`; не редактируйте вручную без явной задачи по пиннингу/обновлению.

## Наблюдаемые соглашения по коду и ассетам
- Unity YAML-ассеты (сцены, quality, render assets) лучше править через Unity Editor, а не вручную в тексте.
- Для каждого добавления/перемещения/удаления ассета сохраняйте `.meta`, иначе ломаются GUID-ссылки.
- Не изменяйте локально-генерируемые каталоги: `Library/`, `Temp/`, `Logs/`, `UserSettings/`.
- Editor-only код хранится в `Assets/.../Editor/` (пример: `Assets/TutorialInfo/Scripts/Editor/ReadmeEditor.cs`).

## Практические workflow для агентов
- Открывайте в Unity корень проекта: `/Users/alexeyrozhnov/unity/unityTest`.
- При добавлении runtime-кода сначала создайте явную папку (например, `Assets/Scripts/`), а editor-инструменты держите в подпапке `Editor/`.
- При добавлении тестов создайте сборки Unity Test Framework для EditMode/PlayMode (сейчас тестовых сборок нет).
- Шаблон headless-проверки (путь к Unity подставляется локально):
  - `"<UNITY_EDITOR>/Unity" -batchmode -projectPath "/Users/alexeyrozhnov/unity/unityTest" -quit -logFile -`
- Шаблон запуска тестов (после добавления тестов):
  - `"<UNITY_EDITOR>/Unity" -batchmode -projectPath "/Users/alexeyrozhnov/unity/unityTest" -runTests -testPlatform EditMode -quit -logFile - -testResults "./TestResults.xml"`

## Результат сканирования AI-инструкций
- Поиск выполнен одним glob-запросом по `.github/copilot-instructions.md`, `AGENT.md`, `AGENTS.md`, `CLAUDE.md`, правилам cursor/windsurf/cline и `README.md`.
- На момент сканирования существующие AI-instruction файлы в репозитории не найдены.


