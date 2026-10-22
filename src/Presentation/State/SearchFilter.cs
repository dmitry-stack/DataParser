using CommunityToolkit.Mvvm.ComponentModel;

namespace ProcessingApp.Presentation.State;

/// <summary>
/// Фильтр записей, общий для страниц «Поиск» и «Экспорт»: что пользователь ввёл
/// на странице поиска, то же самое будет выгружено на странице экспорта.
/// </summary>
public sealed partial class SearchFilter : ObservableObject
{
    [ObservableProperty]
    private DateTime? _date;

    [ObservableProperty]
    private string? _lastName;

    [ObservableProperty]
    private string? _firstName;

    [ObservableProperty]
    private string? _surName;

    [ObservableProperty]
    private string? _city;

    [ObservableProperty]
    private string? _country;

    public void Clear()
    {
        Date = null;
        LastName = null;
        FirstName = null;
        SurName = null;
        City = null;
        Country = null;
    }
}
