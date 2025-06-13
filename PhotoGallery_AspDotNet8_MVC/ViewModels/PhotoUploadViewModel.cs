namespace PhotoGallery_AspDotNet8_MVC.ViewModels;

public class PhotoUploadViewModel
{
    [Required]
    public string Title { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public string Tags { get; set; } // comma‑separated

    [Required]
    public IFormFile PhotoFile { get; set; }
}

