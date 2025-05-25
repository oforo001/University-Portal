using University_Portal.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.CodeDom.Compiler;

namespace University_Portal.Data
{
    public class ApplicationContext : IdentityDbContext<AppUser>
    {
        //1. Dziedziczy po IdentityDbContext<AppUser>,
        //   automatycznie dodaje wszystkie tabele związane z ASP.NET Identity (użytkownicy, role, itd.
        //2. Definiuje dodatkowe tabele bazy danych: Events oraz EventRegistrations, zgodnie z modelami
        //3. Dodaje domyślne role: Admin i User przy pierwszym uruchomieniu migracji.
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
            
        }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventRegistration> EventRegistrations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityRole>().HasData(

                new IdentityRole
                {
                    Id = "1",
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },
                new IdentityRole
                {
                    Id = "2",
                    Name = "User",
                    NormalizedName = "USER"
                }
                );
        }
    }
}
