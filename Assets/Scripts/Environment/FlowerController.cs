using UnityEngine;

namespace BeeSwarm.Environment
{
    /// <summary>
    /// Контроллер цветка — источник нектара для пчёл.
    /// Цветок имеет запас нектара, который восстанавливается со временем.
    /// </summary>
    public class FlowerController : MonoBehaviour
    {
        [Header("Параметры цветка")]
        [SerializeField] private string flowerName = "Цветок";
        [SerializeField] private float maxNectar = 50f;
        [SerializeField] private float currentNectar;
        [SerializeField] private float nectarRegenRate = 1f; // восстановление в секунду
        [SerializeField] private float regenDelay = 5f; // задержка перед восстановлением после сбора
        
        [Header("Тип")]
        [SerializeField] private HoneyType honeyType = HoneyType.Flower;
        [SerializeField] private PollenType pollenType = PollenType.Flower;
        
        [Header("Визуал")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color fullColor = Color.yellow;
        [SerializeField] private Color emptyColor = Color.gray;
        [SerializeField] private GameObject nectarParticlePrefab;
        
        // Состояние
        private float lastCollectTime;
        private float regenTimer;
        
        // Свойства
        public float MaxNectar => maxNectar;
        public float CurrentNectar => currentNectar;
        public float NectarPercentage => currentNectar / maxNectar;
        public bool HasNectar => currentNectar > 0.5f;
        public bool IsFull => currentNectar >= maxNectar;
        public HoneyType HoneyType => honeyType;
        public PollenType PollenType => pollenType;
        
        public enum HoneyType
        {
            Flower,    // Цветочный мёд
            Forest,    // Лесной мёд
            Meadow     // Луговой мёд
        }
        
        public enum PollenType
        {
            Flower,    // Цветочная пыльца
            Meadow,    // Луговая пыльца
            Pine       // Хвойная пыльца
        }
        
        void Start()
        {
            currentNectar = maxNectar;
            UpdateAppearance();
        }
        
        void Update()
        {
            // Восстановление нектара
            if (currentNectar < maxNectar)
            {
                float timeSinceCollect = Time.time - lastCollectTime;
                if (timeSinceCollect >= regenDelay)
                {
                    currentNectar += nectarRegenRate * Time.deltaTime;
                    currentNectar = Mathf.Min(currentNectar, maxNectar);
                    UpdateAppearance();
                }
            }
        }
        
        /// <summary>
        /// Забрать нектар
        /// </summary>
        public float TakeNectar(float amount)
        {
            float taken = Mathf.Min(amount, currentNectar);
            currentNectar -= taken;
            lastCollectTime = Time.time;
            
            UpdateAppearance();
            
            return taken;
        }
        
        /// <summary>
        /// Обновить внешний вид в зависимости от наполнения
        /// </summary>
        private void UpdateAppearance()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(emptyColor, fullColor, NectarPercentage);
                spriteRenderer.transform.localScale = Vector3.Lerp(
                    Vector3.one * 0.5f,
                    Vector3.one,
                    NectarPercentage
                );
            }
        }
        
        void OnDrawGizmosSelected()
        {
            // Визуализация запаса нектара
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.5f + NectarPercentage * 0.5f);
            
            // Радиус привлекательности
            Gizmos.color = new Color(1f, 1f, 0f, 0.1f);
            Gizmos.DrawWireSphere(transform.position, 1.5f);
        }
    }
}
