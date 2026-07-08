using ProcessingApp.Application;
using ProcessingApp.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProcessingApp.Application.Interfaces;

public interface IExcelExporter
{
    Task ExportToExcelAsync(IAsyncEnumerable<RecordDTO> records, string filePath);
}