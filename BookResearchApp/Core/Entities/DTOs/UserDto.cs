namespace BookResearchApp.Core.Entities.DTOs
{
    public class UserDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string UserName { get; set; }
        public bool IsEmailVerified { get; set; }
        public string Token { get; set; }
    }
}
