using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace University_Portal.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(300, MinimumLength = 2)]
        public string Title { get; set; }

        [Required]
        [StringLength(100000, MinimumLength = 50)]
        public string Content { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string Author { get; set; }
        [ValidateNever]
        public string? FeatureImagePath { get; set; }

        public DateTime PublishDate { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(Category))]
        [DisplayName("Category")]
        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }

}
