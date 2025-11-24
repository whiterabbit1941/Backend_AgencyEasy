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
using Google.Api.Ads.AdWords.v201809;
using IdentityServer4.EntityFramework.Stores;
using IdentityServer4.Models;
using System.Net.Http.Headers;
using System.Net.Http;
using Microsoft.BingAds;
using IdentityServer4.Extensions;
using Microsoft.BingAds.V13.CustomerManagement;
using Predicate = Microsoft.BingAds.V13.CustomerManagement.Predicate;
using PredicateOperator = Microsoft.BingAds.V13.CustomerManagement.PredicateOperator;
using Paging = Microsoft.BingAds.V13.CustomerManagement.Paging;
using Microsoft.BingAds.V13.Reporting;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Stripe.Terminal;
using System.IO;
using System.Security.AccessControl;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Drawing;
using MailChimp.Net.Models;
using Microsoft.BingAds.V13.CampaignManagement;
using Microsoft.BingAds.V13.CustomerBilling;
using Google.Apis.Analytics.v3.Data;

namespace EventManagement.Service
{
    public class CampaignMicrosoftAdService : ServiceBase<CampaignMicrosoftAd, Guid>, ICampaignMicrosoftAdService
    {

        #region PRIVATE MEMBERS

        private readonly ICampaignMicrosoftAdRepository _campaignmicrosoftadRepository;
        private readonly IConfiguration _configuration;
        private static AuthorizationData _authorizationData;
        private readonly IHostingEnvironment _hostingEnvironment;

        #endregion


        #region CONSTRUCTOR

        public CampaignMicrosoftAdService(ICampaignMicrosoftAdRepository campaignmicrosoftadRepository,
            ILogger<CampaignMicrosoftAdService> logger, IConfiguration configuration, IHostingEnvironment hostingEnvironment) : base(campaignmicrosoftadRepository, logger)
        {
            _campaignmicrosoftadRepository = campaignmicrosoftadRepository;
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }

        #endregion


        #region PUBLIC MEMBERS   

        public async Task<List<MsAdAccountListDto>> GetMsAdAccountList(Guid campaignId)
        {
            var retVal = new List<MsAdAccountListDto>();

            try
            {
                var campaignMicrosoft = _campaignmicrosoftadRepository.GetAllEntities().Where(x => x.CampaignID == campaignId).FirstOrDefault();

                string redirectUri = _configuration.GetSection("HostedUrl").Value + "api/campaignmicrosoftads/MicrosoftAdsCode";

                var oAuthWebAuthCodeGrant = new OAuthWebAuthCodeGrant(
                  _configuration.GetSection("MicrosoftClientId").Value,
                  _configuration.GetSection("MicrosoftClientSeceret").Value,
                  new Uri(redirectUri),
                  ApiEnvironment.Production.ToString());

                //oAuthWebAuthCodeGrant.State = "12345";


                if (!string.IsNullOrEmpty(campaignMicrosoft.RefreshToken))
                {
                    await oAuthWebAuthCodeGrant.RequestAccessAndRefreshTokensAsync(campaignMicrosoft.RefreshToken);

                    // Serialize the object to JSON
                    string userObjectJson = JsonConvert.SerializeObject(oAuthWebAuthCodeGrant);
                }

                _authorizationData = new AuthorizationData
                {
                    DeveloperToken = "1457ONVLZ8754249",
                    Authentication = oAuthWebAuthCodeGrant
                };

                var _customerManagementService = new ServiceClient<ICustomerManagementService>(_authorizationData);

                var getUserRequest = new GetUserRequest
                {
                    UserId = null,
                    AuthenticationToken = campaignMicrosoft.AccessToken,
                    DeveloperToken = "1457ONVLZ8754249"
                };

                var getUserResponse =
                    (await _customerManagementService.CallAsync((s, r) => s.GetUserAsync(r), getUserRequest));
                var user = getUserResponse.User;

                var predicate = new Microsoft.BingAds.V13.CustomerManagement.Predicate
                {
                    Field = "UserId",
                    Operator = Microsoft.BingAds.V13.CustomerManagement.PredicateOperator.Equals,
                    Value = user.Id.ToString()
                };

                var paging = new Microsoft.BingAds.V13.CustomerManagement.Paging
                {
                    Index = 0,
                    Size = 10
                };

                var searchAccountsRequest = new SearchAccountsRequest
                {
                    Ordering = null,
                    PageInfo = paging,
                    Predicates = new[] { predicate },
                    DeveloperToken = "1457ONVLZ8754249",
                    ApplicationToken = "1457ONVLZ8754249",
                    AuthenticationToken = campaignMicrosoft.AccessToken
                };

                var searchAccountsResponse =
                    await _customerManagementService.CallAsync((s, r) => s.SearchAccountsAsync(r), searchAccountsRequest);

                var accounts = searchAccountsResponse.Accounts.ToArray();

                foreach (var account in accounts)
                {
                    var item = new MsAdAccountListDto();
                    item.AccountName = account.Name;
                    item.AccountId = account.Id;
                    item.CampaignId = campaignId;

                    retVal.Add(item);
                }

                return retVal;
            }
            catch (Exception ex)
            {
                var exception = ex;
            }

            return retVal;
        }
        public async Task<RootCampaignPerformace> GetCampaignPerformanceReport(Guid campaignId, string startDate, string endDate, long adCampaignId = 0)
        {
            var retVal = new List<CampaignPerformanceDto>();

            var retValPrev = new List<CampaignPerformanceDto>();

            var rootDto = new RootCampaignPerformace();

            var campaignMicrosoft = _campaignmicrosoftadRepository.GetAllEntities().Where(x => x.CampaignID == campaignId).FirstOrDefault();

            try
            {
                DateTime startDateDateTme = Convert.ToDateTime(startDate, CultureInfo.GetCultureInfo("hi-IN").DateTimeFormat);
                DateTime endDateDateTme = Convert.ToDateTime(endDate, CultureInfo.GetCultureInfo("hi-IN").DateTimeFormat);

                string redirectUri = _configuration.GetSection("HostedUrl").Value + "api/campaignmicrosoftads/MicrosoftAdsCode";

                var oAuthWebAuthCodeGrant = new OAuthWebAuthCodeGrant(
                  _configuration.GetSection("MicrosoftClientId").Value,
                  _configuration.GetSection("MicrosoftClientSeceret").Value,
                  new Uri(redirectUri),
                  ApiEnvironment.Production.ToString());

                if (!string.IsNullOrEmpty(campaignMicrosoft.RefreshToken))
                {
                    await oAuthWebAuthCodeGrant.RequestAccessAndRefreshTokensAsync(campaignMicrosoft.RefreshToken);

                    // Serialize the object to JSON
                    string userObjectJson = JsonConvert.SerializeObject(oAuthWebAuthCodeGrant);
                }
                // Create an instance of HttpRequestHeaders
                HttpRequestHeaders requestHeaders = new HttpRequestMessage().Headers;

                _authorizationData = new AuthorizationData
                {
                    DeveloperToken = "1457ONVLZ8754249",
                    Authentication = oAuthWebAuthCodeGrant
                };

                _authorizationData.AccountId = long.Parse(campaignMicrosoft.AccountId);
                //_authorizationData.CustomerId = (int)accounts[1].ParentCustomerId;


                //Get reports
                var reportRequest = new CampaignPerformanceReportRequest
                {
                    ReportName = "CampaignPerformanceReport",
                    ExcludeColumnHeaders = false,
                    ExcludeReportFooter = true,
                    ExcludeReportHeader = true,


                    Format = ReportFormat.Csv,
                    Aggregation = ReportAggregation.Daily,
                    ReturnOnlyCompleteData = false,
                    Columns = new[]
                    {
                        CampaignPerformanceReportColumn.CampaignId,
                        CampaignPerformanceReportColumn.CampaignName,
                        CampaignPerformanceReportColumn.CampaignStatus,
                        CampaignPerformanceReportColumn.Impressions,
                        CampaignPerformanceReportColumn.TimePeriod,

                        CampaignPerformanceReportColumn.AverageCpc,
                        CampaignPerformanceReportColumn.Ctr,
                        CampaignPerformanceReportColumn.Clicks,
                        CampaignPerformanceReportColumn.ConversionRate,
                        CampaignPerformanceReportColumn.Conversions,
                        CampaignPerformanceReportColumn.CostPerConversion,
                        CampaignPerformanceReportColumn.ImpressionSharePercent,
                        CampaignPerformanceReportColumn.ImpressionLostToBudgetPercent,
                        CampaignPerformanceReportColumn.ImpressionLostToRankAggPercent,
                        CampaignPerformanceReportColumn.AccountStatus,
                        CampaignPerformanceReportColumn.Spend,
                        CampaignPerformanceReportColumn.AllCostPerConversion,

                        CampaignPerformanceReportColumn.TopImpressionSharePercent,
                        CampaignPerformanceReportColumn.ExactMatchImpressionSharePercent,
                        CampaignPerformanceReportColumn.AbsoluteTopImpressionSharePercent,

                    },
                    Scope = new AccountThroughCampaignReportScope
                    {
                        AccountIds = new List<long>() { long.Parse(campaignMicrosoft.AccountId) },

                    },
                    //Filter = new CampaignPerformanceReportFilter()
                    //{
                    //    Status = CampaignStatusReportFilter.Active | CampaignStatusReportFilter.Paused | CampaignStatusReportFilter.BudgetPaused | CampaignStatusReportFilter.Suspended,

                    //},
                    Time = new ReportTime
                    {
                        CustomDateRangeStart = new Microsoft.BingAds.V13.Reporting.Date() { Day = startDateDateTme.Day, Month = startDateDateTme.Month, Year = startDateDateTme.Year },
                        CustomDateRangeEnd = new Microsoft.BingAds.V13.Reporting.Date() { Day = endDateDateTme.Day, Month = endDateDateTme.Month, Year = endDateDateTme.Year },
                        //ReportTimeZone = ReportTimeZone.EasternTimeUSCanada
                    }

                };


                var fileName = DateTime.UtcNow.Ticks.ToString() + ".csv";

                // Get the content root path on AWS Elastic Beanstalk
                string contentRootPath = Directory.GetCurrentDirectory();

                string path = Path.Combine(_hostingEnvironment.ContentRootPath, "MicrosoftTempFile");

                //string path = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/" + fileName);

                var reportingDownloadParameters = new ReportingDownloadParameters
                {
                    ReportRequest = reportRequest,
                    ResultFileName = fileName,
                    OverwriteResultFile = false,
                    ResultFileDirectory = path,

                };

                //_authorizationData.AccountId = long.Parse(campaignMicrosoft.AccountId);
                //_authorizationData.CustomerId = (int)accounts[1].ParentCustomerId;

                ReportingServiceManager reportingServiceManager = new ReportingServiceManager(_authorizationData, ApiEnvironment.Production);

                reportingServiceManager.DownloadHttpTimeout = new TimeSpan(0, 10, 0);

                reportingServiceManager.WorkingDirectory = path;

                // Sets the time interval in milliseconds between two status polling attempts. The default value is 5000 (5 seconds).
                //reportingServiceManager.StatusPollIntervalInMilliseconds = 5000;

                var report = await reportingServiceManager.DownloadReportAsync(reportingDownloadParameters, CancellationToken.None);

                if (report != null)
                {
                    report.Dispose();

                    //var htmlString = System.IO.File.ReadAllText(path + "/" + fileName);    

                    using (var reader = new StreamReader(path + "/" + fileName))
                    using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                    {
                        var csvFieldHeaders = csv.HeaderRecord;
                        var dtoProperties = typeof(CampaignPerformanceDto).GetProperties()
                                           .Select(property => property.Name)
                                           .ToArray();

                        // Read CSV records
                        var records = csv.GetRecords<CampaignPerformanceDto>().ToList();

                        retVal = records;

                        if (File.Exists(path + "/" + fileName))
                        {
                            reader.Dispose();
                            reportingServiceManager.CleanupTempFiles();

                        }
                        // Convert records to JSON
                        //string jsonResult = JsonConvert.DeserializeObject<List<CampaignPerformanceDto>>(records);

                    }

                    //Filter by ms ads campaignid 
                    if (adCampaignId > 0)
                    {
                        retVal = retVal.Where(x => x.CampaignId == adCampaignId).ToList();
                    }

                    var clicksum = (decimal)retVal.Sum(x => x.Clicks);

                    var impressionsum = (decimal)retVal.Sum(x => x.Impressions);

                    var totalCost = retVal.Sum(x => x.Spend);

                    var totalConversion = retVal.Sum(x => x.Conversions);

                    rootDto.ctr = impressionsum > 0 ? ((clicksum / impressionsum) * 100).ToString("0.00") + "%" : "0%";

                    rootDto.clicks = clicksum.ToString();

                    rootDto.impressions = impressionsum.ToString();

                    rootDto.spend = retVal.Sum(x => x.Spend);

                    rootDto.conversions = retVal.Sum(x => x.Conversions);

                    rootDto.averageCpc = clicksum > 0 ? (totalCost / clicksum).ToString("0.00") + "%" : "0";

                    rootDto.conversionRate = retVal.Sum(x => ConvertPercentageToDecimal(x.ConversionRate)).ToString("0.00") + "%";

                    //remaining no match
                    rootDto.costPerConversion = totalConversion > 0 ? Math.Round(totalCost / totalConversion, 2).ToString() : "0";

                    rootDto.impressionSharePercent = Math.Round(retVal.Where(x => !string.IsNullOrEmpty(x.ImpressionSharePercent) && ConvertPercentageToDecimal(x.ImpressionSharePercent) > 0)
                                                       .Select(x => ConvertPercentageToDecimal(x.ImpressionSharePercent))
                                                       .DefaultIfEmpty()
                                                       .Average(), 2)
                                                       .ToString() ?? "0";

                    rootDto.impressionLostToBudgetPercent = Math.Round(retVal
                     .Where(x => !string.IsNullOrEmpty(x.ImpressionLostToBudgetPercent) && ConvertPercentageToDecimal(x.ImpressionLostToBudgetPercent) > 0)
                     .Select(x => ConvertPercentageToDecimal(x.ImpressionLostToBudgetPercent))
                     .DefaultIfEmpty()
                     .Average(), 2)
                     .ToString() ?? "0";

                    rootDto.impressionLostToRankAggPercent = Math.Round(retVal
                        .Where(x => !string.IsNullOrEmpty(x.ImpressionLostToRankAggPercent) && ConvertPercentageToDecimal(x.ImpressionLostToRankAggPercent) > 0)
                        .Select(x => ConvertPercentageToDecimal(x.ImpressionLostToRankAggPercent))
                        .DefaultIfEmpty()
                        .Average(), 2)
                        .ToString() ?? "0";

                    // Assuming your existing data is in the variable existingData
                    //DateTime startDate = new DateTime(2023, 11, 22);
                    //DateTime endDate = new DateTime(2023, 12, 21);

                    //Line Chart
                    var newData = AddMissingDates(retVal, startDateDateTme, endDateDateTme);


                    rootDto.dates = newData.Select(x => x.TimePeriod.Day <= 1
                                     ? x.TimePeriod.ToString("MMM") // Display month name for 1st day of the month
                                     : x.TimePeriod.Day.ToString() + " " + x.TimePeriod.ToString("MMM"))  // Display day for other days
                                     .ToList();

                    rootDto.clickChartValue = newData.Select(x => x.Clicks).ToList();

                    rootDto.impressionLineChartValue = newData.Select(x => x.Impressions).ToList();

                    rootDto.ctrLineChartValue = newData.Select(x => ConvertPercentageToDecimal(x.Ctr)).ToList();

                    rootDto.avgcpcLineChartValue = newData.Select(x => ConvertPercentageToDecimal(x.AverageCpc)).ToList();

                    rootDto.costLineChartValue = newData.Select(x => x.Spend).ToList();

                    rootDto.conversionLineChartValue = newData.Select(x => x.Conversions).ToList();

                    rootDto.conversionRateLineChartValue = newData.Select(x => ConvertPercentageToDecimal(x.ConversionRate)).ToList();

                    rootDto.costPerConversionLineChartValue = newData.Select(x => ConvertPercentageToDecimal(x.CostPerConversion)).ToList();

                    rootDto.impressionShareLineChartValue = newData.Select(x => ConvertPercentageToDecimal(x.ImpressionSharePercent)).ToList();

                    rootDto.impressionShareBudgetLineChartValue = newData.Select(x => ConvertPercentageToDecimal(x.ImpressionLostToBudgetPercent)).ToList();

                    rootDto.impressionShareRankLineChartValue = newData.Select(x => ConvertPercentageToDecimal(x.ImpressionLostToRankAggPercent)).ToList();

                    //Make groupby campign id table
                    // Assuming retVal is a List<CampaignPerformanceDto>
                    var groupedData = retVal
                                     .GroupBy(x => x.CampaignId)
                                     .Select(group => new CampaignPerformanceDto()
                                     {
                                         CampaignId = group.Key,
                                         CampaignStatus = group.Select(x => x.CampaignStatus).FirstOrDefault(),
                                         CampaignName = group.Select(x => x.CampaignName).FirstOrDefault(),
                                         Clicks = group.Sum(x => x.Clicks),
                                         Impressions = group.Sum(x => x.Impressions),
                                         Spend = group.Sum(x => x.Spend),
                                         Conversions = group.Sum(x => x.Conversions),
                                         AverageCpc = (group.Sum(x => x.Clicks) > 0) ? (group.Sum(x => x.Spend) / group.Sum(x => x.Clicks)).ToString("0.00") : "0.00",

                                         Ctr = (group.Sum(x => x.Impressions) > 0) ?
                                             (((decimal)group.Sum(x => x.Clicks) / group.Sum(x => x.Impressions)) * 100m).ToString("0.00") : "0.00",

                                         CostPerConversion = (group.Sum(x => x.Conversions) > 0) ?
                                             (group.Sum(x => x.Spend) / group.Sum(x => x.Conversions)).ToString("0.00") : "0.00",

                                         ImpressionSharePercent = (group.Any() && group.All(x => ConvertPercentageToDecimal(x.ImpressionSharePercent) > 0)) ?
                                             Math.Round(group.Average(x => ConvertPercentageToDecimal(x.ImpressionSharePercent)), 2).ToString() : "0.00",

                                         ImpressionLostToBudgetPercent = (group.Any()) ?
                                             Math.Round(group.Average(x => ConvertPercentageToDecimal(x.ImpressionLostToBudgetPercent)), 2).ToString() : "0.00",

                                         ImpressionLostToRankAggPercent = (group.Any() && group.All(x => ConvertPercentageToDecimal(x.ImpressionLostToRankAggPercent) > 0)) ?
                                             Math.Round(group.Average(x => ConvertPercentageToDecimal(x.ImpressionLostToRankAggPercent)), 2).ToString() : "0.00",

                                         ConversionRate = (group.Sum(x => ConvertPercentageToDecimal(x.ConversionRate)) > 0) ?
                                             group.Sum(x => ConvertPercentageToDecimal(x.ConversionRate)).ToString("0.00") : "0.00",
                                     })
                                     .ToList();


                    rootDto.campaignPerformanceDto = groupedData;

                    //Bar Chart
                    rootDto.campaignsName = groupedData.Select(x => x.CampaignName).ToList();

                    rootDto.clickBarChartValue = groupedData.Select(x => x.Clicks).ToList();

                    rootDto.impressionBarChartValue = groupedData.Select(x => x.Impressions).ToList();

                    rootDto.ctrBarChartValue = groupedData.Select(x => ConvertPercentageToDecimal(x.Ctr)).ToList();

                    rootDto.avgcpcBarChartValue = groupedData.Select(x => ConvertPercentageToDecimal(x.AverageCpc)).ToList();

                    rootDto.costBarChartValue = groupedData.Select(x => x.Spend).ToList();

                    rootDto.conversionBarChartValue = groupedData.Select(x => x.Conversions).ToList();

                    rootDto.conversionRateBarChartValue = groupedData.Select(x => ConvertPercentageToDecimal(x.ConversionRate)).ToList();

                    rootDto.costPerConversionBarChartValue = groupedData.Select(x => ConvertPercentageToDecimal(x.CostPerConversion)).ToList();

                    rootDto.impressionShareBarChartValue = groupedData.Select(x => ConvertPercentageToDecimal(x.ImpressionSharePercent)).ToList();

                    rootDto.impressionShareBudgetBarChartValue = groupedData.Select(x => ConvertPercentageToDecimal(x.ImpressionLostToBudgetPercent)).ToList();

                    rootDto.impressionShareRankBarChartValue = groupedData.Select(x => ConvertPercentageToDecimal(x.ImpressionLostToRankAggPercent)).ToList();
                }

                //Get the previous data

                ReportingServiceManager reportingServiceManagerForPrev = new ReportingServiceManager(_authorizationData, ApiEnvironment.Production);

                reportingServiceManagerForPrev.DownloadHttpTimeout = new TimeSpan(0, 10, 0);

                reportingServiceManagerForPrev.WorkingDirectory = path;

                var previousDate = CalculatePreviousStartDateAndEndDate(startDateDateTme, endDateDateTme);

                var fileNameForPrev = DateTime.UtcNow.Ticks.ToString() + ".csv";

                var reportRequestForPrev = new CampaignPerformanceReportRequest
                {
                    ReportName = "CampaignPerformanceReport",
                    ExcludeColumnHeaders = false,
                    ExcludeReportFooter = true,
                    ExcludeReportHeader = true,


                    Format = ReportFormat.Csv,
                    Aggregation = ReportAggregation.Daily,
                    ReturnOnlyCompleteData = false,
                    Columns = new[]
                  {
                        CampaignPerformanceReportColumn.CampaignId,
                        CampaignPerformanceReportColumn.CampaignName,
                        CampaignPerformanceReportColumn.CampaignStatus,
                        CampaignPerformanceReportColumn.Impressions,
                        CampaignPerformanceReportColumn.TimePeriod,

                        CampaignPerformanceReportColumn.AverageCpc,
                        CampaignPerformanceReportColumn.Ctr,
                        CampaignPerformanceReportColumn.Clicks,
                        CampaignPerformanceReportColumn.ConversionRate,
                        CampaignPerformanceReportColumn.Conversions,
                        CampaignPerformanceReportColumn.CostPerConversion,
                        CampaignPerformanceReportColumn.ImpressionSharePercent,
                        CampaignPerformanceReportColumn.ImpressionLostToBudgetPercent,
                        CampaignPerformanceReportColumn.ImpressionLostToRankAggPercent,
                        CampaignPerformanceReportColumn.AccountStatus,
                        CampaignPerformanceReportColumn.Spend,
                        CampaignPerformanceReportColumn.AllCostPerConversion,

                        CampaignPerformanceReportColumn.TopImpressionSharePercent,
                        CampaignPerformanceReportColumn.ExactMatchImpressionSharePercent,
                        CampaignPerformanceReportColumn.AbsoluteTopImpressionSharePercent,

                    },
                    Scope = new AccountThroughCampaignReportScope
                    {
                        AccountIds = new List<long>() { long.Parse(campaignMicrosoft.AccountId) },

                    },
                    Time = new ReportTime
                    {
                        CustomDateRangeStart = new Microsoft.BingAds.V13.Reporting.Date() { Day = previousDate.PreviousStartDate.Day, Month = previousDate.PreviousStartDate.Month, Year = previousDate.PreviousStartDate.Year },
                        CustomDateRangeEnd = new Microsoft.BingAds.V13.Reporting.Date() { Day = previousDate.PreviousEndDate.Day, Month = previousDate.PreviousEndDate.Month, Year = previousDate.PreviousEndDate.Year },
                    }

                };

                var reportingDownloadParamForPrev = new ReportingDownloadParameters
                {
                    ReportRequest = reportRequestForPrev,
                    ResultFileName = fileNameForPrev,
                    OverwriteResultFile = false,
                    ResultFileDirectory = path,
                };

                var prevReport = await reportingServiceManager.DownloadReportAsync(reportingDownloadParamForPrev, CancellationToken.None);

                if (prevReport != null)
                {
                    prevReport.Dispose();

                    using (var reader = new StreamReader(path + "/" + fileNameForPrev))
                    using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                    {
                        var csvFieldHeaders = csv.HeaderRecord;
                        var dtoProperties = typeof(CampaignPerformanceDto).GetProperties()
                                           .Select(property => property.Name)
                                           .ToArray();

                        // Read CSV records
                        var records = csv.GetRecords<CampaignPerformanceDto>().ToList();

                        retValPrev = records;

                        if (File.Exists(path + "/" + fileNameForPrev))
                        {
                            reader.Dispose();
                            reportingServiceManager.CleanupTempFiles();

                        }
                        // Convert records to JSON
                        //string jsonResult = JsonConvert.DeserializeObject<List<CampaignPerformanceDto>>(records);

                    }

                    //Filter by ms ads campaignid 
                    if (adCampaignId > 0)
                    {
                        retValPrev = retValPrev.Where(x => x.CampaignId == adCampaignId).ToList();
                    }

                    var clicksum = (decimal)retValPrev.Sum(x => x.Clicks);

                    var impressionsum = (decimal)retValPrev.Sum(x => x.Impressions);

                    var totalCost = retValPrev.Sum(x => x.Spend);

                    var totalConversion = retValPrev.Sum(x => x.Conversions);

                    rootDto.PrevCtr = impressionsum > 0 ? ((clicksum / impressionsum) * 100).ToString("0.00") + "%" : "0%";

                    rootDto.PrevClicks = clicksum.ToString();

                    rootDto.PrevImpressions = impressionsum.ToString();

                    rootDto.PrevSpend = retValPrev.Sum(x => x.Spend);

                    rootDto.PrevConversions = retValPrev.Sum(x => x.Conversions);

                    rootDto.PrevAverageCpc = clicksum > 0 ? (totalCost / clicksum).ToString("0.00") + "%" : "0";

                    rootDto.PrevConversionRate = retValPrev.Sum(x => ConvertPercentageToDecimal(x.ConversionRate)).ToString("0.00") + "%";

                    //remaining no match
                    rootDto.PrevCostPerConversion = totalConversion > 0 ? Math.Round(totalCost / totalConversion, 2).ToString() : "0";

                    rootDto.PrevImpressionSharePercent = Math.Round(retValPrev.Where(x => !string.IsNullOrEmpty(x.ImpressionSharePercent) && ConvertPercentageToDecimal(x.ImpressionSharePercent) > 0)
                                                       .Select(x => ConvertPercentageToDecimal(x.ImpressionSharePercent))
                                                       .DefaultIfEmpty()
                                                       .Average(), 2)
                                                       .ToString() ?? "0";

                    rootDto.PrevImpressionLostToBudgetPercent = Math.Round(retValPrev
                     .Where(x => !string.IsNullOrEmpty(x.ImpressionLostToBudgetPercent) && ConvertPercentageToDecimal(x.ImpressionLostToBudgetPercent) > 0)
                     .Select(x => ConvertPercentageToDecimal(x.ImpressionLostToBudgetPercent))
                     .DefaultIfEmpty()
                     .Average(), 2)
                     .ToString() ?? "0";

                    rootDto.PrevImpressionLostToRankAggPercent = Math.Round(retValPrev
                        .Where(x => !string.IsNullOrEmpty(x.ImpressionLostToRankAggPercent) && ConvertPercentageToDecimal(x.ImpressionLostToRankAggPercent) > 0)
                        .Select(x => ConvertPercentageToDecimal(x.ImpressionLostToRankAggPercent))
                        .DefaultIfEmpty()
                        .Average(), 2)
                        .ToString() ?? "0";

                    //Calculate Difference
                    rootDto.DiffCtr = CalculateCardPercentage(rootDto.ctr, rootDto.PrevCtr);
                    rootDto.DiffImpressions = CalculateCardPercentage(rootDto.impressions, rootDto.PrevImpressions);
                    rootDto.DiffAverageCpc = CalculateCardPercentage(rootDto.averageCpc, rootDto.PrevAverageCpc);
                    rootDto.DiffCtr = CalculateCardPercentage(rootDto.ctr, rootDto.PrevCtr);
                    rootDto.DiffClicks = CalculateCardPercentage(rootDto.clicks, rootDto.PrevClicks);
                    rootDto.DiffSpend = CalculateCardPercentage(rootDto.spend.ToString(), rootDto.PrevSpend.ToString());
                    rootDto.DiffConversionRate = CalculateCardPercentage(rootDto.conversionRate, rootDto.PrevConversionRate);
                    rootDto.DiffConversions = CalculateCardPercentage(rootDto.conversions.ToString(), rootDto.PrevConversions.ToString());
                    rootDto.DiffCostPerConversion = CalculateCardPercentage(rootDto.costPerConversion, rootDto.PrevCostPerConversion);
                    rootDto.DiffImpressionSharePercent = CalculateCardPercentage(rootDto.impressionSharePercent, rootDto.PrevImpressionSharePercent);
                    rootDto.DiffImpressionLostToBudgetPercent = CalculateCardPercentage(rootDto.impressionLostToBudgetPercent, rootDto.PrevImpressionLostToBudgetPercent);
                    rootDto.DiffImpressionLostToRankAggPercent = CalculateCardPercentage(rootDto.impressionLostToRankAggPercent, rootDto.PrevImpressionLostToRankAggPercent);
                }
                else
                {
                    //Filter by ms ads campaignid 
                    if (adCampaignId > 0)
                    {
                        retValPrev = retValPrev.Where(x => x.CampaignId == adCampaignId).ToList();
                    }

                    var clicksum = (decimal)retValPrev.Sum(x => x.Clicks);

                    var impressionsum = (decimal)retValPrev.Sum(x => x.Impressions);

                    var totalCost = retValPrev.Sum(x => x.Spend);

                    var totalConversion = retValPrev.Sum(x => x.Conversions);

                    rootDto.PrevCtr = impressionsum > 0 ? ((clicksum / impressionsum) * 100).ToString("0.00") + "%" : "0%";

                    rootDto.PrevClicks = clicksum.ToString();

                    rootDto.PrevImpressions = impressionsum.ToString();

                    rootDto.PrevSpend = retValPrev.Sum(x => x.Spend);

                    rootDto.PrevConversions = retValPrev.Sum(x => x.Conversions);

                    rootDto.PrevAverageCpc = clicksum > 0 ? (totalCost / clicksum).ToString("0.00") + "%" : "0";

                    rootDto.PrevConversionRate = retValPrev.Sum(x => ConvertPercentageToDecimal(x.ConversionRate)).ToString("0.00") + "%";

                    //remaining no match
                    rootDto.PrevCostPerConversion = totalConversion > 0 ? Math.Round(totalCost / totalConversion, 2).ToString() : "0";

                    rootDto.PrevImpressionSharePercent = Math.Round(retValPrev.Where(x => !string.IsNullOrEmpty(x.ImpressionSharePercent) && ConvertPercentageToDecimal(x.ImpressionSharePercent) > 0)
                                                       .Select(x => ConvertPercentageToDecimal(x.ImpressionSharePercent))
                                                       .DefaultIfEmpty()
                                                       .Average(), 2)
                                                       .ToString() ?? "0";

                    rootDto.PrevImpressionLostToBudgetPercent = Math.Round(retValPrev
                     .Where(x => !string.IsNullOrEmpty(x.ImpressionLostToBudgetPercent) && ConvertPercentageToDecimal(x.ImpressionLostToBudgetPercent) > 0)
                     .Select(x => ConvertPercentageToDecimal(x.ImpressionLostToBudgetPercent))
                     .DefaultIfEmpty()
                     .Average(), 2)
                     .ToString() ?? "0";

                    rootDto.PrevImpressionLostToRankAggPercent = Math.Round(retValPrev
                        .Where(x => !string.IsNullOrEmpty(x.ImpressionLostToRankAggPercent) && ConvertPercentageToDecimal(x.ImpressionLostToRankAggPercent) > 0)
                        .Select(x => ConvertPercentageToDecimal(x.ImpressionLostToRankAggPercent))
                        .DefaultIfEmpty()
                        .Average(), 2)
                        .ToString() ?? "0";

                    //Calculate Difference
                    rootDto.DiffCtr = CalculateCardPercentage(rootDto.ctr, rootDto.PrevCtr);
                    rootDto.DiffImpressions = CalculateCardPercentage(rootDto.impressions, rootDto.PrevImpressions);
                    rootDto.DiffAverageCpc = CalculateCardPercentage(rootDto.averageCpc, rootDto.PrevAverageCpc);
                    rootDto.DiffCtr = CalculateCardPercentage(rootDto.ctr, rootDto.PrevCtr);
                    rootDto.DiffClicks = CalculateCardPercentage(rootDto.clicks, rootDto.PrevClicks);
                    rootDto.DiffSpend = CalculateCardPercentage(rootDto.spend.ToString(), rootDto.PrevSpend.ToString());
                    rootDto.DiffConversionRate = CalculateCardPercentage(rootDto.conversionRate, rootDto.PrevConversionRate);
                    rootDto.DiffConversions = CalculateCardPercentage(rootDto.conversions.ToString(), rootDto.PrevConversions.ToString());
                    rootDto.DiffCostPerConversion = CalculateCardPercentage(rootDto.costPerConversion, rootDto.PrevCostPerConversion);
                    rootDto.DiffImpressionSharePercent = CalculateCardPercentage(rootDto.impressionSharePercent, rootDto.PrevImpressionSharePercent);
                    rootDto.DiffImpressionLostToBudgetPercent = CalculateCardPercentage(rootDto.impressionLostToBudgetPercent, rootDto.PrevImpressionLostToBudgetPercent);
                    rootDto.DiffImpressionLostToRankAggPercent = CalculateCardPercentage(rootDto.impressionLostToRankAggPercent, rootDto.PrevImpressionLostToRankAggPercent);
                }

            }
            catch (Exception ex)
            {
                var test = ex.StackTrace;
                rootDto.errorMessage = ex.StackTrace + "   Message: " + ex.Message;
            }

            return rootDto;
        }
        public async Task<RootAdGroupPerformance> GetAdGroupPerformanceReport(Guid campaignId, string startDate, string endDate, long adCampaignId = 0)
        {
            var retVal = new List<AdGroupPerformanceDto>();

            var retValPrev = new List<AdGroupPerformanceDto>();

            var rootDto = new RootAdGroupPerformance();

            var campaignMicrosoft = _campaignmicrosoftadRepository.GetAllEntities().Where(x => x.CampaignID == campaignId).FirstOrDefault();

            try
            {
                DateTime startDateDateTme = Convert.ToDateTime(startDate, CultureInfo.GetCultureInfo("hi-IN").DateTimeFormat);
                DateTime endDateDateTme = Convert.ToDateTime(endDate, CultureInfo.GetCultureInfo("hi-IN").DateTimeFormat);

                string redirectUri = _configuration.GetSection("HostedUrl").Value + "api/campaignmicrosoftads/MicrosoftAdsCode";

                var oAuthWebAuthCodeGrant = new OAuthWebAuthCodeGrant(
                  _configuration.GetSection("MicrosoftClientId").Value,
                  _configuration.GetSection("MicrosoftClientSeceret").Value,
                  new Uri(redirectUri),
                  ApiEnvironment.Production.ToString());

                if (!string.IsNullOrEmpty(campaignMicrosoft.RefreshToken))
                {
                    await oAuthWebAuthCodeGrant.RequestAccessAndRefreshTokensAsync(campaignMicrosoft.RefreshToken);

                    // Serialize the object to JSON
                    string userObjectJson = JsonConvert.SerializeObject(oAuthWebAuthCodeGrant);
                }
                // Create an instance of HttpRequestHeaders
                HttpRequestHeaders requestHeaders = new HttpRequestMessage().Headers;

                _authorizationData = new AuthorizationData
                {
                    DeveloperToken = "1457ONVLZ8754249",
                    Authentication = oAuthWebAuthCodeGrant
                };

                _authorizationData.AccountId = long.Parse(campaignMicrosoft.AccountId);
                //_authorizationData.CustomerId = (int)accounts[1].ParentCustomerId;

                //Get reports
                var reportRequest = new AdGroupPerformanceReportRequest
                {
                    ReportName = "AdGroupPerformanceReport",
                    ExcludeColumnHeaders = false,
                    ExcludeReportFooter = true,
                    ExcludeReportHeader = true,

                    Format = ReportFormat.Csv,
                    Aggregation = ReportAggregation.Daily,
                    ReturnOnlyCompleteData = false,

                    Columns = new[]
                    {
                        AdGroupPerformanceReportColumn.CampaignId,
                        AdGroupPerformanceReportColumn.AdGroupId,
                        AdGroupPerformanceReportColumn.AdGroupName,
                        AdGroupPerformanceReportColumn.CampaignName,
                        AdGroupPerformanceReportColumn.Status,

                        AdGroupPerformanceReportColumn.Impressions,
                        AdGroupPerformanceReportColumn.AverageCpc,
                        AdGroupPerformanceReportColumn.Spend,
                        AdGroupPerformanceReportColumn.Ctr,
                        AdGroupPerformanceReportColumn.Clicks,
                        AdGroupPerformanceReportColumn.ConversionRate,
                        AdGroupPerformanceReportColumn.Conversions,
                        AdGroupPerformanceReportColumn.CostPerConversion,
                        AdGroupPerformanceReportColumn.ImpressionSharePercent,
                        AdGroupPerformanceReportColumn.ImpressionLostToBudgetPercent,
                        AdGroupPerformanceReportColumn.ImpressionLostToRankAggPercent,
                        AdGroupPerformanceReportColumn.TimePeriod,

                    },
                    Scope = new AccountThroughAdGroupReportScope
                    {
                        AccountIds = new List<long>() { long.Parse(campaignMicrosoft.AccountId) },

                    },
                    //Filter = new AdGroupPerformanceReportFilter()
                    //{
                    //    Status = AdGroupStatusReportFilter.Active | AdGroupStatusReportFilter.Paused,

                    //},
                    Time = new ReportTime
                    {
                        CustomDateRangeStart = new Microsoft.BingAds.V13.Reporting.Date() { Day = startDateDateTme.Day, Month = startDateDateTme.Month, Year = startDateDateTme.Year },
                        CustomDateRangeEnd = new Microsoft.BingAds.V13.Reporting.Date() { Day = endDateDateTme.Day, Month = endDateDateTme.Month, Year = endDateDateTme.Year },
                        //ReportTimeZone = ReportTimeZone.EasternTimeUSCanada
                    },

                };

                var fileName = DateTime.UtcNow.Ticks.ToString() + ".csv";

                // Get the content root path on AWS Elastic Beanstalk
                string contentRootPath = Directory.GetCurrentDirectory();

                string path = Path.Combine(_hostingEnvironment.ContentRootPath, "MicrosoftTempFile");

                //string path = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/" + fileName);

                var reportingDownloadParameters = new ReportingDownloadParameters
                {
                    ReportRequest = reportRequest,
                    ResultFileName = fileName,
                    OverwriteResultFile = false,
                    ResultFileDirectory = path
                };

                _authorizationData.AccountId = long.Parse(campaignMicrosoft.AccountId);
                //_authorizationData.CustomerId = (int)accounts[1].ParentCustomerId;

                ReportingServiceManager reportingServiceManager = new ReportingServiceManager(_authorizationData, ApiEnvironment.Production);

                reportingServiceManager.DownloadHttpTimeout = new TimeSpan(0, 10, 0);

                reportingServiceManager.WorkingDirectory = path;

                // Sets the time interval in milliseconds between two status polling attempts. The default value is 5000 (5 seconds).
                //reportingServiceManager.StatusPollIntervalInMilliseconds = 5000;

                var report = await reportingServiceManager.DownloadReportAsync(reportingDownloadParameters, CancellationToken.None);

                if (report != null)
                {
                    report.Dispose();

                    //var htmlString = System.IO.File.ReadAllText(path + "/" + fileName);    

                    using (var reader = new StreamReader(path + "/" + fileName))
                    using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                    {
                        // Read CSV records
                        var records = csv.GetRecords<AdGroupPerformanceDto>().ToList();

                        retVal = records;

                        if (File.Exists(path + "/" + fileName))
                        {
                            reader.Dispose();
                            reportingServiceManager.CleanupTempFiles();

                        }

                    }

                    //Filter by ms ads campaignid 
                    if (adCampaignId > 0)
                    {
                        retVal = retVal.Where(x => x.CampaignId == adCampaignId).ToList();
                    }

                    var clicksum = (decimal)retVal.Sum(x => x.Clicks);

                    var impressionsum = (decimal)retVal.Sum(x => x.Impressions);

                    var totalCost = retVal.Sum(x => x.Spend);

                    var totalConversion = retVal.Sum(x => x.Conversions);

                    rootDto.ctr = impressionsum > 0 ? ((clicksum / impressionsum) * 100).ToString("0.00") + "%" : "0%";

                    rootDto.clicks = clicksum.ToString();

                    rootDto.impressions = impressionsum.ToString();

                    rootDto.spend = retVal.Sum(x => x.Spend);

                    rootDto.conversions = retVal.Sum(x => x.Conversions);

                    rootDto.averageCpc = clicksum > 0 ? (totalCost / clicksum).ToString("0.00") + "%" : "0";


                    rootDto.conversionRate = retVal.Sum(x => ConvertPercentageToDecimal(x.ConversionRate)).ToString();

                    //remaining no match
                    rootDto.costPerConversion = totalConversion > 0 ?  Math.Round(totalCost / totalConversion, 2).ToString() : "0";

                    rootDto.impressionSharePercent = Math.Round(retVal.Where(x => !string.IsNullOrEmpty(x.ImpressionSharePercent) && ConvertPercentageToDecimal(x.ImpressionSharePercent) > 0)
                                                       .Select(x => ConvertPercentageToDecimal(x.ImpressionSharePercent))
                                                       .DefaultIfEmpty()
                                                       .Average(), 2)
                                                       .ToString() ?? "0";

                    rootDto.impressionLostToBudgetPercent = Math.Round(retVal
                     .Where(x => !string.IsNullOrEmpty(x.ImpressionLostToBudgetPercent) && ConvertPercentageToDecimal(x.ImpressionLostToBudgetPercent) > 0)
                     .Select(x => ConvertPercentageToDecimal(x.ImpressionLostToBudgetPercent))
                     .DefaultIfEmpty()
                     .Average(), 2)
                     .ToString() ?? "0";

                    rootDto.impressionLostToRankAggPercent = Math.Round(retVal
                        .Where(x => !string.IsNullOrEmpty(x.ImpressionLostToRankAggPercent) && ConvertPercentageToDecimal(x.ImpressionLostToRankAggPercent) > 0)
                        .Select(x => ConvertPercentageToDecimal(x.ImpressionLostToRankAggPercent))
                        .DefaultIfEmpty()
                        .Average(), 2)
                        .ToString() ?? "0";

                    // Assuming your existing data is in the variable existingData
                    //DateTime startDate = new DateTime(2023, 11, 22);
                    //DateTime endDate = new DateTime(2023, 12, 21);

                    //Line Chart
                    var newData = AddMissingDates(retVal, startDateDateTme, endDateDateTme);

                    rootDto.dates = newData.Select(x => x.TimePeriod.Day <= 1
                                     ? x.TimePeriod.ToString("MMM") // Display month name for 1st day of the month
                                     : x.TimePeriod.Day.ToString() + " " + x.TimePeriod.ToString("MMM"))  // Display day for other days
                                     .ToList();

                    rootDto.clickChartValue = newData.Select(x => x.Clicks).ToList();

                    rootDto.impressionLineChartValue = newData.Select(x => x.Impressions).ToList();

                    rootDto.ctrLineChartValue = newData.Select(x => ConvertPercentageToDecimal(x.Ctr)).ToList();

                    rootDto.avgcpcLineChartValue = newData.Select(x => ConvertPercentageToDecimal(x.AverageCpc)).ToList();

                    rootDto.costLineChartValue = newData.Select(x => x.Spend).ToList();

                    rootDto.conversionLineChartValue = newData.Select(x => x.Conversions).ToList();

                    rootDto.conversionRateLineChartValue = newData.Select(x => ConvertPercentageToDecimal(x.ConversionRate)).ToList();

                    rootDto.costPerConversionLineChartValue = newData.Select(x => ConvertPercentageToDecimal(x.CostPerConversion)).ToList();

                    rootDto.impressionShareLineChartValue = newData.Select(x => ConvertPercentageToDecimal(x.ImpressionSharePercent)).ToList();

                    rootDto.impressionShareBudgetLineChartValue = newData.Select(x => ConvertPercentageToDecimal(x.ImpressionLostToBudgetPercent)).ToList();

                    rootDto.impressionShareRankLineChartValue = newData.Select(x => ConvertPercentageToDecimal(x.ImpressionLostToRankAggPercent)).ToList();

                    //Make groupby campaign id for table
                    // Assuming retVal is a List<CampaignPerformanceDto>
                    var groupedData = retVal
                                  .GroupBy(x => x.AdGroupId)
                                  .Select(group => new AdGroupPerformanceDto()
                                  {
                                      CampaignId = group.Select(x => x.CampaignId).FirstOrDefault(),
                                      Status = group.Select(x => x.Status).FirstOrDefault(),
                                      CampaignName = group.Select(x => x.CampaignName).FirstOrDefault(),
                                      AdGroupName = group.Select(x => x.AdGroupName).FirstOrDefault(),
                                      AdGroupId = group.Key,
                                      Clicks = group.Sum(x => x.Clicks),
                                      Impressions = group.Sum(x => x.Impressions),
                                      Spend = group.Sum(x => x.Spend),
                                      Conversions = group.Sum(x => x.Conversions),
                                      AverageCpc = (group.Sum(x => x.Clicks) > 0) ? (group.Sum(x => x.Spend) / group.Sum(x => x.Clicks)).ToString("0.00") : "0.00",

                                      Ctr = (group.Sum(x => x.Impressions) > 0) ?
                                             (((decimal)group.Sum(x => x.Clicks) / group.Sum(x => x.Impressions)) * 100m).ToString("0.00") : "0.00",
                                      CostPerConversion = (group.Sum(x => x.Conversions) > 0) ?
                                          (group.Sum(x => x.Spend) / group.Sum(x => x.Conversions)).ToString("0.00") : "0.00",
                                      ImpressionSharePercent = (group.Any() && group.All(x => ConvertPercentageToDecimal(x.ImpressionSharePercent) > 0)) ?
                                          Math.Round(group.Average(x => ConvertPercentageToDecimal(x.ImpressionSharePercent)), 2).ToString() : "0.00",
                                      ImpressionLostToBudgetPercent = (group.Any()) ?
                                          Math.Round(group.Average(x => ConvertPercentageToDecimal(x.ImpressionLostToBudgetPercent)), 2).ToString() : "0.00",
                                      ImpressionLostToRankAggPercent = (group.Any() && group.All(x => ConvertPercentageToDecimal(x.ImpressionLostToRankAggPercent) > 0)) ?
                                          Math.Round(group.Average(x => ConvertPercentageToDecimal(x.ImpressionLostToRankAggPercent)), 2).ToString() : "0.00",
                                      ConversionRate = (group.Sum(x => ConvertPercentageToDecimal(x.ConversionRate)) > 0) ?
                                          group.Sum(x => ConvertPercentageToDecimal(x.ConversionRate)).ToString("0.00") : "0.00",
                                  }).ToList();

                    rootDto.adGroupPerformanceDto = groupedData;

                    //Make group by campaign for bar chart
                    var groupedDataForBarChart = retVal
                              .GroupBy(x => x.CampaignId)
                              .Select(group => new AdGroupPerformanceDto()
                              {
                                  CampaignId = group.Key,
                                  Status = group.Select(x => x.Status).FirstOrDefault(),
                                  CampaignName = group.Select(x => x.CampaignName).FirstOrDefault(),
                                  AdGroupName = group.Select(x => x.AdGroupName).FirstOrDefault(),
                                  AdGroupId = group.Select(x => x.AdGroupId).FirstOrDefault(),
                                  Clicks = group.Sum(x => x.Clicks),
                                  Impressions = group.Sum(x => x.Impressions),
                                  Spend = group.Sum(x => x.Spend),
                                  Conversions = group.Sum(x => x.Conversions),
                                  AverageCpc = (group.Sum(x => x.Clicks) > 0) ? (group.Sum(x => x.Spend) / group.Sum(x => x.Clicks)).ToString("0.00") : "0.00",

                                  Ctr = (group.Sum(x => x.Impressions) > 0) ?
                                             (((decimal)group.Sum(x => x.Clicks) / group.Sum(x => x.Impressions)) * 100m).ToString("0.00") : "0.00",

                                  CostPerConversion = (group.Sum(x => x.Conversions) > 0) ?
                                      (group.Sum(x => x.Spend) / group.Sum(x => x.Conversions)).ToString("0.00") : "0.00",
                                  ImpressionSharePercent = (group.Any()) ?
                                      Math.Round(group.Average(x => ConvertPercentageToDecimal(x.ImpressionSharePercent)), 2).ToString() : "0.00",
                                  ImpressionLostToBudgetPercent = (group.Any()) ?
                                      Math.Round(group.Average(x => ConvertPercentageToDecimal(x.ImpressionLostToBudgetPercent)), 2).ToString() : "0.00",
                                  ImpressionLostToRankAggPercent = (group.Any()) ?
                                      Math.Round(group.Average(x => ConvertPercentageToDecimal(x.ImpressionLostToRankAggPercent)), 2).ToString() : "0.00",
                                  ConversionRate = (group.Sum(x => ConvertPercentageToDecimal(x.ConversionRate)) > 0) ?
                                      group.Sum(x => ConvertPercentageToDecimal(x.ConversionRate)).ToString("0.00") : "0.00",
                              }).ToList();

                    //Bar Chart
                    rootDto.adGroupName = groupedDataForBarChart.Select(x => x.AdGroupName).ToList();

                    rootDto.clickBarChartValue = groupedDataForBarChart.Select(x => x.Clicks).ToList();

                    rootDto.impressionBarChartValue = groupedDataForBarChart.Select(x => x.Impressions).ToList();

                    rootDto.ctrBarChartValue = groupedDataForBarChart.Select(x => ConvertPercentageToDecimal(x.Ctr)).ToList();

                    rootDto.avgcpcBarChartValue = groupedDataForBarChart.Select(x => ConvertPercentageToDecimal(x.AverageCpc)).ToList();

                    rootDto.costBarChartValue = groupedDataForBarChart.Select(x => x.Spend).ToList();

                    rootDto.conversionBarChartValue = groupedDataForBarChart.Select(x => x.Conversions).ToList();

                    rootDto.conversionRateBarChartValue = groupedDataForBarChart.Select(x => ConvertPercentageToDecimal(x.ConversionRate)).ToList();

                    rootDto.costPerConversionBarChartValue = groupedDataForBarChart.Select(x => ConvertPercentageToDecimal(x.CostPerConversion)).ToList();

                    rootDto.impressionShareBarChartValue = groupedDataForBarChart.Select(x => ConvertPercentageToDecimal(x.ImpressionSharePercent)).ToList();

                    rootDto.impressionShareBudgetBarChartValue = groupedDataForBarChart.Select(x => ConvertPercentageToDecimal(x.ImpressionLostToBudgetPercent)).ToList();

                    rootDto.impressionShareRankBarChartValue = groupedDataForBarChart.Select(x => ConvertPercentageToDecimal(x.ImpressionLostToRankAggPercent)).ToList();
                }

                //Get the previous data

                var previousDate = CalculatePreviousStartDateAndEndDate(startDateDateTme, endDateDateTme);

                var reportRequestForPrev = new AdGroupPerformanceReportRequest
                {
                    ReportName = "AdGroupPerformanceReport",
                    ExcludeColumnHeaders = false,
                    ExcludeReportFooter = true,
                    ExcludeReportHeader = true,

                    Format = ReportFormat.Csv,
                    Aggregation = ReportAggregation.Daily,
                    ReturnOnlyCompleteData = false,

                    Columns = new[]
                    {
                                 AdGroupPerformanceReportColumn.CampaignId,
                                 AdGroupPerformanceReportColumn.AdGroupId,
                                 AdGroupPerformanceReportColumn.AdGroupName,
                                 AdGroupPerformanceReportColumn.CampaignName,
                                 AdGroupPerformanceReportColumn.Status,

                                 AdGroupPerformanceReportColumn.Impressions,
                                 AdGroupPerformanceReportColumn.AverageCpc,
                                 AdGroupPerformanceReportColumn.Spend,
                                 AdGroupPerformanceReportColumn.Ctr,
                                 AdGroupPerformanceReportColumn.Clicks,
                                 AdGroupPerformanceReportColumn.ConversionRate,
                                 AdGroupPerformanceReportColumn.Conversions,
                                 AdGroupPerformanceReportColumn.CostPerConversion,
                                 AdGroupPerformanceReportColumn.ImpressionSharePercent,
                                 AdGroupPerformanceReportColumn.ImpressionLostToBudgetPercent,
                                 AdGroupPerformanceReportColumn.ImpressionLostToRankAggPercent,
                                 AdGroupPerformanceReportColumn.TimePeriod,

                             },
                    Scope = new AccountThroughAdGroupReportScope
                    {
                        AccountIds = new List<long>() { long.Parse(campaignMicrosoft.AccountId) },

                    },
                    Time = new ReportTime
                    {
                        CustomDateRangeStart = new Microsoft.BingAds.V13.Reporting.Date() { Day = previousDate.PreviousStartDate.Day, Month = previousDate.PreviousStartDate.Month, Year = previousDate.PreviousStartDate.Year },
                        CustomDateRangeEnd = new Microsoft.BingAds.V13.Reporting.Date() { Day = previousDate.PreviousEndDate.Day, Month = previousDate.PreviousEndDate.Month, Year = previousDate.PreviousEndDate.Year },
                    }
                };

                var fileNameForPrev = DateTime.UtcNow.Ticks.ToString() + ".csv";

                var reportingDownloadParamForPrev = new ReportingDownloadParameters
                {
                    ReportRequest = reportRequestForPrev,
                    ResultFileName = fileNameForPrev,
                    OverwriteResultFile = false,
                    ResultFileDirectory = path
                };

                var prevReport = await reportingServiceManager.DownloadReportAsync(reportingDownloadParamForPrev, CancellationToken.None);

                if (prevReport != null)
                {
                    prevReport.Dispose();

                    using (var reader = new StreamReader(path + "/" + fileNameForPrev))
                    using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                    {
                        // Read CSV records
                        var records = csv.GetRecords<AdGroupPerformanceDto>().ToList();

                        retValPrev = records;

                        if (File.Exists(path + "/" + fileNameForPrev))
                        {
                            reader.Dispose();
                            reportingServiceManager.CleanupTempFiles();
                        }
                    }

                    //Filter by ms ads campaignid 
                    if (adCampaignId > 0)
                    {
                        retValPrev = retValPrev.Where(x => x.CampaignId == adCampaignId).ToList();
                    }

                    var clicksum = (decimal)retValPrev.Sum(x => x.Clicks);

                    var impressionsum = (decimal)retValPrev.Sum(x => x.Impressions);

                    var totalCost = retValPrev.Sum(x => x.Spend);

                    var totalConversion = retValPrev.Sum(x => x.Conversions);

                    rootDto.PrevCtr = impressionsum > 0 ? ((clicksum / impressionsum) * 100).ToString("0.00") + "%" : "0%";

                    rootDto.PrevClicks = clicksum.ToString();

                    rootDto.PrevImpressions = impressionsum.ToString();

                    rootDto.PrevSpend = retValPrev.Sum(x => x.Spend);

                    rootDto.PrevConversions = retValPrev.Sum(x => x.Conversions);

                    rootDto.PrevAverageCpc = clicksum > 0 ? (totalCost / clicksum).ToString("0.00") + "%" : "0";

                    rootDto.PrevConversionRate = retValPrev.Sum(x => ConvertPercentageToDecimal(x.ConversionRate)).ToString("0.00") + "%";

                    //remaining no match
                    rootDto.PrevCostPerConversion = totalConversion > 0 ? Math.Round(totalCost / totalConversion, 2).ToString() : "0";

                    rootDto.PrevImpressionSharePercent = Math.Round(retValPrev.Where(x => !string.IsNullOrEmpty(x.ImpressionSharePercent) && ConvertPercentageToDecimal(x.ImpressionSharePercent) > 0)
                                                       .Select(x => ConvertPercentageToDecimal(x.ImpressionSharePercent))
                                                       .DefaultIfEmpty()
                                                       .Average(), 2)
                                                       .ToString() ?? "0";

                    rootDto.PrevImpressionLostToBudgetPercent = Math.Round(retValPrev
                     .Where(x => !string.IsNullOrEmpty(x.ImpressionLostToBudgetPercent) && ConvertPercentageToDecimal(x.ImpressionLostToBudgetPercent) > 0)
                     .Select(x => ConvertPercentageToDecimal(x.ImpressionLostToBudgetPercent))
                     .DefaultIfEmpty()
                     .Average(), 2)
                     .ToString() ?? "0";

                    rootDto.PrevImpressionLostToRankAggPercent = Math.Round(retValPrev
                        .Where(x => !string.IsNullOrEmpty(x.ImpressionLostToRankAggPercent) && ConvertPercentageToDecimal(x.ImpressionLostToRankAggPercent) > 0)
                        .Select(x => ConvertPercentageToDecimal(x.ImpressionLostToRankAggPercent))
                        .DefaultIfEmpty()
                        .Average(), 2)
                        .ToString() ?? "0";

                    //Calculate Difference
                    rootDto.DiffCtr = CalculateCardPercentage(rootDto.ctr, rootDto.PrevCtr);
                    rootDto.DiffImpressions = CalculateCardPercentage(rootDto.impressions, rootDto.PrevImpressions);
                    rootDto.DiffAverageCpc = CalculateCardPercentage(rootDto.averageCpc, rootDto.PrevAverageCpc);
                    rootDto.DiffCtr = CalculateCardPercentage(rootDto.ctr, rootDto.PrevCtr);
                    rootDto.DiffClicks = CalculateCardPercentage(rootDto.clicks, rootDto.PrevClicks);
                    rootDto.DiffSpend = CalculateCardPercentage(rootDto.spend.ToString(), rootDto.PrevSpend.ToString());
                    rootDto.DiffConversionRate = CalculateCardPercentage(rootDto.conversionRate, rootDto.PrevConversionRate);
                    rootDto.DiffConversions = CalculateCardPercentage(rootDto.conversions.ToString(), rootDto.PrevConversions.ToString());
                    rootDto.DiffCostPerConversion = CalculateCardPercentage(rootDto.costPerConversion, rootDto.PrevCostPerConversion);
                    rootDto.DiffImpressionSharePercent = CalculateCardPercentage(rootDto.impressionSharePercent, rootDto.PrevImpressionSharePercent);
                    rootDto.DiffImpressionLostToBudgetPercent = CalculateCardPercentage(rootDto.impressionLostToBudgetPercent, rootDto.PrevImpressionLostToBudgetPercent);
                    rootDto.DiffImpressionLostToRankAggPercent = CalculateCardPercentage(rootDto.impressionLostToRankAggPercent, rootDto.PrevImpressionLostToRankAggPercent);
                }
                else
                {
                    //Filter by ms ads campaignid 
                    if (adCampaignId > 0)
                    {
                        retValPrev = retValPrev.Where(x => x.CampaignId == adCampaignId).ToList();
                    }

                    var clicksum = (decimal)retValPrev.Sum(x => x.Clicks);

                    var impressionsum = (decimal)retValPrev.Sum(x => x.Impressions);

                    var totalCost = retValPrev.Sum(x => x.Spend);

                    var totalConversion = retValPrev.Sum(x => x.Conversions);

                    rootDto.PrevCtr = impressionsum > 0 ? ((clicksum / impressionsum) * 100).ToString("0.00") + "%" : "0%";

                    rootDto.PrevClicks = clicksum.ToString();

                    rootDto.PrevImpressions = impressionsum.ToString();

                    rootDto.PrevSpend = retValPrev.Sum(x => x.Spend);

                    rootDto.PrevConversions = retValPrev.Sum(x => x.Conversions);

                    rootDto.PrevAverageCpc = clicksum > 0 ? (totalCost / clicksum).ToString("0.00") + "%" : "0";

                    rootDto.PrevConversionRate = retValPrev.Sum(x => ConvertPercentageToDecimal(x.ConversionRate)).ToString("0.00") + "%";

                    //remaining no match
                    rootDto.PrevCostPerConversion = totalConversion > 0 ? Math.Round(totalCost / totalConversion, 2).ToString() : "0";

                    rootDto.PrevImpressionSharePercent = Math.Round(retValPrev.Where(x => !string.IsNullOrEmpty(x.ImpressionSharePercent) && ConvertPercentageToDecimal(x.ImpressionSharePercent) > 0)
                                                       .Select(x => ConvertPercentageToDecimal(x.ImpressionSharePercent))
                                                       .DefaultIfEmpty()
                                                       .Average(), 2)
                                                       .ToString() ?? "0";

                    rootDto.PrevImpressionLostToBudgetPercent = Math.Round(retValPrev
                     .Where(x => !string.IsNullOrEmpty(x.ImpressionLostToBudgetPercent) && ConvertPercentageToDecimal(x.ImpressionLostToBudgetPercent) > 0)
                     .Select(x => ConvertPercentageToDecimal(x.ImpressionLostToBudgetPercent))
                     .DefaultIfEmpty()
                     .Average(), 2)
                     .ToString() ?? "0";

                    rootDto.PrevImpressionLostToRankAggPercent = Math.Round(retValPrev
                        .Where(x => !string.IsNullOrEmpty(x.ImpressionLostToRankAggPercent) && ConvertPercentageToDecimal(x.ImpressionLostToRankAggPercent) > 0)
                        .Select(x => ConvertPercentageToDecimal(x.ImpressionLostToRankAggPercent))
                        .DefaultIfEmpty()
                        .Average(), 2)
                        .ToString() ?? "0";

                    //Calculate Difference
                    rootDto.DiffCtr = CalculateCardPercentage(rootDto.ctr, rootDto.PrevCtr);
                    rootDto.DiffImpressions = CalculateCardPercentage(rootDto.impressions, rootDto.PrevImpressions);
                    rootDto.DiffAverageCpc = CalculateCardPercentage(rootDto.averageCpc, rootDto.PrevAverageCpc);
                    rootDto.DiffCtr = CalculateCardPercentage(rootDto.ctr, rootDto.PrevCtr);
                    rootDto.DiffClicks = CalculateCardPercentage(rootDto.clicks, rootDto.PrevClicks);
                    rootDto.DiffSpend = CalculateCardPercentage(rootDto.spend.ToString(), rootDto.PrevSpend.ToString());
                    rootDto.DiffConversionRate = CalculateCardPercentage(rootDto.conversionRate, rootDto.PrevConversionRate);
                    rootDto.DiffConversions = CalculateCardPercentage(rootDto.conversions.ToString(), rootDto.PrevConversions.ToString());
                    rootDto.DiffCostPerConversion = CalculateCardPercentage(rootDto.costPerConversion, rootDto.PrevCostPerConversion);
                    rootDto.DiffImpressionSharePercent = CalculateCardPercentage(rootDto.impressionSharePercent, rootDto.PrevImpressionSharePercent);
                    rootDto.DiffImpressionLostToBudgetPercent = CalculateCardPercentage(rootDto.impressionLostToBudgetPercent, rootDto.PrevImpressionLostToBudgetPercent);
                    rootDto.DiffImpressionLostToRankAggPercent = CalculateCardPercentage(rootDto.impressionLostToRankAggPercent, rootDto.PrevImpressionLostToRankAggPercent);
                }

            }
            catch (Exception ex)
            {
                var test = ex.InnerException;
            }

            return rootDto;
        }
        public async Task<RootConversionPerformance> GetConversionPerformanceReport(Guid campaignId, string startDate, string endDate, long adCampaignId = 0)
        {
            var retVal = new List<ConversionPerformanceDto>();

            var retValPrev = new List<ConversionPerformanceDto>();

            var rootDto = new RootConversionPerformance();

         

            var campaignMicrosoft = _campaignmicrosoftadRepository.GetAllEntities().Where(x => x.CampaignID == campaignId).FirstOrDefault();

            try
            {
                DateTime startDateDateTme = Convert.ToDateTime(startDate, CultureInfo.GetCultureInfo("hi-IN").DateTimeFormat);
                DateTime endDateDateTme = Convert.ToDateTime(endDate, CultureInfo.GetCultureInfo("hi-IN").DateTimeFormat);

                string redirectUri = _configuration.GetSection("HostedUrl").Value + "api/campaignmicrosoftads/MicrosoftAdsCode";

                var oAuthWebAuthCodeGrant = new OAuthWebAuthCodeGrant(
                  _configuration.GetSection("MicrosoftClientId").Value,
                  _configuration.GetSection("MicrosoftClientSeceret").Value,
                  new Uri(redirectUri),
                  ApiEnvironment.Production.ToString());

                if (!string.IsNullOrEmpty(campaignMicrosoft.RefreshToken))
                {
                    await oAuthWebAuthCodeGrant.RequestAccessAndRefreshTokensAsync(campaignMicrosoft.RefreshToken);

                    // Serialize the object to JSON
                    string userObjectJson = JsonConvert.SerializeObject(oAuthWebAuthCodeGrant);
                }
                // Create an instance of HttpRequestHeaders
                HttpRequestHeaders requestHeaders = new HttpRequestMessage().Headers;

                _authorizationData = new AuthorizationData
                {
                    DeveloperToken = "1457ONVLZ8754249",
                    Authentication = oAuthWebAuthCodeGrant
                };

                _authorizationData.AccountId = long.Parse(campaignMicrosoft.AccountId);
                //_authorizationData.CustomerId = (int)accounts[1].ParentCustomerId;

                var reportRequest = new ConversionPerformanceReportRequest
                {

                    ReportName = "ConversionPerformanceReport",
                    ExcludeColumnHeaders = false,
                    ExcludeReportFooter = true,
                    ExcludeReportHeader = true,


                    Format = ReportFormat.Csv,
                    Aggregation = ReportAggregation.Daily,
                    ReturnOnlyCompleteData = false,
                    Columns = new[]
                    {
                          ConversionPerformanceReportColumn.CampaignId,
                          ConversionPerformanceReportColumn.CampaignName,
                          ConversionPerformanceReportColumn.Goal,
                          ConversionPerformanceReportColumn.Conversions,
                          ConversionPerformanceReportColumn.Revenue,
                          ConversionPerformanceReportColumn.TimePeriod,

                      },
                    Scope = new AccountThroughAdGroupReportScope
                    {
                        AccountIds = new List<long>() { long.Parse(campaignMicrosoft.AccountId) },

                    },
                    //Filter = new ConversionPerformanceReportFilter()
                    //{
                    //    CampaignStatus = ConversionPerformanceReportFilter.a | ConversionPerformanceReportFilter.Paused
                    //},
                    Time = new ReportTime
                    {
                        CustomDateRangeStart = new Microsoft.BingAds.V13.Reporting.Date() { Day = startDateDateTme.Day, Month = startDateDateTme.Month, Year = startDateDateTme.Year },
                        CustomDateRangeEnd = new Microsoft.BingAds.V13.Reporting.Date() { Day = endDateDateTme.Day, Month = endDateDateTme.Month, Year = endDateDateTme.Year },
                        //ReportTimeZone = ReportTimeZone.EasternTimeUSCanada
                    }
                };

                var fileName = DateTime.UtcNow.Ticks.ToString() + ".csv";

                // Get the content root path on AWS Elastic Beanstalk
                string contentRootPath = Directory.GetCurrentDirectory();

                string path = Path.Combine(_hostingEnvironment.ContentRootPath, "MicrosoftTempFile");

                //string path = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/" + fileName);

                var reportingDownloadParameters = new ReportingDownloadParameters
                {
                    ReportRequest = reportRequest,
                    ResultFileName = fileName,
                    OverwriteResultFile = false,
                    ResultFileDirectory = path

                };

                _authorizationData.AccountId = long.Parse(campaignMicrosoft.AccountId);
                //_authorizationData.CustomerId = (int)accounts[1].ParentCustomerId;

                ReportingServiceManager reportingServiceManager = new ReportingServiceManager(_authorizationData, ApiEnvironment.Production);

                reportingServiceManager.DownloadHttpTimeout = new TimeSpan(0, 10, 0);

                reportingServiceManager.WorkingDirectory = path;

                // Sets the time interval in milliseconds between two status polling attempts. The default value is 5000 (5 seconds).
                //reportingServiceManager.StatusPollIntervalInMilliseconds = 5000;

                var report = await reportingServiceManager.DownloadReportAsync(reportingDownloadParameters, CancellationToken.None);

                if (report != null)
                {
                    report.Dispose();

                    //var htmlString = System.IO.File.ReadAllText(path + "/" + fileName);    

                    using (var reader = new StreamReader(path + "/" + fileName))
                    using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                    {
                        // Read CSV records
                        var records = csv.GetRecords<ConversionPerformanceDto>().ToList();

                        retVal = records;

                        if (File.Exists(path + "/" + fileName))
                        {
                            reader.Dispose();
                            reportingServiceManager.CleanupTempFiles();

                        }
                        // Convert records to JSON
                        //string jsonResult = JsonConvert.DeserializeObject<List<CampaignPerformanceDto>>(records);

                    }

                    //Filter by ms ads campaignid 
                    if (adCampaignId > 0)
                    {
                        retVal = retVal.Where(x => x.CampaignId == adCampaignId).ToList();
                    }

                    //Tiles
                    rootDto.conversions = retVal.Sum(x => x.Conversions);

                    rootDto.revenue = retVal.Sum(x => x.Revenue);


                    //Line Chart
                    var newData = AddMissingDates(retVal, startDateDateTme, endDateDateTme);

                    rootDto.dates = newData.Select(x => x.TimePeriod.Day <= 1
                                     ? x.TimePeriod.ToString("MMM") // Display month name for 1st day of the month
                                     : x.TimePeriod.Day.ToString() + " " + x.TimePeriod.ToString("MMM"))  // Display day for other days
                                     .ToList();


                    rootDto.conversionLineChartValue = newData.Select(x => x.Conversions).ToList();

                    rootDto.RevenueLineChartValue = newData.Select(x => x.Revenue).ToList();

                    // Make groupby campign id table
                    // Assuming retVal is a List<CampaignPerformanceDto>
                    var groupedData = retVal.GroupBy(x => x.CampaignId).Select(group => new ConversionPerformanceDto()
                    {
                        CampaignId = group.Key,
                        CampaignName = group.Select(x => x.CampaignName).FirstOrDefault(),
                        Revenue = group.Sum(x => x.Revenue),
                        Goal = group.Where(x => !string.IsNullOrEmpty(x.Goal)).Select(x => x.Goal).FirstOrDefault(),
                        Conversions = group.Sum(x => x.Conversions),
                    }).ToList();

                    rootDto.conversionPerformanceDto = groupedData;

                    //Bar Chart
                    rootDto.campaignsName = groupedData.Select(x => x.CampaignName).ToList();

                    rootDto.conversionBarChartValue = groupedData.Select(x => x.Conversions).ToList();
                    rootDto.revenueBarChartValue = groupedData.Select(x => x.Revenue).ToList();
                }

                //PreviousDate data
                var previousDate = CalculatePreviousStartDateAndEndDate(startDateDateTme, endDateDateTme);

                var reportRequestForPrev = new ConversionPerformanceReportRequest
                {

                    ReportName = "ConversionPerformanceReport",
                    ExcludeColumnHeaders = false,
                    ExcludeReportFooter = true,
                    ExcludeReportHeader = true,


                    Format = ReportFormat.Csv,
                    Aggregation = ReportAggregation.Daily,
                    ReturnOnlyCompleteData = false,
                    Columns = new[]
                   {
                          ConversionPerformanceReportColumn.CampaignId,
                          ConversionPerformanceReportColumn.CampaignName,
                          ConversionPerformanceReportColumn.Goal,
                          ConversionPerformanceReportColumn.Conversions,
                          ConversionPerformanceReportColumn.Revenue,
                          ConversionPerformanceReportColumn.TimePeriod,

                      },
                    Scope = new AccountThroughAdGroupReportScope
                    {
                        AccountIds = new List<long>() { long.Parse(campaignMicrosoft.AccountId) },

                    },
                    Time = new ReportTime
                    {
                        CustomDateRangeStart = new Microsoft.BingAds.V13.Reporting.Date() { Day = previousDate.PreviousStartDate.Day, Month = previousDate.PreviousStartDate.Month, Year = previousDate.PreviousStartDate.Year },
                        CustomDateRangeEnd = new Microsoft.BingAds.V13.Reporting.Date() { Day = previousDate.PreviousEndDate.Day, Month = previousDate.PreviousEndDate.Month, Year = previousDate.PreviousEndDate.Year },
                    }
                };

                var fileNameForPrev = DateTime.UtcNow.Ticks.ToString() + ".csv";

                var reportingDownloadParametersForPrev = new ReportingDownloadParameters
                {
                    ReportRequest = reportRequestForPrev,
                    ResultFileName = fileNameForPrev,
                    OverwriteResultFile = false,
                    ResultFileDirectory = path
                };

                var prevReport = await reportingServiceManager.DownloadReportAsync(reportingDownloadParametersForPrev, CancellationToken.None);

                if (prevReport != null)
                {
                    prevReport.Dispose();

                    //var htmlString = System.IO.File.ReadAllText(path + "/" + fileName);    

                    using (var reader = new StreamReader(path + "/" + fileNameForPrev))
                    using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                    {
                        // Read CSV records
                        var records = csv.GetRecords<ConversionPerformanceDto>().ToList();

                        retValPrev = records;

                        if (File.Exists(path + "/" + fileNameForPrev))
                        {
                            reader.Dispose();
                            reportingServiceManager.CleanupTempFiles();

                        }                       
                    }

                    //Filter by ms ads campaignid 
                    if (adCampaignId > 0)
                    {
                        retValPrev = retValPrev.Where(x => x.CampaignId == adCampaignId).ToList();
                    }

                    //Tiles
                    rootDto.prevConversions = retValPrev.Sum(x => x.Conversions);

                    rootDto.prevRevenue = retValPrev.Sum(x => x.Revenue);

                    //Calculate Difference
                    rootDto.diffConversions = CalculateCardPercentage(rootDto.conversions.ToString(), rootDto.prevConversions.ToString());
                    rootDto.diffRevenue = CalculateCardPercentage(rootDto.revenue.ToString(), rootDto.prevRevenue.ToString());
                }
                else
                {
                    //Filter by ms ads campaignid 
                    if (adCampaignId > 0)
                    {
                        retValPrev = retValPrev.Where(x => x.CampaignId == adCampaignId).ToList();
                    }

                    //Tiles
                    rootDto.prevConversions = retValPrev.Sum(x => x.Conversions);
                    rootDto.prevRevenue = retValPrev.Sum(x => x.Revenue);

                    //Calculate Difference
                    rootDto.diffConversions = CalculateCardPercentage(rootDto.conversions.ToString(), rootDto.prevConversions.ToString());
                    rootDto.diffRevenue = CalculateCardPercentage(rootDto.revenue.ToString(), rootDto.prevRevenue.ToString());
                }

            }
            catch (Exception ex)
            {
                var test = ex.InnerException;
            }

            return rootDto;
        }
        public async Task<RootKeywordPerformance> GetKeywordPerformanceReport(Guid campaignId, string startDate, string endDate, long adCampaignId = 0)
        {
            var retVal = new List<KeywordPerformanceDto>();

            var retValPrev = new List<KeywordPerformanceDto>();

            var rootDto = new RootKeywordPerformance();

            var campaignMicrosoft = _campaignmicrosoftadRepository.GetAllEntities().Where(x => x.CampaignID == campaignId).FirstOrDefault();

            try
            {
                DateTime startDateDateTme = Convert.ToDateTime(startDate, CultureInfo.GetCultureInfo("hi-IN").DateTimeFormat);
                DateTime endDateDateTme = Convert.ToDateTime(endDate, CultureInfo.GetCultureInfo("hi-IN").DateTimeFormat);

                string redirectUri = _configuration.GetSection("HostedUrl").Value + "api/campaignmicrosoftads/MicrosoftAdsCode";

                var oAuthWebAuthCodeGrant = new OAuthWebAuthCodeGrant(
                  _configuration.GetSection("MicrosoftClientId").Value,
                  _configuration.GetSection("MicrosoftClientSeceret").Value,
                  new Uri(redirectUri),
                  ApiEnvironment.Production.ToString());

                if (!string.IsNullOrEmpty(campaignMicrosoft.RefreshToken))
                {
                    await oAuthWebAuthCodeGrant.RequestAccessAndRefreshTokensAsync(campaignMicrosoft.RefreshToken);

                    // Serialize the object to JSON
                    string userObjectJson = JsonConvert.SerializeObject(oAuthWebAuthCodeGrant);
                }
                // Create an instance of HttpRequestHeaders
                HttpRequestHeaders requestHeaders = new HttpRequestMessage().Headers;

                _authorizationData = new AuthorizationData
                {
                    DeveloperToken = "1457ONVLZ8754249",
                    Authentication = oAuthWebAuthCodeGrant
                };

                _authorizationData.AccountId = long.Parse(campaignMicrosoft.AccountId);
                //_authorizationData.CustomerId = (int)accounts[1].ParentCustomerId;

                //Get reports
                var reportRequest = new KeywordPerformanceReportRequest
                {

                    ReportName = "KeywordPerformanceReport",
                    ExcludeColumnHeaders = false,
                    ExcludeReportFooter = true,
                    ExcludeReportHeader = true,

                    Format = ReportFormat.Csv,
                    Aggregation = ReportAggregation.Daily,
                    ReturnOnlyCompleteData = false,
                    Columns = new[]
                    {
                          KeywordPerformanceReportColumn.CampaignId,
                          KeywordPerformanceReportColumn.AdGroupName,
                          KeywordPerformanceReportColumn.CampaignName,
                          KeywordPerformanceReportColumn.Keyword,
                          KeywordPerformanceReportColumn.KeywordStatus,
                          KeywordPerformanceReportColumn.TimePeriod,
                              KeywordPerformanceReportColumn.AdGroupId,

                            KeywordPerformanceReportColumn.AverageCpc,
                            KeywordPerformanceReportColumn.Ctr,
                            KeywordPerformanceReportColumn.Clicks,
                            KeywordPerformanceReportColumn.Impressions,
                            KeywordPerformanceReportColumn.Spend,
                            KeywordPerformanceReportColumn.Conversions,
                            KeywordPerformanceReportColumn.ConversionRate,
                            KeywordPerformanceReportColumn.CostPerConversion


                      },
                    Scope = new AccountThroughAdGroupReportScope
                    {
                        AccountIds = new List<long>() { long.Parse(campaignMicrosoft.AccountId) }
                    },
                    //Filter = new KeywordPerformanceReportFilter()
                    //{
                    //    KeywordStatus = KeywordStatusReportFilter.Active | KeywordStatusReportFilter.Paused
                    //},
                    Time = new ReportTime
                    {
                        CustomDateRangeStart = new Microsoft.BingAds.V13.Reporting.Date() { Day = startDateDateTme.Day, Month = startDateDateTme.Month, Year = startDateDateTme.Year },
                        CustomDateRangeEnd = new Microsoft.BingAds.V13.Reporting.Date() { Day = endDateDateTme.Day, Month = endDateDateTme.Month, Year = endDateDateTme.Year },
                        //ReportTimeZone = ReportTimeZone.EasternTimeUSCanada
                    }
                };

                var fileName = DateTime.UtcNow.Ticks.ToString() + ".csv";

                // Get the content root path on AWS Elastic Beanstalk
                string contentRootPath = Directory.GetCurrentDirectory();

                string path = Path.Combine(_hostingEnvironment.ContentRootPath, "MicrosoftTempFile");

                //string path = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/" + fileName);

                var reportingDownloadParameters = new ReportingDownloadParameters
                {
                    ReportRequest = reportRequest,
                    ResultFileName = fileName,
                    OverwriteResultFile = false,
                    ResultFileDirectory = path
                };

                _authorizationData.AccountId = long.Parse(campaignMicrosoft.AccountId);
                //_authorizationData.CustomerId = (int)accounts[1].ParentCustomerId;

                ReportingServiceManager reportingServiceManager = new ReportingServiceManager(_authorizationData, ApiEnvironment.Production);

                reportingServiceManager.DownloadHttpTimeout = new TimeSpan(0, 10, 0);

                reportingServiceManager.WorkingDirectory = path;

                // Sets the time interval in milliseconds between two status polling attempts. The default value is 5000 (5 seconds).
                //reportingServiceManager.StatusPollIntervalInMilliseconds = 5000;

                var report = await reportingServiceManager.DownloadReportAsync(reportingDownloadParameters, CancellationToken.None);

                if (report != null)
                {
                    report.Dispose();

                    //var htmlString = System.IO.File.ReadAllText(path + "/" + fileName);    

                    using (var reader = new StreamReader(path + "/" + fileName))
                    using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                    {
                        // Read CSV records
                        var records = csv.GetRecords<KeywordPerformanceDto>().ToList();

                        retVal = records;

                        //var myData = JsonConvert.SerializeObject(retVal);

                        if (File.Exists(path + "/" + fileName))
                        {
                            reader.Dispose();
                            reportingServiceManager.CleanupTempFiles();

                        }
                        // Convert records to JSON
                        //string jsonResult = JsonConvert.DeserializeObject<List<CampaignPerformanceDto>>(records);

                    }

                    //Filter by ms ads campaignid 
                    if (adCampaignId > 0)
                    {
                        retVal = retVal.Where(x => x.CampaignId == adCampaignId).ToList();
                    }

                    var clicksum = (decimal)retVal.Sum(x => x.Clicks);
                    var impressionsum = (decimal)retVal.Sum(x => x.Impressions);
                    var totalCost = retVal.Sum(x => x.Spend);
                    var totalConversion = retVal.Sum(x => x.Conversions);

                    // Check for division by zero for CTR calculation
                    rootDto.ctr = (impressionsum > 0) ? ((clicksum / impressionsum) * 100).ToString("0.00") + "%" : "0%";

                    rootDto.clicks = clicksum.ToString();
                    rootDto.impressions = impressionsum.ToString();
                    rootDto.spend = totalCost;
                    rootDto.conversions = totalConversion;

                    // Check for division by zero for average CPC calculation
                    rootDto.averageCpc = (clicksum > 0) ? (totalCost / clicksum).ToString("0.00") + "%" : "0";

                    rootDto.conversionRate = retVal.Sum(x => ConvertPercentageToDecimal(x.ConversionRate)).ToString();

                    // Check for division by zero for cost per conversion calculation
                    rootDto.costPerConversion = (totalConversion > 0) ? Math.Round(totalCost / totalConversion, 2).ToString() : "0";

                    // Assuming your existing data is in the variable existingData
                    //DateTime startDate = new DateTime(2023, 11, 22);
                    //DateTime endDate = new DateTime(2023, 12, 21);

                    //Line Chart
                    var newData = AddMissingDates(retVal, startDateDateTme, endDateDateTme);

                    rootDto.dates = newData.Select(x => x.TimePeriod.Day <= 1
                                     ? x.TimePeriod.ToString("MMM") // Display month name for 1st day of the month
                                     : x.TimePeriod.Day.ToString() + " " + x.TimePeriod.ToString("MMM"))  // Display day for other days
                                     .ToList();

                    rootDto.clickChartValue = newData.Select(x => x.Clicks).ToList();

                    rootDto.impressionLineChartValue = newData.Select(x => x.Impressions).ToList();

                    rootDto.ctrLineChartValue = newData.Select(x => ConvertPercentageToDecimal(x.Ctr)).ToList();

                    rootDto.avgcpcLineChartValue = newData.Select(x => ConvertPercentageToDecimal(x.AverageCpc)).ToList();

                    rootDto.costLineChartValue = newData.Select(x => x.Spend).ToList();

                    rootDto.conversionLineChartValue = newData.Select(x => x.Conversions).ToList();

                    rootDto.conversionRateLineChartValue = newData.Select(x => ConvertPercentageToDecimal(x.ConversionRate)).ToList();

                    rootDto.costPerConversionLineChartValue = newData.Select(x => ConvertPercentageToDecimal(x.CostPerConversion)).ToList();


                    var allKeywords = retVal.GroupBy(x => x.Keyword)
                                            .Select(group => new KeywordPerformanceDto()
                                            {
                                                Keyword = group.Key,
                                                KeywordStatus = group.Select(x => x.KeywordStatus).FirstOrDefault(),
                                                AdGroupName = group.Select(x => x.AdGroupName).FirstOrDefault(),
                                                CampaignName = group.Select(x => x.CampaignName).FirstOrDefault(),
                                                Clicks = group.Sum(x => x.Clicks),
                                                Impressions = group.Sum(x => x.Impressions),
                                                Spend = group.Sum(x => x.Spend),
                                                Conversions = group.Sum(x => x.Conversions),
                                                AverageCpc = (group.Sum(x => x.Clicks) > 0) ? (group.Sum(x => x.Spend) / group.Sum(x => x.Clicks)).ToString("0.00") : "0.00",
                                                Ctr = (group.Sum(x => x.Impressions) > 0) ?
                                                    (((decimal)group.Sum(x => x.Clicks) / group.Sum(x => x.Impressions)) * 100m).ToString("0.00") : "0.00",
                                                CostPerConversion = (group.Sum(x => x.Conversions) > 0) ? (group.Sum(x => x.Spend) / group.Sum(x => x.Conversions)).ToString() : "0.00",
                                                ConversionRate = (group.Sum(x => ConvertPercentageToDecimal(x.ConversionRate)) > 0) ?
                                                    (group.Sum(x => ConvertPercentageToDecimal(x.ConversionRate))).ToString() : "0.00",
                                            })
                                            .ToList();

                    rootDto.keywordPerformanceDto = allKeywords.OrderByDescending(x => x.Impressions).ToList();

                    //var test = allKeywords.Where(x=>x.Keyword == "seo agency").Sum(x => x.Clicks);

                    // Add null checks for properties when creating the top lists
                    var topKeywordsByClicks = allKeywords
                        .Where(x => x.Clicks > 0)
                        .OrderByDescending(x => x.Clicks)
                        .Take(5)
                        .ToList();

                    var topKeywordsByImpressions = allKeywords
                        .Where(x => x.Impressions > 0)
                        .OrderByDescending(x => x.Impressions)
                        .Take(5)
                        .ToList();

                    var topKeywordsByAverageCpc = allKeywords
                        .Where(x => !string.IsNullOrEmpty(x.AverageCpc))
                        .OrderByDescending(x => Convert.ToDecimal(x.AverageCpc))
                        .Take(5)
                        .ToList();

                    var topKeywordsByCtr = allKeywords
                        .Where(x => !string.IsNullOrEmpty(x.Ctr))
                        .OrderByDescending(x => Convert.ToDecimal(x.Ctr))
                        .Take(5)
                        .ToList();

                    var topKeywordsBySpend = allKeywords
                        .Where(x => x.Spend > 0)
                        .OrderByDescending(x => x.Spend)
                        .Take(5)
                        .ToList();

                    var topKeywordsByConversionRate = allKeywords
                        .Where(x => !string.IsNullOrEmpty(x.ConversionRate))
                        .OrderByDescending(x => Convert.ToDecimal(x.ConversionRate))
                        .Take(5)
                        .ToList();

                    var topKeywordsByConversions = allKeywords
                        .Where(x => x.Conversions > 0)
                        .OrderByDescending(x => x.Conversions)
                        .Take(5)
                        .ToList();

                    var topKeywordsByCostPerConversion = allKeywords
                        .Where(x => !string.IsNullOrEmpty(x.CostPerConversion))
                        .OrderByDescending(x => Convert.ToDecimal(x.CostPerConversion))
                        .Take(5)
                        .ToList();

                    //Bar Chart
                    rootDto.clickBarChartValue = topKeywordsByClicks.Select(x => x.Clicks).ToList();
                    rootDto.clickBarChartLabel = topKeywordsByClicks.Select(x => x.Keyword).ToList();

                    rootDto.impressionBarChartValue = topKeywordsByImpressions.Select(x => x.Impressions).ToList();
                    rootDto.impressionBarChartLabel = topKeywordsByImpressions.Select(x => x.Keyword).ToList();

                    rootDto.avgcpcBarChartValue = topKeywordsByAverageCpc.Select(x => Convert.ToDecimal(x.AverageCpc)).ToList();
                    rootDto.avgcpcBarChartLabel = topKeywordsByAverageCpc.Select(x => x.Keyword).ToList();

                    rootDto.ctrBarChartValue = topKeywordsByCtr.Select(x => Convert.ToDecimal(x.Ctr)).ToList();
                    rootDto.ctrBarChartLabel = topKeywordsByCtr.Select(x => x.Keyword).ToList();

                    rootDto.costBarChartValue = topKeywordsBySpend.Select(x => x.Spend).ToList();
                    rootDto.costBarChartLabel = topKeywordsBySpend.Select(x => x.Keyword).ToList();

                    rootDto.conversionRateBarChartValue = topKeywordsByConversionRate.Select(x => Convert.ToDecimal(x.ConversionRate)).ToList();
                    rootDto.conversionRateBarChartLabel = topKeywordsByConversionRate.Select(x => x.Keyword).ToList();

                    rootDto.conversionBarChartValue = topKeywordsByConversions.Select(x => x.Conversions).ToList();
                    rootDto.conversionBarChartLabel = topKeywordsByConversions.Select(x => x.Keyword).ToList();

                    rootDto.costPerConversionBarChartValue = topKeywordsByCostPerConversion.Select(x => Convert.ToDecimal(x.CostPerConversion)).ToList();
                    rootDto.costPerConversionBarChartLabel = topKeywordsByCostPerConversion.Select(x => x.Keyword).ToList();
                }

                //Previous Data ====================================

                var fileNameForPrev = DateTime.UtcNow.Ticks.ToString() + ".csv";

                var previousDate = CalculatePreviousStartDateAndEndDate(startDateDateTme, endDateDateTme);

                var reportRequestForPrev = new KeywordPerformanceReportRequest
                {
                    ReportName = "KeywordPerformanceReport",
                    ExcludeColumnHeaders = false,
                    ExcludeReportFooter = true,
                    ExcludeReportHeader = true,

                    Format = ReportFormat.Csv,
                    Aggregation = ReportAggregation.Daily,
                    ReturnOnlyCompleteData = false,
                    Columns = new[]
                {
                          KeywordPerformanceReportColumn.CampaignId,
                          KeywordPerformanceReportColumn.AdGroupName,
                          KeywordPerformanceReportColumn.CampaignName,
                          KeywordPerformanceReportColumn.Keyword,
                          KeywordPerformanceReportColumn.KeywordStatus,
                          KeywordPerformanceReportColumn.TimePeriod,
                          KeywordPerformanceReportColumn.AdGroupId,

                            KeywordPerformanceReportColumn.AverageCpc,
                            KeywordPerformanceReportColumn.Ctr,
                            KeywordPerformanceReportColumn.Clicks,
                            KeywordPerformanceReportColumn.Impressions,
                            KeywordPerformanceReportColumn.Spend,
                            KeywordPerformanceReportColumn.Conversions,
                            KeywordPerformanceReportColumn.ConversionRate,
                            KeywordPerformanceReportColumn.CostPerConversion


                      },
                    Scope = new AccountThroughAdGroupReportScope
                    {
                        AccountIds = new List<long>() { long.Parse(campaignMicrosoft.AccountId) }
                    },
                    Time = new ReportTime
                    {
                        CustomDateRangeStart = new Microsoft.BingAds.V13.Reporting.Date() { Day = previousDate.PreviousStartDate.Day, Month = previousDate.PreviousStartDate.Month, Year = previousDate.PreviousStartDate.Year },
                        CustomDateRangeEnd = new Microsoft.BingAds.V13.Reporting.Date() { Day = previousDate.PreviousEndDate.Day, Month = previousDate.PreviousEndDate.Month, Year = previousDate.PreviousEndDate.Year },
                    }
                };

                var reportingDownloadParametersForPrev = new ReportingDownloadParameters
                {
                    ReportRequest = reportRequestForPrev,
                    ResultFileName = fileNameForPrev,
                    OverwriteResultFile = false,
                    ResultFileDirectory = path
                };
            
                var prevReport = await reportingServiceManager.DownloadReportAsync(reportingDownloadParametersForPrev, CancellationToken.None);

                if (prevReport != null)
                {
                    prevReport.Dispose();

                    //var htmlString = System.IO.File.ReadAllText(path + "/" + fileName);    

                    using (var reader = new StreamReader(path + "/" + fileNameForPrev))
                    using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                    {
                        // Read CSV records
                        var records = csv.GetRecords<KeywordPerformanceDto>().ToList();

                        retValPrev = records;                       

                        if (File.Exists(path + "/" + fileNameForPrev))
                        {
                            reader.Dispose();
                            reportingServiceManager.CleanupTempFiles();

                        }                        
                    }

                    //Filter by ms ads campaignid 
                    if (adCampaignId > 0)
                    {
                        retValPrev = retValPrev.Where(x => x.CampaignId == adCampaignId).ToList();
                    }

                    var clicksum = (decimal)retValPrev.Sum(x => x.Clicks);
                    var impressionsum = (decimal)retValPrev.Sum(x => x.Impressions);
                    var totalCost = retValPrev.Sum(x => x.Spend);
                    var totalConversion = retValPrev.Sum(x => x.Conversions);

                    // Check for division by zero for CTR calculation
                    rootDto.PrevCtr = (impressionsum > 0) ? ((clicksum / impressionsum) * 100).ToString("0.00") + "%" : "0%";

                    rootDto.PrevClicks = clicksum.ToString();
                    rootDto.PrevImpressions = impressionsum.ToString();
                    rootDto.PrevSpend = totalCost;
                    rootDto.PrevConversions = totalConversion;

                    // Check for division by zero for average CPC calculation
                    rootDto.PrevAverageCpc = (clicksum > 0) ? (totalCost / clicksum).ToString("0.00") + "%" : "0";

                    rootDto.PrevConversionRate = retVal.Sum(x => ConvertPercentageToDecimal(x.ConversionRate)).ToString();

                    // Check for division by zero for cost per conversion calculation
                    rootDto.PrevCostPerConversion = (totalConversion > 0) ? Math.Round(totalCost / totalConversion, 2).ToString() : "0";

                    //Calculate Difference
                    rootDto.DiffCtr = CalculateCardPercentage(rootDto.ctr, rootDto.PrevCtr);
                    rootDto.DiffImpressions = CalculateCardPercentage(rootDto.impressions, rootDto.PrevImpressions);
                    rootDto.DiffAverageCpc = CalculateCardPercentage(rootDto.averageCpc, rootDto.PrevAverageCpc);
                    rootDto.DiffCtr = CalculateCardPercentage(rootDto.ctr, rootDto.PrevCtr);
                    rootDto.DiffClicks = CalculateCardPercentage(rootDto.clicks, rootDto.PrevClicks);
                    rootDto.DiffSpend = CalculateCardPercentage(rootDto.spend.ToString(), rootDto.PrevSpend.ToString());
                    rootDto.DiffConversionRate = CalculateCardPercentage(rootDto.conversionRate, rootDto.PrevConversionRate);
                    rootDto.DiffConversions = CalculateCardPercentage(rootDto.conversions.ToString(), rootDto.PrevConversions.ToString());
                    rootDto.DiffCostPerConversion = CalculateCardPercentage(rootDto.costPerConversion, rootDto.PrevCostPerConversion);

                }
                else
                {
                    //Filter by ms ads campaignid 
                    if (adCampaignId > 0)
                    {
                        retValPrev = retValPrev.Where(x => x.CampaignId == adCampaignId).ToList();
                    }

                    var clicksum = (decimal)retValPrev.Sum(x => x.Clicks);
                    var impressionsum = (decimal)retValPrev.Sum(x => x.Impressions);
                    var totalCost = retValPrev.Sum(x => x.Spend);
                    var totalConversion = retValPrev.Sum(x => x.Conversions);

                    // Check for division by zero for CTR calculation
                    rootDto.PrevCtr = (impressionsum > 0) ? ((clicksum / impressionsum) * 100).ToString("0.00") + "%" : "0%";

                    rootDto.PrevClicks = clicksum.ToString();
                    rootDto.PrevImpressions = impressionsum.ToString();
                    rootDto.PrevSpend = totalCost;
                    rootDto.PrevConversions = totalConversion;

                    // Check for division by zero for average CPC calculation
                    rootDto.PrevAverageCpc = (clicksum > 0) ? (totalCost / clicksum).ToString("0.00") + "%" : "0";

                    rootDto.PrevConversionRate = retVal.Sum(x => ConvertPercentageToDecimal(x.ConversionRate)).ToString();

                    // Check for division by zero for cost per conversion calculation
                    rootDto.PrevCostPerConversion = (totalConversion > 0) ? Math.Round(totalCost / totalConversion, 2).ToString() : "0";

                    //Calculate Difference
                    rootDto.DiffCtr = CalculateCardPercentage(rootDto.ctr, rootDto.PrevCtr);
                    rootDto.DiffImpressions = CalculateCardPercentage(rootDto.impressions, rootDto.PrevImpressions);
                    rootDto.DiffAverageCpc = CalculateCardPercentage(rootDto.averageCpc, rootDto.PrevAverageCpc);
                    rootDto.DiffCtr = CalculateCardPercentage(rootDto.ctr, rootDto.PrevCtr);
                    rootDto.DiffClicks = CalculateCardPercentage(rootDto.clicks, rootDto.PrevClicks);
                    rootDto.DiffSpend = CalculateCardPercentage(rootDto.spend.ToString(), rootDto.PrevSpend.ToString());
                    rootDto.DiffConversionRate = CalculateCardPercentage(rootDto.conversionRate, rootDto.PrevConversionRate);
                    rootDto.DiffConversions = CalculateCardPercentage(rootDto.conversions.ToString(), rootDto.PrevConversions.ToString());
                    rootDto.DiffCostPerConversion = CalculateCardPercentage(rootDto.costPerConversion, rootDto.PrevCostPerConversion);
                }
            }
            catch (Exception ex)
            {
                var test = ex.InnerException;
            }

            return rootDto;
        }

        private string CalculateCardPercentage(string currentStringValue, string previousStringValue)
        {
            // Remove the percentage symbol and then convert string values to double
            double currentValue, previousValue;

            if (!double.TryParse(currentStringValue.TrimEnd('%'), out currentValue) ||
                !double.TryParse(previousStringValue.TrimEnd('%'), out previousValue))
            {
                // Handle invalid input, e.g., return an error message or throw an exception
                return "Invalid input";
            }

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

            double absolutePercentageChange = Math.Ceiling(Math.Abs(percentageChange));

            // Remove the decimal part if it's a whole number
            string formattedPercentageChange = absolutePercentageChange % 1 == 0
                ? absolutePercentageChange.ToString("F0") // No decimals, format as integer
                : absolutePercentageChange.ToString("F2").TrimEnd('0', '.'); // Remove trailing zeros and the dot

            //
            string result = changeDirection + formattedPercentageChange + "%";
            return result;
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

        public async Task<List<MsAdCampaignList>> GetCampaignList(Guid campaignId)
        {
            var retVal = new List<MsAdCampaignList>();
            var campaignMicrosoft = _campaignmicrosoftadRepository.GetAllEntities().Where(x => x.CampaignID == campaignId).FirstOrDefault();

            string redirectUri = _configuration.GetSection("HostedUrl").Value + "api/campaignmicrosoftads/MicrosoftAdsCode";

            var oAuthWebAuthCodeGrant = new OAuthWebAuthCodeGrant(
              _configuration.GetSection("MicrosoftClientId").Value,
              _configuration.GetSection("MicrosoftClientSeceret").Value,
              new Uri(redirectUri),
              ApiEnvironment.Production.ToString());

            //oAuthWebAuthCodeGrant.State = "12345";


            if (!string.IsNullOrEmpty(campaignMicrosoft.RefreshToken))
            {
                await oAuthWebAuthCodeGrant.RequestAccessAndRefreshTokensAsync(campaignMicrosoft.RefreshToken);

                // Serialize the object to JSON
                string userObjectJson = JsonConvert.SerializeObject(oAuthWebAuthCodeGrant);
            }


            _authorizationData = new AuthorizationData
            {
                DeveloperToken = "1457ONVLZ8754249",
                Authentication = oAuthWebAuthCodeGrant,
                AccountId = long.Parse(campaignMicrosoft.AccountId)
            };

            var _campaignManagementService = new ServiceClient<ICampaignManagementService>(_authorizationData);


            var request = new GetCampaignsByAccountIdRequest
            {

                DeveloperToken = "1457ONVLZ8754249",
                ApplicationToken = "1457ONVLZ8754249",
                AuthenticationToken = campaignMicrosoft.AccessToken,
                AccountId = long.Parse(campaignMicrosoft.AccountId),
            };


            var getCampaignResponse = await _campaignManagementService.CallAsync((s, r) => s.GetCampaignsByAccountIdAsync(r), request);

            if (getCampaignResponse != null && getCampaignResponse.Campaigns != null && getCampaignResponse.Campaigns.Count > 0)
            {
                foreach (var campaign in getCampaignResponse.Campaigns)
                {
                    var item = new MsAdCampaignList();
                    item.Id = campaign.Id;
                    item.Name = campaign.Name;
                    retVal.Add(item);
                }
            }

            return retVal;

        }
        public string CalculateAverageOfPercentage(List<CampaignPerformanceDto> data, IGrouping<object, DateTime> group, Func<CampaignPerformanceDto, string> propertySelector)
        {
            var values = data
                .Where(x => !string.IsNullOrEmpty(propertySelector(x)) && ConvertPercentageToDecimal(propertySelector(x)) > 0)
                .Select(x => ConvertPercentageToDecimal(propertySelector(x)))
                .DefaultIfEmpty()
                .Average();

            return values.ToString("0");
        }
        public string CalculateAverageOfPercentageForCampaign(IGrouping<DateTime, CampaignPerformanceDto> group, Func<CampaignPerformanceDto, string> propertySelector)
        {
            var values = group
                .Where(x => !string.IsNullOrEmpty(propertySelector(x)) && ConvertPercentageToDecimal(propertySelector(x)) > 0)
                .Select(x => ConvertPercentageToDecimal(propertySelector(x)))
                .DefaultIfEmpty()
                .Average();

            return values.ToString("0");
        }
        public string CalculateAverageOfPercentage(List<AdGroupPerformanceDto> data, IGrouping<object, DateTime> group, Func<AdGroupPerformanceDto, string> propertySelector)
        {
            var values = data
                .Where(x => !string.IsNullOrEmpty(propertySelector(x)) && ConvertPercentageToDecimal(propertySelector(x)) > 0)
                .Select(x => ConvertPercentageToDecimal(propertySelector(x)))
                .DefaultIfEmpty()
                .Average();

            return values.ToString("0");
        }
        public string CalculateAverageOfPercentage(List<KeywordPerformanceDto> data, IGrouping<object, DateTime> group, Func<KeywordPerformanceDto, string> propertySelector)
        {
            var values = data
                .Where(x => !string.IsNullOrEmpty(propertySelector(x)) && ConvertPercentageToDecimal(propertySelector(x)) > 0)
                .Select(x => ConvertPercentageToDecimal(propertySelector(x)))
                .DefaultIfEmpty()
                .Average();

            return values.ToString("0");
        }
        public string CalculateAverageOfPercentage(List<ConversionPerformanceDto> data, IGrouping<object, DateTime> group, Func<ConversionPerformanceDto, string> propertySelector)
        {
            var values = data
                .Where(x => !string.IsNullOrEmpty(propertySelector(x)) && ConvertPercentageToDecimal(propertySelector(x)) > 0)
                .Select(x => ConvertPercentageToDecimal(propertySelector(x)))
                .DefaultIfEmpty()
                .Average();

            return values.ToString("0");
        }

        private static string CalculateAverageCpc(List<CampaignPerformanceDto> existingData, IGrouping<object, DateTime> group)
        {
            decimal spend = group.Sum(date => existingData.FirstOrDefault(x => x.TimePeriod.Date == date.Date)?.Spend ?? 0);
            decimal clicks = group.Sum(date => existingData.FirstOrDefault(x => x.TimePeriod.Date == date.Date)?.Clicks ?? 0);

            // Check for divide by zero before performing the division
            return clicks > 0 ? (spend / clicks).ToString() : "0";
        }
        private static string CalculateAverageCpc(List<AdGroupPerformanceDto> existingData, IGrouping<object, DateTime> group)
        {
            decimal spend = group.Sum(date => existingData.FirstOrDefault(x => x.TimePeriod.Date == date.Date)?.Spend ?? 0);
            decimal clicks = group.Sum(date => existingData.FirstOrDefault(x => x.TimePeriod.Date == date.Date)?.Clicks ?? 0);

            // Check for divide by zero before performing the division
            return clicks > 0 ? (spend / clicks).ToString() : "0";
        }
        private static string CalculateAverageCpcForKeyword(List<KeywordPerformanceDto> existingData, IGrouping<object, DateTime> group)
        {
            decimal spend = group.Sum(date => existingData.FirstOrDefault(x => x.TimePeriod.Date == date.Date)?.Spend ?? 0);
            decimal clicks = group.Sum(date => existingData.FirstOrDefault(x => x.TimePeriod.Date == date.Date)?.Clicks ?? 0);

            // Check for divide by zero before performing the division
            return clicks > 0 ? (spend / clicks).ToString() : "0";
        }

        private static string CalculateAverageCpcForKeyword1(IGrouping<DateTime, KeywordPerformanceDto> group)
        {
            decimal spend = group.Sum(entry => entry.Spend);
            decimal clicks = group.Sum(entry => entry.Clicks);

            // Check for divide by zero before performing the division
            return clicks > 0 ? (spend / clicks).ToString("0.00") : "0.00";
        }

        private static string CalculateAverageCpcForCampaign1(IGrouping<DateTime, CampaignPerformanceDto> group)
        {
            decimal spend = group.Sum(entry => entry.Spend);
            decimal clicks = group.Sum(entry => entry.Clicks);

            // Check for divide by zero before performing the division
            return clicks > 0 ? (spend / clicks).ToString("0.00") : "0.00";
        }


        private static string CalculateCostPerConversion(List<CampaignPerformanceDto> existingData, IGrouping<object, DateTime> group)
        {
            decimal spend = group.Sum(date => existingData.FirstOrDefault(x => x.TimePeriod.Date == date.Date)?.Spend ?? 0);
            decimal conversions = group.Sum(date => existingData.FirstOrDefault(x => x.TimePeriod.Date == date.Date)?.Conversions ?? 0);

            // Check for divide by zero before performing the division
            return conversions > 0 ? (spend / conversions).ToString() : "0";
        }

        private static string CalculateCostPerConversion1(IGrouping<DateTime, KeywordPerformanceDto> group)
        {
            decimal spend = group.Sum(entry => entry.Spend);
            decimal conversions = group.Sum(entry => entry.Conversions);

            // Check for divide by zero before performing the division
            return conversions > 0 ? (spend / conversions).ToString("0.00") : "0.00";
        }

        private static string CalculateCostPerConversionForCampaign(IGrouping<DateTime, CampaignPerformanceDto> group)
        {
            decimal spend = group.Sum(entry => entry.Spend);
            decimal conversions = group.Sum(entry => entry.Conversions);

            // Check for divide by zero before performing the division
            return conversions > 0 ? (spend / conversions).ToString("0.00") : "0.00";
        }

        private static string CalculateCostPerConversion(List<AdGroupPerformanceDto> existingData, IGrouping<object, DateTime> group)
        {
            decimal spend = group.Sum(date => existingData.FirstOrDefault(x => x.TimePeriod.Date == date.Date)?.Spend ?? 0);
            decimal conversions = group.Sum(date => existingData.FirstOrDefault(x => x.TimePeriod.Date == date.Date)?.Conversions ?? 0);

            // Check for divide by zero before performing the division
            return conversions > 0 ? (spend / conversions).ToString() : "0";
        }
        private static string CalculateCostPerConversion(List<KeywordPerformanceDto> existingData, IGrouping<object, DateTime> group)
        {
            decimal spend = group.Sum(date => existingData.FirstOrDefault(x => x.TimePeriod.Date == date.Date)?.Spend ?? 0);
            decimal conversions = group.Sum(date => existingData.FirstOrDefault(x => x.TimePeriod.Date == date.Date)?.Conversions ?? 0);

            // Check for divide by zero before performing the division
            return conversions > 0 ? (spend / conversions).ToString() : "0";
        }


        private static string CalculateCtr(List<CampaignPerformanceDto> existingData, IGrouping<object, DateTime> group)
        {
            decimal clicks = group.Sum(date => existingData.FirstOrDefault(x => x.TimePeriod.Date == date.Date)?.Clicks ?? 0);
            decimal impressions = group.Sum(date => existingData.FirstOrDefault(x => x.TimePeriod.Date == date.Date)?.Impressions ?? 0);

            // Check for divide by zero before performing the division
            return impressions > 0 ? (clicks / impressions).ToString() : "0";
        }


        private static string CalculateCtrForKeyword1(IGrouping<DateTime, KeywordPerformanceDto> group)
        {
            decimal clicks = group.Sum(entry => entry.Clicks);
            decimal impressions = group.Sum(entry => entry.Impressions);

            // Check for divide by zero before performing the division
            return impressions > 0 ? ((clicks / impressions) * 100).ToString("0.00") : "0.00";
        }

        private static string CalculateCtrForCampaign1(IGrouping<DateTime, CampaignPerformanceDto> group)
        {
            decimal clicks = group.Sum(entry => entry.Clicks);
            decimal impressions = group.Sum(entry => entry.Impressions);

            // Check for divide by zero before performing the division
            return impressions > 0 ? ((clicks / impressions) * 100).ToString("0.00") : "0.00";
        }

        private static string CalculateCtr(List<AdGroupPerformanceDto> existingData, IGrouping<object, DateTime> group)
        {
            decimal clicks = group.Sum(date => existingData.FirstOrDefault(x => x.TimePeriod.Date == date.Date)?.Clicks ?? 0);
            decimal impressions = group.Sum(date => existingData.FirstOrDefault(x => x.TimePeriod.Date == date.Date)?.Impressions ?? 0);

            // Check for divide by zero before performing the division
            return impressions > 0 ? (clicks / impressions).ToString() : "0";
        }
        private static string CalculateCtr(List<KeywordPerformanceDto> existingData, IGrouping<object, DateTime> group)
        {
            decimal clicks = group.Sum(date => existingData.FirstOrDefault(x => x.TimePeriod.Date == date.Date)?.Clicks ?? 0);
            decimal impressions = group.Sum(date => existingData.FirstOrDefault(x => x.TimePeriod.Date == date.Date)?.Impressions ?? 0);

            // Check for divide by zero before performing the division
            return impressions > 0 ? (clicks / impressions).ToString() : "0";
        }


        public List<CampaignPerformanceDto> AddMissingDates(List<CampaignPerformanceDto> existingData, DateTime startDate, DateTime endDate)
        {
            var dateRange = Enumerable.Range(0, (endDate - startDate).Days + 1)
                .Select(offset => startDate.AddDays(offset));

            if (dateRange.Count() > 30)
            {
                return dateRange
                    .GroupBy(date => new { date.Year, date.Month })
                    .Select(group => new CampaignPerformanceDto
                    {
                        TimePeriod = new DateTime(group.Key.Year, group.Key.Month, 1),
                        Impressions = group.Sum(date => existingData.FirstOrDefault(x => x.TimePeriod.Date == date.Date)?.Impressions ?? 0),
                        Clicks = group.Sum(date => existingData.FirstOrDefault(x => x.TimePeriod.Date == date.Date)?.Clicks ?? 0),
                        Spend = group.Sum(date => existingData.FirstOrDefault(x => x.TimePeriod.Date == date.Date)?.Spend ?? 0),
                        Conversions = group.Sum(date => existingData.FirstOrDefault(x => x.TimePeriod.Date == date.Date)?.Conversions ?? 0),
                        AverageCpc = CalculateAverageCpc(existingData, group),
                        Ctr = CalculateCtr(existingData, group),
                        ConversionRate = group.Sum(date => ConvertPercentageToDecimal(existingData.FirstOrDefault(x => x.TimePeriod.Date == date.Date)?.ConversionRate ?? "0")).ToString(),
                        CostPerConversion = CalculateCostPerConversion(existingData, group),
                        ImpressionSharePercent = CalculateAverageOfPercentage(existingData, group, x => x.ImpressionSharePercent),
                        ImpressionLostToBudgetPercent = CalculateAverageOfPercentage(existingData, group, x => x.ImpressionLostToBudgetPercent),
                        ImpressionLostToRankAggPercent = CalculateAverageOfPercentage(existingData, group, x => x.ImpressionLostToRankAggPercent),
                    }).ToList();
            }
            else
            {
                var groupedData = existingData
                       .GroupBy(x => x.TimePeriod.Date)
                       .Select(group => new CampaignPerformanceDto
                       {
                           TimePeriod = group.Key,
                           Impressions = group.Sum(x => x.Impressions),
                           Clicks = group.Sum(x => x.Clicks),
                           Spend = group.Sum(x => x.Spend),
                           Conversions = group.Sum(x => x.Conversions),
                           AverageCpc = CalculateAverageCpcForCampaign1(group),
                           Ctr = CalculateCtrForCampaign1(group),
                           ConversionRate = group.Sum(x => ConvertPercentageToDecimal(x.ConversionRate)).ToString(),
                           CostPerConversion = CalculateCostPerConversionForCampaign(group),
                           ImpressionSharePercent = CalculateAverageOfPercentageForCampaign(group, x => x.ImpressionSharePercent),
                           ImpressionLostToBudgetPercent = CalculateAverageOfPercentageForCampaign(group, x => x.ImpressionLostToBudgetPercent),
                           ImpressionLostToRankAggPercent = CalculateAverageOfPercentageForCampaign(group, x => x.ImpressionLostToRankAggPercent),
                       }).ToList();

                var missingDatesData = dateRange
                    .Where(date => !groupedData.Select(x => x.TimePeriod.Date).Contains(date.Date))
                    .Select(date => new CampaignPerformanceDto
                    {
                        TimePeriod = date,
                        Impressions = 0,
                        Clicks = 0,
                        Spend = 0,
                        Conversions = 0,
                        AverageCpc = "0",
                        Ctr = "0",
                        ConversionRate = "0",
                        CostPerConversion = "0",
                        ImpressionSharePercent = "0",
                        ImpressionLostToBudgetPercent = "0",
                        ImpressionLostToRankAggPercent = "0"
                    });

                return groupedData.Concat(missingDatesData).OrderBy(x => x.TimePeriod).ToList();
            }
        }
        public List<AdGroupPerformanceDto> AddMissingDates(List<AdGroupPerformanceDto> existingData, DateTime startDate, DateTime endDate)
        {
            var dateRange = Enumerable.Range(0, (endDate - startDate).Days + 1)
                .Select(offset => startDate.AddDays(offset));

            if (dateRange.Count() > 30)
            {
                return dateRange
                    .GroupBy(date => new { date.Year, date.Month })
                    .Select(group => new AdGroupPerformanceDto
                    {
                        TimePeriod = new DateTime(group.Key.Year, group.Key.Month, 1),
                        Impressions = group.Sum(date => existingData.FirstOrDefault(x => x.TimePeriod.Date == date.Date)?.Impressions ?? 0),
                        Clicks = group.Sum(date => existingData.FirstOrDefault(x => x.TimePeriod.Date == date.Date)?.Clicks ?? 0),
                        Spend = group.Sum(date => existingData.FirstOrDefault(x => x.TimePeriod.Date == date.Date)?.Spend ?? 0),
                        Conversions = group.Sum(date => existingData.FirstOrDefault(x => x.TimePeriod.Date == date.Date)?.Conversions ?? 0),
                        AverageCpc = CalculateAverageCpc(existingData, group),
                        Ctr = CalculateCtr(existingData, group),
                        ConversionRate = group.Sum(date => ConvertPercentageToDecimal(existingData.FirstOrDefault(x => x.TimePeriod.Date == date.Date)?.ConversionRate ?? "0")).ToString(),
                        CostPerConversion = CalculateCostPerConversion(existingData, group),
                        ImpressionSharePercent = CalculateAverageOfPercentage(existingData, group, x => x.ImpressionSharePercent),
                        ImpressionLostToBudgetPercent = CalculateAverageOfPercentage(existingData, group, x => x.ImpressionLostToBudgetPercent),
                        ImpressionLostToRankAggPercent = CalculateAverageOfPercentage(existingData, group, x => x.ImpressionLostToRankAggPercent),
                    }).ToList();
            }
            else
            {
                return dateRange
                    .Select(date => existingData.FirstOrDefault(x => x.TimePeriod.Date == date.Date) ?? new AdGroupPerformanceDto
                    {
                        TimePeriod = date,
                        Impressions = 0,
                        Clicks = 0,
                        Spend = 0,
                        Conversions = 0,
                        AverageCpc = "0",
                        Ctr = "0",
                        ConversionRate = "0",
                        CostPerConversion = "0",
                        ImpressionSharePercent = "0",
                        ImpressionLostToBudgetPercent = "0",
                        ImpressionLostToRankAggPercent = "0"
                    })
                    .ToList();
            }
        }
        public List<KeywordPerformanceDto> AddMissingDates(List<KeywordPerformanceDto> existingData, DateTime startDate, DateTime endDate)
        {
            var dateRange = Enumerable.Range(0, (endDate - startDate).Days + 1)
                .Select(offset => startDate.AddDays(offset));

            if (dateRange.Count() > 30)
            {
                return dateRange
                    .GroupBy(date => new { date.Year, date.Month })
                    .Select(group => new KeywordPerformanceDto
                    {
                        TimePeriod = new DateTime(group.Key.Year, group.Key.Month, 1),
                        Impressions = group.Sum(date => existingData.FirstOrDefault(x => x.TimePeriod.Date == date.Date)?.Impressions ?? 0),
                        Clicks = group.Sum(date => existingData.FirstOrDefault(x => x.TimePeriod.Date == date.Date)?.Clicks ?? 0),
                        Spend = group.Sum(date => existingData.FirstOrDefault(x => x.TimePeriod.Date == date.Date)?.Spend ?? 0),
                        Conversions = group.Sum(date => existingData.FirstOrDefault(x => x.TimePeriod.Date == date.Date)?.Conversions ?? 0),
                        AverageCpc = CalculateAverageCpcForKeyword(existingData, group),
                        Ctr = CalculateCtr(existingData, group),
                        ConversionRate = group.Sum(date => ConvertPercentageToDecimal(existingData.FirstOrDefault(x => x.TimePeriod.Date == date.Date)?.ConversionRate ?? "0")).ToString(),
                        CostPerConversion = CalculateCostPerConversion(existingData, group)
                    }).ToList();
            }
            else
            {
                var groupedData = existingData
                       .GroupBy(x => x.TimePeriod.Date)
                       .Select(group => new KeywordPerformanceDto
                       {
                           TimePeriod = group.Key,
                           Impressions = group.Sum(x => x.Impressions),
                           Clicks = group.Sum(x => x.Clicks),
                           Spend = group.Sum(x => x.Spend),
                           Conversions = group.Sum(x => x.Conversions),
                           AverageCpc = CalculateAverageCpcForKeyword1(group),
                           Ctr = CalculateCtrForKeyword1(group),
                           ConversionRate = group.Sum(x => ConvertPercentageToDecimal(x.ConversionRate)).ToString(),
                           CostPerConversion = CalculateCostPerConversion1(group)
                       }).ToList();

                var missingDatesData = dateRange
                    .Where(date => !groupedData.Select(x => x.TimePeriod.Date).Contains(date.Date))
                    .Select(date => new KeywordPerformanceDto
                    {
                        TimePeriod = date,
                        Impressions = 0,
                        Clicks = 0,
                        Spend = 0,
                        Conversions = 0,
                        AverageCpc = "0",
                        Ctr = "0",
                        ConversionRate = "0",
                        CostPerConversion = "0"
                    });

                return groupedData.Concat(missingDatesData).OrderBy(x => x.TimePeriod).ToList();
            }
        }

        public List<ConversionPerformanceDto> AddMissingDates(List<ConversionPerformanceDto> existingData, DateTime startDate, DateTime endDate)
        {
            var dateRange = Enumerable.Range(0, (endDate - startDate).Days + 1)
                .Select(offset => startDate.AddDays(offset));

            var groupedData = existingData
                .GroupBy(dto => dto.TimePeriod)
                .Select(group => new ConversionPerformanceDto
                {
                    TimePeriod = group.Key,
                    Revenue = group.Sum(dto => dto.Revenue),
                    Conversions = group.Sum(dto => dto.Conversions),
                    Goal = group.Select(x => x.Goal).FirstOrDefault(),
                    CampaignName = group.Select(x => x.CampaignName).FirstOrDefault()

                })
                .ToList();

            if (dateRange.Count() > 30)
            {
                return dateRange
                    .GroupBy(date => new { date.Year, date.Month })
                    .Select(group => new ConversionPerformanceDto
                    {
                        TimePeriod = new DateTime(group.Key.Year, group.Key.Month, 1),
                        Revenue = groupedData
                            .Where(dto => dto.TimePeriod.Year == group.Key.Year && dto.TimePeriod.Month == group.Key.Month)
                            .Sum(dto => dto.Revenue),
                        Conversions = groupedData
                            .Where(dto => dto.TimePeriod.Year == group.Key.Year && dto.TimePeriod.Month == group.Key.Month)
                            .Sum(dto => dto.Conversions),

                    })
                    .ToList();
            }
            else
            {
                return dateRange
                    .Select(date => groupedData.FirstOrDefault(dto => dto.TimePeriod.Date == date.Date) ?? new ConversionPerformanceDto
                    {
                        TimePeriod = date,
                        Revenue = 0.00m,
                        Conversions = 0,

                    })
                    .ToList();
            }
        }

        private decimal ConvertPercentageToDecimal(string percentage)
        {
            if (string.IsNullOrEmpty(percentage))
            {
                return 0.0m;
            }

            return decimal.TryParse(percentage?.Replace("%", ""), out decimal result) ? Math.Round(result, 2) : 0.0m;
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
            return "Id,AccountId,AccountName,CampaignID";
        }

        #endregion
    }
}
