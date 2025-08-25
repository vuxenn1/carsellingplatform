using System.Collections.Generic;

namespace CARDB_EF.Models.DTOs
{
    public class PagedCarList<T>
    {
        public List<T> Items { get; set; }
        public int TotalItems { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)System.Math.Ceiling((double)TotalItems / PageSize);
    }
}