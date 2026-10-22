using Microsoft.EntityFrameworkCore;
using ProcessingApp.Infrastructure;
using ProcessingApp.Tests.Helpers;
using System.Globalization;

namespace ProcessingApp.Tests;

public class CsvImporterTests : IDisposable
{
    private static readonly CultureInfo RussianCulture = CultureInfo.GetCultureInfo("ru-RU");
    private readonly string _tempDirectory;

    public CsvImporterTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"csv_tests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDirectory);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, recursive: true);
            }
        }
        catch
        {
            
        }
    }

    [Fact]
    public async Task ImportCsvAsync_WithLargeDataset_SuccessfullyImportsAllBatches()
    {
   
        const int recordCount = 12_500;
        var csvPath = Path.Combine(_tempDirectory, "large_dataset.csv");
        await TestDataGenerator.CreateCsvFileAsync(csvPath, recordCount);

        var factory = new TestDbContextFactory();
        var importer = new CsvImporter(factory);

        await importer.ImportCsvAsync(csvPath, RussianCulture);

        using var context = factory.CreateDbContext();
        var totalInDb = await context.Records.CountAsync();

        Assert.Equal(recordCount, totalInDb);
        var firstRecord = await context.Records.OrderBy(r => r.Id).FirstAsync();
        Assert.False(string.IsNullOrWhiteSpace(firstRecord.FirstName));
        Assert.False(string.IsNullOrWhiteSpace(firstRecord.LastName));
        Assert.False(string.IsNullOrWhiteSpace(firstRecord.City));

        var lastRecord = await context.Records.OrderBy(r => r.Id).LastAsync();
        Assert.False(string.IsNullOrWhiteSpace(lastRecord.FirstName));
        Assert.False(string.IsNullOrWhiteSpace(lastRecord.LastName));
    }

    [Fact]
    public async Task ImportCsvAsync_WithMalformedAndMissingRows_SkipsInvalidAndImportsValid()
    {
        const int totalRows = 6_000;
        const int invalidRows = 20;
        var csvPath = Path.Combine(_tempDirectory, "with_invalid_rows.csv");
        await TestDataGenerator.CreateCsvFileAsync(csvPath, totalRows, invalidRowCount: invalidRows);

        var factory = new TestDbContextFactory();
        var importer = new CsvImporter(factory);

        await importer.ImportCsvAsync(csvPath, RussianCulture);

        using var context = factory.CreateDbContext();
        var totalInDb = await context.Records.CountAsync();

        var expectedValidCount = totalRows - invalidRows;
        Assert.Equal(expectedValidCount, totalInDb);
    }

    [Fact]
    public async Task ImportCsvAsync_WhenCancelled_ThrowsOperationCanceledException()
    {
        const int recordCount = 20_000;
        var csvPath = Path.Combine(_tempDirectory, "cancelled_import.csv");
        await TestDataGenerator.CreateCsvFileAsync(csvPath, recordCount);

        var factory = new TestDbContextFactory();
        var importer = new CsvImporter(factory);

        using var cts = new CancellationTokenSource();
        cts.Cancel(); 

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            importer.ImportCsvAsync(csvPath, RussianCulture, cts.Token));
    }

    [Fact]
    public async Task ImportCsvAsync_EmptyFile_InsertsZeroRecords()
    {
        var csvPath = Path.Combine(_tempDirectory, "empty.csv");
        await File.WriteAllTextAsync(csvPath, string.Empty);

        var factory = new TestDbContextFactory();
        var importer = new CsvImporter(factory);

        await importer.ImportCsvAsync(csvPath, RussianCulture);

        using var context = factory.CreateDbContext();
        var count = await context.Records.CountAsync();
        Assert.Equal(0, count);
    }
}
