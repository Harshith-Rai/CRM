using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Models
{
    public class Note
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please provide a title")]
        public string Title { get; set; } // e.g., "Call Summary"

        [Required]
        public string Content { get; set; } // e.g., "Client is interested in..."

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReminderDate { get; set; } // Optional Reminder
        public bool IsReminderDone { get; set; } = false;
        // Foreign Keys
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

        public string AuthorId { get; set; } // The User who wrote the note
        [ForeignKey("AuthorId")]
        public virtual ApplicationUser Author { get; set; }
    }
}