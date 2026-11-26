using Npgsql;
using portfolio.Models;

namespace portfolio.Data;

// Every SQL statement about projects lives in this one class.
public class ProjectRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public ProjectRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    // Copies the current row of the reader into a Project.
    // Positions 0 to 6 match the column order of both SELECTs below.
    private static Project ReadProject(NpgsqlDataReader reader)
    {
        return new Project
        {
            ProjectId = reader.GetInt64(0),
            CourseId = reader.GetInt64(1),
            CourseCode = reader.GetString(2),
            Title = reader.GetString(3),
            Description = reader.IsDBNull(4) ? null : reader.GetString(4),
            YearMade = reader.GetInt32(5),
            VideoUrl = reader.IsDBNull(6) ? null : reader.GetString(6)
        };
    }

    // READ: every project with its course code, sorted by course then title.
    public async Task<List<Project>> GetAllAsync()
    {
        const string sql =
            "SELECT p.project_id, p.course_id, c.code, p.title, p.description, p.year_made, p.video_url " +
            "FROM portfolio.project p " +
            "JOIN portfolio.course c ON c.course_id = p.course_id " +
            "ORDER BY c.code, p.title;";

        var projects = new List<Project>();

        await using var command = _dataSource.CreateCommand(sql);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            projects.Add(ReadProject(reader));
        }

        return projects;
    }

    // READ: one project, or null when no project has that id.
    public async Task<Project?> GetByIdAsync(long id)
    {
        const string sql =
            "SELECT p.project_id, p.course_id, c.code, p.title, p.description, p.year_made, p.video_url " +
            "FROM portfolio.project p " +
            "JOIN portfolio.course c ON c.course_id = p.course_id " +
            "WHERE p.project_id = @id;";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? ReadProject(reader) : null;
    }

    // CREATE: add a project and all its images in ONE transaction.
    // If any image is refused, the project is not saved either.
    public async Task<long> AddWithImagesAsync(Project project, List<string> imageUrls)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            long projectId;
            await using (var insert = new NpgsqlCommand(
                "INSERT INTO portfolio.project (course_id, title, description, year_made, video_url) " +
                "VALUES (@course_id, @title, @description, @year_made, @video_url) RETURNING project_id;",
                connection, transaction))
            {
                insert.Parameters.AddWithValue("course_id", project.CourseId);
                insert.Parameters.AddWithValue("title", project.Title);
                insert.Parameters.AddWithValue("description", (object?)project.Description ?? DBNull.Value);
                insert.Parameters.AddWithValue("year_made", project.YearMade);
                insert.Parameters.AddWithValue("video_url", (object?)project.VideoUrl ?? DBNull.Value);
                projectId = Convert.ToInt64(await insert.ExecuteScalarAsync());
            }

            foreach (var url in imageUrls)
            {
                await using var insertImage = new NpgsqlCommand(
                    "INSERT INTO portfolio.project_image (project_id, image_url) VALUES (@project_id, @image_url);",
                    connection, transaction);
                insertImage.Parameters.AddWithValue("project_id", projectId);
                insertImage.Parameters.AddWithValue("image_url", url);
                await insertImage.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
            return projectId;
        }
        catch
        {
            await transaction.RollbackAsync();   // undo everything, then let the controller show a message
            throw;
        }
    }
}