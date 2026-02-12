using CRM.Data;
using CRM.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text;
using CRM.DTOS.Customers;

namespace CRM.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public CustomerService(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IEnumerable<ApplicationUser>> GetSalesExecutivesAsync()
        {
            try
            {

                // Not in cache, fetch from database
                var salesExecutives = await _userManager.GetUsersInRoleAsync("SalesExecutive");
                var sortedExecs = salesExecutives.OrderBy(e => e.FullName).ToList();

                return sortedExecs;
            }
            catch (Exception ex)
            {
               
                return Enumerable.Empty<ApplicationUser>();
            }
        }
        // --- 1. GET ALL (Active Only) ---
        public async Task<List<Customer>> GetAllCustomersAsync(string userId, bool isAdmin)
        {
            var query = _context.Customers.Where(c => c.IsActive);

            if (!isAdmin)
            {
                query = query.Where(c => c.SalesRepId == userId);
            }

            return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }

        // --- 2. CREATE ---
        public async Task CreateAsync(Customer customer)
        {
            customer.IsActive = true;
            customer.CreatedAt = DateTime.UtcNow;
            // UpdatedAt is null on creation
            _context.Add(customer);
            await _context.SaveChangesAsync();
        }

        // --- 3. GET DETAILS ---
        public async Task<Customer> GetDetailsAsync(int id)
        {
            return await _context.Customers
                .Include(c => c.Contacts)
                .Include(c => c.Notes)
                    .ThenInclude(n => n.Author)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        // --- 4. ADD NOTE ---
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

            // Optional: Update the Customer's UpdatedAt when a note is added
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer != null)
            {
                customer.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        // --- 5. UPDATE (The Key Change) ---
        public async Task UpdateAsync(int id, Customer customer)
        {
            // 1. Fetch the existing record (Tracked)
            var existing = await _context.Customers.FindAsync(id);

            if (existing != null)
            {
                // 2. Map editable fields
                existing.CompanyName = customer.CompanyName;
                existing.Industry = customer.Industry;
                existing.Email = customer.Email;
                existing.Phone = customer.Phone;
                existing.Address = customer.Address;

                // 3. SET LAST EDITED DATE
                // Ensure your Customer model has: public DateTime? UpdatedAt { get; set; }
                existing.UpdatedAt = DateTime.UtcNow;

                // 4. Save (EF Core detects changes automatically)
                await _context.SaveChangesAsync();
            }
        }

        // --- 6. SOFT DELETE ---
        public async Task SoftDeleteAsync(int id, string userId, bool isAdmin)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null && (customer.SalesRepId == userId || isAdmin))
            {
                customer.IsActive = false;
                customer.UpdatedAt = DateTime.UtcNow; // Track when it was archived
                await _context.SaveChangesAsync();
            }
        }
       

        // --- 7. RESTORE ---
        public async Task RestoreAsync(int id, string userId, bool isAdmin)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null && (customer.SalesRepId == userId || isAdmin))
            {
                customer.IsActive = true;
                customer.UpdatedAt = DateTime.UtcNow; // Track when it was restored
                await _context.SaveChangesAsync();
            }
        }

        // --- 8. GET ARCHIVED ---
        public async Task<List<Customer>> GetAllArchivedAsync(string userId, bool isAdmin)
        {
            var query = _context.Customers.Where(c => !c.IsActive);
            if (!isAdmin) query = query.Where(c => c.SalesRepId == userId);
            return await query.ToListAsync();
        }

        // --- 9. CSV EXPORT ---
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

        // --- 10. NOTE MANAGEMENT ---
        public async Task<Note> GetNoteAsync(int id)
        {
            return await _context.Notes.FindAsync(id);
        }

        public async Task UpdateNoteAsync(Note note)
        {
            var existing = await _context.Notes.FindAsync(note.Id);
            if (existing != null)
            {
                existing.Title = note.Title;
                existing.Content = note.Content;
                existing.ReminderDate = note.ReminderDate;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteNoteAsync(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note != null)
            {
                _context.Notes.Remove(note);
                await _context.SaveChangesAsync();
            }
        }
    }
}