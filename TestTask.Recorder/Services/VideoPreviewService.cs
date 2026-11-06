
using Microsoft.Playwright;

namespace TestTask.Recorder.Services;

public class VideoPreviewService
{
    public async Task ShowAsync(
        string videoUrl,
        int viewSeconds)
    {
        using var playwright = await Playwright.CreateAsync();

        var browser = await playwright.Chromium.LaunchAsync(
            new BrowserTypeLaunchOptions
            {
                Headless = false
            });

        try
        {
            var page = await browser.NewPageAsync();

            await page.GotoAsync(
                videoUrl,
                new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded,
                    Timeout = 60000
                });

            if (viewSeconds > 0)
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(viewSeconds));
            }
            else
            {
                Console.WriteLine(
                    "Нажмите Enter, чтобы закрыть просмотр...");

                Console.ReadLine();
            }
        }
        finally
        {
            await browser.CloseAsync();
        }
    }
}
