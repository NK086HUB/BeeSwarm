using System.Collections.Generic;
using UnityEngine;
using BeeSwarm.Gameplay;

namespace BeeSwarm.Core
{
    /// <summary>
    /// Менеджер улья - центральная система управления
    /// </summary>
    public class HiveManager : MonoBehaviour
    {
        [Header("Настройки улья")]
        [SerializeField] private int maxBees = 50;
        [SerializeField] private Transform hiveEntrance;
        [SerializeField] private Vector2 hiveBounds = new Vector2(10f, 5f);
        
        [Header("Ресурсы")]
        [SerializeField] private float honeyAmount = 100f;
        [SerializeField] private float pollenAmount = 50f;
        [SerializeField] private float waxAmount = 30f;
        
        [Header("Зимняя спячка")]
        [SerializeField] private float winterHeatingPerSecond = 0.5f;   // мёд на обогрев улья
        [SerializeField] private bool winterStarvation = true;           // гибнут ли пчёлы без мёда
        [SerializeField] private float starvationInterval = 10f;         // как часто теряем пчелу без мёда

        [Header("Расплод")]
        [SerializeField] private float broodInterval = 15f;              // как часто рой выводит пчелу (не зимой)
        [SerializeField] private float broodCost = 10f;                  // мёд за одну выведенную пчелу

        [Header("Ссылки")]
        [SerializeField] private GameObject beePrefab;
        [SerializeField] private Transform beeSpawnPoint;
        
        // Коллекции
        private List<BeeController> allBees = new List<BeeController>();
        private Queue<BeeController> beePool = new Queue<BeeController>();

        private float starvationTimer;
        private float broodTimer;
        private float winterHoneySpent;
        private int winterLosses;
        
        // Свойства
        public int BeeCount => allBees.Count;
        // allBees хранит только активных пчёл (ReturnBee убирает пчелу из списка),
        // поэтому вычитать пул второй раз нельзя — иначе счётчик занижался.
        public int ActiveBeeCount => allBees.Count;
        public float HoneyAmount => honeyAmount;
        public float PollenAmount => pollenAmount;
        public float WaxAmount => waxAmount;
        public Vector3 HiveCenter => transform.position;
        public Transform HiveEntrance => hiveEntrance;

        /// <summary>Сколько мёда съела текущая зима (обогрев).</summary>
        public float WinterHoneySpent => winterHoneySpent;

        /// <summary>Сколько пчёл рой потерял зимой из-за нехватки мёда.</summary>
        public int WinterLosses => winterLosses;

        /// <summary>Идёт ли зимняя спячка — источник истины SeasonCycle.</summary>
        private bool IsWinter
        {
            get
            {
                var season = SeasonCycle.Instance;
                return season != null && season.IsWinterHibernation;
            }
        }
        
        // Синглтон
        public static HiveManager Instance { get; private set; }
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void Start()
        {
            InitializeBeePool();
            SpawnInitialBees(10); // Начальные 10 пчёл

            var season = SeasonCycle.Instance;
            if (season != null) season.OnSeasonChanged += OnSeasonChanged;
        }

        void OnDestroy()
        {
            var season = SeasonCycle.Instance;
            if (season != null) season.OnSeasonChanged -= OnSeasonChanged;
        }

        /// <summary>
        /// Смена сезона для улья: весной рой выходит из спячки и начинает расплод.
        /// Без этого зимние потери были необратимыми — LoseBeeToWinter() возвращал пчёл
        /// в пул «до весны», а весеннего пробуждения не существовало: рой умел только
        /// уменьшаться и к 2-3-й зиме вымирал.
        /// </summary>
        private void OnSeasonChanged(SeasonCycle.Season season)
        {
            broodTimer = 0f;

            if (season != SeasonCycle.Season.Spring) return;

            int toWake = beePool.Count;
            for (int i = 0; i < toWake && SpawnBee() != null; i++) { }
            winterLosses = 0;
            Debug.Log($"🌱 Весеннее пробуждение: активных {ActiveBeeCount}");
        }

        /// <summary>
        /// Расплод: не зимой рой выводит пчёл, пока хватает мёда. Это противовес
        /// зимнему расходу — колония растёт, а не только тратит запасы.
        /// </summary>
        private void UpdateBrood()
        {
            if (honeyAmount < broodCost)
            {
                broodTimer = 0f;
                return;
            }

            broodTimer += Time.deltaTime;
            if (broodTimer < broodInterval) return;

            broodTimer = 0f;
            if (SpawnBee() == null) return; // лимит улья — мёд не тратим

            honeyAmount -= broodCost;
            Debug.Log($"🍼 Расплод: +1 пчела (мёд {honeyAmount:F1}, активных {ActiveBeeCount})");
        }

        /// <summary>
        /// Зима стоит мёда: улей жгут на обогрев. Без этого запасы копить незачем,
        /// а зимняя спячка была бы просто паузой. Когда мёд кончился — обогревать
        /// нечем, и рой начинает терять пчёл.
        /// </summary>
        void Update()
        {
            if (!IsWinter)
            {
                starvationTimer = 0f;
                UpdateBrood();
                return;
            }

            float wanted = winterHeatingPerSecond * Time.deltaTime;
            float spent = Mathf.Min(honeyAmount, wanted);
            honeyAmount -= spent;
            winterHoneySpent += spent;

            if (honeyAmount > 0f || !winterStarvation)
            {
                starvationTimer = 0f;
                return;
            }

            starvationTimer += Time.deltaTime;
            if (starvationTimer < starvationInterval) return;
            starvationTimer = 0f;
            LoseBeeToWinter();
        }

        /// <summary>Пчела не пережила зиму: возвращается в пул (рой восполнит весной).</summary>
        private void LoseBeeToWinter()
        {
            var bees = GetAllActiveBees();
            if (bees.Count == 0) return;

            winterLosses++;
            ReturnBee(bees[bees.Count - 1]);
            Debug.LogWarning($"❄️ Зимняя потеря: мёд кончился, рой без обогрева (потерь всего: {winterLosses})");
        }
        
        /// <summary>
        /// Инициализация пула пчёл
        /// </summary>
        private void InitializeBeePool()
        {
            // Start with small pool, grow on demand
            for (int i = 0; i < 10; i++)
            {
                CreatePoolBee();
            }
        }

        private void CreatePoolBee()
        {
            GameObject beeObj;
            if (beePrefab != null)
                beeObj = Instantiate(beePrefab, beeSpawnPoint.position, Quaternion.identity);
            else
                beeObj = CreateDefaultBee();

            BeeController bee = beeObj.GetComponent<BeeController>();
            beeObj.SetActive(false);
            beePool.Enqueue(bee);
        }
        
        /// <summary>
        /// Создать начальных пчёл
        /// </summary>
        private void SpawnInitialBees(int count)
        {
            for (int i = 0; i < count; i++)
            {
                SpawnBee();
            }
        }
        
        /// <summary>
        /// Создать новую пчелу
        /// </summary>
        public BeeController SpawnBee()
        {
            // Взять пчелу из пула или создать новую
            BeeController bee;
            if (beePool.Count == 0)
            {
                // Лимит считаем по всей колонии: ReturnBee() переносит пчелу из allBees
                // в пул, поэтому проверка только по allBees позволяла обойти maxBees.
                if (allBees.Count + beePool.Count >= maxBees)
                {
                    Debug.LogWarning("Достигнут лимит пчёл!");
                    return null;
                }
                CreatePoolBee();
            }
            bee = beePool.Dequeue();
            GameObject beeObj = bee.gameObject;
            
            // Установить позицию
            Vector3 spawnPos = beeSpawnPoint.position + (Vector3)(Random.insideUnitCircle * 2f);
            beeObj.transform.position = spawnPos;
            beeObj.transform.rotation = Quaternion.identity;
            
            // Активировать
            beeObj.SetActive(true);
            allBees.Add(bee);
            
            // Сбросить энергию
            bee.RestoreEnergy(100f);
            
            return bee;
        }
        
        /// <summary>
        /// Вернуть пчелу в пул
        /// </summary>
        public void ReturnBee(BeeController bee)
        {
            if (bee == null) return;
            
            bee.gameObject.SetActive(false);
            bee.transform.position = beeSpawnPoint.position;
            
            if (!beePool.Contains(bee))
            {
                beePool.Enqueue(bee);
            }
            
            allBees.Remove(bee);
        }
        
        /// <summary>
        /// Получить случайную позицию внутри улья
        /// </summary>
        public Vector2 GetRandomHivePosition()
        {
            Vector2 halfBounds = hiveBounds * 0.5f;
            Vector2 randomOffset = new Vector2(
                Random.Range(-halfBounds.x, halfBounds.x),
                Random.Range(-halfBounds.y, halfBounds.y)
            );
            
            return (Vector2)transform.position + randomOffset;
        }
        
        /// <summary>
        /// Проверить, находится ли точка внутри улья
        /// </summary>
        public bool IsInsideHive(Vector2 position)
        {
            Vector2 localPos = (Vector2)transform.InverseTransformPoint(position);
            Vector2 halfBounds = hiveBounds * 0.5f;
            
            return Mathf.Abs(localPos.x) <= halfBounds.x &&
                   Mathf.Abs(localPos.y) <= halfBounds.y;
        }
        
        /// <summary>
        /// Добавить ресурсы в улей
        /// </summary>
        public void AddResources(float honey, float pollen, float wax)
        {
            honeyAmount += honey;
            pollenAmount += pollen;
            waxAmount += wax;
            
            honeyAmount = Mathf.Max(0f, honeyAmount);
            pollenAmount = Mathf.Max(0f, pollenAmount);
            waxAmount = Mathf.Max(0f, waxAmount);
        }
        
        /// <summary>
        /// Потратить ресурсы
        /// </summary>
        public bool ConsumeResources(float honey, float pollen, float wax)
        {
            if (honeyAmount >= honey && pollenAmount >= pollen && waxAmount >= wax)
            {
                honeyAmount -= honey;
                pollenAmount -= pollen;
                waxAmount -= wax;
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Получить всех активных пчёл
        /// </summary>
        public List<BeeController> GetAllActiveBees()
        {
            List<BeeController> activeBees = new List<BeeController>();
            
            foreach (var bee in allBees)
            {
                if (bee.gameObject.activeInHierarchy)
                {
                    activeBees.Add(bee);
                }
            }
            
            return activeBees;
        }
        
        /// <summary>
        /// Найти ближайшую пчелу к точке
        /// </summary>
        public BeeController FindNearestBee(Vector3 position, float maxDistance = Mathf.Infinity)
        {
            BeeController nearest = null;
            float nearestDistance = maxDistance;
            
            foreach (var bee in allBees)
            {
                if (!bee.gameObject.activeInHierarchy) continue;
                
                float distance = Vector3.Distance(bee.transform.position, position);
                if (distance < nearestDistance)
                {
                    nearest = bee;
                    nearestDistance = distance;
                }
            }
            
            return nearest;
        }
        
        void OnDrawGizmosSelected()
        {
            // Визуализация границ улья
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawCube(transform.position, hiveBounds);
            
            // Вход в улей
            if (hiveEntrance != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(hiveEntrance.position, 0.5f);
            }
        }
        
        GameObject CreateDefaultBee()
        {
            // 2D bee with sprite
            GameObject beeObj = new GameObject("Bee");
            beeObj.transform.localScale = Vector3.one * 0.5f;
            
            // Add sprite renderer with a simple yellow circle texture
            var sr = beeObj.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite(16, Color.yellow);
            sr.sortingOrder = 1;
            
            // Rigidbody2D
            var rb2d = beeObj.AddComponent<Rigidbody2D>();
            rb2d.gravityScale = 0f;
            rb2d.linearDamping = 0.5f;
            
            beeObj.AddComponent<BeeController>();
            beeObj.AddComponent<BeeMemory>();
            beeObj.transform.position = beeSpawnPoint.position;
            beeObj.name = "DefaultBee";
            return beeObj;
        }
        
        private Sprite CreateCircleSprite(int resolution, Color color)
        {
            Texture2D tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
            float center = resolution / 2f;
            float radius = center - 1f;
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    tex.SetPixel(x, y, dist <= radius ? color : Color.clear);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f), 16f);
        }
        
        void OnGUI()
        {
            // Простой UI для отладки
            GUILayout.BeginArea(new Rect(10, 10, 300, 240));
            GUILayout.Label($"ПЧЁЛЫ: {ActiveBeeCount}/{maxBees}");
            GUILayout.Label($"МЁД: {honeyAmount:F1}");
            GUILayout.Label($"ПЫЛЬЦА: {pollenAmount:F1}");
            GUILayout.Label($"ВОСК: {waxAmount:F1}");

            if (IsWinter)
                GUILayout.Label($"❄️ ЗИМА: обогрев -{winterHeatingPerSecond:F1} мёд/с | съедено {winterHoneySpent:F1} | потерь {winterLosses}");
            
            if (GUILayout.Button("Добавить пчелу"))
            {
                SpawnBee();
            }
            
            if (GUILayout.Button("Убрать пчелу") && ActiveBeeCount > 0)
            {
                var bee = GetAllActiveBees()[0];
                ReturnBee(bee);
            }
            
            GUILayout.EndArea();
        }
    }
}
