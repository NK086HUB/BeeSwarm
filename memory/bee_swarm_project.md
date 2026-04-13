# Пчелиный рой - Unity проект

## 📋 Концепция проекта
- **Название:** Пчелиный рой (Bee Swarm)
- **Жанр:** Симулятор/стратегия
- **Основная механика:** Управление пчелиным роем с коллективным интеллектом

## 🎯 Цели проекта
- Создать реалистичную систему поведения пчёл
- Реализовать интересный геймплей вокруг управления роем
- Разработать визуально привлекательный стиль

## 🛠️ Техническая информация
- **Движок:** Unity
- **Язык программирования:** C#
- **Архитектура:** TBD

## 📁 Структура проекта
- **Assets/Scripts/** - C# скрипты
- **Assets/Scenes/** - Сцены игры
- **Assets/Materials/** - Материалы и текстуры
- **Assets/Prefabs/** - Префабы пчёл и объектов
- **Assets/Audio/** - Звуковые эффекты

## 🐝 Механики пчёл
### Поведение
- **Роевой интеллект:** Коллективное принятие решений
- **Патрулирование:** Облет территории
- **Сбор нектара:** Поиск и сбор ресурсов
- **Защита улья:** Оборонительные действия

### Типы пчёл в игре
- **Матка:** Руководитель роя, откладывает яйца
- **Уборщицы:** Чистят улей, удаляют мусор
- **Кормилицы:** Кормят личинок, ухаживают за маткой
- **Строители:** Строят и ремонтируют соты
- **Охранники:** Защищают улей от угроз
- **Сборщицы:** Собирают нектар и пыльцу
- **Трутни:** Самцы, для размножения
- **Личинки:** Неактивная стадия развития
- **Подростки:** Молодые пчёлы, учатся и работают

### Состояния пчёл
- **Идут за нектаром:** Поиск источников
- **Возвращение в улей:** Доставка ресурсов
- **Охрана:** Защита территории
- **Отдых:** Восстановление энергии

## 📦 Система ресурсов

### 🍯 Готовые продукты (хранятся в улье)

#### Мёд (3 вида с разными свойствами)
| Вид мёда | Источник | Свойства | Использование |
|----------|----------|----------|---------------|
| **🌼 Цветочный мёд** | Нектар цветов | Базовая питательность, быстрая переработка | Основной корм, торговля |
| **🌿 Лесной мёд** | Нектар лесных растений | +30% лечебный эффект, медленное потребление | Лечение болезней, зимний запас |
| **🌸 Луговой мёд** | Нектар луговых цветов | +20% энергетическая ценность | Кормление рабочих пчёл, спецкорм |

#### 🏗️ Строительные материалы
| Ресурс | Получение | Использование |
|--------|-----------|---------------|
| **🟡 Воск** | Вырабатывается пчёлами-строителями | Строительство сот, ремонт улья, запечатывание мёда |
| **🟤 Прополис** | Собирается с почек деревьев | Лечение пчёл, борьба с паразитами, дезинфекция улья |

#### 👑 Особые ресурсы
| Ресурс | Получение | Использование |
|--------|-----------|---------------|
| **⚪ Маточное молочко** | Вырабатывается молодыми пчёлами (5-15 дней) | Кормление матки и личинок, создание супер-кормилиц |
| **🟢 Перга** | Переработанная пыльца | Кормление личинок, зимний запас белков |

### 🌿 Добываемые ресурсы (приносят пчёлы)

#### Основные
| Ресурс | Источник | Особенности |
|--------|----------|-------------|
| **💧 Вода** | Родники, лужи, росы | Необходима для охлаждения улья, разведения мёда |
| **🌺 Нектар** | Цветы растений | Сырьё для производства мёда, разный у разных растений |
| **🌼 Пыльца** | 3 вида растений | Белковый корм, разный состав влияет на свойства |

#### Виды пыльцы
1. **🌿 Луговая пыльца** — Высокая питательность, быстрый рост расплода
2. **🌸 Цветочная пыльца** — Сбалансированный состав, общее здоровье пчёл  
3. **🌲 Хвойная пыльца** — Лечебные свойства, лечение болезней, устойчивость

#### 🗺️ Информационные ресурсы
| Ресурс | Добывают | Использование |
|--------|----------|---------------|
| **📍 Информация о ресурсах** | Пчёлы-разведчицы | **Игрок направляет в "туман войны" на глобальной карте** - открытие новых территорий, поиск источников |
| **⚠️ Информация об угрозах** | Пчёлы-охранники | **Сами патрулируют по очереди на близком расстоянии** - поднимают тревогу при обнаружении врагов |
| **🌡️ Погодная информация** | Все пчёлы | Планирование работ, подготовка к сезонным изменениям |

### 🔄 Процесс переработки
1. **Нектар** → **Мёд** (ферментация + выпаривание воды)
2. **Пыльца** → **Перга** (ферментация + мёд)
3. **Воск** ← Выработка восковых желез пчёл
4. **Прополис** ← Сбор смол с деревьев
5. **Маточное молочко** ← Выработка глоточных желез молодых пчёл

### 💾 Хранение ресурсов
- **Соты:** Мёд, перга, пыльца (запечатаны воском)
- **Спецхранилища:** Прополис, маточное молочко (требуют особых условий)
- **Вода:** Отдельные ячейки или внешние источники

### 🎮 Игровые механики ресурсов
1. **Качество ресурсов:** Зависит от источника и сезона
2. **Порча ресурсов:** Неправильное хранение → плесень, брожение
3. **Воровство:** Другие насекомые/животные могут красть ресурсы
4. **Обмен:** Торговля с другими ульями (в мультиплеере)
5. **Исследования:** Улучшение переработки через учёных пчёл

## 🗺️ Система разведки и патрулирования

### 🔍 Пчёлы-разведчицы
**Механика:** Игрок направляет разведчиц в "туман войны" на глобальной карте

#### Задачи разведчиц:
1. **Открытие территории:** Убирают "туман войны" на карте
2. **Поиск ресурсов:** Обнаруживают новые источники нектара, пыльцы, воды
3. **Разведка маршрутов:** Прокладывают безопасные пути к ресурсам
4. **Обнаружение угроз:** Находят гнёзда врагов, опасные зоны
5. **Картография:** Создают карту местности с отметками

#### Управление разведчицами:
```csharp
public class ScoutBee : MonoBehaviour {
    public MapFog fogOfWar;           // Система тумана войны
    public float explorationRadius = 50f; // Радиус разведки
    public List<ResourceSource> discoveredResources = new();
    public List<ThreatLocation> discoveredThreats = new();
    
    public void SendToExplore(Vector3 targetPosition) {
        // Игрок кликает на карте в зоне тумана войны
        MoveTo(targetPosition);
        StartCoroutine(ExploreArea(targetPosition));
    }
    
    IEnumerator ExploreArea(Vector3 center) {
        // Постепенное открытие территории
        float currentRadius = 0f;
        while (currentRadius < explorationRadius) {
            fogOfWar.RevealArea(center, currentRadius);
            currentRadius += 5f;
            yield return new WaitForSeconds(0.5f);
            
            // Поиск ресурсов в открытой области
            ScanForResources(center, currentRadius);
        }
        
        // Возвращение с информацией
        ReturnWithIntel();
    }
    
    void ScanForResources(Vector3 center, float radius) {
        Collider[] hits = Physics.OverlapSphere(center, radius);
        foreach (var hit in hits) {
            if (hit.TryGetComponent<ResourceSource>(out var resource)) {
                if (!discoveredResources.Contains(resource)) {
                    discoveredResources.Add(resource);
                    MapManager.Instance.AddResourceMarker(resource);
                }
            }
            
            if (hit.TryGetComponent<Threat>(out var threat)) {
                if (!discoveredThreats.Contains(threat.location)) {
                    discoveredThreats.Add(threat.location);
                    MapManager.Instance.AddThreatMarker(threat);
                }
            }
        }
    }
    
    void ReturnWithIntel() {
        // Передача информации улью
        HiveManager.Instance.ReceiveScoutReport(this);
        
        // Обновление карты игрока
        UIManager.Instance.UpdateMap(discoveredResources, discoveredThreats);
    }
}
```

### 🚨 Пчёлы-охранники (патрулирование)
**Механика:** Автономное патрулирование с настройкой игроком, требуют отдыха после дежурства

#### Система патрулирования:
1. **Настройка игроком:** Частота патрулей, количество охранников, зоны
2. **Ротация дежурств:** Охранники сменяют друг друга по графику с обязательным отдыхом
3. **Зоны патрулирования:** Определённый радиус вокруг улья, настраиваемый игроком
4. **Маршруты:** Предсказуемые пути для эффективного покрытия
5. **Реакция на угрозы:** Автоматическое поднятие тревоги
6. **Сезонность:** **Зимой патрулей нет** - все охранники внутри улья, вход закрыт

#### Код патрулирования с настройками игрока и отдыхом:
```csharp
public class GuardBee : MonoBehaviour {
    [Header("Настройки патрулирования (игрок может менять)")]
    public PatrolSettings patrolSettings;    // Настройки от игрока
    public float patrolRadius = 30f;         // Радиус вокруг улья (настраивается)
    public float threatDetectionRange = 20f; // Дальность обнаружения врагов
    
    [Header("Состояние охранника")]
    public GuardState currentState = GuardState.Idle;
    public bool isOnDuty = false;
    public float energy = 100f;              // Энергия охранника
    public float restTimeRemaining = 0f;     // Время отдыха после дежурства
    
    [Header("Дежурство")]
    public float dutyDuration = 120f;        // Длительность дежурства (сек)
    public float dutyTimeElapsed = 0f;
    public float patrolFrequency = 30f;      // Частота патрулей (сек между кругами)
    
    public List<Threat> detectedThreats = new();
    public AlarmSystem alarmSystem;
    
    void Start() {
        // Загрузка настроек игрока
        LoadPlayerSettings();
        
        // Автоматическое назначение в график дежурств
        GuardSchedule.Instance.RegisterGuard(this);
    }
    
    void Update() {
        // Обновление состояния в зависимости от сезона
        UpdateForSeason();
        
        // Обновление энергии и отдыха
        UpdateEnergyAndRest();
        
        // Автоматическое управление дежурством
        ManageDutyCycle();
    }
    
    void UpdateForSeason() {
        Season currentSeason = SeasonManager.Instance.currentSeason;
        
        if (currentSeason == Season.Winter) {
            // ЗИМА: нет патрулей, защита внутри улья
            currentState = GuardState.InsideHive;
            isOnDuty = false;
            
            // Закрытие входа/выхода
            HiveManager.Instance.CloseHiveEntrance();
            
            // Внутренняя охрана (от паразитов, поддержание температуры)
            PerformInternalGuardDuty();
            return;
        }
        
        // В другие сезоны - нормальная работа
        if (currentState == GuardState.InsideHive) {
            // Открытие улья весной
            HiveManager.Instance.OpenHiveEntrance();
            currentState = GuardState.Idle;
        }
    }
    
    void LoadPlayerSettings() {
        // Загрузка настроек из сохранения игрока
        PlayerSettings settings = SaveManager.Instance.LoadGuardSettings();
        
        patrolRadius = settings.patrolRadius;
        dutyDuration = settings.dutyDuration;
        patrolFrequency = settings.patrolFrequency;
        
        // Применение настроек профессии
        if (profession == BeeProfession.EliteGuard) {
            patrolRadius *= 1.5f;           // Элитные видят дальше
            threatDetectionRange *= 1.3f;
        }
    }
    
    void UpdateEnergyAndRest() {
        if (isOnDuty) {
            // Расход энергии на дежурстве
            energy -= Time.deltaTime * 0.5f; // 0.5 энергии в секунду
            
            if (energy <= 30f) {
                // Низкая энергия - снижение эффективности
                threatDetectionRange *= 0.7f;
            }
            
            if (energy <= 10f) {
                // Критически низкая энергия - принудительный отдых
                EndDutyForRest();
            }
        } else if (restTimeRemaining > 0f) {
            // Отдых
            restTimeRemaining -= Time.deltaTime;
            energy += Time.deltaTime * 2f; // Восстановление энергии
            
            if (energy > 100f) energy = 100f;
            
            if (restTimeRemaining <= 0f) {
                // Отдых завершён
                currentState = GuardState.Idle;
                restTimeRemaining = 0f;
            }
        }
    }
    
    void ManageDutyCycle() {
        if (!isOnDuty && currentState == GuardState.Idle && restTimeRemaining <= 0f) {
            // Автоматическое начало дежурства по графику
            if (GuardSchedule.Instance.ShouldStartDuty(this)) {
                StartDuty();
            }
        }
        
        if (isOnDuty) {
            dutyTimeElapsed += Time.deltaTime;
            
            // Проверка окончания дежурства
            if (dutyTimeElapsed >= dutyDuration) {
                EndDutyForRest();
            }
            
            // Патрулирование с заданной частотой
            if (Time.time % patrolFrequency < Time.deltaTime) {
                StartCoroutine(PatrolCycle());
            }
        }
    }
    
    public void StartDuty() {
        if (currentSeason == Season.Winter) {
            Debug.Log("Зимой дежурства на улице отменены");
            return;
        }
        
        isOnDuty = true;
        dutyTimeElapsed = 0f;
        currentState = GuardState.OnDuty;
        
        Debug.Log($"{gameObject.name} начал дежурство");
    }
    
    public void EndDutyForRest() {
        isOnDuty = false;
        dutyTimeElapsed = 0f;
        
        // Расчёт времени отдыха в зависимости от усталости
        float fatigue = 1f - (energy / 100f);
        restTimeRemaining = dutyDuration * 0.5f * fatigue; // Отдых = 50% от времени дежурства
        
        currentState = GuardState.Resting;
        
        Debug.Log($"{gameObject.name} закончил дежурство, отдыхает {restTimeRemaining:F0} сек");
    }
    
    IEnumerator PatrolCycle() {
        if (!isOnDuty || currentState != GuardState.OnDuty) yield break;
        
        // Создание случайного маршрута патрулирования
        Vector3[] patrolPoints = GeneratePatrolRoute();
        
        foreach (var point in patrolPoints) {
            if (!isOnDuty) yield break; // Прерывание если дежурство закончилось
            
            MoveTo(point);
            
            // Ожидание на точке (осмотр территории)
            yield return new WaitForSeconds(patrolSettings.pointWaitTime);
            
            // Сканирование на угрозы
            ScanForThreats();
            
            // Если обнаружена угроза - прерывание патруля
            if (detectedThreats.Count > 0) {
                RaiseAlarm();
                yield break;
            }
        }
    }
    
    Vector3[] GeneratePatrolRoute() {
        // Генерация случайного маршрута в пределах радиуса
        int pointCount = Random.Range(3, 6);
        Vector3[] points = new Vector3[pointCount];
        
        Vector3 hiveCenter = HiveManager.Instance.GetHiveCenter();
        
        for (int i = 0; i < pointCount; i++) {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(patrolRadius * 0.3f, patrolRadius);
            
            points[i] = hiveCenter + new Vector3(
                Mathf.Cos(angle) * distance,
                0f,
                Mathf.Sin(angle) * distance
            );
        }
        
        return points;
    }
    
    void ScanForThreats() {
        Collider[] hits = Physics.OverlapSphere(transform.position, threatDetectionRange);
        foreach (var hit in hits) {
            if (hit.TryGetComponent<Threat>(out var threat)) {
                if (!detectedThreats.Contains(threat)) {
                    detectedThreats.Add(threat);
                    Debug.Log($"Обнаружена угроза: {threat.name}");
                }
            }
        }
    }
    
    void PerformInternalGuardDuty() {
        // Зимняя внутренняя охрана
        if (currentSeason != Season.Winter) return;
        
        // Проверка температуры в улье
        float hiveTemp = HiveManager.Instance.GetTemperature();
        if (hiveTemp < 10f) {
            // Помощь в поддержании температуры
            HelpMaintainTemperature();
        }
        
        // Проверка на внутренних паразитов
        CheckForInternalParasites();
        
        // Защита запасов от плесени/порчи
        ProtectFoodStores();
    }
    
    void HelpMaintainTemperature() {
        // Помощь в поддержании температуры зимой
        if (energy > 20f) {
            energy -= 5f;
            HiveManager.Instance.AddHeat(2f);
        }
    }
    
    void CheckForInternalParasites() {
        // Сканирование на внутренних паразитов (клещи, моль)
        Collider[] hits = Physics.OverlapSphere(transform.position, 5f);
        foreach (var hit in hits) {
            if (hit.TryGetComponent<Parasite>(out var parasite)) {
                // Атака паразита
                Attack(parasite);
            }
        }
    }
    
    void ProtectFoodStores() {
        // Защита запасов мёда от порчи
        FoodStorage[] storages = FindObjectsOfType<FoodStorage>();
        foreach (var storage in storages) {
            if (storage.IsContaminated()) {
                // Очистка запасов
                storage.CleanContamination();
                energy -= 3f;
            }
        }
    }
    
    void RaiseAlarm() {
        // Поднятие тревоги
        Season currentSeason = SeasonManager.Instance.currentSeason;
        
        if (currentSeason == Season.Winter) {
            Debug.Log("Зимняя тревога! Все охранники могут выйти для защиты");
            // Зимой охранники могут выйти по тревоге
            HiveManager.Instance.OpenHiveEntranceForEmergency();
            return;
        }
        
        // Поднятие тревоги
        alarmSystem.ActivateAlarm(detectedThreats);
        
        // Сбор других охранников
        CallForBackup();
        
        // Атака или защита в зависимости от типа угрозы
        EngageThreats();
    }
    
    void CallForBackup() {
        // Вызов других охранников
        foreach (var guard in GuardSchedule.Instance.activeGuards) {
            if (guard != this && guard.isOnDuty) {
                guard.RespondToAlarm(transform.position, detectedThreats);
            }
        }
        
        // Вызов универсальных пчёл для помощи
        HiveManager.Instance.MobilizeBeesForDefense();
    }
    
    void EngageThreats() {
        // Атака или защита в зависимости от профессии
        if (profession == BeeProfession.EliteGuard) {
            // Элитные охранники атакуют активно
            foreach (var threat in detectedThreats) {
                Attack(threat);
            }
        } else {
            // Обычные охранники защищают улей
            DefendHive();
        }
    }
    
    public void RespondToAlarm(Vector3 alarmPosition, List<Threat> threats) {
        // Прерывание текущего патруля
        StopAllCoroutines();
        
        // Перемещение к месту тревоги
        MoveTo(alarmPosition);
        
        // Добавление угроз в список
        detectedThreats.AddRange(threats);
        
        // Включение в оборону
        EngageThreats();
    }
}

// Настройки патрулирования от игрока
[System.Serializable]
public class PatrolSettings {
    [Header("Основные настройки")]
    [Range(10f, 100f)] public float patrolRadius = 30f;
    [Range(60f, 600f)] public float dutyDuration = 120f;    // Секунды
    [Range(10f, 120f)] public float patrolFrequency = 30f;  // Секунды между патрулями
    [Range(1, 10)] public int guardsOnDuty = 3;             // Одновременно на дежурстве
    
    [Header("Дополнительные настройки")]
    [Range(1f, 10f)] public float pointWaitTime = 3f;       // Ожидание на точке
    public bool aggressivePatrol = false;                   // Активный поиск врагов
    public bool callForHelp = true;                         // Вызов подкрепления
    
    [Header("Сезонные настройки")]
    public bool winterGuardEnabled = true;                  // Внутренняя охрана зимой
    public float winterRestMultiplier = 2f;                 // Множитель отдыха зимой
}

// Состояния охранника
public enum GuardState {
    Idle,           // Ожидание
    OnDuty,         // На дежурстве
    Patrolling,     // В патруле
    Resting,        // Отдых после дежурства
    Alert,          // Тревога
    Attacking,      // Атака
    InsideHive,     // Внутри улья (зима)
    Defending       // Защита улья
}

// Система управления ульем (дополнение для зимней защиты)
public class HiveManager : MonoBehaviour {
    public bool isHiveEntranceOpen = true;
    public float internalTemperature = 25f;
    
    public void CloseHiveEntrance() {
        isHiveEntranceOpen = false;
        
        // Визуальное закрытие входа
        EntranceController.Instance.Close();
        
        // Закрытие для внешних угроз
        foreach (var threat in FindObjectsOfType<ExternalThreat>()) {
            threat.CannotEnterHive();
        }
        
        Debug.Log("Вход в улей закрыт на зиму");
    }
    
    public void OpenHiveEntrance() {
        isHiveEntranceOpen = true;
        
        // Визуальное открытие входа
        EntranceController.Instance.Open();
        
        Debug.Log("Вход в улей открыт");
    }
    
    public void OpenHiveEntranceForEmergency() {
        // Экстренное открытие улья (зимняя тревога)
        isHiveEntranceOpen = true;
        
        // Визуальное открытие с аварийными эффектами
        EntranceController.Instance.OpenEmergency();
        
        // Разрешить выход только охранникам
        SetEmergencyExitMode(true);
        
        Debug.Log("ЭКСТРЕННОЕ открытие улья! Охранники могут выйти");
        
        // Автоматическое закрытие через время
        StartCoroutine(CloseAfterEmergency());
    }
    
    IEnumerator CloseAfterEmergency() {
        yield return new WaitForSeconds(300f); // 5 минут
        
        if (ThreatManager.Instance.GetActiveThreatCount() == 0) {
            CloseHiveEntrance();
            SetEmergencyExitMode(false);
            Debug.Log("Угроза устранена, улей закрыт");
        }
    }
    
    void SetEmergencyExitMode(bool emergency) {
        // Режим экстренного выхода (только охранники)
        foreach (var bee in GetAllBees()) {
            if (bee is GuardBee) {
                bee.canExitDuringWinter = emergency;
            } else {
                bee.canExitDuringWinter = false;
            }
        }
    }
    
    public void AddHeat(float amount) {
        // Добавление тепла от пчёл (зимний клуб)
        internalTemperature += amount;
        internalTemperature = Mathf.Clamp(internalTemperature, 5f, 40f);
    }
    
    public float GetTemperature() {
        return internalTemperature;
    }
    
    public bool CanBeeExit(Bee bee) {
        // Проверка может ли пчела выйти из улья
        
        Season currentSeason = SeasonManager.Instance.currentSeason;
        
        // Зимний режим
        if (currentSeason == Season.Winter) {
            if (!isHiveEntranceOpen) {
                // Улей закрыт, но может быть экстренное открытие
                if (bee.canExitDuringWinter && bee is GuardBee) {
                    return true; // Охранники могут выйти в экстренном режиме
                }
                Debug.Log($"{bee.name} не может выйти - улей закрыт на зиму");
                return false;
            }
            
            // Улей открыт (экстренный режим)
            if (bee is GuardBee guard && guard.currentState == GuardState.Alert) {
                return true; // Все охранники могут выйти по тревоге
            }
            return false;
        }
        
        // Другие сезоны - нормальный режим
        if (!isHiveEntranceOpen) {
            Debug.Log($"{bee.name} не может выйти - улей закрыт");
            return false;
        }
        
        return true;
    }
}
```

### 🗺️ Глобальная карта и туман войны
```csharp
public class MapManager : MonoBehaviour {
    public Texture2D fogOfWarTexture;    // Текстура тумана войны
    public float[,] visibilityGrid;      // Сетка видимости (0-1)
    public List<MapMarker> markers = new();
    
    public void RevealArea(Vector3 worldPos, float radius) {
        // Конвертация мировых координат в координаты текстуры
        Vector2Int texCoord = WorldToTextureCoord(worldPos);
        int texRadius = Mathf.RoundToInt(radius * pixelsPerUnit);
        
        // Обновление текстуры тумана войны
        for (int x = -texRadius; x <= texRadius; x++) {
            for (int y = -texRadius; y <= texRadius; y++) {
                int texX = texCoord.x + x;
                int texY = texCoord.y + y;
                
                if (IsInTextureBounds(texX, texY)) {
                    float distance = Mathf.Sqrt(x*x + y*y) / texRadius;
                    float alpha = Mathf.Clamp01(distance);
                    
                    // Уменьшение непрозрачности тумана
                    Color current = fogOfWarTexture.GetPixel(texX, texY);
                    current.a = Mathf.Min(current.a, alpha);
                    fogOfWarTexture.SetPixel(texX, texY, current);
                }
            }
        }
        
        fogOfWarTexture.Apply();
        UpdateVisibilityGrid();
    }
    
    public void AddResourceMarker(ResourceSource resource) {
        MapMarker marker = new MapMarker {
            type = MarkerType.Resource,
            position = resource.transform.position,
            data = resource.GetResourceData(),
            icon = resource.GetIcon()
        };
        markers.Add(marker);
        UpdateMinimap();
    }
    
    public void AddThreatMarker(Threat threat) {
        MapMarker marker = new MapMarker {
            type = MarkerType.Threat,
            position = threat.transform.position,
            data = threat.GetThreatData(),
            icon = threat.GetIcon(),
            dangerLevel = threat.dangerLevel
        };
        markers.Add(marker);
        UpdateMinimap();
        
        // Автоматическое предупреждение игрока
        if (threat.dangerLevel >= DangerLevel.High) {
            UIManager.Instance.ShowThreatWarning(threat);
        }
    }
}
```

### 🎮 Игровой процесс разведки и защиты
1. **Ранняя игра:** Мало разведчиц → маленькая карта → риск неожиданных атак
2. **Расширение:** Больше разведчиц → больше территории → больше ресурсов
3. **Оборона:** Баланс между охранниками и сборщицами
4. **Тактика:** Разведка перед отправкой сборщиц на новые территории
5. **Сезонные изменения:** 
   - **Зимой:** **Нет внешних патрулей**, вход/выход закрыты, охрана внутри улья
   - **Весной:** Усиленные патрули (много врагов после зимы)
   - **Летом:** Стандартное патрулирование, защита от ос/пауков
   - **Осенью:** Уменьшение патрулей, подготовка к зиме

### ⚖️ Баланс системы
- **Стоимость разведки:** Разведчицы не собирают ресурсы, только информацию
- **Риск:** Разведчицы могут быть атакованы в неизвестных территориях
- **Выгода:** Открытие богатых ресурсных зон, избегание опасностей
- **Автоматизация:** Охранники работают самостоятельно с настройками игрока
- **Ресурсы на содержание:** Охранники требуют отдыха после дежурства (+ мёд на восстановление)
- **Сезонная адаптация:** Зимой - внутренняя охрана, нет внешних патрулей

## 🔄 Цикл жизни рабочей пчелы

### 📅 Развитие (21 день)
| День | Стадия | Описание |
|------|--------|----------|
| 1-3 | 🥚 **Яйцо** | Отложено маткой, развитие эмбриона |
| 4-9 | 🐛 **Личинка** | **Ключевая стадия!** Игрок выбирает профессию и кормит специальным кормом |
| 10-20 | 🦋 **Куколка** | Запечатана в ячейке, метаморфоза с учётом выбранной профессии |
| 21 | 🐝 **Выход** | Пчела рождается с готовыми умениями выбранной профессии |

### 🎯 Профессиональная специализация (ВЫБОР ИГРОКА)

#### ⚡ Быстрая сборщица (18 дней)
- **Корм:** Цветочный нектар + Цветочный мёд
- **Бонусы:** +50% скорость полёта, +30% грузоподъёмность, лучше находит ресурсы
- **Способность:** "Сверхскоростной полёт" - временное ускорение ×2
- **Стратегия:** Максимальный сбор летом, быстрая доставка

#### 🛡️ Элитный охранник (24 дня)
- **Корм:** Прополис + Луговая пыльца  
- **Бонусы:** +100% урон, +50% защита улья, отпугивание врагов
- **Способность:** "Щит улья" - временная неуязвимость для всех пчёл в радиусе
- **Стратегия:** Защита от ос, пауков, медведей, лечение раненных

#### 🏗️ Мастер-строитель (22 дня)
- **Корм:** Воск + Луговой мёд
- **Бонусы:** +70% скорость строительства, прочные соты, экономия воска
- **Способность:** "Массовое строительство" - ускорение стройки ×3 на 10 сек
- **Стратегия:** Быстрое расширение улья весной, ремонт после атак

#### 👑 Супер-кормилица (20 дней)
- **Корм:** Маточное молочко + Цветочная пыльца
- **Бонусы:** +100% эффективность кормления, здоровый расплод, лечение болезней
- **Способность:** "Исцеляющий нектар" - лечение других пчёл, снятие отравлений
- **Стратегия:** Быстрый рост популяции, поддержание здоровья улья

#### 🔬 Учёная пчела (26 дней)
- **Корм:** Хвойная пыльца + Лесной мёд
- **Бонусы:** Исследования, улучшения, открытие технологий, анализ ресурсов
- **Способность:** "Научный прорыв" - ускорение исследований ×2, открытие новых рецептов
- **Стратегия:** Долгосрочное развитие улья, создание улучшенных ресурсов

#### 🐝 Универсальная пчела (21 день)
- **Корм:** Любая пыльца + Любой мёд
- **Бонусы:** Нет специализации, но дешёвая, может выполнять любую работу
- **Способность:** Нет
- **Стратегия:** Базовая рабочая сила, массовое производство, заполнение пробелов

### ⏳ Продолжительность жизни
| Профессия | Продолжительность | Износ | Энергопотребление |
|-----------|-------------------|-------|-------------------|
| **⚡ Быстрая сборщица** | 25-35 дней | Высокий | Высокое |
| **🛡️ Элитный охранник** | 40-60 дней | Средний | Высокое |
| **🏗️ Мастер-строитель** | 50-70 дней | Низкий | Среднее |
| **👑 Супер-кормилица** | 30-45 дней | Средний | Среднее |
| **🔬 Учёная пчела** | 60-90 дней | Низкий | Низкое |
| **🐝 Универсальная** | 22-45 дней | Зависит от сезона | Среднее |

### 🌿 Сезонное влияние
| Сезон | Модификатор жизни | Причина |
|-------|-------------------|---------|
| **Весна** | ×0.8 | Максимальная нагрузка |
| **Лето** | ×1.0 | Стандартные условия |
| **Осень** | ×1.5 | Подготовка к зиме |
| **Зима** | ×3.0 | Экономия энергии, мало работы |

### 🎮 Игровые механики профессиональной специализации

#### 🎯 Система выбора профессии
```csharp
public enum BeeProfession {
    FastForager,      // ⚡ Быстрая сборщица
    EliteGuard,       // 🛡️ Элитный охранник  
    MasterBuilder,    // 🏗️ Мастер-строитель
    SuperNurse,       // 👑 Супер-кормилица
    Scientist,        // 🔬 Учёная пчела
    Generalist        // 🐝 Универсальная
}

public class LarvaSpecialization : MonoBehaviour {
    [Header("Статус личинки")]
    public Larva larva;
    public BeeProfession chosenProfession = BeeProfession.Generalist;
    public bool isSpecializationComplete = false;
    
    [Header("Требования к кормлению")]
    public Dictionary<ResourceType, float> requiredFood = new();
    public Dictionary<ResourceType, float> currentFood = new();
    
    [Header("Настройки профессии")]
    public float developmentTimeMultiplier = 1f;
    public ProfessionStats professionStats;
    public List<SpecialAbility> specialAbilities = new();
    
    // События
    public event Action<BeeProfession> OnProfessionChosen;
    public event Action OnSpecializationComplete;
    public event Action<float> OnFeedingProgress; // 0-1
    
    public void ChooseProfession(BeeProfession profession) {
        if (larva.developmentStage != DevelopmentStage.Larva) {
            Debug.LogWarning("Можно выбирать профессию только на стадии личинки!");
            return;
        }
        
        chosenProfession = profession;
        SetupProfessionRequirements(profession);
        
        // Визуальные эффекты выбора
        ShowProfessionSelectionUI();
        OnProfessionChosen?.Invoke(profession);
    }
    
    void SetupProfessionRequirements(BeeProfession profession) {
        requiredFood.Clear();
        currentFood.Clear();
        
        switch(profession) {
            case BeeProfession.FastForager:
                requiredFood[ResourceType.FlowerNectar] = 50f;
                requiredFood[ResourceType.Honey] = 30f;
                developmentTimeMultiplier = 0.85f; // 18 дней вместо 21
                break;
                
            case BeeProfession.EliteGuard:
                requiredFood[ResourceType.Propolis] = 40f;
                requiredFood[ResourceType.Pollen] = 60f;
                developmentTimeMultiplier = 1.14f; // 24 дня
                break;
                
            case BeeProfession.MasterBuilder:
                requiredFood[ResourceType.Wax] = 35f;
                requiredFood[ResourceType.Honey] = 45f;
                developmentTimeMultiplier = 1.05f; // 22 дня
                break;
                
            case BeeProfession.SuperNurse:
                requiredFood[ResourceType.RoyalJelly] = 25f;
                requiredFood[ResourceType.Pollen] = 40f;
                developmentTimeMultiplier = 0.95f; // 20 дней
                break;
                
            case BeeProfession.Scientist:
                requiredFood[ResourceType.RareHerbs] = 30f;
                requiredFood[ResourceType.Honey] = 35f;
                developmentTimeMultiplier = 1.24f; // 26 дней
                break;
                
            case BeeProfession.HiveManager:
                requiredFood[ResourceType.FlowerNectar] = 20f;
                requiredFood[ResourceType.Pollen] = 20f;
                requiredFood[ResourceType.Honey] = 20f;
                requiredFood[ResourceType.Propolis] = 10f;
                developmentTimeMultiplier = 1.19f; // 25 дней
                break;
                
            case BeeProfession.Generalist:
                requiredFood[ResourceType.Pollen] = 30f;
                requiredFood[ResourceType.Honey] = 20f;
                developmentTimeMultiplier = 1.0f; // 21 день
                break;
        }
        
        // Инициализация текущих значений
        foreach (var kvp in requiredFood) {
            currentFood[kvp.Key] = 0f;
        }
    }
    
    public bool FeedLarva(ResourceType foodType, float amount) {
        if (!requiredFood.ContainsKey(foodType)) {
            Debug.LogWarning($"Этот корм не нужен для профессии {chosenProfession}");
            return false;
        }
        
        currentFood[foodType] += amount;
        
        // Проверка перекорма
        if (currentFood[foodType] > requiredFood[foodType] * 1.2f) {
            Debug.LogWarning("Перекорм! Может вызвать мутации.");
            // Шанс негативной мутации
        }
        
        // Обновление прогресса
        float progress = CalculateFeedingProgress();
        OnFeedingProgress?.Invoke(progress);
        
        // Проверка завершения
        if (IsFeedingComplete()) {
            CompleteSpecialization();
            return true;
        }
        
        return true;
    }
    
    float CalculateFeedingProgress() {
        if (requiredFood.Count == 0) return 0f;
        
        float totalRequired = 0f;
        float totalCurrent = 0f;
        
        foreach (var kvp in requiredFood) {
            totalRequired += kvp.Value;
            totalCurrent += Mathf.Min(currentFood[kvp.Key], kvp.Value);
        }
        
        return totalCurrent / totalRequired;
    }
    
    bool IsFeedingComplete() {
        foreach (var kvp in requiredFood) {
            if (currentFood[kvp.Key] < kvp.Value) {
                return false;
            }
        }
        return true;
    }
    
    void CompleteSpecialization() {
        isSpecializationComplete = true;
        
        // Применение бонусов профессии
        ApplyProfessionBonuses();
        
        // Настройка специальных способностей
        SetupSpecialAbilities();
        
        // Событие завершения
        OnSpecializationComplete?.Invoke();
        
        Debug.Log($"Личинка успешно специализирована как {chosenProfession}!");
    }
    
    void ApplyProfessionBonuses() {
        // Получение настроек профессии из конфига
        ProfessionSettings settings = ProfessionBalanceConfig.Instance.GetSettings(chosenProfession);
        
        // Применение статистик
        professionStats = settings.baseStats;
        
        // Модификация времени развития
        larva.developmentTime *= developmentTimeMultiplier;
    }
    
    void SetupSpecialAbilities() {
        ProfessionSettings settings = ProfessionBalanceConfig.Instance.GetSettings(chosenProfession);
        
        foreach (var abilityData in settings.abilities) {
            SpecialAbility ability = new SpecialAbility(abilityData);
            specialAbilities.Add(ability);
        }
    }
}
```

#### 🧬 Профессиональные статистики
```csharp
[System.Serializable]
public class ProfessionStats {
    [Header("Основные характеристики")]
    [Range(0.5f, 3f)] public float flightSpeedMultiplier = 1f;
    [Range(0.5f, 3f)] public float carryCapacityMultiplier = 1f;
    [Range(0.5f, 3f)] public float buildSpeedMultiplier = 1f;
    [Range(0.5f, 3f)] public float attackDamageMultiplier = 1f;
    [Range(0.5f, 3f)] public float feedEfficiencyMultiplier = 1f;
    [Range(0.5f, 3f)] public float researchSpeedMultiplier = 1f;
    
    [Header("Вторичные характеристики")]
    [Range(0.5f, 3f)] public float energyConsumptionMultiplier = 1f;
    [Range(0.5f, 3f)] public float wearRateMultiplier = 1f;
    [Range(0.5f, 3f)] public float lifeSpanMultiplier = 1f;
    [Range(0.5f, 3f)] public float foodConsumptionMultiplier = 1f;
    
    [Header("Специальные бонусы")]
    public float threatDetectionRange = 10f;      // Для охранников
    public float constructionQuality = 1f;        // Для строителей
    public float healingPower = 0f;               // Для кормилиц
    public float researchBonus = 0f;              // Для учёных
}

[System.Serializable]
public class SpecialAbility {
    public string abilityName;
    public string description;
    public Sprite icon;
    
    [Header("Механика")]
    public float cooldown = 30f;
    public float duration = 10f;
    public float energyCost = 20f;
    
    [Header("Эффекты")]
    public StatModifier[] statModifiers;
    public GameObject visualEffect;
    public AudioClip soundEffect;
    
    public Action onActivate;
    public Action onDeactivate;
    
    private float currentCooldown = 0f;
    private bool isActive = false;
    
    public bool CanActivate(Bee bee) {
        return currentCooldown <= 0f && bee.energy >= energyCost;
    }
    
    public void Activate(Bee bee) {
        if (!CanActivate(bee)) return;
        
        bee.energy -= energyCost;
        isActive = true;
        currentCooldown = cooldown;
        
        // Применение модификаторов
        foreach (var modifier in statModifiers) {
            bee.ApplyStatModifier(modifier, duration);
        }
        
        // Визуальные и звуковые эффекты
        if (visualEffect != null) {
            GameObject effect = Instantiate(visualEffect, bee.transform.position, Quaternion.identity);
            Destroy(effect, duration);
        }
        
        onActivate?.Invoke();
        
        // Запуск таймера деактивации
        bee.StartCoroutine(DeactivateAfterDuration(bee, duration));
    }
    
    IEnumerator DeactivateAfterDuration(Bee bee, float waitTime) {
        yield return new WaitForSeconds(waitTime);
        Deactivate(bee);
    }
    
    void Deactivate(Bee bee) {
        isActive = false;
        
        // Снятие модификаторов
        foreach (var modifier in statModifiers) {
            bee.RemoveStatModifier(modifier);
        }
        
        onDeactivate?.Invoke();
    }
    
    void UpdateCooldown(float deltaTime) {
        if (currentCooldown > 0f) {
            currentCooldown -= deltaTime;
        }
    }
}
```

#### ⚖️ Стратегические решения
1. **Ресурсный менеджмент:** Редкий прополис на охранников или учёных?
2. **Временные затраты:** Быстрые сборщицы (18 дней) vs учёные (26 дней)
3. **Сезонное планирование:** Весной — строители, летом — сборщицы
4. **Баланс профессий:** Оптимальное соотношение для улья
5. **Экстренная переквалификация:** Штраф при смене профессии

#### 📊 Баланс профессий
| Профессия | Стоимость | Время | Эффективность | Спецспособность |
|-----------|-----------|-------|---------------|-----------------|
| **⚡ Сборщица** | Средняя | 18д | ×1.8 | Временное ускорение |
| **🛡️ Охранник** | Высокая | 24д | ×2.2 | Щит улья |
| **🏗️ Строитель** | Средняя | 22д | ×1.7 | Массовая стройка |
| **👑 Кормилица** | Высокая | 20д | ×2.0 | Исцеление |
| **🔬 Учёный** | Очень высокая | 26д | ×1.5 | Исследования |
| **💼 Менеджер** | Высокая | 25д | ×1.3 | Буст команды |
| **🐝 Универсальная** | Низкая | 21д | ×1.0 | Нет |

### 🧪 Настройки для балансировки
```csharp
// Конфигурационный файл для тестирования
[System.Serializable]
public class BeeBalanceConfig {
    // Временные параметры
    public float dayDuration = 60f;          // 1 игровой день = 60 секунд
    public int developmentDays = 21;         // Дней развития от яйца
    public int[] roleDays = { 2, 4, 6, 11, 20, 21 }; // Переходы между ролями
    
    // Параметры износа
    public float[] wearRates = { 0.1f, 0.3f, 0.5f, 0.8f, 1.0f, 2.5f };
    public float seasonalModifier = 1.0f;    // Модификатор по сезонам
    
    // Продуктивность
    public float[] efficiency = { 0f, 0.3f, 0.5f, 0.7f, 0.8f, 1.0f };
    public float nectarPerTrip = 40f;        // мг нектара за полёт
    public float pollenPerTrip = 15f;        // мг пыльцы за полёт
}
```

### 💡 Идеи для геймплея
1. **Ускорение времени:** Игрок может ускорять/замедлять время для тестирования
2. **Генетические улучшения:** Увеличивать продолжительность жизни через исследования
3. **Специализация:** Фокусировка на определённых ролях через тренировку
4. **Больные пчёлы:** Заболевания влияют на продуктивность и износ
5. **Пенсия:** Старые пчёлы могут стать "наставниками" для молодых

## 🎮 Геймплей элементы
- **Ресурсы:** Нектар, пыльца, воск
- **Уровни:** Развитие улья
- **Враги:** Осы, пауки, другие угрозы
- **Апгрейды:** Улучшения для роя

## 📈 Прогресс проекта
### ✅ Завершено
- [x] Концепция утверждена
- [x] Базовая архитектура
- [x] Детализация сезонной механики
- [x] Цикл жизни пчёл с профессиональной специализацией
- [x] Система выбора профессии игроком
- [x] Система ресурсов (3 вида мёда, 3 вида пыльцы, спецресурсы)

### 🚧 В работе
- [ ] Разработка системы поведения пчёл
- [ ] Создание визуальных ассетов
- [ ] Реализация системы сезонов
- [ ] Реализация системы кормления личинок
- [ ] Создание UI выбора профессий
- [ ] Система добычи и переработки ресурсов

### 🔮 Планируется
- [ ] Реализация зимней механики клуба
- [ ] Механика изгнания трутней
- [ ] Балансировка ресурсов по сезонам
- [ ] Балансировка профессий (стоимость/эффективность)
- [ ] Специальные способности для каждой профессии
- [ ] ✅ Пчёлы-разведчицы (информация о ресурсах) - ДОБАВЛЕНО
- [ ] ✅ Система патрулирования охранников с настройками игрока - ДОБАВЛЕНО
- [ ] ✅ Зимняя защита (вход закрыт, внутренняя охрана) - ДОБАВЛЕНО
- [ ] Система "тумана войны" на глобальной карте
- [ ] Тестирование и балансировка

## 💡 Идеи и заметки
- ✅ Добавить сезонные изменения (реализовано)
- ✅ Реализовать разные типы пчёл (рабочие, матка, трутни)
- ✅ Ввести систему погоды
- ✅ Добавить элементы выживания

## 🌿 Сезонная механика (детали)

### Весна (март-май)
- **🌸 Цветение растений** — много нектара
- **🐝 Активное размножение** — можно увеличить популяцию
- **🌧️ Дожди** — меньше времени на сбор
- **Задача:** Создать резервный запас, вырастить новых пчёл
- **Расход ресурсов:** 1.5x (выращивание расплода)

### Лето (июнь-август)
- **☀️ Жара** — пчёлы быстрее устают
- **🌼 Максимальный сбор** — пик продуктивности
- **🐛 Больше врагов** — осы, пауки активны
- **Задача:** Максимальный сбор, подготовка к зиме
- **Расход ресурсов:** 1x (норма)

### Осень (сентябрь-ноябрь)
- **🍂 Меньше цветов** — ресурсы скудеют
- **🌬️ Холодные ветра** — пчёлы медленнее
- **🍯 Проверка запасов** — достаточно ли на зиму?
- **👥 Изгнание трутней** — рабочие пчёлы выталкивают трутней для экономии ресурсов
- **Задача:** Завершить сбор, утеплить улей
- **Расход ресурсов:** 0.8x (экономия)

### Зима (декабрь-февраль)
- **❄️ Морозы** — пчёлы в спячке, собираются в "клуб" для согревания
- **🔄 Механика клуба:** внешние и внутренние пчёлы меняются местами, риск переохлаждения
- **🏠 Улей закрыт** — вход/выход закрыты, только внутренние работы
- **🚨 Экстренный выход:** При тревоге все охранники могут выйти для защиты
- **📉 Расход запасов** — контроль потребления, повышенный расход на обогрев
- **Задача:** Выжить до весны, минимизировать потери
- **Расход ресурсов:** 3-4x (обогрев)

## 🎮 Игровые механики сезонов
1. **Сезонный таймер:** 10-15 минут реального времени на сезон
2. **Запасы:** Нектар, пыльца, воск — должны хватить на зиму
3. **Температура:** Влияет на активность пчёл
4. **События:** Внезапные заморозки, засухи, дожди
5. **Зимний клуб:** Мини-игра поддержания температуры (20-35°C)

## ❄️ Механика зимнего согревания
### Клуб пчёл
- Пчёлы собираются в плотный шар вокруг матки
- Внешние пчёлы: Замерзают быстрее, но защищают внутренних
- Внутренние пчёлы: Теплее, должны периодически меняться местами

### Температурный баланс
- **Идеальная температура:** 20-35°C
- **Критическая температура:** <10°C (пчёлы начинают гибнуть)
- **Расход мёда:** Зависит от разницы с внешней температурой
- **Ротация:** Автоматическая или ручная смена позиций пчёл

### Псевдокод реализации
```csharp
public class WinterSurvival {
    float hiveTemperature;     // Текущая температура улья
    float honeyConsumption;    // Расход мёда в час
    float beeRotationTimer;    // Таймер смены позиций
    bool isBeeClusterFormed;   // Сформирован ли клуб
    
    void UpdateWinterMechanics() {
        if (hiveTemperature < 15f) {
            FormBeeCluster();
            honeyConsumption = CalculateHeatConsumption();
            RotateBeesIfNeeded();
        }
    }
}
```

## 🐝 Адаптация пчёл к сезонам
- **Зимние пчёлы:** Дольше живут (до 6 месяцев), но медленнее работают
- **Летние пчёлы:** Быстрые и продуктивные, но недолговечные (4-6 недель)
- **Стратегия:** Баланс между типами в зависимости от сезона
- **Осеннее событие:** Изгнание трутней для экономии зимних запасов

## 🔧 Технические решения
- Использовать Unity AI для навигации
- Реализовать систему событий для коммуникации пчёл
- Оптимизировать производительность для большого количества пчёл

### 🛠️ Реализация сезонной системы
```csharp
// Система сезонов
public enum Season { Spring, Summer, Autumn, Winter }
public class SeasonManager : MonoBehaviour {
    public Season currentSeason;
    public float seasonDuration = 900f; // 15 минут в секундах
    private float seasonTimer;
    
    // Модификаторы сезонов
    public float nectarMultiplier = 1f;
    public float beeSpeedMultiplier = 1f;
    public float threatMultiplier = 1f;
    public float resourceConsumptionMultiplier = 1f;
    
    void UpdateSeason() {
        seasonTimer += Time.deltaTime;
        if (seasonTimer >= seasonDuration) {
            ChangeSeason();
            seasonTimer = 0f;
        }
    }
    
    void ChangeSeason() {
        currentSeason = (Season)(((int)currentSeason + 1) % 4);
        ApplySeasonEffects();
    }
}

// Система зимнего выживания
public class WinterSurvivalSystem : MonoBehaviour {
    public float hiveTemperature = 25f;
    public float externalTemperature = -5f;
    public float honeyConsumptionRate = 1f;
    public bool isClusterFormed = false;
    
    public BeeCluster beeCluster; // Компонент клуба пчёл
    
    void Update() {
        if (SeasonManager.currentSeason == Season.Winter) {
            ManageWinterSurvival();
        }
    }
    
    void ManageWinterSurvival() {
        // Формирование клуба при низкой температуре
        if (hiveTemperature < 15f && !isClusterFormed) {
            beeCluster.FormCluster();
            isClusterFormed = true;
        }
        
        // Расчёт расхода ресурсов
        float tempDifference = externalTemperature - hiveTemperature;
        honeyConsumptionRate = CalculateConsumption(tempDifference);
        
        // Управление ротацией пчёл в клубе
        if (isClusterFormed) {
            beeCluster.ManageRotation();
        }
    }
}
```

### 🐝 Компонент клуба пчёл
```csharp
public class BeeCluster : MonoBehaviour {
    public List<Bee> outerBees;    // Внешние пчёлы
    public List<Bee> innerBees;    // Внутренние пчёлы
    public Bee queenBee;           // Матка в центре
    
    public float rotationInterval = 30f; // Интервал смены позиций
    private float rotationTimer;
    
    public void FormCluster() {
        // Логика формирования клуба вокруг матки
        // Сортировка пчёл по выносливости к холоду
    }
    
    public void ManageRotation() {
        rotationTimer += Time.deltaTime;
        if (rotationTimer >= rotationInterval) {
            RotateBees();
            rotationTimer = 0f;
        }
    }
    
    void RotateBees() {
        // Смена позиций: некоторые внешние ↔ внутренние
        // Учёт здоровья и температуры пчёл
    }
}

### 🔄 Система профессионального развития (полная реализация)
```csharp
// Основной компонент пчелы с профессией
public class ProfessionalBee : MonoBehaviour {
    [Header("Профессиональные характеристики")]
    public BeeProfession profession = BeeProfession.Generalist;
    public ProfessionStats professionStats;
    public List<SpecialAbility> specialAbilities = new();
    
    [Header("Развитие и возраст")]
    public BeeDevelopmentStage developmentStage;
    public int ageInDays = 0;
    public float developmentProgress = 0f; // 0-1
    
    [Header("Состояние")]
    public float health = 100f;
    public float energy = 100f;
    public float wear = 0f;
    public bool isAlive = true;
    
    [Header("Настройки")]
    public ProfessionBalanceConfig professionConfig;
    
    // События
    public event Action<BeeProfession> OnProfessionAssigned;
    public event Action<BeeDevelopmentStage> OnDevelopmentStageChanged;
    public event Action OnBeeBorn;
    public event Action OnBeeDied;
    
    // Ссылки
    private LarvaSpecialization larvaSpecialization;
    private BeeBehavior behavior;
    
    void Start() {
        if (developmentStage == BeeDevelopmentStage.Egg) {
            InitializeAsEgg();
        }
    }
    
    void InitializeAsEgg() {
        developmentStage = BeeDevelopmentStage.Egg;
        StartCoroutine(DevelopmentProcess());
    }
    
    IEnumerator DevelopmentProcess() {
        // Стадия яйца (3 дня)
        float eggDuration = professionConfig.baseDevelopmentTime * 0.14f; // 3/21
        yield return new WaitForSeconds(eggDuration * professionConfig.dayDuration);
        
        developmentStage = BeeDevelopmentStage.Larva;
        OnDevelopmentStageChanged?.Invoke(developmentStage);
        
        // Активация системы специализации
        larvaSpecialization = gameObject.AddComponent<LarvaSpecialization>();
        larvaSpecialization.larva = GetComponent<Larva>();
        larvaSpecialization.OnSpecializationComplete += OnSpecializationComplete;
        
        // Стадия личинки (6 дней + модификатор профессии)
        ProfessionSettings settings = professionConfig.GetSettings(profession);
        float larvaDuration = professionConfig.baseDevelopmentTime * 0.29f * settings.developmentTimeMultiplier;
        yield return new WaitForSeconds(larvaDuration * professionConfig.dayDuration);
        
        // Если профессия не выбрана - становится универсальной
        if (profession == BeeProfession.Generalist && !larvaSpecialization.isSpecializationComplete) {
            larvaSpecialization.ChooseProfession(BeeProfession.Generalist);
            larvaSpecialization.CompleteSpecialization();
        }
        
        developmentStage = BeeDevelopmentStage.Pupa;
        OnDevelopmentStageChanged?.Invoke(developmentStage);
        
        // Стадия куколки (12 дней + модификатор профессии)
        float pupaDuration = professionConfig.baseDevelopmentTime * 0.57f * settings.developmentTimeMultiplier;
        yield return new WaitForSeconds(pupaDuration * professionConfig.dayDuration);
        
        // Рождение пчелы
        Birth();
    }
    
    void OnSpecializationComplete() {
        profession = larvaSpecialization.chosenProfession;
        professionStats = larvaSpecialization.professionStats;
        specialAbilities = larvaSpecialization.specialAbilities;
        
        OnProfessionAssigned?.Invoke(profession);
        
        // Обновление поведения в соответствии с профессией
        UpdateBehaviorForProfession();
    }
    
    void Birth() {
        developmentStage = BeeDevelopmentStage.Adult;
        isAlive = true;
        
        // Инициализация поведения
        behavior = gameObject.AddComponent<BeeBehavior>();
        behavior.InitializeForProfession(profession, professionStats);
        
        // Активация специальных способностей
        foreach (var ability in specialAbilities) {
            ability.Initialize(this);
        }
        
        OnBeeBorn?.Invoke();
        StartCoroutine(LifeProgression());
    }
    
    IEnumerator LifeProgression() {
        ProfessionSettings settings = professionConfig.GetSettings(profession);
        float lifeExpectancy = settings.baseLifeSpan * GetSeasonalLifeMultiplier();
        
        while (isAlive && ageInDays < lifeExpectancy && wear < 100f) {
            // Ежедневные обновления
            UpdateDaily();
            
            // Проверка смерти
            if (ShouldDie()) {
                Die();
                yield break;
            }
            
            // Ожидание следующего дня
            yield return new WaitForSeconds(professionConfig.dayDuration);
            ageInDays++;
        }
        
        // Смерть от старости
        if (isAlive) {
            Die();
        }
    }
    
    void UpdateDaily() {
        // Расход энергии
        float energyConsumption = professionStats.energyConsumptionMultiplier * 10f;
        energy = Mathf.Clamp(energy - energyConsumption, 0f, 100f);
        
        // Износ
        float dailyWear = professionStats.wearRateMultiplier * GetWorkIntensity();
        wear = Mathf.Clamp(wear + dailyWear, 0f, 100f);
        
        // Восстановление здоровья (если есть кормилицы)
        if (HiveManager.Instance.HasNurseBees()) {
            health = Mathf.Clamp(health + 5f, 0f, 100f);
        }
        
        // Обновление способностей
        foreach (var ability in specialAbilities) {
            ability.UpdateCooldown(Time.deltaTime);
        }
    }
    
    float GetSeasonalLifeMultiplier() {
        Season currentSeason = SeasonManager.Instance.currentSeason;
        switch (currentSeason) {
            case Season.Winter: return professionConfig.winterLifeMultiplier;
            case Season.Spring: return professionConfig.springLifeMultiplier;
            case Season.Summer: return professionConfig.summerLifeMultiplier;
            case Season.Autumn: return professionConfig.autumnLifeMultiplier;
            default: return 1f;
        }
    }
    
    float GetWorkIntensity() {
        // Интенсивность зависит от профессии и потребностей улья
        float baseIntensity = 1f;
        
        switch (profession) {
            case BeeProfession.FastForager:
                baseIntensity = HiveManager.Instance.GetForagingIntensity();
                break;
            case BeeProfession.EliteGuard:
                baseIntensity = HiveManager.Instance.GetThreatLevel();
                break;
            case BeeProfession.MasterBuilder:
                baseIntensity = HiveManager.Instance.GetConstructionNeed();
                break;
            // ... другие профессии
        }
        
        return Mathf.Clamp(baseIntensity, 0.5f, 2f);
    }
    
    bool ShouldDie() {
        // Смерть от износа
        if (wear >= 100f) return true;
        
        // Смерть от отсутствия энергии
        if (energy <= 0f) return true;
        
        // Смерть от болезней/ран
        if (health <= 0f) return true;
        
        // Случайная смерть (старость + стресс)
        float deathChance = ageInDays / 100f + (wear / 200f);
        if (Random.value < deathChance) return true;
        
        return false;
    }
    
    void Die() {
        isAlive = false;
        OnBeeDied?.Invoke();
        
        // Визуальные эффекты
        StartCoroutine(DeathAnimation());
        
        // Уведомление системы
        HiveManager.Instance.OnBeeDied(this);
    }
    
    IEnumerator DeathAnimation() {
        // Анимация смерти
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }
    
    void UpdateBehaviorForProfession() {
        if (behavior != null) {
            behavior.AdjustForProfession(profession, professionStats);
        }
    }
    
    // Публичные методы для взаимодействия
    public bool CanUseAbility(int abilityIndex) {
        if (abilityIndex < 0 || abilityIndex >= specialAbilities.Count) return false;
        return specialAbilities[abilityIndex].CanActivate(this);
    }
    
    public void UseAbility(int abilityIndex) {
        if (CanUseAbility(abilityIndex)) {
            specialAbilities[abilityIndex].Activate(this);
        }
    }
    
    public float GetEfficiency() {
        float ageEfficiency = Mathf.Clamp01(ageInDays / 7f); // Пик на 7-й день
        float energyEfficiency = energy / 100f;
        float healthEfficiency = health / 100f;
        
        return professionStats.GetBaseEfficiency() * ageEfficiency * energyEfficiency * healthEfficiency;
    }
}
```

### ⚙️ Конфигурация профессий (ScriptableObject)
```csharp
[CreateAssetMenu(fileName = "ProfessionBalanceConfig", menuName = "BeeSwarm/ProfessionBalance")]
public class ProfessionBalanceConfig : ScriptableObject {
    [Header("Базовые настройки")]
    public float dayDuration = 60f;          // Длительность игрового дня в секундах
    public int baseDevelopmentTime = 21;     // Базовое время развития (дни)
    
    [Header("Сезонные модификаторы жизни")]
    public float winterLifeMultiplier = 3.0f;
    public float springLifeMultiplier = 0.8f;
    public float summerLifeMultiplier = 1.0f;
    public float autumnLifeMultiplier = 1.5f;
    
    [Header("Настройки профессий")]
    public ProfessionSettings[] professionSettings;
    
    [Header("Баланс игры")]
    public float specialistEfficiencyMultiplier = 1.5f;
    public float specialistCostMultiplier = 2.0f;
    public float retrainPenalty = 0.3f;
    public float mutationChance = 0.05f;
    
    public ProfessionSettings GetSettings(BeeProfession profession) {
        foreach (var settings in professionSettings) {
            if (settings.profession == profession) {
                return settings;
            }
        }
        return professionSettings[0]; // Возвращаем универсальную по умолчанию
    }
    
    [System.Serializable]
    public class ProfessionSettings {
        public BeeProfession profession;
        public string displayName;
        public Sprite icon;
        
        [Header("Развитие")]
        public float developmentTimeMultiplier = 1f;
        public ResourceCost[] foodCosts;
        
        [Header("Характеристики")]
        public ProfessionStats baseStats;
        public float baseLifeSpan = 45f; // Дней
        
        [Header("Способности")]
        public SpecialAbilityData[] abilities;
        
        [Header("Описание")]
        [TextArea(3, 5)] public string description;
        public string[] strengths;
        public string[] weaknesses;
    }
    
    [System.Serializable]
    public class ResourceCost {
        public ResourceType resource;
        public float amount;


## 📚 Документация
- [ ] API документация для скриптов
- [ ] Руководство по настройке проекта
- [ ] Гайд по геймплею

---

### 🆕 ДОБАВЛЕНО 13.04.2026:
1. **Детализированная сезонная механика** — все 4 сезона с уникальными задачами
2. **Зимняя механика выживания** — клуб пчёл, температурный баланс, ротация
3. **Осеннее событие** — изгнание трутней для экономии ресурсов
4. **🎯 ПРОФЕССИОНАЛЬНАЯ СПЕЦИАЛИЗАЦИЯ** — игрок выбирает профессию для личинки
5. **5 уникальных профессий** — от Быстрой сборщицы до Учёной пчелы (менеджер удалён)
6. **📦 СИСТЕМА РЕСУРСОВ** — 3 вида мёда, 3 вида пыльцы, воск, прополис, маточное молочко
7. **Система кормления** — разные ресурсы для разных профессий
8. **Специальные способности** — уникальные навыки для каждой профессии
9. **🗺️ СИСТЕМА РАЗВЕДКИ** — пчёлы-разведчицы, "туман войны", глобальная карта
10. **🚨 СИСТЕМА ПАТРУЛИРОВАНИЯ** — охранники автономно защищают улей с настройками игрока
11. **❄️ ЗИМНЯЯ ЗАЩИТА** — вход/выход закрыты, нет внешних патрулей, внутренняя охрана, экстренный выход охранников по тревоге
12. **Стратегический слой** — баланс стоимости, времени и эффективности
13. **Полная техническая реализация** — код системы специализации на C#

---

*Обновлено: 13 апреля 2026*  
*Добавлено: Профессиональная специализация с рождения + полная система выбора профессий*