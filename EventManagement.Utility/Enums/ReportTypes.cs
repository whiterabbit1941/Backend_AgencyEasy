using System;
using System.Collections.Generic;
using System.Text;

namespace EventManagement.Utility.Enums
{
    public enum ReportTypes
    {
        GoogleAnalytics = 1,

        GoogleSearchConsole = 2,

        GoogleAdsCampaign = 3,

        Facebook = 4,

        FacebookAdsCampaign = 5,

        Instagram = 6,

        LinkedInEngagement = 7,

        Keywords = 8,

        FacebookAdsGroup = 9,

        FacebookAdsCopies = 10,

        GoogleAdsGroups = 11,

        GoogleAdsCopies = 12,

        LinkedInDemographic = 13,

        GoogleAnalyticsFour = 14,

        LightHouseData = 15,

        Comments = 19,

        Image = 20,

        LinkedInAdsCampaign = 21,

        LinkedInAdsAdgroups = 22,

        LinkedInAdsCreative = 23,

        //Ga4 subtype

        Ga4OrganicTaffic = 24,
        Ga4OrganicConversion = 25,
        Ga4UserAquasition = 26,
        Ga4TrafficAquasition = 27,
        Ga4EcomPurchase = 28,
        Ga4PurchaseJourney = 29,
        GscClick = 30,
        GscImpression = 31,
        GscCtr = 33,
        GscPosition = 32,

        //Facebook subtypes
        FbImpression = 34,
        FbPerformance = 35,

        //Instagram sub types
        InstaPerformance = 36,
        InstaImpression = 37,
        InstAudiance = 38,

        GoogleBusinessProfile = 39,
        WooCommerce = 40,


        //Gbp Subtype

        GBPSearches = 41,

        GBPInteraction = 42,

        GBPCalls = 43,

        GBPMessages = 44,

        GBPBookings = 45,

        GBPDirections = 46,

        GBPWebsiteClicks = 47,

        GBPViews = 48,


        //Woo commerce Sub type
        WCSales = 49,

        WCOrders = 50,

        WCRevenueTable = 51,

        WCSalesTable = 52,

        WCLocationChart = 53,

        WCReturnCustomer = 54,

        GoogleSheet = 55,

        GoogleSheetPie = 56,

        GoogleSheetBar = 57,

        GoogleSheetLine = 58,

        GoogleSheetTable = 59,

        GoogleSheetStat = 60,

        GoogleSheetSparkLine = 61,


        CallRail = 62,

        CallRailPie = 63,

        CallRailTopSources = 64,

        CallRailCallLine = 65,

        CallRailAnsweredLine = 66,

        CallRailMissedLine = 67,

        CallRailFirstTimeCallLine = 68,

        CallRailLeadLine = 69,

        CallRailAvgDurationLine = 70,

        CallRailAvgAnswerRateLine = 71,

        CallRailAvgMissedRateLine = 72,

        CallRailAvgFirstTimeRateLine = 73,

        CallRailAvgCallPerLead = 74,

        CallRailTable = 75,

        //Mailchimp type

        Mailchimp = 76,

        //Mailchimp sub type
        //
        //------ Campaign List---------

        McCampaignsRecipients = 77,

        McCampaignsOpens = 78,

        McCampaignsClicks = 79,

        McCampaignsTiles = 80,

        McCampaignsTable = 81,

        //----------Single Campaign--------
        McSingleCampaign = 82,

        //--------List---------

        McListsAudianceGrowth = 83,

        McListsOpens = 84,

        McListsClicks = 85,

        McListsTiles = 86,

        McListsTable = 87,

        //------------Single List----------
        McSingleList = 88,

        //Main type
        MicrosoftAds = 91,

        //Subtype for campaign
        MSCampaignLineChart = 93,
        MSCampaignBarChart = 94,
        MSCampaignTiles = 95,
        MSCampaignTable = 96,

        //Subtype for adgroup
        MSAdsGroupLineChart = 98,
        MSAdsGroupBarChart = 99,
        MSAdsGroupTiles = 100,
        MSAdsGroupTable = 101,

        //Sub type for keywords
        MSKeywordsLineChart = 103,
        MSKeywordsBarChart = 104,
        MSKeywordsTiles = 105,
        MSKeywordsTable = 106,

        //Sub type for conversions
        MSConversionLineChart = 108,
        MSConversionBarChart = 109,
        MSConversionTiles = 110,
        MSConversionTable = 111,

        //Single type for microsoft ads
        MSSingleCampaign = 112,
        MSSingleAdsGroup = 113,
        MSSingleKeywords = 114,
        MSSingleConversion = 115,

        //Chat Gpt
        ChatGPT = 116,

        //RESERVERD NUMBER
        //MicrosoftCampaign = 92,       
        //MSAdsGroup = 97,       
        //MSKeywords = 102,       
        //MSConversion = 107,
        //GoogleAds = 117,
        //FacebookAds = 118,
        //linkedin = 119,
        //linkedInAds = 120,
    }
}
