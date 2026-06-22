# Event Management Service

Сервис управления мероприятиями и бронированиями на **ASP.NET Core Web API (.NET 9)**.
Проект включает:
- CRUD и фильтрацию событий;
- регистрацию и вход пользователей с выдачей **JWT**;
- ролевую авторизацию (`User`, `Admin`);
- создание и отмену брони с быстрым ответом (`202 Accepted`);
- бизнес-правила бронирования (лимит активных броней, запрет на прошедшие события);
- резервирование/освобождение мест на событиях;
- отложенную обработку бронирований в фоне через `BackgroundService`.

## Технологии

- **C#**, **.NET 9**
- **ASP.NET Core Web API**
- **Swagger / Swashbuckle**
- **NLog** (через проект `EventManagement.Logging`)
- **Dependency Injection**
- **AutoMapper**
- **FluentValidation**
- **Entity Framework Core** (миграции, репозитории)
- **PostgreSQL** (`Npgsql`)
- **JWT** (`System.IdentityModel.Tokens.Jwt`, `Microsoft.AspNetCore.Authentication.JwtBearer`)
- **Testcontainers** (интеграционные тесты)
- **xUnit**, **Moq**, **FluentAssertions**, **AutoFixture**

## Структура решения

### Events API (чистая архитектура)

| Слой | Проект | Назначение |
|------|--------|------------|
| Domain | `src/EventManagement.Events.Domain` | `Event`, доменные исключения |
| Application | `src/EventManagement.Events.Application` | use cases, DTO, порты (`IEventRepository`), валидация |
| Infrastructure | `src/EventManagement.Events.Infrastructure` | EF Core, репозитории, миграции, сидер |
| Presentation | `src/EventManagement.Event` (`EventManagement.Events`) | Web API, контроллеры, Swagger, composition root |

Зависимости: `Domain` ← `Application` ← `Infrastructure` ← `Presentation` (Web). **Application не ссылается на Infrastructure.**

Вспомогательные проекты: `EventManagement.Logging`, `EventManagement.Events.Api` (HTTP-клиент к Events для Bookings).

### Bookings API (чистая архитектура)

| Слой | Проект | Назначение |
|------|--------|------------|
| Domain | `src/EventManagement.Bookings.Domain` | `Booking`, `User`, `UserRole`, `BookingStatus`, доменные исключения |
| Application | `src/EventManagement.Bookings.Application` | `BookingService`, `AuthService`, фоновая обработка, DTO, порты `IBookingRepository`, `IUserRepository`, `IEventsGateway`, `IAuthService` |
| Infrastructure | `src/EventManagement.Bookings.Infrastructure` | EF Core, репозитории, миграции, `PasswordHasher`, `JwtTokenService`, `BookingBackgroundService`, клиент Events API |
| Presentation | `src/EventManagement.Booking` (`EventManagement.Bookings`) | Web API, контроллеры (`Auth`, `Bookings`), JWT, Swagger, composition root |

Зависимости: `Domain` ← `Application` ← `Infrastructure` ← `Presentation` (Web). **Application не ссылается на Infrastructure.**

### Тесты

- `tests/EventManagement.Events.Tests` — модульные тесты Events (преимущественно Application)
- `tests/EventManagement.Bookings.Tests` — модульные тесты Bookings: бизнес-правила (`BookingRulesTests`), авторизация (`AuthServiceTests`), домен (`BookingDomainTests`)
- `tests/EventApi.IntegrationTests` — интеграционные тесты репозиториев и миграций (PostgreSQL через Testcontainers)

## Запуск

Требуется:
- **.NET SDK 9.0+**
- **Docker** (для локального PostgreSQL)

```bash
dotnet restore
dotnet build EventManagement.sln
```

Запуск PostgreSQL (**перед** стартом API):

```bash
docker compose -f docker/docker-compose.yml up -d
```

Проверка, что контейнеры работают:

```bash
docker ps --filter "name=postgres"
```

Остановка и удаление данных (полный сброс томов):

```bash
docker compose -f docker/docker-compose.yml down -v
```

`docker-compose` поднимает **два** контейнера PostgreSQL (database per service):

| Сервис | Контейнер | Порт на хосте | База | Логин / пароль |
|--------|-----------|---------------|------|----------------|
| Events API | `events-postgres` | `5436` | `events` | `postgres` / `postgres` |
| Bookings API | `bookings-postgres` | `5435` | `bookings` | `postgres` / `postgres` |

Строки подключения задаются в **User Secrets** (в `appsettings.json` только шаблон без пароля):

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5436;Database=events;Username=postgres;Password=postgres" --project src/EventManagement.Event/EventManagement.Events.csproj
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5435;Database=bookings;Username=postgres;Password=postgres" --project src/EventManagement.Booking/EventManagement.Bookings.csproj
```

### Схема базы и миграции EF Core

В решении два контекста и два набора миграций:

| Контекст | Проект (миграции) | Startup-проект | Таблицы |
|----------|-------------------|----------------|---------|
| `EventsDbContext` | `src/EventManagement.Events.Infrastructure` | `src/EventManagement.Event` | `events` |
| `BookingsDbContext` | `src/EventManagement.Bookings.Infrastructure` | `src/EventManagement.Booking` | `bookings`, `users` |

У каждого сервиса **свой** PostgreSQL (локально — два контейнера в compose) — **database per service**. Миграции независимы: запустите каждый API или `database update` для своего контекста.

В `bookings` колонка `EventId` — логическая ссылка на событие в другом сервисе; **FK между базами невозможен**. В Application вызовы к Events идут через порт `IEventsGateway` (HTTP-адаптер в Infrastructure поверх NSwag-клиента `IEventsClient`).

CLI **dotnet-ef** подключён как **локальный инструмент** (файл `.config/dotnet-tools.json`). Перед работой с миграциями из корня репозитория:

```bash
dotnet tool restore
```

Создание новой миграции (пример имени `InitialCreate` замените при необходимости):

```bash
dotnet tool run dotnet-ef -- migrations add InitialCreate --project src/EventManagement.Events.Infrastructure/EventManagement.Events.Infrastructure.csproj --startup-project src/EventManagement.Event/EventManagement.Events.csproj --context EventsDbContext

dotnet tool run dotnet-ef -- migrations add InitialCreate --project src/EventManagement.Bookings.Infrastructure/EventManagement.Bookings.Infrastructure.csproj --startup-project src/EventManagement.Booking/EventManagement.Bookings.csproj --context BookingsDbContext
```

Применить все неприменённые миграции к базе из командной строки (альтернатива — просто запустить API, там уже вызывается `Migrate()`):

```bash
dotnet tool run dotnet-ef -- database update --project src/EventManagement.Events.Infrastructure/EventManagement.Events.Infrastructure.csproj --startup-project src/EventManagement.Event/EventManagement.Events.csproj --context EventsDbContext

dotnet tool run dotnet-ef -- database update --project src/EventManagement.Bookings.Infrastructure/EventManagement.Bookings.Infrastructure.csproj --startup-project src/EventManagement.Booking/EventManagement.Bookings.csproj --context BookingsDbContext
```

Запуск API событий:

```bash
dotnet run --project src/EventManagement.Event/EventManagement.Events.csproj
```

Запуск API бронирований:

```bash
dotnet run --project src/EventManagement.Booking/EventManagement.Bookings.csproj
```

## Тесты

Сборка и запуск всех тестов решения:

```bash
dotnet test EventManagement.sln
```

Отдельно модульные тесты:

```bash
dotnet test tests/EventManagement.Events.Tests/EventManagement.Events.Tests.csproj
dotnet test tests/EventManagement.Bookings.Tests/EventManagement.Bookings.Tests.csproj
```

### Интеграционные тесты

Проект `tests/EventApi.IntegrationTests` проверяет слой доступа к данным на **реальном PostgreSQL** через **Testcontainers**.

**Требования:** запущенный Docker (Docker Desktop или демон Docker). Без него тесты завершатся ошибкой подключения к контейнеру.

**Как устроено:**

- Один контейнер PostgreSQL на весь прогон (`PostgresDbFixture`, `IAsyncLifetime`).
- Две базы внутри контейнера (`events` и `bookings`) — отдельные строки подключения, как у двух сервисов на одном сервере.
- Перед каждым тестом базы сбрасываются: `EnsureDeleted()` + `Migrate()` (`ResetAsync`) — тесты не зависят от порядка запуска.
- Строка подключения берётся из Testcontainers, порт не захардкожен.

**Что покрыто:**

| Область | Тесты |
|---------|--------|
| Миграции | наличие таблиц `events` и `bookings` после `Migrate()` |
| `EventRepository` | CRUD, `Exists`, `TryReserveSeats`, `ReleaseSeats`, фильтры (`Title`, `StartAt`, `EndAt`), пагинация |
| `BookingRepository` | create, get, update, выборка по `BookingStatus` |

```bash
dotnet test tests/EventApi.IntegrationTests/EventApi.IntegrationTests.csproj
```

## Swagger

Swagger UI доступен для каждого API в режиме Development:
- Events API: `http://localhost:5167/swagger` или `https://localhost:7216/swagger`
- Bookings API: `http://localhost:5237/swagger` или `https://localhost:7095/swagger`

В обоих API настроена кнопка **Authorize** (схема `Bearer`). Для защищённых эндпоинтов:
1. Выполните `POST /api/v1/auth/login` в Bookings API и скопируйте `token` из ответа.
2. Нажмите **Authorize** и введите `Bearer {token}` (без фигурных скобок).
3. Для операций Events CRUD нужен пользователь с ролью `Admin`.

## Аутентификация и авторизация (JWT)

Аутентификация реализована в **Bookings API** (`AuthController`). Токен выдаётся при успешном входе и проверяется обоими сервисами по общей секции `Jwt` в `appsettings.json`:

```json
"Jwt": {
  "Secret": "EventManagementSprint8DevSecretKey_Min32Chars!",
  "Issuer": "EventManagement.Bookings",
  "Audience": "EventManagement",
  "LifetimeMinutes": 60
}
```

Секрет должен быть не короче 32 символов. В production замените значение `Secret` (User Secrets или переменные окружения).

**Роли:**
- `User` — создание и отмена своих броней;
- `Admin` — отмена любой брони; создание, изменение и удаление событий в Events API.

**Публичные эндпоинты** (без токена): `POST /api/v1/auth/register`, `POST /api/v1/auth/login`, все `GET` в Events API, служебные `reserve-seats` / `release-seats` / `exists`.

**Защищённые эндпоинты:** все операции с бронями в Bookings API; `POST` / `PUT` / `DELETE` событий в Events API (только `Admin`).

## Модель Event

`Event`:
- `Id` (`Guid`) — идентификатор;
- `Title` (`string`) — название;
- `Description` (`string?`) — описание;
- `StartAt` (`DateTime`) — начало;
- `EndAt` (`DateTime`) — окончание;
- `TotalSeats` (`int`) — общее количество мест;
- `AvailableSeats` (`int`) — текущее количество свободных мест.

## Модель User

`User`:
- `Id` (`Guid`) — идентификатор;
- `Login` (`string`) — уникальный логин;
- `PasswordHash` (`string`) — хеш пароля (SHA-256, hex);
- `Role` (`UserRole`) — роль пользователя.

`UserRole`:
- `User` — обычный пользователь;
- `Admin` — администратор.

## Модель Booking

`Booking`:
- `Id` (`Guid`) — идентификатор брони;
- `EventId` (`Guid`) — идентификатор события;
- `UserId` (`Guid`) — идентификатор пользователя, создавшего бронь;
- `Status` (`BookingStatus`) — статус брони;
- `CreatedAt` (`DateTime`) — дата создания;
- `ProcessedAt` (`DateTime?`) — дата обработки или отмены.

`BookingStatus`:
- `Pending` — ожидает обработки;
- `Confirmed` — подтверждена;
- `Rejected` — отклонена;
- `Cancelled` — отменена пользователем или администратором.

Активными считаются брони в статусах `Pending` и `Confirmed` (свойство `IsActive`). Лимит активных броней на пользователя — **10** (`BookingLimits.MaxActiveBookings`).

`Booking` и `Event` находятся в разных микросервисах, поэтому между ними нет навигационных свойств EF Core.
В `Booking` хранится только `EventId`, а существование/состояние события проверяется через HTTP-вызовы в сервис событий.

## Эндпоинты

### Bookings API — аутентификация (`api/v1/auth`)

- `POST /api/v1/auth/register` — регистрация (`204 No Content`; тело: `login`, `password`, опционально `role`)
- `POST /api/v1/auth/login` — вход (`200 OK` + `{ "token": "..." }`; при неверных данных — `404 Not Found`)

### Events API (`api/v1/events`)

- `GET /api/v1/events` — список событий с фильтрацией и пагинацией *(публичный)*
- `POST /api/v1/events/filter` — фильтрация через тело запроса *(публичный)*
- `GET /api/v1/events/{id}` — получить событие по id *(публичный)*
- `POST /api/v1/events` — создать событие (`201 Created` + `Location`) **(Admin, JWT)**
- `PUT /api/v1/events/{id}` — обновить событие **(Admin, JWT)**
- `DELETE /api/v1/events/{id}` — удалить событие **(Admin, JWT)**
- `GET /api/v1/events/{id}/exists` — проверить, существует ли событие *(публичный)*
- `POST /api/v1/events/{id}/reserve-seats?count=1` — попытка резервирования мест (`true/false`) *(публичный, вызывается Bookings)*
- `POST /api/v1/events/{id}/release-seats?count=1` — освобождение мест (`204 No Content`) *(публичный, вызывается Bookings)*

### Bookings API — бронирования *(требуется JWT)*

- `POST /api/v1/events/{id}/book`
  - создаёт бронь для события от имени текущего пользователя;
  - возвращает `202 Accepted`;
  - в теле — `BookingInfo` (`Id`, `EventId`, `Status`);
  - в `Location` — ссылка на ресурс брони (`/api/v1/bookings/{bookingId}`);
  - если событие не найдено — `404 Not Found`;
  - если событие уже началось — `400 Bad Request`;
  - если мест больше нет — `409 Conflict`;
  - если у пользователя уже 10 активных броней — `409 Conflict`.

- `GET /api/v1/bookings/{id}`
  - возвращает текущее состояние брони;
  - `200 OK` + `BookingDto`;
  - если бронь не найдена — `404 Not Found`.

- `DELETE /api/v1/bookings/{id}`
  - отменяет бронь;
  - `204 No Content` при успехе;
  - владелец брони или `Admin` — успех; иначе — `403 Forbidden`;
  - для активной брони (`Pending` / `Confirmed`) освобождается место на событии.

## Отложенная фоновая обработка

В проекте реализован паттерн **быстрый ответ + отложенная обработка**:
- `POST` на создание брони сразу возвращает `202 Accepted`;
- `BookingBackgroundService` циклически опрашивает очередь с интервалом из `BookingProcessing:PollingIntervalSeconds` (`PeriodicTimer`, по умолчанию 5 с, в Development — 2 с);
- бизнес-обработка вынесена в `BookingProcessingService`;
- список `Pending` бронирований читается в отдельном DI-scope;
- каждая бронь обрабатывается в отдельном DI-scope (отдельный `DbContext` на задачу);
- если событие найдено — бронь подтверждается (`Confirmed`);
- если событие не найдено — бронь отклоняется (`Rejected`), сохраняется и выполняется освобождение места;
- при ошибках обработки бронь отклоняется и выполняется компенсация через освобождение места;
- после обработки `ProcessedAt` заполняется текущим UTC-временем.

## Пример сценария использования

1. Зарегистрировать пользователя: `POST /api/v1/auth/register` с `{ "login": "user1", "password": "secret" }`.
2. Для администратора — зарегистрировать с `"role": "Admin"` или использовать существующего Admin.
3. Войти: `POST /api/v1/auth/login` → получить JWT.
4. Создать событие через `POST /api/v1/events` с заголовком `Authorization: Bearer {token}` (нужна роль Admin).
5. Создать бронь: `POST /api/v1/events/{id}/book` с тем же JWT.
6. Сразу вызвать `GET /api/v1/bookings/{bookingId}` — статус будет `Pending`.
7. Подождать несколько секунд и повторить `GET` — статус станет `Confirmed`, поле `ProcessedAt` будет заполнено.
8. Отменить бронь: `DELETE /api/v1/bookings/{bookingId}` — статус `Cancelled`, место на событии освободится.

## Архитектура

Сервис состоит из двух независимых API:
- **Events** — управление мероприятиями (отдельные сборки Domain / Application / Infrastructure / Presentation);
- **Bookings** — создание и фоновая обработка бронирований (отдельные сборки Domain / Application / Infrastructure / Presentation).

Граница между сервисами:
- **HTTP** — в Bookings.Application используется порт `IEventsGateway`; в Bookings.Infrastructure он реализован через NSwag-клиент `IEventsClient`;
- **Данные** — отдельные БД `events` и `bookings`; `Booking` не обращается к `EventsDbContext` и таблице `events`.

### Events API

- **Domain** — сущность `Event`, `EventNotFoundException` (без EF и ASP.NET).
- **Application** — `EventService`, DTO, FluentValidation; порт `IEventRepository`.
- **Infrastructure** — `EventsDbContext`, `EventRepository`, миграции, `AddInfrastructureServices`, `UseEventsDatabaseInitialization`.
- **Presentation** (`EventManagement.Events`) — контроллеры, JWT-валидация, `[Authorize(Roles = "Admin")]` на CRUD, Swagger с Bearer, exception middleware; composition root в `Program.cs`.

### Bookings API

- **Domain** — `Booking`, `User`, `UserRole`, `BookingStatus`, доменные исключения (`BookingNotFoundException`, `NoAvailableSeatsException`, `EventAlreadyStartedException`, `ActiveBookingsLimitExceededException`, `AccessDeniedException`, `InvalidCredentialsException`, `LoginAlreadyExistsException`).
- **Application** — `BookingService`, `AuthService`, `BookingProcessingService`, DTO; порты `IBookingRepository`, `IUserRepository`, `IEventsGateway`, `IAuthService`, `IJwtTokenService`, `IPasswordHasher`.
- **Infrastructure** — `BookingsDbContext`, `BookingRepository`, `UserRepository`, `PasswordHasher`, `JwtTokenService`, адаптер `EventsGateway` (реализация `IEventsGateway` через `IEventsClient`), миграции, `BookingBackgroundService`, `AddInfrastructureServices`, `UseBookingsDatabaseInitialization`.
- **Presentation** (`EventManagement.Bookings`) — `AuthController`, `BookingsController`, JWT, Swagger с Bearer, middleware; composition root в `Program.cs`.

Для локального наполнения Events используется `EventsDataSeeder` + `EventsFactory` (Infrastructure).
Сидирование: `DatabaseInitialization:SeedOnStartup` — `false` в `appsettings.json`, `true` в `appsettings.Development.json`.

## Известные ограничения

- **NSwag.MSBuild** (генерация `EventsClient` при сборке `EventManagement.Events.Api`): сборка может завершиться ошибкой, если **полный путь к каталогу решения содержит пробелы** — это ограничение цепочки NSwag/вызовов при компиляции, а не логики приложения. Обходной путь: держать репозиторий в пути **без пробелов** (например `D:\work\event-management-service`), либо временно отключать/обходить шаг генерации в таком окружении.

## Обработка ошибок

- В `EventManagement.Event` и `EventManagement.Booking` используется собственная `ExceptionHandlingMiddleware`.
- Middleware формирует ответы в формате `application/problem+json` (`ProblemDetails`).
- В Bookings API исключения маппятся в HTTP-статусы:

| Исключение | HTTP |
|------------|------|
| `BookingNotFoundException`, `InvalidCredentialsException` | 404 |
| `NoAvailableSeatsException`, `ActiveBookingsLimitExceededException` | 409 |
| `EventAlreadyStartedException`, `LoginAlreadyExistsException`, `ArgumentException` | 400 |
| `AccessDeniedException` | 403 |
| `UnauthorizedAccessException` | 401 |
| `EventsGatewayException` | код из внешнего ответа Events API |

- В режиме Development в ответ дополнительно добавляются `traceId` и `stackTrace`.