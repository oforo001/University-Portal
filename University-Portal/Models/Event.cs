using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace University_Portal.Models
{
    public class Event
    {
        // Reprezentuje jedno wydarzenie organizowane na uczelni (np. wykład, spotkanie integracyjne, warsztaty)

        [Key]
        public int Id { get; set; } // Kolumna w DB

        [Required]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Podaj tytuł o długości od 2 do 200 znaków")]
        public string Title { get; set; } // Kolumna w DB

        [Required]
        [StringLength(5000, MinimumLength = 2, ErrorMessage = "Podaj opis o długości od 2 do 5000 znaków")]
        public string Description { get; set; } // Kolumna w DB

        public string? ImagePath { get; set; }

        [Required]
        public DateTime Date { get; set; } // Kolumna w DB

        [Required]
        [StringLength(1000, MinimumLength = 2, ErrorMessage = "Podaj lokalizację o długości od 2 do 1000 znaków")]
        public string Location { get; set; } // Kolumna w DB

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Maksymalna liczba uczestników musi być większa niż 0")]
        public int MaxParticipants { get; set; } // Kolumna w DB

        [Required]
        public string OrganizerId { get; set; } // Kolumna FK w DB

        [ForeignKey("OrganizerId")]
        public virtual AppUser Organizer { get; set; }

        public virtual ICollection<EventRegistration> Registrations { get; set; } = new HashSet<EventRegistration>();
    }
}