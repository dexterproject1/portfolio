using Npgsql;
using portfolio.Models;

namespace portfolio.Data;

// Every SQL statement about courses lives in this one class.
public class CourseRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public CourseRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    // Copies the current row of the reader into a Course.
    private static Course ReadCourse(NpgsqlDataReader reader)
    {
        return new Course
        {
            CourseId = reader.GetInt64(0),
            Code = reader.GetString(1),
            Title = reader.GetString(2),
            Description = reader.IsDBNull(3) ? null : reader.GetString(3)
        };
    }

    // READ: every course, sorted by code.
    public async Task<List<Course>> GetAllAsync()
    {
        const string sql = "SELECT course_id, code, title, description FROM portfolio.course ORDER BY code;";

        var courses = new List<Course>();

        await using var command = _dataSource.CreateCommand(sql);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            courses.Add(ReadCourse(reader));
        }

        return courses;
    }
}