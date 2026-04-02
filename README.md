# Event Management Service

Сервис управления мероприятиями и бронированиями на **ASP.NET Core Web API (.NET 8)**.
Проект включает:
- CRUD и фильтрацию событий;
- создание брони с быстрым ответом (`202 Accepted`);
- отложенную обработку бронирований в фоне через `BackgroundService`.

## Технологии

- **C#**, **.NET 8**
- **ASP.NET Core Web API**
- **Swagger / Swashbuckle**
- **NLog** (через проект `EventManagement.Logging`)
- **Dependency Injection**
- **AutoMapper**
- **FluentValidation**
- **xUnit**, **Moq**, **FluentAssertions**, **AutoFixture**

## Структура решения

- `src/EventManagement.Event` — API для работы с сущностью `Event`
- `src/EventManagement.Booking` — API для работы с сущностью `Booking` и фоновой обработкой
- `tests/EventManagement.Events.Tests` — тесты событий
- `tests/EventManagement.Bookings.Tests` — тесты бронирований

## Запуск

Требуется установленный **.NET SDK 8.0+**.

```bash
dotnet restore
dotnet build EventManagement.sln
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

```bash
dotnet test EventManagement.sln
```

Или отдельно:

```bash
dotnet test tests/EventManagement.Events.Tests/EventManagement.Events.Tests.csproj
dotnet test tests/EventManagement.Bookings.Tests/EventManagement.Bookings.Tests.csproj
```

## Swagger

Swagger UI доступен для каждого API в режиме Development:
- Events API: `http://localhost:5167/swagger` или `https://localhost:7216/swagger`
- Bookings API: `http://localhost:5236/swagger` или `https://localhost:7095/swagger`

## Модель Event

`Event`:
- `Id` (`Guid`) — идентификатор;
- `Title` (`string`) — название;
- `Description` (`string?`) — описание;
- `StartAt` (`DateTime`) — начало;
- `EndAt` (`DateTime`) — окончание.

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

Данные бронирований хранятся в памяти приложения (`InMemoryBookingRepository`).

## Эндпоинты

### Events API (`api/v1/events`)

- `GET /api/v1/events` — список событий с фильтрацией и пагинацией
- `POST /api/v1/events/filter` — фильтрация через тело запроса
- `GET /api/v1/events/{id}` — получить событие по id
- `POST /api/v1/events` — создать событие (`201 Created` + `Location`)
- `PUT /api/v1/events/{id}` — обновить событие
- `DELETE /api/v1/events/{id}` — удалить событие

### Bookings API

- `POST /api/v1/events/{id}/book`
  - создает бронь для события;
  - возвращает `202 Accepted`;
  - в теле возвращает `BookingInfo` (`Id`, `EventId`, `Status`);
  - в `Location` возвращает ссылку на ресурс брони (`/api/v1/bookings/{bookingId}`);
  - если событие не найдено — `404 Not Found`.

- `GET /api/v1/bookings/{id}`
  - возвращает текущее состояние брони;
  - `200 OK` + `BookingDto`;
  - если бронь не найдена — `404 Not Found`.

## Отложенная фоновая обработка

В проекте реализован паттерн **быстрый ответ + отложенная обработка**:
- `POST` на создание брони сразу возвращает `202 Accepted`;
- `BookingBackgroundService` периодически (polling) запускает обработку ожидающих заявок;
- бизнес-обработка вынесена в `BookingProcessingService`;
- для каждой `Pending` брони выполняется искусственная задержка (`Task.Delay`), имитирующая внешний вызов;
- после обработки статус меняется на `Confirmed`, а `ProcessedAt` заполняется текущим UTC-временем.

## Пример сценария использования

1. Создать событие через `POST /api/v1/events`.
2. Создать бронь через `POST /api/v1/events/{id}/book`.
3. Сразу вызвать `GET /api/v1/bookings/{bookingId}` — статус будет `Pending`.
4. Подождать несколько секунд и повторить `GET` — статус станет `Confirmed`, поле `ProcessedAt` будет заполнено.

## Архитектура

Проект разделен по слоям:
- `Models` — доменные модели;
- `Data` — репозитории и доступ к данным;
- `Application` — сервисы, DTO и бизнес-правила;
- `Infrastructure` — конфигурация DI и фоновые задачи;
- `Presentation` — контроллеры, Swagger, middleware.

## Обработка ошибок

- В `EventManagement.Event` и `EventManagement.Booking` используется собственная `ExceptionHandlingMiddleware`.
- Middleware формирует ответы в формате `application/problem+json` (`ProblemDetails`).
- В `Booking` ошибки `BookingNotFoundException` и `ApiException` маппятся в соответствующие HTTP-статусы.
- В режиме Development в ответ дополнительно добавляются `traceId` и `stackTrace`.