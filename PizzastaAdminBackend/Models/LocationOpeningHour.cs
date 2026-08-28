using System.ComponentModel.DataAnnotations;

namespace PizzastaAdminBackend.Models
{
    public class LocationOpeningHour
    {
        public Guid Id { get; set; }

        [Required]
        public Guid LocationId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Day { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Hours { get; set; } = string.Empty;

        // Relationship
        public Location Location { get; set; } = null!;
    }
}