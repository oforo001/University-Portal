using System.ComponentModel.DataAnnotations;

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Pole hasło jest wymagane.")]
    [StringLength(40, MinimumLength = 8,
        ErrorMessage = "{0} musi mieć co najmniej {2} i maksymalnie {1} znaków.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nowe hasło")]
    public string NewPassword { get; set; }

    [Required(ErrorMessage = "Pole potwierdź hasło jest wymagane.")]
    [DataType(DataType.Password)]
    [Display(Name = "Potwierdź nowe hasło")]
    [Compare("NewPassword", ErrorMessage = "Hasła nie są takie same.")]
    public string ConfirmNewPassword { get; set; }

    public string Email { get; set; }

    public string? VerificationCode { get; set; }
}