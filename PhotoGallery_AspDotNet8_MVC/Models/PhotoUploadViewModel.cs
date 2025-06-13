using System.ComponentModel.DataAnnotations;

namespace PhotoGallery_AspDotNet8_MVC.Models;

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

