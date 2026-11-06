
using Microsoft.Playwright;
using TestTask.Recorder.Models;

namespace TestTask.Recorder.Services;

public class BrowserRecorderService
{
    private readonly WebsiteAutomationService _automationService;

    public BrowserRecorderService()
    {
        _automationService = new WebsiteAutomationService();
    }

    public async Task<string> RecordAsync(
        CompanyData company,
        string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);

        using var playwright = await Playwright.CreateAsync();

        var browser = await playwright.Chromium.LaunchAsync(
            new BrowserTypeLaunchOptions
            {
                Headless = false,
                SlowMo = 50
            });

        var context = await browser.NewContextAsync(
            new BrowserNewContextOptions
            {
                ViewportSize = new ViewportSize
                {
                    Width = 1920,
                    Height = 1080
                },
                RecordVideoDir = outputDirectory,
                RecordVideoSize = new RecordVideoSize
                {
                    Width = 1920,
                    Height = 1080
                }
            });

        var page = await context.NewPageAsync();

        try
        {
            await _automationService.ExecuteAsync(page, company);

            var video = page.Video;

            await context.CloseAsync();

            var videoPath = await video!.PathAsync();

            return videoPath;
        }
        finally
        {
            await browser.CloseAsync();
        }
    }
}
