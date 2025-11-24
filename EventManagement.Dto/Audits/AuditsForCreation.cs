using System;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.Dto
{
    public class AuditsForCreation : AuditsAbstractBase
    {

        public Guid Id { get; set; }

        public string WebsiteUrl { get; set; }


        public string Grade { get; set; }

        public bool IsSent { get; set; }


        public string Status { get; set; }

        public long TaskId { get; set; }

        public string CompanyID { get; set; }
    }
}
