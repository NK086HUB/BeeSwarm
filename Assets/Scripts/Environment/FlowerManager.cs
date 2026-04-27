using UnityEngine;
using System.Collections.Generic;
using BeeSwarm.Core;

namespace BeeSwarm.Environment
{
    /// <summary>
    /// Менеджер цветов — спавнит, уничтожает, управляет регенерацией
    /// </summary>
    public class FlowerManager : MonoBehaviour
    {
        [Header("Настройки")]
        [SerializeField] private GameObject flowerPrefab;
        [SerializeField] private int initialFlowerCount = 30;
        [SerializeField] private int maxFlowers = 100;
        [SerializeField] private Vector2 spawnArea = new Vector2(100f, 100f);
        [SerializeField] private float minFlowerDistance = 5f;
        [SerializeField] private LayerMask groundLayer = ~0;
        
        [Header("Весна/Лето/Осень")]
        [SerializeField] private AnimationCurve seasonalDensity = new AnimationCurve(
            new Keyframe(0f, 0.3f),   // Зима: 30%
            new Keyframe(0.25f, 0.7f), // Весна: 70%
            new Keyframe(0.5f, 1.0f),  // Лето: 100%
            new Keyframe(0.75f, 0.6f), // Осень: 60%
            new Keyframe(1f, 0.3f)     // Зима: 30%
        );
        
        private List<FlowerController> allFlowers = new List<FlowerController>();
        
        public List<FlowerController> AllFlowers => allFlowers;
        public int ActiveFlowerCount => allFlowers.Count;
        
        void Start()
        {
            SpawnInitialFlowers();
        }
        
        private void SpawnInitialFlowers()
        {
            for (int i = 0; i < initialFlowerCount; i++)
            {
                SpawnFlower();
            }
        }
        
        /// <summary>
        /// Создать новый цветок
        /// </summary>
        public FlowerController SpawnFlower()
        {
            if (allFlowers.Count >= maxFlowers || flowerPrefab == null)
                return null;
            
            Vector3 spawnPos = FindSpawnPosition();
            if (spawnPos == Vector3.zero)
                return null;
            
            GameObject flowerObj = Instantiate(flowerPrefab, spawnPos, Quaternion.identity, transform);
            FlowerController flower = flowerObj.GetComponent<FlowerController>();
            
            if (flower != null)
            {
                allFlowers.Add(flower);
            }
            
            return flower;
        }
        
        /// <summary>
        /// Найти позицию для спавна (не слишком близко к другим цветам)
        /// </summary>
        private Vector3 FindSpawnPosition()
        {
            for (int attempt = 0; attempt < 20; attempt++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-spawnArea.x, spawnArea.x),
                    0f,
                    Random.Range(-spawnArea.y, spawnArea.y)
                );
                
                // Проверяем расстояние до других цветов
                bool tooClose = false;
                foreach (var flower in allFlowers)
                {
                    if (Vector3.Distance(pos, flower.transform.position) < minFlowerDistance)
                    {
                        tooClose = true;
                        break;
                    }
                }
                
                if (!tooClose)
                {
                    // Проверяем, что на земле
                    if (Physics.Raycast(pos + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, groundLayer))
                    {
                        return hit.point;
                    }
                    return pos;
                }
            }
            
            return Vector3.zero;
        }
        
        /// <summary>
        /// Удалить цветок
        /// </summary>
        public void RemoveFlower(FlowerController flower)
        {
            if (flower != null && allFlowers.Contains(flower))
            {
                allFlowers.Remove(flower);
                Destroy(flower.gameObject);
            }
        }
        
        /// <summary>
        /// Обновить количество цветов по сезону
        /// </summary>
        public void UpdateSeasonalFlowers(float seasonProgress) // 0-1
        {
            float density = seasonalDensity.Evaluate(seasonProgress);
            int targetCount = Mathf.RoundToInt(maxFlowers * density);
            
            // Удаляем лишние
            while (allFlowers.Count > targetCount)
            {
                RemoveFlower(allFlowers[allFlowers.Count - 1]);
            }
            
            // Добавляем недостающие
            while (allFlowers.Count < targetCount)
            {
                SpawnFlower();
            }
        }
        
        /// <summary>
        /// Найти цветок по типу мёда
        /// </summary>
        public List<FlowerController> FindFlowersByHoneyType(FlowerController.HoneyType type)
        {
            List<FlowerController> result = new List<FlowerController>();
            foreach (var flower in allFlowers)
            {
                if (flower.HoneyType == type && flower.HasNectar)
                {
                    result.Add(flower);
                }
            }
            return result;
        }
        
        void OnDrawGizmosSelected()
        {
            // Область спавна
            Gizmos.color = new Color(0f, 1f, 0f, 0.1f);
            Vector3 center = transform.position;
            Gizmos.DrawCube(center, new Vector3(spawnArea.x * 2f, 1f, spawnArea.y * 2f));
        }
    }
}
