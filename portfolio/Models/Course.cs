namespace portfolio.Models;

// One Course object holds one row of portfolio.course.
public class Course
{
    public long CourseId { get; set; }
    public string Code { get; set; } = "";
    public string Title { get; set; } = "";
    public string? Description { get; set; }
}