using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRM.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Company Name is required")]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }

        public string Industry { get; set; } // e.g., "Tech", "Real Estate"

        [EmailAddress]
        public string Email { get; set; } // Main company email

        public string Phone { get; set; } // Main HQ Phone
        public string Address { get; set; }

        public string SalesRepId { get; set; } // The User ID of the Sales Rep
        public bool IsActive { get; set; } = true; // Soft Delete flag
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // --- Relationships ---
        public virtual ICollection<Contact> Contacts { get; set; } = new List<Contact>();
        public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
        public DateTime? UpdatedAt { get; set; } 
    }
}