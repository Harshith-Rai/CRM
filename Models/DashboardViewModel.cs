using CRM.DTOS.SalesManager;
using System.Collections.Generic;

namespace CRM.Models
{
    public class DashboardViewModel
    {
        public int TotalCustomers { get; set; }
        public int NewCustomersThisMonth { get; set; }
        public int TotalContacts { get; set; }
        public decimal TotalPipelineValue { get; set; }

        public List<Note> RecentActivities { get; set; } = new List<Note>();

        // For Industry Pie Chart
        public Dictionary<string, int> CustomersByIndustry { get; set; } = new Dictionary<string, int>();
        public decimal TotalRevenueWon { get; set; }

        // For Monthly Growth Line Chart
        public Dictionary<string, int> MonthlyGrowth { get; set; } = new Dictionary<string, int>();
        public IEnumerable<CustomerDashboardDto> RecentlyAddedCustomers { get; set; }

    }
}