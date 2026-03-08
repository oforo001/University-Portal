using Microsoft.AspNetCore.Identity;
using University_Portal.Helpers;
using University_Portal.Models;

public static class DBUserRolesInit
{
    public static async Task SeedDefaultAdminAsync(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // this block checks whenever ROLES 'Admin' and 'User' exists in DB, If not it creates unexisting roles

        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        if (!await roleManager.RoleExistsAsync("User"))
            await roleManager.CreateAsync(new IdentityRole("User"));

        // this block cheks wnenever we can find ROLE 'Admin' and set the flag
        var admins = await userManager.GetUsersInRoleAsync("Admin");

        if (admins == null || admins.Count == 0)
        {
            AdminSetupState.IsInitialSetUpRequered = true;
        }
        else
        {
            AdminSetupState.IsInitialSetUpRequered = false;
        }

        
    }
}
