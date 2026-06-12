using EventManagement.Events.Domain.Models;
using System;
using System.Collections.Generic;

namespace EventManagement.Events.Infrastructure.Data
{
    /// <summary>
    /// Фабрика для создания тестовых данных мероприятий.
    /// </summary>
    public static class EventsFactory
    {
        public static List<Event> Create()
        {
            var events = new List<Event>
            {
                Event.Create(
                    title: "Новогодний концерт",
                    startAt: new DateTime(2026, 1, 3, 18, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 1, 3, 21, 0, 0, DateTimeKind.Utc),
                    totalSeats: 1200,
                    description: "Праздничный концерт с участием популярных артистов. В программе: любимые новогодние песни, танцевальные номера и праздничная атмосфера. Гостей ждут сюрпризы и подарки от Деда Мороза и Снегурочки."),
                Event.Create(
                    title: "Рождественская ярмарка",
                    startAt: new DateTime(2026, 1, 6, 12, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 1, 6, 20, 0, 0, DateTimeKind.Utc),
                    totalSeats: 3000,
                    description: "Традиционная рождественская ярмарка с угощениями, сувенирами и развлечениями. Горячий глинтвейн, имбирные пряники, изделия ремесленников и выступления фольклорных коллективов."),
                Event.Create(
                    title: "Спектакль 'Снежная королева'",
                    startAt: new DateTime(2026, 1, 10, 12, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 1, 10, 14, 30, 0, DateTimeKind.Utc),
                    totalSeats: 500,
                    description: "Сказочное представление для всей семьи по мотивам произведения Г.Х. Андерсена. Красочные костюмы, талантливые актеры и захватывающий сюжет о настоящей дружбе и любви."),
                Event.Create(
                    title: "Джазовый вечер",
                    startAt: new DateTime(2026, 1, 17, 20, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 1, 17, 23, 0, 0, DateTimeKind.Utc),
                    totalSeats: 120,
                    description: "Камерный джазовый концерт в уютном клубе. Выступление трио под руководством известного саксофониста. В программе: классика джаза и авторские композиции."),
                Event.Create(
                    title: "День всех влюбленных",
                    startAt: new DateTime(2026, 2, 14, 19, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 2, 14, 23, 0, 0, DateTimeKind.Utc),
                    totalSeats: 200,
                    description: "Романтический вечер для пар. Живая музыка, специальное меню от шеф-повара, конкурсы и розыгрыш романтического уикенда. Идеальное место для свидания."),
                Event.Create(
                    title: "Масленица",
                    startAt: new DateTime(2026, 2, 15, 12, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 2, 15, 18, 0, 0, DateTimeKind.Utc),
                    totalSeats: 2500,
                    description: "Народное гулянье с блинами, хороводами и сжиганием чучела. Выступления фольклорных ансамблей, конкурсы, игры и традиционные угощения для всех гостей."),
                Event.Create(
                    title: "Выставка современного искусства",
                    startAt: new DateTime(2026, 2, 20, 15, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 2, 20, 21, 0, 0, DateTimeKind.Utc),
                    totalSeats: 600,
                    description: "Экспозиция работ молодых художников на тему 'Город и человек'. Живопись, графика, инсталляции и фотография. Вход свободный, экскурсии от авторов."),
                Event.Create(
                    title: "Стендап-концерт",
                    startAt: new DateTime(2026, 2, 21, 20, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 2, 21, 22, 30, 0, DateTimeKind.Utc),
                    totalSeats: 800,
                    description: "Большой стендап-концерт с участием резидентов известных комедийных шоу. Два часа искрометного юмора, импровизаций и хорошего настроения."),
                Event.Create(
                    title: "Концерт к 8 марта",
                    startAt: new DateTime(2026, 3, 7, 17, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 3, 7, 20, 0, 0, DateTimeKind.Utc),
                    totalSeats: 900,
                    description: "Праздничный концерт, посвященный Международному женскому дню. Выступление звезд эстрады, балета и симфонического оркестра. Только красивая музыка и поздравления."),
                Event.Create(
                    title: "Фестиваль тюльпанов",
                    startAt: new DateTime(2026, 3, 14, 11, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 3, 14, 19, 0, 0, DateTimeKind.Utc),
                    totalSeats: 1800,
                    description: "Выставка-продажа тюльпанов и весенних цветов. Мастер-классы по флористике, фотозоны, выступления уличных музыкантов и весеннее настроение для всех гостей."),
                Event.Create(
                    title: "Лекция 'Архитектура Петербурга'",
                    startAt: new DateTime(2026, 3, 18, 19, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 3, 18, 21, 30, 0, DateTimeKind.Utc),
                    totalSeats: 100,
                    description: "Образовательная лекция об истории архитектуры Северной столицы. От барокко до модерна: интересные факты, редкие фотографии и секреты старых зданий."),
                Event.Create(
                    title: "День космонавтики",
                    startAt: new DateTime(2026, 4, 12, 14, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 4, 12, 20, 0, 0, DateTimeKind.Utc),
                    totalSeats: 500,
                    description: "Познавательная программа для детей и взрослых, посвященная Дню космонавтики. Лекции астрономов, показ документальных фильмов, наблюдение за звездами в телескоп."),
                Event.Create(
                    title: "Весенний бал",
                    startAt: new DateTime(2026, 4, 18, 18, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 4, 18, 23, 0, 0, DateTimeKind.Utc),
                    totalSeats: 250,
                    description: "Традиционный весенний бал в историческом особняке. Живая музыка, танцевальные мастер-классы, атмосфера XIX века. Дресс-код: вечерние наряды."),
                Event.Create(
                    title: "Книжная ярмарка",
                    startAt: new DateTime(2026, 4, 25, 11, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 4, 25, 20, 0, 0, DateTimeKind.Utc),
                    totalSeats: 1200,
                    description: "Весенняя ярмарка книг от независимых издательств. Встречи с авторами, презентации новинок, скидки на книги и лекции о современной литературе."),
                Event.Create(
                    title: "Концерт фортепианной музыки",
                    startAt: new DateTime(2026, 4, 26, 19, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 4, 26, 21, 0, 0, DateTimeKind.Utc),
                    totalSeats: 200,
                    description: "Сольный концерт лауреата международных конкурсов. В программе: Шопен, Лист, Рахманинов. Вечер в атмосферном зале старинной усадьбы."),
                Event.Create(
                    title: "Праздничный салют",
                    startAt: new DateTime(2026, 5, 9, 15, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 5, 9, 23, 0, 0, DateTimeKind.Utc),
                    totalSeats: 8000,
                    description: "Празднование Дня Победы. Торжественный концерт, полевая кухня, выставка военной техники и праздничный фейерверк над Невой."),
                Event.Create(
                    title: "Фестиваль уличных театров",
                    startAt: new DateTime(2026, 5, 16, 13, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 5, 16, 21, 0, 0, DateTimeKind.Utc),
                    totalSeats: 4000,
                    description: "Фестиваль уличных театров в центре города. Клоуны, мимы, ходулисты и театральные труппы со всей страны. Яркое шоу для всей семьи."),
                Event.Create(
                    title: "Ночь музеев",
                    startAt: new DateTime(2026, 5, 23, 18, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 5, 24, 6, 0, 0, DateTimeKind.Utc),
                    totalSeats: 5000,
                    description: "Ежегодная акция 'Ночь музеев'. Специальная программа в музеях города, ночные экскурсии, концерты и перформансы. Единый билет дает право посещения всех площадок."),
                Event.Create(
                    title: "День защиты детей",
                    startAt: new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 6, 1, 18, 0, 0, DateTimeKind.Utc),
                    totalSeats: 3500,
                    description: "Большой праздник для детей в городском парке. Аниматоры, аттракционы, сладкая вата, мыльные пузыри и выступления детских творческих коллективов."),
                Event.Create(
                    title: "Пушкинский день",
                    startAt: new DateTime(2026, 6, 6, 11, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 6, 6, 20, 0, 0, DateTimeKind.Utc),
                    totalSeats: 1500,
                    description: "Празднование дня рождения А.С. Пушкина. Чтения стихов, спектакли под открытым небом, литературные квесты и концерты в усадьбах поэта."),
                Event.Create(
                    title: "Фестиваль красок",
                    startAt: new DateTime(2026, 6, 20, 14, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 6, 20, 19, 0, 0, DateTimeKind.Utc),
                    totalSeats: 2000,
                    description: "Фестиваль красок Холи. Музыка, танцы и море ярких красок. Все участники получают пакетики с безопасными красками. Зажигательная атмосфера праздника."),
                Event.Create(
                    title: "Музыкальный фестиваль",
                    startAt: new DateTime(2026, 6, 26, 16, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 6, 28, 23, 0, 0, DateTimeKind.Utc),
                    totalSeats: 20000,
                    description: "Трехдневный опен-эйр фестиваль на живописном берегу залива. Хедлайнеры мирового уровня, кемпинг, фуд-корты и незабываемая атмосфера."),
                Event.Create(
                    title: "День семьи, любви и верности",
                    startAt: new DateTime(2026, 7, 8, 16, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 7, 8, 21, 0, 0, DateTimeKind.Utc),
                    totalSeats: 700,
                    description: "Праздник, посвященный Дню семьи. Концерт, конкурсы для семей, чествование многодетных семей и юбиляров супружеской жизни. Ромашковое настроение."),
                Event.Create(
                    title: "Фестиваль воздушных змеев",
                    startAt: new DateTime(2026, 7, 12, 11, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 7, 12, 18, 0, 0, DateTimeKind.Utc),
                    totalSeats: 900,
                    description: "Красочный фестиваль воздушных змеев на побережье. Мастер-классы по созданию змеев, соревнования, шоу гигантских фигур и пикник на траве."),
                Event.Create(
                    title: "Ночной забег",
                    startAt: new DateTime(2026, 7, 18, 22, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 7, 19, 1, 0, 0, DateTimeKind.Utc),
                    totalSeats: 12000,
                    description: "Ночной легкоатлетический забег по историческому центру. Дистанции 5 и 10 км. Светящиеся браслеты, подсветка трассы и незабываемая атмосфера ночного города."),
                Event.Create(
                    title: "Оперный фестиваль",
                    startAt: new DateTime(2026, 7, 25, 20, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 7, 25, 23, 0, 0, DateTimeKind.Utc),
                    totalSeats: 4500,
                    description: "Летний фестиваль оперного искусства под открытым небом. Известные арии, сцены из опер и гала-концерт звезд мировой оперы."),
                Event.Create(
                    title: "День ВМФ",
                    startAt: new DateTime(2026, 8, 2, 10, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 8, 2, 23, 0, 0, DateTimeKind.Utc),
                    totalSeats: 30000,
                    description: "Празднование Дня Военно-морского флота. Парад кораблей, показательные выступления моряков, концерт и праздничный салют в акватории Невы."),
                Event.Create(
                    title: "Фестиваль цветов",
                    startAt: new DateTime(2026, 8, 8, 12, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 8, 8, 20, 0, 0, DateTimeKind.Utc),
                    totalSeats: 3500,
                    description: "Городской фестиваль цветов и ландшафтного дизайна. Цветочные композиции, парад флористов, конкурсы и мастер-классы для всех желающих."),
                Event.Create(
                    title: "Яблочный спас",
                    startAt: new DateTime(2026, 8, 19, 13, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 8, 19, 19, 0, 0, DateTimeKind.Utc),
                    totalSeats: 1500,
                    description: "Фольклорный праздник, посвященный Яблочному спасу. Дегустация яблок, меда и выпечки, народные игры, хороводы и выступления фольклорных ансамблей."),
                Event.Create(
                    title: "День кино",
                    startAt: new DateTime(2026, 8, 27, 18, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 8, 27, 23, 0, 0, DateTimeKind.Utc),
                    totalSeats: 2500,
                    description: "Празднование Дня российского кино. Показы любимых фильмов под открытым небом, встречи с актерами, лекции о кино и киноконцерты."),
                Event.Create(
                    title: "День знаний",
                    startAt: new DateTime(2026, 9, 1, 14, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 9, 1, 19, 0, 0, DateTimeKind.Utc),
                    totalSeats: 3000,
                    description: "Праздничная программа для школьников и студентов в городском парке. Концерт, интерактивные площадки, квесты и подарки к началу учебного года."),
                Event.Create(
                    title: "День города",
                    startAt: new DateTime(2026, 9, 12, 12, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 9, 13, 2, 0, 0, DateTimeKind.Utc),
                    totalSeats: 40000,
                    description: "Празднование Дня основания города. Карнавальное шествие, концерты на главных площадях, фейерверк и народные гуляния до утра."),
                Event.Create(
                    title: "Осенний марафон",
                    startAt: new DateTime(2026, 9, 20, 9, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 9, 20, 16, 0, 0, DateTimeKind.Utc),
                    totalSeats: 15000,
                    description: "Традиционный осенний марафон. Дистанции: 5 км, 10 км, 21,1 км и 42,2 км. Профессиональная трасса, пункты питания и медали всем финишерам."),
                Event.Create(
                    title: "Фестиваль тыквы",
                    startAt: new DateTime(2026, 9, 26, 12, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 9, 26, 18, 0, 0, DateTimeKind.Utc),
                    totalSeats: 800,
                    description: "Кулинарный фестиваль, посвященный тыкве. Дегустация блюд из тыквы, конкурсы на лучший тыквенный пирог, выставка тыквенных композиций."),
                Event.Create(
                    title: "День учителя",
                    startAt: new DateTime(2026, 10, 5, 16, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 10, 5, 20, 0, 0, DateTimeKind.Utc),
                    totalSeats: 600,
                    description: "Праздничный концерт, посвященный Дню учителя. Выступления творческих коллективов, чествование лучших педагогов и праздничный фуршет."),
                Event.Create(
                    title: "Фестиваль науки",
                    startAt: new DateTime(2026, 10, 11, 11, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 10, 11, 19, 0, 0, DateTimeKind.Utc),
                    totalSeats: 2000,
                    description: "Интерактивный фестиваль для детей и взрослых. Научные шоу, эксперименты, лекции ученых и демонстрация современных технологий."),
                Event.Create(
                    title: "Хэллоуин party",
                    startAt: new DateTime(2026, 10, 31, 21, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 11, 1, 3, 0, 0, DateTimeKind.Utc),
                    totalSeats: 400,
                    description: "Костюмированная вечеринка в честь Хэллоуина. Конкурс костюмов, тематические коктейли, мистическая музыка и розыгрыш призов за лучший образ."),
                Event.Create(
                    title: "День народного единства",
                    startAt: new DateTime(2026, 11, 4, 13, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 11, 4, 20, 0, 0, DateTimeKind.Utc),
                    totalSeats: 5000,
                    description: "Праздничные мероприятия, посвященные Дню народного единства. Концерт, флешмобы, выставки народных промыслов и патриотические акции."),
                Event.Create(
                    title: "Фестиваль света",
                    startAt: new DateTime(2026, 11, 14, 19, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 11, 14, 23, 0, 0, DateTimeKind.Utc),
                    totalSeats: 6000,
                    description: "Фестиваль световых инсталляций и видеомэппинга. Преображение городского пространства с помощью света, лазеров и проекций на фасады зданий."),
                Event.Create(
                    title: "День матери",
                    startAt: new DateTime(2026, 11, 29, 16, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 11, 29, 20, 0, 0, DateTimeKind.Utc),
                    totalSeats: 800,
                    description: "Праздничный концерт, посвященный Дню матери. Трогательные поздравления, выступления детских коллективов и подарки для всех мам."),
                Event.Create(
                    title: "Джазовые вечера",
                    startAt: new DateTime(2026, 11, 21, 20, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 11, 21, 23, 0, 0, DateTimeKind.Utc),
                    totalSeats: 150,
                    description: "Серия джазовых концертов в уютном клубе. Лучшие джазовые музыканты города, импровизации и атмосфера старого Нового Орлеана."),
                Event.Create(
                    title: "Открытие новогодней елки",
                    startAt: new DateTime(2026, 12, 20, 17, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 12, 20, 21, 0, 0, DateTimeKind.Utc),
                    totalSeats: 8000,
                    description: "Торжественное открытие главной городской елки. Театрализованное представление, хороводы с Дедом Морозом, праздничный салют и подарки."),
                Event.Create(
                    title: "Новогодний балет 'Щелкунчик'",
                    startAt: new DateTime(2026, 12, 25, 18, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2026, 12, 25, 21, 30, 0, DateTimeKind.Utc),
                    totalSeats: 1200,
                    description: "Праздничный показ балета 'Щелкунчик' в исполнении лучшей балетной труппы города. Волшебная музыка Чайковского и сказочная атмосфера."),
                Event.Create(
                    title: "Новогодний корпоратив",
                    startAt: new DateTime(2026, 12, 31, 22, 0, 0, DateTimeKind.Utc),
                    endAt: new DateTime(2027, 1, 1, 5, 0, 0, DateTimeKind.Utc),
                    totalSeats: 500,
                    description: "Большая новогодняя вечеринка для компаний и частных лиц. Фуршет, конкурсы, выступление кавер-бэнда и встреча Нового года с шампанским."),
            };

            return events;
        }
    }
}