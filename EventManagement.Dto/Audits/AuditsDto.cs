using System;
using System.Collections.Generic;

namespace EventManagement.Dto
{
    /// <summary>
    /// Audits Model
    /// </summary>
    public class AuditsDto : AuditsAbstractBase
    {

        public Guid Id { get; set; }

        public string WebsiteUrl { get; set; }

       
        public string Grade { get; set; }

        public bool IsSent { get; set; }

        
        public string Status { get; set; }

        public long TaskId { get; set; }

        public long CreatedOn { get; set; }

    }
     
}
