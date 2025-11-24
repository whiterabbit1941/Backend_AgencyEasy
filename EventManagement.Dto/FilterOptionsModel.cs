using System;

namespace EventManagement.Dto
{
    /// <summary>
    /// Fitler Model
    /// </summary>
    public class FilterOptionsModel
    {
        const int maxPageSize = 200;

        /// <summary>
        /// Page Number.
        /// </summary>
        public int PageNumber { get; set; } = 1;

        private int _pageSize = 10;

        /// <summary>
        /// Page Size.
        /// </summary>
        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = (value > maxPageSize) ? maxPageSize : value;
            }
        }

        /// <summary>
        /// Filter Criteria
        /// </summary>
        public string SearchQuery { get; set; }

        /// <summary>
        /// Order By Criteria.
        /// </summary>
        public string OrderBy { get; set; }

        /// <summary>
        /// Fields to select.
        /// </summary>
        public string Fields { get; set; }

    }
}
