using UnityEngine;

namespace BeeSwarm.Simple
{
    /// <summary>
    /// 2D упрощённая версия пчелы для тестирования
    /// </summary>
    public class BeeSimple : MonoBehaviour
    {
        [Header("Движение")]
        public float speed = 3f;
        public float rotationSpeed = 360f;

        [Header("Энергия")]
        public float maxEnergy = 100f;
        public float energyConsumptionRate = 0.5f;

        private float currentEnergy;
        private Vector2 targetPosition;
        private bool hasTarget = false;
        private SpriteRenderer spriteRenderer;

        public float EnergyPercentage => currentEnergy / maxEnergy;
        public bool IsExhausted => currentEnergy < 20f;

        void Start()
        {
            currentEnergy = maxEnergy;
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void Update()
        {
            if (hasTarget)
            {
                currentEnergy -= Time.deltaTime * energyConsumptionRate;
                currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
            }
            UpdateColor();
            if (IsExhausted && hasTarget) ClearTarget();
        }

        void FixedUpdate()
        {
            if (hasTarget && !IsExhausted) MoveToTarget();
        }

        void MoveToTarget()
        {
            Vector2 dir = (targetPosition - (Vector2)transform.position).normalized;
            float dist = Vector2.Distance(transform.position, targetPosition);

            if (dist < 0.5f) { hasTarget = false; return; }

            transform.position += (Vector3)dir * speed * Time.fixedDeltaTime;

            // Rotate towards direction
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, angle), rotationSpeed * Time.fixedDeltaTime);
        }

        void UpdateColor()
        {
            if (spriteRenderer == null) return;
            spriteRenderer.color = Color.Lerp(Color.red, Color.yellow, EnergyPercentage);
        }

        public void SetTarget(Vector2 position) { targetPosition = position; hasTarget = true; }
        public void ClearTarget() { hasTarget = false; }
        public void RestoreEnergy(float amount) { currentEnergy += amount; currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy); }

        void OnMouseDown()
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            SetTarget(worldPos);
        }

        void OnDrawGizmosSelected()
        {
            if (hasTarget)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(targetPosition, 0.3f);
                Gizmos.DrawLine(transform.position, targetPosition);
            }
            Gizmos.color = Color.Lerp(Color.red, Color.green, EnergyPercentage);
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}
