using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessingApp.Application
{
    public class RecordDTO
    {
        public DateTime Date { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string SurName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }
}
