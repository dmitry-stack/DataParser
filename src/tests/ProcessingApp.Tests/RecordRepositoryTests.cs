using ProcessingApp.Domain;
using ProcessingApp.Infrastructure;
using ProcessingApp.Tests.Helpers;

namespace ProcessingApp.Tests;

public class RecordRepositoryTests
{
    [Fact]
    public async Task GetFilteredRecordsAsync_WithLargeDataset_FiltersCorrectly()
    {
        var factory = new TestDbContextFactory();
        using (var seedContext = factory.CreateDbContext())
        {
            var baseDate = new DateTime(2023, 5, 10);
            for (var i = 1; i <= 3_000; i++)
            {
                seedContext.Records.Add(new Record
                {
                    Date = baseDate.AddDays(i % 5),
                    FirstName = i % 2 == 0 ? "Иван" : "Петр",
                    LastName = i % 3 == 0 ? "Иванов" : "Сидоров",
                    SurName = "Сергеевич",
                    City = i % 4 == 0 ? "Минск" : "Гомель",
                    Country = "Беларусь",
                });
            }
            await seedContext.SaveChangesAsync();
        }

        var repository = new RecordRepository(factory);

        // Фильтр по городу Минск
        var minskRecords = new List<Application.DTOs.RecordDTO>();
        await foreach (var item in repository.GetFilteredRecordsAsync(
            date: null,
            firstName: null,
            surName: null,
            lastName: null,
            city: "Минск",
            country: null,
            pageNumber: 1,
            pageSize: int.MaxValue))
        {
            minskRecords.Add(item);
        }

        Assert.Equal(750, minskRecords.Count);
        Assert.All(minskRecords, r => Assert.Equal("Минск", r.City));

        // Фильтр по дате и имени
        var targetDate = new DateTime(2023, 5, 10);
        var dateFiltered = new List<Application.DTOs.RecordDTO>();
        await foreach (var item in repository.GetFilteredRecordsAsync(
            date: targetDate,
            firstName: "Иван",
            surName: null,
            lastName: null,
            city: null,
            country: null,
            pageNumber: 1,
            pageSize: int.MaxValue))
        {
            dateFiltered.Add(item);
        }

        Assert.NotEmpty(dateFiltered);
        Assert.All(dateFiltered, r =>
        {
            Assert.Equal(targetDate.Date, r.Date.Date);
            Assert.Contains("Иван", r.FirstName);
        });
    }

    [Fact]
    public async Task GetFilteredRecordsAsync_Pagination_ReturnsPageSizePlusOneForNextPageDetection()
    {
        var factory = new TestDbContextFactory();
        using (var seedContext = factory.CreateDbContext())
        {
            for (var i = 1; i <= 30; i++)
            {
                seedContext.Records.Add(new Record
                {
                    Date = DateTime.Today,
                    FirstName = $"Имя{i}",
                    LastName = $"Фамилия{i}",
                    City = "Минск",
                    Country = "Беларусь",
                });
            }
            await seedContext.SaveChangesAsync();
        }

        var repository = new RecordRepository(factory);

        // Страница 1 с размером 10: репозиторий должен вернуть 11 (10 + 1)
        var page1 = new List<Application.DTOs.RecordDTO>();
        await foreach (var item in repository.GetFilteredRecordsAsync(
            null, null, null, null, null, null, pageNumber: 1, pageSize: 10))
        {
            page1.Add(item);
        }
        Assert.Equal(11, page1.Count);

        // Страница 3 с размером 10 (последняя страница): записей всего 30, значит вернет ровно 10
        var page3 = new List<Application.DTOs.RecordDTO>();
        await foreach (var item in repository.GetFilteredRecordsAsync(
            null, null, null, null, null, null, pageNumber: 3, pageSize: 10))
        {
            page3.Add(item);
        }
        Assert.Equal(10, page3.Count);
    }

    [Fact]
    public async Task GetFilteredRecordsAsync_FullExport_ReturnsAllRecords()
    {
        var factory = new TestDbContextFactory();
        const int totalCount = 500;
        using (var seedContext = factory.CreateDbContext())
        {
            for (var i = 1; i <= totalCount; i++)
            {
                seedContext.Records.Add(new Record
                {
                    Date = DateTime.Today,
                    FirstName = $"First{i}",
                    LastName = $"Last{i}",
                    City = "City",
                    Country = "Country",
                });
            }
            await seedContext.SaveChangesAsync();
        }

        var repository = new RecordRepository(factory);
        var all = new List<Application.DTOs.RecordDTO>();
        await foreach (var item in repository.GetFilteredRecordsAsync(
            null, null, null, null, null, null, pageNumber: 1, pageSize: int.MaxValue))
        {
            all.Add(item);
        }

        Assert.Equal(totalCount, all.Count);
    }
}
