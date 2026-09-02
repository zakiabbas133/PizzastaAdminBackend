using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzastaAdminBackend.Data;
using PizzastaAdminBackend.DTOs.Deals;
using PizzastaAdminBackend.Models;
using System.Text.Json;

namespace PizzastaAdminBackend.Controllers
{
    public class DealController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public DealController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // ============================================================
        // GET: /Deal/ListDeals
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> ListDeals()
        {
            try
            {
                var deals = await _context.Deals
                    .AsNoTracking()
                    .Include(d => d.DealItems)
                        .ThenInclude(di => di.MenuItem)
                    .Include(d => d.DealItems)
                        .ThenInclude(di => di.MenuItemVariant)
                    .OrderBy(d => d.DisplayOrder)
                    .ThenBy(d => d.Title)
                    .ToListAsync();

                var result = deals
                    .Select(MapDeal)
                    .ToList();

                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving deals.",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // GET: /Deal/DealDetails
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> DealDetails(Guid id)
        {
            try
            {
                var deal = await _context.Deals
                    .AsNoTracking()
                    .Include(d => d.DealItems)
                        .ThenInclude(di => di.MenuItem)
                    .Include(d => d.DealItems)
                        .ThenInclude(di => di.MenuItemVariant)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (deal == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Deal not found."
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Deal retrieved successfully.",
                    data = MapDeal(deal)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving the deal.",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // POST: /Deal/AddOrUpdateDeal
        // ============================================================

        [Authorize]
        [HttpPost]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> AddOrUpdateDeal([FromForm] DealAddOrUpdateDto model)
        {
            // ========================================================
            // MODEL VALIDATION
            // ========================================================

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Please fill out the form correctly.",
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

            // ========================================================
            // PARSE DEAL ITEMS
            // ========================================================

            List<DealItemDto> dealItems;

            try
            {
                if (string.IsNullOrWhiteSpace(model.DealItems))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "Please select 1 or more menu items for this deal."
                    });
                }

                dealItems =
                    JsonSerializer.Deserialize<List<DealItemDto>>(
                        model.DealItems,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }
                    ) ?? new List<DealItemDto>();
            }
            catch (JsonException)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid DealItems format."
                });
            }

            // ========================================================
            // BASIC VALIDATION
            // ========================================================

            if (dealItems.Count == 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message =
                        "Please select 1 or more menu items for this deal."
                });
            }

            if (model.Price < 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Price cannot be negative."
                });
            }

            if (model.OriginalPrice.HasValue &&
                model.OriginalPrice.Value < 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Original price cannot be negative."
                });
            }

            if (model.StartTime.HasValue &&
                model.EndTime.HasValue &&
                model.StartTime.Value >= model.EndTime.Value)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "End time must be later than start time."
                });
            }

            // ========================================================
            // VALIDATE DEAL ITEMS
            // ========================================================

            foreach (var item in dealItems)
            {
                if (item.MenuItemId == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "Every deal item must have a valid MenuItemId."
                    });
                }

                if (item.Quantity <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "Deal item quantity must be greater than zero."
                    });
                }

                if (item.MenuItemVariantId == Guid.Empty)
                {
                    item.MenuItemVariantId = null;
                }
            }

            // ========================================================
            // VALIDATE MENU ITEMS
            // ========================================================

            var menuItemIds = dealItems
                .Select(x => x.MenuItemId)
                .Distinct()
                .ToList();

            var existingMenuItemIds = await _context.MenuItems
                .Where(x => menuItemIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync();

            var missingMenuItems = menuItemIds
                .Except(existingMenuItemIds)
                .ToList();

            if (missingMenuItems.Count > 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "One or more MenuItems do not exist.",
                    menuItemIds = missingMenuItems
                });
            }

            // ========================================================
            // VALIDATE VARIANTS
            // ========================================================

            var variantIds = dealItems
                .Where(x => x.MenuItemVariantId.HasValue)
                .Select(x => x.MenuItemVariantId!.Value)
                .Distinct()
                .ToList();

            if (variantIds.Count > 0)
            {
                var variants = await _context.MenuItemVariants
                    .Where(x => variantIds.Contains(x.Id))
                    .Select(x => new
                    {
                        x.Id,
                        x.MenuItemId
                    })
                    .ToListAsync();

                var missingVariants = variantIds
                    .Except(variants.Select(x => x.Id))
                    .ToList();

                if (missingVariants.Count > 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "One or more MenuItemVariants do not exist.",
                        menuItemVariantIds = missingVariants
                    });
                }

                // Make sure variant belongs to selected menu item
                foreach (var item in dealItems.Where(x =>
                    x.MenuItemVariantId.HasValue))
                {
                    var variant = variants.FirstOrDefault(x =>
                        x.Id == item.MenuItemVariantId);

                    if (variant == null)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Invalid menu item variant."
                        });
                    }

                    if (variant.MenuItemId != item.MenuItemId)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message =
                                "The selected MenuItemVariant does not belong to the selected MenuItem."
                        });
                    }
                }
            }

            // ========================================================
            // IMAGE VALIDATION
            // ========================================================

            if (model.Image != null)
            {
                var allowedExtensions = new[]
                {
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".webp"
                };

                var extension = Path
                    .GetExtension(model.Image.FileName)
                    .ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "Invalid image format. Allowed formats: JPG, JPEG, PNG and WEBP."
                    });
                }

                if (model.Image.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "Image size cannot exceed 5 MB."
                    });
                }
            }

            // ========================================================
            // EXECUTION STRATEGY
            // ========================================================

            var strategy =
                _context.Database.CreateExecutionStrategy();

            object? response = null;

            string? oldImageToDelete = null;

            try
            {
                await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction =
                        await _context.Database.BeginTransactionAsync();

                    try
                    {
                        // ====================================================
                        // UPDATE
                        // ====================================================

                        if (model.Id.HasValue)
                        {
                            var deal = await _context.Deals
                                .Include(d => d.DealItems)
                                .FirstOrDefaultAsync(d =>
                                    d.Id == model.Id.Value);

                            if (deal == null)
                            {
                                throw new KeyNotFoundException(
                                    "Deal not found."
                                );
                            }

                            var oldImage = deal.Image;

                            // -----------------------------------------------
                            // UPDATE BASIC DATA
                            // -----------------------------------------------

                            deal.Title =
                                model.Title.Trim();

                            deal.Description =
                                model.Description.Trim();

                            deal.Price =
                                model.Price;

                            deal.OriginalPrice =
                                model.OriginalPrice;

                            deal.Badge =
                                string.IsNullOrWhiteSpace(model.Badge)
                                    ? null
                                    : model.Badge.Trim();

                            deal.Featured =
                                model.Featured;

                            deal.IsActive =
                                model.IsActive;

                            deal.DisplayOrder =
                                model.DisplayOrder;

                            deal.StartTime =
                                model.StartTime;

                            deal.EndTime =
                                model.EndTime;

                            // -----------------------------------------------
                            // IMAGE UPDATE
                            // -----------------------------------------------

                            if (model.Image != null)
                            {
                                var newImagePath =
                                    await SaveDealImage(model.Image);

                                deal.Image =
                                    newImagePath;
                            }
                            else if (model.RemoveImage)
                            {
                                deal.Image = null;
                            }

                            // -----------------------------------------------
                            // UPDATE DEAL ITEMS
                            // -----------------------------------------------

                            var existingItems =
                                deal.DealItems.ToList();

                            // IDs of existing DealItems coming
                            // from frontend
                            var incomingIds = dealItems
                                .Where(x => x.Id != Guid.Empty)
                                .Select(x => x.Id)
                                .ToHashSet();

                            // Delete items removed from frontend
                            foreach (var existingItem in existingItems)
                            {
                                if (!incomingIds.Contains(
                                    existingItem.Id))
                                {
                                    _context.DealItems.Remove(
                                        existingItem
                                    );
                                }
                            }

                            // Add/update incoming items
                            foreach (var itemDto in dealItems)
                            {
                                DealItem? dealItem = null;

                                // Existing item
                                if (itemDto.Id != Guid.Empty)
                                {
                                    dealItem =
                                        existingItems.FirstOrDefault(
                                            x => x.Id == itemDto.Id
                                        );
                                }

                                // New item
                                if (dealItem == null)
                                {
                                    dealItem = new DealItem
                                    {
                                        Id = Guid.NewGuid(),
                                        DealId = deal.Id
                                    };

                                    _context.DealItems.Add(
                                        dealItem
                                    );
                                }

                                dealItem.MenuItemId =
                                    itemDto.MenuItemId;

                                dealItem.Quantity =
                                    itemDto.Quantity;

                                dealItem.DisplayOrder =
                                    itemDto.DisplayOrder;
                            }

                            // -----------------------------------------------
                            // SAVE
                            // -----------------------------------------------

                            await _context.SaveChangesAsync();

                            await transaction.CommitAsync();

                            // -----------------------------------------------
                            // OLD IMAGE
                            // -----------------------------------------------

                            if (!string.IsNullOrWhiteSpace(oldImage) &&
                                (
                                    model.Image != null ||
                                    model.RemoveImage
                                ))
                            {
                                oldImageToDelete = oldImage;
                            }

                            response = new
                            {
                                success = true,
                                message =
                                    "Deal updated successfully.",
                                data = new
                                {
                                    id = deal.Id,
                                    image = deal.Image
                                }
                            };

                            return;
                        }

                        // ====================================================
                        // ADD
                        // ====================================================

                        var newDeal = new Deal
                        {
                            Id = Guid.NewGuid(),

                            Title =
                                model.Title.Trim(),

                            Description =
                                model.Description.Trim(),

                            Price =
                                model.Price,

                            OriginalPrice =
                                model.OriginalPrice,

                            Badge =
                                string.IsNullOrWhiteSpace(model.Badge)
                                    ? null
                                    : model.Badge.Trim(),

                            Featured =
                                model.Featured,

                            IsActive =
                                model.IsActive,

                            DisplayOrder =
                                model.DisplayOrder,

                            StartTime =
                                model.StartTime,

                            EndTime =
                                model.EndTime
                        };

                        // -----------------------------------------------
                        // SAVE IMAGE
                        // -----------------------------------------------

                        if (model.Image != null)
                        {
                            newDeal.Image =
                                await SaveDealImage(
                                    model.Image
                                );
                        }

                        // -----------------------------------------------
                        // ADD DEAL
                        // -----------------------------------------------

                        _context.Deals.Add(newDeal);

                        // -----------------------------------------------
                        // ADD DEAL ITEMS
                        // -----------------------------------------------

                        foreach (var itemDto in dealItems)
                        {
                            var dealItem = new DealItem
                            {
                                Id = Guid.NewGuid(),

                                DealId =
                                    newDeal.Id,

                                MenuItemId =
                                    itemDto.MenuItemId,

                                Quantity =
                                    itemDto.Quantity,

                                DisplayOrder =
                                    itemDto.DisplayOrder
                            };

                            _context.DealItems.Add(
                                dealItem
                            );
                        }

                        // -----------------------------------------------
                        // SAVE
                        // -----------------------------------------------

                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();

                        response = new
                        {
                            success = true,
                            message =
                                "Deal added successfully.",
                            data = new
                            {
                                id = newDeal.Id,
                                image = newDeal.Image
                            }
                        };
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });

                // ========================================================
                // DELETE OLD IMAGE AFTER SUCCESSFUL TRANSACTION
                // ========================================================

                if (!string.IsNullOrWhiteSpace(
                    oldImageToDelete))
                {
                    DeleteDealImage(
                        oldImageToDelete
                    );
                }

                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message =
                        "An error occurred while saving the deal.",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // DELETE: /Deal/DeleteDeal/{id}
        // ============================================================

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteDeal(Guid id)
        {
            var strategy =
                _context.Database.CreateExecutionStrategy();

            string? imageToDelete = null;

            try
            {
                await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction =
                        await _context.Database.BeginTransactionAsync();

                    try
                    {
                        var deal = await _context.Deals
                            .Include(d => d.DealItems)
                            .FirstOrDefaultAsync(d =>
                                d.Id == id);

                        if (deal == null)
                        {
                            throw new KeyNotFoundException(
                                "Deal not found."
                            );
                        }

                        imageToDelete = deal.Image;

                        // -----------------------------------------------
                        // DELETE DEAL ITEMS
                        // -----------------------------------------------

                        if (deal.DealItems.Any())
                        {
                            _context.DealItems.RemoveRange(
                                deal.DealItems
                            );
                        }

                        // -----------------------------------------------
                        // DELETE DEAL
                        // -----------------------------------------------

                        _context.Deals.Remove(deal);

                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });

                // Delete image only after successful DB transaction
                if (!string.IsNullOrWhiteSpace(imageToDelete))
                {
                    DeleteDealImage(imageToDelete);
                }

                return Ok(new
                {
                    success = true,
                    message = "Deal deleted successfully."
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message =
                        "An error occurred while deleting the deal.",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // IMAGE SAVE
        // ============================================================

        private async Task<string> SaveDealImage(IFormFile image)
        {
            var uploadsFolder = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "deals"
            );

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(
                    uploadsFolder
                );
            }

            var extension = Path
                .GetExtension(image.FileName)
                .ToLowerInvariant();

            var fileName =
                $"{Guid.NewGuid():N}{extension}";

            var filePath = Path.Combine(
                uploadsFolder,
                fileName
            );

            await using var stream =
                new FileStream(
                    filePath,
                    FileMode.Create
                );

            await image.CopyToAsync(stream);

            return $"/uploads/deals/{fileName}";
        }

        // ============================================================
        // IMAGE DELETE
        // ============================================================

        private void DeleteDealImage(string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return;
            }

            try
            {
                // Only work with local deal upload paths
                if (!imagePath.StartsWith(
                        "/uploads/deals/",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                var relativePath = imagePath
                    .TrimStart('/')
                    .Replace(
                        '/',
                        Path.DirectorySeparatorChar
                    );

                var fullPath = Path.Combine(
                    _environment.WebRootPath,
                    relativePath
                );

                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
            catch
            {
                // Do not fail API if image deletion fails.
                // Database operation has already completed.
            }
        }

        // ============================================================
        // MAP DEAL
        // ============================================================

        private static DealResponseDto MapDeal(Deal deal)
        {
            return new DealResponseDto
            {
                Id = deal.Id,

                Title = deal.Title,

                Description = deal.Description,

                Image = deal.Image,

                Price = deal.Price,

                OriginalPrice =
                    deal.OriginalPrice,

                Badge = deal.Badge,

                Featured = deal.Featured,

                IsActive = deal.IsActive,

                DisplayOrder =
                    deal.DisplayOrder,

                StartTime =
                    deal.StartTime,

                EndTime =
                    deal.EndTime,

                DealItems = deal.DealItems
                    .OrderBy(x => x.DisplayOrder)
                    .Select(x => new DealItemResponseDto
                    {
                        Id = x.Id,

                        MenuItemId =
                            x.MenuItemId,

                        Quantity =
                            x.Quantity,

                        DisplayOrder =
                            x.DisplayOrder,

                        MenuItemName =
                            x.MenuItem != null
                                ? x.MenuItem.Name
                                : null,

                        MenuItemVariantName =
                            x.MenuItemVariant != null
                                ? x.MenuItemVariant.Name
                                : null
                    })
                    .ToList()
            };
        }
    }
}