namespace BookResearchApp.Core.Entities.DTOs
{
    public class BookDto
    {
        public int Id { get; set; }

        public string Title { get; set; }

        // Enum olarak BookType'ı string şeklinde de alabilirsiniz.
        public string Type { get; set; }

        public string? Description { get; set; }

        // Hesaplanmış toplam rating (1-5 arası oylar sonucu)
        public double Rating { get; set; }

        // Kullanıcı dosya yüklediyse; sunucu dosya yolu
        public string? ImageFile { get; set; }

        // Kullanıcı tarayıcı üzerinden URL seçerse
        public string? ImageUrl { get; set; }

        public int AuthorId { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string UserId { get; set; }

        // İsteğe bağlı: Kitaba ait incelemelerin listesi
        public IEnumerable<ReviewDto>? Reviews { get; set; }
    }
}
