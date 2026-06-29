using UnityEngine;
using BeeSwarm.Core;

namespace BeeSwarm.Gameplay
{
    /// <summary>
    /// 2D визуальное представление улья на сцене
    /// </summary>
    public class BeeHiveDisplay : MonoBehaviour
    {
        [Header("Спрайты")]
        [SerializeField] private Color hiveColor = new Color(0.7f, 0.4f, 0.15f);
        [SerializeField] private Color winterColor = new Color(0.6f, 0.6f, 0.8f);
        [SerializeField] private float baseScale = 2f;
        [SerializeField] private float scalePerBee = 0.03f;

        [Header("Ссылки")]
        [SerializeField] private HiveManager hiveManager;
        [SerializeField] private SeasonCycle seasonCycle;
        [SerializeField] private Transform visualRoot;

        private SpriteRenderer hiveRenderer;
        private SpriteRenderer entranceRenderer;

        void Start()
        {
            if (hiveManager == null) hiveManager = FindObjectOfType<HiveManager>();
            if (seasonCycle == null) seasonCycle = FindObjectOfType<SeasonCycle>();
            if (visualRoot == null) visualRoot = transform;

            if (visualRoot.childCount == 0) CreateHiveVisual();

            hiveRenderer = visualRoot.GetComponentInChildren<SpriteRenderer>();

            if (seasonCycle != null)
            {
                seasonCycle.OnSeasonChanged += OnSeasonChange;
                OnSeasonChange(seasonCycle.CurrentSeason);
            }
        }

        void Update()
        {
            if (hiveManager != null)
            {
                float scale = baseScale + hiveManager.BeeCount * scalePerBee;
                scale = Mathf.Clamp(scale, 0.5f, 5f);
                visualRoot.localScale = Vector3.one * scale;
            }
        }

        void CreateHiveVisual()
        {
            // Hive body
            GameObject body = new GameObject("HiveBody");
            body.transform.SetParent(visualRoot);
            body.transform.localPosition = Vector3.zero;

            var sr = body.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite(32, hiveColor);
            sr.sortingOrder = 0;
            sr.transform.localScale = new Vector3(2f, 1.8f, 1f);
            hiveRenderer = sr;

            // Entrance (smaller dark circle)
            GameObject entrance = new GameObject("Entrance");
            entrance.transform.SetParent(visualRoot);
            entrance.transform.localPosition = new Vector3(1f, -0.3f, 0f);

            var esr = entrance.AddComponent<SpriteRenderer>();
            esr.sprite = CreateCircleSprite(8, new Color(0.2f, 0.15f, 0.1f));
            esr.sortingOrder = 1;
            entrance.transform.localScale = new Vector3(0.4f, 0.3f, 1f);
        }

        Sprite CreateCircleSprite(int resolution, Color color)
        {
            Texture2D tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
            float center = resolution / 2f;
            float radius = center - 1f;
            for (int y = 0; y < resolution; y++)
                for (int x = 0; x < resolution; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    tex.SetPixel(x, y, dist <= radius ? color : Color.clear);
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f), 16f);
        }

        void OnSeasonChange(SeasonCycle.Season season)
        {
            if (hiveRenderer == null) return;
            Color target = season switch
            {
                SeasonCycle.Season.Spring => Color.Lerp(hiveColor, Color.white, 0.2f),
                SeasonCycle.Season.Summer => hiveColor,
                SeasonCycle.Season.Autumn => Color.Lerp(hiveColor, new Color(0.3f, 0.2f, 0.1f), 0.5f),
                SeasonCycle.Season.Winter => winterColor,
                _ => hiveColor
            };
            StartCoroutine(LerpColor(target, 1f));
        }

        System.Collections.IEnumerator LerpColor(Color target, float duration)
        {
            Color start = hiveRenderer.color;
            float t = 0f;
            while (t < duration) { t += Time.deltaTime; hiveRenderer.color = Color.Lerp(start, target, t / duration); yield return null; }
            hiveRenderer.color = target;
        }

        void OnDestroy()
        {
            if (seasonCycle != null) seasonCycle.OnSeasonChanged -= OnSeasonChange;
        }

        void OnGUI()
        {
            if (hiveManager == null) return;
            GUILayout.BeginArea(new Rect(Screen.width / 2 - 100, Screen.height - 60, 200, 50));
            GUILayout.Label($"🏠 Улей | Пчёл: {hiveManager.ActiveBeeCount}/{hiveManager.BeeCount}");
            GUILayout.Label($"🍯 {hiveManager.HoneyAmount:F0} | 🌾 {hiveManager.PollenAmount:F0} | 🕯 {hiveManager.WaxAmount:F0}");
            GUILayout.EndArea();
        }
    }
}
