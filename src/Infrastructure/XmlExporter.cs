using ProcessingApp.Application.DTOs;
using ProcessingApp.Application.Interfaces;
using System.Xml;

namespace ProcessingApp.Infrastructure
{
    public class XmlExporter : IExporter
    {

        public string SupportedExtension => ".xml";

        public async Task ExportAsync(IAsyncEnumerable<RecordDTO> records, string filePath, CancellationToken token = default)
        {
            var settings = new XmlWriterSettings { Indent = true, Async = true };

            using var writer = XmlWriter.Create(filePath, settings);
            await writer.WriteStartDocumentAsync();
            await writer.WriteStartElementAsync(null, "Records", null);

            await foreach (var record in records.WithCancellation(token))
            {
                await writer.WriteStartElementAsync(null, "Record", null);
                await writer.WriteElementStringAsync(null, "FirstName", null, record.FirstName);
                await writer.WriteElementStringAsync(null, "Date", null, record.Date.ToString("dd.MM.yyyy"));
                await writer.WriteElementStringAsync(null, "LastName", null, record.LastName);
                await writer.WriteElementStringAsync(null, "SurName", null, record.SurName);
                await writer.WriteElementStringAsync(null, "City", null, record.City);
                await writer.WriteElementStringAsync(null, "Country", null, record.Country);

                await writer.WriteEndElementAsync();

            }

            await writer.WriteEndElementAsync();
            await writer.WriteEndDocumentAsync();
            await writer.FlushAsync();
        }
    }
}
