using System.ComponentModel.DataAnnotations;

namespace CRM.Models
{
    public class Contact //the Contact model represents the specific people you communicate with at a client organization
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        public string Position { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string Phone { get; set; }

        // Foreign Key: Links this contact to a specific Customer
        [Required]
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; }
    }
}