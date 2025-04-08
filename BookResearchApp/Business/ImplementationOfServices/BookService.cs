using AutoMapper;
using BookResearchApp.Core.Entities;
using BookResearchApp.Core.Entities.Constants;
using BookResearchApp.Core.Entities.DTOs;
using BookResearchApp.Core.Interfaces.Services;
using BookResearchApp.DataAccess.UnitOfWork.UnitOfWork;

namespace BookResearchApp.Business.ImplementationOfServices
{
    public class BookService : IBookService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;


        public BookService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;

        }

        /// <summary>
        /// Tüm kitapları getirir.
        /// </summary>
        public async Task<IEnumerable<BookDto>> GetAllBooksAsync()
        {
            var books = await _unitOfWork.Books.GetAllAsync();

            return _mapper.Map<IEnumerable<BookDto>>(books);
        }

        /// <summary>
        /// Belirtilen ID'ye sahip kitabı, incelemeleriyle birlikte getirir.
        /// </summary>
        public async Task<BookDto> GetBookByIdAsync(int id)
        {
            var book = await _unitOfWork.Books.GetBookWithReviewsAsync(id);

            return _mapper.Map<BookDto>(book);
        }

        /// <summary>
        /// Yeni bir kitap ekler. Eğer dosya yüklenmişse sunucuya kaydeder; aksi halde ImageUrl kullanır.
        /// Ek olarak oluşturma zamanı ve kullanıcısı bilgisi de eklenir.
        /// </summary>
        public async Task<BookDto> AddBookAsync(BookDto bookDto, IFormFile? imageFile, string currentUserName)
        {
            string finalTitle = bookDto.Title.ToUpperInvariant();

            // Aynı başlıkta kitap var mı kontrol edelim.
            var existingBook = await _unitOfWork.Books.GetByTitleAsync(finalTitle);
            if (existingBook != null)
                throw new Exception("Bu kitap zaten sistemde mevcut.");


            var book = _mapper.Map<Book>(bookDto);
            book.Title = finalTitle;

            book.CreatedBy = currentUserName;
            book.CreatedAt = DateTime.UtcNow;

            if (imageFile != null && imageFile.Length > 0)
            {
                //  Dosya uzantısını kontrol et (Sadece görseller yüklensin)
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(imageFile.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                    throw new InvalidOperationException("Geçersiz dosya formatı! Sadece JPG, JPEG, PNG ve GIF yükleyebilirsiniz.");

                //Dosya boyutu sınırı
                const long maxFileSize = 5 * 1024 * 1024; //5MB
                if (imageFile.Length > maxFileSize)
                    throw new InvalidOperationException("Dosya boyutu çok büyük!");

                //Yükleme klasörünü belirleme
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder); //Klasör yoksa oluştur

                //Benzersiz dosya adı oluşturma 
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                //Dosyayı asenkron olarak kaydetme 
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                book.ImageFile = $"/uploads/{uniqueFileName}";

            }
            else
            {
                //File yüklenmediyse , DTO' daki ImageUrl kullanılır.
                book.ImageUrl = bookDto.ImageUrl;
            }
            await _unitOfWork.Books.AddAsync(book);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<BookDto>(book);
        }

        /// <summary>
        /// İlgili kitaba ait tüm review'ların ortalamasını hesaplayarak günceller.
        /// </summary>
        public async Task UpdateBookRatingAsync(int bookId)
        {
            var book = await _unitOfWork.Books.GetBookWithReviewsAsync(bookId);
            if (book == null || book.Reviews == null || !book.Reviews.Any())
                return;
            double totalRating = book.Reviews.Sum(r => r.Rating);
            int reviewCount = book.Reviews.Count();
            book.Rating = totalRating / reviewCount;

            _unitOfWork.Books.Update(book);
            await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Kitapları, belirtilen filtreleme kriterlerine göre getirir.
        /// </summary>
        public async Task<IEnumerable<BookDto>> FilterBooksAsync(string? title, BookTypeEnum? type, string? sortRating)
        {
            var books = await _unitOfWork.Books.FilterBooksAsync(title, type, sortRating);
            if (books == null || books.Count() == 0)
            {
                return Enumerable.Empty<BookDto>();
            }
            return _mapper.Map<IEnumerable<BookDto>>(books);
        }


        /// <summary>
        /// Belirtilen kitabı günceller. Güncelleme sırasında dosya (yeni görsel) yüklenmişse sisteme kaydeder.
        /// Kullanıcının kitap üzerinde güncelleme yetkisi kontrol edilir.
        /// </summary>
        public async Task<BookDto> UpdateBookAsync(int id, BookDto updatedDto, string currentUserId, IFormFile? imageFile)
        {
            var book = await _unitOfWork.Books.GetByIdAsync(id);
            if (book == null)
                throw new Exception("Kitap bulunamadı.");

            if (book.UserId != currentUserId)
                throw new UnauthorizedAccessException("Bu kitabı güncelleme yetkiniz yok.");

            // Güncellenebilir alanlar: Title, Type, Description
            book.Title = updatedDto.Title.ToUpperInvariant();
            if (Enum.TryParse<BookTypeEnum>(updatedDto.Type, true, out var parsedType))
            {
                book.Type = parsedType;
            }
            else
            {
                throw new Exception("Geçersiz kitap türü.");
            }
            book.Description = updatedDto.Description;

            // Dosya güncelleme işlemi
            if (imageFile != null && imageFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                    throw new InvalidOperationException("Geçersiz dosya formatı! Sadece JPG, JPEG, PNG ve GIF yükleyebilirsiniz.");

                const long maxFileSize = 5 * 1024 * 1024; // 5MB
                if (imageFile.Length > maxFileSize)
                    throw new InvalidOperationException("Dosya boyutu çok büyük!");

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                // Yeni dosya yolunu ata
                book.ImageFile = $"/uploads/{uniqueFileName}";
            }
            else
            {
                // Eğer dosya güncellenmediyse, sadece imageUrl güncellenmişse bunu ata.
                book.ImageUrl = updatedDto.ImageUrl;
            }

            book.UpdatedAt = DateTime.UtcNow;
            book.UpdatedBy = currentUserId;

            _unitOfWork.Books.Update(book);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<BookDto>(book);
        }

        /// <summary>
        /// Belirtilen kitabı, güncel kullanıcının yetkisiyle birlikte sistemden siler.
        /// </summary>
        public async Task<bool> DeleteBookAsync(int id, string currentUserId)
        {
            var book = await _unitOfWork.Books.GetByIdAsync(id);
            if (book == null) throw new Exception("Kitap bulunamadı.");

            if (book.UserId != currentUserId)
                throw new UnauthorizedAccessException("Bu kitabı silme yetkiniz yok.");

            _unitOfWork.Books.Remove(book);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }


        public async Task<IEnumerable<BookDto>> GetBooksByUserIdAsync(string userId)
        {
            var books = await _unitOfWork.Books.GetBooksByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<BookDto>>(books);
        }

    }
}
