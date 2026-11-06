
using Microsoft.Playwright;
using TestTask.Recorder.Helpers;
using TestTask.Recorder.Models;

namespace TestTask.Recorder.Services;

public class WebsiteAutomationService
{
    public async Task ExecuteAsync(
        IPage page,
        CompanyData company)
    {
        await page.GotoAsync(
            company.Url,
            new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 60000
            });

        await page.WaitForTimeoutAsync(2000);

        var searchInput = page.Locator("textarea[name='q']");

        await searchInput.WaitForAsync(
            new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 30000
            });

        var searchBox = await searchInput.BoundingBoxAsync();

        if (searchBox == null)
        {
            throw new Exception("Search field position was not found.");
        }

        await MouseHelper.ClickSmoothlyAsync(
            page,
            (float)(searchBox.X + searchBox.Width / 2),
            (float)(searchBox.Y + searchBox.Height / 2));

        var searchText = $"Magento developer in {company.City}";

        foreach (var character in searchText)
        {
            await searchInput.PressSequentiallyAsync(
                character.ToString(),
                new LocatorPressSequentiallyOptions
                {
                    Delay = Random.Shared.Next(80, 180)
                });
        }

        await page.WaitForTimeoutAsync(1000);

        await searchInput.PressAsync("Enter");

        await page.WaitForTimeoutAsync(5000);

        var firstResult = page.Locator(
            "a[data-testid='result-title-a']").First;

        await firstResult.WaitForAsync(
            new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 30000
            });

        var resultBox = await firstResult.BoundingBoxAsync();

        if (resultBox == null)
        {
            throw new Exception("First search result position was not found.");
        }

        await MouseHelper.ClickSmoothlyAsync(
            page,
            (float)(resultBox.X + resultBox.Width / 2),
            (float)(resultBox.Y + resultBox.Height / 2));

        await page.WaitForTimeoutAsync(5000);
    }
}
