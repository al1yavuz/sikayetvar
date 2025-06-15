using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sikayetvar.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Text { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [Required]
        public int ComplaintId { get; set; }

        [ForeignKey("ComplaintId")]
        public Complaint Complaint { get; set; }
        public ICollection<CommentLike> Likes { get; set; } = new List<CommentLike>();
        [NotMapped]
        public bool IsLikedByCurrentUser { get; set; }


    }
}
