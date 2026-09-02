using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzastaAdminBackend.Data;
using PizzastaAdminBackend.Models;

public class CategoriesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public CategoriesController(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<IActionResult> List()    
    {
        return Json(new { success = true, data = await _context.Categories.ToListAsync() });
    }

    public async Task<IActionResult> Details(System.Guid? id)
    {
        if (id == null)
        {
            return Json(new { success = false, data = "No category found." });
        }

        var category = await _context.Categories.FirstOrDefaultAsync(m => m.Id == id);
        if (category == null)
        {
            return Json(new { success = false, data = "No category found." });
        }

        return Json(new { success = true, data = category });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> UpdateCategoryOrder(string draggedCategoryId, string targetCategoryId, int targetOrder, int draggedOrder)
    {
        if(draggedCategoryId == null || targetCategoryId == null)
        {
            return Json(new
            {
                success = false,
                message = "One or both categories could not be found."
            });
        }
        var draggedCategory = await _context.Categories
            .FirstOrDefaultAsync(x => x.Id == Guid.Parse(draggedCategoryId));

        var targetCategory = await _context.Categories
            .FirstOrDefaultAsync(x => x.Id == Guid.Parse(targetCategoryId));

        if (draggedCategory == null || targetCategory == null)
        {
            return Json(new
            {
                success = false,
                message = "One or both categories could not be found."
            });
        }

        // Store the current orders
        var previousDraggedOrder = draggedCategory.DisplayOrder;
        var previousTargetOrder = targetCategory.DisplayOrder;

        // Swap the orders
        draggedCategory.DisplayOrder = previousTargetOrder;
        targetCategory.DisplayOrder = previousDraggedOrder;

        await _context.SaveChangesAsync();

        return Json(new
        {
            success = true,
            message = "Menu order changed."
        });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(Category category, IFormFile? image)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please fill out the form correctly."
                });
            }
            var existingCategory = _context.Categories.FirstOrDefault(x => x.Label.ToLower() == category.Label.ToLower());
            if(existingCategory != null)
            {
                return Json(new
                {
                    success = false,
                    message = "Cannot create a category with the same name."
                });
            }
            if (image == null || image.Length == 0)
            {
                return Json(new
                {
                    success = false,
                    message = "Category image is required."
                });
            }
            var allowedExtensions = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };
            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return Json(new
                {
                    success = false,
                    message = "Only JPG, JPEG, PNG and WEBP images are allowed."
                });
            }
            const long maxFileSize = 5 * 1024 * 1024;
            if (image.Length > maxFileSize)
            {
                return Json(new
                {
                    success = false,
                    message = "Image size cannot exceed 5 MB."
                });
            }
            var webRootPath = _environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRootPath))
            {
                webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
            }
            var uploadDirectory = Path.Combine(webRootPath, "uploads", "categories");
            Directory.CreateDirectory(uploadDirectory);
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadDirectory, fileName);
            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }
            category.Image = $"/uploads/categories/{fileName}";
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return Json(new
            {
                success = true,
                message = "Category created successfully.",
                data = category
            });
        }
        catch (Exception ex)
        {
            return Json(new
            {
                success = false,
                message = "Something went wrong while adding the category. Please try again later."
            });
        }
    }
    
    [Authorize]
    [HttpPatch]
    public async Task<IActionResult> Edit(CategoryUpdateRequest request)
    {
        var category = await _context.Categories.FindAsync(Guid.Parse(request.Id));

        if (category == null)
        {
            return NotFound();
        }

        category.Label = request.Label;
        category.Description = request.Description;
        category.IsActive = request.IsActive;
        category.DisplayOrder = request.DisplayOrder;

        // User explicitly removed the image
        if (request.RemoveImage)
        {
            // Delete physical image if required
            if (!string.IsNullOrEmpty(category.Image))
            {
                var oldImagePath = Path.Combine(
                    _environment.WebRootPath,
                    category.Image.TrimStart('/')
                );

                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            category.Image = null;
        }
        // User selected a new image
        else if (request.Image != null)
        {
            // Delete old image
            if (!string.IsNullOrEmpty(category.Image))
            {
                var oldImagePath = Path.Combine(
                    _environment.WebRootPath,
                    category.Image.TrimStart('/')
                );

                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            // Save new image
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.Image.FileName)}";

            var uploadDirectory = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "categories"
            );

            Directory.CreateDirectory(uploadDirectory);

            var filePath = Path.Combine(uploadDirectory, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await request.Image.CopyToAsync(stream);

            category.Image = $"/uploads/categories/{fileName}";
        }

        // If neither RemoveImage nor Image is provided,
        // category.Image remains unchanged.

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            data = category
        });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Delete(Guid? id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
        {
            return Json(new
            {
                success = false,
                data = "Something went wrong while removing the category. Please try again later."
            });
        }

        // Keep the image path before deleting the database record
        var imagePath = category.Image;

        try
        {
            // Remove category from database
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            // Remove category image from server
            if (!string.IsNullOrWhiteSpace(imagePath))
            {
                var fileName = Path.GetFileName(imagePath);

                var filePath = Path.Combine(_environment.WebRootPath, "uploads", "categories", fileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            return Json(new
            {
                success = true,
                data = "Category removed successfully"
            });
        }
        catch (Exception)
        {
            return Json(new
            {
                success = false,
                data = "Something went wrong while removing the category. Please try again later."
            });
        }
    }
}
