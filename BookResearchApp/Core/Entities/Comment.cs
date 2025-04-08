using BookResearchApp.Core.Entities.Interface;
using System.ComponentModel.DataAnnotations;

namespace BookResearchApp.Core.Entities
{
    public class Comment : BaseEntity<int>, IAuditableEntity
    {
        [Required]
        public string CommentText { get; set; }

        [Required]
        public string UserId { get; set; }
        
        public User User { get; set; }

        public int ReviewId { get; set; }
        public Review? Review { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Comment() { }
        public Comment(string commentText, string userId)
        {

            CommentText = commentText ?? throw new ArgumentNullException(nameof(commentText));
            UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        }

    }
}
