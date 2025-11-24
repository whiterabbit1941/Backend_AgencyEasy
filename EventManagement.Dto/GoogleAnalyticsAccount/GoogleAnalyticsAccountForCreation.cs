using System;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.Dto
{
    public class GoogleAnalyticsAccountForCreation : GoogleAnalyticsAccountAbstractBase
    {
   

        public string CampaignID { get; set; }


        public Guid GoogleAccountSetupID { get; set; }



        public string AccountID { get; set; }



        public string AccountName { get; set; }


        public string WebsiteUrl { get; set; }



        public string PropertyID { get; set; }



        public string ViewName { get; set; }



        public string ViewID { get; set; }


    }
}
