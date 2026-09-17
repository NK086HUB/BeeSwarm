using System.Collections.Generic;
using UnityEngine;

namespace BeeSwarm.Gameplay
{
    /// <summary>
    /// Спавн цветов и реестр поляны.
    ///
    /// Реестр нужен пчёлам: раньше единственным «источником истины» была память
    /// пчелы, а сами объекты Flower никто не искал — цикл фуражировки был разорван.
    /// Здесь же живёт живой счётчик цветов: он считается по факту, а не
    /// инкрементом/декрементом, который «уплывал» после отрастания цветка.
    /// </summary>
    public class FlowerSpawner : MonoBehaviour
    {
        [System.Serializable]
        public class FlowerColorSet
        {
            public Color flowerColor = Color.white;
            public float spawnWeight = 1f;
        }

        [Header("Параметры спавна")]
        [SerializeField] private Vector2 spawnArea = new Vector2(40f, 40f);
        [SerializeField] private float baseFlowerCount = 30f;
        [SerializeField] private float nectarPerFlower = 10f;
        [SerializeField] private float respawnDelay = 15f;
        [SerializeField] private float regrowTime = 30f;

        [Header("Цвета цветов")]
        [SerializeField] private FlowerColorSet[] flowerColors;

        [Header("Ссылки")]
        [SerializeField] private SeasonCycle seasonCycle;

        /// <summary>Все созданные цветы, включая истощённые (они восстанавливаются).</summary>
        private readonly List<Flower> flowers = new List<Flower>();

        private float respawnTimer;

        public static FlowerSpawner Instance { get; private set; }

        /// <summary>Сколько цветов прямо сейчас дают нектар.</summary>
        public int AvailableFlowerCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < flowers.Count; i++)
                    if (flowers[i] != null && !flowers[i].IsDepleted) count++;
                return count;
            }
        }

        /// <summary>Сколько всего цветов на поляне (включая истощённые).</summary>
        public int FlowerCount => flowers.Count;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(this);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Start()
        {
            if (seasonCycle == null) seasonCycle = SeasonCycle.Instance;
            if (flowerColors == null || flowerColors.Length == 0)
                flowerColors = new FlowerColorSet[] { new FlowerColorSet { flowerColor = Color.yellow, spawnWeight = 1f } };
            SpawnInitialFlowers();
        }

        void Update()
        {
            respawnTimer += Time.deltaTime;
            if (respawnTimer < respawnDelay) return;
            respawnTimer = 0f;

            int target = Mathf.RoundToInt(GetTargetFlowerCount());
            // Досаживаем только недостаток. Истощённые цветы возвращаются сами
            // (Flower.Regrow), поэтому цветов не становится бесконечно больше:
            // общий потолок — двойная сезонная норма.
            if (AvailableFlowerCount < target && flowers.Count < target * 2)
                SpawnFlower();
        }

        float GetTargetFlowerCount()
        {
            float multiplier = seasonCycle != null ? seasonCycle.GrowthMultiplier : 1f;
            return baseFlowerCount * multiplier;
        }

        void SpawnInitialFlowers()
        {
            int count = Mathf.RoundToInt(GetTargetFlowerCount());
            for (int i = 0; i < count; i++) SpawnFlower();
            Debug.Log($"🌸 Посажено {count} цветов");
        }

        void SpawnFlower()
        {
            Vector3 pos = GetRandomPosition();
            var type = PickColorSet();

            GameObject flower = new GameObject($"Flower_{pos.x:F0}_{pos.y:F0}");
            flower.transform.position = pos;
            flower.transform.SetParent(transform);

            var sr = flower.AddComponent<SpriteRenderer>();
            sr.sprite = CreateFlowerSprite(8, type.flowerColor);
            sr.sortingOrder = 2;

            var flowerComp = flower.AddComponent<Flower>();
            flowerComp.Initialize(nectarPerFlower, pos, regrowTime);
            flowers.Add(flowerComp);
        }

        /// <summary>Выбор цвета с учётом spawnWeight (раньше вес игнорировался).</summary>
        FlowerColorSet PickColorSet()
        {
            float total = 0f;
            for (int i = 0; i < flowerColors.Length; i++)
                total += Mathf.Max(0f, flowerColors[i].spawnWeight);

            if (total <= 0f) return flowerColors[Random.Range(0, flowerColors.Length)];

            float roll = Random.value * total;
            for (int i = 0; i < flowerColors.Length; i++)
            {
                roll -= Mathf.Max(0f, flowerColors[i].spawnWeight);
                if (roll <= 0f) return flowerColors[i];
            }
            return flowerColors[flowerColors.Length - 1];
        }

        Sprite CreateFlowerSprite(int resolution, Color color)
        {
            Texture2D tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
            float center = resolution / 2f;
            float radius = center - 1f;
            for (int y = 0; y < resolution; y++)
                for (int x = 0; x < resolution; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    tex.SetPixel(x, y, d <= radius ? color : Color.clear);
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f), 16f);
        }

        Vector3 GetRandomPosition()
        {
            float x = Random.Range(-spawnArea.x / 2f, spawnArea.x / 2f);
            float y = Random.Range(-spawnArea.y / 2f, spawnArea.y / 2f);
            return new Vector3(x, y, 0f);
        }

        /// <summary>
        /// Ближайший цветок с нектаром в радиусе. Вызывается пчелой при прибытии
        /// в точку (один проход по списку на визит, не каждый кадр).
        /// </summary>
        public Flower FindNearestFlower(Vector2 from, float radius)
        {
            Flower best = null;
            float bestSqr = radius * radius;

            for (int i = 0; i < flowers.Count; i++)
            {
                var f = flowers[i];
                if (f == null || f.IsDepleted) continue;

                float sqr = ((Vector2)f.Position - from).sqrMagnitude;
                if (sqr <= bestSqr)
                {
                    bestSqr = sqr;
                    best = f;
                }
            }

            return best;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, new Vector3(spawnArea.x, spawnArea.y, 0f));
        }
    }

    /// <summary>
    /// Цветок: хранит нектар, отдаёт его пчеле и отрастает через regrowTime.
    /// </summary>
    public class Flower : MonoBehaviour
    {
        [SerializeField] private float nectarAmount = 10f;
        [SerializeField] private float regrowTime = 30f;
        [SerializeField] private bool isDepleted = false;

        private float initialNectar;
        private float regrowTimer;
        private SpriteRenderer spriteRenderer;
        private Color originalColor;

        public float NectarAmount => nectarAmount;
        public bool IsDepleted => isDepleted;
        public Vector3 Position => transform.position;
        public float RegrowTime => regrowTime;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(float nectar, Vector3 pos, float regrowSeconds = 30f)
        {
            nectarAmount = nectar;
            initialNectar = nectar > 0f ? nectar : 10f;
            regrowTime = regrowSeconds > 0f ? regrowSeconds : 30f;
            isDepleted = false;
            regrowTimer = 0f;

            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) originalColor = spriteRenderer.color;
        }

        void Update()
        {
            if (!isDepleted) return;
            regrowTimer += Time.deltaTime;
            if (regrowTimer >= regrowTime) Regrow();
        }

        /// <summary>
        /// Взять не больше amount нектара. Возвращает, сколько реально взято.
        /// </summary>
        public float Collect(float amount)
        {
            if (isDepleted || amount <= 0f) return 0f;

            float collected = Mathf.Min(amount, nectarAmount);
            nectarAmount -= collected;

            if (nectarAmount <= 0.001f)
            {
                Deplete();
            }
            else if (spriteRenderer != null)
            {
                // Чем меньше нектара, тем бледнее цветок
                spriteRenderer.color = Color.Lerp(Color.gray, originalColor,
                    Mathf.Clamp01(nectarAmount / initialNectar));
            }

            return collected;
        }

        void Deplete()
        {
            isDepleted = true;
            nectarAmount = 0f;
            transform.localScale = Vector3.zero;
        }

        void Regrow()
        {
            isDepleted = false;
            nectarAmount = initialNectar; // раньше была жёсткая 10 — ломало цветы с другим запасом
            transform.localScale = Vector3.one;
            if (spriteRenderer != null) spriteRenderer.color = originalColor;
        }
    }
}
