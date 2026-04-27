using UnityEngine;
using BeeSwarm.Core;

namespace BeeSwarm.Bees
{
    /// <summary>
    /// Состояния пчелы в Finite State Machine
    /// </summary>
    public enum BeeState
    {
        Idle,          // Ожидание в улье
        Foraging,      // Поиск и сбор нектара
        Returning,     // Возвращение в улей
        Resting,       // Отдых/восстановление энергии
        Escaping,      // Экстренное возвращение (зима, danger)
        Guarding       // Охрана территории
    }

    /// <summary>
    /// Профессии пчелы
    /// </summary>
    public enum BeeProfession
    {
        None,         // Не назначена (личинка)
        Gatherer,     // Сборщица
        Guard,        // Охранник
        Builder,      // Строитель
        Nurse,        // Кормилица
        Scientist,    // Учёная
        Universal     // Универсальная
    }

    /// <summary>
    /// ИИ пчелы — управление состоянием и поведением
    /// </summary>
    public class BeeAI : MonoBehaviour
    {
        [Header("Привязка к контроллеру")]
        [SerializeField] private BeeController beeController;
        
        [Header("Профессия")]
        [SerializeField] private BeeProfession profession = BeeProfession.Universal;
        [SerializeField] private int professionLevel = 1;
        
        [Header("Сбор нектара")]
        [SerializeField] private float nectarCapacity = 20f;
        [SerializeField] private float nectarCollected = 0f;
        [SerializeField] private float collectionRate = 3f; // нектара в секунду на цветке
        [SerializeField] private float returnThreshold = 15f; // при скольких возвращаться
        
        [Header("Поиск")]
        [SerializeField] private float searchRadius = 50f;
        [SerializeField] private float flowerDetectionRadius = 30f;
        [SerializeField] private float maxSearchTime = 30f; // сколько искать перед возвратом
        
        [Header("Отдых")]
        [SerializeField] private float restThreshold = 30f; // при какой энергии отдыхать
        [SerializeField] private float restRecoveryRate = 10f; // восстановление в секунду в улье
        
        // Текущие ссылки
        private FlowerController targetFlower;
        private Vector3 searchTarget;
        private float searchTimer;
        private float idleTimer;
        private bool hasTarget;
        
        // Компоненты
        private Transform hiveTransform;
        private HiveManager hiveManager;
        
        // Свойства
        public BeeState CurrentState { get; private set; } = BeeState.Idle;
        public BeeProfession Profession => profession;
        public float NectarCollected => nectarCollected;
        public float NectarPercentage => nectarCollected / nectarCapacity;
        
        void Start()
        {
            if (beeController == null)
                beeController = GetComponent<BeeController>();
            
            hiveManager = HiveManager.Instance;
            hiveTransform = hiveManager != null ? hiveManager.transform : null;
            
            // Настройка под профессию
            ApplyProfessionStats();
        }
        
        void Update()
        {
            switch (CurrentState)
            {
                case BeeState.Idle:
                    UpdateIdle();
                    break;
                case BeeState.Foraging:
                    UpdateForaging();
                    break;
                case BeeState.Returning:
                    UpdateReturning();
                    break;
                case BeeState.Resting:
                    UpdateResting();
                    break;
                case BeeState.Escaping:
                    UpdateEscaping();
                    break;
                case BeeState.Guarding:
                    UpdateGuarding();
                    break;
            }
        }
        
        /// <summary>
        /// Настройка характеристик под профессию
        /// </summary>
        private void ApplyProfessionStats()
        {
            switch (profession)
            {
                case BeeProfession.Gatherer:
                    nectarCapacity *= 1.5f;
                    collectionRate *= 1.3f;
                    flowerDetectionRadius *= 1.5f;
                    break;
                case BeeProfession.Guard:
                    // скорость и энергия выше, сбор ниже
                    break;
                case BeeProfession.Builder:
                    nectarCapacity *= 0.7f;
                    break;
                case BeeProfession.Nurse:
                    restRecoveryRate *= 1.5f;
                    break;
                case BeeProfession.Scientist:
                    flowerDetectionRadius *= 2f;
                    break;
                // Universal — без изменений
            }
            
            // Уровень профессии даёт бонусы
            float levelBonus = 1f + (professionLevel - 1) * 0.1f;
            nectarCapacity *= levelBonus;
            collectionRate *= levelBonus;
        }
        
        // ============================================================
        // СОСТОЯНИЯ
        // ============================================================
        
        private void UpdateIdle()
        {
            // Стоим в улье, ждём
            
            // Восстанавливаем энергию
            if (beeController.CurrentEnergy < maxEnergy * 0.5f)
            {
                beeController.RestoreEnergy(restRecoveryRate * Time.deltaTime);
            }
            
            // Через небольшую паузу — на работу
            idleTimer += Time.deltaTime;
            if (idleTimer > Random.Range(1f, 4f))
            {
                idleTimer = 0f;
                
                if (beeController.CurrentEnergy < restThreshold)
                {
                    SetState(BeeState.Resting);
                }
                else
                {
                    SetState(BeeState.Foraging);
                }
            }
        }
        
        private void UpdateForaging()
        {
            // Потеряли цель — ищем новую
            if (targetFlower == null || !targetFlower.HasNectar)
            {
                FindNearestFlower();
            }
            
            // Если набрали нектара — домой
            if (nectarCollected >= returnThreshold)
            {
                SetState(BeeState.Returning);
                return;
            }
            
            // Если энергия на исходе — домой
            if (beeController.CurrentEnergy < 20f)
            {
                SetState(BeeState.Returning);
                return;
            }
            
            // Ищем слишком долго — домой
            searchTimer += Time.deltaTime;
            if (searchTimer > maxSearchTime)
            {
                searchTimer = 0f;
                SetState(BeeState.Returning);
                return;
            }
            
            // Есть цель — летим к ней
            if (targetFlower != null)
            {
                beeController.SetTarget(targetFlower.transform.position);
                
                float dist = Vector3.Distance(transform.position, targetFlower.transform.position);
                if (dist < 1.5f)
                {
                    // Сбор нектара
                    CollectNectar();
                }
            }
            else if (hasTarget)
            {
                // Летим к точке поиска
                float dist = Vector3.Distance(transform.position, searchTarget);
                if (dist < 3f)
                {
                    hasTarget = false;
                    // Осматриваемся — ищем новый цветок
                }
            }
            else
            {
                // Новая случайная точка поиска
                PickSearchTarget();
            }
        }
        
        private void UpdateReturning()
        {
            if (hiveTransform == null) return;
            
            beeController.SetTarget(hiveTransform.position);
            
            float dist = Vector3.Distance(transform.position, hiveTransform.position);
            if (dist < 3f)
            {
                // Сдали нектар
                if (nectarCollected > 0)
                {
                    hiveManager.AddResources(nectarCollected * 0.5f, 0, 0);
                    nectarCollected = 0f;
                }
                
                // Если энергии мало — отдых
                if (beeController.CurrentEnergy < restThreshold)
                {
                    SetState(BeeState.Resting);
                }
                else
                {
                    SetState(BeeState.Idle);
                }
            }
            
            // Энергия кончилась в пути — экстренный режим
            if (beeController.IsExhausted)
            {
                SetState(BeeState.Escaping);
            }
        }
        
        private void UpdateResting()
        {
            // Стоим в улье, восстанавливаем энергию
            beeController.RestoreEnergy(restRecoveryRate * Time.deltaTime);
            beeController.ClearTarget();
            
            // Восстановились — на работу
            if (beeController.CurrentEnergy >= 80f)
            {
                SetState(BeeState.Idle);
            }
        }
        
        private void UpdateEscaping()
        {
            // Срочно домой, игнорируем всё
            if (hiveTransform == null) return;
            
            beeController.SetTarget(hiveTransform.position);
            
            float dist = Vector3.Distance(transform.position, hiveTransform.position);
            if (dist < 3f)
            {
                beeController.RestoreEnergy(30f);
                SetState(BeeState.Resting);
            }
        }
        
        private void UpdateGuarding()
        {
            // TODO: Патрулирование вокруг улья
            // Пока просто стоим на месте
        }
        
        // ============================================================
        // ВСПОМОГАТЕЛЬНЫЕ
        // ============================================================
        
        private void SetState(BeeState newState)
        {
            BeeState oldState = CurrentState;
            CurrentState = newState;
            
            // Сброс при смене состояния
            switch (newState)
            {
                case BeeState.Foraging:
                    searchTimer = 0f;
                    targetFlower = null;
                    break;
                case BeeState.Idle:
                    idleTimer = 0f;
                    beeController.ClearTarget();
                    break;
                case BeeState.Returning:
                    beeController.ClearTarget();
                    break;
            }
            
            OnStateChanged(oldState, newState);
        }
        
        private void OnStateChanged(BeeState oldState, BeeState newState)
        {
            // Для отладки
            // Debug.Log($"Пчела {GetInstanceID()}: {oldState} → {newState}");
        }
        
        private void FindNearestFlower()
        {
            FlowerController[] flowers = FindObjectsByType<FlowerController>(FindObjectsSortMode.None);
            FlowerController nearest = null;
            float nearestDist = flowerDetectionRadius;
            
            foreach (var flower in flowers)
            {
                if (!flower.HasNectar) continue;
                
                float dist = Vector3.Distance(transform.position, flower.transform.position);
                if (dist < nearestDist)
                {
                    nearest = flower;
                    nearestDist = dist;
                }
            }
            
            targetFlower = nearest;
            
            if (nearest != null)
            {
                beeController.SetTarget(nearest.transform.position);
            }
        }
        
        private void PickSearchTarget()
        {
            if (hiveTransform == null) return;
            
            Vector3 randomDir = Random.insideUnitSphere * searchRadius;
            randomDir.y = Random.Range(-5f, 5f); // полёт в 2D-плоскости с небольшими отклонениями
            searchTarget = hiveTransform.position + randomDir;
            hasTarget = true;
            
            beeController.SetTarget(searchTarget);
        }
        
        private void CollectNectar()
        {
            float amount = collectionRate * Time.deltaTime;
            nectarCollected += amount;
            nectarCollected = Mathf.Min(nectarCollected, nectarCapacity);
            
            // Уменьшаем запас цветка
            if (targetFlower != null)
            {
                targetFlower.TakeNectar(amount);
            }
            
            // Визуальный фидбек
            if (nectarCollected >= returnThreshold)
            {
                // Debug.Log($"Пчела набрала нектар: {nectarCollected:F1}/{returnThreshold:F1}");
            }
        }
        
        /// <summary>
        /// Задать профессию (вызывается при превращении личинки)
        /// </summary>
        public void AssignProfession(BeeProfession newProfession, int level = 1)
        {
            profession = newProfession;
            professionLevel = Mathf.Max(1, level);
            ApplyProfessionStats();
        }
        
        /// <summary>
        /// Отправить пчелу на сбор к конкретному цветку
        /// </summary>
        public void SendToFlower(FlowerController flower)
        {
            targetFlower = flower;
            SetState(BeeState.Foraging);
        }
        
        /// <summary>
        /// Вернуть пчелу в улей (вызывается при смене сезона и т.д.)
        /// </summary>
        public void RecallToHive()
        {
            SetState(BeeState.Returning);
        }
        
        void OnDrawGizmosSelected()
        {
            // Состояние
            Gizmos.color = CurrentState switch
            {
                BeeState.Idle => Color.gray,
                BeeState.Foraging => Color.green,
                BeeState.Returning => Color.blue,
                BeeState.Resting => Color.yellow,
                BeeState.Escaping => Color.red,
                BeeState.Guarding => Color.cyan,
                _ => Color.white
            };
            Gizmos.DrawWireSphere(transform.position, 0.8f);
            
            // Цель сбора
            if (targetFlower != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, targetFlower.transform.position);
            }
            
            // Радиус поиска
            if (CurrentState == BeeState.Foraging)
            {
                Gizmos.color = new Color(0f, 1f, 0f, 0.1f);
                Gizmos.DrawWireSphere(transform.position, searchRadius);
            }
        }
        
        private float maxEnergy = 100f;
    }
}
