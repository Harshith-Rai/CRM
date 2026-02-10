using System.ComponentModel.DataAnnotations;

namespace CRM.DTOS.Admin
{
    public class UpdateStatusDto
    {
        [Required]
        [MaxLength(50, ErrorMessage = "Position cannot exceed 50 characters.")]
        public string NewPosition { get; set; }
    }
}
