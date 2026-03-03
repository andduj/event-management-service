# Event Management Service

Учебный сервис для управления мероприятиями на **ASP.NET Core Web API (.NET 8)**.  
Проект реализует базовый REST API для создания, просмотра, обновления и удаления мероприятий (CRUD) с хранением данных в памяти приложения.

## Технологии

- **C#**, **.NET 8**
- **ASP.NET Core Web API**
- **Swagger / Swashbuckle**
- **Dependency Injection**
- **AutoMapper**

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

## Swagger

Интерактивная документация и возможность тестирования API доступны через **Swagger UI**:

- `http://localhost:5167/swagger`
- или `https://localhost:7216/swagger`

Swagger отображает все эндпоинты и схемы моделей, включая комментарии из XML-документации.

## Модель Event

Сущность мероприятия `Event` содержит следующие поля:

- `Id` (`Guid`, обязательное) — идентификатор мероприятия
- `Title` (`string`, обязательное) — заголовок
- `Description` (`string?`, опциональное) — описание
- `StartAt` (`DateTime`, обязательное) — дата и время начала
- `EndAt` (`DateTime`, обязательное) — дата и время окончания

Данные хранятся в памяти приложения в `InMemoryEventRepository`.

## Эндпоинты API

Базовый префикс для всех методов: `api/v1/events`.

- **GET `api/v1/events`**  
  Получить список всех мероприятий.

- **GET `api/v1/events/{id}`**  
  Получить мероприятие по идентификатору.
  - `404 Not Found`, если событие не найдено.

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
  - Если событие не найдено — `404 Not Found`.

- **DELETE `api/v1/events/{id}`**  
  Удалить мероприятие по идентификатору.
  - Если событие не найдено — `404 Not Found`.

## Валидация

Для входных моделей (`AddEventRequest`, `UpdateEventRequest`) настроена валидация:

- Обязательные поля:
  - `Title`
  - `StartAt`
  - `EndAt`
- Дополнительное правило:
  - `EndAt` должна быть **строго позже** `StartAt`.

При нарушении правил валидации API возвращает код `400 Bad Request` с описанием ошибок.

## Архитектура и слои

Проект разделён на несколько уровней:

- `Models` — доменные модели (`Event`).
- `Data` — работа с данными (`IEventRepository`, `InMemoryEventRepository`).
- `Application` — бизнес-логика (`IEventService`, `EventService`, DTO, запросы, профили AutoMapper).
- `Infrastructure` — регистрация инфраструктурных зависимостей.
- `Presentation` — веб-слой (контроллеры, middleware, расширения, Swagger).

Бизнес-логика вынесена в сервис `EventService` и подключена через DI.  
Контроллер `EventsController` не содержит бизнес-логики, а только вызывает сервис.

## Обработка ошибок

В проекте реализован глобальный middleware для обработки исключений:

- `EventNotFoundException` маппится в `404 Not Found`.
- Другие ошибки — в соответствующие HTTP-статусы (`400`, `500` и т.д.).

Ответ формируется в формате `application/problem+json` (`ProblemDetails`).

## Тестирование через `.http` файл

В корне проекта `src` есть файл `EventManagement.http` с примерами запросов:

- `GET /api/v1/events`
- `POST /api/v1/events`