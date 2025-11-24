using System;
using System.Collections.Generic;

namespace EventManagement.Dto
{
    /// <summary>
    /// GoogleAnalyticsAccount Model
    /// </summary>
    public class GoogleAnalyticsAccountDto : GoogleAnalyticsAccountAbstractBase
    {

        public Guid Id { get; set; }

        public string CampaignID { get; set; }


        public Guid GoogleAccountSetupID { get; set; }


       
        public string AccountID { get; set; }


    
        public string AccountName { get; set; }


        public string WebsiteUrl { get; set; }


     
        public string PropertyID { get; set; }



     
        public string ViewName { get; set; }


  
        public string ViewID { get; set; }

        public GoogleAccountSetupDto  GoogleAccountSetups { get; set; }



    }

}
 