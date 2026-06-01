# Event Management Service

Сервис управления мероприятиями и бронированиями на **ASP.NET Core Web API (.NET 9)**.
Проект включает:
- CRUD и фильтрацию событий;
- создание брони с быстрым ответом (`202 Accepted`);
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
- **Testcontainers** (интеграционные тесты)
- **xUnit**, **Moq**, **FluentAssertions**, **AutoFixture**

## Структура решения

### Events API (чистая архитектура, sprint-7)

| Слой | Проект | Назначение |
|------|--------|------------|
| Domain | `src/EventManagement.Events.Domain` | `Event`, доменные исключения |
| Application | `src/EventManagement.Events.Application` | use cases, DTO, порты (`IEventRepository`), валидация |
| Infrastructure | `src/EventManagement.Events.Infrastructure` | EF Core, репозитории, миграции, сидер |
| Presentation | `src/EventManagement.Event` (`EventManagement.Events`) | Web API, контроллеры, Swagger, composition root |

Зависимости: `Domain` ← `Application` ← `Infrastructure` ← `Presentation` (Web). **Application не ссылается на Infrastructure.**

Вспомогательные проекты: `EventManagement.Logging`, `EventManagement.Events.Api` (HTTP-клиент к Events для Bookings).

### Bookings API (пока монолитный Web-проект)

- `src/EventManagement.Booking` — API бронирований и фоновая обработка

### Тесты

- `tests/EventManagement.Events.Tests` — модульные тесты Events (Application + Infrastructure)
- `tests/EventManagement.Bookings.Tests` — модульные тесты бронирований
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

| Контекст | Проект (миграции) | Startup-проект | Таблица |
|----------|-------------------|----------------|---------|
| `EventsDbContext` | `src/EventManagement.Events.Infrastructure` | `src/EventManagement.Event` | `events` |
| `BookingsDbContext` | `src/EventManagement.Booking` | `src/EventManagement.Booking` | `bookings` |

У каждого сервиса **свой** PostgreSQL (локально — два контейнера в compose) — **database per service**. Миграции независимы: запустите каждый API или `database update` для своего контекста.

В `bookings` колонка `EventId` — логическая ссылка на событие в другом сервисе; **FK между базами невозможен**. Проверка существования события при бронировании — через HTTP (`IEventsClient`).

CLI **dotnet-ef** подключён как **локальный инструмент** (файл `.config/dotnet-tools.json`). Перед работой с миграциями из корня репозитория:

```bash
dotnet tool restore
```

Создание новой миграции (пример имени `InitialCreate` замените при необходимости):

```bash
dotnet tool run dotnet-ef -- migrations add InitialCreate --project src/EventManagement.Events.Infrastructure/EventManagement.Events.Infrastructure.csproj --startup-project src/EventManagement.Event/EventManagement.Events.csproj --context EventsDbContext

dotnet tool run dotnet-ef -- migrations add InitialCreate --project src/EventManagement.Booking/EventManagement.Bookings.csproj --startup-project src/EventManagement.Booking/EventManagement.Bookings.csproj --context BookingsDbContext
```

Применить все неприменённые миграции к базе из командной строки (альтернатива — просто запустить API, там уже вызывается `Migrate()`):

```bash
dotnet tool run dotnet-ef -- database update --project src/EventManagement.Events.Infrastructure/EventManagement.Events.Infrastructure.csproj --startup-project src/EventManagement.Event/EventManagement.Events.csproj --context EventsDbContext

dotnet tool run dotnet-ef -- database update --project src/EventManagement.Booking/EventManagement.Bookings.csproj --startup-project src/EventManagement.Booking/EventManagement.Bookings.csproj --context BookingsDbContext
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

## Модель Event

`Event`:
- `Id` (`Guid`) — идентификатор;
- `Title` (`string`) — название;
- `Description` (`string?`) — описание;
- `StartAt` (`DateTime`) — начало;
- `EndAt` (`DateTime`) — окончание;
- `TotalSeats` (`int`) — общее количество мест;
- `AvailableSeats` (`int`) — текущее количество свободных мест.

## Модель Booking

`Booking`:
- `Id` (`Guid`) — идентификатор брони;
- `EventId` (`Guid`) — идентификатор события;
- `Status` (`BookingStatus`) — статус брони;
- `CreatedAt` (`DateTime`) — дата создания;
- `ProcessedAt` (`DateTime?`) — дата обработки.

`BookingStatus`:
- `Pending` — ожидает обработки;
- `Confirmed` — подтверждена;
- `Rejected` — отклонена.

`Booking` и `Event` находятся в разных микросервисах, поэтому между ними нет навигационных свойств EF Core.
В `Booking` хранится только `EventId`, а существование/состояние события проверяется через HTTP-вызовы в сервис событий.

## Эндпоинты

### Events API (`api/v1/events`)

- `GET /api/v1/events` — список событий с фильтрацией и пагинацией
- `POST /api/v1/events/filter` — фильтрация через тело запроса
- `GET /api/v1/events/{id}` — получить событие по id
- `POST /api/v1/events` — создать событие (`201 Created` + `Location`)
- `PUT /api/v1/events/{id}` — обновить событие
- `DELETE /api/v1/events/{id}` — удалить событие
- `GET /api/v1/events/{id}/exists` — проверить, существует ли событие
- `POST /api/v1/events/{id}/reserve-seats?count=1` — попытка резервирования мест (`true/false`)
- `POST /api/v1/events/{id}/release-seats?count=1` — освобождение мест (`204 No Content`)

### Bookings API

- `POST /api/v1/events/{id}/book`
  - создает бронь для события;
  - возвращает `202 Accepted`;
  - в теле возвращает `BookingInfo` (`Id`, `EventId`, `Status`);
  - в `Location` возвращает ссылку на ресурс брони (`/api/v1/bookings/{bookingId}`);
  - если событие не найдено — `404 Not Found`;
  - если мест больше нет — `409 Conflict`.

- `GET /api/v1/bookings/{id}`
  - возвращает текущее состояние брони;
  - `200 OK` + `BookingDto`;
  - если бронь не найдена — `404 Not Found`.

## Отложенная фоновая обработка

В проекте реализован паттерн **быстрый ответ + отложенная обработка**:
- `POST` на создание брони сразу возвращает `202 Accepted`;
- `BookingBackgroundService` циклически опрашивает очередь с интервалом из `BookingProcessing:PollingIntervalSeconds` (`PeriodicTimer`, по умолчанию 5 с, в Development — 2 с);
- бизнес-обработка вынесена в `BookingProcessingService`;
- список `Pending` бронирований читается в отдельном DI-scope;
- каждая бронь обрабатывается в отдельном DI-scope (отдельный `DbContext` на задачу);
- если событие найдено — бронь подтверждается (`Confirmed`);
- если событие не найдено — бронь отклоняется (`Rejected`) и сохраняется;
- при ошибках обработки бронь отклоняется и выполняется компенсация через освобождение места;
- после обработки `ProcessedAt` заполняется текущим UTC-временем.

## Пример сценария использования

1. Создать событие через `POST /api/v1/events`.
2. Создать бронь через `POST /api/v1/events/{id}/book`.
3. Сразу вызвать `GET /api/v1/bookings/{bookingId}` — статус будет `Pending`.
4. Подождать несколько секунд и повторить `GET` — статус станет `Confirmed`, поле `ProcessedAt` будет заполнено.

## Архитектура

Сервис состоит из двух независимых API:
- **Events** — управление мероприятиями (отдельные сборки Domain / Application / Infrastructure / Presentation);
- **Bookings** — создание и фоновая обработка бронирований (пока один Web-проект с папками слоёв).

Граница между сервисами:
- **HTTP** — `IEventsClient` для проверки события, резервирования мест;
- **Данные** — отдельные БД `events` и `bookings`; `Booking` не обращается к `EventsDbContext` и таблице `events`.

### Events API

- **Domain** — сущность `Event`, `EventNotFoundException` (без EF и ASP.NET).
- **Application** — `EventService`, DTO, FluentValidation; порт `IEventRepository`.
- **Infrastructure** — `EventsDbContext`, `EventRepository`, миграции, `AddInfrastructureServices`, `UseEventsDatabaseInitialization`.
- **Presentation** (`EventManagement.Events`) — тонкие контроллеры, Swagger, exception middleware; composition root в `Program.cs` (`AddApplicationServices`, `AddInfrastructureServices`, `AddPresentationServices`).

### Bookings API

- Папки `Models`, `Application`, `Infrastructure`, `Presentation` внутри одной сборки;
- `BookingBackgroundService` — scoped-зависимости через `IServiceScopeFactory`.

Для локального наполнения Events используется `EventsDataSeeder` + `EventsFactory` (Infrastructure).
Сидирование: `DatabaseInitialization:SeedOnStartup` — `false` в `appsettings.json`, `true` в `appsettings.Development.json`.

## Известные ограничения

- **NSwag.MSBuild** (генерация `EventsClient` при сборке `EventManagement.Events.Api`): сборка может завершиться ошибкой, если **полный путь к каталогу решения содержит пробелы** — это ограничение цепочки NSwag/вызовов при компиляции, а не логики приложения. Обходной путь: держать репозиторий в пути **без пробелов** (например `D:\work\event-management-service`), либо временно отключать/обходить шаг генерации в таком окружении.

## Обработка ошибок

- В `EventManagement.Event` и `EventManagement.Booking` используется собственная `ExceptionHandlingMiddleware`.
- Middleware формирует ответы в формате `application/problem+json` (`ProblemDetails`).
- В `Booking` ошибки `BookingNotFoundException` и `ApiException` маппятся в соответствующие HTTP-статусы.
- В режиме Development в ответ дополнительно добавляются `traceId` и `stackTrace`.