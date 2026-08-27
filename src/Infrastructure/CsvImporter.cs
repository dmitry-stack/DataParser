using CsvHelper;
using CsvHelper.Configuration;
using ProcessingApp.Application.Interfaces;
using ProcessingApp.Domain; // Убедись, что тут правильный using для твоего класса Record
using Serilog;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace ProcessingApp.Infrastructure;

public class CSVImport : ICsvImporter
{
    private readonly AppDbContext _context;
    private const int BatchSize = 5000;

    public CSVImport(AppDbContext context)
    {
        _context = context;
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

            Record record = null;

            try
            {
                record = new Record
                {
                    Date = csv.GetField<DateTime>("Дата"),
                    FirstName = csv.GetField<string>("Имя"),
                    LastName = csv.GetField<string>("Фамилия"),
                    SurName = csv.GetField<string>("Отчество"),
                    City = csv.GetField<string>("Город"),
                    Country = csv.GetField<string>("Страна")
                };
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Ошибка парсинга CSV на строке {Row}. Сырые данные: {RawRecord}", rowNumber, csv.Parser.RawRecord);
                continue;
            }

            if (string.IsNullOrWhiteSpace(record.LastName))
            {
                Log.Warning("Пропущена строка {Row}: Отсутствует обязательное поле LastName.", rowNumber);
                continue;
            }

            yield return record;
        }
    }

    private async Task SaveToDatabaseInBatchesAsync(IAsyncEnumerable<Record> records, CancellationToken token)
    {
        int recordsCount = 0;

        await foreach (var record in records.WithCancellation(token))
        {
            _context.Records.Add(record);
            recordsCount++;

            if (recordsCount % BatchSize == 0)
            {
                await _context.SaveChangesAsync(token);
                _context.ChangeTracker.Clear();
                Log.Information("Успешно импортировано {Count} записей...", recordsCount);
            }
        }

        if (recordsCount % BatchSize != 0)
        {
            await _context.SaveChangesAsync(token);
            _context.ChangeTracker.Clear();
        }

        Log.Information("Импорт завершен. Всего добавлено: {Total}", recordsCount);
    }
}