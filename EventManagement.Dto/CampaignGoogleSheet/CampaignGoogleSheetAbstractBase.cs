using System;

namespace EventManagement.Dto
{
    public abstract class CampaignGoogleSheetAbstractBase
    {
        
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid CampaignID { get; set; }
        public string AccountId { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string EmailId { get; set; }       
        public string Settings { get; set; }

    }
}
