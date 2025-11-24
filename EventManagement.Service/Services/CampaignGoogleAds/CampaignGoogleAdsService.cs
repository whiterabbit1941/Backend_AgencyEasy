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
using Google.Api.Ads.AdWords.Lib;
using Google.Apis.Auth.OAuth2;
using System.Threading;
using Google.Apis.Util.Store;
using Google.Ads.GoogleAds.Config;
using Google.Ads.GoogleAds.Lib;
using Google.Ads.GoogleAds;
using Google.Api.Gax;
using Google.Protobuf.Collections;
using EventManagement.Utility.Enums;
using RestSharp;
using Google.Ads.GoogleAds.V15.Services;
using Google.Ads.GoogleAds.V15.Resources;
using RestSharp.Authenticators.OAuth2;
using IdentityServer4.Models;

namespace EventManagement.Service
{
    public class CampaignGoogleAdsService : ServiceBase<CampaignGoogleAds, Guid>, ICampaignGoogleAdsService
    {

        #region PRIVATE MEMBERS

        private readonly ICampaignGoogleAdsRepository _campaigngoogleadsRepository;
        private readonly IConfiguration _configuration;
        private readonly ICampaignRepository _campaignRepository;

        #endregion


        #region CONSTRUCTOR

        public CampaignGoogleAdsService(ICampaignGoogleAdsRepository campaigngoogleadsRepository, ILogger<CampaignGoogleAdsService> logger, IConfiguration configuration, ICampaignRepository campaignRepository) : base(campaigngoogleadsRepository, logger)
        {
            _campaigngoogleadsRepository = campaigngoogleadsRepository;
            _configuration = configuration;
            _campaignRepository = campaignRepository;
        }

        #endregion


        #region PUBLIC MEMBERS   

        /// <summary>
        /// Gets List of google customer
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <returns>list of customers</returns>
        public List<GoogleAdsCustomerDto> GetListOfGaAdsCustomer(string refreshToken)
        {

            List<GoogleAdsCustomerDto> gaAdsCustomerList = new List<GoogleAdsCustomerDto>();
            // var camid = new Guid(campaignId);
            //var gaAdsSetup = _campaigngoogleadsRepository.GetAllEntities(true).Where(x => x.CampaignID == camid).Select(ga => new CampaignGoogleAdsDto
            //{
            //    Id = ga.Id,
            //    RefreshToken = ga.RefreshToken

            //}).FirstOrDefault();
            try
            {
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    GoogleAdsConfig config = new GoogleAdsConfig();
                    config.OAuth2ClientId = _configuration.GetSection("ClientIdForGoogleAds").Value;
                    config.OAuth2ClientSecret = _configuration.GetSection("ClientSecretForGoogleAds").Value;
                    config.OAuth2Scope = "https://www.googleapis.com/auth/adwords";
                    config.DeveloperToken = _configuration.GetSection("DeveloperTokenForGoogleAds").Value;
                    config.OAuth2RefreshToken = refreshToken;
                    GoogleAdsClient client = new GoogleAdsClient(config);

                    GoogleAdsServiceClient googleAdsServiceClient =
                        client.GetService(Services.V15.GoogleAdsService);

                    CustomerServiceClient customerServiceClient =
                        client.GetService(Services.V15.CustomerService);

                    string[] customerResourceNames = customerServiceClient.ListAccessibleCustomers();


                    List<long> seedCustomerIds = new List<long>();
                    foreach (string customerResourceName1 in customerResourceNames)
                    {
                        CustomerName customerName1 = CustomerName.Parse(customerResourceName1);
                        seedCustomerIds.Add(long.Parse(customerName1.CustomerId));
                    }

                    long? managerCustomerId = null;
                    GoogleAdsConfig config1 = new GoogleAdsConfig();
                    config1.OAuth2ClientId = _configuration.GetSection("ClientIdForGoogleAds").Value;
                    config1.OAuth2ClientSecret = _configuration.GetSection("ClientSecretForGoogleAds").Value;
                    config1.OAuth2Scope = "https://www.googleapis.com/auth/adwords";
                    config1.DeveloperToken = _configuration.GetSection("DeveloperTokenForGoogleAds").Value;
                    config1.OAuth2RefreshToken = refreshToken;
                    GoogleAdsClient client1 = new GoogleAdsClient(config1);

                    GoogleAdsServiceClient googleAdsServiceClient1 =
                        client1.GetService(Services.V15.GoogleAdsService);

                    CustomerServiceClient customerServiceClient1 =
                        client1.GetService(Services.V15.CustomerService);

                    // Create a query that retrieves all child accounts of the manager specified in
                    // search calls below.
                    const string query = @"SELECT
                                    customer_client.client_customer,
                                    customer_client.level,
                                    customer_client.manager,
                                    customer_client.descriptive_name,                                                                      
                                    customer_client.id
                                FROM customer_client
                                WHERE
                                    customer_client.level <= 1";

                    // Perform a breadth-first search to build a Dictionary that maps managers to their
                    // child accounts.
                    Dictionary<long, List<CustomerClient>> customerIdsToChildAccounts =
                        new Dictionary<long, List<CustomerClient>>();

                    foreach (long seedCustomerId in seedCustomerIds)
                    {
                        try
                        {
                            Queue<long> unprocessedCustomerIds = new Queue<long>();
                            unprocessedCustomerIds.Enqueue(seedCustomerId);
                            CustomerClient rootCustomerClient = null;

                            while (unprocessedCustomerIds.Count > 0)
                            {
                                managerCustomerId = unprocessedCustomerIds.Dequeue();
                                PagedEnumerable<SearchGoogleAdsResponse, GoogleAdsRow> response =
                                    googleAdsServiceClient.Search(
                                        managerCustomerId.ToString(),
                                        query
                                    );

                                // Iterate over all rows in all pages to get all customer clients under the
                                // specified customer's hierarchy.
                                foreach (GoogleAdsRow googleAdsRow in response)
                                {
                                    CustomerClient customerClient = googleAdsRow.CustomerClient;

                                    // The customer client that with level 0 is the specified customer.
                                    if (customerClient.Level == 0)
                                    {
                                        if (rootCustomerClient == null)
                                        {
                                            rootCustomerClient = customerClient;
                                            if (rootCustomerClient.Manager == false)
                                            {
                                                GoogleAdsCustomerDto gaAdsCustomer = new GoogleAdsCustomerDto();
                                                gaAdsCustomer.CustomerId = rootCustomerClient.Id;
                                                gaAdsCustomer.Name = string.IsNullOrEmpty(rootCustomerClient.DescriptiveName) ? "Google Ads account" : rootCustomerClient.DescriptiveName;

                                                if (customerClient.HasClientCustomer && !string.IsNullOrEmpty(customerClient.ResourceName))
                                                {
                                                    var fullResourceName = customerClient.ResourceName.Split("/");

                                                    gaAdsCustomer.LoginCustomerId = fullResourceName[1];
                                                }
                                                else
                                                {
                                                    gaAdsCustomer.LoginCustomerId = "0";
                                                }

                                                var isExist = gaAdsCustomerList.Exists(x => x.CustomerId == rootCustomerClient.Id);
                                                if (!isExist)
                                                {
                                                    gaAdsCustomerList.Add(gaAdsCustomer);
                                                }

                                            }
                                        }
                                        continue;
                                    }

                                    // For all level-1 (direct child) accounts that are a manager account,
                                    // the above query will be run against them to create a Dictionary of
                                    // managers mapped to their child accounts for printing the hierarchy
                                    // afterwards.
                                    if (!customerIdsToChildAccounts.ContainsKey(managerCustomerId.Value))

                                        customerIdsToChildAccounts.Add(managerCustomerId.Value, new List<CustomerClient>());
                                    customerIdsToChildAccounts[managerCustomerId.Value].Add(customerClient);

                                    if (customerClient.Manager == false)
                                    {
                                        GoogleAdsCustomerDto gaAdsCustomer = new GoogleAdsCustomerDto();
                                        gaAdsCustomer.CustomerId = customerClient.Id;
                                        gaAdsCustomer.Name = string.IsNullOrEmpty(customerClient.DescriptiveName) ? "Google Ads account" : customerClient.DescriptiveName;

                                        if (customerClient.HasClientCustomer && !string.IsNullOrEmpty(customerClient.ResourceName))
                                        {
                                            var fullResourceName = customerClient.ResourceName.Split("/");

                                            gaAdsCustomer.LoginCustomerId = fullResourceName[1];
                                        }
                                        else
                                        {
                                            gaAdsCustomer.LoginCustomerId = "0";
                                        }
                                        var isExist = gaAdsCustomerList.Exists(x => x.CustomerId == customerClient.Id);
                                        if (!isExist)
                                        {
                                            gaAdsCustomerList.Add(gaAdsCustomer);
                                        }
                                    }

                                    if (customerClient.Manager)
                                        // A customer can be managed by multiple managers, so to prevent
                                        // visiting the same customer many times, we need to check if it's
                                        // already in the Dictionary.
                                        if (!customerIdsToChildAccounts.ContainsKey(customerClient.Id) &&
                                            customerClient.Level == 1 && customerClient.Manager == false)
                                            unprocessedCustomerIds.Enqueue(customerClient.Id);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            var test = ex;
                        }
                    }
                }

            }
            catch(Exception ex)
            {
                return gaAdsCustomerList;
            }


            return gaAdsCustomerList;
        }


        public bool IsPropertiesExists(string refresh_token)
        {
            List<GoogleAdsCustomerDto> gaAdsCustomerList = new List<GoogleAdsCustomerDto>();

            try
            {
               
                if (!string.IsNullOrEmpty(refresh_token))
                {
                    GoogleAdsConfig config = new GoogleAdsConfig();
                    config.OAuth2ClientId = _configuration.GetSection("ClientIdForGoogleAds").Value;
                    config.OAuth2ClientSecret = _configuration.GetSection("ClientSecretForGoogleAds").Value;
                    config.OAuth2Scope = "https://www.googleapis.com/auth/adwords";
                    config.DeveloperToken = _configuration.GetSection("DeveloperTokenForGoogleAds").Value;
                    config.OAuth2RefreshToken = refresh_token;
                    GoogleAdsClient client = new GoogleAdsClient(config);

                    GoogleAdsServiceClient googleAdsServiceClient =
                        client.GetService(Services.V15.GoogleAdsService);

                    CustomerServiceClient customerServiceClient =
                        client.GetService(Services.V15.CustomerService);

                    string[] customerResourceNames = customerServiceClient.ListAccessibleCustomers();


                    List<long> seedCustomerIds = new List<long>();
                    foreach (string customerResourceName1 in customerResourceNames)
                    {
                        CustomerName customerName1 = CustomerName.Parse(customerResourceName1);
                        seedCustomerIds.Add(long.Parse(customerName1.CustomerId));
                    }

                    long? managerCustomerId = null;
                    GoogleAdsConfig config1 = new GoogleAdsConfig();
                    config1.OAuth2ClientId = _configuration.GetSection("ClientIdForGoogleAds").Value;
                    config1.OAuth2ClientSecret = _configuration.GetSection("ClientSecretForGoogleAds").Value;
                    config1.OAuth2Scope = "https://www.googleapis.com/auth/adwords";
                    config1.DeveloperToken = _configuration.GetSection("DeveloperTokenForGoogleAds").Value;
                    config1.OAuth2RefreshToken = refresh_token;
                    GoogleAdsClient client1 = new GoogleAdsClient(config1);

                    GoogleAdsServiceClient googleAdsServiceClient1 =
                        client1.GetService(Services.V15.GoogleAdsService);

                    CustomerServiceClient customerServiceClient1 =
                        client1.GetService(Services.V15.CustomerService);

                    // Create a query that retrieves all child accounts of the manager specified in
                    // search calls below.
                    const string query = @"SELECT
                                    customer_client.client_customer,
                                    customer_client.level,
                                    customer_client.manager,
                                    customer_client.descriptive_name,                                                                      
                                    customer_client.id
                                FROM customer_client
                                WHERE
                                    customer_client.level <= 1";

                    // Perform a breadth-first search to build a Dictionary that maps managers to their
                    // child accounts.
                    Dictionary<long, List<CustomerClient>> customerIdsToChildAccounts =
                        new Dictionary<long, List<CustomerClient>>();

                    foreach (long seedCustomerId in seedCustomerIds)
                    {
                        try
                        {
                            Queue<long> unprocessedCustomerIds = new Queue<long>();
                            unprocessedCustomerIds.Enqueue(seedCustomerId);
                            CustomerClient rootCustomerClient = null;

                            while (unprocessedCustomerIds.Count > 0)
                            {
                                managerCustomerId = unprocessedCustomerIds.Dequeue();
                                PagedEnumerable<SearchGoogleAdsResponse, GoogleAdsRow> response =
                                    googleAdsServiceClient.Search(
                                        managerCustomerId.ToString(),
                                        query
                                    );

                                // Iterate over all rows in all pages to get all customer clients under the
                                // specified customer's hierarchy.
                                foreach (GoogleAdsRow googleAdsRow in response)
                                {
                                    CustomerClient customerClient = googleAdsRow.CustomerClient;

                                    // The customer client that with level 0 is the specified customer.
                                    if (customerClient.Level == 0)
                                    {
                                        if (rootCustomerClient == null)
                                        {
                                            rootCustomerClient = customerClient;
                                            if (rootCustomerClient.Manager == false)
                                            {
                                                GoogleAdsCustomerDto gaAdsCustomer = new GoogleAdsCustomerDto();
                                                gaAdsCustomer.CustomerId = rootCustomerClient.Id;
                                                gaAdsCustomer.Name = string.IsNullOrEmpty(rootCustomerClient.DescriptiveName) ? "Google Ads account" : rootCustomerClient.DescriptiveName;

                                                if (customerClient.HasClientCustomer && !string.IsNullOrEmpty(customerClient.ResourceName))
                                                {
                                                    var fullResourceName = customerClient.ResourceName.Split("/");

                                                    gaAdsCustomer.LoginCustomerId = fullResourceName[1];
                                                }
                                                else
                                                {
                                                    gaAdsCustomer.LoginCustomerId = "0";
                                                }

                                                var isExist = gaAdsCustomerList.Exists(x => x.CustomerId == rootCustomerClient.Id);
                                                if (!isExist)
                                                {
                                                    gaAdsCustomerList.Add(gaAdsCustomer);
                                                }

                                            }
                                        }
                                        continue;
                                    }

                                    // For all level-1 (direct child) accounts that are a manager account,
                                    // the above query will be run against them to create a Dictionary of
                                    // managers mapped to their child accounts for printing the hierarchy
                                    // afterwards.
                                    if (!customerIdsToChildAccounts.ContainsKey(managerCustomerId.Value))

                                        customerIdsToChildAccounts.Add(managerCustomerId.Value, new List<CustomerClient>());
                                    customerIdsToChildAccounts[managerCustomerId.Value].Add(customerClient);

                                    if (customerClient.Manager == false)
                                    {
                                        GoogleAdsCustomerDto gaAdsCustomer = new GoogleAdsCustomerDto();
                                        gaAdsCustomer.CustomerId = customerClient.Id;
                                        gaAdsCustomer.Name = string.IsNullOrEmpty(customerClient.DescriptiveName) ? "Google Ads account" : customerClient.DescriptiveName;
                                        if (customerClient.HasClientCustomer && !string.IsNullOrEmpty(customerClient.ResourceName))
                                        {
                                            var fullResourceName = customerClient.ResourceName.Split("/");

                                            gaAdsCustomer.LoginCustomerId = fullResourceName[1];
                                        }
                                        else
                                        {
                                            gaAdsCustomer.LoginCustomerId = "0";
                                        }
                                        var isExist = gaAdsCustomerList.Exists(x => x.CustomerId == customerClient.Id);
                                        if (!isExist)
                                        {
                                            gaAdsCustomerList.Add(gaAdsCustomer);
                                        }
                                    }

                                    if (customerClient.Manager)
                                        // A customer can be managed by multiple managers, so to prevent
                                        // visiting the same customer many times, we need to check if it's
                                        // already in the Dictionary.
                                        if (!customerIdsToChildAccounts.ContainsKey(customerClient.Id) &&
                                            customerClient.Level == 1 && customerClient.Manager == false)
                                            unprocessedCustomerIds.Enqueue(customerClient.Id);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            var test = ex;
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }




        /// <summary>
        /// Get Google Ads data
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <param name="reportType">reportType</param>
        /// <returns>reports data</returns>
        public List<GoogleAdsCampaignReport> GetGoogleAdsReports(string campaignId, string startDate, string endDate, int reportType)
        {
            List<GoogleAdsCampaignReport> ListOfGoogleAdsRow = new List<GoogleAdsCampaignReport>();
            var camid = new Guid(campaignId);
            var gaAdsSetup = _campaigngoogleadsRepository.GetAllEntities(true).Where(x => x.CampaignID == camid).Select(ga => new CampaignGoogleAdsDto
            {
                Id = ga.Id,
                RefreshToken = ga.RefreshToken,
                Name = ga.Name,
                CustomerId = ga.CustomerId,
                LoginCustomerID = ga.LoginCustomerID

            }).FirstOrDefault();
            if (gaAdsSetup != null)
            {
                GoogleAdsConfig config1 = new GoogleAdsConfig();
                config1.OAuth2ClientId = _configuration.GetSection("ClientIdForGoogleAds").Value;
                config1.OAuth2ClientSecret = _configuration.GetSection("ClientSecretForGoogleAds").Value;
                config1.OAuth2Scope = "https://www.googleapis.com/auth/adwords";
                config1.DeveloperToken = _configuration.GetSection("DeveloperTokenForGoogleAds").Value;
                config1.OAuth2RefreshToken = gaAdsSetup.RefreshToken;
                config1.LoginCustomerId = gaAdsSetup.LoginCustomerID;
                //Get the GoogleAdsService.
                GoogleAdsClient client2 = new GoogleAdsClient(config1);
                GoogleAdsServiceClient googleAdsService = client2.GetService(
                    Services.V15.GoogleAdsService);

                // Create the query.
                var query = PrepareQuery(startDate, endDate, reportType);

                googleAdsService.SearchStream(gaAdsSetup.CustomerId, query, delegate (SearchGoogleAdsStreamResponse resp)
                {
                    if (resp.Results.Count > 0)
                    {
                        var currencyCode = new List<Currency>();
                        var restClient = new RestClient(_configuration.GetSection("BlobUrl").Value + "Json");
                        var restRequest = new RestRequest("/currency_code.json", Method.Get);

                        var response = restClient.GetAsync(restRequest).Result;
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            currencyCode = JsonConvert.DeserializeObject<List<Currency>>(response.Content);
                        }

                        var currency_symbol = currencyCode.Where(y => y.code == resp.Results[0].Customer.CurrencyCode).Select(x => x.symbol).FirstOrDefault();


                        // Display the results.
                        foreach (GoogleAdsRow criterionRow in resp.Results)
                        {
                            GoogleAdsCampaignReport googleAdsCampaignReport = new GoogleAdsCampaignReport();
                            googleAdsCampaignReport.Date = criterionRow.Segments.Date;
                            googleAdsCampaignReport.Currency = currency_symbol;

                            switch ((ReportTypes)reportType)
                            {
                                case ReportTypes.GoogleAdsCampaign:
                                    {
                                        googleAdsCampaignReport.Name = criterionRow.Campaign.Name;
                                        if (criterionRow.Campaign != null && !string.IsNullOrEmpty(criterionRow.Campaign.ResourceName))
                                        {
                                            string[] segments = criterionRow.Campaign.ResourceName.Split('/');

                                            int campaignIndex = Array.IndexOf(segments, "campaigns");
                                            if (campaignIndex != -1 && campaignIndex + 1 < segments.Length)
                                            {
                                                string campaign_id = segments[campaignIndex + 1];
                                                googleAdsCampaignReport.CampaignId = campaign_id;
                                                //Console.WriteLine("adGroups value: " + adGroupsValue);
                                            }
                                            else
                                            {
                                                //Console.WriteLine("adGroups segment not found or value is missing.");
                                            }
                                        }
                                        break;
                                    }
                                case ReportTypes.GoogleAdsGroups:
                                    {
                                        googleAdsCampaignReport.Name = criterionRow.AdGroup.Name;

                                        if (criterionRow.AdGroup != null && !string.IsNullOrEmpty(criterionRow.AdGroup.ResourceName))
                                        {
                                            string[] segments = criterionRow.AdGroup.ResourceName.Split('/');

                                            int adGroupsIndex = Array.IndexOf(segments, "adGroups");
                                            if (adGroupsIndex != -1 && adGroupsIndex + 1 < segments.Length)
                                            {
                                                string adGroupsValue = segments[adGroupsIndex + 1];
                                                googleAdsCampaignReport.AdGroupId = adGroupsValue;
                                                //Console.WriteLine("adGroups value: " + adGroupsValue);
                                            }
                                            else
                                            {
                                                //Console.WriteLine("adGroups segment not found or value is missing.");
                                            }
                                        }
                                                                               
                                        break;
                                    }
                                case ReportTypes.GoogleAdsCopies:
                                    {
                                        if (criterionRow.AdGroupAd.Ad != null && !string.IsNullOrEmpty(criterionRow.AdGroupAd.Ad.ResourceName))
                                        {
                                            string[] segments = criterionRow.AdGroupAd.Ad.ResourceName.Split('/');

                                            int adIndex = Array.IndexOf(segments, "ads");
                                            if (adIndex != -1 && adIndex + 1 < segments.Length)
                                            {
                                                string ad_id = segments[adIndex + 1];
                                                googleAdsCampaignReport.AdId = ad_id;
                                                //Console.WriteLine("adGroups value: " + adGroupsValue);
                                            }
                                            else
                                            {
                                                //Console.WriteLine("adGroups segment not found or value is missing.");
                                            }
                                        }

                                        if (criterionRow.AdGroupAd.Ad.ExpandedTextAd != null)
                                        {
                                            googleAdsCampaignReport.Name = criterionRow.AdGroupAd.Ad.ExpandedTextAd.HeadlinePart1 + " " +
                                            criterionRow.AdGroupAd.Ad.ExpandedTextAd.HeadlinePart2 + " " +
                                            criterionRow.AdGroupAd.Ad.ExpandedTextAd.HeadlinePart3;

                                            googleAdsCampaignReport.Url = criterionRow.AdGroupAd.Ad.FinalUrls.ToString();

                                            googleAdsCampaignReport.Description = criterionRow.AdGroupAd.Ad.ExpandedTextAd.Description + " " +
                                            criterionRow.AdGroupAd.Ad.ExpandedTextAd.Description2;
                                        }
                                        else if (criterionRow.AdGroupAd.Ad.ResponsiveDisplayAd != null){

                                            var rr = criterionRow.AdGroupAd.Ad.ResponsiveDisplayAd.Headlines.Select(x=>x.Text).ToList();
                                            var des = criterionRow.AdGroupAd.Ad.ResponsiveDisplayAd.Descriptions.Select(x => x.Text).ToList();
                                            var head = string.Join(" | ",rr);
                                            var description2 = string.Join(" | ", des);

                                            googleAdsCampaignReport.Name = head;

                                            googleAdsCampaignReport.Url = criterionRow.AdGroupAd.Ad.FinalUrls.ToString();

                                            googleAdsCampaignReport.Description = description2;
                                        }

                                        break;
                                    }
                                default:
                                    {
                                        break;
                                    }
                            }

                            googleAdsCampaignReport.Clicks = criterionRow.Metrics.Clicks;
                            googleAdsCampaignReport.Impressions = criterionRow.Metrics.Impressions;
                            googleAdsCampaignReport.ViewThroughConversions = criterionRow.Metrics.ViewThroughConversions;
                            googleAdsCampaignReport.Conversation = criterionRow.Metrics.Conversions;
                            googleAdsCampaignReport.ConversationRate = criterionRow.Metrics.ConversionsFromInteractionsRate;
                            googleAdsCampaignReport.Cost = criterionRow.Metrics.CostMicros;
                            googleAdsCampaignReport.Avg_CPC = criterionRow.Metrics.AverageCpc;
                            googleAdsCampaignReport.Interaction = criterionRow.Metrics.Interactions.ToString();
                             
                            ListOfGoogleAdsRow.Add(googleAdsCampaignReport);
                        }
                    }
                    
                });
            }           

            return ListOfGoogleAdsRow;
        }

        /// <summary>
        /// Prepare query
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="reportType"></param>
        /// <returns></returns>
        public string PrepareQuery(string startDate, string endDate, int reportType)
        {
            var query = string.Empty;
            switch ((ReportTypes)reportType)
            {
                case ReportTypes.GoogleAdsCampaign:
                    {
                        query =
                                   @"SELECT
                                          campaign.name,                      
                                          segments.date,
                                          customer.currency_code,                 
                                          metrics.view_through_conversions,
                                          metrics.clicks,
                                          metrics.impressions,
                                          metrics.conversions,                                          
                                          metrics.cost_micros,
                                          metrics.average_cpc,
                                          metrics.interactions
                                                                           
                                          FROM campaign WHERE segments.date BETWEEN '" + startDate + "' AND  '"
                                          + endDate + "' ";
                        break;
                    }
                case ReportTypes.GoogleAdsGroups:
                    {
                        query =
                                    @"SELECT
                                          ad_group.name,                      
                                          segments.date,
                                          customer.currency_code,
                                          metrics.view_through_conversions,
                                          metrics.clicks,
                                          metrics.impressions,
                                          metrics.conversions,
                                          metrics.cost_micros,
                                          metrics.average_cpc,
                                          metrics.interactions
                                          
                                          FROM ad_group WHERE segments.date BETWEEN '" + startDate + "' AND  '"
                                          + endDate + "' ";
                        break;
                    }

                case ReportTypes.GoogleAdsCopies:
                    {
                        query =
                                   @"SELECT                                         
                                        ad_group_ad.ad.app_ad.descriptions,
                                        ad_group_ad.ad.app_ad.headlines,
                                        ad_group_ad.ad.app_engagement_ad.descriptions,
                                        ad_group_ad.ad.app_engagement_ad.headlines,
                                        ad_group_ad.ad.call_ad.description1,
                                        ad_group_ad.ad.call_ad.description2,
                                        ad_group_ad.ad.call_ad.headline1,
                                        ad_group_ad.ad.call_ad.headline2,
                                        ad_group_ad.ad.call_ad.path1,
                                        ad_group_ad.ad.call_ad.path2,
                                        ad_group_ad.ad.expanded_dynamic_search_ad.description,
                                        ad_group_ad.ad.expanded_dynamic_search_ad.description2,
                                        ad_group_ad.ad.expanded_text_ad.description,
                                        ad_group_ad.ad.expanded_text_ad.description2,
                                        ad_group_ad.ad.expanded_text_ad.headline_part1,
                                        ad_group_ad.ad.expanded_text_ad.headline_part2,
                                        ad_group_ad.ad.expanded_text_ad.headline_part3,
                                        ad_group_ad.ad.expanded_text_ad.path1,
                                        ad_group_ad.ad.expanded_text_ad.path2,
                                        ad_group_ad.ad.final_urls,
                                        ad_group_ad.ad.local_ad.call_to_actions,
                                        ad_group_ad.ad.local_ad.descriptions,
                                        ad_group_ad.ad.local_ad.headlines,
                                        ad_group_ad.ad.local_ad.path1,
                                        ad_group_ad.ad.local_ad.path2,
                                        ad_group_ad.ad.responsive_display_ad.descriptions,
                                        ad_group_ad.ad.responsive_display_ad.headlines,
                                        ad_group_ad.ad.responsive_display_ad.long_headline,
                                        ad_group_ad.ad.responsive_search_ad.descriptions,
                                        ad_group_ad.ad.responsive_search_ad.headlines,
                                        ad_group_ad.ad.responsive_search_ad.path1,
                                        ad_group_ad.ad.responsive_search_ad.path2,
                                        ad_group_ad.ad.shopping_comparison_listing_ad.headline,
                                        ad_group_ad.ad.shopping_product_ad,
                                        ad_group_ad.ad.shopping_smart_ad,
                                        ad_group_ad.ad.smart_campaign_ad.descriptions,
                                        ad_group_ad.ad.smart_campaign_ad.headlines,
                                        ad_group_ad.ad.system_managed_resource_source,
                                        ad_group_ad.ad.text_ad.description1,
                                        ad_group_ad.ad.text_ad.description2,
                                        ad_group_ad.ad.text_ad.headline,
                                        ad_group_ad.ad_group,
                                          
                                          segments.date,
                                          customer.currency_code,

                                          metrics.view_through_conversions,
                                          metrics.clicks,
                                          metrics.impressions,
                                          metrics.conversions,                                         
                                          metrics.cost_micros,
                                          metrics.average_cpc,
                                          metrics.interactions
  
                                          FROM ad_group_ad WHERE segments.date BETWEEN '" + startDate + "' AND  '"
                                          + endDate + "' ";
                        break;
                    }


                default:
                    {
                        break;
                    }
            }

            return query;
        }

        /// <summary>
        /// Update previously access token, refresh token if email id same in same company 
        /// </summary>
        /// <param name="campaignAds">campaignAds</param>
        /// <param name="companyId">companyId</param>
        /// <returns>bool</returns>
        public async Task<bool> UpdateRefreshTokenAndEmail(CampaignGoogleAdsForCreation campaignAds, string companyId)
        {
            var campaignIds = _campaignRepository.GetFilteredEntities(true).Where(x => x.CompanyID == new Guid(companyId)).Select(x => x.Id).ToList();
            campaignIds.Remove(campaignAds.CampaignID);

            foreach (var campaignId in campaignIds)
            {
                var campaignGa = _campaigngoogleadsRepository.GetFilteredEntities(true).Where(x => x.EmailId == campaignAds.EmailId && x.CampaignID == campaignId).FirstOrDefault();
                if (campaignGa != null)
                {
                    campaignGa.RefreshToken = campaignAds.RefreshToken;
                    campaignGa.AccessToken = campaignAds.AccessToken;
                    _campaigngoogleadsRepository.UpdateEntity(campaignGa);
                    var response = await _campaigngoogleadsRepository.SaveChangesAsync();
                }
            }
            return true;
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
            return "Id, Name, CampaignID, IsActive, Campaign, AccessToken,CustomerId, EmailId, LoginCustomerID";
        }

        #endregion
    }
}
