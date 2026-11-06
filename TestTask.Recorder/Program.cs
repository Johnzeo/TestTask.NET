
using TestTask.Recorder.Services;

var csvPath = Path.Combine(
    AppContext.BaseDirectory,
    "input.csv");

var outputDirectory = Path.Combine(
    AppContext.BaseDirectory,
    "videos");

string? serverUrl = null;
var viewSeconds = 0;

for (var i = 0; i < args.Length; i++)
{
    if (args[i] == "--server" && i + 1 < args.Length)
    {
        serverUrl = args[++i];
    }
    else if (args[i] == "--view-seconds" && i + 1 < args.Length)
    {
        if (!int.TryParse(args[++i], out viewSeconds) || viewSeconds < 0)
        {
            Console.WriteLine("Invalid --view-seconds value.");
            return;
        }
    }
}

var csvReader = new CsvReaderService();
var recorder = new BrowserRecorderService();
var preview = new VideoPreviewService();

using var httpClient = new HttpClient();
var uploader = new VideoUploadService(httpClient);

List<TestTask.Recorder.Models.CompanyData> companies;

try
{
    companies = csvReader.Read(csvPath);
}
catch (Exception exception)
{
    Console.WriteLine($"CSV error: {exception.Message}");
    return;
}

if (companies.Count == 0)
{
    Console.WriteLine("No companies found in CSV.");
    return;
}

Console.WriteLine($"Found companies: {companies.Count}");

foreach (var company in companies)
{
    Console.WriteLine();
    Console.WriteLine($"Processing: {company.CompanyName}");

    try
    {
        var videoPath = await recorder.RecordAsync(
            company,
            outputDirectory);

        Console.WriteLine($"Video saved: {videoPath}");

        if (string.IsNullOrWhiteSpace(serverUrl))
        {
            continue;
        }

        try
        {
            var videoUrl = await uploader.UploadAsync(
                serverUrl,
                company.CompanyName,
                videoPath);

            Console.WriteLine($"Uploaded: {videoUrl}");

            await preview.ShowAsync(videoUrl, viewSeconds);
        }
        catch (Exception exception)
        {
            Console.WriteLine(
                $"Upload or preview failed: {exception.Message}");
        }
    }
    catch (Exception exception)
    {
        Console.WriteLine(
            $"Recording failed for {company.CompanyName}: {exception.Message}");
    }
}

Console.WriteLine();
Console.WriteLine("Processing completed.");
