using System.ComponentModel.DataAnnotations;

namespace CRM.Models
{
    public class Note
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please provide a title for this interaction")]
        public string Title { get; set; } // e.g., "Follow-up Call", "Initial Meeting"

        [Required(ErrorMessage = "Note content cannot be empty")]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Optional: Project Requirement #4 (Reminder System)
        public DateTime? ReminderDate { get; set; }

        // Foreign Key: Links this note to a specific Customer
        [Required]
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        // Security: Link to the ApplicationUser (Admin/Sales Rep) who wrote the note
        public string AuthorId { get; set; }
        public virtual ApplicationUser Author { get; set; }
    }
}