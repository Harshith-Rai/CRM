namespace CRM.DTOS.SalesManager
{
    public class SalesManagerDashBoardDto
    {
        public int TotalCustomers { get; set; }
        public int UnassignedCustomers { get; set; }
        public int ActiveTeamMembers { get; set; }
        public IEnumerable<CustomerDashboardDto> RecentlyAddedCustomers { get; set; }

        public Dictionary<String,int> CustomerDistribution { get; set; }
    }
}