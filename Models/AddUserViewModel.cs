using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CRM.Models
{
    public class AddUserViewModel
    {
        [Required]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please select a role")]
        public string SelectedRole { get; set; }

    }
}
