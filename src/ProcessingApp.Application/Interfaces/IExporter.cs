using ProcessingApp.Application.DTOs;

namespace ProcessingApp.Application.Interfaces;

public interface IExporter
{

    string SupportedExtension { get; }

    Task ExportAsync(IAsyncEnumerable<RecordDTO> records, string filePath, CancellationToken token = default);
}