using CRM.Models;

namespace CRM.Services
{
    public interface ICustomerService
    {
        Task<List<Customer>> GetAllActiveAsync(string userId, bool isAdmin);
        Task<List<Customer>> GetAllArchivedAsync(string userId, bool isAdmin);
        Task<Customer> GetDetailsAsync(int id);
        Task CreateAsync(Customer customer, string userId);
        Task UpdateAsync(int id, Customer customer);
        Task SoftDeleteAsync(int id, string userId, bool isAdmin);
        Task RestoreAsync(int id, string userId, bool isAdmin);
        Task<string> GenerateCscAsync(string userId);
        Task AddNoteAsync(int customerId, string title, string content, DateTime? reminderDate, string userId);
    }
}
