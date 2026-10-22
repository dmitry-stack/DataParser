using ProcessingApp.Application.DTOs;
using System.Runtime.CompilerServices;
using System.Text;

namespace ProcessingApp.Tests.Helpers;

public static class TestDataGenerator
{
    private static readonly string[] FirstNames = ["Иван", "Петр", "Сергей", "Алексей", "Дмитрий", "Ольга", "Анна", "Елена"];
    private static readonly string[] LastNames = ["Иванов", "Петров", "Смирнов", "Соколов", "Васильев", "Кузнецова", "Новикова", "Попова"];
    private static readonly string[] SurNames = ["Иванович", "Петрович", "Сергеевич", "Алексеевич", "Михайлович", "Дмитриевна", "Николаевна", "Сергеевна"];
    private static readonly string[] Cities = ["Минск", "Гомель", "Витебск", "Гродно", "Брест", "Могилев"];
    private static readonly string[] Countries = ["Беларусь", "Польша", "Россия", "Литва"];

    public static async Task CreateCsvFileAsync(
        string filePath,
        int count,
        int invalidRowCount = 0,
        CancellationToken cancellationToken = default)
    {
        var baseDate = new DateTime(2020, 1, 1);

        await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 65536, useAsync: true);
        await using var writer = new StreamWriter(stream, Encoding.UTF8);

        await writer.WriteLineAsync("Дата;Имя;Фамилия;Отчество;Город;Страна".AsMemory(), cancellationToken);

        var invalidInterval = invalidRowCount > 0 ? Math.Max(1, count / invalidRowCount) : int.MaxValue;

        for (var i = 1; i <= count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (invalidRowCount > 0 && i % invalidInterval == 0)
            {
                // Невалидная строка (пропущена обязательная фамилия или некорректная дата)
                if (i % 2 == 0)
                {
                    await writer.WriteLineAsync($"01.01.2020;Имя;;Отчество;Город;Страна".AsMemory(), cancellationToken);
                }
                else
                {
                    await writer.WriteLineAsync($"не_дата;Имя;Фамилия;Отчество;Город;Страна".AsMemory(), cancellationToken);
                }
                continue;
            }

            var date = baseDate.AddDays(i % 1000).ToString("dd.MM.yyyy");
            var first = FirstNames[i % FirstNames.Length];
            var last = LastNames[i % LastNames.Length];
            var sur = SurNames[i % SurNames.Length];
            var city = Cities[i % Cities.Length];
            var country = Countries[i % Countries.Length];

            await writer.WriteLineAsync($"{date};{first};{last};{sur};{city};{country}".AsMemory(), cancellationToken);
        }

        await writer.FlushAsync(cancellationToken);
    }

    public static async IAsyncEnumerable<RecordDTO> GenerateAsyncRecords(
        int count,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var baseDate = new DateTime(2020, 1, 1);

        for (var i = 1; i <= count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            yield return new RecordDTO(
                baseDate.AddDays(i % 1000),
                FirstNames[i % FirstNames.Length],
                LastNames[i % LastNames.Length],
                SurNames[i % SurNames.Length],
                Cities[i % Cities.Length],
                Countries[i % Countries.Length]);
        }

        await Task.CompletedTask;
    }
}
