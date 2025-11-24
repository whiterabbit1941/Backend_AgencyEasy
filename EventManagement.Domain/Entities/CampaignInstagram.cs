using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagement.Domain.Entities
{
    public class CampaignInstagram : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string UrlOrName { get; set; }
        public Guid CampaignID { get; set; }
        public bool IsActive { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string PageToken { get; set; }

        [ForeignKey("CampaignID")]
        public virtual Campaign Campaign { get; set; }
    }

    public class InstagramReportsData
    {
        public string errror_msg { get; set; }

        public int ProfileViewTotal { get; set; }
        public int FollowersTotal { get; set; }
        public int InstaFollowersCountTotal { get; set; }
        public int ImpressionsTotal { get; set; }
        public int ReachTotal { get; set; }
        public int WebsiteClickTotal { get; set; }

        public int AvgProfileViews { get; set; }
        public int AvgFollowers { get; set; }
        public int AvgImpressions { get; set; }
        public int AvgReachTotals { get; set; }
        public int AvgWebSiteClickTotal { get; set; }


        public List<int> GenderDataChart { get; set; }
        public List<ListOfInstaLocale> ListOfLocale { get; set; }
        public List<ListOfInstaLocale> ListOfCountries { get; set; }
        public List<ListOfInstaLocale> ListOfCities { get; set; }

    }

    public class ListOfInstaLocale
    {
        public string name { get; set; }
        public long value { get; set; }
    }
}
