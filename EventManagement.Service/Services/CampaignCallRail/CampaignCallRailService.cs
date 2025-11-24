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
using static EventManagement.Dto.CampaignCallRailDto;
using Google.Apis.Analytics.v3.Data;
using Grpc.Core;
using MailChimp.Net.Core;
using MailChimp.Net.Models;
using Method = RestSharp.Method;
using System.Net;
using IdentityServer4.Extensions;

namespace EventManagement.Service
{
    public class CampaignCallRailService : ServiceBase<CampaignCallRail, Guid>, ICampaignCallRailService
    {

        #region PRIVATE MEMBERS

        private readonly ICampaignCallRailRepository _campaigncallrailRepository;
        private readonly IConfiguration _configuration;
        #endregion


        #region CONSTRUCTOR

        public CampaignCallRailService(ICampaignCallRailRepository campaigncallrailRepository, ILogger<CampaignCallRailService> logger, IConfiguration configuration) : base(campaigncallrailRepository, logger)
        {
            _campaigncallrailRepository = campaigncallrailRepository;
            _configuration = configuration;
        }

        #endregion


        #region PUBLIC MEMBERS

        public async Task<CampaignCallRailDto> VaidateApiKeyAndSetup(string apiKey, Guid campaignId)
        {
            // Specify the API endpoint
            string apiUrl = "https://api.callrail.com/v3/";

            // Create a RestSharp client
            var client = new RestClient(apiUrl);

      
          
                // Create a RestSharp request
                var request = new RestRequest("a.json", Method.Get);

                // Add required headers, including the API key
                request.AddHeader("Authorization", $"Token token={apiKey}");
                request.AddHeader("Content-Type", "application/json");

                // Add pagination parameters
                request.AddParameter("sorting", "name", false);
                request.AddParameter("page", 1, ParameterType.QueryString);
                request.AddParameter("per_page", 1, ParameterType.QueryString);

                // Execute the request and get the response
                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful && response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var data = JsonConvert.DeserializeObject<AccountResponse>(response.Content);
                  

                    CampaignCallRailForCreation callRailSetup = new CampaignCallRailForCreation();
                    callRailSetup.ApiKey = apiKey;
                    callRailSetup.CampaignID = campaignId;
                    var res = await CreateEntityAsync<CampaignCallRailDto, CampaignCallRailForCreation>(callRailSetup);
                    await SaveChangesAsync();

                    return res;
                  
                }
                else
                {                  
                    // Handle the case when the request is not successful
                    return null;
                }
            }
          
           
        

        public async Task<AccountResponse> GetAccountList(Guid campaignId)
        {
            var retVal = new AccountResponse();
            var campaign = _campaigncallrailRepository.GetAllEntities(false).FirstOrDefault(x => x.CampaignID == campaignId);

            if (campaign != null)
            {

                // Specify the API endpoint
                string apiUrl = "https://api.callrail.com/v3/";

                // Create a RestSharp client
                var client = new RestClient(apiUrl);

                // Initialize variables for pagination
                int page = 1;
                int perPage = 100; // Adjust as needed
                int totalRecords = 0;
                var accounts = new AccountResponse();

                do
                {
                    // Create a RestSharp request
                    var request = new RestRequest("a.json", Method.Get);

                    // Add required headers, including the API key
                    request.AddHeader("Authorization", $"Token token={campaign.ApiKey}");
                    request.AddHeader("Content-Type", "application/json");

                    // Add pagination parameters
                    request.AddParameter("sorting", "name", false);
                    request.AddParameter("page", page, ParameterType.QueryString);
                    request.AddParameter("per_page", perPage, ParameterType.QueryString);

                    // Execute the request and get the response
                    var response = await client.ExecuteAsync(request);

                    if (response.IsSuccessful && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        retVal = JsonConvert.DeserializeObject<AccountResponse>(response.Content);
                        retVal.HttpStatus = response.StatusCode;


                        // Process the retrieved accounts as needed

                        // Update pagination variables
                        page++;
                        totalRecords += accounts.Accounts.Count;
                    }
                    else
                    {
                        retVal.ErrorMessage = response.ErrorMessage;
                        retVal.HttpStatus = response.StatusCode;

                        // Handle the case when the request is not successful
                        return retVal;
                    }
                }
                while (totalRecords < accounts.TotalRecords);
                return retVal;
            }
            else
            {
                retVal.ErrorMessage = "No campaign found";
                retVal.HttpStatus = System.Net.HttpStatusCode.NotFound;
                return retVal;
            }        

        }

        //public async Task<CallRailDashboardData> GetCallRailReport1(Guid campaignId, string startDate, string endDate)
        //{
        //    var campaign = _campaigncallrailRepository.GetAllEntities(false).Where(x => x.CampaignID == campaignId).FirstOrDefault();

        //    var dashboardData = new CallRailDashboardData();

        //    // Specify the API endpoint
        //    string apiUrl = "https://api.callrail.com/v3/";

        //    // Create a RestSharp client
        //    var client = new RestClient(apiUrl);

        //    var request = new RestRequest($"a/{campaign.NumericId}/calls.json", Method.Get);

        //    // Add required headers, including the API key
        //    request.AddHeader("Authorization", $"Token token={campaign.ApiKey}");
        //    request.AddHeader("Content-Type", "application/json");
        //    request.AddParameter("start_date", startDate);
        //    request.AddParameter("end_date", endDate);
        //    request.AddParameter("fields", "created_at,customer_name,source_name,source,customer_phone_number,duration,customer_city,answered,first_call,landing_page_url,device_type,lead_status,keywords,tags,company_name,recording,recording_player,campaign");


        //    var response = await client.ExecuteAsync(request);

        //    if (response.IsSuccessful && response.StatusCode == System.Net.HttpStatusCode.OK)
        //    {
        //        var callResponse = JsonConvert.DeserializeObject<CallResponse>(response.Content);

        //        // Count occurrences of each source, sorted by count in descending order
        //        Dictionary<string, int> sourceCounts = callResponse.calls
        //            .GroupBy(call => call.source)
        //            .OrderByDescending(group => group.Count())
        //            .ToDictionary(group => group.Key, group => group.Count());

        //        DateTime startDateDateTme = DateTime.Parse(startDate);
        //        DateTime endDateDateTme = DateTime.Parse(endDate);

        //        // Generate a list of dates within the specified range
        //        var dateRange = Enumerable.Range(0, (endDateDateTme - startDateDateTme).Days + 1)
        //            .Select(offset => startDateDateTme.AddDays(offset))
        //            .ToList();

        //        var dateDifference = (endDateDateTme - startDateDateTme).Days;

        //        // Adjust the grouping interval based on the date difference
        //        var groupingInterval = dateDifference > 30 ? "MMM-yyyy" : "dd-MM-yyyy";

        //        var groupedData = dateRange.GroupBy(date => date.ToString(groupingInterval))
        //             .Select(group =>
        //             {
        //                 var groupedCalls = callResponse?.calls
        //                     .Where(call =>
        //                         (groupingInterval == "dd-MM-yyyy" ? call?.start_time.Date == DateTime.Parse(group.Key)
        //                         : call?.start_time.ToString("MMM-yyyy") == group.Key)).ToList() ?? new List<Call>(); // Replace 'Call' with your actual type

        //                 // Calculate rates for the current group
        //                 var totalCalls = groupedCalls?.Count() ?? 0;
        //                 var totalAnswered = groupedCalls?.Count(call => call?.answered == true) ?? 0;
        //                 var totalMissed = groupedCalls?.Count(call => call?.answered == false) ?? 0;
        //                 var totalFirstTimeCalls = groupedCalls?.Count(call => call?.first_call == true) ?? 0;


        //                 // Calculate average duration for the current group
        //                 var averageDuration = groupedCalls?.Any() == true ? groupedCalls.Average(x => x?.duration ?? 0) : 0;

        //                 return new LineChart
        //                 {
        //                     Dates = group.Key,
        //                     Answered = totalAnswered,
        //                     Missed = totalMissed,
        //                     FirstTime = groupedCalls?.Count(call => call?.first_call == true) ?? 0,
        //                     Calls = totalCalls,
        //                     Duration = groupedCalls?.Sum(x => x?.duration ?? 0) ?? 0,
        //                     DurationGroupByAvg = averageDuration,
        //                     AnswerRateDaily = totalCalls > 0 ? ((double)totalAnswered / totalCalls) * 100 : 0,
        //                     MissedRateDaily = totalCalls > 0 ? ((double)totalMissed / totalCalls) * 100 : 0,
        //                     FirstTimeRateDaily = totalCalls > 0 ? ((double)totalFirstTimeCalls / totalCalls) * 100 : 0,
        //                 };
        //             }).ToList();


        //        // Extract lists and put null or empty checks
        //        dashboardData.Dates = groupedData?.Select(x => x.Dates)?.ToList() ?? new List<string>();
        //        dashboardData.AnsweredList = groupedData?.Select(x => x.Answered)?.ToList() ?? new List<int>();
        //        dashboardData.MissedCallList = groupedData?.Select(x => x.Missed)?.ToList() ?? new List<int>();
        //        dashboardData.FirstTimeList = groupedData?.Select(x => x.FirstTime)?.ToList() ?? new List<int>();
        //        dashboardData.CallsList = groupedData?.Select(x => x.Calls)?.ToList() ?? new List<int>();
        //        dashboardData.LeadsList = new List<int>();
        //        dashboardData.AvgCallsPerLeadList = new List<double>();

        //        // Format the list of durations
        //        dashboardData.DurationList = groupedData?.Select(x => TimeSpan.FromSeconds(x.DurationGroupByAvg).ToString(@"hh\:mm\:ss"))?.ToList() ?? new List<string>();

        //        // Calculate totals
        //        dashboardData.TotalAnswered = groupedData?.Sum(x => x.Answered) ?? 0;
        //        dashboardData.TotalMissed = groupedData?.Sum(x => x.Missed) ?? 0;
        //        dashboardData.TotalFirstTime = groupedData?.Sum(x => x.FirstTime) ?? 0;
        //        dashboardData.TotalCalls = groupedData?.Sum(x => x.Calls) ?? 0;


        //        // Calculate total average rates
        //        var totalCallsAll = groupedData.Sum(x => x.Calls);
        //        var totalAnsweredAll = groupedData.Sum(x => x.Answered);
        //        var totalMissedAll = groupedData.Sum(x => x.Missed);
        //        var totalFirstTimeAll = groupedData.Sum(x => x.FirstTime);
        //        var totalAnswerRateAvg = totalCallsAll > 0 ? ((double)totalAnsweredAll / totalCallsAll) * 100 : 0;
        //        var totalMissedRateAvg = totalCallsAll > 0 ? ((double)totalMissedAll / totalCallsAll) * 100 : 0;
        //        var totalFirstRateAvg = totalCallsAll > 0 ? ((double)totalFirstTimeAll / totalCallsAll) * 100 : 0;

        //        // Apply Math.Round to ensure two decimal places
        //        var avgAnswerRateList = groupedData.Select(rate => Math.Round(rate.AnswerRateDaily, 2)).ToList();
        //        var avgMissedRateList = groupedData.Select(rate => Math.Round(rate.MissedRateDaily, 2)).ToList();

        //        dashboardData.AvgAnswerRateList = avgAnswerRateList;
        //        dashboardData.AvgMissedRateList = avgMissedRateList;

        //        dashboardData.TotalAnsweredRateAvg = Math.Round(totalAnswerRateAvg, 2);
        //        dashboardData.TotalMissedRateAvg = Math.Round(totalMissedRateAvg, 2);

        //        //calculate Avg First Time Call Rate
        //        dashboardData.TotalAvgFirstTimeCallRate = Math.Round(totalFirstRateAvg, 2);
        //        dashboardData.AvgFirstTimeCallRateList = groupedData.Select(x => x.FirstTimeRateDaily).ToList();

        //        // Format the list of durations
        //        var formattedDurationList = groupedData?.Select(x => TimeSpan.FromSeconds(x?.DurationGroupByAvg ?? 0).ToString(@"hh\:mm\:ss"))?.ToList() ?? new List<string>();
        //        dashboardData.DurationList = formattedDurationList;

        //        // Calculate average duration
        //        var avgDuration = groupedData?.Where(x => x?.DurationGroupByAvg > 0)?.Average(x => x?.DurationGroupByAvg ?? 0) ?? 0;
        //        var avgDurationTimeSpan = TimeSpan.FromSeconds(avgDuration).ToString(@"hh\:mm\:ss");
        //        dashboardData.AvgDuration = avgDurationTimeSpan;
        //        dashboardData.AvgDurationDouble = avgDuration;

        //        // Set source counts
        //        dashboardData.SourceCounts = sourceCounts?.OrderByDescending(group => group.Value)?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? new Dictionary<string, int>();
        //        dashboardData.StatusCodes = System.Net.HttpStatusCode.OK;

        //        return dashboardData;
        //    }
        //    else
        //    {
        //        dashboardData.StatusCodes = response.StatusCode;
        //        dashboardData.ErrorMessage = response.ErrorMessage;

        //        // Handle the case where the request for calls was not successful
        //        // You might want to throw an exception or log the error
        //        return dashboardData;
        //    }
        //}

        public async Task<Recording> GetRecording(Guid campaignId, string url)
        {
            var retVal = new Recording();
            var campaign = _campaigncallrailRepository.GetAllEntities(false).Where(x => x.CampaignID == campaignId).FirstOrDefault();

            try {
                // Create a RestSharp client
                var client = new RestClient(url);

                var request = new RestRequest($"", Method.Get);

                // Add required headers, including the API key
                request.AddHeader("Authorization", $"Token token={campaign.ApiKey}");
                request.AddHeader("Content-Type", "application/json");

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful && response.StatusCode == System.Net.HttpStatusCode.OK)
                {

                    retVal = JsonConvert.DeserializeObject<Recording>(response.Content);

                    if (!string.IsNullOrEmpty(retVal.url))
                    {
                        // Create a RestSharp client
                        var client1 = new RestClient(retVal.url);

                        var request1 = new RestRequest($"", Method.Get);

                        request1.AddHeader("Content-Type", "application/json");

                        var response1 = await client1.ExecuteAsync(request1);

                        retVal.url = response1.ResponseUri.ToString();

                    }
                    else
                    {
                        retVal.StatusCodes = response.StatusCode;
                        retVal.ErrorMessage = response.ErrorMessage;
                    }

                }
            }
            catch (Exception ex)
            {
                retVal.StatusCodes = HttpStatusCode.InternalServerError;
                retVal.ErrorMessage = ex.Message;
            }
           

            return retVal;
        }


        public async Task<CallResponse> GetCallRailTableReport(Guid campaignId, string startDate, string endDate, int pageNumber)
        {
            var campaign = _campaigncallrailRepository.GetAllEntities(false).Where(x => x.CampaignID == campaignId).FirstOrDefault();

            var callResponse = new CallResponse();

            // Specify the API endpoint
            string apiUrl = "https://api.callrail.com/v3/";

            // Create a RestSharp client
            var client = new RestClient(apiUrl);

            var request = new RestRequest($"a/{campaign.NumericId}/calls.json", Method.Get);

            // Add required headers, including the API key
            request.AddHeader("Authorization", $"Token token={campaign.ApiKey}");
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("start_date", startDate);
            request.AddParameter("end_date", endDate);
            request.AddParameter("fields", "created_at,customer_name,source_name,source,customer_phone_number,duration,customer_city,answered,first_call,landing_page_url,device_type,lead_status,keywords,tags,company_name,recording,recording_player,campaign");
            request.AddParameter("page", pageNumber);
            request.AddParameter("per_page", 25);

            var response = await client.ExecuteAsync(request);

            if (response.IsSuccessful && response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                 callResponse = JsonConvert.DeserializeObject<CallResponse>(response.Content);

                callResponse.StatusCodes = System.Net.HttpStatusCode.OK;

                callResponse.calls = callResponse.calls.OrderByDescending(c => c.created_at).ToList();

                return callResponse;
            }
            else
            {
                callResponse.StatusCodes = response.StatusCode;
                callResponse.ErrorMessage = response.ErrorMessage;

                // Handle the case where the request for calls was not successful
                // You might want to throw an exception or log the error
                return callResponse;
            }
        }

        public async Task<CallResponse> GetAllCallRailCallsForPdf(Guid campaignId, string startDate, string endDate)
        {
            var campaign = _campaigncallrailRepository.GetAllEntities(false)
                .FirstOrDefault(x => x.CampaignID == campaignId);

            var callResponse = new CallResponse
            {
                calls = new List<Call>(), // Initialize the list
                StatusCodes = HttpStatusCode.OK // Assume success initially
            };

            if (campaign == null)
            {
                // Handle the case where the campaign is not found
                callResponse.StatusCodes = HttpStatusCode.NotFound;
                callResponse.ErrorMessage = "Campaign not found";
                return callResponse;
            }

            string apiUrl = "https://api.callrail.com/v3/";
            var client = new RestClient(apiUrl);

            int pageNumber = 1;
            int perPage = 100; // Set the maximum per_page value

            while (true)
            {
                var request = new RestRequest($"a/{campaign.NumericId}/calls.json", Method.Get);

                // Add headers and parameters
                request.AddHeader("Authorization", $"Token token={campaign.ApiKey}");
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("start_date", startDate);
                request.AddParameter("end_date", endDate);
                request.AddParameter("fields", "created_at,customer_name,source_name,source,customer_phone_number,duration,customer_city,answered,first_call,landing_page_url,device_type,lead_status,keywords,tags,company_name,recording,recording_player,campaign");
                request.AddParameter("page", pageNumber);
                request.AddParameter("per_page", perPage);

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful && response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var pageCallResponse = JsonConvert.DeserializeObject<CallResponse>(response.Content);

                    // Update the main CallResponse with the current page's information
                    callResponse.page = pageCallResponse.page;
                    callResponse.per_page = pageCallResponse.per_page;
                    callResponse.total_pages = pageCallResponse.total_pages;
                    callResponse.total_records = pageCallResponse.total_records;

                    // Add calls to the list
                    callResponse.calls.AddRange(pageCallResponse.calls);

                    // Update pageNumber and check if there are more pages
                    pageNumber++;

                    if (pageNumber > pageCallResponse.total_pages)
                    {
                        // Break the loop if all pages have been fetched
                        break;
                    }
                }
                else
                {
                    // Handle the case where the request for calls was not successful
                    // You might want to throw an exception or log the error
                    callResponse.StatusCodes = response.StatusCode;
                    callResponse.ErrorMessage = response.ErrorMessage;
                    break; // Break the loop on error
                }
            }

            callResponse.calls = callResponse.calls.OrderByDescending(c => c.created_at).ToList();

            return callResponse;
        }

     
        public async Task<CallRailReportData> GetCallRailReport(CallReportDTO callReportDTO)
        {
            CallRailReportData data = new CallRailReportData();

            try
            {
                data.CurrentPeriodData = await GetCallRailReportByDate(callReportDTO.CampaignId, callReportDTO.StartDate, callReportDTO.EndDate);
                data.PreviousPeriodData = await GetCallRailReportByDate(callReportDTO.CampaignId, callReportDTO.PrevStartDate, callReportDTO.PrevEndDate);

                //Calculate diff
                data.PieChartDiff = CalculateCardPercentage(data.CurrentPeriodData.TotalCalls, data.PreviousPeriodData.TotalCalls);
                data.CallsDiff = CalculateCardPercentage(data.CurrentPeriodData.TotalCalls, data.PreviousPeriodData.TotalCalls);
                data.AnsweredDiff = CalculateCardPercentage(data.CurrentPeriodData.TotalAnswered, data.PreviousPeriodData.TotalAnswered);
                data.MissedDiff = CalculateCardPercentage(data.CurrentPeriodData.TotalMissed, data.PreviousPeriodData.TotalMissed);
                data.FirstTimeDiff = CalculateCardPercentage(data.CurrentPeriodData.TotalFirstTime, data.PreviousPeriodData.TotalFirstTime);
                data.AvgDurationDiff = CalculateCardPercentage(data.CurrentPeriodData.AvgDurationDouble, data.PreviousPeriodData.AvgDurationDouble);
                data.AnsweredRateAvgDiff = CalculateCardPercentage(data.CurrentPeriodData.TotalAnsweredRateAvg, data.PreviousPeriodData.TotalAnsweredRateAvg);
                data.MissedRateAvgDiff = CalculateCardPercentage(data.CurrentPeriodData.TotalMissedRateAvg, data.PreviousPeriodData.TotalMissedRateAvg);
                data.FirstRateAvgDiff = CalculateCardPercentage(data.CurrentPeriodData.TotalAvgFirstTimeCallRate, data.PreviousPeriodData.TotalAvgFirstTimeCallRate);
                data.LeadsDiff = CalculateCardPercentage(data.CurrentPeriodData.TotalLeads, data.PreviousPeriodData.TotalLeads); 
                data.AvgCallPerLeadsDiff = CalculateCardPercentage(data.CurrentPeriodData.TotalAvgCallsPerLead, data.PreviousPeriodData.TotalAvgCallsPerLead);

            }
            catch (Exception ex)
            {
                data.ErrorMessage = ex.Message;
                data.StatusCodes = System.Net.HttpStatusCode.InternalServerError;
            }

            return data;          
        }

        private string CalculateCardPercentage(double currentValue, double previousValue)
        {
            if (previousValue == 0 && currentValue == 0)
            {
                return "0%";
            }

            if (previousValue == 0)
            {
                return "+100%";
            }

            if (currentValue == 0)
            {
                return "-100%";
            }

            
            double percentageChange = (currentValue - previousValue) / previousValue * 100;

            string changeDirection = (percentageChange < 0) ? "-" : "+";

            double absolutePercentageChange = Math.Round(Math.Abs(percentageChange), MidpointRounding.AwayFromZero);

            return changeDirection + absolutePercentageChange + "%";
        }


        public async Task<CallRailDashboardData> GetCallRailReportByDate(Guid campaignId, string startDate,string endDate)
        {
            var campaign = _campaigncallrailRepository.GetAllEntities(false).FirstOrDefault(x => x.CampaignID == campaignId);

            var dashboardData = new CallRailDashboardData();

            var allCalls = new List<Call>(); // List to store all calls from all pages

            try
            {
                // Make the initial request to get the first page
                var page = 1;

                var response = await ExecuteCallRailRequest(campaign, startDate, endDate, page);

                if (response.IsSuccessful && response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var callResponse = JsonConvert.DeserializeObject<CallResponse>(response.Content);

                    // Process the first page data
                    allCalls.AddRange(callResponse.calls);

                    // Check if there are more pages
                    while (page < callResponse.total_pages)
                    {
                        // Increment the page number
                        page++;

                        // Make the next request for the next page
                        response = await ExecuteCallRailRequest(campaign, startDate, endDate, page);

                        if (response.IsSuccessful && response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            callResponse = JsonConvert.DeserializeObject<CallResponse>(response.Content);

                            // Add calls from the current page to the list
                            allCalls.AddRange(callResponse.calls);
                        }
                        else
                        {
                            dashboardData.StatusCodes = response.StatusCode;
                            dashboardData.ErrorMessage = response.ErrorMessage;

                            // Handle the case where the request for calls was not successful
                            // You might want to throw an exception or log the error
                            return dashboardData;
                        }
                    }

                    // Process all collected calls
                    dashboardData = ProcessPageData(allCalls, startDate, endDate);

                    return dashboardData;
                }
                else
                {
                    dashboardData.StatusCodes = response.StatusCode;
                    dashboardData.ErrorMessage = response.ErrorMessage;

                    // Handle the case where the initial request for calls was not successful
                    // You might want to throw an exception or log the error
                    return dashboardData;
                }
            }
            catch (Exception ex)
            {
                dashboardData.StatusCodes = System.Net.HttpStatusCode.InternalServerError;
                dashboardData.ErrorMessage = ex.Message;

                // Handle the case where the initial request for calls was not successful
                // You might want to throw an exception or log the error
                return dashboardData;
            }       
        }

        private async Task<RestResponse> ExecuteCallRailRequest(CampaignCallRail campaign,string startDate,string endDate, int page)
        {
            try
            {
                // Specify the API endpoint
                string apiUrl = "https://api.callrail.com/v3/";

                // Create a RestSharp client
                var client = new RestClient(apiUrl);

                var request = new RestRequest($"a/{campaign.NumericId}/calls.json", Method.Get);

                // Add required headers, including the API key
                request.AddHeader("Authorization", $"Token token={campaign.ApiKey}");
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("start_date", startDate);
                request.AddParameter("end_date", endDate);
                request.AddParameter("page", page);
                request.AddParameter("fields", "created_at,customer_name,source_name,source,customer_phone_number,duration,customer_city,answered,first_call,landing_page_url,device_type,lead_status,keywords,tags,company_name,recording,recording_player,campaign");

                var response = await client.ExecuteAsync(request);
                return response;
            }
            catch (Exception ex)
            {
                // Handle exceptions here (log, throw, etc.)
                throw new Exception($"Error executing API request: {ex.Message}", ex);
            }
           
        }

        private CallRailDashboardData ProcessPageData(List<Call> allCalls,string startDate,string endDate)
        {
            CallRailDashboardData dashboardData = new CallRailDashboardData();

            try
            {
                // Count occurrences of each source, sorted by count in descending order
                Dictionary<string, int> sourceCounts = allCalls
                        .GroupBy(call => call.source)
                        .OrderByDescending(group => group.Count())
                        .ToDictionary(group => group.Key, group => group.Count());

                DateTime startDateDateTme = DateTime.Parse(startDate);
                DateTime endDateDateTme = DateTime.Parse(endDate);

                // Generate a list of dates within the specified range
                var dateRange = Enumerable.Range(0, (endDateDateTme - startDateDateTme).Days + 1)
                    .Select(offset => startDateDateTme.AddDays(offset))
                    .ToList();

                var dateDifference = (endDateDateTme - startDateDateTme).Days;

                // Adjust the grouping interval based on the date difference
                var groupingInterval = dateDifference > 30 ? "MMM-yyyy" : "dd-MMM";

                var groupedData = dateRange.GroupBy(date => date.ToString(groupingInterval))
                     .Select(group =>
                     {
                         var groupedCalls = allCalls
                             .Where(call =>
                                 (groupingInterval == "dd-MMM" ? call?.start_time.Date == DateTime.Parse(group.Key)
                                 : call?.start_time.ToString("MMM-yyyy") == group.Key)).ToList() ?? new List<Call>(); // Replace 'Call' with your actual type

                         // Calculate rates for the current group
                         var totalCalls = groupedCalls?.Count() ?? 0;
                         var totalAnswered = groupedCalls?.Count(call => call?.answered == true) ?? 0;
                         var totalMissed = groupedCalls?.Count(call => call?.answered == false) ?? 0;
                         var totalFirstTimeCalls = groupedCalls?.Count(call => call?.first_call == true) ?? 0;

                         // Calculate average duration for the current group
                         var averageDuration = groupedCalls?.Any() == true ? groupedCalls.Average(x => x?.duration ?? 0) : 0;

                         return new LineChart
                         {
                             Dates = group.Key,
                             Answered = totalAnswered,
                             Missed = totalMissed,
                             FirstTime = groupedCalls?.Count(call => call?.first_call == true) ?? 0,
                             Calls = totalCalls,
                             Duration = groupedCalls?.Sum(x => x?.duration ?? 0) ?? 0,
                             DurationGroupByAvg = averageDuration,
                             AnswerRateDaily = totalCalls > 0 ? ((double)totalAnswered / totalCalls) * 100 : 0,
                             MissedRateDaily = totalCalls > 0 ? ((double)totalMissed / totalCalls) * 100 : 0,
                             FirstTimeRateDaily = totalCalls > 0 ? ((double)totalFirstTimeCalls / totalCalls) * 100 : 0,
                         };
                     }).ToList();


                // Extract lists and put null or empty checks
                dashboardData.Dates = groupedData?.Select(x => x.Dates)?.ToList() ?? new List<string>();
                dashboardData.AnsweredList = groupedData?.Select(x => x.Answered)?.ToList() ?? new List<int>();
                dashboardData.MissedCallList = groupedData?.Select(x => x.Missed)?.ToList() ?? new List<int>();
                dashboardData.FirstTimeList = groupedData?.Select(x => x.FirstTime)?.ToList() ?? new List<int>();
                dashboardData.CallsList = groupedData?.Select(x => x.Calls)?.ToList() ?? new List<int>();
                dashboardData.LeadsList = new List<int>();
                dashboardData.AvgCallsPerLeadList = new List<double>();

                // Format the list of durations
                dashboardData.DurationList = groupedData?.Select(x => TimeSpan.FromSeconds(x.DurationGroupByAvg).ToString(@"hh\:mm\:ss"))?.ToList() ?? new List<string>();

                //seconds in integer
                dashboardData.DurationListInt = groupedData.Select(x => x.Duration).ToList();

                // Calculate totals
                dashboardData.TotalAnswered = groupedData?.Sum(x => x.Answered) ?? 0;
                dashboardData.TotalMissed = groupedData?.Sum(x => x.Missed) ?? 0;
                dashboardData.TotalFirstTime = groupedData?.Sum(x => x.FirstTime) ?? 0;
                dashboardData.TotalCalls = groupedData?.Sum(x => x.Calls) ?? 0;
                dashboardData.TotalLeads = 0;
                dashboardData.TotalAvgCallsPerLead = 0;

                // Calculate total average rates
                var totalCallsAll = groupedData.Sum(x => x.Calls);
                var totalAnsweredAll = groupedData.Sum(x => x.Answered);
                var totalMissedAll = groupedData.Sum(x => x.Missed);
                var totalFirstTimeAll = groupedData.Sum(x => x.FirstTime);
                var totalAnswerRateAvg = totalCallsAll > 0 ? ((double)totalAnsweredAll / totalCallsAll) * 100 : 0;
                var totalMissedRateAvg = totalCallsAll > 0 ? ((double)totalMissedAll / totalCallsAll) * 100 : 0;
                var totalFirstRateAvg = totalCallsAll > 0 ? ((double)totalFirstTimeAll / totalCallsAll) * 100 : 0;

                // Apply Math.Round to ensure two decimal places
                var avgAnswerRateList = groupedData.Select(rate => Math.Round(rate.AnswerRateDaily, 2)).ToList();
                var avgMissedRateList = groupedData.Select(rate => Math.Round(rate.MissedRateDaily, 2)).ToList();

                dashboardData.AvgAnswerRateList = avgAnswerRateList;
                dashboardData.AvgMissedRateList = avgMissedRateList;

                dashboardData.TotalAnsweredRateAvg = Math.Round(totalAnswerRateAvg, 2);
                dashboardData.TotalMissedRateAvg = Math.Round(totalMissedRateAvg, 2);

                //calculate Avg First Time Call Rate
                dashboardData.TotalAvgFirstTimeCallRate = Math.Round(totalFirstRateAvg, 2);
                dashboardData.AvgFirstTimeCallRateList = groupedData.Select(x => x.FirstTimeRateDaily).ToList();

                // Format the list of durations
                var formattedDurationList = groupedData?.Select(x => TimeSpan.FromSeconds(x?.DurationGroupByAvg ?? 0).ToString(@"hh\:mm\:ss"))?.ToList() ?? new List<string>();
                dashboardData.DurationList = formattedDurationList;

                // Calculate average duration
                var nonZeroAvgDuration = groupedData.Where(x => x?.DurationGroupByAvg > 0)
                                                       .Select(x => x?.DurationGroupByAvg ?? 0)
                                                       .DefaultIfEmpty() // Ensure there is at least one element (0 if none found)
                                                       .Average();

                var avgDurationTimeSpan = TimeSpan.FromSeconds(nonZeroAvgDuration).ToString(@"hh\:mm\:ss");

                dashboardData.AvgDuration = avgDurationTimeSpan;
                dashboardData.AvgDurationDouble = nonZeroAvgDuration;

                // Set source counts
                dashboardData.SourceCounts = sourceCounts?.OrderByDescending(group => group.Value)?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? new Dictionary<string, int>();
                dashboardData.StatusCodes = System.Net.HttpStatusCode.OK;

                return dashboardData;
            }
            catch (Exception ex)
            {
                // Handle exceptions here (log, throw, etc.)
                dashboardData.ErrorMessage = ex.Message;
                dashboardData.StatusCodes = System.Net.HttpStatusCode.InternalServerError;

                return dashboardData;
            } 
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
            return "Id,AccoundId,AccountName,NumericId,CampaignID";
        }

        #endregion
    }
}
