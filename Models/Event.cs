using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace University_Portal.Models
{
    public class Event
    {
      //  Reprezentuje jedno wydarzenie organizowane na uczelni(np.wykład, spotkanie integracyjne, warsztaty)


        [Key]
        public int Id { get; set; } // DB column
        [Required]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Provide Title with min. 2 and max. 200 characters")]
        public string Title { get; set; } // DB column
        [Required]
        [StringLength(5000, MinimumLength = 2, ErrorMessage = "Provide Description with min. 2 and max. 5000 characters")]
        public string Description { get; set; } // DB column
        [Required]
        public DateTime Date { get; set; } // DB column
        [Required]
        [StringLength(1000, MinimumLength = 2, ErrorMessage = "Provide Location with min. 2 and max. 1000 characters")]
        public string Location { get; set; } // DB column
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Max participants must be greater than 0")]
        public int MaxParticipants { get; set; } // DB column
        [Required]
        public string OrganizerId { get; set; } // DB FK column
        [ForeignKey("OrganizerId")]
        public virtual AppUser Organizer { get; set; } 

        public virtual ICollection<EventRegistration> Registrations { get; set; }




    }
}
