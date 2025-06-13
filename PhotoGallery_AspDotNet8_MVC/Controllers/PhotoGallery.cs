namespace PhotoGallery_AspDotNet8_MVC.Controllers;

    public class PhotoGallery : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public PhotoGallery(ApplicationDbContext ctx, UserManager<ApplicationUser> um)
        {
            _dbContext = ctx;
            _userManager = um;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var photos = await _dbContext.Photos
                .Include(p => p.Owner)
                .OrderByDescending(p => p.UploadedDate)
                .ToListAsync();
            return View(photos);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var p = await _dbContext.Photos
                .Include(x => x.Owner)
                .Include(x => x.Tags)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (p == null) return NotFound();
            return View(p);
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
                ImageMimeType = vm.PhotoFile.ContentType
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
            var photo = await _dbContext.Photos.FindAsync(id);
            if (photo == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (photo.OwnerId != user.Id && !User.IsInRole("Admin"))
                return Forbid();

            _dbContext.Photos.Remove(photo);
            await _dbContext.SaveChangesAsync();

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
    }
