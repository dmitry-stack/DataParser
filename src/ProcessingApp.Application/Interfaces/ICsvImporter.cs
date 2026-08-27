using System.Globalization;

namespace ProcessingApp.Application.Interfaces;

public interface ICsvImporter
{
    Task ImportCsvAsync(string filePath, CultureInfo culture, CancellationToken token = default);
}