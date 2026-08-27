using Microsoft.EntityFrameworkCore;
using ProcessingApp.Application.DTOs;
using ProcessingApp.Application.Interfaces;

namespace ProcessingApp.Infrastructure
{
    public class RecordRepository : IRecordRepository
    {
        private readonly AppDbContext _context;

        public RecordRepository(AppDbContext context)
        {
            _context = context;
        }
        public async IAsyncEnumerable<RecordDTO> GetFilteredRecordsAsync(
           DateTime? date, string? firstName, string? surName,
            string? city, string? country, string? lastName,
            int pageNumber = 1, int pageSize = 50)
        {
            var query = _context.Records.AsQueryable();

            if (date.HasValue)
                query = query.Where(r => r.Date == date.Value);

            if (!string.IsNullOrWhiteSpace(firstName))
                query = query.Where(r => r.FirstName.Contains(firstName));


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
