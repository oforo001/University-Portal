using System.ComponentModel.DataAnnotations;

namespace University_Portal.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public ICollection<Post> Posts { get; set; } = new List<Post>();
    }

}
