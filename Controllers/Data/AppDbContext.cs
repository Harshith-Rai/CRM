using CRM.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CRM.Models;

namespace CRM.Data
{
    // Inheriting from IdentityDbContext<ApplicationUser> integrates your custom user model
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Registering your CRM Entities as Database Tables
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Note> Notes { get; set; }
        //public DbSet<Lead> Leads { get; set; }

        public DbSet<Activity> Tasks { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configuring the One-to-Many relationship for Customers and Notes
            builder.Entity<Note>()
                .HasOne(n => n.Customer)
                .WithMany(c => c.Notes)
                .HasForeignKey(n => n.CustomerId)
                .OnDelete(DeleteBehavior.Cascade); // Deleting a customer deletes their notes

            // Configuring the One-to-Many relationship for Customers and Contacts
            builder.Entity<Contact>()
                .HasOne(c => c.Customer)
                .WithMany(cust => cust.Contacts)
                .HasForeignKey(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
