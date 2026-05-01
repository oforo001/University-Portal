using University_Portal.Data;
using University_Portal.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using University_Portal.Helpers;
using University_Portal.AppServices.Account;
using Microsoft.AspNetCore.Hosting.Server;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();


builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure();
        }));

builder.Services.AddScoped<University_Portal.AppServices.E_mail.EmailService>();

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<ApplicationContext>()
.AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<University_Portal.AppServices.E_mail.IEmailService,
    University_Portal.AppServices.E_mail.EmailService>();

builder.Services.AddScoped<University_Portal.AppServices.E_mail.IVerificationTokenService,
    University_Portal.AppServices.E_mail.VerificationTokenService>();

builder.Services.AddScoped<IAccountActionStrategy<ChangePasswordViewModel>, ChangePasswordStrategy>();
builder.Services.AddScoped<IAccountActionStrategy<string>, SendPasswordResetTokenStrategy>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
    db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.Use(async (context, next) =>
{
    var path = context.Request.Path;

    if (AdminSetupState.IsInitialSetUpRequered &&
        !path.StartsWithSegments("/Account/AccountSetup") &&
        !path.StartsWithSegments("/css") &&
        !path.StartsWithSegments("/js") &&
        !path.StartsWithSegments("/lib"))
    {
        context.Response.Redirect("/Account/AccountSetup");
        return;
    }

    await next();
});

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

    DBUserRolesInit
        .SeedDefaultAdminAsync(userManager, roleManager)
        .GetAwaiter()
        .GetResult();
}

app.Run();