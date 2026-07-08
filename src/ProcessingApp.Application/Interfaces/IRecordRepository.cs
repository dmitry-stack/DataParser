using ProcessingApp.Application;
using ProcessingApp.Domain;
using System;
using System.Collections.Generic;

namespace ProcessingApp.Application.Interfaces;

public interface IRecordRepository
{
    IAsyncEnumerable<RecordDTO> GetFilteredRecordsAsync(DateTime? date, string? firstName, string? surName, string? country, string? city, string? lastName);
}