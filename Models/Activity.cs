using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Models
{
    public class Activity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Subject { get; set; }

        public string Description { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime DueDate { get; set; }

        public bool Status { get; set; } = false; // False = Pending, True = Completed

        [Required]
        public string AssignTo { get; set; }

        [ForeignKey("AssignTo")]
        public virtual ApplicationUser User { get; set; }

    }
}
