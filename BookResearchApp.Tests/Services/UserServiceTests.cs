using AutoMapper;
using BookResearchApp.Business.ImplementationOfServices;
using BookResearchApp.Core.Configurations;
using BookResearchApp.Core.Entities;
using BookResearchApp.Core.Entities.DTOs;
using BookResearchApp.Core.Interfaces.Services;
using BookResearchApp.DataAccess.UnitOfWork.UnitOfWork;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace BookResearchApp.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly JwtSettings _jwtSettings;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _emailServiceMock = new Mock<IEmailService>();
            _jwtSettings = new JwtSettings
            {
                Secret = "VerySecretKeyForJWTSigning_1234567890ABCDEF", // development için uzunluk yeterli
                Issuer = "BookResearchApp",
                Audience = "BookResearchAppUsers",
                ExpiryMinutes = 60
            };

            _userService = new UserService(
                _unitOfWorkMock.Object,
                _mapperMock.Object,
                _jwtSettings,
                _emailServiceMock.Object
            );
        }

        [Fact]
        public async Task RegisterAsync_ShouldRegisterUser_WhenDataIsValid()
        {
            // Arrange
            var registrationDto = new UserRegistrationDto
            {
                FirstName = "Test",
                LastName = "User",
                EmailAddress = "test@example.com",
                UserName = "testuser",
                Password = "password123"
            };

            _unitOfWorkMock.Setup(u => u.Users.GetByUserNameAsync(registrationDto.UserName))
                .ReturnsAsync((User)null);

            _unitOfWorkMock.Setup(u => u.Users.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var userEntity = new User(
                registrationDto.FirstName,
                registrationDto.LastName,
                registrationDto.EmailAddress,
                registrationDto.UserName,
                "hashedpassword"
            );

            var userDto = new UserDto { Id = userEntity.Id, UserName = userEntity.UserName, EmailAddress = userEntity.EmailAddress };
            _mapperMock.Setup(m => m.Map<UserDto>(It.IsAny<User>())).Returns(userDto);

            // Act
            var result = await _userService.RegisterAsync(registrationDto);

            // Assert
            result.Should().NotBeNull();
            result.UserName.Should().Be(registrationDto.UserName);
            _emailServiceMock.Verify(e => e.SendEmailAsync(
                registrationDto.EmailAddress,
                It.IsAny<string>(),
                It.Is<string>(s => s.Contains("http://localhost:5217"))
            ), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowException_WhenUsernameExists()
        {
            var dto = new UserRegistrationDto { UserName = "existing" };
            _unitOfWorkMock.Setup(u => u.Users.GetByUserNameAsync("existing")).ReturnsAsync(new User());

            var act = async () => await _userService.RegisterAsync(dto);
            await act.Should().ThrowAsync<Exception>().WithMessage("Kullanıcı adı zaten mevcut.");
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
        {
            var dto = new UserLoginDto { UserName = "validuser", Password = "password123" };
            var user = new User("a", "b", "c@x.com", "validuser", BCrypt.Net.BCrypt.HashPassword("password123"))
            {
                Id = Guid.NewGuid().ToString(),
                IsEmailVerified = true
            };

            _unitOfWorkMock.Setup(u => u.Users.GetByUserNameAsync(dto.UserName)).ReturnsAsync(user);
            _mapperMock.Setup(m => m.Map<UserDto>(It.IsAny<User>())).Returns(new UserDto { UserName = user.UserName });

            var result = await _userService.LoginAsync(dto);

            result.Should().NotBeNull();
            result.UserName.Should().Be(user.UserName);
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowException_WhenEmailNotVerified()
        {
            var dto = new UserLoginDto { UserName = "test", Password = "pass" };
            var user = new User("f", "l", "email", "test", BCrypt.Net.BCrypt.HashPassword("pass"))
            {
                IsEmailVerified = false
            };

            _unitOfWorkMock.Setup(u => u.Users.GetByUserNameAsync(dto.UserName)).ReturnsAsync(user);

            var act = async () => await _userService.LoginAsync(dto);
            await act.Should().ThrowAsync<Exception>().WithMessage("E-posta doğrulanmamış.");
        }

        [Fact]
        public async Task VerifyEmailAsync_ShouldVerify_WhenTokenValid()
        {
            var userId = Guid.NewGuid().ToString();
            var user = new User("a", "b", "c", "d", "e")
            {
                Id = userId,
                EmailVerificationToken = "valid-token",
                EmailVerificationTokenExpires = DateTime.UtcNow.AddHours(1),
                IsEmailVerified = false
            };

            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(userId)).ReturnsAsync(user);

            var result = await _userService.VerifyEmailAsync(new EmailVerificationDto
            {
                UserId = userId,
                VerificationToken = "valid-token"
            });

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ForgotPasswordAsync_ShouldGenerateResetToken_WhenEmailExists()
        {
            var email = "test@domain.com";
            var user = new User("f", "l", email, "user", "pass");

            _unitOfWorkMock.Setup(u => u.Users.GetByEmailAsync(email)).ReturnsAsync(user);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            await _userService.ForgotPasswordAsync(new ForgotPasswordDto { EmailAddress = email });

            _emailServiceMock.Verify(x => x.SendEmailAsync(email, It.IsAny<string>(), It.Is<string>(s => s.Contains("reset-password"))), Times.Once);
        }

        [Fact]
        public async Task ResetPasswordAsync_ShouldReset_WhenTokenValid()
        {
            var userId = Guid.NewGuid().ToString();
            var user = new User("f", "l", "email", "user", "pass")
            {
                Id = userId,
                PasswordResetToken = "reset-token",
                PasswordResetTokenExpires = DateTime.UtcNow.AddMinutes(30)
            };

            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(userId)).ReturnsAsync(user);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            await _userService.ResetPasswordAsync(new ResetPasswordDto
            {
                UserId = userId,
                ResetToken = "reset-token",
                NewPassword = "newpass123"
            });

            user.Password.Should().NotBe("pass");
            user.PasswordResetToken.Should().BeNull();
        }
    }
}
