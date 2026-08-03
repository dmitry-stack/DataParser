using CsvHelper;
using CsvHelper.Configuration;
using ProcessingApp.Application.Interfaces;
using Serilog;
using System.Globalization;

using System;
using System.Collections.Generic;

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessingApp.Infrastructure
{
    public class CSVImport : ICsvImporter
    {
        private readonly AppDbContext _context;

        public CSVImport(AppDbContext context)
        {
            _context = context;
        }

        public async Task ImportCsvAsync(string filePath, CancellationToken token = default)
        {
            var config = new CsvConfiguration(new CultureInfo("ru-RU"))
            {
                Delimiter = ";",
                HasHeaderRecord = true,
                MissingFieldFound = null
            };

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, config);

            var recordsToAdd = new List<ProcessingApp.Domain.Record>(); 
            int rowNumber = 1;

            await csv.ReadAsync();
            csv.ReadHeader();

            while (await csv.ReadAsync())
            {
             
                token.ThrowIfCancellationRequested();
                rowNumber++;

                try
                {
                   
                    var record = new ProcessingApp.Domain.Record
                    {

                        Date = csv.GetField<DateTime>("Дата"),
                        FirstName = csv.GetField<string>("Имя"),
                        LastName = csv.GetField<string>("Фамилия"),
                        SurName = csv.GetField<string>("Отчество"),
                        City = csv.GetField<string>("Город"),
                        Country = csv.GetField<string>("Страна")
                    };
                    if (string.IsNullOrWhiteSpace(record.LastName))
                    {
                        Log.Warning("Пропущена строка {Row}: Отсутствует обязательное поле LastName.", rowNumber);
                        continue;
                    }

                    recordsToAdd.Add(record);
                }
                catch (Exception ex)
                {
               
                    Log.Warning(ex, "Ошибка парсинга CSV на строке {Row}. Сырые данные: {RawRecord}", rowNumber, csv.Parser.RawRecord);
                }
            }

            if (recordsToAdd.Count > 0)
            {
                try
                {
                    await _context.Records.AddRangeAsync(recordsToAdd, token);
                    await _context.SaveChangesAsync(token);

                    Log.Information("Успешно импортировано {Count} записей.", recordsToAdd.Count);
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
                {
                   
                    string exactError = dbEx.InnerException != null ? dbEx.InnerException.Message : dbEx.Message;
                    Log.Error(dbEx, "База данных отклонила сохранение! Причина SQL: {SqlError}", exactError);
                    throw;
                }
            }
        }
    }
}