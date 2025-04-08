namespace BookResearchApp.Core.Entities.DTOs
{
    public class ReviewDto
    {
        public int Id { get; set; }

        public double Rating { get; set; }
        public string? BookTitle { get; set; }
        public string ReviewText { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public int BookId { get; set; }

        public string UserId { get; set; }

        // İsteğe bağlı: İncelemeye ait yorumların listesi
        public IEnumerable<CommentDto>? Comments { get; set; }
    }
}
