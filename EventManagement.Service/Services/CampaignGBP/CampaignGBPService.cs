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
using MailChimp.Net.Models;
using System.Xml.Linq;
using Method = RestSharp.Method;
using Amazon.S3.Model;
using Grpc.Core;
using System.Net.Http;
using System.Text;
using IdentityServer4.Models;

namespace EventManagement.Service
{
    public class CampaignGBPService : ServiceBase<CampaignGBP, Guid>, ICampaignGBPService
    {

        #region PRIVATE MEMBERS

        private readonly ICampaignGBPRepository _campaigngbpRepository;
        private readonly IConfiguration _configuration;

        #endregion


        #region CONSTRUCTOR

        public CampaignGBPService(ICampaignGBPRepository campaigngbpRepository,
            ILogger<CampaignGBPService> logger, IConfiguration configuration
            ) : base(campaigngbpRepository, logger)
        {
            _campaigngbpRepository = campaigngbpRepository;
            _configuration = configuration;          
        }

        #endregion


        #region PUBLIC MEMBERS   

        public async Task<List<GbpLocation>> GetLocationList(Guid campaignId)
        {
            var retVal = new List<GbpLocation>();
            var campaign = _campaigngbpRepository.GetAllEntities(true).Where(x => x.CampaignID == campaignId).FirstOrDefault();
            try
            {
                if (campaign != null)
                {
                    string apiUrl = "https://mybusinessaccountmanagement.googleapis.com/v1";

                    // Create RestClient instance
                    var client = new RestClient(apiUrl);

                    // Create RestRequest instance
                    var request = new RestRequest("accounts/", Method.Get);

                    // Add authorization header
                    request.AddHeader("Authorization", $"Bearer {campaign.AccessToken}");

                    // Execute the request
                    var response = await client.ExecuteAsync(request);

                    // Check the response
                    if (response.IsSuccessful && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var accounts = JsonConvert.DeserializeObject<GbpAccounts>(response.Content);
                        var listOfLocations = new List<GbpLocation>();
                        foreach (var account in accounts.accounts)
                        {
                            string id = account.name.Split('/').Last();
                            var locations = await GetLocationById(campaign.AccessToken, id);
                            listOfLocations.AddRange(locations);
                        }

                        retVal = listOfLocations;
                    }
                    else if(response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        var accessToken = await GetAccessTokenUsingRefreshToken(campaign.RefreshToken);

                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            campaign.AccessToken = accessToken;
                            campaign.UpdatedOn = DateTime.UtcNow;
                            _campaigngbpRepository.UpdateEntity(campaign);
                            _campaigngbpRepository.SaveChanges();

                            var res = await GetLocationList(campaign.CampaignID);

                            retVal = res;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                
            }

            return retVal;
        }

        public async Task<string> GetAccessTokenUsingRefreshToken(string refreshToken)
        {
            try
            {
                var clientId = _configuration["ClientIdForGoogleAds"];
                var clientSecret = _configuration["ClientSecretForGoogleAds"];

                var httpClient = new HttpClient
                {
                    BaseAddress = new Uri("https://www.googleapis.com"),
                };

                var data = new
                {
                    client_id = clientId,
                    client_secret = clientSecret,
                    refresh_token = refreshToken,
                    grant_type = "refresh_token",
                    access_type = "offline"
                };

                var stringContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                var resp = await httpClient.PostAsync("/oauth2/v4/token", stringContent);
                var gaResponse = JsonConvert.DeserializeObject<GaToken>(await resp.Content.ReadAsStringAsync());
                return gaResponse.access_token;

            }
            catch (Exception ex)
            {
                var test = ex;
            }
            return "";
        }


        public async Task<RootGbpData> GetGbpPerformanceData(Guid campaignId, string startDate, string endDate)
        {
            var retVal = new RootGbpData();
            bool isMonthly = false;
            var campaign = _campaigngbpRepository.GetAllEntities(true).Where(x => x.CampaignID == campaignId).FirstOrDefault();

            DateTime startDateObj = DateTime.Parse(startDate);
            DateTime endDateObj = DateTime.Parse(endDate);

            isMonthly = (endDateObj - startDateObj).TotalDays > 30;

            List<string> formattedDates = GenerateFormattedDateList(startDateObj, endDateObj, isMonthly);

            //isMonthly = formattedDates.Count > 30 ? true : false;
           
            //current date
            retVal = await GetDataFromGbpApi(campaign, startDate, endDate,isMonthly);
            if (retVal.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var accessToken = await GetAccessTokenUsingRefreshToken(campaign.RefreshToken);

                if (!string.IsNullOrEmpty(accessToken))
                {
                    campaign.AccessToken = accessToken;
                    campaign.UpdatedOn = DateTime.UtcNow;
                    _campaigngbpRepository.UpdateEntity(campaign);
                    _campaigngbpRepository.SaveChanges();

                    retVal = await GetDataFromGbpApi(campaign, startDate, endDate, isMonthly);
                }               
            }

            
            retVal.DateLabels = formattedDates;
                                
            var previousDate = await CalculatePreviousDate(startDate, endDate);

            DateTime prevStartDateObj = DateTime.Parse(previousDate[0]);
            DateTime prevEndDateObj = DateTime.Parse(previousDate[1]);

            retVal.VsDateRange = "(vs "+ prevStartDateObj.ToString("MMM yyyy") + "-" + prevEndDateObj.ToString("MMM yyyy") + ")";
            //previous date
            var prevResult = await GetDataFromGbpApi(campaign, previousDate[0], previousDate[1], isMonthly);
            if (retVal.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var accessToken = await GetAccessTokenUsingRefreshToken(campaign.RefreshToken);

                if (!string.IsNullOrEmpty(accessToken))
                {
                    campaign.AccessToken = accessToken;
                    campaign.UpdatedOn = DateTime.UtcNow;
                    _campaigngbpRepository.UpdateEntity(campaign);
                    _campaigngbpRepository.SaveChanges();

                    prevResult = await GetDataFromGbpApi(campaign, previousDate[0], previousDate[1], isMonthly);
                }
            }

            //Previos Card
            retVal.PrevTotalCalls = prevResult.TotalCalls;
            retVal.PrevTotalInteraction = prevResult.TotalInteraction;
            retVal.PrevTotalWebsiteClick = prevResult.TotalWebsiteClick;
            retVal.PrevTotalDirections = prevResult.TotalDirections;
            retVal.PrevTotalBooking  = prevResult.TotalBooking;
            retVal.PrevTotalDirections = prevResult.TotalDirections;
            retVal.PrevTotalMessage = prevResult.TotalMessage;
            retVal.PrevTotalProfileView = prevResult.TotalProfileView;
            //retVal.PrevTotalSearchKeyword = prevResult.TotalSearchKeyword;

            //retVal.PrevPercentGoogleMapMobile = prevResult.PercentGoogleMapMobile;
            //retVal.PrevPercentGoogleMapDesktop = prevResult.PercentGoogleMapDesktop;
            //retVal.PrevPercentGoogleSearchDesktop = prevResult.PercentGoogleSearchDesktop;
            //retVal.PrevPercentGoogleSearchMobile = prevResult.PercentGoogleSearchMobile;

            //retVal.PrevBookingChartData = prevResult.BookingChartData;
            //retVal.PrevCallChartData = prevResult.CallChartData;
            //retVal.PrevDirectionChartData = prevResult.DirectionChartData;
            //retVal.PrevInteractionChartData = prevResult.InteractionChartData;
            //retVal.PrevWebsiteChartData = prevResult.WebsiteChartData;
            //retVal.PrevMessageChartData = prevResult.MessageChartData;
            

            var keywordData = await KeywordData(campaignId, startDate, endDate);
            if (keywordData.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var accessToken = await GetAccessTokenUsingRefreshToken(campaign.RefreshToken);

                if (!string.IsNullOrEmpty(accessToken))
                {
                    campaign.AccessToken = accessToken;
                    campaign.UpdatedOn = DateTime.UtcNow;
                    _campaigngbpRepository.UpdateEntity(campaign);
                    _campaigngbpRepository.SaveChanges();

                     keywordData = await KeywordData(campaignId, startDate, endDate);                    
                }
            }

            //PreviousDate Keyword
            var preKeywordData = await KeywordData(campaignId, previousDate[0], previousDate[1]);
            if (preKeywordData.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var accessToken = await GetAccessTokenUsingRefreshToken(campaign.RefreshToken);

                if (!string.IsNullOrEmpty(accessToken))
                {
                    campaign.AccessToken = accessToken;
                    campaign.UpdatedOn = DateTime.UtcNow;
                    _campaigngbpRepository.UpdateEntity(campaign);
                    _campaigngbpRepository.SaveChanges();

                    preKeywordData = await KeywordData(campaignId, previousDate[0], previousDate[1]);
                }
            }

            retVal.KeywordData = keywordData.searchKeywordsCounts != null && keywordData.searchKeywordsCounts.Count() > 0 ? keywordData.searchKeywordsCounts.Take(5).ToList() : new List<SearchKeywordsCount>();

            retVal.TotalSearchKeyword = keywordData.searchKeywordsCounts != null && keywordData.searchKeywordsCounts.Count() > 0 ? keywordData.searchKeywordsCounts.Sum(x => Convert.ToInt32(x.insightsValue.value)) : 0;
            retVal.PrevTotalSearchKeyword = preKeywordData.searchKeywordsCounts != null && preKeywordData.searchKeywordsCounts.Count() > 0 ? preKeywordData.searchKeywordsCounts.Sum(x => Convert.ToInt32(x.insightsValue.value)) : 0;

            retVal.CallCardPercent = CalculateCardPercentage(retVal.TotalCalls, retVal.PrevTotalCalls);
            retVal.InteractionCardPercent = CalculateCardPercentage(retVal.TotalInteraction, retVal.PrevTotalInteraction);
            retVal.DirectionCardPercent = CalculateCardPercentage(retVal.TotalDirections, retVal.PrevTotalDirections);
            retVal.WebsiteCardPercent = CalculateCardPercentage(retVal.TotalWebsiteClick, retVal.PrevTotalWebsiteClick);

            retVal.MessageDiff = PrepareDifference(retVal.TotalMessage,retVal.PrevTotalMessage);
            retVal.CallDiff = PrepareDifference(retVal.TotalCalls, retVal.PrevTotalCalls);
            retVal.WebsiteDiff = PrepareDifference(retVal.TotalWebsiteClick, retVal.PrevTotalWebsiteClick);
            retVal.DirectionDiff = PrepareDifference(retVal.TotalDirections, retVal.PrevTotalDirections);
            retVal.ProfileInteractionDiff = PrepareDifference(retVal.TotalInteraction, retVal.PrevTotalInteraction);
            retVal.BookingDiff = PrepareDifference(retVal.TotalBooking, retVal.PrevTotalBooking);
            retVal.ProfileViewDiff = PrepareDifference(retVal.TotalProfileView, retVal.PrevTotalProfileView);
            retVal.SearchKeywordDiff = PrepareDifference(retVal.TotalSearchKeyword, retVal.PrevTotalSearchKeyword);

            return retVal;
        }

        public List<string> GenerateFormattedDateList(DateTime startDate, DateTime endDate, bool isMonthly)
        {
            List<string> formattedDates = new List<string>();

            while (startDate <= endDate)
            {
                if (isMonthly)
                {
                    formattedDates.Add(startDate.ToString("MMM yyyy"));
                    startDate = startDate.AddMonths(1);
                }
                else
                {
                    formattedDates.Add(startDate.ToString("MM-dd"));
                    startDate = startDate.AddDays(1);
                }
            }

            return formattedDates;
        }

        private string CalculateCardPercentage(int currentValue, int previousValue)
        {
            if (previousValue > 0)
            {
                decimal currentValueDecimal = Convert.ToDecimal(currentValue);
                decimal previousValueDecimal = Convert.ToDecimal(previousValue);

                decimal percentageChange = ((currentValueDecimal - previousValueDecimal) / previousValueDecimal) * 100;

                string changeDirection = (percentageChange < 0) ? "-" : "+";
                decimal absolutePercentageChange = Math.Round(Math.Abs(percentageChange), 2);
                if (changeDirection == "+" )
                {
                    return  absolutePercentageChange + "%";
                }
                else
                {
                    return changeDirection + absolutePercentageChange + "%";
                }
                
            }
            else
            {
                return "0";
            }           
        }

        public string PrepareDifference(int current,int previous)
        {
            int difference = current - previous;
            string sign = difference >= 0 ? "+" : "-";           
            return $"{current}({sign}{Math.Abs(difference)})";
        }

        private async Task<GbpKeywords> KeywordData(Guid campaignId, string startTime, string endTime)
        {
            var retVal = new GbpKeywords();
            string[] parts = startTime.Split('-');

            // Extract the date, month, and year
            int year = int.Parse(parts[0]);
            int month = int.Parse(parts[1]);
           

            // Split the string by '-' delimiter
            string[] parts1 = endTime.Split('-');

            // Extract the date, month, and year
            int yearEnd = int.Parse(parts1[0]);
            int monthEnd = int.Parse(parts1[1]);
          
            var campaign = _campaigngbpRepository.GetAllEntities(true).Where(x => x.CampaignID == campaignId).FirstOrDefault();
            if (campaign != null)
            {
                string baseUrl = "https://businessprofileperformance.googleapis.com";
                string endpoint = "/v1/locations/"+campaign.AccountId;

                string fullUrl = baseUrl + endpoint;

                var client = new RestClient(fullUrl);
                var request = new RestRequest("/searchkeywords/impressions/monthly", Method.Get);

                // Add authorization header
                request.AddHeader("Authorization", $"Bearer {campaign.AccessToken}");


                request.AddParameter("monthlyRange.start_month.year", year,false);
                request.AddParameter("monthlyRange.start_month.month",month, false);
                request.AddParameter("monthlyRange.end_month.year", yearEnd, false);
                request.AddParameter("monthlyRange.end_month.month", monthEnd, false);

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    var list = JsonConvert.DeserializeObject<GbpKeywords>(response.Content);
                    if (list != null && list.searchKeywordsCounts != null  && list.searchKeywordsCounts.Count() > 0)
                    {
                        retVal.searchKeywordsCounts = list.searchKeywordsCounts.ToList();
                        retVal.StatusCode = response.StatusCode;
                    }
                    
                }
                else if(response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    retVal.StatusCode = response.StatusCode;
                    retVal.searchKeywordsCounts = new List<SearchKeywordsCount>();
                }
            }
            return retVal;
        }

        private async Task<List<string>> CalculatePreviousDate(string startDate, string endDate)
        {
            DateTime parsedStartDate = DateTime.Parse(startDate);
            DateTime parsedEndDate = DateTime.Parse(endDate);

            DateTime previousYearStartDate = parsedStartDate.AddYears(-1);
            DateTime previousYearEndDate = parsedEndDate.AddYears(-1);

            var sDate = previousYearStartDate.ToString("yyyy-MM-dd");
            var eDate = previousYearEndDate.ToString("yyyy-MM-dd");

            return new List<string> { sDate, eDate };                
         }

        private async Task<RootGbpData> GetDataFromGbpApi(CampaignGBP campaign, string startTime, string endTime,bool isMonthly)
        {
            var retVal = new RootGbpData();
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

            if (campaign != null)
            {
                
                // Replace with your actual token and URL               
                string apiUrl = "https://businessprofileperformance.googleapis.com/v1/locations/" + campaign.AccountId;

                // Create RestClient instance
                var client = new RestClient(apiUrl);

                // Create RestRequest instance
                var request = new RestRequest(":fetchMultiDailyMetricsTimeSeries", Method.Get);

                // Add authorization header
                request.AddHeader("Authorization", $"Bearer {campaign.AccessToken}");

                // Add query parameters
                request.AddQueryParameter("dailyMetrics", "WEBSITE_CLICKS", false);
                request.AddQueryParameter("dailyMetrics", "CALL_CLICKS", false);
                request.AddQueryParameter("dailyMetrics", "BUSINESS_DIRECTION_REQUESTS", false);
                request.AddQueryParameter("dailyMetrics", "BUSINESS_BOOKINGS", false);
                request.AddQueryParameter("dailyMetrics", "BUSINESS_CONVERSATIONS", false);
                request.AddQueryParameter("dailyMetrics", "BUSINESS_FOOD_ORDERS", false);
                request.AddQueryParameter("dailyMetrics", "BUSINESS_IMPRESSIONS_DESKTOP_SEARCH", false);
                request.AddQueryParameter("dailyMetrics", "BUSINESS_IMPRESSIONS_MOBILE_SEARCH", false);
                request.AddQueryParameter("dailyMetrics", "BUSINESS_IMPRESSIONS_MOBILE_MAPS", false);
                request.AddQueryParameter("dailyMetrics", "BUSINESS_IMPRESSIONS_DESKTOP_MAPS", false);


                request.AddQueryParameter("dailyRange.start_date.year", year, false);
                request.AddQueryParameter("dailyRange.start_date.month", month, false);
                request.AddQueryParameter("dailyRange.start_date.day", day, false);
                request.AddQueryParameter("dailyRange.end_date.year", yearEnd, false);
                request.AddQueryParameter("dailyRange.end_date.month", monthEnd, false);
                request.AddQueryParameter("dailyRange.end_date.day", dayEnd, false);

                // Execute the request
                var response = await client.ExecuteAsync(request);


                // Check the response
                if (response.IsSuccessful)
                {
                    //Card total
                    var totalCalls = 0;
                    var totalDirections = 0;
                    var totalWebsiteClick = 0;
                    var totalFoodOrder = 0;
                    var totalMessage = 0;
                    var totalBooking = 0;

                    //Chart Data
                    int[] callChartData = { };
                    int[] directionChartData = { };
                    int[] websiteChartData = { };
                    int[] bookingChartData = { };
                    int[] messageChartData = { };
                    int[] foodOrderChartData = { };
                    int[] interactionChartData = { };

                    var callChartMonthly = new List<int>();
                    var directionChartMonthly = new List<int>();
                    var websiteChartMonthly = new List<int>();
                    var bookingChartMonthly = new List<int>();
                    var messageChartMonthly = new List<int>();
                    var foodChartMonthly = new List<int>();
                    var interactionChartMonthly = new List<int>();


                    //Pie Chart
                    int totalGoogleSearchMobile = 0;
                    int totalGoogleSearchDesktop = 0;
                    int totalGoogleMapMobile = 0;
                    int totalGoogleMapDesktop = 0;

                    var data = JsonConvert.DeserializeObject<GbpMetricsData>(response.Content);

                    if (data != null && data.multiDailyMetricTimeSeries.Count > 0)
                    {
                        //For total calls
                        // Find the relevant metric data
                        var callClicksData = data.multiDailyMetricTimeSeries
                            .SelectMany(series => series.dailyMetricTimeSeries)
                            .Where(metric => (string)metric.dailyMetric == "CALL_CLICKS")
                            .SelectMany(metric => metric.timeSeries.datedValues)
                            .ToList();

                        var directionData = data.multiDailyMetricTimeSeries
                           .SelectMany(series => series.dailyMetricTimeSeries)
                           .Where(metric => (string)metric.dailyMetric == "BUSINESS_DIRECTION_REQUESTS")
                           .SelectMany(metric => metric.timeSeries.datedValues)
                           .ToList();

                        var websiteClickData = data.multiDailyMetricTimeSeries
                         .SelectMany(series => series.dailyMetricTimeSeries)
                         .Where(metric => (string)metric.dailyMetric == "WEBSITE_CLICKS")
                         .SelectMany(metric => metric.timeSeries.datedValues)
                         .ToList();

                        var messageClickData = data.multiDailyMetricTimeSeries
                        .SelectMany(series => series.dailyMetricTimeSeries)
                        .Where(metric => (string)metric.dailyMetric == "BUSINESS_CONVERSATIONS")
                        .SelectMany(metric => metric.timeSeries.datedValues)
                        .ToList();

                        var bookingClickData = data.multiDailyMetricTimeSeries
                          .SelectMany(series => series.dailyMetricTimeSeries)
                          .Where(metric => (string)metric.dailyMetric == "BUSINESS_BOOKINGS")
                          .SelectMany(metric => metric.timeSeries.datedValues)
                          .ToList();


                        var googleSearchMobile =
                            data.multiDailyMetricTimeSeries
                          .SelectMany(series => series.dailyMetricTimeSeries)
                          .Where(metric => (string)metric.dailyMetric == "BUSINESS_IMPRESSIONS_MOBILE_SEARCH")
                          .SelectMany(metric => metric.timeSeries.datedValues)
                          .ToList();

                        var googleSearchDesktop = data.multiDailyMetricTimeSeries
                          .SelectMany(series => series.dailyMetricTimeSeries)
                          .Where(metric => (string)metric.dailyMetric == "BUSINESS_IMPRESSIONS_DESKTOP_SEARCH")
                          .SelectMany(metric => metric.timeSeries.datedValues)
                          .ToList();

                        var googleMapMobile = data.multiDailyMetricTimeSeries
                          .SelectMany(series => series.dailyMetricTimeSeries)
                          .Where(metric => (string)metric.dailyMetric == "BUSINESS_IMPRESSIONS_MOBILE_MAPS")
                          .SelectMany(metric => metric.timeSeries.datedValues)
                          .ToList();

                        var googleMapDesktop = data.multiDailyMetricTimeSeries
                          .SelectMany(series => series.dailyMetricTimeSeries)
                          .Where(metric => (string)metric.dailyMetric == "BUSINESS_IMPRESSIONS_DESKTOP_MAPS")
                          .SelectMany(metric => metric.timeSeries.datedValues)
                          .ToList();


                        var foodOrderData = data.multiDailyMetricTimeSeries
                         .SelectMany(series => series.dailyMetricTimeSeries)
                         .Where(metric => (string)metric.dailyMetric == "BUSINESS_FOOD_ORDERS")
                         .SelectMany(metric => metric.timeSeries.datedValues)
                         .ToList();

                        // Calculate the sum of the "value" for CALL_CLICKS
                        totalCalls = callClicksData
                                  .Where(datedValue => datedValue.value != null)
                                  .Sum(datedValue => Convert.ToInt32(datedValue.value));

                        totalDirections = directionData
                              .Where(datedValue => datedValue.value != null)
                              .Sum(datedValue => Convert.ToInt32(datedValue.value));

                        totalWebsiteClick = websiteClickData
                              .Where(datedValue => datedValue.value != null)
                              .Sum(datedValue => Convert.ToInt32(datedValue.value));

                        totalFoodOrder = foodOrderData
                             .Where(datedValue => datedValue.value != null)
                             .Sum(datedValue => Convert.ToInt32(datedValue.value));

                        totalMessage = messageClickData
                          .Where(datedValue => datedValue.value != null)
                          .Sum(datedValue => Convert.ToInt32(datedValue.value));

                        totalBooking = bookingClickData
                          .Where(datedValue => datedValue.value != null)
                          .Sum(datedValue => Convert.ToInt32(datedValue.value));

                        //Total interaction of all metrics
                        var totalInteraction = totalCalls + totalDirections + totalWebsiteClick +
                                                totalFoodOrder + totalMessage + totalBooking;

                        if (isMonthly)
                        {
                            //Dictionary<int, Dictionary<int, int>> monthlySumProfileInterClicks = new Dictionary<int, Dictionary<int, int>>();
                            Dictionary<int, Dictionary<int, int>> monthlySumCallClicks = new Dictionary<int, Dictionary<int, int>>();
                            Dictionary<int, Dictionary<int, int>> monthlySumMessageClicks = new Dictionary<int, Dictionary<int, int>>();
                            Dictionary<int, Dictionary<int, int>> monthlySumBookingClicks = new Dictionary<int, Dictionary<int, int>>();
                            Dictionary<int, Dictionary<int, int>> monthlySumDirectionClicks = new Dictionary<int, Dictionary<int, int>>();
                            Dictionary<int, Dictionary<int, int>> monthlySumWebsiteClicks = new Dictionary<int, Dictionary<int, int>>();
                            Dictionary<int, Dictionary<int, int>> monthlySumFoodClicks = new Dictionary<int, Dictionary<int, int>>();


                            foreach (var entry in data.multiDailyMetricTimeSeries)
                            {
                                foreach (var metricTimeSeries in entry.dailyMetricTimeSeries)
                                {
                                    string metric = metricTimeSeries.dailyMetric;

                                    foreach (var datedValue in metricTimeSeries.timeSeries.datedValues)
                                    {
                                        if (datedValue.date.year != 0 && datedValue.date.month != 0 && datedValue.value != null)
                                        {
                                            int year1 = datedValue.date.year;
                                            int month1 = datedValue.date.month;
                                            int value1 = int.Parse(datedValue.value);

                                            Dictionary<int, Dictionary<int, int>> monthlySum;

                                            if (metric == "WEBSITE_CLICKS")
                                            {
                                                monthlySum = monthlySumWebsiteClicks;
                                            }
                                            else if (metric == "CALL_CLICKS")
                                            {
                                                monthlySum = monthlySumCallClicks;
                                            }
                                            else if (metric == "BUSINESS_DIRECTION_REQUESTS")
                                            {
                                                monthlySum = monthlySumDirectionClicks;
                                            }
                                            else if (metric == "BUSINESS_BOOKINGS")
                                            {
                                                monthlySum = monthlySumBookingClicks;
                                            }
                                            else if (metric == "BUSINESS_CONVERSATIONS")
                                            {
                                                monthlySum = monthlySumMessageClicks;
                                            }
                                            else if (metric == "BUSINESS_FOOD_ORDERS")
                                            {
                                                monthlySum = monthlySumFoodClicks;
                                            }
                                            else
                                            {
                                                continue;
                                            }

                                            if (!monthlySum.ContainsKey(year1))
                                            {
                                                monthlySum[year1] = new Dictionary<int, int>();
                                            }

                                            if (!monthlySum[year1].ContainsKey(month1))
                                            {
                                                monthlySum[year1][month1] = 0;
                                            }

                                            monthlySum[year1][month1] += value1;
                                        }
                                    }
                                }
                            }

                            foreach (var year1 in monthlySumWebsiteClicks.Keys)
                            {
                                foreach (var month1 in monthlySumWebsiteClicks[year1].Keys)
                                {
                                    websiteChartMonthly.Add(monthlySumWebsiteClicks[year1][month1]);
                                }
                            }

                            foreach (var year1 in monthlySumCallClicks.Keys)
                            {
                                foreach (var month1 in monthlySumCallClicks[year1].Keys)
                                {
                                    callChartMonthly.Add(monthlySumCallClicks[year1][month1]);                                    
                                }
                            }

                            foreach (var year1 in monthlySumDirectionClicks.Keys)
                            {
                                foreach (var month1 in monthlySumDirectionClicks[year1].Keys)
                                {
                                    directionChartMonthly.Add(monthlySumDirectionClicks[year1][month1]);
                                }
                            }


                            foreach (var year1 in monthlySumBookingClicks.Keys)
                            {
                                foreach (var month1 in monthlySumBookingClicks[year1].Keys)
                                {
                                    bookingChartMonthly.Add(monthlySumBookingClicks[year1][month1]);
                                }
                            }


                            foreach (var year1 in monthlySumMessageClicks.Keys)
                            {
                                foreach (var month1 in monthlySumMessageClicks[year1].Keys)
                                {
                                    messageChartMonthly.Add(monthlySumMessageClicks[year1][month1]);
                                }
                            }

                            foreach (var year1 in monthlySumFoodClicks.Keys)
                            {
                                foreach (var month1 in monthlySumFoodClicks[year1].Keys)
                                {
                                    foodChartMonthly.Add(monthlySumFoodClicks[year1][month1]);
                                }
                            }

                            callChartData = callChartMonthly.ToArray();
                            directionChartData = directionChartMonthly.ToArray();
                            websiteChartData = websiteChartMonthly.ToArray();
                            messageChartData = messageChartMonthly .ToArray();
                            bookingChartData = bookingChartMonthly.ToArray();
                            foodOrderChartData = foodChartMonthly.ToArray();

                            interactionChartData = new int[callChartData.Length];

                            for (int i = 0; i < callChartData.Length; i++)
                            {
                                interactionChartData[i] = callChartData[i] + directionChartData[i] + websiteChartData[i] +
                                           bookingChartData[i] + messageChartData[i] + foodOrderChartData[i];
                            }

                        }
                        else
                        {
                            //Chart data
                            callChartData = callClicksData.Select(x => Convert.ToInt32(x.value)).ToArray();
                            directionChartData = directionData.Select(x => Convert.ToInt32(x.value)).ToArray();
                            websiteChartData = websiteClickData.Select(x => Convert.ToInt32(x.value)).ToArray();
                            messageChartData = messageClickData.Select(x => Convert.ToInt32(x.value)).ToArray();
                            bookingChartData = bookingClickData.Select(x => Convert.ToInt32(x.value)).ToArray();
                            foodOrderChartData = foodOrderData.Select(x => Convert.ToInt32(x.value)).ToArray();

                             interactionChartData = new int[callChartData.Length];

                            for (int i = 0; i < callChartData.Length; i++)
                            {
                                interactionChartData[i] = callChartData[i] + directionChartData[i] + websiteChartData[i] +
                                           bookingChartData[i] + messageChartData[i] + foodOrderChartData[i];
                            }
                        }

                        //Pie Chart Data
                        totalGoogleSearchMobile = googleSearchMobile
                                  .Where(datedValue => datedValue.value != null)
                                  .Sum(datedValue => Convert.ToInt32(datedValue.value));

                        totalGoogleSearchDesktop = googleSearchDesktop
                                 .Where(datedValue => datedValue.value != null)
                                 .Sum(datedValue => Convert.ToInt32(datedValue.value));

                        totalGoogleMapMobile = googleMapMobile
                                 .Where(datedValue => datedValue.value != null)
                                 .Sum(datedValue => Convert.ToInt32(datedValue.value));

                        totalGoogleMapDesktop = googleMapDesktop
                                 .Where(datedValue => datedValue.value != null)
                                 .Sum(datedValue => Convert.ToInt32(datedValue.value));

                        var sumOfPie = totalGoogleSearchMobile + totalGoogleSearchDesktop
                                       + totalGoogleMapMobile + totalGoogleMapDesktop;


                        //Return Card value
                        retVal.TotalCalls = totalCalls;
                        retVal.TotalDirections = totalDirections;
                        retVal.TotalWebsiteClick = totalWebsiteClick;
                        retVal.TotalInteraction = totalInteraction;
                        retVal.TotalFoodOrder = totalFoodOrder;
                        retVal.TotalMessage =  totalMessage;
                        retVal.TotalBooking = totalBooking;
                        retVal.TotalProfileView = sumOfPie;
                        retVal.TotalSearchMobile = totalGoogleSearchMobile;
                        retVal.TotalSearchDesktop = totalGoogleSearchDesktop;
                        retVal.TotalMapMobile = totalGoogleMapMobile;
                        retVal.TotalMapDesktop = totalGoogleMapDesktop;

                        var percentGoogleSearchMobile =  CalculatePercentage(sumOfPie, totalGoogleSearchMobile);
                        var percentGoogleSearchDesktop =  CalculatePercentage(sumOfPie, totalGoogleSearchDesktop);
                        var percentGoogleMapMobile =  CalculatePercentage(sumOfPie, totalGoogleMapMobile);
                        var percentGoogleMapDesktop =  CalculatePercentage(sumOfPie, totalGoogleMapDesktop);

                        //Return ChartData
                        retVal.CallChartData = callChartData;
                        retVal.DirectionChartData = directionChartData;
                        retVal.MessageChartData = messageChartData;
                        retVal.BookingChartData = bookingChartData;
                        retVal.WebsiteChartData = websiteChartData;
                        retVal.InteractionChartData = interactionChartData;

                        retVal.PercentGoogleSearchDesktop = percentGoogleSearchDesktop;
                        retVal.PercentGoogleSearchMobile = percentGoogleSearchMobile;
                        retVal.PercentGoogleMapMobile = percentGoogleMapMobile;
                        retVal.PercentGoogleMapDesktop = percentGoogleMapDesktop;
                        retVal.StatusCode = response.StatusCode;
                    }

                }
                else if(response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    retVal.StatusCode = response.StatusCode;                 
                }              
            }
       
            return retVal;
        }

       
        private decimal CalculatePercentage(int sum, int value)
        {
            if (sum > 0)
            {
                decimal percentage = ((decimal)value/sum) * 100;
                return Math.Round(percentage,2);
            }
            else
            {
                return 0.00m;
            }
          
        }

        //Need to handle paging if record are more than 100 (Maximum is 100)
        public async Task<List<GbpLocation>> GetLocationById(string accessToken, string locationId)
        {
            var retVal = new List<GbpLocation>();

            string apiUrl = "https://mybusinessbusinessinformation.googleapis.com/v1/accounts/" + locationId;
            //string queryParams = "?readMask=labels,name,storeCode,title,websiteUri,storefrontAddress,serviceArea&pageSize=100";

            var resLocation = new List<GbpLocation>();

            string nextPageToken = null;

            do
            {
                // Create RestClient instance
                var client = new RestClient(apiUrl);

                // Create RestRequest instance
                var request = new RestRequest("locations/", Method.Get);
                request.AddParameter("readMask", "labels,name,storeCode,title,websiteUri,storefrontAddress,serviceArea");
                request.AddParameter("pageSize", 100);

                if (nextPageToken != null)
                {
                    request.AddParameter("pageToken", nextPageToken);
                }


                // Add authorization header
                request.AddHeader("Authorization", $"Bearer {accessToken}");

                // Execute the request
                var response = await client.ExecuteAsync(request);

                // Check the response
                if (response.IsSuccessful && response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var locations = JsonConvert.DeserializeObject<GbpLocations>(response.Content);                                       

                    if (locations.locations != null)
                    {
                        foreach (var location in locations.locations)
                        {
                            var loc = new GbpLocation();

                            string id = location.name.Split('/').Last();
                            loc.accountId = id;
                            loc.title = location.title;

                            var address = location.storefrontAddress != null ?
                               string.Join(",", location.storefrontAddress.addressLines) + ", " + location.storefrontAddress.locality : location.serviceArea.places.placeInfos[0].placeName;

                            loc.address = address;
                            resLocation.Add(loc);
                        }

                    }

                    nextPageToken = locations.nextPageToken;                   
                }
                else
                {
                    Console.WriteLine("Request failed");
                    Console.WriteLine($"Status Code: {response.StatusCode}");
                    Console.WriteLine($"Error Message: {response.ErrorMessage}");
                }
            } while (!string.IsNullOrEmpty(nextPageToken));

            retVal = resLocation;

            return retVal;
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
            return "Id,EmailId,AccountId,CampaignID,Name";
        }

        #endregion
    }
}
