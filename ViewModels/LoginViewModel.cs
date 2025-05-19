using System.ComponentModel.DataAnnotations;

namespace University_Portal.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is requred.")]
        [EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is requred.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Display(Name = "Remember me.")]
        public bool RememberMe { get; set; }
    }
}
