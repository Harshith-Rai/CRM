using CRM.Models;

namespace CRM.Services
{
    public interface IHomeService
    {
        Task<DashboardViewModel> GetDashboardDataAsync(string userId);
    }
}