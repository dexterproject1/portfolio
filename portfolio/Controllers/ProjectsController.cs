using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Npgsql;
using portfolio.Data;
using portfolio.Models;

namespace portfolio.Controllers;

public class ProjectsController : Controller
{
    // Each image URL must be an https address with a host and a path.
    private const string ImageUrlPattern = @"^https://[^\s/]+/\S+$";

    private readonly ProjectRepository _projects;
    private readonly ImageRepository _images;
    private readonly CourseRepository _courses;

    public ProjectsController(ProjectRepository projects, ImageRepository images, CourseRepository courses)
    {
        _projects = projects;
        _images = images;
        _courses = courses;
    }

    // Fills the course dropdown from the database.
    private async Task FillCoursesAsync()
    {
        ViewBag.Courses = new SelectList(await _courses.GetAllAsync(), "CourseId", "Code");
    }

    // Splits the text box into one URL per line. Duplicates are kept on purpose.
    private static List<string> ParseUrls(string? text)
    {
        return (text ?? "")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => line != "")
            .ToList();
    }

    // GET /Projects : the list of projects.
    public async Task<IActionResult> Index()
    {
        var projects = await _projects.GetAllAsync();
        return View(projects);
    }

    // GET /Projects/Details/5 : one project with its images and video.
    public async Task<IActionResult> Details(long id)
    {
        var project = await _projects.GetByIdAsync(id);
        if (project == null)
        {
            return NotFound();
        }

        var details = new ProjectDetails
        {
            Project = project,
            Images = await _images.GetByProjectAsync(id)
        };
        return View(details);
    }

    // GET /Projects/Create : an empty form.
    public async Task<IActionResult> Create()
    {
        await FillCoursesAsync();
        return View(new ProjectForm());
    }

    // POST /Projects/Create : the form was submitted.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProjectForm form)
    {
        var urls = ParseUrls(form.ImageUrls);

        // Check every image URL, line by line.
        foreach (var url in urls)
        {
            if (!Regex.IsMatch(url, ImageUrlPattern))
            {
                ModelState.AddModelError("ImageUrls", $"Not a valid https address: {url}");
            }
        }

        if (!ModelState.IsValid)
        {
            await FillCoursesAsync();
            return View(form);
        }

        var project = new Project
        {
            CourseId = form.CourseId!.Value,
            Title = form.Title,
            Description = form.Description,
            YearMade = form.YearMade,
            VideoUrl = form.VideoUrl
        };

        try
        {
            await _projects.AddWithImagesAsync(project, urls);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            ModelState.AddModelError("ImageUrls", "The same image address appears twice. Nothing was saved.");
            await FillCoursesAsync();
            return View(form);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.CheckViolation)
        {
            ModelState.AddModelError("YearMade", "The database refused this year. Nothing was saved.");
            await FillCoursesAsync();
            return View(form);
        }

        return RedirectToAction(nameof(Index));
    }
}