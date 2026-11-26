using System.ComponentModel.DataAnnotations;

namespace portfolio.Models;

// What the project form sends. The image URLs arrive as one text box, one URL per line.
public class ProjectForm
{
    public long ProjectId { get; set; }

    [Required(ErrorMessage = "Choose a course.")]
    public long? CourseId { get; set; }

    [Required(ErrorMessage = "Enter a title.")]
    [StringLength(100, ErrorMessage = "The title can be at most 100 characters.")]
    public string Title { get; set; } = "";

    [StringLength(1000, ErrorMessage = "The description can be at most 1000 characters.")]
    public string? Description { get; set; }

    // Wider than the database CHECK (2000 to 2100) on purpose, so the database can refuse a year.
    [Range(1990, 2100, ErrorMessage = "The year must be from 1990 to 2100.")]
    public int YearMade { get; set; } = DateTime.Now.Year;

    [RegularExpression(@"^https?://(www\.)?(youtube\.com/(watch\?v=|shorts/)|youtu\.be/)[A-Za-z0-9_-]{11}([&?].*)?$",
        ErrorMessage = "Enter a YouTube link, such as https://www.youtube.com/watch?v=abcdefghijk")]
    [StringLength(300, ErrorMessage = "The link can be at most 300 characters.")]
    public string? VideoUrl { get; set; }

    public string? ImageUrls { get; set; }
}