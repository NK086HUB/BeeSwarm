using System.Collections.Generic;
using UnityEngine;

namespace BeeSwarm.Resources
{
    /// <summary>
    /// Типы ресурсов в игре
    /// </summary>
    public enum ResourceType
    {
        // Основные ресурсы
        Honey,
        Pollen,
        Wax,
        Water,
        Nectar,
        
        // Специальные ресурсы
        RoyalJelly,    // Маточное молочко
        Propolis,      // Прополис
        BeeBread,      // Перга
        
        // Виды мёда
        FlowerHoney,   // Цветочный мёд
        ForestHoney,   // Лесной мёд
        MeadowHoney,   // Луговой мёд
        
        // Виды пыльцы
        MeadowPollen,  // Луговая пыльца
        FlowerPollen,  // Цветочная пыльца
        PinePollen     // Хвойная пыльца
    }
    
    /// <summary>
    /// Система управления ресурсами
    /// </summary>
    public class ResourceSystem : MonoBehaviour
    {
        [System.Serializable]
        public class ResourceData
        {
            public ResourceType type;
            public float amount;
            public float maxCapacity;
            public Sprite icon;
            public Color color = Color.white;
            
            public ResourceData(ResourceType type, float maxCapacity)
            {
                this.type = type;
                this.amount = 0f;
                this.maxCapacity = maxCapacity;
            }
        }
        
        [Header("Настройки ресурсов")]
        [SerializeField] private List<ResourceData> resources = new List<ResourceData>();
        
        // Синглтон
        public static ResourceSystem Instance { get; private set; }
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeResources();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Инициализация ресурсов
        /// </summary>
        private void InitializeResources()
        {
            // Создаём базовые ресурсы если их нет
            if (resources.Count == 0)
            {
                resources.Add(new ResourceData(ResourceType.Honey, 1000f));
                resources.Add(new ResourceData(ResourceType.Pollen, 500f));
                resources.Add(new ResourceData(ResourceType.Wax, 300f));
                resources.Add(new ResourceData(ResourceType.Water, 200f));
                resources.Add(new ResourceData(ResourceType.Nectar, 1000f));
                
                // Спецресурсы
                resources.Add(new ResourceData(ResourceType.RoyalJelly, 50f));
                resources.Add(new ResourceData(ResourceType.Propolis, 100f));
                resources.Add(new ResourceData(ResourceType.BeeBread, 200f));
                
                // Виды мёда
                resources.Add(new ResourceData(ResourceType.FlowerHoney, 500f));
                resources.Add(new ResourceData(ResourceType.ForestHoney, 500f));
                resources.Add(new ResourceData(ResourceType.MeadowHoney, 500f));
                
                // Виды пыльцы
                resources.Add(new ResourceData(ResourceType.MeadowPollen, 300f));
                resources.Add(new ResourceData(ResourceType.FlowerPollen, 300f));
                resources.Add(new ResourceData(ResourceType.PinePollen, 300f));
            }
        }
        
        /// <summary>
        /// Получить количество ресурса
        /// </summary>
        public float GetResourceAmount(ResourceType type)
        {
            foreach (var resource in resources)
            {
                if (resource.type == type)
                {
                    return resource.amount;
                }
            }
            
            Debug.LogWarning($"Ресурс {type} не найден!");
            return 0f;
        }
        
        /// <summary>
        /// Получить максимальную вместимость ресурса
        /// </summary>
        public float GetResourceCapacity(ResourceType type)
        {
            foreach (var resource in resources)
            {
                if (resource.type == type)
                {
                    return resource.maxCapacity;
                }
            }
            
            return 0f;
        }
        
        /// <summary>
        /// Получить заполненность ресурса (0-1)
        /// </summary>
        public float GetResourceFillPercentage(ResourceType type)
        {
            float amount = GetResourceAmount(type);
            float capacity = GetResourceCapacity(type);
            
            if (capacity <= 0f) return 0f;
            return Mathf.Clamp01(amount / capacity);
        }
        
        /// <summary>
        /// Добавить ресурс
        /// </summary>
        public bool AddResource(ResourceType type, float amount)
        {
            for (int i = 0; i < resources.Count; i++)
            {
                if (resources[i].type == type)
                {
                    float newAmount = resources[i].amount + amount;
                    
                    if (newAmount > resources[i].maxCapacity)
                    {
                        // Переполнение - сохраняем только до максимума
                        resources[i].amount = resources[i].maxCapacity;
                        Debug.Log($"Ресурс {type} переполнен! Потеряно: {newAmount - resources[i].maxCapacity}");
                        return false;
                    }
                    
                    resources[i].amount = newAmount;
                    return true;
                }
            }
            
            // Ресурс не найден - создаём новый
            resources.Add(new ResourceData(type, 100f) { amount = amount });
            return true;
        }
        
        /// <summary>
        /// Потратить ресурс
        /// </summary>
        public bool ConsumeResource(ResourceType type, float amount)
        {
            for (int i = 0; i < resources.Count; i++)
            {
                if (resources[i].type == type)
                {
                    if (resources[i].amount >= amount)
                    {
                        resources[i].amount -= amount;
                        return true;
                    }
                    else
                    {
                        Debug.Log($"Недостаточно ресурса {type}: {resources[i].amount}/{amount}");
                        return false;
                    }
                }
            }
            
            Debug.LogWarning($"Ресурс {type} не найден для потребления!");
            return false;
        }
        
        /// <summary>
        /// Проверить, достаточно ли ресурса
        /// </summary>
        public bool HasEnoughResource(ResourceType type, float amount)
        {
            return GetResourceAmount(type) >= amount;
        }
        
        /// <summary>
        /// Установить максимальную вместимость ресурса
        /// </summary>
        public void SetResourceCapacity(ResourceType type, float capacity)
        {
            foreach (var resource in resources)
            {
                if (resource.type == type)
                {
                    resource.maxCapacity = Mathf.Max(capacity, 0f);
                    
                    // Если текущее количество больше новой вместимости - обрезаем
                    if (resource.amount > resource.maxCapacity)
                    {
                        resource.amount = resource.maxCapacity;
                    }
                    
                    return;
                }
            }
            
            // Ресурс не найден - создаём новый
            resources.Add(new ResourceData(type, capacity));
        }
        
        /// <summary>
        /// Получить все ресурсы
        /// </summary>
        public List<ResourceData> GetAllResources()
        {
            return new List<ResourceData>(resources);
        }
        
        /// <summary>
        /// Получить общее количество всех ресурсов
        /// </summary>
        public float GetTotalResourcesValue()
        {
            float total = 0f;
            foreach (var resource in resources)
            {
                total += resource.amount;
            }
            return total;
        }
        
        void OnGUI()
        {
            // UI для отладки ресурсов
            GUILayout.BeginArea(new Rect(10, 220, 400, 400));
            
            GUILayout.Label("РЕСУРСЫ УЛЬЯ");
            GUILayout.Label($"Всего ресурсов: {GetTotalResourcesValue():F1}");
            
            foreach (var resource in resources)
            {
                if (resource.amount > 0 || resource.maxCapacity > 0)
                {
                    float percentage = resource.maxCapacity > 0 ? resource.amount / resource.maxCapacity : 0f;
                    GUILayout.Label($"{resource.type}: {resource.amount:F1}/{resource.maxCapacity:F1} ({percentage:P0})");
                }
            }
            
            GUILayout.EndArea();
        }
    }
}
