using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzastaAdminBackend.Data;
using PizzastaAdminBackend.Models;

namespace PizzastaAdminBackend.Controllers
{
    [Authorize]
    public class WebsiteSettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public WebsiteSettingsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public IActionResult GetSettings()
        {
            var data = _context.WebsiteSettings.FirstOrDefault();
            return Json(data);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrUpdate(WebsiteSettingsDto model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please fill out the form correctly."
                });
            }

            try
            {
                var settings = await _context.WebsiteSettings
                    .FirstOrDefaultAsync();

                var uploadsFolder = Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    "website"
                );

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // ============================================================
                // CREATE
                // ============================================================

                if (settings == null)
                {
                    settings = new WebsiteSettings
                    {
                        Id = Guid.NewGuid(),
                        FacebookUrl = model.FacebookUrl.Trim(),
                        InstagramUrl = model.InstagramUrl.Trim(),
                        WhatsappUrl = model.WhatsappUrl.Trim(),
                        WhatsappMessage = model.WhatsappMessage.Trim(),
                        Email = model.Email.Trim(),
                        Logo = string.Empty
                    };

                    // --------------------------------------------------------
                    // Upload logo
                    // --------------------------------------------------------

                    if (model.LogoFile != null)
                    {
                        var extension = Path.GetExtension(model.LogoFile.FileName);

                        var fileName =
                            $"{Guid.NewGuid()}{extension}";

                        var filePath = Path.Combine(
                            uploadsFolder,
                            fileName
                        );

                        await using var stream =
                            new FileStream(filePath, FileMode.Create);

                        await model.LogoFile.CopyToAsync(stream);

                        settings.Logo =
                            $"/uploads/website/{fileName}";
                    }

                    _context.WebsiteSettings.Add(settings);

                    await _context.SaveChangesAsync();

                    return Json(new
                    {
                        success = true,
                        message = "Website settings created successfully.",
                        data = settings
                    });
                }

                // ============================================================
                // UPDATE
                // ============================================================

                settings.FacebookUrl = model.FacebookUrl.Trim();
                settings.InstagramUrl = model.InstagramUrl.Trim();
                settings.WhatsappUrl = model.WhatsappUrl.Trim();
                settings.WhatsappMessage = model.WhatsappMessage.Trim();
                settings.Email = model.Email.Trim();

                // ------------------------------------------------------------
                // Replace logo only if a new file was uploaded
                // ------------------------------------------------------------

                if (model.LogoFile != null)
                {
                    // Delete old logo
                    if (!string.IsNullOrWhiteSpace(settings.Logo))
                    {
                        var oldLogoPath = settings.Logo
                            .Replace("/", Path.DirectorySeparatorChar.ToString())
                            .TrimStart(Path.DirectorySeparatorChar);

                        var fullOldLogoPath = Path.Combine(
                            _environment.WebRootPath,
                            oldLogoPath
                        );

                        if (System.IO.File.Exists(fullOldLogoPath))
                        {
                            System.IO.File.Delete(fullOldLogoPath);
                        }
                    }

                    // Save new logo
                    var extension =
                        Path.GetExtension(model.LogoFile.FileName);

                    var fileName =
                        $"{Guid.NewGuid()}{extension}";

                    var newFilePath = Path.Combine(
                        uploadsFolder,
                        fileName
                    );

                    await using var stream =
                        new FileStream(newFilePath, FileMode.Create);

                    await model.LogoFile.CopyToAsync(stream);

                    settings.Logo =
                        $"/uploads/website/{fileName}";
                }

                _context.WebsiteSettings.Update(settings);

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Website settings updated successfully.",
                    data = settings
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Something went wrong while saving website settings.",
                    error = ex.Message
                });
            }
        }
    }
}
