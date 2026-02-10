using System.ComponentModel.DataAnnotations;

namespace CRM.DTOS.Admin
{
    public class UpdateRoleDto
    {
        [Required]
        public string NewRole { get; set; }
    }
}
