using System.ComponentModel.DataAnnotations;

public class CategoryUpdateRequest
{
    public string Id { get; set; }
    public string Label { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public int DisplayOrder { get; set; }

    public IFormFile? Image { get; set; }

    public bool RemoveImage { get; set; }
}