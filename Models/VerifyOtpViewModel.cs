using System.ComponentModel.DataAnnotations;

namespace CRM.Models
{
    public class VerifyOtpViewModel
    {
        public string Email { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Code { get; set; }
    }
}
