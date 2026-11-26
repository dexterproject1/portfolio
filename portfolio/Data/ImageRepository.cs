using Npgsql;
using portfolio.Models;

namespace portfolio.Data;

// Every SQL statement about project images lives in this one class.
public class ImageRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public ImageRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    // Copies the current row of the reader into a ProjectImage.
    private static ProjectImage ReadImage(NpgsqlDataReader reader)
    {
        return new ProjectImage
        {
            ImageId = reader.GetInt64(0),
            ProjectId = reader.GetInt64(1),
            ImageUrl = reader.GetString(2)
        };
    }

    // READ: every image of one project, in the order they were added.
    public async Task<List<ProjectImage>> GetByProjectAsync(long projectId)
    {
        const string sql = "SELECT image_id, project_id, image_url FROM portfolio.project_image " +
                           "WHERE project_id = @project_id ORDER BY image_id;";

        var images = new List<ProjectImage>();

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("project_id", projectId);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            images.Add(ReadImage(reader));
        }

        return images;
    }
}