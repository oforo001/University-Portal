using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;


namespace University_Portal.Models
{
    public class AppUser : IdentityUser
    {
        public AppUser()
        {
            IsActive = true;
            CreatedAt = DateTime.Now;
        }
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
}
