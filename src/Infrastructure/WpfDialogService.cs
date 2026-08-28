using Microsoft.Win32;
using System.Windows;
using ProcessingApp.Application.Interfaces;

namespace ProcessingApp.Infrastructure.Services;

public class WpfDialogService : IDialogService
{
    public void ShowMessage(string message, string title) =>
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);

    public void ShowError(string message, string title) =>
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

    public string? ShowSaveFileDialog(string filter, string defaultExt)
    {
        var dialog = new SaveFileDialog { Filter = filter, DefaultExt = defaultExt };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? ShowOpenFileDialog(string filter)
    {
        var dialog = new OpenFileDialog { Filter = filter };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}