using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using University_Portal.AppServices.Account;
using University_Portal.AppServices.E_mail;
using University_Portal.Models;
using Xunit;

public class SendPasswordResetTokenStrategyTests
{
    private Mock<UserManager<AppUser>> GetUserManagerMock()
    {
        var store = new Mock<IUserStore<AppUser>>();
        return new Mock<UserManager<AppUser>>(
            store.Object,
            Options.Create(new IdentityOptions()),
            new Mock<IPasswordHasher<AppUser>>().Object,
            new IUserValidator<AppUser>[0],
            new IPasswordValidator<AppUser>[0],
            new Mock<ILookupNormalizer>().Object,
            new IdentityErrorDescriber(),
            new Mock<IServiceProvider>().Object,
            new Mock<ILogger<UserManager<AppUser>>>().Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_UserNotFound_ReturnsFalse()
    {
        var userManagerMock = GetUserManagerMock();
        userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                       .ReturnsAsync((AppUser)null);

        var emailServiceMock = new Mock<IEmailService>();
        var tokenServiceMock = new Mock<IVerificationTokenService>();

        var strategy = new SendPasswordResetTokenStrategy(
            userManagerMock.Object,
            emailServiceMock.Object,
            tokenServiceMock.Object
        );

        var result = await strategy.ExecuteAsync("test@example.com");

        Assert.False(result.Success);
        Assert.Equal("Nie znaleziono użytkownika.", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_UserInactive_ReturnsFalse()
    {
        var user = new AppUser { IsActive = false };
        var userManagerMock = GetUserManagerMock();
        userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                       .ReturnsAsync(user);

        var emailServiceMock = new Mock<IEmailService>();
        var tokenServiceMock = new Mock<IVerificationTokenService>();
        tokenServiceMock.Setup(x => x.GenerateToken()).Returns("token");

        var strategy = new SendPasswordResetTokenStrategy(
            userManagerMock.Object,
            emailServiceMock.Object,
            tokenServiceMock.Object
        );

        var result = await strategy.ExecuteAsync("test@example.com");

        Assert.False(result.Success);
        Assert.Equal("Konto nieaktywne.", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ValidUser_SendsEmailAndSetsToken()
    {
        var user = new AppUser { IsActive = true };
        var userManagerMock = GetUserManagerMock();
        userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var emailServiceMock = new Mock<IEmailService>();
        emailServiceMock.Setup(x => x.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
                        .Returns(Task.CompletedTask);

        var tokenServiceMock = new Mock<IVerificationTokenService>();
        tokenServiceMock.Setup(x => x.GenerateToken()).Returns("token");

        var strategy = new SendPasswordResetTokenStrategy(
            userManagerMock.Object,
            emailServiceMock.Object,
            tokenServiceMock.Object
        );

        var result = await strategy.ExecuteAsync("test@example.com");

        Assert.True(result.Success);
        Assert.Equal("Kod resetu został wysłany.", result.Message);
        Assert.Equal("token", user.PasswordResetToken);
        Assert.NotNull(user.PasswordResetTokenExpiry);

        emailServiceMock.Verify(x => x.SendPasswordResetEmailAsync("test@example.com", "token"), Times.Once);
        userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }
}