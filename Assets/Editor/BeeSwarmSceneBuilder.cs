using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using BeeSwarm.Core;
using BeeSwarm.Gameplay;

/// <summary>
/// 2D scene builder for BeeSwarm. Tools > BeeSwarm > Build Main Scene
/// </summary>
public class BeeSwarmSceneBuilder : Editor
{
    [MenuItem("Tools/BeeSwarm/Build Main Scene")]
    static void BuildMainScene()
    {
        GameObject root = new GameObject("_BeeSwarm");
        Undo.RegisterCreatedObjectUndo(root, "Build BeeSwarm Scene");

        // === 1. Camera (orthographic 2D) ===
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("MainCamera", typeof(Camera), typeof(AudioListener));
            cam = camObj.GetComponent<Camera>();
        }
        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.transform.rotation = Quaternion.identity;
        cam.orthographic = true;
        cam.orthographicSize = 15f;
        cam.clearFlags = CameraClearFlags.Color;
        cam.backgroundColor = new Color(0.5f, 0.7f, 1f);
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 50f;
        cam.gameObject.name = "MainCamera";

        // === 2. Directional Light ===
        if (FindObjectOfType<Light>() == null)
        {
            GameObject lightObj = new GameObject("Directional Light", typeof(Light));
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            var light = lightObj.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 0.8f;
            light.shadows = LightShadows.None;
        }

        // === 3. Ground (green background tile) ===
        GameObject ground = new GameObject("Ground");
        ground.transform.position = Vector3.zero;
        var gsr = ground.AddComponent<SpriteRenderer>();
        gsr.sprite = CreateSolidSprite(32, new Color(0.2f, 0.55f, 0.1f));
        gsr.transform.localScale = new Vector3(60f, 60f, 1f);
        gsr.sortingOrder = -10;

        // === 4. Hive ===
        GameObject hiveObj = new GameObject("Hive");
        hiveObj.transform.SetParent(root.transform);
        hiveObj.transform.position = Vector3.zero;

        HiveManager hiveManager = hiveObj.AddComponent<HiveManager>();
        hiveManager.gameObject.AddComponent<HiveKnowledgeBase>();

        var spawnPoint = new GameObject("BeeSpawnPoint").transform;
        spawnPoint.SetParent(hiveObj.transform);
        spawnPoint.localPosition = new Vector3(1f, 0.5f, 0f);
        SetPrivateField(hiveManager, "beeSpawnPoint", spawnPoint);

        var entrance = new GameObject("HiveEntrance").transform;
        entrance.SetParent(hiveObj.transform);
        entrance.localPosition = new Vector3(1f, 0.5f, 0f);
        SetPrivateField(hiveManager, "hiveEntrance", entrance);

        // === 5. Bee prefab ===
        GameObject beePrefab = CreateBeePrefab();
        SetPrivateField(hiveManager, "beePrefab", beePrefab);

        // === 6. GameManager ===
        GameObject gmObj = new GameObject("GameManager");
        gmObj.transform.SetParent(root.transform);
        GameManager gm = gmObj.AddComponent<GameManager>();
        SetPrivateField(gm, "hiveManager", hiveManager);

        // === 7. SeasonCycle ===
        GameObject seasonObj = new GameObject("SeasonCycle");
        seasonObj.transform.SetParent(root.transform);
        seasonObj.AddComponent<SeasonCycle>();

        // === 8. FlowerSpawner ===
        GameObject flowerObj = new GameObject("FlowerSpawner");
        flowerObj.transform.SetParent(root.transform);
        var spawner = flowerObj.AddComponent<FlowerSpawner>();
        SetPrivateField(spawner, "seasonCycle", seasonObj.GetComponent<SeasonCycle>());

        // === 9. BeeHiveDisplay ===
        GameObject displayObj = new GameObject("BeeHiveDisplay");
        displayObj.transform.SetParent(hiveObj.transform);
        var display = displayObj.AddComponent<BeeHiveDisplay>();
        SetPrivateField(display, "hiveManager", hiveManager);
        SetPrivateField(display, "seasonCycle", seasonObj.GetComponent<SeasonCycle>());

        // === 10. HeatmapController ===
        var heatmapObj = new GameObject("HeatmapController");
        heatmapObj.transform.SetParent(root.transform);
        heatmapObj.AddComponent<HeatmapController>();

        // Save scene
        var scene = EditorSceneManager.GetActiveScene();
        if (string.IsNullOrEmpty(scene.path) || !scene.path.EndsWith("MainScene.unity"))
            scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainScene.unity");
        Debug.Log("BeeSwarm 2D scene built! Press Play");
    }

    [MenuItem("Tools/BeeSwarm/Clear Scene")]
    static void ClearScene()
    {
        var bs = GameObject.Find("_BeeSwarm");
        var gm = GameObject.Find("GameManager");
        if (bs != null) DestroyImmediate(bs);
        if (gm != null) DestroyImmediate(gm);
        Debug.Log("Cleared");
    }

    static void SetPrivateField(object obj, string field, object value)
    {
        var f = obj.GetType().GetField(field,
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (f != null) f.SetValue(obj, value);
    }

    static Sprite CreateSolidSprite(int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++) for (int x = 0; x < size; x++) tex.SetPixel(x, y, color);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
    }

    static GameObject CreateBeePrefab()
    {
        GameObject bee = new GameObject("BeePrefab");
        bee.transform.localScale = Vector3.one * 0.5f;

        // Body circle sprite
        var sr = bee.AddComponent<SpriteRenderer>();
        Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        float c = 8f, r = 7f;
        for (int y = 0; y < 16; y++) for (int x = 0; x < 16; x++)
                tex.SetPixel(x, y, Vector2.Distance(new Vector2(x, y), new Vector2(c, c)) <= r ? Color.yellow : Color.clear);
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
        sr.sortingOrder = 1;

        // Wings (two small white circles)
        for (int i = -1; i <= 1; i += 2)
        {
            GameObject wing = new GameObject(i < 0 ? "Wing_L" : "Wing_R");
            wing.transform.SetParent(bee.transform);
            wing.transform.localPosition = new Vector3(i * 0.3f, 0.2f, 0f);
            var wsr = wing.AddComponent<SpriteRenderer>();
            Texture2D wt = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            c = 4f; r = 3f;
            for (int wy = 0; wy < 8; wy++) for (int wx = 0; wx < 8; wx++)
                    wt.SetPixel(wx, wy, Vector2.Distance(new Vector2(wx, wy), new Vector2(c, c)) <= r ? new Color(0.8f, 0.9f, 1f, 0.6f) : Color.clear);
            wt.Apply();
            wsr.sprite = Sprite.Create(wt, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 16f);
            wsr.sortingOrder = 2;
        }

        var rb = bee.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearDamping = 0.5f;

        bee.AddComponent<BeeController>();
        bee.AddComponent<BeeMemory>();
        bee.SetActive(false);

        string path = "Assets/Prefabs/Bee.prefab";
        PrefabUtility.SaveAsPrefabAsset(bee, path);
        Debug.Log("Bee prefab saved: " + path);
        return bee;
    }
}
