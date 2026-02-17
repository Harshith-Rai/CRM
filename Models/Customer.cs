using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Company Name is required")]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }

        public string Industry { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        // --- VALIDATION ADDED HERE ---
        [Required(ErrorMessage = "Phone Number is required")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^(\+\d{1,2}\s?)?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$",
            ErrorMessage = "Invalid phone number. Use format like 123-456-7890 or (123) 456-7890 or +91 9876543210")]
        public string Phone { get; set; }
        // -----------------------------

        public string Address { get; set; }

        public string? SalesRepId { get; set; }
        public DateTime? ArchivedAt { get; set; }

        [ForeignKey("SalesRepId")]
        public virtual ApplicationUser? SalesRep { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // --- Relationships ---
        public virtual ICollection<Contact> Contacts { get; set; } = new List<Contact>();
        public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
        public DateTime? UpdatedAt { get; set; }
        public bool IsHiddenFromBin { get; set; } = false;
    }
}