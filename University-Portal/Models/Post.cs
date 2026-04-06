using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
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
        [Display(Name = "Tytuł")]
        public string Title { get; set; }

        [Required]
        [StringLength(100000, MinimumLength = 5)]
        [Display(Name = "Treść")]
        public string Content { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 2)]
        [Display(Name = "Autor")]
        public string Author { get; set; }

        [ValidateNever]
        [Display(Name = "Obraz wyróżniający")]
        public string? FeatureImagePath { get; set; }

        [Display(Name = "Data publikacji")]
        public DateTime PublishDate { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(Category))]
        [Display(Name = "Kategoria")]
        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}