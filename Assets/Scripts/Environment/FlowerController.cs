using UnityEngine;

namespace BeeSwarm.Environment
{
    /// <summary>
    /// Цветок — источник нектара (2D).
    /// Регенерируется, меняет цвет/масштаб от заполненности.
    /// </summary>
    public class FlowerController : MonoBehaviour
    {
        [Header("Параметры")]
        [SerializeField] private string flowerName = "Цветок";
        [SerializeField] private float maxNectar = 50f;
        [SerializeField] private float nectarRegenRate = 1f;
        [SerializeField] private float regenDelay = 5f;

        [Header("Тип")]
        [SerializeField] private HoneyType honeyType = HoneyType.Flower;
        [SerializeField] private PollenType pollenType = PollenType.Flower;

        [Header("Визуал (2D)")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color fullColor = Color.yellow;
        [SerializeField] private Color emptyColor = Color.gray;

        // Состояние
        private float currentNectar;
        private float lastCollectTime;

        // Свойства
        public float MaxNectar => maxNectar;
        public float CurrentNectar => currentNectar;
        public float NectarPercentage => currentNectar / maxNectar;
        public bool HasNectar => currentNectar > 0.5f;
        public bool IsFull => currentNectar >= maxNectar;
        public HoneyType HoneyType => honeyType;
        public PollenType PollenType => pollenType;

        public enum HoneyType { Flower, Forest, Meadow }
        public enum PollenType { Flower, Meadow, Pine }

        void Start()
        {
            currentNectar = maxNectar;
            UpdateAppearance();
        }

        void Update()
        {
            if (currentNectar < maxNectar && Time.time - lastCollectTime >= regenDelay)
            {
                currentNectar = Mathf.Min(currentNectar + nectarRegenRate * Time.deltaTime, maxNectar);
                UpdateAppearance();
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

        private void UpdateAppearance()
        {
            if (spriteRenderer == null) return;
            spriteRenderer.color = Color.Lerp(emptyColor, fullColor, NectarPercentage);
            float scale = Mathf.Lerp(0.6f, 1f, NectarPercentage);
            transform.localScale = new Vector3(scale, scale, 1f);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.3f + NectarPercentage * 0.5f);
        }
    }
}
