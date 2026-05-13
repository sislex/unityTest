using System.Collections.Generic;
using TMPro;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public static class ResearchComplexSceneBuilder
{
    private const string DeskPrefabGuidKey = "ResearchComplex.DeskPrefabGuid";
    private const string ChairPrefabGuidKey = "ResearchComplex.ChairPrefabGuid";
    private const string DefaultDeskPrefabPath = "Assets/Corkboard_Desk/Prefab/Desk.prefab";
    private const string DefaultChairPrefabPath = "Assets/PBR Folding Chairs/chFolding.A2.prefab";

    private struct LocationPreset
    {
        public string Name;
        public string Subtitle;
        public Vector3 Center;
        public Vector3 Size;
        public Color Color;

        public LocationPreset(string name, string subtitle, Vector3 center, Vector3 size, Color color)
        {
            Name = name;
            Subtitle = subtitle;
            Center = center;
            Size = size;
            Color = color;
        }
    }

    private static readonly LocationPreset[] Presets =
    {
        new LocationPreset("Главный вход", "Точка доступа персонала", new Vector3(0f, 0f, 0f), new Vector3(22f, 4f, 14f), new Color(0.2f, 0.6f, 1f)),
        new LocationPreset("Лаборатория", "Сектор научных экспериментов", new Vector3(35f, 0f, 0f), new Vector3(26f, 4f, 18f), new Color(0.7f, 0.9f, 1f)),
        new LocationPreset("Склад", "Хранение материалов и оборудования", new Vector3(-35f, 0f, 0f), new Vector3(30f, 4f, 18f), new Color(0.9f, 0.8f, 0.5f)),
        new LocationPreset("Центр управления", "Мониторинг систем комплекса", new Vector3(0f, 0f, 32f), new Vector3(24f, 4f, 18f), new Color(0.4f, 1f, 0.8f)),
        new LocationPreset("Ангар", "Подготовка транспорта и дронов", new Vector3(0f, 0f, -34f), new Vector3(34f, 5f, 22f), new Color(0.6f, 0.7f, 1f))
    };

    [MenuItem("Tools/Research Complex/Build Location Layout")]
    public static void BuildLocationLayout()
    {
        Transform gameplayRoot = GetOrCreateRoot("Gameplay");

        foreach (LocationPreset preset in Presets)
        {
            BuildOrUpdateLocation(gameplayRoot, preset);
        }

        BuildOrUpdatePerimeterWalls(gameplayRoot);

        CreateOrUpdateDirectionalLight();
        CreateOrUpdateNavMeshSurface();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[ResearchComplexSceneBuilder] Layout built/updated successfully.");
    }

    [MenuItem("Tools/Research Complex/Create NPC Prefab")]
    public static void CreateNpcPrefab()
    {
        GameObject npc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        npc.name = "NPC_Basic";

        NavMeshAgent agent = npc.AddComponent<NavMeshAgent>();
        agent.speed = 3.5f;
        agent.angularSpeed = 180f;
        agent.acceleration = 8f;
        agent.stoppingDistance = 0.15f;

        NPCPatrol patrol = npc.AddComponent<NPCPatrol>();
        Transform waypointsRoot = new GameObject("Waypoints").transform;
        waypointsRoot.SetParent(npc.transform);

        List<Transform> points = new List<Transform>();
        Vector3[] pointOffsets =
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(4f, 0f, 0f),
            new Vector3(4f, 0f, 4f),
            new Vector3(0f, 0f, 4f)
        };

        for (int i = 0; i < pointOffsets.Length; i++)
        {
            Transform point = new GameObject($"Point_{i + 1}").transform;
            point.SetParent(waypointsRoot);
            point.localPosition = pointOffsets[i];
            points.Add(point);
        }

        patrol.SetWaypoints(points.ToArray());

        string prefabPath = "Assets/Prefabs/NPC_Basic.prefab";
        PrefabUtility.SaveAsPrefabAsset(npc, prefabPath);
        Object.DestroyImmediate(npc);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[ResearchComplexSceneBuilder] NPC prefab created at {prefabPath}");
    }

    [MenuItem("Tools/Research Complex/Build University Classroom")]
    public static void BuildUniversityClassroom()
    {
        Transform root = GetOrCreateRoot("UniversityClassroom");
        root.position = Vector3.zero;

        GameObject deskPrefab = LoadPreferredPrefab(DeskPrefabGuidKey);
        if (deskPrefab == null)
        {
            deskPrefab = LoadPrefabAtPath(DefaultDeskPrefabPath);
        }

        if (deskPrefab == null)
        {
            deskPrefab = TryFindLibraryPrefab(new[] { "desk", "table", "school" });
        }

        GameObject chairPrefab = LoadPreferredPrefab(ChairPrefabGuidKey);
        if (chairPrefab == null)
        {
            chairPrefab = LoadPrefabAtPath(DefaultChairPrefabPath);
        }

        if (chairPrefab == null)
        {
            chairPrefab = TryFindLibraryPrefab(new[] { "chair", "seat", "school" });
        }

        float width = 24f;
        float depth = 16f;
        float height = 4.2f;
        float wallThickness = 0.4f;

        GameObject floor = GetOrCreatePrimitive(root, "Floor", PrimitiveType.Cube);
        floor.transform.localPosition = new Vector3(0f, -0.1f, 0f);
        floor.transform.localScale = new Vector3(width, 0.2f, depth);
        floor.isStatic = true;
        ApplyColor(floor, new Color(0.52f, 0.52f, 0.56f, 1f));

        GameObject ceiling = GetOrCreatePrimitive(root, "Ceiling", PrimitiveType.Cube);
        ceiling.transform.localPosition = new Vector3(0f, height, 0f);
        ceiling.transform.localScale = new Vector3(width, 0.2f, depth);
        ceiling.isStatic = true;
        ApplyColor(ceiling, new Color(0.88f, 0.9f, 0.92f, 1f));

        Transform walls = GetOrCreateChild(root, "Walls");

        GameObject south = GetOrCreatePrimitive(walls, "SouthWall", PrimitiveType.Cube);
        south.transform.localPosition = new Vector3(0f, height * 0.5f, -depth * 0.5f);
        south.transform.localScale = new Vector3(width, height, wallThickness);

        GameObject east = GetOrCreatePrimitive(walls, "EastWall", PrimitiveType.Cube);
        east.transform.localPosition = new Vector3(width * 0.5f, height * 0.5f, 0f);
        east.transform.localScale = new Vector3(wallThickness, height, depth);

        GameObject west = GetOrCreatePrimitive(walls, "WestWall", PrimitiveType.Cube);
        west.transform.localPosition = new Vector3(-width * 0.5f, height * 0.5f, 0f);
        west.transform.localScale = new Vector3(wallThickness, height, depth);

        // Стена с окнами (север): два простенка + верхняя балка + стеклянные панели.
        float northZ = depth * 0.5f;
        GameObject northLeft = GetOrCreatePrimitive(walls, "NorthWall_Left", PrimitiveType.Cube);
        northLeft.transform.localPosition = new Vector3(-width * 0.35f, height * 0.5f, northZ);
        northLeft.transform.localScale = new Vector3(width * 0.3f, height, wallThickness);

        GameObject northRight = GetOrCreatePrimitive(walls, "NorthWall_Right", PrimitiveType.Cube);
        northRight.transform.localPosition = new Vector3(width * 0.35f, height * 0.5f, northZ);
        northRight.transform.localScale = new Vector3(width * 0.3f, height, wallThickness);

        GameObject northTop = GetOrCreatePrimitive(walls, "NorthWall_Top", PrimitiveType.Cube);
        northTop.transform.localPosition = new Vector3(0f, height - 0.5f, northZ);
        northTop.transform.localScale = new Vector3(width * 0.4f, 1f, wallThickness);

        Transform windows = GetOrCreateChild(root, "Windows");
        GameObject windowA = GetOrCreatePrimitive(windows, "Window_A", PrimitiveType.Cube);
        windowA.transform.localPosition = new Vector3(-3f, 1.8f, northZ - 0.03f);
        windowA.transform.localScale = new Vector3(4.2f, 2.1f, 0.06f);

        GameObject windowB = GetOrCreatePrimitive(windows, "Window_B", PrimitiveType.Cube);
        windowB.transform.localPosition = new Vector3(3f, 1.8f, northZ - 0.03f);
        windowB.transform.localScale = new Vector3(4.2f, 2.1f, 0.06f);

        BuildOrUpdateWindowFrames(root, northZ);

        Color wallColor = new Color(0.78f, 0.8f, 0.84f, 1f);
        ApplyColor(south, wallColor);
        ApplyColor(east, wallColor);
        ApplyColor(west, wallColor);
        ApplyColor(northLeft, wallColor);
        ApplyColor(northRight, wallColor);
        ApplyColor(northTop, wallColor);

        Color glassColor = new Color(0.6f, 0.82f, 1f, 0.35f);
        ApplyColor(windowA, glassColor);
        ApplyColor(windowB, glassColor);

        Transform front = GetOrCreateChild(root, "FrontArea");

        GameObject board = GetOrCreatePrimitive(front, "Board", PrimitiveType.Cube);
        board.transform.localPosition = new Vector3(0f, 2f, -depth * 0.5f + 0.22f);
        board.transform.localScale = new Vector3(8.5f, 2.4f, 0.08f);
        ApplyColor(board, new Color(0.09f, 0.15f, 0.11f, 1f));

        GameObject podium = GetOrCreatePrimitive(front, "Podium", PrimitiveType.Cube);
        podium.transform.localPosition = new Vector3(0f, 0.65f, -depth * 0.5f + 2f);
        podium.transform.localScale = new Vector3(1.8f, 1.3f, 1f);
        ApplyColor(podium, new Color(0.36f, 0.24f, 0.14f, 1f));

        Transform desks = GetOrCreateChild(root, "Desks");
        BuildDeskGrid(desks, -5.5f, -0.5f, 4, 4, 3f, 2.4f, deskPrefab, chairPrefab);

        BuildCartoonLighting(root, width, depth, height);
        ApplyStylizedRenderSettings();

        foreach (Transform child in root.GetComponentsInChildren<Transform>())
        {
            if (child == root)
            {
                continue;
            }

            // Стулья оставляем динамическими, чтобы реакция на выстрел работала.
            if (IsChildOf(child, desks))
            {
                child.gameObject.isStatic = false;
                continue;
            }

            child.gameObject.isStatic = true;
        }

        CreateOrUpdateDirectionalLight();
        CreateOrUpdateNavMeshSurface();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[ResearchComplexSceneBuilder] University classroom built/updated.");

        if (deskPrefab == null || chairPrefab == null)
        {
            Debug.Log("[ResearchComplexSceneBuilder] Stylized desk/chair prefabs not found in project libraries. Fallback primitives were used.");
        }
    }

    [MenuItem("Tools/Research Complex/Set Selected As Desk Prefab")]
    public static void SetSelectedAsDeskPrefab()
    {
        SaveSelectedPrefabGuid(DeskPrefabGuidKey, "desk");
    }

    [MenuItem("Tools/Research Complex/Set Selected As Chair Prefab")]
    public static void SetSelectedAsChairPrefab()
    {
        SaveSelectedPrefabGuid(ChairPrefabGuidKey, "chair");
    }

    [MenuItem("Tools/Research Complex/Clear Assigned Desk/Chair Prefabs")]
    public static void ClearAssignedDeskChairPrefabs()
    {
        EditorPrefs.DeleteKey(DeskPrefabGuidKey);
        EditorPrefs.DeleteKey(ChairPrefabGuidKey);
        Debug.Log("[ResearchComplexSceneBuilder] Assigned desk/chair prefabs cleared.");
    }

    private static void BuildOrUpdateLocation(Transform parent, LocationPreset preset)
    {
        Transform location = GetOrCreateChild(parent, SanitizeName(preset.Name));
        location.position = preset.Center;

        GameObject floor = GetOrCreatePrimitive(location, "Floor", PrimitiveType.Cube);
        floor.transform.localPosition = new Vector3(0f, -0.1f, 0f);
        floor.transform.localScale = new Vector3(preset.Size.x, 0.2f, preset.Size.z);
        floor.isStatic = true;

        ApplyColor(floor, preset.Color * 0.7f);
        BuildOrUpdateLocationWalls(location, preset);

        Transform props = GetOrCreateChild(location, "Props");
        BuildSimpleProp(props, "Console_A", new Vector3(-2.5f, 0.6f, -2f), new Vector3(1.4f, 1.2f, 0.6f), preset.Color * 0.8f);
        BuildSimpleProp(props, "Console_B", new Vector3(2.3f, 0.6f, 2.4f), new Vector3(1.6f, 1.2f, 0.6f), preset.Color * 0.9f);

        Transform signRoot = GetOrCreateChild(location, "Sign");
        signRoot.localPosition = new Vector3(0f, preset.Size.y + 0.7f, 0f);
        TMP_Text signText = GetOrCreateTextMesh(signRoot, "SignText", preset.Name);

        GameObject triggerObject = GetOrCreatePrimitive(location, "LocationTrigger", PrimitiveType.Cube);
        triggerObject.transform.localPosition = new Vector3(0f, 1.2f, 0f);
        triggerObject.transform.localScale = new Vector3(preset.Size.x * 0.8f, 2.4f, preset.Size.z * 0.8f);

        Renderer triggerRenderer = triggerObject.GetComponent<Renderer>();
        if (triggerRenderer != null)
        {
            triggerRenderer.enabled = false;
        }

        BoxCollider triggerCollider = triggerObject.GetComponent<BoxCollider>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }

        LocationTrigger locationTrigger = triggerObject.GetComponent<LocationTrigger>();
        if (locationTrigger == null)
        {
            locationTrigger = triggerObject.AddComponent<LocationTrigger>();
        }

        locationTrigger.SetLocationData(preset.Name, preset.Subtitle);

        LocationZoneTemplate zoneTemplate = location.GetComponent<LocationZoneTemplate>();
        if (zoneTemplate == null)
        {
            zoneTemplate = location.gameObject.AddComponent<LocationZoneTemplate>();
        }

        zoneTemplate.SetReferences(locationTrigger, signText);
        zoneTemplate.SetLocationData(preset.Name, preset.Subtitle);
    }

    private static void BuildOrUpdateLocationWalls(Transform location, LocationPreset preset)
    {
        Transform wallsRoot = GetOrCreateChild(location, "Walls");

        float height = preset.Size.y;
        float thickness = 0.6f;
        float halfX = preset.Size.x * 0.5f;
        float halfZ = preset.Size.z * 0.5f;

        GameObject north = GetOrCreatePrimitive(wallsRoot, "North", PrimitiveType.Cube);
        north.transform.localPosition = new Vector3(0f, height * 0.5f, halfZ);
        north.transform.localScale = new Vector3(preset.Size.x, height, thickness);

        GameObject south = GetOrCreatePrimitive(wallsRoot, "South", PrimitiveType.Cube);
        south.transform.localPosition = new Vector3(0f, height * 0.5f, -halfZ);
        south.transform.localScale = new Vector3(preset.Size.x, height, thickness);

        GameObject east = GetOrCreatePrimitive(wallsRoot, "East", PrimitiveType.Cube);
        east.transform.localPosition = new Vector3(halfX, height * 0.5f, 0f);
        east.transform.localScale = new Vector3(thickness, height, preset.Size.z);

        GameObject west = GetOrCreatePrimitive(wallsRoot, "West", PrimitiveType.Cube);
        west.transform.localPosition = new Vector3(-halfX, height * 0.5f, 0f);
        west.transform.localScale = new Vector3(thickness, height, preset.Size.z);

        north.isStatic = true;
        south.isStatic = true;
        east.isStatic = true;
        west.isStatic = true;

        Color wallColor = preset.Color * 0.35f;
        ApplyColor(north, wallColor);
        ApplyColor(south, wallColor);
        ApplyColor(east, wallColor);
        ApplyColor(west, wallColor);
    }

    private static void BuildOrUpdatePerimeterWalls(Transform gameplayRoot)
    {
        Transform perimeterRoot = GetOrCreateChild(gameplayRoot, "PerimeterWalls");

        float height = 5f;
        float thickness = 1f;
        float width = 140f;
        float depth = 140f;
        float halfW = width * 0.5f;
        float halfD = depth * 0.5f;

        GameObject north = GetOrCreatePrimitive(perimeterRoot, "North", PrimitiveType.Cube);
        north.transform.position = new Vector3(0f, height * 0.5f, halfD);
        north.transform.localScale = new Vector3(width, height, thickness);

        GameObject south = GetOrCreatePrimitive(perimeterRoot, "South", PrimitiveType.Cube);
        south.transform.position = new Vector3(0f, height * 0.5f, -halfD);
        south.transform.localScale = new Vector3(width, height, thickness);

        GameObject east = GetOrCreatePrimitive(perimeterRoot, "East", PrimitiveType.Cube);
        east.transform.position = new Vector3(halfW, height * 0.5f, 0f);
        east.transform.localScale = new Vector3(thickness, height, depth);

        GameObject west = GetOrCreatePrimitive(perimeterRoot, "West", PrimitiveType.Cube);
        west.transform.position = new Vector3(-halfW, height * 0.5f, 0f);
        west.transform.localScale = new Vector3(thickness, height, depth);

        north.isStatic = true;
        south.isStatic = true;
        east.isStatic = true;
        west.isStatic = true;

        Color perimeterColor = new Color(0.2f, 0.27f, 0.35f, 1f);
        ApplyColor(north, perimeterColor);
        ApplyColor(south, perimeterColor);
        ApplyColor(east, perimeterColor);
        ApplyColor(west, perimeterColor);
    }

    private static void CreateOrUpdateDirectionalLight()
    {
        Light dirLight = Object.FindFirstObjectByType<Light>();
        if (dirLight == null || dirLight.type != LightType.Directional)
        {
            GameObject lightObject = new GameObject("Directional Light");
            dirLight = lightObject.AddComponent<Light>();
            dirLight.type = LightType.Directional;
        }

        dirLight.intensity = 1.4f;
        dirLight.shadows = LightShadows.Soft;
        dirLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    private static void CreateOrUpdateNavMeshSurface()
    {
        Transform environment = GetOrCreateRoot("Environment");
        NavMeshSurface surface = environment.GetComponent<NavMeshSurface>();
        if (surface == null)
        {
            surface = environment.gameObject.AddComponent<NavMeshSurface>();
        }

        surface.collectObjects = CollectObjects.All;
        surface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
        surface.defaultArea = 0;
    }

    private static void BuildSimpleProp(Transform parent, string name, Vector3 localPos, Vector3 localScale, Color color)
    {
        GameObject prop = GetOrCreatePrimitive(parent, name, PrimitiveType.Cube);
        prop.transform.localPosition = localPos;
        prop.transform.localScale = localScale;
        ApplyColor(prop, color);
    }

    private static bool IsChildOf(Transform node, Transform potentialParent)
    {
        if (node == null || potentialParent == null)
        {
            return false;
        }

        Transform current = node;
        while (current != null)
        {
            if (current == potentialParent)
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private static void BuildDeskGrid(Transform parent, float startX, float startZ, int columns, int rows, float spacingX, float spacingZ, GameObject deskPrefab, GameObject chairPrefab)
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                string deskName = $"Desk_{row + 1}_{col + 1}";
                Transform deskRoot = GetOrCreateChild(parent, deskName);
                deskRoot.localPosition = new Vector3(startX + (col * spacingX), 0f, startZ + (row * spacingZ));

                // По запросу сцены в этом режиме столы полностью убраны.
                RemoveChildIfExists(deskRoot, "DeskMesh");
                RemoveChildIfExists(deskRoot, "TableTop");
                RemoveChildIfExists(deskRoot, "LegLeft");
                RemoveChildIfExists(deskRoot, "LegRight");

                if (chairPrefab != null)
                {
                    RemoveChildIfExists(deskRoot, "ChairSeat");
                    RemoveChildIfExists(deskRoot, "ChairBack");
                    CreateOrUpdatePrefabInstance(deskRoot, "ChairMesh", chairPrefab, new Vector3(0f, 0f, 1.1f), new Vector3(0.85f, 0.85f, 0.85f));
                }
                else
                {
                    RemoveChildIfExists(deskRoot, "ChairMesh");
                    BuildFallbackCartoonChair(deskRoot);
                }
            }
        }
    }

    private static void RemoveChildIfExists(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);
        if (child != null)
        {
            Object.DestroyImmediate(child.gameObject);
        }
    }

    private static void BuildFallbackCartoonDesk(Transform deskRoot)
    {
        GameObject tableTop = GetOrCreatePrimitive(deskRoot, "TableTop", PrimitiveType.Cube);
        tableTop.transform.localPosition = new Vector3(0f, 0.78f, 0f);
        tableTop.transform.localScale = new Vector3(1.7f, 0.12f, 0.95f);
        ApplyColor(tableTop, new Color(0.96f, 0.67f, 0.26f, 1f));

        GameObject leftLeg = GetOrCreatePrimitive(deskRoot, "LegLeft", PrimitiveType.Cylinder);
        leftLeg.transform.localPosition = new Vector3(-0.65f, 0.38f, 0f);
        leftLeg.transform.localScale = new Vector3(0.08f, 0.38f, 0.08f);

        GameObject rightLeg = GetOrCreatePrimitive(deskRoot, "LegRight", PrimitiveType.Cylinder);
        rightLeg.transform.localPosition = new Vector3(0.65f, 0.38f, 0f);
        rightLeg.transform.localScale = new Vector3(0.08f, 0.38f, 0.08f);

        ApplyColor(leftLeg, new Color(0.22f, 0.25f, 0.34f, 1f));
        ApplyColor(rightLeg, new Color(0.22f, 0.25f, 0.34f, 1f));
    }

    private static void BuildFallbackCartoonChair(Transform deskRoot)
    {
        GameObject chairSeat = GetOrCreatePrimitive(deskRoot, "ChairSeat", PrimitiveType.Cube);
        chairSeat.transform.localPosition = new Vector3(0f, 0.47f, 1.0f);
        chairSeat.transform.localScale = new Vector3(0.72f, 0.12f, 0.72f);

        GameObject chairBack = GetOrCreatePrimitive(deskRoot, "ChairBack", PrimitiveType.Cube);
        chairBack.transform.localPosition = new Vector3(0f, 0.95f, 1.28f);
        chairBack.transform.localScale = new Vector3(0.72f, 0.85f, 0.12f);

        ApplyColor(chairSeat, new Color(0.28f, 0.68f, 0.94f, 1f));
        ApplyColor(chairBack, new Color(0.28f, 0.68f, 0.94f, 1f));
    }

    private static void BuildOrUpdateWindowFrames(Transform root, float northZ)
    {
        Transform frames = GetOrCreateChild(root, "WindowFrames");
        Color frameColor = new Color(0.34f, 0.39f, 0.45f, 1f);

        BuildFrame(frames, "Frame_A_Left", new Vector3(-5.1f, 1.8f, northZ), new Vector3(0.12f, 2.15f, 0.16f), frameColor);
        BuildFrame(frames, "Frame_A_Right", new Vector3(-0.9f, 1.8f, northZ), new Vector3(0.12f, 2.15f, 0.16f), frameColor);
        BuildFrame(frames, "Frame_B_Left", new Vector3(0.9f, 1.8f, northZ), new Vector3(0.12f, 2.15f, 0.16f), frameColor);
        BuildFrame(frames, "Frame_B_Right", new Vector3(5.1f, 1.8f, northZ), new Vector3(0.12f, 2.15f, 0.16f), frameColor);
        BuildFrame(frames, "Frame_A_Top", new Vector3(-3f, 2.85f, northZ), new Vector3(4.2f, 0.12f, 0.16f), frameColor);
        BuildFrame(frames, "Frame_B_Top", new Vector3(3f, 2.85f, northZ), new Vector3(4.2f, 0.12f, 0.16f), frameColor);
    }

    private static void BuildFrame(Transform parent, string name, Vector3 localPos, Vector3 localScale, Color color)
    {
        GameObject frame = GetOrCreatePrimitive(parent, name, PrimitiveType.Cube);
        frame.transform.localPosition = localPos;
        frame.transform.localScale = localScale;
        ApplyColor(frame, color);
    }

    private static void BuildCartoonLighting(Transform root, float width, float depth, float height)
    {
        Transform lighting = GetOrCreateChild(root, "Lighting");

        BuildLamp(lighting, "Lamp_FrontLeft", new Vector3(-width * 0.25f, height - 0.35f, -depth * 0.25f));
        BuildLamp(lighting, "Lamp_FrontRight", new Vector3(width * 0.25f, height - 0.35f, -depth * 0.25f));
        BuildLamp(lighting, "Lamp_BackLeft", new Vector3(-width * 0.25f, height - 0.35f, depth * 0.2f));
        BuildLamp(lighting, "Lamp_BackRight", new Vector3(width * 0.25f, height - 0.35f, depth * 0.2f));
    }

    private static void BuildLamp(Transform parent, string name, Vector3 localPos)
    {
        Transform lampRoot = GetOrCreateChild(parent, name);
        lampRoot.localPosition = localPos;

        GameObject lampMesh = GetOrCreatePrimitive(lampRoot, "LampMesh", PrimitiveType.Sphere);
        lampMesh.transform.localPosition = Vector3.zero;
        lampMesh.transform.localScale = new Vector3(0.55f, 0.18f, 0.55f);
        ApplyColor(lampMesh, new Color(1f, 0.95f, 0.82f, 1f));

        Light point = lampRoot.GetComponent<Light>();
        if (point == null)
        {
            point = lampRoot.gameObject.AddComponent<Light>();
        }

        point.type = LightType.Point;
        point.range = 10f;
        point.intensity = 5.2f;
        point.color = new Color(1f, 0.93f, 0.8f, 1f);
        point.shadows = LightShadows.Soft;
    }

    private static void ApplyStylizedRenderSettings()
    {
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.72f, 0.8f, 0.94f, 1f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.82f, 0.9f, 1f, 1f);
        RenderSettings.fogDensity = 0.006f;
    }

    private static GameObject TryFindLibraryPrefab(string[] keywords)
    {
        string[] searchRoots = { "Assets/Models", "Assets/Prefabs", "Assets" };
        foreach (string root in searchRoots)
        {
            string filter = "t:Prefab";
            foreach (string keyword in keywords)
            {
                filter += $" {keyword}";
            }

            string[] guids = AssetDatabase.FindAssets(filter, new[] { root });
            if (guids.Length == 0)
            {
                continue;
            }

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    continue;
                }

                return prefab;
            }
        }

        return null;
    }

    private static void SaveSelectedPrefabGuid(string prefsKey, string label)
    {
        Object selected = Selection.activeObject;
        if (selected == null)
        {
            Debug.LogWarning($"[ResearchComplexSceneBuilder] Select a prefab asset first to assign {label}.");
            return;
        }

        string path = AssetDatabase.GetAssetPath(selected);
        if (string.IsNullOrWhiteSpace(path))
        {
            Debug.LogWarning($"[ResearchComplexSceneBuilder] Selected object is not an asset. Assign {label} from Project window.");
            return;
        }

        PrefabAssetType prefabType = PrefabUtility.GetPrefabAssetType(selected);
        if (prefabType == PrefabAssetType.NotAPrefab)
        {
            Debug.LogWarning($"[ResearchComplexSceneBuilder] Selected asset is not a prefab. Assign {label} prefab.");
            return;
        }

        string guid = AssetDatabase.AssetPathToGUID(path);
        if (string.IsNullOrWhiteSpace(guid))
        {
            Debug.LogWarning($"[ResearchComplexSceneBuilder] Could not resolve GUID for selected {label} prefab.");
            return;
        }

        EditorPrefs.SetString(prefsKey, guid);
        Debug.Log($"[ResearchComplexSceneBuilder] Assigned {label} prefab: {path}");
    }

    private static GameObject LoadPreferredPrefab(string prefsKey)
    {
        if (!EditorPrefs.HasKey(prefsKey))
        {
            return null;
        }

        string guid = EditorPrefs.GetString(prefsKey);
        if (string.IsNullOrWhiteSpace(guid))
        {
            return null;
        }

        string path = AssetDatabase.GUIDToAssetPath(guid);
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    private static GameObject LoadPrefabAtPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    private static void CreateOrUpdatePrefabInstance(Transform parent, string childName, GameObject prefab, Vector3 localPos, Vector3 localScale)
    {
        Transform existing = parent.Find(childName);
        GameObject instance;

        if (existing == null)
        {
            instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = childName;
            instance.transform.SetParent(parent, false);
        }
        else
        {
            instance = existing.gameObject;
        }

        instance.transform.localPosition = localPos;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = localScale;
    }

    private static TMP_Text GetOrCreateTextMesh(Transform parent, string name, string text)
    {
        Transform textTransform = GetOrCreateChild(parent, name);
        TMP_Text tmp = textTransform.GetComponent<TextMeshPro>();
        if (tmp == null)
        {
            tmp = textTransform.gameObject.AddComponent<TextMeshPro>();
        }

        tmp.text = text;
        tmp.fontSize = 2.8f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.8f, 0.95f, 1f, 1f);

        textTransform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        textTransform.localPosition = Vector3.zero;

        return tmp;
    }

    private static GameObject GetOrCreatePrimitive(Transform parent, string childName, PrimitiveType type)
    {
        Transform child = parent.Find(childName);
        if (child != null)
        {
            return child.gameObject;
        }

        GameObject created = GameObject.CreatePrimitive(type);
        created.name = childName;
        created.transform.SetParent(parent);
        created.transform.localPosition = Vector3.zero;
        created.transform.localRotation = Quaternion.identity;
        created.transform.localScale = Vector3.one;
        return created;
    }

    private static Transform GetOrCreateRoot(string name)
    {
        GameObject root = GameObject.Find(name);
        if (root != null)
        {
            return root.transform;
        }

        return new GameObject(name).transform;
    }

    private static Transform GetOrCreateChild(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null)
        {
            return child;
        }

        GameObject created = new GameObject(name);
        created.transform.SetParent(parent);
        created.transform.localPosition = Vector3.zero;
        created.transform.localRotation = Quaternion.identity;
        created.transform.localScale = Vector3.one;
        return created.transform;
    }

    private static void ApplyColor(GameObject target, Color color)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        Material mat = renderer.sharedMaterial;
        if (mat == null || mat.name == "Default-Material")
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        }

        mat.color = color;
        renderer.sharedMaterial = mat;
    }

    private static string SanitizeName(string text)
    {
        return text.Replace(" ", string.Empty);
    }
}

