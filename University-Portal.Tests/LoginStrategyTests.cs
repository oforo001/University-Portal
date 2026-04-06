using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using University_Portal.AppServices.Account;
using University_Portal.Models;
using University_Portal.ViewModels;
using Xunit;

public class LoginStrategyTests
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

    private Mock<SignInManager<AppUser>> GetSignInManagerMock(Mock<UserManager<AppUser>> userManagerMock)
    {
        return new Mock<SignInManager<AppUser>>(
            userManagerMock.Object,
            Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<AppUser>>(),
            null, null, null, null
        );
    }

    [Fact]
    public async Task ExecuteAsync_InvalidModel_ReturnsFalse()
    {
        var strategy = new LoginStrategy(GetSignInManagerMock(GetUserManagerMock()).Object, GetUserManagerMock().Object);

        var result = await strategy.ExecuteAsync(null);

        Assert.False(result.Success);
        Assert.Equal("Nieprawidłowe dane logowania.", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_UserNotFound_ReturnsFalse()
    {
        var userManagerMock = GetUserManagerMock();
        userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((AppUser)null);

        var strategy = new LoginStrategy(GetSignInManagerMock(userManagerMock).Object, userManagerMock.Object);

        var result = await strategy.ExecuteAsync(new LoginViewModel { Email = "a@b.com", Password = "pass" });

        Assert.False(result.Success);
        Assert.Equal("Niepoprawny adres e-mail lub hasło.", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_UserInactive_ReturnsFalse()
    {
        var user = new AppUser { IsActive = false };
        var userManagerMock = GetUserManagerMock();
        userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);

        var strategy = new LoginStrategy(GetSignInManagerMock(userManagerMock).Object, userManagerMock.Object);

        var result = await strategy.ExecuteAsync(new LoginViewModel { Email = "a@b.com", Password = "pass" });

        Assert.False(result.Success);
        Assert.Equal("Konto zostało dezaktywowane.", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ValidUser_SignsIn()
    {
        var user = new AppUser { IsActive = true };
        var userManagerMock = GetUserManagerMock();
        userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<AppUser>())).ReturnsAsync(IdentityResult.Success);
        userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin" });

        var signInManagerMock = GetSignInManagerMock(userManagerMock);
        signInManagerMock.Setup(x => x.PasswordSignInAsync(user, "pass", false, false))
                         .ReturnsAsync(SignInResult.Success);

        var strategy = new LoginStrategy(signInManagerMock.Object, userManagerMock.Object);

        var result = await strategy.ExecuteAsync(new LoginViewModel { Email = "a@b.com", Password = "pass" });

        Assert.True(result.Success);
        Assert.Equal("Admin", result.Message);
        Assert.NotNull(user.LastLoginAt);
    }
}