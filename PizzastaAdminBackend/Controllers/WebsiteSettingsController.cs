using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzastaAdminBackend.Data;
using PizzastaAdminBackend.Models;

namespace PizzastaAdminBackend.Controllers
{
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
            if(data == null)
            {
                return Json(new { success = false, data = new WebsiteSettings() });
            }
            return Json(new { success = true, data });
        }

        [Authorize]
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

                // ============================================================
                // UPLOAD FOLDERS
                // ============================================================

                var websiteFolder = Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    "website"
                );

                var sliderFolder = Path.Combine(
                    websiteFolder,
                    "sliders"
                );

                var videoFolder = Path.Combine(
                    websiteFolder,
                    "videos"
                );

                if (!Directory.Exists(websiteFolder))
                {
                    Directory.CreateDirectory(websiteFolder);
                }

                if (!Directory.Exists(sliderFolder))
                {
                    Directory.CreateDirectory(sliderFolder);
                }

                if (!Directory.Exists(videoFolder))
                {
                    Directory.CreateDirectory(videoFolder);
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

                        Logo = string.Empty,
                        SliderImages = "[]",
                        Video = string.Empty
                    };

                    // ========================================================
                    // LOGO
                    // ========================================================

                    if (model.LogoFile != null)
                    {
                        var extension = Path.GetExtension(
                            model.LogoFile.FileName
                        );

                        var fileName = $"{Guid.NewGuid()}{extension}";

                        var filePath = Path.Combine(
                            websiteFolder,
                            fileName
                        );

                        await using var stream = new FileStream(
                            filePath,
                            FileMode.Create
                        );

                        await model.LogoFile.CopyToAsync(stream);

                        settings.Logo =
                            $"/uploads/website/{fileName}";
                    }

                    // ========================================================
                    // SLIDER IMAGES
                    // ========================================================

                    var sliderImagePaths = new List<string>();

                    if (model.SliderImageFiles != null &&
                        model.SliderImageFiles.Count > 0)
                    {
                        foreach (var file in model.SliderImageFiles)
                        {
                            if (file == null || file.Length == 0)
                            {
                                continue;
                            }

                            var extension = Path.GetExtension(
                                file.FileName
                            );

                            var fileName =
                                $"{Guid.NewGuid()}{extension}";

                            var filePath = Path.Combine(
                                sliderFolder,
                                fileName
                            );

                            await using var stream = new FileStream(
                                filePath,
                                FileMode.Create
                            );

                            await file.CopyToAsync(stream);

                            sliderImagePaths.Add(
                                $"/uploads/website/sliders/{fileName}"
                            );
                        }
                    }

                    settings.SliderImages =
                        System.Text.Json.JsonSerializer.Serialize(
                            sliderImagePaths
                        );

                    // ========================================================
                    // VIDEO
                    // ========================================================

                    if (model.VideoFile != null &&
                        model.VideoFile.Length > 0)
                    {
                        var extension =
                            Path.GetExtension(model.VideoFile.FileName);

                        var fileName =
                            $"{Guid.NewGuid()}{extension}";

                        var filePath = Path.Combine(
                            videoFolder,
                            fileName
                        );

                        await using var stream = new FileStream(
                            filePath,
                            FileMode.Create
                        );

                        await model.VideoFile.CopyToAsync(stream);

                        settings.Video =
                            $"/uploads/website/videos/{fileName}";
                    }

                    // ========================================================
                    // SAVE
                    // ========================================================

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
                // UPDATE BASIC SETTINGS
                // ============================================================

                settings.FacebookUrl = model.FacebookUrl.Trim();
                settings.InstagramUrl = model.InstagramUrl.Trim();
                settings.WhatsappUrl = model.WhatsappUrl.Trim();
                settings.WhatsappMessage = model.WhatsappMessage.Trim();
                settings.Email = model.Email.Trim();

                // ============================================================
                // LOGO
                // ============================================================

                if (model.LogoFile != null &&
                    model.LogoFile.Length > 0)
                {
                    // --------------------------------------------------------
                    // Delete old logo
                    // --------------------------------------------------------

                    if (!string.IsNullOrWhiteSpace(settings.Logo))
                    {
                        var oldLogoPath = settings.Logo
                            .Replace(
                                "/",
                                Path.DirectorySeparatorChar.ToString()
                            )
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

                    // --------------------------------------------------------
                    // Save new logo
                    // --------------------------------------------------------

                    var extension = Path.GetExtension(
                        model.LogoFile.FileName
                    );

                    var fileName =
                        $"{Guid.NewGuid()}{extension}";

                    var newFilePath = Path.Combine(
                        websiteFolder,
                        fileName
                    );

                    await using var stream = new FileStream(
                        newFilePath,
                        FileMode.Create
                    );

                    await model.LogoFile.CopyToAsync(stream);

                    settings.Logo =
                        $"/uploads/website/{fileName}";
                }

                // ============================================================
                // SLIDER IMAGES
                // ============================================================

                var existingSliderImages =
                    new List<string>();

                if (!string.IsNullOrWhiteSpace(settings.SliderImages))
                {
                    try
                    {
                        existingSliderImages =
                            System.Text.Json.JsonSerializer
                                .Deserialize<List<string>>(
                                    settings.SliderImages
                                ) ?? new List<string>();
                    }
                    catch
                    {
                        existingSliderImages =
                            new List<string>();
                    }
                }

                // ------------------------------------------------------------
                // Existing images frontend wants to KEEP
                // ------------------------------------------------------------

                var imagesToKeep =
                    new List<string>();

                if (!string.IsNullOrWhiteSpace(model.SliderImages))
                {
                    try
                    {
                        imagesToKeep =
                            System.Text.Json.JsonSerializer
                                .Deserialize<List<string>>(
                                    model.SliderImages
                                ) ?? new List<string>();
                    }
                    catch
                    {
                        imagesToKeep =
                            new List<string>();
                    }
                }

                // ------------------------------------------------------------
                // Delete slider images removed from frontend
                // ------------------------------------------------------------

                foreach (var oldImage in existingSliderImages)
                {
                    if (!imagesToKeep.Contains(oldImage))
                    {
                        var oldImagePath = oldImage
                            .Replace(
                                "/",
                                Path.DirectorySeparatorChar.ToString()
                            )
                            .TrimStart(Path.DirectorySeparatorChar);

                        var fullOldImagePath = Path.Combine(
                            _environment.WebRootPath,
                            oldImagePath
                        );

                        if (System.IO.File.Exists(fullOldImagePath))
                        {
                            System.IO.File.Delete(
                                fullOldImagePath
                            );
                        }
                    }
                }

                // ------------------------------------------------------------
                // Upload NEW slider images
                // ------------------------------------------------------------

                if (model.SliderImageFiles != null &&
                    model.SliderImageFiles.Count > 0)
                {
                    foreach (var file in model.SliderImageFiles)
                    {
                        if (file == null || file.Length == 0)
                        {
                            continue;
                        }

                        var extension =
                            Path.GetExtension(file.FileName);

                        var fileName =
                            $"{Guid.NewGuid()}{extension}";

                        var filePath = Path.Combine(
                            sliderFolder,
                            fileName
                        );

                        await using var stream = new FileStream(
                            filePath,
                            FileMode.Create
                        );

                        await file.CopyToAsync(stream);

                        imagesToKeep.Add(
                            $"/uploads/website/sliders/{fileName}"
                        );
                    }
                }

                // ------------------------------------------------------------
                // Save slider images
                // ------------------------------------------------------------

                settings.SliderImages =
                    System.Text.Json.JsonSerializer.Serialize(
                        imagesToKeep
                    );

                // ============================================================
                // VIDEO
                // ============================================================

                /*
                 * Video behavior:
                 *
                 * 1. New VideoFile exists:
                 *      - Upload new video
                 *      - Delete old video
                 *      - Save new video path
                 *
                 * 2. No VideoFile + model.Video contains existing path:
                 *      - Keep existing video
                 *
                 * 3. No VideoFile + model.Video is empty:
                 *      - Delete existing video
                 *      - Clear database value
                 */

                var oldVideo = settings.Video;

                var hasNewVideo =
                    model.VideoFile != null &&
                    model.VideoFile.Length > 0;

                var wantsToKeepExistingVideo =
                    !string.IsNullOrWhiteSpace(model.Video);

                // ------------------------------------------------------------
                // NEW VIDEO UPLOAD
                // ------------------------------------------------------------
                if (model.RemoveVideo)
                {
                    if (!string.IsNullOrWhiteSpace(oldVideo))
                    {
                        var oldVideoPath = oldVideo
                            .Replace(
                                "/",
                                Path.DirectorySeparatorChar.ToString()
                            )
                            .TrimStart(Path.DirectorySeparatorChar);

                        var fullOldVideoPath = Path.Combine(
                            _environment.WebRootPath,
                            oldVideoPath
                        );

                        if (System.IO.File.Exists(fullOldVideoPath))
                        {
                            System.IO.File.Delete(fullOldVideoPath);
                        }
                    }

                    settings.Video = string.Empty;
                }
                else if (hasNewVideo)
                {
                    // --------------------------------------------------------
                    // Upload the new video FIRST
                    // --------------------------------------------------------

                    var extension =
                        Path.GetExtension(model.VideoFile!.FileName);

                    var fileName =
                        $"{Guid.NewGuid()}{extension}";

                    var newVideoPath = Path.Combine(
                        videoFolder,
                        fileName
                    );

                    await using (var stream = new FileStream(
                        newVideoPath,
                        FileMode.Create
                    ))
                    {
                        await model.VideoFile.CopyToAsync(stream);
                    }

                    var newVideoUrl =
                        $"/uploads/website/videos/{fileName}";

                    // --------------------------------------------------------
                    // Delete old video AFTER new upload succeeds
                    // --------------------------------------------------------

                    if (!string.IsNullOrWhiteSpace(oldVideo))
                    {
                        var oldVideoPath = oldVideo
                            .Replace(
                                "/",
                                Path.DirectorySeparatorChar.ToString()
                            )
                            .TrimStart(Path.DirectorySeparatorChar);

                        var fullOldVideoPath = Path.Combine(
                            _environment.WebRootPath,
                            oldVideoPath
                        );

                        if (System.IO.File.Exists(fullOldVideoPath))
                        {
                            System.IO.File.Delete(
                                fullOldVideoPath
                            );
                        }
                    }

                    // --------------------------------------------------------
                    // Save new video path
                    // --------------------------------------------------------

                    settings.Video = newVideoUrl;
                }

                // ------------------------------------------------------------
                // DELETE EXISTING VIDEO
                // ------------------------------------------------------------

                else if (!wantsToKeepExistingVideo &&
                         !string.IsNullOrWhiteSpace(settings.Video))
                {
                    var oldVideoPath = settings.Video
                        .Replace(
                            "/",
                            Path.DirectorySeparatorChar.ToString()
                        )
                        .TrimStart(Path.DirectorySeparatorChar);

                    var fullOldVideoPath = Path.Combine(
                        _environment.WebRootPath,
                        oldVideoPath
                    );

                    if (System.IO.File.Exists(fullOldVideoPath))
                    {
                        System.IO.File.Delete(
                            fullOldVideoPath
                        );
                    }

                    settings.Video = string.Empty;
                }

                // ============================================================
                // SAVE DATABASE
                // ============================================================

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
