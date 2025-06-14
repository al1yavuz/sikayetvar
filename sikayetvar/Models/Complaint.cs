using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sikayetvar.Models
{
    public class Complaint
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(250)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? UserId { get; set; }

        [Required]
        public string Status { get; set; } = "Pending"; 

        public bool IsApproved { get; set; }
        public DateTime? ApprovalDate { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [MaxLength(300)]
        public string? ImagePath { get; set; } = null!;
        public bool IsResolved { get; set; } = false;
        public DateTime? ResolvedAt { get; set; }


        public ICollection<Comment>? Comments { get; set; }


    }
}

