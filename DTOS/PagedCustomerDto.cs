namespace CRM.DTOS
{
    public class PagedCustomerDto<T>
    {
        public List<T> Customers { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public string SearchTerm { get; set; }
        public string Status { get; set; }
        public string Industry { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        public int StartRecord => ((PageNumber - 1) * PageSize) + 1;
        public int EndRecord => Math.Min(PageNumber * PageSize, TotalCount);
    }
}
