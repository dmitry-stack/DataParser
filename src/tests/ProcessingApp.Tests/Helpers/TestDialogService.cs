using ProcessingApp.Application.Interfaces;

namespace ProcessingApp.Tests.Helpers;

public class TestDialogService : IDialogService
{
    public string? SaveFileDialogResult { get; set; }
    public string? OpenFileDialogResult { get; set; }

    public List<string> Messages { get; } = [];
    public List<string> Errors { get; } = [];

    public void ShowMessage(string message, string title) => Messages.Add($"{title}: {message}");

    public void ShowError(string message, string title) => Errors.Add($"{title}: {message}");

    public string? ShowSaveFileDialog(string filter, string defaultExt) => SaveFileDialogResult;

    public string? ShowOpenFileDialog(string filter) => OpenFileDialogResult;
}
