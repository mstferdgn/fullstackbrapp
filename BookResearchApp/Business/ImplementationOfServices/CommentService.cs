using AutoMapper;
using BookResearchApp.Core.Entities.DTOs;
using BookResearchApp.Core.Entities;
using BookResearchApp.Core.Interfaces.Services;
using BookResearchApp.DataAccess.UnitOfWork.UnitOfWork;
using System.Security.Claims;

namespace BookResearchApp.Business.ImplementationOfServices
{
    public class CommentService :ICommentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;



        public CommentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
           

        }

        /// <summary>
        /// Sayfalama destekli olarak belirli bir incelemeye ait yorumları döndürür.
        /// </summary
        public async Task<PaginationVm<CommentDto>> GetCommentsByReviewIdPagedAsync(int reviewId, int pageNumber, int pageSize)
        {
            var pagedComments = await _unitOfWork.Comments.GetCommentsByReviewIdPagedAsync(reviewId, pageNumber, pageSize);
            var mapped = new PaginationVm<CommentDto>
            {
                Items = _mapper.Map<IEnumerable<CommentDto>>(pagedComments.Items),
                TotalCount = pagedComments.TotalCount,
                PageNumber = pagedComments.PageNumber,
                PageSize = pagedComments.PageSize
            };
            return mapped;
        }

        /// <summary>
        /// Belirtilen incelemeye ait tüm yorumları döndürür.
        /// </summary>
        public async Task<IEnumerable<CommentDto>> GetCommentsByReviewIdAsync(int reviewId)
        {
            
            var comments = await _unitOfWork.Comments.GetCommentsByReviewIdAsync(reviewId);
            return _mapper.Map<IEnumerable<CommentDto>>(comments);
        }

        /// <summary>
        /// Yeni yorum ekler. Yorum, JWT’den alınan kullanıcı bilgileriyle (UserId, UserName) ve oluşturulma zamanı ile kaydedilir.
        /// </summary>
        public async Task AddCommentAsync(CommentDto commentDto, string currentUserId, string currentUserName)
        {
            var comment = _mapper.Map<Comment>(commentDto);


            comment.UserId = currentUserId;
            comment.CreatedBy = currentUserName;
            comment.CreatedAt = DateTime.UtcNow;


            await _unitOfWork.Comments.AddAsync(comment);
            await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Yorum günceller. Yalnızca yorumu oluşturan kullanıcı, güncelleme işlemi yapabilir.
        /// </summary>
        public async Task UpdateCommentAsync(CommentDto commentDto, string currentUserId, string currentUserName)
        {
            var comment = await _unitOfWork.Comments.GetByIdAsync(commentDto.Id);
            if (comment == null)
                throw new Exception("yorum bulunamadı");

            _mapper.Map(commentDto, comment);

            if (comment.UserId != currentUserId)
                throw new UnauthorizedAccessException("Bu yorumu güncelleme yetkiniz yok.");

            comment.CommentText = commentDto.CommentText;

            comment.UpdatedBy = currentUserName;
            comment.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Comments.Update(comment);
            await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Yorum siler. Yalnızca yorumu oluşturan kullanıcı, silme işlemi yapabilir.
        /// </summary>
        public async Task DeleteCommentAsync(int commentId, string currentUserId)
        {
            var comment = await _unitOfWork.Comments.GetByIdAsync(commentId);
            if (comment == null)
                throw new Exception("Yorum bulunamadı.");

            if (comment.UserId != currentUserId)
                throw new UnauthorizedAccessException("Bu yorumu silme yetkiniz yok.");

            _unitOfWork.Comments.Remove(comment);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
