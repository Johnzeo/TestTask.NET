
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestTask.Api.DTOs;
using TestTask.Core.Entities;
using TestTask.Infrastructure.Data;

namespace TestTask.Api.Controllers;

[ApiController]
[Route("")]
public class VideosController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public VideosController(
        ApplicationDbContext context,
        IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(500_000_000)]
    public async Task<ActionResult<VideoResponse>> Upload(
        IFormFile file,
        [FromForm(Name = "company_name")] string companyName)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Video file is required.");
        }

        if (string.IsNullOrWhiteSpace(companyName))
        {
            return BadRequest("Company name is required.");
        }

        if (!string.Equals(
                Path.GetExtension(file.FileName),
                ".webm",
                StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Only WEBM files are allowed.");
        }

        companyName = companyName.Trim();

        var uploadsFolder = Path.Combine(
            _environment.ContentRootPath,
            "uploads");

        Directory.CreateDirectory(uploadsFolder);

        var existingVideo = await _context.Videos
            .FirstOrDefaultAsync(v => v.CompanyName == companyName);

        var fileName = $"{Guid.NewGuid()}.webm";
        var filePath = Path.Combine(uploadsFolder, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var videoUrl = $"{Request.Scheme}://{Request.Host}/media/{fileName}";

        if (existingVideo == null)
        {
            var video = new Video
            {
                CompanyName = companyName,
                FilePath = filePath,
                VideoUrl = videoUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Videos.Add(video);
        }
        else
        {
            var oldFilePath = existingVideo.FilePath;

            existingVideo.FilePath = filePath;
            existingVideo.VideoUrl = videoUrl;
            existingVideo.UpdatedAt = DateTime.UtcNow;

            if (System.IO.File.Exists(oldFilePath))
            {
                System.IO.File.Delete(oldFilePath);
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new VideoResponse
        {
            CompanyName = companyName,
            VideoUrl = videoUrl
        });
    }

    [HttpGet("media/{fileName}")]
    public IActionResult GetMedia(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) ||
            fileName != Path.GetFileName(fileName))
        {
            return BadRequest("Invalid file name.");
        }

        var uploadsFolder = Path.Combine(
            _environment.ContentRootPath,
            "uploads");

        var filePath = Path.Combine(uploadsFolder, fileName);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        return PhysicalFile(
            filePath,
            "video/webm",
            enableRangeProcessing: true);
    }

    [HttpGet("get_video_url")]
    public async Task<ActionResult<VideoResponse>> GetVideoUrl(
        [FromQuery(Name = "company_name")] string companyName)
    {
        if (string.IsNullOrWhiteSpace(companyName))
        {
            return BadRequest("Company name is required.");
        }

        companyName = companyName.Trim();

        var video = await _context.Videos
            .FirstOrDefaultAsync(v => v.CompanyName == companyName);

        if (video == null)
        {
            return NotFound("Video for this company was not found.");
        }

        return Ok(new VideoResponse
        {
            CompanyName = video.CompanyName,
            VideoUrl = video.VideoUrl
        });
    }

}
