using UnityEngine;

namespace BeeSwarm.Gameplay
{
    /// <summary>
    /// Спавн цветов на сцене с учётом сезона и плотности.
    /// Каждый цветок — источник нектара для пчёл.
    /// </summary>
    public class FlowerSpawner : MonoBehaviour
    {
        [System.Serializable]
        public class FlowerPrefabSet
        {
            public GameObject flowerPrefab;
            public float spawnWeight = 1f;
            public Color flowerColor = Color.white;
        }

        [Header("Параметры спавна")]
        [SerializeField] private Vector2 spawnArea = new Vector2(40f, 40f);
        [SerializeField] private float baseFlowerCount = 30f;
        [SerializeField] private float nectarPerFlower = 10f;
        [SerializeField] private float respawnDelay = 15f;

        [Header("Префабы цветов")]
        [SerializeField] private FlowerPrefabSet[] flowerTypes;

        [Header("Ссылки")]
        [SerializeField] private SeasonCycle seasonCycle;

        private float currentFlowerCount;
        private float respawnTimer;

        void Start()
        {
            if (seasonCycle == null)
                seasonCycle = FindObjectOfType<SeasonCycle>();

            if (flowerTypes == null || flowerTypes.Length == 0)
            {
                // Создаём заглушку, если нет префабов
                flowerTypes = new FlowerPrefabSet[] {
                    new FlowerPrefabSet { flowerPrefab = null, spawnWeight = 1f, flowerColor = new Color(1f, 0.8f, 0f) }
                };
            }

            SpawnInitialFlowers();
        }

        void Update()
        {
            // Респавн по таймеру
            respawnTimer += Time.deltaTime;
            if (respawnTimer >= respawnDelay)
            {
                respawnTimer = 0f;
                if (currentFlowerCount < GetTargetFlowerCount())
                {
                    RespawnFlower();
                }
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
            for (int i = 0; i < count; i++)
            {
                SpawnFlower();
            }
            currentFlowerCount = count;
            Debug.Log($"🌸 Посажено {count} цветов");
        }

        void SpawnFlower()
        {
            Vector3 pos = GetRandomPosition();

            if (flowerTypes[0].flowerPrefab != null)
            {
                var type = flowerTypes[Random.Range(0, flowerTypes.Length)];
                GameObject flower = Instantiate(type.flowerPrefab, pos, Quaternion.identity, transform);
                flower.name = $"Flower_{Random.Range(1000, 9999)}";

                var flowerComp = flower.GetComponent<Flower>();
                if (flowerComp == null) flowerComp = flower.AddComponent<Flower>();
                flowerComp.Initialize(nectarPerFlower, pos);
            }
            else
            {
                // Заглушка — создаём спрайтовый цветок без префаба
                GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                placeholder.transform.position = pos;
                placeholder.transform.localScale = Vector3.one * 0.6f;
                placeholder.transform.SetParent(transform);

                // Убираем коллайдер, ставим жёлтый/розовый цвет
                DestroyImmediate(placeholder.GetComponent<Collider>());

                var renderer = placeholder.GetComponent<MeshRenderer>();
                var type = flowerTypes[Random.Range(0, flowerTypes.Length)];
                renderer.material.color = type.flowerColor;

                var flowerComp = placeholder.AddComponent<Flower>();
                flowerComp.Initialize(nectarPerFlower, pos);

                placeholder.name = $"Flower_{pos.x:F0}_{pos.z:F0}";

                // Добавляем стебель (цилиндр)
                GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stem.transform.position = pos + Vector3.down * 1f;
                stem.transform.localScale = new Vector3(0.1f, 1f, 0.1f);
                stem.transform.SetParent(placeholder.transform);
                DestroyImmediate(stem.GetComponent<Collider>());
                stem.GetComponent<MeshRenderer>().material.color = Color.green;
                stem.name = "Stem";
            }
        }

        void RespawnFlower()
        {
            SpawnFlower();
            currentFlowerCount++;
        }

        Vector3 GetRandomPosition()
        {
            float x = Random.Range(-spawnArea.x / 2f, spawnArea.x / 2f);
            float z = Random.Range(-spawnArea.y / 2f, spawnArea.y / 2f);
            return new Vector3(x, 0f, z);
        }

        public void OnFlowerDepleted()
        {
            currentFlowerCount--;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, new Vector3(spawnArea.x, 0.5f, spawnArea.y));
        }
    }

    /// <summary>
    /// Отдельный цветок с нектаром
    /// </summary>
    public class Flower : MonoBehaviour
    {
        [SerializeField] private float nectarAmount = 10f;
        [SerializeField] private float regrowTime = 30f;
        [SerializeField] private bool isDepleted = false;

        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private float regrowTimer;
        private FlowerSpawner spawner;
        private MeshRenderer meshRenderer;
        private Color originalColor;

        public float NectarAmount => nectarAmount;
        public bool IsDepleted => isDepleted;
        public Vector3 Position => transform.position;

        void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        public void Initialize(float nectar, Vector3 pos)
        {
            nectarAmount = nectar;
            originalPosition = pos;
            originalRotation = transform.rotation;
            isDepleted = false;
            regrowTimer = 0f;

            spawner = GetComponentInParent<FlowerSpawner>();
            if (spawner == null) spawner = FindObjectOfType<FlowerSpawner>();

            if (meshRenderer != null)
                originalColor = meshRenderer.material.color;
        }

        void Update()
        {
            if (isDepleted)
            {
                regrowTimer += Time.deltaTime;
                if (regrowTimer >= regrowTime)
                {
                    Regrow();
                }
            }
        }

        /// <summary>Собрать нектар. Возвращает сколько собрано.</summary>
        public float Collect(float amount)
        {
            float collected = Mathf.Min(amount, nectarAmount);
            nectarAmount -= collected;

            if (nectarAmount <= 0f)
            {
                Deplete();
            }
            else
            {
                // Визуальный фидбек — бледнеет
                if (meshRenderer != null)
                {
                    float t = nectarAmount / 10f;
                    meshRenderer.material.color = Color.Lerp(Color.gray, originalColor, t);
                }
            }

            return collected;
        }

        void Deplete()
        {
            isDepleted = true;
            nectarAmount = 0f;

            // Прячем цветок
            transform.localScale = Vector3.zero;
            spawner?.OnFlowerDepleted();
        }

        void Regrow()
        {
            isDepleted = false;
            nectarAmount = 10f;
            transform.localScale = Vector3.one * 0.6f;

            if (meshRenderer != null)
                meshRenderer.material.color = originalColor;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = isDepleted ? Color.gray : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.5f);

            if (!isDepleted)
            {
                Gizmos.color = Color.Lerp(Color.red, Color.green, nectarAmount / 10f);
                Gizmos.DrawRay(transform.position, Vector3.up * nectarAmount * 0.1f);
            }
        }
    }
}
