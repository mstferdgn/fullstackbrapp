using System.ComponentModel.DataAnnotations;

namespace BookResearchApp.Core.Entities
{
    public class Author : BaseEntity<int>
    {
        [Required]
        public string Name { get; set; }

        public ICollection<Book> Books { get; set; } = new List<Book>();

        public Author() { }
        public Author(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
    }
}
