# Импорт мультяшных столов и стульев из библиотек

## Что уже сделано в коде
- Инструмент `Tools > Research Complex > Build University Classroom` теперь:
  - применяет более яркую мультяшную палитру;
  - добавляет потолочные лампы (`Point Light`);
  - ищет префабы столов/стульев в `Assets/Models` и `Assets/Prefabs`.
- Если префабы не найдены, использует встроенные fallback-модели из примитивов.

## Как скачать ассеты из Unity Asset Store
1. Откройте `Window > Package Manager`.
2. В левом выпадающем списке выберите `My Assets`.
3. Найдите и импортируйте пакеты со stylized furniture (desk/chair).
4. После импорта переместите нужные префабы в `Assets/Models/Classroom/`.

## Рекомендуемые имена префабов
- Для стола: содержит `desk` или `table`.
- Для стула: содержит `chair`.

Автопоиск использует ключевые слова в имени префаба, поэтому удобнее называть, например:
- `Stylized_Desk.prefab`
- `Stylized_Chair.prefab`

## Применение после импорта
1. Запустите `Tools > Research Complex > Build University Classroom`.
2. Проверьте в `Hierarchy > UniversityClassroom > Desks`, что вместо fallback-геометрии стоят импортированные префабы.
3. При необходимости подправьте масштабы в коде метода `CreateOrUpdatePrefabInstance(...)` в `Assets/Scripts/Editor/ResearchComplexSceneBuilder.cs`.

