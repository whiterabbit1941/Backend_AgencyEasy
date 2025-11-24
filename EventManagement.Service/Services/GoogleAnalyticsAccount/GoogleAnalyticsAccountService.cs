using EventManagement.Domain;
using EventManagement.Domain.Entities;
using EventManagement.Dto;
using Google.Ads.GoogleAds;
using Google.Ads.GoogleAds.Config;
using Google.Ads.GoogleAds.Lib;
using Google.Ads.GoogleAds.V15.Errors;
using Google.Ads.GoogleAds.V15.Resources;
using Google.Ads.GoogleAds.V15.Services;
using Google.Api.Ads.AdWords.Lib;
using Google.Api.Ads.AdWords.Util.Reports;
using Google.Api.Ads.AdWords.Util.Reports.v201809;
using Google.Api.Ads.Common.Lib;
using Google.Api.Ads.Common.Util.Reports;
using Google.Api.Gax;
using Google.Apis.Analytics.v3;
using Google.Apis.AnalyticsReporting.v4;
using Google.Apis.AnalyticsReporting.v4.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.PagespeedInsights.v5;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.Webmasters.v3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Audience = EventManagement.Dto.Audience;

namespace EventManagement.Service
{
    public class GoogleAnalyticsAccountService : ServiceBase<GoogleAnalyticsAccount, Guid>, IGoogleAnalyticsAccountService
    {

        #region PRIVATE MEMBERS

        private readonly IGoogleAnalyticsAccountRepository _googleanalyticsaccountRepository;
        private readonly IConfiguration _configuration;
        private readonly IGoogleAccountSetupService _googleaccountsetupService;
        private readonly IUserInfoService _userInfoService;

        #endregion


        #region CONSTRUCTOR

        public GoogleAnalyticsAccountService(IUserInfoService userinfoService, IGoogleAnalyticsAccountRepository googleanalyticsaccountRepository, ILogger<GoogleAnalyticsAccountService> logger, IConfiguration configuration, IGoogleAccountSetupService googleaccountsetupService) : base(googleanalyticsaccountRepository, logger)
        {
            _googleanalyticsaccountRepository = googleanalyticsaccountRepository;
            _configuration = configuration;
            _googleaccountsetupService = googleaccountsetupService;
            _userInfoService = userinfoService;
        }

        #endregion


        #region PUBLIC MEMBERS 

        /// <summary>
        /// Get GA setups by campaignID
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <returns>List of GoogleAnalyticsAccountDto</returns>
        public List<GoogleAnalyticsAccountDto> GetGaAccountByCampaignID(string campaignId)
        {
            // var gaSetup = _googleanalyticsaccountRepository.GetAllEntities(true).Where(x => x.CampaignID == campaignId).ToList();

            var gaSetup = _googleanalyticsaccountRepository.GetAllEntities(true).Where(x => x.CampaignID == campaignId).Select(ga => new GoogleAnalyticsAccountDto
            {
                Id = ga.Id,
                AccountID = ga.AccountID,
                AccountName = ga.AccountName,
                ViewID = ga.ViewID,
                ViewName = ga.ViewName,
                CampaignID = ga.CampaignID,
                GoogleAccountSetupID = ga.GoogleAccountSetupID,
                PropertyID = ga.PropertyID,
                WebsiteUrl = ga.WebsiteUrl,
                Active = ga.Active,
                GoogleAccountSetups = new GoogleAccountSetupDto
                {
                    Id = ga.GoogleAccountSetup.Id,
                    AccessToken = ga.GoogleAccountSetup.AccessToken,
                    IsAuthorize = ga.GoogleAccountSetup.IsAuthorize,
                    RefreshToken = ga.GoogleAccountSetup.RefreshToken,
                    UserId = ga.GoogleAccountSetup.UserId,
                    UserName = ga.GoogleAccountSetup.UserName
                }

            }).ToList();
            return gaSetup;
        }

        /// <summary>
        /// Get Ga Analytics Reports By CampaignId
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <returns>GaReportsDto</returns>
        public async Task<GaReportsDto> GetGaAnalyticsReports(string campaignId, string startDate, string endDate)
        {
            GaReportsDto gaReportsDto = new GaReportsDto();

            List<GoogleCustomDataDto> listGoogleCustomDataDto = new List<GoogleCustomDataDto>();


            var gaClientId = _configuration["ClientIdForGoogleAds"];
            var gaClientSecret = _configuration["ClientSecretForGoogleAds"];

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://www.googleapis.com"),
            };

            var data = new
            {
                client_id = gaClientId,
                client_secret = gaClientSecret,
                // IMPORTANT: This should use the refresh_token from the specific user/account setup
                // The refresh_token parameter should be passed to this method or retrieved from database
                refresh_token = "", // TODO: Pass refresh token as parameter
                grant_type = "refresh_token",
                access_type = "offline"
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var resp = httpClient.PostAsync("/oauth2/v4/token", stringContent);
            var gaResponse = JsonConvert.DeserializeObject<GaToken>(await resp.Result.Content.ReadAsStringAsync());


            // Create the DateRange object.
            DateRange dateRange = new DateRange() { StartDate = startDate, EndDate = endDate };

            var metrics = GetGaMetrics();

            Dimension date = new Dimension
            {
                Name = "ga:date"
            };

            // Create the ReportRequest object. 224814020
            ReportRequest reportRequest = new ReportRequest
            {
                ViewId = "211487879",
                Metrics = metrics,
                DateRanges = new List<DateRange> { dateRange },
                Dimensions = new List<Dimension> { date }
            };

            var requests = new List<ReportRequest>();
            requests.Add(reportRequest);

            try
            {
                // Call the batchGet method.
                var response = GetGaReports(requests, "", gaResponse.access_token);
                if (response != null)
                {
                    var report = response.Reports[0];

                    GoogleCustomDataDto customData = new GoogleCustomDataDto();
                    customData.Sessions = report.Data.Totals[0].Values[0];
                    customData.Users = report.Data.Totals[0].Values[1];
                    customData.Pageviews = report.Data.Totals[0].Values[2];
                    customData.PercentNewSessions = report.Data.Totals[0].Values[3];
                    customData.BounceRate = report.Data.Totals[0].Values[4];
                    customData.PageviewsPerSession = report.Data.Totals[0].Values[5];
                    customData.AvgSessionDuration = report.Data.Totals[0].Values[6];
                    customData.GoalCompletionsAll = report.Data.Totals[0].Values[7];
                    customData.GoalConversionRateAll = report.Data.Totals[0].Values[8];

                    gaReportsDto.GoogleCustomDataDto = customData;

                    List<ReportRow> rows = (List<ReportRow>)report.Data.Rows;

                    foreach (ReportRow row in rows)
                    {
                        GoogleCustomDataDto googleCustomDataDto = new GoogleCustomDataDto();
                        googleCustomDataDto.Date = row.Dimensions[0];

                        googleCustomDataDto.Sessions = row.Metrics[0].Values[0];
                        googleCustomDataDto.Users = row.Metrics[0].Values[1];
                        googleCustomDataDto.Pageviews = row.Metrics[0].Values[2];
                        googleCustomDataDto.PercentNewSessions = row.Metrics[0].Values[3];
                        googleCustomDataDto.BounceRate = row.Metrics[0].Values[4];
                        googleCustomDataDto.PageviewsPerSession = row.Metrics[0].Values[5];
                        googleCustomDataDto.AvgSessionDuration = row.Metrics[0].Values[6];
                        googleCustomDataDto.GoalCompletionsAll = row.Metrics[0].Values[7];
                        googleCustomDataDto.GoalConversionRateAll = row.Metrics[0].Values[8];

                        listGoogleCustomDataDto.Add(googleCustomDataDto);

                    }

                    GaPreparedDataDto gaPreparedDataDto = new GaPreparedDataDto();
                    gaPreparedDataDto.Date = listGoogleCustomDataDto.Select(x => x.Date).ToList();
                    gaPreparedDataDto.Sessions = listGoogleCustomDataDto.Select(x => x.Sessions).ToList();
                    gaPreparedDataDto.Users = listGoogleCustomDataDto.Select(x => x.Users).ToList();
                    gaPreparedDataDto.Pageviews = listGoogleCustomDataDto.Select(x => x.Pageviews).ToList();
                    gaPreparedDataDto.PercentNewSessions = listGoogleCustomDataDto.Select(x => x.PercentNewSessions).ToList();
                    gaPreparedDataDto.BounceRate = listGoogleCustomDataDto.Select(x => x.BounceRate).ToList();
                    gaPreparedDataDto.PageviewsPerSession = listGoogleCustomDataDto.Select(x => x.PageviewsPerSession).ToList();
                    gaPreparedDataDto.AvgSessionDuration = listGoogleCustomDataDto.Select(x => x.AvgSessionDuration).ToList();
                    gaPreparedDataDto.GoalCompletionsAll = listGoogleCustomDataDto.Select(x => x.GoalCompletionsAll).ToList();
                    gaPreparedDataDto.GoalConversionRateAll = listGoogleCustomDataDto.Select(x => x.GoalConversionRateAll).ToList();

                    gaReportsDto.GaPreparedDataDto = gaPreparedDataDto;

                }

                return gaReportsDto;
            }
            catch (Exception ex)
            {
                var exception = ex;
            }

            return gaReportsDto;
        }


        /// <summary>
        /// Get Behavior Report
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <returns>BehaviorDto</returns>
        public BehaviorDto GetGaBehaviorAnalyticsReports(string campaignId, string startDate, string endDate)
        {
            BehaviorDto behaviorDto = new BehaviorDto();

            var gaSetup = GetGaByCampaignId(campaignId);

            if (gaSetup != null)
            {
                // Create the DateRange object.
                DateRange dateRange = new DateRange() { StartDate = startDate, EndDate = endDate };

                // Create the Metrics object.
                Metric sessions = new Metric { Expression = "ga:sessions", Alias = "Sessions" };

                // Create the Metrics object.
                Metric newUsers = new Metric { Expression = "ga:newUsers", Alias = "User" };

                Metric pageviews = new Metric { Expression = "ga:pageviews", Alias = "Pageviews" };

                Metric percentNewSessions = new Metric { Expression = "ga:percentNewSessions", Alias = "PercentNewSessions" };

                Metric bounceRate = new Metric { Expression = "ga:bounceRate", Alias = "BounceRate" };

                Metric pageviewsPerSession = new Metric { Expression = "ga:pageviewsPerSession", Alias = "PageviewsPerSession" };

                Metric avgSessionDuration = new Metric { Expression = "ga:avgSessionDuration", Alias = "AvgSessionDuration" };

                Metric goalCompletionsAll = new Metric { Expression = "ga:goalCompletionsAll", Alias = "GoalCompletionsAll" };

                Metric goalConversionRateAll = new Metric { Expression = "ga:goalConversionRateAll", Alias = "GoalConversionRateAll" };

                Metric totalEvents = new Metric { Expression = "ga:totalEvents", Alias = "TotalEvents" };

                Metric uniqueEvents = new Metric { Expression = "ga:uniqueEvents", Alias = "UniqueEvents" };

                Metric eventValue = new Metric { Expression = "ga:eventValue", Alias = "EventValue" };

                Metric avgEventValue = new Metric { Expression = "ga:avgEventValue", Alias = "AvgEventValue" };

                Metric uniquePageviews = new Metric { Expression = "ga:uniquePageviews", Alias = "UniquePageviews" };

                Metric domInteractiveTime = new Metric { Expression = "ga:domInteractiveTime", Alias = "DomInteractiveTime" };

                Metric domLatencyMetricsSample = new Metric { Expression = "ga:domLatencyMetricsSample", Alias = "DomLatencyMetricsSample" };

                Metric pageLoadTime = new Metric { Expression = "ga:pageLoadTime", Alias = "PageLoadTime" };

                Metric avgPageLoadTime = new Metric { Expression = "ga:avgPageLoadTime", Alias = "AvgPageLoadTime" };

                Metric pageLoadSample = new Metric { Expression = "ga:pageLoadSample", Alias = "PageLoadSample" };

                Dimension pagePath = new Dimension
                {
                    Name = "ga:pagePath"
                };
                Dimension landingPagePath = new Dimension
                {
                    Name = "ga:landingPagePath"
                };

                ReportRequest pagePathReq = new ReportRequest
                {
                    ViewId = gaSetup.ViewID,
                    Metrics = new List<Metric> { sessions, newUsers, pageviews, percentNewSessions, bounceRate, pageviewsPerSession, avgSessionDuration, goalCompletionsAll, goalConversionRateAll },
                    DateRanges = new List<DateRange> { dateRange },
                    Dimensions = new List<Dimension> { pagePath }
                };

                ReportRequest landingPageReq = new ReportRequest
                {
                    ViewId = gaSetup.ViewID,
                    Metrics = new List<Metric> { sessions, newUsers, pageviews, percentNewSessions, bounceRate, pageviewsPerSession, avgSessionDuration, goalCompletionsAll, goalConversionRateAll },
                    DateRanges = new List<DateRange> { dateRange },
                    Dimensions = new List<Dimension> { landingPagePath }
                };

                ReportRequest eventReq = new ReportRequest
                {
                    ViewId = gaSetup.ViewID,
                    Metrics = new List<Metric> { totalEvents, uniqueEvents, eventValue, avgEventValue, sessions, pageviewsPerSession, newUsers },
                    DateRanges = new List<DateRange> { dateRange }

                };

                ReportRequest siteSpeedReq = new ReportRequest
                {
                    ViewId = gaSetup.ViewID,
                    Metrics = new List<Metric> { pageviews, uniquePageviews, avgSessionDuration, domInteractiveTime, domLatencyMetricsSample, pageLoadTime, avgPageLoadTime, pageLoadSample },
                    DateRanges = new List<DateRange> { dateRange },
                    Dimensions = new List<Dimension> { pagePath }
                };

                var requests = new List<ReportRequest>();
                requests.Add(pagePathReq);
                requests.Add(landingPageReq);
                requests.Add(eventReq);
                requests.Add(siteSpeedReq);

                try
                {
                    // Call the batchGet method.
                    var reportResponse = GetGaReports(requests, gaSetup.GoogleAccountSetups.RefreshToken, gaSetup.GoogleAccountSetups.AccessToken);

                    List<ReportRow> rowsPagePath = (List<ReportRow>)reportResponse.Reports[0].Data.Rows;

                    List<ReportRow> rowsLandingPagePath = (List<ReportRow>)reportResponse.Reports[1].Data.Rows;

                    List<ReportRow> rowsEvent = (List<ReportRow>)reportResponse.Reports[2].Data.Rows;

                    List<ReportRow> rowsSiteSpeed = (List<ReportRow>)reportResponse.Reports[3].Data.Rows;

                    if (rowsPagePath != null && rowsPagePath.Count > 0)
                    {
                        behaviorDto.Behavior = new List<GoogleCustomDataDto>();

                        foreach (var row in rowsPagePath)
                        {
                            GoogleCustomDataDto pagePathData = new GoogleCustomDataDto();

                            pagePathData.Name = row.Dimensions[0];
                            pagePathData.Sessions = row.Metrics[0].Values[0];
                            pagePathData.Users = row.Metrics[0].Values[1];


                            pagePathData.PercentNewSessions = row.Metrics[0].Values[3];
                            pagePathData.BounceRate = row.Metrics[0].Values[4];
                            pagePathData.PageviewsPerSession = row.Metrics[0].Values[5];

                            pagePathData.AvgSessionDuration = row.Metrics[0].Values[6];
                            pagePathData.GoalCompletionsAll = row.Metrics[0].Values[7];
                            pagePathData.GoalConversionRateAll = row.Metrics[0].Values[8];

                            behaviorDto.Behavior.Add(pagePathData);
                        }
                    }

                    if (rowsLandingPagePath != null && rowsLandingPagePath.Count > 0)
                    {
                        behaviorDto.LandingPages = new List<GoogleCustomDataDto>();

                        foreach (var row in rowsLandingPagePath)
                        {
                            GoogleCustomDataDto landingPageData = new GoogleCustomDataDto();

                            landingPageData.Name = row.Dimensions[0];
                            landingPageData.Sessions = row.Metrics[0].Values[0];
                            landingPageData.Users = row.Metrics[0].Values[1];
                            landingPageData.Pageviews = row.Metrics[0].Values[2];

                            landingPageData.PercentNewSessions = row.Metrics[0].Values[3];
                            landingPageData.BounceRate = row.Metrics[0].Values[4];
                            landingPageData.PageviewsPerSession = row.Metrics[0].Values[5];

                            landingPageData.AvgSessionDuration = row.Metrics[0].Values[6];
                            landingPageData.GoalCompletionsAll = row.Metrics[0].Values[7];
                            landingPageData.GoalConversionRateAll = row.Metrics[0].Values[8];

                            behaviorDto.LandingPages.Add(landingPageData);
                        }
                    }

                    if (rowsSiteSpeed != null && rowsLandingPagePath.Count > 0)
                    {
                        behaviorDto.SiteSpeed = new List<SiteSpeed>();

                        foreach (var row in rowsSiteSpeed)
                        {
                            SiteSpeed siteSpeed = new SiteSpeed();

                            siteSpeed.Name = row.Dimensions[0];
                            siteSpeed.Pageviews = row.Metrics[0].Values[0];
                            siteSpeed.UniquePageviews = row.Metrics[0].Values[1];


                            siteSpeed.AvgSessionDuration = row.Metrics[0].Values[2];
                            siteSpeed.DomInteractiveTime = row.Metrics[0].Values[3];
                            siteSpeed.DomLatencyMatricsSample = row.Metrics[0].Values[4];

                            siteSpeed.PageLoadTime = row.Metrics[0].Values[5];
                            siteSpeed.AvgPageLoadTime = row.Metrics[0].Values[6];
                            siteSpeed.PageLoadSample = row.Metrics[0].Values[7];

                            behaviorDto.SiteSpeed.Add(siteSpeed);
                        }

                    }

                    return behaviorDto;
                }
                catch (Exception ex)
                {
                    var exception = ex;
                }
            }
            return behaviorDto;
        }

        /// <summary>
        /// Get Conversion DATA
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <returns>ConversionDto</returns>
        public ConversionDto GetGaConversionsAnalyticsReports(string campaignId, string startDate, string endDate)
        {
            ConversionDto conversionDto = new ConversionDto();

            var gaSetup = GetGaByCampaignId(campaignId);

            if (gaSetup != null)
            {
                // Create the DateRange object.
                DateRange dateRange = new DateRange() { StartDate = startDate, EndDate = endDate };

                // Create the Metrics object.
                Metric sessions = new Metric { Expression = "ga:sessions", Alias = "Sessions" };

                // Create the Metrics object.
                Metric newUsers = new Metric { Expression = "ga:newUsers", Alias = "User" };

                Metric pageviews = new Metric { Expression = "ga:pageviews", Alias = "Pageviews" };

                Metric percentNewSessions = new Metric { Expression = "ga:percentNewSessions", Alias = "PercentNewSessions" };

                Metric bounceRate = new Metric { Expression = "ga:bounceRate", Alias = "BounceRate" };

                Metric pageviewsPerSession = new Metric { Expression = "ga:pageviewsPerSession", Alias = "PageviewsPerSession" };

                Metric avgSessionDuration = new Metric { Expression = "ga:avgSessionDuration", Alias = "AvgSessionDuration" };

                Metric goalCompletionsAll = new Metric { Expression = "ga:goalCompletionsAll", Alias = "GoalCompletionsAll" };

                Metric goalConversionRateAll = new Metric { Expression = "ga:goalConversionRateAll", Alias = "GoalConversionRateAll" };

                Metric transactions = new Metric { Expression = "ga:transactions", Alias = "Transactions" };

                Metric transactionRevenue = new Metric { Expression = "ga:transactionRevenue", Alias = "TransactionRevenue" };

                Metric transactionTax = new Metric { Expression = "ga:transactionTax", Alias = "TransactionTax" };

                Metric localTransactionShipping = new Metric { Expression = "ga:localTransactionShipping", Alias = "LocalTransactionShipping" };

                Dimension month = new Dimension
                {
                    Name = "ga:month"
                };

                ReportRequest conversionsReq = new ReportRequest
                {
                    ViewId = gaSetup.ViewID,
                    Metrics = new List<Metric> { transactions, transactionRevenue, transactionTax, localTransactionShipping },
                    DateRanges = new List<DateRange> { dateRange },
                    Dimensions = new List<Dimension> { month }
                };

                var requests = new List<ReportRequest>();
                requests.Add(conversionsReq);

                try
                {
                    // Call the batchGet method.
                    var reportResponse = GetGaReports(requests, gaSetup.GoogleAccountSetups.RefreshToken, gaSetup.GoogleAccountSetups.AccessToken);

                    List<ReportRow> rowsPagePath = (List<ReportRow>)reportResponse.Reports[0].Data.Rows;

                    List<Conversion> ConversionList = new List<Conversion>();

                    if (rowsPagePath != null)
                    {
                        foreach (ReportRow row in rowsPagePath)
                        {
                            Conversion conversion = new Conversion();
                            conversion.Month = row.Dimensions[0];
                            conversion.Transactions = row.Metrics[0].Values[0];
                            conversion.TransactionsRevenue = row.Metrics[0].Values[1];
                            conversion.TransactionsTax = row.Metrics[0].Values[2];
                            conversion.LocalTransactionsShipping = row.Metrics[0].Values[3];

                            ConversionList.Add(conversion);
                        }

                        ConversionPrepared getConversion = new ConversionPrepared();
                        getConversion.Month = ConversionList.Select(x => x.Month).ToList();
                        getConversion.Transactions = ConversionList.Select(x => x.Transactions).ToList();
                        getConversion.TransactionsRevenue = ConversionList.Select(x => x.TransactionsRevenue).ToList();
                        getConversion.TransactionsTax = ConversionList.Select(x => x.TransactionsTax).ToList();
                        getConversion.LocalTransactionsShipping = ConversionList.Select(x => x.LocalTransactionsShipping).ToList();

                        conversionDto.Conversion = getConversion;

                    }
                    else
                    {
                        ConversionPrepared getConversion = new ConversionPrepared();

                        getConversion.Transactions = new List<string> { "0" };
                        getConversion.TransactionsRevenue = new List<string> { "0" };
                        getConversion.TransactionsTax = new List<string> { "0" };
                        getConversion.LocalTransactionsShipping = new List<string> { "0" };

                        conversionDto.Conversion = getConversion;
                        conversionDto.Ecommerce = getConversion;
                        conversionDto.GoalConversion = new GoogleCustomDataDto();
                    }

                    return conversionDto;
                }
                catch (Exception ex)
                {
                    var exception = ex;
                }
            }
            return conversionDto;
        }

        /// <summary>
        /// Prepare matrics
        /// </summary>
        /// <returns>list of matrics</returns>
        public List<Metric> GetGaMetrics()
        {
            var matrics = new List<Metric>();

            matrics.Add(new Metric { Expression = "ga:sessions", Alias = "Sessions" });

            // Create the Metrics object.
            matrics.Add(new Metric { Expression = "ga:newUsers", Alias = "User" });

            matrics.Add(new Metric { Expression = "ga:pageviews", Alias = "Pageviews" });

            matrics.Add(new Metric { Expression = "ga:percentNewSessions", Alias = "PercentNewSessions" });

            matrics.Add(new Metric { Expression = "ga:bounceRate", Alias = "BounceRate" });

            matrics.Add(new Metric { Expression = "ga:pageviewsPerSession", Alias = "PageviewsPerSession" });

            matrics.Add(new Metric { Expression = "ga:avgSessionDuration", Alias = "AvgSessionDuration" });

            matrics.Add(new Metric { Expression = "ga:goalCompletionsAll", Alias = "GoalCompletionsAll" });

            matrics.Add(new Metric { Expression = "ga:goalConversionRateAll", Alias = "GoalConversionRateAll" });

            return matrics;

        }

        /// <summary>
        /// Get Ga By CampaignId
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <returns>status of the operations</returns>
        public GoogleAnalyticsAccountDto GetGaByCampaignId(string campaignId)
        {
            return _googleanalyticsaccountRepository.GetAllEntities(true).Where(x => x.CampaignID == campaignId && x.Active == true).Select(ga => new GoogleAnalyticsAccountDto
            {
                Id = ga.Id,
                AccountID = ga.AccountID,
                AccountName = ga.AccountName,
                ViewID = ga.ViewID,
                ViewName = ga.ViewName,
                CampaignID = ga.CampaignID,
                GoogleAccountSetupID = ga.GoogleAccountSetupID,
                PropertyID = ga.PropertyID,
                WebsiteUrl = ga.WebsiteUrl,
                Active = ga.Active,
                GoogleAccountSetups = new GoogleAccountSetupDto
                {
                    Id = ga.GoogleAccountSetup.Id,
                    AccessToken = ga.GoogleAccountSetup.AccessToken,
                    IsAuthorize = ga.GoogleAccountSetup.IsAuthorize,
                    RefreshToken = ga.GoogleAccountSetup.RefreshToken,
                    UserId = ga.GoogleAccountSetup.UserId,
                    UserName = ga.GoogleAccountSetup.UserName
                }

            }).FirstOrDefault();
        }

        /// <summary>
        /// Executes requests and Get Ga Reports
        /// </summary>
        /// <param name="requests">requests</param>
        /// <param name="refreshToken">refreshToken</param>
        /// <param name="accessToken">accessToken</param>
        /// <returns>status of the operation</returns>
        public GetReportsResponse GetGaReports(List<ReportRequest> requests, string refreshToken, string accessToken)
        {
            UserCredential customCredential = null;

            GetReportsRequest getReport = new GetReportsRequest() { ReportRequests = requests };

            // Removed hardcoded credentials from commented code
            GoogleCredential cred = GoogleCredential.FromAccessToken(accessToken);


            var reportService = new AnalyticsReportingService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = cred,
                ApplicationName = "Analytics Reporting"
            });



            // Call the batchGet method.
            GetReportsResponse response = reportService.Reports.BatchGet(getReport).Execute();

            return response;
        }


        public GetReportsResponse GetGoogleAnalyticsReports(List<ReportRequest> requests, string refreshToken, string accessToken)
        {
            GetReportsRequest getReport = new GetReportsRequest() { ReportRequests = requests };

            GoogleCredential cred = GoogleCredential.FromAccessToken(accessToken);

            var reportService = new AnalyticsReportingService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = cred,
                ApplicationName = "Analytics Reporting"
            });

            // Call the batchGet method.
            GetReportsResponse response = reportService.Reports.BatchGet(getReport).Execute();

            return response;
        }

        /// <summary>
        /// Get TrafficSources Reports
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <returns>status of the operation</returns>
        public async Task<ListTrafficSource> GetTrafficSourcesReports(string campaignId, string startDate, string endDate)
        {
            TrafficSources trafficSources = new TrafficSources();

            ListTrafficSource trafficSource = new ListTrafficSource();

            trafficSource.TotalData = new TrafficSources();



            // Create the DateRange object.
            DateRange dateRange = new DateRange() { StartDate = "2021-11-10", EndDate = "2021-12-09" };

            var metrics = GetGaMetrics();

            Dimension hasSocialSourceReferral = new Dimension
            {
                Name = "ga:hasSocialSourceReferral"
            };

            Dimension source = new Dimension
            {
                Name = "ga:source"
            };

            Dimension medium = new Dimension
            {
                Name = "ga:medium"
            };

            Dimension sourceMedium = new Dimension
            {
                Name = "ga:sourceMedium"
            };

            Dimension date = new Dimension
            {
                Name = "ga:date"
            };

            ReportRequest socialRequest = new ReportRequest
            {
                ViewId = "211487879",
                Metrics = metrics,
                DateRanges = new List<DateRange> { dateRange },
                Dimensions = new List<Dimension> { hasSocialSourceReferral, date },
                FiltersExpression = "ga:hasSocialSourceReferral==Yes"
            };

            ReportRequest sourceRequest = new ReportRequest
            {
                ViewId = "211487879",
                Metrics = metrics,
                DateRanges = new List<DateRange> { dateRange },
                Dimensions = new List<Dimension> { source, date },
                FiltersExpression = "ga:source==(direct)"
            };

            ReportRequest mediumRequest = new ReportRequest
            {
                ViewId = "211487879",
                Metrics = metrics,
                DateRanges = new List<DateRange> { dateRange },
                Dimensions = new List<Dimension> { medium, date },
                FiltersExpression = "ga:medium==organic"
            };

            ReportRequest referralRequest = new ReportRequest
            {
                ViewId = "211487879",
                Metrics = metrics,
                DateRanges = new List<DateRange> { dateRange },
                Dimensions = new List<Dimension> { medium, hasSocialSourceReferral, date },
                FiltersExpression = "ga:medium==referral;ga:hasSocialSourceReferral==No"
            };

            ReportRequest displayRequest = new ReportRequest
            {
                ViewId = "211487879",
                Metrics = metrics,
                DateRanges = new List<DateRange> { dateRange },
                Dimensions = new List<Dimension> { sourceMedium, date },
                FiltersExpression = "ga:sourceMedium==google / cpc"
            };

            var requests = new List<ReportRequest>();
            requests.Add(sourceRequest); //redirect
            requests.Add(mediumRequest); // organic search
            requests.Add(referralRequest);
            requests.Add(socialRequest);
            requests.Add(displayRequest);

            var gaClientId = _configuration["ClientIdForGoogleAds"];
            var gaClientSecret = _configuration["ClientSecretForGoogleAds"];

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://www.googleapis.com"),
            };

            var data = new
            {
                client_id = gaClientId,
                client_secret = gaClientSecret,
                // IMPORTANT: This should use the refresh_token from the specific user/account setup
                // The refresh_token parameter should be passed to this method or retrieved from database
                refresh_token = "", // TODO: Pass refresh token as parameter
                grant_type = "refresh_token",
                access_type = "offline"
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var resp = httpClient.PostAsync("/oauth2/v4/token", stringContent);
            var gaResponse = JsonConvert.DeserializeObject<GaToken>(await resp.Result.Content.ReadAsStringAsync());



            try
            {
                // Call the batchGet method.
                var reportResponse = GetGaReports(requests, "", gaResponse.access_token);

                if (reportResponse != null)
                {
                    List<ReportRow> rowsSource = (List<ReportRow>)reportResponse.Reports[0].Data.Rows;

                    List<ReportRow> rowsMedium = (List<ReportRow>)reportResponse.Reports[1].Data.Rows;

                    List<ReportRow> rowsReferral = (List<ReportRow>)reportResponse.Reports[2].Data.Rows;

                    List<ReportRow> rowsSocial = (List<ReportRow>)reportResponse.Reports[3].Data.Rows;

                    List<ReportRow> rowsDisplay = (List<ReportRow>)reportResponse.Reports[4].Data.Rows;

                    if (rowsSource != null && rowsSource.Count > 0)
                    {
                        trafficSources.Direct = new GoogleCustomDataDto();
                        trafficSources.Direct.Sessions = reportResponse.Reports[0].Data.Totals[0].Values[0];
                        trafficSources.Direct.Users = reportResponse.Reports[0].Data.Totals[0].Values[1];
                        trafficSources.Direct.Pageviews = reportResponse.Reports[0].Data.Totals[0].Values[2];

                        trafficSources.Direct.PercentNewSessions = reportResponse.Reports[0].Data.Totals[0].Values[3];
                        trafficSources.Direct.BounceRate = reportResponse.Reports[0].Data.Totals[0].Values[4];
                        trafficSources.Direct.PageviewsPerSession = reportResponse.Reports[0].Data.Totals[0].Values[5];

                        trafficSources.Direct.AvgSessionDuration = reportResponse.Reports[0].Data.Totals[0].Values[6];
                        trafficSources.Direct.GoalCompletionsAll = reportResponse.Reports[0].Data.Totals[0].Values[7];
                        trafficSources.Direct.GoalConversionRateAll = reportResponse.Reports[0].Data.Totals[0].Values[8];
                        trafficSource.TotalData.Direct = trafficSources.Direct;
                        List<GoogleCustomDataDto> listGoogleCustomDataDto = new List<GoogleCustomDataDto>();

                        foreach (ReportRow row in rowsSource)
                        {
                            GoogleCustomDataDto googleCustomDataDto = new GoogleCustomDataDto();
                            googleCustomDataDto.Date = row.Dimensions[1];
                            googleCustomDataDto.Sessions = row.Metrics[0].Values[0];
                            listGoogleCustomDataDto.Add(googleCustomDataDto);
                        }

                        GaPreparedDataDto gaPreparedDataDto = new GaPreparedDataDto();
                        gaPreparedDataDto.Date = listGoogleCustomDataDto.Select(x => x.Date).ToList();
                        gaPreparedDataDto.Sessions = listGoogleCustomDataDto.Select(x => x.Sessions).ToList();
                        trafficSource.Source = gaPreparedDataDto;
                    }

                    if (rowsMedium != null && rowsMedium.Count > 0)
                    {
                        trafficSources.Organic = new GoogleCustomDataDto();
                        trafficSources.Organic.Sessions = reportResponse.Reports[1].Data.Totals[0].Values[0];
                        trafficSources.Organic.Users = reportResponse.Reports[1].Data.Totals[0].Values[1];
                        trafficSources.Organic.Pageviews = reportResponse.Reports[1].Data.Totals[0].Values[2];

                        trafficSources.Organic.PercentNewSessions = reportResponse.Reports[1].Data.Totals[0].Values[3];
                        trafficSources.Organic.BounceRate = reportResponse.Reports[1].Data.Totals[0].Values[4];
                        trafficSources.Organic.PageviewsPerSession = reportResponse.Reports[1].Data.Totals[0].Values[5];

                        trafficSources.Organic.AvgSessionDuration = reportResponse.Reports[1].Data.Totals[0].Values[6];
                        trafficSources.Organic.GoalCompletionsAll = reportResponse.Reports[1].Data.Totals[0].Values[7];
                        trafficSources.Organic.GoalConversionRateAll = reportResponse.Reports[1].Data.Totals[0].Values[8];
                        trafficSource.TotalData.Organic = trafficSources.Organic;

                        List<GoogleCustomDataDto> listGoogleCustomDataDto = new List<GoogleCustomDataDto>();

                        foreach (ReportRow row in rowsMedium)
                        {
                            GoogleCustomDataDto googleCustomDataDto = new GoogleCustomDataDto();
                            googleCustomDataDto.Date = row.Dimensions[1];
                            googleCustomDataDto.Sessions = row.Metrics[0].Values[0];
                            listGoogleCustomDataDto.Add(googleCustomDataDto);
                        }

                        GaPreparedDataDto gaPreparedDataDto = new GaPreparedDataDto();
                        gaPreparedDataDto.Date = listGoogleCustomDataDto.Select(x => x.Date).ToList();
                        gaPreparedDataDto.Sessions = listGoogleCustomDataDto.Select(x => x.Sessions).ToList();
                        trafficSource.Medium = gaPreparedDataDto;
                    }

                    if (rowsReferral != null && rowsReferral.Count > 0)
                    {
                        trafficSources.Referral = new GoogleCustomDataDto();
                        trafficSources.Referral.Sessions = reportResponse.Reports[2].Data.Totals[0].Values[0];
                        trafficSources.Referral.Users = reportResponse.Reports[2].Data.Totals[0].Values[1];
                        trafficSources.Referral.Pageviews = reportResponse.Reports[2].Data.Totals[0].Values[2];


                        trafficSources.Referral.PercentNewSessions = reportResponse.Reports[2].Data.Totals[0].Values[3];
                        trafficSources.Referral.BounceRate = reportResponse.Reports[2].Data.Totals[0].Values[4];
                        trafficSources.Referral.PageviewsPerSession = reportResponse.Reports[2].Data.Totals[0].Values[5];

                        trafficSources.Referral.AvgSessionDuration = reportResponse.Reports[2].Data.Totals[0].Values[6];
                        trafficSources.Referral.GoalCompletionsAll = reportResponse.Reports[2].Data.Totals[0].Values[7];
                        trafficSources.Referral.GoalConversionRateAll = reportResponse.Reports[2].Data.Totals[0].Values[8];
                        trafficSource.TotalData.Referral = trafficSources.Referral;

                        List<GoogleCustomDataDto> listGoogleCustomDataDto = new List<GoogleCustomDataDto>();

                        foreach (ReportRow row in rowsReferral)
                        {
                            GoogleCustomDataDto googleCustomDataDto = new GoogleCustomDataDto();
                            googleCustomDataDto.Date = row.Dimensions[2];
                            googleCustomDataDto.Sessions = row.Metrics[0].Values[0];
                            listGoogleCustomDataDto.Add(googleCustomDataDto);
                        }

                        GaPreparedDataDto gaPreparedDataDto = new GaPreparedDataDto();
                        gaPreparedDataDto.Date = listGoogleCustomDataDto.Select(x => x.Date).ToList();
                        gaPreparedDataDto.Sessions = listGoogleCustomDataDto.Select(x => x.Sessions).ToList();
                        trafficSource.Referral = gaPreparedDataDto;
                    }

                    if (rowsSocial != null && rowsSocial.Count > 0)
                    {
                        trafficSources.Social = new GoogleCustomDataDto();
                        trafficSources.Social.Sessions = reportResponse.Reports[3].Data.Totals[0].Values[0];
                        trafficSources.Social.Users = reportResponse.Reports[3].Data.Totals[0].Values[1];
                        trafficSources.Social.Pageviews = reportResponse.Reports[3].Data.Totals[0].Values[2];

                        trafficSources.Social.PercentNewSessions = reportResponse.Reports[3].Data.Totals[0].Values[3];
                        trafficSources.Social.BounceRate = reportResponse.Reports[3].Data.Totals[0].Values[4];
                        trafficSources.Social.PageviewsPerSession = reportResponse.Reports[3].Data.Totals[0].Values[5];

                        trafficSources.Social.AvgSessionDuration = reportResponse.Reports[3].Data.Totals[0].Values[6];
                        trafficSources.Social.GoalCompletionsAll = reportResponse.Reports[3].Data.Totals[0].Values[7];
                        trafficSources.Social.GoalConversionRateAll = reportResponse.Reports[3].Data.Totals[0].Values[8];
                        trafficSource.TotalData.Social = trafficSources.Social;

                        List<GoogleCustomDataDto> listGoogleCustomDataDto = new List<GoogleCustomDataDto>();

                        foreach (ReportRow row in rowsSocial)
                        {
                            GoogleCustomDataDto googleCustomDataDto = new GoogleCustomDataDto();
                            googleCustomDataDto.Date = row.Dimensions[1];
                            googleCustomDataDto.Sessions = row.Metrics[0].Values[0];
                            listGoogleCustomDataDto.Add(googleCustomDataDto);
                        }

                        GaPreparedDataDto gaPreparedDataDto = new GaPreparedDataDto();
                        gaPreparedDataDto.Date = listGoogleCustomDataDto.Select(x => x.Date).ToList();
                        gaPreparedDataDto.Sessions = listGoogleCustomDataDto.Select(x => x.Sessions).ToList();
                        trafficSource.Social = gaPreparedDataDto;
                    }

                    if (rowsDisplay != null && rowsDisplay.Count > 0)
                    {
                        trafficSources.Display = new GoogleCustomDataDto();
                        trafficSources.Display.Sessions = reportResponse.Reports[4].Data.Totals[0].Values[0];
                        trafficSources.Display.Users = reportResponse.Reports[4].Data.Totals[0].Values[1];
                        trafficSources.Display.Pageviews = reportResponse.Reports[4].Data.Totals[0].Values[2];

                        trafficSources.Display.PercentNewSessions = reportResponse.Reports[4].Data.Totals[0].Values[3];
                        trafficSources.Display.BounceRate = reportResponse.Reports[4].Data.Totals[0].Values[4];
                        trafficSources.Display.PageviewsPerSession = reportResponse.Reports[4].Data.Totals[0].Values[5];

                        trafficSources.Display.AvgSessionDuration = reportResponse.Reports[4].Data.Totals[0].Values[6];
                        trafficSources.Display.GoalCompletionsAll = reportResponse.Reports[4].Data.Totals[0].Values[7];
                        trafficSources.Display.GoalConversionRateAll = reportResponse.Reports[4].Data.Totals[0].Values[8];
                        trafficSource.TotalData.Display = trafficSources.Display;

                        List<GoogleCustomDataDto> listGoogleCustomDataDto = new List<GoogleCustomDataDto>();

                        foreach (ReportRow row in rowsSocial)
                        {
                            GoogleCustomDataDto googleCustomDataDto = new GoogleCustomDataDto();
                            googleCustomDataDto.Date = row.Dimensions[1];
                            googleCustomDataDto.Sessions = row.Metrics[0].Values[0];
                            listGoogleCustomDataDto.Add(googleCustomDataDto);
                        }

                        GaPreparedDataDto gaPreparedDataDto = new GaPreparedDataDto();
                        gaPreparedDataDto.Date = listGoogleCustomDataDto.Select(x => x.Date).ToList();
                        gaPreparedDataDto.Sessions = listGoogleCustomDataDto.Select(x => x.Sessions).ToList();
                        trafficSource.Display = gaPreparedDataDto;
                    }
                }

                return trafficSource;
            }
            catch (Exception ex)
            {
                var exception = ex;
            }


            return trafficSource;
        }

        public async Task<ListTrafficSource> GetAnalyticsOrganicTraffic(string profileId, string accessToken, string startDate, string endDate)
        {
            TrafficSources trafficSources = new TrafficSources();

            ListTrafficSource trafficSource = new ListTrafficSource();

            trafficSource.TotalData = new TrafficSources();

            // Create the DateRange object.
            //DateRange dateRange = new DateRange() { StartDate = "2021-11-10", EndDate = "2021-12-09" };

            DateRange dateRange = new DateRange() { StartDate = startDate, EndDate = endDate };

            var metrics = GetGaMetrics();

            Dimension hasSocialSourceReferral = new Dimension
            {
                Name = "ga:hasSocialSourceReferral"
            };

            Dimension source = new Dimension
            {
                Name = "ga:source"
            };

            Dimension medium = new Dimension
            {
                Name = "ga:medium"
            };

            Dimension sourceMedium = new Dimension
            {
                Name = "ga:sourceMedium"
            };

            Dimension date = new Dimension
            {
                Name = "ga:date"
            };

            ReportRequest socialRequest = new ReportRequest
            {
                ViewId = profileId,
                Metrics = metrics,
                DateRanges = new List<DateRange> { dateRange },
                Dimensions = new List<Dimension> { hasSocialSourceReferral, date },
                FiltersExpression = "ga:hasSocialSourceReferral==Yes"
            };

            ReportRequest sourceRequest = new ReportRequest
            {
                ViewId = profileId,
                Metrics = metrics,
                DateRanges = new List<DateRange> { dateRange },
                Dimensions = new List<Dimension> { source, date },
                FiltersExpression = "ga:source==(direct)"
            };

            ReportRequest mediumRequest = new ReportRequest
            {
                ViewId = profileId,
                Metrics = metrics,
                DateRanges = new List<DateRange> { dateRange },
                Dimensions = new List<Dimension> { medium, date },
                FiltersExpression = "ga:medium==organic"
            };

            ReportRequest referralRequest = new ReportRequest
            {
                ViewId = profileId,
                Metrics = metrics,
                DateRanges = new List<DateRange> { dateRange },
                Dimensions = new List<Dimension> { medium, hasSocialSourceReferral, date },
                FiltersExpression = "ga:medium==referral;ga:hasSocialSourceReferral==No"
            };

            ReportRequest displayRequest = new ReportRequest
            {
                ViewId = profileId,
                Metrics = metrics,
                DateRanges = new List<DateRange> { dateRange },
                Dimensions = new List<Dimension> { sourceMedium, date },
                FiltersExpression = "ga:sourceMedium==google / cpc"
            };

            var requests = new List<ReportRequest>();
            requests.Add(sourceRequest); //redirect
            requests.Add(mediumRequest); // organic search
            requests.Add(referralRequest);
            requests.Add(socialRequest);
            requests.Add(displayRequest);

            try
            {
                // Call the batchGet method.
                var reportResponse = GetGoogleAnalyticsReports(requests, "", accessToken);

                if (reportResponse != null)
                {
                    List<ReportRow> rowsSource = (List<ReportRow>)reportResponse.Reports[0].Data.Rows;

                    List<ReportRow> rowsMedium = (List<ReportRow>)reportResponse.Reports[1].Data.Rows;

                    List<ReportRow> rowsReferral = (List<ReportRow>)reportResponse.Reports[2].Data.Rows;

                    List<ReportRow> rowsSocial = (List<ReportRow>)reportResponse.Reports[3].Data.Rows;

                    List<ReportRow> rowsDisplay = (List<ReportRow>)reportResponse.Reports[4].Data.Rows;

                    if (rowsSource != null && rowsSource.Count > 0)
                    {
                        trafficSources.Direct = new GoogleCustomDataDto();
                        trafficSources.Direct.Sessions = reportResponse.Reports[0].Data.Totals[0].Values[0];
                        trafficSource.TotalData.Direct = trafficSources.Direct;
                        List<GoogleCustomDataDto> listGoogleCustomDataDto = new List<GoogleCustomDataDto>();

                        foreach (ReportRow row in rowsSource)
                        {
                            GoogleCustomDataDto googleCustomDataDto = new GoogleCustomDataDto();
                            googleCustomDataDto.Date = row.Dimensions[1];
                            googleCustomDataDto.Sessions = row.Metrics[0].Values[0];
                            listGoogleCustomDataDto.Add(googleCustomDataDto);
                        }

                        GaPreparedDataDto gaPreparedDataDto = new GaPreparedDataDto();
                        gaPreparedDataDto.Date = listGoogleCustomDataDto.Select(x => x.Date).ToList();
                        gaPreparedDataDto.Sessions = listGoogleCustomDataDto.Select(x => x.Sessions).ToList();
                        trafficSource.Source = gaPreparedDataDto;
                    }

                    if (rowsMedium != null && rowsMedium.Count > 0)
                    {
                        trafficSources.Organic = new GoogleCustomDataDto();
                        trafficSources.Organic.Sessions = reportResponse.Reports[1].Data.Totals[0].Values[0];

                        trafficSource.TotalData.Organic = trafficSources.Organic;

                        List<GoogleCustomDataDto> listGoogleCustomDataDto = new List<GoogleCustomDataDto>();

                        foreach (ReportRow row in rowsMedium)
                        {
                            GoogleCustomDataDto googleCustomDataDto = new GoogleCustomDataDto();
                            googleCustomDataDto.Date = row.Dimensions[1];
                            googleCustomDataDto.Sessions = row.Metrics[0].Values[0];
                            listGoogleCustomDataDto.Add(googleCustomDataDto);
                        }

                        GaPreparedDataDto gaPreparedDataDto = new GaPreparedDataDto();
                        gaPreparedDataDto.Date = listGoogleCustomDataDto.Select(x => x.Date).ToList();
                        gaPreparedDataDto.Sessions = listGoogleCustomDataDto.Select(x => x.Sessions).ToList();
                        trafficSource.Medium = gaPreparedDataDto;
                    }

                    if (rowsReferral != null && rowsReferral.Count > 0)
                    {
                        trafficSources.Referral = new GoogleCustomDataDto();
                        trafficSources.Referral.Sessions = reportResponse.Reports[2].Data.Totals[0].Values[0];
                        trafficSource.TotalData.Referral = trafficSources.Referral;

                        List<GoogleCustomDataDto> listGoogleCustomDataDto = new List<GoogleCustomDataDto>();

                        foreach (ReportRow row in rowsReferral)
                        {
                            GoogleCustomDataDto googleCustomDataDto = new GoogleCustomDataDto();
                            googleCustomDataDto.Date = row.Dimensions[2];
                            googleCustomDataDto.Sessions = row.Metrics[0].Values[0];
                            listGoogleCustomDataDto.Add(googleCustomDataDto);
                        }

                        GaPreparedDataDto gaPreparedDataDto = new GaPreparedDataDto();
                        gaPreparedDataDto.Date = listGoogleCustomDataDto.Select(x => x.Date).ToList();
                        gaPreparedDataDto.Sessions = listGoogleCustomDataDto.Select(x => x.Sessions).ToList();
                        trafficSource.Referral = gaPreparedDataDto;
                    }

                    if (rowsSocial != null && rowsSocial.Count > 0)
                    {
                        trafficSources.Social = new GoogleCustomDataDto();
                        trafficSources.Social.Sessions = reportResponse.Reports[3].Data.Totals[0].Values[0];
                        trafficSource.TotalData.Social = trafficSources.Social;

                        List<GoogleCustomDataDto> listGoogleCustomDataDto = new List<GoogleCustomDataDto>();

                        foreach (ReportRow row in rowsSocial)
                        {
                            GoogleCustomDataDto googleCustomDataDto = new GoogleCustomDataDto();
                            googleCustomDataDto.Date = row.Dimensions[1];
                            googleCustomDataDto.Sessions = row.Metrics[0].Values[0];
                            listGoogleCustomDataDto.Add(googleCustomDataDto);
                        }

                        GaPreparedDataDto gaPreparedDataDto = new GaPreparedDataDto();
                        gaPreparedDataDto.Date = listGoogleCustomDataDto.Select(x => x.Date).ToList();
                        gaPreparedDataDto.Sessions = listGoogleCustomDataDto.Select(x => x.Sessions).ToList();
                        trafficSource.Social = gaPreparedDataDto;
                    }

                    if (rowsDisplay != null && rowsDisplay.Count > 0)
                    {
                        trafficSources.Display = new GoogleCustomDataDto();
                        trafficSources.Display.Sessions = reportResponse.Reports[4].Data.Totals[0].Values[0];
                        trafficSource.TotalData.Display = trafficSources.Display;

                        List<GoogleCustomDataDto> listGoogleCustomDataDto = new List<GoogleCustomDataDto>();

                        foreach (ReportRow row in rowsSocial)
                        {
                            GoogleCustomDataDto googleCustomDataDto = new GoogleCustomDataDto();
                            googleCustomDataDto.Date = row.Dimensions[1];
                            googleCustomDataDto.Sessions = row.Metrics[0].Values[0];
                            listGoogleCustomDataDto.Add(googleCustomDataDto);
                        }

                        GaPreparedDataDto gaPreparedDataDto = new GaPreparedDataDto();
                        gaPreparedDataDto.Date = listGoogleCustomDataDto.Select(x => x.Date).ToList();
                        gaPreparedDataDto.Sessions = listGoogleCustomDataDto.Select(x => x.Sessions).ToList();
                        trafficSource.Display = gaPreparedDataDto;
                    }
                }

                return trafficSource;
            }
            catch (Exception ex)
            {
                var exception = ex;
            }


            return trafficSource;
        }


        /// <summary>
        /// Get Traffic SourcesMediums Reports
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <returns>status of the operation</returns>
        public SourcesMediums GetTrafficSourcesMediumsReports(string campaignId, string startDate, string endDate)
        {
            SourcesMediums sourcesMediums = new SourcesMediums();

            var gaSetup = GetGaByCampaignId(campaignId);

            if (gaSetup != null)
            {
                // Create the DateRange object.
                DateRange dateRange = new DateRange() { StartDate = startDate, EndDate = endDate };

                Dimension sourceMedium = new Dimension
                {
                    Name = "ga:sourceMedium"
                };

                var metrics = GetGaMetrics();

                ReportRequest reportRequest = new ReportRequest
                {
                    ViewId = gaSetup.ViewID,
                    Metrics = metrics,
                    DateRanges = new List<DateRange> { dateRange },
                    Dimensions = new List<Dimension> { sourceMedium }
                };

                var requests = new List<ReportRequest>();

                requests.Add(reportRequest);

                try
                {
                    // Call the batchGet method.
                    var reportResponse = GetGaReports(requests, gaSetup.GoogleAccountSetups.RefreshToken, gaSetup.GoogleAccountSetups.AccessToken);
                    if (reportResponse != null)
                    {
                        List<ReportRow> rowsSourceMediums = (List<ReportRow>)reportResponse.Reports[0].Data.Rows;

                        if (rowsSourceMediums != null && rowsSourceMediums.Count > 0)
                        {
                            sourcesMediums.SourcemediumList = new List<GoogleCustomDataDto>();
                            foreach (var row in rowsSourceMediums)
                            {
                                GoogleCustomDataDto sourceMediumData = new GoogleCustomDataDto();

                                sourceMediumData.Name = row.Dimensions[0];
                                sourceMediumData.Sessions = row.Metrics[0].Values[0];
                                sourceMediumData.Users = row.Metrics[0].Values[1];

                                sourceMediumData.PercentNewSessions = row.Metrics[0].Values[3];
                                sourceMediumData.BounceRate = row.Metrics[0].Values[4];
                                sourceMediumData.PageviewsPerSession = row.Metrics[0].Values[5];

                                sourceMediumData.AvgSessionDuration = row.Metrics[0].Values[6];
                                sourceMediumData.GoalCompletionsAll = row.Metrics[0].Values[7];
                                sourceMediumData.GoalConversionRateAll = row.Metrics[0].Values[8];

                                sourcesMediums.SourcemediumList.Add(sourceMediumData);
                            }
                        }
                    }
                    return sourcesMediums;
                }
                catch (Exception ex)
                {
                    var exception = ex;
                }
            }
            return sourcesMediums;
        }

        /// <summary>
        /// Get Campaign Reports
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <returns>status of the operation</returns>
        public Campaigns GetCampaignReports(string campaignId, string startDate, string endDate)
        {
            Campaigns campaigns = new Campaigns();

            var gaSetup = GetGaByCampaignId(campaignId);

            if (gaSetup != null)
            {
                // Create the DateRange object.
                DateRange dateRange = new DateRange() { StartDate = startDate, EndDate = endDate };

                Dimension campaign = new Dimension
                {
                    Name = "ga:campaign"
                };

                var metrics = GetGaMetrics();

                ReportRequest campaignRequest = new ReportRequest
                {
                    ViewId = gaSetup.ViewID,
                    Metrics = metrics,
                    DateRanges = new List<DateRange> { dateRange },
                    Dimensions = new List<Dimension> { campaign },
                    FiltersExpression = "ga:campaign!=(not set)"
                };

                ReportRequest campaignNotSetRequest = new ReportRequest
                {
                    ViewId = gaSetup.ViewID,
                    Metrics = metrics,
                    DateRanges = new List<DateRange> { dateRange },
                    Dimensions = new List<Dimension> { campaign },
                    FiltersExpression = "ga:campaign==(not set)"
                };

                var requests = new List<ReportRequest>();

                requests.Add(campaignRequest);
                requests.Add(campaignNotSetRequest);

                try
                {
                    // Call the batchGet method.
                    var reportResponse = GetGaReports(requests, gaSetup.GoogleAccountSetups.RefreshToken, gaSetup.GoogleAccountSetups.AccessToken);
                    if (reportResponse != null)
                    {
                        List<ReportRow> rowsCampaigns = (List<ReportRow>)reportResponse.Reports[0].Data.Rows;
                        List<ReportRow> rowsCampaignsNotSet = (List<ReportRow>)reportResponse.Reports[1].Data.Rows;

                        if (rowsCampaigns != null && rowsCampaigns.Count > 0)
                        {
                            campaigns.Campaign = new List<GoogleCustomDataDto>();
                            foreach (var row in rowsCampaigns)
                            {
                                GoogleCustomDataDto campaignData = new GoogleCustomDataDto();

                                campaignData.Sessions = row.Metrics[0].Values[0];
                                campaignData.Users = row.Metrics[0].Values[1];

                                campaignData.Name = row.Dimensions[0];
                                campaignData.PercentNewSessions = row.Metrics[0].Values[3];
                                campaignData.BounceRate = row.Metrics[0].Values[4];
                                campaignData.PageviewsPerSession = row.Metrics[0].Values[5];

                                campaignData.AvgSessionDuration = row.Metrics[0].Values[6];
                                campaignData.GoalCompletionsAll = row.Metrics[0].Values[7];
                                campaignData.GoalConversionRateAll = row.Metrics[0].Values[8];

                                campaigns.Campaign.Add(campaignData);
                            }
                        }

                        if (rowsCampaignsNotSet != null && rowsCampaignsNotSet.Count > 0)
                        {
                            campaigns.CampaignNotSet = new GoogleCustomDataDto();

                            campaigns.CampaignNotSet.Sessions = rowsCampaignsNotSet[0].Metrics[0].Values[0];
                            campaigns.CampaignNotSet.Users = rowsCampaignsNotSet[0].Metrics[0].Values[1];

                            campaigns.CampaignNotSet.Name = rowsCampaignsNotSet[0].Dimensions[0];
                            campaigns.CampaignNotSet.PercentNewSessions = rowsCampaignsNotSet[0].Metrics[0].Values[3];
                            campaigns.CampaignNotSet.BounceRate = rowsCampaignsNotSet[0].Metrics[0].Values[4];
                            campaigns.CampaignNotSet.PageviewsPerSession = rowsCampaignsNotSet[0].Metrics[0].Values[5];

                            campaigns.CampaignNotSet.AvgSessionDuration = rowsCampaignsNotSet[0].Metrics[0].Values[6];
                            campaigns.CampaignNotSet.GoalCompletionsAll = rowsCampaignsNotSet[0].Metrics[0].Values[7];
                            campaigns.CampaignNotSet.GoalConversionRateAll = rowsCampaignsNotSet[0].Metrics[0].Values[8];
                        }

                        return campaigns;
                    }
                }
                catch (Exception ex)
                {
                    var exception = ex;
                }
            }
            return campaigns;
        }

        /// <summary>
        /// Get Audience Reports
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <returns>status of the operation</returns>
        public Audience GetAudienceReports(string campaignId, string startDate, string endDate)
        {
            Audience audience = new Audience();

            var gaSetup = GetGaByCampaignId(campaignId);

            if (gaSetup != null)
            {
                // Create the DateRange object.
                DateRange dateRange = new DateRange() { StartDate = startDate, EndDate = endDate };

                Dimension userType = new Dimension
                {
                    Name = "ga:userType"
                };

                var metrics = GetGaMetrics();

                ReportRequest newVisitorReq = new ReportRequest
                {
                    ViewId = gaSetup.ViewID,
                    Metrics = metrics,
                    DateRanges = new List<DateRange> { dateRange },
                    Dimensions = new List<Dimension> { userType },
                    FiltersExpression = "ga:userType==New Visitor"
                };

                ReportRequest returningReq = new ReportRequest
                {
                    ViewId = gaSetup.ViewID,
                    Metrics = metrics,
                    DateRanges = new List<DateRange> { dateRange },
                    Dimensions = new List<Dimension> { userType },
                    FiltersExpression = "ga:userType==Returning Visitor"
                };

                var requests = new List<ReportRequest>();

                requests.Add(newVisitorReq);
                requests.Add(returningReq);

                try
                {
                    // Call the batchGet method.
                    var reportResponse = GetGaReports(requests, gaSetup.GoogleAccountSetups.RefreshToken, gaSetup.GoogleAccountSetups.AccessToken);
                    if (reportResponse != null)
                    {
                        List<ReportRow> rowsNewVisitor = (List<ReportRow>)reportResponse.Reports[0].Data.Rows;
                        List<ReportRow> rowsReturning = (List<ReportRow>)reportResponse.Reports[1].Data.Rows;

                        if (rowsNewVisitor != null && rowsNewVisitor.Count > 0)
                        {
                            audience.NewVisitors = new GoogleCustomDataDto();

                            audience.NewVisitors.Sessions = rowsNewVisitor[0].Metrics[0].Values[0];
                            audience.NewVisitors.Users = rowsNewVisitor[0].Metrics[0].Values[1];

                            audience.NewVisitors.Name = rowsNewVisitor[0].Dimensions[0];
                            audience.NewVisitors.PercentNewSessions = rowsNewVisitor[0].Metrics[0].Values[3];
                            audience.NewVisitors.BounceRate = rowsNewVisitor[0].Metrics[0].Values[4];
                            audience.NewVisitors.PageviewsPerSession = rowsNewVisitor[0].Metrics[0].Values[5];

                            audience.NewVisitors.AvgSessionDuration = rowsNewVisitor[0].Metrics[0].Values[6];
                            audience.NewVisitors.GoalCompletionsAll = rowsNewVisitor[0].Metrics[0].Values[7];
                            audience.NewVisitors.GoalConversionRateAll = rowsNewVisitor[0].Metrics[0].Values[8];
                        }

                        if (rowsReturning != null && rowsReturning.Count > 0)
                        {
                            audience.ReturnVisitors = new GoogleCustomDataDto();

                            audience.ReturnVisitors.Sessions = rowsReturning[0].Metrics[0].Values[0];
                            audience.ReturnVisitors.Users = rowsReturning[0].Metrics[0].Values[1];

                            audience.ReturnVisitors.Name = rowsReturning[0].Dimensions[0];
                            audience.ReturnVisitors.PercentNewSessions = rowsReturning[0].Metrics[0].Values[3];
                            audience.ReturnVisitors.BounceRate = rowsReturning[0].Metrics[0].Values[4];
                            audience.ReturnVisitors.PageviewsPerSession = rowsReturning[0].Metrics[0].Values[5];

                            audience.ReturnVisitors.AvgSessionDuration = rowsReturning[0].Metrics[0].Values[6];
                            audience.ReturnVisitors.GoalCompletionsAll = rowsReturning[0].Metrics[0].Values[7];
                            audience.ReturnVisitors.GoalConversionRateAll = rowsReturning[0].Metrics[0].Values[8];

                        }
                        return audience;
                    }
                }
                catch (Exception ex)
                {
                    var exception = ex;
                }
            }
            return audience;
        }

        /// <summary>
        /// Get DeviceCategory Reports
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <returns>status of the operation</returns>
        public DeviceCategory GetDeviceCategoryReports(string campaignId, string startDate, string endDate)
        {
            DeviceCategory deviceCategory = new DeviceCategory();

            var gaSetup = GetGaByCampaignId(campaignId);

            if (gaSetup != null)
            {
                // Create the DateRange object.
                DateRange dateRange = new DateRange() { StartDate = startDate, EndDate = endDate };

                Dimension deviceCategoryDim = new Dimension
                {
                    Name = "ga:deviceCategory"
                };

                var metrics = GetGaMetrics();

                ReportRequest desktopReq = new ReportRequest
                {
                    ViewId = gaSetup.ViewID,
                    Metrics = metrics,
                    DateRanges = new List<DateRange> { dateRange },
                    Dimensions = new List<Dimension> { deviceCategoryDim },
                    FiltersExpression = "ga:deviceCategory==desktop"
                };

                ReportRequest mobileReq = new ReportRequest
                {
                    ViewId = gaSetup.ViewID,
                    Metrics = metrics,
                    DateRanges = new List<DateRange> { dateRange },
                    Dimensions = new List<Dimension> { deviceCategoryDim },
                    FiltersExpression = "ga:deviceCategory==mobile"
                };

                ReportRequest tabletReq = new ReportRequest
                {
                    ViewId = gaSetup.ViewID,
                    Metrics = metrics,
                    DateRanges = new List<DateRange> { dateRange },
                    Dimensions = new List<Dimension> { deviceCategoryDim },
                    FiltersExpression = "ga:deviceCategory==tablet"
                };


                var requests = new List<ReportRequest>();

                requests.Add(desktopReq);
                requests.Add(mobileReq);
                requests.Add(tabletReq);

                try
                {
                    // Call the batchGet method.
                    var reportResponse = GetGaReports(requests, gaSetup.GoogleAccountSetups.RefreshToken, gaSetup.GoogleAccountSetups.AccessToken);
                    if (reportResponse != null)
                    {
                        List<ReportRow> rowsDesktop = (List<ReportRow>)reportResponse.Reports[0].Data.Rows;
                        List<ReportRow> rowsMobile = (List<ReportRow>)reportResponse.Reports[1].Data.Rows;
                        List<ReportRow> rowsTablet = (List<ReportRow>)reportResponse.Reports[2].Data.Rows;

                        if (rowsDesktop != null && rowsDesktop.Count > 0)
                        {
                            deviceCategory.Desktop = new GoogleCustomDataDto();

                            deviceCategory.Desktop.Sessions = rowsDesktop[0].Metrics[0].Values[0];
                            deviceCategory.Desktop.Users = rowsDesktop[0].Metrics[0].Values[1];

                            deviceCategory.Desktop.Name = rowsDesktop[0].Dimensions[0];
                            deviceCategory.Desktop.PercentNewSessions = rowsDesktop[0].Metrics[0].Values[3];
                            deviceCategory.Desktop.BounceRate = rowsDesktop[0].Metrics[0].Values[4];
                            deviceCategory.Desktop.PageviewsPerSession = rowsDesktop[0].Metrics[0].Values[5];

                            deviceCategory.Desktop.AvgSessionDuration = rowsDesktop[0].Metrics[0].Values[6];
                            deviceCategory.Desktop.GoalCompletionsAll = rowsDesktop[0].Metrics[0].Values[7];
                            deviceCategory.Desktop.GoalConversionRateAll = rowsDesktop[0].Metrics[0].Values[8];

                        }


                        if (rowsMobile != null && rowsMobile.Count > 0)
                        {
                            deviceCategory.Mobile = new GoogleCustomDataDto();

                            deviceCategory.Mobile.Sessions = rowsMobile[0].Metrics[0].Values[0];
                            deviceCategory.Mobile.Users = rowsMobile[0].Metrics[0].Values[1];

                            deviceCategory.Mobile.Name = rowsMobile[0].Dimensions[0];
                            deviceCategory.Mobile.PercentNewSessions = rowsMobile[0].Metrics[0].Values[3];
                            deviceCategory.Mobile.BounceRate = rowsMobile[0].Metrics[0].Values[4];
                            deviceCategory.Mobile.PageviewsPerSession = rowsMobile[0].Metrics[0].Values[5];

                            deviceCategory.Mobile.AvgSessionDuration = rowsMobile[0].Metrics[0].Values[6];
                            deviceCategory.Mobile.GoalCompletionsAll = rowsMobile[0].Metrics[0].Values[7];
                            deviceCategory.Mobile.GoalConversionRateAll = rowsMobile[0].Metrics[0].Values[8];

                        }

                        if (rowsTablet != null && rowsTablet.Count > 0)
                        {
                            deviceCategory.Tablet = new GoogleCustomDataDto();

                            deviceCategory.Tablet.Sessions = rowsTablet[0].Metrics[0].Values[0];
                            deviceCategory.Tablet.Users = rowsTablet[0].Metrics[0].Values[1];

                            deviceCategory.Tablet.Name = rowsTablet[0].Dimensions[0];
                            deviceCategory.Tablet.PercentNewSessions = rowsTablet[0].Metrics[0].Values[3];
                            deviceCategory.Tablet.BounceRate = rowsTablet[0].Metrics[0].Values[4];
                            deviceCategory.Tablet.PageviewsPerSession = rowsTablet[0].Metrics[0].Values[5];

                            deviceCategory.Tablet.AvgSessionDuration = rowsTablet[0].Metrics[0].Values[6];
                            deviceCategory.Tablet.GoalCompletionsAll = rowsTablet[0].Metrics[0].Values[7];
                            deviceCategory.Tablet.GoalConversionRateAll = rowsTablet[0].Metrics[0].Values[8];

                        }

                        return deviceCategory;
                    }
                }
                catch (Exception ex)
                {
                    var exception = ex;
                }
            }

            return deviceCategory;
        }

        /// <summary>
        /// Get GeoLocation Reports
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <returns>status of the operation</returns>
        public GeoLocationDto GetGeoLocationReports(string campaignId, string startDate, string endDate)
        {
            GeoLocationDto geoLocationDto = new GeoLocationDto();

            var gaSetup = GetGaByCampaignId(campaignId);

            if (gaSetup != null)
            {
                // Create the DateRange object.
                DateRange dateRange = new DateRange() { StartDate = startDate, EndDate = endDate };

                Dimension location = new Dimension
                {
                    Name = "ga:country"
                };

                var metrics = GetGaMetrics();

                ReportRequest locationReq = new ReportRequest
                {
                    ViewId = gaSetup.ViewID,
                    Metrics = metrics,
                    DateRanges = new List<DateRange> { dateRange },
                    Dimensions = new List<Dimension> { location }

                };

                var requests = new List<ReportRequest>();

                requests.Add(locationReq);

                try
                {
                    // Call the batchGet method.
                    var reportResponse = GetGaReports(requests, gaSetup.GoogleAccountSetups.RefreshToken, gaSetup.GoogleAccountSetups.AccessToken);
                    if (reportResponse != null)
                    {
                        List<ReportRow> rowslocation = (List<ReportRow>)reportResponse.Reports[0].Data.Rows;

                        if (rowslocation != null && rowslocation.Count > 0)
                        {
                            geoLocationDto.GeoLocation = new List<GoogleCustomDataDto>();

                            foreach (var row in rowslocation)
                            {
                                GoogleCustomDataDto locationData = new GoogleCustomDataDto();

                                locationData.Name = row.Dimensions[0];
                                locationData.Sessions = row.Metrics[0].Values[0];
                                locationData.Users = row.Metrics[0].Values[1];

                                locationData.PercentNewSessions = row.Metrics[0].Values[3];
                                locationData.BounceRate = row.Metrics[0].Values[4];
                                locationData.PageviewsPerSession = row.Metrics[0].Values[5];

                                locationData.AvgSessionDuration = row.Metrics[0].Values[6];
                                locationData.GoalCompletionsAll = row.Metrics[0].Values[7];
                                locationData.GoalConversionRateAll = row.Metrics[0].Values[8];

                                geoLocationDto.GeoLocation.Add(locationData);
                            }
                        }
                        return geoLocationDto;
                    }
                }
                catch (Exception ex)
                {
                    var exception = ex;
                }
            }
            return geoLocationDto;
        }

        /// <summary>
        /// Get Language Reports
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <returns>status of the operation</returns>
        public LanguageDto GetLanguageReports(string campaignId, string startDate, string endDate)
        {
            LanguageDto languageDto = new LanguageDto();

            var gaSetup = GetGaByCampaignId(campaignId);

            if (gaSetup != null)
            {
                // Create the DateRange object.
                DateRange dateRange = new DateRange() { StartDate = startDate, EndDate = endDate };

                Dimension languageDim = new Dimension
                {
                    Name = "ga:language"
                };

                var metrics = GetGaMetrics();

                ReportRequest languageReq = new ReportRequest
                {
                    ViewId = gaSetup.ViewID,
                    Metrics = metrics,
                    DateRanges = new List<DateRange> { dateRange },
                    Dimensions = new List<Dimension> { languageDim }

                };

                var requests = new List<ReportRequest>();

                requests.Add(languageReq);

                try
                {
                    // Call the batchGet method.
                    var reportResponse = GetGaReports(requests, gaSetup.GoogleAccountSetups.RefreshToken, gaSetup.GoogleAccountSetups.AccessToken);
                    if (reportResponse != null)
                    {
                        List<ReportRow> rowsLanguage = (List<ReportRow>)reportResponse.Reports[0].Data.Rows;

                        if (rowsLanguage != null && rowsLanguage.Count > 0)
                        {
                            languageDto.Language = new List<GoogleCustomDataDto>();

                            foreach (var row in rowsLanguage)
                            {
                                GoogleCustomDataDto languageData = new GoogleCustomDataDto();

                                languageData.Name = row.Dimensions[0];
                                languageData.Sessions = row.Metrics[0].Values[0];
                                languageData.Users = row.Metrics[0].Values[1];
                                languageData.Users = row.Metrics[0].Values[2];

                                languageData.PercentNewSessions = row.Metrics[0].Values[3];
                                languageData.BounceRate = row.Metrics[0].Values[4];
                                languageData.PageviewsPerSession = row.Metrics[0].Values[5];

                                languageData.AvgSessionDuration = row.Metrics[0].Values[6];
                                languageData.GoalCompletionsAll = row.Metrics[0].Values[7];
                                languageData.GoalConversionRateAll = row.Metrics[0].Values[8];

                                languageDto.Language.Add(languageData);
                            }
                        }
                        return languageDto;
                    }
                }
                catch (Exception ex)
                {
                    var exception = ex;
                }
            }
            return languageDto;
        }

        /// <summary>
        /// InActive All Ga Analytics While Select New 
        /// </summary>
        /// <param name="id">Google analytics Account Id</param>
        /// <returns>Int</returns>
        public async Task<int> InActiveAllGaAnalytics(Guid id)
        {
            var campaign = _googleanalyticsaccountRepository.GetAllEntities().Where(x => x.Id == id).FirstOrDefault();
            var campaignId = campaign.CampaignID.ToString();
            return await _googleanalyticsaccountRepository.UpdateBulkEntityAsync(y => new GoogleAnalyticsAccount { Active = false }, x => x.CampaignID == campaignId);

        }

        public GoogleTokenResponse GenerateToken()
        {
            var loggedInUser = _userInfoService;

            // Use configuration values instead of hardcoded credentials
            string _clientId = _configuration["ClientIdForGoogleAds"];
            string _clientSecret = _configuration["ClientSecretForGoogleAds"];
            string _callbackUrl = loggedInUser.CustomDomain + "home/campaign";
            string _accessToken;
            string _refreshToken;
            string _authorizeCode = "";
            DateTime _tokenExpiry;
            _accessToken = null;
            _refreshToken = null;
            var request = new RestRequest("/token", Method.Post);
            request.AddParameter("client_id", _clientId, ParameterType.GetOrPost);
            request.AddParameter("client_secret", _clientSecret, ParameterType.GetOrPost);

            request.AddParameter("redirect_uri", _callbackUrl, ParameterType.GetOrPost);
            if (string.IsNullOrEmpty(_refreshToken))
            {
                request.AddParameter("grant_type", "authorization_code", ParameterType.GetOrPost);
                request.AddParameter("code", _authorizeCode, ParameterType.GetOrPost);
            }
            else
            {
                request.AddParameter("grant_type", "refresh_token", ParameterType.GetOrPost);
                request.AddParameter("refresh_token", _refreshToken, ParameterType.GetOrPost);
            }

            var client = new RestClient("https://oauth2.googleapis.com");
            var response = client.ExecuteAsync<GoogleTokenResponse>(request).Result;
            if (response.StatusCode != HttpStatusCode.OK)
                throw new AuthenticationException("Error Getting Request Bearer Token");

            _accessToken = response.Data.AccessToken;
            _refreshToken = response.Data.RefreshToken;
            _tokenExpiry = DateTime.UtcNow.AddSeconds(response.Data.ExpiresIn - 20);
            return response.Data;



        }
        public GoogleTokenResponse Callback(string code)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            string _authorizeCode = code;
            string realmId = "";
            TokenGoogleUtils tu = new TokenGoogleUtils();
            GoogleTokenResponse trs = GenerateToken();
            // ViewBag.Error = Request.QueryString["error"] ?? "none";

            //  _qb_AuthService.SetAccessToken(trs.AccessToken, realmId);
            tu.AccessToken = trs.AccessToken;
            tu.RealmId = realmId;
            tu.RefreshToken = trs.RefreshToken;
            //TokenQBUtils.RefreshTokenExpiry = trs.RefreshTokenExpiresIn;
            tu.AccessTokenExpiry = trs.ExpiresIn;
            return trs;
        }
        public class GoogleUserOutputData
        {
            public string id { get; set; }
            public string name { get; set; }
            public string given_name { get; set; }
            public string email { get; set; }
            public string picture { get; set; }
        }
        /// <summary>
        /// Setup GoogleAnalytics Account
        /// </summary>
        /// <param name="id">Campaign Id</param>
        /// <returns>true or false</returns>
        public async Task<bool> SetupGoogleAnalyticsAccount(string id, Guid CompanyId)
        {
            UserCredential credential = null;
            var gaAccountList = new List<GoogleAnalyticsAccountDto>();
            GoogleAccountSetupForCreation googleAccountSetup = new GoogleAccountSetupForCreation();

            try
            {
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
                {
                    ClientId = _configuration["ClientIdForGoogleAds"],
                    ClientSecret = _configuration["ClientSecretForGoogleAds"]
                }, new[] { AnalyticsReportingService.Scope.AnalyticsReadonly,//for analytics
                    AnalyticsReportingService.Scope.Analytics,//for analytics
                    WebmastersService.Scope.WebmastersReadonly,//for GSC
                    WebmastersService.Scope.Webmasters, //for GSC
                    PagespeedInsightsService.Scope.Openid//for page speed
                },
            "user",
            CancellationToken.None, new NullDataStore()).Result;

                // Create the service
                AnalyticsService analyticsService = new AnalyticsService(
                    new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential
                    }
                );

                // some calls to Google API
                var act1 = analyticsService.Management.Accounts.List().Execute();

                var actSum = analyticsService.Management.AccountSummaries.List().Execute();

                var isExistsGa = await _googleaccountsetupService.ExistAsync(x => x.UserName == act1.Username && x.CompanyId == CompanyId);




                if (!isExistsGa)
                {
                    googleAccountSetup.AccessToken = credential.Token.AccessToken;
                    googleAccountSetup.RefreshToken = credential.Token.RefreshToken;
                    googleAccountSetup.UserName = act1.Username;
                    googleAccountSetup.UserId = credential.UserId;
                    googleAccountSetup.IsAuthorize = true;
                    googleAccountSetup.CompanyId = CompanyId;
                    //create a show in db.
                    var googleanalyticsaccountToReturn = await _googleaccountsetupService.CreateEntityAsync<GoogleAccountSetupDto, GoogleAccountSetupForCreation>(googleAccountSetup);

                    foreach (var account in actSum.Items)
                    {
                        GoogleAnalyticsAccountForCreation googleAnalyticsAccount = new GoogleAnalyticsAccountForCreation();
                        googleAnalyticsAccount.CampaignID = id;
                        googleAnalyticsAccount.AccountID = account.Id;
                        googleAnalyticsAccount.AccountName = account.Name;
                        googleAnalyticsAccount.GoogleAccountSetupID = googleanalyticsaccountToReturn.Id;
                        googleAnalyticsAccount.Active = false;

                        if (account.WebProperties.Count > 0)
                        {
                            googleAnalyticsAccount.PropertyID = account.WebProperties[0].Id;

                            googleAnalyticsAccount.ViewID = account.WebProperties[0].Profiles[0].Id;
                            googleAnalyticsAccount.ViewName = account.WebProperties[0].Profiles[0].Name;

                            googleAnalyticsAccount.WebsiteUrl = account.WebProperties[0].WebsiteUrl;
                            var ga = await CreateEntityAsync<GoogleAnalyticsAccountDto, GoogleAnalyticsAccountForCreation>(googleAnalyticsAccount);
                            gaAccountList.Add(ga);
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
        public async Task<Dictionary<string, string>> SetupGoogleAnalyticsAccountWithJson(string id, string CompanyId)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            UserCredential credential = null;
            var gaAccountList = new List<GoogleAnalyticsAccountDto>();
            GoogleAccountSetupForCreation googleAccountSetup = new GoogleAccountSetupForCreation();

            try
            {
                string[] scopes = new string[] {
                     AnalyticsReportingService.Scope.AnalyticsReadonly,
                    AnalyticsReportingService.Scope.Analytics
					//WebmastersService.Scope.WebmastersReadonly,
					//WebmastersService.Scope.Webmasters
				};
                //using (var stream = new FileStream("D:\\home\\site\\wwwroot\\client_secret.json", FileMode.Open, FileAccess.Read)) {
                using (var stream = new FileStream("D:\\client_secret.json", FileMode.Open, FileAccess.Read))
                {
                    string credPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                    credPath = Path.Combine(credPath, ".credentials/", System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);

                    // Requesting Authentication or loading previously stored authentication for userName
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.Load(stream).Secrets,
                        scopes,
                       "user",
                       CancellationToken.None,
                       new FileDataStore(credPath, true)).Result;
                }
                dict.Add("accessToken", credential.Token.AccessToken);
                dict.Add("RefreshToken", credential.Token.RefreshToken);
                // some calls to Google API
            }
            catch (Exception ex)
            {
                dict.Add("error", ex.Message);
                return dict;
            }
            return dict;
        }
        public async Task<Dictionary<string, string>> SetupGoogleAnalyticsAccountNew(string id, Guid CompanyId)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            UserCredential credential = null;
            var gaAccountList = new List<GoogleAnalyticsAccountDto>();
            GoogleAccountSetupForCreation googleAccountSetup = new GoogleAccountSetupForCreation();

            try
            {
                string[] scopes = new string[] {

                    "openid email profile",
                     AnalyticsReportingService.Scope.AnalyticsReadonly,
                    AnalyticsReportingService.Scope.Analytics,
                    WebmastersService.Scope.WebmastersReadonly,
                    WebmastersService.Scope.Webmasters
                };

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
                {
                    ClientId = _configuration["ClientIdForGoogleAds"],
                    ClientSecret = _configuration["ClientSecretForGoogleAds"]
                }, new[] { "openid email profile",
                     AnalyticsReportingService.Scope.AnalyticsReadonly,
                    AnalyticsReportingService.Scope.Analytics,
                    WebmastersService.Scope.WebmastersReadonly,
                    WebmastersService.Scope.Webmasters },
                "user",
                CancellationToken.None, new NullDataStore()).Result;

                // Create the service
                AnalyticsService analyticsService = new AnalyticsService(
                    new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential
                    }
                );
                dict.Add("accessToken", credential.Token.AccessToken);
                dict.Add("RefreshToken", credential.Token.RefreshToken);
                // some calls to Google API
            }
            catch (Exception ex)
            {
                dict.Add("error", ex.Message);
                return dict;
            }
            return dict;
        }
        public async Task<Dictionary<string, string>> RefreshGoogleAccount(string refreshToken, string accessToken)
        {
            UserCredential customCredential = null;
            Dictionary<string, string> dict = new Dictionary<string, string>();
            string[] scopes = new string[] {
                     AnalyticsReportingService.Scope.AnalyticsReadonly,
                    AnalyticsReportingService.Scope.Analytics,
                    WebmastersService.Scope.WebmastersReadonly,
                    WebmastersService.Scope.Webmasters
                };


            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _configuration["ClientIdForGoogleAds"],
                    ClientSecret = _configuration["ClientSecretForGoogleAds"]
                },
                Scopes = scopes
            });

            var token = new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

            customCredential = new UserCredential(flow, "user", token);

            var reportService = new AnalyticsReportingService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = customCredential,
                ApplicationName = "Analytics Reporting"
            });

            dict.Add("accessToken", customCredential.Token.AccessToken);
            dict.Add("RefreshToken", customCredential.Token.RefreshToken);

            return dict;
        }


        public string SetupGoogleAdsAccount()
        {

            try
            {

                AdWordsUser user = new AdWordsUser();

                UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
                {
                    ClientId = _configuration["ClientIdForGoogleAds"],
                    ClientSecret = _configuration["ClientSecretForGoogleAds"]
                }, new[] { "https://www.googleapis.com/auth/adwords" },
                "user",
                CancellationToken.None, new NullDataStore()).Result;

                // Store this token for future use.
                string refreshToken = credential.Token.RefreshToken;

                //credential.RevokeTokenAsync(new CancellationToken());

                //AdWordsAppConfig config = user.Config as AdWordsAppConfig;

                //config.OAuth2RefreshToken = refreshToken;

                // Removed hardcoded developer token

                //config1.OAuth2AccessToken = credential.Token.AccessToken;

                GoogleAdsConfig config1 = new GoogleAdsConfig();
                config1.OAuth2ClientId = _configuration["ClientIdForGoogleAds"];
                config1.OAuth2ClientSecret = _configuration["ClientSecretForGoogleAds"];
                config1.OAuth2Scope = "https://www.googleapis.com/auth/adwords";
                config1.DeveloperToken = _configuration["GoogleAdsDeveloperToken"];
                //config1.LoginCustomerId = "9351615649"; // Test account id
                //config1.LoginCustomerId = "9351615649";
                config1.OAuth2RefreshToken = refreshToken;
                GoogleAdsClient client = new GoogleAdsClient(config1);


                GoogleAdsServiceClient googleAdsServiceClient =
               client.GetService(Services.V15.GoogleAdsService);

                CustomerServiceClient customerServiceClient =
                    client.GetService(Services.V15.CustomerService);

                string[] customerResourceNames = customerServiceClient.ListAccessibleCustomers();

                var total = customerResourceNames.Length;
                var limit = 0;

                do
                {
                    try
                    {
                        foreach (string customerResourceName in customerResourceNames)
                        {
                            // List of Customer IDs to handle.
                            List<long> seedCustomerIds = new List<long>();
                            //long? managerCustomerId = 2400772741;
                            long? managerCustomerId = null;
                            CustomerName customerName = CustomerName.Parse(customerResourceName);
                            GoogleAdsConfig config2 = new GoogleAdsConfig();
                            config2.OAuth2ClientId = _configuration["ClientIdForGoogleAds"];
                            config2.OAuth2ClientSecret = _configuration["ClientSecretForGoogleAds"];
                            config2.OAuth2Scope = "https://www.googleapis.com/auth/adwords";
                            config2.DeveloperToken = _configuration["GoogleAdsDeveloperToken"];
                            //config2.LoginCustomerId = "2400772741"; // Test account id
                            config2.LoginCustomerId = customerName.CustomerId;
                            config2.OAuth2RefreshToken = refreshToken;
                            GoogleAdsClient client1 = new GoogleAdsClient(config2);


                            GoogleAdsServiceClient googleAdsServiceClient1 =
                           client1.GetService(Services.V15.GoogleAdsService);

                            CustomerServiceClient customerServiceClient1 =
                                client1.GetService(Services.V15.CustomerService);

                            //string[] customerResourceNames1 = customerServiceClient.ListAccessibleCustomers();
                            foreach (string customerResourceName1 in customerResourceNames)
                            {
                                CustomerName customerName1 = CustomerName.Parse(customerResourceName1);

                                seedCustomerIds.Add(long.Parse(customerName1.CustomerId));
                            }

                            //Get hierarchy account

                            // If a Manager ID was provided in the customerId parameter, it will be the only ID
                            // in the list. Otherwise, we will issue a request for all customers accessible by
                            // this authenticated Google account.


                            // Create a query that retrieves all child accounts of the manager specified in
                            // search calls below.
                            const string query = @"SELECT
                                    customer_client.client_customer,
                                    customer_client.level,
                                    customer_client.manager,
                                    customer_client.descriptive_name,
                                    customer_client.currency_code,
                                    customer_client.time_zone,
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
                                            }

                                            continue;
                                        }

                                        // For all level-1 (direct child) accounts that are a manager account,
                                        // the above query will be run against them to create a Dictionary of
                                        // managers mapped to their child accounts for printing the hierarchy
                                        // afterwards.
                                        if (!customerIdsToChildAccounts.ContainsKey(managerCustomerId.Value))
                                            customerIdsToChildAccounts.Add(managerCustomerId.Value,
                                                new List<CustomerClient>());

                                        customerIdsToChildAccounts[managerCustomerId.Value].Add(customerClient);

                                        if (customerClient.Manager)
                                            // A customer can be managed by multiple managers, so to prevent
                                            // visiting the same customer many times, we need to check if it's
                                            // already in the Dictionary.
                                            if (!customerIdsToChildAccounts.ContainsKey(customerClient.Id) &&
                                                customerClient.Level == 1)
                                                unprocessedCustomerIds.Enqueue(customerClient.Id);
                                    }
                                }

                                if (rootCustomerClient != null)
                                {
                                    Console.WriteLine("The hierarchy of customer ID {0} is printed below:",
                                        rootCustomerClient.Id);
                                    //PrintAccountHierarchy(rootCustomerClient, customerIdsToChildAccounts, 0);
                                    Console.WriteLine();
                                }
                                else
                                {
                                    Console.WriteLine(
                                        "Customer ID {0} is likely a test account, so its customer client " +
                                        " information cannot be retrieved.", managerCustomerId);
                                }
                            }

                            //// Display the result.
                            //foreach (string customerResourceName in customerResourceNames)
                            //{
                            //	Console.WriteLine(
                            //		$"Found customer with resource name = '{customerResourceName}'.");
                            //}

                            // Get the GoogleAdsService.
                            //GoogleAdsClient client2 = new GoogleAdsClient(config1);
                            //GoogleAdsServiceClient googleAdsService = client2.GetService(
                            //	Services.V8.GoogleAdsService);

                            //// Create the query.
                            //string query1 =
                            //	  @"SELECT
                            //metrics.impressions, 
                            //metrics.clicks 
                            //FROM keyword_view WHERE segments.date DURING LAST_30_DAYS";


                            //var customerId = "YOUR_CUSTOMER_ID"; // Replace with your Google Ads customer ID
                            //							   // Issue a search request.
                            //googleAdsService.SearchStream(customerId, query1, delegate (SearchGoogleAdsStreamResponse resp)
                            //{
                            //	// Display the results.
                            //	foreach (GoogleAdsRow criterionRow in resp.Results)
                            //	{
                            //		//Console.WriteLine(
                            //		//	"Keyword with text " +
                            //		//	$"'{criterionRow.AdGroupCriterion.Keyword.Text}', match type " +
                            //		//	$"'{criterionRow.AdGroupCriterion.Keyword.MatchType}' and ID " +
                            //		//	$"{criterionRow.AdGroupCriterion.CriterionId} in ad group " +
                            //		//	$"'{criterionRow.AdGroup.Name}' with ID " +
                            //		//	$"{criterionRow.AdGroup.Id} in campaign " +
                            //		//	$"'{criterionRow.Campaign.Name}' with ID " +
                            //		//	$"{criterionRow.Campaign.Id} had " +
                            //		//	$"{criterionRow.Metrics.Impressions.ToString()} impressions, " +
                            //		//	$"{criterionRow.Metrics.Clicks} clicks, and " +
                            //		//	$"{criterionRow.Metrics.CostMicros} cost (in micros) during the " +
                            //		//	"last 7 days.");
                            //	}
                            //}
                            //);
                        }
                    }
                    catch (GoogleAdsException ex)
                    {
                        if (customerResourceNames.Length > 0)
                        {
                            customerResourceNames = customerResourceNames.Skip(1).ToArray();
                        }
                        limit++;
                    }

                } while (total > limit);

            }
            catch (GoogleAdsException ex)
            {
                throw ex;
            }
            return "";
        }



        private class ManagedCustomerTreeNode
        {
            /// <summary>
            /// The parent node.
            /// </summary>
            private ManagedCustomerTreeNode parentNode;

            /// <summary>
            /// The account associated with this node.
            /// </summary>
            private Google.Api.Ads.AdWords.v201809.ManagedCustomer account;

            /// <summary>
            /// The list of child accounts.
            /// </summary>
            private List<ManagedCustomerTreeNode> childAccounts =
                new List<ManagedCustomerTreeNode>();

            /// <summary>
            /// Gets or sets the parent node.
            /// </summary>
            public ManagedCustomerTreeNode ParentNode
            {
                get { return parentNode; }
                set { parentNode = value; }
            }

            /// <summary>
            /// Gets or sets the account.
            /// </summary>
            public Google.Api.Ads.AdWords.v201809.ManagedCustomer Account
            {
                get { return account; }
                set { account = value; }
            }

            /// <summary>
            /// Gets the child accounts.
            /// </summary>
            public List<ManagedCustomerTreeNode> ChildAccounts
            {
                get { return childAccounts; }
            }

            /// <summary>
            /// Returns a <see cref="System.String"/> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String"/> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                return string.Format("{0}, {1}", account.customerId, account.name);
            }

            /// <summary>
            /// Returns a string representation of the current level of the tree and
            /// recursively returns the string representation of the levels below it.
            /// </summary>
            /// <param name="depth">The depth of the node.</param>
            /// <param name="sb">The String Builder containing the tree
            /// representation.</param>
            /// <returns>The tree string representation.</returns>
            public StringBuilder ToTreeString(int depth, StringBuilder sb)
            {
                sb.Append('-', depth * 2);
                sb.Append(this);
                sb.AppendLine();
                foreach (ManagedCustomerTreeNode childAccount in childAccounts)
                {
                    childAccount.ToTreeString(depth + 1, sb);
                }

                return sb;
            }
        }
        public string PrepareGoogleAdsToken(string code)
        {
            var loggedInUser = _userInfoService;

            AdWordsUser user = new AdWordsUser();
            //UserCredential credential = null;


            // Removed hardcoded credentials from commented code

            user.Config.OAuth2RedirectUri = _configuration["HostedUrl"] + "api/googleanalyticsaccounts/GoogleAdsCallback";

            //AdsOAuthProviderForApplications oAuth2Provider =
            //    (user.OAuthProvider as AdsOAuthProviderForApplications);

            //oAuth2Provider.RefreshAccessTokenInOfflineMode();
            //oAuth2Provider.FetchAccessAndRefreshTokens(code);





            // This code example shows how to run an AdWords API web application
            // while incorporating the OAuth2 installed application flow into your
            // application. If your application uses a single AdWords manager account
            // login to make calls to all your accounts, you shouldn't use this code
            // example. Instead, you should run OAuthTokenGenerator.exe to generate a
            // refresh token and use that configuration in your application's
            // App.config.
            AdWordsAppConfig config = user.Config as AdWordsAppConfig;
            config.DeveloperToken = _configuration["GoogleAdsDeveloperToken"];
            config.ClientCustomerId = _configuration["GoogleAdsClientCustomerId"];
            //config.AuthorizationMethod = AdWordsAuthorizationMethod.OAuth2;
            //config.OAuth2Mode = OAuth2Flow.APPLICATION;

            Google.Api.Ads.AdWords.v201809.Selector selector = new Google.Api.Ads.AdWords.v201809.Selector()
            {
                fields = new string[]
               {
                     Google.Api.Ads.AdWords.v201809.Campaign.Fields.Id,
                    Google.Api.Ads.AdWords.v201809.Campaign.Fields.Name,
                    Google.Api.Ads.AdWords.v201809.Campaign.Fields.Status
               },

                paging = Google.Api.Ads.AdWords.v201809.Paging.Default
            };

            // Removed hardcoded credentials from commented code

            //Google.Api.Ads.AdWords.v201809.CampaignService campaignSercive = new Google.Api.Ads.AdWords.v201809.CampaignService(
            //        new BaseClientService.Initializer()
            //        {
            //            HttpClientInitializer = credential
            //        }
            //    );




            // Get the CampaignService.

            using (Google.Api.Ads.AdWords.v201809.CampaignService campaignService =
                (Google.Api.Ads.AdWords.v201809.CampaignService)user.GetService(AdWordsService.v201809.CampaignService))
            {

                var tests = campaignService.get(selector);
                //  oAuth2Provider.RevokeRefreshToken();

            }

            var redirectUrl = loggedInUser.CustomDomain + "integrations";

            return redirectUrl;

        }

        public string DoAuth2Authorization(AdWordsUser user)
        {
            try
            {

                user.Config.OAuth2RedirectUri = _configuration["HostedUrl"] + "api/googleanalyticsaccounts/GoogleAdsCallback";
                AdsOAuthProviderForApplications oAuth2Provider =
                    (user.OAuthProvider as AdsOAuthProviderForApplications);

                // Get the authorization url.
                string authorizationUrl = oAuth2Provider.GetAuthorizationUrl();


                return authorizationUrl;

            }
            catch (Exception e)
            {
                var error = e;
            }

            return "";
            // Since we are using a console application, set the callback url to null.

        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override Dictionary<string, PropertyMappingValue> GetPropertyMapping()
        {
            return new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "Id", new PropertyMappingValue(new List<string>() { "Id" } ) },
                        { "Name", new PropertyMappingValue(new List<string>() { "Name" } )},
                    };
        }

        public override string GetDefaultOrderByColumn()
        {
            return "UpdatedOn";
        }

        public override string GetDefaultFieldsToSelect()
        {
            return "Id,Name";
        }

        #endregion
    }

}
