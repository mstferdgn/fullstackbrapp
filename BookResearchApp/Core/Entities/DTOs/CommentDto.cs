namespace BookResearchApp.Core.Entities.DTOs
{
    public class CommentDto
    {
        public int Id { get; set; }

        public string CommentText { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? UpdatedBy   { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UserId { get; set; }
        public string? UserName { get; set; } 
        public int ReviewId { get; set; }
    }
}
