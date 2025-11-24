using System;
using System.Collections.Generic;
using System.Net;

namespace EventManagement.Dto
{
    /// <summary>
    /// CampaignMailchimp Model
    /// </summary>
    public class CampaignMailchimpDto : CampaignMailchimpAbstractBase
    {

    }


    public class MailchimpAuth
    {
        public string access_token { get; set; }
    }

    public class MailchimpMetadataDto
    {
        public string dc { get; set; }
        public string role { get; set; }
        public string accountname { get; set; }
        public int user_id { get; set; }
        public Login login { get; set; }
        public string login_url { get; set; }
        public string api_endpoint { get; set; }
    }

    public class Login
    {
        public string email { get; set; }
        public object avatar { get; set; }
        public int login_id { get; set; }
        public string login_name { get; set; }
        public string login_email { get; set; }
    }

    public class MailchimpReports
    {
        public List<McReport> reports { get; set; }

        public int total_items { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public string ErrorMsg { get; set; }
    }

    public class MCCampaignList
    {
        public List<ReportCampaignList> reports { get; set; }

        public int total_items { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public string ErrorMsg { get; set; }
    }

    //public class ReportCapaignList
    //{
    //    public string id { get; set; }
    //    public string campaign_title { get; set; }
    //    public string subject_line { get; set; }

    //}

    public class ReportCampaignList
    {
        private string _campaignTitle;

        public string id { get; set; }

        public string campaign_title
        {
            get { return string.IsNullOrEmpty(_campaignTitle) ? subject_line : _campaignTitle; }
            set { _campaignTitle = value; }
        }

        public string subject_line { get; set; }
    }



    public class LocationResponse
    {
        public List<LocationItem> locations { get; set; }
        public string campaign_id { get; set; }
        public int total_items { get; set; }
        public List<Link> _links { get; set; }

        public HttpStatusCode StatusCode { get; set; }
        public string ErrorMsg { get; set; }
    }


    public class UrlClicked
    {
        public string url { get; set; }
        public int total_clicks { get; set; }

        public int percentage { get; set; }

    }

    public class EmailActivity
    {
        public string email_address { get; set; }
        public List<Activity> activity { get; set; } // You might want to create a separate class for activity   
    }

    public class Activity
    {
        public string action { get; set; }
        public string type { get; set; }
        public DateTime timestamp { get; set; }
    }

    public class EmailActivityResponse
    {
        public List<EmailActivity> emails { get; set; }
        public string campaign_id { get; set; }
        public int total_items { get; set; }
    }

    public class CampaignTableResponse
    {
        public string Email { get; set; }
        public string Status { get; set; }
        public int OpenCount { get; set; }

    }

    public class GrowthHistory
    {
        public string list_id { get; set; }
        public string month { get; set; }
        public int subscribed { get; set; }

    }

    public class RecentOpenActivity
    {
        public DateTime day { get; set; }
        public int unique_opens { get; set; }
        public int recipient_clicks { get; set; }

    }

    public class RecentActivity
    {
        public List<RecentOpenActivity> activity { get; set; }
        public string list_id { get; set; }
        public int total_items { get; set; }
    }

    public class RootRecentActivity
    {
        public List<RecentOpenActivity> activity { get; set; } = new List<RecentOpenActivity>();
        public HttpStatusCode StatusCode { get; set; }
        public string ErrorMsg { get; set; }
    }




    public class GrowthHistoryResponse
    {
        public List<GrowthHistory> history { get; set; } = new List<GrowthHistory>();

        public HttpStatusCode StatusCode { get; set; }
        public string ErrorMsg { get; set; }
    }

    public class AudianceGrowth
    {
        public List<string> Months { get; set; }

        public List<int> Subscribers { get; set; }
    }



    public class ListOpenChart
    {
        public string Months { get; set; }

        public int Opens { get; set; }
        public int Clicks { get; set; }
    }



    public class CampaignTableRoot
    {
        public List<CampaignTableResponse> CampaignTableResponse { get; set; }

        public int total_items { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string ErrorMsg { get; set; }
    }

    public class ClickDetailsDto
    {
        public List<UrlClicked> urls_clicked { get; set; }

        public HttpStatusCode StatusCode { get; set; }
        public string ErrorMsg { get; set; }
    }

    public class LocationItem
    {
        public string country_code { get; set; }
        public string region { get; set; }
        public string region_name { get; set; }
        public int opens { get; set; }
    }

    public class McReport
    {
        public string id { get; set; }
        public string campaign_title { get; set; }
        public string subject_line { get; set; }

        public int emails_sent { get; set; }
        public int abuse_reports { get; set; }
        public int unsubscribed { get; set; }

        public string send_time { get; set; }
        public Bounces bounces { get; set; }
        public Opens opens { get; set; }
        public Clicks clicks { get; set; }
        public List<TimeSeries1> timeseries { get; set; }
        public Ecommerce ecommerce { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string ErrorMsg { get; set; }
    }

    public class Bounces
    {
        public int hard_bounces { get; set; }
        public int soft_bounces { get; set; }
        public int syntax_errors { get; set; }
    }

    public class Forwards
    {
        public int forwards_count { get; set; }
        public int forwards_opens { get; set; }
    }

    public class Opens
    {
        public int opens_total { get; set; }
        public int unique_opens { get; set; }
        public double open_rate { get; set; }
        public string last_open { get; set; }
    }

    public class Clicks
    {
        public int clicks_total { get; set; }
        public int unique_clicks { get; set; }
        public int unique_subscriber_clicks { get; set; }
        public double click_rate { get; set; }
        public string last_click { get; set; }
    }

    public class FacebookLikes
    {
        public int recipient_likes { get; set; }
        public int unique_likes { get; set; }
        public int facebook_likes { get; set; }
    }

    public class IndustryStats
    {
        public string type { get; set; }
        public double open_rate { get; set; }
        public double click_rate { get; set; }
        public double bounce_rate { get; set; }
        public double unopen_rate { get; set; }
        public double unsub_rate { get; set; }
        public double abuse_rate { get; set; }
    }

    public class ListStats
    {
        public int sub_rate { get; set; }
        public int unsub_rate { get; set; }
        public double open_rate { get; set; }
        public double click_rate { get; set; }
    }

    public class TimeSeries1
    {
        public string timestamp { get; set; }
        public int emails_sent { get; set; }
        public int unique_opens { get; set; }
        public int recipients_clicks { get; set; }
    }

    public class Ecommerce
    {
        public int total_orders { get; set; }
        public int total_spent { get; set; }
        public int total_revenue { get; set; }
        public string currency_code { get; set; }
    }

    public class DeliveryStatus
    {
        public bool enabled { get; set; }
    }

    public class MCChartData
    {
        public string Dates { get; set; }

        public decimal Values { get; set; }
    }

    public class ListReport
    {
        public List<List> lists { get; set; }
        public int total_items { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string ErrorMsg { get; set; }
    }

    public class Contact
    {
        public string company { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip { get; set; }
        public string country { get; set; }
        public string phone { get; set; }
    }

    public class CampaignDefaults
    {
        public string from_name { get; set; }
        public string from_email { get; set; }
        public string subject { get; set; }
        public string language { get; set; }
    }

    public class Stats
    {
        public int member_count { get; set; }
        public int unsubscribe_count { get; set; }
        public int cleaned_count { get; set; }
        public int member_count_since_send { get; set; }
        public int unsubscribe_count_since_send { get; set; }
        public int cleaned_count_since_send { get; set; }
        public int campaign_count { get; set; }

        public int merge_field_count { get; set; }
        public int avg_sub_rate { get; set; }
        public int avg_unsub_rate { get; set; }
        public int target_sub_rate { get; set; }
        public double open_rate { get; set; }
        public double click_rate { get; set; }
    }



    public class EmailClient
    {
        public string client { get; set; }
        public int members { get; set; }
    }

    public class EmailClientRoot
    {
        public List<EmailClient> clients { get; set; }
        public int total_items { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string ErrorMsg { get; set; }
    }


    public class McMember
    {
        public string id { get; set; }
        public string email_address { get; set; }
        public string unique_email_id { get; set; }
        public string contact_id { get; set; }
        public string full_name { get; set; }
        public int web_id { get; set; }
        public string email_type { get; set; }
        public string status { get; set; }
        public bool consents_to_one_to_one_messaging { get; set; }
        public MergeFields merge_fields { get; set; }
        public Dictionary<string, object> interests { get; set; }
        public McStats stats { get; set; }
        public string ip_signup { get; set; }
        public string timestamp_signup { get; set; }
        public string ip_opt { get; set; }
        public string timestamp_opt { get; set; }
        public int member_rating { get; set; }
        public string last_changed { get; set; }
        public string language { get; set; }
        public bool vip { get; set; }
        public string email_client { get; set; }
        public MemberLocation location { get; set; }
        public string source { get; set; }
        public int tags_count { get; set; }
        public List<Link> _links { get; set; }
    }

    public class MergeFields
    {
        public string FNAME { get; set; }
        public string LNAME { get; set; }
        public string ADDRESS { get; set; }
        public string PHONE { get; set; }
        public string CNAME { get; set; }
    }

    public class McStats
    {
        public decimal avg_open_rate { get; set; }
        public decimal avg_click_rate { get; set; }
    }

    public class MemberLocation
    {
        public int latitude { get; set; }
        public int longitude { get; set; }
        public int gmtoff { get; set; }
        public int dstoff { get; set; }
        public string country_code { get; set; }
        public string timezone { get; set; }
        public string region { get; set; }
    }

    public class MailChimpMemberResponse
    {
        public List<McMember> members { get; set; }
        public string list_id { get; set; }
        public int total_items { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string ErrorMsg { get; set; }

    }

    public class MailChimpMemberRoot
    {
        public List<MailChimpMember> MailChimpMembers { get; set; }

        public int TotalItems { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string ErrorMsg { get; set; }

    }


    public class MailChimpMember
    {
        public string Email { get; set; }

        public string SignupDate { get; set; }

        public string Country { get; set; }

        public int MemberRating { get; set; }

        public string Status { get; set; }

        public decimal OpenRate { get; set; }

        public decimal ClickRate { get; set; }

    }

    public class GetSingleListDto
    {
        public List mcList { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string ErrorMsg { get; set; }
    }

    public class RootSingleList
    {
        public string Name { get; set; }
        public decimal Subscribers { get; set; }

        public decimal OpenRate { get; set; }

        public decimal ClickRate { get; set; }

        public decimal Campaigns { get; set; }

        public decimal Unsubscribes { get; set; }

        public decimal AvgSubscribeRate { get; set; }

        public decimal AvguNSubscribeRate { get; set; }

        public List<string> AudianceGrowthChartDates { get; set; }

        public List<int> AudianceGrowthChartValues { get; set; }

        public List<string> Clients { get; set; }

        public List<string> ChartDates { get; set; }

        public List<int> OpensChartValues { get; set; }

        public List<int> ClicksChartValues { get; set; }

        public int OpensChartTotal { get; set; }

        public int ClickChartTotal { get; set; }

        public int AudienceGrowthTotal { get; set; }

        public List<int> Members { get; set; }

       public List<MailChimpMember> MailChimpMembers { get; set; }

        public HttpStatusCode StatusCode { get; set; }
        public string ErrorMsg { get; set; }
    }


    public class List
    {
        public string id { get; set; }
        public string name { get; set; }
        public int list_rating { get; set; }
        public Stats stats { get; set; }

    }

    public class MailchimpPreviewData
    {
        public MCRootCampaignList McRootCampaignList { get; set;  }

        public MCRootList McRootList { get; set; }

        public List<SingleCampaignReport> SingleCampaignReport { get; set; }

        public List<RootSingleList> RootSingleList { get; set; }

        public MailchimpSettings MailchimpSettings { get; set; }

    }

    public class MCRootCampaignList
    {
        
        public List<string> recipientsChartDates { get; set; }
        
        public List<decimal> recipientsChartValues { get; set; }

        public List<string> openChartDates { get; set; }

        public List<decimal> openChartValues { get; set; }


        public List<string> clickChartDates { get; set; }

        public List<decimal> clickChartValues { get; set; }


        public decimal recipientsChartTotal { get; set; }

        public decimal uniqueOpenChartTotal { get; set; }

        public decimal clickChartTotal { get; set; }

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

        public string sendTime { get; set; }

        public List<CampaignListTable> campaignListTable { get; set; }

        public McCampaignsSetting mcCampaignsSetting { get; set; }

        public string mcCampaignsSettingString { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public string ErrorMsg { get; set; }

        public string currencyCode { get; set; } = "";



    }

    public class McCampaignsSetting
    {
        public bool recipients { get; set; }

        public bool unopenedEmails { get; set; }

        public bool bouncedEmails { get; set; }

        public bool uniqueOpens { get; set; }

        public bool openRate { get; set; }

        public bool clickRate { get; set; }

        public bool clicks { get; set; }

        public bool unsubscribeRate { get; set; }

        public bool bounceRate { get; set; }

        public bool unsubscribes { get; set; }

        public bool opens { get; set; }

        public bool orders { get; set; }

        public bool averageOrder { get; set; }

        public bool revenue { get; set; }

        public bool totalSpent { get; set; }

        public bool deliveries { get; set; }

        public bool deliveryRate { get; set; }

        public bool spams { get; set; }

        public bool spamRate { get; set; }

        public bool subsciberClicks { get; set; }

   
    }

    public class McListSetting
    {
        public bool rating { get; set; }

        public bool subscribers { get; set; }

        public bool openRate { get; set; }

        public bool clickRate { get; set; }

        public bool campaigns { get; set; }

        public bool unsubscribes { get; set; }

        public bool avgSubscribeRate { get; set; }

        public bool avgUnsubscribeRate { get; set; }

    }

    public class IndexSettings
    {
        public List<string> IndexSettingsData { get; set; }
    }

    public class MailchimpSettings
    {
        public  McCampaignsSetting mcCampaignsSetting {get;set;}

        public McListSetting mcListSetting { get; set; }
    }

    public class McTilesData
    {
        public string Value { get; set; }

        public bool Status { get; set; }

        public string Display { get; set; } = "none";

        public string Name { get; set; }

    }
    public class MCRootList
    {
        public List<string> audianceGrowthChartDates { get; set; }

        public List<int> audianceGrowthChartValues { get; set; }

        public List<string> openChartDates { get; set; }

        public List<int> openChartValues { get; set; }

        public List<int> clickChartValues { get; set; }

        public int opensChartTotal { get; set; }

        public int clickChartTotal { get; set; }

        public int growthChartTotal { get; set; }

        public decimal rating { get; set; }

        public decimal subscribers { get; set; }

        public decimal openRate { get; set; }

        public decimal clickRate { get; set; }

        public decimal campaigns { get; set; }

        public decimal unsubscribes { get; set; }

        public decimal avgSubscribeRate { get; set; }

        public decimal avgUnsubscribeRate { get; set; }

        public McListSetting mcListSetting { get; set; }

        public string mcListSettingString { get; set; }

        public List<ListTable> listTable { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public string ErrorMsg { get; set; }

    }


    public class SingleCampaignReport
    {
        public List<string> OpenChartDates { get; set; }

        public List<decimal> OpenChartValues { get; set; }

        public List<string> ClickChartDates { get; set; }

        public List<decimal> ClickChartValues { get; set; }

        public List<string> PieLabels { get; set; } = new List<string> { "Not Open", "Unique Open", "Bounced" };

        public List<decimal> PieValues { get; set; }

        public List<string> LocationLabels { get; set; }

        public List<int> LocationValues { get; set; }

        public int LocationChartTotal { get; set; }

        public List<UrlClicked> TopUrlsClick { get; set; }

        public decimal UniqueOpenChartTotal { get; set; }

        public decimal ClickChartTotal { get; set; }

        public decimal Recipients { get; set; }

        public decimal UnopenedEmails { get; set; }

        public decimal BouncedEmails { get; set; }

        public decimal UniqueOpens { get; set; }

        public decimal OpenRate { get; set; }

        public decimal ClickRate { get; set; }

        public decimal Click { get; set; }

        public decimal UnsubscribeRate { get; set; }

        public decimal BounceRate { get; set; }

        public decimal Unsubscribes { get; set; }

        public decimal Opens { get; set; }

        public decimal Orders { get; set; }

        public decimal AverageOrder { get; set; }

        public decimal Revenue { get; set; }

        public decimal TotalSpent { get; set; }

        public decimal Deliveries { get; set; }

        public decimal DeliveryRate { get; set; }

        public decimal Spams { get; set; }

        public decimal SpamRate { get; set; }

        public decimal SubsciberClick { get; set; }

        public string SendTime { get; set; }

        public string Name { get; set; }

        public List<CampaignTableResponse> CampaignTableResponse { get; set; }

        public string CurrencyCode { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public string ErrorMsg { get; set; }

    }

    public class CampaignListTable
    {
        public string Name { get; set; }
        public decimal RecipientsChartTotal { get; set; }

        public decimal UniqueOpenChartTotal { get; set; }

        public decimal ClickChartTotal { get; set; }

        public decimal Recipients { get; set; }

        public decimal UnopenedEmails { get; set; }

        public decimal BouncedEmails { get; set; }

        public decimal UniqueOpens { get; set; }

        public decimal OpenRate { get; set; }

        public decimal ClickRate { get; set; }

        public decimal Click { get; set; }

        public decimal UnsubscribeRate { get; set; }

        public decimal BounceRate { get; set; }

        public decimal Unsubscribes { get; set; }

        public decimal Opens { get; set; }

        public decimal Orders { get; set; }

        public decimal AverageOrder { get; set; }

        public decimal Revenue { get; set; }

        public decimal TotalSpent { get; set; }

        public decimal Deliveries { get; set; }

        public decimal DeliveryRate { get; set; }

        public decimal Spams { get; set; }

        public decimal SpamRate { get; set; }

        public decimal SubsciberClick { get; set; }

        public string SendTime { get; set; }

        public string CurrencyCode { get; set; }

        public string McCampaignId { get; set; }
    }

    public class McListRoot
    {
        public  List<List> McList { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string ErrorMsg { get; set; }
    }

    public class ListTable
    {
        public string Name { get; set; }

        public string Id { get; set; }

        public decimal Rating { get; set; }


        public decimal Subscribers { get; set; }

        public decimal OpenRate { get; set; }

        public decimal ClickRate { get; set; }

        public decimal Campaigns { get; set; }

        public decimal Unsubscribes { get; set; }

        public decimal AvgSubscribeRate { get; set; }

        public decimal AvguNSubscribeRate { get; set; }
    }

}
