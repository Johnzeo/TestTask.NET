
namespace TestTask.Core.Entities;

public class Video
{
    public int Id { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public string VideoUrl { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
