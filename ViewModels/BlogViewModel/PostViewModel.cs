using Microsoft.AspNetCore.Mvc.Rendering;
using University_Portal.Models;

namespace University_Portal.ViewModels.BlogViewModel
{
    public class PostViewModel
    {
        public Post Post { get; set; }
        public IEnumerable<SelectListItem> Categories { get; set; }
        public IFormFile FeatureImage { get; set; }
    }
}
