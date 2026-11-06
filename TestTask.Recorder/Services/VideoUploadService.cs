
using System.Net.Http.Headers;
using System.Text.Json;

namespace TestTask.Recorder.Services;

public class VideoUploadService
{
    private readonly HttpClient _httpClient;

    public VideoUploadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> UploadAsync(
        string serverUrl,
        string companyName,
        string videoPath)
    {
        if (!File.Exists(videoPath))
        {
            throw new FileNotFoundException(
                "Video file was not found.",
                videoPath);
        }

        using var form = new MultipartFormDataContent();

        await using var fileStream = File.OpenRead(videoPath);

        using var fileContent = new StreamContent(fileStream);

        fileContent.Headers.ContentType =
            new MediaTypeHeaderValue("video/webm");

        form.Add(fileContent, "file", Path.GetFileName(videoPath));
        form.Add(
            new StringContent(companyName),
            "company_name");

        var uploadUrl = $"{serverUrl.TrimEnd('/')}/upload";

        using var response = await _httpClient.PostAsync(
            uploadUrl,
            form);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(json);

        if (!document.RootElement.TryGetProperty(
                "video_url",
                out var videoUrlElement))
        {
            throw new Exception(
                "API response does not contain video_url.");
        }

        return videoUrlElement.GetString()
            ?? throw new Exception("Video URL is empty.");
    }
}
