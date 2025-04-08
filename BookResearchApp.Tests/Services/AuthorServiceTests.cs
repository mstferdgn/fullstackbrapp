using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using AutoMapper;
using BookResearchApp.Business.ImplementationOfServices;
using BookResearchApp.Core.Entities;
using BookResearchApp.Core.Entities.DTOs;
using BookResearchApp.Core.Interfaces.Repositories;
using BookResearchApp.DataAccess.UnitOfWork.UnitOfWork;

namespace BookResearchApp.Tests.Services
{
    public class AuthorServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly AuthorService _authorService;
        private readonly Mock<IAuthorRepository> _authorRepositoryMock;

        public AuthorServiceTests()
        {
            // AuthorRepository mock'unu oluştur
            _authorRepositoryMock = new Mock<IAuthorRepository>();

            // IUnitOfWork mock'unu oluştur ve Authors property'sini ayarla.
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _unitOfWorkMock.Setup(u => u.Authors).Returns(_authorRepositoryMock.Object);

            _mapperMock = new Mock<IMapper>();

            // AuthorService, IUnitOfWork ve IMapper bekliyor.
            _authorService = new AuthorService(_unitOfWorkMock.Object, _mapperMock.Object);
        }

        #region GetAllAuthorsAsync Tests
        [Fact]
        public async Task GetAllAuthorsAsync_ShouldReturnAuthorDtos()
        {
            // Arrange
            var authors = new List<Author>
            {
                new Author("Orhan Pamuk") { Id = 1 },
                new Author("Elif Şafak") { Id = 2 }
            };

            var authorDtos = new List<AuthorDto>
            {
                new AuthorDto { Id = 1, Name = "ORHAN PAMUK" },
                new AuthorDto { Id = 2, Name = "ELIF ŞAFAK" }
            };

            _authorRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(authors);
            _mapperMock.Setup(m => m.Map<IEnumerable<AuthorDto>>(authors)).Returns(authorDtos);

            // Act
            var result = await _authorService.GetAllAuthorsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Count().Should().Be(2);
            result.First().Name.Should().Be("ORHAN PAMUK");

            _authorRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
            _mapperMock.Verify(m => m.Map<IEnumerable<AuthorDto>>(authors), Times.Once);
        }
        #endregion

        #region GetAuthorByIdAsync Tests
        [Fact]
        public async Task GetAuthorByIdAsync_ShouldReturnAuthorDto_WhenAuthorExists()
        {
            // Arrange
            int authorId = 1;
            var author = new Author("Orhan Pamuk") { Id = authorId };
            var authorDto = new AuthorDto { Id = authorId, Name = "ORHAN PAMUK" };

            _authorRepositoryMock.Setup(r => r.GetAuthorWithBooksAsync(authorId)).ReturnsAsync(author);
            _mapperMock.Setup(m => m.Map<AuthorDto>(author)).Returns(authorDto);

            // Act
            var result = await _authorService.GetAuthorByIdAsync(authorId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(authorId);
            result.Name.Should().Be("ORHAN PAMUK");

            _authorRepositoryMock.Verify(r => r.GetAuthorWithBooksAsync(authorId), Times.Once);
            _mapperMock.Verify(m => m.Map<AuthorDto>(author), Times.Once);
        }
        #endregion

        #region GetBooksByAuthorAsync Tests
        [Fact]
        public async Task GetBooksByAuthorAsync_ShouldReturnBookDtos_WhenAuthorExists()
        {
            // Arrange
            int authorId = 1;
            var books = new List<Book>
            {
                new Book("Kitap 1", "orhanpamuk") { Id = 1, Title = "KITAP 1" },
                new Book("Kitap 2", "orhanpamuk") { Id = 2, Title = "KITAP 2" }
            };

            var author = new Author("Orhan Pamuk")
            {
                Id = authorId,
                Books = books
            };

            var bookDtos = new List<BookDto>
            {
                new BookDto { Id = 1, Title = "KITAP 1" },
                new BookDto { Id = 2, Title = "KITAP 2" }
            };

            _authorRepositoryMock.Setup(r => r.GetAuthorWithBooksAsync(authorId)).ReturnsAsync(author);
            _mapperMock.Setup(m => m.Map<IEnumerable<BookDto>>(author.Books)).Returns(bookDtos);

            // Act
            var result = await _authorService.GetBooksByAuthorAsync(authorId);

            // Assert
            result.Should().NotBeNull();
            result.Count().Should().Be(2);
            result.First().Title.Should().Be("KITAP 1");
        }
        #endregion

        #region AddAuthorAsync Tests
        [Fact]
        public async Task AddAuthorAsync_ShouldAddAuthor_WhenNameIsUnique()
        {
            // Arrange
            var authorDto = new AuthorDto { Name = "Orhan Pamuk" };
            // Setup: GetByNameAsync döndürsün null (yani yazar yok)
            _authorRepositoryMock.Setup(r => r.GetByNameAsync("ORHAN PAMUK")).ReturnsAsync((Author)null);

            // Mapping: AuthorDto'dan Author nesnesi üretiliyor.
            var author = new Author("Orhan Pamuk") { Id = 1 };
            _mapperMock.Setup(m => m.Map<Author>(authorDto)).Returns(author);

            // Act
            Func<Task> act = async () => await _authorService.AddAuthorAsync(authorDto);

            // Assert
            await act.Should().NotThrowAsync();
            _authorRepositoryMock.Verify(r => r.AddAsync(author), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AddAuthorAsync_ShouldThrowException_WhenAuthorAlreadyExists()
        {
            // Arrange
            var authorDto = new AuthorDto { Name = "Orhan Pamuk" };
            var existingAuthor = new Author("Orhan Pamuk") { Id = 1 };
            _authorRepositoryMock.Setup(r => r.GetByNameAsync("ORHAN PAMUK")).ReturnsAsync(existingAuthor);

            // Act
            Func<Task> act = async () => await _authorService.AddAuthorAsync(authorDto);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Bu yazar zaten kayıtlı.");
        }
        #endregion

        #region SearchAuthorsAsync Tests
        [Fact]
        public async Task SearchAuthorsAsync_ShouldReturnMappedAuthors()
        {
            // Arrange
            string search = "Orhan";
            var authors = new List<Author>
            {
                new Author("Orhan Pamuk") { Id = 1 }
            };
            var authorDtos = new List<AuthorDto>
            {
                new AuthorDto { Id = 1, Name = "ORHAN PAMUK" }
            };

            _authorRepositoryMock.Setup(r => r.SearchByNameAsync(search)).ReturnsAsync(authors);
            _mapperMock.Setup(m => m.Map<IEnumerable<AuthorDto>>(authors)).Returns(authorDtos);

            // Act
            var result = await _authorService.SearchAuthorsAsync(search);

            // Assert
            result.Should().NotBeNull();
            result.Count().Should().Be(1);
            result.First().Name.Should().Be("ORHAN PAMUK");
        }
        #endregion

        #region GetAuthorReviewCountsAsync Tests
        [Fact]
        public async Task GetAuthorReviewCountsAsync_ShouldReturnReviewCounts()
        {
            // Arrange
            var book1 = new Book("Kitap 1", "orhanpamuk")
            {
                Id = 1,
                Reviews = new List<Review> { new Review(1, "user1", "Review 1") { Rating = 4 } }
            };
            var book2 = new Book("Kitap 2", "orhanpamuk")
            {
                Id = 2,
                Reviews = new List<Review>
                {
                    new Review(2, "user1", "Review 2") { Rating = 5 },
                    new Review(2, "user2", "Review 3") { Rating = 3 }
                }
            };
            var author = new Author("Orhan Pamuk")
            {
                Id = 1,
                Books = new List<Book> { book1, book2 },
                Name = "ORHAN PAMUK"
            };

            var authorsList = new List<Author> { author };

            _authorRepositoryMock.Setup(r => r.GetAuthorsWithReviewsAsync()).ReturnsAsync(authorsList);

            // Act
            var result = await _authorService.GetAuthorReviewCountsAsync();

            // Assert
            result.Should().NotBeNull();
            result.First().Name.Should().Be("ORHAN PAMUK");
            // Toplam inceleme sayısı: 1 (book1) + 2 (book2) = 3
            result.First().Value.Should().Be(3);
        }
        #endregion
    }
}
