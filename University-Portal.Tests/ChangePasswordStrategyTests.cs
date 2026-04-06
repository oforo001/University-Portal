using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using University_Portal.Models;
using University_Portal.AppServices.Account;
using University_Portal.ViewModels;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ChangePasswordStrategyTests
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
            new Mock<System.IServiceProvider>().Object,
            new Mock<ILogger<UserManager<AppUser>>>().Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_PasswordsDoNotMatch_ReturnsFalse()
    {
        // Arrange
        var userManagerMock = GetUserManagerMock();
        var strategy = new ChangePasswordStrategy(userManagerMock.Object);

        var model = new ChangePasswordViewModel
        {
            Email = "test@test.com",
            NewPassword = "Password1",
            ConfirmNewPassword = "Password2"
        };

        var result = await strategy.ExecuteAsync(model);

        Assert.False(result.Success);
        Assert.Equal("Hasła nie są takie same.", result.Message);
    }
}