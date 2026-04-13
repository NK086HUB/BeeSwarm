using UnityEngine;

namespace BeeSwarm.Core
{
    /// <summary>
    /// Базовый контроллер пчелы
    /// </summary>
    public class BeeController : MonoBehaviour
    {
        [Header("Основные параметры")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float rotationSpeed = 5f;
        [SerializeField] private float maxEnergy = 100f;
        
        [Header("Ссылки")]
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Animator animator;
        
        // Текущие значения
        private float currentEnergy;
        private Vector3 targetPosition;
        private bool hasTarget = false;
        
        // Свойства
        public float CurrentEnergy => currentEnergy;
        public float EnergyPercentage => currentEnergy / maxEnergy;
        public bool IsExhausted => currentEnergy < 20f;
        
        void Start()
        {
            currentEnergy = maxEnergy;
            targetPosition = transform.position;
        }
        
        void Update()
        {
            // Анимация полёта
            UpdateAnimation();
            
            // Расход энергии
            if (hasTarget)
            {
                currentEnergy -= Time.deltaTime * 0.5f; // 0.5 энергии в секунду
                currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
            }
        }
        
        void FixedUpdate()
        {
            if (hasTarget && !IsExhausted)
            {
                MoveToTarget();
            }
        }
        
        /// <summary>
        /// Установить цель движения
        /// </summary>
        public void SetTarget(Vector3 position)
        {
            targetPosition = position;
            hasTarget = true;
        }
        
        /// <summary>
        /// Очистить цель движения
        /// </summary>
        public void ClearTarget()
        {
            hasTarget = false;
        }
        
        /// <summary>
        /// Движение к цели
        /// </summary>
        private void MoveToTarget()
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, targetPosition);
            
            // Если близко к цели - остановиться
            if (distance < 0.5f)
            {
                hasTarget = false;
                return;
            }
            
            // Движение
            Vector3 moveDirection = direction * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + moveDirection);
            
            // Поворот
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            }
        }
        
        /// <summary>
        /// Обновление анимации
        /// </summary>
        private void UpdateAnimation()
        {
            if (animator != null)
            {
                // Скорость для анимации
                float speed = hasTarget ? 1f : 0f;
                animator.SetFloat("Speed", speed);
                
                // Энергия для анимации усталости
                animator.SetFloat("Energy", EnergyPercentage);
            }
        }
        
        /// <summary>
        /// Восстановить энергию
        /// </summary>
        public void RestoreEnergy(float amount)
        {
            currentEnergy += amount;
            currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
        }
        
        /// <summary>
        /// Потратить энергию
        /// </summary>
        public void ConsumeEnergy(float amount)
        {
            currentEnergy -= amount;
            currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
        }
        
        /// <summary>
        /// Проверить, может ли пчела выполнять действия
        /// </summary>
        public bool CanWork()
        {
            return !IsExhausted && currentEnergy > 30f;
        }
        
        void OnDrawGizmosSelected()
        {
            // Визуализация цели в редакторе
            if (hasTarget)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(targetPosition, 0.3f);
                Gizmos.DrawLine(transform.position, targetPosition);
            }
            
            // Визуализация энергии
            Gizmos.color = Color.Lerp(Color.red, Color.green, EnergyPercentage);
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
}
