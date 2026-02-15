// The Parent (Common fields)
using CRM.DTOS.SalesManager;
using CRM.Models;

public class DashboardBaseDto
{
    public IEnumerable<CustomerDashboardDto> RecentlyAddedCustomers { get; set; }
}

public class ManagerDashboardDto : DashboardBaseDto
{
    public int TotalCustomers { get; set; }
    public int UnassignedCustomers { get; set; }
    public int ActiveTeamMembers { get; set; }
    public Dictionary<string, int> CustomerDistribution { get; set; }
}

public class ExecutiveDashboardDto : DashboardBaseDto
{
    public List<Activity> RecentTasks { get; set; }
}