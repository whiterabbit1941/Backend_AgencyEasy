using System;
using System.Collections.Generic;

namespace EventManagement.Dto
{
    /// <summary>
    /// CampaignGSC Model
    /// </summary>
    public class CampaignGSCDto : CampaignGSCAbstractBase
    {

    }

    public class RootObjectOfGSCList
    {
        public List<SiteEntry> siteEntry { get; set; }
}
 
    public class SiteEntry
    {
        public string siteUrl { get; set; }
        public string permissionLevel { get; set; }
    }

}
