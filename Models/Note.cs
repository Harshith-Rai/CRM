using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Models
{
    public class Note
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please provide a title")]
        public string Title { get; set; } 

        [Required]
        public string Content { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        private DateTime? reminderDate;
        public DateTime? ReminderDate
        {
            get => reminderDate;
            set
            {
                if (value.HasValue && value.Value.Kind == DateTimeKind.Unspecified)
                {
                    value = DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
                }
                reminderDate = value;
            }
        } 
        public bool IsReminderDone { get; set; } = false;
       
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

        public string AuthorId { get; set; } 
        [ForeignKey("AuthorId")]
        public virtual ApplicationUser Author { get; set; }
    }
}