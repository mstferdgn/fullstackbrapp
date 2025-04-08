namespace BookResearchApp.Core.Entities.DTOs
{
    public class ReviewResponseDto
    {
        public string BookTitle { get; set; }
        public IEnumerable<ReviewDto> Reviews { get; set; }
    }
}
