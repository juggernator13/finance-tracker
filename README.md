# Личные финансы и аналитика

![CI](https://github.com/juggernator13/finance-tracker/actions/workflows/ci.yml/badge.svg)

Система для учёта счетов и расходов. Можно загружать банковские выписки в CSV, операции сами раскладываются по категориям, есть бюджеты с уведомлением о превышении лимита и отчёты по категориям и месяцам.

Учебный проект для учебной практики (19.10.2026 – 16.11.2026), тема 9, продвинутый уровень.

## Технологический стек

- .NET 9, ASP.NET Core, EF Core
- PostgreSQL (своя БД у каждого сервиса)
- RabbitMQ, Redis
- YARP (API Gateway), SignalR
- Blazor (веб-клиент)
- Docker, Docker Compose, GitHub Actions
- xUnit, Testcontainers, k6 (нагрузочный тест)

## Архитектура

Пять сервисов за одним Gateway: Identity, Accounts & Transactions, Import, Categorization, Reports & Budgets. Сервисы общаются по HTTP и через RabbitMQ. Импорт выписки сделан как Saga с откатом при ошибке, для надёжности используются Outbox и идемпотентные обработчики.

Подробнее: [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)

## Быстрый старт

Нужны .NET SDK 9.0 и Docker.

```bash
git clone <repository-url>
cd <repository-folder>
cp .env.example .env
docker compose up --build
```

Адреса (будут уточнены после настройки Compose):
- клиент: http://localhost:8081
- Gateway: http://localhost:8080
- RabbitMQ: http://localhost:15672

Тестовые учётные записи (только для демонстрации):

| Роль | Логин | Пароль |
|------|-------|--------|
| User | user@example.com | User123! |
| Admin | admin@example.com | Admin123! |

Примеры выписок для импорта лежат в папке `samples/`.

## Реализованная функциональность

Список обновляю по ходу работы.

- [ ] Регистрация и вход, JWT, роли
- [ ] Счета и операции
- [ ] Импорт CSV, защита от дублей
- [ ] Категории, правила, автокатегоризация
- [ ] Бюджеты и отчёты
- [ ] Saga импорта с откатом, Outbox
- [ ] SignalR (прогресс импорта, уведомления о бюджете)
- [ ] Кэш в Redis

## Тесты и CI

```bash
dotnet test
```

Интеграционные тесты запускаются тем же способом, для них нужен Docker. CI на GitHub Actions собирает проект и запускает тесты при каждом push и Pull Request. Результаты тестов лежат в `docs/test-results/`.

## Структура репозитория

```
src/        сервисы, Gateway, клиент, общие библиотеки
tests/      тесты (unit, интеграционные, нагрузочный)
docs/       ARCHITECTURE.md, PLAN.md, результаты тестов
samples/    примеры выписок
docker-compose.yml
```

## План работ

[docs/PLAN.md](docs/PLAN.md)

## Использование ИИ

Использовал ИИ-ассистента (Claude) для обсуждения архитектуры и составления черновиков документации. Код пишу и разбираю сам.

## Ограничения и планы развития

- Категоризация только по правилам.
- Одна валюта на счёт.
- RabbitMQ и Redis в одном экземпляре.
- Можно добавить прогноз расходов и поиск повторяющихся платежей.

