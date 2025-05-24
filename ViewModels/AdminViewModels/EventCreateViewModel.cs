using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace University_Portal.ViewModels.AdminViewModels
{
    public class EventCreateViewModel
    {
        [Required]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Provide Title with min. 2 and max. 200 characters")]
        public string Title { get; set; }

        [Required]
        [StringLength(5000, MinimumLength = 2, ErrorMessage = "Provide Description with min. 2 and max. 5000 characters")]
        public string Description { get; set; }

        // This property is for handling file uploads from the form
        public IFormFile Image { get; set; } 

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 2, ErrorMessage = "Provide Location with min. 2 and max. 1000 characters")]
        public string Location { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Max participants must be greater than 0")]
        public int MaxParticipants { get; set; }
    }
}
