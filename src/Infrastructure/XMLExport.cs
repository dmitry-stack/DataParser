using ProcessingApp.Application.Interfaces;
using ProcessingApp.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace ProcessingApp.Infrastructure
{
    public class XmlExporter : IXmlExporter
    {

   

    public async Task ExportToXmlAsync(IAsyncEnumerable<RecordDTO> records, string filePath)
    {
        var settings = new XmlWriterSettings { Async = true, Indent = true };

        using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
        using var writer = XmlWriter.Create(stream, settings);

        await writer.WriteStartDocumentAsync();
        await writer.WriteStartElementAsync(null, "TestProgram", null); 

        int currentIndex = 1;
        await foreach (var record in records)
        {
            await writer.WriteStartElementAsync(null, "Record", null); 
            await writer.WriteAttributeStringAsync(null, "id", null, currentIndex.ToString()); 

            await writer.WriteElementStringAsync(null, "Date", null, record.Date.ToString("dd.MM.yyyy"));
            await writer.WriteElementStringAsync(null, "FirstName", null, record.FirstName);
            await writer.WriteElementStringAsync(null, "LastName", null, record.LastName);
            await writer.WriteElementStringAsync(null, "SurName", null, record.SurName);
            await writer.WriteElementStringAsync(null, "City", null, record.City);
            await writer.WriteElementStringAsync(null, "Country", null, record.Country);

            await writer.WriteEndElementAsync(); 
            currentIndex++;
        }

        await writer.WriteEndElementAsync(); 
        await writer.WriteEndDocumentAsync();
        await writer.FlushAsync();
    }
}
}
