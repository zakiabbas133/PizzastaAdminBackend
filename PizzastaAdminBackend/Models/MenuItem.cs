using System.ComponentModel.DataAnnotations;

namespace PizzastaAdminBackend.Models
{
    public class MenuItem
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string Slug { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Image { get; set; }

        public bool Featured { get; set; }

        public bool Popular { get; set; }

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; }

        // Foreign Key
        [Required]
        public Guid CategoryId { get; set; }

        // Relationship
        public Category Category { get; set; } = null!;

        // Relationships
        public ICollection<MenuItemVariant> Variants { get; set; }
            = new List<MenuItemVariant>();

        public ICollection<DealItem> DealItems { get; set; }
            = new List<DealItem>();
    }
}