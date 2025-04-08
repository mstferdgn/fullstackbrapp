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
using BookResearchApp.Core.Interfaces.Services;
using BookResearchApp.DataAccess.UnitOfWork.UnitOfWork;

namespace BookResearchApp.Tests.Services
{
    public class ReviewServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IBookService> _bookServiceMock;
        private readonly ReviewService _reviewService;
        private readonly Mock<IReviewRepository> _reviewRepositoryMock;

        public ReviewServiceTests()
        {
            // ReviewRepository mock'unu oluştur
            _reviewRepositoryMock = new Mock<IReviewRepository>();

            // IUnitOfWork mock'unu oluştur ve Reviews property'sini ayarla.
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _unitOfWorkMock.Setup(u => u.Reviews).Returns(_reviewRepositoryMock.Object);

            _mapperMock = new Mock<IMapper>();

            // IBookService mock'unu oluştur (UpdateBookRatingAsync için)
            _bookServiceMock = new Mock<IBookService>();

            // ReviewService, IUnitOfWork, IMapper ve IBookService bekliyor.
            _reviewService = new ReviewService(_unitOfWorkMock.Object, _mapperMock.Object, _bookServiceMock.Object);
        }

        #region GetReviewsByBookIdAsync Tests
        [Fact]
        public async Task GetReviewsByBookIdAsync_ShouldReturnReviewDtos_WhenReviewsExist()
        {
            // Arrange
            int bookId = 1;
            var reviews = new List<Review>
            {
                new Review(bookId, "user1", "Review 1") { Id = 1, Rating = 4 },
                new Review(bookId, "user2", "Review 2") { Id = 2, Rating = 5 }
            };
            var reviewDtos = new List<ReviewDto>
            {
                new ReviewDto { Id = 1, ReviewText = "Review 1", Rating = 4, BookId = bookId },
                new ReviewDto { Id = 2, ReviewText = "Review 2", Rating = 5, BookId = bookId }
            };

            _reviewRepositoryMock.Setup(r => r.GetReviewsByBookIdAsync(bookId))
                .ReturnsAsync(reviews);
            _mapperMock.Setup(m => m.Map<IEnumerable<ReviewDto>>(reviews))
                .Returns(reviewDtos);

            // Act
            var result = await _reviewService.GetReviewsByBookIdAsync(bookId);

            // Assert
            result.Should().NotBeNull();
            result.Count().Should().Be(2);
            result.First().ReviewText.Should().Be("Review 1");
            _reviewRepositoryMock.Verify(r => r.GetReviewsByBookIdAsync(bookId), Times.Once);
            _mapperMock.Verify(m => m.Map<IEnumerable<ReviewDto>>(reviews), Times.Once);
        }
        #endregion

        #region AddReviewAsync Tests
        [Fact]
        public async Task AddReviewAsync_ShouldAddReviewAndUpdateRating()
        {
            // Arrange
            var reviewDto = new ReviewDto
            {
                BookId = 1,
                Rating = 4,
                ReviewText = "Good book"
            };
            var review = new Review(reviewDto.BookId, "", reviewDto.ReviewText)
            {
                Id = 1,
                Rating = reviewDto.Rating
            };

            // Mapping: ReviewDto => Review
            _mapperMock.Setup(m => m.Map<Review>(reviewDto)).Returns(review);

            // Act
            await _reviewService.AddReviewAsync(reviewDto, "user123", "TestUser");

            // Assert
            review.UserId.Should().Be("user123");
            review.CreatedBy.Should().Be("TestUser");
            review.CreatedAt.Should().NotBeNull();

            _reviewRepositoryMock.Verify(r => r.AddAsync(review), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            _bookServiceMock.Verify(bs => bs.UpdateBookRatingAsync(review.BookId), Times.Once);
        }
        #endregion

        #region UpdateReviewAsync Tests
        [Fact]
        public async Task UpdateReviewAsync_ShouldUpdateReview_WhenUserIsAuthorized()
        {
            // Arrange
            var reviewDto = new ReviewDto
            {
                Id = 1,
                Rating = 5,
                ReviewText = "Updated review",
                BookId = 2
            };

            // Existing review created by user "user123"
            var review = new Review(reviewDto.BookId, "user123", "Old review")
            {
                Id = 1,
                Rating = 4,
                CreatedBy = "TestUser",
                CreatedAt = DateTime.UtcNow
            };

            _reviewRepositoryMock.Setup(r => r.GetByIdAsync(reviewDto.Id)).ReturnsAsync(review);

            // Act
            await _reviewService.UpdateReviewAsync(reviewDto, "user123", "TestUserUpdated");

            // Assert
            review.ReviewText.Should().Be("Updated review");
            review.Rating.Should().Be(5);
            review.UpdatedBy.Should().Be("TestUserUpdated");
            review.UpdatedAt.Should().NotBeNull();

            _reviewRepositoryMock.Verify(r => r.Update(review), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            _bookServiceMock.Verify(bs => bs.UpdateBookRatingAsync(review.BookId), Times.Once);
        }

        [Fact]
        public async Task UpdateReviewAsync_ShouldThrowUnauthorizedAccessException_WhenUserIsNotAuthorized()
        {
            // Arrange
            var reviewDto = new ReviewDto { Id = 1, Rating = 5, ReviewText = "Updated review", BookId = 2 };
            var review = new Review(reviewDto.BookId, "differentUser", "Old review") { Id = 1 };

            _reviewRepositoryMock.Setup(r => r.GetByIdAsync(reviewDto.Id)).ReturnsAsync(review);

            // Act
            Func<Task> act = async () => await _reviewService.UpdateReviewAsync(reviewDto, "user123", "TestUser");

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Bu incelemeyi güncelleme yetkiniz yok.");
        }
        #endregion

        #region DeleteReviewAsync Tests
        [Fact]
        public async Task DeleteReviewAsync_ShouldDeleteReview_WhenUserIsAuthorized()
        {
            // Arrange
            int reviewId = 1;
            var review = new Review(2, "user123", "Review to delete") { Id = reviewId, BookId = 3 };

            _reviewRepositoryMock.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(review);

            // Act
            await _reviewService.DeleteReviewAsync(reviewId, "user123");

            // Assert
            _reviewRepositoryMock.Verify(r => r.Remove(review), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            _bookServiceMock.Verify(bs => bs.UpdateBookRatingAsync(review.BookId), Times.Once);
        }

        [Fact]
        public async Task DeleteReviewAsync_ShouldThrowUnauthorizedAccessException_WhenUserIsNotAuthorized()
        {
            // Arrange
            int reviewId = 1;
            var review = new Review(2, "differentUser", "Review to delete") { Id = reviewId, BookId = 3 };

            _reviewRepositoryMock.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(review);

            // Act
            Func<Task> act = async () => await _reviewService.DeleteReviewAsync(reviewId, "user123");

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Bu review'u silme yetkiniz yok.");
        }
        #endregion
    }
}
