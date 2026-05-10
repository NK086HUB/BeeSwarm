using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeeSwarm.Core
{
    /// <summary>
    /// База знаний роя — агрегирует и хранит знания от всех пчёл.
    /// Подвешивается на HiveManager.
    /// 
    /// Пчёлы делятся знаниями при возвращении в улей (аналог танца).
    /// База объединяет данные, отбрасывает устаревшее и выдаёт
    /// лучшие цели по запросу.
    /// </summary>
    public class HiveKnowledgeBase : MonoBehaviour
    {
        [Header("Параметры базы знаний")]
        [SerializeField] private int maxFlowerRecords = 100;
        [SerializeField] private int maxDangerRecords = 50;
        [SerializeField] private float knowledgeDecayTime = 300f; // 5 минут — полное устаревание
        [SerializeField] private float mergeRadius = 3f;          // радиус слияния записей
        [SerializeField] private bool debugLog = false;

        // Общая память роя
        private List<HiveFlowerRecord> knownFlowers = new List<HiveFlowerRecord>();
        private List<HiveDangerRecord> knownDangers = new List<HiveDangerRecord>();

        // Статистика
        private int totalSharesReceived;
        private int totalSharesMerged;

        /// <summary>
        /// Запись о цветке в базе знаний улья
        /// </summary>
        [System.Serializable]
        public class HiveFlowerRecord
        {
            public Vector3 position;
            public float nectarYield;
            public float confidence;         // агрегированная уверенность
            public int reportCount;          // сколько пчёл подтвердило
            public float lastReportedTime;
            public string reportedBy;        // для отладки

            public float Score => nectarYield * confidence;
            public bool IsFresh => Time.time - lastReportedTime < knowledgeDecayTime;
        }

        /// <summary>
        /// Запись об опасности в базе знаний улья
        /// </summary>
        [System.Serializable]
        public class HiveDangerRecord
        {
            public Vector3 position;
            public float threatLevel;
            public float confidence;
            public int reportCount;
            public float lastReportedTime;
            public string dangerType;

            public bool IsFresh => Time.time - lastReportedTime < knowledgeDecayTime;
        }

        void Update()
        {
            // Периодическая очистка устаревших знаний
            CleanupOldKnowledge();
        }

        // ======================== ПРИЁМ ЗНАНИЙ ========================

        /// <summary>
        /// Получить пакет знаний от пчелы
        /// </summary>
        public void ReceiveKnowledge(BeeMemory.KnowledgePacket packet)
        {
            totalSharesReceived++;

            foreach (var flower in packet.flowers)
            {
                MergeFlower(flower);
            }

            foreach (var danger in packet.dangers)
            {
                MergeDanger(danger);
            }
        }

        /// <summary>
        /// Слияние информации о цветке с существующей записью
        /// </summary>
        private void MergeFlower(BeeMemory.FlowerMemory incoming)
        {
            var existing = knownFlowers
                .FirstOrDefault(r => Vector3.Distance(r.position, incoming.position) < mergeRadius);

            if (existing != null)
            {
                // Взвешенное усреднение (по количеству отчётов)
                float total = existing.reportCount + 1;
                existing.nectarYield = (existing.nectarYield * existing.reportCount + incoming.nectarYield) / total;
                existing.confidence = Mathf.Min(existing.confidence + 0.15f, 1f);
                existing.reportCount++;
                existing.lastReportedTime = Time.time;

                // Уточняем позицию (центр тяжести)
                existing.position = Vector3.Lerp(existing.position, incoming.position, 1f / total);

                totalSharesMerged++;

                if (debugLog)
                    Debug.Log($"[HiveKnowledge] Уточнён цветок: +{incoming.nectarYield:F1} нк (x{existing.reportCount})");
            }
            else
            {
                // Новая запись
                if (knownFlowers.Count >= maxFlowerRecords)
                {
                    // Вытесняем наименее полезную запись
                    var weakest = knownFlowers
                        .OrderBy(r => r.Score)
                        .ThenBy(r => r.lastReportedTime)
                        .First();
                    knownFlowers.Remove(weakest);

                    if (debugLog)
                        Debug.Log($"[HiveKnowledge] Вытеснен цветок: ({weakest.position.x:F1}, {weakest.position.y:F1})");
                }

                knownFlowers.Add(new HiveFlowerRecord
                {
                    position = incoming.position,
                    nectarYield = incoming.nectarYield,
                    confidence = incoming.confidence * 0.8f, // умеренная уверенность
                    reportCount = 1,
                    lastReportedTime = Time.time,
                    reportedBy = $"bee_{incoming.GetHashCode()}"
                });

                if (debugLog)
                    Debug.Log($"[HiveKnowledge] Новый цветок: {incoming.nectarYield:F1} нк");
            }
        }

        /// <summary>
        /// Слияние информации об опасности
        /// </summary>
        private void MergeDanger(BeeMemory.DangerMemory incoming)
        {
            var existing = knownDangers
                .FirstOrDefault(r => Vector3.Distance(r.position, incoming.position) < mergeRadius);

            if (existing != null)
            {
                float total = existing.reportCount + 1;
                existing.threatLevel = (existing.threatLevel * existing.reportCount + incoming.threatLevel) / total;
                existing.confidence = Mathf.Min(existing.confidence + 0.2f, 1f);
                existing.reportCount++;
                existing.lastReportedTime = Time.time;

                // Приоритет опасного типа
                if (incoming.threatLevel > existing.threatLevel)
                    existing.dangerType = incoming.dangerType;

                totalSharesMerged++;
            }
            else
            {
                if (knownDangers.Count >= maxDangerRecords)
                {
                    var weakest = knownDangers
                        .OrderBy(r => r.threatLevel * r.confidence)
                        .ThenBy(r => r.lastReportedTime)
                        .First();
                    knownDangers.Remove(weakest);
                }

                knownDangers.Add(new HiveDangerRecord
                {
                    position = incoming.position,
                    threatLevel = incoming.threatLevel,
                    confidence = incoming.confidence * 0.7f,
                    reportCount = 1,
                    lastReportedTime = Time.time,
                    dangerType = incoming.dangerType
                });
            }
        }

        // ======================== ЗАПРОСЫ ========================

        /// <summary>
        /// Получить лучший цветок относительно позиции.
        /// Учитывает: урожайность, расстояние, свежесть данных, количество подтверждений.
        /// </summary>
        public HiveFlowerRecord GetBestFlower(Vector3 fromPosition)
        {
            return knownFlowers
                .Where(r => r.IsFresh && r.nectarYield > 0.1f)
                .OrderByDescending(r =>
                {
                    float dist = Vector3.Distance(fromPosition, r.position);
                    return (r.nectarYield / (dist + 5f)) * r.confidence * Mathf.Log(r.reportCount + 1);
                })
                .FirstOrDefault();
        }

        /// <summary>
        /// Получить N лучших цветков (для разведчиц)
        /// </summary>
        public List<HiveFlowerRecord> GetBestFlowers(Vector3 fromPosition, int count = 3)
        {
            return knownFlowers
                .Where(r => r.IsFresh && r.nectarYield > 0.1f)
                .OrderByDescending(r =>
                {
                    float dist = Vector3.Distance(fromPosition, r.position);
                    return (r.nectarYield / (dist + 5f)) * r.confidence * Mathf.Log(r.reportCount + 1);
                })
                .Take(count)
                .ToList();
        }

        /// <summary>
        /// Проверить зону на опасность
        /// </summary>
        public bool IsPositionDangerous(Vector3 position, float checkRadius = 5f)
        {
            return knownDangers.Any(r =>
                r.IsFresh &&
                r.threatLevel > 0.3f &&
                r.confidence > 0.3f &&
                Vector3.Distance(r.position, position) < checkRadius);
        }

        /// <summary>
        /// Получить все известные цветы (для распространения)
        /// </summary>
        public List<HiveFlowerRecord> GetAllKnownFlowers()
        {
            return knownFlowers.Where(r => r.IsFresh).ToList();
        }

        /// <summary>
        /// Получить все известные опасности (для распространения)
        /// </summary>
        public List<HiveDangerRecord> GetAllKnownDangers()
        {
            return knownDangers.Where(r => r.IsFresh).ToList();
        }

        // ======================== ОЧИСТКА ========================

        /// <summary>
        /// Удаление устаревших записей (раз в 10 секунд)
        /// </summary>
        private float lastCleanupTime;

        private void CleanupOldKnowledge()
        {
            if (Time.time - lastCleanupTime < 10f) return;
            lastCleanupTime = Time.time;

            int before = knownFlowers.Count + knownDangers.Count;

            knownFlowers.RemoveAll(r => !r.IsFresh);
            knownDangers.RemoveAll(r => !r.IsFresh);

            int removed = before - (knownFlowers.Count + knownDangers.Count);
            if (removed > 0 && debugLog)
                Debug.Log($"[HiveKnowledge] Очищено {removed} устаревших записей");
        }

        // ======================== СТАТИСТИКА ========================

        /// <summary>
        /// Получить текущую статистику базы знаний
        /// </summary>
        public string GetStatusReport()
        {
            return $"🧠 База знаний роя:\n" +
                   $"   Цветов: {knownFlowers.Count}\n" +
                   $"   Опасностей: {knownDangers.Count}\n" +
                   $"   Всего отчётов: {totalSharesReceived}\n" +
                   $"   Слияний: {totalSharesMerged}\n" +
                   $"   Самый сочный цветок: {(knownFlowers.Count > 0 ? knownFlowers.OrderByDescending(r => r.nectarYield).First().nectarYield.ToString("F1") : "—")} нк";
        }

        // ======================== ВИЗУАЛИЗАЦИЯ ========================

        void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying) return;

            // Цветы
            foreach (var r in knownFlowers)
            {
                if (!r.IsFresh) continue;
                Gizmos.color = Color.Lerp(Color.gray, Color.green, r.confidence);
                Gizmos.DrawSphere(r.position, 0.25f + r.nectarYield * 0.1f);

                // Размер отчёта
                #if UNITY_EDITOR
                UnityEditor.Handles.Label(
                    r.position + Vector3.up * 0.5f,
                    $"x{r.reportCount} {r.nectarYield:F1}нк"
                );
                #endif
            }

            // Опасности
            foreach (var r in knownDangers)
            {
                if (!r.IsFresh) continue;
                Gizmos.color = Color.Lerp(Color.yellow, Color.red, r.threatLevel);
                Gizmos.DrawWireSphere(r.position, r.confidence * 1.5f);
            }
        }
    }
}
