using MahApps.Metro.Controls;
using ProcessingApp.Presentation.ViewModels;

namespace ProcessingApp.Presentation;

public partial class MainWindow : MetroWindow
{
    public MainWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        Loaded += async (_, _) => await viewModel.InitializeCommand.ExecuteAsync(null);
    }
}
