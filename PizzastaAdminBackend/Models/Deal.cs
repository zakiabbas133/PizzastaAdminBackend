using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PizzastaAdminBackend.Models
{
    public class Deal
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Image { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? OriginalPrice { get; set; }

        [MaxLength(100)]
        public string? Badge { get; set; }

        public bool Featured { get; set; }

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; }

        // Optional scheduling
        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        // Relationship
        public ICollection<DealItem> DealItems { get; set; }
            = new List<DealItem>();
    }
}