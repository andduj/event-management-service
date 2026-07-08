# Event Management Service

Система управления мероприятиями и бронированиями на **ASP.NET Core Web API (.NET 9)**.
Состоит из **трёх микросервисов** (Auth, Events, Bookings), обменивающихся данными через **Kafka**.

Возможности:
- CRUD и фильтрация событий;
- регистрация и вход пользователей с выдачей **JWT** (отдельный Auth-сервис);
- ролевая авторизация (`User`, `Admin`);
- создание и отмена брони с быстрым ответом (`202 Accepted`);
- бизнес-правила бронирования (лимит активных броней, запрет на прошедшие события);
- синхронизация событий в Bookings через Kafka (`BookableEvent`);
- отложенная обработка бронирований в фоне через `BackgroundService`;
- подтверждение брони и резервирование мест в Events через Kafka (`booking-confirmed`);
- отмена подтверждённой брони и освобождение мест в Events через Kafka (`booking-cancelled`);
- компенсация при сбое публикации в Kafka (откат `Confirmed` → `Cancelled` и освобождение места в Bookings).

## Технологии

- **C#**, **.NET 9**
- **ASP.NET Core Web API**
- **Swagger / Swashbuckle**
- **NLog** (через проект `EventManagement.Logging`)
- **Dependency Injection**
- **AutoMapper**
- **FluentValidation**
- **Entity Framework Core** (миграции, репозитории)
- **PostgreSQL** (`Npgsql`) — database per service
- **Apache Kafka** (`Confluent.Kafka`) — асинхронный обмен между сервисами
- **JWT** (`System.IdentityModel.Tokens.Jwt`, `Microsoft.AspNetCore.Authentication.JwtBearer`)
- **Docker Compose** — локальный полный стек
- **Testcontainers** (интеграционные тесты)
- **xUnit**, **Moq**, **FluentAssertions**, **AutoFixture**

## Структура решения

### Auth API

| Слой | Проект | Назначение |
|------|--------|------------|
| Domain | `src/EventManagement.Auth.Domain` | `User`, `UserRole`, доменные исключения |
| Application | `src/EventManagement.Auth.Application` | `AuthService`, DTO, порты `IUserRepository`, `IJwtTokenService` |
| Infrastructure | `src/EventManagement.Auth.Infrastructure` | EF Core, репозитории, миграции, `PasswordHasher`, `JwtTokenService` |
| Presentation | `src/EventManagement.Auth` | Web API, `AuthController`, Swagger, composition root |

### Events API

| Слой | Проект | Назначение |
|------|--------|------------|
| Domain | `src/EventManagement.Events.Domain` | `Event`, доменные исключения |
| Application | `src/EventManagement.Events.Application` | use cases, DTO, порты (`IEventRepository`, `IEventLifecyclePublisher`) |
| Infrastructure | `src/EventManagement.Events.Infrastructure` | EF Core, репозитории, миграции, Kafka publisher/consumer, `KafkaTopicInitializer` |
| Presentation | `src/EventManagement.Event` (`EventManagement.Events`) | Web API, контроллеры, Swagger, JWT-валидация |

### Bookings API

| Слой | Проект | Назначение |
|------|--------|------------|
| Domain | `src/EventManagement.Bookings.Domain` | `Booking`, `BookableEvent`, `BookingStatus`, доменные исключения |
| Application | `src/EventManagement.Bookings.Application` | `BookingService`, `BookingProcessingService`, DTO, порты репозиториев |
| Infrastructure | `src/EventManagement.Bookings.Infrastructure` | EF Core, репозитории, миграции, Kafka consumer/publisher (`booking-confirmed`, `booking-cancelled`), `BookingBackgroundService` |
| Presentation | `src/EventManagement.Booking` (`EventManagement.Bookings`) | Web API, `BookingsController`, JWT, Swagger |

### Общие проекты

- `src/EventManagement.Contracts` — DTO сообщений Kafka и имена топиков
- `src/EventManagement.Logging` — обёртка над NLog

Зависимости в каждом сервисе: `Domain` ← `Application` ← `Infrastructure` ← `Presentation`. **Application не ссылается на Infrastructure.**

### Тесты

- `tests/EventManagement.Auth.Tests` — `AuthService`, `JwtTokenService`
- `tests/EventManagement.Events.Tests` — модульные тесты Events (Application)
- `tests/EventManagement.Bookings.Tests` — бизнес-правила, `BookingService`, `BookingProcessingService`, `BookableEvent`, JWT claims, отмена подтверждённых броней
- `tests/EventApi.IntegrationTests` — репозитории и миграции на PostgreSQL через Testcontainers

## Архитектура

Три независимых API с отдельными базами данных (**database per service**):

| Сервис | База | Порт (HTTP) | Назначение |
|--------|------|-------------|------------|
| **Auth** | `auth` | `5238` | регистрация, вход, выдача JWT |
| **Events** | `events` | `5167` | CRUD мероприятий, источник правды по событиям |
| **Bookings** | `bookings` | `5237` | бронирования, локальная проекция `BookableEvent` |

Межсервисное взаимодействие — **только через Kafka** (без HTTP между Bookings и Events):

```
Events ──event-created/updated/deleted──► Bookings (синхронизация BookableEvent)
Bookings ──booking-confirmed────────────► Events (резерв мест)
Bookings ──booking-cancelled────────────► Events (освобождение мест)
```

Топики (`EventManagement.Contracts.Kafka.KafkaTopics`):

| Топик | Направление | Назначение |
|-------|-------------|------------|
| `event-created` | Events → Bookings | новое мероприятие |
| `event-updated` | Events → Bookings | обновление проекции (`AvailableSeats` не увеличивается выше локального значения) |
| `event-deleted` | Events → Bookings | удаление проекции |
| `booking-confirmed` | Bookings → Events | резерв мест в `events` |
| `booking-cancelled` | Bookings → Events | освобождение мест в `events` |

Топики `booking-confirmed` и `booking-cancelled` создаются при старте Events API (`KafkaTopicInitializer`). Топики жизненного цикла событий создаются Kafka автоматически при первой публикации.

### Поток бронирования

1. Пользователь создаёт бронь → Bookings резервирует место в локальной `bookable_events` и возвращает `202 Accepted` (`Pending`).
2. `BookingBackgroundService` подтверждает бронь (`Confirmed`) и публикует `booking-confirmed` в Kafka.
3. Events consumer уменьшает `AvailableSeats` в таблице `events`.
4. При отмене `Pending`/`Confirmed` Bookings освобождает место в `bookable_events`.
5. При отмене `Confirmed` дополнительно публикуется `booking-cancelled` → Events освобождает место.
6. Если публикация `booking-confirmed` не удалась, Bookings откатывает бронь в `Cancelled` и освобождает место локально.

## Запуск

Требуется:
- **.NET SDK 9.0+**
- **Docker** (для PostgreSQL, Kafka и/или полного стека)

```bash
dotnet restore
dotnet build EventManagement.sln
```

### Вариант 1: полный стек в Docker (рекомендуется)

Поднимает Kafka (KRaft, без Zookeeper), три PostgreSQL и три API:

```bash
docker compose -f docker/docker-compose.yml up -d --build
```

| Компонент | Контейнер | Порт (host) |
|-----------|-----------|-------------|
| Kafka | `event-management-kafka` | `9092` |
| PostgreSQL Events | `events-postgres` | `5436` |
| PostgreSQL Bookings | `bookings-postgres` | `5435` |
| PostgreSQL Auth | `auth-postgres` | `5437` |
| Auth API | `auth-api` | `5238` |
| Events API | `events-api` | `5167` |
| Bookings API | `bookings-api` | `5237` |

| Сервис | URL Swagger |
|--------|-------------|
| Auth | http://localhost:5238/swagger |
| Events | http://localhost:5167/swagger |
| Bookings | http://localhost:5237/swagger |

Базы данных при старте **пустые** — применяются только миграции EF Core. События нужно создавать через Events API (или зарегистрировать пользователя и войти через Auth API).

Остановка и удаление данных:

```bash
docker compose -f docker/docker-compose.yml down -v
```

### Вариант 2: только инфраструктура + `dotnet run`

Для локальной разработки можно поднять PostgreSQL и Kafka из compose и запускать API через `dotnet run`. Строки подключения и Kafka задаются в `appsettings.json` / User Secrets.

Только инфраструктура:

```bash
docker compose -f docker/docker-compose.yml up -d kafka postgres-events postgres-bookings postgres-auth
```

PostgreSQL в compose:

| Сервис | Контейнер | Порт | База |
|--------|-----------|------|------|
| Events | `events-postgres` | `5436` | `events` |
| Bookings | `bookings-postgres` | `5435` | `bookings` |
| Auth | `auth-postgres` | `5437` | `auth` |

Пример User Secrets:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5437;Database=auth;Username=postgres;Password=postgres" --project src/EventManagement.Auth/EventManagement.Auth.csproj
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5436;Database=events;Username=postgres;Password=postgres" --project src/EventManagement.Event/EventManagement.Events.csproj
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5435;Database=bookings;Username=postgres;Password=postgres" --project src/EventManagement.Booking/EventManagement.Bookings.csproj
```

Запуск API (в отдельных терминалах, профиль `http`):

```bash
dotnet run --project src/EventManagement.Auth/EventManagement.Auth.csproj --launch-profile http
dotnet run --project src/EventManagement.Event/EventManagement.Events.csproj --launch-profile http
dotnet run --project src/EventManagement.Booking/EventManagement.Bookings.csproj --launch-profile http
```

Для синхронизации событий и подтверждения/отмены броней нужен **Kafka** (`Kafka:BootstrapServers` — по умолчанию `localhost:9092`).

### Схема базы и миграции EF Core

Три контекста и три набора миграций:

| Контекст | Проект (миграции) | Startup-проект | Таблицы |
|----------|-------------------|----------------|---------|
| `AuthDbContext` | `src/EventManagement.Auth.Infrastructure` | `src/EventManagement.Auth` | `users` |
| `EventsDbContext` | `src/EventManagement.Events.Infrastructure` | `src/EventManagement.Event` | `events` |
| `BookingsDbContext` | `src/EventManagement.Bookings.Infrastructure` | `src/EventManagement.Booking` | `bookings`, `bookable_events` |

Миграции применяются при старте каждого API (`Migrate()`). Тестовые данные **не** загружаются автоматически. Между базами **нет FK** — `Booking.EventId` и `BookableEvent.Id` ссылаются на событие в Events логически.

CLI **dotnet-ef** — локальный инструмент (`.config/dotnet-tools.json`):

```bash
dotnet tool restore
```

Пример применения миграций:

```bash
dotnet tool run dotnet-ef -- database update --project src/EventManagement.Auth.Infrastructure/EventManagement.Auth.Infrastructure.csproj --startup-project src/EventManagement.Auth/EventManagement.Auth.csproj --context AuthDbContext

dotnet tool run dotnet-ef -- database update --project src/EventManagement.Events.Infrastructure/EventManagement.Events.Infrastructure.csproj --startup-project src/EventManagement.Event/EventManagement.Events.csproj --context EventsDbContext

dotnet tool run dotnet-ef -- database update --project src/EventManagement.Bookings.Infrastructure/EventManagement.Bookings.Infrastructure.csproj --startup-project src/EventManagement.Booking/EventManagement.Bookings.csproj --context BookingsDbContext
```

## Тесты

```bash
dotnet test EventManagement.sln
```

Отдельные проекты:

```bash
dotnet test tests/EventManagement.Auth.Tests/EventManagement.Auth.Tests.csproj
dotnet test tests/EventManagement.Events.Tests/EventManagement.Events.Tests.csproj
dotnet test tests/EventManagement.Bookings.Tests/EventManagement.Bookings.Tests.csproj
dotnet test tests/EventApi.IntegrationTests/EventApi.IntegrationTests.csproj
```

### Интеграционные тесты

Проект `tests/EventApi.IntegrationTests` проверяет слой данных на **реальном PostgreSQL** через **Testcontainers**.

**Требования:** запущенный Docker.

**Как устроено:**
- Один контейнер PostgreSQL на весь прогон (`PostgresDbFixture`).
- Три базы (`auth`, `events`, `bookings`) внутри контейнера.
- Перед каждым тестом — `EnsureDeleted()` + `Migrate()` (`ResetAsync`).

**Покрытие:**

| Область | Тесты |
|---------|--------|
| Миграции | таблицы `users`, `events`, `bookings`, `bookable_events` |
| `EventRepository` | CRUD, резерв/освобождение мест, фильтры, пагинация |
| `BookingRepository` | create, get, update, выборка по статусу |
| `BookableEventRepository` | upsert, резерв и освобождение мест |
| `UserRepository` | создание и поиск пользователей (Auth) |

## Swagger и JWT

Swagger UI доступен в режиме Development для всех трёх API (см. порты выше).

**Auth API** — только `register` и `login`, кнопка **Authorize** не нужна.

**Events / Bookings API** — кнопка **Authorize** (схема `Bearer`):
1. `POST /api/v1/auth/register` и `POST /api/v1/auth/login` в **Auth API** → скопировать `token`.
2. В Events/Bookings нажать **Authorize** → `Bearer {token}`.
3. CRUD событий и `reserve-seats` / `release-seats` требуют роль `Admin`.

### Настройка JWT

Токен выдаёт **Auth API**. Events и Bookings **проверяют** токен по общей секции `Jwt`:

```json
"Jwt": {
  "Secret": "EventManagementSprint8DevSecretKey_Min32Chars!",
  "Issuer": "EventManagement.Auth",
  "Audience": "EventManagement",
  "LifetimeMinutes": 60
}
```

Секрет не короче 32 символов. В production — User Secrets или переменные окружения (`Jwt__Secret` и т.д.).

**Роли:**
- `User` — создание и отмена своих броней;
- `Admin` — отмена любой брони; CRUD событий; `reserve-seats` / `release-seats`.

**Публичные эндпоинты:** `POST /api/v1/auth/register`, `POST /api/v1/auth/login`, все `GET` в Events API.

## Модели

### Event (Events)

- `Id`, `Title`, `Description`, `StartAt`, `EndAt`, `TotalSeats`, `AvailableSeats`

### User (Auth)

- `Id`, `Login`, `PasswordHash`, `Role`

### BookableEvent (Bookings)

Локальная проекция события для принятия решений о бронировании. Синхронизируется из Kafka.

### Booking (Bookings)

- `Id`, `EventId`, `UserId`, `Status`, `CreatedAt`, `ProcessedAt`

`BookingStatus`: `Pending`, `Confirmed`, `Rejected`, `Cancelled`.

Активные брони — `Pending` и `Confirmed`. Лимит на пользователя — **10** (`BookingLimits.MaxActiveBookings`).

## Эндпоинты

### Auth API (`api/v1/auth`)

- `POST /api/v1/auth/register` — регистрация (`204`)
- `POST /api/v1/auth/login` — вход (`200` + `{ "token": "..." }`)

### Events API (`api/v1/events`)

- `GET /api/v1/events` — список с фильтрацией *(публичный)*
- `POST /api/v1/events/filter` — фильтрация через тело *(публичный)*
- `GET /api/v1/events/{id}` — событие по id *(публичный)*
- `GET /api/v1/events/{id}/exists` — проверка существования *(публичный)*
- `POST /api/v1/events` — создать **(Admin)**
- `PUT /api/v1/events/{id}` — обновить **(Admin)**
- `DELETE /api/v1/events/{id}` — удалить **(Admin)**
- `POST /api/v1/events/{id}/reserve-seats` — резерв мест **(Admin)**
- `POST /api/v1/events/{id}/release-seats` — освобождение мест **(Admin)**

### Bookings API (`api/v1`) — требуется JWT

- `POST /api/v1/events/{id}/book` — создать бронь (`202 Accepted`)
- `GET /api/v1/bookings/{id}` — статус брони
- `DELETE /api/v1/bookings/{id}` — отмена брони

## Отложенная фоновая обработка

- `POST .../book` сразу возвращает `202 Accepted`;
- `BookingBackgroundService` опрашивает очередь (`BookingProcessing:PollingIntervalSeconds`, по умолчанию 5 с);
- `BookingProcessingService` подтверждает бронь и публикует `booking-confirmed`;
- при сбое публикации `booking-confirmed` бронь откатывается в `Cancelled`, место освобождается в `bookable_events`;
- при отмене подтверждённой брони публикуется `booking-cancelled`;
- при отсутствии события в `bookable_events` — `Rejected` и освобождение места локально.

## Пример сценария

1. `POST http://localhost:5238/api/v1/auth/register` — `{ "login": "user1", "password": "secret" }`.
2. Для Admin — `"role": "Admin"` при регистрации.
3. `POST http://localhost:5238/api/v1/auth/login` → JWT.
4. `POST http://localhost:5167/api/v1/events` с `Authorization: Bearer {token}` (Admin) — создать событие (БД изначально пустая).
5. Дождаться синхронизации в Bookings через Kafka (`event-created`).
6. `POST http://localhost:5237/api/v1/events/{id}/book` с JWT → `202`, статус `Pending`.
7. Через несколько секунд `GET http://localhost:5237/api/v1/bookings/{id}` → `Confirmed`.
8. В Events `AvailableSeats` уменьшится после обработки `booking-confirmed`.
9. `DELETE http://localhost:5237/api/v1/bookings/{id}` — отмена; при `Confirmed` место вернётся и в Bookings, и в Events (`booking-cancelled`).

## Обработка ошибок

В каждом API — `ExceptionHandlingMiddleware` с ответами `application/problem+json`.

Bookings API:

| Исключение | HTTP |
|------------|------|
| `BookingNotFoundException`, `EventNotFoundException` | 404 |
| `NoAvailableSeatsException`, `ActiveBookingsLimitExceededException` | 409 |
| `EventAlreadyStartedException`, `ArgumentException` | 400 |
| `AccessDeniedException` | 403 |
| `UnauthorizedAccessException` | 401 |

В Development в ответ добавляются `traceId` и `stackTrace`.
