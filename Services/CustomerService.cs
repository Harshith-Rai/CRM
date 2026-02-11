using CRM.Data;
using CRM.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
namespace CRM.Services;

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _context;
    public CustomerService(AppDbContext context)
    {
        _context = context;
    }
    public async Task<List<Customer>> GetAllCustomersAsync(string userId, bool isAdmin) // Renamed
    {
        // REMOVED: .Where(c => c.IsActive)
        var query = _context.Customers.AsQueryable();

        if (!isAdmin)
        {
            query = query.Where(c => c.SalesRepId == userId);
        }

        // Order by newest first usually makes sense for a main list
        return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
    }
    public async Task CreateAsync(Customer customer, string userId)
        {
            customer.SalesRepId = userId;
            customer.IsActive = true;
            customer.CreatedAt = DateTime.UtcNow;
            _context.Add(customer);
            await _context.SaveChangesAsync();
        }

        public async Task<Customer> GetDetailsAsync(int id)
        {
            return await _context.Customers
                .Include(c => c.Contacts)
                .Include(c => c.Notes).ThenInclude(n => n.Author)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task AddNoteAsync(int customerId, string title, string content, DateTime? reminderDate, string userId)
        {
            if (reminderDate.HasValue)
                reminderDate = DateTime.SpecifyKind(reminderDate.Value, DateTimeKind.Utc);

            var note = new Note
            {
                CustomerId = customerId,
                Title = title ?? "Interaction",
                Content = content,
                CreatedAt = DateTime.UtcNow,
                ReminderDate = reminderDate,
                AuthorId = userId
            };

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(int id, Customer customer)
        {
            var existing = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (existing != null)
            {
                customer.CreatedAt = existing.CreatedAt;
                customer.SalesRepId = existing.SalesRepId;
                customer.IsActive = existing.IsActive;
                _context.Update(customer);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SoftDeleteAsync(int id, string userId, bool isAdmin)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null && (customer.SalesRepId == userId || isAdmin))
            {
                customer.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RestoreAsync(int id, string userId, bool isAdmin)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null && (customer.SalesRepId == userId || isAdmin))
            {
                customer.IsActive = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Customer>> GetAllArchivedAsync(string userId, bool isAdmin)
        {
            var query = _context.Customers.Where(c => !c.IsActive);
            if (!isAdmin) query = query.Where(c => c.SalesRepId == userId);
            return await query.ToListAsync();
        }

        public async Task<string> GenerateCsvAsync(string userId)
        {
            var customers = await _context.Customers
                .Where(c => c.IsActive && c.SalesRepId == userId)
                .ToListAsync();

            var builder = new StringBuilder();
            builder.AppendLine("Company Name,Industry,Email,Phone,Address,Created Date");
            foreach (var c in customers)
            {
                builder.AppendLine($"{c.CompanyName},{c.Industry},{c.Email},{c.Phone},{c.Address},{c.CreatedAt.ToShortDateString()}");
            }
            return builder.ToString();
        }
}
