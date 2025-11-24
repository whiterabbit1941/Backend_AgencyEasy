using System;

namespace EventManagement.Dto
{
    public abstract class AuditsAbstractBase
    {
        /// <summary>
        /// Audits Id.
        /// </summary>
        public Guid Id { get; set; }

        public string WebsiteUrl { get; set; }


        public string Grade { get; set; }

        public bool IsSent { get; set; }


        public string Status { get; set; }

        public long TaskId { get; set; }

    }
}
