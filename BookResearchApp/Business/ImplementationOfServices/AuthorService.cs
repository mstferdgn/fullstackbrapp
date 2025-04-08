using AutoMapper;
using BookResearchApp.Core.Entities;
using BookResearchApp.Core.Entities.Constants;
using BookResearchApp.Core.Entities.DTOs;
using BookResearchApp.Core.Interfaces.Services;
using BookResearchApp.DataAccess.UnitOfWork.UnitOfWork;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace BookResearchApp.Business.ImplementationOfServices
{
    public class AuthorService : IAuthorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AuthorService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Mevcut tüm yazarları getirir.
        /// </summary>
        public async Task<IEnumerable<AuthorDto>> GetAllAuthorsAsync()
        {
            var authors = await _unitOfWork.Authors.GetAllAsync();

            return _mapper.Map<IEnumerable<AuthorDto>>(authors);
        }

        /// <summary>
        /// Belirtilen ID'ye göre yazar döndürür.
        /// </summary>
        public async Task<AuthorDto> GetAuthorByIdAsync(int authorId)
        {
            var author = await _unitOfWork.Authors.GetAuthorWithBooksAsync(authorId);

            return _mapper.Map<AuthorDto>(author);

        }


        /// <summary>
        /// Belirtilen yazarın kitaplarını döndürür.
        /// </summary>
        public async Task<IEnumerable<BookDto>> GetBooksByAuthorAsync(int authorId)
        {
            var author = await _unitOfWork.Authors.GetAuthorWithBooksAsync(authorId);
            if (author == null)
            {
                return new List<BookDto>();
            }


            return _mapper.Map<IEnumerable<BookDto>>(author.Books);

        }

        /// <summary>
        /// Yeni yazar ekler. İsim büyük harfe çevrilir ve eğer aynı isimde yazar varsa hata fırlatılır.
        /// </summary>
        public async Task AddAuthorAsync(AuthorDto authorDto)
        {
            // Kullanıcı inputundan gelen adı al (manuel giriş için de kullanılır)
            string inputName = authorDto.Name;
            string finalAuthorName = inputName.ToUpperInvariant();

            // Eğer aynı isimde yazar varsa, hata fırlat
            var existingAuthor = await _unitOfWork.Authors.GetByNameAsync(finalAuthorName);
            if (existingAuthor != null)
                throw new Exception("Bu yazar zaten kayıtlı.");

            // Eğer kullanıcı manuel giriş yapmışsa, CustomName dolu olacaktır.
            // Boş değilse onu, aksi halde Name alanındaki değeri kullanırız.
            var author = _mapper.Map<Author>(authorDto);
            author.Name = finalAuthorName;
            await _unitOfWork.Authors.AddAsync(author);
            await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Girilen arama ifadesine göre yazarları döndürür.
        /// </summary>
        public async Task<IEnumerable<AuthorDto>> SearchAuthorsAsync(string search)
        {
            var authors = await _unitOfWork.Authors.SearchByNameAsync(search);
            return _mapper.Map<IEnumerable<AuthorDto>>(authors);
        }

        /// <summary>
        /// Yazarların kitaplarındaki incelemelerden toplam inceleme sayısını hesaplar.
        /// </summary>
        public async Task<IEnumerable<AuthorReviewCountDto>> GetAuthorReviewCountsAsync()
        {
            var authors = await _unitOfWork.Authors.GetAuthorsWithReviewsAsync();

            var result = authors.Select(author => new AuthorReviewCountDto
            {
                Name = author.Name,
                Value = author.Books.SelectMany(b => b.Reviews).Count()
            }).ToList();

            return result;
        }


    }

}

