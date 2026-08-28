using System.ComponentModel.DataAnnotations;

namespace PizzastaAdminBackend.Models
{
    public class DealItem
    {
        public Guid Id { get; set; }

        [Required]
        public Guid DealId { get; set; }

        [Required]
        public Guid MenuItemId { get; set; }

        public int Quantity { get; set; } = 1;

        // Optional variant, e.g. "Large", "XL", "Pitcher"
        public Guid? MenuItemVariantId { get; set; }

        public int DisplayOrder { get; set; }

        // Relationships
        public Deal Deal { get; set; } = null!;

        public MenuItem MenuItem { get; set; } = null!;

        public MenuItemVariant? MenuItemVariant { get; set; }
    }
}