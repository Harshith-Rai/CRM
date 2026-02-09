using System.ComponentModel.DataAnnotations;

namespace CRM.DTOS
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public String Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public String Password { get; set; }

        [Required]
        public String FullName { get; set; }
    }
}
