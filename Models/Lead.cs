//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace CRM.Models
//{
//    public enum LeadStatus
//    {
//        New,
//        Qualification,
//        Proposal,
//        Negotiation,
//        Won,
//        Lost
//    }

//    public class Lead
//    {
//        public int Id { get; set; }

//        [Required]
//        [Display(Name = "Deal Name")]
//        public string Title { get; set; } // e.g. "500 Unit License Deal"

//        [Column(TypeName = "decimal(18,2)")] // Stores money correctly
//        public decimal Value { get; set; } // Estimated Revenue

//        public LeadStatus Status { get; set; } = LeadStatus.New;

//        public string Source { get; set; } // e.g. "Website", "Referral", "Cold Call"

//        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//        public DateTime? ClosedAt { get; set; } // When did we win/lose?

//        // --- Relationships ---
//        public int CustomerId { get; set; }
//        public virtual Customer Customer { get; set; }

//        public string SalesRepId { get; set; } // Owner of the deal
//    }
//}