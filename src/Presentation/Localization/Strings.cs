using System.ComponentModel;

namespace ProcessingApp.Presentation.Localization
{
    public enum AppLanguage
    {
        Russian,
        English
    }


    public sealed class Strings : INotifyPropertyChanged
    {
        public static Strings Instance { get; } = new Strings();

        public event PropertyChangedEventHandler? PropertyChanged;

        private AppLanguage _language = AppLanguage.Russian;

        private Strings()
        {
        }

        public AppLanguage Language => _language;

        public void SetLanguage(AppLanguage language)
        {
            if (_language == language)
            {
                return;
            }

            _language = language;


            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        }

        private string Get(string key) =>
            _resources.TryGetValue(_language, out var dict) && dict.TryGetValue(key, out var value)
                ? value
                : key;

        public string WindowTitle => Get(nameof(WindowTitle));
        public string SearchGroupHeader => Get(nameof(SearchGroupHeader));
        public string DateWatermark => Get(nameof(DateWatermark));
        public string LastNameWatermark => Get(nameof(LastNameWatermark));
        public string FirstNameWatermark => Get(nameof(FirstNameWatermark));
        public string SurNameWatermark => Get(nameof(SurNameWatermark));
        public string CityWatermark => Get(nameof(CityWatermark));
        public string CountryWatermark => Get(nameof(CountryWatermark));
        public string SearchButton => Get(nameof(SearchButton));
        public string ImportCsvButton => Get(nameof(ImportCsvButton));
        public string CancelButton => Get(nameof(CancelButton));
        public string ExportExcelButton => Get(nameof(ExportExcelButton));
        public string ExportXmlButton => Get(nameof(ExportXmlButton));

        public string ColumnDate => Get(nameof(ColumnDate));
        public string ColumnLastName => Get(nameof(ColumnLastName));
        public string ColumnFirstName => Get(nameof(ColumnFirstName));
        public string ColumnSurName => Get(nameof(ColumnSurName));
        public string ColumnCity => Get(nameof(ColumnCity));
        public string ColumnCountry => Get(nameof(ColumnCountry));

        public string ImportSuccessTitle => Get(nameof(ImportSuccessTitle));
        public string ImportSuccessMessage => Get(nameof(ImportSuccessMessage));
        public string ImportCancelledTitle => Get(nameof(ImportCancelledTitle));
        public string ImportCancelledMessage => Get(nameof(ImportCancelledMessage));
        public string ImportErrorTitle => Get(nameof(ImportErrorTitle));
        public string ImportErrorMessage => Get(nameof(ImportErrorMessage));

        public string SearchErrorTitle => Get(nameof(SearchErrorTitle));
        public string SearchErrorMessage => Get(nameof(SearchErrorMessage));

        public string ExportSuccessTitle => Get(nameof(ExportSuccessTitle));
        public string ExportExcelSuccessMessage => Get(nameof(ExportExcelSuccessMessage));
        public string ExportXmlSuccessMessage => Get(nameof(ExportXmlSuccessMessage));
        public string ExportErrorTitle => Get(nameof(ExportErrorTitle));
        public string ExportErrorMessage => Get(nameof(ExportErrorMessage));

        public string StartupDbErrorTitle => Get(nameof(StartupDbErrorTitle));
        public string StartupDbErrorMessage => Get(nameof(StartupDbErrorMessage));

        private static readonly Dictionary<AppLanguage, Dictionary<string, string>> _resources = new()
        {
            [AppLanguage.Russian] = new Dictionary<string, string>
            {
                [nameof(WindowTitle)] = "Управление данными",
                [nameof(SearchGroupHeader)] = "Параметры поиска",
                [nameof(DateWatermark)] = "Выберите дату",
                [nameof(LastNameWatermark)] = "Фамилия",
                [nameof(FirstNameWatermark)] = "Имя",
                [nameof(SurNameWatermark)] = "Отчество",
                [nameof(CityWatermark)] = "Город",
                [nameof(CountryWatermark)] = "Страна",
                [nameof(SearchButton)] = "Поиск",
                [nameof(ImportCsvButton)] = "Импорт CSV...",
                [nameof(CancelButton)] = "Отмена",
                [nameof(ExportExcelButton)] = "Экспорт в Excel",
                [nameof(ExportXmlButton)] = "Экспорт в XML",

                [nameof(ColumnDate)] = "Дата",
                [nameof(ColumnLastName)] = "Фамилия",
                [nameof(ColumnFirstName)] = "Имя",
                [nameof(ColumnSurName)] = "Отчество",
                [nameof(ColumnCity)] = "Город",
                [nameof(ColumnCountry)] = "Страна",

                [nameof(ImportSuccessTitle)] = "Успех",
                [nameof(ImportSuccessMessage)] = "Данные успешно импортированы!",
                [nameof(ImportCancelledTitle)] = "Отмена",
                [nameof(ImportCancelledMessage)] = "Импорт отменён.",
                [nameof(ImportErrorTitle)] = "Ошибка",
                [nameof(ImportErrorMessage)] = "Произошла ошибка при импорте. Подробности в логах.",

                [nameof(SearchErrorTitle)] = "Ошибка БД",
                [nameof(SearchErrorMessage)] = "Не удалось выполнить поиск. Проверьте подключение к базе данных.",

                [nameof(ExportSuccessTitle)] = "Успех",
                [nameof(ExportExcelSuccessMessage)] = "Файл Excel сохранён!",
                [nameof(ExportXmlSuccessMessage)] = "Файл XML сохранён!",
                [nameof(ExportErrorTitle)] = "Ошибка",
                [nameof(ExportErrorMessage)] = "Ошибка при экспорте. Подробности в логах.",

                [nameof(StartupDbErrorTitle)] = "Ошибка запуска",
                [nameof(StartupDbErrorMessage)] = "Не удалось подключиться к базе данных. Подробности в логах.",
            },
            [AppLanguage.English] = new Dictionary<string, string>
            {
                [nameof(WindowTitle)] = "Data Management",
                [nameof(SearchGroupHeader)] = "Search parameters",
                [nameof(DateWatermark)] = "Select date",
                [nameof(LastNameWatermark)] = "Last name",
                [nameof(FirstNameWatermark)] = "First name",
                [nameof(SurNameWatermark)] = "Middle name",
                [nameof(CityWatermark)] = "City",
                [nameof(CountryWatermark)] = "Country",
                [nameof(SearchButton)] = "Search",
                [nameof(ImportCsvButton)] = "Import CSV...",
                [nameof(CancelButton)] = "Cancel",
                [nameof(ExportExcelButton)] = "Export to Excel",
                [nameof(ExportXmlButton)] = "Export to XML",

                [nameof(ColumnDate)] = "Date",
                [nameof(ColumnLastName)] = "Last name",
                [nameof(ColumnFirstName)] = "First name",
                [nameof(ColumnSurName)] = "Middle name",
                [nameof(ColumnCity)] = "City",
                [nameof(ColumnCountry)] = "Country",

                [nameof(ImportSuccessTitle)] = "Success",
                [nameof(ImportSuccessMessage)] = "Data imported successfully!",
                [nameof(ImportCancelledTitle)] = "Cancelled",
                [nameof(ImportCancelledMessage)] = "Import cancelled.",
                [nameof(ImportErrorTitle)] = "Error",
                [nameof(ImportErrorMessage)] = "An error occurred during import. See logs for details.",

                [nameof(SearchErrorTitle)] = "Database error",
                [nameof(SearchErrorMessage)] = "Search failed. Check the database connection.",

                [nameof(ExportSuccessTitle)] = "Success",
                [nameof(ExportExcelSuccessMessage)] = "Excel file saved!",
                [nameof(ExportXmlSuccessMessage)] = "XML file saved!",
                [nameof(ExportErrorTitle)] = "Error",
                [nameof(ExportErrorMessage)] = "Export failed. See logs for details.",

                [nameof(StartupDbErrorTitle)] = "Startup error",
                [nameof(StartupDbErrorMessage)] = "Could not connect to the database. See logs for details.",
            },
        };
    }
}