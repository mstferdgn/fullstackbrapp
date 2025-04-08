using BookResearchApp.Core.Entities.DTOs;

namespace BookResearchApp.Core.Interfaces.Services
{
    public interface IUserService
    {
        Task<UserDto> RegisterAsync(UserRegistrationDto registrationDto);
        Task<UserDto> LoginAsync(UserLoginDto loginDto);
        Task<bool> VerifyEmailAsync(EmailVerificationDto verificationDto);
        Task ForgotPasswordAsync(ForgotPasswordDto forgotDto);
        Task ResetPasswordAsync(ResetPasswordDto resetDto);
    }
}
