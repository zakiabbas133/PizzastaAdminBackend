using System.ComponentModel.DataAnnotations;

namespace PizzastaAdminBackend.DTOs.Locations
{
    public class CoordinatesDto
    {
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
    }

    public class LocationDto
    {
        public Guid? Id { get; set; }

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

        // Expecting 7 items (one per day) but allow any number
        public string OpeningHours { get; set; } = "03:00 PM - 03:00 AM";

        public CoordinatesDto? Coordinates { get; set; }
    }
}
