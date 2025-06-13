namespace PhotoGallery_AspDotNet8_MVC.DTOs
{
    public class PhotoDetailDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Owner { get; set; }
        public string Location { get; set; }
        public DateTime UploadedDate { get; set; }
        public List<string> Tags { get; set; }
        public string PhotoBase64 { get; set; }
    }
}
