using UnityEngine;
using BeeSwarm.Core;

namespace BeeSwarm.Gameplay
{
    /// <summary>
    /// Визуальное представление улья на сцене.
    /// Показывает мёд, сезонный цвет, состояние.
    /// </summary>
    public class BeeHiveDisplay : MonoBehaviour
    {
        [Header("Визуал")]
        [SerializeField] private Color summerColor = new Color(0.8f, 0.5f, 0.2f);
        [SerializeField] private Color winterColor = new Color(0.6f, 0.6f, 0.8f);
        [SerializeField] private float scalePerBee = 0.02f;
        [SerializeField] private float baseScale = 1.5f;

        [Header("Ссылки")]
        [SerializeField] private HiveManager hiveManager;
        [SerializeField] private SeasonCycle seasonCycle;
        [SerializeField] private Transform visualRoot;

        private MeshRenderer hiveRenderer;
        private SeasonCycle.Season lastSeason;

        void Start()
        {
            if (hiveManager == null)
                hiveManager = FindObjectOfType<HiveManager>();
            if (seasonCycle == null)
                seasonCycle = FindObjectOfType<SeasonCycle>();
            if (visualRoot == null)
                visualRoot = transform;

            // Создаём визуал улья если нет
            if (visualRoot.childCount == 0)
                CreateHiveVisual();

            hiveRenderer = GetComponentInChildren<MeshRenderer>();

            if (seasonCycle != null)
            {
                seasonCycle.OnSeasonChanged += OnSeasonChange;
                OnSeasonChange(seasonCycle.CurrentSeason);
            }

            lastSeason = seasonCycle != null ? seasonCycle.CurrentSeason : SeasonCycle.Season.Spring;
        }

        void Update()
        {
            // Обновляем размер улья по количеству пчёл
            if (hiveManager != null)
            {
                float scale = baseScale + hiveManager.BeeCount * scalePerBee;
                scale = Mathf.Clamp(scale, 0.5f, 5f);
                visualRoot.localScale = Vector3.one * scale;
            }
        }

        void CreateHiveVisual()
        {
            // Создаём тело улья (сфера/капля)
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.transform.SetParent(visualRoot);
            body.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            body.transform.localScale = new Vector3(2f, 1.8f, 2f);
            body.name = "HiveBody";

            // Убираем коллайдер (улей статичный, коллайдер у HiveManager)
            DestroyImmediate(body.GetComponent<Collider>());

            hiveRenderer = body.GetComponent<MeshRenderer>();
            if (hiveRenderer != null)
            {
                hiveRenderer.material = new Material(Shader.Find("Standard"));
                hiveRenderer.material.color = summerColor;
            }

            // Создаём вход в улей (маленькая сфера)
            GameObject entrance = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            entrance.transform.SetParent(visualRoot);
            entrance.transform.localPosition = new Vector3(1.2f, 0.5f, 0f);
            entrance.transform.localScale = new Vector3(0.4f, 0.3f, 0.4f);
            entrance.name = "Entrance";
            DestroyImmediate(entrance.GetComponent<Collider>());
            var eRenderer = entrance.GetComponent<MeshRenderer>();
            if (eRenderer != null)
            {
                eRenderer.material = new Material(Shader.Find("Standard"));
                eRenderer.material.color = new Color(0.2f, 0.15f, 0.1f);
            }
        }

        void OnSeasonChange(SeasonCycle.Season season)
        {
            if (hiveRenderer == null) return;

            // Меняем цвет улья по сезону
            Color targetColor;
            switch (season)
            {
                case SeasonCycle.Season.Spring:
                    targetColor = Color.Lerp(summerColor, Color.white, 0.2f);
                    break;
                case SeasonCycle.Season.Summer:
                    targetColor = summerColor;
                    break;
                case SeasonCycle.Season.Autumn:
                    targetColor = Color.Lerp(summerColor, new Color(0.3f, 0.2f, 0.1f), 0.5f);
                    break;
                case SeasonCycle.Season.Winter:
                    targetColor = winterColor;
                    break;
                default:
                    targetColor = summerColor;
                    break;
            }

            StartCoroutine(LerpColor(targetColor, 1f));
        }

        System.Collections.IEnumerator LerpColor(Color target, float duration)
        {
            Color start = hiveRenderer.material.color;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                hiveRenderer.material.color = Color.Lerp(start, target, t / duration);
                yield return null;
            }
            hiveRenderer.material.color = target;
        }

        void OnDestroy()
        {
            if (seasonCycle != null)
                seasonCycle.OnSeasonChanged -= OnSeasonChange;
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
