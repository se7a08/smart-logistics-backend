using System;
using System.Collections.Generic;

namespace SmartLogistics.Application.Common.Models
{
    
    public class PaginatedList<T>
    {
       
        public List<T> Items { get; set; } = new List<T>();

        
        public int TotalCount { get; set; }

        
        public int PageNumber { get; set; }

       
        public int PageSize { get; set; }

        
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        
        public static PaginatedList<T> Create(List<T> items, int totalCount, int pageNumber, int pageSize)
        {
            return new PaginatedList<T>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}