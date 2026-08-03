using System.Threading;
using System.Threading.Tasks;

namespace ProcessingApp.Application.Interfaces;

public interface ICsvImporter
{
    Task ImportCsvAsync(string filePath, CancellationToken token = default);
}