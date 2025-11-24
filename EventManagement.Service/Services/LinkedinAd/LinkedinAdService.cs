using System;
using System.Collections.Generic;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using EventManagement.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using RestSharp.Authenticators.OAuth2;
using RestSharp;
using System.Net.Http;
using System.Net;
using Microsoft.Extensions.Options;
using Google.Apis.Pagespeedonline.v1.Data;
using Google.Protobuf.WellKnownTypes;
using Method = RestSharp.Method;
using ThirdParty.Json.LitJson;
using System.Globalization;
using System.Xml.Linq;
using System.Web;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.IO;
using System.Drawing;
using EventManagement.Utility.Enums;
using static Google.Apis.Pagespeedonline.v1.Data.Result.FormattedResultsData.RuleResultsDataElement.UrlBlocksData.UrlsData;

namespace EventManagement.Service
{
    public class LinkedinAdService : ServiceBase<LinkedinAd, Guid>, ILinkedinAdService
    {

        #region PRIVATE MEMBERS

        private readonly ILinkedinAdRepository _linkedinadRepository;
        private readonly IConfiguration _configuration;
        private readonly ICampaignLinkedinService _campaignLinkedinService;
        #endregion


        #region CONSTRUCTOR

        public LinkedinAdService(ILinkedinAdRepository linkedinadRepository, ILogger<LinkedinAdService> logger,
            IConfiguration configuration, ICampaignLinkedinService campaignLinkedinService) : base(linkedinadRepository, logger)
        {
            _linkedinadRepository = linkedinadRepository;
            _configuration = configuration;
            _campaignLinkedinService = campaignLinkedinService;
        }

        #endregion


        #region PUBLIC MEMBERS   

        public async Task<LinkedinAdRoot> GetLinkedInPages(Guid campaignId)
        {
            var returnData = new LinkedinAdRoot();
            var campaign = _linkedinadRepository.GetAllEntities().Where(x => x.CampaignID == campaignId).FirstOrDefault();

            if (campaign != null)
            {
                var options = new RestClientOptions("https://api.linkedin.com/rest/")
                {
                    Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(campaign.AccessToken, "Bearer")
                };

                var clientForData = new RestClient(options);

                var requestForData = new RestRequest("/adAccounts?q=search&search=(type:(values:List(BUSINESS)),status:(values:List(ACTIVE,CANCELED)))&fields=name,id,status,currency", Method.Post);

                requestForData.AddHeader("Linkedin-Version", "202305");
                requestForData.AddHeader("X-Restli-Protocol-Version", "2.0.0");


                try
                {
                    var response = await clientForData.GetAsync(requestForData);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        returnData = JsonConvert.DeserializeObject<LinkedinAdRoot>(response.Content);
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        var res = await _campaignLinkedinService.GetAccessTokenUsingRefreshToken(campaign.RefreshToken);

                        if (res != null && !string.IsNullOrEmpty(res.access_token))
                        {
                            campaign.AccessToken = res.access_token;
                            campaign.AccessTokenExpiresIn = res.expires_in;
                            campaign.RefreshToken = res.refresh_token;
                            campaign.RefreshTokenExpiresIn = res.refresh_token_expires_in;
                            campaign.UpdatedOn = DateTime.UtcNow;

                            _linkedinadRepository.UpdateEntity(campaign);
                            _linkedinadRepository.SaveChanges();
                            options = new RestClientOptions("https://api.linkedin.com/rest/")
                            {
                                Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(campaign.AccessToken, "Bearer")
                            };

                            clientForData = new RestClient(options);

                            response = clientForData.GetAsync(requestForData).Result;
                            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                returnData = JsonConvert.DeserializeObject<LinkedinAdRoot>(response.Content);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message == "Request failed with status code Unauthorized" && campaign != null)
                    {
                        var res = await _campaignLinkedinService.GetAccessTokenUsingRefreshToken(campaign.RefreshToken);

                        if (res != null)
                        {
                            campaign.AccessToken = res.access_token;
                            campaign.AccessTokenExpiresIn = res.expires_in;
                            campaign.RefreshToken = res.refresh_token;
                            campaign.RefreshTokenExpiresIn = res.refresh_token_expires_in;
                            campaign.UpdatedOn = DateTime.UtcNow;

                            _linkedinadRepository.UpdateEntity(campaign);
                            _linkedinadRepository.SaveChanges();
                        }
                       

                        options = new RestClientOptions("https://api.linkedin.com/rest/")
                        {
                            Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(campaign.AccessToken, "Bearer")
                        };

                        clientForData = new RestClient(options);

                        var response = clientForData.GetAsync(requestForData).Result;
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            return returnData = JsonConvert.DeserializeObject<LinkedinAdRoot>(response.Content);
                        }
                    }
                    else
                    {
                        return null;
                    }
                }

            }
            return returnData;
        }

        public async Task<AnalyticsRoot> GetPreparedLinkedinAdData(string campaignId, string type, string startDate, string endDate)
        {
            AnalyticsRoot analyticsRootData = new AnalyticsRoot();
            AnalyticsRoot prevAnalyticsRootData = new AnalyticsRoot();
            DempgraphicRoot dempgraphicRoot = new DempgraphicRoot();
            LinkedinAdsCardData linkedinAdsCardData = new LinkedinAdsCardData();
            double total_clicks = 0;
            double total_spent = 0;
            double total_leads = 0;
            double total_cpl = 0;
            double total_impression = 0;

            double percent_clicks = 0;
            double percent_spent = 0;
            double percent_leads = 0;
            double percent_cpl = 0;

            int ad_count = 0;

            //var startTime = System.DateTime.ParseExact("2019-03-01", "yyyy-MM-dd", CultureInfo.InvariantCulture);

            //var endTime = System.DateTime.ParseExact("2019-03-31", "yyyy-MM-dd", CultureInfo.InvariantCulture);

            var startTime = System.DateTime.ParseExact(startDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            var endTime = System.DateTime.ParseExact(endDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            var previousDate = CalculatePreviousStartDateAndEndDate(startTime, endTime);

            if (type == "CAMPAIGN")
            {

                analyticsRootData = await GetLinkedinAdsAnalytics(campaignId.ToString(), type, startDate, endDate);
                //analyticsRootData = await GetLinkedinAdsAnalytics(campaignId.ToString(), type, "2019-03-01", "2019-03-31");
                prevAnalyticsRootData = await GetLinkedinAdsAnalytics(campaignId.ToString(), type, previousDate.PreviousStartDate.ToString("yyyy-MM-dd"), previousDate.PreviousEndDate.ToString("yyyy-MM-dd"));

                if (analyticsRootData.campaignRoot != null && prevAnalyticsRootData.campaignRoot != null)
                {

                    percent_clicks = GetYearWiseDifference(analyticsRootData.campaignRoot.elements.Sum(x => x.clicks), prevAnalyticsRootData.campaignRoot.elements.Sum(x => x.clicks));
                    percent_spent = GetYearWiseDifference(analyticsRootData.campaignRoot.elements.Sum(x => x.spent), prevAnalyticsRootData.campaignRoot.elements.Sum(x => x.spent));
                    percent_leads = GetYearWiseDifference(analyticsRootData.campaignRoot.elements.Sum(x => x.lead), prevAnalyticsRootData.campaignRoot.elements.Sum(x => x.lead));
                    percent_cpl = GetYearWiseDifference(analyticsRootData.campaignRoot.elements.Sum(x => x.cost_per_lead), prevAnalyticsRootData.campaignRoot.elements.Sum(x => x.cost_per_lead));

                    total_clicks = Math.Round(analyticsRootData.campaignRoot.elements.Sum(x => x.clicks), 2);
                    total_spent = Math.Round(analyticsRootData.campaignRoot.elements.Sum(x => x.spent), 2);
                    total_impression = Math.Round(analyticsRootData.campaignRoot.elements.Sum(x => x.impressions), 2);
                    total_leads = Math.Round(analyticsRootData.campaignRoot.elements.Sum(x => x.lead), 2);
                    total_cpl = Math.Round(analyticsRootData.campaignRoot.elements.Sum(x => x.cost_per_lead), 2);

                    ad_count = analyticsRootData.campaignRoot.elements.Count();
                }

            }
            else if (type == "CAMPAIGN_GROUP")
            {
                analyticsRootData = await GetLinkedinAdsAnalytics(campaignId.ToString(), type, startDate, endDate);
                prevAnalyticsRootData = await GetLinkedinAdsAnalytics(campaignId.ToString(), type, previousDate.PreviousStartDate.ToString("yyyy-MM-dd"), previousDate.PreviousEndDate.ToString("yyyy-MM-dd"));

                if (analyticsRootData.adGroupRoot != null && prevAnalyticsRootData.adGroupRoot != null)
                {
                    percent_clicks = GetYearWiseDifference(analyticsRootData.adGroupRoot.elements.Sum(x => x.clicks), prevAnalyticsRootData.adGroupRoot.elements.Sum(x => x.clicks));
                    percent_spent = GetYearWiseDifference(analyticsRootData.adGroupRoot.elements.Sum(x => x.spent), prevAnalyticsRootData.adGroupRoot.elements.Sum(x => x.spent));
                    percent_leads = GetYearWiseDifference(analyticsRootData.adGroupRoot.elements.Sum(x => x.lead), prevAnalyticsRootData.adGroupRoot.elements.Sum(x => x.lead));
                    percent_cpl = GetYearWiseDifference(analyticsRootData.adGroupRoot.elements.Sum(x => x.cost_per_lead), prevAnalyticsRootData.adGroupRoot.elements.Sum(x => x.cost_per_lead));

                    total_clicks = Math.Round(analyticsRootData.adGroupRoot.elements.Sum(x => x.clicks), 2);
                    total_spent = Math.Round(analyticsRootData.adGroupRoot.elements.Sum(x => x.spent), 2);
                    total_impression = Math.Round(analyticsRootData.adGroupRoot.elements.Sum(x => x.impressions), 2);
                    total_leads = Math.Round(analyticsRootData.adGroupRoot.elements.Sum(x => x.lead), 2);
                    total_cpl = Math.Round(analyticsRootData.adGroupRoot.elements.Sum(x => x.cost_per_lead), 2);

                    ad_count = analyticsRootData.adGroupRoot.elements.Count();
                }
            }
            else if (type == "CREATIVE")
            {

                analyticsRootData = await GetLinkedinAdsAnalytics(campaignId.ToString(), type, startDate, endDate);
                prevAnalyticsRootData = await GetLinkedinAdsAnalytics(campaignId.ToString(), type, previousDate.PreviousStartDate.ToString("yyyy-MM-dd"), previousDate.PreviousEndDate.ToString("yyyy-MM-dd"));

                if (analyticsRootData.creativeRoot != null && prevAnalyticsRootData.creativeRoot != null)
                {
                    percent_clicks = GetYearWiseDifference(analyticsRootData.creativeRoot.elements.Sum(x => x.clicks), prevAnalyticsRootData.creativeRoot.elements.Sum(x => x.clicks));
                    percent_spent = GetYearWiseDifference(analyticsRootData.creativeRoot.elements.Sum(x => x.spent), prevAnalyticsRootData.creativeRoot.elements.Sum(x => x.spent));
                    percent_leads = GetYearWiseDifference(analyticsRootData.creativeRoot.elements.Sum(x => x.lead), prevAnalyticsRootData.creativeRoot.elements.Sum(x => x.lead));
                    percent_cpl = GetYearWiseDifference(analyticsRootData.creativeRoot.elements.Sum(x => x.cost_per_lead), prevAnalyticsRootData.creativeRoot.elements.Sum(x => x.cost_per_lead));

                    total_clicks = Math.Round(analyticsRootData.creativeRoot.elements.Sum(x => x.clicks), 2);
                    total_spent = Math.Round(analyticsRootData.creativeRoot.elements.Sum(x => x.spent), 2);
                    total_impression = Math.Round(analyticsRootData.creativeRoot.elements.Sum(x => x.impressions), 2);
                    total_leads = Math.Round(analyticsRootData.creativeRoot.elements.Sum(x => x.lead), 2);
                    total_cpl = Math.Round(analyticsRootData.creativeRoot.elements.Sum(x => x.cost_per_lead), 2);

                    ad_count = analyticsRootData.creativeRoot.elements.Count();
                }

            }

            linkedinAdsCardData.percent_clicks = percent_clicks.ToString();
            linkedinAdsCardData.percent_spent = percent_spent.ToString();
            linkedinAdsCardData.percent_leads = percent_leads.ToString();
            linkedinAdsCardData.percent_cpl = percent_cpl.ToString();

            linkedinAdsCardData.ad_count = ad_count.ToString();

            linkedinAdsCardData.total_clicks = total_clicks.ToString();
            linkedinAdsCardData.total_leads = total_leads.ToString();
            linkedinAdsCardData.total_cpl = total_cpl.ToString();
            linkedinAdsCardData.total_spent = total_spent.ToString();
            linkedinAdsCardData.total_impressions = total_impression.ToString();

            analyticsRootData.cardData = linkedinAdsCardData;

            return analyticsRootData;
        }

        public PreviousDate CalculatePreviousStartDateAndEndDate(DateTime startDate, DateTime endDate)
        {
            var previousDate = new PreviousDate();
            var diff = (endDate - startDate).TotalDays;
            diff = Math.Round(diff);

            previousDate.PreviousEndDate = startDate.AddDays(-1);
            previousDate.PreviousStartDate = previousDate.PreviousEndDate.AddDays(-diff);

            return previousDate;
        }

        private double GetYearWiseDifference(double current, double previous)
        {
            double res;
            if (previous == 0 && current != 0)
            {
                res = 100.00;
            }
            else if (previous != 0 && current == 0)
            {
                res = -100.00;
            }
            else if (current == 0 && previous == 0)
            {
                res = 0;
            }
            else
            {
                var diff = (current - previous) / previous * 100.0;
                res = diff;
            }
            res = Math.Round(res, 2);
            return res;

        }


        public async Task<AnalyticsRoot> GetLinkedinAdsAnalytics(string campaignId, string type, string startTime, string endTime)
        {
            var retval = new AnalyticsRoot();

            //startTime = "2019-03-01";
            //endTime = "2019-03-31";
            //type = "CAMPAIGN";
            // Split the string by '-' delimiter
            string[] parts = startTime.Split('-');

            // Extract the date, month, and year
            int year = int.Parse(parts[0]);
            int month = int.Parse(parts[1]);
            int day = int.Parse(parts[2]);

            // Split the string by '-' delimiter
            string[] parts1 = endTime.Split('-');

            // Extract the date, month, and year
            int yearEnd = int.Parse(parts1[0]);
            int monthEnd = int.Parse(parts1[1]);
            int dayEnd = int.Parse(parts1[2]);

            var campaign = _linkedinadRepository.GetAllEntities().Where(x => x.CampaignID == new Guid(campaignId)).FirstOrDefault();
            var options = new RestClientOptions("https://api.linkedin.com/rest/")
            {
                Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(campaign.AccessToken, "Bearer")
            };

            var clientForData = new RestClient(options);

            var requestForData = new RestRequest("adAnalytics", Method.Get);
            requestForData.AddQueryParameter("q", "analytics", false);
            requestForData.AddQueryParameter("count", "1000", false);
            requestForData.AddQueryParameter("dateRange", "(start:(year:" + year + ",month:" + month + ",day:" + day + "),end:(year:" + yearEnd + ",month:" + monthEnd + ",day:" + dayEnd + "))", false);
            requestForData.AddQueryParameter("timeGranularity", "(value:DAILY)", false);
            requestForData.AddQueryParameter("accounts", "List(urn%3Ali%3AsponsoredAccount%3A" + campaign.OrganizationalEntity + ")", false);
            requestForData.AddQueryParameter("pivot", "(value:" + type + ")", false);
            requestForData.AddQueryParameter("fields", "clicks,externalWebsiteConversions,dateRange,impressions,landingPageClicks,likes,shares,costInLocalCurrency,approximateUniqueImpressions,pivotValue,oneClickLeads", false);

            requestForData.AddHeader("Linkedin-Version", "202304");
            requestForData.AddHeader("X-Restli-Protocol-Version", "2.0.0");

            try
            {
                if (campaign != null)
                {
                    var response = await clientForData.GetAsync(requestForData);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var returnData = JsonConvert.DeserializeObject<LinkedinStatRoot>(response.Content);

                        //Get currency code
                        var currency = campaign.Currency;

                        var currencyCode = new List<Currency>();
                        var restClient = new RestClient(_configuration.GetSection("BlobUrl").Value + "Json");
                        var restRequest = new RestRequest("/currency_code.json", Method.Get);

                        var responseCode = restClient.GetAsync(restRequest).Result;
                        if (responseCode.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            currencyCode = JsonConvert.DeserializeObject<List<Currency>>(responseCode.Content);
                        }

                        var currency_symbol = currencyCode.Where(y => y.code == currency).Select(x => x.symbol).FirstOrDefault();


                        if (type == "CAMPAIGN")
                        {
                            var campaignData = await GetAllCampaigns(campaign);


                            foreach (var item in campaignData.elements)
                            {
                                double totalclick = 0;
                                double total_impression = 0;
                                double cost = 0;
                                double totalOneClickLeads = 0;

                                var cam_id = "urn:li:sponsoredCampaign:" + item.id;
                                totalclick = returnData.elements.Where(x => x.pivotValue == cam_id).Sum(x => x.clicks);
                                total_impression = returnData.elements.Where(x => x.pivotValue == cam_id).Sum(x => x.impressions);
                                cost = returnData.elements.Where(x => x.pivotValue == cam_id).Sum(x => double.Parse(x.costInLocalCurrency));
                                totalOneClickLeads = returnData.elements.Where(x => x.pivotValue == cam_id).Sum(x => x.oneClickLeads);


                                var avgctr = total_impression > 0 ? (double)((double)(totalclick / total_impression) * 100) : 0;
                                var avgcpm = total_impression > 0 ? (double)((double)(cost / total_impression) * 1000) : 0;
                                var avgcpc = totalclick > 0 ? (double)(cost / totalclick) : 0;

                                item.clicks = totalclick > 0 ? Math.Round(totalclick) : 0;
                                item.impressions = total_impression > 0 ? Math.Round(total_impression, 2) : 0;
                                item.spent = cost > 0 ? Math.Round(cost, 2) : 0;
                                item.avg_ctr = total_impression > 0 ? Math.Round(avgctr, 2) : 0;
                                item.avg_cpm = total_impression > 0 ? Math.Round(avgcpm, 2) : 0;
                                item.avg_cpc = totalclick > 0 ? Math.Round(avgcpc, 2) : 0;
                                item.lead = totalOneClickLeads;
                                item.cost_per_lead = totalOneClickLeads > 0 ? Math.Round(cost/totalOneClickLeads) : 0;
                                item.currency = currency_symbol; 
                                
                            }


                            //adding calucation for avg cpc cpm ctr

                            foreach (var item in returnData.elements)
                            {
                                item.avg_cpm = item.impressions > 0 ? (double)((double)(double.Parse(item.costInLocalCurrency) / item.impressions) * 1000) : 0;
                                item.avg_cpc = item.clicks > 0 ? (double)(double.Parse(item.costInLocalCurrency) / item.clicks) : 0;
                                item.date = item.dateRange.start.year + "-" + item.dateRange.start.month.ToString("D2") + "-" +
                                                    item.dateRange.start.day.ToString("D2");

                            }


                            //adding zero data                      
                            // Create a list of all dates within the specified range
                            DateTime startDate = DateTime.ParseExact(startTime, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            DateTime endDate = DateTime.ParseExact(endTime, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                            List<DateTime> allDates = Enumerable.Range(0, 1 + endDate.Subtract(startDate).Days)
                                .Select(offset => startDate.AddDays(offset))
                                .ToList();

                            //Make sum and delete duplicate entries date wise
                            returnData.elements = returnData.elements.GroupBy(e => e.date)
                                      .Select(g => new LinkedinAdsElement
                                      {
                                          clicks = g.Sum(e => e.clicks),
                                          impressions = g.Sum(e => e.impressions),
                                          avg_cpc = g.Average(e => e.avg_cpc),
                                          avg_cpm = g.Average(e => e.avg_cpm),
                                          costInLocalCurrency = g.Sum(e => double.Parse(e.costInLocalCurrency)).ToString(),
                                          date = g.First().date,
                                          dateRange = g.First().dateRange,
                                          pivotValue = g.First().pivotValue                                       
                                      }).ToList();

                            // Create a dictionary to store the data by date
                            var dataByDate = returnData.elements.ToDictionary(e => GetDateFromRange(e.dateRange), e => e);

                            // Iterate through all dates in the range
                            foreach (DateTime date in allDates)
                            {
                                // Check if the date exists in the data
                                if (!dataByDate.ContainsKey(date))
                                {
                                    var daydigit = date.Day.ToString("D2");
                                    var monthdigit = date.Month.ToString("D2");
                                    // Add an empty LinkedinAdsElement object for the missing date
                                    var emptyElement = new LinkedinAdsElement
                                    {

                                        dateRange = new DateRangeForLinkedin
                                        {
                                            start = new Start { month = date.Month, day = date.Day, year = date.Year },
                                            end = new End { month = date.Month, day = date.Day, year = date.Year },
                                        },
                                        clicks = 0,
                                        impressions = 0,
                                        avg_cpc = 0,
                                        avg_cpm = 0,
                                        date = date.Year + "-" + monthdigit + "-" + daydigit,
                                        oneClickLeads = 0
                                        
                                    };
                                    returnData.elements.Add(emptyElement);
                                }
                            }

                            //sorting date wise
                            returnData.elements.Sort((e1, e2) => GetDateFromRange(e1.dateRange).CompareTo(GetDateFromRange(e2.dateRange)));



                            retval.linkedinStat = returnData;
                            retval.campaignRoot = campaignData;

                            return retval;

                        }
                        else if (type == "CAMPAIGN_GROUP")
                        {
                            var adgroupsData = await GetAllAdGroupsData(campaign);

                            foreach (var item in adgroupsData.elements)
                            {
                                double totalclick = 0;
                                double total_impression = 0;
                                double cost = 0;
                                double totalOneClickLeads = 0;

                                var cam_id = "urn:li:sponsoredCampaignGroup:" + item.id;
                                totalclick = returnData.elements.Where(x => x.pivotValue == cam_id).Sum(x => x.clicks);
                                total_impression = returnData.elements.Where(x => x.pivotValue == cam_id).Sum(x => x.impressions);
                                cost = returnData.elements.Where(x => x.pivotValue == cam_id).Sum(x => double.Parse(x.costInLocalCurrency));
                                totalOneClickLeads = returnData.elements.Where(x => x.pivotValue == cam_id).Sum(x => x.oneClickLeads);


                                var avgctr = total_impression > 0 ? (double)((double)(totalclick / total_impression) * 100) : 0;
                                var avgcpm = total_impression > 0 ? (double)((double)(cost / total_impression) * 1000) : 0;
                                var avgcpc = totalclick > 0 ? (double)(cost / totalclick) : 0;

                                item.clicks = totalclick > 0 ? Math.Round(totalclick) : 0;
                                item.impressions = total_impression > 0 ? Math.Round(total_impression, 2) : 0;
                                item.spent = cost > 0 ? Math.Round(cost, 2) : 0;
                                item.avg_ctr = total_impression > 0 ? Math.Round(avgctr, 2) : 0;
                                item.avg_cpm = total_impression > 0 ? Math.Round(avgcpm, 2) : 0;
                                item.avg_cpc = totalclick > 0 ? Math.Round(avgcpc, 2) : 0;
                                item.lead = totalOneClickLeads;
                                item.cost_per_lead = totalOneClickLeads > 0 ? Math.Round(cost / totalOneClickLeads) : 0;
                                currency = currency_symbol;
                                item.currency = currency_symbol;                               
                            }


                            //adding calucation for avg cpc cpm ctr

                            foreach (var item in returnData.elements)
                            {
                                item.avg_cpm = item.impressions > 0 ? (double)((double)(double.Parse(item.costInLocalCurrency) / item.impressions) * 1000) : 0;
                                item.avg_cpc = item.clicks > 0 ? (double)(double.Parse(item.costInLocalCurrency) / item.clicks) : 0;
                                item.date = item.dateRange.start.year + "-" + item.dateRange.start.month.ToString("D2") + "-" + item.dateRange.start.day.ToString("D2");
                            }


                            //adding zero data                      
                            // Create a list of all dates within the specified range
                            DateTime startDate = DateTime.ParseExact(startTime, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            DateTime endDate = DateTime.ParseExact(endTime, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                            List<DateTime> allDates = Enumerable.Range(0, 1 + endDate.Subtract(startDate).Days)
                                .Select(offset => startDate.AddDays(offset))
                                .ToList();
                            //Make sum and delete duplicate entries date wise
                            returnData.elements = returnData.elements.GroupBy(e => e.date)
                                      .Select(g => new LinkedinAdsElement
                                      {
                                          clicks = g.Sum(e => e.clicks),
                                          impressions = g.Sum(e => e.impressions),
                                          avg_cpc = g.Average(e => e.avg_cpc),
                                          avg_cpm = g.Average(e => e.avg_cpm),
                                          costInLocalCurrency = g.Sum(e => double.Parse(e.costInLocalCurrency)).ToString(),
                                          date = g.First().date,
                                          dateRange = g.First().dateRange,
                                          pivotValue = g.First().pivotValue                                         
                                      }).ToList();

                            // Create a dictionary to store the data by date
                            var dataByDate = returnData.elements.ToDictionary(e => GetDateFromRange(e.dateRange), e => e);

                            // Iterate through all dates in the range
                            foreach (DateTime date in allDates)
                            {

                                // Check if the date exists in the data
                                if (!dataByDate.ContainsKey(date))
                                {
                                    var daydigit = date.Day.ToString("D2");
                                    var monthdigit = date.Month.ToString("D2");

                                    // Add an empty LinkedinAdsElement object for the missing date
                                    var emptyElement = new LinkedinAdsElement
                                    {

                                        dateRange = new DateRangeForLinkedin
                                        {
                                            start = new Start { month = date.Month, day = date.Day, year = date.Year },
                                            end = new End { month = date.Month, day = date.Day, year = date.Year },
                                        },
                                        clicks = 0,
                                        impressions = 0,
                                        avg_cpc = 0,
                                        avg_cpm = 0,
                                        date = date.Year + "-" + monthdigit + "-" + daydigit,

                                    };
                                    returnData.elements.Add(emptyElement);
                                }
                            }

                            //sorting date wise
                            returnData.elements.Sort((e1, e2) => GetDateFromRange(e1.dateRange).CompareTo(GetDateFromRange(e2.dateRange)));

                            retval.linkedinStat = returnData;
                            retval.adGroupRoot = adgroupsData;

                            return retval;
                        }
                        else if (type == "CREATIVE")
                        {
                            var creativeData = await GetAdsData(campaign);

                            List<string> listOfShareIds = creativeData.elements.Where(x => x.content?.reference != null).Select(x => HttpUtility.UrlEncode(x.content.reference)).Distinct().ToList();
                            List<string> listOfImageId = creativeData.elements.Where(x => x.content.reference == null).Select(x => HttpUtility.UrlEncode(x.content.follow.logo)).Distinct().ToList();


                            ShareResponse shareResponse = null;
                            if (listOfShareIds.Count > 0)
                            {
                                shareResponse = await GetListOfShares(campaign, listOfShareIds);
                            }

                            //List<string> listOfImageId = creativeData.elements.Where(x => x.content.reference == null).Select(x => HttpUtility.UrlEncode(x.content.follow.logo)).ToList();
                            ImageResponse imageResponse = null;
                            if (listOfImageId.Count > 0)
                            {
                                imageResponse = await GetListOfImages(campaign, listOfImageId);
                            }

                            foreach (var item in creativeData.elements)
                            {
                                double totalclick = 0;
                                double total_impression = 0;
                                double cost = 0;
                                string id = item.id.Substring(item.id.LastIndexOf(':') + 1);
                                string share_id = string.Empty;
                                var image = string.Empty;
                                var name = id;
                                double totalOneClickLeads = 0;

                                //set image after getting from apis
                                if (!string.IsNullOrEmpty(item.content.reference) && shareResponse != null && shareResponse.results != null)
                                {
                                    share_id = item.content.reference.Substring(item.content.reference.LastIndexOf(':') + 1);
                                    if (shareResponse.results.ContainsKey(share_id))
                                    {
                                        image = shareResponse.results[share_id].content.contentEntities[0].thumbnails[0].resolvedUrl;
                                        name = shareResponse.results[share_id].text.text;
                                    }

                                }
                                else if (string.IsNullOrEmpty(item.content.reference) && imageResponse != null && imageResponse.results != null && imageResponse.results.ContainsKey(item.content.follow.logo))
                                {
                                    image = imageResponse.results[item.content.follow.logo].downloadUrl;
                                    name = item.content.follow.headline.preApproved + " " + item.content.follow.headline.custom + " " + item.content.follow.description.preApproved + " " + item.content.follow.description.custom;
                                }

                                var creative_id = item.id;
                                totalclick = returnData.elements.Where(x => x.pivotValue == creative_id).Sum(x => x.clicks);
                                total_impression = returnData.elements.Where(x => x.pivotValue == creative_id).Sum(x => x.impressions);
                                cost = returnData.elements.Where(x => x.pivotValue == creative_id).Sum(x => double.Parse(x.costInLocalCurrency));
                                totalOneClickLeads = returnData.elements.Where(x => x.pivotValue == creative_id).Sum(x => x.oneClickLeads);

                                var avgctr = total_impression > 0 ? (double)((double)(totalclick / total_impression) * 100) : 0;
                                var avgcpm = total_impression > 0 ? (double)((double)(cost / total_impression) * 1000) : 0;
                                var avgcpc = totalclick > 0 ? (double)(cost / totalclick) : 0;

                                item.clicks = totalclick > 0 ? Math.Round(totalclick) : 0;
                                item.impressions = total_impression > 0 ? Math.Round(total_impression, 2) : 0;
                                item.spent = cost > 0 ? Math.Round(cost, 2) : 0;
                                item.avg_ctr = total_impression > 0 ? Math.Round(avgctr, 2) : 0;
                                item.avg_cpm = total_impression > 0 ? Math.Round(avgcpm, 2) : 0;
                                item.avg_cpc = totalclick > 0 ? Math.Round(avgcpc, 2) : 0;
                                item.lead = totalOneClickLeads;
                                item.cost_per_lead = totalOneClickLeads > 0 ? Math.Round(cost / totalOneClickLeads) : 0;
                                item.image = image;

                                item.name = name;
                                item.status = item.intendedStatus;
                                item.currency = currency_symbol;                               
                            }


                            //adding calucation for avg cpc cpm ctr

                            foreach (var item in returnData.elements)
                            {
                                item.avg_cpm = item.impressions > 0 ? (double)((double)(double.Parse(item.costInLocalCurrency) / item.impressions) * 1000) : 0;
                                item.avg_cpc = item.clicks > 0 ? (double)(double.Parse(item.costInLocalCurrency) / item.clicks) : 0;
                                item.date = item.dateRange.start.year + "-" + item.dateRange.start.month.ToString("D2") + "-" + item.dateRange.start.day.ToString("D2");
                            }

                            //adding zero data                      
                            // Create a list of all dates within the specified range
                            DateTime startDate = DateTime.ParseExact(startTime, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            DateTime endDate = DateTime.ParseExact(endTime, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                            List<DateTime> allDates = Enumerable.Range(0, 1 + endDate.Subtract(startDate).Days)
                                .Select(offset => startDate.AddDays(offset))
                                .ToList();

                            //Make sum and delete duplicate entries date wise
                            returnData.elements = returnData.elements.GroupBy(e => e.date)
                                      .Select(g => new LinkedinAdsElement
                                      {
                                          clicks = g.Sum(e => e.clicks),
                                          impressions = g.Sum(e => e.impressions),
                                          avg_cpc = g.Average(e => e.avg_cpc),
                                          avg_cpm = g.Average(e => e.avg_cpm),
                                          costInLocalCurrency = g.Sum(e => double.Parse(e.costInLocalCurrency)).ToString(),
                                          date = g.First().date,
                                          dateRange = g.First().dateRange,
                                          pivotValue = g.First().pivotValue
                                      }).ToList();


                            // Create a dictionary to store the data by date
                            var dataByDate = returnData.elements.ToDictionary(e => GetDateFromRange(e.dateRange), e => e);

                            // Iterate through all dates in the range
                            foreach (DateTime date in allDates)
                            {
                                // Check if the date exists in the data
                                if (!dataByDate.ContainsKey(date))
                                {
                                    var daydigit = date.Day.ToString("D2");
                                    var monthdigit = date.Month.ToString("D2");

                                    // Add an empty LinkedinAdsElement object for the missing date
                                    var emptyElement = new LinkedinAdsElement
                                    {

                                        dateRange = new DateRangeForLinkedin
                                        {
                                            start = new Start { month = date.Month, day = date.Day, year = date.Year },
                                            end = new End { month = date.Month, day = date.Day, year = date.Year },
                                        },
                                        clicks = 0,
                                        impressions = 0,
                                        avg_cpc = 0,
                                        avg_cpm = 0,
                                        date = date.Year + "-" + monthdigit + "-" + daydigit,

                                    };
                                    returnData.elements.Add(emptyElement);
                                }
                            }

                            //sorting date wise
                            returnData.elements.Sort((e1, e2) => GetDateFromRange(e1.dateRange).CompareTo(GetDateFromRange(e2.dateRange)));

                            retval.linkedinStat = returnData;
                            retval.creativeRoot = creativeData;


                        }

                        return retval;
                    }

                }
                return retval;
            }
            catch (Exception ex)
            {
                if (ex.Message == "Request failed with status code Unauthorized" && campaign != null)
                {
                    var res = _campaignLinkedinService.GetAccessTokenUsingRefreshToken(campaign.RefreshToken).Result;

                    if(res != null)
                    {
                        campaign.AccessToken = res.access_token;
                        campaign.AccessTokenExpiresIn = res.expires_in;
                        campaign.RefreshToken = res.refresh_token;
                        campaign.RefreshTokenExpiresIn = res.refresh_token_expires_in;
                        campaign.UpdatedOn = DateTime.UtcNow;

                        _linkedinadRepository.UpdateEntity(campaign);
                        _linkedinadRepository.SaveChanges();
                    }                

                    options = new RestClientOptions("https://api.linkedin.com/rest/")
                    {
                        Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(campaign.AccessToken, "Bearer")
                    };

                    clientForData = new RestClient(options);

                    var response = clientForData.GetAsync(requestForData).Result;

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var returnData = JsonConvert.DeserializeObject<LinkedinStatRoot>(response.Content);

                        //Get currency code
                        var currency = campaign.Currency;

                        var currencyCode = new List<Currency>();
                        var restClient = new RestClient(_configuration.GetSection("BlobUrl").Value + "Json");
                        var restRequest = new RestRequest("/currency_code.json", Method.Get);

                        var responseCode = restClient.GetAsync(restRequest).Result;
                        if (responseCode.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            currencyCode = JsonConvert.DeserializeObject<List<Currency>>(responseCode.Content);
                        }

                        var currency_symbol = currencyCode.Where(y => y.code == currency).Select(x => x.symbol).FirstOrDefault();


                        if (type == "CAMPAIGN")
                        {
                            var campaignData = await GetAllCampaigns(campaign);


                            foreach (var item in campaignData.elements)
                            {
                                double totalclick = 0;
                                double total_impression = 0;
                                double cost = 0;

                                var cam_id = "urn:li:sponsoredCampaign:" + item.id;
                                totalclick = returnData.elements.Where(x => x.pivotValue == cam_id).Sum(x => x.clicks);
                                total_impression = returnData.elements.Where(x => x.pivotValue == cam_id).Sum(x => x.impressions);
                                cost = returnData.elements.Where(x => x.pivotValue == cam_id).Sum(x => double.Parse(x.costInLocalCurrency));
                                var avgctr = total_impression > 0 ? (double)((double)(totalclick / total_impression) * 100) : 0;
                                var avgcpm = total_impression > 0 ? (double)((double)(cost / total_impression) * 1000) : 0;
                                var avgcpc = totalclick > 0 ? (double)(cost / totalclick) : 0;

                                item.clicks = totalclick > 0 ? Math.Round(totalclick) : 0;
                                item.impressions = total_impression > 0 ? Math.Round(total_impression, 2) : 0;
                                item.spent = cost > 0 ? Math.Round(cost, 2) : 0;
                                item.avg_ctr = total_impression > 0 ? Math.Round(avgctr, 2) : 0;
                                item.avg_cpm = total_impression > 0 ? Math.Round(avgcpm, 2) : 0;
                                item.avg_cpc = totalclick > 0 ? Math.Round(avgcpc, 2) : 0;
                                item.lead = 0;
                                item.cost_per_lead = 0;
                                item.currency = currency_symbol;

                            }


                            //adding calucation for avg cpc cpm ctr

                            foreach (var item in returnData.elements)
                            {
                                item.avg_cpm = item.impressions > 0 ? (double)((double)(double.Parse(item.costInLocalCurrency) / item.impressions) * 1000) : 0;
                                item.avg_cpc = item.clicks > 0 ? (double)(double.Parse(item.costInLocalCurrency) / item.clicks) : 0;
                                item.date = item.dateRange.start.year + "-" + item.dateRange.start.month.ToString("D2") + "-" +
                                                    item.dateRange.start.day.ToString("D2");

                            }


                            //adding zero data                      
                            // Create a list of all dates within the specified range
                            DateTime startDate = DateTime.ParseExact(startTime, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            DateTime endDate = DateTime.ParseExact(endTime, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                            List<DateTime> allDates = Enumerable.Range(0, 1 + endDate.Subtract(startDate).Days)
                                .Select(offset => startDate.AddDays(offset))
                                .ToList();

                            //Make sum and delete duplicate entries date wise
                            returnData.elements = returnData.elements.GroupBy(e => e.date)
                                      .Select(g => new LinkedinAdsElement
                                      {
                                          clicks = g.Sum(e => e.clicks),
                                          impressions = g.Sum(e => e.impressions),
                                          avg_cpc = g.Average(e => e.avg_cpc),
                                          avg_cpm = g.Average(e => e.avg_cpm),
                                          costInLocalCurrency = g.Sum(e => double.Parse(e.costInLocalCurrency)).ToString(),
                                          date = g.First().date,
                                          dateRange = g.First().dateRange,
                                          pivotValue = g.First().pivotValue
                                      }).ToList();

                            // Create a dictionary to store the data by date
                            var dataByDate = returnData.elements.ToDictionary(e => GetDateFromRange(e.dateRange), e => e);

                            // Iterate through all dates in the range
                            foreach (DateTime date in allDates)
                            {
                                // Check if the date exists in the data
                                if (!dataByDate.ContainsKey(date))
                                {
                                    var daydigit = date.Day.ToString("D2");
                                    var monthdigit = date.Month.ToString("D2");
                                    // Add an empty LinkedinAdsElement object for the missing date
                                    var emptyElement = new LinkedinAdsElement
                                    {

                                        dateRange = new DateRangeForLinkedin
                                        {
                                            start = new Start { month = date.Month, day = date.Day, year = date.Year },
                                            end = new End { month = date.Month, day = date.Day, year = date.Year },
                                        },
                                        clicks = 0,
                                        impressions = 0,
                                        avg_cpc = 0,
                                        avg_cpm = 0,
                                        date = date.Year + "-" + monthdigit + "-" + daydigit,

                                    };
                                    returnData.elements.Add(emptyElement);
                                }
                            }

                            //sorting date wise
                            returnData.elements.Sort((e1, e2) => GetDateFromRange(e1.dateRange).CompareTo(GetDateFromRange(e2.dateRange)));



                            retval.linkedinStat = returnData;
                            retval.campaignRoot = campaignData;

                            return retval;

                        }
                        else if (type == "CAMPAIGN_GROUP")
                        {
                            var adgroupsData = await GetAllAdGroupsData(campaign);

                            foreach (var item in adgroupsData.elements)
                            {
                                double totalclick = 0;
                                double total_impression = 0;
                                double cost = 0;

                                var cam_id = "urn:li:sponsoredCampaignGroup:" + item.id;
                                totalclick = returnData.elements.Where(x => x.pivotValue == cam_id).Sum(x => x.clicks);
                                total_impression = returnData.elements.Where(x => x.pivotValue == cam_id).Sum(x => x.impressions);
                                cost = returnData.elements.Where(x => x.pivotValue == cam_id).Sum(x => double.Parse(x.costInLocalCurrency));

                                var avgctr = total_impression > 0 ? (double)((double)(totalclick / total_impression) * 100) : 0;
                                var avgcpm = total_impression > 0 ? (double)((double)(cost / total_impression) * 1000) : 0;
                                var avgcpc = totalclick > 0 ? (double)(cost / totalclick) : 0;

                                item.clicks = totalclick > 0 ? Math.Round(totalclick) : 0;
                                item.impressions = total_impression > 0 ? Math.Round(total_impression, 2) : 0;
                                item.spent = cost > 0 ? Math.Round(cost, 2) : 0;
                                item.avg_ctr = total_impression > 0 ? Math.Round(avgctr, 2) : 0;
                                item.avg_cpm = total_impression > 0 ? Math.Round(avgcpm, 2) : 0;
                                item.avg_cpc = totalclick > 0 ? Math.Round(avgcpc, 2) : 0;
                                item.lead = 0;
                                item.cost_per_lead = 0;
                                currency = currency_symbol;
                                item.currency = currency_symbol;
                            }


                            //adding calucation for avg cpc cpm ctr

                            foreach (var item in returnData.elements)
                            {
                                item.avg_cpm = item.impressions > 0 ? (double)((double)(double.Parse(item.costInLocalCurrency) / item.impressions) * 1000) : 0;
                                item.avg_cpc = item.clicks > 0 ? (double)(double.Parse(item.costInLocalCurrency) / item.clicks) : 0;
                                item.date = item.dateRange.start.year + "-" + item.dateRange.start.month.ToString("D2") + "-" + item.dateRange.start.day.ToString("D2");
                            }


                            //adding zero data                      
                            // Create a list of all dates within the specified range
                            DateTime startDate = DateTime.ParseExact(startTime, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            DateTime endDate = DateTime.ParseExact(endTime, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                            List<DateTime> allDates = Enumerable.Range(0, 1 + endDate.Subtract(startDate).Days)
                                .Select(offset => startDate.AddDays(offset))
                                .ToList();
                            //Make sum and delete duplicate entries date wise
                            returnData.elements = returnData.elements.GroupBy(e => e.date)
                                      .Select(g => new LinkedinAdsElement
                                      {
                                          clicks = g.Sum(e => e.clicks),
                                          impressions = g.Sum(e => e.impressions),
                                          avg_cpc = g.Average(e => e.avg_cpc),
                                          avg_cpm = g.Average(e => e.avg_cpm),
                                          costInLocalCurrency = g.Sum(e => double.Parse(e.costInLocalCurrency)).ToString(),
                                          date = g.First().date,
                                          dateRange = g.First().dateRange,
                                          pivotValue = g.First().pivotValue
                                      }).ToList();

                            // Create a dictionary to store the data by date
                            var dataByDate = returnData.elements.ToDictionary(e => GetDateFromRange(e.dateRange), e => e);

                            // Iterate through all dates in the range
                            foreach (DateTime date in allDates)
                            {

                                // Check if the date exists in the data
                                if (!dataByDate.ContainsKey(date))
                                {
                                    var daydigit = date.Day.ToString("D2");
                                    var monthdigit = date.Month.ToString("D2");

                                    // Add an empty LinkedinAdsElement object for the missing date
                                    var emptyElement = new LinkedinAdsElement
                                    {

                                        dateRange = new DateRangeForLinkedin
                                        {
                                            start = new Start { month = date.Month, day = date.Day, year = date.Year },
                                            end = new End { month = date.Month, day = date.Day, year = date.Year },
                                        },
                                        clicks = 0,
                                        impressions = 0,
                                        avg_cpc = 0,
                                        avg_cpm = 0,
                                        date = date.Year + "-" + monthdigit + "-" + daydigit,

                                    };
                                    returnData.elements.Add(emptyElement);
                                }
                            }

                            //sorting date wise
                            returnData.elements.Sort((e1, e2) => GetDateFromRange(e1.dateRange).CompareTo(GetDateFromRange(e2.dateRange)));

                            retval.linkedinStat = returnData;
                            retval.adGroupRoot = adgroupsData;

                            return retval;
                        }
                        else if (type == "CREATIVE")
                        {
                            var creativeData = await GetAdsData(campaign);

                            List<string> listOfShareIds = creativeData.elements.Where(x => x.content?.reference != null).Select(x => HttpUtility.UrlEncode(x.content.reference)).Distinct().ToList();
                            List<string> listOfImageId = creativeData.elements.Where(x => x.content.reference == null).Select(x => HttpUtility.UrlEncode(x.content.follow.logo)).Distinct().ToList();


                            ShareResponse shareResponse = null;
                            if (listOfShareIds.Count > 0)
                            {
                                shareResponse = await GetListOfShares(campaign, listOfShareIds);
                            }

                            //List<string> listOfImageId = creativeData.elements.Where(x => x.content.reference == null).Select(x => HttpUtility.UrlEncode(x.content.follow.logo)).ToList();
                            ImageResponse imageResponse = null;
                            if (listOfImageId.Count > 0)
                            {
                                imageResponse = await GetListOfImages(campaign, listOfImageId);
                            }

                            foreach (var item in creativeData.elements)
                            {
                                double totalclick = 0;
                                double total_impression = 0;
                                double cost = 0;
                                string id = item.id.Substring(item.id.LastIndexOf(':') + 1);
                                string share_id = string.Empty;
                                var image = string.Empty;
                                var name = id;

                                //set image after getting from apis
                                if (!string.IsNullOrEmpty(item.content.reference) && shareResponse != null && shareResponse.results != null)
                                {
                                    share_id = item.content.reference.Substring(item.content.reference.LastIndexOf(':') + 1);
                                    if (shareResponse.results.ContainsKey(share_id))
                                    {
                                        image = shareResponse.results[share_id].content.contentEntities[0].thumbnails[0].resolvedUrl;
                                        name = shareResponse.results[share_id].text.text;
                                    }

                                }
                                else if (string.IsNullOrEmpty(item.content.reference) && imageResponse != null && imageResponse.results != null && imageResponse.results.ContainsKey(item.content.follow.logo))
                                {
                                    image = imageResponse.results[item.content.follow.logo].downloadUrl;
                                    name = item.content.follow.headline.preApproved + " " + item.content.follow.headline.custom + " " + item.content.follow.description.preApproved + " " + item.content.follow.description.custom;
                                }

                                var creative_id = item.id;
                                totalclick = returnData.elements.Where(x => x.pivotValue == creative_id).Sum(x => x.clicks);
                                total_impression = returnData.elements.Where(x => x.pivotValue == creative_id).Sum(x => x.impressions);
                                cost = returnData.elements.Where(x => x.pivotValue == creative_id).Sum(x => double.Parse(x.costInLocalCurrency));

                                var avgctr = total_impression > 0 ? (double)((double)(totalclick / total_impression) * 100) : 0;
                                var avgcpm = total_impression > 0 ? (double)((double)(cost / total_impression) * 1000) : 0;
                                var avgcpc = totalclick > 0 ? (double)(cost / totalclick) : 0;

                                item.clicks = totalclick > 0 ? Math.Round(totalclick) : 0;
                                item.impressions = total_impression > 0 ? Math.Round(total_impression, 2) : 0;
                                item.spent = cost > 0 ? Math.Round(cost, 2) : 0;
                                item.avg_ctr = total_impression > 0 ? Math.Round(avgctr, 2) : 0;
                                item.avg_cpm = total_impression > 0 ? Math.Round(avgcpm, 2) : 0;
                                item.avg_cpc = totalclick > 0 ? Math.Round(avgcpc, 2) : 0;
                                item.lead = 0;
                                item.cost_per_lead = 0;
                                item.image = image;

                                item.name = name;
                                item.status = item.intendedStatus;
                                item.currency = currency_symbol;
                            }


                            //adding calucation for avg cpc cpm ctr

                            foreach (var item in returnData.elements)
                            {
                                item.avg_cpm = item.impressions > 0 ? (double)((double)(double.Parse(item.costInLocalCurrency) / item.impressions) * 1000) : 0;
                                item.avg_cpc = item.clicks > 0 ? (double)(double.Parse(item.costInLocalCurrency) / item.clicks) : 0;
                                item.date = item.dateRange.start.year + "-" + item.dateRange.start.month.ToString("D2") + "-" + item.dateRange.start.day.ToString("D2");
                            }

                            //adding zero data                      
                            // Create a list of all dates within the specified range
                            DateTime startDate = DateTime.ParseExact(startTime, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            DateTime endDate = DateTime.ParseExact(endTime, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                            List<DateTime> allDates = Enumerable.Range(0, 1 + endDate.Subtract(startDate).Days)
                                .Select(offset => startDate.AddDays(offset))
                                .ToList();

                            //Make sum and delete duplicate entries date wise
                            returnData.elements = returnData.elements.GroupBy(e => e.date)
                                      .Select(g => new LinkedinAdsElement
                                      {
                                          clicks = g.Sum(e => e.clicks),
                                          impressions = g.Sum(e => e.impressions),
                                          avg_cpc = g.Average(e => e.avg_cpc),
                                          avg_cpm = g.Average(e => e.avg_cpm),
                                          costInLocalCurrency = g.Sum(e => double.Parse(e.costInLocalCurrency)).ToString(),
                                          date = g.First().date,
                                          dateRange = g.First().dateRange,
                                          pivotValue = g.First().pivotValue
                                      }).ToList();


                            // Create a dictionary to store the data by date
                            var dataByDate = returnData.elements.ToDictionary(e => GetDateFromRange(e.dateRange), e => e);

                            // Iterate through all dates in the range
                            foreach (DateTime date in allDates)
                            {
                                // Check if the date exists in the data
                                if (!dataByDate.ContainsKey(date))
                                {
                                    var daydigit = date.Day.ToString("D2");
                                    var monthdigit = date.Month.ToString("D2");

                                    // Add an empty LinkedinAdsElement object for the missing date
                                    var emptyElement = new LinkedinAdsElement
                                    {

                                        dateRange = new DateRangeForLinkedin
                                        {
                                            start = new Start { month = date.Month, day = date.Day, year = date.Year },
                                            end = new End { month = date.Month, day = date.Day, year = date.Year },
                                        },
                                        clicks = 0,
                                        impressions = 0,
                                        avg_cpc = 0,
                                        avg_cpm = 0,
                                        date = date.Year + "-" + monthdigit + "-" + daydigit,

                                    };
                                    returnData.elements.Add(emptyElement);
                                }
                            }

                            //sorting date wise
                            returnData.elements.Sort((e1, e2) => GetDateFromRange(e1.dateRange).CompareTo(GetDateFromRange(e2.dateRange)));

                            retval.linkedinStat = returnData;
                            retval.creativeRoot = creativeData;


                        }

                        return retval;
                    }

                }
            }

            return retval;
        }

        public async Task<DempgraphicRoot> GetLinkedinAdsDemographic(string campaignId, string type, string startTime, string endTime)
        {
            var retval = new DempgraphicRoot();
            //startTime = "2020-08-01";
            //endTime = "2023-05-25";
            //type = "MEMBER_JOB_FUNCTION";
            // Split the string by '-' delimiter
            string[] parts = startTime.Split('-');

            // Extract the date, month, and year
            int year = int.Parse(parts[0]);
            int month = int.Parse(parts[1]);
            int day = int.Parse(parts[2]);

            // Split the string by '-' delimiter
            string[] parts1 = endTime.Split('-');

            // Extract the date, month, and year
            int yearEnd = int.Parse(parts1[0]);
            int monthEnd = int.Parse(parts1[1]);
            int dayEnd = int.Parse(parts1[2]);

            LinkedinAdsOrganization linkedinOrganization = null;

            var campaign = _linkedinadRepository.GetAllEntities().Where(x => x.CampaignID == new Guid(campaignId)).FirstOrDefault();
            if (campaign != null)
            {
                var options = new RestClientOptions("https://api.linkedin.com/rest/")
                {
                    Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(campaign.AccessToken, "Bearer")
                };

                var clientForData = new RestClient(options);

                var requestForData = new RestRequest("adAnalytics", Method.Get);

                requestForData.AddQueryParameter("q", "analytics", false);
                requestForData.AddQueryParameter("dateRange", "(start:(year:" + year + ",month:" + month + ",day:" + day + "),end:(year:" + yearEnd + ",month:" + monthEnd + ",day:" + dayEnd + "))", false);
                requestForData.AddQueryParameter("timeGranularity", "(value:ALL)", false);
                requestForData.AddQueryParameter("accounts", "List(urn%3Ali%3AsponsoredAccount%3A" + campaign.OrganizationalEntity + ")", false);
                requestForData.AddQueryParameter("pivot", "(value:" + type + ")", false);
                requestForData.AddQueryParameter("fields", "clicks,dateRange,impressions,pivotValue", false);
                requestForData.AddQueryParameter("sortBy", "(field:IMPRESSIONS,order:DESCENDING)", false);

                requestForData.AddHeader("Linkedin-Version", "202304");
                requestForData.AddHeader("X-Restli-Protocol-Version", "2.0.0");

                var response = await clientForData.GetAsync(requestForData);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var returnData = JsonConvert.DeserializeObject<DempgraphicRoot>(response.Content);


                    var demographicCode = new AdsDemographicCode();
                    var restClient1 = new RestClient(_configuration["BlobUrl"] + "Json");
                    var restRequest1 = new RestRequest("/demographic_code.json", Method.Get);

                    var response1 = restClient1.GetAsync(restRequest1).Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        demographicCode = JsonConvert.DeserializeObject<AdsDemographicCode>(response1.Content);
                    }

                    if (type == "MEMBER_COMPANY")
                    {
                        List<string> org_urn_ids = returnData.elements.Select(urn => urn.pivotValue.Substring(urn.pivotValue.LastIndexOf(':') + 1)).Take(25).ToList();

                        if (org_urn_ids.Count > 0)
                        {
                            linkedinOrganization = await GetOrganiztionData(campaign, org_urn_ids);
                        }
                    }

                    if (returnData != null && returnData.elements != null)
                    {
                        double totalImpressions = returnData.elements.Sum(e => e.impressions);

                        double totalClicks = returnData.elements.Sum(e => e.clicks);


                        foreach (var item in returnData.elements)
                        {
                            // Find the last occurrence of ':' character
                            int lastColonIndex = item.pivotValue.LastIndexOf(':');

                            // Extract the substring after the last ':'
                            string id = item.pivotValue.Substring(lastColonIndex + 1);

                            // Parse the ID as an integer
                            int intId = int.Parse(id);

                            var avg_ctr = item.impressions > 0 ? (double)((double)(item.clicks / item.impressions) * 100) : 0;

                            item.avg_ctr = item.impressions > 0 ? Math.Round(avg_ctr, 2) : 0;

                            item.name = item.pivotValue;

                            if (type == "MEMBER_JOB_FUNCTION")
                            {
                                var name = demographicCode.functions.Where(y => y.id == intId).Select(x => x.name.localized.en_US).FirstOrDefault();
                                item.name = !String.IsNullOrEmpty(name) ? name : item.pivotValue;
                            }
                            else if (type == "MEMBER_JOB_TITLE")
                            {
                                var name = demographicCode.titles.Where(y => y.id == intId).Select(x => x.name.localized.en_US).FirstOrDefault();
                                item.name = !String.IsNullOrEmpty(name) ? name : item.pivotValue;
                            }
                            else if (type == "MEMBER_COMPANY" && linkedinOrganization != null)
                            {
                                string name = linkedinOrganization.results.ContainsKey(id) ? linkedinOrganization.results[id].localizedName : item.pivotValue;

                                item.name = !String.IsNullOrEmpty(item.pivotValue) ? name : item.pivotValue;
                            }
                            else if (type == "MEMBER_INDUSTRY")
                            {
                                var name = demographicCode.industries.Where(y => y.id == intId).Select(x => x.name.localized.en_US).FirstOrDefault();
                                item.name = !String.IsNullOrEmpty(name) ? name : item.pivotValue;
                            }
                        }
                    }
                    if (returnData != null && returnData.elements.Count > 24)
                    {
                        // Read the JSON file
                        returnData.elements = returnData.elements.Take(25).ToList();
                    }

                    return returnData;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && campaign != null)
                {
                    var res = _campaignLinkedinService.GetAccessTokenUsingRefreshToken(campaign.RefreshToken).Result;

                    if (res != null && !string.IsNullOrEmpty(res.access_token))
                    {
                        campaign.AccessToken = res.access_token;
                        campaign.AccessTokenExpiresIn = res.expires_in;
                        campaign.RefreshToken = res.refresh_token;
                        campaign.RefreshTokenExpiresIn = res.refresh_token_expires_in;
                        campaign.UpdatedOn = DateTime.UtcNow;

                        _linkedinadRepository.UpdateEntity(campaign);
                        _linkedinadRepository.SaveChanges();
                    }                    

                    options = new RestClientOptions("https://api.linkedin.com/rest/")
                    {
                        Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(campaign.AccessToken, "Bearer")
                    };

                    clientForData = new RestClient(options);

                    response = clientForData.GetAsync(requestForData).Result;

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var returnData = JsonConvert.DeserializeObject<DempgraphicRoot>(response.Content);

                        return returnData;
                    }
                }
            }
            return retval;
        }

        private async Task<LinkedinAdsOrganization> GetOrganiztionData(LinkedinAd campaign, List<string> org_ids)
        {
            List<string> formattedList = org_ids.Select((id, index) => $"ids[{index}]={id}").ToList();

            string formattedString = string.Join("&", formattedList);

            var options = new RestClientOptions("https://api.linkedin.com/v2/")
            {
                Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(campaign.AccessToken, "Bearer")
            };

            var client = new RestClient(options);

            var request = new RestRequest("organizationsLookup?" + formattedString, Method.Get);

            request.AddQueryParameter("projection", "(results*(localizedName,id))", false);
            request.AddHeader("Linkedin-Version", "202306");

            try
            {
                var response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = JsonConvert.DeserializeObject<LinkedinAdsOrganization>(response.Content);
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "Request failed with status code Unauthorized" && campaign != null)
                {
                    var res = await _campaignLinkedinService.GetAccessTokenUsingRefreshToken(campaign.RefreshToken);
                    if (res != null)
                    {
                        campaign.AccessToken = res.access_token;
                        campaign.AccessTokenExpiresIn = res.expires_in;
                        campaign.RefreshToken = res.refresh_token;
                        campaign.RefreshTokenExpiresIn = res.refresh_token_expires_in;
                        campaign.UpdatedOn = DateTime.UtcNow;

                        _linkedinadRepository.UpdateEntity(campaign);
                        _linkedinadRepository.SaveChanges();
                    }                   

                    options = new RestClientOptions("https://api.linkedin.com/rest/")
                    {
                        Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(campaign.AccessToken, "Bearer")
                    };

                    client = new RestClient(options);

                    var response = client.GetAsync(request).Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<LinkedinAdsOrganization>(response.Content);
                    }
                }
                else
                {
                    return null;
                }

            }

            return null;

        }

        public List<LinkedinAdDto> GetCampaignLinkedinByCampaignId(string campaignId)
        {
            var linkedinSetup = _linkedinadRepository.GetAllEntities(true).Where(x => x.CampaignID == new Guid(campaignId))
               .Select(linkedin => new LinkedinAdDto
               {
                   Id = linkedin.Id,
                   AccessToken = linkedin.AccessToken,
                   RefreshToken = linkedin.RefreshToken,
                   RefreshTokenExpiresIn = linkedin.RefreshTokenExpiresIn,
                   CampaignID = linkedin.CampaignID,
                   OrganizationalEntity = linkedin.OrganizationalEntity
               }).ToList();

            return linkedinSetup;
        }

        // Helper method to get the DateTime object from the DateRange object
        DateTime GetDateFromRange(DateRangeForLinkedin range)
        {
            return new DateTime(range.start.year, range.start.month, range.start.day);
        }

        public async Task<CreativeRoot> GetAdsData(LinkedinAd linkedinad)
        {
            var options = new RestClientOptions("https://api.linkedin.com/rest/")
            {
                Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(linkedinad.AccessToken, "Bearer")
            };

            var client = new RestClient(options);

            var request = new RestRequest("adAccounts/" + linkedinad.OrganizationalEntity + "/creatives", Method.Get);

            request.AddQueryParameter("q", "criteria", false);
            request.AddQueryParameter("count", 1000, false);
            request.AddQueryParameter("intendedStatuses", "(value:List(ACTIVE,PAUSED,ARCHIVED,CANCELED,DRAFT,PENDING_DELETION,REMOVED))", false);
            request.AddQueryParameter("fields", "content,id,intendedStatus,account,campaign", false);
            request.AddHeader("Linkedin-Version", "202306");
            request.AddHeader("X-Restli-Protocol-Version", "2.0.0");

            try
            {
                var response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = JsonConvert.DeserializeObject<CreativeRoot>(response.Content);
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "Request failed with status code Unauthorized" && linkedinad != null)
                {
                    var res = await _campaignLinkedinService.GetAccessTokenUsingRefreshToken(linkedinad.RefreshToken);
                    if (res != null)
                    {
                        linkedinad.AccessToken = res.access_token;
                        linkedinad.AccessTokenExpiresIn = res.expires_in;
                        linkedinad.RefreshToken = res.refresh_token;
                        linkedinad.RefreshTokenExpiresIn = res.refresh_token_expires_in;
                        linkedinad.UpdatedOn = DateTime.UtcNow;

                        _linkedinadRepository.UpdateEntity(linkedinad);
                        _linkedinadRepository.SaveChanges();
                    }                    

                    options = new RestClientOptions("https://api.linkedin.com/rest/")
                    {
                        Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(linkedinad.AccessToken, "Bearer")
                    };

                    client = new RestClient(options);

                    var response = client.GetAsync(request).Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<CreativeRoot>(response.Content);
                    }
                }
                else
                {
                    return null;
                }

            }

            return null;

        }

        public async Task<ShareResponse> GetListOfShares(LinkedinAd linkedinad, List<string> shareIds)
        {

            var shareids = string.Join(",", shareIds);
            //var encode_url = 
            var options = new RestClientOptions("https://api.linkedin.com/v2/")
            {
                Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(linkedinad.AccessToken, "Bearer")
            };

            var client = new RestClient(options);

            var request = new RestRequest("shares", Method.Get);

            request.AddQueryParameter("ids", "List(" + shareids + ")", false);
            request.AddQueryParameter("fields", "text,id,content", false);
            //request.AddHeader("Linkedin-Version", "202306");
            request.AddHeader("X-Restli-Protocol-Version", "2.0.0");

            try
            {
                var response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = JsonConvert.DeserializeObject<ShareResponse>(response.Content);
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "Request failed with status code Unauthorized" && linkedinad != null)
                {
                    var res = await _campaignLinkedinService.GetAccessTokenUsingRefreshToken(linkedinad.RefreshToken);
                    if (res != null)
                    {

                        linkedinad.AccessToken = res.access_token;
                        linkedinad.AccessTokenExpiresIn = res.expires_in;
                        linkedinad.RefreshToken = res.refresh_token;
                        linkedinad.RefreshTokenExpiresIn = res.refresh_token_expires_in;
                        linkedinad.UpdatedOn = DateTime.UtcNow;

                        _linkedinadRepository.UpdateEntity(linkedinad);
                        _linkedinadRepository.SaveChanges();
                    }

                    options = new RestClientOptions("https://api.linkedin.com/rest/")
                    {
                        Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(linkedinad.AccessToken, "Bearer")
                    };

                    client = new RestClient(options);

                    var response = client.GetAsync(request).Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<ShareResponse>(response.Content);
                    }
                }
                else
                {
                    return null;
                }

            }

            return null;

        }

        public async Task<ImageResponse> GetListOfImages(LinkedinAd linkedinad, List<string> imageIds)
        {
            var shareids = string.Join(",", imageIds);

            var options = new RestClientOptions("https://api.linkedin.com/rest/")
            {
                Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(linkedinad.AccessToken, "Bearer")
            };

            var client = new RestClient(options);

            var request = new RestRequest("images", Method.Get);

            request.AddQueryParameter("ids", "List(" + shareids + ")", false);

            request.AddQueryParameter("fields", "downloadUrl,id,status", false);
            request.AddHeader("Linkedin-Version", "202306");
            request.AddHeader("X-Restli-Protocol-Version", "2.0.0");

            try
            {
                var response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = JsonConvert.DeserializeObject<ImageResponse>(response.Content);
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "Request failed with status code Unauthorized" && linkedinad != null)
                {
                    var res = await _campaignLinkedinService.GetAccessTokenUsingRefreshToken(linkedinad.RefreshToken);
                    if (res != null)
                    {
                        linkedinad.AccessToken = res.access_token;
                        linkedinad.AccessTokenExpiresIn = res.expires_in;
                        linkedinad.RefreshToken = res.refresh_token;
                        linkedinad.RefreshTokenExpiresIn = res.refresh_token_expires_in;
                        linkedinad.UpdatedOn = DateTime.UtcNow;

                        _linkedinadRepository.UpdateEntity(linkedinad);
                        _linkedinadRepository.SaveChanges();
                    }
                   
                    options = new RestClientOptions("https://api.linkedin.com/rest/")
                    {
                        Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(linkedinad.AccessToken, "Bearer")
                    };

                    client = new RestClient(options);

                    var response = client.GetAsync(request).Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<ImageResponse>(response.Content);
                    }
                }
                else
                {
                    return null;
                }

            }

            return null;

        }



        public async Task<AdGroupsRoot> GetAllAdGroupsData(LinkedinAd linkedinad)
        {
            var options = new RestClientOptions("https://api.linkedin.com/rest/")
            {
                Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(linkedinad.AccessToken, "Bearer")
            };

            var client = new RestClient(options);

            var request = new RestRequest("adAccounts/" + linkedinad.OrganizationalEntity + "/adCampaignGroups", Method.Get);

            request.AddQueryParameter("q", "search", false);
            request.AddQueryParameter("search", "(status:(values:List(ACTIVE,PAUSED,ARCHIVED,CANCELED,DRAFT,PENDING_DELETION,REMOVED)))", false);
            request.AddQueryParameter("fields", "status,id,account,name", false);
            request.AddHeader("Linkedin-Version", "202305");
            request.AddHeader("X-Restli-Protocol-Version", "2.0.0");

            try
            {
                var response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = JsonConvert.DeserializeObject<AdGroupsRoot>(response.Content);
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "Request failed with status code Unauthorized" && linkedinad != null)
                {
                    var res = await _campaignLinkedinService.GetAccessTokenUsingRefreshToken(linkedinad.RefreshToken);
                    if (res != null)
                    {

                        linkedinad.AccessToken = res.access_token;
                        linkedinad.AccessTokenExpiresIn = res.expires_in;
                        linkedinad.RefreshToken = res.refresh_token;
                        linkedinad.RefreshTokenExpiresIn = res.refresh_token_expires_in;
                        linkedinad.UpdatedOn = DateTime.UtcNow;

                        _linkedinadRepository.UpdateEntity(linkedinad);
                        _linkedinadRepository.SaveChanges();
                    }

                    options = new RestClientOptions("https://api.linkedin.com/rest/")
                    {
                        Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(linkedinad.AccessToken, "Bearer")
                    };

                    client = new RestClient(options);

                    var response = client.GetAsync(request).Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<AdGroupsRoot>(response.Content);
                    }
                }
                else
                {
                    return null;
                }

            }

            return null;
        }

        public async Task<CampaignRoot> GetAllCampaigns(LinkedinAd linkedinad)
        {
            var options = new RestClientOptions("https://api.linkedin.com/rest/")
            {
                Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(linkedinad.AccessToken, "Bearer")
            };

            var client = new RestClient(options);

            var request = new RestRequest("adAccounts/" + linkedinad.OrganizationalEntity + "/adCampaigns", Method.Get);

            request.AddQueryParameter("q", "search", false);
            request.AddQueryParameter("search", "(status:(values:List(ACTIVE,PAUSED,ARCHIVED,COMPLETED,CANCELED,DRAFT,PENDING_DELETION,REMOVED)))", false);
            request.AddQueryParameter("fields", "name,account,campaignGroup,unitCost,id,status", false);
            request.AddHeader("Linkedin-Version", "202305");
            request.AddHeader("X-Restli-Protocol-Version", "2.0.0");

            try
            {
                var response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = JsonConvert.DeserializeObject<CampaignRoot>(response.Content);
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "Request failed with status code Unauthorized" && linkedinad != null)
                {
                    var res = await _campaignLinkedinService.GetAccessTokenUsingRefreshToken(linkedinad.RefreshToken);
                    if (res != null)
                    {
                        linkedinad.AccessToken = res.access_token;
                        linkedinad.AccessTokenExpiresIn = res.expires_in;
                        linkedinad.RefreshToken = res.refresh_token;
                        linkedinad.RefreshTokenExpiresIn = res.refresh_token_expires_in;
                        linkedinad.UpdatedOn = DateTime.UtcNow;

                        _linkedinadRepository.UpdateEntity(linkedinad);
                        _linkedinadRepository.SaveChanges();
                    }

                    options = new RestClientOptions("https://api.linkedin.com/rest/")
                    {
                        Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(linkedinad.AccessToken, "Bearer")
                    };

                    client = new RestClient(options);

                    var response = client.GetAsync(request).Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<CampaignRoot>(response.Content);
                    }
                }
                else
                {
                    return null;
                }

            }

            return null;
        }

        public async Task<CreativeRoot> GetAdNameImage(LinkedinAd linkedinad)
        {
            var options = new RestClientOptions("https://api.linkedin.com/v2/")
            {
                Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(linkedinad.AccessToken, "Bearer")
            };

            var client = new RestClient(options);

            var request = new RestRequest("adCreativesV2/" + linkedinad.OrganizationalEntity, Method.Get);

            request.AddQueryParameter("projection", "(variables(data(*,com.linkedin.ads.SponsoredUpdateCreativeVariables(*,share~(subject,text(text),content(contentEntities(*(description,entityLocation,title))))))))", false);
            request.AddQueryParameter("intendedStatuses", "(value:List(ACTIVE,PAUSED,ARCHIVED,CANCELED,DRAFT,PENDING_DELETION,REMOVED))", false);
            request.AddQueryParameter("fields", "content,id,intendedStatus,account,campaign", false);
            //request.AddHeader("Linkedin-Version", "202306");
            //request.AddHeader("X-Restli-Protocol-Version", "2.0.0");

            try
            {
                var response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = JsonConvert.DeserializeObject<CreativeRoot>(response.Content);
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "Request failed with status code Unauthorized" && linkedinad != null)
                {
                    var res = await _campaignLinkedinService.GetAccessTokenUsingRefreshToken(linkedinad.RefreshToken);
                    if (res != null)
                    {
                        linkedinad.AccessToken = res.access_token;
                        linkedinad.AccessTokenExpiresIn = res.expires_in;
                        linkedinad.RefreshToken = res.refresh_token;
                        linkedinad.RefreshTokenExpiresIn = res.refresh_token_expires_in;
                        linkedinad.UpdatedOn = DateTime.UtcNow;

                        _linkedinadRepository.UpdateEntity(linkedinad);
                        _linkedinadRepository.SaveChanges();
                    }
                    

                    options = new RestClientOptions("https://api.linkedin.com/rest/")
                    {
                        Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(linkedinad.AccessToken, "Bearer")
                    };

                    client = new RestClient(options);

                    var response = client.GetAsync(request).Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<CreativeRoot>(response.Content);
                    }
                }
                else
                {
                    return null;
                }

            }

            return null;

        }


        #endregion


        #region OVERRIDDEN IMPLEMENTATION

        public override Dictionary<string, PropertyMappingValue> GetPropertyMapping()
        {
            return new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
                    {
                       { "Id", new PropertyMappingValue(new List<string>() { "Id" } ) },
                        { "UpdatedOn", new PropertyMappingValue(new List<string>() { "UpdatedOn" } )}
                    };
        }

        public override string GetDefaultOrderByColumn()
        {
            return "UpdatedOn";
        }

        public override string GetDefaultFieldsToSelect()
        {
            return "Id,PageName,OrganizationalEntity,CampaignID,Currency";
        }

        #endregion
    }
}
