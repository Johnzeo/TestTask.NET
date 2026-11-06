
using TestTask.Recorder.Models;

namespace TestTask.Recorder.Services;

public class CsvReaderService
{
    public List<CompanyData> Read(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                $"CSV file not found: {filePath}");
        }

        var lines = File.ReadAllLines(filePath);

        if (lines.Length < 2)
        {
            return new List<CompanyData>();
        }

        var companies = new List<CompanyData>();

        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var values = line.Split(';');

            if (values.Length < 8)
            {
                continue;
            }

            companies.Add(new CompanyData
            {
                Url = values[0].Trim(),
                CompanyName = values[1].Trim(),
                Language = values[2].Trim(),
                Country = values[3].Trim(),
                Postcode = values[4].Trim(),
                City = values[5].Trim(),
                Street = values[6].Trim(),
                HouseNo = values[7].Trim()
            });
        }

        return companies;
    }
}
