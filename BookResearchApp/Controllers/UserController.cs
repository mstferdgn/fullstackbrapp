using BookResearchApp.Core.Entities.DTOs;
using BookResearchApp.Core.Interfaces.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookResearchApp.Controllers
{
    
    [Route("api/v1/account")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Kullanıcı kaydı oluşturur.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto registrationDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userDto = await _userService.RegisterAsync(registrationDto);
                return CreatedAtAction(nameof(GetCurrentUser), new { id = userDto.Id }, userDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Kullanıcı giriş işlemi yapar ve JWT token döner.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userDto = await _userService.LoginAsync(loginDto);
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        /// <summary>
        /// E-posta doğrulama işlemi (GET isteği ile) Post isteğinde sunucu kısmında güvenlik hatası aldığım için bu şekilde göndermek zorunda kaldım
        /// </summary>
        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string userId, [FromQuery] string token)
        {
            var verificationDto = new EmailVerificationDto
            {
                UserId = userId,
                VerificationToken = token
            };

            try
            {
                bool verified = await _userService.VerifyEmailAsync(verificationDto);
                if (verified)
                    return Ok("E-posta doğrulandı. Lütfen giriş yapın.");
                else
                    return BadRequest("E-posta doğrulaması başarısız.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Şifremi unuttum: Reset token oluşturur ve e-posta gönderir.
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotDto)
        {
            try
            {
                await _userService.ForgotPasswordAsync(forgotDto);
                return Ok("Şifre sıfırlama linki gönderildi.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Şifre yenileme: Reset token ve yeni şifre ile şifreyi günceller.
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetDto)
        {
            try
            {
                await _userService.ResetPasswordAsync(resetDto);
                return Ok("Şifre başarıyla güncellendi.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Giriş yapmış kullanıcı bilgisini döndürür.
        /// </summary>
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            // JWT ile doğrulama yapıldığında User.Identity üzerinden kullanıcı bilgileri alınır.
            return Ok(new { Message = "Giriş yapılmış kullanıcı bilgisi" });
        }


    }
}
