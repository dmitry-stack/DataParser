using Microsoft.EntityFrameworkCore;
using ProcessingApp.Application.DTOs;
using ProcessingApp.Application.Interfaces;

namespace ProcessingApp.Infrastructure
{
    public class RecordRepository : IRecordRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        public RecordRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async IAsyncEnumerable<RecordDTO> GetFilteredRecordsAsync(
           DateTime? date, string? firstName, string? surName,
            string? city, string? country, string? lastName,
            int pageNumber = 1, int pageSize = 20)
        {
            using var context = _contextFactory.CreateDbContext();
            var query = context.Records.AsQueryable();

            if (date.HasValue)
                query = query.Where(r => r.Date.Date == date.Value.Date);

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


            query = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);


            await foreach (var record in query.AsAsyncEnumerable())
            {
                yield return new RecordDTO(
                    record.Date,
                    record.FirstName,
                    record.LastName,
                    record.SurName,
                    record.City,
                    record.Country
                );
            }
        }
    }
}
