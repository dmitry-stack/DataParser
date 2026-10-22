using CommunityToolkit.Mvvm.Messaging;
using ProcessingApp.Application.DTOs;
using ProcessingApp.Application.Interfaces;
using ProcessingApp.Infrastructure;
using ProcessingApp.Presentation.Messages;
using ProcessingApp.Presentation.State;
using ProcessingApp.Presentation.ViewModels;
using ProcessingApp.Tests.Helpers;

namespace ProcessingApp.Tests;

public class ViewModelTests : IDisposable
{
    private readonly string _tempDirectory;

    public ViewModelTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"vm_tests_{Guid.NewGuid():N}");
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
        }
    }

    [Fact]
    public async Task ExportViewModel_SuccessfulExport_CreatesFinalFileAndCleansTemp()
    {
        var factory = new TestDbContextFactory();
        var repository = new RecordRepository(factory);
        var exporters = new List<IExporter> { new XmlExporter() };
        var dialogService = new TestDialogService();
        var filter = new SearchFilter { City = "Минск" };

        var destinationFile = Path.Combine(_tempDirectory, "output.xml");
        dialogService.SaveFileDialogResult = destinationFile;

        var vm = new ExportViewModel(repository, exporters, dialogService, filter);

        Assert.Contains("Минск", vm.FilterSummary);

        await vm.ExportCommand.ExecuteAsync(".xml");

        Assert.True(File.Exists(destinationFile));
        // Убеждаемся, что временных .tmp файлов не осталось
        var tmpFiles = Directory.GetFiles(_tempDirectory, "*.tmp.*");
        Assert.Empty(tmpFiles);
        Assert.Single(dialogService.Messages);
    }

    [Fact]
    public async Task ExportViewModel_CancelledExport_CleansTempAndDoesNotCreateDestinationFile()
    {
        var factory = new TestDbContextFactory();
        var repository = new RecordRepository(factory);
        var exporters = new List<IExporter> { new SlowMockExporter() };
        var dialogService = new TestDialogService();
        var filter = new SearchFilter();

        var destinationFile = Path.Combine(_tempDirectory, "should_not_exist.xml");
        dialogService.SaveFileDialogResult = destinationFile;

        var vm = new ExportViewModel(repository, exporters, dialogService, filter);

        using var cts = new CancellationTokenSource();
        cts.Cancel(); // отменяем сразу

        await vm.ExportCommand.ExecuteAsync(".xml");

        // Если отменено через CancellationToken, финальный файл не должен быть создан
        var tmpFiles = Directory.GetFiles(_tempDirectory, "*.tmp.*");
        Assert.Empty(tmpFiles);
    }

    [Fact]
    public async Task ImportViewModel_ImportsCsvAndBroadcastsRecordsImportedMessage()
    {
        var csvPath = Path.Combine(_tempDirectory, "data.csv");
        await TestDataGenerator.CreateCsvFileAsync(csvPath, 100);

        var factory = new TestDbContextFactory();
        var importer = new CsvImporter(factory);
        var dialogService = new TestDialogService { OpenFileDialogResult = csvPath };
        var messenger = new WeakReferenceMessenger();

        var messageReceived = false;
        messenger.Register<RecordsImportedMessage>(this, (_, _) => messageReceived = true);

        var vm = new ImportViewModel(importer, dialogService, messenger);

        Assert.False(vm.ImportCsvCommand.CanExecute(null));

        vm.BrowseFileCommand.Execute(null);

        Assert.Equal(csvPath, vm.SelectedFilePath);
        Assert.True(vm.ImportCsvCommand.CanExecute(null));

        await vm.ImportCsvCommand.ExecuteAsync(null);

        Assert.True(messageReceived);
        Assert.Single(dialogService.Messages);

        using var context = factory.CreateDbContext();
        Assert.Equal(100, context.Records.Count());
    }

    private class SlowMockExporter : IExporter
    {
        public string SupportedExtension => ".xml";

        public async Task ExportAsync(IAsyncEnumerable<RecordDTO> records, string filePath, CancellationToken token = default)
        {
            await using var stream = File.Create(filePath);
            token.ThrowIfCancellationRequested();
            await Task.Delay(1000, token);
        }
    }
}
