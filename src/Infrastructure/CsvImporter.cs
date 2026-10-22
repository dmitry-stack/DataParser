using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using ProcessingApp.Application.DTOs;
using ProcessingApp.Application.Interfaces;
using ProcessingApp.Domain;
using Serilog;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace ProcessingApp.Infrastructure;

public class CsvImporter(IDbContextFactory<AppDbContext> contextFactory) : ICsvImporter
{
    private const int BatchSize = 5000;

    public async Task ImportCsvAsync(string filePath, CultureInfo culture, CancellationToken token = default)
    {
        Log.Information("Начало импорта файла {FilePath} с культурой {Culture}", filePath, culture.Name);

        var parsedRecords = ReadAndParseCsvAsync(filePath, culture, token);
        await SaveToDatabaseInBatchesAsync(parsedRecords, token);
    }

    /// <summary>
    /// Читает CSV и отдаёт строки как <see cref="RecordDTO"/>: это чистая передача данных,
    /// в сущность БД строка превращается только непосредственно перед сохранением.
    /// </summary>
    private static async IAsyncEnumerable<RecordDTO> ReadAndParseCsvAsync(
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
        var rowNumber = 1;

        while (await csv.ReadAsync())
        {
            token.ThrowIfCancellationRequested();
            rowNumber++;

            RecordDTO dto;

            try
            {
                dto = new RecordDTO(
                    csv.GetField<DateTime>("Дата"),
                    csv.GetField<string>("Имя") ?? string.Empty,
                    csv.GetField<string>("Фамилия") ?? string.Empty,
                    csv.GetField<string>("Отчество") ?? string.Empty,
                    csv.GetField<string>("Город") ?? string.Empty,
                    csv.GetField<string>("Страна") ?? string.Empty);
            }
            catch (Exception ex)
            {
                Log.Warning(
                    ex,
                    "Ошибка парсинга CSV на строке {Row}. Сырые данные: {RawRecord}",
                    rowNumber,
                    csv.Parser.RawRecord);
                continue;
            }

            if (string.IsNullOrWhiteSpace(dto.LastName) ||
                string.IsNullOrWhiteSpace(dto.City) ||
                string.IsNullOrWhiteSpace(dto.Country))
            {
                Log.Warning("Пропущена строка {Row}: отсутствуют обязательные поля.", rowNumber);
                continue;
            }

            yield return dto;
        }
    }

    private async Task SaveToDatabaseInBatchesAsync(IAsyncEnumerable<RecordDTO> records, CancellationToken token)
    {
        var recordsCount = 0;
        using var context = await contextFactory.CreateDbContextAsync(token);

        await foreach (var dto in records.WithCancellation(token))
        {
            context.Records.Add(ToEntity(dto));
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

    private static Record ToEntity(RecordDTO dto) => new()
    {
        Date = dto.Date,
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        SurName = dto.SurName,
        City = dto.City,
        Country = dto.Country,
    };
}
