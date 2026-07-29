using Microsoft.VisualBasic;
using ProcessingApp.Application.Interfaces;
using ProcessingApp.Domain;
using ProcessingApp.Application;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;


namespace ProcessingApp.Infrastructure;

public class CSVImport : ICsvImporter
{

    private const char CsvSeparator = ';';
    private const string DateFormat = "dd.MM.yyyy";
    private const int BatchSize = 5000;
    private const int ExpectedFieldsCount = 6;
    private readonly AppDbContext _context;

    public CSVImport(AppDbContext context)
    {
        _context = context; 
    }
    public async Task ImportCsvAsync(string filePath)
    {
       

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var batch = new List<Record>();

         

            await foreach (var dto in ReadRecordsFromCsvAsync(filePath))
            {

                var entity = MapToEntity(dto);
                batch.Add(entity);
               
               

                if (batch.Count >= BatchSize)
                {
                    await _context.Records.AddRangeAsync(batch);
                    await _context.SaveChangesAsync();
                    batch.Clear();
                }


            }

            if (batch.Count > 0)
            {
                await _context.Records.AddRangeAsync(batch);
                await _context.SaveChangesAsync();
            }
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync(); throw;
        }


    }

    private async IAsyncEnumerable<RecordDTO> ReadRecordsFromCsvAsync(string filePath)
    {
        using var reader = new StreamReader(filePath, Encoding.UTF8);

        await reader.ReadLineAsync();

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var dto = ParseLineToDto(line);
            if (dto != null)
            {
               
                yield return dto;
            }
        }
    }

   

    private RecordDTO? ParseLineToDto(string line)
    {
        var parts = line.Split(CsvSeparator);
        if (parts.Length < ExpectedFieldsCount) return null;

        
        if (!DateTime.TryParseExact(parts[0], DateFormat, null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
        {
            return null; 
        }

        return new RecordDTO
        {
            Date = parsedDate,
            FirstName = parts[1].Trim(),
            LastName = parts[2].Trim(),
            SurName = parts[3].Trim(),
            City = parts[4].Trim(),
            Country = parts[5].Trim()
        };
    }

    private Record MapToEntity(RecordDTO dto)
    {
        return new Record
        {
            Date = dto.Date,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            SurName = dto.SurName,
            City = dto.City,
            Country = dto.Country
        };
    }
}