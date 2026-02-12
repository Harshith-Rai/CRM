// CRM/DTOS/ForgotPasswordDto.cs
using System.ComponentModel.DataAnnotations;

namespace CRM.DTOS
{
    public class ForgotPasswordDto
    {
        [Required, EmailAddress]
        public string Email { get; set; }
    }
}
