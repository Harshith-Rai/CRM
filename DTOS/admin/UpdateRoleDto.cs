using System.ComponentModel.DataAnnotations;

namespace CRM.DTOS.admin
{
    public class UpdateRoleDto
    {
        [Required]
        public string NewRole { get; set; } // e.g. "SalesManager"
    }
}
