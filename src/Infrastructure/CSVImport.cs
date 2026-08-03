using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProcessingApp.Application.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessingApp.Infrastructure
{
    public class CSVImport : ICsvImporter
    {
        private const int BatchSize = 5000;

        private readonly AppDbContext _context;

        public CSVImport(AppDbContext context)
        {
            _context = context;
        }

        public async Task ImportCsvAsync(string filePath, CancellationToken cancellationToken = default)
        {
            var config = new CsvConfiguration(new CultureInfo("ru-RU"))
            {
                Delimiter = ";",
                HasHeaderRecord = true,
                MissingFieldFound = null
            };

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, config);

            await csv.ReadAsync();
            csv.ReadHeader();

            int rowNumber = 1;
            int importedCount = 0;
            int skippedCount = 0;

            IDbContextTransaction transaction =
                await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                while (await csv.ReadAsync())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    rowNumber++;

                    Domain.Record record;
                    try
                    {
                        record = new Domain.Record
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
                        skippedCount++;
                        Log.Warning(ex, "Ошибка парсинга строки {Row}. Сырые данные: {Raw}",
                            rowNumber, csv.Parser.RawRecord);
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(record.LastName))
                    {
                        skippedCount++;
                        Log.Warning("Пропущена строка {Row}: отсутствует обязательное поле LastName.", rowNumber);
                        continue;
                    }

                    _context.Records.Add(record);
                    importedCount++;

                    if (importedCount % BatchSize == 0)
                    {
                        await _context.SaveChangesAsync(cancellationToken);
                        _context.ChangeTracker.Clear();
                        Log.Information("Импортировано {Count} записей...", importedCount);
                    }
                }

                if (importedCount % BatchSize != 0)
                {
                    await _context.SaveChangesAsync(cancellationToken);
                    _context.ChangeTracker.Clear();
                }

                await transaction.CommitAsync(cancellationToken);

                Log.Information(
                    "Импорт завершён. Добавлено: {Imported}, пропущено: {Skipped}.",
                    importedCount, skippedCount);
            }
            catch (OperationCanceledException)
            {
                Log.Information(
                    "Импорт отменён пользователем на строке {Row}. Изменения откатываются.", rowNumber);
                await transaction.RollbackAsync(CancellationToken.None);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Импорт прерван ошибкой на строке {Row}. Изменения откатываются.", rowNumber);
                await transaction.RollbackAsync(CancellationToken.None);
                throw;
            }
            finally
            {
                await transaction.DisposeAsync();
            }
        }
    }
}