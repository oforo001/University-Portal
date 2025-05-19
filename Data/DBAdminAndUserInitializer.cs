using Microsoft.AspNetCore.Identity;
using University_Portal.Models;

public static class DbInitializer
{
    public static async Task SeedDefaultAdminAsync(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // code checks if the roles e.g : Admin or User existing

        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        if (!await roleManager.RoleExistsAsync("User"))
            await roleManager.CreateAsync(new IdentityRole("User"));

        string adminEmail = "admin@university.com"; // E-Mail do Admin Account

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        // If admin  does NOT exist in the database
        if (adminUser == null)
        {

            adminUser = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "Default Admin"
            };
            var result = await userManager.CreateAsync(adminUser, "Admin@123"); // haslo do Admin Account
            if (result.Succeeded)
            {
                // Adds the user to the “Admin” role
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
            else
            {
                // Handle creation errors (log, throw, etc.)
            }
        }
        else
        {
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
