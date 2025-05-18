using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;


namespace University_Portal.Models
{
    public class AppUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }
        public bool IsBlocked { get; set; }
    }
}
