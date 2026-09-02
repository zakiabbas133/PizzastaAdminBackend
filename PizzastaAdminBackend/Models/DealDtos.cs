using Microsoft.AspNetCore.Http;
using PizzastaAdminBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace PizzastaAdminBackend.DTOs.Deals
{
    public class DealItemDto
    {
        public Guid? Id { get; set; }

        [Required]
        public Guid MenuItemId { get; set; }

        public Guid? MenuItemVariantId { get; set; }

        public int Quantity { get; set; } = 1;

        public int DisplayOrder { get; set; }
    }

    public class DealAddOrUpdateDto
    {
        public Guid? Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public decimal? OriginalPrice { get; set; }

        [MaxLength(100)]
        public string? Badge { get; set; }

        public bool Featured { get; set; }

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        // New image uploaded from frontend
        public IFormFile? Image { get; set; }

        // Set true when user removes the existing image
        public bool RemoveImage { get; set; }

        // Deal items are sent as JSON string in multipart/form-data
        public string DealItems { get; set; } = string.Empty;
    }
}