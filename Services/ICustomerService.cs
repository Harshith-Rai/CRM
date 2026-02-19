using CRM.Models;
using CRM.DTOS.Customers;
using CRM.DTOS;
namespace CRM.Services
{
    public interface ICustomerService
    {
        Task<IEnumerable<ApplicationUser>> GetSalesExecutivesAsync();
        //Task<ApplicationUser> GetUserByIdAsync(string id);
        Task<PagedCustomerDto<Customer>> GetAllCustomersAsync(string userId, bool isAdmin, int pageNumber, int pageSize,String searchTem,String status,String industry);
        Task<List<Customer>> GetAllArchivedAsync(string userId, bool isAdmin);
        Task<Customer> GetDetailsAsync(int id);
        Task CreateAsync(Customer customer);
        //Task CreateAsync(Customer customer, string userId, string assignedToId);
        Task UpdateAsync(int id, Customer customer);
        Task InactivateCustomerAsync(int id);
        Task RestoreAsync(int id, string userId, bool isAdmin);
        Task<string> GenerateCsvAsync(string userId);

        Task AddNoteAsync(int customerId, string title, string content, DateTime? reminderDate, string userId);
        Task<Note> GetNoteAsync(int id);
        Task UpdateNoteAsync(Note note);
        Task DeleteNoteAsync(int id);
        Task<bool> ToggleCustomerStatusAsync(int id);
    }
}