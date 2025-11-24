using System;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.Dto
{
    public abstract class CampaignGBPAbstractBase
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid CampaignID { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string EmailId { get; set; }

        public string AccountId { get; set; }
    }
}
