namespace PhotoGallery_AspDotNet8_MVC.DTOs;
public class PhotoListDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string ThumbnailBase64 { get; set; }
    public string UploadedDateShort { get; set; }
}
