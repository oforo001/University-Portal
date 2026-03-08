using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using University_Portal.Models;

namespace University_Portal.ViewModels.BlogViewModel
{
    public class EditPostViewModel
    {
        public Post Post { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> Categories { get; set; }
        [ValidateNever]
        public IFormFile? FeatureImage { get; set; }
    }
}
