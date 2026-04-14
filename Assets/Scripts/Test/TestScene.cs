using UnityEngine;

namespace BeeSwarm.Test
{
    /// <summary>
    /// Тестовая сцена для проверки работы проекта
    /// </summary>
    public class TestScene : MonoBehaviour
    {
        [Header("Тестовые настройки")]
        [SerializeField] private bool autoSpawnBees = true;
        [SerializeField] private int beesToSpawn = 5;
        [SerializeField] private float spawnInterval = 2f;
        
        [Header("Тестовые цели")]
        [SerializeField] private Transform[] testTargets;
        [SerializeField] private bool moveBeesToTargets = true;
        
        private float spawnTimer;
        private int spawnedCount;
        
        void Start()
        {
            Debug.Log("🐝 Тестовая сцена BeeSwarm запущена!");
            Debug.Log($"Версия Unity: {Application.unityVersion}");
            Debug.Log($"Платформа: {Application.platform}");
            
            // Проверка менеджеров
            CheckManagers();
        }
        
        void Update()
        {
            // Автоматическое создание пчёл
            if (autoSpawnBees && spawnedCount < beesToSpawn)
            {
                spawnTimer += Time.deltaTime;
                if (spawnTimer >= spawnInterval)
                {
                    SpawnTestBee();
                    spawnTimer = 0f;
                    spawnedCount++;
                }
            }
            
            // Тест управления пчёлами
            if (moveBeesToTargets && testTargets.Length > 0)
            {
                TestBeeMovement();
            }
        }
        
        void CheckManagers()
        {
            // Проверка GameManager
            if (GameManager.Instance == null)
            {
                Debug.LogError("❌ GameManager не найден!");
            }
            else
            {
                Debug.Log("✅ GameManager работает");
            }
            
            // Проверка HiveManager
            if (HiveManager.Instance == null)
            {
                Debug.LogError("❌ HiveManager не найден!");
            }
            else
            {
                Debug.Log($"✅ HiveManager работает: {HiveManager.Instance.BeeCount} пчёл");
            }
        }
        
        void SpawnTestBee()
        {
            if (HiveManager.Instance == null)
            {
                Debug.LogError("Не могу создать пчелу: HiveManager не найден");
                return;
            }
            
            var bee = HiveManager.Instance.SpawnBee();
            if (bee != null)
            {
                Debug.Log($"✅ Создана тестовая пчела #{spawnedCount + 1}");
                
                // Дать пчеле случайную цель
                if (testTargets.Length > 0)
                {
                    Transform target = testTargets[Random.Range(0, testTargets.Length)];
                    bee.SetTarget(target.position);
                }
            }
            else
            {
                Debug.LogWarning("⚠️ Не удалось создать пчелу (возможно, лимит)");
            }
        }
        
        void TestBeeMovement()
        {
            if (HiveManager.Instance == null) return;
            
            var activeBees = HiveManager.Instance.GetAllActiveBees();
            if (activeBees.Count == 0) return;
            
            // Каждые 5 секунд меняем цели у случайных пчёл
            if (Time.time % 5f < Time.deltaTime)
            {
                foreach (var bee in activeBees)
                {
                    if (Random.value > 0.3f) continue; // 30% шанс сменить цель
                    
                    Transform target = testTargets[Random.Range(0, testTargets.Length)];
                    bee.SetTarget(target.position);
                }
            }
        }
        
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 220, 300, 200));
            
            GUILayout.Label("🧪 ТЕСТОВАЯ СЦЕНА");
            GUILayout.Label($"Создано пчёл: {spawnedCount}/{beesToSpawn}");
            GUILayout.Label($"Активных пчёл: {HiveManager.Instance?.ActiveBeeCount ?? 0}");
            
            if (GUILayout.Button("Создать пчелу"))
            {
                SpawnTestBee();
                spawnedCount++;
            }
            
            if (GUILayout.Button("Очистить всех пчёл"))
            {
                ClearAllBees();
            }
            
            GUILayout.Label("Управление:");
            if (GUILayout.Button("Пауза/Продолжить"))
            {
                GameManager.Instance?.TogglePause();
            }
            
            GUILayout.EndArea();
        }
        
        void ClearAllBees()
        {
            if (HiveManager.Instance == null) return;
            
            var activeBees = HiveManager.Instance.GetAllActiveBees();
            foreach (var bee in activeBees)
            {
                HiveManager.Instance.ReturnBee(bee);
            }
            
            spawnedCount = 0;
            Debug.Log("🧹 Все пчёлы очищены");
        }
        
        void OnDrawGizmos()
        {
            // Визуализация тестовых целей
            if (testTargets != null)
            {
                Gizmos.color = Color.green;
                foreach (var target in testTargets)
                {
                    if (target != null)
                    {
                        Gizmos.DrawWireSphere(target.position, 1f);
                        Gizmos.DrawLine(target.position, target.position + Vector3.up * 2f);
                    }
                }
            }
        }
    }
}