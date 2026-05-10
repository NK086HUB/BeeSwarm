# Техническая архитектура - Bee Swarm

## Технологический стек
- **Движок:** Unity 2022.3 LTS
- **Язык:** C# (.NET 6+)
- **Версионный контроль:** Git + GitHub + Git LFS
- **CI/CD:** GitHub Actions
- **Документация:** Markdown + Wiki

## Оптимизация для 500 пчёл

### Уровни оптимизации:
1. **LOD система** (3 уровня детализации)
2. **Object Pooling** для переиспользования объектов
3. **Smart Updates** (обновление по расстоянию от камеры)
4. **GPU Instancing** для рендеринга
5. **Compute Shaders** для сложных вычислений

### Производительность:
- **Цель:** 60 FPS на среднем железе
- **Максимум пчёл:** 500 (качество > количество)
- **Память:** < 2GB RAM
- **Загрузка CPU:** < 70%
- **Загрузка GPU:** < 80%

## Архитектура кода

### Основные системы:
```csharp
// 1. Система управления пчёлами
BeeManager
├── BeePool (Object Pooling)
├── BeeAI (Искусственный интеллект)
├── BeePhysics (Физика и движение)
└── BeeAnimation (Анимации)

// 2. Система ресурсов
ResourceSystem
├── ResourceManager (Управление ресурсами)
├── ProductionChain (Цепочки производства)
└── StorageSystem (Хранение)

// 3. Сезонная система
SeasonManager
├── SeasonCycle (Цикл сезонов)
├── WeatherSystem (Погода)
└── SeasonalEvents (Сезонные события)

// 4. Система разведки и защиты
ScoutDefenseSystem
├── FogOfWar (Туман войны)
├── ScoutAI (Разведчицы)
└── GuardAI (Охранники)
```

## Структура проекта Unity

### Папки Assets:
```
Assets/
├── Scripts/
│   ├── Core/           # Ядро игры
│   ├── Bees/           # Система пчёл
│   ├── Resources/      # Система ресурсов
│   ├── Seasons/        # Сезонная система
│   ├── UI/            # Пользовательский интерфейс
│   └── Utils/         # Вспомогательные скрипты
├── Prefabs/
│   ├── Bees/          # Префабы пчёл
│   ├── Hive/          # Префабы улья
│   ├── Resources/     # Префабы ресурсов
│   └── Environment/   # Префабы окружения
├── Scenes/
│   ├── MainMenu.unity
│   ├── Game.unity
│   └── Tutorial.unity
└── ...
```

## Требования к системе

### Минимальные:
- **OS:** Windows 10, macOS 10.15, Ubuntu 20.04
- **CPU:** 4 ядра, 2.5 GHz
- **RAM:** 8 GB
- **GPU:** DirectX 11 / OpenGL 4.5, 2 GB VRAM
- **Storage:** 5 GB

### Рекомендуемые:
- **OS:** Windows 11, macOS 12, Ubuntu 22.04
- **CPU:** 6 ядер, 3.5 GHz
- **RAM:** 16 GB
- **GPU:** DirectX 12 / Vulkan, 4 GB VRAM
- **Storage:** 10 GB (SSD)
