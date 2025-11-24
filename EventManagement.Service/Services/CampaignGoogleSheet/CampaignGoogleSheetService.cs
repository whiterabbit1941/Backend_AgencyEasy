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

using System.Globalization;

using Method = RestSharp.Method;
using Microsoft.AspNetCore.JsonPatch.Internal;
using static EventManagement.Service.CampaignGoogleSheetService;
using Google.Apis.Analytics.v3.Data;
using System.Net.Http;
using System.Text;

namespace EventManagement.Service
{
    public class CampaignGoogleSheetService : ServiceBase<CampaignGoogleSheet, Guid>, ICampaignGoogleSheetService
    {

        #region PRIVATE MEMBERS

        private readonly ICampaignGoogleSheetRepository _campaigngooglesheetRepository;
        private readonly IConfiguration _configuration;

        #endregion


        #region CONSTRUCTOR

        public CampaignGoogleSheetService(ICampaignGoogleSheetRepository campaigngooglesheetRepository,
            ILogger<CampaignGoogleSheetService> logger, IConfiguration configuration) : base(campaigngooglesheetRepository, logger)
        {
            _campaigngooglesheetRepository = campaigngooglesheetRepository;
            _configuration = configuration;
        }

        #endregion


        #region PUBLIC MEMBERS   

        public async Task<GoogleAccountDto> GetGoogleAccountDetails(Guid campaignId)
        {
            var retVal = new GoogleAccountDto();
            var campaignGSheet = _campaigngooglesheetRepository.GetAllEntities().Where(x => x.CampaignID == campaignId).FirstOrDefault();

            try
            {
                var requestForData = new RestRequest("userinfo", Method.Post);
                requestForData.AddHeader("X-Restli-Protocol-Version", "2.0.0");
                requestForData.AddHeader("Content-Type", "application/json");
                requestForData.AddHeader("Bearer", campaignGSheet.AccessToken);

                var options = new RestClientOptions("https://www.googleapis.com/oauth2/v2/")
                {
                    Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(campaignGSheet.AccessToken, "Bearer")
                };

                var clientForData = new RestClient(options);

                var response = await clientForData.GetAsync(requestForData);

                var data = JsonConvert.DeserializeObject<GoogleAccountDto>(response.Content);

                retVal = data;

                return retVal;
            }
            catch (Exception ex)
            {
                if (ex.Message == "Request failed with status code Unauthorized" || ex.InnerException.Message == "Request failed with status code Unauthorized")
                {
                    var accessToken = await GetAccessTokenUsingRefreshToken(campaignGSheet.RefreshToken);

                    campaignGSheet.AccessToken = accessToken;
                    campaignGSheet.UpdatedOn = DateTime.UtcNow;
                    _campaigngooglesheetRepository.UpdateEntity(campaignGSheet);
                    _campaigngooglesheetRepository.SaveChanges();

                    var response = await GetGoogleAccountDetails(campaignGSheet.CampaignID);
                    retVal = response;
                }
            }

            return retVal;
        }

        public async Task<List<DriveFile>> GetListSpreadSheet(Guid campaignId)
        {
            var retVal = new List<DriveFile>();

            var campaignGSheet = _campaigngooglesheetRepository.GetAllEntities().Where(x => x.CampaignID == campaignId).FirstOrDefault();

            try
            {
                if (campaignGSheet != null)
                {
                    var options = new RestClientOptions("https://www.googleapis.com/drive/v3/");
                    var clientForData = new RestClient(options);

                    string nextPageToken = null;

                    do
                    {
                        var requestForData = new RestRequest("files", Method.Get);

                        requestForData.AddHeader("X-Restli-Protocol-Version", "2.0.0");
                        requestForData.AddHeader("Content-Type", "application/json");
                        requestForData.AddHeader("Authorization", $"Bearer {campaignGSheet.AccessToken}");

                        requestForData.AddQueryParameter("q", "mimeType='application/vnd.google-apps.spreadsheet'", false);
                        requestForData.AddQueryParameter("pageSize", "1000", false);

                        if (nextPageToken != null)
                        {
                            requestForData.AddQueryParameter("pageToken", nextPageToken, false);
                        }

                        var response = await clientForData.ExecuteAsync<FileListResponse>(requestForData);

                        if (!response.IsSuccessful && response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            var accessToken = await GetAccessTokenUsingRefreshToken(campaignGSheet.RefreshToken);

                            if (!string.IsNullOrEmpty(accessToken))
                            {
                                campaignGSheet.AccessToken = accessToken;
                                campaignGSheet.UpdatedOn = DateTime.UtcNow;
                                _campaigngooglesheetRepository.UpdateEntity(campaignGSheet);
                                _campaigngooglesheetRepository.SaveChanges();

                                var res = await GetListSpreadSheet(campaignId);
                                retVal = res;
                            }
                                                     
                            // Handle error here
                            //break;
                        }

                        var data = response.Data;

                        if (data != null && data.Files != null)
                        {
                            retVal.AddRange(data.Files);
                        }

                        nextPageToken = data?.NextPageToken;

                    } while (!string.IsNullOrEmpty(nextPageToken));
                }

                return retVal;


            }
            catch (Exception ex)
            {
                var accessToken = await GetAccessTokenUsingRefreshToken(campaignGSheet.RefreshToken);

                campaignGSheet.AccessToken = accessToken;
                campaignGSheet.UpdatedOn = DateTime.UtcNow;
                _campaigngooglesheetRepository.UpdateEntity(campaignGSheet);
                _campaigngooglesheetRepository.SaveChanges();

                var res = await GetListSpreadSheet(campaignId);

                retVal = res;
            }

            return retVal;

        }

        public async Task<List<SheetProperties>> GetListSheets(Guid campaignId, string spreadSheetId)
        {
            var retVal = new List<SheetProperties>();

            var campaignGSheet = _campaigngooglesheetRepository.GetAllEntities().Where(x => x.CampaignID == campaignId).FirstOrDefault();

            try
            {
                if (campaignGSheet != null)
                {
                    var options = new RestClientOptions("https://sheets.googleapis.com/v4/");
                    var clientForData = new RestClient(options);

                    var requestForData = new RestRequest("spreadsheets/" + spreadSheetId, Method.Get);

                    requestForData.AddHeader("X-Restli-Protocol-Version", "2.0.0");
                    requestForData.AddHeader("Content-Type", "application/json");
                    requestForData.AddHeader("Authorization", $"Bearer {campaignGSheet.AccessToken}");

                    var response = await clientForData.ExecuteAsync<AllSheets>(requestForData);

                    var data = response.Data;

                    if (data != null && data.Sheets != null && data.Sheets.Count > 0 && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var sheetIdAndTitle = data.Sheets.Select(sheet => new SheetProperties { SheetId = sheet.Properties.SheetId, Title = sheet.Properties.Title })
                                                .ToList();

                        retVal = sheetIdAndTitle;
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        var accessToken = await GetAccessTokenUsingRefreshToken(campaignGSheet.RefreshToken);

                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            campaignGSheet.AccessToken = accessToken;
                            campaignGSheet.UpdatedOn = DateTime.UtcNow;
                            _campaigngooglesheetRepository.UpdateEntity(campaignGSheet);
                            _campaigngooglesheetRepository.SaveChanges();

                            var res = await GetListSheets(campaignId, spreadSheetId);

                            retVal = res;
                        }                        
                    }
                }

            }
            catch (Exception ex)
            {
                if (ex.Message == "Request failed with status code Unauthorized" || ex.InnerException.Message == "Request failed with status code Unauthorized")
                {
                    var accessToken = await GetAccessTokenUsingRefreshToken(campaignGSheet.RefreshToken);

                    campaignGSheet.AccessToken = accessToken;
                    campaignGSheet.UpdatedOn = DateTime.UtcNow;
                    _campaigngooglesheetRepository.UpdateEntity(campaignGSheet);
                    _campaigngooglesheetRepository.SaveChanges();

                    var res = await GetListSheets(campaignId, spreadSheetId);

                    retVal = res;
                }
            }

            return retVal;

        }

        public async Task<List<GoogleSheetData>> GetGoogleSheetReport(List<GoogleSheetSettingsDto> settingsDto)
        {
            var retVal = new List<GoogleSheetData>();

            var campaignGSheet = _campaigngooglesheetRepository.GetAllEntities().Where(x => x.CampaignID == settingsDto[0].campaignId).FirstOrDefault();

            foreach (var settingDto in settingsDto)
            {
                var prepareData = new GoogleSheetData();

                try
                {
                    prepareData = await GetDataFromGoogleSheet(settingDto, campaignGSheet);
                    retVal.Add(prepareData);
                }
                catch (Exception ex)
                {
                    prepareData.HttpStatusCode = System.Net.HttpStatusCode.NotFound;
                    prepareData.ErrorMessage = ex.Message;
                    prepareData.ChartId = settingDto.chartId;
                    retVal.Add(prepareData);
                }
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


        private async Task<GoogleSheetData> GetDataFromGoogleSheet(GoogleSheetSettingsDto settingDto, CampaignGoogleSheet campaignGSheet)
        {
            var retVal = new GoogleSheetData();

            bool isAuthorized = false;

            try
            {
                while (!isAuthorized)
                {
                    switch (settingDto.reportType)
                    {
                        //Pie Chart
                        case 56:
                        case 57:

                            var response = await GenerateDataRequest(settingDto.spreadSheetId, settingDto.spreadSheetTab, settingDto.dimensionColumn, settingDto.metricColumn, settingDto.dateColumn, campaignGSheet.AccessToken);

                            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                            {
                                // Renew the token
                                var accessToken = await GetAccessTokenUsingRefreshToken(campaignGSheet.RefreshToken);

                                if (!string.IsNullOrEmpty(accessToken))
                                {
                                    campaignGSheet.AccessToken = accessToken;
                                    campaignGSheet.UpdatedOn = DateTime.UtcNow;
                                    _campaigngooglesheetRepository.UpdateEntity(campaignGSheet);
                                    _campaigngooglesheetRepository.SaveChanges();

                                    campaignGSheet.AccessToken = accessToken;
                                }
                            }
                            else
                            {
                                // Authorized response, set the flag to exit the loop
                                isAuthorized = true;
                                bool dateColumnFound = false;
                                List<string> dates = new List<string>();
                                List<DateTime?> formattedDatesForChart = new List<DateTime?>();
                                var datesOfData = ConvertToDateRange(settingDto.startDate, settingDto.endDate);
                               
                                // Continue with your existing logic to process the response
                                var sheetData = response.Data;

                                // Extract labels and data
                                //var labels = sheetData.ValueRanges[0]?.Values[0].Skip(1).ToList(); // Skip the header
                                //var data = sheetData.ValueRanges[1].Values[0].Skip(1).Select(ExtractNumericValue).ToList(); // Skip the header and parse currency
                                                      
                                 var labels = sheetData?.ValueRanges?[0]?.Values?[0]?.Skip(1)?.ToList();
                               
                                 var data = sheetData?.ValueRanges?[1]?.Values?[0]?.Skip(1)?.Select(ExtractNumericValue)?.ToList();


                                if (!string.IsNullOrEmpty(settingDto.dateColumn))
                                {
                                    dates = sheetData.ValueRanges.ElementAtOrDefault(2).Values.ElementAtOrDefault(0).Skip(1).ToList();
                                    dateColumnFound = dates.Count() > 0;                                    
                                }
                               
                                retVal.xAxisColumnName = sheetData.ValueRanges.ElementAtOrDefault(0)?.Values.ElementAtOrDefault(0)?.FirstOrDefault();
                                retVal.yAxisColumnName = sheetData.ValueRanges.ElementAtOrDefault(1)?.Values.ElementAtOrDefault(0)?.FirstOrDefault();

                                //Logic for pie and bar
                                // Start date and end date filter
                                var startDate = datesOfData.Value.Item1;
                                var endDate = datesOfData.Value.Item2;
                           
                                //Grouping
                                if (labels.Count == data.Count)
                                {
                                    if (dateColumnFound)
                                    {
                                        formattedDatesForChart = dates.Select(ParseDate).ToList();

                                        // Grouping labels and calculating the sum of corresponding data within the date range
                                        var currentGroupedData = labels.Zip(data, (label, value) => new { Label = label, Value = value })
                                                               .Zip(formattedDatesForChart, (item, date) => new { item.Label, item.Value, Date = date })
                                                               .Where(item => item.Date >= startDate && item.Date <= endDate)
                                                               .GroupBy(item => item.Label)
                                                               .Select(group => new { Label = group.Key, Sum = (decimal?)group.Sum(item => item.Value ?? 0) })
                                                               .ToList();


                                        // LineChart: Date 
                                        // Bar chart and Pie Chart: LABEL
                                        retVal.XAxis = currentGroupedData.Select(x => x.Label).ToList();

                                        // Data
                                        retVal.YAxis = currentGroupedData.Select(x => x.Sum).ToList();

                                        retVal.AggregationData = settingDto.aggregator.ToLower() == "sum"
                                            ? Math.Max(0, currentGroupedData.Sum(x => x.Sum) ?? 0)
                                            : Math.Max(0, currentGroupedData.Average(x => x.Sum) ?? 0);
                                    }
                                    else
                                    {
                                        // Grouping labels and calculating the sum of corresponding data within the date range
                                        var currentGroupedData = labels.Zip(data, (label, value) => new { Label = label, Value = value })                                                                                                                           
                                                               .GroupBy(item => item.Label)
                                                               .Select(group => new { Label = group.Key, Sum = (decimal?)group.Sum(item => item.Value ?? 0) })
                                                               .ToList();
                                        
                                    // LineChart: Date 
                                    // Bar chart and Pie Chart: LABEL
                                    retVal.XAxis = currentGroupedData.Select(x => x.Label).ToList();

                                    // Data
                                    retVal.YAxis = currentGroupedData.Select(x => x.Sum).ToList();

                                    retVal.AggregationData = settingDto.aggregator.ToLower() == "sum"
                                        ? Math.Max(0, currentGroupedData.Sum(x => x.Sum) ?? 0)
                                        : Math.Max(0, currentGroupedData.Average(x => x.Sum) ?? 0);
                                    }                                                                   
                                }
                                else
                                {
                                    if (dateColumnFound)
                                    {

                                        // If you want to proceed and fill in missing values with 0 within the date range:
                                        var maxCount = Math.Max(labels.Count, data.Count);

                                        var currentGroupedData = Enumerable.Range(0, maxCount)
                                                        .Select(i => new { Label = i < labels.Count ? labels[i] : "", Value = i < data.Count ? data[i] : (decimal?)0, Date = formattedDatesForChart.ElementAtOrDefault(i) })
                                                        .Where(item => item.Date >= startDate && item.Date <= endDate)
                                                        .GroupBy(item => item.Label)
                                                        .Select(group => new { Label = group.Key, Sum = group.Sum(item => item.Value), Dates = group.Select(item => item.Date).ToList() })
                                                        .ToList();
                                        // LineChart: Date 
                                        // Bar chart and Pie Chart: LABEL
                                        retVal.XAxis = currentGroupedData.Select(x => x.Label).ToList();

                                        // Data
                                        retVal.YAxis = currentGroupedData.Select(x => x.Sum).ToList();

                                        retVal.AggregationData = settingDto.aggregator.ToLower() == "sum"
                                            ? Math.Max(0, currentGroupedData.Sum(x => x.Sum) ?? 0)
                                            : Math.Max(0, currentGroupedData.Average(x => x.Sum) ?? 0);
                                    }
                                    else
                                    {
                                        // If you want to proceed and fill in missing values with 0 within the date range:
                                        var maxCount = Math.Max(labels.Count, data.Count);

                                        var currentGroupedData = Enumerable.Range(0, maxCount)
                                                        .Select(i => new { Label = i < labels.Count ? labels[i] : "", Value = i < data.Count ? data[i] : (decimal?)0 })                                                      
                                                        .GroupBy(item => item.Label)
                                                        .Select(group => new { Label = group.Key, Sum = group.Sum(item => item.Value) })
                                                        .ToList();
                                        // LineChart: Date 
                                        // Bar chart and Pie Chart: LABEL
                                        retVal.XAxis = currentGroupedData.Select(x => x.Label).ToList();

                                        // Data
                                        retVal.YAxis = currentGroupedData.Select(x => x.Sum).ToList();

                                        retVal.AggregationData = settingDto.aggregator.ToLower() == "sum"
                                            ? Math.Max(0, currentGroupedData.Sum(x => x.Sum) ?? 0)
                                            : Math.Max(0, currentGroupedData.Average(x => x.Sum) ?? 0);
                                    }
                                }

                                //For PRevios date range
                                if (settingDto.isComparePrevious && dateColumnFound)
                                {                                    
                                    var prevDatesOfData = ConvertToDateRange(settingDto.prevStartDate, settingDto.prevEndDate);
                                    var prevStartDate = prevDatesOfData.Value.Item1;
                                    var prevEndDate = prevDatesOfData.Value.Item2;

                                    if (labels.Count == data.Count)
                                    {
                                        // Grouping labels and calculating the sum of corresponding data within the date range
                                        var prevGroupedData = labels.Zip(data, (label, value) => new { Label = label, Value = value })
                                                               .Zip(formattedDatesForChart, (item, date) => new { item.Label, item.Value, Date = date })
                                                               .Where(item => item.Date >= prevStartDate && item.Date <= prevEndDate)
                                                               .GroupBy(item => item.Label)
                                                               .Select(group => new { Label = group.Key, Sum = (decimal?)group.Sum(item => item.Value ?? 0) })
                                                               .ToList();


                                        // LineChart: Date 
                                        // Bar chart and Pie Chart: LABEL
                                        retVal.PrevXAxis = retVal.XAxis;  // Include all labels

                                        retVal.PrevYAxis = Enumerable.Repeat<decimal?>(0, retVal.XAxis.Count).ToList();


                                        // Fill in the data for the labels from the previous period
                                        foreach (var prevData in prevGroupedData)
                                        {
                                            int index = retVal.PrevXAxis.IndexOf(prevData.Label);
                                            if (index >= 0)
                                            {
                                                // Prioritize taking data from the previous period
                                                retVal.PrevYAxis[index] = prevData.Sum;
                                            }
                                        }

                                        retVal.PrevAggregationData = settingDto.aggregator.ToLower() == "sum"
                                            ? Math.Max(0, prevGroupedData.Sum(x => x.Sum) ?? 0)
                                            : Math.Max(0, prevGroupedData.Average(x => x.Sum) ?? 0);

                                        retVal.DiffAggregator = PrepareDifference(retVal.AggregationData, retVal.PrevAggregationData);
                                    }
                                    else
                                    {
                                        // If you want to proceed and fill in missing values with 0 within the date range:
                                        var maxCount = Math.Max(labels.Count, data.Count);

                                        var prevGroupedData = Enumerable.Range(0, maxCount)
                                                        .Select(i => new { Label = i < labels.Count ? labels[i] : "", Value = i < data.Count ? data[i] : (decimal?)0, Date = formattedDatesForChart.ElementAtOrDefault(i) })
                                                        .Where(item => item.Date >= prevStartDate && item.Date <= prevEndDate)
                                                        .GroupBy(item => item.Label)
                                                        .Select(group => new { Label = group.Key, Sum = group.Sum(item => item.Value), Dates = group.Select(item => item.Date).ToList() })
                                                        .ToList();


                                        // LineChart: Date 
                                        // Bar chart and Pie Chart: LABEL
                                        retVal.PrevXAxis = retVal.XAxis;  // Include all labels

                                        retVal.PrevYAxis = Enumerable.Repeat<decimal?>(0, retVal.XAxis.Count).ToList();


                                        // Fill in the data for the labels from the previous period
                                        foreach (var prevData in prevGroupedData)
                                        {
                                            int index = retVal.PrevXAxis.IndexOf(prevData.Label);
                                            if (index >= 0)
                                            {
                                                // Prioritize taking data from the previous period
                                                retVal.PrevYAxis[index] = prevData.Sum;
                                            }
                                        }

                                        retVal.PrevAggregationData = settingDto.aggregator.ToLower() == "sum"
                                            ? Math.Max(0, prevGroupedData.Sum(x => x.Sum) ?? 0)
                                            : Math.Max(0, prevGroupedData.Average(x => x.Sum) ?? 0);

                                        retVal.DiffAggregator = PrepareDifference(retVal.AggregationData, retVal.PrevAggregationData);

                                    }
                                }


                                retVal.Tooltip = settingDto.tooltip;
                                retVal.Title = settingDto.title;
                                retVal.ReportSubType = settingDto.reportType;
                                retVal.Aggregator = settingDto.aggregator;
                                retVal.IsComparePrevious = settingDto.isComparePrevious;
                                retVal.ChartId = settingDto.chartId;

                                retVal.HttpStatusCode = response.StatusCode;
                            }

                            if (!isAuthorized)
                            {
                                // Handle the case where the maximum retry count is reached and still unauthorized
                                // Log an error, throw an exception, or take appropriate action
                                retVal.HttpStatusCode = System.Net.HttpStatusCode.Unauthorized; // Set appropriate status code                      
                            }

                            break;

                        //Line and Spark
                        case 58:
                        case 61:
                            var responseLine = await GenerateDataRequestForSpark(settingDto.spreadSheetId, settingDto.spreadSheetTab, settingDto.metricColumn, settingDto.dateColumn, campaignGSheet.AccessToken);

                            if (responseLine.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                            {
                                // Renew the token
                                var accessToken = await GetAccessTokenUsingRefreshToken(campaignGSheet.RefreshToken);

                                if (!string.IsNullOrEmpty(accessToken))
                                {
                                    campaignGSheet.AccessToken = accessToken;
                                    campaignGSheet.UpdatedOn = DateTime.UtcNow;
                                    _campaigngooglesheetRepository.UpdateEntity(campaignGSheet);
                                    _campaigngooglesheetRepository.SaveChanges();

                                    campaignGSheet.AccessToken = accessToken;
                                }
                            }
                            else
                            {
                                // Authorized response, set the flag to exit the loop
                                isAuthorized = true;
                                var datesOfData = ConvertToDateRange(settingDto.startDate, settingDto.endDate);
                              
                                // Continue with your existing logic to process the response
                                var sheetData = responseLine.Data;

                                // Extract labels and data

                               
                              var labels = sheetData?.ValueRanges?[1]?.Values?[0]?.Skip(1)?.ToList();
                              var data = sheetData?.ValueRanges?[0]?.Values?[0]?.Skip(1)?.Select(ExtractNumericValue)?.ToList();

                                //var labels = sheetData.ValueRanges[1].Values[0].Skip(1).ToList(); // Skip the header
                                //var data = sheetData.ValueRanges[0].Values[0].Skip(1).Select(ExtractNumericValue).ToList(); // Skip the header and parse currency

                                retVal.xAxisColumnName = sheetData.ValueRanges.ElementAtOrDefault(1)?.Values.ElementAtOrDefault(0)?.FirstOrDefault();
                                retVal.yAxisColumnName = sheetData.ValueRanges.ElementAtOrDefault(0)?.Values.ElementAtOrDefault(0)?.FirstOrDefault();


                                // Replace null values with 0 for  data
                                var originalList = data.Select(value => value ?? 0).ToList();

                                List<decimal?> formattedData = originalList.Select(value => (decimal?)value).ToList();

                                // Convert date strings to a standard format ("yyyy-MM-dd")
                                List<DateTime?> formattedDates = labels.Select(ParseDate).ToList();

                                
                                //var dd = formattedDates.Zip(formattedProfitData, (date, profitValue) => (date, profitValue)).ToList();
                                List<(DateTime?, decimal?)> myData = formattedDates.Zip(formattedData, (date, profitValue) => (date, profitValue)).ToList();

                                //List<DateTime> newDateRanges = GenerateFormattedDates(startDate, endDate, frequency);

                                if (formattedData.Count == formattedDates.Count)
                                {
                                    // Fill in missing dates with 0 profit
                                    List<(string, decimal?)> filledData = FillMissingDates(myData, datesOfData.Value.Item1, datesOfData.Value.Item2, settingDto.dateInterval);

                                    retVal.XAxis = filledData.Select(x => x.Item1).ToList();

                                    //Data
                                    retVal.YAxis = filledData.Select(x => x.Item2).ToList();

                                    retVal.AggregationData = settingDto.aggregator.ToLower() == "sum"
                                                            ? (filledData.Sum(x => x.Item2) ?? 0)
                                                            : (filledData.Average(x => x.Item2) ?? 0);

                                    //for Previous Date
                                    if (settingDto.isComparePrevious)
                                    {
                                        var prevDatesOfData = ConvertToDateRange(settingDto.prevStartDate, settingDto.prevEndDate);

                                        //Prepare for previous data
                                        List<(string, decimal?)> prevFilledData = FillMissingDates(myData, prevDatesOfData.Value.Item1, prevDatesOfData.Value.Item2, settingDto.dateInterval);


                                        retVal.PrevXAxis = prevFilledData.Select(x => x.Item1).ToList();

                                        //Data
                                        retVal.PrevYAxis = prevFilledData.Select(x => x.Item2).ToList();

                                        retVal.PrevAggregationData = settingDto.aggregator.ToLower() == "sum"
                                                                ? (prevFilledData.Sum(x => x.Item2) ?? 0)
                                                                : (prevFilledData.Average(x => x.Item2) ?? 0);


                                        retVal.DiffAggregator = PrepareDifference(retVal.AggregationData, retVal.PrevAggregationData);

                                    }
                                }
                                else
                                {
                                    retVal.ErrorMessage = "Count of x axis data and y axis data not matched.";
                                    retVal.HttpStatusCode = System.Net.HttpStatusCode.NotFound;
                                    retVal.ChartId = settingDto.chartId;
                                    return retVal;
                                }

                                retVal.Tooltip = settingDto.tooltip;
                                retVal.Title = settingDto.title;
                                retVal.ReportSubType = settingDto.reportType;
                                retVal.Aggregator = settingDto.aggregator;
                                retVal.IsComparePrevious = settingDto.isComparePrevious;
                                retVal.ChartId = settingDto.chartId;

                                retVal.HttpStatusCode = responseLine.StatusCode;
                            }

                            if (!isAuthorized)
                            {
                                // Handle the case where the maximum retry count is reached and still unauthorized
                                // Log an error, throw an exception, or take appropriate action
                                retVal.HttpStatusCode = System.Net.HttpStatusCode.Unauthorized; // Set appropriate status code                      
                            }

                            break;

                        //Table
                        case 59:

                            var response2 = await GenerateDataRequestForTable(settingDto.spreadSheetId, settingDto.spreadSheetTab, campaignGSheet.AccessToken);

                            if (response2.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                            {
                                // Renew the token
                                var accessToken = await GetAccessTokenUsingRefreshToken(campaignGSheet.RefreshToken);

                                if (!string.IsNullOrEmpty(accessToken))
                                {
                                    campaignGSheet.AccessToken = accessToken;
                                    campaignGSheet.UpdatedOn = DateTime.UtcNow;
                                    _campaigngooglesheetRepository.UpdateEntity(campaignGSheet);
                                    _campaigngooglesheetRepository.SaveChanges();

                                    campaignGSheet.AccessToken = accessToken;
                                }
                            }
                            else
                            {
                                // Authorized response, set the flag to exit the loop
                                isAuthorized = true;

                                var datesOfData = ConvertToDateRange(settingDto.startDate, settingDto.endDate);

                                var filterOptions = new TableFilterOptions
                                {
                                    RowLimit = settingDto.tableRowLimit,
                                    SortColumn = settingDto.sortMetrics,
                                    ExcludeEmptyColumns = settingDto.excludeEmptyColumns,
                                    StartDate = datesOfData.Value.Item1,
                                    EndDate = datesOfData.Value.Item2,
                                    SortingOrder = settingDto.sortDirection
                                };

                                // Convert CellData to TableData with applied filters
                                var tableData = ConvertToTableData(response2.Data, filterOptions);

                                retVal.TableData = tableData;

                                retVal.HttpStatusCode = response2.StatusCode;
                            }

                            if (!isAuthorized)
                            {
                                // Handle the case where the maximum retry count is reached and still unauthorized
                                // Log an error, throw an exception, or take appropriate action
                                retVal.HttpStatusCode = System.Net.HttpStatusCode.Unauthorized; // Set appropriate status code                      
                            }

                            retVal.Tooltip = settingDto.tooltip;
                            retVal.Title = settingDto.title;
                            retVal.ReportSubType = settingDto.reportType;
                            retVal.ChartId = settingDto.chartId;

                            break;



                        //Stat Cell Value
                        case 60:

                            var singleCellResponse = await GenerateDataRequestForSingleCell(settingDto.spreadSheetId, settingDto.spreadSheetTab, settingDto.cell, campaignGSheet.AccessToken);

                            if (singleCellResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                            {
                                // Renew the token
                                var accessToken = await GetAccessTokenUsingRefreshToken(campaignGSheet.RefreshToken);

                                if (!string.IsNullOrEmpty(accessToken))
                                {
                                    campaignGSheet.AccessToken = accessToken;
                                    campaignGSheet.UpdatedOn = DateTime.UtcNow;
                                    _campaigngooglesheetRepository.UpdateEntity(campaignGSheet);
                                    _campaigngooglesheetRepository.SaveChanges();

                                    campaignGSheet.AccessToken = accessToken;
                                }
                            }
                            else
                            {
                                // Authorized response, set the flag to exit the loop
                                isAuthorized = true;

                                retVal.CellData = singleCellResponse.Data.Values != null ? singleCellResponse.Data.Values.SelectMany(x => x).FirstOrDefault() : "";

                                retVal.HttpStatusCode = singleCellResponse.StatusCode;
                            }

                            if (!isAuthorized)
                            {
                                // Handle the case where the maximum retry count is reached and still unauthorized
                                // Log an error, throw an exception, or take appropriate action
                                retVal.HttpStatusCode = System.Net.HttpStatusCode.Unauthorized; // Set appropriate status code                      
                            }

                            retVal.Tooltip = settingDto.tooltip;
                            retVal.Title = settingDto.title;
                            retVal.ReportSubType = settingDto.reportType;
                            retVal.ChartId = settingDto.chartId;

                            break;                        

                        default:

                            retVal.Tooltip = settingDto.tooltip;
                            retVal.Title = settingDto.title;
                            retVal.ErrorMessage = "No Report Type Found...";
                            break;
                    }

                }
            }
            catch (Exception ex)
            {
                isAuthorized = true;
                retVal.ErrorMessage = ex.Message;
                retVal.ChartId = settingDto.chartId;
            }

            return retVal;
        }

        public string PrepareDifference(decimal current, decimal previous)
        {
            decimal difference = current - previous;
            string sign = difference >= 0 ? "+" : "-";
            return $"{current}({sign}{Math.Abs(difference)})";
        }

        public TableData ConvertToTableData(CellData cellData, TableFilterOptions filterOptions)
        {
            if (cellData.MajorDimension != "COLUMNS" || cellData.Values.Count == 0)
            {
                throw new ArgumentException("Invalid data format or major dimension");
            }

            var tableData = new TableData();

            // First row contains column names
            tableData.Columns = cellData.Values.Select(colName => new ColumnData { Name = string.IsNullOrEmpty(colName.ElementAtOrDefault(0)) ? "" : colName.ElementAtOrDefault(0) }).ToList();


            // Initialize the Values list for each column
            foreach (var column in tableData.Columns)
            {
                column.Values = new List<string>();
            }

            var maxRowCount = cellData.Values.Max(row => row.Count);

            // Iterate over the rows starting from the second row (skipping the header)
            for (int i = 0; i < cellData.Values.Count; i++)
            {
                var rowData = cellData.Values[i].Skip(1).ToList();
                if (rowData.Count == 0)
                {
                    //checked data count other element. Handling for empty column
                    List<string> emptyStringList = Enumerable.Repeat(string.Empty, maxRowCount - 1).ToList();
                    tableData.Columns[i].Values.AddRange(emptyStringList);
                }
                else if (rowData.Count < maxRowCount - 1)
                {
                    // If rowData.Count is less than maxRowCount - 1, add missing data
                    int missingCount = maxRowCount - 1 - rowData.Count;
                    List<string> missingDataList = Enumerable.Repeat(string.Empty, missingCount).ToList();
                    tableData.Columns[i].Values.AddRange(rowData.Concat(missingDataList));
                }
                else
                {
                    tableData.Columns[i].Values.AddRange(rowData);
                }

                
            }

            // Apply filtering, sorting, and date range
            ApplyFiltering(tableData, filterOptions);

            return tableData;
        }

        private static void ApplyFiltering(TableData tableData, TableFilterOptions filterOptions)
        {
            // Filter by date range if applicable
            if (tableData.Columns.Any(col => IsDateColumn(col.Name)) && filterOptions.StartDate.HasValue && filterOptions.EndDate.HasValue)
            {
                FilterByDateRange(tableData, filterOptions.StartDate.Value, filterOptions.EndDate.Value);
            }

            // Exclude empty columns
            if (filterOptions.ExcludeEmptyColumns)
            {
                ExcludeEmptyColumns(tableData);
            }

            // Sort by column if applicable
            if (!string.IsNullOrEmpty(filterOptions.SortColumn))
            {
                SortByColumn(tableData, filterOptions.SortColumn, filterOptions.SortingOrder);
            }

            // Apply row limit
            if (filterOptions.RowLimit > 0 && tableData.Columns.Any())
            {
                ApplyRowLimit(tableData, filterOptions.RowLimit);
            }
        }

        private static void FilterByDateRange(TableData tableData, DateTime startDate, DateTime endDate)
        {
            var dateColumn = tableData.Columns.FirstOrDefault(col => IsDateColumn(col.Name));

            // Check if the date column exists
            if (dateColumn != null)
            {
                var validIndices = new List<int>();

                for (int i = 0; i < dateColumn.Values.Count; i++)
                {
                    if (DateTime.TryParse(dateColumn.Values[i], out DateTime date) &&
                        IsDateWithinRange(date.ToString(), startDate, endDate))
                    {
                        validIndices.Add(i);
                    }
                }

                if (validIndices.Count > 0)
                {
                    foreach (var column in tableData.Columns)
                    {
                        column.Values = validIndices.Select(i => column.Values[i]).ToList();
                    }
                }
                else
                {
                    // No valid indices, keep the original data
                    // You can choose to do nothing in this case, or you may create a copy of the original values
                    // and assign them back to the columns
                    var originalValues = tableData.Columns.ToDictionary(col => col.Name, col => new List<string>(col.Values));
                    foreach (var column in tableData.Columns)
                    {
                        column.Values = originalValues[column.Name];
                    }
                }
            }
        }

        private static bool IsDateWithinRange(string dateString, DateTime startDate, DateTime endDate)
        {
            if (DateTime.TryParse(dateString, out DateTime date))
            {
                return date >= startDate && date <= endDate;
            }
            return false;
        }

        private static void ExcludeEmptyColumns(TableData tableData)
        {
            tableData.Columns = tableData.Columns
                .Where(col => col.Values.Any(value => !string.IsNullOrEmpty(value)))
                .ToList();
        }

        private static void SortByColumn(TableData tableData, string columnName, string sortOrder)
        {
            var columnToSort = tableData.Columns.FirstOrDefault(col => col.Name.ToLower() == columnName.ToLower());

            if (columnToSort != null)
            {
                var columnIndex = tableData.Columns.IndexOf(columnToSort);
                var sortedIndices = sortOrder.ToUpper() == "DESC"
                    ? Enumerable.Range(0, columnToSort.Values.Count).OrderByDescending(i => columnToSort.Values[i]).ToList()
                    : Enumerable.Range(0, columnToSort.Values.Count).OrderBy(i => columnToSort.Values[i]).ToList();

                foreach (var column in tableData.Columns)
                {
                    column.Values = sortedIndices.Select(i => column.Values[i]).ToList();
                }
            }
        }

        private static void ApplyRowLimit(TableData tableData, int rowLimit)
        {
            var minRowCount = tableData.Columns.Min(col => col.Values.Count);
            var limitedRowCount = Math.Min(rowLimit, minRowCount);

            foreach (var column in tableData.Columns)
            {
                column.Values = column.Values.Take(limitedRowCount).ToList();
            }
        }

        private static bool IsDateColumn(string columnName)
        {
            // Replace with your logic to identify date columns
           return columnName.Trim().ToLower().Equals("date");
        }

        private async Task<RestResponse<SpreadsheetData>> GenerateDataRequest(string spreadsheetId, string tab, string dimensionColumn, string metricColumn, string dateColumn, string accessToken)
        {
            // (X-Axis)
            // Line: Date column
            // Bar, Pie: Label column
            var ranges = $"{tab}!{dimensionColumn}:{dimensionColumn}";

            // (Y-Axis)
            // Data column
            var range1 = $"{tab}!{metricColumn}:{metricColumn}";

            var dateRange = $"{tab}!{dateColumn}:{dateColumn}";

            var requestForData = new RestRequest("", Method.Get);

            requestForData.AddHeader("Authorization", $"Bearer {accessToken}");

            requestForData.AddQueryParameter("ranges", ranges, false);
            requestForData.AddQueryParameter("ranges", range1, false);
            if (!string.IsNullOrEmpty(dateColumn))
            {
                requestForData.AddQueryParameter("ranges", dateRange, false);
            }
           
            requestForData.AddQueryParameter("valueRenderOption", "FORMATTED_VALUE", false);
            requestForData.AddQueryParameter("majorDimension", "COLUMNS", false);

            var options = new RestClientOptions($"https://sheets.googleapis.com/v4/spreadsheets/{spreadsheetId}/values:batchGet");

            var clientForData = new RestClient(options);
            var response = await clientForData.ExecuteAsync<SpreadsheetData>(requestForData);
            return response;
        }

        private async Task<RestResponse<SpreadsheetData>> GenerateDataRequestForSpark(string spreadsheetId, string tab, string metricColumn, string dateColumn, string accessToken)
        {

            // (Y-Axis)
            // Data column
            var range1 = $"{tab}!{metricColumn}:{metricColumn}";

            var dateRange = $"{tab}!{dateColumn}:{dateColumn}";

            var requestForData = new RestRequest("", Method.Get);

            requestForData.AddHeader("Authorization", $"Bearer {accessToken}");

            requestForData.AddQueryParameter("ranges", range1, false);
            requestForData.AddQueryParameter("ranges", dateRange, false);
            requestForData.AddQueryParameter("valueRenderOption", "FORMATTED_VALUE", false);
            requestForData.AddQueryParameter("majorDimension", "COLUMNS", false);

            var options = new RestClientOptions($"https://sheets.googleapis.com/v4/spreadsheets/{spreadsheetId}/values:batchGet");

            var clientForData = new RestClient(options);
            var response = await clientForData.ExecuteAsync<SpreadsheetData>(requestForData);
            return response;
        }


        private async Task<RestResponse<CellData>> GenerateDataRequestForTable(string spreadsheetId, string tab, string accessToken)
        {
            var requestForData = new RestRequest("", Method.Get);

            requestForData.AddHeader("Authorization", $"Bearer {accessToken}");
            requestForData.AddQueryParameter("valueRenderOption", "FORMATTED_VALUE", false);
            requestForData.AddQueryParameter("majorDimension", "COLUMNS", false);

            var options = new RestClientOptions($"https://sheets.googleapis.com/v4/spreadsheets/{spreadsheetId}/values/" + tab);

            var clientForData = new RestClient(options);
            var response = await clientForData.ExecuteAsync<CellData>(requestForData);
            return response;
        }

        private async Task<RestResponse<CellData>> GenerateDataRequestForSingleCell(string spreadsheetId, string tab, string cell, string accessToken)
        {

            var requestForData = new RestRequest("", Method.Get);

            requestForData.AddHeader("Authorization", $"Bearer {accessToken}");

            var options = new RestClientOptions($"https://sheets.googleapis.com/v4/spreadsheets/{spreadsheetId}/values/" + tab + "!" + cell);

            var clientForData = new RestClient(options);
            var response = await clientForData.ExecuteAsync<CellData>(requestForData);
            return response;
        }

        static (DateTime, DateTime)? ConvertToDateRange(string startDateString, string endDateString)
        {
            string format = "dd/MM/yyyy";

            DateTime? startDate = DateTime.TryParseExact(startDateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedStartDate)
                ? parsedStartDate
                : (DateTime?)null;

            DateTime? endDate = DateTime.TryParseExact(endDateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedEndDate)
                ? parsedEndDate
                : (DateTime?)null;

            if (startDate.HasValue && endDate.HasValue)
            {
                return (startDate.Value, endDate.Value);
            }

            return null;
        }

        public static List<(string, decimal?)> FillMissingDates(List<(DateTime?, decimal?)> data, DateTime startDate, DateTime endDate, string dateInterval)
        {
            List<DateTime> allDates = GenerateFormattedDates(startDate, endDate, dateInterval);
            Dictionary<DateTime, decimal> accumulatedProfit = new Dictionary<DateTime, decimal>();

            foreach (var (date, profit) in data)
            {             
                    DateTime keyDate = GetKeyDate(date, dateInterval);

                    if (accumulatedProfit.ContainsKey(keyDate))
                    {
                        // If date already exists, accumulate the profit
                        accumulatedProfit[keyDate] += profit ?? 0;
                    }
                    else
                    {
                        // Add new date and profit
                        accumulatedProfit[keyDate] = profit ?? 0;
                    }                             
            }

            var filledData = allDates.Select(date => (GetFormattedDate(date, dateInterval), GetAccumulatedProfit(date, accumulatedProfit, dateInterval))).ToList();
            return filledData;
        }

        public static string GetFormattedDate(DateTime date, string dateInterval)
        {
            return dateInterval.ToLower() == "monthly" ? date.ToString("MMM yyyy") : date.ToString("d MMM yyyy");
        }

        public static DateTime GetKeyDate(DateTime? date, string dateInterval)
        {
            if (!date.HasValue)
            {
                return DateTime.MinValue;
            }

            return dateInterval.ToLower() == "monthly" ? new DateTime(date.Value.Year, date.Value.Month, 1) : date.Value;
        }

        public static decimal? GetAccumulatedProfit(DateTime date, Dictionary<DateTime, decimal> accumulatedProfit, string dateInterval)
        {
            if (dateInterval.ToLower() == "monthly")
            {
                DateTime keyDate = new DateTime(date.Year, date.Month, 1);
                return accumulatedProfit.ContainsKey(keyDate) ? accumulatedProfit[keyDate] : (decimal?)0;
            }

            return accumulatedProfit.ContainsKey(date) ? accumulatedProfit[date] : (decimal?)0;
        }

        public static List<DateTime> GenerateFormattedDates(DateTime startDate, DateTime endDate, string dateInterval)
        {
            List<DateTime> formattedDates = new List<DateTime>();

            DateTime currentDate = startDate;

            while (currentDate <= endDate)
            {
                formattedDates.Add(currentDate);

                currentDate = dateInterval.ToLower() == "monthly" ? currentDate.AddMonths(1) : currentDate.AddDays(1);
            }

            return formattedDates;
        }

        static DateTime? ParseDate(string dateString)
        {
            DateTime parsedDate;

            string[] dateFormats = {
                 
                                     
                                        "d/MM/yyyy",             // 26/04/2023
                                        "d/M/yyyy",              // 26/4/2023                                                                       
                                        "dd/MM/yyyy",            // 01/06/2023
                                        "dd/M/yyyy",             // 01/6/2023
                                        "d MMMM yyyy",
                                        "d MMM yyyy",
                                        "M/d/yyyy",              // 4/26/2023
                                        "M/dd/yyyy",             // 4/26/2023
                                        "MM/dd/yyyy",            // 04/26/2023
                                        "MM/d/yyyy",             // 04/26/2023
                                        "MM/dd/yy",              // 04/26/23
                                        "MM/d/yy",               // 04/26/23
                                        "MMMM d, yyyy",          // April 26, 2023
                                        "MMMM d, yy",            // April 26, 23
                                        "yyyy/MM/dd",            // 2023/04/26
                                        "yyyy/M/d",              // 2023/4/26
                                        
                                        "MMMM d yyyy",           // July 1st 2023
                                        "MMMM d, yyyy",          // July 1, 2023
                                        "MM/dd/yyyy",            // 12/01/2023
                                        "yyyy/MM/dd",            // 2023/06/01
                                        
                                };

            if (DateTime.TryParseExact(dateString, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
            {
                return parsedDate;
            }

            return null;
        }

        static decimal? ExtractNumericValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return 0;
            }

            string cleanedValue = value.Replace("$", "").Replace(",", "").Trim();

            if (cleanedValue == "-")
            {
                return 0;
            }

            bool isNegative = cleanedValue.StartsWith("(") && cleanedValue.EndsWith(")");
            if (isNegative)
            {
                cleanedValue = "-" + cleanedValue.Substring(1, cleanedValue.Length - 2);
            }

            if (decimal.TryParse(cleanedValue, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal result))
            {
                // Check if there is a decimal part
                if (result == Math.Floor(result))
                {
                    // If no decimal part, return as integer
                    return (int)result;
                }
                else
                {
                    // If there is a decimal part, return as decimal
                    return result;
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
                        { "UpdatedOn", new PropertyMappingValue(new List<string>() { "UpdatedOn" } )},
                    };
        }

        public override string GetDefaultOrderByColumn()
        {
            return "UpdatedOn";
        }

        public override string GetDefaultFieldsToSelect()
        {
            return "Id,Name,AccountId,CampaignID,EmailId,Settings";
        }

        #endregion
    }
}
