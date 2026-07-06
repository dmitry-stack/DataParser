using ProcessingApp.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessingApp.Infrastructure
{
    public class RecordRepository
    {
        public List<RecordDTO> GetFilteredRecords(DateTime? date, string? firstName,string? surName, string? country, string? city, string? lastName)
        {
            using var context = new AppDbContext();

            IQueryable<Record> query = context.Records;

            if (!string.IsNullOrWhiteSpace(city))
            {
                
                query = query.Where(x => x.City == city.Trim());
            }


            if (!string.IsNullOrWhiteSpace(lastName))
            {
                query = query.Where(x => x.LastName == lastName.Trim());
            }

            if (!string.IsNullOrWhiteSpace(firstName))
            {
                query = query.Where(x => x.FirstName == firstName.Trim());
            }

            if (!string.IsNullOrWhiteSpace(surName))
            {
                query = query.Where(x => x.SurName == surName.Trim());
            }

            if (!string.IsNullOrWhiteSpace(country))
            {
                query = query.Where(x => x.Country == country.Trim());
            }

            if (date.HasValue)
            {
                query = query.Where(x => x.Date == date.Value.Date);
            }

           
            return query.Select(x => new RecordDTO
            {
                Date = x.Date,
                FirstName = x.FirstName,
                LastName = x.LastName,
                SurName = x.SurName,
                City = x.City,
                Country = x.Country
            }).ToList();
        }
    }
}
