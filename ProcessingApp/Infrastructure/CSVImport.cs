using ProcessingApp.Domain;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ProcessingApp.Infrastructure
{
    public class CSVImport
    {
        public async Task ImportCsvAsync(string filePath)
        {
            using var context = new AppDbContext();
            using var reader = new StreamReader(filePath);

            await reader.ReadLineAsync();

            var batch = new List<Record>();
            const int batchSize = 5000; 
            string? line;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] parts = line.Split(';');
                if (parts.Length < 6) continue; 

                if (!DateTime.TryParseExact(parts[0], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                   
                    continue;
                }

                var record = new Record
                {
                    Date = parsedDate,
                    FirstName = parts[1].Trim(),
                    LastName = parts[2].Trim(),
                    SurName = parts[3].Trim(),
                    City = parts[4].Trim(),
                    Country = parts[5].Trim()
                };

                batch.Add(record);

                if (batch.Count >= batchSize)
                {
                    await context.Records.AddRangeAsync(batch);
                    await context.SaveChangesAsync();
                    batch.Clear(); 
                }
            }

            if (batch.Count > 0)
            {
                await context.Records.AddRangeAsync(batch);
                await context.SaveChangesAsync();
            }
        }
    }
}
