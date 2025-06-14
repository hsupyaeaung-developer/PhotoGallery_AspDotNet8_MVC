using Microsoft.EntityFrameworkCore;
using PhotoGallery_AspDotNet8_MVC.ViewModels;

namespace PhotoGallery_AspDotNet8_MVC.Controllers;

[Authorize]
public class PhotoGalleryController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public PhotoGalleryController(ApplicationDbContext ctx, UserManager<ApplicationUser> um)
    {
        _dbContext = ctx;
        _userManager = um;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(string? tagFilter)
    {
        var query = _dbContext.Photos
            .Include(p => p.Owner)
            .Include(p => p.Tags)
            .AsQueryable();

        if (!string.IsNullOrEmpty(tagFilter))
        {
            query = query.Where(p => p.Tags.Any(pt => pt.Name == tagFilter));
        }

        var photos = await query
            .OrderByDescending(p => p.Id)
            .Select(p => new PhotoViewModel
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Location = p.Location,
                UploadedDate = p.UploadedDate,
                OwnerId = p.OwnerId,
                Owner = p.Owner,
                ImageData = p.ImageData,
                ImageMimeType = p.ImageMimeType,
            }).ToListAsync();

        // Get all tags for filter dropdown or links
        var allTags = await _dbContext.Tags.Select(t => t.Name).ToListAsync();

        ViewBag.AllTags = allTags;
        ViewBag.SelectedTag = tagFilter;

        return View(photos);
    }

    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var PhotoInfo = await _dbContext.Photos
            .Include(x => x.Owner)
            .Include(x => x.Tags)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (PhotoInfo == null) return NotFound();

        var PhotoViewModel = BindPhotoDetailsViewModel(PhotoInfo);
        return View(PhotoViewModel);
    }

    private static PhotoViewModel BindPhotoDetailsViewModel(Photo? p)
    {
        var photoViewModel = new PhotoViewModel
        {
            Id = p.Id,
            Title = p.Title,
            Description = p.Description,
            Location = p.Location,
            UploadedDate = p.UploadedDate,
            OwnerId = p.OwnerId,
            Owner = p.Owner, // or map to a smaller owner VM if needed
            ImageData = p.ImageData,
            ImageMimeType = p.ImageMimeType,
            Tags = p.Tags.Select(t => new TagViewModel
            {
                Id = t.Id,
                Name = t.Name
            }).ToList()
        };
        return photoViewModel;
    }

    public IActionResult Upload() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(PhotoUploadViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        using var ms = new MemoryStream();
        await vm.PhotoFile.CopyToAsync(ms);

        var user = await _userManager.GetUserAsync(User);
        var photo = new Photo
        {
            Title = vm.Title,
            Description = vm.Description,
            Location = vm.Location,
            UploadedDate = DateTime.UtcNow,
            OwnerId = user.Id,
            ImageData = ms.ToArray(),
            ImageMimeType = vm.PhotoFile.ContentType,
            FileName = vm.PhotoFile.FileName
        };

        if (!string.IsNullOrEmpty(vm.Tags))
        {
            var names = vm.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim().ToLower()).Distinct();
            foreach (var n in names)
            {
                var tag = await _dbContext.Tags.FirstOrDefaultAsync(x => x.Name == n)
                    ?? new Tag { Name = n };
                photo.Tags.Add(tag);
            }
        }

        _dbContext.Photos.Add(photo);
        await _dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var photo = await _dbContext.Photos
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (photo == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (photo.OwnerId != user.Id && !User.IsInRole("Admin"))
            return Forbid();

        // Store tag IDs before deleting the photo
        var tagIds = photo.Tags.Select(t => t.Id).ToList();

        _dbContext.Photos.Remove(photo);
        await _dbContext.SaveChangesAsync();

        // Check and remove unused tags
        var unusedTags = await _dbContext.Tags
            .Where(t => tagIds.Contains(t.Id) && !t.Photos.Any())
            .ToListAsync();

        if (unusedTags.Any())
        {
            _dbContext.Tags.RemoveRange(unusedTags);
            await _dbContext.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }


    [AllowAnonymous]
    public async Task<IActionResult> GetImage(int id)
    {
        var p = await _dbContext.Photos.FindAsync(id);
        return p == null
            ? NotFound()
            : File(p.ImageData, p.ImageMimeType);
    }

    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> Download(int id)
    {
        var photo = await _dbContext.Photos.FindAsync(id);
        if (photo == null || photo.ImageData == null)
            return NotFound();

        var fileName = string.IsNullOrEmpty(photo.FileName) ? $"{photo.Title}.jpg" : photo.FileName;
        var contentType = string.IsNullOrEmpty(photo.ImageMimeType) ? "application/octet-stream" : photo.ImageMimeType;

        return File(photo.ImageData, contentType, fileName);
    }
}
