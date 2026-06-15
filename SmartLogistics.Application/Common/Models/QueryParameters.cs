using System;

namespace SmartLogistics.Application.Common.Models
{
    public class QueryParameters
    {
        private const int MaxPageSize = 100; 
        private int _pageSize = 10;          

        
        public int PageNumber { get; set; } = 1;

        
        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
            }
        }

        
        public string? SearchTerm { get; set; }

       
        public string? SortBy { get; set; }

        
        public bool SortDescending { get; set; } = false;
    }
}