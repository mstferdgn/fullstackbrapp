using System.ComponentModel.DataAnnotations;

namespace BookResearchApp.Core.Entities.DTOs
{
    public class ForgotPasswordDto
    {
        [EmailAddress]
        public string EmailAddress { get; set; }
    }
}
