using System.ComponentModel.DataAnnotations;

namespace University_Portal.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Imię i nazwisko jest wymagane.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Adres e-mail jest wymagany.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Hasło jest wymagane.")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "{0} musi mieć co najmniej {2} i maksymalnie {1} znaków.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Potwierdzenie hasła jest wymagane.")]
        [DataType(DataType.Password)]
        [Display(Name = "Potwierdź hasło")]
        [Compare("Password", ErrorMessage = "Hasła nie są zgodne.")]
        public string ConfirmPassword { get; set; }
    }
}