using UnityEngine;
using System;
using BeeSwarm.Environment;

namespace BeeSwarm.Seasons
{
    /// <summary>
    /// Времена года
    /// </summary>
    public enum Season
    {
        Spring = 0,  // Весна — размножение, создание запасов
        Summer = 1,  // Лето — максимальный сбор, защита
        Autumn = 2,  // Осень — экономия, изгнание трутней
        Winter = 3   // Зима — выживание, температурный баланс
    }

    /// <summary>
    /// Цикл сезонов — управляет сменой времени года и влияет на все системы
    /// </summary>
    public class SeasonCycle : MonoBehaviour
    {
        [Header("Время")]
        [SerializeField] private Season currentSeason = Season.Spring;
        [SerializeField] private int currentDay = 1;
        [SerializeField] private int daysInSeason = 30;
        [SerializeField] private float seasonProgress = 0f; // 0–1 внутри сезона
        [SerializeField] private float yearProgress = 0f;   // 0–1 за весь год

        [Header("Скорость (для отладки)")]
        [SerializeField] private bool autoAdvance = true;
        [SerializeField] private float dayLengthSeconds = 60f; // 1 день = 1 минута в реальности
        [SerializeField] private float timeScale = 1f;

        [Header("События сезона")]
        [SerializeField] private bool isWinterMode = false;
        [SerializeField] private float temperature = 25f; // градусов
        [SerializeField] private float temperatureRange = 15f; // колебания

        [Header("Влияние на цветы")]
        [SerializeField] private AnimationCurve flowerDensityBySeason = new AnimationCurve(
            new Keyframe(0f, 0.6f),  // Весна 0.0
            new Keyframe(0.25f, 0.9f), // Весна 0.25
            new Keyframe(1f, 0.3f)    // Зима 1.0
        );

        // Приватное
        private float dayTimer;
        private float temperatureTimer;
        private FlowerManager flowerManager;
        private Action<Season, Season> onSeasonChanged;

        // Свойства
        public Season CurrentSeason => currentSeason;
        public int CurrentDay => currentDay;
        public int DaysInSeason => daysInSeason;
        public float SeasonProgress => seasonProgress;
        public float YearProgress => yearProgress;
        public bool IsWinterMode => isWinterMode;
        public float Temperature => temperature;

        public static SeasonCycle Instance { get; private set; }

        // Событие — вызывается при смене сезона
        public event Action<Season> OnSeasonChanged;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        void Start()
        {
            flowerManager = FindObjectOfType<FlowerManager>();
            ApplySeasonEffects(currentSeason);
        }

        void Update()
        {
            if (!autoAdvance) return;

            dayTimer += Time.deltaTime * timeScale;

            if (dayTimer >= dayLengthSeconds)
            {
                dayTimer = 0f;
                AdvanceDay();
            }

            // Температура меняется плавно
            temperatureTimer += Time.deltaTime;
            if (temperatureTimer >= 1f)
            {
                temperatureTimer = 0f;
                UpdateTemperature();
            }

            // Обновляем прогресс
            yearProgress = ((int)currentSeason * daysInSeason + currentDay) / (float)(4 * daysInSeason);
            seasonProgress = currentDay / (float)daysInSeason;
        }

        /// <summary>
        /// Перейти на следующий день
        /// </summary>
        public void AdvanceDay()
        {
            currentDay++;

            if (currentDay > daysInSeason)
            {
                currentDay = 1;
                Season previousSeason = currentSeason;
                currentSeason = (Season)(((int)currentSeason + 1) % 4);

                OnSeasonChanged?.Invoke(currentSeason);
                ApplySeasonEffects(currentSeason);

                Debug.Log($"🌿 Сезон сменился: {previousSeason} → {currentSeason}");
            }
        }

        /// <summary>
        /// Применить эффекты сезона
        /// </summary>
        private void ApplySeasonEffects(Season season)
        {
            switch (season)
            {
                case Season.Spring:
                    isWinterMode = false;
                    timeScale = 1.5f; // весна быстрее
                    break;

                case Season.Summer:
                    isWinterMode = false;
                    timeScale = 1f;
                    break;

                case Season.Autumn:
                    isWinterMode = false;
                    timeScale = 1.2f;
                    break;

                case Season.Winter:
                    isWinterMode = true;
                    timeScale = 0.5f; // зима медленнее
                    // Отзываем всех пчёл в улей
                    RecallAllBees();
                    break;
            }

            // Обновляем количество цветов
            if (flowerManager != null)
            {
                float density = flowerDensityBySeason.Evaluate(yearProgress);
                flowerManager.UpdateSeasonalFlowers(yearProgress);
                Debug.Log($"🌼 Плотность цветов: {density:P}");
            }

            Debug.Log($"🌡️ Температура установлена: {temperature:F1}°C");
        }

        /// <summary>
        /// Обновить температуру в зависимости от сезона
        /// </summary>
        private void UpdateTemperature()
        {
            float baseTemp = currentSeason switch
            {
                Season.Spring => 15f,
                Season.Summer => 30f,
                Season.Autumn => 10f,
                Season.Winter => -5f,
                _ => 20f
            };

            // Суточные колебания
            float dailyVariation = Mathf.Sin(Time.time * 0.1f) * temperatureRange * 0.5f;
            temperature = baseTemp + dailyVariation;

            // Зимой ниже
            if (isWinterMode && temperature > 5f)
            {
                temperature = Mathf.Lerp(temperature, 0f, Time.deltaTime * 0.01f);
            }
        }

        /// <summary>
        /// Отозвать всех пчёл в улей
        /// </summary>
        private void RecallAllBees()
        {
            var bees = FindObjectsByType<BeeAI>(FindObjectsSortMode.None);
            foreach (var bee in bees)
            {
                bee.RecallToHive();
            }
            Debug.Log($"🏠 Отозвано пчёл: {bees.Length}");
        }

        /// <summary>
        /// Получить множитель скорости сбора для сезона
        /// </summary>
        public float GetForageMultiplier()
        {
            return currentSeason switch
            {
                Season.Spring => 0.8f,
                Season.Summer => 1.3f,
                Season.Autumn => 0.6f,
                Season.Winter => 0f, // зимой не собираем
                _ => 1f
            };
        }

        /// <summary>
        /// Получить множитель расхода энергии от температуры
        /// </summary>
        public float GetEnergyConsumptionMultiplier()
        {
            if (temperature < 0f) return 2f;     // холодно — тратим вдвое
            if (temperature < 10f) return 1.3f;
            if (temperature > 35f) return 1.5f;  // жарко — тоже тяжело
            return 1f;
        }

        // ==================== GUI ДЛЯ ОТЛАДКИ ====================

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 310, 220, 300, 180));

            string seasonEmoji = currentSeason switch
            {
                Season.Spring => "🌸",
                Season.Summer => "☀️",
                Season.Autumn => "🍂",
                Season.Winter => "❄️",
                _ => ""
            };

            GUILayout.Label($"🌤️ {seasonEmoji} {currentSeason}");
            GUILayout.Label($"📅 День {currentDay}/{daysInSeason}");
            GUILayout.Label($"🌡️ {temperature:F1}°C");
            GUILayout.Label($"⏱️ x{timeScale:F1}");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("⏸️ Пауза")) autoAdvance = !autoAdvance;
            if (GUILayout.Button("➕ День")) AdvanceDay();
            if (GUILayout.Button("🏠 Отзыв")) RecallAllBees();
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        void OnDrawGizmosSelected()
        {
            // Отображение текущего сезона в сцене
            Gizmos.color = currentSeason switch
            {
                Season.Spring => Color.green,
                Season.Summer => Color.yellow,
                Season.Autumn => new Color(1f, 0.5f, 0f),
                Season.Winter => Color.cyan,
                _ => Color.white
            };
            Gizmos.DrawWireCube(transform.position + Vector3.up * 5f, Vector3.one * 2f);
        }
    }
}
