using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeeSwarm.Core
{
    /// <summary>
    /// Индивидуальная память пчелы — запоминает цветы, опасности и делится
    /// знаниями с ульем при возвращении.
    /// </summary>
    public class BeeMemory : MonoBehaviour
    {
        [Header("Параметры памяти")]
        [SerializeField] private int maxFlowerMemories = 20;
        [SerializeField] private int maxDangerMemories = 10;
        [SerializeField] private float memoryDecayDuration = 120f; // секунд до полного забывания
        [SerializeField] private float forgetThreshold = 0.1f;     // ниже этого — забываем

        // Запомненные цветы
        private List<FlowerMemory> flowerMemories = new List<FlowerMemory>();

        // Запомненные опасности
        private List<DangerMemory> dangerMemories = new List<DangerMemory>();

        // Ссылка на пчелу-владельца
        private BeeController owner;
        private HiveKnowledgeBase hiveKnowledge;

        // Время последнего обмена знаниями
        private float lastShareTime;

        /// <summary>
        /// Запись о цветке в памяти
        /// </summary>
        [System.Serializable]
        public class FlowerMemory
        {
            public Vector3 position;
            public float nectarYield;       // сколько нектара дал в прошлый раз
            public float confidence;        // уверенность (1.0 — только что проверено)
            public float lastVisitedTime;   // время игры когда посетили
            public int visitCount;          // сколько раз посещали

            public bool IsValid => confidence > 0.1f;
            public float Score => nectarYield * confidence;
        }

        /// <summary>
        /// Запись об опасности в памяти
        /// </summary>
        [System.Serializable]
        public class DangerMemory
        {
            public Vector3 position;
            public float threatLevel;       // уровень угрозы (0..1)
            public float confidence;        // уверенность
            public float lastEncounterTime; // когда встретили
            public string dangerType;       // "predator", "pesticide", "storm"

            public bool IsValid => confidence > 0.1f;
        }

        /// <summary>
        /// Пакет знаний для передачи в улей
        /// </summary>
        public struct KnowledgePacket
        {
            public List<FlowerMemory> flowers;
            public List<DangerMemory> dangers;
        }

        void Awake()
        {
            owner = GetComponent<BeeController>();
        }

        void Start()
        {
            hiveKnowledge = FindObjectOfType<HiveKnowledgeBase>();
        }

        void Update()
        {
            // Забывание старых воспоминаний
            ForgetOldMemories();
        }

        // ======================== ЦВЕТЫ ========================

        /// <summary>
        /// Запомнить цветок. Обновляет существующую запись или создаёт новую.
        /// </summary>
        public void RememberFlower(Vector3 position, float nectarAmount)
        {
            // Ищем существующую запись рядом (радиус 2 единицы)
            FlowerMemory existing = flowerMemories
                .FirstOrDefault(m => Vector3.Distance(m.position, position) < 2f);

            if (existing != null)
            {
                // Обновляем: усредняем урожайность, повышаем уверенность
                existing.nectarYield = Mathf.Lerp(existing.nectarYield, nectarAmount, 0.5f);
                existing.confidence = Mathf.Min(existing.confidence + 0.3f, 1f);
                existing.lastVisitedTime = Time.time;
                existing.visitCount++;
                existing.position = position; // уточняем позицию
            }
            else
            {
                // Новая запись
                if (flowerMemories.Count >= maxFlowerMemories)
                {
                    // Вытесняем самую слабую/старую
                    var weakest = flowerMemories
                        .OrderBy(m => m.Score)
                        .ThenBy(m => m.lastVisitedTime)
                        .First();
                    flowerMemories.Remove(weakest);
                }

                flowerMemories.Add(new FlowerMemory
                {
                    position = position,
                    nectarYield = nectarAmount,
                    confidence = 0.7f, // начальная осторожная уверенность
                    lastVisitedTime = Time.time,
                    visitCount = 1
                });
            }
        }

        /// <summary>
        /// Запомнить опасную зону
        /// </summary>
        public void RememberDanger(Vector3 position, float threatLevel, string dangerType = "unknown")
        {
            DangerMemory existing = dangerMemories
                .FirstOrDefault(m => Vector3.Distance(m.position, position) < 3f);

            if (existing != null)
            {
                existing.threatLevel = Mathf.Lerp(existing.threatLevel, threatLevel, 0.6f);
                existing.confidence = Mathf.Min(existing.confidence + 0.4f, 1f);
                existing.lastEncounterTime = Time.time;
                existing.position = position;
            }
            else
            {
                if (dangerMemories.Count >= maxDangerMemories)
                {
                    var weakest = dangerMemories
                        .OrderBy(m => m.threatLevel * m.confidence)
                        .ThenBy(m => m.lastEncounterTime)
                        .First();
                    dangerMemories.Remove(weakest);
                }

                dangerMemories.Add(new DangerMemory
                {
                    position = position,
                    threatLevel = threatLevel,
                    confidence = 0.8f,
                    lastEncounterTime = Time.time,
                    dangerType = dangerType
                });
            }
        }

        // ======================== ПОИСК ========================

        /// <summary>
        /// Получить лучший целевой цветок с учётом расстояния и урожайности.
        /// Возвращает null если ничего не помним.
        /// </summary>
        public FlowerMemory GetBestFlower(Vector3 fromPosition)
        {
            var valid = flowerMemories
                .Where(m => m.IsValid && m.nectarYield > 0.1f)
                .OrderByDescending(m =>
                {
                    // Оценка: урожайность / (расстояние + 1) * уверенность
                    float dist = Vector3.Distance(fromPosition, m.position);
                    return (m.nectarYield / (dist + 5f)) * m.confidence;
                })
                .ToList();

            return valid.FirstOrDefault();
        }

        /// <summary>
        /// Проверить, есть ли в точке или рядом опасность
        /// </summary>
        public bool IsPositionDangerous(Vector3 position, float checkRadius = 5f)
        {
            return dangerMemories.Any(m =>
                m.IsValid &&
                m.threatLevel > 0.4f &&
                Vector3.Distance(m.position, position) < checkRadius);
        }

        /// <summary>
        /// Получить ближайшую запомненную угрозу
        /// </summary>
        public DangerMemory GetNearestDanger(Vector3 fromPosition)
        {
            return dangerMemories
                .Where(m => m.IsValid)
                .OrderBy(m => Vector3.Distance(fromPosition, m.position))
                .FirstOrDefault();
        }

        // ======================== ЗАБЫВАНИЕ ========================

        /// <summary>
        /// Со временем уверенность падает — пчела забывает.
        /// </summary>
        private void ForgetOldMemories()
        {
            float decay = Time.deltaTime / memoryDecayDuration;

            for (int i = flowerMemories.Count - 1; i >= 0; i--)
            {
                flowerMemories[i].confidence -= decay;
                if (flowerMemories[i].confidence <= forgetThreshold)
                    flowerMemories.RemoveAt(i);
            }

            for (int i = dangerMemories.Count - 1; i >= 0; i--)
            {
                dangerMemories[i].confidence -= decay * 0.5f; // опасности забываются медленнее
                if (dangerMemories[i].confidence <= forgetThreshold)
                    dangerMemories.RemoveAt(i);
            }
        }

        // ======================== ОБМЕН ЗНАНИЯМИ ========================

        /// <summary>
        /// Поделиться знаниями с ульем. Вызывается когда пчела в улье.
        /// </summary>
        public void ShareWithHive()
        {
            if (hiveKnowledge == null) return;

            // Ограничиваем частоту обмена — не чаще раза в 3 секунды
            if (Time.time - lastShareTime < 3f) return;
            lastShareTime = Time.time;

            // Делимся только самым ценным (остальное улей уже знает)
            var bestFlowers = flowerMemories
                .Where(m => m.IsValid && m.confidence > 0.5f)
                .OrderByDescending(m => m.Score)
                .Take(5)
                .ToList();

            var significantDangers = dangerMemories
                .Where(m => m.IsValid && m.confidence > 0.6f)
                .ToList();

            if (bestFlowers.Count == 0 && significantDangers.Count == 0)
                return;

            hiveKnowledge.ReceiveKnowledge(new KnowledgePacket
            {
                flowers = bestFlowers,
                dangers = significantDangers
            });

            // Получить новые знания от улья
            ApplyHiveKnowledge();
        }

        /// <summary>
        /// Впитать знания улья — новые цветы и опасности
        /// </summary>
        private void ApplyHiveKnowledge()
        {
            if (hiveKnowledge == null) return;

            // Добавляем цветы, которых ещё нет в нашей памяти
            var hiveFlowers = hiveKnowledge.GetAllKnownFlowers();
            foreach (var hf in hiveFlowers)
            {
                bool alreadyKnow = flowerMemories
                    .Any(m => Vector3.Distance(m.position, hf.position) < 3f);

                if (!alreadyKnow)
                {
                    flowerMemories.Add(new FlowerMemory
                    {
                        position = hf.position,
                        nectarYield = hf.nectarYield,
                        confidence = hf.confidence * 0.5f, // слухи — половина уверенности
                        lastVisitedTime = Time.time,
                        visitCount = 0
                    });
                }
            }

            // Добавляем опасности
            var hiveDangers = hiveKnowledge.GetAllKnownDangers();
            foreach (var hd in hiveDangers)
            {
                bool alreadyKnow = dangerMemories
                    .Any(m => Vector3.Distance(m.position, hd.position) < 3f);

                if (!alreadyKnow)
                {
                    dangerMemories.Add(new DangerMemory
                    {
                        position = hd.position,
                        threatLevel = hd.threatLevel,
                        confidence = hd.confidence * 0.5f,
                        lastEncounterTime = Time.time,
                        dangerType = hd.dangerType
                    });
                }
            }
        }

        // ======================== ОТЛАДКА ========================

        void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying) return;

            // Цветы в памяти
            foreach (var m in flowerMemories)
            {
                if (!m.IsValid) continue;
                Gizmos.color = Color.Lerp(Color.gray, Color.yellow, m.confidence);
                Gizmos.DrawSphere(m.position, 0.15f + m.Score * 0.3f);

                // Линия к пчеле
                if (owner != null)
                    Gizmos.DrawLine(transform.position, m.position);
            }

            // Опасности
            foreach (var d in dangerMemories)
            {
                if (!d.IsValid) continue;
                Gizmos.color = Color.Lerp(Color.yellow, Color.red, d.threatLevel);
                Gizmos.DrawWireSphere(d.position, 0.5f + d.confidence * 0.5f);
            }
        }
    }
}
