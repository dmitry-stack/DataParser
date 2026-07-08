using ProcessingApp.Application;
using ProcessingApp.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProcessingApp.Application.Interfaces;

public interface IXmlExporter
{
    Task ExportToXmlAsync(IAsyncEnumerable<RecordDTO> records, string filePath);
}