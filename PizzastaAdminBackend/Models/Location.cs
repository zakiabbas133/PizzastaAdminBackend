using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PizzastaAdminBackend.Models
{
    public class Location
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(80)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string Whatsapp { get; set; } = string.Empty;

        [Range(-90, 90)]
        public decimal Latitude { get; set; }

        [Range(-180, 180)]
        public decimal Longitude { get; set; }

        // Relationship
        public ICollection<LocationOpeningHour> OpeningHours { get; set; }
            = new List<LocationOpeningHour>();
    }
}