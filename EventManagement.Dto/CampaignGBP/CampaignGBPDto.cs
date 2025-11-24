using System;
using System.Collections.Generic;
using System.Net;

namespace EventManagement.Dto
{
    /// <summary>
    /// CampaignGBP Model
    /// </summary>
    public class CampaignGBPDto : CampaignGBPAbstractBase
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid CampaignID { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string EmailId { get; set; }

        public string AccountId { get; set; }
    }

    public class Account
    {
        public string name { get; set; }
        public string accountName { get; set; }
    }

    public class GbpAccounts
    {
        public List<Account> accounts { get; set; }
    }

    public class GbpLocation
    {
        public string accountId { get; set; }
        public string title { get; set; }
        public string address { get; set; }
    }


    public class Location
    {
        public string name { get; set; }
        public string title { get; set; }
        public StorefrontAddress storefrontAddress { get; set; }
        public string websiteUri { get; set; }
        public ServiceArea serviceArea { get; set; }
        public string storeCode { get; set; }
        public List<string> labels { get; set; }
    }

    public class PlaceInfo
    {
        public string placeName { get; set; }
        public string placeId { get; set; }
    }

    public class Places
    {
        public List<PlaceInfo> placeInfos { get; set; }
    }

    public class GbpLocationResponse
    {
        public List<GbpLocation> gbpLocations { get; set; }
    }

    public class GbpLocations
    {
        public List<Location> locations { get; set; }
        public string nextPageToken { get; set; }
    }

    public class ServiceArea
    {
        public string businessType { get; set; }
        public Places places { get; set; }
    }

    public class StorefrontAddress
    {
        public string regionCode { get; set; }
        public string languageCode { get; set; }
        public string postalCode { get; set; }
        public string administrativeArea { get; set; }
        public string locality { get; set; }
        public List<string> addressLines { get; set; }
    }


    public class DailyMetricTimeSeries
    {
        public string dailyMetric { get; set; }
        public TimeSeries timeSeries { get; set; }
    }

    public class Date
    {
        public int year { get; set; }
        public int month { get; set; }
        public int day { get; set; }
    }

    public class DatedValue
    {
        public Date date { get; set; }
        public string value { get; set; } = "0";
    }

    public class MultiDailyMetricTimeSeries
    {
        public List<DailyMetricTimeSeries> dailyMetricTimeSeries { get; set; }
    }

    public class GbpMetricsData
    {
        public List<MultiDailyMetricTimeSeries> multiDailyMetricTimeSeries { get; set; }
    }

    public class RootGbpData
    {
        public int TotalInteraction { get; set; }
        public int TotalCalls { get; set; }

        public int TotalDirections { get; set; }

        public int TotalWebsiteClick { get; set; }

        public int TotalFoodOrder{ get; set; }

        public int TotalMessage { get; set; }

        public int TotalBooking { get; set; }

        public int TotalProfileView { get; set; }

        public int TotalSearchMobile { get; set; }

        public int TotalSearchDesktop { get; set; }

        public int TotalMapDesktop { get; set; }

        public int TotalMapMobile { get; set; }

        public int PrevTotalProfileView { get; set; }
        public int[] CallChartData { get; set; }

        public int[] DirectionChartData { get; set; }

        public int[] WebsiteChartData { get; set; }

        public int[] MessageChartData { get; set; }

        public int[] BookingChartData { get; set; }

        public int[] InteractionChartData { get; set; }

        public decimal PercentGoogleSearchMobile { get; set; }

        public decimal PercentGoogleMapDesktop { get; set; }

        public decimal PercentGoogleSearchDesktop { get; set; }

        public decimal PercentGoogleMapMobile { get; set; }

        //Previous
        public int PrevTotalInteraction { get; set; }
        public int PrevTotalCalls { get; set; }
        public int PrevTotalDirections { get; set; }
        public int PrevTotalWebsiteClick { get; set; }

        public int PrevTotalFoodOrder { get; set; }

        public int PrevTotalMessage { get; set; }

        public int PrevTotalBooking { get; set; }


        //public int[] PrevCallChartData { get; set; }
        //public int[] PrevDirectionChartData { get; set; }
        //public int[] PrevWebsiteChartData { get; set; }
        //public int[] PrevMessageChartData { get; set; }
        //public int[] PrevBookingChartData { get; set; }
        //public int[] PrevInteractionChartData { get; set; }

        //public decimal PrevPercentGoogleSearchMobile { get; set; }
        //public decimal PrevPercentGoogleMapDesktop { get; set; }
        //public decimal PrevPercentGoogleSearchDesktop { get; set; }
        //public decimal PrevPercentGoogleMapMobile { get; set; }

        public List<SearchKeywordsCount> KeywordData { get; set; }

        public int TotalSearchKeyword { get; set; }

        public int PrevTotalSearchKeyword { get; set; }

        public string InteractionCardPercent { get; set; } = "0%";

        public string CallCardPercent { get; set; } = "0%";

        public string DirectionCardPercent { get; set; } = "0%";

        public string WebsiteCardPercent { get; set; } = "0%";

        public string ProfileViewDiff { get; set; }

        public string SearchKeywordDiff { get; set; }

        public string ProfileInteractionDiff { get; set; }

        public string CallDiff { get; set; }

        public string MessageDiff { get; set; }

        public string BookingDiff { get; set; }

        public string WebsiteDiff { get; set; }

        public string DirectionDiff { get; set; }

        public List<string> DateLabels { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public string VsDateRange { get; set; }

    }

    public class TimeSeries
    {
        public List<DatedValue> datedValues { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class InsightsValue
    {
        public string value { get; set; }
        public string threshold { get; set; }
    }

    public class GbpKeywords
    {
        public List<SearchKeywordsCount> searchKeywordsCounts { get; set; }
        public HttpStatusCode StatusCode { get; set; }

    }

    public class SearchKeywordsCount
    {
        public string searchKeyword { get; set; }
        public InsightsValue insightsValue { get; set; }
    }

}
