using System.Collections.Generic;
using UnityEngine;

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
        [SerializeField] private Vector3 hiveBounds = new Vector3(10f, 5f, 10f);
        
        [Header("Ресурсы")]
        [SerializeField] private float honeyAmount = 100f;
        [SerializeField] private float pollenAmount = 50f;
        [SerializeField] private float waxAmount = 30f;
        
        [Header("Ссылки")]
        [SerializeField] private GameObject beePrefab;
        [SerializeField] private Transform beeSpawnPoint;
        
        // Коллекции
        private List<BeeController> allBees = new List<BeeController>();
        private Queue<BeeController> beePool = new Queue<BeeController>();
        
        // Свойства
        public int BeeCount => allBees.Count;
        public int ActiveBeeCount => allBees.Count - beePool.Count;
        public float HoneyAmount => honeyAmount;
        public float PollenAmount => pollenAmount;
        public float WaxAmount => waxAmount;
        public Vector3 HiveCenter => transform.position;
        public Transform HiveEntrance => hiveEntrance;
        
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
                if (allBees.Count >= maxBees)
                {
                    Debug.LogWarning("Достигнут лимит пчёл!");
                    return null;
                }
                CreatePoolBee();
            }
            bee = beePool.Dequeue();
            GameObject beeObj = bee.gameObject;
            
            // Установить позицию
            Vector3 spawnPos = beeSpawnPoint.position + Random.insideUnitSphere * 2f;
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
        public Vector3 GetRandomHivePosition()
        {
            Vector3 halfBounds = hiveBounds * 0.5f;
            Vector3 randomOffset = new Vector3(
                Random.Range(-halfBounds.x, halfBounds.x),
                Random.Range(-halfBounds.y, halfBounds.y),
                Random.Range(-halfBounds.z, halfBounds.z)
            );
            
            return transform.position + randomOffset;
        }
        
        /// <summary>
        /// Проверить, находится ли точка внутри улья
        /// </summary>
        public bool IsInsideHive(Vector3 position)
        {
            Vector3 localPos = transform.InverseTransformPoint(position);
            Vector3 halfBounds = hiveBounds * 0.5f;
            
            return Mathf.Abs(localPos.x) <= halfBounds.x &&
                   Mathf.Abs(localPos.y) <= halfBounds.y &&
                   Mathf.Abs(localPos.z) <= halfBounds.z;
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
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label($"ПЧЁЛЫ: {ActiveBeeCount}/{maxBees}");
            GUILayout.Label($"МЁД: {honeyAmount:F1}");
            GUILayout.Label($"ПЫЛЬЦА: {pollenAmount:F1}");
            GUILayout.Label($"ВОСК: {waxAmount:F1}");
            
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
