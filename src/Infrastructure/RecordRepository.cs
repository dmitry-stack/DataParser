using Microsoft.EntityFrameworkCore;
using ProcessingApp.Application.DTOs;
using ProcessingApp.Application.Interfaces;
using System.Runtime.CompilerServices;

namespace ProcessingApp.Infrastructure;

public class RecordRepository(IDbContextFactory<AppDbContext> contextFactory) : IRecordRepository
{
    public async IAsyncEnumerable<RecordDTO> GetFilteredRecordsAsync(
        DateTime? date,
        string? firstName,
        string? surName,
        string? lastName,
        string? city,
        string? country,
        int pageNumber = 1,
        int pageSize = 20,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var context = contextFactory.CreateDbContext();
        var query = context.Records.AsNoTracking();

        if (date.HasValue)
        {
            var targetDate = date.Value.Date;
            query = query.Where(r => r.Date.Date == targetDate);
        }

        if (!string.IsNullOrWhiteSpace(firstName))
            query = query.Where(r => r.FirstName.Contains(firstName));

        if (!string.IsNullOrWhiteSpace(lastName))
            query = query.Where(r => r.LastName.Contains(lastName));

        if (!string.IsNullOrWhiteSpace(surName))
            query = query.Where(r => r.SurName.Contains(surName));

        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(r => r.City.Contains(city));

        if (!string.IsNullOrWhiteSpace(country))
            query = query.Where(r => r.Country.Contains(country));

        query = query.OrderBy(r => r.Id);

        if (pageSize != int.MaxValue)
        {
            query = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize + 1);
        }

        await foreach (var record in query.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            yield return new RecordDTO(
                record.Date,
                record.FirstName,
                record.LastName,
                record.SurName,
                record.City,
                record.Country);
        }
    }
}
