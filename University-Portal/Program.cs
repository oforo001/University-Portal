using University_Portal.Data;
using University_Portal.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using University_Portal.Helpers;
using University_Portal.AppServices.Account;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<University_Portal.AppServices.E_mail.EmailService>();

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<ApplicationContext>()
.AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<University_Portal.AppServices.E_mail.IEmailService, University_Portal.AppServices.E_mail.EmailService>();
builder.Services.AddScoped<University_Portal.AppServices.E_mail.IVerificationTokenService, University_Portal.AppServices.E_mail.VerificationTokenService>();

builder.Services.AddScoped<IAccountActionStrategy<ChangePasswordViewModel>, ChangePasswordStrategy>();
builder.Services.AddScoped<IAccountActionStrategy<string>, SendPasswordResetTokenStrategy>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    DBUserRolesInit.SeedDefaultAdminAsync(userManager, roleManager).GetAwaiter().GetResult();
}

app.Use(async (context, next) =>
{
    if (AdminSetupState.IsInitialSetUpRequered && !context.Request.Path.StartsWithSegments("/Account/AccountSetup"))
    {
        context.Response.Redirect("/Account/AccountSetup");
        return;
    }
    await next();
});
    

    

app.Run();
