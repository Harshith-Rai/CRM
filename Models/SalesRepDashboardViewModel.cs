using System.Collections.Generic;

namespace CRM.Models;

public class SalesRepDashboardViewModel
{
    // KPI Stats
    public decimal TotalSalesThisMonth { get; set; }
    public decimal MonthlyTarget { get; set; }

    // Helper to calculate progress % (prevents divide by zero)
    public int TargetProgressPercentage =>
        MonthlyTarget == 0 ? 0 : (int)((TotalSalesThisMonth / MonthlyTarget) * 100);

    // Lists
    public List<TaskItem> TodaysTasks { get; set; } = new List<TaskItem>();
    public List<LeadItem> HotLeads { get; set; } = new List<LeadItem>();

    // Pipeline Counts
    public int DealsInProposal { get; set; }
    public int DealsInNegotiation { get; set; }
    public int DealsClosedWon { get; set; }
}

public class TaskItem
{
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