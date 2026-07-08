using ClosedXML.Excel;
using ProcessingApp.Application.Interfaces;
using ProcessingApp.Domain;
using ProcessingApp.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProcessingApp.Infrastructure
{
    public class ExcelExporter : IExcelExporter
    {
        public async Task ExportToExcelAsync(IAsyncEnumerable<RecordDTO> records, string filePath)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Экспорт Данных");

            worksheet.Cell(1, 1).Value = "№";
            worksheet.Cell(1, 2).Value = "Дата";
            worksheet.Cell(1, 3).Value = "Имя";
            worksheet.Cell(1, 4).Value = "Фамилия";
            worksheet.Cell(1, 5).Value = "Отчество";
            worksheet.Cell(1, 6).Value = "Город";
            worksheet.Cell(1, 7).Value = "Страна";

            worksheet.Row(1).Style.Font.Bold = true;

            int currentRow = 2;

            await foreach (var record in records)
            {
                worksheet.Cell(currentRow, 1).Value = currentRow - 1;
                worksheet.Cell(currentRow, 2).Value = record.Date.ToString("dd.MM.yyyy");
                worksheet.Cell(currentRow, 3).Value = record.FirstName;
                worksheet.Cell(currentRow, 4).Value = record.LastName;
                worksheet.Cell(currentRow, 5).Value = record.SurName;
                worksheet.Cell(currentRow, 6).Value = record.City;
                worksheet.Cell(currentRow, 7).Value = record.Country;
                currentRow++;
            }

            worksheet.Columns().AdjustToContents();

            workbook.SaveAs(filePath);
        }
    }
}