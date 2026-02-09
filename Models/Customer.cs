using System.ComponentModel.DataAnnotations;

namespace CRM.Models
{
    public class Customer //salesRep , Manager,Admin etc
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Customer name is required")]
        public string Name { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string Phone { get; set; }
        public string Address { get; set; }
        public string Company { get; set; }

        [Required]
        public string Status { get; set; } // e.g., "Active", "Inactive"

        // Interconnection: A Customer has many Contacts and many Notes
        public virtual ICollection<Contact> Contacts { get; set; } = new List<Contact>();
        public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
    }
}