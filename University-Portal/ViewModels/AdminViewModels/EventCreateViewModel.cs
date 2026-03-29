using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace University_Portal.ViewModels.AdminViewModels
{
    public class EventCreateViewModel
    {
        [Required(ErrorMessage = "Pole Tytuł jest wymagane")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Tytuł musi mieć od 2 do 200 znaków")]
        [Display(Name = "Tytuł wydarzenia")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Pole Opis jest wymagane")]
        [StringLength(5000, MinimumLength = 2, ErrorMessage = "Opis musi mieć od 2 do 5000 znaków")]
        [Display(Name = "Opis wydarzenia")]
        public string Description { get; set; }

        [Display(Name = "Zdjęcie wydarzenia")]
        public IFormFile Image { get; set; }
        public string ImagePath { get; set; }

        [Required(ErrorMessage = "Pole Data jest wymagane")]
        [Display(Name = "Data i godzina")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Pole Lokalizacja jest wymagane")]
        [StringLength(1000, MinimumLength = 2, ErrorMessage = "Lokalizacja musi mieć od 2 do 1000 znaków")]
        [Display(Name = "Lokalizacja")]
        public string Location { get; set; }

        [Required(ErrorMessage = "Pole Maks. uczestników jest wymagane")]
        [Range(1, int.MaxValue, ErrorMessage = "Maksymalna liczba uczestników musi być większa od 0")]
        [Display(Name = "Maksymalna liczba uczestników")]
        public int MaxParticipants { get; set; }

        public List<RegisteredUser> RegisteredUsers { get; set; } = new();
    }
}