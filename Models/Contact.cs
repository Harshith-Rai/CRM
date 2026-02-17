using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Models
{
    public class Contact
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        public string Position { get; set; } // e.g. "Procurement Manager"

        [EmailAddress]
        public string Email { get; set; }
       
        
        [Required(ErrorMessage = "Phone Number is required")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^(\+\d{1,2}\s?)?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$",
                    ErrorMessage = "Invalid phone number. Use format like 123-456-7890 or (123) 456-7890")]
        public string Phone { get; set; }

        // Link back to Customer
        [Required]
        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }
    }
}