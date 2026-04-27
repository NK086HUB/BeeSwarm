using UnityEngine;
using BeeSwarm.Core;

namespace BeeSwarm.Bees
{
    /// <summary>
    /// Состояния пчелы — конечный автомат
    /// </summary>
    public enum BeeState
    {
        Idle,          // Ожидание в улье
        Foraging,      // Поиск и сбор нектара
        Returning,     // Возвращение в улей
        Resting,       // Отдых
        Escaping,      // Экстренное возвращение
        Guarding       // Охрана
    }

    /// <summary>
    /// Профессии пчелы
    /// </summary>
    public enum BeeProfession
    {
        Gatherer,    // Сборщица
        Guard,       // Охранник
        Builder,     // Строитель
        Nurse,       // Кормилица
        Scientist,   // Учёная
        Universal    // Универсальная
    }

    /// <summary>
    /// ИИ пчелы — управление состояниями (2D)
    /// </summary>
    public class BeeAI : MonoBehaviour
    {
        [Header("Привязка")]
        [SerializeField] private BeeController beeController;
        [SerializeField] private new Rigidbody2D rigidbody;

        [Header("Профессия")]
        [SerializeField] private BeeProfession profession = BeeProfession.Universal;
        [SerializeField] private int professionLevel = 1;

        [Header("Сбор")]
        [SerializeField] private float nectarCapacity = 20f;
        [SerializeField] private float nectarCollected = 0f;
        [SerializeField] private float collectionRate = 3f;
        [SerializeField] private float returnThreshold = 15f;

        [Header("Поиск")]
        [SerializeField] private float searchRadius = 50f;
        [SerializeField] private float flowerDetectionRadius = 30f;
        [SerializeField] private float maxSearchTime = 30f;

        [Header("Отдых")]
        [SerializeField] private float restThreshold = 30f;
        [SerializeField] private float restRecoveryRate = 10f;

        // Приватное
        private FlowerController targetFlower;
        private Vector2 searchTarget;
        private float searchTimer;
        private float idleTimer;
        private BeeState currentState = BeeState.Idle;
        private Transform hiveTransform;
        private HiveManager hiveManager;

        // Свойства
        public BeeState CurrentState => currentState;
        public BeeProfession Profession => profession;
        public float NectarCollected => nectarCollected;
        public float NectarPercentage => nectarCollected / nectarCapacity;

        void Start()
        {
            if (beeController == null) beeController = GetComponent<BeeController>();
            if (rigidbody == null) rigidbody = GetComponent<Rigidbody2D>();

            hiveManager = HiveManager.Instance;
            hiveTransform = hiveManager?.transform;

            ApplyProfessionStats();
        }

        void Update()
        {
            switch (currentState)
            {
                case BeeState.Idle: UpdateIdle(); break;
                case BeeState.Foraging: UpdateForaging(); break;
                case BeeState.Returning: UpdateReturning(); break;
                case BeeState.Resting: UpdateResting(); break;
                case BeeState.Escaping: UpdateEscaping(); break;
                case BeeState.Guarding: UpdateGuarding(); break;
            }
        }

        private void ApplyProfessionStats()
        {
            switch (profession)
            {
                case BeeProfession.Gatherer:
                    nectarCapacity *= 1.5f;
                    collectionRate *= 1.3f;
                    flowerDetectionRadius *= 1.5f;
                    break;
                case BeeProfession.Nurse:
                    restRecoveryRate *= 1.5f;
                    break;
                case BeeProfession.Scientist:
                    flowerDetectionRadius *= 2f;
                    break;
                case BeeProfession.Builder:
                    nectarCapacity *= 0.7f;
                    break;
            }
            float levelBonus = 1f + (professionLevel - 1) * 0.1f;
            nectarCapacity *= levelBonus;
            collectionRate *= levelBonus;
        }

        // ==================== СОСТОЯНИЯ ====================

        private void UpdateIdle()
        {
            idleTimer += Time.deltaTime;

            if (beeController.CurrentEnergy < restThreshold)
            {
                SetState(BeeState.Resting);
                return;
            }

            if (idleTimer > Random.Range(1f, 4f))
            {
                idleTimer = 0f;
                SetState(BeeState.Foraging);
            }
        }

        private void UpdateForaging()
        {
            // Проверка — пора домой?
            if (nectarCollected >= returnThreshold || beeController.CurrentEnergy < 20f)
            {
                SetState(BeeState.Returning);
                return;
            }

            searchTimer += Time.deltaTime;
            if (searchTimer > maxSearchTime)
            {
                searchTimer = 0f;
                SetState(BeeState.Returning);
                return;
            }

            // Есть цветок — летим к нему
            if (targetFlower != null)
            {
                if (!targetFlower.HasNectar)
                {
                    targetFlower = null;
                    return;
                }

                // Летим
                beeController.SetTarget(targetFlower.transform.position);

                float dist = Vector2.Distance(transform.position, targetFlower.transform.position);
                if (dist < 1.5f)
                {
                    // Собираем нектар
                    float collected = collectionRate * Time.deltaTime;
                    nectarCollected = Mathf.Min(nectarCollected + collected, nectarCapacity);
                    targetFlower.TakeNectar(collected);
                }
            }
            else
            {
                // Ищем цветок
                FindNearestFlower();
                if (targetFlower == null)
                {
                    // Нет цветов — летим в случайную точку
                    searchTarget = (Vector2)hiveTransform.position + Random.insideUnitCircle * searchRadius;
                    beeController.SetTarget(searchTarget);

                    if (Vector2.Distance(transform.position, searchTarget) < 3f)
                    {
                        searchTarget = (Vector2)hiveTransform.position + Random.insideUnitCircle * searchRadius;
                        beeController.SetTarget(searchTarget);
                    }
                }
            }
        }

        private void UpdateReturning()
        {
            if (hiveTransform == null) return;

            beeController.SetTarget(hiveTransform.position);

            float dist = Vector2.Distance(transform.position, hiveTransform.position);
            if (dist < 3f)
            {
                // Сдали нектар
                if (nectarCollected > 0)
                {
                    hiveManager?.AddResources(nectarCollected * 0.5f, 0, 0);
                    nectarCollected = 0f;
                }

                SetState(beeController.CurrentEnergy < restThreshold ? BeeState.Resting : BeeState.Idle);
            }

            if (beeController.IsExhausted)
                SetState(BeeState.Escaping);
        }

        private void UpdateResting()
        {
            beeController.RestoreEnergy(restRecoveryRate * Time.deltaTime);
            beeController.ClearTarget();

            if (beeController.CurrentEnergy >= 80f)
                SetState(BeeState.Idle);
        }

        private void UpdateEscaping()
        {
            if (hiveTransform == null) return;
            beeController.SetTarget(hiveTransform.position);

            if (Vector2.Distance(transform.position, hiveTransform.position) < 3f)
            {
                beeController.RestoreEnergy(30f);
                SetState(BeeState.Resting);
            }
        }

        private void UpdateGuarding()
        {
            // TODO: патрулирование вокруг улья
        }

        // ==================== ВСПОМОГАТЕЛЬНЫЕ ====================

        private void SetState(BeeState newState)
        {
            currentState = newState;

            switch (newState)
            {
                case BeeState.Foraging:
                    searchTimer = 0f;
                    targetFlower = null;
                    break;
                case BeeState.Idle:
                case BeeState.Resting:
                    beeController.ClearTarget();
                    break;
            }
        }

        private void FindNearestFlower()
        {
            FlowerController nearest = null;
            float nearestDist = flowerDetectionRadius;

            foreach (var flower in FindObjectsByType<FlowerController>(FindObjectsSortMode.None))
            {
                if (!flower.HasNectar) continue;
                float dist = Vector2.Distance(transform.position, flower.transform.position);
                if (dist < nearestDist)
                {
                    nearest = flower;
                    nearestDist = dist;
                }
            }

            targetFlower = nearest;
        }

        public void AssignProfession(BeeProfession newProfession, int level = 1)
        {
            profession = newProfession;
            professionLevel = Mathf.Max(1, level);
            ApplyProfessionStats();
        }

        public void RecallToHive() => SetState(BeeState.Returning);
    }
}
