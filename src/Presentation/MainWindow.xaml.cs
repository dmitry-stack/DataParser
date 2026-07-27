using System.Windows;
using ProcessingApp.Presentation.ViewModels;

namespace ProcessingApp.Presentation
{
    public partial class MainWindow : Window
    {
        internal MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}