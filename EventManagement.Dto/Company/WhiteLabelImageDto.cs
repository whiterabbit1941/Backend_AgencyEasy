using System;

namespace EventManagement.Dto
{
    /// <summary>
    /// Company Model
    /// </summary>
    public class WhiteLabelImageDto
    {
        public Guid CompanyId { get; set; }
        public string FileName { get; set; }
        public string ImageBase64 { get; set; }
        public string Fevicon { get; set; }

    }
}