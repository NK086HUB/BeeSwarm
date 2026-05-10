using UnityEngine;

namespace BeeSwarm.Simple
{
    /// <summary>
    /// Упрощённая версия пчелы для тестирования
    /// </summary>
    public class BeeSimple : MonoBehaviour
    {
        [Header("Движение")]
        public float speed = 3f;
        public float rotationSpeed = 5f;
        
        [Header("Энергия")]
        public float maxEnergy = 100f;
        public float energyConsumptionRate = 0.5f;
        
        [Header("Визуал")]
        public MeshRenderer bodyRenderer;
        public Color energyColor = Color.yellow;
        public Color exhaustedColor = Color.red;
        
        // Текущие значения
        private float currentEnergy;
        private Vector3 targetPosition;
        private bool hasTarget = false;
        
        // Свойства
        public float EnergyPercentage => currentEnergy / maxEnergy;
        public bool IsExhausted => currentEnergy < 20f;
        
        void Start()
        {
            currentEnergy = maxEnergy;
            targetPosition = transform.position;
            
            // Настройка цвета
            if (bodyRenderer != null)
            {
                bodyRenderer.material.color = energyColor;
            }
        }
        
        void Update()
        {
            // Обновление энергии
            if (hasTarget)
            {
                currentEnergy -= Time.deltaTime * energyConsumptionRate;
                currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
            }
            
            // Обновление цвета
            UpdateColor();
            
            // Автоматическое возвращение при низкой энергии
            if (IsExhausted && hasTarget)
            {
                ReturnToHive();
            }
        }
        
        void FixedUpdate()
        {
            if (hasTarget && !IsExhausted)
            {
                MoveToTarget();
            }
        }
        
        void MoveToTarget()
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, targetPosition);
            
            // Достигли цели
            if (distance < 0.5f)
            {
                hasTarget = false;
                return;
            }
            
            // Движение
            transform.position += direction * speed * Time.fixedDeltaTime;
            
            // Поворот
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            }
        }
        
        void UpdateColor()
        {
            if (bodyRenderer == null) return;
            
            // Интерполяция цвета в зависимости от энергии
            Color targetColor = Color.Lerp(exhaustedColor, energyColor, EnergyPercentage);
            bodyRenderer.material.color = Color.Lerp(bodyRenderer.material.color, targetColor, Time.deltaTime * 2f);
        }
        
        public void SetTarget(Vector3 position)
        {
            targetPosition = position;
            hasTarget = true;
        }
        
        public void ClearTarget()
        {
            hasTarget = false;
        }
        
        public void RestoreEnergy(float amount)
        {
            currentEnergy += amount;
            currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
        }
        
        void ReturnToHive()
        {
            // Просто сбрасываем цель
            ClearTarget();
            
            // Медленное восстановление энергии
            RestoreEnergy(Time.deltaTime * 2f);
        }
        
        void OnMouseDown()
        {
            // Клик по пчеле - выбрать цель
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                SetTarget(hit.point);
                Debug.Log($"Пчела выбрала цель: {hit.point}");
            }
        }
        
        void OnDrawGizmosSelected()
        {
            // Цель
            if (hasTarget)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(targetPosition, 0.3f);
                Gizmos.DrawLine(transform.position, targetPosition);
            }
            
            // Энергия
            Gizmos.color = Color.Lerp(Color.red, Color.green, EnergyPercentage);
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}