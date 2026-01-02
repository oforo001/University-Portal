using System.ComponentModel.DataAnnotations;

namespace University_Portal.ViewModels.AdminViewModels
{
    public class AddUserViewModel
    {
        [Required(ErrorMessage = "Imię i nazwisko jest wymagane.")]
        [Display(Name = "Imię i nazwisko")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Adres e-mail jest wymagany.")]
        [EmailAddress(ErrorMessage = "Nieprawidłowy format adresu e-mail.")]
        [Display(Name = "E-mail")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Hasło jest wymagane.")]
        [MinLength(8, ErrorMessage = "Hasło musi zawierać co najmniej 8 znaków.")]
        [Display(Name = "Hasło")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Wybór roli jest wymagany.")]
        [Display(Name = "Rola")]
        public string Role { get; set; } // Admin | User

        [Display(Name = "Aktywny")]
        public bool IsActive { get; set; } = true;
    }
}
