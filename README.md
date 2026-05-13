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
- **Entity Framework Core**
- **PostgreSQL** (`Npgsql`)
- **xUnit**, **Moq**, **FluentAssertions**, **AutoFixture**

## Структура решения

- `src/EventManagement.Event` — API для работы с сущностью `Event`
- `src/EventManagement.Booking` — API для работы с сущностью `Booking` и фоновой обработкой
- `tests/EventManagement.Events.Tests` — модульные тесты событий
- `tests/EventManagement.Bookings.Tests` — модульные тесты бронирований
- `tests/EventApi.IntegrationTests` — интеграционные тесты (PostgreSQL через Testcontainers)

## Запуск

Требуется:
- **.NET SDK 9.0+**
- **Docker** (для локального PostgreSQL)

```bash
dotnet restore
dotnet build EventManagement.sln
```

Запуск PostgreSQL:

```bash
docker compose -f docker/docker-compose.yml up -d
```

Строка подключения по умолчанию:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=eventapi;Username=;Password="
}
```

Пароль к базе хранится в **User Secrets** (не в репозитории). Для локальной настройки:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=eventapi;Username=postgres;Password=postgres" --project src/EventManagement.Event/EventManagement.Events.csproj
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=eventapi;Username=postgres;Password=postgres" --project src/EventManagement.Booking/EventManagement.Bookings.csproj
```

### Схема базы и миграции EF Core

Схема PostgreSQL **не создаётся через `EnsureCreated()`**: при старте каждого API вызывается `Database.Migrate()`, поэтому актуальная структура таблиц задаётся **миграциями EF Core**.

В решении два контекста и два набора миграций (отдельный сервис событий и отдельный сервис бронирований):

- `EventsDbContext` — проект `src/EventManagement.Event`, папка `Migrations`
- `BookingsDbContext` — проект `src/EventManagement.Booking`, папка `Migrations`

Локально оба API часто используют **одну и ту же базу** (`Database=eventapi` в строке подключения). Убедитесь, что при первом развёртывании миграции событий успевают примениться до миграций бронирований, если в схеме есть связь `bookings` → `events` (например, сначала запустите Events API, затем Bookings API, либо примените миграции командами ниже в осмысленном порядке).

CLI **dotnet-ef** подключён как **локальный инструмент** (файл `.config/dotnet-tools.json`). Перед работой с миграциями из корня репозитория:

```bash
dotnet tool restore
```

Создание новой миграции (пример имени `InitialCreate` замените при необходимости):

```bash
dotnet tool run dotnet-ef -- migrations add InitialCreate --project src/EventManagement.Event/EventManagement.Events.csproj --startup-project src/EventManagement.Event/EventManagement.Events.csproj --context EventsDbContext

dotnet tool run dotnet-ef -- migrations add InitialCreate --project src/EventManagement.Booking/EventManagement.Bookings.csproj --startup-project src/EventManagement.Booking/EventManagement.Bookings.csproj --context BookingsDbContext
```

Применить все неприменённые миграции к базе из командной строки (альтернатива — просто запустить API, там уже вызывается `Migrate()`):

```bash
dotnet tool run dotnet-ef -- database update --project src/EventManagement.Event/EventManagement.Events.csproj --startup-project src/EventManagement.Event/EventManagement.Events.csproj --context EventsDbContext

dotnet tool run dotnet-ef -- database update --project src/EventManagement.Booking/EventManagement.Bookings.csproj --startup-project src/EventManagement.Booking/EventManagement.Bookings.csproj --context BookingsDbContext
```

Если `dotnet-ef` не видит строку подключения, передайте её явно: добавьте в конец команды параметр `--connection "..."` с теми же `Host`, `Database`, `Username` и `Password`, что в User Secrets.

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

Проект `tests/EventApi.IntegrationTests` поднимает **настоящий PostgreSQL в Docker** через **Testcontainers**. Без запущенного **Docker Desktop** (или иного демона Docker) эти тесты не смогут стартовать контейнер и завершатся ошибкой. Перед `dotnet test` убедитесь, что Docker работает.

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
- `BookingBackgroundService` циклически запускает обработку ожидающих заявок;
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
- `Event` — управление мероприятиями;
- `Booking` — создание и фоновая обработка бронирований.

Граница между сервисами проходит по HTTP API:
- `Booking` **не использует** `DbContext` или репозитории `Event`-сервиса напрямую;
- взаимодействие выполняется через клиент `IEventsClient` (контракт `Event` API);
- репозитории в каждом сервисе работают только со «своими» сущностями и своим `DbContext`; при локальной разработке оба API могут указывать на **одну** базу PostgreSQL, но доступ к чужим таблицам через чужой контекст не выполняется.

Это сделано намеренно, чтобы сохранить изоляцию сервисов и независимость хранения данных.

Проект разделен по слоям:
- `Models` — доменные модели;
- `Data` — репозитории и доступ к данным;
- `Application` — сервисы, DTO и бизнес-правила;
- `Infrastructure` — конфигурация DI и фоновые задачи;
- `Presentation` — контроллеры, Swagger, middleware.

Для локального наполнения данных в `Event` API используется `EventsDataSeeder` + `EventsFactory`.
Сидирование управляется флагом `DatabaseInitialization:SeedOnStartup`:
- `false` в `appsettings.json`;
- `true` в `appsettings.Development.json`.

## Известные ограничения

- **NSwag.MSBuild** (генерация `EventsClient` при сборке `EventManagement.Events.Api`): сборка может завершиться ошибкой, если **полный путь к каталогу решения содержит пробелы** — это ограничение цепочки NSwag/вызовов при компиляции, а не логики приложения. Обходной путь: держать репозиторий в пути **без пробелов** (например `D:\work\event-management-service`), либо временно отключать/обходить шаг генерации в таком окружении.

## Обработка ошибок

- В `EventManagement.Event` и `EventManagement.Booking` используется собственная `ExceptionHandlingMiddleware`.
- Middleware формирует ответы в формате `application/problem+json` (`ProblemDetails`).
- В `Booking` ошибки `BookingNotFoundException` и `ApiException` маппятся в соответствующие HTTP-статусы.
- В режиме Development в ответ дополнительно добавляются `traceId` и `stackTrace`.