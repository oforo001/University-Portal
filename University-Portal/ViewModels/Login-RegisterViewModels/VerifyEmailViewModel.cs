using System.ComponentModel.DataAnnotations;

namespace University_Portal.ViewModels
{
    public class VerifyEmailViewModel
    {
        [StringLength(6, MinimumLength = 6)]
        [Display(Name = "Verification Code")]
        public string? Token { get; set; }

        public string? Email { get; set; } 
    }
}
