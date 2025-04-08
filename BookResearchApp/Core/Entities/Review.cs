using BookResearchApp.Core.Entities.Interface;
using System.ComponentModel.DataAnnotations;

namespace BookResearchApp.Core.Entities
{
    public class Review : BaseEntity<int>, IAuditableEntity
    {
        
        [Range(1, 5, ErrorMessage = "Rating değeri 1 ile 5 arasında olmalıdır.")]
        public double Rating { get; set; }
        [Required]
        public string ReviewText { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }


        public int BookId { get; set; }
        [Required]
        public Book Book { get; set; }

        [Required]
        public string UserId { get; set; }
        public User User { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public Review() { }

        public Review(int bookId, string userId, string reviewText)
        {
            BookId = bookId;
            UserId = userId ?? throw new ArgumentNullException(nameof(userId));
            ReviewText = reviewText ?? throw new ArgumentNullException(nameof(reviewText));
            CreatedAt = DateTime.UtcNow;
        }

    }
}
