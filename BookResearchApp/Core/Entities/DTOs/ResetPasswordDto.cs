namespace BookResearchApp.Core.Entities.DTOs
{
    public class ResetPasswordDto
    {
        public string UserId { get; set; }
        public string ResetToken { get; set; }
        public string NewPassword { get; set; }
    }
}
