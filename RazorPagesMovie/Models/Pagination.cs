namespace RazorPagesMovie.Models
{
    public class Pagination<T>
    {
        public List<T> Items { get; set; } = new List<T>(); // List of items for the current page
        public int TotalRecords { get; set; } // Total number of records
        public int PageSize { get; set; } // Number of records per page
        public int CurrentPage { get; set; } // Current page number
        public int TotalPages => (int)Math.Ceiling(TotalRecords / (double)PageSize); // Total number of pages
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        public Pagination(List<T> items, int totalRecords, int pageSize, int currentPage)
        {
            Items = items;
            TotalRecords = totalRecords;
            PageSize = pageSize;
            CurrentPage = currentPage;
        }
    }

}
