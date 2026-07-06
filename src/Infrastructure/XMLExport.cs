using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using ProcessingApp.Domain;

namespace ProcessingApp.Infrastructure
{
    public class XmlExporter
    {

        public void ExportToXml(List<RecordDTO> records, string filePath)
        {
            var root = new XElement("TestProgram");
            int currentIndex = 1;

            foreach (var record in records)
            {
                var recordElement = new XElement("Record", new XAttribute("id", currentIndex),
                     new XElement("Date", record.Date.ToString("dd.MM.yyyy")),
                    new XElement("FirstName", record.FirstName),
                    new XElement("LastName", record.LastName),
                    new XElement("SurName", record.SurName),
                    new XElement("City", record.City),
                    new XElement("Country", record.Country)
                );

                root.Add(recordElement);
                currentIndex++;
            }

            var doc = new XDocument(root);
            doc.Save(filePath);
        }
    }
}
