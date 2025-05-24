using System.ComponentModel.DataAnnotations;

namespace University_Portal.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Name is requered.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Email is requered.")]
        [EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is requered.")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "The {0} must be at {2} and max. {1} character")]
        [DataType(DataType.Password)]
        [Compare("ConfirmPassword", ErrorMessage = "Password does not match.")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Confirm password is requered.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

    }
}
