using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using BeeSwarm.Core;
using BeeSwarm.Gameplay;

/// <summary>
/// Утилита для автоматической сборки игровой сцены BeeSwarm.
/// Запуск: Tools > BeeSwarm > Build Scene
/// </summary>
public class BeeSwarmSceneBuilder : Editor
{
    [MenuItem("Tools/BeeSwarm/Build Main Scene")]
    static void BuildMainScene()
    {
        // Создаём корневую структуру
        GameObject root = new GameObject("_BeeSwarm");
        Undo.RegisterCreatedObjectUndo(root, "Build BeeSwarm Scene");

        // === 1. Camera ===
        GameObject camObj = CreateOrFindCamera();

        // === 2. Directional Light ===
        CreateDirectionalLight();

        // === 3. Ground ===
        CreateGround();

        // === 4. Hive (HiveManager) ===
        GameObject hiveObj = new GameObject("Hive");
        hiveObj.transform.SetParent(root.transform);
        hiveObj.transform.position = Vector3.zero;

        HiveManager hiveManager = hiveObj.AddComponent<HiveManager>();
        hiveManager.gameObject.AddComponent<HiveKnowledgeBase>();

        // Создаём beeSpawnPoint
        GameObject spawnPoint = new GameObject("BeeSpawnPoint");
        spawnPoint.transform.SetParent(hiveObj.transform);
        spawnPoint.transform.localPosition = new Vector3(0f, 0.5f, 1.5f);
        var spawnField = typeof(HiveManager).GetField("beeSpawnPoint",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (spawnField != null) spawnField.SetValue(hiveManager, spawnPoint.transform);

        // Создаём hiveEntrance
        GameObject entrance = new GameObject("HiveEntrance");
        entrance.transform.SetParent(hiveObj.transform);
        entrance.transform.localPosition = new Vector3(0f, 0.5f, 1.5f);
        var entrField = typeof(HiveManager).GetField("hiveEntrance",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (entrField != null) entrField.SetValue(hiveManager, entrance.transform);

        // === 5. Bee Prefab (создаём программно) ===
        GameObject beePrefab = CreateBeePrefab();
        var prefabField = typeof(HiveManager).GetField("beePrefab",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (prefabField != null) prefabField.SetValue(hiveManager, beePrefab);

        // === 6. GameManager ===
        GameObject gmObj = new GameObject("GameManager");
        gmObj.transform.SetParent(root.transform);
        GameManager gm = gmObj.AddComponent<GameManager>();

        var hiveRefField = typeof(GameManager).GetField("hiveManager",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (hiveRefField != null) hiveRefField.SetValue(gm, hiveManager);

        // === 7. SeasonCycle ===
        GameObject seasonObj = new GameObject("SeasonCycle");
        seasonObj.transform.SetParent(root.transform);
        seasonObj.AddComponent<SeasonCycle>();

        // === 8. FlowerSpawner ===
        GameObject flowerObj = new GameObject("FlowerSpawner");
        flowerObj.transform.SetParent(root.transform);
        FlowerSpawner spawner = flowerObj.AddComponent<FlowerSpawner>();
        var seasonField = typeof(FlowerSpawner).GetField("seasonCycle",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (seasonField != null) seasonField.SetValue(spawner, seasonObj.GetComponent<SeasonCycle>());

        // === 9. BeeHiveDisplay ===
        GameObject displayObj = new GameObject("BeeHiveDisplay");
        displayObj.transform.SetParent(hiveObj.transform);
        BeeHiveDisplay display = displayObj.AddComponent<BeeHiveDisplay>();
        var dHiveField = typeof(BeeHiveDisplay).GetField("hiveManager",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (dHiveField != null) dHiveField.SetValue(display, hiveManager);
        var dSeasonField = typeof(BeeHiveDisplay).GetField("seasonCycle",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (dSeasonField != null) dSeasonField.SetValue(display, seasonObj.GetComponent<SeasonCycle>());

        // === 10. HeatmapController ===
        GameObject heatmapObj = new GameObject("HeatmapController");
        heatmapObj.transform.SetParent(root.transform);
        heatmapObj.AddComponent<HeatmapController>();

        // Сохраняем сцену
        var scene = EditorSceneManager.GetActiveScene();
        if (string.IsNullOrEmpty(scene.path) || !scene.path.EndsWith("MainScene.unity"))
        {
            scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainScene.unity");

        Debug.Log("BeeSwarm scene built! Press Play");
    }

    [MenuItem("Tools/BeeSwarm/Clear Scene")]
    static void ClearScene()
    {
        var beeSwarm = GameObject.Find("_BeeSwarm");
        if (beeSwarm != null)
            DestroyImmediate(beeSwarm);

        var gm = GameObject.Find("GameManager");
        if (gm != null) DestroyImmediate(gm);

        Debug.Log("🧹 BeeSwarm сцена очищена");
    }

    static GameObject CreateOrFindCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("MainCamera", typeof(Camera), typeof(AudioListener));
            cam = camObj.GetComponent<Camera>();
        }

        cam.transform.position = new Vector3(0f, 15f, -10f);
        cam.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
        cam.orthographic = true;
        cam.orthographicSize = 15f;
        cam.clearFlags = CameraClearFlags.Skybox;
        cam.nearClipPlane = 0.3f;
        cam.farClipPlane = 100f;
        cam.gameObject.name = "MainCamera";

        return cam.gameObject;
    }

    static void CreateDirectionalLight()
    {
        var existing = GameObject.FindObjectOfType<Light>();
        if (existing != null) return;

        GameObject lightObj = new GameObject("Directional Light", typeof(Light));
        lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        Light light = lightObj.GetComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1f;
        light.shadows = LightShadows.Soft;
        light.color = new Color(1f, 0.95f, 0.8f);

        // Создаём ambient light через RenderSettings
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.6f, 0.7f, 0.8f);
    }

    static void CreateGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = new Vector3(0f, -0.5f, 0f);
        ground.transform.localScale = new Vector3(3f, 1f, 3f);

        // Травяной цвет
        Renderer renderer = ground.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = new Color(0.2f, 0.6f, 0.1f);
        }

        // Добавляем layerMask для коллизий пчёл
        ground.layer = LayerMask.NameToLayer("Default");
    }

    static GameObject CreateBeePrefab()
    {
        GameObject bee = new GameObject("BeePrefab");
        bee.transform.localScale = Vector3.one * 0.3f;

        // 3D модель пчелы — тело (сфера) + полоски
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        body.name = "Body";
        body.transform.SetParent(bee.transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = Vector3.one;
        DestroyImmediate(body.GetComponent<Collider>());

        Renderer bodyRenderer = body.GetComponent<Renderer>();
        if (bodyRenderer != null)
        {
            bodyRenderer.material = new Material(Shader.Find("Standard"));
            bodyRenderer.material.color = new Color(1f, 0.8f, 0f); // Жёлтый
        }

        // Крылья (2 spheres приплюснутые)
        for (int i = -1; i <= 1; i += 2)
        {
            GameObject wing = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            wing.name = i < 0 ? "Wing_L" : "Wing_R";
            wing.transform.SetParent(bee.transform);
            wing.transform.localPosition = new Vector3(i * 0.4f, 0.2f, 0f);
            wing.transform.localScale = new Vector3(0.5f, 0.05f, 0.3f);
            DestroyImmediate(wing.GetComponent<Collider>());

            Renderer wingRenderer = wing.GetComponent<Renderer>();
            if (wingRenderer != null)
            {
                wingRenderer.material = new Material(Shader.Find("Standard"));
                wingRenderer.material.color = new Color(0.8f, 0.9f, 1f, 0.5f);
            }
        }

        // Полоски (3 тора/кольца)
        for (int i = 0; i < 3; i++)
        {
            GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stripe.name = $"Stripe_{i}";
            stripe.transform.SetParent(bee.transform);
            stripe.transform.localPosition = new Vector3(0f, 0f, -0.2f + i * 0.2f);
            stripe.transform.localScale = new Vector3(0.7f, 0.05f, 0.7f);
            DestroyImmediate(stripe.GetComponent<Collider>());

            Renderer sRenderer = stripe.GetComponent<Renderer>();
            if (sRenderer != null)
            {
                sRenderer.material = new Material(Shader.Find("Standard"));
                sRenderer.material.color = Color.black;
            }
        }

        // Компоненты
        bee.AddComponent<Rigidbody>();
        bee.AddComponent<BeeController>();
        bee.AddComponent<BeeMemory>();

        // Отключаем prefab — будет включён при спавне
        bee.SetActive(false);

        // Save as prefab
        string prefabPath = "Assets/Prefabs/Bee.prefab";
        PrefabUtility.SaveAsPrefabAsset(bee, prefabPath);
        Debug.Log("Bee prefab saved: " + prefabPath);

        return bee;
    }
}
