using System.ComponentModel.DataAnnotations;

namespace University_Portal.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Email is requered.")]
        [EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is requered.")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "The {0} must be at {2} and max. {1} character")]
        [DataType(DataType.Password)]
        [Compare("ConfirmNewPassword", ErrorMessage = "Password does not match.")]
        [Display(Name = "New Password")]

        public string NewPassword { get; set; }
        [Required(ErrorMessage = "Confirm password is requered.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        public string ConfirmNewPassword { get; set; }
    }
}
