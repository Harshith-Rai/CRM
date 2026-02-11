using CRM.Data;
using CRM.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace CRM.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly AppDbContext _context;

        public CustomerService(AppDbContext context)
        {
            _context = context;
        }

        // --- 1. GET ALL (Active Only) ---
        public async Task<List<Customer>> GetAllCustomersAsync(string userId, bool isAdmin)
        {
            // FIX: Restore the IsActive check so Archived customers don't appear here
            var query = _context.Customers.Where(c => c.IsActive);

            if (!isAdmin)
            {
                query = query.Where(c => c.SalesRepId == userId);
            }

            return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }

        // --- 2. CREATE ---
        public async Task CreateAsync(Customer customer, string userId)
        {
            customer.SalesRepId = userId;
            customer.IsActive = true;
            customer.CreatedAt = DateTime.UtcNow;
            _context.Add(customer);
            await _context.SaveChangesAsync();
        }

        // --- 3. GET DETAILS (With Contacts & Notes) ---
        public async Task<Customer> GetDetailsAsync(int id)
        {
            return await _context.Customers
                .Include(c => c.Contacts) // Load Contacts
                .Include(c => c.Notes)    // Load Notes
                    .ThenInclude(n => n.Author) // Load the User who wrote the note
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        // --- 4. ADD NOTE (New Feature) ---
        public async Task AddNoteAsync(int customerId, string title, string content, DateTime? reminderDate, string userId)
        {
            // Ensure proper UTC handling for PostgreSQL
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

        // --- 5. UPDATE ---
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

        // --- 6. SOFT DELETE ---
        public async Task SoftDeleteAsync(int id, string userId, bool isAdmin)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null && (customer.SalesRepId == userId || isAdmin))
            {
                customer.IsActive = false; // Soft delete
                await _context.SaveChangesAsync();
            }
        }

        // --- 7. RESTORE ---
        public async Task RestoreAsync(int id, string userId, bool isAdmin)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null && (customer.SalesRepId == userId || isAdmin))
            {
                customer.IsActive = true; // Set back to active
                await _context.SaveChangesAsync();
            }
        }

        // --- 8. GET ARCHIVED ---
        public async Task<List<Customer>> GetAllArchivedAsync(string userId, bool isAdmin)
        {
            var query = _context.Customers.Where(c => !c.IsActive); // Only inactive
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

        //Note-Management
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