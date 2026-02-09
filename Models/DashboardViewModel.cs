namespace CRM.Models
{
    public class DashboardViewModel
    {
        public int TotalCustomers { get; set; }
        public int TotalContacts { get; set; }
        public int ActiveLeads { get; set; }

        // Activity Feed: The most recent 5-10 notes across all customers
        public List<Note> RecentInteractions { get; set; }
    }
}