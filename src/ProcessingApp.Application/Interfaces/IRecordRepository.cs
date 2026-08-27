using ProcessingApp.Application.DTOs;

namespace ProcessingApp.Application.Interfaces;

public interface IRecordRepository
{
    IAsyncEnumerable<RecordDTO> GetFilteredRecordsAsync(DateTime? date,
        string? firstName, string? surName, string? lastName, string? city, string? country,
        int pageNumber = 1,
        int pageSize = 50);
}