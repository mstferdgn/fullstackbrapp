namespace BookResearchApp.Core.Entities.DTOs
{
    public class BookCreateDto
    {
        public string Title { get; set; }

        public string Type { get; set; }

        public string? Description { get; set; }

        public string? ImageFile { get; set; }

        public string? ImageUrl { get; set; }

        public int AuthorId { get; set; }

        public string UserId { get; set; }
    }
}
