// CRM/DTOS/ResetPasswordDto.cs
using System.ComponentModel.DataAnnotations;

namespace CRM.DTOS
{
    public class ResetPasswordDto
    {
        [Required]
        public string Token { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; }

        [Required, Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}