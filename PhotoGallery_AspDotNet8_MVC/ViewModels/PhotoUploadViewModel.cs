namespace PhotoGallery_AspDotNet8_MVC.ViewModels;

public class PhotoUploadViewModel
{
    [Required]
    public string Title { get; set; }
    [Required]
    public string Description { get; set; }
    [Required]
    public string Location { get; set; }
    [Required]
    public string Tags { get; set; } // comma‑separated

    [Required]
    public IFormFile PhotoFile { get; set; }
}

