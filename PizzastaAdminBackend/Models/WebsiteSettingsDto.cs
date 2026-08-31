using System.ComponentModel.DataAnnotations;

namespace PizzastaAdminBackend.Models
{
    public class WebsiteSettingsDto
    {
        public Guid Id { get; set; }

        [MaxLength(1000)]
        public string Logo { get; set; } = string.Empty;

        public IFormFile? LogoFile { get; set; }

        // Multiple slider images
        public List<IFormFile>? SliderImageFiles { get; set; }

        // Existing slider images that should remain
        public string SliderImages { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Video { get; set; } = string.Empty;

        public IFormFile? VideoFile { get; set; }

        public bool RemoveVideo { get; set; }

        [Required]
        [MaxLength(200)]
        public string FacebookUrl { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string InstagramUrl { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string WhatsappUrl { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string WhatsappMessage { get; set; } =
            "Hello! I would like to know more about your menu.";

        [Required]
        [MaxLength(254)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}