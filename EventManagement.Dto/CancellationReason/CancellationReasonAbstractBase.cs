using System;

namespace EventManagement.Dto
{
    public abstract class CancellationReasonAbstractBase
    {
        /// <summary>
        /// CancellationReason Id.
        /// </summary>
        public Guid Id { get; set; }

        public Guid CompanyId { get; set; }

        public string Reason { get; set; }

        public string OtherSolution { get; set; }

        public string Rating { get; set; }

        public string Feedback { get; set; }
    }
}
