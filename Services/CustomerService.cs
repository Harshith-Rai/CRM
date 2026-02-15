using CRM.Data;
using CRM.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text;
using CRM.DTOS;

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
                var salesExecutives = await _userManager.GetUsersInRoleAsync("SalesExecutive");
                return salesExecutives.OrderBy(e => e.FullName).ToList();
            }
            catch
            {
                return Enumerable.Empty<ApplicationUser>();
            }
        }

        // --- 1. GET ALL (Active Only) ---
        public async Task<PagedCustomerDto<Customer>> GetAllCustomersAsync(string userId,bool canSeeAll,int pageNumber,int pageSize,String searchTerm=null,string status=null,String industry=null)
        {
            var query = _context.Customers
                .Include(c => c.SalesRep)
                .Where(c => c.IsActive);

            if (!canSeeAll)
            {
                query = query.Where(c => c.SalesRepId == userId);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c => c.Email.Contains(searchTerm) ||
                c.CompanyName.Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(status)){
                bool isActive = status.Equals("active", StringComparison.OrdinalIgnoreCase);
                query = query.Where(q => q.IsActive == isActive);
            }

            if (!string.IsNullOrWhiteSpace(industry))
            {
                query = query.Where(c => c.Industry ==industry);
            }

            var totalcount = await query.CountAsync();

            var customers = await query.OrderByDescending(c => c.CreatedAt).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedCustomerDto<Customer>
            {
                Customers = customers,
                TotalCount = totalcount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                Status = status,
                Industry = industry
            };
        }

        // --- 2. CREATE ---
        public async Task CreateAsync(Customer customer)
        {
            customer.IsActive = true;
            customer.CreatedAt = DateTime.UtcNow; // Always UTC
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
                .Include(c => c.SalesRep)
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

            var customer = await _context.Customers.FindAsync(customerId);
            if (customer != null)
            {
                customer.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        // --- 5. UPDATE ---
        public async Task UpdateAsync(int id, Customer customer)
        {
            var existing = await _context.Customers.FindAsync(id);
            if (existing != null)
            {
                existing.CompanyName = customer.CompanyName;
                existing.Industry = customer.Industry;
                existing.Email = customer.Email;
                existing.Phone = customer.Phone;
                existing.Address = customer.Address;

                // Track Last Edit in UTC
                existing.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
        }

        // --- 6. SOFT DELETE (ARCHIVE) ---
        public async Task SoftDeleteAsync(int id, string userId, bool isAdmin)
        {
            var customer = await _context.Customers.FindAsync(id);

            // Permission Check
            if (customer != null && (customer.SalesRepId == userId || isAdmin))
            {
                customer.IsActive = false;

                // CORRECT: Using UtcNow prevents the "Double Conversion" error
                customer.ArchivedAt = DateTime.UtcNow;

                customer.UpdatedAt = DateTime.UtcNow;

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

                // Clear Archive Flags
                customer.ArchivedAt = null;
                customer.IsHiddenFromBin = false;

                customer.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        // --- 8. GET ARCHIVED ---
        public async Task<List<Customer>> GetAllArchivedAsync(string userId, bool isAdmin)
        {
            // Filter: Not Active AND Not Hidden
            var query = _context.Customers.Where(c => !c.IsActive && !c.IsHiddenFromBin);

            if (!isAdmin)
            {
                query = query.Where(c => c.SalesRepId == userId);
            }

            return await query.OrderByDescending(c => c.ArchivedAt).ToListAsync();
        }

        // --- 9. CSV EXPORT ---
        public async Task<string> GenerateCsvAsync(string userId)
        {
            var customers = await _context.Customers
                .Where(c => c.IsActive && c.SalesRepId == userId)
                .ToListAsync();

            var builder = new StringBuilder();
            builder.AppendLine("Company Name,Industry,Email,Phone,Address,Created Date (IST)");

            // Robust Timezone Logic (Same as View)
            TimeZoneInfo istZone;
            try
            {
                istZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
            }
            catch
            {
                try
                {
                    istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                }
                catch
                {
                    istZone = TimeZoneInfo.Utc; // Fallback
                }
            }

            foreach (var c in customers)
            {
                DateTime istDate;
                if (istZone == TimeZoneInfo.Utc)
                {
                    // Manual fallback if OS timezone DB is missing
                    istDate = c.CreatedAt.AddHours(5).AddMinutes(30);
                }
                else
                {
                    istDate = TimeZoneInfo.ConvertTimeFromUtc(c.CreatedAt, istZone);
                }

                builder.AppendLine($"{c.CompanyName},{c.Industry},{c.Email},{c.Phone},{c.Address},{istDate}");
            }
            return builder.ToString();
        }

        // ... Notes Methods ...
        public async Task<Note> GetNoteAsync(int id) => await _context.Notes.FindAsync(id);
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