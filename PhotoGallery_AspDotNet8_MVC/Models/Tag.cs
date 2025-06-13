namespace PhotoGallery_AspDotNet8_MVC.Models;
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Photo> Photos { get; set; } = new();
}
