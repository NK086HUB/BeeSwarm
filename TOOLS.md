# TOOLS.md - Local Notes

Skills define _how_ tools work. This file is for _your_ specifics — the stuff that's unique to your setup.

## What Goes Here

Things like:

- Camera names and locations
- SSH hosts and aliases
- Preferred voices for TTS
- Speaker/room names
- Device nicknames
- Anything environment-specific

## Examples

```markdown
### Cameras

- living-room → Main area, 180° wide angle
- front-door → Entrance, motion-triggered

### SSH

- home-server → 192.168.1.100, user: admin

### TTS

- Preferred voice: "Nova" (warm, slightly British)
- Default speaker: Kitchen HomePod
```

## Мои настройки (Лира)

### Голосовой синтез (TTS)
- **Сервис:** SaluteSpeech (Сбер)
- **Голос:** Мужской по умолчанию (бесплатная версия)
- **Скорость:** 0.9 (оптимальная)
- **Скрипт:** `/home/openclaw/.openclaw/workspace/salutespeech_simple.sh`
- **Ограничение:** Параметр `voice` не работает в бесплатном API
- **Альтернативы на будущее:** ElevenLabs, Yandex SpeechKit, RHVoice

### SFTP доступ
- **Сервер:** `82.25.161.19:22`
- **Пользователь:** `sftp86ocw`
- **Пароль:** `***REMOVED***`
- **Директория загрузок:** `/uploads/`
- **Назначение:** Обмен файлами, SSL сертификаты, голосовые сообщения

### Система экстренного реагирования
- **"ТГ красный":** Проблемы с Telegram → перезапуск gateway
- **"Код красный":** Критические проблемы → полная диагностика
- **Каналы активации:** Telegram, TUI, SFTP
- **Скрипт:** `/home/sftp86ocw/emergency_red_code.sh`

### GigaChat API (тестовый режим)
- **Статус:** Активное тестирование (30 марта - 28 июня 2026)
- **SSL:** `verify=False` (безопасный режим)
- **Лимиты:** До 20 запросов в день
- **План:** Оценка 28 июня → решение об установке SSL сертификатов

### Hostkey Panel API
- **API Key:** `***REMOVED***`
- **Панель управления:** https://panel.hostkey.com/controlpanel.html?key=***REMOVED***
- **Назначение:** Управление серверами Hostkey через API
- **Возможности:** Перезагрузка, мониторинг, управление услугами

### Grafana Monitoring
- **URL:** http://82.25.161.19:3000
- **Логин:** `admin`
- **Пароль:** `***REMOVED***` (установлен 12.04.2026)
- **Дашборд 86mtp:** http://82.25.161.19:3000/d/6eb64f8b-2f18-4f44-938d-e87f185956e5/86mtp-server-monitoring
- **Prometheus:** http://82.25.161.19:9090
- **Node Exporter порт:** 9100

## Why Separate?

Skills are shared. Your setup is yours. Keeping them apart means you can update skills without losing your notes, and share skills without leaking your infrastructure.

---

Add whatever helps you do your job. This is your cheat sheet.
