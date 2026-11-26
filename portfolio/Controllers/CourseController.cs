using Microsoft.AspNetCore.Mvc;
using Npgsql;
using portfolio.Data;
using portfolio.Models;

namespace portfolio.Controllers;

public class CoursesController : Controller
{
    private readonly CourseRepository _courses;

    public CoursesController(CourseRepository courses)
    {
        _courses = courses;
    }

    // GET /Courses : the list of courses. Everyone can see it.
    public async Task<IActionResult> Index()
    {
        var courses = await _courses.GetAllAsync();
        return View(courses);
    }

    // GET /Courses/Create : an empty form.
    [EditorOnly]
    public IActionResult Create()
    {
        return View(new Course());
    }

    // POST /Courses/Create : the form was submitted.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [EditorOnly]
    public async Task<IActionResult> Create(Course course)
    {
        if (!ModelState.IsValid)
        {
            return View(course);
        }

        try
        {
            await _courses.AddAsync(course);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            ModelState.AddModelError("Code", "Another course already uses this code.");
            return View(course);
        }

        return RedirectToAction(nameof(Index));
    }

    // GET /Courses/Edit/5 : the form, filled with course 5.
    [EditorOnly]
    public async Task<IActionResult> Edit(long id)
    {
        var course = await _courses.GetByIdAsync(id);
        if (course == null)
        {
            return NotFound();
        }
        return View(course);
    }

    // POST /Courses/Edit/5 : the changed form was submitted.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [EditorOnly]
    public async Task<IActionResult> Edit(Course course)
    {
        if (!ModelState.IsValid)
        {
            return View(course);
        }

        try
        {
            await _courses.UpdateAsync(course);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            ModelState.AddModelError("Code", "Another course already uses this code.");
            return View(course);
        }

        return RedirectToAction(nameof(Index));
    }

    // GET /Courses/Delete/5 : ask "are you sure?"
    [EditorOnly]
    public async Task<IActionResult> Delete(long id)
    {
        var course = await _courses.GetByIdAsync(id);
        if (course == null)
        {
            return NotFound();
        }
        return View(course);
    }

    // POST /Courses/Delete/5 : the delete was confirmed.
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [EditorOnly]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        try
        {
            await _courses.DeleteAsync(id);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            var course = await _courses.GetByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            ViewBag.Error = "This course still has projects, so the database will not delete it. Delete its projects first.";
            return View("Delete", course);
        }

        return RedirectToAction(nameof(Index));
    }
}