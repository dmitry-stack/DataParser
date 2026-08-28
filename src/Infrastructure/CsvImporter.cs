using CsvHelper;
using CsvHelper.Configuration;
using ProcessingApp.Application.Interfaces;
using ProcessingApp.Domain;
using Serilog;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;

namespace ProcessingApp.Infrastructure;

public class CSVImport : ICsvImporter
{
    private readonly Microsoft.EntityFrameworkCore.IDbContextFactory<AppDbContext> _contextFactory;
    private const int BatchSize = 5000;

    public CSVImport(Microsoft.EntityFrameworkCore.IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }


    public async Task ImportCsvAsync(string filePath, CultureInfo culture, CancellationToken token = default)
    {
        Log.Information("Начало импорта файла {FilePath} с культурой {Culture}", filePath, culture.Name);

        var parsedRecords = ReadAndParseCsvAsync(filePath, culture, token);
        await SaveToDatabaseInBatchesAsync(parsedRecords, token);
    }

    private async IAsyncEnumerable<Record> ReadAndParseCsvAsync(
        string filePath,
        CultureInfo culture,
        [EnumeratorCancellation] CancellationToken token)
    {
        var config = new CsvConfiguration(culture)
        {
            Delimiter = ";",
            HasHeaderRecord = true,
            MissingFieldFound = null,
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);

        if (!await csv.ReadAsync())
        {
            Log.Warning("Попытка импорта пустого файла: {FilePath}", filePath);
            yield break;
        }

        csv.ReadHeader();
        int rowNumber = 1;

        while (await csv.ReadAsync())
        {
            token.ThrowIfCancellationRequested();
            rowNumber++;

            Record? record = null;

            try
            {
                record = new Record
                {
                    Date = csv.GetField<DateTime>("Дата"),
                    FirstName = csv.GetField<string>("Имя") ?? string.Empty,
                    LastName = csv.GetField<string>("Фамилия") ?? string.Empty,
                    SurName = csv.GetField<string>("Отчество") ?? string.Empty,
                    City = csv.GetField<string>("Город") ?? string.Empty,
                    Country = csv.GetField<string>("Страна") ?? string.Empty,
                };
            }
            catch (Exception ex)
            {

                Log.Warning(ex, "Ошибка парсинга CSV на строке {Row}. Сырые данные: {RawRecord}", rowNumber, csv.Parser.RawRecord);
                continue;
            }

            if (string.IsNullOrWhiteSpace(record.LastName) ||
                string.IsNullOrWhiteSpace(record.City) ||
                string.IsNullOrWhiteSpace(record.Country))
            {
                Log.Warning("Пропущена строка {Row}: отсутствуют обязательные поля.", rowNumber);
                continue;
            }

            yield return record;
        }
    }

    private async Task SaveToDatabaseInBatchesAsync(IAsyncEnumerable<Record> records, CancellationToken token)
    {
        int recordsCount = 0;
        using var context = await _contextFactory.CreateDbContextAsync(token);

        await foreach (var record in records.WithCancellation(token))
        {
            context.Records.Add(record);
            recordsCount++;

            if (recordsCount % BatchSize == 0)
            {
                await context.SaveChangesAsync(token);
                context.ChangeTracker.Clear();
                Log.Information("Успешно импортировано {Count} записей...", recordsCount);
            }
        }

        if (recordsCount % BatchSize != 0)
        {
            await context.SaveChangesAsync(token);
            context.ChangeTracker.Clear();
        }

        Log.Information("Импорт завершен. Всего добавлено: {Total}", recordsCount);
    }
}