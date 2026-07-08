using System.Threading.Tasks;

namespace ProcessingApp.Application.Interfaces;

public interface ICsvImporter
{
    Task ImportCsvAsync(string filePath);
}