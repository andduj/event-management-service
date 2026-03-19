# Event Management Service

Cервис для управления мероприятиями на **ASP.NET Core Web API (.NET 8)**.  
Проект реализует REST API для создания, просмотра, обновления и удаления мероприятий (CRUD), а также фильтрацию, пагинацию и централизованную обработку ошибок.

## Технологии

- **C#**, **.NET 8**
- **ASP.NET Core Web API**
- **Swagger / Swashbuckle**
- **Dependency Injection**
- **AutoMapper**
- **FluentValidation**
- **LINQ / LinqKit**
- **xUnit** (юнит-тесты)

## Запуск проекта

Предполагается, что у вас установлен **.NET SDK 8.0** или выше.

1. Клонировать репозиторий:

```bash
git clone <url-репозитория>
cd event-management-service/src
```

2. Восстановить зависимости и собрать проект:

```bash
dotnet restore
dotnet build
```

3. Запустить приложение:

```bash
dotnet run
```

По умолчанию приложение запускается на адресах, указанных в `launchSettings.json`, например:

- `http://localhost:5167`
- `https://localhost:7216`

## Запуск тестов

Из корня репозитория:

```bash
dotnet test src/EventManagement.sln
```

Тесты находятся в проекте `tests/EventService.Tests`.

## Swagger

Интерактивная документация и возможность тестирования API доступны через **Swagger UI**:

- `http://localhost:5167/swagger`
- или `https://localhost:7216/swagger`

Swagger отображает все эндпоинты и схемы моделей, включая комментарии из XML-документации.

## Модель Event

Сущность мероприятия `Event` содержит следующие поля:

- `Id` (`Guid`, обязательное) — идентификатор мероприятия
- `Title` (`string`, обязательное) — заголовок
- `Description` (`string?`) — описание
- `StartAt` (`DateTime`, обязательное) — дата и время начала
- `EndAt` (`DateTime`, обязательное) — дата и время окончания

Данные хранятся в памяти приложения в `InMemoryEventRepository`.

## Эндпоинты API

Базовый префикс для всех методов: `api/v1/events`.

- **GET `api/v1/events`**  
  Получить список мероприятий с фильтрацией и пагинацией.
  Поддерживаемые query-параметры:
  - `title` (`string`, опционально) — поиск по названию (частичное совпадение, без учета регистра)
  - `from` (`DateTime`, опционально) — события, начинающиеся не раньше указанной даты
  - `to` (`DateTime`, опционально) — события, заканчивающиеся не позже указанной даты
  - `page` (`int`, опционально, по умолчанию `1`) — номер страницы
  - `pageSize` (`int`, опционально, по умолчанию `10`) — размер страницы

  Формат ответа:
  - `items` — элементы текущей страницы
  - `page` — номер текущей страницы
  - `pageSize` — размер страницы
  - `totalItems` — общее количество элементов после фильтрации
  - `totalPages` — общее количество страниц

- **POST `api/v1/events/filter`**  
  Альтернативный способ фильтрации через тело запроса (`EventFilter`) и параметры пагинации `page`, `pageSize`.

- **GET `api/v1/events/{id}`**  
  Получить мероприятие по идентификатору.
  - `404 Not Found`, если мероприятие не найдено.

- **POST `api/v1/events`**  
  Создать новое мероприятие.
  - Тело запроса (`application/json`):

    ```json
    {
      "title": "Sample event",
      "description": "Optional description",
      "startAt": "2026-03-03T10:00:00Z",
      "endAt": "2026-03-03T12:00:00Z"
    }
    ```

  - Ответ: `201 Created` с созданным объектом и заголовком `Location`.

- **PUT `api/v1/events/{id}`**  
  Полностью обновить мероприятие по идентификатору.
  - Если мероприятие не найдено — `404 Not Found`.

- **DELETE `api/v1/events/{id}`**  
  Удалить мероприятие по идентификатору.
  - Если мероприятие не найдено — `404 Not Found`.

## Валидация

Валидация выполняется в `EventsService` через `FluentValidation` (`EventValidator`) для доменной модели `Event`.

Проверяются правила:

- `Title` обязателен;
- `StartAt` обязателен;
- `EndAt` обязателен;
- `EndAt` должна быть **строго позже** `StartAt`.

При нарушении правил API возвращает `400 Bad Request` с деталями ошибок.

## Архитектура и слои

Проект разделён на несколько уровней:

- `Models` — доменные модели (`Event`).
- `Data` — работа с данными (`IEventRepository`, `InMemoryEventRepository`).
- `Application` — бизнес-логика (`IEventsService`, `EventsService`, DTO, запросы, профили AutoMapper).
- `Infrastructure` — регистрация инфраструктурных зависимостей.
- `Presentation` — веб-слой (контроллеры, middleware, расширения, Swagger).

Бизнес-логика вынесена в сервис `EventsService` и подключена через DI.  
Контроллер `EventsController` не содержит бизнес-логики, а только вызывает сервис.

## Обработка ошибок

В проекте реализован глобальный middleware для обработки исключений:

- `ValidationException` маппится в `400 Bad Request`.
- `EventNotFoundException` маппится в `404 Not Found`.
- Непредвиденные ошибки маппятся в `500 Internal Server Error`.
- Поддерживается логирование ошибок через `ILogger`.

Ответ формируется в формате `application/problem+json` (`ProblemDetails`).

## Тестирование через `.http` файл

В корне проекта `src` есть файл `EventManagement.http` с примерами запросов:

- `GET /api/v1/events`
- `POST /api/v1/events`

## Юнит-тесты

Для сервиса `EventsService` реализован набор тестов на:

- успешные сценарии (`Add`, `GetById`, `Update`, `Delete`);
- фильтрацию по названию и датам;
- пагинацию и комбинированную фильтрацию;
- неуспешные сценарии (несуществующий `id`, ошибки валидации).