using UnityEngine;
using System.Collections.Generic;

namespace BeeSwarm.Environment
{
    /// <summary>
    /// Менеджер цветов (2D) — спавн, сезонная плотность, поиск
    /// </summary>
    public class FlowerManager : MonoBehaviour
    {
        [Header("Настройки")]
        [SerializeField] private GameObject flowerPrefab;
        [SerializeField] private int initialFlowerCount = 30;
        [SerializeField] private int maxFlowers = 100;
        [SerializeField] private Vector2 spawnAreaSize = new Vector2(100f, 100f);
        [SerializeField] private float minFlowerDistance = 4f;

        [Header("Сезонность")]
        [SerializeField] private AnimationCurve seasonalDensity = new AnimationCurve(
            new Keyframe(0f, 0.3f),
            new Keyframe(0.25f, 0.7f),
            new Keyframe(0.5f, 1.0f),
            new Keyframe(0.75f, 0.6f),
            new Keyframe(1f, 0.3f)
        );

        [Header("Слои (2D)")]
        [SerializeField] private LayerMask groundLayer = 1;
        [SerializeField] private LayerMask flowerLayer = 1;

        private List<FlowerController> allFlowers = new List<FlowerController>();
        public List<FlowerController> AllFlowers => allFlowers;
        public int ActiveFlowerCount => allFlowers.Count;

        void Start() => SpawnInitialFlowers();

        private void SpawnInitialFlowers()
        {
            for (int i = 0; i < initialFlowerCount; i++)
                SpawnFlower();
        }

        public FlowerController SpawnFlower()
        {
            if (allFlowers.Count >= maxFlowers || flowerPrefab == null) return null;

            Vector2 pos = FindSpawnPosition();
            if (pos == Vector2.zero) return null;

            // Находим поверхность (Raycast2D)
            RaycastHit2D hit = Physics2D.Raycast(pos + Vector2.up * 10f, Vector2.down, 20f, groundLayer);
            Vector3 spawnPos = hit.collider != null ? (Vector3)hit.point : (Vector3)pos;
            spawnPos.z = 0f;

            GameObject obj = Instantiate(flowerPrefab, spawnPos, Quaternion.identity, transform);
            var flower = obj.GetComponent<FlowerController>();
            if (flower != null) allFlowers.Add(flower);
            return flower;
        }

        private Vector2 FindSpawnPosition()
        {
            for (int i = 0; i < 20; i++)
            {
                Vector2 pos = new Vector2(
                    Random.Range(-spawnAreaSize.x, spawnAreaSize.x),
                    Random.Range(-spawnAreaSize.y, spawnAreaSize.y)
                );

                bool tooClose = false;
                foreach (var f in allFlowers)
                {
                    if (Vector2.Distance(pos, f.transform.position) < minFlowerDistance)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose) return pos;
            }
            return Vector2.zero;
        }

        public void RemoveFlower(FlowerController flower)
        {
            if (flower != null && allFlowers.Remove(flower))
                Destroy(flower.gameObject);
        }

        /// <summary>
        /// Адаптировать количество цветов под сезон (0–1)
        /// </summary>
        public void UpdateSeasonalFlowers(float seasonProgress)
        {
            int target = Mathf.RoundToInt(maxFlowers * seasonalDensity.Evaluate(seasonProgress));

            while (allFlowers.Count > target)
                RemoveFlower(allFlowers[^1]);

            while (allFlowers.Count < target)
                SpawnFlower();
        }

        public List<FlowerController> FindFlowersByType(FlowerController.HoneyType type)
        {
            var result = new List<FlowerController>();
            foreach (var f in allFlowers)
                if (f.HoneyType == type && f.HasNectar)
                    result.Add(f);
            return result;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.1f);
            Gizmos.DrawCube(transform.position, new Vector3(spawnAreaSize.x * 2f, spawnAreaSize.y * 2f, 1f));
        }
    }
}
