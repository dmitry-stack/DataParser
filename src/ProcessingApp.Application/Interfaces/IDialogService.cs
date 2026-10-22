namespace ProcessingApp.Application.Interfaces;

public interface IDialogService
{
    void ShowMessage(string message, string title);

    void ShowError(string message, string title);

    string? ShowSaveFileDialog(string filter, string defaultExt);

    string? ShowOpenFileDialog(string filter);
}
