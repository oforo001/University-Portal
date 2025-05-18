using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace University_Portal.Models
{
    public class EventRegistration
    {
        //Reprezentuje zgłoszenie konkretnego użytkownika
        //na wybrane wydarzenie.Zawiera informację o powiązaniu użytkownika i wydarzenia oraz status zgłoszenia

        [Key]
        public int Id { get; set; } // DB Column
        public bool IsCancelled { get; set; } = false; // DB Column
        [Required]
        public int EventId { get; set; } // DB Column
        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }
        [Required]
        public string UserId { get; set; } // DB Column
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; }

        [Required]
        public DateTime RegisteredAt { get; set; }
    }
}