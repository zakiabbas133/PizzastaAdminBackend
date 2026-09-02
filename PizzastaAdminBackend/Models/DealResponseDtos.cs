namespace PizzastaAdminBackend.DTOs.Deals
{
    public class DealItemResponseDto
    {
        public Guid Id { get; set; }

        public Guid MenuItemId { get; set; }

        public Guid? MenuItemVariantId { get; set; }

        public int Quantity { get; set; }

        public int DisplayOrder { get; set; }

        public string? MenuItemName { get; set; }

        public string? MenuItemVariantName { get; set; }
    }

    public class DealResponseDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string? Image { get; set; }

        public decimal Price { get; set; }

        public decimal? OriginalPrice { get; set; }

        public string? Badge { get; set; }

        public bool Featured { get; set; }

        public bool IsActive { get; set; }

        public int DisplayOrder { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        public List<DealItemResponseDto> DealItems { get; set; }
            = new List<DealItemResponseDto>();
    }
}