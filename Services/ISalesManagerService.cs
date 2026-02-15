using CRM.DTOS.SalesManager;
using CRM.Models;

namespace CRM.Services
{
    public interface ISalesManagerService
    {
        Task<DashboardBaseDto> GetDashboardDataAsync(string userId=null,bool isManager=true);

        Task<List<Activity>> GetRecentTasks(String userId);
        Task<IEnumerable<CustomerDashboardDto>> GetRecentlyAddedCustomersAsync(String userId);
        Task<int> GetTotalCustomersAsync();
        Task<int> GetUnassignedCustomersCountAsync();
    }
}
