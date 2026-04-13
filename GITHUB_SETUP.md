# Инструкция по настройке GitHub репозитория

## Шаг 1: Создание репозитория на GitHub

1. **Зайдите на** [github.com](https://github.com) и войдите в аккаунт
2. **Нажмите "+"** в правом верхнем углу → **"New repository"**
3. **Заполните данные:**
   - **Repository name:** `BeeSwarm`
   - **Description:** "Unity strategy simulator - управление пчелиным роем"
   - **Public** (поставьте галочку)
   - **Initialize with README:** НЕ СТАВИТЬ ГАЛОЧКУ (у нас уже есть README)
   - **Add .gitignore:** Выберите **Unity** из списка
   - **Choose a license:** Выберите **MIT License**
4. **Нажмите "Create repository"**

## Шаг 2: Проверка создания

После создания репозитория проверьте:
- URL: `https://github.com/NK086tme/BeeSwarm`
- Репозиторий должен быть пустым (только .gitignore и LICENSE)

## Шаг 3: Отправка проекта с сервера

После создания репозитория, выполните на сервере:

```bash
cd /home/openclaw/.openclaw/workspace

# Проверьте remote
git remote -v

# Если нужно, установите правильный remote
git remote set-url origin git@github.com:NK086tme/BeeSwarm.git

# Отправьте проект
git push -u origin main
```

## Шаг 4: Проверка на GitHub

После успешного пуша:
1. **Обновите страницу репозитория** на GitHub
2. **Убедитесь**, что все файлы загружены:
   - README.md
   - Assets/ (скрипты Unity)
   - Docs/ (документация)
   - memory/bee_swarm_project.md (геймдизайн)
   - .gitignore, .gitattributes, LICENSE

## Шаг 5: Настройка проекта на GitHub

### Создайте Issues (задачи):
1. 🐝 **Профессиональная специализация** - система выбора профессий
2. 📦 **Система ресурсов** - 3 вида мёда, пыльцы, спецресурсы
3. 🌿 **Сезонная механика** - 4 сезона с уникальными задачами
4. 🗺️ **Разведка и защита** - туман войны, патрулирование
5. ⚙️ **Оптимизация** - LOD, pooling для 500 пчёл

### Создайте Projects (канбан-доску):
- Название: "Bee Swarm Development"
- Template: "Automated kanban"
- Колонки: Todo, In Progress, Done

### Настройте Wiki:
- Скопируйте содержимое `memory/bee_swarm_project.md`
- Разбейте на разделы: Геймдизайн, Техническая документация, Арт-стиль

## Шаг 6: Начало разработки

После настройки GitHub:
1. **Клонируйте репозиторий** на локальную машину:
   ```bash
   git clone git@github.com:NK086tme/BeeSwarm.git
   cd BeeSwarm
   git lfs install
   ```
2. **Откройте проект в Unity** 2022.3+
3. **Начните разработку** с базовых механик

## Проблемы и решения

### Если push не работает:
```bash
# Проверьте SSH подключение
ssh -T git@github.com

# Если "Permission denied", проверьте:
# 1. SSH ключ добавлен на GitHub
# 2. Правильность remote URL
# 3. Наличие репозитория
```

### Если конфликт с README:
```bash
# Если на GitHub уже есть README
git pull origin main --allow-unrelated-histories
# Разрешите конфликты, затем:
git push -u origin main
```

## Контакты и поддержка

- **GitHub:** [NK086tme](https://github.com/NK086tme)
- **Проект:** [BeeSwarm](https://github.com/NK086tme/BeeSwarm)
- **Документация:** В папке `Docs/` и `memory/bee_swarm_project.md`

Удачи в разработке! 🐝
