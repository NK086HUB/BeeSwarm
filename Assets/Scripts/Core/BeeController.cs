using UnityEngine;
using BeeSwarm.Gameplay;

namespace BeeSwarm.Core
{
    /// <summary>
    /// 2D контроллер пчелы с интеграцией системы памяти роя
    /// </summary>
    public class BeeController : MonoBehaviour
    {
        [Header("Основные параметры")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float rotationSpeed = 360f;
        [SerializeField] private float maxEnergy = 100f;
        [SerializeField] private float explorationRadius = 20f;

        [Header("Память и обучение")]
        [SerializeField] private bool useMemory = true;
        [SerializeField] private float shareInterval = 5f;
        [SerializeField] private float exploreChance = 0.3f;

        [Header("Фуражировка")]
        [SerializeField] private float carryCapacity = 12f;      // сколько нектара уносит за рейс
        [SerializeField] private float harvestPerVisit = 6f;     // сколько берёт с цветка за подход
        [SerializeField] private float harvestRadius = 1.5f;     // на каком расстоянии цветок считается «под пчелой»
        [SerializeField] private float forageScanRadius = 25f;   // поиск следующего цветка, если зобик не полон
        [SerializeField] private float noticeRadius = 5f;        // радиус, в котором разведчица замечает цветок
        [SerializeField] private float emptyFlowerYield = 0.1f;  // оценка для вытоптанного места

        [Header("Переработка нектара в улье")]
        [SerializeField] private float honeyPerNectar = 0.4f;
        [SerializeField] private float pollenPerNectar = 0.25f;
        [SerializeField] private float waxPerNectar = 0.05f;

        [Header("Ссылки")]
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private SpriteRenderer spriteRenderer;

        // Текущие значения
        private float currentEnergy;
        private float carriedNectar;
        private Vector2 targetPosition;
        private bool hasTarget = false;
        private BeeState state = BeeState.Idle;

        private BeeMemory beeMemory;
        private float lastShareTime;

        public enum BeeState
        {
            Idle,
            Exploring,
            Foraging,
            Collecting,
            Returning,
            Avoiding
        }

        public float CurrentEnergy => currentEnergy;
        public float EnergyPercentage => currentEnergy / maxEnergy;
        public bool IsExhausted => currentEnergy < 20f;
        public BeeState State => state;
        public float CarriedNectar => carriedNectar;
        public Vector2 CurrentPosition => transform.position;
        public BeeMemory Memory => beeMemory;

        void Awake()
        {
            beeMemory = GetComponent<BeeMemory>();
            if (beeMemory == null && useMemory)
                beeMemory = gameObject.AddComponent<BeeMemory>();
        }

        void Start()
        {
            currentEnergy = maxEnergy;
            targetPosition = transform.position;
            state = BeeState.Exploring;
            LockZ();
        }

        void Update()
        {
            LockZ();

            if (hasTarget && state != BeeState.Idle)
            {
                currentEnergy -= Time.deltaTime * 0.5f;
                currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
            }

            UpdateState();
            UpdateRotation();

            // Возвращение домой: сдаём нектар, когда пчела оказалась в улье
            if (state == BeeState.Returning)
            {
                if (IsInsideHive())
                {
                    DepositNectar();

                    if (useMemory && beeMemory != null && Time.time - lastShareTime > shareInterval)
                    {
                        beeMemory.ShareWithHive();
                        lastShareTime = Time.time;
                    }

                    state = BeeState.Idle;
                    hasTarget = false;
                }
                else if (!hasTarget)
                {
                    // Дошли до входа, но он оказался вне границ улья — идём к центру,
                    // иначе пчела зависнет с нектаром и не сдаст его никогда
                    var hive = HiveManager.Instance;
                    if (hive != null)
                    {
                        targetPosition = hive.HiveCenter;
                        hasTarget = true;
                    }
                    else
                    {
                        carriedNectar = 0f;
                        state = BeeState.Idle;
                    }
                }
            }
        }

        void FixedUpdate()
        {
            if (hasTarget && !IsExhausted)
                MoveToTarget();
        }

        void LockZ()
        {
            Vector3 p = transform.position;
            p.z = 0f;
            transform.position = p;
        }

        // ======================== СОСТОЯНИЯ ========================

        private void UpdateState()
        {
            switch (state)
            {
                case BeeState.Idle:
                    ChooseNextAction();
                    break;

                case BeeState.Exploring:
                    if (!hasTarget) SetRandomExplorationTarget();
                    break;

                case BeeState.Foraging:
                    if (hasTarget && useMemory && beeMemory != null)
                    {
                        if (beeMemory.IsPositionDangerous(transform.position, 3f))
                        {
                            state = BeeState.Avoiding;
                            SetAvoidanceTarget();
                        }
                    }
                    break;

                case BeeState.Avoiding:
                    if (!hasTarget || Vector2.Distance(transform.position, targetPosition) < 1f)
                        state = BeeState.Foraging;
                    break;
            }
        }

        private void ChooseNextAction()
        {
            if (IsExhausted) { RestInHive(); return; }

            if (useMemory && beeMemory != null && Random.value > exploreChance)
            {
                var bestFlower = beeMemory.GetBestFlower(transform.position);
                // Синглтон вместо FindObjectOfType: раньше поиск по всей сцене шёл
                // у каждой пчелы на каждом выборе цели
                var hiveFlower = HiveKnowledgeBase.Instance?.GetBestFlower(transform.position);

                BeeMemory.FlowerMemory target = null;
                if (bestFlower != null && hiveFlower != null)
                    target = bestFlower.Score >= hiveFlower.Score * 0.8f ? bestFlower : ConvertHiveToMemory(hiveFlower);
                else if (bestFlower != null) target = bestFlower;
                else if (hiveFlower != null) target = ConvertHiveToMemory(hiveFlower);

                if (target != null) { SetForageTarget(target.position); return; }
            }

            SetExplorationTarget();
        }

        private BeeMemory.FlowerMemory ConvertHiveToMemory(HiveKnowledgeBase.HiveFlowerRecord hiveRecord)
        {
            if (hiveRecord == null) return null;
            var temp = new BeeMemory.FlowerMemory
            {
                position = hiveRecord.position, nectarYield = hiveRecord.nectarYield,
                confidence = hiveRecord.confidence * 0.6f, lastVisitedTime = Time.time, visitCount = 0
            };
            if (beeMemory != null) beeMemory.RememberFlower(temp.position, temp.nectarYield);
            return temp;
        }

        private void SetForageTarget(Vector2 position) { targetPosition = position; hasTarget = true; state = BeeState.Foraging; }

        private void SetExplorationTarget()
        {
            Vector2 randomOffset = Random.insideUnitCircle * explorationRadius;
            Vector2 target = (Vector2)transform.position + randomOffset;
            var hiveBase = HiveKnowledgeBase.Instance;
            if (hiveBase != null && hiveBase.IsPositionDangerous(target))
            {
                for (int i = 0; i < 5; i++)
                {
                    randomOffset = Random.insideUnitCircle * explorationRadius;
                    target = (Vector2)transform.position + randomOffset;
                    if (!hiveBase.IsPositionDangerous(target)) break;
                }
            }
            targetPosition = target; hasTarget = true; state = BeeState.Exploring;
        }

        private void SetRandomExplorationTarget()
        {
            targetPosition = (Vector2)transform.position + Random.insideUnitCircle * explorationRadius;
            hasTarget = true;
        }

        private void SetAvoidanceTarget()
        {
            Vector2 dangerDir = Vector2.zero;
            if (useMemory && beeMemory != null)
            {
                var danger = beeMemory.GetNearestDanger(transform.position);
                if (danger != null) dangerDir = ((Vector2)transform.position - (Vector2)danger.position).normalized;
            }
            if (dangerDir == Vector2.zero) dangerDir = Random.insideUnitCircle.normalized;
            targetPosition = (Vector2)transform.position + dangerDir * explorationRadius * 0.5f;
            hasTarget = true;
        }

        private void RestInHive()
        {
            currentEnergy += Time.deltaTime * 10f;
            currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
            if (currentEnergy >= 80f) state = BeeState.Idle;
        }

        // ======================== ДВИЖЕНИЕ ========================

        public void SetTarget(Vector2 position) { targetPosition = position; hasTarget = true; state = BeeState.Foraging; }
        public void ClearTarget() { hasTarget = false; state = BeeState.Idle; }

        private void MoveToTarget()
        {
            if (rb == null) return;
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            float distance = Vector2.Distance(transform.position, targetPosition);

            if (distance < 0.5f)
            {
                hasTarget = false;

                // Обработчики сами решают, каким будет следующее состояние
                // (раньше строка state = Idle затирала Returning и пчела не шла домой)
                if (state == BeeState.Foraging)
                {
                    OnReachTarget();
                    return;
                }

                if (state == BeeState.Exploring)
                {
                    if (TrySpotFlower()) return;
                    state = BeeState.Idle;
                    return;
                }

                if (state == BeeState.Returning) return; // Update сдаст нектар в улье

                state = BeeState.Idle;
                return;
            }

            rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
        }

        private void UpdateRotation()
        {
            if (!hasTarget || rb == null) return;
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            rb.rotation = Mathf.LerpAngle(rb.rotation, targetAngle, rotationSpeed * Time.deltaTime / 360f);
        }

        /// <summary>
        /// Пчела дошла до цели. Раньше здесь начислялся Random.Range(1,10) —
        /// память роя училась на шуме, цветы не истощались, а мёд в улье не менялся.
        /// Теперь нектар берётся у реального цветка.
        /// </summary>
        private void OnReachTarget()
        {
            var spawner = FlowerSpawner.Instance;
            Flower flower = spawner != null ? spawner.FindNearestFlower(targetPosition, harvestRadius) : null;

            if (flower == null)
            {
                // Цветок из памяти уже объеден: помечаем место как малополезное
                if (useMemory && beeMemory != null)
                    beeMemory.RememberFlower(targetPosition, emptyFlowerYield);
                currentEnergy = Mathf.Min(currentEnergy + 2f, maxEnergy);
                ReturnToHive();
                return;
            }

            float room = carryCapacity - carriedNectar;
            float gathered = flower.Collect(Mathf.Min(harvestPerVisit, room));

            if (gathered <= 0f)
            {
                if (useMemory && beeMemory != null)
                    beeMemory.RememberFlower(flower.Position, emptyFlowerYield);
                ReturnToHive();
                return;
            }

            carriedNectar += gathered;
            if (useMemory && beeMemory != null)
                beeMemory.RememberFlower(flower.Position, gathered);

            currentEnergy = Mathf.Min(currentEnergy + 3f, maxEnergy);

            // Ещё есть место — летим к следующему цветку, иначе домой
            if (TryContinueForaging()) return;

            ReturnToHive();
        }

        /// <summary>
        /// Разведчица дошла до случайной точки и заметила цветок рядом —
        /// летим к нему (иначе пчёлы вообще никогда не находили цветы,
        /// пока память роя пуста).
        /// </summary>
        private bool TrySpotFlower()
        {
            var spawner = FlowerSpawner.Instance;
            if (spawner == null) return false;

            Flower flower = spawner.FindNearestFlower(transform.position, noticeRadius);
            if (flower == null) return false;

            SetForageTarget(flower.Position);
            return true;
        }

        /// <summary>Ищем следующий цветок поблизости, если зобик ещё не полон.</summary>
        private bool TryContinueForaging()
        {
            if (carriedNectar >= carryCapacity - 0.5f || IsExhausted) return false;

            var spawner = FlowerSpawner.Instance;
            if (spawner == null) return false;

            Flower next = spawner.FindNearestFlower(transform.position, forageScanRadius);
            if (next == null) return false;

            SetForageTarget(next.Position);
            return true;
        }

        /// <summary>Сдать принесённый нектар в улей. HiveManager — владелец складских запасов.</summary>
        private void DepositNectar()
        {
            if (carriedNectar <= 0f) return;

            float nectar = carriedNectar;
            carriedNectar = 0f;

            if (HiveManager.Instance != null)
            {
                HiveManager.Instance.AddResources(
                    nectar * honeyPerNectar,
                    nectar * pollenPerNectar,
                    nectar * waxPerNectar);
            }
        }

        // ======================== УЛЕЙ ========================

        public void ReturnToHive()
        {
            if (HiveManager.Instance != null)
            {
                targetPosition = HiveManager.Instance.HiveEntrance != null
                    ? HiveManager.Instance.HiveEntrance.position : HiveManager.Instance.HiveCenter;
                hasTarget = true;
                state = BeeState.Returning;
            }
        }

        public bool IsInsideHive()
        {
            return HiveManager.Instance != null && HiveManager.Instance.IsInsideHive(transform.position);
        }

        // ======================== ЭНЕРГИЯ ========================

        public void RestoreEnergy(float amount) { currentEnergy += amount; currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy); }
        public void ConsumeEnergy(float amount) { currentEnergy -= amount; currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy); }
        public bool CanWork() => !IsExhausted && currentEnergy > 30f;

        // ======================== ВИЗУАЛИЗАЦИЯ ========================

        void OnDrawGizmosSelected()
        {
            if (hasTarget)
            {
                Color stateColor = state switch
                {
                    BeeState.Exploring => Color.cyan,
                    BeeState.Foraging => Color.yellow,
                    BeeState.Returning => Color.green,
                    BeeState.Avoiding => Color.red,
                    _ => Color.gray
                };
                Gizmos.color = stateColor;
                Gizmos.DrawSphere(targetPosition, 0.3f);
                Gizmos.DrawLine(transform.position, targetPosition);
            }
            Gizmos.color = Color.Lerp(Color.red, Color.green, EnergyPercentage);
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
}
