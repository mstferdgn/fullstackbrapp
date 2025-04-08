
using BookResearchApp.Core.Entities.Constants;
using System.Text.Json.Serialization;

namespace BookResearchApp.Core.Entities.DTOs
{
    public class AuthorDto
    {
        public int Id { get; set; }
        public string Name { get; set; }


        // Yazara ait kitapların listesi 
        public IEnumerable<BookDto>? Books { get; set; }

    }
}
