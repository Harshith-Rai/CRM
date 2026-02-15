namespace CRM.DTOS.SalesManager
{
    public class CustomerDashboardDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }

        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public string SalesRepId { get; set; }
        public string SalesRepName { get; set; }
    }
}