using Microsoft.AspNetCore.Mvc;
using portfolio.Data;

namespace portfolio.Controllers;

public class CoursesController : Controller
{
    private readonly CourseRepository _courses;

    public CoursesController(CourseRepository courses)
    {
        _courses = courses;
    }

    // GET /Courses : the list of courses.
    public async Task<IActionResult> Index()
    {
        var courses = await _courses.GetAllAsync();
        return View(courses);
    }
}