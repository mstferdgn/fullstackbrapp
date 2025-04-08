using AutoMapper;
using BookResearchApp.Core.Configurations;
using BookResearchApp.Core.Entities;
using BookResearchApp.Core.Entities.DTOs;
using BookResearchApp.Core.Interfaces.Services;
using BookResearchApp.DataAccess.UnitOfWork.UnitOfWork;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BookResearchApp.Business.ImplementationOfServices
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly JwtSettings _jwtSettings;
        private readonly IEmailService _emailService;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper, JwtSettings jwtSettings, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _jwtSettings = jwtSettings;
            _emailService = emailService;
        }

        /// <summary>
        /// Yeni bir kullanıcı kaydı oluşturur, e-posta doğrulama token'ı üretir, doğrulama e-postası gönderir,
        /// ve başarıyla kayıt olan kullanıcıya JWT token döner.
        /// </summary>
        public async Task<UserDto> RegisterAsync(UserRegistrationDto registrationDto)
        {
            // Kullanıcı adı kontrolü, e-posta kontrolü vs. yapılabilir
            var existingUser = await _unitOfWork.Users.GetByUserNameAsync(registrationDto.UserName);
            if (existingUser != null)
                throw new Exception("Kullanıcı adı zaten mevcut.");

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(registrationDto.Password);

            var user = new User(
                registrationDto.FirstName,
                registrationDto.LastName,
                registrationDto.EmailAddress,
                registrationDto.UserName,
                hashedPassword
            );

            if (string.IsNullOrEmpty(user.Id))
            {
                user.Id = Guid.NewGuid().ToString();
            }

            user.EmailVerificationToken = Guid.NewGuid().ToString();
            user.EmailVerificationTokenExpires = DateTime.UtcNow.AddHours(24);


            _unitOfWork.BeginTransaction();
            try
            {
                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();
                _unitOfWork.Commit();
            }
            catch (Exception)
            {
                _unitOfWork.Rollback();
                throw;
            }

            // Email gönderimi: Doğrulama linkini oluştur
            string verificationLink = $"http://localhost:5217/api/v1/account/verify-email?userId={user.Id}&token={user.EmailVerificationToken}";
            string subject = "E-Posta Doğrulama";
            string body = $"Lütfen e-posta adresinizi doğrulamak için <a href='{verificationLink}'>buraya tıklayın</a>.";
            await _emailService.SendEmailAsync(user.EmailAddress, subject, body);

            // Token oluştur
            var token = GenerateJwtToken(user);

            var userDto = _mapper.Map<UserDto>(user);
            userDto.Token = token;
            return userDto;
        }

        /// <summary>
        /// Girilen kullanıcı adı ve şifre geçerli ise JWT token ile birlikte kullanıcı bilgilerini döner.
        /// </summary>
        public async Task<UserDto> LoginAsync(UserLoginDto loginDto)
        {
            var user = await _unitOfWork.Users.GetByUserNameAsync(loginDto.UserName);

           
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
                throw new Exception("Geçersiz kullanıcı adı veya şifre.");

            if (!user.IsEmailVerified)
                throw new Exception("E-posta doğrulanmamış.");

            var token = GenerateJwtToken(user);
            var userDto = _mapper.Map<UserDto>(user);
            userDto.Token = token;
            return userDto;
        }


        /// <summary>
        /// Belirtilen kullanıcı için JWT token oluşturur.
        /// </summary>
        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.EmailAddress),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Girilen doğrulama token'ı geçerli ise kullanıcının e-posta adresini doğrular.
        /// </summary>
        public async Task<bool> VerifyEmailAsync(EmailVerificationDto dto)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(dto.UserId);
            if (user == null)
                throw new Exception("Kullanıcı bulunamadı.");

            if (user.IsEmailVerified)
                return true;

            if (user.EmailVerificationToken != dto.VerificationToken || user.EmailVerificationTokenExpires < DateTime.UtcNow)
                throw new Exception("Geçersiz veya süresi dolmuş doğrulama token'ı.");

            _unitOfWork.BeginTransaction();
            try
            {
                user.IsEmailVerified = true;
                user.EmailVerificationToken = null;
                user.EmailVerificationTokenExpires = null;
                _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveChangesAsync();
                _unitOfWork.Commit();
                return true;
            }
            catch (Exception)
            {
                _unitOfWork.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Girilen e-posta adresine ait kullanıcının şifre sıfırlama token'ı oluşturur, günceller ve sıfırlama e-postası gönderir.
        /// </summary>
        public async Task ForgotPasswordAsync(ForgotPasswordDto forgotDto)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(forgotDto.EmailAddress);
            if (user == null)
                throw new Exception("Bu e-posta adresine kayıtlı kullanıcı bulunamadı.");

            user.PasswordResetToken = Guid.NewGuid().ToString();
            user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1);

            _unitOfWork.BeginTransaction();
            try
            {
                _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveChangesAsync();
                _unitOfWork.Commit();
            }
            catch (Exception)
            {
                _unitOfWork.Rollback();
                throw;
            }

            string resetLink = $"https://yourdomain.com/reset-password?userId={user.Id}&token={user.PasswordResetToken}";
            string subject = "Şifre Sıfırlama";
            string body = $"Şifrenizi sıfırlamak için <a href='{resetLink}'>buraya tıklayın</a>. Bu link 1 saat geçerlidir.";
            await _emailService.SendEmailAsync(user.EmailAddress, subject, body);
        }

        /// <summary>
        /// Girilen şifre sıfırlama token'ı geçerli ise kullanıcının şifresini sıfırlar.
        /// </summary>
        public async Task ResetPasswordAsync(ResetPasswordDto resetDto)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(resetDto.UserId);
            if (user == null)
                throw new Exception("Kullanıcı bulunamadı.");

            if (user.PasswordResetToken != resetDto.ResetToken || user.PasswordResetTokenExpires < DateTime.UtcNow)
                throw new Exception("Geçersiz veya süresi dolmuş şifre sıfırlama token'ı.");

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(resetDto.NewPassword);

            _unitOfWork.BeginTransaction();
            try
            {
                user.Password = hashedPassword;
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpires = null;
                _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveChangesAsync();
                _unitOfWork.Commit();
            }
            catch (Exception)
            {
                _unitOfWork.Rollback();
                throw;
            }
        }

    }

}
