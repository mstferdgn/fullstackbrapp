using System.ComponentModel.DataAnnotations;
using System.Net;

namespace BookResearchApp.Core.Entities
{
    public class User : BaseEntity<string>
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string EmailAddress { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }


        public bool IsEmailVerified { get; set; } = false;
        public string? EmailVerificationToken { get; set; }
        public DateTime? EmailVerificationTokenExpires { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpires { get; set; }

        //public ICollection<Book> Books { get; set; } = new List<Book>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public User() { }
        public User(string firstName, string lastName, string emailAdress, string userName, string password)
        {
            FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName)); 
            LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
            EmailAddress = emailAdress ?? throw new ArgumentNullException(nameof(emailAdress));
            UserName = userName ?? throw new ArgumentNullException(nameof(userName));
            Password = password ?? throw new ArgumentNullException(nameof(password));
        }
    }
}
