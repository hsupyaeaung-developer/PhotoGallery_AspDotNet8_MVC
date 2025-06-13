namespace PhotoGallery_AspDotNet8_MVC.ViewModels;

    public class PhotoViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime UploadedDate { get; set; }

        [Required]
        public string OwnerId { get; set; }
        public ApplicationUser Owner { get; set; }

        [Required]
        public byte[] ImageData { get; set; }
        [Required]
        public string ImageMimeType { get; set; }

        public List<TagViewModel> Tags { get; set; } = new();
    }
