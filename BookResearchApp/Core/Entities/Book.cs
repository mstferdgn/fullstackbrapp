using BookResearchApp.Core.Entities.Constants;
using BookResearchApp.Core.Entities.Interface;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BookResearchApp.Core.Entities
{
    public class Book : BaseEntity<int>, IAuditableEntity
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public BookTypeEnum Type { get; set; }
        public string? Description { get; set; }

        [JsonIgnore]
        public double Rating { get; set; }

        public string? ImageFile { get; set; }
        public string? ImageUrl { get; set; }
        public int AuthorId { get; set; }
        public Author? Author { get; set; }

        [Required]
        public string UserId { get; set; }
        public User User { get; set; }

        public ICollection<Review> Reviews { get; set; } = new List<Review>();

        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Book() { }
        public Book(string title, string userId)
        {
            Title = title ?? throw new ArgumentNullException(nameof(title));
            
            UserId = userId ?? throw new ArgumentNullException(nameof(userId));

        }

    }
}
