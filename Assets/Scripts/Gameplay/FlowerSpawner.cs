using UnityEngine;

namespace BeeSwarm.Gameplay
{
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

        [Header("Цвета цветов")]
        [SerializeField] private FlowerColorSet[] flowerColors;

        [Header("Ссылки")]
        [SerializeField] private SeasonCycle seasonCycle;

        private float currentFlowerCount;
        private float respawnTimer;

        void Start()
        {
            if (seasonCycle == null) seasonCycle = FindObjectOfType<SeasonCycle>();
            if (flowerColors == null || flowerColors.Length == 0)
                flowerColors = new FlowerColorSet[] { new FlowerColorSet { flowerColor = Color.yellow, spawnWeight = 1f } };
            SpawnInitialFlowers();
        }

        void Update()
        {
            respawnTimer += Time.deltaTime;
            if (respawnTimer >= respawnDelay)
            {
                respawnTimer = 0f;
                if (currentFlowerCount < GetTargetFlowerCount())
                { SpawnFlower(); currentFlowerCount++; }
            }
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
            currentFlowerCount = count;
            Debug.Log($"🌸 Посажено {count} цветов");
        }

        void SpawnFlower()
        {
            Vector3 pos = GetRandomPosition();
            var type = flowerColors[Random.Range(0, flowerColors.Length)];

            GameObject flower = new GameObject($"Flower_{pos.x:F0}_{pos.y:F0}");
            flower.transform.position = pos;
            flower.transform.SetParent(transform);

            var sr = flower.AddComponent<SpriteRenderer>();
            sr.sprite = CreateFlowerSprite(8, type.flowerColor);
            sr.sortingOrder = 2;

            var flowerComp = flower.AddComponent<Flower>();
            flowerComp.Initialize(nectarPerFlower, pos);
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

        public void OnFlowerDepleted() { currentFlowerCount--; }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, new Vector3(spawnArea.x, spawnArea.y, 0f));
        }
    }

    public class Flower : MonoBehaviour
    {
        [SerializeField] private float nectarAmount = 10f;
        [SerializeField] private float regrowTime = 30f;
        [SerializeField] private bool isDepleted = false;

        private Vector3 originalPosition;
        private float regrowTimer;
        private FlowerSpawner spawner;
        private SpriteRenderer spriteRenderer;
        private Color originalColor;

        public float NectarAmount => nectarAmount;
        public bool IsDepleted => isDepleted;
        public Vector3 Position => transform.position;

        void Awake() { spriteRenderer = GetComponent<SpriteRenderer>(); }

        public void Initialize(float nectar, Vector3 pos)
        {
            nectarAmount = nectar;
            originalPosition = pos;
            isDepleted = false;
            regrowTimer = 0f;
            spawner = GetComponentInParent<FlowerSpawner>() ?? FindObjectOfType<FlowerSpawner>();
            if (spriteRenderer != null) originalColor = spriteRenderer.color;
        }

        void Update()
        {
            if (isDepleted)
            {
                regrowTimer += Time.deltaTime;
                if (regrowTimer >= regrowTime) Regrow();
            }
        }

        public float Collect(float amount)
        {
            float collected = Mathf.Min(amount, nectarAmount);
            nectarAmount -= collected;
            if (nectarAmount <= 0f) Deplete();
            else if (spriteRenderer != null)
                spriteRenderer.color = Color.Lerp(Color.gray, originalColor, nectarAmount / 10f);
            return collected;
        }

        void Deplete()
        {
            isDepleted = true;
            nectarAmount = 0f;
            transform.localScale = Vector3.zero;
            spawner?.OnFlowerDepleted();
        }

        void Regrow()
        {
            isDepleted = false;
            nectarAmount = 10f;
            transform.localScale = Vector3.one;
            if (spriteRenderer != null) spriteRenderer.color = originalColor;
        }
    }
}
