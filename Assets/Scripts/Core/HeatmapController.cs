using System.Collections.Generic;
using UnityEngine;

namespace BeeSwarm.Core
{
    /// <summary>
    /// Тепловая карта активности пчёл в реальном времени.
    /// Накладывает полупрозрачную текстуру поверх сцены,
    /// показывая где пчёлы проводят больше всего времени.
    /// </summary>
    public class HeatmapController : MonoBehaviour
    {
        [Header("Разрешение карты")]
        [SerializeField] private int textureWidth = 128;
        [SerializeField] private int textureHeight = 128;

        [Header("Границы мира")]
        [SerializeField] private Vector2 worldMin = new Vector2(-50f, -50f);
        [SerializeField] private Vector2 worldMax = new Vector2(50f, 50f);

        [Header("Настройка тепла")]
        [SerializeField] private float heatPerTick = 0.15f;      // насколько греем пиксель за шаг
        [SerializeField] private float decayPerSecond = 0.97f;   // затухание (1.0 = никогда)
        [SerializeField] private float updateInterval = 0.5f;    // обновление раз в N секунд
        [SerializeField] private float maxHeat = 1.0f;           // максимум яркости

        [Header("Цвета градиента")]
        [SerializeField] private Color coldColor = new Color(0f, 0f, 0.5f, 0.1f);    // синий
        [SerializeField] private Color midColor = new Color(0f, 1f, 0f, 0.3f);       // зелёный
        [SerializeField] private Color hotColor = new Color(1f, 0f, 0f, 0.6f);        // красный

        [Header("Ссылки")]
        [SerializeField] private Camera targetCamera;
        [SerializeField] [Range(0, 100)] private int sortingOrder = 10;

        // Внутреннее состояние
        private Texture2D heatTexture;
        private float[,] heatData;
        private float updateTimer;
        private GameObject overlayObject;
        private SpriteRenderer overlayRenderer;

        // Порог — сколько пчёл нужно для минимального нагрева
        private const float MIN_BEES_FOR_HEAT = 0.01f;
        private static readonly Vector3 PLANE_NORMAL = Vector3.forward;

        void Start()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;

            if (targetCamera == null)
            {
                Debug.LogError("HeatmapController: нет камеры!");
                enabled = false;
                return;
            }

            InitializeTexture();
            CreateOverlay();
        }

        void Update()
        {
            updateTimer += Time.deltaTime;
            if (updateTimer < updateInterval)
                return;

            updateTimer = 0f;

            DecayHeat();
            SampleBeePositions();
            RenderHeatmap();
        }

        /// <summary>
        /// Создать тепловую текстуру
        /// </summary>
        private void InitializeTexture()
        {
            heatData = new float[textureWidth, textureHeight];

            heatTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                anisoLevel = 0
            };

            // Заливка прозрачным
            Color[] pixels = new Color[textureWidth * textureHeight];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.clear;
            heatTexture.SetPixels(pixels);
            heatTexture.Apply();
        }

        /// <summary>
        /// Создать спрайтовый оверлей поверх камеры
        /// </summary>
        private void CreateOverlay()
        {
            overlayObject = new GameObject("_HeatmapOverlay");
            overlayObject.transform.SetParent(transform, false);

            // Подгоняем размер спрайта под область мира
            Vector2 worldSize = worldMax - worldMin;
            Vector2 worldCenter = (worldMin + worldMax) * 0.5f;

            overlayRenderer = overlayObject.AddComponent<SpriteRenderer>();
            Sprite sprite = Sprite.Create(
                heatTexture,
                new Rect(0, 0, textureWidth, textureHeight),
                new Vector2(0.5f, 0.5f),
                1f,                                    // pixelsPerUnit
                0,
                SpriteMeshType.FullRect
            );

            overlayRenderer.sprite = sprite;
            overlayRenderer.sortingOrder = sortingOrder;

            overlayObject.transform.position = new Vector3(worldCenter.x, worldCenter.y, 0f);
            overlayObject.transform.localScale = new Vector3(worldSize.x, worldSize.y, 1f);
        }

        /// <summary>
        /// Затухание тепла со временем
        /// </summary>
        private void DecayHeat()
        {
            float factor = Mathf.Pow(decayPerSecond, updateInterval);

            for (int x = 0; x < textureWidth; x++)
            {
                for (int y = 0; y < textureHeight; y++)
                {
                    heatData[x, y] *= factor;

                    // Обнуляем околонулевое для оптимизации
                    if (heatData[x, y] < 0.001f)
                        heatData[x, y] = 0f;
                }
            }
        }

        /// <summary>
        /// Собрать позиции всех активных пчёл и нагреть пиксели
        /// </summary>
        private void SampleBeePositions()
        {
            if (HiveManager.Instance == null) return;

            List<BeeController> bees = HiveManager.Instance.GetAllActiveBees();
            if (bees.Count == 0) return;

            foreach (BeeController bee in bees)
            {
                if (bee == null || !bee.gameObject.activeInHierarchy)
                    continue;

                Vector3 worldPos = bee.transform.position;
                Vector2Int texel = WorldToTexel(worldPos);

                AddHeat(texel.x, texel.y, heatPerTick);
            }
        }

        /// <summary>
        /// Из мировых координат в текстурные
        /// </summary>
        private Vector2Int WorldToTexel(Vector3 worldPos)
        {
            float nx = Mathf.InverseLerp(worldMin.x, worldMax.x, worldPos.x);
            float ny = Mathf.InverseLerp(worldMin.y, worldMax.y, worldPos.y);

            int tx = Mathf.RoundToInt(nx * (textureWidth - 1));
            int ty = Mathf.RoundToInt(ny * (textureHeight - 1));

            tx = Mathf.Clamp(tx, 0, textureWidth - 1);
            ty = Mathf.Clamp(ty, 0, textureHeight - 1);

            return new Vector2Int(tx, ty);
        }

        /// <summary>
        /// Добавить тепло в пиксель и соседние (размытие/сглаживание)
        /// </summary>
        private void AddHeat(int cx, int cy, float heat)
        {
            // 3x3 размытие — греем пиксель и его соседей с весами
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int x = cx + dx;
                    int y = cy + dy;

                    if (x < 0 || x >= textureWidth || y < 0 || y >= textureHeight)
                        continue;

                    // Вес: центр = 1.0, диагонали = 0.5, стороны = 0.75
                    float weight = (dx == 0 && dy == 0) ? 1.0f
                                 : (dx == 0 || dy == 0) ? 0.75f
                                 : 0.5f;

                    heatData[x, y] = Mathf.Min(heatData[x, y] + heat * weight, maxHeat);
                }
            }
        }

        /// <summary>
        /// Отрендерить тепловую карту в текстуру
        /// </summary>
        private void RenderHeatmap()
        {
            Color[] pixels = new Color[textureWidth * textureHeight];

            for (int y = 0; y < textureHeight; y++)
            {
                for (int x = 0; x < textureWidth; x++)
                {
                    float t = Mathf.Clamp01(heatData[x, y] / maxHeat);
                    pixels[y * textureWidth + x] = EvaluateGradient(t);
                }
            }

            heatTexture.SetPixels(pixels);
            heatTexture.Apply();

            // Обновляем спрайт
            if (overlayRenderer != null)
            {
                overlayRenderer.sprite = Sprite.Create(
                    heatTexture,
                    new Rect(0, 0, textureWidth, textureHeight),
                    new Vector2(0.5f, 0.5f),
                    1f,
                    0,
                    SpriteMeshType.FullRect
                );
            }
        }

        /// <summary>
        /// Интерполяция цвета по градиенту cold → mid → hot
        /// </summary>
        private Color EvaluateGradient(float t)
        {
            if (t <= 0f)
                return Color.clear;

            Color color;
            if (t < 0.5f)
            {
                // cold → mid
                float u = t / 0.5f;
                color = Color.Lerp(coldColor, midColor, u);
            }
            else
            {
                // mid → hot
                float u = (t - 0.5f) / 0.5f;
                color = Color.Lerp(midColor, hotColor, u);
            }

            return color;
        }

        /// <summary>
        /// Сбросить тепловую карту
        /// </summary>
        public void ResetHeatmap()
        {
            for (int x = 0; x < textureWidth; x++)
                for (int y = 0; y < textureHeight; y++)
                    heatData[x, y] = 0f;

            RenderHeatmap();
        }

        /// <summary>
        /// Включить/выключить оверлей
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (overlayRenderer != null)
                overlayRenderer.enabled = visible;
        }

        void OnDestroy()
        {
            if (heatTexture != null)
            {
                if (Application.isPlaying)
                    Destroy(heatTexture);
                else
                    DestroyImmediate(heatTexture);
            }

            if (overlayObject != null)
            {
                if (Application.isPlaying)
                    Destroy(overlayObject);
                else
                    DestroyImmediate(overlayObject);
            }
        }

        void OnDrawGizmosSelected()
        {
            // Показать границы тепловой карты
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
            Vector3 center = (worldMin + worldMax) * 0.5f;
            Vector3 size = worldMax - worldMin;
            Gizmos.DrawWireCube(new Vector3(center.x, center.y, 0f), new Vector3(size.x, size.y, 0f));
        }
    }
}
