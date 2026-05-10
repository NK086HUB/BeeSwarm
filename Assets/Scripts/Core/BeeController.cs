using UnityEngine;

namespace BeeSwarm.Core
{
    /// <summary>
    /// Базовый контроллер пчелы с интеграцией системы памяти роя
    /// </summary>
    public class BeeController : MonoBehaviour
    {
        [Header("Основные параметры")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float rotationSpeed = 5f;
        [SerializeField] private float maxEnergy = 100f;
        [SerializeField] private float explorationRadius = 20f;

        [Header("Память и обучение")]
        [SerializeField] private bool useMemory = true;         // использовать память?
        [SerializeField] private float shareInterval = 5f;      // сек между обменами в улье
        [SerializeField] private float exploreChance = 0.3f;    // шанс исследовать вместо цели

        [Header("Ссылки")]
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Animator animator;

        // Текущие значения
        private float currentEnergy;
        private Vector3 targetPosition;
        private bool hasTarget = false;
        private BeeState state = BeeState.Idle;

        // Память
        private BeeMemory beeMemory;
        private float lastShareTime;

        // Состояния пчелы
        public enum BeeState
        {
            Idle,           // бездействие
            Exploring,      // исследование новой территории
            Foraging,       // полёт к цели
            Collecting,     // сбор нектара
            Returning,      // возвращение в улей
            Avoiding        // уклонение от опасности
        }

        // Свойства
        public float CurrentEnergy => currentEnergy;
        public float EnergyPercentage => currentEnergy / maxEnergy;
        public bool IsExhausted => currentEnergy < 20f;
        public BeeState State => state;
        public Vector3 CurrentPosition => transform.position;
        public BeeMemory Memory => beeMemory;

        void Awake()
        {
            beeMemory = GetComponent<BeeMemory>();
            if (beeMemory == null && useMemory)
            {
                beeMemory = gameObject.AddComponent<BeeMemory>();
            }
        }

        void Start()
        {
            currentEnergy = maxEnergy;
            targetPosition = transform.position;
            state = BeeState.Exploration;
        }

        void Update()
        {
            // Расход энергии
            if (hasTarget && state != BeeState.Idle)
            {
                currentEnergy -= Time.deltaTime * 0.5f;
                currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
            }

            // Анимация
            UpdateAnimation();

            // Логика состояний
            UpdateState();

            // Обмен знаниями в улье
            if (useMemory && beeMemory != null && IsInsideHive())
            {
                if (Time.time - lastShareTime > shareInterval)
                {
                    beeMemory.ShareWithHive();
                    lastShareTime = Time.time;

                    // Если возвращались — сброс состояния на исследование
                    if (state == BeeState.Returning)
                    {
                        state = BeeState.Idle;
                        hasTarget = false;
                    }
                }
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
        /// Обновление логики состояний пчелы
        /// </summary>
        private void UpdateState()
        {
            switch (state)
            {
                case BeeState.Idle:
                    // Бездействие — выбираем следующее действие
                    ChooseNextAction();
                    break;

                case BeeState.Exploration:
                    // Исследуем — ищем новые цветы
                    if (!hasTarget)
                    {
                        SetRandomExplorationTarget();
                    }
                    break;

                case BeeState.Foraging:
                    // Летим к цели — проверяем, не пора ли обновить цель
                    if (hasTarget && useMemory && beeMemory != null)
                    {
                        // Проверяем опасность по пути
                        if (beeMemory.IsPositionDangerous(transform.position, 3f))
                        {
                            state = BeeState.Avoiding;
                            SetAvoidanceTarget();
                        }
                    }
                    break;

                case BeeState.Avoiding:
                    // Уклоняемся — летим в сторону от опасности
                    if (!hasTarget || Vector3.Distance(transform.position, targetPosition) < 1f)
                    {
                        state = BeeState.Foraging;
                    }
                    break;
            }
        }

        /// <summary>
        /// Выбор следующего действия на основе памяти
        /// </summary>
        private void ChooseNextAction()
        {
            // Если устали — отдыхаем в улье
            if (IsExhausted)
            {
                RestInHive();
                return;
            }

            // Случайное исследование vs осознанный выбор
            if (useMemory && beeMemory != null && Random.value > exploreChance)
            {
                // Ищем лучший цветок в памяти
                var bestFlower = beeMemory.GetBestFlower(transform.position);
                var hiveFlower = FindObjectOfType<HiveKnowledgeBase>()
                    ?.GetBestFlower(transform.position);

                // Выбираем между личной памятью и знаниями улья
                BeeMemory.FlowerMemory target = null;

                if (bestFlower != null && hiveFlower != null)
                {
                    target = bestFlower.Score >= hiveFlower.Score * 0.8f
                        ? bestFlower
                        : ConvertHiveToMemory(hiveFlower);
                }
                else if (bestFlower != null)
                {
                    target = bestFlower;
                }
                else if (hiveFlower != null)
                {
                    target = ConvertHiveToMemory(hiveFlower);
                }

                if (target != null)
                {
                    SetForageTarget(target.position);
                    return;
                }
            }

            // Ничего не помним или решили исследовать — идём разведывать
            SetExplorationTarget();
        }

        /// <summary>
        /// Конвертировать запись из базы улья в личную память
        /// </summary>
        private BeeMemory.FlowerMemory ConvertHiveToMemory(HiveKnowledgeBase.HiveFlowerRecord hiveRecord)
        {
            if (hiveRecord == null) return null;

            // Создаём временную запись
            var temp = new BeeMemory.FlowerMemory
            {
                position = hiveRecord.position,
                nectarYield = hiveRecord.nectarYield,
                confidence = hiveRecord.confidence * 0.6f,
                lastVisitedTime = Time.time,
                visitCount = 0
            };

            // Запоминаем
            if (beeMemory != null)
                beeMemory.RememberFlower(temp.position, temp.nectarYield);

            return temp;
        }

        /// <summary>
        /// Установить цель для сбора нектара
        /// </summary>
        private void SetForageTarget(Vector3 position)
        {
            targetPosition = position;
            hasTarget = true;
            state = BeeState.Foraging;
        }

        /// <summary>
        /// Установить случайную цель для исследования
        /// </summary>
        private void SetExplorationTarget()
        {
            Vector3 randomDir = Random.insideUnitSphere * explorationRadius;
            randomDir.z = 0f; // 2D игра
            Vector3 target = transform.position + randomDir;

            // Проверяем по базе знаний — не опасно ли там
            var hiveBase = FindObjectOfType<HiveKnowledgeBase>();
            if (hiveBase != null && hiveBase.IsPositionDangerous(target))
            {
                // Ищем другое направление
                for (int i = 0; i < 5; i++)
                {
                    randomDir = Random.insideUnitSphere * explorationRadius;
                    randomDir.z = 0f;
                    target = transform.position + randomDir;

                    if (!hiveBase.IsPositionDangerous(target))
                        break;
                }
            }

            targetPosition = target;
            hasTarget = true;
            state = BeeState.Exploration;
        }

        /// <summary>
        /// Установить цель для случайного полёта (без памяти)
        /// </summary>
        private void SetRandomExplorationTarget()
        {
            Vector3 randomDir = Random.insideUnitSphere * explorationRadius;
            randomDir.z = 0f;
            targetPosition = transform.position + randomDir;
            hasTarget = true;
        }

        /// <summary>
        /// Установить цель уклонения от опасности
        /// </summary>
        private void SetAvoidanceTarget()
        {
            // Летим в противоположную сторону от опасности
            Vector3 dangerDir = Vector3.zero;

            if (useMemory && beeMemory != null)
            {
                var danger = beeMemory.GetNearestDanger(transform.position);
                if (danger != null)
                {
                    dangerDir = (transform.position - danger.position).normalized;
                }
            }

            if (dangerDir == Vector3.zero)
                dangerDir = Random.insideUnitSphere.normalized;

            dangerDir.z = 0f;
            targetPosition = transform.position + dangerDir * explorationRadius * 0.5f;
            hasTarget = true;
        }

        /// <summary>
        /// Отдых в улье — восстановление энергии
        /// </summary>
        private void RestInHive()
        {
            // Энергия восстанавливается быстрее внутри улья
            currentEnergy += Time.deltaTime * 10f;
            currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);

            if (currentEnergy >= 80f)
            {
                state = BeeState.Idle;
            }
        }

        // ======================== ДВИЖЕНИЕ ========================

        /// <summary>
        /// Установить цель движения
        /// </summary>
        public void SetTarget(Vector3 position)
        {
            targetPosition = position;
            hasTarget = true;
            state = BeeState.Foraging;
        }

        /// <summary>
        /// Очистить цель движения
        /// </summary>
        public void ClearTarget()
        {
            hasTarget = false;
            state = BeeState.Idle;
        }

        /// <summary>
        /// Движение к цели
        /// </summary>
        private void MoveToTarget()
        {
            if (rb == null) return;

            Vector3 direction = (targetPosition - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, targetPosition);

            // Если близко к цели
            if (distance < 0.5f)
            {
                hasTarget = false;

                // Если летели за нектаром — запоминаем
                if (state == BeeState.Foraging)
                {
                    OnReachTarget();
                }

                state = BeeState.Idle;
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
        /// Вызывается когда пчела достигла цели
        /// </summary>
        private void OnReachTarget()
        {
            // Эмулируем нахождение цветка (в реальной игре будет проверка коллизий)
            float randomNectar = Random.Range(1f, 10f);

            // Запоминаем
            if (useMemory && beeMemory != null)
            {
                beeMemory.RememberFlower(targetPosition, randomNectar);
            }

            // Восстанавливаем немного энергии (сбор)
            currentEnergy = Mathf.Min(currentEnergy + 5f, maxEnergy);

            // Возвращаемся в улей
            ReturnToHive();
        }

        // ======================== УЛЕЙ ========================

        /// <summary>
        /// Вернуться в улей
        /// </summary>
        public void ReturnToHive()
        {
            if (HiveManager.Instance != null)
            {
                targetPosition = HiveManager.Instance.HiveEntrance != null
                    ? HiveManager.Instance.HiveEntrance.position
                    : HiveManager.Instance.HiveCenter;
                hasTarget = true;
                state = BeeState.Returning;
            }
        }

        /// <summary>
        /// Проверить, находится ли пчела внутри улья
        /// </summary>
        public bool IsInsideHive()
        {
            return HiveManager.Instance != null && HiveManager.Instance.IsInsideHive(transform.position);
        }

        /// <summary>
        /// Анимация
        /// </summary>
        private void UpdateAnimation()
        {
            if (animator != null)
            {
                float speed = hasTarget ? 1f : 0f;
                animator.SetFloat("Speed", speed);
                animator.SetFloat("Energy", EnergyPercentage);

                // Состояние
                animator.SetInteger("State", (int)state);
            }
        }

        // ======================== ЭНЕРГИЯ ========================

        public void RestoreEnergy(float amount)
        {
            currentEnergy += amount;
            currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
        }

        public void ConsumeEnergy(float amount)
        {
            currentEnergy -= amount;
            currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
        }

        public bool CanWork()
        {
            return !IsExhausted && currentEnergy > 30f;
        }

        // ======================== ВИЗУАЛИЗАЦИЯ ========================

        void OnDrawGizmosSelected()
        {
            // Цель движения
            if (hasTarget)
            {
                Color stateColor = state switch
                {
                    BeeState.Exploration => Color.cyan,
                    BeeState.Foraging => Color.yellow,
                    BeeState.Returning => Color.green,
                    BeeState.Avoiding => Color.red,
                    _ => Color.gray
                };

                Gizmos.color = stateColor;
                Gizmos.DrawSphere(targetPosition, 0.3f);
                Gizmos.DrawLine(transform.position, targetPosition);

                // Метка состояния
                #if UNITY_EDITOR
                UnityEditor.Handles.Label(
                    transform.position + Vector3.up * 1.5f,
                    state.ToString()
                );
                #endif
            }

            // Энергия
            Gizmos.color = Color.Lerp(Color.red, Color.green, EnergyPercentage);
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
}
