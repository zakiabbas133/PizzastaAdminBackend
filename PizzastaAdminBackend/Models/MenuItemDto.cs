using System.ComponentModel.DataAnnotations;

namespace PizzastaAdminBackend.DTOs.MenuItems
{
    public class MenuItemDto
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Slug { get; set; } = string.Empty;

        [Required]
        public Guid CategoryId { get; set; }
        public string? CategoryName { get; set; }

        public string Description { get; set; } = string.Empty;

        public string? Image { get; set; }
        public int Price { get; set; }
        public IFormFile? ImageFile { get; set; }

        public bool Featured { get; set; }

        public bool Popular { get; set; }

        public bool IsActive { get; set; }

        public int DisplayOrder { get; set; }

        public List<MenuItemVariantDto> Variants { get; set; } = new();
    }

    public class MenuItemVariantDto
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; }
    }
}