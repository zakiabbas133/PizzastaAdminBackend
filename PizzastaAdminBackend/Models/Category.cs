using System.ComponentModel.DataAnnotations;

namespace PizzastaAdminBackend.Models
{
    public class Category
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Label { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(1000)]
        public string? Image { get; set; }

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; }

        // Relationship
        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    }
}