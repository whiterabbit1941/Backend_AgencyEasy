using System;
using System.Collections.Generic;

namespace EventManagement.Dto
{
    /// <summary>
    /// LinkedinAd Model
    /// </summary>
    public class LinkedinAdDto : LinkedinAdAbstractBase
    {

    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
 


    public class Element
    {
 
        public string name { get; set; }
        public int id { get; set; }
        public string status { get; set; }

        public string currency { get; set; }
    }

    //For getting all values of linkedin ads account
    public class LinkedinAdRoot
    {
        public List<Element> elements { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class CampaignElement
    {
        public string name { get; set; }
        public int id { get; set; }
        public string campaignGroup { get; set; }
        public string account { get; set; }
        public UnitCost unitCost { get; set; }

        public double clicks { get; set; }

        public double impressions { get; set; }

        public double spent { get; set; }

        public string status { get; set; }

        public double avg_ctr { get; set; }

        public double avg_cpm { get; set; }

        public double avg_cpc { get; set; }

        public double lead { get; set; }

        public double cost_per_lead { get; set; }

        public string intendedStatus { get; set; }

        public string currency { get; set; }
    }

    

    public class CampaignRoot
    {
      
        public List<CampaignElement> elements { get; set; }
    }

    public class AdGroupsRoot
    {
        public List<AdgroupElement> elements { get; set; }
    }

    public class AdgroupElement
    {
        public string name { get; set; }
        public int id { get; set; }
        public string account { get; set; }

        public double clicks { get; set; }

        public double impressions { get; set; }

        public double spent { get; set; }

        public string status { get; set; }

        public double avg_ctr { get; set; }

        public double avg_cpm { get; set; }

        public double avg_cpc { get; set; }

        public double lead { get; set; }

        public double cost_per_lead { get; set; }

        public string intendedStatus { get; set; }

        public string currency { get; set; }

    }

    public class CreativeRoot
    {
        public List<CreativeElement> elements { get; set; }
    }

    public class LinkedinAdsCardData
    {
        public string ad_count { get; set; }

        public string total_spent { get; set; }

        public string total_clicks { get; set; }

        public string total_impressions { get; set; }

        public string total_leads { get; set; }

        public string total_cpl{ get; set; }

        public string percent_spent { get; set; }

        public string percent_clicks { get; set; }

        public string percent_leads { get; set; }

        public string percent_cpl { get; set; }

    }

    public class Content
    {
        public string reference { get; set; }
        public Follow follow { get; set; }
    }
    public class Follow
    {
        public Description description { get; set; }
        public string logo { get; set; }
        public bool showMemberProfilePhoto { get; set; }
        public string organizationName { get; set; }
        public Headline headline { get; set; }
        public string callToAction { get; set; }
    }


    public class UnitCost
    {
        public string currencyCode { get; set; }
        public string amount { get; set; }
    }

    public class Description
    {
        public string preApproved { get; set; }
        public string custom { get; set; }
    }

    public class LinkedinStatRoot
    {       
        public List<LinkedinAdsElement> elements { get; set; }
    }

    public class DateRangeForLinkedin
    {
        public Start start { get; set; }
        public End end { get; set; }
    }

    public class LinkedinAdsElement
    {
        
        public string pivotValue { get; set; }
        public DateRangeForLinkedin dateRange { get; set; }

        public int clicks { get; set; }

        public int impressions { get; set; }

        public int oneClickLeads { get; set; }

        public double avg_cpm { get; set; }

        public double avg_cpc { get; set; }

        public string date { get; set; }

        public string costInLocalCurrency { get; set; }

        //public string currency { get; set; }
    }

    public class Headline
    {
        public string preApproved { get; set; }
        public string custom { get; set; }
    }
    public class End
    {
        public int month { get; set; }
        public int day { get; set; }
        public int year { get; set; }
    }

    public class Start
    {
        public int month { get; set; }
        public int day { get; set; }
        public int year { get; set; }
    }


    public class AnalyticsRoot
    {
        public LinkedinStatRoot linkedinStat { get; set; }

        public CampaignRoot campaignRoot { get; set; }

        public AdGroupsRoot adGroupRoot { get; set; }

        public CreativeRoot creativeRoot { get; set; }

        public LinkedinAdsCardData cardData { get; set; }
    }
    public class FrontCampaignRoot
    {
        public LinkedinStatRoot linkedinStat { get; set; }

        public CampaignRoot campaignRoot { get; set; }

        public LinkedinAdsCardData cardData { get; set; }

        public DempgraphicRoot dempgraphicRoot { get; set; }
    }
    public class FrontAdGroupRoot
    {
        public LinkedinStatRoot linkedinStat { get; set; }

        public AdGroupsRoot adGroupRoot { get; set; }

        public LinkedinAdsCardData cardData { get; set; }
        public DempgraphicRoot dempgraphicRoot { get; set; }
    }
    public class FrontCreativeRoot
    {
        public LinkedinStatRoot linkedinStat { get; set; }

        public CreativeRoot creativeRoot { get; set; }

        public LinkedinAdsCardData cardData { get; set; }

        public DempgraphicRoot dempgraphicRoot { get; set; }
    }

    public class CreativeElement
    {
        public string name { get; set; }
        public string id { get; set; }
        public string account { get; set; }

        public string intendedStatus { get; set; }

        public Content content { get; set; }

        public string campaign { get; set; }

        public double clicks { get; set; }

        public double impressions { get; set; }

        public double spent { get; set; }

        public double avg_ctr { get; set; }

        public double avg_cpm { get; set; }

        public double avg_cpc { get; set; }

        public double lead { get; set; }

        public double cost_per_lead { get; set; }

        public string image { get; set; }

        public string status { get; set; }

        public string currency { get; set; }

    }

    //get list of shares
  

    public class Thumbnail
    {       
        public string resolvedUrl { get; set; }
    }

    public class ContentEntity
    {
        public List<Thumbnail> thumbnails { get; set; }
    }

    public class AdContent
    {
        public List<ContentEntity> contentEntities { get; set; }        
    }

    public class Text
    {       
        public string text { get; set; }
    }

    public class ShareItem
    {
        public AdContent content { get; set; }
        public Text text { get; set; }
        public string id { get; set; }
    }

    public class ShareResponse
    {
        public Dictionary<string, ShareItem> results { get; set; }    
       
    }


    //images 
    public class ImageResult
    {
        public string downloadUrl { get; set; }        
        public string id { get; set; }      
        public string status { get; set; }
    }

    public class ImageResponse
    {
        public Dictionary<string, ImageResult> results { get; set; }      
    }


    //Demographic 

    public class DempgraphicRoot
    {
        public List<DemographicAdElement> elements { get; set; }
    }

    public class DemographicAdElement
    {
        public string name { get; set; }
       
        public double clicks { get; set; }

        public double impressions { get; set; }
     
        public double avg_ctr { get; set; }

        public string pivotValue { get; set; }
        public DateRangeForLinkedin dateRange { get; set; }

        public string percent_clicks { get; set; }

        public string percent_impressions { get; set; }

    }


    //code
    public class AdsDemographicCode
    {     
        public List<Industry> industries { get; set; }
        public List<JobFunction> functions { get; set; }
        public List<JobTitle> titles { get; set; }
    }

    //for organzation


    public class ResultItem
    {
        public string localizedName { get; set; }
       
        public int id { get; set; }
    }

    public class LinkedinAdsOrganization
    {
        public Dictionary<string, ResultItem> results { get; set; }
    }

}
