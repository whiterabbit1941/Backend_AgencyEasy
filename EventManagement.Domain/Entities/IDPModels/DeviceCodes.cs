using System;
using System.Collections.Generic;

namespace FinanaceManagement.API.Models
{
    public partial class DeviceCodes
    {
        public string DeviceCode { get; set; }
        public string UserCode { get; set; }
        public string SubjectId { get; set; }
        public string ClientId { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime Expiration { get; set; }
        public string Data { get; set; }
    }
}
