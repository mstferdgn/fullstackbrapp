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
    public class CommentServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly CommentService _commentService;
        private readonly Mock<ICommentRepository> _commentRepositoryMock;

        public CommentServiceTests()
        {
            // CommentRepository mock'unu oluştur
            _commentRepositoryMock = new Mock<ICommentRepository>();

            // IUnitOfWork mock'unu oluştur ve Comments property'sini ayarla.
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _unitOfWorkMock.Setup(u => u.Comments).Returns(_commentRepositoryMock.Object);

            _mapperMock = new Mock<IMapper>();

            // CommentService, IUnitOfWork ve IMapper bekliyor.
            _commentService = new CommentService(_unitOfWorkMock.Object, _mapperMock.Object);
        }

        #region GetCommentsByReviewIdAsync Tests
        [Fact]
        public async Task GetCommentsByReviewIdAsync_ShouldReturnMappedCommentDtos()
        {
            // Arrange
            int reviewId = 10;
            var comments = new List<Comment>
            {
                new Comment("Test Comment 1", "user1") { Id = 1, ReviewId = reviewId },
                new Comment("Test Comment 2", "user2") { Id = 2, ReviewId = reviewId }
            };
            var commentDtos = new List<CommentDto>
            {
                new CommentDto { Id = 1, CommentText = "Test Comment 1", ReviewId = reviewId },
                new CommentDto { Id = 2, CommentText = "Test Comment 2", ReviewId = reviewId }
            };

            _commentRepositoryMock.Setup(r => r.GetCommentsByReviewIdAsync(reviewId))
                                  .ReturnsAsync(comments);
            _mapperMock.Setup(m => m.Map<IEnumerable<CommentDto>>(comments))
                       .Returns(commentDtos);

            // Act
            var result = await _commentService.GetCommentsByReviewIdAsync(reviewId);

            // Assert
            result.Should().NotBeNull();
            result.Count().Should().Be(2);
            result.First().CommentText.Should().Be("Test Comment 1");
        }
        #endregion

        #region AddCommentAsync Tests
        [Fact]
        public async Task AddCommentAsync_ShouldAddCommentSuccessfully()
        {
            // Arrange
            var commentDto = new CommentDto { CommentText = "Yeni Yorum", ReviewId = 5, UserId = "" };
            var comment = new Comment("Yeni Yorum", "user123") { Id = 1, ReviewId = 5 };

            _mapperMock.Setup(m => m.Map<Comment>(commentDto)).Returns(comment);

            // Act
            await _commentService.AddCommentAsync(commentDto, "user123", "TestUser");

            // Assert
            // Testte CreatedAt ve CreatedBy set edildiğini doğrulamak için:
            comment.CreatedBy.Should().Be("TestUser");
            comment.CreatedAt.Should().NotBeNull();

            _commentRepositoryMock.Verify(r => r.AddAsync(It.Is<Comment>(c => c.UserId == "user123" && c.CreatedBy == "TestUser")), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
        #endregion

        #region UpdateCommentAsync Tests
        [Fact]
        public async Task UpdateCommentAsync_ShouldUpdateComment_WhenUserIsAuthorized()
        {
            // Arrange
            var commentDto = new CommentDto { Id = 1, CommentText = "Güncellenmiş Yorum", ReviewId = 5 };
            var comment = new Comment("Eski Yorum", "user123") { Id = 1, ReviewId = 5 };

            _commentRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(comment);

            // Act
            await _commentService.UpdateCommentAsync(commentDto, "user123", "TestUser");

            // Assert
            comment.CommentText.Should().Be("Güncellenmiş Yorum");
            comment.UpdatedBy.Should().Be("TestUser");
            comment.UpdatedAt.Should().NotBeNull();

            _commentRepositoryMock.Verify(r => r.Update(comment), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateCommentAsync_ShouldThrowUnauthorizedAccessException_WhenUserIsNotAuthorized()
        {
            // Arrange
            var commentDto = new CommentDto { Id = 1, CommentText = "Güncellenmiş Yorum", ReviewId = 5 };
            var comment = new Comment("Eski Yorum", "differentUser") { Id = 1, ReviewId = 5 };

            _commentRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(comment);

            // Act
            Func<Task> act = async () => await _commentService.UpdateCommentAsync(commentDto, "user123", "TestUser");

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                     .WithMessage("Bu yorumu güncelleme yetkiniz yok.");
        }
        #endregion

        #region DeleteCommentAsync Tests
        [Fact]
        public async Task DeleteCommentAsync_ShouldDeleteComment_WhenUserIsAuthorized()
        {
            // Arrange
            int commentId = 1;
            var comment = new Comment("Silinecek Yorum", "user123") { Id = commentId, ReviewId = 5 };

            _commentRepositoryMock.Setup(r => r.GetByIdAsync(commentId)).ReturnsAsync(comment);

            // Act
            await _commentService.DeleteCommentAsync(commentId, "user123");

            // Assert
            _commentRepositoryMock.Verify(r => r.Remove(comment), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteCommentAsync_ShouldThrowUnauthorizedAccessException_WhenUserIsNotAuthorized()
        {
            // Arrange
            int commentId = 1;
            var comment = new Comment("Silinecek Yorum", "differentUser") { Id = commentId, ReviewId = 5 };

            _commentRepositoryMock.Setup(r => r.GetByIdAsync(commentId)).ReturnsAsync(comment);

            // Act
            Func<Task> act = async () => await _commentService.DeleteCommentAsync(commentId, "user123");

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                     .WithMessage("Bu yorumu silme yetkiniz yok.");
        }
        #endregion

        #region Pagination Test for GetCommentsByReviewIdPagedAsync
        [Fact]
        public async Task GetCommentsByReviewIdPagedAsync_ShouldReturnPagedComments()
        {
            // Arrange
            int reviewId = 5;
            int pageNumber = 1, pageSize = 2;
            var pagedResult = new PaginationVm<Comment>
            {
                Items = new List<Comment>
                {
                    new Comment("Yorum 1", "user1") { Id = 1, ReviewId = reviewId },
                    new Comment("Yorum 2", "user2") { Id = 2, ReviewId = reviewId }
                },
                TotalCount = 5,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var mappedDtos = new List<CommentDto>
            {
                new CommentDto { Id = 1, CommentText = "Yorum 1", ReviewId = reviewId },
                new CommentDto { Id = 2, CommentText = "Yorum 2", ReviewId = reviewId }
            };

            _commentRepositoryMock.Setup(r => r.GetCommentsByReviewIdPagedAsync(reviewId, pageNumber, pageSize))
                                  .ReturnsAsync(pagedResult);
            _mapperMock.Setup(m => m.Map<IEnumerable<CommentDto>>(pagedResult.Items))
                       .Returns(mappedDtos);

            // Act
            var result = await _commentService.GetCommentsByReviewIdPagedAsync(reviewId, pageNumber, pageSize);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(5);
            result.PageNumber.Should().Be(pageNumber);
            result.PageSize.Should().Be(pageSize);
        }
        #endregion
    }
}
