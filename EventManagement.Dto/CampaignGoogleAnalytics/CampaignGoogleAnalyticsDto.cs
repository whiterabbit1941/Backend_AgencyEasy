using System;
using System.Collections.Generic;

namespace EventManagement.Dto
{
    /// <summary>
    /// CampaignGoogleAnalytics Model
    /// </summary>
    public class CampaignGoogleAnalyticsDto : CampaignGoogleAnalyticsAbstractBase
    {

    }


    public class GaToken
    {
        public string access_token { get; set; }

        public string expires_in { get; set; }

        public string refresh_token { get; set; }

        public string refresh_token_expires_in { get; set; }

        public string scope { get; set; }

        public string email { get; set; }

        public string name { get; set; }    



    }

    public class GoogleType
    {
        public string ga { get; set; }

        public string gsc { get; set; }

        public string gads { get; set; }
    }

    public class GAItem
    {
        public string id { get; set; }
        public string kind { get; set; }
        public string name { get; set; }
        public List<WebProperty> webProperties { get; set; }
    }

    public class Profile
    {
        public string kind { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public bool? starred { get; set; }
    }

    public class RootObjectGoogleAnayltics
    {
        public string kind { get; set; }
        public string username { get; set; }
        public int totalResults { get; set; }
        public int startIndex { get; set; }
        public int itemsPerPage { get; set; }
        public List<GAItem> items { get; set; }
    }

    public class WebProperty
    {
        public string kind { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string internalWebPropertyId { get; set; }
        public string level { get; set; }
        public string websiteUrl { get; set; }
        public List<Profile> profiles { get; set; }
    }

    public class RootObjectOfGoogleEmail
    {
        public string id { get; set; }
        public string email { get; set; }
        public bool verified_email { get; set; }
        public string picture { get; set; }
        public string hd { get; set; }
    }

    //Google Analytics4
    public class Ga4AccountSummary
    {
        public string name { get; set; }
        public string account { get; set; }
        public string displayName { get; set; }
        public List<Ga4PropertySummary> propertySummaries { get; set; }
    }
    public class Ga4PropertySummary
    {
        public string property { get; set; }
        public string displayName { get; set; }
        public string propertyType { get; set; }
        public string parent { get; set; }
    }
    public class Ga4RootList
    {
        public List<Ga4AccountSummary> accountSummaries { get; set; }
        public string nextPageToken { get; set; }
    }

}
