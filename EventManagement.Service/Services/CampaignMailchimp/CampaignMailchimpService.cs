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
using RestSharp;
using AutoMapper;
using MailChimp.Net.Models;
using Report = EventManagement.Dto;
using Method = RestSharp.Method;
using static Google.Api.ResourceDescriptor.Types;
using List = EventManagement.Dto.List;
using Google.Apis.AnalyticsReporting.v4.Data;
using Google.Api.Ads.AdWords.v201809;
using Google.Api.Ads.AdWords.Util.Reports;

namespace EventManagement.Service
{
    public class CampaignMailchimpService : ServiceBase<CampaignMailchimp, Guid>, ICampaignMailchimpService
    {

        #region PRIVATE MEMBERS

        private readonly ICampaignMailchimpRepository _campaignmailchimpRepository;
        private readonly IConfiguration _configuration;
        #endregion


        #region CONSTRUCTOR

        public CampaignMailchimpService(ICampaignMailchimpRepository campaignmailchimpRepository, ILogger<CampaignMailchimpService> logger, IConfiguration configuration) : base(campaignmailchimpRepository, logger)
        {
            _campaignmailchimpRepository = campaignmailchimpRepository;
            _configuration = configuration;
        }

        #endregion


        #region PUBLIC MEMBERS   

        public string MailchimpAuth()
        {
            //var redirectUrl = "http://127.0.0.1:3000/html/auth-redirect.html";

            var frontendUrl = _configuration.GetSection("FrontendUrl").Value;

            var redirectUrl = frontendUrl + _configuration["MailchimpRedirectUrl"];

            string _clientId = _configuration["MailchimpClientId"];

            var url = "https://login.mailchimp.com/oauth2/authorize?response_type=code&client_id=" + _clientId + "&redirect_uri=" + redirectUrl;

            return url;
        }

        public async Task<string> GetAccessTokenUsingCode(string code)
        {
            string _clientId = _configuration["MailchimpClientId"];

            string secret = _configuration["MailchimpSecretId"];

            var frontendUrl = _configuration.GetSection("FrontendUrl").Value;

            var redirectUrl = frontendUrl + _configuration["MailchimpRedirectUrl"];

            //var redirectUrl = "http://127.0.0.1:3000/html/auth-redirect.html";

            // Define the base URL
            string baseUrl = "https://login.mailchimp.com/oauth2";

            // Create a RestClient
            var restClient = new RestClient(baseUrl);

            // Create a RestRequest with POST method
            var restRequest = new RestRequest("/token", Method.Post);

            // Add form-data parameters to the request
            restRequest.AddParameter("code", code, ParameterType.GetOrPost);
            restRequest.AddParameter("client_id", _clientId, ParameterType.GetOrPost);
            restRequest.AddParameter("redirect_uri", redirectUrl, ParameterType.GetOrPost);
            restRequest.AddParameter("client_secret", secret, ParameterType.GetOrPost);
            restRequest.AddParameter("grant_type", "authorization_code", ParameterType.GetOrPost);

            // Execute the request
            var response = await restClient.ExecuteAsync(restRequest);

            var res = JsonConvert.DeserializeObject<MailchimpAuth>(response.Content);

            return res.access_token;

        }

        public async Task<MailchimpMetadataDto> GetMailchimpAccount(string access_token)
        {

            // Define the base URL
            string baseUrl = "https://login.mailchimp.com/oauth2";

            // Create a RestClient
            var restClient = new RestClient(baseUrl);

            // Create a RestRequest
            var restRequest = new RestRequest("/metadata", Method.Get);

            restRequest.AddHeader("Authorization", $"Bearer {access_token}");

            // Execute the request
            var response = await restClient.ExecuteAsync(restRequest);

            var res = JsonConvert.DeserializeObject<MailchimpMetadataDto>(response.Content);

            return res;

        }

        public async Task<MCRootCampaignList> GetCampaignListReport(Guid campaignId)
        {
            var retVal = new MCRootCampaignList();

            var res = await GetCampaignListReportApi(campaignId);
            var currencySymbol = string.Empty;

            if (!string.IsNullOrEmpty(res.reports.Select(x => x.ecommerce.currency_code).FirstOrDefault()))
            {
                currencySymbol = GetCurrencySymbol(res.reports.Select(x => x.ecommerce.currency_code).FirstOrDefault());
            }

            if (res.StatusCode == System.Net.HttpStatusCode.OK && res != null && res.reports.Count > 0)
            {


                var recipientsChartData11 = res.reports
                    .SelectMany(report => report.timeseries
                        .Select(entry => (Timestamp: entry.timestamp, EmailsSent: entry.emails_sent))
                    )
                    .OrderBy(entry => DateTimeOffset.Parse(entry.Timestamp))
                    .GroupBy(entry => DateTimeOffset.Parse(entry.Timestamp).ToString("dddd d MMMM yyyy"))
                    .Select(group => new MCChartData
                    {
                        Dates = group.Key,
                        Values = group.Sum(entry => entry.EmailsSent)
                    })
                    .ToList();

                var recipentsDates = recipientsChartData11.Select(x => x.Dates).ToList();

                var recipentsValue = recipientsChartData11.Select(x => x.Values).ToList();

                var totalRecipients = recipientsChartData11.Sum(x => x.Values);

                retVal.recipientsChartDates = recipentsDates;

                retVal.recipientsChartValues = recipentsValue;

                retVal.recipientsChartTotal = totalRecipients;

                //Prepare Open 24 hours chart
                var uniqueOpenChartData11 = res.reports
                    .SelectMany(report => report.timeseries
                        .Select(entry => (Timestamp: entry.timestamp, Unique: entry.unique_opens))
                    )
                    .OrderBy(entry => DateTimeOffset.Parse(entry.Timestamp))
                    .GroupBy(entry => DateTimeOffset.Parse(entry.Timestamp).ToString("dddd d MMMM yyyy"))
                    .Select(group => new MCChartData
                    {
                        Dates = group.Key,
                        Values = group.Sum(entry => entry.Unique)
                    })
                    .ToList();

                var uniqueOpenDates = uniqueOpenChartData11.Select(x => x.Dates).ToList();

                var uniqueOpenValue = uniqueOpenChartData11.Select(x => x.Values).ToList();

                var totalUniqueOpenChart = uniqueOpenChartData11.Sum(x => x.Values);

                retVal.openChartDates = uniqueOpenDates;

                retVal.openChartValues = uniqueOpenValue;

                retVal.uniqueOpenChartTotal = totalUniqueOpenChart;

                //Prepare Clicks 24 hours chart
                var clickData = res.reports
                    .SelectMany(report => report.timeseries
                        .Select(entry => (Timestamp: entry.timestamp, Click: entry.recipients_clicks))
                    )
                    .OrderBy(entry => DateTimeOffset.Parse(entry.Timestamp))
                    .GroupBy(entry => DateTimeOffset.Parse(entry.Timestamp).ToString("dddd d MMMM yyyy"))
                    .Select(group => new MCChartData
                    {
                        Dates = group.Key,
                        Values = group.Sum(entry => entry.Click)
                    })
                    .ToList();

                var clickDates = clickData.Select(x => x.Dates).ToList();

                var clickValue = clickData.Select(x => x.Values).ToList();

                var totalClickChart = clickData.Sum(x => x.Values);


                retVal.clickChartDates = clickDates;
                
                retVal.clickChartValues = clickValue;

                retVal.clickChartTotal = totalClickChart;

                ///Table data

                var list = new List<CampaignListTable>();

                foreach (var report in res.reports)
                {
                    var singleCampaign = new CampaignListTable();

                    singleCampaign.Name = string.IsNullOrEmpty(report.campaign_title) ? report.subject_line : report.campaign_title;
                    singleCampaign.Recipients = report.emails_sent;

                    singleCampaign.UniqueOpens = report.opens.unique_opens;

                    singleCampaign.UnopenedEmails = singleCampaign.Recipients - singleCampaign.UniqueOpens;

                    singleCampaign.BouncedEmails = report.bounces.soft_bounces + report.bounces.hard_bounces;

                    if (report.opens.open_rate > 0)
                    {
                        singleCampaign.OpenRate = Math.Round((decimal)report.opens.open_rate * 100, 2);
                    }

                    if (report.clicks.click_rate > 0)
                    {
                        singleCampaign.ClickRate = Math.Round((decimal)report.clicks.click_rate * 100, 2);
                    }

                    singleCampaign.Unsubscribes = report.unsubscribed;

                    if (singleCampaign.Recipients > 0)
                    {
                        singleCampaign.UnsubscribeRate = Math.Round((singleCampaign.Unsubscribes / singleCampaign.Recipients) * 100, 2);
                    }

                    if (report.emails_sent > 0)
                    {
                        singleCampaign.BounceRate = Math.Round((singleCampaign.BouncedEmails / report.emails_sent) * 100, 2);
                        singleCampaign.DeliveryRate = Math.Round(((singleCampaign.Recipients - singleCampaign.BouncedEmails) / singleCampaign.Recipients) * 100, 2);
                        singleCampaign.SpamRate = Math.Round((decimal)(report.abuse_reports / report.emails_sent) * 100, 2);
                    }

                    singleCampaign.Click = (decimal)report.clicks.clicks_total;
                    singleCampaign.SubsciberClick = report.clicks.unique_subscriber_clicks;
                    singleCampaign.Opens = report.opens.opens_total;
                    singleCampaign.Orders = report.ecommerce.total_orders;

                    if (report.ecommerce.total_orders > 0)
                    {
                        singleCampaign.AverageOrder = Math.Round((decimal)(report.ecommerce.total_spent / report.ecommerce.total_orders), 2);
                    }

                    singleCampaign.Revenue = report.ecommerce.total_revenue;
                    singleCampaign.TotalSpent = report.ecommerce.total_spent;
                    singleCampaign.Deliveries = singleCampaign.Recipients - singleCampaign.BouncedEmails;
                    singleCampaign.Spams = report.abuse_reports;
                    singleCampaign.McCampaignId = report.id;
                    singleCampaign.SendTime = DateTime.Parse(report.send_time).ToString("MMM dd, yyyy hh:mm tt");
                    singleCampaign.CurrencyCode = currencySymbol;

                    list.Add(singleCampaign);
                }

                retVal.campaignListTable = list;

                /// Small Tiles
                retVal.recipients = res.reports.Sum(x => x.emails_sent);

                retVal.uniqueOpens = res.reports.Sum(x => x.opens.unique_opens);

                retVal.unopenedEmails = retVal.recipients - retVal.uniqueOpens;

                retVal.bouncedEmails = res.reports.Sum(x => x.bounces.hard_bounces + x.bounces.soft_bounces);

                retVal.subsciberClick = res.reports.Sum(x => (decimal)x.clicks.unique_subscriber_clicks);

                retVal.unsubscribes = res.reports.Sum(x => (decimal)x.unsubscribed);

                retVal.clicks = res.reports.Sum(x => (decimal)x.clicks.clicks_total);

                retVal.opens = res.reports.Sum(x => (decimal)x.opens.opens_total);

                retVal.orders = res.reports.Sum(x => (decimal)x.ecommerce.total_orders);

                retVal.revenue = res.reports.Sum(x => (decimal)x.ecommerce.total_revenue);

                retVal.totalSpent = res.reports.Sum(x => (decimal)x.ecommerce.total_spent);

                retVal.deliveries = retVal.recipients - retVal.bouncedEmails;

                retVal.spams = res.reports.Sum(x => (decimal)x.abuse_reports);

                retVal.averageOrder = retVal.orders > 0 ? retVal.totalSpent / retVal.orders : 0;

                var openCampaignCount = list.Where(x => x.OpenRate > 0).Select(x => x.OpenRate).Count();

                var clickCampaignCount = list.Where(x => x.ClickRate > 0).Select(x => x.ClickRate).Count();

                var unsubscribeRateCount = list.Where(x => x.UnsubscribeRate > 0).Select(x => x.UnsubscribeRate).Count();

                var bounceRateCount = list.Where(x => x.BounceRate > 0).Select(x => x.BounceRate).Count();

                var deliveryRateCount = list.Where(x => x.DeliveryRate > 0).Select(x => x.DeliveryRate).Count();

                var spamRateCount = list.Where(x => x.SpamRate > 0).Select(x => x.SpamRate).Count();


                retVal.openRate = openCampaignCount > 0 ? Math.Round(list.Sum(x => x.OpenRate) / openCampaignCount, 2) : 0;

                retVal.clickRate = clickCampaignCount > 0 ? Math.Round(list.Sum(x => x.ClickRate) / clickCampaignCount, 2) : 0;

                retVal.unsubscribeRate = unsubscribeRateCount > 0 ? Math.Round(list.Sum(x => x.UnsubscribeRate) / unsubscribeRateCount, 2) : 0;

                retVal.bounceRate = bounceRateCount > 0 ? Math.Round(list.Sum(x => x.BounceRate) / bounceRateCount, 2) : 0;

                retVal.deliveryRate = deliveryRateCount > 0 ? Math.Round(list.Sum(x => x.DeliveryRate) / deliveryRateCount, 2) : 0;

                retVal.spamRate = spamRateCount > 0 ? Math.Round(list.Sum(x => x.SpamRate) / spamRateCount, 2) : 0;

                retVal.currencyCode = currencySymbol != null ? currencySymbol :"";


            }

            return retVal;
        }

        private async Task<MailchimpReports> GetCampaignListReportApi(Guid campaignId)
        {
            var res = new MailchimpReports();
            try
            {
                var campaign = _campaignmailchimpRepository.GetAllEntities().Where(x => x.CampaignID == campaignId).FirstOrDefault();

                // Mailchimp API endpoint
                string apiEndpoint = campaign.ApiEndpoint + "/3.0";

                // Resource path
                string resourcePath = "/reports";

                // Create the RestSharp client
                var client = new RestClient(apiEndpoint);

                // Create the RestSharp request
                var request = new RestRequest(resourcePath, Method.Get);

                request.AddHeader("Content-Type", "application/json");

                // Add authorization header
                request.AddHeader("Authorization", $"Bearer {campaign.AccessToken}");

                request.AddParameter("count", 1000, ParameterType.QueryString);

                request.AddParameter("fields", "reports.id,reports.campaign_title,reports.emails_sent,reports.abuse_reports,reports.unsubscribed,reports.bounces,reports.opens,reports.clicks,reports.ecommerce,reports.timeseries,reports.send_time,reports.subject_line", ParameterType.QueryString);

                // Add query parameters
                //request.AddParameter("application/json", queryParams, ParameterType.QueryString);

                // Execute the request and get the response
                var response = await client.ExecuteAsync(request);
                res = JsonConvert.DeserializeObject<MailchimpReports>(response.Content);

                // Check if the request was successful (status code 200)
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    res.StatusCode = System.Net.HttpStatusCode.OK;
                }
                else
                {
                    res.StatusCode = res.StatusCode;
                    res.ErrorMsg = res.ErrorMsg;
                }
            }
            catch (Exception ex)
            {
                res.StatusCode = res.StatusCode;
                res.ErrorMsg = ex.Message;
            }

            return res;

        }

        public async Task<MCRootList> GetListReport(Guid campaignId)
        {
            var retVal = new MCRootList();

            var campaign = _campaignmailchimpRepository.GetAllEntities().Where(x => x.CampaignID == campaignId).FirstOrDefault();

            var res = await GetListReportApi(campaign);

            var list = new List<ListTable>();

            if (res.StatusCode == System.Net.HttpStatusCode.OK && res != null && res.lists.Count > 0)
            {
                foreach (var item in res.lists)
                {
                    var singleItem = new ListTable();

                    singleItem.Name = item.name;
                    singleItem.Id = item.id;
                    singleItem.Campaigns = item.stats.campaign_count;
                    singleItem.Unsubscribes = item.stats.unsubscribe_count;
                    singleItem.Subscribers = item.stats.member_count;
                    singleItem.Rating = item.list_rating;


                    if (item.stats.open_rate > 0)
                    {
                        singleItem.OpenRate = Math.Round((decimal)item.stats.open_rate, 2);
                    }

                    if (item.stats.click_rate > 0)
                    {
                        singleItem.ClickRate = Math.Round((decimal)item.stats.click_rate, 2);
                    }

                    if (item.stats.avg_sub_rate > 0)
                    {
                        singleItem.AvgSubscribeRate = Math.Round((decimal)item.stats.avg_sub_rate, 2);
                    }

                    if (item.stats.avg_unsub_rate > 0)
                    {
                        singleItem.AvguNSubscribeRate = Math.Round((decimal)item.stats.avg_unsub_rate);
                    }

                    list.Add(singleItem);
                }

                retVal.listTable = list;

                retVal.subscribers = list.Sum(x => x.Subscribers);

                retVal.campaigns = list.Sum(x => x.Campaigns);

                 
                var openRateCount = list.Where(x => x.OpenRate > 0).Select(x => x.OpenRate).Count();

                var clickRateCount = list.Where(x => x.ClickRate > 0).Select(x => x.ClickRate).Count();

                var avgSubscribeRateCount = list.Where(x => x.AvgSubscribeRate > 0).Select(x => x.AvgSubscribeRate).Count();

                var avgUnsubscribeRateCount = list.Where(x => x.OpenRate > 0).Select(x => x.OpenRate).Count();

             
                retVal.openRate = list.Where(x => x.OpenRate > 0).Count() > 0 ? Math.Round(list.Where(x => x.OpenRate > 0).Average(x => x.OpenRate), 2) : 0;

                retVal.clickRate = list.Where(x => x.ClickRate > 0).Count() > 0 ? Math.Round(list.Where(x => x.ClickRate > 0).Average(x => x.ClickRate), 2) : 0;

                retVal.avgSubscribeRate = list.Where(x => x.AvgSubscribeRate > 0).Count() > 0 ? Math.Round(list.Where(x => x.AvgSubscribeRate > 0).Average(x => x.AvgSubscribeRate), 2) : 0;

                retVal.avgUnsubscribeRate = list.Where(x => x.AvguNSubscribeRate > 0).Count() > 0 ? Math.Round(list.Where(x => x.AvguNSubscribeRate > 0).Average(x => x.AvguNSubscribeRate), 2) : 0;

                retVal.unsubscribes = list.Sum(x => x.Unsubscribes);

                retVal.rating = list.Sum(x => x.Rating);

            }

            var audianceGrowth = await GetAudianceGrowthApi(campaign, list.Select(x => x.Id).ToList());

            if (audianceGrowth != null && audianceGrowth.history.Count > 0)
            {
                var result = audianceGrowth.history
                .GroupBy(h => h.month)
                .OrderBy(g => g.Key) // Order by month in ascending order
                .Select(g => new AudianceGrowth
                {
                    Months = new List<string> { g.Key },
                    Subscribers = new List<int> { g.Sum(item => item.subscribed) }
                }).ToList();


                retVal.audianceGrowthChartDates = result.SelectMany(g => g.Months).ToList();
                retVal.audianceGrowthChartValues = result.SelectMany(g => g.Subscribers).ToList();
                retVal.growthChartTotal = result.SelectMany(g => g.Subscribers).Sum();
            }
            else
            {
                retVal.audianceGrowthChartDates = new List<string>() { };
                retVal.audianceGrowthChartValues = new List<int>() { };
                retVal.growthChartTotal = 0;
            }
     
            //Open and click 
            var listOpen = await GetOpenChartForListApi(campaign, list.Select(x => x.Id).ToList());

            if (listOpen.activity.Count > 0)
            {
                var monthlyUniqueOpens = listOpen.activity
                .GroupBy(activity => new { activity.day.Year, activity.day.Month })
                .OrderBy(g => g.Key.Month)
                .Select(group => new ListOpenChart
                {
                    Months = group.Key.Year.ToString() +"-"+ group.Key.Month.ToString(),
                    Opens = group.Sum(activity => activity.unique_opens),
                    Clicks = group.Sum(activity => activity.recipient_clicks),

                }).ToList();

                retVal.openChartDates = monthlyUniqueOpens.Select(g => g.Months).ToList();
                retVal.openChartValues = monthlyUniqueOpens.Select(g => g.Opens).ToList();
                retVal.clickChartValues = monthlyUniqueOpens.Select(g => g.Clicks).ToList();
           

                retVal.opensChartTotal = monthlyUniqueOpens.Count > 0 ? monthlyUniqueOpens.Sum(x => x.Opens) : 0;
                retVal.clickChartTotal = monthlyUniqueOpens.Count > 0 ? monthlyUniqueOpens.Sum(x => x.Clicks) : 0;
            }          

            return retVal;
        }

        public async Task<RootSingleList> GetSingleListReport(Guid campaignId, string id)
        {
            var retVal = new RootSingleList();

            var campaign = _campaignmailchimpRepository.GetAllEntities().Where(x => x.CampaignID == campaignId).FirstOrDefault();

            var res = await GetSingleListReportApi(campaign, id);

            var list = new GetSingleListDto();

            if (res.StatusCode == System.Net.HttpStatusCode.OK && res != null)
            {
                retVal.Campaigns = res.mcList.stats.campaign_count;
                retVal.Unsubscribes = res.mcList.stats.unsubscribe_count; ;
                retVal.Subscribers = res.mcList.stats.member_count;
                retVal.OpenRate = (decimal)res.mcList.stats.open_rate;
                retVal.ClickRate = (decimal)res.mcList.stats.click_rate;
                retVal.AvgSubscribeRate = (decimal)res.mcList.stats.avg_sub_rate;
                retVal.AvguNSubscribeRate = (decimal)res.mcList.stats.avg_unsub_rate;
                retVal.Name = res.mcList.name;

                if (res.mcList.stats.open_rate > 0)
                {
                    retVal.OpenRate = Math.Round((decimal)res.mcList.stats.open_rate, 2);
                }

                if (res.mcList.stats.click_rate > 0)
                {
                    retVal.ClickRate = Math.Round((decimal)res.mcList.stats.click_rate, 2);
                }

                if (res.mcList.stats.avg_sub_rate > 0)
                {
                    retVal.AvgSubscribeRate = Math.Round((decimal)res.mcList.stats.avg_sub_rate, 2);
                }

                if (res.mcList.stats.avg_unsub_rate > 0)
                {
                    retVal.AvguNSubscribeRate = Math.Round((decimal)res.mcList.stats.click_rate, 2);
                }

            }

            //Top email clients

            var clientsResponse = await GetClientOfListApi(campaign, id);

            if (clientsResponse.StatusCode == System.Net.HttpStatusCode.OK && clientsResponse != null && clientsResponse.clients != null)
            {
                retVal.Clients = clientsResponse.clients.Select(x => x.client).ToList();

                retVal.Members = clientsResponse.clients.Select(x => x.members).ToList();                
            }

            //audience growth

            var audianceGrowth = await GetAudianceGrowthApi(campaign, new List<string> { id });

            if (audianceGrowth.StatusCode == System.Net.HttpStatusCode.OK && audianceGrowth != null)
            {
                var result = audianceGrowth.history
                      .GroupBy(h => h.month)
                      .OrderBy(g => g.Key) // Order by month in ascending order
                      .Select(g => new AudianceGrowth
                      {
                          Months = new List<string> { g.Key },
                          Subscribers = new List<int> { g.Sum(item => item.subscribed) }
                      }).ToList();


                retVal.AudianceGrowthChartDates = result.SelectMany(g => g.Months).ToList();
                retVal.AudianceGrowthChartValues = result.SelectMany(g => g.Subscribers).ToList();
                retVal.AudienceGrowthTotal = result.SelectMany(g => g.Subscribers).Sum();

            }

            //Open and click for single list 
            var listOpen = await GetOpenChartForListApi(campaign, new List<string> { id });

            if (listOpen.activity.Count > 0)
            {
                var monthlyUniqueOpens = listOpen.activity
                .GroupBy(activity => new { activity.day.Year, activity.day.Month })
                .OrderBy(g => g.Key.Month)
                .Select(group => new ListOpenChart
                {
                    Months = group.Key.Year.ToString() + "-" + group.Key.Month.ToString(),
                    Opens = group.Sum(activity => activity.unique_opens),
                    Clicks = group.Sum(activity => activity.recipient_clicks),

                }).ToList();

                retVal.ChartDates = monthlyUniqueOpens.Select(g => g.Months).ToList();
                retVal.OpensChartValues = monthlyUniqueOpens.Select(g => g.Opens).ToList();
                retVal.ClicksChartValues = monthlyUniqueOpens.Select(g => g.Clicks).ToList();

                retVal.OpensChartTotal = monthlyUniqueOpens.Count > 0 ? monthlyUniqueOpens.Sum(x => x.Opens) : 0;
                retVal.ClickChartTotal = monthlyUniqueOpens.Count > 0 ? monthlyUniqueOpens.Sum(x => x.Clicks) : 0;
            }


            return retVal;
        }
        public string GetCurrencySymbol(string currency)
        {
            var currencyCode = new List<Currency>();
            var restClient = new RestClient(_configuration.GetSection("BlobUrl").Value + "Json");
            var restRequest = new RestRequest("/currency_code.json", Method.Get);

            var responseCode = restClient.GetAsync(restRequest).Result;
            if (responseCode.StatusCode == System.Net.HttpStatusCode.OK)
            {
                currencyCode = JsonConvert.DeserializeObject<List<Currency>>(responseCode.Content);
            }

            var currencySymbol = currencyCode
                .Where(c => c.code == currency)
                .Select(c => c.symbol)
                .FirstOrDefault();

            return !string.IsNullOrEmpty(currencySymbol) ? currencySymbol : currency;
        }

        public async Task<SingleCampaignReport> GetSingleCampaignReport(Guid campaignId, string mcCampaignId)
        {
            var retVal = new SingleCampaignReport();

            var report = await GetSingleCampaignReportApi(campaignId, mcCampaignId);

            var currencySymbol = string.Empty;

            if (!string.IsNullOrEmpty(report.ecommerce.currency_code))
            {
                 currencySymbol = GetCurrencySymbol(report.ecommerce.currency_code);
            }

            if (report.StatusCode == System.Net.HttpStatusCode.OK && report != null)
            {
                if (report != null && report.timeseries != null)
                {
                    // Open 24 hours chart
                    var uniqueOpenChartData11 = report.timeseries
                        .Select(entry => (Timestamp: entry.timestamp, Unique: entry.unique_opens))
                        .OrderBy(entry => DateTimeOffset.Parse(entry.Timestamp))
                        .GroupBy(entry => DateTimeOffset.Parse(entry.Timestamp).ToString("dddd d MMMM yyyy"))
                        .Select(group => new MCChartData
                        {
                            Dates = group.Key,
                            Values = group.Sum(entry => entry.Unique)
                        })
                        .ToList();


                    var uniqueOpenDates = uniqueOpenChartData11.Select(x => x.Dates).ToList();
                    var uniqueOpenValue = uniqueOpenChartData11.Select(x => x.Values).ToList();
                    var totalUniqueOpenChart = uniqueOpenChartData11.Sum(x => x.Values);

                    retVal.OpenChartDates = uniqueOpenDates;
                    retVal.OpenChartValues = uniqueOpenValue;
                    retVal.UniqueOpenChartTotal = totalUniqueOpenChart;

                    // Clicks 24 hours chart
                    var clickData = report.timeseries
                        .Select(entry => (Timestamp: entry.timestamp, Click: entry.recipients_clicks))
                        .OrderBy(entry => DateTimeOffset.Parse(entry.Timestamp))
                        .GroupBy(entry => DateTimeOffset.Parse(entry.Timestamp).ToString("dddd d MMMM yyyy"))
                        .Select(group => new MCChartData
                        {
                            Dates = group.Key,
                            Values = group.Sum(entry => entry.Click)
                        })
                        .ToList();

                    var clickDates = clickData.Select(x => x.Dates).ToList();
                    var clickValue = clickData.Select(x => x.Values).ToList();
                    var totalClickChart = clickData.Sum(x => x.Values);

                    retVal.ClickChartDates = clickDates;
                    retVal.ClickChartTotal = totalClickChart;
                    retVal.ClickChartValues = clickValue;

                }

                var notOpened = report.emails_sent - report.opens.unique_opens;

                //pieChart
                //emails_sent-opens.unique_opens = Not Open
                //bounce = bounces.hard_bounces + bounces.soft_bounces

                retVal.PieValues = new List<decimal> { notOpened, report.opens.unique_opens, report.bounces.soft_bounces + report.bounces.hard_bounces };

                retVal.Name = string.IsNullOrEmpty(report.campaign_title) ? report.subject_line : report.campaign_title;
                // Small Tiles            
                retVal.Recipients = report.emails_sent;
                retVal.UniqueOpens = report.opens.unique_opens;
                retVal.UnopenedEmails = retVal.Recipients - retVal.UniqueOpens;
                retVal.BouncedEmails = report.bounces.hard_bounces + report.bounces.soft_bounces;
                retVal.SubsciberClick = (decimal)report.clicks.unique_subscriber_clicks;
                retVal.Unsubscribes = (decimal)report.unsubscribed;
                retVal.Click = (decimal)report.clicks.clicks_total;
                retVal.Opens = (decimal)report.opens.opens_total;
                retVal.Orders = (decimal)report.ecommerce.total_orders;
                retVal.Revenue = (decimal)report.ecommerce.total_revenue;
                retVal.TotalSpent = (decimal)report.ecommerce.total_spent;
                retVal.Deliveries = retVal.Recipients - retVal.BouncedEmails;
                retVal.Spams = (decimal)report.abuse_reports;
                retVal.AverageOrder = retVal.Orders > 0 ? Math.Round(retVal.TotalSpent / retVal.Orders, 2) : 0;

                if (report.opens.open_rate > 0)
                {
                    retVal.OpenRate = Math.Round((decimal)report.opens.open_rate * 100, 2);
                }

                if (report.clicks.click_rate > 0)
                {
                    retVal.ClickRate = Math.Round((decimal)report.clicks.click_rate * 100, 2);
                }

                retVal.Unsubscribes = report.unsubscribed;

                if (retVal.Recipients > 0)
                {
                    retVal.UnsubscribeRate = Math.Round((retVal.Unsubscribes / retVal.Recipients) * 100, 2);
                }

                if (report.emails_sent > 0)
                {
                    retVal.BounceRate = Math.Round((retVal.BouncedEmails / report.emails_sent) * 100, 2);
                    retVal.DeliveryRate = Math.Round(((retVal.Recipients - retVal.BouncedEmails) / retVal.Recipients) * 100, 2);
                    retVal.SpamRate = Math.Round((decimal)(report.abuse_reports / report.emails_sent) * 100, 2);
                }
            }

            var topLocation = await GetTopLocationByCampaign(campaignId, mcCampaignId);

            if (topLocation.StatusCode == System.Net.HttpStatusCode.OK && topLocation != null)
            {
                var top3Countries = topLocation.locations
                                    .GroupBy(location => location.country_code)
                                    .Select(group => new
                                    {
                                        CountryCode = group.Key,
                                        TotalOpens = group.Sum(location => location.opens)
                                    })
                                    .OrderByDescending(item => item.TotalOpens)
                                    .Take(3)
                                    .ToList();

                var labels = top3Countries.Select(x => x.CountryCode).ToList();

                var values = top3Countries.Select(x => x.TotalOpens).ToList();

                retVal.LocationLabels = labels;

                retVal.LocationValues = values;

                retVal.LocationChartTotal = top3Countries.Select(x => x.TotalOpens).Sum();

            }

            var topLinks = await GetTopLinksByCampaign(campaignId, mcCampaignId);

            if (topLinks.StatusCode == System.Net.HttpStatusCode.OK && topLinks != null)
            {
                var urlsOrderedByClicks = topLinks.urls_clicked
                                    .OrderByDescending(url => url.total_clicks)
                                    .ToList();

                retVal.TopUrlsClick = urlsOrderedByClicks;

            }

            retVal.CurrencyCode = currencySymbol;

            return retVal;
        }

        private async Task<LocationResponse> GetTopLocationByCampaign(Guid campaignId, string mcCampaignId)
        {
            var res = new LocationResponse();
            try
            {
                var campaign = _campaignmailchimpRepository.GetAllEntities().Where(x => x.CampaignID == campaignId).FirstOrDefault();

                // Mailchimp API endpoint
                string apiEndpoint = campaign.ApiEndpoint + "/3.0";

                // Resource path
                string resourcePath = "/reports/" + mcCampaignId + "/locations";

                // Create the RestSharp client
                var client = new RestClient(apiEndpoint);

                // Create the RestSharp request
                var request = new RestRequest(resourcePath, Method.Get);

                request.AddHeader("Content-Type", "application/json");

                // Add authorization header
                request.AddHeader("Authorization", $"Bearer {campaign.AccessToken}");

                request.AddParameter("count", 1000, ParameterType.QueryString);

                // Add query parameters
                //request.AddParameter("application/json", queryParams, ParameterType.QueryString);

                // Execute the request and get the response
                var response = await client.ExecuteAsync(request);
                res = JsonConvert.DeserializeObject<LocationResponse>(response.Content);

                // Check if the request was successful (status code 200)
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    res.StatusCode = System.Net.HttpStatusCode.OK;
                }
                else
                {
                    res.StatusCode = res.StatusCode;
                    res.ErrorMsg = res.ErrorMsg;
                }
            }
            catch (Exception ex)
            {
                res.StatusCode = res.StatusCode;
                res.ErrorMsg = ex.Message;
            }

            return res;
        }

        public async Task<ClickDetailsDto> GetTopLinksByCampaign(Guid campaignId, string mcCampaignId)
        {
            var res = new ClickDetailsDto();
            try
            {
                var campaign = _campaignmailchimpRepository.GetAllEntities().Where(x => x.CampaignID == campaignId).FirstOrDefault();

                // Mailchimp API endpoint
                string apiEndpoint = campaign.ApiEndpoint + "/3.0";

                // Resource path
                string resourcePath = "/reports/" + mcCampaignId + "/click-details";

                // Create the RestSharp client
                var client = new RestClient(apiEndpoint);

                // Create the RestSharp request
                var request = new RestRequest(resourcePath, Method.Get);

                request.AddHeader("Content-Type", "application/json");

                // Add authorization header
                request.AddHeader("Authorization", $"Bearer {campaign.AccessToken}");

                request.AddParameter("count", 1000, ParameterType.QueryString);

                // Add query parameters
                //request.AddParameter("application/json", queryParams, ParameterType.QueryString);

                // Execute the request and get the response
                var response = await client.ExecuteAsync(request);
                res = JsonConvert.DeserializeObject<ClickDetailsDto>(response.Content);

                // Check if the request was successful (status code 200)
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    res.StatusCode = System.Net.HttpStatusCode.OK;
                }
                else
                {
                    res.StatusCode = res.StatusCode;
                    res.ErrorMsg = res.ErrorMsg;
                }
            }
            catch (Exception ex)
            {
                res.StatusCode = res.StatusCode;
                res.ErrorMsg = ex.Message;
            }

            return res;
        }

        public async Task<CampaignTableRoot> GetCampaignTable(Guid campaignId, string mcCampaignId, int offset, int count)
        {
            var res = new CampaignTableRoot();
            try
            {
                var campaign = _campaignmailchimpRepository.GetAllEntities().Where(x => x.CampaignID == campaignId).FirstOrDefault();

                // Mailchimp API endpoint
                string apiEndpoint = campaign.ApiEndpoint + "/3.0";

                // Resource path
                string resourcePath = "/reports/" + mcCampaignId + "/email-activity";

                // Create the RestSharp client
                var client = new RestClient(apiEndpoint);

                // Create the RestSharp request
                var request = new RestRequest(resourcePath, Method.Get);

                request.AddHeader("Content-Type", "application/json");

                // Add authorization header
                request.AddHeader("Authorization", $"Bearer {campaign.AccessToken}");

                request.AddParameter("count", count, ParameterType.QueryString);

                request.AddParameter("offset", offset, ParameterType.QueryString);

                // Add query parameters
                //request.AddParameter("application/json", queryParams, ParameterType.QueryString);

                // Execute the request and get the response
                var response = await client.ExecuteAsync(request);
                var emailActivityResponse = JsonConvert.DeserializeObject<EmailActivityResponse>(response.Content);


                //var emailTableList = new List<CampaignTableResponse>();



                // Check if the request was successful (status code 200)
                if (response.StatusCode == System.Net.HttpStatusCode.OK && emailActivityResponse != null)
                {

                    var emailTableList = emailActivityResponse.emails.Select(item => new CampaignTableResponse
                    {
                        Email = item.email_address,
                        OpenCount = item.activity?.Count(x => x.action == "open") ?? 0,
                        Status = item.activity?.Any() == true
                       ? item.activity[0].action == "bounce"
                           ? item.activity[0].type.ToUpper() + " " + item.activity[0].action.ToUpper()
                           : "SENT"
                       : "SENT"
                    }).ToList();

                    res.CampaignTableResponse = emailTableList;
                    res.total_items = emailActivityResponse.total_items;
                    res.StatusCode = System.Net.HttpStatusCode.OK;
                }
                else
                {
                    res.StatusCode = res.StatusCode;
                    res.ErrorMsg = res.ErrorMsg;
                }
            }
            catch (Exception ex)
            {
                res.StatusCode = res.StatusCode;
                res.ErrorMsg = ex.Message;
            }

            return res;
        }

        public async Task<CampaignTableRoot> GetCampaignTable(Guid campaignId, string mcCampaignId)
        {
            var res = new CampaignTableRoot();

            try
            {
                var campaign = _campaignmailchimpRepository.GetAllEntities().FirstOrDefault(x => x.CampaignID == campaignId);

                // Mailchimp API endpoint
                string apiEndpoint = campaign.ApiEndpoint + "/3.0";

                // Resource path
                string resourcePath = "/reports/" + mcCampaignId + "/email-activity";

                // Create the RestSharp client
                var client = new RestClient(apiEndpoint);

                // Create the RestSharp request
                var request = new RestRequest(resourcePath, Method.Get);

                request.AddHeader("Content-Type", "application/json");

                // Add authorization header
                request.AddHeader("Authorization", $"Bearer {campaign.AccessToken}");

                int offset = 0;
                int count = 1000; // Assuming the maximum count is 1000, adjust as needed

                var emailTableList = new List<CampaignTableResponse>();

                int totalItems;

                do
                {
                    // Set offset and count in the request
                    request.AddParameter("offset", offset, ParameterType.QueryString);
                    request.AddParameter("count", count, ParameterType.QueryString);

                    // Execute the request and get the response
                    var response = await client.ExecuteAsync(request);
                    var emailActivityResponse = JsonConvert.DeserializeObject<EmailActivityResponse>(response.Content);

                    // Check if the request was successful (status code 200)
                    if (response.StatusCode == System.Net.HttpStatusCode.OK && emailActivityResponse != null)
                    {
                        totalItems = emailActivityResponse.total_items; // Retrieve total items

                        var batchEmailTableList = emailActivityResponse.emails.Select(item => new CampaignTableResponse
                        {
                            Email = item.email_address,
                            OpenCount = item.activity?.Count(x => x.action == "open") ?? 0,
                            Status = item.activity?.Any() == true
                                ? item.activity[0].action == "bounce"
                                    ? item.activity[0].type.ToUpper() + " " + item.activity[0].action.ToUpper()
                                    : "SENT"
                                : "SENT"
                        }).ToList();

                        emailTableList.AddRange(batchEmailTableList);

                        // Increment offset for the next batch
                        offset += count;
                    }
                    else
                    {
                        res.StatusCode = response.StatusCode;
                        res.ErrorMsg = response.ErrorMessage;
                        break; // Break the loop if an error occurs
                    }

                } while (offset < totalItems);

                res.CampaignTableResponse = emailTableList;
                res.StatusCode = System.Net.HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                res.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                res.ErrorMsg = ex.Message;
            }

            return res;
        }

        private async Task<GrowthHistoryResponse> GetAudianceGrowthApi(CampaignMailchimp campaign, List<string> listId)
        {
            var res = new GrowthHistoryResponse();

            foreach (var id in listId)
            {
                try
                {
                    // Mailchimp API endpoint
                    string apiEndpoint = campaign.ApiEndpoint + "/3.0";

                    // Resource path
                    string resourcePath = "/lists/" + id + "/growth-history";

                    // Create the RestSharp client
                    var client = new RestClient(apiEndpoint);

                    // Create the RestSharp request
                    var request = new RestRequest(resourcePath, Method.Get);

                    request.AddHeader("Content-Type", "application/json");

                    // Add authorization header
                    request.AddHeader("Authorization", $"Bearer {campaign.AccessToken}");

                    request.AddParameter("count", 1000, ParameterType.QueryString);


                    request.AddParameter("fields", "history.list_id,history.month,history.subscribed", ParameterType.QueryString);

                    // Execute the request and get the response
                    var response = await client.ExecuteAsync(request);

                    var growthHistoryResponse = JsonConvert.DeserializeObject<GrowthHistoryResponse>(response.Content);

                    // Check if the request was successful (status code 200)
                    if (response.StatusCode == System.Net.HttpStatusCode.OK && growthHistoryResponse != null)
                    {                     
                        res.history.AddRange(growthHistoryResponse.history);
                        res.StatusCode = System.Net.HttpStatusCode.OK;
                    }
                    else
                    {
                        res.StatusCode = response.StatusCode;
                        res.ErrorMsg = response.ErrorMessage;
                    }
                }
                catch (Exception ex)
                {
                    res.ErrorMsg = ex.Message;
                }
            }

            return res;
        }

        private async Task<RootRecentActivity> GetOpenChartForListApi(CampaignMailchimp campaign, List<string> listId)
        {
            var res = new RootRecentActivity();

            foreach (var id in listId)
            {
                try
                {
                    // Mailchimp API endpoint
                    string apiEndpoint = campaign.ApiEndpoint + "/3.0";

                    // Resource path
                    string resourcePath = "/lists/" + id + "/activity";

                    // Create the RestSharp client
                    var client = new RestClient(apiEndpoint);

                    // Create the RestSharp request
                    var request = new RestRequest(resourcePath, Method.Get);

                    request.AddHeader("Content-Type", "application/json");

                    // Add authorization header
                    request.AddHeader("Authorization", $"Bearer {campaign.AccessToken}");

                    request.AddParameter("count", 1000, ParameterType.QueryString);


                    request.AddParameter("fields", "activity.day,activity.unique_opens,activity.recipient_clicks,total_items", ParameterType.QueryString);

                    // Execute the request and get the response
                    var response = await client.ExecuteAsync(request);

                    var activityResponse = JsonConvert.DeserializeObject<RecentActivity>(response.Content);

                    // Check if the request was successful (status code 200)
                    if (response.StatusCode == System.Net.HttpStatusCode.OK && activityResponse != null && activityResponse.activity != null)
                    {
                        res.activity.AddRange(activityResponse.activity);
                        res.StatusCode = System.Net.HttpStatusCode.OK;
                    }
                    else
                    {
                        res.StatusCode = response.StatusCode;
                        res.ErrorMsg = response.ErrorMessage;
                    }
                }
                catch (Exception ex)
                {
                    res.ErrorMsg = ex.Message;
                }
            }

            return res;
        }


        private async Task<EmailClientRoot> GetClientOfListApi(CampaignMailchimp campaign, string listId)
        {
            var res = new EmailClientRoot();

            try
            {
                // Mailchimp API endpoint
                string apiEndpoint = campaign.ApiEndpoint + "/3.0";

                // Resource path
                string resourcePath = "/lists/" + listId + "/clients";

                // Create the RestSharp client
                var client = new RestClient(apiEndpoint);

                // Create the RestSharp request
                var request = new RestRequest(resourcePath, Method.Get);

                request.AddHeader("Content-Type", "application/json");

                // Add authorization header
                request.AddHeader("Authorization", $"Bearer {campaign.AccessToken}");

                request.AddParameter("count", 1000, ParameterType.QueryString);


                request.AddParameter("fields", "clients.client,clients.members,total_items", ParameterType.QueryString);

                // Execute the request and get the response
                var responseRequest = await client.ExecuteAsync(request);
                var emailClientRoot = JsonConvert.DeserializeObject<EmailClientRoot>(responseRequest.Content);


                // Check if the request was successful (status code 200)
                if (responseRequest.StatusCode == System.Net.HttpStatusCode.OK && emailClientRoot != null)
                {
                    res.clients = emailClientRoot.clients;
                    res.StatusCode = System.Net.HttpStatusCode.OK;
                }
                else
                {
                    res.StatusCode = responseRequest.StatusCode;
                    res.ErrorMsg = responseRequest.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                res.ErrorMsg = ex.Message;
            }

            return res;
        }

        public async Task<MailChimpMemberRoot> GetMemberOfListApi(Guid campaignId, string listId, int offset, int count, string status)
        {
            var retVal = new MailChimpMemberRoot();

            try
            {
                var campaign = _campaignmailchimpRepository.GetAllEntities().FirstOrDefault(x => x.CampaignID == campaignId);

                // Mailchimp API endpoint
                string apiEndpoint = campaign.ApiEndpoint + "/3.0";

                // Resource path
                string resourcePath = "/lists/" + listId + "/members";

                // Create the RestSharp client
                var client = new RestClient(apiEndpoint);

                // Create the RestSharp request
                var request = new RestRequest(resourcePath, Method.Get);

                request.AddHeader("Content-Type", "application/json");

                // Add authorization header
                request.AddHeader("Authorization", $"Bearer {campaign.AccessToken}");

                request.AddParameter("count", count, ParameterType.QueryString);

                request.AddParameter("offset", offset, ParameterType.QueryString);

                if (!string.IsNullOrEmpty(status))
                {
                    request.AddParameter("status", status, ParameterType.QueryString);
                }

                request.AddParameter("fields", "total_items,members.email_address,members.timestamp_signup,members.location.region,members.member_rating,members.status,members.stats.avg_open_rate,members.stats.avg_click_rate", ParameterType.QueryString);

                // Execute the request and get the response
                var response = await client.ExecuteAsync(request);

                var res = JsonConvert.DeserializeObject<MailChimpMemberResponse>(response.Content);

                // Check if the request was successful (status code 200)
                if (response.StatusCode == System.Net.HttpStatusCode.OK && res != null)
                {
                    var batchEmailTableList = res.members.Select(item => new MailChimpMember
                    {
                        Email = item.email_address,
                        SignupDate = item.timestamp_signup,
                        Country = item.location.region,
                        MemberRating = item.member_rating,
                        Status = item.status,
                        OpenRate = item.stats.avg_open_rate,
                        ClickRate = item.stats.avg_click_rate

                    }).ToList();

                    retVal.MailChimpMembers = batchEmailTableList;

                    retVal.TotalItems = res.total_items;

                    retVal.StatusCode = System.Net.HttpStatusCode.OK;
                }
                else
                {
                    retVal.StatusCode = res.StatusCode;
                    retVal.ErrorMsg = res.ErrorMsg;
                }
            }
            catch (Exception ex)
            {
                retVal.ErrorMsg = ex.Message;
            }


            return retVal;
        }

        public async Task<MailChimpMemberRoot> GetMemberOfListApi(Guid campaignId, string listId)
        {
            var retVal = new MailChimpMemberRoot();

            try
            {
                var campaign = _campaignmailchimpRepository.GetAllEntities().FirstOrDefault(x => x.CampaignID == campaignId);

                // Mailchimp API endpoint
                string apiEndpoint = campaign.ApiEndpoint + "/3.0";

                // Resource path
                string resourcePath = "/lists/" + listId + "/members";

                // Create the RestSharp client
                var client = new RestClient(apiEndpoint);

                // Create the RestSharp request
                var request = new RestRequest(resourcePath, Method.Get);

                request.AddHeader("Content-Type", "application/json");

                // Add authorization header
                request.AddHeader("Authorization", $"Bearer {campaign.AccessToken}");

                request.AddParameter("fields", "total_items,members.email_address,members.timestamp_signup,members.location.region,members.member_rating,members.status,members.stats.avg_open_rate,members.stats.avg_click_rate", ParameterType.QueryString);



                int offset = 0;
                int count = 1000; // Assuming the maximum count is 1000, adjust as needed
                var emailTableList = new List<MailChimpMember>();
                int totalItems = 0;

                do
                {
                    // Set offset and count in the request
                    request.AddParameter("offset", offset, ParameterType.QueryString);
                    request.AddParameter("count", count, ParameterType.QueryString);

                    // Execute the request and get the response
                    var response = await client.ExecuteAsync(request);

                    var res = JsonConvert.DeserializeObject<MailChimpMemberResponse>(response.Content);

                    // Check if the request was successful (status code 200)
                    if (response.StatusCode == System.Net.HttpStatusCode.OK && res != null)
                    {
                        totalItems = res.total_items; // Retrieve total items

                        var batchEmailTableList = res.members.Select(item => new MailChimpMember
                        {
                            Email = item.email_address,
                            SignupDate = item.timestamp_signup,
                            Country = item.location.region,
                            MemberRating = item.member_rating,
                            Status = item.status,
                            OpenRate = item.stats.avg_open_rate,
                            ClickRate = item.stats.avg_click_rate

                        }).ToList();

                        emailTableList.AddRange(batchEmailTableList);

                        offset += count;
                    }
                    else
                    {
                        retVal.StatusCode = res.StatusCode;
                        retVal.ErrorMsg = res.ErrorMsg;
                        break;
                    }


                } while (offset < totalItems);

                retVal.MailChimpMembers = emailTableList;

                retVal.TotalItems = totalItems;

            }
            catch (Exception ex)
            {
                retVal.ErrorMsg = ex.Message;
            }


            return retVal;
        }

        private async Task<ListReport> GetListReportApi(CampaignMailchimp campaign)
        {
            var res = new ListReport();
            try
            {

                // Mailchimp API endpoint
                string apiEndpoint = campaign.ApiEndpoint + "/3.0";

                // Resource path
                string resourcePath = "/lists";

                // Create the RestSharp client
                var client = new RestClient(apiEndpoint);

                // Create the RestSharp request
                var request = new RestRequest(resourcePath, Method.Get);

                request.AddHeader("Content-Type", "application/json");

                // Add authorization header
                request.AddHeader("Authorization", $"Bearer {campaign.AccessToken}");

                request.AddParameter("count", 1000, ParameterType.QueryString);

                request.AddParameter("fields", "total_items,lists.stats,lists.list_rating,lists.name,lists.id", ParameterType.QueryString);

                // Add query parameters
                //request.AddParameter("application/json", queryParams, ParameterType.QueryString);

                // Execute the request and get the response
                var response = await client.ExecuteAsync(request);
                res = JsonConvert.DeserializeObject<ListReport>(response.Content);

                // Check if the request was successful (status code 200)
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    res.StatusCode = System.Net.HttpStatusCode.OK;
                }
                else
                {
                    res.StatusCode = res.StatusCode;
                    res.ErrorMsg = res.ErrorMsg;
                }
            }
            catch (Exception ex)
            {
                res.StatusCode = res.StatusCode;
                res.ErrorMsg = ex.Message;
            }

            return res;

        }

        private async Task<GetSingleListDto> GetSingleListReportApi(CampaignMailchimp campaign, string id)
        {
            var retVal = new GetSingleListDto();
            try
            {

                // Mailchimp API endpoint
                string apiEndpoint = campaign.ApiEndpoint + "/3.0";

                // Resource path
                string resourcePath = "/lists/" + id;

                // Create the RestSharp client
                var client = new RestClient(apiEndpoint);

                // Create the RestSharp request
                var request = new RestRequest(resourcePath, Method.Get);

                request.AddHeader("Content-Type", "application/json");

                // Add authorization header
                request.AddHeader("Authorization", $"Bearer {campaign.AccessToken}");

                // Execute the request and get the response
                var response = await client.ExecuteAsync(request);
                retVal.mcList = JsonConvert.DeserializeObject<List>(response.Content);

                // Check if the request was successful (status code 200)
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    retVal.StatusCode = System.Net.HttpStatusCode.OK;
                }
                else
                {
                    retVal.StatusCode = response.StatusCode;
                    retVal.ErrorMsg = response.ErrorMessage;
                }
            }
            catch (Exception ex)
            {

                retVal.ErrorMsg = ex.Message;
            }

            return retVal;

        }

        private async Task<McReport> GetSingleCampaignReportApi(Guid campaignId, string mcCampaignId)
        {
            var res = new McReport();
            try
            {
                var campaign = _campaignmailchimpRepository.GetAllEntities().Where(x => x.CampaignID == campaignId).FirstOrDefault();

                // Mailchimp API endpoint
                string apiEndpoint = campaign.ApiEndpoint + "/3.0";

                // Resource path
                string resourcePath = "/reports/" + mcCampaignId;

                // Create the RestSharp client
                var client = new RestClient(apiEndpoint);

                // Create the RestSharp request
                var request = new RestRequest(resourcePath, Method.Get);

                request.AddHeader("Content-Type", "application/json");

                // Add authorization header
                request.AddHeader("Authorization", $"Bearer {campaign.AccessToken}");

                request.AddParameter("count", 1000, ParameterType.QueryString);

                // Add query parameters
                //request.AddParameter("application/json", queryParams, ParameterType.QueryString);

                // Execute the request and get the response
                var response = await client.ExecuteAsync(request);
                res = JsonConvert.DeserializeObject<McReport>(response.Content);

                // Check if the request was successful (status code 200)
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    res.StatusCode = System.Net.HttpStatusCode.OK;
                }
                else
                {
                    res.StatusCode = res.StatusCode;
                    res.ErrorMsg = res.ErrorMsg;
                }
            }
            catch (Exception ex)
            {
                res.StatusCode = res.StatusCode;
                res.ErrorMsg = ex.Message;
            }

            return res;

        }

        public async Task<MCCampaignList> GetCampaignList(Guid campaignId)
        {
            var res = new MCCampaignList();
            try
            {
                var campaign = _campaignmailchimpRepository.GetAllEntities().Where(x => x.CampaignID == campaignId).FirstOrDefault();

                // Mailchimp API endpoint
                string apiEndpoint = campaign.ApiEndpoint + "/3.0";

                // Resource path
                string resourcePath = "/reports";

                // Create the RestSharp client
                var client = new RestClient(apiEndpoint);

                // Create the RestSharp request
                var request = new RestRequest(resourcePath, Method.Get);

                request.AddHeader("Content-Type", "application/json");

                // Add authorization header
                request.AddHeader("Authorization", $"Bearer {campaign.AccessToken}");

                request.AddParameter("count", 1000, ParameterType.QueryString);

                request.AddParameter("fields", "reports.id,reports.campaign_title,reports.subject_line", ParameterType.QueryString);

                // Add query parameters
                //request.AddParameter("application/json", queryParams, ParameterType.QueryString);

                // Execute the request and get the response
                var response = await client.ExecuteAsync(request);
                res = JsonConvert.DeserializeObject<MCCampaignList>(response.Content);

                // Check if the request was successful (status code 200)
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    res.StatusCode = System.Net.HttpStatusCode.OK;
                }
                else
                {
                    res.StatusCode = res.StatusCode;
                    res.ErrorMsg = res.ErrorMsg;
                }
            }
            catch (Exception ex)
            {
                res.StatusCode = res.StatusCode;
                res.ErrorMsg = ex.Message;
            }

            return res;

        }

        public async Task<McListRoot> GetMcList(Guid campaignId)
        {
            var res = new McListRoot();
            try
            {
                var campaign = _campaignmailchimpRepository.GetAllEntities().Where(x => x.CampaignID == campaignId).FirstOrDefault();

                // Mailchimp API endpoint
                string apiEndpoint = campaign.ApiEndpoint + "/3.0";

                // Resource path
                string resourcePath = "/lists";

                // Create the RestSharp client
                var client = new RestClient(apiEndpoint);

                // Create the RestSharp request
                var request = new RestRequest(resourcePath, Method.Get);

                request.AddHeader("Content-Type", "application/json");

                // Add authorization header
                request.AddHeader("Authorization", $"Bearer {campaign.AccessToken}");

                request.AddParameter("count", 1000, ParameterType.QueryString);

                request.AddParameter("fields", "lists.id,lists.name", ParameterType.QueryString);

                // Add query parameters
                //request.AddParameter("application/json", queryParams, ParameterType.QueryString);

                // Execute the request and get the response
                var response = await client.ExecuteAsync(request);
                var mcList = JsonConvert.DeserializeObject<ListReport>(response.Content);

                // Check if the request was successful (status code 200)
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    res.StatusCode = System.Net.HttpStatusCode.OK;
                    res.McList = mcList.lists;
                }
                else
                {
                    res.StatusCode = res.StatusCode;
                    res.ErrorMsg = res.ErrorMsg;
                }
            }
            catch (Exception ex)
            {
                res.StatusCode = res.StatusCode;
                res.ErrorMsg = ex.Message;
            }

            return res;

        }


        #endregion


        #region OVERRIDDEN IMPLEMENTATION

        public override Dictionary<string, PropertyMappingValue> GetPropertyMapping()
        {
            return new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "Id", new PropertyMappingValue(new List<string>() { "Id" } ) },
                        { "UpdatedOn", new PropertyMappingValue(new List<string>() { "UpdatedOn" } )},
                    };
        }

        public override string GetDefaultOrderByColumn()
        {
            return "UpdatedOn";
        }

        public override string GetDefaultFieldsToSelect()
        {
            return "Id,AccountName,AccountId,CampaignID";
        }

        #endregion
    }
}
