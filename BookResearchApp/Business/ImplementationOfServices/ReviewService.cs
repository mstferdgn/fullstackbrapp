using AutoMapper;
using BookResearchApp.Core.Entities;
using BookResearchApp.Core.Entities.DTOs;
using BookResearchApp.Core.Interfaces.Services;
using BookResearchApp.DataAccess.UnitOfWork.UnitOfWork;
using System.Security.Claims;

namespace BookResearchApp.Business.ImplementationOfServices
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IBookService _bookService;


        public ReviewService(IUnitOfWork unitOfWork, IMapper mapper, IBookService bookService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _bookService = bookService;
 
        }

        /// <summary>
        /// Belirtilen kitaba ait tüm incelemeleri getirir.
        /// </summary>
        public async Task<IEnumerable<ReviewDto>> GetReviewsByBookIdAsync(int bookId)
        {
            var reviews = await _unitOfWork.Reviews.GetReviewsByBookIdAsync(bookId);
            if (reviews == null)
                throw new Exception("Kitaba ait inceleme bulunamadı.");

            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        /// <summary>
        /// Yeni bir inceleme ekler ve eklenmesinin ardından ilgili kitabın rating değerini günceller.
        /// </summary>
        public async Task AddReviewAsync(ReviewDto reviewDto, string currentUserId, string currentUserName)
        {
            var review = _mapper.Map<Review>(reviewDto);

            // Giriş yapan kullanıcının UserName'ini JWT token'dan alıyoruz.
            review.UserId = currentUserId;
            review.CreatedBy = currentUserName;
            review.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Reviews.AddAsync(review);
            await _unitOfWork.SaveChangesAsync();

            //review eklendikten sonra kitabın rating' ini güncelle
            await _bookService.UpdateBookRatingAsync(review.BookId);
        }

        /// <summary>
        /// Mevcut bir incelemeyi günceller. Kullanıcının inceleme üzerinde güncelleme yetkisi kontrol edilir.
        /// </summary>
        public async Task UpdateReviewAsync(ReviewDto reviewDto, string currentUserId, string currentUserName)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(reviewDto.Id);
            if (review == null)
                throw new Exception("Review bulunamadı.");

            // Giriş yapan kullanıcının ID'sini alıyoruz.
            if (review.UserId != currentUserId)
                throw new UnauthorizedAccessException("Bu incelemeyi güncelleme yetkiniz yok.");

            
            review.ReviewText = reviewDto.ReviewText;
            review.Rating = reviewDto.Rating;
            review.UpdatedBy = currentUserName;
            review.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Reviews.Update(review);
            await _unitOfWork.SaveChangesAsync();

            // Gerekirse kitabın rating'ini yeniden güncelleyin
            await _bookService.UpdateBookRatingAsync(review.BookId);
        }

        /// <summary>
        /// Belirtilen incelemeyi, güncel kullanıcının yetkisiyle sistemden siler ve ardından kitabın rating değerini günceller.
        /// </summary>
        public async Task DeleteReviewAsync(int reviewId, string currentUserId)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
            if (review == null)
                throw new Exception("Review bulunamadı.");

            if (review.UserId != currentUserId)
                throw new UnauthorizedAccessException("Bu review'u silme yetkiniz yok.");

            _unitOfWork.Reviews.Remove(review);
            await _unitOfWork.SaveChangesAsync();

            // Gerekirse kitabın rating'ini yeniden güncelleyin
            await _bookService.UpdateBookRatingAsync(review.BookId);
        }

    }
}
