namespace BookResearchApp.Core.Entities.DTOs
{
    public class EmailVerificationDto
    {
        public string UserId { get; set; }
        public string VerificationToken { get; set; }
    }
}
