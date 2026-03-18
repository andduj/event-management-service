using System.Collections.ObjectModel;

namespace EventManagement.Application.DTOs
{
    /// <summary>
    /// Результат выборки с пагинацией.
    /// </summary>
    public class PaginatedResult<T>
    {
        /// <summary>
        /// Элементы текущей страницы.
        /// </summary>
        public ReadOnlyCollection<T> Items { get; set; }

        /// <summary>
        /// Номер текущей страницы.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Размер страницы.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Общее количество элементов.
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Общее количество страниц.
        /// </summary>
        public int TotalPages { get; set; }
    }
}
