using System;
using System.Collections.Generic;

namespace EventManagement.Dto
{
    /// <summary>
    /// CampaignInstagram Model
    /// </summary>
    public class CampaignInstagramDto : CampaignInstagramAbstractBase
    {

    }

    public class InstaList
    {
        public string name { get; set; }
        public string id { get; set; }
        public string username { get; set; }
    }

    public class InstaLocaleCode
    {
        public string code { get; set; }
        public string name { get; set; }
    }

    public class RootObjectInstaData
    {
        public List<InstaList> data { get; set; }
        public string errror_msg { get; set; }
    }
}
