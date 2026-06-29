using UnityEngine;

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

        [Header("Ссылки")]
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private SpriteRenderer spriteRenderer;

        // Текущие значения
        private float currentEnergy;
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

            // Обмен знаниями в улье
            if (state == BeeState.Returning && IsInsideHive() && useMemory && beeMemory != null)
            {
                if (Time.time - lastShareTime > shareInterval)
                {
                    beeMemory.ShareWithHive();
                    lastShareTime = Time.time;
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
                var hiveFlower = FindObjectOfType<HiveKnowledgeBase>()?.GetBestFlower(transform.position);

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
            var hiveBase = FindObjectOfType<HiveKnowledgeBase>();
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
                if (state == BeeState.Foraging) OnReachTarget();
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

        private void OnReachTarget()
        {
            float randomNectar = Random.Range(1f, 10f);
            if (useMemory && beeMemory != null)
                beeMemory.RememberFlower(targetPosition, randomNectar);
            currentEnergy = Mathf.Min(currentEnergy + 5f, maxEnergy);
            ReturnToHive();
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
