using UnityEngine;

namespace BeeSwarm.Core
{
    /// <summary>
    /// Базовый контроллер пчелы (2D)
    /// </summary>
    public class BeeController : MonoBehaviour
    {
        [Header("Основные параметры")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float rotationSpeed = 5f;
        [SerializeField] private float maxEnergy = 100f;

        [Header("Ссылки (2D)")]
        [SerializeField] private new Rigidbody2D rigidbody2D;
        [SerializeField] private Animator animator;

        // Текущие значения
        private float currentEnergy;
        private Vector2 targetPosition;
        private bool hasTarget = false;

        // Свойства
        public float CurrentEnergy => currentEnergy;
        public float EnergyPercentage => currentEnergy / maxEnergy;
        public bool IsExhausted => currentEnergy < 20f;
        public bool HasTarget => hasTarget;
        public Vector2 TargetPosition => targetPosition;

        void Start()
        {
            if (rigidbody2D == null) rigidbody2D = GetComponent<Rigidbody2D>();
            currentEnergy = maxEnergy;
        }

        void Update()
        {
            if (hasTarget)
            {
                currentEnergy -= Time.deltaTime * 0.5f;
                currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
            }
            UpdateAnimation();
        }

        void FixedUpdate()
        {
            if (hasTarget && !IsExhausted)
                MoveToTarget();
        }

        public void SetTarget(Vector2 position)
        {
            targetPosition = position;
            hasTarget = true;
        }

        public void SetTarget(Vector3 position)
        {
            targetPosition = (Vector2)position;
            hasTarget = true;
        }

        public void ClearTarget()
        {
            hasTarget = false;
        }

        private void MoveToTarget()
        {
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            float distance = Vector2.Distance(transform.position, targetPosition);

            if (distance < 0.5f)
            {
                hasTarget = false;
                return;
            }

            // Движение через Rigidbody2D
            Vector2 move = direction * moveSpeed * Time.fixedDeltaTime;
            rigidbody2D.MovePosition(rigidbody2D.position + move);

            // Поворот по направлению движения (2D — flip по X)
            if (direction.x != 0f)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                rigidbody2D.MoveRotation(Mathf.LerpAngle(rigidbody2D.rotation, angle, rotationSpeed * Time.fixedDeltaTime));

                // Flip спрайта если летит влево
                Vector3 scale = transform.localScale;
                scale.x = direction.x < 0 ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
        }

        private void UpdateAnimation()
        {
            if (animator == null) return;
            animator.SetFloat("Speed", hasTarget ? 1f : 0f);
            animator.SetFloat("Energy", EnergyPercentage);
        }

        public void RestoreEnergy(float amount)
        {
            currentEnergy = Mathf.Clamp(currentEnergy + amount, 0f, maxEnergy);
        }

        public void ConsumeEnergy(float amount)
        {
            currentEnergy = Mathf.Clamp(currentEnergy - amount, 0f, maxEnergy);
        }

        public bool CanWork() => !IsExhausted && currentEnergy > 30f;

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
