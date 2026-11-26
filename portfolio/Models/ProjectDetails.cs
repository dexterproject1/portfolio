namespace portfolio.Models;

// The project page shows two things: the project, and its images.
public class ProjectDetails
{
    public Project Project { get; set; } = new();
    public List<ProjectImage> Images { get; set; } = new();
}