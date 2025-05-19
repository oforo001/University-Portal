using System.ComponentModel.DataAnnotations;

namespace University_Portal.ViewModels
{
    public class VerifyEmailViewModel
    {
        [Required(ErrorMessage = "Email is requered.")]
        [EmailAddress]
        public string Email { get; set; }
    }
}
