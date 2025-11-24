using EventManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace EventManagement.Dto
{
    /// <summary>
    /// ReportScheduling Model
    /// </summary>
    public class ReportSchedulingDto : ReportSchedulingAbstractBase
    {

    }

    #region Google Analytics

    public class GoogleAnalyticsResponseDto
    {

        public int totalResults { get; set; }

        public List<List<string>> rows { get; set; }

        public HttpStatusCode statusCode { get; set; }


    }

    public class GaIntegrationData
    {
        public GoogleAnalyticsDataDto GoogleAnalyticsDataDto { get; set; }

        public GoogleAnalyticsDataDto PreviousGoogleAnalyticsDataDto { get; set; }

        public string HtmlString { get; set; }

    }

    public class GscIntegrationData
    {
        public string CurrentImpression { get; set; }

        public string PreviousImpression { get; set; }

        public List<Dictionary<string, string>> HtmlString { get; set; }

        public GscRawData GscRawData { get; set; }

        public HttpStatusCode StatusCode { get; set; }
    }




    public class GoogleAnalyticsDataDto
    {
        public int[] WebTrafficByDevice { get; set; }

        public int[] WebTrafficData { get; set; }

        public string HtmlString { get; set; }

        public int[] Conversions { get; set; }
    }

    public class PageSpeedLightHouseDto
    {
        public string Desktop { get; set; }

        public string Mobile { get; set; }
    }

    public class PageSpeedDesktopDataDto
    {
        public string DesktopPerformanceScore { get; set; }


        public string HtmlString { get; set; }
    }

    public class PageSpeedMobileDataDto
    {
        public string MobilePerformanceScore { get; set; }


        public string HtmlString { get; set; }
    }

    public class gaConversionDto
    {
        public string date { get; set; }
        public int value { get; set; }
    }

    #endregion

    #region LinkedIn


    public class TimeRange
    {
        public object start { get; set; }
        public object end { get; set; }
    }
    public class Link
    {
        public string type { get; set; }
        public string rel { get; set; }
        public string href { get; set; }
    }
    public class Paging
    {
        public int start { get; set; }
        public int count { get; set; }
        public List<Link> links { get; set; }
        public int total { get; set; }
    }
    public class FollowerGains
    {
        public int organicFollowerGain { get; set; }
        public int paidFollowerGain { get; set; }
    }
    public class ElementFollowerGains
    {
        public FollowerGains followerGains { get; set; }
        public string organizationalEntity { get; set; }
        public TimeRange timeRange { get; set; }
    }
    public class RootFollowerGains
    {
        public Paging paging { get; set; }
        public List<ElementFollowerGains> elements { get; set; }
    }
    public class TotalShareStatistics
    {
        public int uniqueImpressionsCount { get; set; }
        public int shareCount { get; set; }
        public double engagement { get; set; }
        public int clickCount { get; set; }
        public int likeCount { get; set; }
        public int impressionCount { get; set; }
        public int commentCount { get; set; }
    }
    public class ElementTotalShareStatistics
    {
        public TotalShareStatistics totalShareStatistics { get; set; }
        public string organizationalEntity { get; set; }
        public TimeRange timeRange { get; set; }
    }
    public class RootTotalShareStatistics
    {
        public Paging paging { get; set; }
        public List<ElementTotalShareStatistics> elements { get; set; }
    }
    public class RootLinkedInDataObject
    {
        public List<string> Dates { get; set; }
        public List<FollowerGains> FollowerGains { get; set; }
        public List<TotalShareStatistics> ShareStatistics { get; set; }
    }



    public class LinkedInDataDto
    {
        public int[][] followerGainsDataSet { get; set; }

        public int[][] ShareStatisticsDataSet { get; set; }
    }

    //Linkedin Demographic

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class DemographicElement
    {
        public List<FollowerCountsByAssociationType> followerCountsByAssociationType { get; set; }
        public List<FollowerCountsByRegion> followerCountsByRegion { get; set; }
        public List<FollowerCountsBySeniority> followerCountsBySeniority { get; set; }
        public List<FollowerCountsByIndustry> followerCountsByIndustry { get; set; }
        public List<FollowerCountsByStaffCountRange> followerCountsByStaffCountRange { get; set; }
        public List<FollowerCountsByFunction> followerCountsByFunction { get; set; }
        public List<FollowerCountsByCountry> followerCountsByGeoCountry { get; set; }
        public string organizationalEntity { get; set; }
    }

    public class FollowerCounts
    {
        public int organicFollowerCount { get; set; }
        public int paidFollowerCount { get; set; }
    }

    public class FollowerCountsByAssociationType
    {
        public FollowerCounts followerCounts { get; set; }
        public string associationType { get; set; }
    }

    public class FollowerCountsByCountry
    {
        public FollowerCounts followerCounts { get; set; }
        public string country { get; set; }

        public string geo { get; set; }

        public string countryName { get; set; }
    }

    public class FollowerCountsByFunction
    {
        public FollowerCounts followerCounts { get; set; }
        public string function { get; set; }

        public string name { get; set; }
    }

    public class FollowerCountsByIndustry
    {
        public FollowerCounts followerCounts { get; set; }
        public string industry { get; set; }
        public string name { get; set; }
    }

    public class FollowerCountsByRegion
    {
        public string region { get; set; }
        public FollowerCounts followerCounts { get; set; }
    }

    public class FollowerCountsBySeniority
    {
        public FollowerCounts followerCounts { get; set; }
        public string seniority { get; set; }

        public string name { get; set; }

    }

    public class FollowerCountsByStaffCountRange
    {
        public FollowerCounts followerCounts { get; set; }
        public string staffCountRange { get; set; }

        public string name { get; set; }
    }

    public class DemographicPaging
    {
        public int start { get; set; }
        public int count { get; set; }
        public List<object> links { get; set; }
    }

    public class LinkedinDemographic
    {
        public DemographicPaging paging { get; set; }
        public List<DemographicElement> elements { get; set; }
    }

    public class CompanySize
    {
        public string name { get; set; }
        public string code { get; set; }
    }

    public class CountriesName
    {
        public string name { get; set; }
        public string code { get; set; }
    }

    public class Industry
    {
        [JsonProperty("$URN")]
        public string URN { get; set; }
        public Name name { get; set; }
        public int id { get; set; }
    }

    public class JobTitle
    {
        [JsonProperty("$URN")]
        public string URN { get; set; }
        public Name name { get; set; }
        public int id { get; set; }
    }

    public class JobFunction
    {
        [JsonProperty("$URN")]
        public string URN { get; set; }
        public Name name { get; set; }
        public int id { get; set; }
    }

    public class Localized
    {
        public string en_US { get; set; }
    }

    public class Name
    {
        public Localized localized { get; set; }
    }

    public class LinkedInDemographicCode
    {
        public List<CountriesName> countries_name { get; set; }
        public List<Seniority> seniority { get; set; }
        public List<Industry> industries { get; set; }
        public List<JobFunction> job_function { get; set; }
        public List<CompanySize> company_size { get; set; }
    }


    public class DefaultLocalizedName
    {
        public string value { get; set; }
    }


    public class CountryLocation
    {
        public DefaultLocalizedName defaultLocalizedName { get; set; }
        public int id { get; set; }
    }



    public class Seniority
    {
        public string code { get; set; }
        public string name { get; set; }
    }

    public class LinkedInDemographicChart
    {
        public List<int> CountryData { get; set; }
        public List<string> CountryLabel { get; set; }

        public List<int> SeniorityData { get; set; }
        public List<string> SeniorityLabel { get; set; }

        public List<int> IndustryData { get; set; }
        public List<string> IndustryLabel { get; set; }

        public List<int> JobFunctionData { get; set; }
        public List<string> JobFunctionLabel { get; set; }

        public List<int> CompanySizeData { get; set; }
        public List<string> CompanySizeLabel { get; set; }




    }




    #endregion

    #region Google Search Console


    public class Row
    {
        public List<string> keys { get; set; }
        public int clicks { get; set; }
        public int impressions { get; set; }
        public double ctr { get; set; }
        public double position { get; set; }
    }

    public class GscData
    {
        public List<Row> rows { get; set; }
        public string responseAggregationType { get; set; }

        public HttpStatusCode statusCode { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);


    public class GscChartResponse
    {
        public List<Row> rows { get; set; }
        public string responseAggregationType { get; set; }
    }

    public class PreviousDate
    {
        public DateTime PreviousStartDate { get; set; }

        public DateTime PreviousEndDate { get; set; }
    }







    #endregion

    #region Google Ads

    public class GaAdsCampaigns
    {
        public string campName { get; set; }

        public int click { get; set; }

        public int impression { get; set; }
        public decimal vtc { get; set; }
        public decimal cost { get; set; }
        public decimal cost_conv { get; set; }
        public int conv { get; set; }
        public decimal avg_cpc { get; set; }
        public decimal conv_rate { get; set; }


    }
    #endregion

    #region Instagram

    public class InstagramResModel
    {
        public List<List<string>> rows { get; set; }
    }
    public class InstaListOfFBPages
    {
        public string access_token { get; set; }

        public string name { get; set; }

        public string id { get; set; }


    }

    public class InstagramIdModel
    {
        public string instgramid { get; set; }

        public string accessToken { get; set; }



    }

    public class InstagramPageModel
    {
        public string instgramid { get; set; }

        public string accessToken { get; set; }

        public string pageName { get; set; }

        public string username { get; set; }



    }

    public class Value2
    {
        [JsonProperty("F.13-17")]
        public int F1317 { get; set; }

        [JsonProperty("F.18-24")]
        public int F1824 { get; set; }

        [JsonProperty("F.25-34")]
        public int F2534 { get; set; }

        [JsonProperty("F.35-44")]
        public int F3544 { get; set; }

        [JsonProperty("F.45-54")]
        public int F4554 { get; set; }

        [JsonProperty("F.55-64")]
        public int F5564 { get; set; }

        [JsonProperty("F.65+")]
        public int F65 { get; set; }

        [JsonProperty("M.13-17")]
        public int M1317 { get; set; }

        [JsonProperty("M.18-24")]
        public int M1824 { get; set; }

        [JsonProperty("M.25-34")]
        public int M2534 { get; set; }

        [JsonProperty("M.35-44")]
        public int M3544 { get; set; }

        [JsonProperty("M.45-54")]
        public int M4554 { get; set; }

        [JsonProperty("M.55-64")]
        public int M5564 { get; set; }

        [JsonProperty("M.65+")]
        public int M65 { get; set; }

        [JsonProperty("U.13-17")]
        public int U1317 { get; set; }

        [JsonProperty("U.18-24")]
        public int U1824 { get; set; }

        [JsonProperty("U.25-34")]
        public int U2534 { get; set; }

        [JsonProperty("U.35-44")]
        public int U3544 { get; set; }

        [JsonProperty("U.45-54")]
        public int U4554 { get; set; }

        [JsonProperty("U.55-64")]
        public int U5564 { get; set; }

    }
    public class ageGenderDto
    {
        public List<string> data { get; set; }
        //public string name { get; set; }
        //public string period { get; set; }
        //public List<List<string>> values { get; set; }

        //public string title { get; set; }

        //public string description { get; set; }
    }


    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

    public class instaGender
    {
        public List<Attributes> data { get; set; }
    }

    public class Attributes
    {
        public string name { get; set; }
        public string period { get; set; }
        public List<Value5> values { get; set; }
        public string title { get; set; }

        public string description { get; set; }
    }

    public class Value5
    {
        public Object value { get; set; }

    }
    public class Value3
    {
        [JsonProperty("F.13-17")]
        public int F1317 { get; set; }

        [JsonProperty("F.18-24")]
        public int F1824 { get; set; }

        [JsonProperty("F.25-34")]
        public int F2534 { get; set; }

        [JsonProperty("F.35-44")]
        public int F3544 { get; set; }

        [JsonProperty("F.45-54")]
        public int F4554 { get; set; }

        [JsonProperty("F.55-64")]
        public int F5564 { get; set; }

        [JsonProperty("F.65+")]
        public int F65 { get; set; }

        [JsonProperty("M.13-17")]
        public int M1317 { get; set; }

        [JsonProperty("M.18-24")]
        public int M1824 { get; set; }

        [JsonProperty("M.25-34")]
        public int M2534 { get; set; }

        [JsonProperty("M.35-44")]
        public int M3544 { get; set; }

        [JsonProperty("M.45-54")]
        public int M4554 { get; set; }

        [JsonProperty("M.55-64")]
        public int M5564 { get; set; }

        [JsonProperty("M.65+")]
        public int M65 { get; set; }

        [JsonProperty("U.13-17")]
        public int U1317 { get; set; }

        [JsonProperty("U.18-24")]
        public int U1824 { get; set; }

        [JsonProperty("U.25-34")]
        public int U2534 { get; set; }

        [JsonProperty("U.35-44")]
        public int U3544 { get; set; }

        [JsonProperty("U.45-54")]
        public int U4554 { get; set; }

        [JsonProperty("U.55-64")]
        public int U5564 { get; set; }

        [JsonProperty("U.65+")]
        public int U65 { get; set; }
    }

    public class Value
    {
        public Value value { get; set; }
        public DateTime end_time { get; set; }
    }


    public class listOfLocale
    {
        public string keyName { get; set; }
        public long keyValue { get; set; }
    }


    public class FollowerDemographicsDto
    {
        public List<DataDto> data { get; set; }
    }

    public class DataDto
    {
        public string name { get; set; }
        public string period { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public TotalValueDto total_value { get; set; }
        public string id { get; set; }
    }

    public class TotalValueDto
    {
        public List<BreakdownDto> breakdowns { get; set; }
    }

    public class BreakdownDto
    {
        public List<string> dimension_keys { get; set; }
        public List<ResultDto> results { get; set; }
    }

    public class ResultDto
    {
        public List<string> dimension_values { get; set; }
        public int value { get; set; }

        public ListOfInstaLocale ToListOfInstaLocale()
        {
            return new ListOfInstaLocale
            {
                name = dimension_values[0], // Assuming dimension_values always contains at least one element
                value = value
            };
        }
    }

    #endregion


    #region Facebook Ads



    public class Data
    {
        public int data_access_expires_at { get; set; }
        public bool is_valid { get; set; }
        public Error error { get; set; }


    }
    public class Error
    {
        public int subcode { get; set; }

    }



    public class GraphApiTokenValid
    {
        public Data data { get; set; }
    }




    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Datum
    {
        public string account_id { get; set; }
        public string business_name { get; set; }
        public string id { get; set; }
        public string name { get; set; }
    }



    public class Adaccounts
    {
        public List<Datum> data { get; set; }
    }



    public class FacebookAdsAccount
    {
        public string id { get; set; }
        public string name { get; set; }
        [JsonIgnore]
        public string access_token { get; set; }
        [JsonIgnore]
        public string ad_account_name { get; set; }
        public Adaccounts adaccounts { get; set; }
    }

    public class RootObjectFBAdsData
    {
        public FacebookAdsAccount data { get; set; }
        public string errror_msg { get; set; }
    }



    public class FbAdsCampaignsData
    {
        public List<Datum> data { get; set; }

    }



    public class FbAdsCampaigns
    {
        public FbAdsCampaignsData campaigns { get; set; }
        public string currency { get; set; }
        public string id { get; set; }
    }



    public class FbAdsCampaignsDetails
    {
        public List<InsightsField> data { get; set; }
    }
    public class BodyData
    {
        public List<FbInsightData> data { get; set; }

    }

    public class BatchCampaignChartData
    {
        public List<int> Impressions { get; set; }
        public List<int> Reach { get; set; }
        public List<int> Click { get; set; }
        public List<decimal> Ctr { get; set; }
        public List<decimal> Spend { get; set; }
        public List<int> LinkClick { get; set; }
        public List<decimal> CPLC { get; set; }
        public List<decimal> CPC { get; set; }
        public string Currency { get; set; }

    }

    public class FbInsightData
    {
        public string impressions { get; set; }
        public string reach { get; set; }
        public string clicks { get; set; }
        public string ctr { get; set; }
        public string spend { get; set; }
        public string campaign_name { get; set; }
        public string adset_name { get; set; }
        public string campaign_id { get; set; }
        public string adset_id { get; set; }
        public string account_id { get; set; }
        public string date_start { get; set; }
        public string date_stop { get; set; }

        public string account_currency { get; set; }
    }

    public class BatchResponse
    {

        public string body { get; set; }
        public BodyData bodyData { get; set; }
    }



    public class BatchDto
    {
        public string method { get; set; }
        public string relative_url { get; set; }
    }

    public class FbAdsAdsCopiesData
    {
        public Ads ads { get; set; }

    }

    public class Ads
    {
        public List<InsightsFieldForCopies> data { get; set; }
    }

    //public class AdsetConfig
    //{
    //    public List<AdsetConfigField> data { get; set; }

    //}

    //public class AdsetConfigField
    //{
    //    public string id { get; set; }

    //    public string name { get; set; }

    //    public string optimization_goal { get; set; }

    //    public Promoted_object promoted_object { get; set; }


    //}

    public class Promoted_object
    {
        public string custom_conversion_id { get; set; }

        public string custom_event_type { get; set; }
    }


    public class InsightsField
    {
        public string clicks { get; set; }
        public string cost_per_unique_inline_link_click { get; set; }
        public string reach { get; set; }
        public string impressions { get; set; }
        public string results { get; set; }
        public string cpr { get; set; }
        public string spend { get; set; }
        public string inline_link_clicks { get; set; }
        public string ctr { get; set; }
        public string cpc { get; set; }
        public string date_start { get; set; }
        public string date_stop { get; set; }
        public string campaign_name { get; set; }
        public string campaign_id { get; set; }
        public string adset_id { get; set; }
        public string estimated_ad_recallers { get; set; }
        public string adset_name { get; set; }
        public string ad_name { get; set; }
        public string ad_image { get; set; }
        public List<InsightsFieldValue> video_15_sec_watched_actions { get; set; }

        public List<ActionObject> actions { get; set; }

        public string account_currency { get; set; }



    }







    public class InsightsFieldValue
    {
        public string value { get; set; }
    }

    public class ActionObject
    {
        public string action_type { get; set; }

        public string value { get; set; }

    }

    public class CampaignChartData
    {
        public int reach { get; set; }

        public int impression { get; set; }

        //public int click { get; set; }

        public int result { get; set; }

        public int spend { get; set; }

        public int costPerResult { get; set; }

        public int link { get; set; }

        public int ctr { get; set; }

        public int cplc { get; set; }

    }


    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class PublishedClick
    {
        public string clicks { get; set; }

        public string publisher_platform { get; set; }
    }

    public class PublisherPlatformData
    {
        public List<PublishedClick> data { get; set; }
    }



    public class PublisherPlatformResponse
    {
        public int FacebookClicks { get; set; }

        public int InstagramClicks { get; set; }

        public int AudienceNetworkClick { get; set; }
    }

    public class FacebookGetData
    {
        public FacebookAdsCampaignData facebookAdsCampaignData { get; set; }
        public string Error { get; set; } = "";
    }


    public class FacebookAdsCampaignData
    {
        //Current data
        public double totalClicks { get; set; }
        public decimal totalCtr { get; set; }
        public decimal totalSpend { get; set; }
        public double totalImpressions { get; set; }
        public double totalReachs { get; set; }
        public decimal totalCplc { get; set; }
        public double results { get; set; }
        public decimal spends { get; set; }
        public double cpr { get; set; }
        public double totalLinkClick { get; set; }
        public string currency { get; set; }
        public decimal ctr { get; set; }
        public decimal cpc { get; set; }




        public int[] ImpressionData { get; set; }
        public int[] ReachData { get; set; }
        public int[] ClickData { get; set; }
        public int[] ResultData { get; set; }
        public int[] CprData { get; set; }
        public decimal[] SpendData { get; set; }
        public decimal[] CpcData { get; set; }


        public int[] LinkClickData { get; set; }
        public decimal[] CtrData { get; set; }
        public decimal[] CplcData { get; set; }

        //PreviousData

        public int[] PrevImpressionData { get; set; }
        public int[] PrevClickData { get; set; }
        public int[] PrevReachData { get; set; }
        public int[] PrevResultData { get; set; }
        public int[] PrevCprData { get; set; }

        public decimal[] PrevCpcData { get; set; }
        public decimal[] PrevSpendData { get; set; }
        public int[] PrevLinkClickData { get; set; }
        public decimal[] PrevCtrData { get; set; }
        public decimal[] PrevCplcData { get; set; }

        public List<String> shortDate { get; set; }
        public List<InsightsField> listInsights { get; set; }

    }


    //New Region for facebook ads
    public class Adsets
    {
        public List<Datum> data;
        public Paging paging;
    }


    public class FbAdsSetResponse
    {
        public Adsets adsets { get; set; }
        public String id { get; set; }
        public String currency { get; set; }
    }

    public class PromotedObject
    {
        public String pixel_id { get; set; }
        public String custom_event_type { get; set; }

        public String custom_conversion_id { get; set; }

    }

    public class AdsetConfig
    {
        public String status { get; set; }
        public PromotedObject promoted_object { get; set; }
        public String name { get; set; }
        public String optimization_goal { get; set; }
        public String campaign_id { get; set; }
        public String account_id { get; set; }
        public String id { get; set; }
    }


    public class InsightsFieldForCopies
    {
        public AdsCopies ads;
        public String id;
    }
    public class AdsCopies
    {
        public List<DatumCopies> data;
    }

    public class DatumCopies
    {
        public String name;
        public Adset adset;
        public CampaignCopies campaign;
        public Adcreatives adcreatives;
        public String id;
        public String thumbnail_url;
    }

    public class CampaignCopies
    {
        public String name;
        public String id;
    }

    public class Adcreatives
    {
        public List<DatumCopies> data;
    }
    public class Adset
    {
        public String name;
        public String id;
    }



    #endregion

    #region Serp Keyword
    public class SerpKeywordDataDto
    {
        public string Keyword { get; set; }
        public string CurrentPosition { get; set; }
        public string PreviousPosition { get; set; }
        public string change { get; set; }
        public string CurrentDate { get; set; }
        public string PreDate { get; set; }
        public string Location { get; set; }
        public string LocalPackCount { get; set; }
    }
    #endregion

    #region Keyword Equality Comparer
    public class KeywordEqualityComparer : IEqualityComparer<SerpDto>
    {
        public bool Equals(SerpDto x, SerpDto y)
        {
            // Two items are equal if their keys are equal.
            return x.Keywords == y.Keywords && x.LocationName == y.LocationName;
        }

        public int GetHashCode(SerpDto obj)
        {
            return obj.Keywords.GetHashCode();
        }
    }
    #endregion
    #region Facebook



    public class CategoryList
    {
        public string id { get; set; }
        public string name { get; set; }
    }



    public class FbPage
    {
        public string access_token { get; set; }
        public string category { get; set; }
        public List<CategoryList> category_list { get; set; }
        public string name { get; set; }
        public string id { get; set; }
        public List<string> tasks { get; set; }
    }



    public class FbPageList
    {
        public List<FbPage> data { get; set; }



    }




    public class FBValue
    {
        public object value { get; set; }
        public DateTime end_time { get; set; }
    }



    public class FacebookInsightsData
    {
        public string name { get; set; }
        public string period { get; set; }
        public List<FBValue> values { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string id { get; set; }




    }




    public class FacebookInsights
    {
        public List<FacebookInsightsData> data { get; set; }





    }



    public class FacebookReportDates
    {
        public string startDate { get; set; }
        public string endDate { get; set; }

    }



    public class customValues
    {
        public FacebookValueTypes value { get; set; }
        public DateTime end_time { get; set; }
    }



    public class customValues1
    {
        public FacebookValueTypes1 value { get; set; }
        public DateTime end_time { get; set; }
    }



    public class FacebookValueTypes
    {
        public int? other { get; set; }
        public int? like { get; set; }
        public int? comment { get; set; }



    }



    public class FacebookValueTypes1
    {
        public int? unlike_page_clicks { get; set; }



        public int? hide_clicks { get; set; }



        public int? hide_all_clicks { get; set; }



        public int? report_spam_clicks { get; set; }



    }




    public class ExternalReferrerList
    {
        public string url { get; set; }
        public long count { get; set; }
        public string percent { get; set; }
    }



    public class CountryImpression
    {
        public string country { get; set; }
        public long count { get; set; }
        public string percent { get; set; }
    }



    public class FacebookNewLike
    {
        public int new_like_count { get; set; }
        public string id { get; set; }
    }



    public class PageLike
    {
        public int country_page_likes { get; set; }
        public string id { get; set; }
    }



    public class FacebookReportOneCall
    {
        public int pageReachTotal { get; set; }
        public int pageClicksTotal { get; set; }
        public int positiveFeedback { get; set; }
        public int totalFeedback { get; set; }
        public int negativeFeedback { get; set; }
        public int profileViewTotal { get; set; }
        public int pageImpressionsTotal { get; set; }
        public int organicReach { get; set; }
        public int paidReach { get; set; }
        public int lostLikes { get; set; }
        public int newLikes { get; set; }
        public int paidReachTotal { get; set; }

        public List<ExternalReferrerList> ExternalReferrerList { get; set; }

        public List<CountryImpression> CountryImpression { get; set; }

        public LikesType LikeType { get; set; }

    }
    public class LikesType
    {
        public int PaidLike { get; set; }

        public int OrganicLike { get; set; }
    }



    public class FacebookAllCallData
    {
        public List<FacebookReportOneCall> FacebookReportMultipleCall { get; set; }



        public List<int> newLikes { get; set; }



        public List<int> pageLikes { get; set; }



    }


    public class LikeTypeDto
    {
        public LikeType value { get; set; }
        public DateTime end_time { get; set; }
    }

    public class LikeType
    {
        public int? Ads { get; set; }

        [JsonProperty(PropertyName = "News Feed")]
        public int? NewsFeed { get; set; }

        [JsonProperty(PropertyName = "Page Suggestions")]
        public int? PageSuggestions { get; set; }

        [JsonProperty(PropertyName = "Restored Likes from Reactivated Accounts")]
        public int? RestoredLikesFromReactivatedAccounts { get; set; }

        public int? Search { get; set; }

        [JsonProperty(PropertyName = "Your Page")]
        public int? YourPage { get; set; }

        public int? Other { get; set; }


    }




    #endregion

    #region Google Analytics 4
    public class GA4DimensionHeader
    {
        public string name { get; set; }
    }

    public class GA4DimensionValue
    {
        public string value { get; set; }
    }

    public class GA4Metadata
    {
        public string currencyCode { get; set; }
        public string timeZone { get; set; }
    }

    public class GA4MetricHeader
    {
        public string name { get; set; }
        public string type { get; set; }
    }

    public class GA4MetricValue
    {
        public string value { get; set; }
    }

    public class GA4Row
    {
        public List<GA4DimensionValue> dimensionValues { get; set; }
        public List<GA4MetricValue> metricValues { get; set; }
    }

    public class GA4Root
    {
        public List<GA4DimensionHeader> dimensionHeaders { get; set; }
        public List<GA4MetricHeader> metricHeaders { get; set; }
        public List<GA4Row> rows { get; set; }
        public int rowCount { get; set; }
        public GA4Metadata metadata { get; set; }
        public string kind { get; set; }
        public HttpStatusCode statusCode { get; set; }
    }

    public class Ga4OrganicData
    {
        public string date { get; set; }
        public int value { get; set; }
    }

    public class Ga4ConversionData
    {
        public string date { get; set; }
        public int value { get; set; }
    }

    public class Ga4Details
    {
        public List<Ga4OrganicData> organicData { get; set; }
        public List<Ga4ConversionData> conversionData { get; set; }
        public List<string> dates { get; set; }
        public Acquisition userAcquisition { get; set; }
        public Acquisition trafficAcquisition { get; set; }
        public List<EcomPurchase> EcomPurchases { get; set; }
        public Ga4PurchaseJourney Ga4PurchaseJourney { get; set; }

    }

    public class Ga4PurchaseJourney
    {
        public string PurchaseTotalSessionStart { get; set; }
        public string PurchaseTotalPurchase { get; set; }
        public string PurchaseTotalCheckout { get; set; }
        public string PurchaseTotalAddedCart { get; set; }
        public string PurchaseTotalViewItem { get; set; }
    }

    public class EcomPurchase
    {
        public string ItemName { get; set; }
        public string TotalPurchased { get; set; }
        public string TotalRevenue { get; set; }
        public string TotalAddToCart { get; set; }
        public string TotalViewed { get; set; }
        public List<string> ItemsViewed { get; set; }
        public List<string> ItemsAddedToCart { get; set; }
        public List<string> ItemPurchased { get; set; }
        public List<string> ItemRevenue { get; set; }





    }

    public class TrafficAquisition
    {
        public Acquisition LineChartData { get; set; }
        public List<int> Current { get; set; }
        public List<int> Previous { get; set; }
    }


    public class Acquisition
    {
        public List<string> Direct { get; set; }
        public List<string> OrganicSearch { get; set; }
        public List<string> OrganicSocial { get; set; }
        public List<string> Referral { get; set; }
        public List<string> Unassigned { get; set; }

    }

    #endregion

    public class TypeSubType
    {
        public int Type { get; set; }

        public List<string> Subtype { get; set; }
    }

    public class GenerateReportDto
    {
        public string CampaignId { get; set; }
        public string CampaignName { get; set; }
        public string CompanyId { get; set; }
        public string CompanyName { get; set; }
        public List<string> ReportTypeList { get; set; }
        public bool IsCoverPage { get; set; }
        public bool TableOfContent { get; set; }
        public string Name { get; set; }
        public string Comments { get; set; }
        public string Images { get; set; }
        public int Frequency { get; set; }
        public ReportGenerateSetting ReportGenerateSetting { get; set; }
        public RootReportDataDto ReportData { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string IndexSettings { get; set; }

    }

    public class ShareReportPdfEmail
    {
        public string Email { get; set; }
        public string PdfUrl { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid CompanyId { get; set; }

        public string CompanyName { get; set; }

        public string Footer { get; set; }

        public string Header { get; set; }

        public string HtmlContent { get; set; }
    }
    public class ReportGenerateSetting
    {
        public string HeaderTextValue { get; set; }
        public string HeaderLableImg { get; set; }
        public string HeaderLableImgCamp { get; set; }
        public bool ShowHeader { get; set; }
        public bool ShowFooter { get; set; }
        public bool ShowFooterPageNumber { get; set; }
        public bool ShowReportGenerationDate { get; set; }
        public string HeaderTextColor { get; set; }
        public string HeaderBgColor { get; set; }
        public string FooterText { get; set; }
        public string CoverPageTextColor { get; set; }
        public string CoverPageBgColor { get; set; }
        public string CoverPageBgImage { get; set; }
        public string Name { get; set; }
        public string ThemeBgColor { get; set; }
        public string ThemeTextColor { get; set; }
        public string Font { get; set; }

    }

    public class RootReportDataDto
    {
        public string GoogleAnalytics { get; set; }
        public string GoogleSearchConsole { get; set; }
        public string GoogleAdsCampaign { get; set; }
        public string Facebook { get; set; }
        public string FacebookAdsCampaign { get; set; }
        public string Instagram { get; set; }
        public string LinkedInEngagement { get; set; }
        public string Keywords { get; set; }
        public string FacebookAdsGroup { get; set; }
        public string FacebookAdsCopies { get; set; }
        public string GoogleAdsGroups { get; set; }
        public string GoogleAdsCopies { get; set; }
        public string LinkedInDemographic { get; set; }
        public string GoogleAnalyticsFour { get; set; }
        public string LightHouse { get; set; }
        public string FrontCampaignRoot { get; set; }
        public string FrontCreativeRoot { get; set; }
        public string FrontAdGroupRoot { get; set; }
        public string GbpData { get; set; }
        public string WcReportData { get; set; }
        public string GoogleSheetData { get; set; }
        public string CallRailReportData { get; set; }
        public string CallRailReportTableData { get; set; }
        public string MailchimpPreviewData { get; set; }
        public string MsAdsPreviewData { get;set;}
        public string AiSummaryData { get; set; }



    }

    public class ReportImageDto
    {
        public string src { get; set; }
        public string height { get; set; }
        public string width { get; set; }

        public string align { get; set; }

    }

    #region ROOT GSC DATA
    public class RootGSCReportData
    {
        [JsonProperty("Card")]
        public Card Card { get; set; }
        [JsonProperty("Chart")]
        public Chart Chart { get; set; }
    }
    public class Card
    {
        [JsonProperty("cardValue")]
        public CardData CardValue { get; set; }
        [JsonProperty("cardPercentValue")]
        public CardData CardPercentValue { get; set; }
    }
    public class CardData
    {
        [JsonProperty("Clicks")]
        public double Clicks { get; set; }
        [JsonProperty("CTR")]
        public double Ctr { get; set; }
        [JsonProperty("Impressions")]
        public double Impressions { get; set; }
        [JsonProperty("Position")]
        public double Position { get; set; }
    }
    public class Chart
    {
        [JsonProperty("Dates")]
        public string[] Dates { get; set; }
        [JsonProperty("Clicks")]
        public ChartData Clicks { get; set; }
        [JsonProperty("CTR")]
        public ChartData Ctr { get; set; }
        [JsonProperty("Impressions")]
        public ChartData Impressions { get; set; }
        [JsonProperty("Position")]
        public ChartData Position { get; set; }
    }
    public class ChartData
    {
        [JsonProperty("Current")]
        public double[] Current { get; set; }
        [JsonProperty("Previous")]
        public double[] Previous { get; set; }
    }
    #endregion

    #region ROOT GA DATA
    public class RootGAReportData
    {
        [JsonProperty("Dates")]
        public List<string> Dates { get; set; }
        [JsonProperty("Traffic")]
        public GaData Traffic { get; set; }
        [JsonProperty("Conversions")]
        public GaData Conversions { get; set; }
        public TrafficAquisition UserAquisition { get; set; }
        public TrafficAquisition TrafficAquisition { get; set; }
        public Ga4PurchaseJourney PurchaseJourney { get; set; }
        public List<EcomPurchase> EcommercePurchases { get; set; }

    }
    public class GaData
    {
        [JsonProperty("Current")]
        public List<int> Current { get; set; }
        [JsonProperty("Previous")]
        public List<int> Previous { get; set; }
    }
    #endregion

    #region ROOT PAGESPEED DATA
    public class RootPageSpeedReportData
    {
        [JsonProperty("Mobile")]
        public string Mobile { get; set; }
        [JsonProperty("Desktop")]
        public string Desktop { get; set; }
    }
    #endregion

    #region ROOT KEYWORDS DATA
    public class RootKeywordsReportData
    {
        [JsonProperty("id")]
        public string id { get; set; }
        [JsonProperty("Keyword")]
        public string Keyword { get; set; }
        [JsonProperty("Location")]
        public string Location { get; set; }
        [JsonProperty("PreDate")]
        public string PreDate { get; set; }
        [JsonProperty("PreviousPosition")]
        public string PreviousPosition { get; set; }
        [JsonProperty("CurrentDate")]
        public string CurrentDate { get; set; }
        [JsonProperty("CurrentPosition")]
        public string CurrentPosition { get; set; }
        [JsonProperty("change")]
        public string change { get; set; }

        [JsonProperty("LocalPackStatus")]
        public bool LocalPackStatus { get; set; }

        [JsonProperty("CurrentLocalPackCount")]
        public string CurrentLocalPackCount { get; set; }

        [JsonProperty("PrevLocalPackCount")]
        public string PrevLocalPackCount { get; set; }
    }
    #endregion

    #region ROOT GADS DATA
    public class GadsCard
    {
        [JsonProperty("clicks")]
        public string clicks { get; set; }
        [JsonProperty("impressions")]
        public string impressions { get; set; }
        [JsonProperty("vtc")]
        public string vtc { get; set; }
        [JsonProperty("avg_cpc")]
        public string avg_cpc { get; set; }
        [JsonProperty("conv_rate")]
        public string conv_rate { get; set; }
        [JsonProperty("conv")]
        public string conv { get; set; }
        [JsonProperty("cost")]
        public string cost { get; set; }
        [JsonProperty("cost_conv")]
        public string cost_conv { get; set; }
        [JsonProperty("currency")]
        public string currency { get; set; }
    }

    public class RootGoogleAdsReportData
    {
        [JsonProperty("ChartName")]
        public string ChartName { get; set; }
        [JsonProperty("Card")]
        public GadsCard Card { get; set; }
        [JsonProperty("LeftChartLabels")]
        public List<string> LeftChartLabels { get; set; }
        [JsonProperty("LeftChartValue")]
        public List<string> LeftChartValue { get; set; }
        [JsonProperty("RightChartLabels")]
        public List<string> RightChartLabels { get; set; }
        [JsonProperty("RightChartValue")]
        public List<string> RightChartValue { get; set; }
        [JsonProperty("TableList")]
        public List<GadsTableList> TableList { get; set; }
    }

    public class GadsTableList
    {
        [JsonProperty("campName")]
        public string campName { get; set; }
        [JsonProperty("click")]
        public int click { get; set; }
        [JsonProperty("impression")]
        public int impression { get; set; }
        [JsonProperty("vtc")]
        public int vtc { get; set; }
        [JsonProperty("avg_cpc")]
        public double avg_cpc { get; set; }
        [JsonProperty("conv")]
        public int conv { get; set; }
        [JsonProperty("cost")]
        public double cost { get; set; }
        [JsonProperty("cost_conv")]
        public string cost_conv { get; set; }
        [JsonProperty("conv_rate")]
        public string conv_rate { get; set; }
        [JsonProperty("currency")]
        public string currency { get; set; }
    }
    #endregion

    public class PageData
    {
        public string name { get; set; }
        public string period { get; set; }
        public List<PageValue> values { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string id { get; set; }
    }

    public class PageLiked
    {
        public List<PageData> data { get; set; }
    }

    public class PageValue
    {
        public int value { get; set; }
        public DateTime end_time { get; set; }
    }

    public class Dataset
    {
        public bool fill { get; set; }
        public List<string> data { get; set; }
        public string label { get; set; }
        public string backgroundColor { get; set; }
        public string borderColor { get; set; }

        public int pointRadius { get; set; }

        public int borderWidth { get; set; }
    }

    public class ReportReplaceData
    {
        public string HtmlString { get; set; }
        public string FooterText { get; set; }
        public bool ShowFooter { get; set; }
        public string ShowPageNumberId { get; set; }
        public string ShowPageNumber { get; set; }
        public bool ShowHeader { get; set; }
        public int PageNumber { get; set; }
        public RootReportDataDto RootReportData { get; set; }
        public int Type { get; set; }
        public ReportGenerateSetting ReportSetting { get; set; }

        public List<string> SubType { get; set; }

    }

    public class Ga4RawData
    {
        public string OrganicSessions { get; set; }

        public string OrganicConversions { get; set; }

        public string NewUserAcquisition { get; set; }

        public string TrafficAcquisition { get; set; }

        public decimal EcommerceTotalRevenue { get; set; }


    }

    public class HtmlAndRawData
    {
        public List<Dictionary<string, string>> htmlString { get; set; }

        public Ga4RawData Ga4RawData { get; set; }


    }

    public class GscRawData
    {
        public string Clicks { get; set; }

        public string Impressions { get; set; }

        public string Position { get; set; }

        public string CTR { get; set; }

        public string CurrentDateRange { get; set; }

        public string PreviousDateRange { get; set; }
    }

    public class GoogleAdsRawData
    {
        public string ViewThroughConversions { get; set; }
        public string Avg_CPC { get; set; }
        public string Clicks { get; set; }
        public string ConversationRate { get; set; }
        public string Conversation { get; set; }
        public string Cost { get; set; }
        public string CostPerConversions { get; set; }
        public string Impressions { get; set; }
    }

    public class LinkedinRawData
    {
        public int OrganicFollowerGain { get; set; }
        public int PaidFollowerGain { get; set; }
        public int LostFollower { get; set; }
        public int OrganicFollower { get; set; }
        public int PaidFollower { get; set; }
        public int Clicks { get; set; }
        public int Impressions { get; set; }
    }

    public class LinkedinDemographicRawData
    {
        public string TopFollowerByCountry { get; set; }
        public string TopFollowerBySeniority { get; set; }
        public string TopFollowerByIndustry { get; set; }
        public string TopFollowerByJobFunction { get; set; }
        public string TopFollowerByCompanySize { get; set; }
    }

    public class PrepareGAdsData
    {
        public string Html { get; set; }
        public GoogleAdsRawData GoogleAdsRawData { get; set; }
    }

    public class PrepareLinkedinData
    {
        public string Html { get; set; }
        public LinkedinRawData LinkedinRawData { get; set; }
    }

    public class PrepareLinkedinDemographicRawData
    {
        public string Html { get; set; }
        public LinkedinDemographicRawData LinkedinDemographicRawData { get; set; }
    }

    public class PrepareLinkedinAds
    {
        public string Html { get; set; }
        public LinkedinAdsCardData LinkedinAdsCardData { get; set; }

    }

    public class PageSpeedData
    {
        public string Desktop { get; set; }
        public string Mobile { get; set; }
    }

    public class PrepareLightHouseData
    {
        public string Html { get; set; }
        public PageSpeedData LightHouseData { get; set; }

    }


    public class InstagramReportsRawData
    {

        public int PageProfileViewTotal { get; set; }
        public int NewFollowers { get; set; }
        public int FollowersTotal { get; set; }
        public int AvgPerDayProfileViews { get; set; }
        public int AvgFollowersPerDay { get; set; }

        public int IGMediaImpressions { get; set; }
        public int IGMediaReach { get; set; }
        public int WebsiteReach { get; set; }
        public int IGMediaImpressionsAvgPerDay { get; set; }
        public int IGMediaReachAvgPerDay { get; set; }

        public string TopLanguages { get; set; }
        public string TopFollowerByContry { get; set; }
        public string TopFollowerByCity { get; set; }
        public string GenderPercentage { get; set; }

    }

    public class PrepareInstagramData
    {
        public List<string> HtmlList { get; set; }
        public InstagramReportsRawData InstagramReportsData { get; set; }
    }

    public class GbpRawData
    {
        public string ProfileViewDiff { get; set; }

        public string SearchKeywordDiff { get; set; }

        public string ProfileInteractionDiff { get; set; }

        public string CallDiff { get; set; }

        public string MessageDiff { get; set; }

        public string BookingDiff { get; set; }

        public string WebsiteDiff { get; set; }

        public string DirectionDiff { get; set; }

    }

    public class PrepareGbpRootData
    {
        public List<Dictionary<string, string>> HtmlList { get; set; }
        public RootGbpData RootGbpData { get; set; }
    }

    public class WcRawData
    {
        public string Sales { get; set; }

        public string Orders { get; set; }

        public string Inventory { get; set; }

        public string RegisteredUsers { get; set; }

        public string AvgOrderValue { get; set; }

        public string HighestRevenuePerProduct { get; set; }

        public string HighestSalePerProduct { get; set; }

        public string HighestOrderLocation { get; set; }

        public string ReturningCustomerRate { get; set; }
    }


    public class PrepareWcData
    {
        public List<Dictionary<string, string>> HtmlList { get; set; }
        public WcRawData WcRawData { get; set; }
    }

    public class CrRawData
    {
        public string Answered { get; set; }
        public string Missed { get; set; }
        public string FirstTime { get; set; }
        public string Calls { get; set; }
        public string Leads { get; set; }
        public string AvgDuration { get; set; }
        public string MissedRateAvg { get; set; }
        public string AnsweredRateAvg { get; set; }
        public string AvgFirstTimeCallRate { get; set; }
        public string AvgCallsPerLead { get; set; }

        public string TopSourceData { get; set; }
        public string AnswerVsMissed { get; set; }
    }

    public class CallRailCurrentPeriodData
    {
        public int TotalAnswered { get; set; }
        public int TotalMissed { get; set; }
        public int TotalFirstTime { get; set; }
        public int TotalCalls { get; set; }
        public int TotalLeads { get; set; }
        public string AvgDuration { get; set; }
        public double AvgDurationDouble { get; set; }
        public double TotalMissedRateAvg { get; set; }
        public double TotalAnsweredRateAvg { get; set; }
        public double TotalAvgFirstTimeCallRate { get; set; }
        public double TotalAvgCallsPerLead { get; set; }
    }
    public class PrepareCallRailData
    {
        public List<Dictionary<string, string>> HtmlList { get; set; }
        public CrRawData CrRawData { get; set; }
    }

    public class PrepareMailChimpData
    {
        public List<Dictionary<string, string>> HtmlList { get; set; }
        public MCRootCampaignListRawData MailchimpCampaignsRawData { get; set; }
        public MCRootListRawData MailchimpListRawData { get; set; }
        public List<SingleCampaignReportRawData> SingleCampaignsRawData { get; set; }
        public List<RootSingleListRawData> SingleListsRawData { get; set; }

    }

    public class MCRootCampaignListRawData
    {
        public decimal recipients24HourPeriod { get; set; }

        public decimal open24HourPeriod { get; set; }

        public decimal click24HourPeriod { get; set; }

        public decimal recipients { get; set; }

        public decimal unopenedEmails { get; set; }

        public decimal bouncedEmails { get; set; }

        public decimal uniqueOpens { get; set; }

        public decimal openRate { get; set; }

        public decimal clickRate { get; set; }

        public decimal clicks { get; set; }

        public decimal unsubscribeRate { get; set; }

        public decimal bounceRate { get; set; }

        public decimal unsubscribes { get; set; }

        public decimal opens { get; set; }

        public decimal orders { get; set; }

        public decimal averageOrder { get; set; }

        public decimal revenue { get; set; }

        public decimal totalSpent { get; set; }

        public decimal deliveries { get; set; }

        public decimal deliveryRate { get; set; }

        public decimal spams { get; set; }

        public decimal spamRate { get; set; }

        public decimal subsciberClick { get; set; }

    }

    public class MCRootListRawData
    {       
        public int opens { get; set; }

        public int clicks { get; set; }

        public int audienceGrowth { get; set; }

        public decimal rating { get; set; }

        public decimal subscribers { get; set; }

        public decimal openRate { get; set; }

        public decimal clickRate { get; set; }

        public decimal campaigns { get; set; }

        public decimal unsubscribes { get; set; }

        public decimal avgSubscribeRate { get; set; }

        public decimal avgUnsubscribeRate { get; set; }       

    }

    public class SingleCampaignReportRawData
    {      
        public int LocationTotal { get; set; }

        public string TopUrl { get; set; }

        public decimal UniqueOpenTotal { get; set; }

        public decimal ClickTotal { get; set; }

        public decimal Recipients { get; set; }

        public decimal UnopenedEmails { get; set; }

        public decimal BouncedEmails { get; set; }

        public decimal UniqueOpens { get; set; }

        public decimal OpenRate { get; set; }

        public decimal ClickRate { get; set; }

        public decimal Click24HoursPeriod { get; set; }

        public decimal UnsubscribeRate { get; set; }

        public decimal BounceRate { get; set; }

        public decimal Unsubscribes { get; set; }

        public decimal Opens24HoursPeriod { get; set; }

        public decimal Orders { get; set; }

        public decimal AverageOrder { get; set; }

        public decimal Revenue { get; set; }

        public decimal TotalSpent { get; set; }

        public decimal Deliveries { get; set; }

        public decimal DeliveryRate { get; set; }

        public decimal Spams { get; set; }

        public decimal SpamRate { get; set; }

        public decimal SubsciberClick { get; set; }

        public string Name { get; set; }

        public string TopLocations { get; set; }

        public string TopSources { get; set; }

    }

    public class RootSingleListRawData
    {
        public string Name { get; set; }
        public decimal Subscribers { get; set; }

        public decimal OpenRate { get; set; }

        public decimal ClickRate { get; set; }

        public decimal Campaigns { get; set; }

        public decimal Unsubscribes { get; set; }

        public decimal AvgSubscribeRate { get; set; }

        public decimal AvgUnsubscribeRate { get; set; }

        public int Opens { get; set; }

        public int Clicks { get; set; }

        public int AudienceGrowth { get; set; }

        public string TopEmailClient { get; set; }

    }


    public class CampaignPerformace
    {
        public string impressions { get; set; }
        public string averageCpc { get; set; }
        public string ctr { get; set; }
        public string clicks { get; set; }
        public string spend { get; set; }
        public string conversionRate { get; set; }
        public string conversions { get; set; }
        public string costPerConversion { get; set; }
        public string impressionSharePercent { get; set; }
        public string impressionLostToBudgetPercent { get; set; }
        public string impressionLostToRankAggPercent { get; set; }
    }

    public class AdGroupPerformance
    {
        public string name { get; set; }
        public string impressions { get; set; }
        public string averageCpc { get; set; }
        public string ctr { get; set; }
        public string clicks { get; set; }
        public string spend { get; set; }
        public string conversionRate { get; set; }
        public string conversions { get; set; }
        public string costPerConversion { get; set; }
        public string impressionSharePercent { get; set; }
        public string impressionLostToBudgetPercent { get; set; }
        public string impressionLostToRankAggPercent { get; set; }

    }

    public class KeywordPerformance
    {
        public string name { get; set; }
        public string impressions { get; set; }
        public string averageCpc { get; set; }
        public string ctr { get; set; }
        public string clicks { get; set; }
        public string spend { get; set; }
        public string conversionRate { get; set; }
        public string conversions { get; set; }
        public string costPerConversion { get; set; }
        public string highestClickKeyword { get; set; }
    }

    public class ConversionPerformance
    {
        public string name { get; set; }
        public string conversions { get; set; }
        public string revenue { get; set; }

    }

    public class PrepareMicrosoftAdsData
    {
        public List<Dictionary<string, string>> HtmlList { get; set; }
        public CampaignPerformace CampaignPerformace { get; set; }
        public AdGroupPerformance AdGroupPerformance { get; set; }
        public KeywordPerformance KeywordPerformance { get; set; }
        public ConversionPerformance ConversionPerformance { get; set; }

        public List<CampaignPerformace> SingleCampaignPerformaceList { get; set; }
        public List<AdGroupPerformance> SingleGroupPerformance { get; set; }
        public List<KeywordPerformance> SingleKeywordPerformance { get; set; }
        public List<ConversionPerformance> SingleConversionPerformance { get; set; }
    }

    public class PrepareFbAdsCampaignData
    {
        public string Html { get; set; }
        public FbAdsCampaignData CampaignPerformace { get; set; }        
    }

    public class PrepareFbAdsSetData
    {
        public string Html { get; set; }
        public FbAdsSetData FbAdsSetData { get; set; }
    }


    public class FbAdsCampaignData
    {
        //Current data
        public string Clicks { get; set; }
        public string Ctr { get; set; }
        public string Spend { get; set; }
        public string Impressions { get; set; }
        public string Reachs { get; set; }       

    }

    public class FbAdsSetData
    {
        public string Impressions { get; set; }
        public string Reachs { get; set; }
        public string Clicks { get; set; }
        public string Cpc { get; set; }
        public string Spend { get; set; }
        public string LinkClick { get; set; }
        public string Ctr { get; set; }
        public string Cplc { get; set; }
    }
}
