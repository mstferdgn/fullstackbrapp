using AutoMapper;
using BookResearchApp.Business.ImplementationOfServices;
using BookResearchApp.Core.Entities.DTOs;
using BookResearchApp.Core.Entities;
using BookResearchApp.Core.Interfaces.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using BookResearchApp.DataAccess.UnitOfWork.UnitOfWork;
using BookResearchApp.Core.Entities.Constants;
using Microsoft.AspNetCore.Http;

namespace BookResearchApp.Tests.Services
{
    public class BookServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly BookService _bookService;
        private readonly Mock<IBookRepository> _bookRepositoryMock;

        public BookServiceTests()
        {
            // IBookRepository mock'unu oluştur
            _bookRepositoryMock = new Mock<IBookRepository>();

            // IUnitOfWork mock'unu oluştur ve Books property'sini ayarla.
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _unitOfWorkMock.Setup(u => u.Books).Returns(_bookRepositoryMock.Object);

            _mapperMock = new Mock<IMapper>();

            // BookService constructor'ı IUnitOfWork ve IMapper bekliyor.
            _bookService = new BookService(_unitOfWorkMock.Object, _mapperMock.Object);
        }

        #region GetBookByIdAsync
        [Fact]
        public async Task GetBookByIdAsync_ShouldReturnBookDto_WhenBookExists()
        {
            // Arrange
            int bookId = 1;
            var book = new Book { Id = bookId, Title = "Test Book" };
            var bookDto = new BookDto { Id = bookId, Title = "TEST BOOK" };

            _bookRepositoryMock.Setup(r => r.GetBookWithReviewsAsync(bookId)).ReturnsAsync(book);
            _mapperMock.Setup(m => m.Map<BookDto>(book)).Returns(bookDto);

            // Act
            var result = await _bookService.GetBookByIdAsync(bookId);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("TEST BOOK");
            _bookRepositoryMock.Verify(r => r.GetBookWithReviewsAsync(bookId), Times.Once);
            _mapperMock.Verify(m => m.Map<BookDto>(book), Times.Once);
        }
        #endregion

        //#region AddBookAsync
        //// Örnek: imageFile gönderilmediğinde
        //[Fact]
        //public async Task AddBookAsync_ShouldReturnCreatedBookDto_WhenNoImageFileProvided()
        //{
        //    // Arrange
        //    var bookDto = new BookCreateDto
        //    {
        //        Title = "Yeni Kitap",
        //        Type = "Fiction",
        //        Description = "Açıklama örneği",
        //        ImageUrl = "http://ornek.com/resim.jpg",
        //        AuthorId = 1,
        //        UserId = "" // Controller tarafından set edilecek
        //    };

        //    // Hazırlanacak Book entity (mapping sonrası)
        //    var book = new Book("Yeni Kitap", "dummyUserId")
        //    {
        //        Id = 1,
        //        Title = "YENİ KİTAP",
        //        ImageUrl = bookDto.ImageUrl,
        //        // CreatedBy ve CreatedAt servis içinde set edilecek
        //    };

        //    var bookDto = new BookDto
        //    {
        //        Id = 1,
        //        Title = "YENİ KİTAP",
        //        ImageUrl = bookDto.ImageUrl
        //    };

        //    _bookRepositoryMock.Setup(r => r.GetByTitleAsync("YENİ KİTAP")).ReturnsAsync((Book)null);
        //    _mapperMock.Setup(m => m.Map<Book>(bookDto)).Returns(book);
        //    _mapperMock.Setup(m => m.Map<BookDto>(book)).Returns(bookDto);

        //    // Act
        //    var result = await _bookService.AddBookAsync(bookDto, null, "testUser");

        //    // Assert
        //    result.Should().NotBeNull();
        //    result.Title.Should().Be("YENİ KİTAP");
        //    result.ImageUrl.Should().Be("http://ornek.com/resim.jpg");

        //    _bookRepositoryMock.Verify(r => r.AddAsync(book), Times.Once);
        //    _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        //}

        //// Örnek: imageFile gönderildiğinde
        //[Fact]
        //public async Task AddBookAsync_ShouldSetImageFile_WhenImageFileProvided()
        //{
        //    // Arrange
        //    var bookDto = new BookCreateDto
        //    {
        //        Title = "Resimli Kitap",
        //        Type = "Fiction",
        //        Description = "Resimli açıklama",
        //        ImageUrl = null,
        //        AuthorId = 1,
        //        UserId = ""
        //    };

        //    var book = new Book("Resimli Kitap", "dummyUserId")
        //    {
        //        Id = 2,
        //        Title = "RESİMLİ KİTAP",
        //    };

        //    var bookDto = new BookDto
        //    {
        //        Id = 2,
        //        Title = "RESİMLİ KİTAP",
        //        ImageFile = "/uploads/dummyfile.jpg"
        //    };

        //    // Mock IFormFile: using a MemoryStream to simulate a file.
        //    var content = "fake image content";
        //    var fileName = "test.jpg";
        //    var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        //    var formFile = new FormFile(stream, 0, stream.Length, "Data", fileName)
        //    {
        //        Headers = new HeaderDictionary(),
        //        ContentType = "image/jpeg"
        //    };

        //    _bookRepositoryMock.Setup(r => r.GetByTitleAsync("RESİMLİ KİTAP")).ReturnsAsync((Book)null);
        //    _mapperMock.Setup(m => m.Map<Book>(bookDto)).Returns(book);
        //    _mapperMock.Setup(m => m.Map<BookDto>(book)).Returns(bookDto);

        //    // Act
        //    var result = await _bookService.AddBookAsync(bookDto, formFile, "testUser");

        //    // Assert
        //    result.Should().NotBeNull();
        //    result.Title.Should().Be("RESİMLİ KİTAP");
        //    result.ImageFile.Should().NotBeNull();
        //    result.ImageFile.Should().Contain("/uploads/");

        //    _bookRepositoryMock.Verify(r => r.AddAsync(book), Times.Once);
        //    _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        //}
        //#endregion

        //#region UpdateBookAsync
        //[Fact]
        //public async Task UpdateBookAsync_ShouldReturnUpdatedBookDto_WhenUserIsAuthorized()
        //{
        //    // Arrange
        //    int bookId = 3;
        //    string currentUserId = "user123";
        //    var book = new Book("Eski Kitap", currentUserId)
        //    {
        //        Id = bookId,
        //        Title = "ESKİ KİTAP",
        //        Type = BookTypeEnum.Fiction,
        //        Description = "Eski açıklama",
        //        ImageUrl = "http://ornek.com/eski.jpg"
        //    };

        //    var updateDto = new BookUpdateDto
        //    {
        //        Id = bookId,
        //        Title = "Yeni Kitap Adı",
        //        Type = "Fiction",
        //        Description = "Yeni açıklama",
        //        ImageUrl = "http://ornek.com/yeni.jpg",
        //        AuthorId = 1
        //    };

        //    var updatedBook = new Book("Yeni Kitap Adı", currentUserId)
        //    {
        //        Id = bookId,
        //        Title = "YENİ KİTAP ADI",
        //        Type = BookTypeEnum.Fiction,
        //        Description = "Yeni açıklama",
        //        ImageUrl = "http://ornek.com/yeni.jpg",
        //        UpdatedBy = currentUserId,
        //        UpdatedAt = DateTime.UtcNow
        //    };

        //    var updatedBookDto = new BookDto
        //    {
        //        Id = bookId,
        //        Title = "YENİ KİTAP ADI",
        //        Description = "Yeni açıklama",
        //        ImageUrl = "http://ornek.com/yeni.jpg"
        //    };

        //    _bookRepositoryMock.Setup(r => r.GetByIdAsync(bookId)).ReturnsAsync(book);
        //    _mapperMock.Setup(m => m.Map<BookDto>(book)).Returns(updatedBookDto);

        //    // Act
        //    // Test için imageFile parametresini null gönderiyoruz.
        //    var result = await _bookService.UpdateBookAsync(bookId, updateDto, currentUserId, null);

        //    // Assert
        //    result.Should().NotBeNull();
        //    result.Title.Should().Be("YENİ KİTAP ADI");
        //    result.Description.Should().Be("Yeni açıklama");

        //    _bookRepositoryMock.Verify(r => r.Update(book), Times.Once);
        //    _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        //}

        //[Fact]
        //public async Task UpdateBookAsync_ShouldThrowUnauthorizedAccessException_WhenUserIsNotAuthorized()
        //{
        //    // Arrange
        //    int bookId = 4;
        //    string currentUserId = "user123";
        //    var book = new Book("Kitap", "differentUser")
        //    {
        //        Id = bookId,
        //        Title = "KİTAP"
        //    };

        //    var bookDto = new BookUpdateDto
        //    {
        //        Id = bookId,
        //        Title = "Yeni Kitap",
        //        Type = "Fiction",
        //        Description = "Yeni açıklama",
        //        ImageUrl = "http://ornek.com/yeni.jpg",
        //        AuthorId = 1
        //    };

        //    _bookRepositoryMock.Setup(r => r.GetByIdAsync(bookId)).ReturnsAsync(book);

        //    // Act
        //    Func<Task> act = async () => await _bookService.UpdateBookAsync(bookId, bookDto, currentUserId, null);

        //    // Assert
        //    await act.Should().ThrowAsync<UnauthorizedAccessException>()
        //        .WithMessage("Bu kitabı güncelleme yetkiniz yok.");
        //}
        //#endregion

        #region DeleteBookAsync
        [Fact]
        public async Task DeleteBookAsync_ShouldReturnTrue_WhenUserIsAuthorized()
        {
            // Arrange
            int bookId = 5;
            string currentUserId = "user123";
            var book = new Book("Silinecek Kitap", currentUserId) { Id = bookId };

            _bookRepositoryMock.Setup(r => r.GetByIdAsync(bookId)).ReturnsAsync(book);

            // Act
            var result = await _bookService.DeleteBookAsync(bookId, currentUserId);

            // Assert
            result.Should().BeTrue();
            _bookRepositoryMock.Verify(r => r.Remove(book), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteBookAsync_ShouldThrowUnauthorizedAccessException_WhenUserIsNotAuthorized()
        {
            // Arrange
            int bookId = 6;
            string currentUserId = "user123";
            var book = new Book("Kitap", "differentUser") { Id = bookId };

            _bookRepositoryMock.Setup(r => r.GetByIdAsync(bookId)).ReturnsAsync(book);

            // Act
            Func<Task> act = async () => await _bookService.DeleteBookAsync(bookId, currentUserId);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Bu kitabı silme yetkiniz yok.");
        }
        #endregion

        #region FilterBooksAsync
        [Fact]
        public async Task FilterBooksAsync_ShouldReturnEmptyList_WhenNoBooksMatch()
        {
            // Arrange
            string? title = "NonExisting";
            BookTypeEnum? type = null;
            string? sortRating = "desc";
            var emptyList = new List<Book>();

            _bookRepositoryMock.Setup(r => r.FilterBooksAsync(title, type, sortRating))
                .ReturnsAsync(emptyList);

            // Act
            var result = await _bookService.FilterBooksAsync(title, type, sortRating);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task FilterBooksAsync_ShouldReturnMappedBookDtos_WhenBooksMatch()
        {
            // Arrange
            string? title = "Test";
            BookTypeEnum? type = BookTypeEnum.Fiction;
            string? sortRating = "asc";
            var books = new List<Book>
            {
                new Book("Test Book", "user123") { Id = 7, Title = "TEST BOOK" }
            };
            var bookDtos = new List<BookDto>
            {
                new BookDto { Id = 7, Title = "TEST BOOK" }
            };

            _bookRepositoryMock.Setup(r => r.FilterBooksAsync(title, type, sortRating))
                .ReturnsAsync(books);
            _mapperMock.Setup(m => m.Map<IEnumerable<BookDto>>(books))
                .Returns(bookDtos);

            // Act
            var result = await _bookService.FilterBooksAsync(title, type, sortRating);

            // Assert
            result.Should().NotBeEmpty();
            result.First().Title.Should().Be("TEST BOOK");
        }
        #endregion

        #region UpdateBookRatingAsync
        [Fact]
        public async Task UpdateBookRatingAsync_ShouldUpdateRating_WhenReviewsExist()
        {
            // Arrange
            int bookId = 8;
            var reviews = new List<Review>
            {
                new Review( bookId, "user1", "Good" ){ Rating = 4 },
                new Review( bookId, "user2", "Very good" ){ Rating = 5 }
            };
            var book = new Book("Rating Test Book", "user123")
            {
                Id = bookId,
                Reviews = reviews
            };

            _bookRepositoryMock.Setup(r => r.GetBookWithReviewsAsync(bookId)).ReturnsAsync(book);

            // Act
            await _bookService.UpdateBookRatingAsync(bookId);

            // Assert: Ortalama rating = (4+5)/2 = 4.5
            book.Rating.Should().BeApproximately(4.5, 0.001);
            _bookRepositoryMock.Verify(r => r.Update(book), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
        #endregion
    }
}
