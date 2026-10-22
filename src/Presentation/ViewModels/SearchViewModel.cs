using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ProcessingApp.Application.DTOs;
using ProcessingApp.Application.Interfaces;
using ProcessingApp.Presentation.Localization;
using ProcessingApp.Presentation.Messages;
using ProcessingApp.Presentation.State;
using Serilog;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ProcessingApp.Presentation.ViewModels;

public partial class SearchViewModel : ObservableObject, IRecipient<RecordsImportedMessage>
{
    private const int PageSize = 15;

    private readonly IRecordRepository _repository;
    private readonly IDialogService _dialogService;

    public SearchViewModel(
        IRecordRepository repository,
        IDialogService dialogService,
        SearchFilter filter,
        IMessenger messenger)
    {
        _repository = repository;
        _dialogService = dialogService;
        Filter = filter;

        messenger.Register<RecordsImportedMessage>(this);
        Strings.Instance.PropertyChanged += OnStringsPropertyChanged;
    }

    public SearchFilter Filter { get; }

    public ObservableCollection<RecordDTO> Records { get; } = [];

    public string CurrentPageDisplay => string.Format(Strings.Instance.PageFormat, CurrentPage);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentPageDisplay))]
    [NotifyCanExecuteChangedFor(nameof(PreviousPageCommand))]
    private int _currentPage = 1;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NextPageCommand))]
    private bool _hasNextPage;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ExecuteNewSearchCommand))]
    [NotifyCanExecuteChangedFor(nameof(ClearFilterCommand))]
    [NotifyCanExecuteChangedFor(nameof(PreviousPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextPageCommand))]
    private bool _isLoading;

    public void Receive(RecordsImportedMessage message) => _ = ReloadFromFirstPageAsync();

    private bool CanSearch() => !IsLoading;

    private bool CanGoPrevious() => !IsLoading && CurrentPage > 1;

    private bool CanGoNext() => !IsLoading && HasNextPage;

    [RelayCommand(CanExecute = nameof(CanSearch))]
    private Task ExecuteNewSearchAsync() => ReloadFromFirstPageAsync();

    [RelayCommand(CanExecute = nameof(CanSearch))]
    private Task ClearFilterAsync()
    {
        Filter.Clear();
        return ReloadFromFirstPageAsync();
    }

    [RelayCommand(CanExecute = nameof(CanGoPrevious))]
    private async Task PreviousPageAsync()
    {
        CurrentPage--;
        await LoadCurrentPageAsync();
    }

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private async Task NextPageAsync()
    {
        CurrentPage++;
        await LoadCurrentPageAsync();
    }

    private async Task ReloadFromFirstPageAsync()
    {
        CurrentPage = 1;
        await LoadCurrentPageAsync();
    }

    private async Task LoadCurrentPageAsync()
    {
        IsLoading = true;

        try
        {
            Records.Clear();
            HasNextPage = false;

            var found = 0;

            await foreach (var record in _repository.GetFilteredRecordsAsync(
                Filter.Date,
                Filter.FirstName,
                Filter.SurName,
                Filter.LastName,
                Filter.City,
                Filter.Country,
                CurrentPage,
                PageSize))
            {
                // Репозиторий отдаёт на одну запись больше размера страницы:
                // лишняя запись означает, что следующая страница существует.
                if (++found > PageSize)
                {
                    HasNextPage = true;
                    break;
                }

                Records.Add(record);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Ошибка при поиске записей в БД.");
            _dialogService.ShowError(Strings.Instance.SearchErrorMessage, Strings.Instance.SearchErrorTitle);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OnStringsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(Strings.PageFormat))
        {
            OnPropertyChanged(nameof(CurrentPageDisplay));
        }
    }
}
