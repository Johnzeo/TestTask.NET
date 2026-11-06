
using System.Text.Json.Serialization;

namespace TestTask.Api.DTOs;

public class VideoResponse
{
    [JsonPropertyName("company_name")]
    public string CompanyName { get; set; } = string.Empty;

    [JsonPropertyName("video_url")]
    public string VideoUrl { get; set; } = string.Empty;
}
