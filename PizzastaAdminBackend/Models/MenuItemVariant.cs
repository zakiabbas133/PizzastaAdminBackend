using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PizzastaAdminBackend.Models
{
    public class MenuItemVariant
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;

        // Foreign Key
        [Required]
        public Guid MenuItemId { get; set; }

        // Relationship
        public MenuItem MenuItem { get; set; } = null!;
    }
}