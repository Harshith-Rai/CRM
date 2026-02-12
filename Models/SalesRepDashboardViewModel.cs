using System.Collections.Generic;

namespace CRM.Models
{
    public class SalesRepDashboardViewModel
    {
        // --- 1. Top Card Stats (Real DB Data) ---
        public int TotalCustomers { get; set; }
        public int NewCustomersThisMonth { get; set; }
        public int TotalContacts { get; set; }

        // Revenue Stats
        public decimal TotalSalesThisMonth { get; set; } // Was 'TotalRevenueWon'
        public decimal MonthlyTarget { get; set; } = 20000; // Hardcoded target for now
        public decimal TotalPipelineValue { get; set; }

        // Helper for the Progress Bar
        public int TargetProgressPercentage =>
            MonthlyTarget == 0 ? 0 : (int)((TotalSalesThisMonth / MonthlyTarget) * 100);

        // --- 2. Charts Data (Real DB Data) ---
        public Dictionary<string, int> MonthlyGrowth { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> CustomersByIndustry { get; set; } = new Dictionary<string, int>();

        // --- 3. Lists (Real DB Data) ---
        public List<TaskItem> TodaysTasks { get; set; } = new List<TaskItem>();
        public List<LeadItem> HotLeads { get; set; } = new List<LeadItem>();
        public List<ActivityItem> RecentActivities { get; set; } = new List<ActivityItem>();

        // --- 4. Pipeline Counts (Placeholders for now) ---
        public int DealsInProposal { get; set; }
        public int DealsInNegotiation { get; set; }
        public int DealsClosedWon { get; set; }
    }

    // --- Helper Classes ---
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Type { get; set; } // "Call", "Email", "Meeting"
    }

    public class LeadItem
    {
        public string Name { get; set; }
        public string Company { get; set; }
        public int Score { get; set; } // 0-100
    }

    public class ActivityItem
    {
        public string CustomerName { get; set; }
        public string Action { get; set; }
        public string TimeAgo { get; set; }
        public string Type { get; set; } // "Note", "Call", "Status"
    }
}