using System;
using System.Collections.Generic;

namespace DecentraCloud.API.DTOs
{
    public class FileRecordDto
    {
        public string Id { get; set; }
        public string Filename { get; set; }
        public long Size { get; set; }
        public DateTime DateAdded { get; set; }
        public List<string> SharedWithEmails { get; set; }
    }
}
