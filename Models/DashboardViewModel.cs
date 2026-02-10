using System.Collections.Generic;

namespace CRM.Models
{
    public class DashboardViewModel
    {
        // 1. The Big Numbers
        public int TotalCustomers { get; set; }
        public int NewCustomersThisMonth { get; set; }
        public int TotalContacts { get; set; }

        // 2. The Lists (for tables/feeds)
        public List<Note> RecentActivities { get; set; } = new List<Note>();

        // 3. Chart Data (Simple version)
        public Dictionary<string, int> CustomersByIndustry { get; set; } = new Dictionary<string, int>();
    }
}