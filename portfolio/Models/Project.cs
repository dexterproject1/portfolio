namespace portfolio.Models;

// One Project object holds one row of portfolio.project.
public class Project
{
    public long ProjectId { get; set; }
    public long CourseId { get; set; }
    public string CourseCode { get; set; } = "";   // comes from the join, shown on the list
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public int YearMade { get; set; }
    public string? VideoUrl { get; set; }
}