using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcessingApp.Presentation.Localization;
using ProcessingApp.Presentation.Navigation;

namespace ProcessingApp.Presentation.ViewModels;

/// <summary>
/// ViewModel главного окна: хранит выбранный раздел и ViewModel текущей страницы.
/// Нужный View подбирается в MainWindow.xaml через неявные DataTemplate.
/// </summary>
public partial class ShellViewModel : ObservableObject
{
    private readonly SearchViewModel _searchViewModel;
    private readonly ImportViewModel _importViewModel;
    private readonly ExportViewModel _exportViewModel;

    private ObservableObject _currentViewModel;

    public ShellViewModel(
        SearchViewModel searchViewModel,
        ImportViewModel importViewModel,
        ExportViewModel exportViewModel)
    {
        _searchViewModel = searchViewModel;
        _importViewModel = importViewModel;
        _exportViewModel = exportViewModel;

        _currentViewModel = searchViewModel;
    }

    public ObservableObject CurrentViewModel
    {
        get => _currentViewModel;
        private set => SetProperty(ref _currentViewModel, value);
    }

    [ObservableProperty]
    private AppPage _selectedPage = AppPage.Search;

    partial void OnSelectedPageChanged(AppPage value)
    {
        CurrentViewModel = value switch
        {
            AppPage.Search => _searchViewModel,
            AppPage.Import => _importViewModel,
            AppPage.Export => _exportViewModel,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Неизвестная страница."),
        };
    }

    /// <summary>Загружает первую страницу результатов при открытии окна.</summary>
    [RelayCommand]
    private Task InitializeAsync() => _searchViewModel.ExecuteNewSearchCommand.ExecuteAsync(null);

    [RelayCommand]
    private void SetLanguage(string cultureCode) =>
        Strings.Instance.SetLanguage(cultureCode == "en" ? AppLanguage.English : AppLanguage.Russian);
}
