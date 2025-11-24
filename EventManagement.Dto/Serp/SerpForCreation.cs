using System;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.Dto
{
    public class SerpForCreation : SerpAbstractBase
    {
        public Guid Id { get; set; }

        public Guid CampaignID { get; set; }

        public long Position { get; set; }

        public long LocalPackCount { get; set; }

        public long Searches { get; set; }

        public string Location { get; set; }

        public string Keywords { get; set; }

        public string LambdaLogger { get; set; }

        public string BusinessName { get; set; }

        public bool LocalPacksStatus { get; set; }

        public string TaskId { get; set; }

        public bool IsWebhookRecieved { get; set; }
    }
}
