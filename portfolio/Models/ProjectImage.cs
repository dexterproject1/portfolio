namespace portfolio.Models;

// One ProjectImage object holds one row of portfolio.project_image.
public class ProjectImage
{
    public long ImageId { get; set; }
    public long ProjectId { get; set; }
    public string ImageUrl { get; set; } = "";
}