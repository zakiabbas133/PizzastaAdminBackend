using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzastaAdminBackend.Data;
using PizzastaAdminBackend.DTOs.MenuItems;
using PizzastaAdminBackend.Models;

namespace PizzastaAdminBackend.Controllers
{
    public class MenuController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public MenuController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }
        public async Task<IActionResult> GetMenuItems()
        {
            var menuItems = await _context.MenuItems
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Variants)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name)
                .Select(x => new MenuItemDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Slug = x.Slug,
                    Description = x.Description,
                    Image = x.Image,
                    Featured = x.Featured,
                    Popular = x.Popular,
                    IsActive = x.IsActive,
                    DisplayOrder = x.DisplayOrder,
                    Price = x.Price,
                    CategoryName = _context.Categories
                        .Where(c => c.Id == x.CategoryId)
                        .Select(c => c.Label)
                        .FirstOrDefault(),
                    CategoryId = x.CategoryId,

                    Variants = x.Variants
                        .OrderBy(v => v.DisplayOrder)
                        .Select(v => new MenuItemVariantDto
                        {
                            Id = v.Id,
                            Name = v.Name,
                            Price = v.Price,
                            DisplayOrder = v.DisplayOrder,
                            IsActive = v.IsActive
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(new { success = true, data = menuItems });
        }

        public async Task<IActionResult> GetMenuItem(Guid id)
        {
            var menuItem = await _context.MenuItems
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Variants)
                .Where(x => x.Id == id)
                .Select(x => new MenuItemDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Slug = x.Slug,
                    Description = x.Description,
                    Image = x.Image,
                    Featured = x.Featured,
                    Popular = x.Popular,
                    IsActive = x.IsActive,
                    DisplayOrder = x.DisplayOrder,

                    CategoryId = x.CategoryId,

                    Variants = x.Variants
                        .OrderBy(v => v.DisplayOrder)
                        .Select(v => new MenuItemVariantDto
                        {
                            Id = v.Id,
                            Name = v.Name,
                            Price = v.Price,
                            DisplayOrder = v.DisplayOrder,
                            IsActive = v.IsActive
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (menuItem == null)
            {
                return NotFound(new
                {
                    message = "Menu item not found."
                });
            }

            return Ok(menuItem);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMenuItem([FromForm] MenuItemDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid menu item data.",
                    errors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            x => x.Key,
                            x => x.Value!.Errors
                                .Select(e => e.ErrorMessage)
                                .ToArray()
                        )
                });
            }

            // ============================================================
            // Check category
            // ============================================================

            var categoryExists = await _context.Categories
                .AnyAsync(x => x.Id == dto.CategoryId);

            if (!categoryExists)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "The selected category does not exist."
                });
            }

            // ============================================================
            // Check duplicate slug
            // ============================================================

            var slug = dto.Slug.Trim();

            var slugExists = await _context.MenuItems
                .AnyAsync(x => x.Slug == slug);

            if (slugExists)
            {
                return Conflict(new
                {
                    success = false,
                    message = "A menu item with the same name already exists."
                });
            }

            // ============================================================
            // Upload folder
            // ============================================================

            var uploadsFolder = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "menu"
            );

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // ============================================================
            // Image upload
            // ============================================================

            string? imagePath = null;

            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                var extension = Path.GetExtension(dto.ImageFile.FileName);

                var allowedExtensions = new[]
                {
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".webp"
                };

                if (!allowedExtensions.Contains(
                        extension,
                        StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid image format. Only JPG, JPEG, PNG and WEBP are allowed."
                    });
                }

                // Optional 5 MB validation
                const long maxFileSize = 5 * 1024 * 1024;

                if (dto.ImageFile.Length > maxFileSize)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Image size must be less than 5MB."
                    });
                }

                var fileName = $"{Guid.NewGuid()}{extension}";

                var filePath = Path.Combine(
                    uploadsFolder,
                    fileName
                );

                await using var stream = new FileStream(
                    filePath,
                    FileMode.Create
                );

                await dto.ImageFile.CopyToAsync(stream);

                imagePath = $"/uploads/menu/{fileName}";
            }

            // ============================================================
            // Create menu item
            // ============================================================

            var menuItem = new MenuItem
            {
                Id = Guid.NewGuid(),

                Name = dto.Name.Trim(),

                Slug = slug,

                Description = dto.Description?.Trim() ?? string.Empty,

                Image = imagePath,

                Featured = dto.Featured,

                Popular = dto.Popular,

                IsActive = dto.IsActive,

                Price = dto.Price,

                DisplayOrder = await _context.MenuItems.AnyAsync()
                    ? await _context.MenuItems.CountAsync() + 1
                    : 1,

                CategoryId = dto.CategoryId
            };

            // ============================================================
            // Create variants
            // ============================================================

            if (dto.Variants != null)
            {
                foreach (var variantDto in dto.Variants)
                {
                    var variant = new MenuItemVariant
                    {
                        Id = Guid.NewGuid(),

                        Name = variantDto.Name.Trim(),

                        Price = variantDto.Price,

                        DisplayOrder = variantDto.DisplayOrder,

                        IsActive = variantDto.IsActive,

                        MenuItemId = menuItem.Id
                    };

                    menuItem.Variants.Add(variant);
                }
            }

            // ============================================================
            // Save
            // ============================================================

            _context.MenuItems.Add(menuItem);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetMenuItem),
                new { id = menuItem.Id },
                new
                {
                    success = true,
                    message = "Menu item created successfully.",
                    id = menuItem.Id,
                    image = menuItem.Image
                }
            );
        }

        [HttpPut]
        public async Task<IActionResult> UpdateMenuItem(
    Guid id,
    [FromForm] MenuItemDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid menu item data.",
                    errors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            x => x.Key,
                            x => x.Value!.Errors
                                .Select(e => e.ErrorMessage)
                                .ToArray()
                        )
                });
            }

            try
            {
                // ========================================================
                // SQL SERVER EXECUTION STRATEGY
                // ========================================================

                var strategy = _context.Database.CreateExecutionStrategy();

                IActionResult? result = null;

                await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction =
                        await _context.Database.BeginTransactionAsync();

                    try
                    {
                        // ========================================================
                        // GET MENU ITEM
                        // ========================================================

                        var menuItem = await _context.MenuItems
                            .FirstOrDefaultAsync(x => x.Id == id);

                        if (menuItem == null)
                        {
                            result = NotFound(new
                            {
                                success = false,
                                message = "Menu item not found."
                            });

                            await transaction.RollbackAsync();
                            return;
                        }

                        // ========================================================
                        // CHECK CATEGORY
                        // ========================================================

                        var categoryExists = await _context.Categories
                            .AnyAsync(x => x.Id == dto.CategoryId);

                        if (!categoryExists)
                        {
                            result = BadRequest(new
                            {
                                success = false,
                                message = "Selected category does not exist."
                            });

                            await transaction.RollbackAsync();
                            return;
                        }

                        // ========================================================
                        // CHECK DUPLICATE SLUG
                        // ========================================================

                        var slug = dto.Slug.Trim();

                        var slugExists = await _context.MenuItems
                            .AnyAsync(x =>
                                x.Slug == slug &&
                                x.Id != id);

                        if (slugExists)
                        {
                            result = BadRequest(new
                            {
                                success = false,
                                message =
                                    "A menu item with this slug already exists."
                            });

                            await transaction.RollbackAsync();
                            return;
                        }

                        // ========================================================
                        // UPDATE MENU ITEM
                        // ========================================================

                        menuItem.Name = dto.Name.Trim();
                        menuItem.Slug = slug;
                        menuItem.Description =
                            dto.Description?.Trim() ?? string.Empty;

                        menuItem.Price = dto.Price;
                        menuItem.Featured = dto.Featured;
                        menuItem.Popular = dto.Popular;
                        menuItem.IsActive = dto.IsActive;
                        menuItem.DisplayOrder = dto.DisplayOrder;
                        menuItem.CategoryId = dto.CategoryId;

                        // ========================================================
                        // HANDLE IMAGE
                        // ========================================================

                        if (dto.ImageFile != null &&
                            dto.ImageFile.Length > 0)
                        {
                            var uploadsFolder = Path.Combine(
                                _environment.WebRootPath,
                                "uploads",
                                "menu"
                            );

                            if (!Directory.Exists(uploadsFolder))
                            {
                                Directory.CreateDirectory(uploadsFolder);
                            }

                            // ----------------------------------------------------
                            // DELETE OLD IMAGE
                            // ----------------------------------------------------

                            if (!string.IsNullOrWhiteSpace(menuItem.Image))
                            {
                                var oldImagePath = menuItem.Image
                                    .Replace(
                                        "/",
                                        Path.DirectorySeparatorChar.ToString()
                                    )
                                    .TrimStart(
                                        Path.DirectorySeparatorChar
                                    );

                                var fullOldImagePath = Path.Combine(
                                    _environment.WebRootPath,
                                    oldImagePath
                                );

                                if (System.IO.File.Exists(fullOldImagePath))
                                {
                                    System.IO.File.Delete(fullOldImagePath);
                                }
                            }

                            // ----------------------------------------------------
                            // SAVE NEW IMAGE
                            // ----------------------------------------------------

                            var extension =
                                Path.GetExtension(
                                    dto.ImageFile.FileName
                                );

                            var fileName =
                                $"{Guid.NewGuid()}{extension}";

                            var newFilePath = Path.Combine(
                                uploadsFolder,
                                fileName
                            );

                            await using var stream =
                                new FileStream(
                                    newFilePath,
                                    FileMode.Create
                                );

                            await dto.ImageFile.CopyToAsync(stream);

                            menuItem.Image =
                                $"/uploads/menu/{fileName}";
                        }

                        // ========================================================
                        // SAVE MENU ITEM
                        // ========================================================

                        await _context.SaveChangesAsync();

                        // ========================================================
                        // GET CURRENT VARIANTS
                        // ========================================================

                        var existingVariants =
                            await _context.MenuItemVariants
                                .Where(x =>
                                    x.MenuItemId == menuItem.Id)
                                .AsNoTracking()
                                .ToListAsync();

                        var existingVariantIds =
                            existingVariants
                                .Select(x => x.Id)
                                .ToHashSet();

                        // ========================================================
                        // INCOMING VARIANTS
                        // ========================================================

                        var incomingVariants =
                            dto.Variants ??
                            new List<MenuItemVariantDto>();

                        // ========================================================
                        // CHECK DUPLICATE VARIANT IDS
                        // ========================================================

                        var duplicateVariantIds =
                            incomingVariants
                                .Where(x =>
                                    x.Id.HasValue &&
                                    x.Id.Value != Guid.Empty)
                                .GroupBy(x => x.Id!.Value)
                                .Where(g => g.Count() > 1)
                                .Select(g => g.Key)
                                .ToList();

                        if (duplicateVariantIds.Any())
                        {
                            result = BadRequest(new
                            {
                                success = false,
                                message =
                                    "The same variant cannot be submitted more than once.",
                                variantIds = duplicateVariantIds
                            });

                            await transaction.RollbackAsync();
                            return;
                        }

                        // ========================================================
                        // VALIDATE VARIANT IDS
                        // ========================================================

                        foreach (var variantDto in incomingVariants)
                        {
                            if (!variantDto.Id.HasValue ||
                                variantDto.Id.Value == Guid.Empty)
                            {
                                continue;
                            }

                            var variantId = variantDto.Id.Value;

                            // ----------------------------------------------------
                            // ID doesn't belong to this menu item
                            // ----------------------------------------------------

                            if (!existingVariantIds.Contains(variantId))
                            {
                                var variantExists =
                                    await _context.MenuItemVariants
                                        .AsNoTracking()
                                        .AnyAsync(x =>
                                            x.Id == variantId);

                                await transaction.RollbackAsync();

                                if (variantExists)
                                {
                                    result = BadRequest(new
                                    {
                                        success = false,
                                        message =
                                            "One of the selected variants does not belong to this menu item.",
                                        variantId
                                    });
                                }
                                else
                                {
                                    result = BadRequest(new
                                    {
                                        success = false,
                                        message =
                                            "One of the selected variant IDs does not exist.",
                                        variantId
                                    });
                                }

                                return;
                            }
                        }

                        // ========================================================
                        // INCOMING EXISTING VARIANT IDS
                        // ========================================================

                        var incomingVariantIds =
                            incomingVariants
                                .Where(x =>
                                    x.Id.HasValue &&
                                    x.Id.Value != Guid.Empty)
                                .Select(x => x.Id!.Value)
                                .ToHashSet();

                        // ========================================================
                        // UPDATE EXISTING / ADD NEW
                        // ========================================================

                        foreach (var variantDto in incomingVariants)
                        {
                            // ====================================================
                            // UPDATE EXISTING VARIANT
                            // ====================================================

                            if (variantDto.Id.HasValue &&
                                variantDto.Id.Value != Guid.Empty)
                            {
                                var variantId =
                                    variantDto.Id.Value;

                                var affectedRows =
                                    await _context.MenuItemVariants
                                        .Where(x =>
                                            x.Id == variantId &&
                                            x.MenuItemId == menuItem.Id)
                                        .ExecuteUpdateAsync(setters =>
                                            setters
                                                .SetProperty(
                                                    x => x.Name,
                                                    variantDto.Name.Trim()
                                                )
                                                .SetProperty(
                                                    x => x.Price,
                                                    variantDto.Price
                                                )
                                                .SetProperty(
                                                    x => x.DisplayOrder,
                                                    variantDto.DisplayOrder
                                                )
                                                .SetProperty(
                                                    x => x.IsActive,
                                                    variantDto.IsActive
                                                )
                                        );

                                if (affectedRows == 0)
                                {
                                    result = Conflict(new
                                    {
                                        success = false,
                                        message =
                                            "One of the variants no longer exists. Please reload the menu item and try again.",
                                        variantId
                                    });

                                    await transaction.RollbackAsync();
                                    return;
                                }
                            }

                            // ====================================================
                            // ADD NEW VARIANT
                            // ====================================================

                            else
                            {
                                var newVariant = new MenuItemVariant
                                {
                                    Id = Guid.NewGuid(),
                                    Name =
                                        variantDto.Name.Trim(),

                                    Price =
                                        variantDto.Price,

                                    DisplayOrder =
                                        variantDto.DisplayOrder,

                                    IsActive = true,

                                    MenuItemId =
                                        menuItem.Id
                                };

                                await _context.MenuItemVariants
                                    .AddAsync(newVariant);
                            }
                        }

                        // ========================================================
                        // SAVE NEW VARIANTS
                        // ========================================================

                        await _context.SaveChangesAsync();

                        // ========================================================
                        // DELETE REMOVED VARIANTS
                        // ========================================================

                        var variantIdsToDelete =
                            existingVariantIds
                                .Except(incomingVariantIds)
                                .ToList();

                        if (variantIdsToDelete.Any())
                        {
                            await _context.MenuItemVariants
                                .Where(x =>
                                    x.MenuItemId == menuItem.Id &&
                                    variantIdsToDelete.Contains(x.Id))
                                .ExecuteDeleteAsync();
                        }

                        // ========================================================
                        // COMMIT TRANSACTION
                        // ========================================================

                        await transaction.CommitAsync();

                        result = Ok(new
                        {
                            success = true,
                            message =
                                "Menu item updated successfully."
                        });
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });

                return result ?? StatusCode(500, new
                {
                    success = false,
                    message =
                        "The update operation did not produce a result."
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new
                {
                    success = false,
                    message =
                        "The menu item or one of its variants was modified or deleted by another user. Please reload the menu item and try again."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message =
                        "Something went wrong while updating the menu item.",
                    error = ex.Message
                });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteMenuItem(Guid id)
        {
            var menuItem = await _context.MenuItems
                .FirstOrDefaultAsync(x => x.Id == id);
            var variants = await _context.MenuItemVariants.Where(v => v.MenuItemId == id).ToListAsync();

            if (menuItem == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Menu item not found."
                });
            }
            // Check for DealItems referencing this menu item. The FK is configured with Restrict
            // so attempting to delete the MenuItem while DealItems exist will cause a SQL error.
            var referencingDealItems = await _context.DealItems
                .Where(d => d.MenuItemId == id)
                .Select(d => new { d.Id, d.DealId })
                .ToListAsync();

            if (referencingDealItems.Any())
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Cannot delete menu item because it is used in one or more deals.",
                    // include referencing deal item ids and deal ids so frontend can show context
                    references = referencingDealItems
                });
            }

            // No referencing DealItems -> safe to delete. Delete variants first, then the menu item.
            if (variants.Count > 0)
            {
                _context.MenuItemVariants.RemoveRange(variants);
            }

            _context.MenuItems.Remove(menuItem);

            try
            {
                await _context.SaveChangesAsync();

                var imagePath = menuItem.Image;

                if (!string.IsNullOrWhiteSpace(imagePath))
                {
                    var fileName = Path.GetFileName(imagePath);

                    var filePath = Path.Combine(_environment.WebRootPath, "uploads", "menu", fileName);

                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                return Ok(new
                {
                    success = true,
                    message = "Menu item deleted successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Something went wrong while deleting the menu item.",
                    error = ex.Message
                });
            }
        }
    }
}
