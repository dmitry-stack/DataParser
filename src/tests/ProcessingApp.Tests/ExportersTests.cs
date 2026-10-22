using ClosedXML.Excel;
using ProcessingApp.Infrastructure;
using ProcessingApp.Tests.Helpers;
using System.Xml.Linq;

namespace ProcessingApp.Tests;

public class ExportersTests : IDisposable
{
    private readonly string _tempDirectory;

    public ExportersTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"exporter_tests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDirectory);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, recursive: true);
            }
        }
        catch
        {
            // Игнорируем ошибки очистки временных файлов тестов
        }
    }

    [Fact]
    public async Task ExcelExporter_WithLargeDataset_ExportsCorrectRowsAndHeaders()
    {
        const int recordCount = 10_000;
        var filePath = Path.Combine(_tempDirectory, "large_export.xlsx");
        var records = TestDataGenerator.GenerateAsyncRecords(recordCount);

        var exporter = new ExcelExporter();
        await exporter.ExportAsync(records, filePath);

        Assert.True(File.Exists(filePath));
        var fileInfo = new FileInfo(filePath);
        Assert.True(fileInfo.Length > 0);

        // Открываем созданную книгу и проверяем структуру
        using var workbook = new XLWorkbook(filePath);
        var worksheet = workbook.Worksheet("Экспорт Данных");
        Assert.NotNull(worksheet);

        // Проверяем заголовки
        Assert.Equal("№", worksheet.Cell(1, 1).GetString());
        Assert.Equal("Дата", worksheet.Cell(1, 2).GetString());
        Assert.Equal("Имя", worksheet.Cell(1, 3).GetString());
        Assert.Equal("Фамилия", worksheet.Cell(1, 4).GetString());
        Assert.Equal("Отчество", worksheet.Cell(1, 5).GetString());
        Assert.Equal("Город", worksheet.Cell(1, 6).GetString());
        Assert.Equal("Страна", worksheet.Cell(1, 7).GetString());

        // Проверяем количество строк: 1 строка заголовка + 10 000 строк данных
        var lastRowUsed = worksheet.LastRowUsed()?.RowNumber() ?? 0;
        Assert.Equal(recordCount + 1, lastRowUsed);

        // Проверяем значения первой строки данных
        Assert.Equal("1", worksheet.Cell(2, 1).GetString());
        Assert.False(string.IsNullOrWhiteSpace(worksheet.Cell(2, 2).GetString()));
        Assert.False(string.IsNullOrWhiteSpace(worksheet.Cell(2, 4).GetString()));
    }

    [Fact]
    public async Task ExcelExporter_WhenCancelled_ThrowsOperationCanceledException()
    {
        var filePath = Path.Combine(_tempDirectory, "cancelled.xlsx");
        var records = TestDataGenerator.GenerateAsyncRecords(50_000);

        var exporter = new ExcelExporter();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            exporter.ExportAsync(records, filePath, cts.Token));
    }

    [Fact]
    public async Task XmlExporter_WithLargeDataset_StreamsValidXmlDocument()
    {
        const int recordCount = 20_000;
        var filePath = Path.Combine(_tempDirectory, "large_export.xml");
        var records = TestDataGenerator.GenerateAsyncRecords(recordCount);

        var exporter = new XmlExporter();
        await exporter.ExportAsync(records, filePath);

        Assert.True(File.Exists(filePath));
        var fileInfo = new FileInfo(filePath);
        Assert.True(fileInfo.Length > 0);

        // Читаем XML и проверяем корневой элемент и количество дочерних записей
        var doc = await Task.Run(() => XDocument.Load(filePath));
        Assert.NotNull(doc.Root);
        Assert.Equal("Records", doc.Root.Name.LocalName);

        var recordElements = doc.Root.Elements("Record").ToList();
        Assert.Equal(recordCount, recordElements.Count);

        // Проверяем дочерние элементы первой записи
        var firstRecord = recordElements[0];
        Assert.NotNull(firstRecord.Element("Date"));
        Assert.NotNull(firstRecord.Element("FirstName"));
        Assert.NotNull(firstRecord.Element("LastName"));
        Assert.NotNull(firstRecord.Element("SurName"));
        Assert.NotNull(firstRecord.Element("City"));
        Assert.NotNull(firstRecord.Element("Country"));
    }

    [Fact]
    public async Task XmlExporter_WhenCancelled_ThrowsOperationCanceledException()
    {
        var filePath = Path.Combine(_tempDirectory, "cancelled.xml");
        var records = TestDataGenerator.GenerateAsyncRecords(50_000);

        var exporter = new XmlExporter();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            exporter.ExportAsync(records, filePath, cts.Token));
    }
}
