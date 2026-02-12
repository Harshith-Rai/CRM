using CRM.DTOS.SalesManager;

namespace CRM.Services
{
    public interface ISalesManagerService
    {
        Task<SalesManagerDashBoardDto> GetDashboardDataAsync();
        Task<IEnumerable<CustomerDashboardDto>> GetRecentlyAddedCustomersAsync(int count = 6);
        Task<int> GetTotalCustomersAsync();
        Task<int> GetUnassignedCustomersCountAsync();
    }
}
