using UnityEngine;

namespace BeeSwarm.Gameplay
{
    /// <summary>
    /// Цикл смены сезонов: Весна → Лето → Осень → Зима
    /// Влияет на температуру, спавн цветов, поведение пчёл.
    /// </summary>
    public class SeasonCycle : MonoBehaviour
    {
        public enum Season { Spring, Summer, Autumn, Winter }

        [Header("Длительность сезона (игровые секунды)")]
        [SerializeField] private float seasonDuration = 120f;

        [Header("Температура")]
        [SerializeField] private float baseTemperature = 20f;
        [SerializeField] private AnimationCurve temperatureCurve;

        [Header("Эффекты сезонов")]
        [SerializeField] private float springGrowthBonus = 1.3f;
        [SerializeField] private float summerGrowthBonus = 1.5f;
        [SerializeField] private float autumnDecayRate = 0.7f;
        [SerializeField] private float winterPenalty = 0.1f;

        private Season currentSeason = Season.Spring;
        private float seasonTimer;
        private int totalSeasonsPassed;

        public Season CurrentSeason => currentSeason;
        public float SeasonProgress => seasonTimer / seasonDuration;
        public float Temperature => GetCurrentTemperature();
        public float GrowthMultiplier => GetGrowthMultiplier();
        public int TotalSeasonsPassed => totalSeasonsPassed;

        public static SeasonCycle Instance { get; private set; }

        public System.Action<Season> OnSeasonChanged;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        void Start()
        {
            currentSeason = Season.Spring;
            seasonTimer = 0f;

            if (temperatureCurve == null || temperatureCurve.keys.Length < 2)
            {
                temperatureCurve = new AnimationCurve(
                    new Keyframe(0f, 15f),    // Весна
                    new Keyframe(0.25f, 25f),  // Лето (пик)
                    new Keyframe(0.5f, 10f),   // Осень
                    new Keyframe(0.75f, -5f),  // Зима
                    new Keyframe(1f, 15f)      // След. весна
                );
            }

            Debug.Log($"🌱 Сезон: {currentSeason}, Температура: {Temperature:F1}°C");
        }

        void Update()
        {
            seasonTimer += Time.deltaTime;

            if (seasonTimer >= seasonDuration)
            {
                AdvanceSeason();
            }
        }

        void AdvanceSeason()
        {
            seasonTimer = 0f;
            totalSeasonsPassed++;

            int next = ((int)currentSeason + 1) % 4;
            currentSeason = (Season)next;

            OnSeasonChanged?.Invoke(currentSeason);
            Debug.Log($"🍂 Сезон сменился: {currentSeason} (день {totalSeasonsPassed})");
        }

        float GetCurrentTemperature()
        {
            float seasonOffset = (int)currentSeason * 0.25f;
            float progress = seasonOffset + SeasonProgress * 0.25f;
            return baseTemperature + temperatureCurve.Evaluate(progress);
        }

        float GetGrowthMultiplier()
        {
            switch (currentSeason)
            {
                case Season.Spring: return springGrowthBonus;
                case Season.Summer: return summerGrowthBonus;
                case Season.Autumn: return autumnDecayRate;
                case Season.Winter: return winterPenalty;
                default: return 1f;
            }
        }

        /// <summary>Пчёлы уходят в улей зимой?</summary>
        public bool IsWinterHibernation => currentSeason == Season.Winter && Temperature < 0f;

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width / 2 - 100, 10, 200, 30));
            GUILayout.Label($"🌤 {currentSeason} | {Temperature:F1}°C");
            GUILayout.EndArea();
        }
    }
}
