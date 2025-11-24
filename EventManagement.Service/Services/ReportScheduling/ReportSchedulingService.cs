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
using EventManagement.Utility.Enums;
using System.Net.Http;
using System.Text;
using System.Net.Http.Headers;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Analytics.v3;
using Google.Apis.Services;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Api2Pdf;
using AutoMapper;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Text.RegularExpressions;
using System.Globalization;
using Newtonsoft.Json.Linq;
using CampaignFacebookAds = EventManagement.Domain.Entities.CampaignFacebookAds;
using IdentityServer4.Extensions;
using RestSharp;
using Method = RestSharp.Method;
using System.Net;
using DateTime = System.DateTime;
using Decimal = System.Decimal;
using Google.Apis.Analytics.v3.Data;
using static EventManagement.Dto.CampaignCallRailDto;
using EventManagement.Domain.Migrations;

namespace EventManagement.Service
{
    public class ReportSchedulingService : ServiceBase<ReportScheduling, Guid>, IReportSchedulingService
    {

        #region PRIVATE MEMBERS

        private readonly IReportSchedulingRepository _reportschedulingRepository;
        private readonly IConfiguration _configuration;
        private readonly ICampaignGoogleAnalyticsRepository _campaignGoogleAnalyticsRepository;
        private readonly IGoogleAnalyticsAccountService _googleAnalyticsAccountService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ICampaignLinkedinService _campaignLinkedinService;
        private readonly IEmailWhitelabelRepository _emailWhitelabelRepository;
        private readonly ICampaignGSCRepository _campaignGSCRepository;
        private readonly ICampaignFacebookAdsRepository _campaignFacebookAdsRepository;
        private readonly ICampaignGoogleAdsService _campaignGoogleAdsService;
        private readonly ICampaignGoogleAdsRepository _campaignGoogleAdsRepository;
        private readonly ICampaignInstagramRepository _campaignInstagramRepository;
        private readonly ISerpRepository _serpRepository;
        private readonly ICampaignFacebookService _campaignFacebookService;
        private readonly ICompanyService _companyService;
        private readonly ICompanyRepository _companyRepository;
        private readonly ICampaignRepository _campaignRepository;
        private readonly ILinkedinAdService _linkedinAdService;
        private readonly ICampaignGBPRepository _campaignGBPRepository;
        private readonly ICampaignGBPService _campaignGBPService;
        private readonly ICampaignWooCommerceRepository _campaignWooCommerceRepository;
        private readonly ICampaignWooCommerceService _campaignWooCommerceService;
        private readonly ICampaignGoogleSheetService _campaignGoogleSheetService;
        private readonly ICampaignCallRailRepository _campaignCallRailRepository;
        private readonly ICampaignCallRailService _campaignCallRailService;
        private readonly ICampaignMailchimpRepository _campaignMailchimpRepository;
        private readonly ICampaignMailchimpService _campaignMailchimpService;
        private readonly ICampaignMicrosoftAdRepository _campaignMicrosoftAdRepository;
        private readonly ICampaignMicrosoftAdService _campaignMicrosoftAdService;
        private readonly IChatGptReportService _chatGptReportService;


        #endregion


        #region CONSTRUCTOR

        public ReportSchedulingService(ICampaignFacebookAdsRepository campaignFacebookAdsRepository,
            ICampaignInstagramRepository campaignInstagramRepository, ICampaignGoogleAdsRepository campaignGoogleAdsRepository,
            ICampaignGoogleAdsService campaignGoogleAdsService, ICampaignLinkedinService campaignLinkedinService,
            IReportSchedulingRepository reportschedulingRepository, ILogger<ReportSchedulingService> logger, IConfiguration configuration,
            ICampaignGoogleAnalyticsRepository campaignGoogleAnalyticsRepository, IGoogleAnalyticsAccountService googleAnalyticsAccountService,
            IHostingEnvironment hostingEnvironment, IEmailWhitelabelRepository emailWhitelabelRepository, ICampaignGSCRepository campaignGSCRepository,
            ISerpRepository serpRepository, ICampaignFacebookService campaignFacebookService, ICompanyService companyService,
            ICompanyRepository companyRepository, ICampaignRepository campaignRepository,
            ILinkedinAdService linkedinAdService, ICampaignGBPRepository campaignGBPRepository,
            ICampaignGBPService campaignGBPService,
            ICampaignWooCommerceRepository campaignWooCommerceRepository,
            ICampaignWooCommerceService campaignWooCommerceService,
            ICampaignGoogleSheetService campaignGoogleSheetService,
            ICampaignCallRailRepository campaignCallRailRepository,
            ICampaignCallRailService campaignCallRailService,
            ICampaignMailchimpRepository campaignMailchimpRepository,
            ICampaignMailchimpService campaignMailchimpService,
            ICampaignMicrosoftAdRepository campaignMicrosoftAdRepository,
            ICampaignMicrosoftAdService campaignMicrosoftAdService,
            IChatGptReportService chatGptReportService

            ) : base(reportschedulingRepository, logger)
        {
            _reportschedulingRepository = reportschedulingRepository;
            _configuration = configuration;
            _campaignGoogleAnalyticsRepository = campaignGoogleAnalyticsRepository;
            _googleAnalyticsAccountService = googleAnalyticsAccountService;
            _hostingEnvironment = hostingEnvironment;
            _campaignLinkedinService = campaignLinkedinService;
            _emailWhitelabelRepository = emailWhitelabelRepository;
            _campaignGSCRepository = campaignGSCRepository;
            _campaignFacebookAdsRepository = campaignFacebookAdsRepository;
            _campaignGoogleAdsService = campaignGoogleAdsService;
            _campaignGoogleAdsRepository = campaignGoogleAdsRepository;
            _campaignInstagramRepository = campaignInstagramRepository;
            _serpRepository = serpRepository;
            _campaignFacebookService = campaignFacebookService;
            _companyService = companyService;
            _companyRepository = companyRepository;
            _campaignRepository = campaignRepository;
            _linkedinAdService = linkedinAdService;
            _campaignGBPRepository = campaignGBPRepository;
            _campaignGBPService = campaignGBPService;
            _campaignWooCommerceRepository = campaignWooCommerceRepository;
            _campaignWooCommerceService = campaignWooCommerceService;
            _campaignGoogleSheetService = campaignGoogleSheetService;
            _campaignCallRailRepository = campaignCallRailRepository;
            _campaignCallRailService = campaignCallRailService;
            _campaignMailchimpRepository = campaignMailchimpRepository;
            _campaignMailchimpService = campaignMailchimpService;
            _campaignMicrosoftAdRepository = campaignMicrosoftAdRepository;
            _campaignMicrosoftAdService = campaignMicrosoftAdService;
            _chatGptReportService = chatGptReportService;
        }

        #endregion


        #region PUBLIC MEMBERS   

        /// <summary>
        /// Schedule report and send email called from webjob
        /// </summary>
        /// <returns>true or false</returns>
        public async Task<bool> EmailReportSchedule()
        {
            var retval = false;
            DateTime todayDate = DateTime.UtcNow;

            DateTime endDateForEmailReport = new DateTime();
            DateTime startDateForEmailReport = new DateTime();
            DateTime prevDateForEmailReport = new DateTime();
            var ReportSchedulingNotificationList = GetReportScheduleNotificationList();

            var a2pClient = new Api2Pdf.Api2Pdf(_configuration["Api2Pdf"]); 

            foreach (var ReportSchedule in ReportSchedulingNotificationList)
            {
                //today
                if (ReportSchedule.ReportSetting.Frequency == "0")
                {
                    endDateForEmailReport = todayDate.Date;
                    startDateForEmailReport = todayDate.Date;
                }
                //7 days
                else if (ReportSchedule.ReportSetting.Frequency == "7")
                {
                    endDateForEmailReport = todayDate.Date.AddDays(-1);
                    startDateForEmailReport = todayDate.AddDays(-7).Date;
                }
                //30 days
                else if (ReportSchedule.ReportSetting.Frequency == "30")
                {
                    endDateForEmailReport = todayDate.Date.AddDays(-1);
                    startDateForEmailReport = todayDate.AddDays(-30).Date;
                }
                //60 days
                else if (ReportSchedule.ReportSetting.Frequency == "60")
                {
                    endDateForEmailReport = todayDate.Date.AddDays(-1);
                    startDateForEmailReport = todayDate.AddDays(-60).Date;
                }
                //Last Month
                else if (ReportSchedule.ReportSetting.Frequency == "1")
                {
                    var month = new DateTime(todayDate.Year, todayDate.Month, 01);
                    endDateForEmailReport = month.AddDays(-1);
                    startDateForEmailReport = month.AddMonths(-1);
                }
                //last 3 months
                else if (ReportSchedule.ReportSetting.Frequency == "90")
                {
                    var month = new DateTime(todayDate.Year, todayDate.Month, 01);
                    endDateForEmailReport = month.AddDays(-1);
                    startDateForEmailReport = month.AddMonths(-3);
                }
                //last year
                else if (ReportSchedule.ReportSetting.Frequency == "4")
                {
                    prevDateForEmailReport = todayDate.AddYears(-1);
                    startDateForEmailReport = new DateTime(prevDateForEmailReport.Year, 01, 01);
                    endDateForEmailReport = new DateTime(prevDateForEmailReport.Year, 12, 31);
                }
                //This month 
                else if (ReportSchedule.ReportSetting.Frequency == "2")
                {
                    startDateForEmailReport = new DateTime(todayDate.Year, todayDate.Month, 01);
                    endDateForEmailReport = todayDate.Date;
                }
                //This year 
                else if (ReportSchedule.ReportSetting.Frequency == "3")
                {
                    startDateForEmailReport = new DateTime(todayDate.Year, 01, 01);
                    endDateForEmailReport = todayDate.Date;
                }
                //Custom Range 
                else if (ReportSchedule.ReportSetting.Frequency == "5")
                {
                    startDateForEmailReport = ReportSchedule.ReportSetting.StartDate;
                    endDateForEmailReport = ReportSchedule.ReportSetting.EndDate;
                }

                var startDate1 = startDateForEmailReport.ToString("ddd, MMMM dd yyyy");
                var endDate1 = endDateForEmailReport.ToString("ddd, MMMM dd yyyy");

                var previousDate = CalculatePreviousStartDateAndEndDate(startDateForEmailReport, endDateForEmailReport);

                var campaignId = ReportSchedule.ReportSetting.CampaignId.Value;

                var companyId = ReportSchedule.ReportSetting.CompanyId;

                var reportTypeList = (ReportSchedule.ReportSetting.ReportType ?? string.Empty)
                .Split(',')
                .Where(type => !string.IsNullOrEmpty(type))
                .ToList();

                //var reportTypeList = new List<string>(){ "16","17","14(24)", "14(25)", "14(26)", "14(27)", "14(28)", "14(29)", "8","116" };

                var AnalyticsOrganicTrafficReport = String.Empty;
                var googleSearchConsoleReport = String.Empty;
                var googleAdsReport = String.Empty;
                var linkedInReport = String.Empty;
                var instagramReport = String.Empty;
                var facebookAdsReport = String.Empty;
                var facebookReportHtml = String.Empty;
                var isCoverPageExist = ReportSchedule.ReportSetting.IsCoverPage;
                var isTocExist = ReportSchedule.ReportSetting.TableOfContent;
                int pageNumber = 0;
                int cmtIndex = 0;
                int imgIndex = 0;

                var companyName = _companyRepository.GetAllEntities(true).Where(x => x.Id == companyId).Select(x => x.Name).ToList();
                var campaignName = _campaignRepository.GetAllEntities(true).Where(x => x.Id == campaignId).Select(x => x.Name).ToList();

                List<string> htmlArray = new List<string>();

                List<Dictionary<string, string>> htmlArrayForIntegrations = new List<Dictionary<string, string>>();

                List<Dictionary<string, string>> listOfResult = new List<Dictionary<string, string>>();

                List<string> listOfRawData = new List<string>();

                //add head and style in list of html
                string configHtmlString = string.Empty;

                //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/config.html");
                //configHtmlString = System.IO.File.ReadAllText(pathGa);

                string pathConfig = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/config.html");
                using (HttpClient httpclient = new HttpClient())
                {
                    configHtmlString = httpclient.GetStringAsync(pathConfig).Result;
                }

                htmlArray.Add(configHtmlString);

                if (isCoverPageExist)
                {

                    string htmlString2 = "";

                    //string path = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/coverPAge.html");
                    //htmlString2 =  System.IO.File.ReadAllText(path);


                    string pathCover = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/coverPAge.html");
                    using (HttpClient httpclient = new HttpClient())
                    {
                        htmlString2 = httpclient.GetStringAsync(pathCover).Result;
                    }

                    var reportName = ReportSchedule.ReportSetting.Name;
                    dynamic data = JObject.Parse(ReportSchedule.ReportSetting.HeaderSettings);
                    string companyLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImg).Value.ToString();
                    string campaignLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImgCamp).Value.ToString();
                    string coverPageTextColor = ((Newtonsoft.Json.Linq.JValue)data._coverPageTextColor).Value.ToString();
                    string coverPageBgColor = ((Newtonsoft.Json.Linq.JValue)data._coverPageBgColor).Value.ToString();
                    string coverPageBgImage = ((Newtonsoft.Json.Linq.JValue)data._coverPageBgImage).Value.ToString();

                    htmlString2 = await PrepareCoverPageHtml(reportName, startDateForEmailReport.ToString("ddd, MMMM dd yyyy"), endDateForEmailReport.ToString("ddd, MMMM dd yyyy"), htmlString2, companyLogo, campaignLogo, companyName[0].ToString(), coverPageTextColor, coverPageBgColor, coverPageBgImage, campaignName[0].ToString());
                    htmlArray.Insert(0, htmlString2);
                }
                if (isTocExist.Length > 0)
                {
                    var modifiedTypeList = new List<string> { };
                    IndexSettings indexContent = null;
                    bool oldIndexExist = false;
                    string htmlString1 = "";
                    string footerText = string.Empty;
                    string showPageNumberId = string.Empty;
                    string showPageNumber = "hidden";
                    pageNumber = pageNumber + 1;

                    //string path = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/tableOfContent.html");
                    //htmlString1 =  System.IO.File.ReadAllText(path);

                    string pathToc = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/tableOfContent.html");
                    using (HttpClient httpclient = new HttpClient())
                    {
                        htmlString1 = httpclient.GetStringAsync(pathToc).Result;
                    }

                    dynamic data = JObject.Parse(ReportSchedule.ReportSetting.HeaderSettings);
                    string companyLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImg).Value.ToString();
                    string campaignLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImgCamp).Value.ToString();
                    string headerText = ((Newtonsoft.Json.Linq.JValue)data._headerTextValue).Value.ToString();
                    string showFooter = ((Newtonsoft.Json.Linq.JValue)data._showFooter).Value.ToString();
                    string showFooterPageNo = ((Newtonsoft.Json.Linq.JValue)data._showFooterPageNumber).Value.ToString();
                    string headerTextColor = ((Newtonsoft.Json.Linq.JValue)data._headerTextColor).Value.ToString();
                    string headerBgColor = ((Newtonsoft.Json.Linq.JValue)data._headerBgColor).Value.ToString();
                    string showHeader = ((Newtonsoft.Json.Linq.JValue)data._showHeader).Value.ToString();
                    if (Convert.ToBoolean(showFooter))
                    {
                        footerText = ((Newtonsoft.Json.Linq.JValue)data._footerText).Value.ToString();
                    }
                    if (Convert.ToBoolean(showFooterPageNo))
                    {
                        showPageNumberId = "pageFooter";
                        showPageNumber = "none";
                    }

                    //i have list of sting like 76(88[8af9ff7617]) , 76(89),75(76) if exist this type of data remove[8af9ff7617]
                    //var modifiedTypeList = RemovePatternFromList(reportTypeList);

                    if (ReportSchedule.ReportSetting?.IndexSettings != null)
                    {
                        indexContent = JsonConvert.DeserializeObject<IndexSettings>(ReportSchedule.ReportSetting?.IndexSettings);
                        modifiedTypeList = indexContent.IndexSettingsData;
                        oldIndexExist = false;

                    }
                    else
                    {

                        modifiedTypeList = RemovePatternFromList(reportTypeList);
                        oldIndexExist = true;
                    }

                    htmlString1 = await PrepareTableOfContentHtml(modifiedTypeList, htmlString1, companyLogo, campaignLogo, headerText, footerText, headerTextColor, headerBgColor, Convert.ToBoolean(showFooter), showPageNumberId, showPageNumber, Convert.ToBoolean(showHeader), pageNumber, oldIndexExist);
                    htmlArray.Add(htmlString1);
                }


                // Create a dictionary to group subtypes by type
                Dictionary<int, List<string>> typeSubtypeDictionary = new Dictionary<int, List<string>>();

                // Traverse the reportTypeList to combine subtypes for each type
                foreach (var reportType in reportTypeList)
                {
                    // Extract the type and subtypes using the provided method
                    TypeSubType typeSubType = ExtractTypeAndSubtypes(reportType);

                    // Check if the dictionary already contains an entry with the same type
                    if (typeSubtypeDictionary.ContainsKey(typeSubType.Type))
                    {
                        // If an entry with the same type exists, merge the subtypes
                        typeSubtypeDictionary[typeSubType.Type].AddRange(typeSubType.Subtype);
                    }
                    else
                    {
                        // If there's no entry with the same type, add the current reportType's subtypes to the dictionary
                        typeSubtypeDictionary[typeSubType.Type] = typeSubType.Subtype;
                    }
                }

                // Create the updatedReportTypeList by combining types and subtypes
                List<string> updatedReportTypeList = typeSubtypeDictionary
                    .Select(kv => kv.Value.Count > 0 ? $"{kv.Key}({string.Join(",", kv.Value)})" : kv.Key.ToString())
                    .ToList();

                foreach (var reportType in updatedReportTypeList)
                {
                    if (!String.IsNullOrEmpty(reportType))
                    {

                        List<string> types = new List<string>();
                        List<string> subtypes = new List<string>();
                        int type = 0;

                        var res = ExtractTypeAndSubtypes(reportType);

                        type = res.Type;
                        subtypes = res.Subtype;

                        if (type == (int)ReportTypes.GoogleAnalyticsFour)
                        {
                            string htmlString = "";
                            string footerText = string.Empty;
                            string showPageNumberId = string.Empty;
                            string showPageNumber = "hidden";
                            pageNumber = pageNumber + 1;
                            try
                            {
                                var g4Setup = _campaignGoogleAnalyticsRepository.GetAllEntities(true).Where(x => x.CampaignID == campaignId && x.IsGa4 == true).FirstOrDefault();

                                if (g4Setup != null)
                                {

                                    dynamic data = JObject.Parse(ReportSchedule.ReportSetting.HeaderSettings);
                                    string companyLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImg).Value.ToString();
                                    string campaignLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImgCamp).Value.ToString();
                                    string headerText = ((Newtonsoft.Json.Linq.JValue)data._headerTextValue).Value.ToString();
                                    string showFooter = ((Newtonsoft.Json.Linq.JValue)data._showFooter).Value.ToString();
                                    string showFooterPageNo = ((Newtonsoft.Json.Linq.JValue)data._showFooterPageNumber).Value.ToString();
                                    string headerTextColor = ((Newtonsoft.Json.Linq.JValue)data._headerTextColor).Value.ToString();
                                    string headerBgColor = ((Newtonsoft.Json.Linq.JValue)data._headerBgColor).Value.ToString();
                                    string showHeader = ((Newtonsoft.Json.Linq.JValue)data._showHeader).Value.ToString();
                                    if (Convert.ToBoolean(showFooter))
                                    {
                                        footerText = ((Newtonsoft.Json.Linq.JValue)data._footerText).Value.ToString();
                                    }
                                    if (Convert.ToBoolean(showFooterPageNo))
                                    {
                                        showPageNumberId = "pageFooter";
                                        showPageNumber = "none";
                                    }

                                    //add all subtypes in ga4 sub type list
                                    if (subtypes.Count == 0)
                                    {
                                        subtypes.AddRange(new List<string>
                                        {
                                            "24", "25","26","27","28","29"
                                        });
                                    }

                                    var result = await PrepareGa4Reports(g4Setup,
                                        startDateForEmailReport, endDateForEmailReport, subtypes);
                                    listOfResult.AddRange(result.htmlString);

                                    listOfRawData.Add("Google analytics 4 data " +JsonConvert.SerializeObject(result.Ga4RawData));
                                }
                            }
                            catch (Exception ex)
                            {
                                htmlString = "<p><h3>Something went wrong in google analytics 4.</h3></p>";
                                htmlArray.Add(htmlString);
                            }

                        }
                        else if (type == (int)ReportTypes.GoogleSearchConsole)
                        {
                            string htmlString = "";
                            string footerText = string.Empty;
                            string showPageNumberId = string.Empty;
                            string showPageNumber = "hidden";
                            pageNumber = pageNumber + 1;
                            try
                            {
                                var gscSetup = _campaignGSCRepository.GetFilteredEntities(true).Where(x => x.CampaignID == campaignId).FirstOrDefault();

                                if (gscSetup != null)
                                {
                                    var accessToken = await GetAccessTokenUsingRefreshToken(gscSetup.RefreshToken);

                                    string pathGsc = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/gsc.html");

                                    using (HttpClient httpclient = new HttpClient())
                                    {
                                        htmlString = httpclient.GetStringAsync(pathGsc).Result;
                                    }
                                    dynamic data = JObject.Parse(ReportSchedule.ReportSetting.HeaderSettings);
                                    string companyLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImg).Value.ToString();
                                    string campaignLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImgCamp).Value.ToString();
                                    string headerText = ((Newtonsoft.Json.Linq.JValue)data._headerTextValue).Value.ToString();
                                    string showFooter = ((Newtonsoft.Json.Linq.JValue)data._showFooter).Value.ToString();
                                    string showFooterPageNo = ((Newtonsoft.Json.Linq.JValue)data._showFooterPageNumber).Value.ToString();
                                    string headerTextColor = ((Newtonsoft.Json.Linq.JValue)data._headerTextColor).Value.ToString();
                                    string headerBgColor = ((Newtonsoft.Json.Linq.JValue)data._headerBgColor).Value.ToString();
                                    string showHeader = ((Newtonsoft.Json.Linq.JValue)data._showHeader).Value.ToString();
                                    if (Convert.ToBoolean(showFooter))
                                    {
                                        footerText = ((Newtonsoft.Json.Linq.JValue)data._footerText).Value.ToString();
                                    }
                                    if (Convert.ToBoolean(showFooterPageNo))
                                    {
                                        showPageNumberId = "pageFooter";
                                        showPageNumber = "none";
                                    }

                                    if (subtypes.Count == 0)
                                    {
                                        subtypes.AddRange(new List<string>
                                    {
                                        "30", "31","32","33"
                                    });
                                    }
                                    var GscIntegrationData = await PrepareGoogleSearchConsole(startDateForEmailReport.ToString("yyyy-MM-dd"), endDateForEmailReport.ToString("yyyy-MM-dd"), gscSetup.UrlOrName, accessToken, previousDate, subtypes);
                                    listOfResult.AddRange(GscIntegrationData.HtmlString);
                                    //raw data of gsc
                                    listOfRawData.Add( "Google search console data " + JsonConvert.SerializeObject(GscIntegrationData.GscRawData));
                                }

                            }
                            catch (Exception)
                            {
                                htmlString = "<p><h3>Something went wrong in google search console.</h3></p>";
                                htmlArray.Add(htmlString);
                            }

                        }
                        else if (type == (int)ReportTypes.GoogleAdsCampaign)
                        {
                            string htmlString = "";
                            string footerText = string.Empty;
                            string showPageNumberId = string.Empty;
                            string showPageNumber = "hidden";
                            pageNumber = pageNumber + 1;
                            try
                            {
                                var gAdsSetup = _campaignGoogleAdsRepository.GetFilteredEntities(true).Where(x => x.CampaignID == campaignId).FirstOrDefault();

                                if (gAdsSetup != null)
                                {

                                    string pathGAds = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/gAds.html");

                                    using (HttpClient httpclient = new HttpClient())
                                    {
                                        htmlString = httpclient.GetStringAsync(pathGAds).Result;
                                    }
                                    dynamic data = JObject.Parse(ReportSchedule.ReportSetting.HeaderSettings);
                                    string companyLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImg).Value.ToString();
                                    string campaignLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImgCamp).Value.ToString();
                                    string headerText = ((Newtonsoft.Json.Linq.JValue)data._headerTextValue).Value.ToString();
                                    string showFooter = ((Newtonsoft.Json.Linq.JValue)data._showFooter).Value.ToString();
                                    string showFooterPageNo = ((Newtonsoft.Json.Linq.JValue)data._showFooterPageNumber).Value.ToString();
                                    string headerTextColor = ((Newtonsoft.Json.Linq.JValue)data._headerTextColor).Value.ToString();
                                    string headerBgColor = ((Newtonsoft.Json.Linq.JValue)data._headerBgColor).Value.ToString();
                                    string showHeader = ((Newtonsoft.Json.Linq.JValue)data._showHeader).Value.ToString();
                                    if (Convert.ToBoolean(showFooter))
                                    {
                                        footerText = ((Newtonsoft.Json.Linq.JValue)data._footerText).Value.ToString();
                                    }
                                    if (Convert.ToBoolean(showFooterPageNo))
                                    {
                                        showPageNumberId = "pageFooter";
                                        showPageNumber = "none";
                                    }

                                    var googleAdsReportData = PrepareGoogleAdsReport(htmlString, campaignId, startDateForEmailReport.ToString("yyyy-MM-dd"), endDateForEmailReport.ToString("yyyy-MM-dd"), type, campaignLogo, companyLogo, headerText, footerText, headerBgColor, headerTextColor, Convert.ToBoolean(showFooter), showPageNumberId, showPageNumber, Convert.ToBoolean(showHeader), pageNumber);

                                    string uniqueKey = $"{type}";
                                    listOfResult.Add(new Dictionary<string, string>()
                                    {
                                        { uniqueKey, googleAdsReportData.Html }
                                    });

                                    listOfRawData.Add("Google ads data " + JsonConvert.SerializeObject(googleAdsReportData.GoogleAdsRawData));

                                }
                            }
                            catch (Exception)
                            {
                                htmlString = "<p><h3>Something went wrong in google ads campaign.</h3></p>";
                                htmlArray.Add(htmlString);
                            }

                        }
                        else if (type == (int)ReportTypes.GoogleAdsGroups)
                        {
                            string htmlString = "";
                            string footerText = string.Empty;
                            string showPageNumberId = string.Empty;
                            string showPageNumber = "hidden";
                            pageNumber = pageNumber + 1;
                            try
                            {
                                var gAdsSetup = _campaignGoogleAdsRepository.GetFilteredEntities(true).Where(x => x.CampaignID == campaignId).FirstOrDefault();

                                if (gAdsSetup != null)
                                {

                                    string pathGAds = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/gAdsGroups.html");

                                    using (HttpClient httpclient = new HttpClient())
                                    {
                                        htmlString = httpclient.GetStringAsync(pathGAds).Result;
                                    }
                                    dynamic data = JObject.Parse(ReportSchedule.ReportSetting.HeaderSettings);
                                    string companyLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImg).Value.ToString();
                                    string campaignLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImgCamp).Value.ToString();
                                    string headerText = ((Newtonsoft.Json.Linq.JValue)data._headerTextValue).Value.ToString();
                                    string showFooter = ((Newtonsoft.Json.Linq.JValue)data._showFooter).Value.ToString();
                                    string showFooterPageNo = ((Newtonsoft.Json.Linq.JValue)data._showFooterPageNumber).Value.ToString();
                                    string headerTextColor = ((Newtonsoft.Json.Linq.JValue)data._headerTextColor).Value.ToString();
                                    string headerBgColor = ((Newtonsoft.Json.Linq.JValue)data._headerBgColor).Value.ToString();
                                    string showHeader = ((Newtonsoft.Json.Linq.JValue)data._showHeader).Value.ToString();
                                    if (Convert.ToBoolean(showFooter))
                                    {
                                        footerText = ((Newtonsoft.Json.Linq.JValue)data._footerText).Value.ToString();
                                    }
                                    if (Convert.ToBoolean(showFooterPageNo))
                                    {
                                        showPageNumberId = "pageFooter";
                                        showPageNumber = "none";
                                    }

                                    var googleAdsReportData = PrepareGoogleAdsReport(htmlString, campaignId, startDateForEmailReport.ToString("yyyy-MM-dd"), endDateForEmailReport.ToString("yyyy-MM-dd"), type, campaignLogo, companyLogo, headerText, footerText, headerBgColor, headerTextColor, Convert.ToBoolean(showFooter), showPageNumberId, showPageNumber, Convert.ToBoolean(showHeader), pageNumber);

                                    string uniqueKey = $"{type}";
                                    listOfResult.Add(new Dictionary<string, string>()
                                    {
                                        { uniqueKey, googleAdsReportData.Html }
                                    });

                                    listOfRawData.Add("Google ads data " + JsonConvert.SerializeObject(googleAdsReportData.GoogleAdsRawData));
                                }
                            }
                            catch (Exception)
                            {
                                htmlString = "<p><h3>Something went wrong in google ads groups.</h3></p>";
                                htmlArray.Add(htmlString);
                            }

                        }
                        else if (type == (int)ReportTypes.GoogleAdsCopies)
                        {
                            string htmlString = "";
                            string footerText = string.Empty;
                            string showPageNumberId = string.Empty;
                            string showPageNumber = "hidden";
                            pageNumber = pageNumber + 1;
                            try
                            {
                                var gAdsSetup = _campaignGoogleAdsRepository.GetFilteredEntities(true).Where(x => x.CampaignID == campaignId).FirstOrDefault();

                                if (gAdsSetup != null)
                                {

                                    string pathGAds = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/gAdsCopies.html");

                                    using (HttpClient httpclient = new HttpClient())
                                    {
                                        htmlString = httpclient.GetStringAsync(pathGAds).Result;
                                    }
                                    dynamic data = JObject.Parse(ReportSchedule.ReportSetting.HeaderSettings);
                                    string companyLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImg).Value.ToString();
                                    string campaignLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImgCamp).Value.ToString();
                                    string headerText = ((Newtonsoft.Json.Linq.JValue)data._headerTextValue).Value.ToString();
                                    string showFooter = ((Newtonsoft.Json.Linq.JValue)data._showFooter).Value.ToString();
                                    string showFooterPageNo = ((Newtonsoft.Json.Linq.JValue)data._showFooterPageNumber).Value.ToString();
                                    string headerTextColor = ((Newtonsoft.Json.Linq.JValue)data._headerTextColor).Value.ToString();
                                    string headerBgColor = ((Newtonsoft.Json.Linq.JValue)data._headerBgColor).Value.ToString();
                                    string showHeader = ((Newtonsoft.Json.Linq.JValue)data._showHeader).Value.ToString();
                                    if (Convert.ToBoolean(showFooter))
                                    {
                                        footerText = ((Newtonsoft.Json.Linq.JValue)data._footerText).Value.ToString();
                                    }
                                    if (Convert.ToBoolean(showFooterPageNo))
                                    {
                                        showPageNumberId = "pageFooter";
                                        showPageNumber = "none";
                                    }

                                    //var googleAdsReportData = PrepareGoogleAdsReport(htmlString, campaignId, "2023-06-01", "2023-06-30", type, campaignLogo, companyLogo, headerText, footerText, headerBgColor, headerTextColor, Convert.ToBoolean(showFooter), showPageNumberId, showPageNumber, Convert.ToBoolean(showHeader), pageNumber);

                                    var googleAdsReportData = PrepareGoogleAdsReport(htmlString, campaignId, startDateForEmailReport.ToString("yyyy-MM-dd"), endDateForEmailReport.ToString("yyyy-MM-dd"), type, campaignLogo, companyLogo, headerText, footerText, headerBgColor, headerTextColor, Convert.ToBoolean(showFooter), showPageNumberId, showPageNumber, Convert.ToBoolean(showHeader), pageNumber);

                                    string uniqueKey = $"{type}";
                                    listOfResult.Add(new Dictionary<string, string>()
                                    {
                                        { uniqueKey, googleAdsReportData.Html }
                                    });

                                    listOfRawData.Add("Google ads data " + JsonConvert.SerializeObject(googleAdsReportData.GoogleAdsRawData));

                                }
                            }
                            catch (Exception)
                            {
                                htmlString = "<p><h3>Something went wrong in google ads copies.</h3></p>";
                                htmlArray.Add(htmlString);
                            }

                        }
                        else if (type == (int)ReportTypes.Facebook)
                        {

                            string footerText = string.Empty;
                            string showPageNumberId = string.Empty;
                            string htmlString = "";
                            string showPageNumber = "hidden";
                            pageNumber = pageNumber + 1;
                            try
                            {
                                var facebookReportData = await _campaignFacebookService.GetFacebookReport(campaignId, startDateForEmailReport, endDateForEmailReport);

                                dynamic data = JObject.Parse(ReportSchedule.ReportSetting.HeaderSettings);
                                string companyLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImg).Value.ToString();
                                string campaignLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImgCamp).Value.ToString();
                                string headerText = ((Newtonsoft.Json.Linq.JValue)data._headerTextValue).Value.ToString();
                                string showFooter = ((Newtonsoft.Json.Linq.JValue)data._showFooter).Value.ToString();
                                string showFooterPageNo = ((Newtonsoft.Json.Linq.JValue)data._showFooterPageNumber).Value.ToString();
                                string headerTextColor = ((Newtonsoft.Json.Linq.JValue)data._headerTextColor).Value.ToString();
                                string headerBgColor = ((Newtonsoft.Json.Linq.JValue)data._headerBgColor).Value.ToString();
                                string showHeader = ((Newtonsoft.Json.Linq.JValue)data._showHeader).Value.ToString();
                                if (Convert.ToBoolean(showFooter))
                                {
                                    footerText = ((Newtonsoft.Json.Linq.JValue)data._footerText).Value.ToString();
                                }
                                if (Convert.ToBoolean(showFooterPageNo))
                                {
                                    showPageNumberId = "pageFooter";
                                    showPageNumber = "none";
                                }

                                //add all subtypes in ga4 sub type list
                                if (subtypes.Count == 0)
                                {
                                    subtypes.AddRange(new List<string>
                                    {
                                        "34","35"
                                    });
                                }

                                var listHtml = PrepareFacebookHtml(facebookReportData, subtypes);

                                listOfResult.AddRange(listHtml);

                                listOfRawData.Add("Facebook data " + JsonConvert.SerializeObject(facebookReportData));
                            }
                            catch (Exception e)
                            {
                                htmlString = "<p><h3>Something went wrong in facebook.</h3></p>";
                                htmlArray.Add(htmlString);
                            }

                        }
                        else if (type == (int)ReportTypes.FacebookAdsCampaign)
                        {
                            string htmlString = "";
                            string footerText = string.Empty;
                            string showPageNumberId = string.Empty;
                            string showPageNumber = "hidden";
                            pageNumber = pageNumber + 1;
                            try
                            {
                                var facebookAdsSetup = _campaignFacebookAdsRepository.GetFilteredEntities(true).Where(x => x.CampaignID == campaignId).FirstOrDefault();

                                if (facebookAdsSetup != null)
                                {
                                    string pathFbAds1 = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/facebookAdsCampaign.html");

                                    using (HttpClient httpclient = new HttpClient())
                                    {
                                        htmlString = httpclient.GetStringAsync(pathFbAds1).Result;
                                    }

                                    //string pathFbAds1 = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/facebookAdsCampaign.html");
                                    //htmlString = System.IO.File.ReadAllText(pathFbAds1);
                                    var isTokenValid = await _campaignFacebookService.GetAccessTokenDetails(facebookAdsSetup.AccessToken);
                                    if (isTokenValid.data.is_valid)
                                    {
                                        dynamic data = JObject.Parse(ReportSchedule.ReportSetting.HeaderSettings);
                                        string companyLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImg).Value.ToString();
                                        string campaignLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImgCamp).Value.ToString();
                                        string headerText = ((Newtonsoft.Json.Linq.JValue)data._headerTextValue).Value.ToString();
                                        string showFooter = ((Newtonsoft.Json.Linq.JValue)data._showFooter).Value.ToString();
                                        string showFooterPageNo = ((Newtonsoft.Json.Linq.JValue)data._showFooterPageNumber).Value.ToString();
                                        string headerTextColor = ((Newtonsoft.Json.Linq.JValue)data._headerTextColor).Value.ToString();
                                        string headerBgColor = ((Newtonsoft.Json.Linq.JValue)data._headerBgColor).Value.ToString();
                                        string showHeader = ((Newtonsoft.Json.Linq.JValue)data._showHeader).Value.ToString();
                                        if (Convert.ToBoolean(showFooter))
                                        {
                                            footerText = ((Newtonsoft.Json.Linq.JValue)data._footerText).Value.ToString();
                                        }
                                        if (Convert.ToBoolean(showFooterPageNo))
                                        {
                                            showPageNumberId = "pageFooter";
                                            showPageNumber = "none";
                                        }

                                        var result = await PrepareFacebookAdsCampaign(htmlString, campaignId, startDateForEmailReport, endDateForEmailReport, facebookAdsSetup.AccessToken, facebookAdsSetup, previousDate, campaignLogo, companyLogo, headerText, footerText, headerBgColor, headerTextColor, Convert.ToBoolean(showFooter), showPageNumberId, showPageNumber, Convert.ToBoolean(showHeader), pageNumber);
                                        string uniqueKey = $"{type}";
                                        listOfResult.Add(new Dictionary<string, string>()
                                        {
                                            { uniqueKey, result.Html }
                                        });

                                        listOfRawData.Add("Facebook ads campaign data :" + JsonConvert.SerializeObject(result.CampaignPerformace));
                                    }
                                }
                                else
                                {
                                    //Token is not valid
                                }
                            }
                            catch (Exception)
                            {
                                htmlString = "<p><h3>Something went wrong in facebook ads campaign.</h3></p>";
                                htmlArray.Add(htmlString);
                            }

                        }
                        else if (type == (int)ReportTypes.FacebookAdsGroup)
                        {
                            string htmlString = "";
                            string footerText = string.Empty;
                            string showPageNumberId = string.Empty;
                            string showPageNumber = "hidden";
                            pageNumber = pageNumber + 1;
                            try
                            {
                                var facebookAdsSetup = _campaignFacebookAdsRepository.GetFilteredEntities(true).Where(x => x.CampaignID == campaignId).FirstOrDefault();


                                if (facebookAdsSetup != null)
                                {
                                    string pathFbAds1 = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/facebookAdsGroups.html");

                                    using (HttpClient httpclient = new HttpClient())
                                    {
                                        htmlString = httpclient.GetStringAsync(pathFbAds1).Result;
                                    }

                                    var isTokenValid = await _campaignFacebookService.GetAccessTokenDetails(facebookAdsSetup.AccessToken);
                                    if (isTokenValid.data.is_valid)
                                    {
                                        dynamic data = JObject.Parse(ReportSchedule.ReportSetting.HeaderSettings);
                                        string companyLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImg).Value.ToString();
                                        string campaignLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImgCamp).Value.ToString();
                                        string headerText = ((Newtonsoft.Json.Linq.JValue)data._headerTextValue).Value.ToString();
                                        string showFooter = ((Newtonsoft.Json.Linq.JValue)data._showFooter).Value.ToString();
                                        string showFooterPageNo = ((Newtonsoft.Json.Linq.JValue)data._showFooterPageNumber).Value.ToString();
                                        string headerTextColor = ((Newtonsoft.Json.Linq.JValue)data._headerTextColor).Value.ToString();
                                        string headerBgColor = ((Newtonsoft.Json.Linq.JValue)data._headerBgColor).Value.ToString();
                                        string showHeader = ((Newtonsoft.Json.Linq.JValue)data._showHeader).Value.ToString();
                                        if (Convert.ToBoolean(showFooter))
                                        {
                                            footerText = ((Newtonsoft.Json.Linq.JValue)data._footerText).Value.ToString();
                                        }
                                        if (Convert.ToBoolean(showFooterPageNo))
                                        {
                                            showPageNumberId = "pageFooter";
                                            showPageNumber = "none";
                                        }

                                        var result = await PrepareFacebookAdsGroups(htmlString, campaignId, startDateForEmailReport, endDateForEmailReport, facebookAdsSetup.AccessToken, facebookAdsSetup, previousDate, campaignLogo, companyLogo, headerText, footerText, headerBgColor, headerTextColor, Convert.ToBoolean(showFooter), showPageNumberId, showPageNumber, Convert.ToBoolean(showHeader), pageNumber);

                                        //htmlString = await PrepareFacebookAdsCampaign(htmlString, campaignId, startDateForEmailReport, endDateForEmailReport, facebookAdsSetup.AccessToken, facebookAdsSetup, previousDate, campaignLogo, companyLogo, headerText, footerText, headerBgColor, headerTextColor, Convert.ToBoolean(showFooter), showPageNumberId, showPageNumber, Convert.ToBoolean(showHeader), pageNumber);
                                        string uniqueKey = $"{type}";
                                        listOfResult.Add(new Dictionary<string, string>()
                                        {
                                            { uniqueKey, result.Html }
                                        });

                                        listOfRawData.Add("Facebook ads groups data :" + JsonConvert.SerializeObject(result.FbAdsSetData));
                                    }
                                }
                                else
                                {
                                    //Token is not valid
                                }
                            }
                            catch (Exception)
                            {
                                htmlString = "<p><h3>Something went wrong in facebook ads group.</h3></p>";
                                htmlArray.Add(htmlString);
                            }

                        }
                        else if (type == (int)ReportTypes.FacebookAdsCopies)
                        {
                            string htmlString = "";
                            string footerText = string.Empty;
                            string showPageNumberId = string.Empty;
                            string showPageNumber = "hidden";
                            pageNumber = pageNumber + 1;
                            try
                            {
                                var facebookAdsSetup = _campaignFacebookAdsRepository.GetFilteredEntities(true).Where(x => x.CampaignID == campaignId).FirstOrDefault();


                                if (facebookAdsSetup != null)
                                {
                                    string pathFbAds1 = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/facebookAdsCopies.html");

                                    using (HttpClient httpclient = new HttpClient())
                                    {
                                        htmlString = httpclient.GetStringAsync(pathFbAds1).Result;
                                    }

                                    var isTokenValid = await _campaignFacebookService.GetAccessTokenDetails(facebookAdsSetup.AccessToken);
                                    if (isTokenValid.data.is_valid)
                                    {
                                        dynamic data = JObject.Parse(ReportSchedule.ReportSetting.HeaderSettings);
                                        string companyLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImg).Value.ToString();
                                        string campaignLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImgCamp).Value.ToString();
                                        string headerText = ((Newtonsoft.Json.Linq.JValue)data._headerTextValue).Value.ToString();
                                        string showFooter = ((Newtonsoft.Json.Linq.JValue)data._showFooter).Value.ToString();
                                        string showFooterPageNo = ((Newtonsoft.Json.Linq.JValue)data._showFooterPageNumber).Value.ToString();
                                        string headerTextColor = ((Newtonsoft.Json.Linq.JValue)data._headerTextColor).Value.ToString();
                                        string headerBgColor = ((Newtonsoft.Json.Linq.JValue)data._headerBgColor).Value.ToString();
                                        string showHeader = ((Newtonsoft.Json.Linq.JValue)data._showHeader).Value.ToString();
                                        if (Convert.ToBoolean(showFooter))
                                        {
                                            footerText = ((Newtonsoft.Json.Linq.JValue)data._footerText).Value.ToString();
                                        }
                                        if (Convert.ToBoolean(showFooterPageNo))
                                        {
                                            showPageNumberId = "pageFooter";
                                            showPageNumber = "none";
                                        }

                                        htmlString = await PrepareFacebookAdsCopies(htmlString, campaignId, startDateForEmailReport, endDateForEmailReport, facebookAdsSetup.AccessToken, facebookAdsSetup, previousDate, campaignLogo, companyLogo, headerText, footerText, headerBgColor, headerTextColor, Convert.ToBoolean(showFooter), showPageNumberId, showPageNumber, Convert.ToBoolean(showHeader), pageNumber);

                                        // htmlString = await PrepareFacebookAdsCampaign(htmlString, campaignId, startDateForEmailReport, endDateForEmailReport, facebookAdsSetup.AccessToken, facebookAdsSetup, previousDate, campaignLogo, companyLogo, headerText, footerText, headerBgColor, headerTextColor, Convert.ToBoolean(showFooter), showPageNumberId, showPageNumber, Convert.ToBoolean(showHeader), pageNumber);
                                        string uniqueKey = $"{type}";
                                        listOfResult.Add(new Dictionary<string, string>()
                                        {
                                            { uniqueKey, htmlString }
                                        });
                                    }
                                }
                                else
                                {
                                    //Token is not valid
                                }
                            }
                            catch (Exception)
                            {
                                htmlString = "<p><h3>Something went wrong in facebook ads copies.</h3></p>";
                                htmlArray.Add(htmlString);
                            }

                        }
                        else if (type == (int)ReportTypes.Instagram)
                        {
                            string htmlString = "";
                            string footerText = string.Empty;
                            string showPageNumberId = string.Empty;
                            string showPageNumber = "hidden";
                            pageNumber = pageNumber + 1;
                            try
                            {
                                var instaSetup = _campaignInstagramRepository.GetFilteredEntities(true).Where(x => x.CampaignID == campaignId).FirstOrDefault();

                                if (instaSetup != null)
                                {
                                    var isTokenValid = await _campaignFacebookService.GetAccessTokenDetails(instaSetup.AccessToken);
                                    if (isTokenValid.data.is_valid)
                                    {
                                        dynamic data = JObject.Parse(ReportSchedule.ReportSetting.HeaderSettings);
                                        string companyLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImg).Value.ToString();
                                        string campaignLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImgCamp).Value.ToString();
                                        string headerText = ((Newtonsoft.Json.Linq.JValue)data._headerTextValue).Value.ToString();
                                        string showFooter = ((Newtonsoft.Json.Linq.JValue)data._showFooter).Value.ToString();
                                        string showFooterPageNo = ((Newtonsoft.Json.Linq.JValue)data._showFooterPageNumber).Value.ToString();
                                        string headerTextColor = ((Newtonsoft.Json.Linq.JValue)data._headerTextColor).Value.ToString();
                                        string headerBgColor = ((Newtonsoft.Json.Linq.JValue)data._headerBgColor).Value.ToString();
                                        string showHeader = ((Newtonsoft.Json.Linq.JValue)data._showHeader).Value.ToString();
                                        if (Convert.ToBoolean(showFooter))
                                        {
                                            footerText = ((Newtonsoft.Json.Linq.JValue)data._footerText).Value.ToString();
                                        }
                                        if (Convert.ToBoolean(showFooterPageNo))
                                        {
                                            showPageNumberId = "pageFooter";
                                            showPageNumber = "none";
                                        }

                                        //add all subtypes in ga4 sub type list
                                        if (subtypes.Count == 0)
                                        {
                                            subtypes.AddRange(new List<string>
                                            {
                                                "36","37","38"
                                            });
                                        }

                                        var instagramReportList = await PrepareInstagramReport(startDateForEmailReport, endDateForEmailReport, instaSetup.AccessToken, instaSetup.UrlOrName, subtypes);
                                        htmlArray.AddRange(instagramReportList.HtmlList);

                                        listOfRawData.Add("Instagram data " + JsonConvert.SerializeObject(instagramReportList.InstagramReportsData));
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                htmlString = "<p><h3>Something went wrong in instagram.</h3></p>";
                                htmlArray.Add(htmlString);
                            }

                        }
                        else if (type == (int)ReportTypes.LinkedInEngagement)
                        {
                            string htmlString = "";
                            string footerText = string.Empty;
                            string showPageNumberId = string.Empty;
                            string showPageNumber = "hidden";
                            pageNumber = pageNumber + 1;
                            try
                            {
                                // prepare follower gains data
                                var linkedInSetup = _campaignLinkedinService.GetCampaignLinkedinByCampaignId(campaignId.ToString());

                                if (linkedInSetup.Count > 0)
                                {
                                    DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();
                                    TimeSpan spanStart = (startDateForEmailReport - epoch);
                                    TimeSpan spanEnd = (endDateForEmailReport - epoch);

                                    // add 3 digit to manage 13 digit miliseconds
                                    string startDate = spanStart.TotalSeconds + "000";
                                    string endDate = spanEnd.TotalSeconds + "000";

                                    string pathLinkedin = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/linkedIn.html");

                                    using (HttpClient httpclient = new HttpClient())
                                    {
                                        htmlString = httpclient.GetStringAsync(pathLinkedin).Result;
                                    }

                                    //string pathLinkedin = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/linkedIn.html");
                                    //htmlString = System.IO.File.ReadAllText(pathLinkedin);

                                    dynamic data = JObject.Parse(ReportSchedule.ReportSetting.HeaderSettings);
                                    string companyLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImg).Value.ToString();
                                    string campaignLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImgCamp).Value.ToString();
                                    string headerText = ((Newtonsoft.Json.Linq.JValue)data._headerTextValue).Value.ToString();
                                    string showFooter = ((Newtonsoft.Json.Linq.JValue)data._showFooter).Value.ToString();
                                    string showFooterPageNo = ((Newtonsoft.Json.Linq.JValue)data._showFooterPageNumber).Value.ToString();
                                    string headerTextColor = ((Newtonsoft.Json.Linq.JValue)data._headerTextColor).Value.ToString();
                                    string headerBgColor = ((Newtonsoft.Json.Linq.JValue)data._headerBgColor).Value.ToString();
                                    string showHeader = ((Newtonsoft.Json.Linq.JValue)data._showHeader).Value.ToString();
                                    if (Convert.ToBoolean(showFooter))
                                    {
                                        footerText = ((Newtonsoft.Json.Linq.JValue)data._footerText).Value.ToString();
                                    }
                                    if (Convert.ToBoolean(showFooterPageNo))
                                    {
                                        showPageNumberId = "pageFooter";
                                        showPageNumber = "none";
                                    }

                                    //"1639353600000", "1641859200000"
                                    var linkedInReportData = PrepareLinkedInReport(htmlString, campaignId, startDate, endDate, campaignLogo, companyLogo, headerText, footerText, headerBgColor, headerTextColor, Convert.ToBoolean(showFooter), showPageNumberId, showPageNumber, Convert.ToBoolean(showHeader), pageNumber);

                                    string uniqueKey = $"{type}";
                                    listOfResult.Add(new Dictionary<string, string>()
                                        {
                                            { uniqueKey, linkedInReportData.Html }
                                        });

                                    var test = JsonConvert.SerializeObject(linkedInReportData.LinkedinRawData);

                                    listOfRawData.Add("Linkedin data " + JsonConvert.SerializeObject(linkedInReportData.LinkedinRawData));
                                }
                            }
                            catch (Exception)
                            {
                                htmlString = "<p><h3>Something went wrong in linkedin engagement.</h3></p>";
                                htmlArray.Add(htmlString);
                            }

                        }
                        else if (type == (int)ReportTypes.LinkedInDemographic)
                        {
                            string htmlString = "";
                            string footerText = string.Empty;
                            string showPageNumberId = string.Empty;
                            string showPageNumber = "hidden";
                            pageNumber = pageNumber + 1;
                            try
                            {
                                // prepare follower gains data
                                var linkedInSetup = _campaignLinkedinService.GetCampaignLinkedinByCampaignId(campaignId.ToString());

                                if (linkedInSetup.Count > 0)
                                {

                                    string pathLinkedin = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/linkedInDemographics.html");

                                    using (HttpClient httpclient = new HttpClient())
                                    {
                                        htmlString = httpclient.GetStringAsync(pathLinkedin).Result;
                                    }

                                    //string pathLinkedin = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/linkedInDemographics.html");
                                    // htmlString = System.IO.File.ReadAllText(pathLinkedin);


                                    dynamic data = JObject.Parse(ReportSchedule.ReportSetting.HeaderSettings);
                                    string companyLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImg).Value.ToString();
                                    string campaignLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImgCamp).Value.ToString();
                                    string headerText = ((Newtonsoft.Json.Linq.JValue)data._headerTextValue).Value.ToString();
                                    string showFooter = ((Newtonsoft.Json.Linq.JValue)data._showFooter).Value.ToString();
                                    string showFooterPageNo = ((Newtonsoft.Json.Linq.JValue)data._showFooterPageNumber).Value.ToString();
                                    string headerTextColor = ((Newtonsoft.Json.Linq.JValue)data._headerTextColor).Value.ToString();
                                    string headerBgColor = ((Newtonsoft.Json.Linq.JValue)data._headerBgColor).Value.ToString();
                                    string showHeader = ((Newtonsoft.Json.Linq.JValue)data._showHeader).Value.ToString();
                                    if (Convert.ToBoolean(showFooter))
                                    {
                                        footerText = ((Newtonsoft.Json.Linq.JValue)data._footerText).Value.ToString();
                                    }
                                    if (Convert.ToBoolean(showFooterPageNo))
                                    {
                                        showPageNumberId = "pageFooter";
                                        showPageNumber = "none";
                                    }

                                    //"1639353600000", "1641859200000"
                                    var linkedInReportData = await PrepareDemographicLinkedInReport(htmlString, campaignId, campaignLogo, companyLogo, headerText, footerText, headerBgColor, headerTextColor, Convert.ToBoolean(showFooter), showPageNumberId, showPageNumber, Convert.ToBoolean(showHeader), pageNumber);

                                    string uniqueKey = $"{type}";
                                    listOfResult.Add(new Dictionary<string, string>()
                                        {
                                            { uniqueKey, linkedInReportData.Html }
                                        });

                                    listOfRawData.Add("Linkedin data demographic " + JsonConvert.SerializeObject(linkedInReportData.LinkedinDemographicRawData));
                                }
                            }
                            catch (Exception)
                            {
                                htmlString = "<p><h3>Something went wrong in linkedin demographic.</h3></p>";
                                htmlArray.Add(htmlString);
                            }

                        }
                        else if (type == (int)ReportTypes.LinkedInAdsCampaign)
                        {
                            string htmlString = "";
                            string footerText = string.Empty;
                            string showPageNumberId = string.Empty;
                            string showPageNumber = "hidden";
                            pageNumber = pageNumber + 1;
                            try
                            {
                                // prepare follower gains data
                                var linkedInSetup = _linkedinAdService.GetCampaignLinkedinByCampaignId(campaignId.ToString());

                                if (linkedInSetup.Count > 0)
                                {

                                    // string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/linkedinAdCampaign.html");
                                    //htmlString = System.IO.File.ReadAllText(pathGa);

                                    string pathLinkedin = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/linkedinAdCampaign.html");

                                    using (HttpClient httpclient = new HttpClient())
                                    {
                                        htmlString = httpclient.GetStringAsync(pathLinkedin).Result;
                                    }
                                    dynamic data = JObject.Parse(ReportSchedule.ReportSetting.HeaderSettings);
                                    string companyLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImg).Value.ToString();
                                    string campaignLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImgCamp).Value.ToString();
                                    string headerText = ((Newtonsoft.Json.Linq.JValue)data._headerTextValue).Value.ToString();
                                    string showFooter = ((Newtonsoft.Json.Linq.JValue)data._showFooter).Value.ToString();
                                    string showFooterPageNo = ((Newtonsoft.Json.Linq.JValue)data._showFooterPageNumber).Value.ToString();
                                    string headerTextColor = ((Newtonsoft.Json.Linq.JValue)data._headerTextColor).Value.ToString();
                                    string headerBgColor = ((Newtonsoft.Json.Linq.JValue)data._headerBgColor).Value.ToString();
                                    string showHeader = ((Newtonsoft.Json.Linq.JValue)data._showHeader).Value.ToString();
                                    if (Convert.ToBoolean(showFooter))
                                    {
                                        footerText = ((Newtonsoft.Json.Linq.JValue)data._footerText).Value.ToString();
                                    }
                                    if (Convert.ToBoolean(showFooterPageNo))
                                    {
                                        showPageNumberId = "pageFooter";
                                        showPageNumber = "none";
                                    }

                                    //"1639353600000", "1641859200000"
                                    var linkedInReportData = await PrepareLinkedInAdReport(htmlString, campaignId, campaignLogo, companyLogo, headerText, footerText, headerBgColor, headerTextColor, Convert.ToBoolean(showFooter), showPageNumberId, showPageNumber, Convert.ToBoolean(showHeader), pageNumber, (short)type, startDateForEmailReport.ToString("yyyy-MM-dd"), endDateForEmailReport.ToString("yyyy-MM-dd"), previousDate);

                                    string uniqueKey = $"{type}";
                                    listOfResult.Add(new Dictionary<string, string>()
                                        {
                                            { uniqueKey, linkedInReportData.Html }
                                        });

                                    listOfRawData.Add("Linkedin ads data " + JsonConvert.SerializeObject(linkedInReportData.LinkedinAdsCardData));
                                }
                            }
                            catch (Exception ex)
                            {
                                htmlString = "<p><h3>Something went wrong in linkedin demographic.</h3></p>";
                                htmlArray.Add(htmlString);
                            }

                        }
                        else if (type == (int)ReportTypes.LinkedInAdsAdgroups)
                        {
                            string htmlString = "";
                            string footerText = string.Empty;
                            string showPageNumberId = string.Empty;
                            string showPageNumber = "hidden";
                            pageNumber = pageNumber + 1;
                            try
                            {
                                // prepare follower gains data
                                var linkedInSetup = _linkedinAdService.GetCampaignLinkedinByCampaignId(campaignId.ToString());

                                if (linkedInSetup.Count > 0)
                                {

                                    string pathLinkedin = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/linkedinAdAdgroup.html");

                                    using (HttpClient httpclient = new HttpClient())
                                    {
                                        htmlString = httpclient.GetStringAsync(pathLinkedin).Result;
                                    }

                                    //string pathLinkedin = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/linkedinAdAdgroup.html");
                                    //htmlString = System.IO.File.ReadAllText(pathLinkedin);

                                    dynamic data = JObject.Parse(ReportSchedule.ReportSetting.HeaderSettings);
                                    string companyLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImg).Value.ToString();
                                    string campaignLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImgCamp).Value.ToString();
                                    string headerText = ((Newtonsoft.Json.Linq.JValue)data._headerTextValue).Value.ToString();
                                    string showFooter = ((Newtonsoft.Json.Linq.JValue)data._showFooter).Value.ToString();
                                    string showFooterPageNo = ((Newtonsoft.Json.Linq.JValue)data._showFooterPageNumber).Value.ToString();
                                    string headerTextColor = ((Newtonsoft.Json.Linq.JValue)data._headerTextColor).Value.ToString();
                                    string headerBgColor = ((Newtonsoft.Json.Linq.JValue)data._headerBgColor).Value.ToString();
                                    string showHeader = ((Newtonsoft.Json.Linq.JValue)data._showHeader).Value.ToString();
                                    if (Convert.ToBoolean(showFooter))
                                    {
                                        footerText = ((Newtonsoft.Json.Linq.JValue)data._footerText).Value.ToString();
                                    }
                                    if (Convert.ToBoolean(showFooterPageNo))
                                    {
                                        showPageNumberId = "pageFooter";
                                        showPageNumber = "none";
                                    }

                                    //"1639353600000", "1641859200000"
                                    var linkedInReportData = await PrepareLinkedInAdReport(htmlString, campaignId, campaignLogo, companyLogo, headerText, footerText, headerBgColor, headerTextColor, Convert.ToBoolean(showFooter), showPageNumberId, showPageNumber, Convert.ToBoolean(showHeader), pageNumber, (short)type, startDateForEmailReport.ToString("yyyy-MM-dd"), endDateForEmailReport.ToString("yyyy-MM-dd"), previousDate);

                                    string uniqueKey = $"{type}";
                                    listOfResult.Add(new Dictionary<string, string>()
                                        {
                                            { uniqueKey, linkedInReportData.Html }
                                        });

                                    listOfRawData.Add("Linkedin ads data " + JsonConvert.SerializeObject(linkedInReportData.LinkedinAdsCardData));

                                }
                            }
                            catch (Exception)
                            {
                                htmlString = "<p><h3>Something went wrong in linkedin demographic.</h3></p>";
                                htmlArray.Add(htmlString);
                            }

                        }
                        else if (type == (int)ReportTypes.LinkedInAdsCreative)
                        {
                            string htmlString = "";
                            string footerText = string.Empty;
                            string showPageNumberId = string.Empty;
                            string showPageNumber = "hidden";
                            pageNumber = pageNumber + 1;
                            try
                            {
                                // prepare follower gains data
                                var linkedInSetup = _linkedinAdService.GetCampaignLinkedinByCampaignId(campaignId.ToString());

                                if (linkedInSetup.Count > 0)
                                {
                                    string pathLinkedin = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/linkedinAdCreative.html");

                                    using (HttpClient httpclient = new HttpClient())
                                    {
                                        htmlString = httpclient.GetStringAsync(pathLinkedin).Result;
                                    }

                                    //string pathLinkedin = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/linkedinAdCreative.html");
                                    //htmlString = System.IO.File.ReadAllText(pathLinkedin);

                                    dynamic data = JObject.Parse(ReportSchedule.ReportSetting.HeaderSettings);
                                    string companyLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImg).Value.ToString();
                                    string campaignLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImgCamp).Value.ToString();
                                    string headerText = ((Newtonsoft.Json.Linq.JValue)data._headerTextValue).Value.ToString();
                                    string showFooter = ((Newtonsoft.Json.Linq.JValue)data._showFooter).Value.ToString();
                                    string showFooterPageNo = ((Newtonsoft.Json.Linq.JValue)data._showFooterPageNumber).Value.ToString();
                                    string headerTextColor = ((Newtonsoft.Json.Linq.JValue)data._headerTextColor).Value.ToString();
                                    string headerBgColor = ((Newtonsoft.Json.Linq.JValue)data._headerBgColor).Value.ToString();
                                    string showHeader = ((Newtonsoft.Json.Linq.JValue)data._showHeader).Value.ToString();
                                    if (Convert.ToBoolean(showFooter))
                                    {
                                        footerText = ((Newtonsoft.Json.Linq.JValue)data._footerText).Value.ToString();
                                    }
                                    if (Convert.ToBoolean(showFooterPageNo))
                                    {
                                        showPageNumberId = "pageFooter";
                                        showPageNumber = "none";
                                    }

                                    //"1639353600000", "1641859200000"
                                    var linkedInReportData = await PrepareLinkedInAdReport(htmlString, campaignId, campaignLogo, companyLogo, headerText, footerText, headerBgColor, headerTextColor, Convert.ToBoolean(showFooter), showPageNumberId, showPageNumber, Convert.ToBoolean(showHeader), pageNumber, (short)type, startDateForEmailReport.ToString("yyyy-MM-dd"), endDateForEmailReport.ToString("yyyy-MM-dd"), previousDate);

                                    string uniqueKey = $"{type}";
                                    listOfResult.Add(new Dictionary<string, string>()
                                        {
                                            { uniqueKey, linkedInReportData.Html }
                                        });

                                    listOfRawData.Add("Linkedin ads data " + JsonConvert.SerializeObject(linkedInReportData.LinkedinAdsCardData));
                                }
                            }
                            catch (Exception)
                            {
                                htmlString = "<p><h3>Something went wrong in linkedin demographic.</h3></p>";
                                htmlArray.Add(htmlString);
                            }

                        }
                        else if (type == (int)ReportTypes.Keywords)
                        {
                            string htmlString = "";
                            int offset = 40;
                            try
                            {
                                var keywordList = GetSerpKewordList(startDateForEmailReport.ToString("yyyy-MM-dd"), endDateForEmailReport.ToString("yyyy-MM-dd"), campaignId);
                                for (var i = 0; i < (float)keywordList.Count / offset; i++)
                                {
                                    pageNumber = pageNumber + 1;
                                    htmlString = "";
                                    string footerText = string.Empty;
                                    string showPageNumberId = string.Empty;
                                    string showPageNumber = "hidden";
                                    string pathKeyword = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/keywords.html");

                                    using (HttpClient httpclient = new HttpClient())
                                    {
                                        htmlString = httpclient.GetStringAsync(pathKeyword).Result;
                                    }

                                    dynamic data = JObject.Parse(ReportSchedule.ReportSetting.HeaderSettings);
                                    string companyLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImg).Value.ToString();
                                    string campaignLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImgCamp).Value.ToString();
                                    string headerText = ((Newtonsoft.Json.Linq.JValue)data._headerTextValue).Value.ToString();
                                    string showFooter = ((Newtonsoft.Json.Linq.JValue)data._showFooter).Value.ToString();
                                    string showFooterPageNo = ((Newtonsoft.Json.Linq.JValue)data._showFooterPageNumber).Value.ToString();
                                    string headerTextColor = ((Newtonsoft.Json.Linq.JValue)data._headerTextColor).Value.ToString();
                                    string headerBgColor = ((Newtonsoft.Json.Linq.JValue)data._headerBgColor).Value.ToString();
                                    string showHeader = ((Newtonsoft.Json.Linq.JValue)data._showHeader).Value.ToString();
                                    if (Convert.ToBoolean(showFooter))
                                    {
                                        footerText = ((Newtonsoft.Json.Linq.JValue)data._footerText).Value.ToString();
                                    }
                                    if (Convert.ToBoolean(showFooterPageNo))
                                    {
                                        showPageNumberId = "pageFooter";
                                        showPageNumber = "none";
                                    }
                                    htmlString = htmlString.Replace("_headerImage1_", companyLogo);
                                    htmlString = htmlString.Replace("_headerImage_", campaignLogo);
                                    htmlString = htmlString.Replace("_headerText_", headerText);
                                    htmlString = htmlString.Replace("_footerText_", footerText);
                                    htmlString = htmlString.Replace("_headerTextColor_", headerTextColor);
                                    htmlString = htmlString.Replace("_headerBgColor_", headerBgColor);
                                    htmlString = htmlString.Replace("_showPageNumber_", showPageNumber);
                                    htmlString = htmlString.Replace("_pageNumberId_", showPageNumberId);
                                    htmlString = htmlString.Replace("_pageNumber_", "Page " + pageNumber.ToString());
                                    if (Convert.ToBoolean(showHeader))
                                    {
                                        htmlString = htmlString.Replace("_showHeader_", "none");
                                    }
                                    else
                                    {
                                        htmlString = htmlString.Replace("_showHeader_", "hidden");
                                    }
                                    if (Convert.ToBoolean(showFooter))
                                    {
                                        htmlString = htmlString.Replace("_showFooter_", "none");
                                    }
                                    else
                                    {
                                        htmlString = htmlString.Replace("_showFooter_", "hidden");
                                    }

                                    if (campaignLogo == "")
                                    {
                                        htmlString = htmlString.Replace("_showImg2_", "hidden");
                                    }
                                    else
                                    {
                                        htmlString = htmlString.Replace("_showImg2_", "none");
                                    }

                                    if (companyLogo == "")
                                    {
                                        htmlString = htmlString.Replace("_showImg1_", "hidden");
                                    }
                                    else
                                    {
                                        htmlString = htmlString.Replace("_showImg1_", "none");
                                    }

                                    var newData = new List<SerpKeywordDataDto>();
                                    newData.AddRange(keywordList.Skip(i * offset).Take(offset));
                                    // serialize object
                                    var tableString = JsonConvert.SerializeObject(newData);
                                    htmlString = htmlString.Replace("_tableArrayList_", tableString);
                                    string uniqueKey = $"{type}";
                                    listOfResult.Add(new Dictionary<string, string>()
                                        {
                                            { uniqueKey, linkedInReport }
                                        });

                                    // Find the highest difference
                                    int maxDifference = newData.Select(keyword => Math.Abs(int.Parse(keyword.CurrentPosition) - int.Parse(keyword.PreviousPosition)))
                                        .Max();

                                    // Filter keywords with the highest difference
                                    var keywordsWithMaxDifference = newData.Where(keyword => Math.Abs(int.Parse(keyword.CurrentPosition) - int.Parse(keyword.PreviousPosition)) == maxDifference)
                                        .ToList().Take(10);

                                    listOfRawData.Add("Keywords ranking data :" + JsonConvert.SerializeObject(keywordsWithMaxDifference));

                                }
                            }
                            catch (Exception)
                            {
                                htmlString = "<p><h3>Something went wrong in keywords.</h3></p>";
                                htmlArray.Add(htmlString);
                            }
                        }
                        else if (type == (int)ReportTypes.LightHouseData)
                        {
                            string htmlString = "";
                            string footerText = string.Empty;
                            string showPageNumberId = string.Empty;
                            string showPageNumber = "hidden";
                            pageNumber = pageNumber + 1;
                            try
                            {
                                var campaign = _campaignRepository.GetAllEntities(true).Where(x => x.Id == campaignId).FirstOrDefault();

                                if (campaign != null)
                                {
                                    string path = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/pageSpeed.html");

                                    using (HttpClient httpclient = new HttpClient())
                                    {
                                        htmlString = httpclient.GetStringAsync(path).Result;
                                    }
                                    dynamic data = JObject.Parse(ReportSchedule.ReportSetting.HeaderSettings);
                                    string companyLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImg).Value.ToString();
                                    string campaignLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImgCamp).Value.ToString();
                                    string headerText = ((Newtonsoft.Json.Linq.JValue)data._headerTextValue).Value.ToString();
                                    string showFooter = ((Newtonsoft.Json.Linq.JValue)data._showFooter).Value.ToString();
                                    string showFooterPageNo = ((Newtonsoft.Json.Linq.JValue)data._showFooterPageNumber).Value.ToString();
                                    string headerTextColor = ((Newtonsoft.Json.Linq.JValue)data._headerTextColor).Value.ToString();
                                    string headerBgColor = ((Newtonsoft.Json.Linq.JValue)data._headerBgColor).Value.ToString();
                                    string showHeader = ((Newtonsoft.Json.Linq.JValue)data._showHeader).Value.ToString();
                                    if (Convert.ToBoolean(showFooter))
                                    {
                                        footerText = ((Newtonsoft.Json.Linq.JValue)data._footerText).Value.ToString();
                                    }
                                    if (Convert.ToBoolean(showFooterPageNo))
                                    {
                                        showPageNumberId = "pageFooter";
                                        showPageNumber = "none";
                                    }

                                    var result = await PrepareLightHouseReports(htmlString, campaign.WebUrl, campaignLogo, companyLogo, headerText, footerText, headerBgColor, headerTextColor, Convert.ToBoolean(showFooter), showPageNumberId, showPageNumber, Convert.ToBoolean(showHeader), pageNumber);
                                    string uniqueKey = $"{type}";
                                    listOfResult.Add(new Dictionary<string, string>()
                                        {
                                            { uniqueKey, result.Html }
                                        });

                                    listOfRawData.Add("Page Speed Data " + JsonConvert.SerializeObject(result.LightHouseData));
                                }
                            }
                            catch (Exception ex)
                            {
                                htmlString = "<p><h3>Something went wrong in light house data.</h3></p>";
                                htmlArray.Add(htmlString);
                            }

                        }
                        else if (type == (int)ReportTypes.GoogleBusinessProfile)
                        {
                            string htmlString = "";
                            string footerText = string.Empty;
                            string showPageNumberId = string.Empty;

                            try
                            {
                                var gbpSetup = _campaignGBPRepository.GetAllEntities(true).Where(x => x.CampaignID == campaignId).FirstOrDefault();

                                if (gbpSetup != null)
                                {
                                    dynamic data = JObject.Parse(ReportSchedule.ReportSetting.HeaderSettings);
                                    string companyLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImg).Value.ToString();
                                    string campaignLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImgCamp).Value.ToString();
                                    string headerText = ((Newtonsoft.Json.Linq.JValue)data._headerTextValue).Value.ToString();
                                    string showFooter = ((Newtonsoft.Json.Linq.JValue)data._showFooter).Value.ToString();
                                    string showFooterPageNo = ((Newtonsoft.Json.Linq.JValue)data._showFooterPageNumber).Value.ToString();
                                    string headerTextColor = ((Newtonsoft.Json.Linq.JValue)data._headerTextColor).Value.ToString();
                                    string headerBgColor = ((Newtonsoft.Json.Linq.JValue)data._headerBgColor).Value.ToString();
                                    string showHeader = ((Newtonsoft.Json.Linq.JValue)data._showHeader).Value.ToString();

                                    //add all subtypes in gbp sub type list
                                    if (subtypes.Count == 0)
                                    {
                                        subtypes.AddRange(new List<string>
                                        {
                                             "41", "42","43","44","45","46","47","48"
                                        });
                                    }

                                    var result = await PrepareGbpReport(gbpSetup.CampaignID, startDateForEmailReport, endDateForEmailReport, subtypes);
                                    listOfResult.AddRange(result.HtmlList);

                                    var gbpRawData = new GbpRawData()
                                    {
                                        BookingDiff = result.RootGbpData.BookingDiff,

                                        ProfileViewDiff = result.RootGbpData.ProfileViewDiff,

                                        SearchKeywordDiff = result.RootGbpData.SearchKeywordDiff,

                                        ProfileInteractionDiff = result.RootGbpData.ProfileInteractionDiff,

                                        CallDiff = result.RootGbpData.CallDiff,

                                        MessageDiff = result.RootGbpData.MessageDiff,

                                        WebsiteDiff = result.RootGbpData.WebsiteDiff,

                                        DirectionDiff = result.RootGbpData.DirectionDiff

                                    };

                                    listOfRawData.Add("Gbp data " + JsonConvert.SerializeObject(gbpRawData));
                                }
                            }
                            catch (Exception ex)
                            {
                                htmlString = "<p><h3>Something went wrong in google business profile.</h3></p>";
                                htmlArray.Add(htmlString);
                            }
                        }
                        else if (type == (int)ReportTypes.WooCommerce)
                        {
                            string htmlString = "";
                            string footerText = string.Empty;
                            string showPageNumberId = string.Empty;

                            try
                            {
                                var wcSetup = _campaignWooCommerceRepository.GetAllEntities(true).Where(x => x.CampaignID == campaignId).FirstOrDefault();

                                if (wcSetup != null)
                                {
                                    string pathWc = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/woocommerce.html");

                                    using (HttpClient httpclient = new HttpClient())
                                    {
                                        htmlString = httpclient.GetStringAsync(pathWc).Result;
                                    }

                                    //string path = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/woocommerce.html");
                                    //htmlString = System.IO.File.ReadAllText(path);

                                    dynamic data = JObject.Parse(ReportSchedule.ReportSetting.HeaderSettings);
                                    string companyLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImg).Value.ToString();
                                    string campaignLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImgCamp).Value.ToString();
                                    string headerText = ((Newtonsoft.Json.Linq.JValue)data._headerTextValue).Value.ToString();
                                    string showFooter = ((Newtonsoft.Json.Linq.JValue)data._showFooter).Value.ToString();
                                    string showFooterPageNo = ((Newtonsoft.Json.Linq.JValue)data._showFooterPageNumber).Value.ToString();
                                    string headerTextColor = ((Newtonsoft.Json.Linq.JValue)data._headerTextColor).Value.ToString();
                                    string headerBgColor = ((Newtonsoft.Json.Linq.JValue)data._headerBgColor).Value.ToString();
                                    string showHeader = ((Newtonsoft.Json.Linq.JValue)data._showHeader).Value.ToString();

                                    //add all subtypes in wc sub type list
                                    if (subtypes.Count == 0)
                                    {
                                        subtypes.AddRange(new List<string>
                                        {
                                           "49", "50","51","52","53","54"
                                        });
                                    }

                                    var result = await PrepareWooCommerceReport(wcSetup.CampaignID, startDateForEmailReport, endDateForEmailReport, subtypes);

                                    listOfResult.AddRange(result.HtmlList);

                                    listOfRawData.Add("Woo commerce data " + JsonConvert.SerializeObject(result.WcRawData));
                                }
                            }
                            catch (Exception ex)
                            {
                                htmlString = "<p><h3>Something went wrong in Woo commerce.</h3></p>";
                                htmlArray.Add(htmlString);
                            }
                        }
                        else if (type == (int)ReportTypes.CallRail)
                        {

                            string htmlString = "";
                            string footerText = string.Empty;
                            string showPageNumberId = string.Empty;

                            try
                            {
                                var callRailSetup = _campaignCallRailRepository.GetAllEntities(true).Where(x => x.CampaignID == campaignId).FirstOrDefault();

                                if (callRailSetup != null)
                                {
                                    dynamic data = JObject.Parse(ReportSchedule.ReportSetting.HeaderSettings);
                                    string companyLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImg).Value.ToString();
                                    string campaignLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImgCamp).Value.ToString();
                                    string headerText = ((Newtonsoft.Json.Linq.JValue)data._headerTextValue).Value.ToString();
                                    string showFooter = ((Newtonsoft.Json.Linq.JValue)data._showFooter).Value.ToString();
                                    string showFooterPageNo = ((Newtonsoft.Json.Linq.JValue)data._showFooterPageNumber).Value.ToString();
                                    string headerTextColor = ((Newtonsoft.Json.Linq.JValue)data._headerTextColor).Value.ToString();
                                    string headerBgColor = ((Newtonsoft.Json.Linq.JValue)data._headerBgColor).Value.ToString();
                                    string showHeader = ((Newtonsoft.Json.Linq.JValue)data._showHeader).Value.ToString();

                                    //add all subtypes in wc sub type list
                                    if (subtypes.Count == 0)
                                    {
                                        subtypes.AddRange(new List<string>
                                        {
                                            "63","64","65","66","67","68","69","70","71","72","73","74","75"
                                        });
                                    }

                                    var result = await PrepareCallRailReport(callRailSetup.CampaignID, startDateForEmailReport.ToString("yyyy-MM-dd"), endDateForEmailReport.ToString("yyyy-MM-dd"), subtypes);

                                    listOfResult.AddRange(result.HtmlList);

                                    listOfRawData.Add("Call Rail Data " + JsonConvert.SerializeObject(result.CrRawData));
                                }
                            }
                            catch (Exception ex)
                            {
                                htmlString = "<p><h3>Something went wrong in call rail.</h3></p>";
                                htmlArray.Add(htmlString);
                            }
                        }
                        else if (type == (int)ReportTypes.Mailchimp)
                        {

                            string htmlString = "";

                            try
                            {
                                var mailchimpSetup = _campaignMailchimpRepository.GetAllEntities(true).Where(x => x.CampaignID == campaignId).FirstOrDefault();

                                if (mailchimpSetup != null)
                                {

                                    //add all subtypes in mc sub type list
                                    if (subtypes.Count == 0)
                                    {
                                        subtypes.AddRange(new List<string>
                                        {
                                            "77","78","79","80","81","83","84","85","86","87"
                                        });
                                    }

                                    var mailchimpSettings = JsonConvert.DeserializeObject<MailchimpSettings>(ReportSchedule.ReportSetting?.MailchimpSettings);

                                    var result = await PrepareMailchimpReport(mailchimpSetup.CampaignID, subtypes, mailchimpSettings, ReportSchedule.ReportSetting?.MailchimpSettings);

                                    listOfResult.AddRange(result.HtmlList);

                                    listOfRawData.Add("Mailchimp all campaigns Data:" + JsonConvert.SerializeObject(result.MailchimpCampaignsRawData) + " Mailchimp lists all data :" + JsonConvert.SerializeObject(result.MailchimpListRawData) +" List of single mailchimp campaign data :" + JsonConvert.SerializeObject(result.SingleCampaignsRawData) + "List of single mailchimp list data :" + JsonConvert.SerializeObject(result.SingleListsRawData));
                                }
                            }
                            catch (Exception ex)
                            {
                                htmlString = "<p><h3>Something went wrong in Mailchimp.</h3></p>";
                                htmlArray.Add(htmlString);
                            }
                        }
                        else if (type == (int)ReportTypes.MicrosoftAds)
                        {

                            string htmlString = "";

                            try
                            {
                                var msAdsSetup = _campaignMicrosoftAdService.GetAllEntities(true).Where(x => x.CampaignID == campaignId).FirstOrDefault();

                                if (msAdsSetup != null)
                                {

                                    //add all subtypes in mc sub type list
                                    if (subtypes.Count == 0)
                                    {
                                        subtypes.AddRange(new List<string>
                                        {
                                            "93", "94", "95", "96", "98", "99", "100", "101", "103", "104", "105", "106" , "108", "109", "110", "111"
                                        });
                                    }

                                    var result = await PrepareMicrosoftAdsReport(msAdsSetup.CampaignID, subtypes, startDateForEmailReport.ToString("yyyy-MM-dd"), endDateForEmailReport.ToString("yyyy-MM-dd"));

                                    listOfResult.AddRange(result.HtmlList);

                                    listOfRawData.Add("Microsoft Ads campaigns data :" + JsonConvert.SerializeObject(result.CampaignPerformace) + "Microsoft Ads ad groups data " + JsonConvert.SerializeObject(result.AdGroupPerformance) +
                                        "Microsoft Ads keywords data :" + JsonConvert.SerializeObject(result.KeywordPerformance) + "Microsoft Ads conversion data " + JsonConvert.SerializeObject(result.ConversionPerformance) +
                                        "Single list campaigns data :" + JsonConvert.SerializeObject(result.SingleCampaignPerformaceList) +
                                        "Single list ad groups data :" + JsonConvert.SerializeObject(result.SingleGroupPerformance) +
                                        "Single list keyword data :" + JsonConvert.SerializeObject(result.SingleKeywordPerformance) +
                                        "Single list conversions data :" + JsonConvert.SerializeObject(result.SingleConversionPerformance));
                                }
                            }
                            catch (Exception ex)
                            {
                                htmlString = "<p><h3>Something went wrong in ms ads.</h3></p>";
                                htmlArray.Add(htmlString);
                            }
                        }
                    }
                }


                string? chatGptContent = string.Empty;

                //ChatGpt
                if (updatedReportTypeList.Where(x=>x == "116").Any())
                {
                    string htmlString = string.Empty;
                    Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                    string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/chatGpt.html");
                    htmlString = System.IO.File.ReadAllText(pathGa);

                    var serializeRawData = "\"\"\"" +  JsonConvert.SerializeObject(listOfRawData) + "\"\"\"";

                    var chatGPTanswer = await _chatGptReportService.GetReportSummary(new Prompts { Content = serializeRawData, DateRange = startDate1 + " to " + endDate1 });

                    chatGPTanswer.Content = chatGPTanswer.Content.Replace("Overall Positive Achievements", "<strong>Overall Positive Achievements</strong>")
                               .Replace("Comparative Success", "<strong>Comparative Success</strong>")
                               .Replace("Performance Achievements", "<strong>Performance Achievements</strong>");


                    htmlString = htmlString.Replace("_innerChatGptData_", chatGPTanswer.Content);

                    htmlArray.Add(htmlString);

                    chatGptContent = chatGPTanswer.Content;
                }

                foreach (var reportType in reportTypeList)
                {
                    var res = ExtractTypeAndSubtypes(reportType);

                    var uniqueKey = string.Empty;

                    if (res.Subtype.Count > 0)
                    {
                        uniqueKey = $"{res.Type}({string.Join(",", res.Subtype)})";
                    }
                    else
                    {
                        uniqueKey = $"{res.Type}";
                    }

                    if (reportType != "19" && reportType != "20" && reportType != "55")
                    {
                        // Check if the listOfResult list contains the uniqueKey
                        foreach (var dictionary in listOfResult)
                        {
                            if (dictionary.ContainsKey(uniqueKey))
                            {
                                // If there is a match, add the corresponding HTML data to the matchingHtmlArray
                                string htmlData = dictionary[uniqueKey];
                                htmlArray.Add(htmlData);
                            }
                        }
                    }
                    else
                    {
                        if (reportType == "19")
                        {
                            string footerText = string.Empty;
                            string showPageNumberId = string.Empty;
                            string showPageNumber = "hidden";
                            dynamic data = JObject.Parse(ReportSchedule.ReportSetting.HeaderSettings);
                            string companyLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImg).Value.ToString();
                            string campaignLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImgCamp).Value.ToString();
                            string headerText = ((Newtonsoft.Json.Linq.JValue)data._headerTextValue).Value.ToString();
                            string showFooter = ((Newtonsoft.Json.Linq.JValue)data._showFooter).Value.ToString();
                            string showFooterPageNo = ((Newtonsoft.Json.Linq.JValue)data._showFooterPageNumber).Value.ToString();
                            string headerTextColor = ((Newtonsoft.Json.Linq.JValue)data._headerTextColor).Value.ToString();
                            string headerBgColor = ((Newtonsoft.Json.Linq.JValue)data._headerBgColor).Value.ToString();
                            string showHeader = ((Newtonsoft.Json.Linq.JValue)data._showHeader).Value.ToString();
                            if (Convert.ToBoolean(showFooter))
                            {
                                footerText = ((Newtonsoft.Json.Linq.JValue)data._footerText).Value.ToString();
                            }
                            if (Convert.ToBoolean(showFooterPageNo))
                            {
                                showPageNumberId = "pageFooter";
                                showPageNumber = "none";
                            }

                            string htmlString = "";
                            var commentList = ReportSchedule.ReportSetting?.Comments.Split("||");

                            //string path = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/text.html");
                            //htmlString =  System.IO.File.ReadAllText(path);


                            string pathText = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/text.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathText).Result;
                            }

                            htmlString = await PrepareTextAndImageHtml(commentList[cmtIndex], htmlString, companyLogo, campaignLogo, headerText, footerText, headerTextColor, headerBgColor, Convert.ToBoolean(showFooter), showPageNumberId, showPageNumber, Convert.ToBoolean(showHeader), pageNumber);

                            htmlArray.Add(htmlString);

                            cmtIndex += 1;
                        }

                        if (reportType == "20")
                        {
                            string footerText = string.Empty;
                            string showPageNumberId = string.Empty;
                            string showPageNumber = "hidden";
                            dynamic data = JObject.Parse(ReportSchedule.ReportSetting.HeaderSettings);
                            string companyLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImg).Value.ToString();
                            string campaignLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImgCamp).Value.ToString();
                            string headerText = ((Newtonsoft.Json.Linq.JValue)data._headerTextValue).Value.ToString();
                            string showFooter = ((Newtonsoft.Json.Linq.JValue)data._showFooter).Value.ToString();
                            string showFooterPageNo = ((Newtonsoft.Json.Linq.JValue)data._showFooterPageNumber).Value.ToString();
                            string headerTextColor = ((Newtonsoft.Json.Linq.JValue)data._headerTextColor).Value.ToString();
                            string headerBgColor = ((Newtonsoft.Json.Linq.JValue)data._headerBgColor).Value.ToString();
                            string showHeader = ((Newtonsoft.Json.Linq.JValue)data._showHeader).Value.ToString();
                            if (Convert.ToBoolean(showFooter))
                            {
                                footerText = ((Newtonsoft.Json.Linq.JValue)data._footerText).Value.ToString();
                            }
                            if (Convert.ToBoolean(showFooterPageNo))
                            {
                                showPageNumberId = "pageFooter";
                                showPageNumber = "none";
                            }

                            string htmlString = "";
                            var imageList = ReportSchedule.ReportSetting?.Images.Split("||");
                            var imgObj = JsonConvert.DeserializeObject<ReportImageDto>(imageList[imgIndex]);

                            string path = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/image.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(path).Result;
                            }

                            //string path = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/image.html");
                            //htmlString =  System.IO.File.ReadAllText(path) ;


                            string dataImg = "<img src='" + imgObj.src + "' style=\"height:" + imgObj.height + "px; width:" + imgObj.width + "px;\"/> ";
                            htmlString = await PrepareTextAndImageHtml(dataImg, htmlString, companyLogo, campaignLogo, headerText, footerText, headerTextColor, headerBgColor, Convert.ToBoolean(showFooter), showPageNumberId, showPageNumber, Convert.ToBoolean(showHeader), pageNumber);

                            htmlArray.Add(htmlString);

                            imgIndex += 1;
                        }

                        if (reportType == "55")
                        {
                            string footerText = string.Empty;
                            string showPageNumberId = string.Empty;
                            string showPageNumber = "hidden";
                            dynamic data = JObject.Parse(ReportSchedule.ReportSetting.HeaderSettings);
                            string companyLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImg).Value.ToString();
                            string campaignLogo = ((Newtonsoft.Json.Linq.JValue)data._headerLableImgCamp).Value.ToString();
                            string headerText = ((Newtonsoft.Json.Linq.JValue)data._headerTextValue).Value.ToString();
                            string showFooter = ((Newtonsoft.Json.Linq.JValue)data._showFooter).Value.ToString();
                            string showFooterPageNo = ((Newtonsoft.Json.Linq.JValue)data._showFooterPageNumber).Value.ToString();
                            string headerTextColor = ((Newtonsoft.Json.Linq.JValue)data._headerTextColor).Value.ToString();
                            string headerBgColor = ((Newtonsoft.Json.Linq.JValue)data._headerBgColor).Value.ToString();
                            string showHeader = ((Newtonsoft.Json.Linq.JValue)data._showHeader).Value.ToString();
                            if (Convert.ToBoolean(showFooter))
                            {
                                footerText = ((Newtonsoft.Json.Linq.JValue)data._footerText).Value.ToString();
                            }
                            if (Convert.ToBoolean(showFooterPageNo))
                            {
                                showPageNumberId = "pageFooter";
                                showPageNumber = "none";
                            }

                            var googleSheetReportList = JsonConvert.DeserializeObject<List<GoogleSheetSettingsDto>>(ReportSchedule.ReportSetting?.GoogleSheetSettings);

                            var googleSheetReport = await PrepareGoogleSheetReport(googleSheetReportList);

                            htmlArray.AddRange(googleSheetReport);
                        }
                    }
                }

                var customFont = string.Empty;

                for (var i = 0; i < htmlArray.Count; i++)
                {
                    htmlArray[i] = htmlArray[i].Replace("_companyName_", companyName[0].ToString());
                    htmlArray[i] = htmlArray[i].Replace("_campaignName_", campaignName[0].ToString());

                    var startDate = startDateForEmailReport.ToString("ddd, MMMM dd yyyy");
                    var endDate = endDateForEmailReport.ToString("ddd, MMMM dd yyyy");
                    htmlArray[i] = htmlArray[i].Replace("_reportDateRange_", startDate + " - " + endDate);

                    // theme setting
                    dynamic themeData = JObject.Parse(ReportSchedule.ReportSetting.HeaderSettings);
                    string bgClr = ((Newtonsoft.Json.Linq.JValue)themeData._themeBgColor).Value.ToString();
                    string txtClr = ((Newtonsoft.Json.Linq.JValue)themeData._themeTextColor).Value.ToString();
                    string txtFont = ((Newtonsoft.Json.Linq.JValue)themeData._font).Value.ToString(); ;
                    string themeBgColor = String.IsNullOrEmpty(bgClr) ? "#3445f6" : bgClr;
                    string themeTextColor = String.IsNullOrEmpty(txtClr) ? "#ffffff" : txtClr;
                    customFont = String.IsNullOrEmpty(txtFont) ? "Montserrat" : txtFont;

                    htmlArray[i] = htmlArray[i].Replace("_themeBgColor_", themeBgColor);
                    htmlArray[i] = htmlArray[i].Replace("_themeTextColor_", themeTextColor);
                    htmlArray[i] = htmlArray[i].Replace("_font_", customFont);

                }

                var mergedHtml = string.Join(" ", htmlArray);

                dynamic data1 = JObject.Parse(ReportSchedule.ReportSetting.HeaderSettings);
                string headerText1 = ((Newtonsoft.Json.Linq.JValue)data1._headerTextValue).Value.ToString();
                string headerTextColor1 = ((Newtonsoft.Json.Linq.JValue)data1._headerTextColor).Value.ToString();
                string headerBgColor1 = ((Newtonsoft.Json.Linq.JValue)data1._headerBgColor).Value.ToString();


                //Prepare Header and Footer            
                var header = "<style>html { -webkit-print-color-adjust: exact;}</style><div style=\"margin-top: -15px;font-size:8px;height: 100%; width:100%; padding: 0px 20px; display: flex; flex-direction: row; align-items: center; justify-content: space-between;background-color: " + headerBgColor1 + "; color: " + headerTextColor1 + "; height: 25px; font-family: " + customFont + ";\"><span>" + campaignName[0].ToString() + "</span><span>" + startDate1 + " - " + endDate1 + "</span></div>";
                var footer = "<div style=\"margin-bottom: -15px;font-size: 8px; height: 25px; width: 100%; padding: 0px 20px; display: flex; align-items: center; justify-content: center; background-color: " + headerBgColor1 + "; color: " + headerTextColor1 + ";font-family:" + customFont + ";\"><span>Prepared by " + companyName[0].ToString() + "</span></div>";

                var options1 = new ChromeHtmlToPdfOptions
                {
                    Delay = 3000,
                    DisplayHeaderFooter = true,
                    Landscape = false,
                    MarginBottom = ".6in",
                    MarginTop = ".6in",
                    Width = "8.27in",
                    Height = "11.69in",
                    HeaderTemplate = header,
                    FooterTemplate = footer
                };

                var request1 = new ChromeHtmlToPdfRequest
                {
                    Html = mergedHtml,
                    FileName = "sample.pdf",
                    Inline = true,
                    Options = options1,
                };
                var apiResponse1 = a2pClient.Chrome.HtmlToPdf(request1);

                var mergeResultAsBytes = apiResponse1.GetFileBytes();

                DateTime epoch1 = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();
                TimeSpan SendAt = (ReportSchedule.ScheduleDateAndTime - epoch1);

                var fromWhom = string.Empty;
                var emailWhiteLabel = _emailWhitelabelRepository.GetFilteredEntities().Where(x => x.CompanyID == companyId && x.IsVerify == true).FirstOrDefault();
                if (emailWhiteLabel != null)
                {
                    fromWhom = "no-reply@" + emailWhiteLabel.DomainName;
                }
                else
                {
                    fromWhom = _configuration.GetSection("MailFrom").Value;
                }

                await SendReportInEmail(ReportSchedule.Subject, ReportSchedule.EmaildIds, startDateForEmailReport, endDateForEmailReport, mergeResultAsBytes, long.Parse(SendAt.TotalSeconds.ToString()), fromWhom, companyId, ReportSchedule.HtmlHeader, ReportSchedule.HtmlFooter, chatGptContent);

                var scheduleData = _reportschedulingRepository.GetEntityById(ReportSchedule.Id);

                if (ReportSchedule.Scheduled == ReportScheduleType.Daily)
                {
                    scheduleData.ScheduleDateAndTime = ReportSchedule.ScheduleDateAndTime.AddDays(1);
                }
                else if (ReportSchedule.Scheduled == ReportScheduleType.Weekly)
                {
                    scheduleData.ScheduleDateAndTime = ReportSchedule.ScheduleDateAndTime.AddDays(7);
                }
                else if (ReportSchedule.Scheduled == ReportScheduleType.Monthly)
                {
                    scheduleData.ScheduleDateAndTime = ReportSchedule.ScheduleDateAndTime.AddMonths(1);
                }

                scheduleData.UpdatedOn = DateTime.UtcNow;

                _reportschedulingRepository.UpdateEntity(scheduleData);
                _reportschedulingRepository.SaveChanges();

            }

            //Send email after execution
            var client = new SendGridClient(_configuration.GetSection("Client").Value);

            var notificationEmailsList = new List<EmailAddress>();
            var notificationEmails = _configuration.GetSection("NotificationEmails").Value;
            if (!string.IsNullOrEmpty(notificationEmails))
            {
                foreach (var email in notificationEmails.Split(','))
                {
                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        notificationEmailsList.Add(new EmailAddress(email.Trim()));
                    }
                }
            }

            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(new EmailAddress(_configuration.GetSection("MailFrom").Value), notificationEmailsList,
            "Successfully: Lambda Execution For Report Scheduling", "", "Execution Successfuly For Report Scheduling");

            var response = client.SendEmailAsync(msg);

            retval = true;

            return retval;
        }

        private async Task<List<string>> PrepareGoogleSheetReport(List<GoogleSheetSettingsDto> googleSheetReportList)
        {
            var retVal = new List<string>();

            var gsDataList = await _campaignGoogleSheetService.GetGoogleSheetReport(googleSheetReportList);

            foreach (var googleSheetData in gsDataList)
            {
                var htmlString = string.Empty;

                switch (googleSheetData.ReportSubType)
                {
                    //Pie Chart
                    case 56:

                        string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/googleSheetPie.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(pathPie).Result;
                        }

                        htmlString = htmlString.Replace("_gsChartId_", DateTimeOffset.Now.Ticks.ToString());

                        List<string> randomColors = GenerateRandomColors(googleSheetData.XAxis.Count);


                        if (googleSheetData.XAxis != null && googleSheetData.XAxis.Count() > 0)
                        {
                            // Create a new list by selecting elements from the original list based on the count
                            var selectedColors = string.Join(",", randomColors.Select(x => "'" + x + "'"));
                            htmlString = htmlString.Replace("_gsBgColorPieChart_", selectedColors);
                        }

                        var labelsAndData = googleSheetData.XAxis.Zip(googleSheetData.YAxis, (label, data) => $"'{label}: {data}'");

                        var labels = string.Join(",", labelsAndData);

                        var data = string.Join(",", googleSheetData.YAxis != null ? googleSheetData.YAxis : new List<decimal?>() { });

                        htmlString = htmlString.Replace("_gsPieChartData_", data);
                        htmlString = htmlString.Replace("_gsPieChartLabel_", labels);
                        htmlString = htmlString.Replace("_gsTitle_", !string.IsNullOrEmpty(googleSheetData.Title) ? googleSheetData.Title : "Pie Chart");

                        if (googleSheetData.Aggregator.ToUpper() == "SUM")
                        {
                            htmlString = htmlString.Replace("_gsAggeragatorValue_", "Sum: " + googleSheetData.AggregationData);
                        }
                        else
                        {
                            htmlString = htmlString.Replace("_gsAggeragatorValue_", "Avg: " + googleSheetData.AggregationData);
                        }

                        retVal.Add(htmlString);

                        break;

                    //bar
                    case 57:

                        string pathBar = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/googleSheetBar.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(pathBar).Result;
                        }

                        htmlString = htmlString.Replace("_gsChartId_", DateTimeOffset.Now.Ticks.ToString());

                        var barLabels = googleSheetData.XAxis.Select(x => "'" + x + "'");

                        var labels1 = string.Join(",", barLabels);

                        var data1 = string.Join(",", googleSheetData.YAxis != null ? googleSheetData.YAxis : new List<decimal?>() { });

                        htmlString = htmlString.Replace("_gsBarData1_", data1);
                        htmlString = htmlString.Replace("_gsBarLabels_", labels1);
                        htmlString = htmlString.Replace("_gsBarChartTitle_", !string.IsNullOrEmpty(googleSheetData.Title) ? googleSheetData.Title : "Bar Chart");

                        if (googleSheetData.IsComparePrevious)
                        {

                            var prevBarData = string.Join(",", googleSheetData.PrevYAxis != null ? googleSheetData.PrevYAxis : new List<decimal?>() { });

                            htmlString = htmlString.Replace("_gsBarData2_", prevBarData);

                            htmlString = htmlString.Replace("_gsBarData2_", "");
                            if (googleSheetData.Aggregator.ToUpper() == "SUM")
                            {
                                htmlString = htmlString.Replace("_gsBarAggeragatorValue_", "Sum: " + googleSheetData.DiffAggregator);
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_gsBarAggeragatorValue_", "Avg: " + googleSheetData.DiffAggregator);
                            }

                            htmlString = htmlString.Replace("_IsComparePrevious_", "false");
                        }
                        else
                        {
                            htmlString = htmlString.Replace("_gsBarData2_", "");
                            if (googleSheetData.Aggregator.ToUpper() == "SUM")
                            {
                                htmlString = htmlString.Replace("_gsBarAggeragatorValue_", "Sum: " + googleSheetData.AggregationData);
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_gsBarAggeragatorValue_", "Avg: " + googleSheetData.AggregationData);
                            }

                            htmlString = htmlString.Replace("_IsComparePrevious_", "true");
                        }

                        retVal.Add(htmlString);
                        break;

                    //line
                    case 58:

                        string pathLine = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/googleSheetLine.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(pathLine).Result;
                        }

                        htmlString = htmlString.Replace("_gsChartId_", DateTimeOffset.Now.Ticks.ToString());

                        var lineLabels = googleSheetData.XAxis.Select(x => "'" + x + "'").ToList();

                        var labels2 = string.Join(",", lineLabels);

                        var data2 = string.Join(",", googleSheetData.YAxis != null ? googleSheetData.YAxis : new List<decimal?>() { });

                        int intervalRes = lineLabels.Count() <= 31 ? 3 : (lineLabels.Count() <= 91 ? 7 : 31);
                        htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

                        htmlString = htmlString.Replace("_gsLineData1_", data2);
                        htmlString = htmlString.Replace("_gsLineLabels_", labels2);
                        htmlString = htmlString.Replace("_gsLineChartTitle_", !string.IsNullOrEmpty(googleSheetData.Title) ? googleSheetData.Title : "Line Chart");

                        if (googleSheetData.IsComparePrevious)
                        {

                            var prevData = string.Join(",", googleSheetData.PrevYAxis != null ? googleSheetData.PrevYAxis : new List<decimal?>() { });

                            htmlString = htmlString.Replace("_gsLineData2_", prevData);

                            htmlString = htmlString.Replace("_gsLineData2_", "");
                            if (googleSheetData.Aggregator.ToUpper() == "SUM")
                            {
                                htmlString = htmlString.Replace("_gsLineAggeragatorValue_", "Sum: " + googleSheetData.DiffAggregator);
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_gsLineAggeragatorValue_", "Avg: " + googleSheetData.DiffAggregator);
                            }

                            htmlString = htmlString.Replace("_IsComparePrevious_", "false");
                        }
                        else
                        {
                            htmlString = htmlString.Replace("_gsLineData2_", "");
                            if (googleSheetData.Aggregator.ToUpper() == "SUM")
                            {
                                htmlString = htmlString.Replace("_gsLineAggeragatorValue_", "Sum: " + googleSheetData.AggregationData);
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_gsLineAggeragatorValue_", "Avg: " + googleSheetData.AggregationData);
                            }

                            htmlString = htmlString.Replace("_IsComparePrevious_", "true");
                        }

                        retVal.Add(htmlString);
                        break;

                    //Table
                    case 59:

                        string pathTable = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/googleSheetTable.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(pathTable).Result;
                        }

                        var tableData = googleSheetData.TableData;

                        var tableString = JsonConvert.SerializeObject(tableData);

                        htmlString = htmlString.Replace("_gsDataTable1_", tableString);

                        htmlString = htmlString.Replace("_gsTableTitle_", googleSheetData.Title);

                        retVal.Add(htmlString);

                        break;

                    //Stat Cell Value
                    case 60:

                        string pathCell = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/googleSheetCell.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(pathCell).Result;
                        }

                        var cellData = googleSheetData.CellData;

                        htmlString = htmlString.Replace("_gsCellData_", cellData);

                        htmlString = htmlString.Replace("_gsCellTitle_", googleSheetData.Title);

                        retVal.Add(htmlString);

                        break;

                    //SparkLine Chart
                    case 61:

                        string pathSpark = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/googleSheetSpark.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(pathSpark).Result;
                        }

                        htmlString = htmlString.Replace("_gsChartId_", DateTimeOffset.Now.Ticks.ToString());

                        var sparkLabels = googleSheetData.XAxis.Select(x => "'" + x + "'");

                        var sparkLabelsString = string.Join(",", sparkLabels);

                        var sparkData = string.Join(",", googleSheetData.YAxis != null ? googleSheetData.YAxis : new List<decimal?>() { });

                        htmlString = htmlString.Replace("_gsSparkData1_", sparkData);
                        htmlString = htmlString.Replace("_gsSparkLabels_", sparkLabelsString);
                        htmlString = htmlString.Replace("_gsSparkChartTitle_", !string.IsNullOrEmpty(googleSheetData.Title) ? googleSheetData.Title : "Spark Line Chart");

                        if (googleSheetData.IsComparePrevious)
                        {

                            var prevData = string.Join(",", googleSheetData.PrevYAxis != null ? googleSheetData.PrevYAxis : new List<decimal?>() { });

                            htmlString = htmlString.Replace("_gsSparkData2_", prevData);

                            htmlString = htmlString.Replace("_gsSparkData2_", "");
                            if (googleSheetData.Aggregator.ToUpper() == "SUM")
                            {
                                htmlString = htmlString.Replace("_gsSparkAggeragatorValue_", "Sum: " + googleSheetData.DiffAggregator);
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_gsSparkAggeragatorValue_", "Avg: " + googleSheetData.DiffAggregator);
                            }

                            htmlString = htmlString.Replace("_IsComparePrevious_", "false");
                        }
                        else
                        {
                            htmlString = htmlString.Replace("_gsSparkData2_", "");
                            if (googleSheetData.Aggregator.ToUpper() == "SUM")
                            {
                                htmlString = htmlString.Replace("_gsSparkAggeragatorValue_", "Sum: " + googleSheetData.AggregationData);
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_gsSparkAggeragatorValue_", "Avg: " + googleSheetData.AggregationData);
                            }

                            htmlString = htmlString.Replace("_IsComparePrevious_", "true");
                        }

                        retVal.Add(htmlString);
                        break;

                    default:

                        break;
                }
            }

            return retVal;
        }


        static List<string> GenerateRandomColors(int numberOfColors)
        {
            List<string> colorShades = new List<string>
            {
                 "#FFEB3B", "#FFC107", "#FF9800", "#FF5722", "#E91E63",
            "#9C27B0", "#673AB7", "#3F51B5", "#2196F3", "#03A9F4",
            "#00BCD4", "#009688", "#4CAF50", "#8BC34A", "#CDDC39",
            "#FFEB3B", "#FFC107", "#FF9800", "#FF5722", "#E91E63",
            "#9C27B0", "#673AB7", "#3F51B5", "#2196F3", "#03A9F4",
            "#00BCD4", "#009688", "#4CAF50", "#8BC34A", "#CDDC39",
            "#FFEB3B", "#FFC107", "#FF9800", "#FF5722", "#E91E63",
            "#9C27B0", "#673AB7", "#3F51B5", "#2196F3", "#03A9F4",
            "#00BCD4", "#009688", "#4CAF50", "#8BC34A", "#CDDC39",
            "#FFEB3B", "#FFC107", "#FF9800", "#FF5722"
            };

            List<string> randomColors = new List<string>();
            HashSet<string> usedColors = new HashSet<string>();
            Random random = new Random();

            while (randomColors.Count < numberOfColors)
            {
                // Pick a random color shade from the list
                string randomShade = colorShades[random.Next(colorShades.Count)];

                // If the color is not already used, add it to the result and the set of used colors
                if (usedColors.Add(randomShade))
                {
                    randomColors.Add(randomShade);
                }
            }

            return randomColors;
        }

        private async Task<PrepareLightHouseData> PrepareLightHouseReports(string htmlString, string url, string campaignLogo, string companyLogo, string headerText, string footerText, string headerBgColor, string headerTextColor, bool showFooter, string showPageNumberId, string showPageNumber, bool showHeader, int pageNumber)
        {
            var prepareLightHouseData = new PrepareLightHouseData();
            prepareLightHouseData.LightHouseData = new PageSpeedData();


            var mobileData = await PreparePageSpeedLighthouseByStrategy(url, "MOBILE");
            var desktopData = await PreparePageSpeedLighthouseByStrategy(url, "DESKTOP");

            htmlString = htmlString.Replace("_headerImage1_", companyLogo);
            htmlString = htmlString.Replace("_headerImage_", campaignLogo);
            htmlString = htmlString.Replace("_headerText_", headerText);
            htmlString = htmlString.Replace("_mperformanceScoreMobile", mobileData);
            htmlString = htmlString.Replace("_mperformanceScoreDesktop", desktopData);
            htmlString = htmlString.Replace("_footerText_", footerText);
            htmlString = htmlString.Replace("_headerTextColor_", headerTextColor);
            htmlString = htmlString.Replace("_headerBgColor_", headerBgColor);
            htmlString = htmlString.Replace("_showPageNumber_", showPageNumber);
            htmlString = htmlString.Replace("_pageNumberId_", showPageNumberId);

            htmlString = htmlString.Replace("_pageNumber_", "Page " + pageNumber.ToString());
            if (showHeader)
            {
                htmlString = htmlString.Replace("_showHeader_", "none");
            }
            else
            {
                htmlString = htmlString.Replace("_showHeader_", "hidden");
            }

            if (!showFooter)
            {
                htmlString = htmlString.Replace("_showFooter_", "hidden");
            }
            else
            {
                htmlString = htmlString.Replace("_showFooter_", "none");
            }
            if (campaignLogo == "")
            {
                htmlString = htmlString.Replace("_showImg2_", "hidden");
            }
            else
            {
                htmlString = htmlString.Replace("_showImg2_", "none");
            }

            if (companyLogo == "")
            {
                htmlString = htmlString.Replace("_showImg1_", "hidden");
            }
            else
            {
                htmlString = htmlString.Replace("_showImg1_", "none");
            }

            prepareLightHouseData.Html = htmlString;

            prepareLightHouseData.LightHouseData.Mobile = mobileData;

            prepareLightHouseData.LightHouseData.Desktop = desktopData;

            return prepareLightHouseData;
        }



        private async Task<string> PrepareCoverPageHtml(string reportName, string startDate, string endDate, string htmlString, string companyLogo, string campaignLogo, string companyName, string coverPageTextColor, string coverPageBgColor, string coverPageBgImage, string campaignName)
        {
            htmlString = htmlString.Replace("_reportName_", reportName);
            htmlString = htmlString.Replace("_reportDateRange_", startDate + " - " + endDate);
            htmlString = htmlString.Replace("_headerImage_", campaignLogo);
            htmlString = htmlString.Replace("_headerImage1_", companyLogo);
            htmlString = htmlString.Replace("_companyName_", companyName);
            htmlString = htmlString.Replace("_campaignName_", campaignName);
            htmlString = htmlString.Replace("_textColor_", coverPageTextColor);
            htmlString = htmlString.Replace("_bgImage_", coverPageBgImage);
            htmlString = htmlString.Replace("_bgColor_", coverPageBgColor);
            if (campaignLogo == "")
            {
                htmlString = htmlString.Replace("_showImg1_", "hidden");
            }
            else
            {
                htmlString = htmlString.Replace("_showImg1_", "none");
            }

            if (companyLogo == "")
            {
                htmlString = htmlString.Replace("_showImg2_", "hidden");
            }
            else
            {
                htmlString = htmlString.Replace("_showImg2_", "none");
            }
            return htmlString;
        }

        private async Task<string> PrepareTableOfContentHtml(List<string> tocList, string htmlString, string companyLogo, string campaignLogo, string headerText, string footerText, string headerTextColor, string headerBgColor, bool showFooter, string showPageNumberId, string showPageNumber, bool showHeader, int pageNumber, bool oldIndexExist)
        {
            string tocStr = string.Join(",", tocList.Select(x => "'" + x + "'"));

            htmlString = htmlString.Replace("_oldIndexExist_", oldIndexExist.ToString().ToLower());

            htmlString = htmlString.Replace("_listOfTableOfContent_", tocStr);

            htmlString = htmlString.Replace("_headerImage1_", companyLogo);
            htmlString = htmlString.Replace("_headerImage_", campaignLogo);
            htmlString = htmlString.Replace("_headerText_", headerText);
            htmlString = htmlString.Replace("_footerText_", footerText);
            htmlString = htmlString.Replace("_headerTextColor_", headerTextColor);
            htmlString = htmlString.Replace("_headerBgColor_", headerBgColor);
            htmlString = htmlString.Replace("_showPageNumber_", showPageNumber);
            htmlString = htmlString.Replace("_pageNumberId_", showPageNumberId);
            htmlString = htmlString.Replace("_pageNumber_", "Page " + pageNumber.ToString());
            if (campaignLogo == "")
            {
                htmlString = htmlString.Replace("_showImg2_", "hidden");
            }
            else
            {
                htmlString = htmlString.Replace("_showImg2_", "none");
            }

            if (companyLogo == "")
            {
                htmlString = htmlString.Replace("_showImg1_", "hidden");
            }
            else
            {
                htmlString = htmlString.Replace("_showImg1_", "none");
            }

            if (showHeader)
            {
                htmlString = htmlString.Replace("_showHeader_", "none");
            }
            else
            {
                htmlString = htmlString.Replace("_showHeader_", "hidden");
            }

            if (!showFooter)
            {
                htmlString = htmlString.Replace("_showFooter_", "hidden");
            }
            else
            {
                htmlString = htmlString.Replace("_showFooter_", "none");
            }


            return htmlString;
        }

        private async Task<string> PrepareTextAndImageHtml(string data, string htmlString, string companyLogo, string campaignLogo, string headerText, string footerText, string headerTextColor, string headerBgColor, bool showFooter, string showPageNumberId, string showPageNumber, bool showHeader, int pageNumber)
        {
            htmlString = htmlString.Replace("_innerData", data);
            htmlString = htmlString.Replace("_headerImage1_", companyLogo);
            htmlString = htmlString.Replace("_headerImage_", campaignLogo);
            htmlString = htmlString.Replace("_headerText_", headerText);
            htmlString = htmlString.Replace("_footerText_", footerText);
            htmlString = htmlString.Replace("_headerTextColor_", headerTextColor);
            htmlString = htmlString.Replace("_headerBgColor_", headerBgColor);
            htmlString = htmlString.Replace("_showPageNumber_", showPageNumber);
            htmlString = htmlString.Replace("_pageNumberId_", showPageNumberId);
            htmlString = htmlString.Replace("_pageNumber_", "Page " + pageNumber.ToString());
            if (campaignLogo == "")
            {
                htmlString = htmlString.Replace("_showImg2_", "hidden");
            }
            else
            {
                htmlString = htmlString.Replace("_showImg2_", "none");
            }

            if (companyLogo == "")
            {
                htmlString = htmlString.Replace("_showImg1_", "hidden");
            }
            else
            {
                htmlString = htmlString.Replace("_showImg1_", "none");
            }

            if (showHeader)
            {
                htmlString = htmlString.Replace("_showHeader_", "none");
            }
            else
            {
                htmlString = htmlString.Replace("_showHeader_", "hidden");
            }

            if (!showFooter)
            {
                htmlString = htmlString.Replace("_showFooter_", "hidden");
            }
            else
            {
                htmlString = htmlString.Replace("_showFooter_", "none");
            }


            return htmlString;
        }

        /// <summary>
        /// Prepare html for sending reports in pdf format
        /// </summary>
        /// <param name="facebookReportData">facebook Report Data</param>
        /// <param name="htmlString">htmlString</param>
        /// <returns>html string</returns>
        private List<Dictionary<string, string>> PrepareFacebookHtml(FacebookData facebookReportData, List<string> subTypes)
        {
            List<Dictionary<string, string>> listOfResult = new List<Dictionary<string, string>>();
            List<string> reachPieChartData = new List<string> { facebookReportData.OrganicReach.ToString(), facebookReportData.PaidReach.ToString() };
            var reachPiechartDataStr = String.Join(",", reachPieChartData);
            var countryLabelStr = String.Join(",", facebookReportData.CountryLabelStr.Select(x => "'" + x + "'"));
            var countryDataStr = String.Join(",", facebookReportData.CountryDataStr);

            List<string> likePieData = new List<string> { facebookReportData.PercentOrganicLike.ToString(), facebookReportData.PercentPaidLike.ToString() };
            var likePieDataStr = String.Join(",", likePieData);

            foreach (var subtype in subTypes)
            {
                var intsubtype = Convert.ToInt16(subtype);
                if (intsubtype == (int)ReportTypes.FbImpression)
                {
                    Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                    var htmlString = string.Empty;

                    //string path1 = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/fbImpression.html");
                    //htmlString = System.IO.File.ReadAllText(path1);


                    string path1 = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/fbImpression.html");
                    using (HttpClient httpclient = new HttpClient())
                    {
                        htmlString = httpclient.GetStringAsync(path1).Result;
                    }

                    htmlString = htmlString.Replace("_1fbPageImpressionMainTotal1_", facebookReportData.PageImpressionsTotal.ToString());
                    htmlString = htmlString.Replace("_1fbPageImpressionDiff1_", facebookReportData.PercentPageImpression.ToString());
                    htmlString = htmlString.Replace("_1fbPageImpressionsAvgTotal1_", facebookReportData.AvgPageImpression.ToString());

                    htmlString = htmlString.Replace("_1fbTopCountForCountries", facebookReportData.TopCountForCity.ToString());

                    htmlString = htmlString.Replace("_1fbPageReachMainTotal1_", facebookReportData.PageReachTotal.ToString());
                    htmlString = htmlString.Replace("_1fbPageReachDiff1_", facebookReportData.PercentPageReach.ToString());
                    htmlString = htmlString.Replace("_1fbPageReachAvgTotal1_", facebookReportData.AvgPageReach.ToString());

                    htmlString = htmlString.Replace("_1fbPieChartReachData1_", reachPiechartDataStr);

                    htmlString = htmlString.Replace("_1fbPieChartCountriesLabel1_", countryLabelStr);
                    htmlString = htmlString.Replace("_1fbPieChartCountriesData1_", countryDataStr);

                    string uniqueKey = $"{4}({string.Join(",", subtype)})";
                    uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                    listOfResult.Add(uniqueTypeSubtypeResults);
                }
                else if (intsubtype == (int)ReportTypes.FbPerformance)
                {
                    Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                    var htmlString = string.Empty;

                    //string path2 = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/fbPerformance.html");
                    //htmlString = System.IO.File.ReadAllText(path2);


                    string path2 = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/fbPerformance.html");
                    using (HttpClient httpclient = new HttpClient())
                    {
                        htmlString = httpclient.GetStringAsync(path2).Result;
                    }

                    htmlString = htmlString.Replace("_1fbPageProfileMainTotal1_", facebookReportData.ProfileViewTotal.ToString());
                    htmlString = htmlString.Replace("_1fbPageProfileDiff1_", facebookReportData.PercentProfileView.ToString());
                    htmlString = htmlString.Replace("_1fbPageProfileAvgTotal1_", facebookReportData.AvgPageProfileView.ToString());

                    htmlString = htmlString.Replace("_1fbPageLikesMainTotal1_", facebookReportData.TotalPageLike.ToString());

                    htmlString = htmlString.Replace("_1fbPageNewLikesMainTotal1_", facebookReportData.TotalNewLike.ToString());
                    htmlString = htmlString.Replace("_1fbPageNewLikesAvgTotal1_", facebookReportData.AvgPerDayLike.ToString());
                    //htmlString = htmlString.Replace("_avgPerDayLike_", facebookReportData.AvgPerDayLike.ToString());

                    htmlString = htmlString.Replace("_1fbPieChartLikesData1_", likePieDataStr);
                    string uniqueKey = $"{4}({string.Join(",", subtype)})";
                    uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                    listOfResult.Add(uniqueTypeSubtypeResults);
                }
            }


            return listOfResult;
        }

        /// <summary>
        /// Get Publisher Platform Data
        /// </summary>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <param name="facebookAdsAccountID">facebookAdsAccountID</param>
        /// <param name="accessToken">accessToken</param>        
        /// <returns>PublisherPlatformResponse</returns>
        private async Task<PublisherPlatformResponse> GetPublisherPlatformData(string startDate, string endDate, string facebookAdsAccountID, string accessToken)
        {
            PublisherPlatformResponse retVal = null;

            try
            {
                var httpClient = new HttpClient
                {
                    BaseAddress = new Uri("https://graph.facebook.com/v22.0/")
                };

                var url = facebookAdsAccountID + "/insights?level=campaign&breakdowns=publisher_platform&fields=clicks&access_token=" + accessToken + "&time_range={'since':" + "'" + startDate + "'" + ",'until':" + "'" + endDate + "'}";

                var response = await httpClient.GetAsync(url);

                var data = await response.Content.ReadAsStringAsync();

                var fbAdsCampaignsDetails = JsonConvert.DeserializeObject<PublisherPlatformData>(data);

                var instagramClicks = fbAdsCampaignsDetails.data.Where(x => x.publisher_platform == "instagram").Select(x => Int32.Parse(x.clicks)).Sum();

                var facebookClicks = fbAdsCampaignsDetails.data.Where(x => x.publisher_platform == "facebook").Select(x => Int32.Parse(x.clicks)).Sum();

                var audienceNetworkClick = fbAdsCampaignsDetails.data.Where(x => x.publisher_platform == "audience_network").Select(x => Int32.Parse(x.clicks)).Sum();
                retVal = new PublisherPlatformResponse();
                retVal.InstagramClicks = instagramClicks;
                retVal.FacebookClicks = facebookClicks;
                retVal.AudienceNetworkClick = audienceNetworkClick;
                return retVal;

            }
            catch (Exception)
            {
                return retVal;
            }

        }

        /// <summary>
        /// Get FB Ads Campaigns Details With EachDate
        /// </summary>
        /// <param name="facebookAdsAccountID">facebookAdsAccountID</param>
        /// <param name="accessToken">accessToken</param>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <returns>status of the operation</returns>
        private async Task<CampaignChartData> GetFBAdsCampaignsDetailsWithEachDate(string facebookAdsAccountID, string accessToken, string startDate, string endDate, List<AdsetConfig> listConfig)
        {
            CampaignChartData retVal = new CampaignChartData();
            List<InsightsField> listInsights = new List<InsightsField>();

            try
            {
                var httpClient = new HttpClient
                {
                    BaseAddress = new Uri("https://graph.facebook.com/v22.0/")
                };
                var url = facebookAdsAccountID + "/insights?level=adset&fields=inline_link_clicks,cost_per_unique_inline_link_click,reach,impressions,ctr,cpc,actions,campaign_id,adset_id,account_id,video_15_sec_watched_actions,spend,estimated_ad_recallers,cost_per_action_type&access_token=" + accessToken + "&time_range={'since':" + "'" + startDate + "'" + ",'until':" + "'" + endDate + "'}";

                var response = await httpClient.GetAsync(url);

                var data = await response.Content.ReadAsStringAsync();

                var fbAdsCampaignsDetails = JsonConvert.DeserializeObject<FbAdsCampaignsDetails>(data);

                listInsights.Add(fbAdsCampaignsDetails.data[0]);

                //listInsights = await CalculateResult(listInsights, listConfig);

                if (fbAdsCampaignsDetails.data.Count > 1)
                {
                    //retVal.click = fbAdsCampaignsDetails.data.Select(x => Int32.Parse(x.clicks)).Sum();
                    retVal.reach = fbAdsCampaignsDetails.data.Select(x => Int32.Parse(x.reach)).Sum();
                    retVal.impression = fbAdsCampaignsDetails.data.Select(x => Int32.Parse(x.impressions)).Sum();
                    retVal.result = listInsights.Where(x => x.results != null).Select(x => Int32.Parse(x.results)).Sum();
                    retVal.costPerResult = listInsights.Where(x => x.cpr != null).Select(x => Int32.Parse(x.cpr)).Sum();
                    retVal.spend = listInsights.Where(x => x.spend != null).Select(x => Int32.Parse(x.spend)).Sum();
                    retVal.link = listInsights.Where(x => x.inline_link_clicks != null).Select(x => Int32.Parse(x.inline_link_clicks)).Sum();
                    retVal.ctr = listInsights.Where(x => x.ctr != null).Select(x => Int32.Parse(x.ctr)).Sum();
                    retVal.cplc = listInsights.Where(x => x.cost_per_unique_inline_link_click != null).Select(x => Int32.Parse(x.cost_per_unique_inline_link_click)).Sum();
                }
                else
                {
                    //retVal.click = Int32.Parse(fbAdsCampaignsDetails.data[0].clicks);
                    retVal.reach = Int32.Parse(fbAdsCampaignsDetails.data[0].reach);
                    retVal.impression = Int32.Parse(fbAdsCampaignsDetails.data[0].impressions);
                    retVal.result = Int32.Parse(listInsights[0].results);
                    retVal.costPerResult = Int32.Parse(listInsights[0].cpr);
                    retVal.spend = Int32.Parse(listInsights[0].spend);
                    retVal.link = Int32.Parse(listInsights[0].inline_link_clicks);
                    retVal.ctr = Int32.Parse(listInsights[0].ctr);
                    retVal.cplc = Int32.Parse(listInsights[0].cost_per_unique_inline_link_click);
                }
            }
            catch (Exception)
            {
                return retVal;
            }

            return retVal;
        }


        private async Task<BatchCampaignChartData> BatchRequest(string accessToken, List<BatchDto> batch)
        {
            string url = "https://graph.facebook.com";

            var payload = new
            {
                access_token = accessToken,
                batch
            };

            try
            {
                using (var client = new HttpClient())
                {
                    var ss = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(payload));
                    var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        var results = JsonConvert.DeserializeObject<List<BatchResponse>>(responseContent);

                        List<FbAdsCampaignsDetails> listOfAll = new List<FbAdsCampaignsDetails>();

                        List<int> impressions = new List<int>();
                        List<int> reachs = new List<int>();
                        List<decimal> spends = new List<decimal>();
                        List<decimal> ctr = new List<decimal>();
                        List<int> clicks = new List<int>();

                        List<decimal> cpc = new List<decimal>();
                        List<int> link_clicks = new List<int>();
                        List<decimal> cost_per_lclick = new List<decimal>();

                        var currency = string.Empty;


                        foreach (var res in results)
                        {
                            var fbAdsCampaignsDetails = JsonConvert.DeserializeObject<FbAdsCampaignsDetails>(res.body);

                            var imp = fbAdsCampaignsDetails.data.Sum(x => Convert.ToInt32(x.impressions));
                            impressions.Add(imp);

                            var reachVal = fbAdsCampaignsDetails.data.Sum(x => Convert.ToInt32(x.reach));
                            reachs.Add(reachVal);

                            var spendVal = fbAdsCampaignsDetails.data.Sum(x => Convert.ToDecimal(x.spend));
                            spends.Add(spendVal);

                            var ctrVal = fbAdsCampaignsDetails.data.Sum(x => Convert.ToDecimal(x.ctr));
                            ctr.Add(ctrVal);

                            var clickVal = fbAdsCampaignsDetails.data.Sum(x => Convert.ToInt32(x.clicks));
                            clicks.Add(clickVal);

                            var cpcVal = fbAdsCampaignsDetails.data.Sum(x => Convert.ToDecimal(x.cpc));
                            cpc.Add(cpcVal);

                            var linkVal = fbAdsCampaignsDetails.data.Sum(x => Convert.ToInt32(x.inline_link_clicks));
                            link_clicks.Add(linkVal);

                            var cplcVal = fbAdsCampaignsDetails.data.Sum(x => Convert.ToDecimal(x.cost_per_unique_inline_link_click));
                            cost_per_lclick.Add(cplcVal);

                            //currency = fbAdsCampaignsDetails.data.Select(x => x.account_currency).FirstOrDefault();

                            //listOfAll.Add(fbAdsCampaignsDetails);
                        }



                        return new BatchCampaignChartData { Impressions = impressions, Reach = reachs, Spend = spends, Ctr = ctr, Click = clicks, CPC = cpc, CPLC = cost_per_lclick, LinkClick = link_clicks };

                    }
                    else
                    {
                        Console.WriteLine($"Request failed with status code {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                //var msg = ex;
                return new BatchCampaignChartData { Impressions = new List<int> { }, Reach = new List<int> { }, Spend = new List<decimal> { }, Ctr = new List<decimal> { }, Click = new List<int> { }, CPC = new List<Decimal> { }, CPLC = new List<Decimal> { }, LinkClick = new List<int> { } };

            }

            return new BatchCampaignChartData { Impressions = new List<int> { }, Reach = new List<int> { }, Spend = new List<decimal> { }, Ctr = new List<decimal> { }, Click = new List<int> { }, CPC = new List<Decimal> { }, CPLC = new List<Decimal> { }, LinkClick = new List<int> { } };
        }

        /// <summary>
        /// Get FB Ads Campaigns Details
        /// </summary>
        /// <param name="fbAdsCampaigns">fbAdsCampaigns</param>
        /// <param name="accessToken">accessToken</param>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <returns>FbAdsCampaignsDetails</returns>
        private async Task<FbAdsCampaignsDetails> GetFBAdsAdsetsInsights(string id, string accessToken, string startDate, string endDate)
        {
            FbAdsCampaignsDetails retVal = null;

            try
            {
                var httpClient = new HttpClient
                {
                    BaseAddress = new Uri("https://graph.facebook.com/v22.0/")
                };

                var preparedUrl = id + "/insights?fields=inline_link_clicks,cost_per_unique_inline_link_click,clicks,reach,impressions,ctr,cpc,actions,campaign_id,adset_id,account_id,video_15_sec_watched_actions,spend,cost_per_action_type,estimated_ad_recallers&access_token=" + accessToken + "&time_range={'since':" + "'" + startDate + "'" + ",'until':" + "'" + endDate + "'" + "}";

                var response = await httpClient.GetAsync(preparedUrl);

                var data = await response.Content.ReadAsStringAsync();

                var fbAdsCampaignsDetails = JsonConvert.DeserializeObject<FbAdsCampaignsDetails>(data);

                retVal = fbAdsCampaignsDetails;
            }
            catch (Exception)
            {
                return retVal;
            }

            return retVal;
        }

        private async Task<AdsetConfig> GetFBAdsAdsetsConfig(string id, string accessToken, string startDate, string endDate)
        {
            AdsetConfig retVal = null;

            try
            {
                var httpClient = new HttpClient
                {
                    BaseAddress = new Uri("https://graph.facebook.com/v22.0/")
                };

                var preparedUrl = id + "?fields=status,promoted_object,name,optimization_goal,campaign_id,account_id&access_token=" + accessToken;

                var response = await httpClient.GetAsync(preparedUrl);

                var data = await response.Content.ReadAsStringAsync();

                var fbAdsCampaignsDetails = JsonConvert.DeserializeObject<AdsetConfig>(data);

                //fbAdsCampaignsDetails.data[0].campaign_name = fbAdsCampaigns.name;

                retVal = fbAdsCampaignsDetails;
            }
            catch (Exception)
            {
                return retVal;
            }

            return retVal;
        }

        private async Task<AdsetConfig> GetFBAdsCopiesConfig(string id, string accessToken, string startDate, string endDate)
        {
            AdsetConfig retVal = null;

            try
            {
                var httpClient = new HttpClient
                {
                    BaseAddress = new Uri("https://graph.facebook.com/v22.0/")
                };

                var preparedUrl = id + "?fields=status,promoted_object,name,optimization_goal,campaign_id,account_id&access_token=" + accessToken + "&time_range={'since':" + "'" + startDate + "'" + ",'until':" + "'" + endDate + "'" + "}";

                var response = await httpClient.GetAsync(preparedUrl);

                var data = await response.Content.ReadAsStringAsync();

                var fbAdsCampaignsDetails = JsonConvert.DeserializeObject<AdsetConfig>(data);

                //fbAdsCampaignsDetails.data[0].campaign_name = fbAdsCampaigns.name;

                retVal = fbAdsCampaignsDetails;
            }
            catch (Exception)
            {
                return retVal;
            }

            return retVal;
        }


        private async Task<FbAdsCampaignsDetails> GetFBAdsCopiesInsights(string id, string adName, string adsetName, string adsCampaignName, string thumbnailUrl, string accessToken, string startDate, string endDate)
        {
            FbAdsCampaignsDetails retVal = null;

            try
            {
                var httpClient = new HttpClient
                {
                    BaseAddress = new Uri("https://graph.facebook.com/v22.0/")
                };

                var preparedUrl = id + "/insights?fields=inline_link_clicks,cost_per_unique_inline_link_click,clicks,reach,impressions,ctr,cpc,actions,campaign_id,adset_id,account_id,video_15_sec_watched_actions,spend,estimated_ad_recallers,cost_per_action_type&access_token=" + accessToken + "&time_range={'since':" + "'" + startDate + "'" + ",'until':" + "'" + endDate + "'" + "}";

                var response = await httpClient.GetAsync(preparedUrl);

                var data = await response.Content.ReadAsStringAsync();

                var fbAdsCampaignsDetails = JsonConvert.DeserializeObject<FbAdsCampaignsDetails>(data);

                fbAdsCampaignsDetails.data[0].campaign_name = adsCampaignName;
                fbAdsCampaignsDetails.data[0].adset_name = adsetName;
                fbAdsCampaignsDetails.data[0].ad_name = adName;
                fbAdsCampaignsDetails.data[0].ad_image = thumbnailUrl;


                retVal = fbAdsCampaignsDetails;
            }
            catch (Exception)
            {
                return retVal;
            }

            return retVal;
        }


        /// <summary>
        /// Get FBAds Campaigns
        /// </summary>
        /// <param name="accessToken">accessToken</param>
        /// <param name="facebookAdsAccountID">facebookAdsAccountID</param>
        /// <returns>GetFBAdsCampaigns</returns>
        private async Task<FbAdsCampaignsDetails> GetFBAdsCampaigns(string accessToken, string facebookAdsAccountID, string level, string startDate, string endDate)
        {
            FbAdsCampaignsDetails retVal = null;

            try
            {
                //var total = 0;

                var httpClient = new HttpClient
                {
                    BaseAddress = new Uri("https://graph.facebook.com/v22.0/")
                };

                var preparedUrl = facebookAdsAccountID + "/insights?level=" + level + "&limit=2000&fields=cpc,inline_link_clicks,cost_per_unique_inline_link_click,account_currency,impressions,reach,clicks,ctr,campaign_name,ad_name,adset_name,ad_id,campaign_id,adset_id,account_id,spend&time_range={'since':" + "'" + startDate + "'" + ",'until':" + "'" + endDate + "'}&access_token=" + accessToken;

                var response = await httpClient.GetAsync(preparedUrl);

                var data = await response.Content.ReadAsStringAsync();

                var res = JsonConvert.DeserializeObject<FbAdsCampaignsDetails>(data);

                //total = res.campaigns.data.Count;

                //if (total == 25)
                //{
                //    do
                //    {
                //        var res1 = await GetNextDataForCampaign(res.campaigns.paging.next);

                //        res.campaigns.data.AddRange(res1.data);
                //        total = res1.data.Count;
                //        res.campaigns.paging.next = res1.paging.next;

                //    }
                //    while (total == 25);
                //}

                return res;

            }
            catch (Exception)
            {
                return retVal;
            }

        }

        private async Task<FbAdsSetResponse> GetFBAdsSets(string accessToken, string facebookAdsAccountID)
        {
            FbAdsSetResponse retVal = null;

            try
            {
                //var total = 0;

                var httpClient = new HttpClient
                {
                    BaseAddress = new Uri("https://graph.facebook.com/v22.0/")
                };

                var preparedUrl = facebookAdsAccountID + "?fields=adsets.limit(500){name,campaign{name}},currency&access_token=" + accessToken;

                var response = await httpClient.GetAsync(preparedUrl);

                var data = await response.Content.ReadAsStringAsync();

                var res = JsonConvert.DeserializeObject<FbAdsSetResponse>(data);

                //total = res.adsets.data.Count;

                //if (total == 25)
                //{
                //    do
                //    {
                //        var res1 = await GetNextDataForAdset(res.adsets.paging.next);

                //        res.adsets.data.AddRange(res1.data);
                //        total = res1.data.Count;
                //        res.adsets.paging.next = res1.paging.next;

                //    }
                //    while (total == 25);
                //}

                return res;
            }
            catch (Exception ex)
            {
                return retVal;
            }

        }

        public async Task<FbAdsCampaignsData> GetNextDataForCampaign(string url)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://graph.facebook.com/v22.0/")
            };
            var response = await httpClient.GetAsync(url);

            var data = await response.Content.ReadAsStringAsync();

            var test = JsonConvert.DeserializeObject<FbAdsCampaignsData>(data);
            return test;
        }

        public async Task<Adsets> GetNextDataForAdset(string url)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://graph.facebook.com/v22.0/")
            };
            var response = await httpClient.GetAsync(url);

            var data = await response.Content.ReadAsStringAsync();

            var test = JsonConvert.DeserializeObject<Adsets>(data);
            return test;
        }

        public async Task<InsightsFieldForCopies> GetFBAdsForCopies(string accessToken, string facebookAdsAccountID)
        {
            InsightsFieldForCopies retVal = null;

            try
            {
                var httpClient = new HttpClient
                {
                    BaseAddress = new Uri("https://graph.facebook.com/v22.0/")
                };

                var preparedUrl = facebookAdsAccountID + "?fields=ads.limit(500){name,url,adset{name},campaign{name},adcreatives{thumbnail_url}},currency&access_token=" + accessToken;

                var response = await httpClient.GetAsync(preparedUrl);

                var data = await response.Content.ReadAsStringAsync();

                var fbAdsAdsCopiesData = JsonConvert.DeserializeObject<InsightsFieldForCopies>(data);

                return fbAdsAdsCopiesData;
            }
            catch (Exception)
            {
                return retVal;
            }

        }


        /// <summary>
        /// Calculate PreviousStartDate And EndDate
        /// </summary>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <returns>PreviousDate</returns>
        public PreviousDate CalculatePreviousStartDateAndEndDate(DateTime startDate, DateTime endDate)
        {
            var previousDate = new PreviousDate();
            var diff = (endDate - startDate).TotalDays;
            diff = Math.Round(diff);

            previousDate.PreviousEndDate = startDate.AddDays(-1);
            previousDate.PreviousStartDate = previousDate.PreviousEndDate.AddDays(-diff);

            return previousDate;
        }

        public PreviousDate CalculatePreviousStartDateAndEndDate(string startDateString, string endDateString)
        {
            var previousDate = new PreviousDate();

            DateTime startDate = DateTime.ParseExact(startDateString, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime endDate = DateTime.ParseExact(endDateString, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            var diff = (endDate - startDate).TotalDays;
            diff = Math.Round(diff);

            previousDate.PreviousEndDate = startDate.AddDays(-1);
            previousDate.PreviousStartDate = previousDate.PreviousEndDate.AddDays(-diff);

            return previousDate;
        }

        /// <summary>
        /// Send pdf reports into email
        /// </summary>
        /// <param name="subject">subject</param>
        /// <param name="emailIds">emailIds</param>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <param name="campaignId">campaignId</param>
        /// <param name="pdfByte">pdfByte</param>
        private async Task<bool> SendReportInEmail(string subject, string emailIds, DateTime startDate, DateTime endDate, byte[] pdfByte, long sendAt, string fromWhom, Guid companyId,string header,string footer,string chatGptContent)
        {
            // Use configuration value instead of hardcoded key
            var sendGridApiKey = _configuration["Client"];
            var client = new SendGridClient(sendGridApiKey);

            List<EmailAddress> listOfEmail = new List<EmailAddress>();

            var emails = emailIds.Split(',');

            foreach (var email in emails)
            {
                listOfEmail.Add(new EmailAddress(email.Trim()));
            }

            var htmlContent = string.Empty;

            htmlContent  += header;     
            htmlContent += chatGptContent;
            htmlContent += footer;

            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(new EmailAddress(fromWhom), listOfEmail,
            subject, "", htmlContent);

            //msg.SendAt = sendAt;

            string file = Convert.ToBase64String(pdfByte);

            var company = _companyService.GetEntityById(companyId);

            msg.AddAttachment("" + "_Report.pdf", file);

            try
            {
                var response = await client.SendEmailAsync(msg);
                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> SendReportInEmailFronEnd(ShareReportPdfEmail shareReportPdfEmail)
        {
            try
            {
                var fromWhom = string.Empty;

                byte[] imageAsByteArray = new System.Net.WebClient().DownloadData(shareReportPdfEmail.PdfUrl);

                var emailWhiteLabel = _emailWhitelabelRepository.GetFilteredEntities().Where(x => x.CompanyID == shareReportPdfEmail.CompanyId && x.IsVerify == true).FirstOrDefault();
                if (emailWhiteLabel != null)
                {
                    fromWhom = "no-reply@" + emailWhiteLabel.DomainName;
                }
                else
                {
                    fromWhom = _configuration.GetSection("MailFrom").Value;
                }

                var client = new SendGridClient(_configuration.GetSection("Client").Value);

                List<EmailAddress> listOfEmail = new List<EmailAddress>();


                listOfEmail.Add(new EmailAddress(shareReportPdfEmail.Email.Trim()));


                var htmlContent = string.Empty;

                htmlContent += shareReportPdfEmail.Header;
                htmlContent += "</br> The report from " + shareReportPdfEmail.StartDate.ToString("MM/dd/yyyy") + " to " + shareReportPdfEmail.EndDate.ToString("MM/dd/yyyy") + "<br/>" + shareReportPdfEmail.HtmlContent;
                htmlContent += shareReportPdfEmail.Footer;

                var msg = MailHelper.CreateSingleEmailToMultipleRecipients(new EmailAddress(fromWhom), listOfEmail,
                shareReportPdfEmail.CompanyName + ": Project Report Update", "", htmlContent);

                string file = Convert.ToBase64String(imageAsByteArray);

                msg.AddAttachment(shareReportPdfEmail.CompanyName + "_Report.pdf", file);
                var response = await client.SendEmailAsync(msg);
                return response.StatusCode == HttpStatusCode.Accepted ? true : false;

            }
            catch (Exception ex)
            {
                return false;
            }
        }


        /// <summary>
        /// Prepare GaOrganicTrafficReports 
        /// </summary>
        /// <param name="htmlString">htmlString</param>
        /// <param name="accessToken">accessToken</param>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <param name="profileId">profileId</param>
        /// <param name="isPrevious">isPrevious</param>
        /// <returns>status of the operation</returns>
        private async Task<GoogleAnalyticsDataDto> PrepareGaOrganicTrafficReportsByGet(string htmlString, string accessToken, string startDate, string endDate, string profileId, bool isPrevious = false)
        {
            var retVal = new GoogleAnalyticsDataDto();
            retVal.HtmlString = "";
            retVal.WebTrafficData = new int[] { 0, 0, 0, 0 };
            retVal.Conversions = new int[] { 0 };

            try
            {

                var httpClient = new HttpClient
                {
                    BaseAddress = new Uri("https://www.googleapis.com/"),
                    DefaultRequestHeaders = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) }
                };

                var prepareUrl = "analytics/v3/data/ga?ids=ga:" + profileId + "&start-date=" + startDate + "&end-date=" + endDate + "&metrics=ga%3Asessions%2Cga%3AgoalCompletionsAll&dimensions=ga%3Asource%2Cga%3Amedium%2Cga%3AadContent%2Cga%3AsocialNetwork%2Cga%3AdeviceCategory%2cga%3Adate";
                ////Get task status
                var response = await httpClient.GetAsync(prepareUrl);
                var data = await response.Content.ReadAsStringAsync();
                var res = JsonConvert.DeserializeObject<GoogleAnalyticsResponseDto>(data);

                var rows = res.rows;

                if (res.totalResults > 0)
                {
                    var direct = 0;
                    var organic = 0;
                    var referral = 0;
                    var social = 0;
                    var desktop = 0;
                    var mobile = 0;
                    var tablet = 0;
                    var conversions = 0;

                    List<gaConversionDto> conversionArray = new List<gaConversionDto>();
                    List<gaConversionDto> conversionArray1 = new List<gaConversionDto>();

                    List<gaConversionDto> orgTrafficArray = new List<gaConversionDto>();
                    List<gaConversionDto> orgTrafficArray1 = new List<gaConversionDto>();

                    for (var i = 0; i < rows.Count; i++)
                    {
                        if (rows[i][0] == "(direct)")
                        {
                            direct = direct + Int32.Parse(rows[i][6]);//Organic Search
                        }
                        if (rows[i][1] == "organic")
                        {
                            organic = organic + Int32.Parse(rows[i][6]);//Organic Search
                            orgTrafficArray.Add(new gaConversionDto() { date = rows[i][5], value = int.Parse(rows[i][6]) });
                            orgTrafficArray1.Add(new gaConversionDto() { date = rows[i][5], value = int.Parse(rows[i][6]) });
                        }
                        if (rows[i][1] == "referral")
                        {
                            referral = referral + Int32.Parse(rows[i][6]);//referral
                        }
                        if (rows[i][3] != "(not set)")
                        {
                            social = social + Int32.Parse(rows[i][6]);//social
                        }
                        if (rows[i][4] == "mobile")
                        {
                            mobile = mobile + Int32.Parse(rows[i][6]);//mobile
                        }
                        if (rows[i][4] == "desktop")
                        {
                            desktop = desktop + Int32.Parse(rows[i][6]);//desktop
                        }
                        if (rows[i][4] == "tablet")
                        {
                            tablet = tablet + Int32.Parse(rows[i][6]);//desktop
                        }
                        if (rows[i][6] != "(not set)")
                        {
                            conversionArray.Add(new gaConversionDto() { date = rows[i][5], value = int.Parse(rows[i][7]) });
                            conversionArray1.Add(new gaConversionDto() { date = rows[i][5], value = int.Parse(rows[i][7]) });
                            conversions = conversions + int.Parse(rows[i][7]);
                        }
                    }

                    int[] fconvArray = new int[conversionArray.Count];
                    int convArrayLength = conversionArray.Count;

                    var gaConversions = conversionArray.GroupBy(d => d.date)
                         .Select(g => new
                         {
                             Value = g.Sum(s => s.value)
                         });
                    var orgTrafficData = orgTrafficArray.GroupBy(d => d.date)
                        .Select(g => new
                        {
                            Value = g.Sum(s => s.value)
                        });

                    DateTime DT = DateTime.ParseExact(startDate, "yyyy-MM-dd", new System.Globalization.CultureInfo("en-US"));
                    DateTime DT1 = DateTime.ParseExact(endDate, "yyyy-MM-dd", new System.Globalization.CultureInfo("en-US"));

                    var dates = new List<string>();

                    for (var dt = DT; dt <= DT1; dt = dt.AddDays(1))
                    {
                        String date = dt.ToString("MM-dd");
                        dates.Add(date);
                    }

                    int[] webTrafficData = new int[] { direct, organic, referral, social };

                    int[] webTrafficByDevice = new int[] { desktop, mobile, tablet };

                    int[] conversionsData = new int[] { conversions };

                    var str = String.Join(",", webTrafficData);
                    var str1 = String.Join(",", gaConversions.Select(x => "'" + x.Value + "'"));
                    var orgTraStr = String.Join(",", orgTrafficData.Select(x => "'" + x.Value + "'"));
                    var dateListStr = String.Join(",", dates.Select(x => "'" + x + "'"));
                    htmlString = htmlString.Replace(isPrevious ? "_2gaConverionData2_" : "_1gaConverionData1_", str1);
                    htmlString = htmlString.Replace("_1gaConverionLabels1_", dateListStr);
                    htmlString = htmlString.Replace("_1gaTrafficLabels1_", dateListStr);
                    htmlString = htmlString.Replace(isPrevious ? "_2gaTrafficData2_" : "_1gaTrafficData1_", orgTraStr);
                    retVal.WebTrafficByDevice = webTrafficByDevice;
                    retVal.WebTrafficData = webTrafficData;
                    retVal.Conversions = conversionsData;
                    retVal.HtmlString = htmlString;

                    return retVal;
                }
            }
            catch (Exception ex)
            {
                var error = ex;
            }


            return retVal;
        }

        public async Task<GoogleAnalyticsResponseDto> PrepareGAReportsByPost(string accessToken, string startDate, string endDate, string profileId)
        {
            var gaResponse = new GoogleAnalyticsResponseDto();

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://www.googleapis.com/"),
                DefaultRequestHeaders = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) }
            };

            var prepareUrl = "analytics/v3/data/ga?ids=ga:" + profileId + "&start-date=" + startDate + "&end-date=" + endDate + "&metrics=ga%3Asessions%2Cga%3AgoalCompletionsAll&dimensions=ga%3Asource%2Cga%3Amedium%2Cga%3AadContent%2Cga%3AsocialNetwork%2Cga%3AdeviceCategory%2cga%3Adate";
            ////Get task status
            var resp = await httpClient.GetAsync(prepareUrl);
            if (resp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                gaResponse.statusCode = System.Net.HttpStatusCode.OK;
                gaResponse = JsonConvert.DeserializeObject<GoogleAnalyticsResponseDto>(await resp.Content.ReadAsStringAsync());
                return gaResponse;

            }
            else
            {
                gaResponse.statusCode = resp.StatusCode;
                return gaResponse;
            }
        }

        public async Task<GA4Root> PrepareGa4OrganicTrafficReportsByGet(string accessToken, string fromDate, string toDate, string profileId)
        {
            try
            {
                var retVal = new GA4Root();

                var httpClient = new HttpClient
                {
                    BaseAddress = new Uri("https://analyticsdata.googleapis.com"),
                    DefaultRequestHeaders = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) }
                };

                var prepareUrl = "/v1beta/properties/" + profileId + ":runReport?key=" + _configuration.GetSection("GoogleApiKey").Value;

                var dateRanges = new object[]
                {
                    new { startDate = fromDate , endDate = toDate },
                };
                var metrics = new object[]
                {
                    new {name= "conversions" },
                    new { name= "sessions" },
                    new {name = "newUsers"}
                };
                var dimensions = new object[]
                {
                    new { name = "date" },
                    new { name = "sessionDefaultChannelGrouping" }
                };

                //var dimensionFilter = new
                //{
                //    filter = new
                //    {
                //        fieldName = "sessionDefaultChannelGroup",
                //        stringFilter = new
                //        {
                //            matchType = "EXACT",
                //            value = "Organic Search"
                //        }
                //    }
                //};


                var orderBys = new object[]
                {
                                new
                                {
                                    dimension = new
                                    {
                                        orderType = "NUMERIC",
                                        dimensionName = "date"
                                    }
                                }
                };

                var data = new
                {
                    dateRanges = dateRanges,
                    metrics = metrics,
                    dimensions = dimensions,
                    //dimensionFilter = dimensionFilter,
                    orderBys = orderBys
                };
                var stringContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                var resp = await httpClient.PostAsync(prepareUrl, stringContent);
                if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var response = JsonConvert.DeserializeObject<GA4Root>(await resp.Content.ReadAsStringAsync());

                    if (response.rows != null && response.rows.Count > 0)
                    {
                        //Add missing properties(where Organic Social not availble in some dates) and add row where date is missing
                        List<string> targetProperties = new List<string> { "Unassigned", "Referral", "Organic Social", "Organic Search", "Direct" };

                        var missingRows = new List<GA4Row>();

                        var distinctDates = response.rows.Select(row => row.dimensionValues[0].value).Distinct().ToList();


                        foreach (var row in distinctDates)
                        {
                            var currentDate = row;
                            var existingProperties = response.rows.Where(x => x.dimensionValues[0].value.Contains(currentDate)).Select(dv => dv.dimensionValues[1].value).Distinct().ToList();
                            var missingProperties = targetProperties.Except(existingProperties);

                            foreach (var missingProperty in missingProperties)
                            {
                                var newRow = new GA4Row
                                {
                                    dimensionValues = new List<GA4DimensionValue>
                            {
                                new GA4DimensionValue { value = currentDate },
                                new GA4DimensionValue { value = missingProperty }
                            },
                                    metricValues = new List<GA4MetricValue>
                                {
                                  new GA4MetricValue { value = "0" },
                                                new GA4MetricValue { value = "0" },
                                                 new GA4MetricValue { value = "0" }
                                }
                                };

                                missingRows.Add(newRow);
                            }
                        }

                        response.rows.AddRange(missingRows);

                        //Add rows if missing row for perticular date(data not availble for any date)
                        DateTime startDate = DateTime.ParseExact(fromDate, "yyyy-MM-dd", new System.Globalization.CultureInfo("en-US"));
                        DateTime endDate = DateTime.ParseExact(toDate, "yyyy-MM-dd", new System.Globalization.CultureInfo("en-US"));


                        // Create a new list to store the modified response data
                        List<GA4Row> newRows = new List<GA4Row>();


                        for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                        {
                            // Check if the current date exists in the response data
                            bool found = false;
                            foreach (var row in response.rows)
                            {
                                DateTime rowDate = DateTime.ParseExact(row.dimensionValues[0].value.ToString(), "yyyyMMdd", null);
                                if (rowDate == date)
                                {
                                    newRows.Add(row);
                                    found = true;
                                }
                            }

                            // If the current date was not found, add a new entry with value 0
                            if (!found)
                            {
                                var lisofrow = new List<GA4Row>
                                {
                                     new GA4Row
                                        {
                                            dimensionValues = new List<GA4DimensionValue>
                                            {
                                                new GA4DimensionValue {  value = date.ToString("yyyyMMdd") },
                                                new GA4DimensionValue { value = "Direct"}
                                            },
                                                metricValues = new List<GA4MetricValue>
                                            {
                                                new GA4MetricValue { value = "0" },
                                                new GA4MetricValue { value = "0" },
                                                 new GA4MetricValue { value = "0" }
                                            }
                                        },
                                     new GA4Row
                                        {
                                            dimensionValues = new List<GA4DimensionValue>
                                            {
                                                new GA4DimensionValue {  value = date.ToString("yyyyMMdd") },
                                                new GA4DimensionValue { value = "Organic Search"}
                                            },
                                                metricValues = new List<GA4MetricValue>
                                            {
                                                new GA4MetricValue { value = "0" },
                                                new GA4MetricValue { value = "0" },
                                                 new GA4MetricValue { value = "0" }
                                            }
                                        },
                                     new GA4Row
                                        {
                                            dimensionValues = new List<GA4DimensionValue>
                                            {
                                                new GA4DimensionValue {  value = date.ToString("yyyyMMdd") },
                                                new GA4DimensionValue { value = "Organic Social"}
                                            },
                                                metricValues = new List<GA4MetricValue>
                                            {
                                                new GA4MetricValue { value = "0" },
                                                new GA4MetricValue { value = "0" },
                                                 new GA4MetricValue { value = "0" }
                                            }
                                        },
                                     new GA4Row
                                        {
                                            dimensionValues = new List<GA4DimensionValue>
                                            {
                                                new GA4DimensionValue {  value = date.ToString("yyyyMMdd") },
                                                new GA4DimensionValue { value = "Referral"}
                                            },
                                                metricValues = new List<GA4MetricValue>
                                            {
                                                new GA4MetricValue { value = "0" },
                                                new GA4MetricValue { value = "0" },
                                                 new GA4MetricValue { value = "0" }
                                            }
                                        },
                                     new GA4Row
                                        {
                                            dimensionValues = new List<GA4DimensionValue>
                                            {
                                                new GA4DimensionValue {  value = date.ToString("yyyyMMdd") },
                                                new GA4DimensionValue { value = "Unassigned"}
                                            },
                                                metricValues = new List<GA4MetricValue>
                                            {
                                                new GA4MetricValue { value = "0" },
                                                new GA4MetricValue { value = "0" },
                                                 new GA4MetricValue { value = "0" }
                                            }
                                        },
                                };

                                newRows.AddRange(lisofrow);
                            }
                        }


                        // Update the modified response data with the new rows
                        response.rows = newRows;
                        response.rows = newRows.OrderBy(row => DateTime.ParseExact(row.dimensionValues[0].value, "yyyyMMdd", null)).ToList();
                        response.statusCode = System.Net.HttpStatusCode.OK;
                        retVal = response;
                        return retVal;
                    }
                    else
                    {
                        return retVal;
                    }

                }
                else
                {
                    retVal.statusCode = resp.StatusCode;
                    return retVal;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<EcomPurchase>> PrepareGa4EcomReportsByGet(string accessToken, string fromDate, string toDate, string profileId)
        {
            try
            {
                var retVal = new List<EcomPurchase>();

                var httpClient = new HttpClient
                {
                    BaseAddress = new Uri("https://analyticsdata.googleapis.com"),
                    DefaultRequestHeaders = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) }
                };

                var prepareUrl = "/v1beta/properties/" + profileId + ":runReport?key=" + _configuration.GetSection("GoogleApiKey").Value;

                var dateRanges = new object[]
                {
                    new { startDate = fromDate , endDate = toDate },
                };
                var metrics = new object[]
                {
                    new {name= "itemRevenue" },
                    new { name= "itemsAddedToCart" },
                    new {name = "itemsPurchased"},
                     new {name = "itemsViewed"}
                };
                var dimensions = new object[]
                {
                    new { name = "date" },
                    new { name = "itemName" }
                };

                var orderBys = new object[]
                {
                                new
                                {
                                    dimension = new
                                    {
                                        orderType = "NUMERIC",
                                        dimensionName = "date"
                                    }
                                }
                };

                var data = new
                {
                    dateRanges = dateRanges,
                    metrics = metrics,
                    dimensions = dimensions,
                    //dimensionFilter = dimensionFilter,
                    orderBys = orderBys
                };
                var stringContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                var resp = await httpClient.PostAsync(prepareUrl, stringContent);
                if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var response = JsonConvert.DeserializeObject<GA4Root>(await resp.Content.ReadAsStringAsync());

                    if (response.rows != null && response.rows.Count > 0)
                    {
                        List<string> targetProperties = response.rows
                               .GroupBy(row => row.dimensionValues[1].value)
                               .Select(group => new
                               {
                                   Property = group.Key,
                                   SumMaxValue = group.Sum(row => double.Parse(row.metricValues[3].value))
                               })
                               .OrderByDescending(item => item.SumMaxValue)
                               .Select(item => item.Property)
                               .ToList();

                        var missingRows = new List<GA4Row>();

                        var distinctDates = response.rows.Select(row => row.dimensionValues[0].value).Distinct().ToList();

                        foreach (var row in distinctDates)
                        {
                            var currentDate = row;
                            var existingProperties = response.rows.Where(x => x.dimensionValues[0].value.Contains(currentDate)).Select(dv => dv.dimensionValues[1].value).Distinct().ToList();
                            var missingProperties = targetProperties.Except(existingProperties);

                            foreach (var missingProperty in missingProperties)
                            {
                                var newRow = new GA4Row
                                {
                                    dimensionValues = new List<GA4DimensionValue>
                            {
                                new GA4DimensionValue { value = currentDate },
                                new GA4DimensionValue { value = missingProperty }
                            },
                                    metricValues = new List<GA4MetricValue>
                                {
                                  new GA4MetricValue { value = "0" },
                                  new GA4MetricValue { value = "0" },
                                  new GA4MetricValue { value = "0" },
                                  new GA4MetricValue { value = "0" }
                                }
                                };

                                missingRows.Add(newRow);
                            }
                        }

                        response.rows.AddRange(missingRows);

                        //Add rows if missing row for perticular date(data not availble for any date)
                        DateTime startDate = DateTime.ParseExact(fromDate, "yyyy-MM-dd", new System.Globalization.CultureInfo("en-US"));
                        DateTime endDate = DateTime.ParseExact(toDate, "yyyy-MM-dd", new System.Globalization.CultureInfo("en-US"));


                        // Create a new list to store the modified response data
                        List<GA4Row> newRows = new List<GA4Row>();

                        for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                        {
                            // Check if the current date exists in the response data
                            bool found = false;
                            foreach (var row in response.rows)
                            {
                                DateTime rowDate = DateTime.ParseExact(row.dimensionValues[0].value.ToString(), "yyyyMMdd", null);
                                if (rowDate == date)
                                {
                                    newRows.Add(row);
                                    found = true;
                                }
                            }

                            // If the current date was not found, add a new entry with value 0
                            if (!found)
                            {
                                var listofrow = new List<GA4Row>();
                                foreach (var item in targetProperties)
                                {
                                    var oneRow = new GA4Row
                                    {
                                        dimensionValues = new List<GA4DimensionValue>
                                            {
                                                new GA4DimensionValue {  value = date.ToString("yyyyMMdd") },
                                                new GA4DimensionValue { value = item}
                                            },
                                        metricValues = new List<GA4MetricValue>
                                            {
                                                new GA4MetricValue { value = "0" },
                                                new GA4MetricValue { value = "0" },
                                                new GA4MetricValue { value = "0" },
                                                new GA4MetricValue { value = "0" }
                                            }
                                    };

                                    listofrow.Add(oneRow);
                                }

                                newRows.AddRange(listofrow);
                            }
                        }

                        newRows = newRows.OrderBy(row => DateTime.ParseExact(row.dimensionValues[0].value, "yyyyMMdd", null)).ToList();

                        var listOfEcom = new List<EcomPurchase>();
                        foreach (var name in targetProperties)
                        {
                            var ecomPurchase = new EcomPurchase();
                            ecomPurchase.ItemName = name;
                            ecomPurchase.ItemRevenue = newRows.Where(row => row.dimensionValues.Count > 1 && row.dimensionValues[1].value == name).Select(row => row.metricValues[0].value).ToList();
                            ecomPurchase.ItemsAddedToCart = newRows.Where(row => row.dimensionValues.Count > 1 && row.dimensionValues[1].value == name).Select(row => row.metricValues[1].value).ToList();
                            ecomPurchase.ItemPurchased = newRows.Where(row => row.dimensionValues.Count > 1 && row.dimensionValues[1].value == name).Select(row => row.metricValues[2].value).ToList();
                            ecomPurchase.ItemsViewed = newRows.Where(row => row.dimensionValues.Count > 1 && row.dimensionValues[1].value == name).Select(row => row.metricValues[3].value).ToList();

                            ecomPurchase.TotalRevenue = newRows.Where(row => row.dimensionValues.Count > 1 && row.dimensionValues[1].value == name).Sum(row => double.Parse(row.metricValues[0].value)).ToString();
                            ecomPurchase.TotalAddToCart = newRows.Where(row => row.dimensionValues.Count > 1 && row.dimensionValues[1].value == name).Sum(row => double.Parse(row.metricValues[1].value)).ToString();
                            ecomPurchase.TotalPurchased = newRows.Where(row => row.dimensionValues.Count > 1 && row.dimensionValues[1].value == name).Sum(row => double.Parse(row.metricValues[2].value)).ToString();
                            ecomPurchase.TotalViewed = newRows.Where(row => row.dimensionValues.Count > 1 && row.dimensionValues[1].value == name).Sum(row => double.Parse(row.metricValues[3].value)).ToString();

                            listOfEcom.Add(ecomPurchase);
                        }

                        retVal = listOfEcom;
                        return retVal;
                    }
                    else
                    {
                        return retVal;
                    }
                }
                else
                {
                    return retVal;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Ga4PurchaseJourney> PrepareGa4PurchaseJourneyReports(string accessToken, string fromDate, string toDate, string profileId)
        {
            var retVal = new Ga4PurchaseJourney();

            try
            {
                var httpClient = new HttpClient
                {
                    BaseAddress = new Uri("https://analyticsdata.googleapis.com"),
                    DefaultRequestHeaders = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) }
                };

                var prepareUrl = "/v1beta/properties/" + profileId + ":runReport?key=" + _configuration.GetSection("GoogleApiKey").Value;

                var dateRanges = new object[]
                {
                    new { startDate = fromDate , endDate = toDate },
                };
                var metrics = new object[]
                {
                    new {name= "activeUsers" }

                };
                var dimensions = new object[]
                {

                    new { name = "eventName" }
                };


                var data = new
                {
                    dateRanges = dateRanges,
                    metrics = metrics,
                    dimensions = dimensions,

                };
                var stringContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                var resp = await httpClient.PostAsync(prepareUrl, stringContent);
                if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var response = JsonConvert.DeserializeObject<GA4Root>(await resp.Content.ReadAsStringAsync());

                    if (response.rows != null && response.rows.Count > 0)
                    {
                        var purchaseJourney = new Ga4PurchaseJourney();
                        purchaseJourney.PurchaseTotalSessionStart = response.rows.Where(row => row.dimensionValues.Count > 0 && row.dimensionValues[0].value == "session_start").Select(row => row.metricValues[0].value).FirstOrDefault();
                        purchaseJourney.PurchaseTotalViewItem = response.rows.Where(row => row.dimensionValues.Count > 0 && row.dimensionValues[0].value == "view_item").Select(row => row.metricValues[0].value).FirstOrDefault();
                        purchaseJourney.PurchaseTotalAddedCart = response.rows.Where(row => row.dimensionValues.Count > 0 && row.dimensionValues[0].value == "add_to_cart").Select(row => row.metricValues[0].value).FirstOrDefault();
                        purchaseJourney.PurchaseTotalCheckout = response.rows.Where(row => row.dimensionValues.Count > 0 && row.dimensionValues[0].value == "purchase").Select(row => row.metricValues[0].value).FirstOrDefault();
                        purchaseJourney.PurchaseTotalPurchase = response.rows.Where(row => row.dimensionValues.Count > 0 && row.dimensionValues[0].value == "purchase").Select(row => row.metricValues[0].value).FirstOrDefault();

                        retVal = purchaseJourney;
                        return retVal;
                    }
                    else
                    {
                        return retVal;
                    }
                }
                else
                {
                    return retVal;
                }
            }
            catch (Exception ex)
            {
                return retVal;
            }
        }

        /// <summary>
        /// Prepare LinkedIn Report
        /// </summary>
        /// <param name="htmlString">htmlString</param>
        /// <param name="campaignId">campaignId</param>
        /// <param name="startTime">startTime</param>
        /// <param name="endTime">endTime</param>
        /// <returns>status of the operation</returns>
        private PrepareLinkedinData PrepareLinkedInReport(string htmlString, Guid campaignId, string startTime, string endTime, string campaignLogo, string companyLogo, string headerText, string footerText, string headerBgColor, string headerTextColor, bool showFooter, string showPageNumberId, string showPageNumber, bool showHeader, int pageNumber)
        {
            var retVal = new LinkedInDataDto();
            var rootObj = new RootLinkedInDataObject();
            var retValLinkedin = new PrepareLinkedinData();
            retValLinkedin.LinkedinRawData = new LinkedinRawData();

            // prepare follower gains data
            var TotalOrganicPaidFollowerStatistics = _campaignLinkedinService.GetLinkedinTotalOrganicPaidFollowerStatistics(campaignId.ToString(), startTime, endTime);

            var followerGainsData = JsonConvert.DeserializeObject<RootFollowerGains>(TotalOrganicPaidFollowerStatistics);

            rootObj.FollowerGains = followerGainsData == null ? new
                List<FollowerGains>() : followerGainsData.elements.Select(x => x.followerGains).ToList();

            int[] organic = Mapper.Map<int[]>(rootObj.FollowerGains == null ? new List<int>() : rootObj.FollowerGains.Where(e => e.organicFollowerGain >= 0).Select(x => x.organicFollowerGain).ToList());
            int[] lost = Mapper.Map<int[]>(rootObj.FollowerGains == null ? new List<int>() : rootObj.FollowerGains.Where(e => e.organicFollowerGain < 0).Select(x => x.organicFollowerGain).ToList());
            int[] paid = Mapper.Map<int[]>(rootObj.FollowerGains == null ? new List<int>() : rootObj.FollowerGains.Where(e => e.paidFollowerGain >= 0).Select(x => x.paidFollowerGain).ToList());

            int[][] followerGainsDataSet = new int[][] { organic, lost, paid };

            // add data
            retVal.followerGainsDataSet = followerGainsDataSet;

            // prepare share statistics data
            var TotalShareStatistics = _campaignLinkedinService.GetLinkedinTotalShareStatistics(campaignId.ToString(), startTime, endTime);
            var shareStatisticsData = JsonConvert.DeserializeObject<RootTotalShareStatistics>(TotalShareStatistics);
            rootObj.ShareStatistics = shareStatisticsData.elements.Select(x => x.totalShareStatistics).ToList();
            int[] clicks = Mapper.Map<int[]>(rootObj.ShareStatistics.Select(x => x.clickCount).ToList());
            int[] impressions = Mapper.Map<int[]>(rootObj.ShareStatistics.Select(x => x.impressionCount).ToList());
            int[] comments = Mapper.Map<int[]>(rootObj.ShareStatistics.Select(x => x.commentCount).ToList());
            int[] likes = Mapper.Map<int[]>(rootObj.ShareStatistics.Select(x => x.likeCount).ToList());
            int[] share = Mapper.Map<int[]>(rootObj.ShareStatistics.Select(x => x.shareCount).ToList());

            int[][] ShareStatisticsDataSet = new int[][] { comments, likes, share };

            // convert time stamp into date string
            var start = (new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime()).AddMilliseconds(double.Parse(startTime));
            var end = (new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime()).AddMilliseconds(double.Parse(endTime));
            List<string> dateList = new List<string>();
            while (end > start)
            {
                String date = start.ToString("MM-dd");
                dateList.Add(date);
                start = start.AddDays(1);
            }

            var orgStr = String.Join(",", organic);
            var lostStr = String.Join(",", lost);
            var paidStr = String.Join(",", paid);

            var commStr = String.Join(",", comments);
            var likesStr = String.Join(",", likes);
            var shareStr = String.Join(",", share);
            var clicksStr = String.Join(",", clicks);
            var impressionStr = String.Join(",", impressions);
            var orgPieStr = organic.Sum();
            var paidPieStr = paid.Sum();
            var lostFollower = lost.Sum();
            var clickSum = clicks.Sum();
            var impressionSum = impressions.Sum();
            var dateListStr = String.Join(",", dateList.Select(x => "'" + x + "'"));



            htmlString = htmlString.Replace("_1firstLineChartData1", orgStr);
            htmlString = htmlString.Replace("_2FirstLineChartData2", lostStr);
            htmlString = htmlString.Replace("_3FirstLineChartData3", paidStr);

            htmlString = htmlString.Replace("_4firstLineChartData4", commStr);
            htmlString = htmlString.Replace("_5firstLineChartData5", likesStr);
            htmlString = htmlString.Replace("_6firstLineChartData6", shareStr);

            htmlString = htmlString.Replace("_2secondLineChartData2", clicksStr);
            htmlString = htmlString.Replace("_3thirdLineChartData3", impressionStr);

            htmlString = htmlString.Replace("_7pieChartData7", orgPieStr.ToString());
            htmlString = htmlString.Replace("_8pieChartData8", paidPieStr.ToString());
            htmlString = htmlString.Replace("_1firstLineChartLabels1", dateListStr);

            int intervalRes = dateList.Count <= 31 ? 3 : (dateList.Count <= 91 ? 7 : 31);
            htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

            htmlString = htmlString.Replace("_headerImage1_", companyLogo);
            htmlString = htmlString.Replace("_headerImage_", campaignLogo);
            htmlString = htmlString.Replace("_headerText_", headerText);
            htmlString = htmlString.Replace("_footerText_", footerText);
            htmlString = htmlString.Replace("_headerTextColor_", headerTextColor);
            htmlString = htmlString.Replace("_headerBgColor_", headerBgColor);
            htmlString = htmlString.Replace("_showPageNumber_", showPageNumber);
            htmlString = htmlString.Replace("_pageNumberId_", showPageNumberId);
            htmlString = htmlString.Replace("_pageNumber_", "Page " + pageNumber.ToString());

            if (showHeader)
            {
                htmlString = htmlString.Replace("_showHeader_", "none");
            }
            else
            {
                htmlString = htmlString.Replace("_showHeader_", "hidden");
            }
            if (!showFooter)
            {
                htmlString = htmlString.Replace("_showFooter_", "hidden");
            }
            else
            {
                htmlString = htmlString.Replace("_showFooter_", "none");
            }
            if (campaignLogo == "")
            {
                htmlString = htmlString.Replace("_showImg2_", "hidden");
            }
            else
            {
                htmlString = htmlString.Replace("_showImg2_", "none");
            }

            if (companyLogo == "")
            {
                htmlString = htmlString.Replace("_showImg1_", "hidden");
            }
            else
            {
                htmlString = htmlString.Replace("_showImg1_", "none");
            }
            // add data
            retVal.ShareStatisticsDataSet = ShareStatisticsDataSet;

            retValLinkedin.LinkedinRawData.OrganicFollowerGain = orgPieStr;
            retValLinkedin.LinkedinRawData.PaidFollowerGain = paidPieStr;
            retValLinkedin.LinkedinRawData.LostFollower = lostFollower;

            retValLinkedin.LinkedinRawData.PaidFollower = orgPieStr;
            retValLinkedin.LinkedinRawData.OrganicFollower = paidPieStr;

            retValLinkedin.LinkedinRawData.Impressions = impressionSum;
            retValLinkedin.LinkedinRawData.Clicks = clickSum;


            retValLinkedin.Html = htmlString;
            retValLinkedin.LinkedinRawData = retValLinkedin.LinkedinRawData;

            return retValLinkedin;
        }

        //private LinkedInDemographicChart CalculateLinkedinDemographic(Guid campaignId)
        //{
        //    var retVal = new LinkedInDemographicChart();

        //    // prepare follower gains data
        //    var demographicData = _campaignLinkedinService.GetLinkedinTotalDemographicStatistics(campaignId.ToString());

        //    var linkedinDemographicDto = JsonConvert.DeserializeObject<LinkedinDemographic>(demographicData);

        //    string path = Path.GetFullPath(Path.Combine(_hostingEnvironment.ContentRootPath, @"..\\EventManagement.Utility\\Json\\countries.json"));
        //    string text = File.ReadAllText(path);
        //    var demographicCode = JsonConvert.DeserializeObject<LinkedInDemographicCode>(text);
        //    if (linkedinDemographicDto != null && linkedinDemographicDto.elements != null && linkedinDemographicDto.elements.Count > 0)
        //    {
        //        var topFiveCoutriesFollower = linkedinDemographicDto.elements[0].followerCountsByCountry.OrderByDescending(x => x.followerCounts.organicFollowerCount).Take(5).ToList();

        //        var topFiveSeniority = linkedinDemographicDto.elements[0].followerCountsBySeniority.OrderByDescending(x => x.followerCounts.organicFollowerCount).Take(5).ToList();

        //        var topFiveIndustries = linkedinDemographicDto.elements[0].followerCountsByIndustry.OrderByDescending(x => x.followerCounts.organicFollowerCount).Take(5).ToList();

        //        var topFiveJobFunction = linkedinDemographicDto.elements[0].followerCountsByFunction.OrderByDescending(x => x.followerCounts.organicFollowerCount).Take(5).ToList();

        //        var topFiveCompanySize = linkedinDemographicDto.elements[0].followerCountsByStaffCountRange.OrderByDescending(x => x.followerCounts.organicFollowerCount).Take(5).ToList();

        //        foreach (var con in topFiveCoutriesFollower)
        //        {
        //            var countrycode = con.country.Split(":");
        //            var countryName = demographicCode.countries_name.Where(y => y.code.Equals(countrycode[3].ToUpper())).Select(x => x.name).FirstOrDefault();
        //            con.countryName = !String.IsNullOrEmpty(countryName) ? countryName : "Others";
        //        }

        //        foreach (var con in topFiveSeniority)
        //        {
        //            var code = con.seniority;
        //            var seniorityName = demographicCode.seniority.Where(y => y.code.Equals(code)).Select(x => x.name).FirstOrDefault();
        //            con.name = !String.IsNullOrEmpty(seniorityName) ? seniorityName : "Others";
        //        }

        //        foreach (var con in topFiveIndustries)
        //        {
        //            var code = con.industry;
        //            var industriesName = demographicCode.industries.Where(y => y.URN.Equals(code)).Select(x => x.name.localized.en_US).FirstOrDefault();
        //            con.name = !String.IsNullOrEmpty(industriesName) ? industriesName : "Others";
        //        }

        //        foreach (var con in topFiveJobFunction)
        //        {
        //            var code = con.function;
        //            var jobFunctionName = demographicCode.job_function.Where(y => y.URN.Equals(code)).Select(x => x.name.localized.en_US).FirstOrDefault();
        //            con.name = !String.IsNullOrEmpty(jobFunctionName) ? jobFunctionName : "Others";
        //        }

        //        foreach (var con in topFiveCompanySize)
        //        {
        //            var code = con.staffCountRange;
        //            var companySizeName = demographicCode.company_size.Where(y => y.code.Equals(code)).Select(x => x.name).FirstOrDefault();
        //            con.name = !String.IsNullOrEmpty(companySizeName) ? companySizeName : "Others";
        //        }

        //        var topFiveCoutriesFollowerData = topFiveCoutriesFollower.Select(x => x.followerCounts.organicFollowerCount).ToList();

        //        var topFiveCoutriesFollowerLabel = topFiveCoutriesFollower.Select(x => x.countryName).ToList();


        //        var topFiveSeniorityData = topFiveSeniority.Select(x => x.followerCounts.organicFollowerCount).ToList();

        //        var topFiveSeniorityLabel = topFiveSeniority.Select(x => x.name).ToList();


        //        var topFiveIndustriesData = topFiveIndustries.Select(x => x.followerCounts.organicFollowerCount).ToList();

        //        var topFiveIndustriesLabel = topFiveIndustries.Select(x => x.name).ToList();


        //        var topFiveJobData = topFiveJobFunction.Select(x => x.followerCounts.organicFollowerCount).ToList();

        //        var topFiveJobLabel = topFiveJobFunction.Select(x => x.name).ToList();


        //        var topFiveCompanySizeData = topFiveCompanySize.Select(x => x.followerCounts.organicFollowerCount).ToList();

        //        var topFiveCompanySizeLabel = topFiveCompanySize.Select(x => x.name).ToList();

        //        retVal.CountryLabel = topFiveCoutriesFollowerLabel;
        //        retVal.CountryData = topFiveCoutriesFollowerData;

        //        retVal.SeniorityLabel = topFiveSeniorityLabel;
        //        retVal.SeniorityData = topFiveSeniorityData;

        //        retVal.IndustryData = topFiveIndustriesData;
        //        retVal.IndustryLabel = topFiveIndustriesLabel;

        //        retVal.JobFunctionData = topFiveJobData;
        //        retVal.JobFunctionLabel = topFiveJobLabel;

        //        retVal.CompanySizeData = topFiveCompanySizeData;
        //        retVal.CompanySizeLabel = topFiveCompanySizeLabel;

        //        return retVal;
        //    }
        //    else
        //    {
        //        return retVal;
        //    }
        //}

        private async Task<PrepareLinkedinDemographicRawData> PrepareDemographicLinkedInReport(string htmlString, Guid campaignId, string campaignLogo, string companyLogo, string headerText, string footerText, string headerBgColor, string headerTextColor, bool showFooter, string showPageNumberId, string showPageNumber, bool showHeader, int pageNumber)
        {
            var retVal = new PrepareLinkedinDemographicRawData();
            retVal.LinkedinDemographicRawData = new LinkedinDemographicRawData();

            //var demographicdDto = CalculateLinkedinDemographic(campaignId);
            var demographicdDto = _campaignLinkedinService.GetLinkedinDemographicData(campaignId);

            var countryLabel = demographicdDto.CountryLabel.Select(date => date.ToString()).ToArray();
            //var countryLabelStr = String.Join(",", countryLabel.Select(x => "'" + x + "'"));
            var countryLabelStr = String.Join(",", countryLabel.Select(x => '"' + x + '"'));

            var industryLabel = demographicdDto.IndustryLabel.Select(date => date.ToString()).ToArray();
            //var industryLabelStr = String.Join(",", industryLabel.Select(x => "'" + x + "'"));
            var industryLabelStr = String.Join(",", industryLabel.Select(x => '"' + x + '"'));

            var seniorityLabel = demographicdDto.SeniorityLabel.Select(date => date.ToString()).ToArray();
            //var seniorityLabelStr = String.Join(",", seniorityLabel.Select(x => "'" + x + "'"));
            var seniorityLabelStr = String.Join(",", seniorityLabel.Select(x => '"' + x + '"'));

            var companySizeLabel = demographicdDto.CompanySizeLabel.Select(date => date.ToString()).ToArray();
            //var companySizeLabelStr = String.Join(",", companySizeLabel.Select(x => "'" + x + "'"));
            var companySizeLabelStr = String.Join(",", companySizeLabel.Select(x => '"' + x + '"'));

            var jobFunctionLabel = demographicdDto.JobFunctionLabel.Select(date => date.ToString()).ToArray();
            //var jobFunctionLabelStr = String.Join(",", jobFunctionLabel.Select(x => "'" + x + "'"));
            var jobFunctionLabelStr = String.Join(",", jobFunctionLabel.Select(x => '"' + x + '"'));

            var countryData = String.Join(",", demographicdDto.CountryData);
            var industryData = String.Join(",", demographicdDto.IndustryData);
            var seniorityData = String.Join(",", demographicdDto.SeniorityData);
            var companySizeData = String.Join(",", demographicdDto.CompanySizeData);
            var jobFunctionData = String.Join(",", demographicdDto.JobFunctionData);

            htmlString = htmlString.Replace("_countryChartLabels_", countryLabelStr);
            htmlString = htmlString.Replace("_countryChartData_", countryData);

            htmlString = htmlString.Replace("_seniorityChartLabels_", seniorityLabelStr);
            htmlString = htmlString.Replace("_seniorityChartData_", seniorityData);

            htmlString = htmlString.Replace("_industryChartLabels_", industryLabelStr);
            htmlString = htmlString.Replace("_industryChartData_", industryData);

            htmlString = htmlString.Replace("_jobFunctionChartLabels_", jobFunctionLabelStr);
            htmlString = htmlString.Replace("_jobFunctionChartData_", jobFunctionData);

            htmlString = htmlString.Replace("_companySizeChartLabels_", companySizeLabelStr);
            htmlString = htmlString.Replace("_companySizeChartData_", companySizeData);

            htmlString = htmlString.Replace("_headerImage1_", companyLogo);
            htmlString = htmlString.Replace("_headerImage_", campaignLogo);
            htmlString = htmlString.Replace("_headerText_", headerText);
            htmlString = htmlString.Replace("_footerText_", footerText);
            htmlString = htmlString.Replace("_headerTextColor_", headerTextColor);
            htmlString = htmlString.Replace("_headerBgColor_", headerBgColor);
            htmlString = htmlString.Replace("_showPageNumber_", showPageNumber);
            htmlString = htmlString.Replace("_pageNumberId_", showPageNumberId);
            htmlString = htmlString.Replace("_pageNumber_", "Page " + pageNumber.ToString());

            if (campaignLogo == "")
            {
                htmlString = htmlString.Replace("_showImg2_", "hidden");
            }
            else
            {
                htmlString = htmlString.Replace("_showImg2_", "none");
            }

            if (companyLogo == "")
            {
                htmlString = htmlString.Replace("_showImg1_", "hidden");
            }
            else
            {
                htmlString = htmlString.Replace("_showImg1_", "none");
            }

            if (showHeader)
            {
                htmlString = htmlString.Replace("_showHeader_", "none");
            }
            else
            {
                htmlString = htmlString.Replace("_showHeader_", "hidden");
            }
            if (!showFooter)
            {
                htmlString = htmlString.Replace("_showFooter_", "hidden");
            }
            else
            {
                htmlString = htmlString.Replace("_showFooter_", "none");
            }

            retVal.Html = htmlString;
            // Assuming demographicdDto is an instance of the appropriate class
            // and CountryData, SeniorityData, IndustryData, JobFunctionData, CompanySizeData are collections or arrays.

            retVal.LinkedinDemographicRawData.TopFollowerByCountry =
                demographicdDto.CountryData.Count > 0 && demographicdDto.CountryLabel.Count > 0 ? demographicdDto.CountryLabel[0] + " " + demographicdDto.CountryData[0].ToString() : "null";

            retVal.LinkedinDemographicRawData.TopFollowerBySeniority =
                demographicdDto.SeniorityData.Count > 0 && demographicdDto.SeniorityLabel.Count > 0 ? demographicdDto.SeniorityLabel[0] + " " + demographicdDto.SeniorityData[0].ToString() : "null";

            retVal.LinkedinDemographicRawData.TopFollowerByIndustry =
                 demographicdDto.IndustryData.Count > 0 && demographicdDto.IndustryLabel.Count > 0 ? demographicdDto.IndustryLabel[0] + " " + demographicdDto.IndustryData[0].ToString() : "null";

            retVal.LinkedinDemographicRawData.TopFollowerByJobFunction =
                 demographicdDto.JobFunctionLabel.Count > 0 && demographicdDto.JobFunctionData.Count > 0 ? demographicdDto.JobFunctionLabel[0] + " " + demographicdDto.JobFunctionData[0].ToString() : "null";

            retVal.LinkedinDemographicRawData.TopFollowerByCompanySize =
                 demographicdDto.CompanySizeLabel.Count > 0 && demographicdDto.CompanySizeData.Count > 0 ? demographicdDto.CompanySizeLabel[0] + " " + demographicdDto.CompanySizeData[0].ToString() : "null";

            return retVal;
        }

        private async Task<PrepareLinkedinAds> PrepareLinkedInAdReport(string htmlString, Guid campaignId, string campaignLogo, string companyLogo, string headerText, string footerText, string headerBgColor, string headerTextColor, bool showFooter, string showPageNumberId, string showPageNumber, bool showHeader, int pageNumber, short type, string startDate, string endDate, PreviousDate previousDate)
        {
            var retVal = new PrepareLinkedinAds();
            retVal.LinkedinAdsCardData = new LinkedinAdsCardData();

            var reporType = string.Empty;
            AnalyticsRoot analyticsRootData = new AnalyticsRoot();
            DempgraphicRoot dempgraphicRoot = new DempgraphicRoot();
            string dateLabelStr = string.Empty;
            string chartStr = string.Empty;

            if (type == (int)ReportTypes.LinkedInAdsCampaign)
            {
                reporType = "CAMPAIGN";
                analyticsRootData = await _linkedinAdService.GetPreparedLinkedinAdData(campaignId.ToString(), reporType, startDate, endDate);
                var tableListStr = JsonConvert.SerializeObject(analyticsRootData.campaignRoot.elements, Formatting.Indented);
                htmlString = htmlString.Replace("_tableArrayList_", tableListStr);
                htmlString = htmlString.Replace("_1ladsCurrency1_", analyticsRootData.campaignRoot.elements[0].currency);
            }
            else if (type == (int)ReportTypes.LinkedInAdsAdgroups)
            {
                reporType = "CAMPAIGN_GROUP";
                analyticsRootData = await _linkedinAdService.GetPreparedLinkedinAdData(campaignId.ToString(), reporType, startDate, endDate);
                var tableListStr = JsonConvert.SerializeObject(analyticsRootData.adGroupRoot.elements, Formatting.Indented);
                htmlString = htmlString.Replace("_tableArrayList_", tableListStr);
                htmlString = htmlString.Replace("_1ladsCurrency1_", analyticsRootData.adGroupRoot.elements[0].currency);
            }
            else if (type == (int)ReportTypes.LinkedInAdsCreative)
            {
                reporType = "CREATIVE";
                analyticsRootData = await _linkedinAdService.GetPreparedLinkedinAdData(campaignId.ToString(), reporType, startDate, endDate);

                if (analyticsRootData.creativeRoot.elements != null)
                {
                    foreach (var element in analyticsRootData.creativeRoot.elements)
                    {
                        if (element.name.Length > 20)
                        {
                            element.name = element.name.Substring(0, 20) + "...";
                        }
                    }
                    var tableListStr = JsonConvert.SerializeObject(analyticsRootData.creativeRoot.elements, Formatting.Indented);
                    htmlString = htmlString.Replace("_tableArrayList_", tableListStr);
                    htmlString = htmlString.Replace("_1ladsCurrency1_", analyticsRootData.creativeRoot.elements[0].currency);
                }

            }

            //prepare chart data
            var chartData = analyticsRootData.linkedinStat.elements.Select(x => x.clicks).ToList();
            chartStr = String.Join(",", chartData);

            var formattedDates = analyticsRootData.linkedinStat.elements
                .Select(x => DateTime.ParseExact(x.date, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("MM-dd"))
                .ToList();

            dateLabelStr = String.Join(",", formattedDates.Select(x => "'" + x + "'"));


            htmlString = htmlString.Replace("_1noOfCampaigns1_", analyticsRootData.cardData.ad_count.ToString());



            // data current
            htmlString = htmlString.Replace("_1ladsClicks1_", analyticsRootData.cardData.total_clicks.ToString());
            htmlString = htmlString.Replace("_1ladsSpent1_", analyticsRootData.cardData.total_spent.ToString());
            htmlString = htmlString.Replace("_1ladsLeads1_", analyticsRootData.cardData.total_leads.ToString());
            htmlString = htmlString.Replace("_1ladsCpl1_", analyticsRootData.cardData.total_cpl.ToString());

            // data previous
            htmlString = htmlString.Replace("_1ladsSpendDiff1_", analyticsRootData.cardData.percent_spent.ToString());
            htmlString = htmlString.Replace("_1ladsClicksDiff1_", analyticsRootData.cardData.percent_clicks.ToString());
            htmlString = htmlString.Replace("_1ladsLeadsDiff1_", analyticsRootData.cardData.percent_leads.ToString());
            htmlString = htmlString.Replace("_1ladsCplDiff1_", analyticsRootData.cardData.percent_cpl.ToString());

            htmlString = htmlString.Replace("_1linkedinAdsLineChartLabels_", dateLabelStr);
            htmlString = htmlString.Replace("_1linkedinAdsLinchartData_", chartStr);

            int intervalRes = formattedDates.Count <= 31 ? 3 : (formattedDates.Count <= 91 ? 7 : 31);
            htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

            var demographicdData = await _linkedinAdService.GetLinkedinAdsDemographic(campaignId.ToString(), "MEMBER_JOB_FUNCTION", startDate, endDate);

            if (demographicdData != null && demographicdData.elements != null)
            {
                foreach (var item in demographicdData.elements)
                {
                    //calculate percentage for demographic

                    double percentage = double.Parse(analyticsRootData.cardData.total_impressions) > 0 ? (double)item.impressions / double.Parse(analyticsRootData.cardData.total_impressions) * 100 : 0;
                    item.percent_impressions = Math.Round(percentage, 2).ToString();

                    double percentage_click = double.Parse(analyticsRootData.cardData.total_clicks) > 0 ? (double)item.clicks / double.Parse(analyticsRootData.cardData.total_clicks) * 100 : 0;
                    item.percent_clicks = Math.Round(percentage_click, 2).ToString();
                }

                var tableListStr1 = JsonConvert.SerializeObject(demographicdData.elements, Formatting.Indented);
                htmlString = htmlString.Replace("_tableArrayList1_", tableListStr1);
            }

            htmlString = htmlString.Replace("_headerImage1_", companyLogo);
            htmlString = htmlString.Replace("_headerImage_", campaignLogo);
            htmlString = htmlString.Replace("_headerText_", headerText);
            htmlString = htmlString.Replace("_footerText_", footerText);
            htmlString = htmlString.Replace("_headerTextColor_", headerTextColor);
            htmlString = htmlString.Replace("_headerBgColor_", headerBgColor);
            htmlString = htmlString.Replace("_showPageNumber_", showPageNumber);
            htmlString = htmlString.Replace("_pageNumberId_", showPageNumberId);
            htmlString = htmlString.Replace("_pageNumber_", "Page " + pageNumber.ToString());

            if (campaignLogo == "")
            {
                htmlString = htmlString.Replace("_showImg2_", "hidden");
            }
            else
            {
                htmlString = htmlString.Replace("_showImg2_", "none");
            }

            if (companyLogo == "")
            {
                htmlString = htmlString.Replace("_showImg1_", "hidden");
            }
            else
            {
                htmlString = htmlString.Replace("_showImg1_", "none");
            }

            if (showHeader)
            {
                htmlString = htmlString.Replace("_showHeader_", "none");
            }
            else
            {
                htmlString = htmlString.Replace("_showHeader_", "hidden");
            }
            if (!showFooter)
            {
                htmlString = htmlString.Replace("_showFooter_", "hidden");
            }
            else
            {
                htmlString = htmlString.Replace("_showFooter_", "none");
            }

            retVal.Html = htmlString;
            retVal.LinkedinAdsCardData = analyticsRootData != null && analyticsRootData.cardData != null ? analyticsRootData.cardData : new LinkedinAdsCardData();

            return retVal;
        }

        private async Task<PrepareInstagramData> PrepareInstagramReport(DateTime startDate, DateTime endDate, string accessToken, string selectedPageName, List<string> subtypes)
        {
            var retVal = new PrepareInstagramData();
            retVal.InstagramReportsData = new InstagramReportsRawData();
            var tempsdt = string.Empty;
            var tempedt = string.Empty;
            List<string> htmlArray = new List<string>();
            var ListOfDates = new List<FacebookReportDates>();
            double d = 0;
            double tempDiff = 0;
            startDate = startDate.AddHours(07);
            endDate = endDate.AddHours(07);
            var diff = CalculateDateSlabDiff(startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

            if (diff > 30)
            {
                d = diff / 30;
                if (d > 0)
                {
                    d = Math.Round(d);

                    for (var i = 0; i <= d; i++)
                    {
                        if (i == 0)
                        {
                            tempsdt = startDate.ToString("yyyy-MM-dd");
                            tempedt = startDate.AddDays(29).ToString("yyyy-MM-dd");
                            tempDiff = (int)diff - 29;

                            var fbDates = new FacebookReportDates();
                            fbDates.startDate = tempsdt;
                            fbDates.endDate = tempedt;
                            ListOfDates.Add(fbDates);
                        }
                        else
                        {
                            tempsdt = Convert.ToDateTime(tempedt).ToString("yyyy-MM-dd");
                            if (tempDiff >= 29)
                            {
                                tempedt = Convert.ToDateTime(tempedt).AddDays(29).ToString("yyyy-MM-dd");
                                tempDiff = tempDiff - 29;
                            }
                            else
                            {
                                tempedt = Convert.ToDateTime(tempedt).AddDays(tempDiff).ToString("yyyy-MM-dd");
                            }

                            var fbDates = new FacebookReportDates();
                            fbDates.startDate = tempsdt;
                            fbDates.endDate = tempedt;
                            ListOfDates.Add(fbDates);
                        }
                    }
                }
            }
            else
            {
                var fbDates = new FacebookReportDates();
                fbDates.startDate = Convert.ToDateTime(startDate).ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture);
                fbDates.endDate = Convert.ToDateTime(endDate).ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture);
                ListOfDates.Add(fbDates);
            }

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://graph.facebook.com/"),
                //DefaultRequestHeaders = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) }
            };

            var prepareUrl = "me/accounts?limit=1000&access_token=" + accessToken;
            ////Get task status
            var response = await httpClient.GetAsync(prepareUrl);
            var data = await response.Content.ReadAsStringAsync();
            var res = JsonConvert.DeserializeObject<dynamic>(data);

            List<InstagramIdModel> instaList = new List<InstagramIdModel> { };
            List<InstagramPageModel> instaPageList = new List<InstagramPageModel> { };
            var rows = res.data;
            for (var i = 0; i < rows.Count; i++)
            {
                string id = rows[i].id;
                var prepareUrl1 = id + "?fields=instagram_business_account,access_token&access_token=" + accessToken;
                var response1 = await httpClient.GetAsync(prepareUrl1);
                var data1 = await response1.Content.ReadAsStringAsync();
                var res1 = JsonConvert.DeserializeObject<dynamic>(data1);
                if (!String.IsNullOrEmpty(Convert.ToString(res1.instagram_business_account)))
                {
                    string instaId = res1.instagram_business_account.id;
                    string accessToken1 = res1.access_token;
                    instaList.Add(new InstagramIdModel { instgramid = instaId, accessToken = accessToken1 });
                }
            }

            for (var j = 0; j < instaList.Count; j++)
            {
                var prepareUrl2 = instaList[j].instgramid + "?fields=name,username,ig_id,id&access_token=" + accessToken;
                var response2 = await httpClient.GetAsync(prepareUrl2);
                var data2 = await response2.Content.ReadAsStringAsync();
                var res2 = JsonConvert.DeserializeObject<dynamic>(data2);
                instaPageList.Add(new InstagramPageModel { instgramid = instaList[j].instgramid, accessToken = instaList[j].accessToken, pageName = res2.name });

            }

            var currPage = instaPageList.Where(x => x.pageName.ToLower() == selectedPageName.ToLower()).FirstOrDefault();

            if (currPage != null)
            {
                var pageId = currPage.instgramid;
                var pageToken = currPage.accessToken;

                int profileViewTotal = 0;
                int followersTotal = 0;
                int impressionsTotal = 0;
                int reachTotal = 0;

                int websiteClickTotal = 0;
                int avgProfileViews = 0;
                int avgFollowers = 0;
                int avgImpressions = 0;
                int avgReachTotals = 0;
                int avgWebSiteClickTotal = 0;

                int totalFemaleFollowers = 0;
                int totalMaleFollowers = 0;
                int fTotalFemaleFollowers = 0;
                int fTotalMaleFollowers = 0;
                List<string> instaPieChartData = new List<string> { };
                List<ListOfInstaLocale> listOfLocale = new List<ListOfInstaLocale> { };
                List<ListOfInstaLocale> listOfCountries = new List<ListOfInstaLocale> { };
                List<ListOfInstaLocale> listOfCities = new List<ListOfInstaLocale> { };

                var instaFollowersCountTotal = 0;

                if (ListOfDates.Count > 1)
                {
                    foreach (var dates in ListOfDates)
                    {
                        var prepareMonthlyData = pageId + "/insights?metric=impressions,reach,website_clicks,profile_views&access_token=" + pageToken + "&since=" + dates.startDate + "&until=" + dates.endDate + "&period=day";
                        var resonseMonthlyData = await httpClient.GetAsync(prepareMonthlyData);
                        var finalData = await resonseMonthlyData.Content.ReadAsStringAsync();
                        var finalDataDynamic = JsonConvert.DeserializeObject<dynamic>(finalData);

                        var instaData = finalDataDynamic.data;
                        for (var j = 0; j < instaData.Count; j++)
                        {
                            var p = instaData[j];
                            if (p.name == "profile_views")
                            {
                                var l = p.values;
                                for (var k = 0; k < l.Count; k++)
                                {
                                    profileViewTotal = profileViewTotal + (int)(l[k].value);
                                }
                                avgProfileViews = profileViewTotal / l.Count;
                            }

                            if (p.name == "impressions")
                            {
                                var l = p.values;
                                for (var k = 0; k < l.Count; k++)
                                {
                                    impressionsTotal = impressionsTotal + (int)(l[k].value);
                                }
                                avgImpressions = impressionsTotal / l.Count;
                            }
                            if (p.name == "reach")
                            {
                                var l = p.values;
                                for (var k = 0; k < l.Count; k++)
                                {
                                    reachTotal = reachTotal + (int)(l[k].value);
                                }
                                avgReachTotals = reachTotal / l.Count;
                            }
                            if (p.name == "website_clicks")
                            {
                                var l = p.values;
                                for (var k = 0; k < l.Count; k++)
                                {
                                    websiteClickTotal = websiteClickTotal + (int)(l[k].value);
                                }
                                avgWebSiteClickTotal = websiteClickTotal / l.Count;
                            }
                        }
                    }
                }
                else
                {
                    var prepareUrl3 = string.Empty;
                    //end date is a less than 1 date from today
                    DateTime currentDateLessOneDay = DateTime.UtcNow.AddDays(-1);
                    currentDateLessOneDay.AddHours(12).AddMinutes(30);
                    var endDateForApi = Convert.ToDateTime(currentDateLessOneDay).ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture);

                    //start date is a less than 30 date from today
                    DateTime currentDateLessThirtyDay = DateTime.UtcNow.AddDays(-30);
                    currentDateLessThirtyDay.AddHours(12).AddMinutes(30);
                    var startDateForApi = Convert.ToDateTime(currentDateLessThirtyDay).ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture);

                    if (DateTime.Parse(ListOfDates[0].endDate, CultureInfo.InvariantCulture) <= DateTime.Parse(endDateForApi, CultureInfo.InvariantCulture) && DateTime.Parse(ListOfDates[0].startDate, CultureInfo.InvariantCulture) >= DateTime.Parse(startDateForApi, CultureInfo.InvariantCulture))
                    {
                        prepareUrl3 = pageId + "/insights/impressions,reach,email_contacts,phone_call_clicks,text_message_clicks,get_directions_clicks,website_clicks,profile_views,follower_count?access_token=" + pageToken + "&since=" + ListOfDates[0].startDate + "&until=" + ListOfDates[0].endDate + "&period=day";
                    }
                    else
                    {
                        prepareUrl3 = pageId + "/insights/impressions,reach,email_contacts,phone_call_clicks,text_message_clicks,get_directions_clicks,website_clicks,profile_views?access_token=" + pageToken + "&since=" + ListOfDates[0].startDate + "&until=" + ListOfDates[0].endDate + "&period=day";
                    }
                    var response3 = await httpClient.GetAsync(prepareUrl3);
                    var data3 = await response3.Content.ReadAsStringAsync();
                    var res3 = JsonConvert.DeserializeObject<dynamic>(data3);

                    var instaData = res3.data;
                    for (var j = 0; j < instaData.Count; j++)
                    {
                        var p = instaData[j];
                        if (p.name == "profile_views")
                        {
                            var l = p.values;
                            for (var k = 0; k < l.Count; k++)
                            {
                                profileViewTotal = profileViewTotal + (int)(l[k].value);
                            }
                            avgProfileViews = profileViewTotal / l.Count;
                        }
                        if (p.name == "follower_count")
                        {
                            var l = p.values;
                            for (var k = 0; k < l.Count; k++)
                            {
                                followersTotal = followersTotal + (int)(l[k].value);
                            }

                            avgFollowers = followersTotal / l.Count;
                        }
                        if (p.name == "impressions")
                        {
                            var l = p.values;
                            for (var k = 0; k < l.Count; k++)
                            {
                                impressionsTotal = impressionsTotal + (int)(l[k].value);
                            }
                            avgImpressions = impressionsTotal / l.Count;
                        }
                        if (p.name == "reach")
                        {
                            var l = p.values;
                            for (var k = 0; k < l.Count; k++)
                            {
                                reachTotal = reachTotal + (int)(l[k].value);
                            }
                            avgReachTotals = reachTotal / l.Count;
                        }
                        if (p.name == "website_clicks")
                        {
                            var l = p.values;
                            for (var k = 0; k < l.Count; k++)
                            {
                                websiteClickTotal = websiteClickTotal + (int)(l[k].value);
                            }
                            avgWebSiteClickTotal = websiteClickTotal / l.Count;
                        }
                    }

                }

                var prepareUrl5 = pageId + "?access_token=" + pageToken + "&fields=followers_count";
                var response5 = await httpClient.GetAsync(prepareUrl5);
                var data5 = await response5.Content.ReadAsStringAsync();
                var res5 = JsonConvert.DeserializeObject<dynamic>(data5);
                instaFollowersCountTotal = res5.followers_count;

                // get insta audiences

                var genderDataChart = new List<int>();

                //for country
                var prepareUrlCountry = pageId + "/insights?metric=follower_demographics&metric_type=total_value&breakdown=['country']&period=lifetime&access_token=" + pageToken;
                var responseCountry = await httpClient.GetAsync(prepareUrlCountry);
                var dataCountry = responseCountry.Content.ReadAsStringAsync().Result;
                var resCountry = JsonConvert.DeserializeObject<FollowerDemographicsDto>(dataCountry);

                if (resCountry.data != null && resCountry.data.Count > 0 &&
                    resCountry.data[0].total_value != null &&
                    resCountry.data[0].total_value.breakdowns != null &&
                    resCountry.data[0].total_value.breakdowns.Count > 0 &&
                    resCountry.data[0].total_value.breakdowns[0].results != null)
                {
                    foreach (var result in resCountry.data[0].total_value.breakdowns[0].results)
                    {
                        var asdf = result.ToListOfInstaLocale();
                        listOfCountries.Add(result.ToListOfInstaLocale());
                    }
                }

                //for city
                var prepareUrlCity = pageId + "/insights?metric=follower_demographics&metric_type=total_value&breakdown=['city']&period=lifetime&access_token=" + pageToken;
                var responseCity = await httpClient.GetAsync(prepareUrlCity);
                var dataCity = responseCity.Content.ReadAsStringAsync().Result;
                var resCity = JsonConvert.DeserializeObject<FollowerDemographicsDto>(dataCity);

                if (resCity.data != null && resCity.data.Count > 0 &&
                resCity.data[0].total_value != null &&
                resCity.data[0].total_value.breakdowns != null &&
                resCity.data[0].total_value.breakdowns.Count > 0 &&
                resCity.data[0].total_value.breakdowns[0].results != null)
                {
                    foreach (var result in resCity.data[0].total_value.breakdowns[0].results)
                    {
                        listOfCities.Add(result.ToListOfInstaLocale());
                    }
                }

                var prepareUrlAgeGender = pageId + "/insights?metric=follower_demographics&metric_type=total_value&breakdown=['age','gender']&period=lifetime&access_token=" + pageToken;
                var responseAgeGender = await httpClient.GetAsync(prepareUrlAgeGender);
                var dataAgeGender = responseAgeGender.Content.ReadAsStringAsync().Result;
                var resAgeGender = JsonConvert.DeserializeObject<FollowerDemographicsDto>(dataAgeGender);

                if (resAgeGender.data != null && resAgeGender.data.Count > 0 &&
                resAgeGender.data[0].total_value != null &&
                resAgeGender.data[0].total_value.breakdowns != null &&
                resAgeGender.data[0].total_value.breakdowns.Count > 0 &&
                resAgeGender.data[0].total_value.breakdowns[0].results != null)
                {

                    //Age group                   
                    var groupedData = resAgeGender.data[0].total_value.breakdowns[0].results
                            .GroupBy(item => item.dimension_values[0])
                            .Select(group => new ListOfInstaLocale
                            {
                                name = group.Key,
                                value = group.Sum(item => item.value)
                            })
                            .ToList();

                    listOfLocale = groupedData;

                    //Male and female

                    foreach (var item in resAgeGender.data[0].total_value.breakdowns[0].results)
                    {
                        if (item.dimension_values[1] == "M")
                        {
                            totalMaleFollowers += item.value;
                        }
                        else if (item.dimension_values[1] == "F")
                        {
                            totalFemaleFollowers += item.value;
                        }
                    }

                    var total = totalFemaleFollowers + totalMaleFollowers;
                    fTotalFemaleFollowers = total > 0 ? (totalFemaleFollowers * 100) / (totalFemaleFollowers + totalMaleFollowers) : 0;
                    fTotalMaleFollowers = total > 0 ? (totalMaleFollowers * 100) / (totalFemaleFollowers + totalMaleFollowers) : 0;
                    genderDataChart.Add(fTotalFemaleFollowers);
                    genderDataChart.Add(fTotalMaleFollowers);
                }


                var pieChartDataStr = String.Join(",", genderDataChart);
                var listCityStr = JsonConvert.SerializeObject(listOfCities);
                var listCountryStr = JsonConvert.SerializeObject(listOfCountries);
                var listLocaleStr = JsonConvert.SerializeObject(listOfLocale);

                foreach (var subtype in subtypes)
                {
                    var intsubtype = Convert.ToInt16(subtype);
                    if (intsubtype == (int)ReportTypes.InstaPerformance)
                    {
                        var htmlString = string.Empty;

                        //string path1 = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/instaPerformance.html");
                        //htmlString = System.IO.File.ReadAllText(path1);

                        string path1 = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/instaPerformance.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(path1).Result;
                        }

                        htmlString = htmlString.Replace("_1instaTotalPageProfileViews_1", profileViewTotal.ToString());
                        htmlString = htmlString.Replace("_1instaTotalNewFollowers_1", followersTotal.ToString());
                        htmlString = htmlString.Replace("_1instaTotalFollowers_1", instaFollowersCountTotal.ToString());
                        htmlString = htmlString.Replace("_1instaAvgPageProfileViews_1", avgProfileViews.ToString());
                        htmlString = htmlString.Replace("_1instaAvgNewFollowers_1", avgFollowers.ToString());

                        htmlArray.Add(htmlString);

                        retVal.InstagramReportsData.PageProfileViewTotal = profileViewTotal;
                        retVal.InstagramReportsData.NewFollowers = followersTotal;
                        retVal.InstagramReportsData.FollowersTotal = instaFollowersCountTotal;
                        retVal.InstagramReportsData.AvgPerDayProfileViews = avgProfileViews;
                        retVal.InstagramReportsData.AvgFollowersPerDay = avgFollowers;


                    }
                    else if (intsubtype == (int)ReportTypes.InstaImpression)
                    {
                        var htmlString = string.Empty;
                        //string path2 = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/instaImpression.html");
                        //htmlString = System.IO.File.ReadAllText(path2);

                        string path2 = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/instaImpression.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(path2).Result;
                        }


                        htmlString = htmlString.Replace("_1instaTotalImpressions_1", impressionsTotal.ToString());
                        htmlString = htmlString.Replace("_1instaTotalReach_1", reachTotal.ToString());
                        htmlString = htmlString.Replace("_1instaTotalWebClicks_1", websiteClickTotal.ToString());


                        htmlString = htmlString.Replace("_1instaAvgMediaImpressions_1", avgImpressions.ToString());
                        htmlString = htmlString.Replace("_1instaAvgMediaReach_1", avgReachTotals.ToString());
                        htmlString = htmlString.Replace("_1instaAvgWebsiteClicksTotal_1", avgWebSiteClickTotal.ToString());

                        htmlArray.Add(htmlString);

                        retVal.InstagramReportsData.IGMediaImpressions = impressionsTotal;
                        retVal.InstagramReportsData.IGMediaReach = reachTotal;
                        retVal.InstagramReportsData.WebsiteReach = websiteClickTotal;
                        retVal.InstagramReportsData.IGMediaImpressionsAvgPerDay = avgImpressions;
                        retVal.InstagramReportsData.IGMediaReachAvgPerDay = avgReachTotals;
                    }
                    else if (intsubtype == (int)ReportTypes.InstAudiance)
                    {
                        var htmlString = string.Empty;
                        string path3 = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/instaAudiance.html");
                        htmlString = System.IO.File.ReadAllText(path3);

                        //string path3 = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/instaAudiance.html");
                        //using (HttpClient httpclient = new HttpClient())
                        //{
                        //    htmlString = httpclient.GetStringAsync(path3).Result;
                        //}

                        htmlString = htmlString.Replace("_1instaGenderPieChartData_", pieChartDataStr);

                        htmlString = htmlString.Replace("_1cityListArray1_", listCityStr);
                        htmlString = htmlString.Replace("_1countryListArray1_", listCountryStr);
                        htmlString = htmlString.Replace("_1localeListArray1_", listLocaleStr);

                        retVal.InstagramReportsData.TopLanguages = listOfLocale.Count > 0 ? listOfLocale.FirstOrDefault().name  + " " + listOfLocale.FirstOrDefault().value : "";
                        retVal.InstagramReportsData.TopFollowerByContry = listOfCountries.Count > 0 ? listOfCountries.FirstOrDefault().name + " " + listOfCountries.FirstOrDefault().value : "";
                        retVal.InstagramReportsData.TopFollowerByCity = listOfCities.Count > 0 ? listOfCities.FirstOrDefault().name + " " + listOfCities.FirstOrDefault().value : "";
                        retVal.InstagramReportsData.GenderPercentage = "Female " +fTotalFemaleFollowers +" Male"+ fTotalMaleFollowers;

                        htmlArray.Add(htmlString);
                    }

                }

            }

            retVal.HtmlList = htmlArray;

            return retVal;
        }

        /// <summary>
        /// Calculate total difference of days
        /// </summary>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <returns>Status of the operation</returns>
        private double CalculateDateSlabDiff(string startDate, string endDate)
        {
            var difference = (Convert.ToDateTime(endDate) - Convert.ToDateTime(startDate)).TotalDays; return difference;
        }

        private async Task<string> GetFbAllAdsAccountDetails(CampaignFacebookAds campaignFacebookAds)
        {
            var retVal = string.Empty;

            try
            {
                var httpClient = new HttpClient
                {
                    BaseAddress = new Uri("https://graph.facebook.com/v22.0/")
                };
                var prepareUrl = "me?fields=id,name,adaccounts.limit(1000){account_id,business_name,id,name}&access_token=" + campaignFacebookAds.AccessToken;

                var response = await httpClient.GetAsync(prepareUrl);

                var data = await response.Content.ReadAsStringAsync();

                var facebookAdsAccount = JsonConvert.DeserializeObject<FacebookAdsAccount>(data);

                return facebookAdsAccount.adaccounts.data.Where(x => x.name == campaignFacebookAds.AdAccountName).Select(x => x.id).FirstOrDefault();
            }
            catch (Exception)
            {
                return retVal;
            }

        }

        public async Task<PrepareFbAdsCampaignData> PrepareFacebookAdsCampaign(string htmlString, Guid campaignId, DateTime startDate, DateTime endDate, string accessToken, CampaignFacebookAds facebookAdsSetup, PreviousDate previousDate, string campaignLogo, string companyLogo, string headerText, string footerText, string headerBgColor, string headerTextColor, bool showFooter, string showPageNumberId, string showPageNumber, bool showHeader, int pageNumber)
        {
            var retVal = new PrepareFbAdsCampaignData();
            retVal.CampaignPerformace = new FbAdsCampaignData();

            var listOfDates = await PrepareDateListForFacebook(startDate, endDate);
            string[] shortDateArray;

            var campaignDataForFbAds = await PrepareFacebookAdsCampaignsView(facebookAdsSetup, startDate, endDate, listOfDates);

            var impressionCurrentChartStr = String.Join(",", campaignDataForFbAds.ImpressionData);
            var impressionPreviousChartStr = String.Join(",", campaignDataForFbAds.PrevImpressionData);

            var reachCurrentChartStr = String.Join(",", campaignDataForFbAds.ReachData);
            var reachPreviousChartStr = String.Join(",", campaignDataForFbAds.PrevReachData);

            var resultCurrentChartStr = String.Join(",", campaignDataForFbAds.ClickData);
            var resultPreviousChartStr = String.Join(",", campaignDataForFbAds.PrevClickData);

            var cprCurrentChartStr = String.Join(",", campaignDataForFbAds.CtrData);
            var cprPreviousChartStr = String.Join(",", campaignDataForFbAds.PrevCtrData);

            var tableString = JsonConvert.SerializeObject(campaignDataForFbAds.listInsights);

            var shortDate = campaignDataForFbAds.shortDate.Select(date => date.ToString()).ToArray();
            var dateLabelStr = String.Join(",", shortDate.Select(x => "'" + x + "'"));

            htmlString = htmlString.Replace("_1fbAdsImpressionsTotal1_", campaignDataForFbAds.totalImpressions.ToString());
            htmlString = htmlString.Replace("_1fbAdsReachTotal1_", campaignDataForFbAds.totalReachs.ToString());
            htmlString = htmlString.Replace("_1fbAdsResultTotal1_", campaignDataForFbAds.results.ToString());
            htmlString = htmlString.Replace("_1fbAdsCPRTotal1_", campaignDataForFbAds.cpr.ToString());
            htmlString = htmlString.Replace("_1fbAdsAmtSpentTotal1_", campaignDataForFbAds.spends.ToString());
            htmlString = htmlString.Replace("_fbAdsCurrency_", string.IsNullOrEmpty(campaignDataForFbAds.currency) ? "" : campaignDataForFbAds.currency);
            htmlString = htmlString.Replace("_fbAdsCurrencyTable_", string.IsNullOrEmpty(campaignDataForFbAds.currency) ? "" : "'" + campaignDataForFbAds.currency + "'");

            int intervalRes = shortDate.Length <= 31 ? 3 : (shortDate.Length <= 91 ? 7 : 31);
            htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

            htmlString = htmlString.Replace("_fbadsImpressionChartLabels_", dateLabelStr);
            htmlString = htmlString.Replace("_fbadsReachChartLabels_", dateLabelStr);
            htmlString = htmlString.Replace("_fbadsResultChartLabels_", dateLabelStr);
            htmlString = htmlString.Replace("_fbadsCPRChartLabels_", dateLabelStr);

            htmlString = htmlString.Replace("_fbadsChartImpression_", impressionCurrentChartStr);
            htmlString = htmlString.Replace("_fbadsDiffChartImpression_", impressionPreviousChartStr);

            htmlString = htmlString.Replace("_1fbadsChartReach1_", reachCurrentChartStr);
            htmlString = htmlString.Replace("_1fbadsDiffChartReach1_", reachPreviousChartStr);

            htmlString = htmlString.Replace("_2fbadsChartResult2_", resultCurrentChartStr);
            htmlString = htmlString.Replace("_2fbadsDiffChartResult2_", resultPreviousChartStr);

            htmlString = htmlString.Replace("_3fbadsChartCPR3_", cprCurrentChartStr);
            htmlString = htmlString.Replace("_3fbadsDiffChartCPR3_", cprPreviousChartStr);

            htmlString = htmlString.Replace("_tableArrayList_", tableString);


            //different calculation for impression
            var data = (campaignDataForFbAds.ImpressionData.Length > 0 ? campaignDataForFbAds.ImpressionData.Sum(x => x).ToString() : "0") + "--" + (campaignDataForFbAds.PrevImpressionData.Length > 0 ? campaignDataForFbAds.PrevImpressionData.Sum(x => x).ToString() : "0");
            var dataDifference = PrepareDataGa4(data);

            //difference logic
            htmlString = htmlString.Replace("_fbAdImpDifference_", dataDifference);

            var str = dataDifference;
            var hasPlusSign = str.Contains("+");
            if (hasPlusSign)
            {
                htmlString = htmlString.Replace("_fbAdImpColor_", "green");
            }
            else
            {
                htmlString = htmlString.Replace("_fbAdImpColor_", "red");
            }

            //for reach
            var data1 = (campaignDataForFbAds.ReachData.Length > 0 ? campaignDataForFbAds.ReachData.Sum(x => x).ToString() : "0") + "--" + (campaignDataForFbAds.PrevReachData.Length > 0 ? campaignDataForFbAds.PrevReachData.Sum(x => x).ToString() : "0");
            var dataDifference1 = PrepareDataGa4(data1);

            //difference logic
            htmlString = htmlString.Replace("_fbAdReachDifference_", dataDifference1);

            var str1 = dataDifference1;
            var hasPlusSign1 = str1.Contains("+");
            if (hasPlusSign1)
            {
                htmlString = htmlString.Replace("_fbAdReachColor_", "green");
            }
            else
            {
                htmlString = htmlString.Replace("_fbAdReachColor_", "red");
            }

            //for CLICKS
            var data2 = (campaignDataForFbAds.ClickData.Length > 0 ? campaignDataForFbAds.ClickData.Sum(x => x).ToString() : "0") + "--" + (campaignDataForFbAds.PrevClickData.Length > 0 ? campaignDataForFbAds.PrevClickData.Sum(x => x).ToString() : "0");
            var dataDifference2 = PrepareDataGa4(data2);

            //difference logic
            htmlString = htmlString.Replace("_fbAdResultDifference_", dataDifference2);

            var str2 = dataDifference2;
            var hasPlusSign2 = str2.Contains("+");
            if (hasPlusSign2)
            {
                htmlString = htmlString.Replace("_fbAdResultColor_", "green");
            }
            else
            {
                htmlString = htmlString.Replace("_fbAdResultColor_", "red");
            }

            //for CTR
            var data3 = (campaignDataForFbAds.CtrData.Length > 0 ? campaignDataForFbAds.CtrData.Sum(x => x).ToString() : "0") + "--" + (campaignDataForFbAds.PrevCtrData.Length > 0 ? campaignDataForFbAds.PrevCtrData.Sum(x => x).ToString() : "0");
            var dataDifference3 = PrepareDataGa4(data3);

            //difference logic
            htmlString = htmlString.Replace("_fbCpcDifference_", dataDifference3);

            var str3 = dataDifference3;
            var hasPlusSign3 = str3.Contains("+");
            if (hasPlusSign3)
            {
                htmlString = htmlString.Replace("_fbAdCpcColor_", "green");
            }
            else
            {
                htmlString = htmlString.Replace("_fbAdCpcColor_", "red");
            }


            htmlString = htmlString.Replace("_headerImage1_", companyLogo);
            htmlString = htmlString.Replace("_headerImage_", campaignLogo);
            htmlString = htmlString.Replace("_headerText_", headerText);
            htmlString = htmlString.Replace("_footerText_", footerText);
            htmlString = htmlString.Replace("_headerTextColor_", headerTextColor);
            htmlString = htmlString.Replace("_headerBgColor_", headerBgColor);
            htmlString = htmlString.Replace("_showPageNumber_", showPageNumber);
            htmlString = htmlString.Replace("_pageNumberId_", showPageNumberId);
            htmlString = htmlString.Replace("_pageNumber_", "Page " + pageNumber.ToString());
            if (showHeader)
            {
                htmlString = htmlString.Replace("_showHeader_", "none");
            }
            else
            {
                htmlString = htmlString.Replace("_showHeader_", "hidden");
            }
            if (!showFooter)
            {
                htmlString = htmlString.Replace("_showFooter_", "hidden");
            }
            else
            {
                htmlString = htmlString.Replace("_showFooter_", "none");
            }
            if (campaignLogo == "")
            {
                htmlString = htmlString.Replace("_showImg2_", "hidden");
            }
            else
            {
                htmlString = htmlString.Replace("_showImg2_", "none");
            }

            if (companyLogo == "")
            {
                htmlString = htmlString.Replace("_showImg1_", "hidden");
            }
            else
            {
                htmlString = htmlString.Replace("_showImg1_", "none");
            }

            retVal.CampaignPerformace.Impressions = dataDifference;
            retVal.CampaignPerformace.Reachs = dataDifference1;
            retVal.CampaignPerformace.Clicks = dataDifference2;
            retVal.CampaignPerformace.Ctr = dataDifference3;
            retVal.CampaignPerformace.Spend = campaignDataForFbAds.totalSpend.ToString();

            retVal.Html = htmlString;

            return retVal;
        }

        public async Task<PrepareFbAdsSetData> PrepareFacebookAdsGroups(string htmlString, Guid campaignId, DateTime startDate, DateTime endDate, string accessToken, CampaignFacebookAds facebookAdsSetup, PreviousDate previousDate, string campaignLogo, string companyLogo, string headerText, string footerText, string headerBgColor, string headerTextColor, bool showFooter, string showPageNumberId, string showPageNumber, bool showHeader, int pageNumber)
        {
            var retVal = new PrepareFbAdsSetData();
            retVal.FbAdsSetData = new FbAdsSetData();

            string pathFbAds1 = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/facebookAdsGroups.html");

            using (HttpClient httpclient = new HttpClient())
            {
                htmlString = httpclient.GetStringAsync(pathFbAds1).Result;
            }

            //string pathFbAds1 = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/facebookAdsGroups.html");
            //htmlString = System.IO.File.ReadAllText(pathFbAds1);

            var listOfDates = await PrepareDateListForFacebook(startDate, endDate);

            var campaignDataForFbAds = await PrepareFacebookAdsAdsGroupView(facebookAdsSetup, startDate, endDate, listOfDates);
            var tableString = JsonConvert.SerializeObject(campaignDataForFbAds.listInsights);
            var impressionChartStr = String.Join(",", campaignDataForFbAds.ImpressionData);

            var shortDate = campaignDataForFbAds.shortDate.Select(date => date.ToString()).ToArray();
            var dateLabelStr = String.Join(",", shortDate.Select(x => "'" + x + "'"));

            htmlString = htmlString.Replace("_fbAdsImpressionData_", campaignDataForFbAds.totalImpressions.ToString());
            htmlString = htmlString.Replace("_fbAdReachData_", campaignDataForFbAds.totalReachs.ToString());
            htmlString = htmlString.Replace("_fbAdsResultData_", campaignDataForFbAds.totalClicks.ToString());
            htmlString = htmlString.Replace("_fbAdcostPerResultData_", campaignDataForFbAds.cpc.ToString());
            htmlString = htmlString.Replace("_fbAdsAmtSpentData_", campaignDataForFbAds.spends.ToString());
            htmlString = htmlString.Replace("_fbAdsLinkClickData_", campaignDataForFbAds.totalLinkClick.ToString());
            htmlString = htmlString.Replace("_fbAdsCtrData_", campaignDataForFbAds.ctr.ToString("0.00"));
            htmlString = htmlString.Replace("_fbAdsCostPerLinkData_", campaignDataForFbAds.totalCplc.ToString());
            htmlString = htmlString.Replace("_fbAdsCurrency_", string.IsNullOrEmpty(campaignDataForFbAds.currency) ? "" : campaignDataForFbAds.currency);
            htmlString = htmlString.Replace("_fbAdsCurrencyTable_", string.IsNullOrEmpty(campaignDataForFbAds.currency) ? "" : "'" + campaignDataForFbAds.currency + "'");

            htmlString = htmlString.Replace("_1fbAdsLineChartLabels_", dateLabelStr);
            htmlString = htmlString.Replace("_1fbAdsLinchartData_", impressionChartStr);

            int intervalRes = shortDate.Length <= 31 ? 3 : (shortDate.Length <= 91 ? 7 : 31);
            htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

            htmlString = htmlString.Replace("_tableArrayList_", tableString);
            htmlString = htmlString.Replace("_headerImage1_", companyLogo);
            htmlString = htmlString.Replace("_headerImage_", campaignLogo);
            htmlString = htmlString.Replace("_headerText_", headerText);
            htmlString = htmlString.Replace("_footerText_", footerText);
            htmlString = htmlString.Replace("_headerTextColor_", headerTextColor);
            htmlString = htmlString.Replace("_headerBgColor_", headerBgColor);
            htmlString = htmlString.Replace("_showPageNumber_", showPageNumber);
            htmlString = htmlString.Replace("_pageNumberId_", showPageNumberId);
            htmlString = htmlString.Replace("_pageNumber_", "Page " + pageNumber.ToString());

            if (campaignLogo == "")
            {
                htmlString = htmlString.Replace("_showImg2_", "hidden");
            }
            else
            {
                htmlString = htmlString.Replace("_showImg2_", "none");
            }

            if (companyLogo == "")
            {
                htmlString = htmlString.Replace("_showImg1_", "hidden");
            }
            else
            {
                htmlString = htmlString.Replace("_showImg1_", "none");
            }

            if (showHeader)
            {
                htmlString = htmlString.Replace("_showHeader_", "none");
            }
            else
            {
                htmlString = htmlString.Replace("_showHeader_", "hidden");
            }
            if (!showFooter)
            {
                htmlString = htmlString.Replace("_showFooter_", "hidden");
            }
            else
            {
                htmlString = htmlString.Replace("_showFooter_", "none");
            }

            var data7 = (campaignDataForFbAds.ReachData.Length > 0 ? campaignDataForFbAds.ReachData.Sum(x => x).ToString() : "0") + "--" + (campaignDataForFbAds.PrevReachData.Length > 0 ? campaignDataForFbAds.PrevReachData.Sum(x => x).ToString() : "0");
            var dataDifference7 = PrepareDataGa4(data7);
            retVal.FbAdsSetData.Reachs = dataDifference7;


            var data = (campaignDataForFbAds.ImpressionData.Length > 0 ? campaignDataForFbAds.ImpressionData.Sum(x => x).ToString() : "0") + "--" + (campaignDataForFbAds.PrevImpressionData.Length > 0 ? campaignDataForFbAds.PrevImpressionData.Sum(x => x).ToString() : "0");
            var dataDifference = PrepareDataGa4(data);
            retVal.FbAdsSetData.Impressions = dataDifference;

            var data1 = (campaignDataForFbAds.ClickData.Length > 0 ? campaignDataForFbAds.ClickData.Sum(x => x).ToString() : "0") + "--" + (campaignDataForFbAds.PrevClickData.Length > 0 ? campaignDataForFbAds.PrevClickData.Sum(x => x).ToString() : "0");
            var dataDifference1 = PrepareDataGa4(data1);
            retVal.FbAdsSetData.Clicks = dataDifference1;

            var data2 = (campaignDataForFbAds.CpcData.Length > 0 ? campaignDataForFbAds.CpcData.Sum(x => x).ToString() : "0") + "--" + (campaignDataForFbAds.PrevCpcData.Length > 0 ? campaignDataForFbAds.PrevCpcData.Sum(x => x).ToString() : "0");
            var dataDifference2 = PrepareDataGa4(data2);
            retVal.FbAdsSetData.Cpc = dataDifference2;

            var data3 = (campaignDataForFbAds.SpendData.Length > 0 ? campaignDataForFbAds.SpendData.Sum(x => x).ToString() : "0") + "--" + (campaignDataForFbAds.PrevSpendData.Length > 0 ? campaignDataForFbAds.PrevSpendData.Sum(x => x).ToString() : "0");
            var dataDifference3 = PrepareDataGa4(data3);
            retVal.FbAdsSetData.Spend = dataDifference3;

            var data4 = (campaignDataForFbAds.LinkClickData.Length > 0 ? campaignDataForFbAds.LinkClickData.Sum(x => x).ToString() : "0") + "--" + (campaignDataForFbAds.PrevClickData.Length > 0 ? campaignDataForFbAds.PrevClickData.Sum(x => x).ToString() : "0");
            var dataDifference4 = PrepareDataGa4(data4);
            retVal.FbAdsSetData.LinkClick = dataDifference4;

            var data5 = (campaignDataForFbAds.CtrData.Length > 0 ? campaignDataForFbAds.CtrData.Sum(x => x).ToString() : "0") + "--" + (campaignDataForFbAds.PrevCtrData.Length > 0 ? campaignDataForFbAds.PrevCtrData.Sum(x => x).ToString() : "0");
            var dataDifference5 = PrepareDataGa4(data5);
            retVal.FbAdsSetData.Ctr = dataDifference5;

            var data6 = (campaignDataForFbAds.CplcData.Length > 0 ? campaignDataForFbAds.CplcData.Sum(x => x).ToString() : "0") + "--" + (campaignDataForFbAds.PrevCplcData.Length > 0 ? campaignDataForFbAds.PrevCplcData.Sum(x => x).ToString() : "0");
            var dataDifference6 = PrepareDataGa4(data6);
            retVal.FbAdsSetData.Cplc = dataDifference6;

            retVal.Html = htmlString;

            return retVal;
        }

        public async Task<string> PrepareFacebookAdsCopies(string htmlString, Guid campaignId, DateTime startDate, DateTime endDate, string accessToken, CampaignFacebookAds facebookAdsSetup, PreviousDate previousDate, string campaignLogo, string companyLogo, string headerText, string footerText, string headerBgColor, string headerTextColor, bool showFooter, string showPageNumberId, string showPageNumber, bool showHeader, int pageNumber)
        {
            var listOfDates = await PrepareDateListForFacebook(startDate, endDate);

            string pathFbAds1 = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/facebookAdsCopies.html");

            using (HttpClient httpclient = new HttpClient())
            {
                htmlString = httpclient.GetStringAsync(pathFbAds1).Result;
            }

            //string pathFbAds1 = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/facebookAdsCopies.html");
            //htmlString = System.IO.File.ReadAllText(pathFbAds1);


            var campaignDataForFbAds = await PrepareFacebookAdsCopiesView(facebookAdsSetup, startDate, endDate, listOfDates);
            var tableString = JsonConvert.SerializeObject(campaignDataForFbAds.listInsights);

            htmlString = htmlString.Replace("_tableArrayList_", tableString);
            htmlString = htmlString.Replace("_fbAdsCurrencyTable_", string.IsNullOrEmpty(campaignDataForFbAds.currency) ? "" : "'" + campaignDataForFbAds.currency + "'");
            htmlString = htmlString.Replace("_headerImage1_", companyLogo);
            htmlString = htmlString.Replace("_headerImage_", campaignLogo);
            htmlString = htmlString.Replace("_headerText_", headerText);
            htmlString = htmlString.Replace("_footerText_", footerText);
            htmlString = htmlString.Replace("_headerTextColor_", headerTextColor);
            htmlString = htmlString.Replace("_headerBgColor_", headerBgColor);
            htmlString = htmlString.Replace("_showPageNumber_", showPageNumber);
            htmlString = htmlString.Replace("_pageNumberId_", showPageNumberId);
            htmlString = htmlString.Replace("_pageNumber_", "Page " + pageNumber.ToString());
            if (showHeader)
            {
                htmlString = htmlString.Replace("_showHeader_", "none");
            }
            else
            {
                htmlString = htmlString.Replace("_showHeader_", "hidden");
            }
            if (!showFooter)
            {
                htmlString = htmlString.Replace("_showFooter_", "hidden");
            }
            else
            {
                htmlString = htmlString.Replace("_showFooter_", "none");
            }
            if (campaignLogo == "")
            {
                htmlString = htmlString.Replace("_showImg2_", "hidden");
            }
            else
            {
                htmlString = htmlString.Replace("_showImg2_", "none");
            }

            if (companyLogo == "")
            {
                htmlString = htmlString.Replace("_showImg1_", "hidden");
            }
            else
            {
                htmlString = htmlString.Replace("_showImg1_", "none");
            }

            return htmlString;
        }

        public async Task<List<FacebookReportDates>> PrepareDateListForFacebook(DateTime startDate, DateTime endDate)
        {
            var tempsdt = string.Empty;
            var tempedt = string.Empty;
            var listOfDates = new List<FacebookReportDates>();
            double d = 0;
            double tempDiff = 0;
            var startDateModified = startDate;
            var endDateModified = endDate;
            var diff = CalculateDateSlabDiff(startDateModified.ToString("yyyy-MM-dd"), endDateModified.ToString("yyyy-MM-dd"));

            if (diff > 30)
            {
                d = diff / 30;
                if (d > 0)
                {
                    d = Math.Round(d);

                    for (var i = 1; i <= d; i++)
                    {
                        if (i == 1)
                        {
                            tempsdt = startDateModified.ToString("yyyy-MM-dd");
                            tempedt = startDateModified.AddDays(30).ToString("yyyy-MM-dd");
                            tempDiff = (int)diff - 30;

                            var fbDates = new FacebookReportDates();
                            fbDates.startDate = tempsdt;
                            fbDates.endDate = tempedt;

                            listOfDates.Add(fbDates);
                        }
                        else
                        {
                            tempsdt = Convert.ToDateTime(tempedt).AddDays(1).ToString("yyyy-MM-dd");
                            if (tempDiff >= 30)
                            {
                                tempedt = Convert.ToDateTime(tempedt).AddDays(30).ToString("yyyy-MM-dd");
                                tempDiff = tempDiff - 30;
                            }
                            else
                            {
                                tempedt = Convert.ToDateTime(tempedt).AddDays(tempDiff).ToString("yyyy-MM-dd");
                            }

                            var fbDates = new FacebookReportDates();
                            fbDates.startDate = tempsdt;
                            fbDates.endDate = tempedt;
                            listOfDates.Add(fbDates);
                        }
                    }
                }
            }
            else
            {
                List<FacebookReportDates> allDates = new List<FacebookReportDates>();
                for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    var newdate = new FacebookReportDates();
                    newdate.startDate = date.ToString("yyyy-MM-dd");
                    newdate.endDate = date.ToString("yyyy-MM-dd");
                    allDates.Add(newdate);
                }
                listOfDates = allDates;
            }

            return listOfDates;
        }
        public async Task<FacebookAdsCampaignData> PrepareFacebookAdsCampaignsView(CampaignFacebookAds facebookAdsSetup, DateTime startDate,
        DateTime endDate, List<FacebookReportDates> dateList)
        {
            var returnValue = new FacebookAdsCampaignData();

            //Current data          
            List<InsightsField> listInsights = new List<InsightsField>();
            List<InsightsField> finalListInsights = new List<InsightsField>();
            List<AdsetConfig> listConfig = new List<AdsetConfig>();

            List<InsightsField> preListInsights = new List<InsightsField>();
            List<AdsetConfig> preListConfig = new List<AdsetConfig>();

            var facebookAdsAccountID = await GetFbAllAdsAccountDetails(facebookAdsSetup);

            if (!string.IsNullOrEmpty(facebookAdsAccountID))
            {
                var fbAdsCampaigns = await GetFBAdsCampaigns(facebookAdsSetup.AccessToken, facebookAdsAccountID, "campaign", startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

                if (fbAdsCampaigns != null)
                {
                    //var fbAdsSetResponse = await GetFBAdsSets(facebookAdsSetup.AccessToken, facebookAdsAccountID);

                    // var tableString = JsonConvert.SerializeObject(finalListInsights);

                    var batchRequests = new List<BatchDto>();

                    for (var i = 0; i < dateList.Count; i++)
                    {
                        var relativeUrl = $"v22.0/" + facebookAdsAccountID + "/insights?level=campaign&limit=2000&fields=account_currency,impressions,reach,clicks,ctr,spend,campaign_name,ad_name,adset_name,ad_id,campaign_id,adset_id,account_id&time_range={'since':" + "'" + dateList[i].startDate + "'" + ",'until':" + "'" + dateList[i].endDate + "'}";
                        batchRequests.Add(new BatchDto
                        {
                            method = "GET",
                            relative_url = relativeUrl
                        });

                    }

                    var batchRes = await BatchRequest(facebookAdsSetup.AccessToken, batchRequests);

                    int[] reachData = Mapper.Map<int[]>(batchRes.Reach);
                    int[] impressionData = Mapper.Map<int[]>(batchRes.Impressions);
                    int[] clickData = Mapper.Map<int[]>(batchRes.Click);
                    decimal[] ctrData = Mapper.Map<decimal[]>(batchRes.Ctr);
                    decimal[] spendData = Mapper.Map<decimal[]>(batchRes.Spend);

                    returnValue.ReachData = reachData;
                    returnValue.ImpressionData = impressionData;
                    returnValue.ClickData = clickData;
                    returnValue.CtrData = ctrData;

                    returnValue.totalReachs = fbAdsCampaigns.data.Sum(x => Convert.ToDouble(x.reach));
                    returnValue.totalImpressions = fbAdsCampaigns.data.Sum(x => Convert.ToDouble(x.impressions));
                    returnValue.totalClicks = fbAdsCampaigns.data.Sum(x => Convert.ToDouble(x.clicks));
                    returnValue.totalCtr = fbAdsCampaigns.data.Sum(x => Convert.ToDecimal(x.ctr));
                    returnValue.spends = fbAdsCampaigns.data.Sum(x => Convert.ToDecimal(x.spend));

                    //Previous Data

                    var prevDate = CalculatePreviousStartDateAndEndDate(startDate, endDate);
                    var listOfPrevDates = await PrepareDateListForFacebook(prevDate.PreviousStartDate, prevDate.PreviousEndDate);

                    var pevBatchRequests = new List<BatchDto>();

                    for (var i = 0; i < listOfPrevDates.Count; i++)
                    {
                        var relativeUrl = $"v22.0/" + facebookAdsAccountID + "/insights?level=campaign&limit=2000&fields=impressions,reach,clicks,ctr,spend,campaign_name,ad_name,adset_name,ad_id,campaign_id,adset_id,account_id&time_range={'since':" + "'" + listOfPrevDates[i].startDate + "'" + ",'until':" + "'" + listOfPrevDates[i].endDate + "'}";
                        pevBatchRequests.Add(new BatchDto
                        {
                            method = "GET",
                            relative_url = relativeUrl
                        });
                    }

                    var prevBatchRes = await BatchRequest(facebookAdsSetup.AccessToken, pevBatchRequests);

                    //Get Currency Icon


                    int[] prevReachData = Mapper.Map<int[]>(prevBatchRes.Reach);
                    int[] prevImpressionData = Mapper.Map<int[]>(prevBatchRes.Impressions);
                    int[] prevClickData = Mapper.Map<int[]>(prevBatchRes.Click);
                    decimal[] prevCtrData = Mapper.Map<decimal[]>(prevBatchRes.Ctr);
                    decimal[] prevSpendData = Mapper.Map<decimal[]>(prevBatchRes.Spend);

                    returnValue.PrevReachData = prevReachData;
                    returnValue.PrevImpressionData = prevImpressionData;
                    returnValue.PrevClickData = prevClickData;
                    returnValue.PrevCtrData = prevCtrData;

                    var currency = fbAdsCampaigns.data.Select(x => x.account_currency).FirstOrDefault();

                    var currencyCode = new List<Currency>();
                    var restClient = new RestClient(_configuration.GetSection("BlobUrl").Value + "Json");
                    var restRequest = new RestRequest("/currency_code.json", Method.Get);

                    var responseCode = restClient.GetAsync(restRequest).Result;
                    if (responseCode.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        currencyCode = JsonConvert.DeserializeObject<List<Currency>>(responseCode.Content);
                    }

                    var currency_symbol = currencyCode.Where(y => y.code == currency).Select(x => x.symbol).FirstOrDefault();

                    returnValue.currency = currency_symbol;

                    returnValue.shortDate = dateList.Select(date => date.startDate).ToList();

                    returnValue.listInsights = fbAdsCampaigns.data.Count() > 0 ? fbAdsCampaigns.data : new List<InsightsField>();
                }
                return returnValue;
            }

            return returnValue;
        }

        public async Task<FacebookAdsCampaignData> PrepareFacebookAdsAdsGroupView(CampaignFacebookAds facebookAdsSetup, DateTime startDate,
        DateTime endDate, List<FacebookReportDates> dateList)
        {
            var returnValue = new FacebookAdsCampaignData();

            var facebookAdsAccountID = await GetFbAllAdsAccountDetails(facebookAdsSetup);

            if (!string.IsNullOrEmpty(facebookAdsAccountID))
            {
                var fbAdsCampaigns = await GetFBAdsCampaigns(facebookAdsSetup.AccessToken, facebookAdsAccountID, "adset", startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

                var batchRequests = new List<BatchDto>();


                for (var i = 0; i < dateList.Count; i++)
                {
                    var relativeUrl = $"v22.0/" + facebookAdsAccountID + "/insights?level=adset&limit=2000&fields=cpc,inline_link_clicks,cost_per_unique_inline_link_click,account_currency,impressions,reach,clicks,ctr,spend,campaign_name,ad_name,adset_name,ad_id,campaign_id,adset_id,account_id&time_range={'since':" + "'" + dateList[i].startDate + "'" + ",'until':" + "'" + dateList[i].endDate + "'}";
                    batchRequests.Add(new BatchDto
                    {
                        method = "GET",
                        relative_url = relativeUrl
                    });

                }

                var batchRes = await BatchRequest(facebookAdsSetup.AccessToken, batchRequests);

                //Card Value
                returnValue.totalClicks = fbAdsCampaigns.data.Sum(x => Convert.ToDouble(x.clicks));
                returnValue.totalReachs = fbAdsCampaigns.data.Sum(x => Convert.ToDouble(x.reach));
                returnValue.totalImpressions = fbAdsCampaigns.data.Sum(x => Convert.ToDouble(x.impressions));


                returnValue.spends = fbAdsCampaigns.data.Sum(x => Convert.ToDecimal(x.spend));

                returnValue.totalCplc = fbAdsCampaigns.data.Sum(x => Convert.ToDecimal(x.cost_per_unique_inline_link_click));
                returnValue.totalLinkClick = fbAdsCampaigns.data.Sum(x => Convert.ToDouble(x.inline_link_clicks));
                returnValue.ctr = fbAdsCampaigns.data.Sum(x => Convert.ToDecimal(x.ctr));
                returnValue.cpc = fbAdsCampaigns.data.Sum(x => Convert.ToDecimal(x.cpc));


                int[] reachData = Mapper.Map<int[]>(batchRes.Reach);
                int[] impressionData = Mapper.Map<int[]>(batchRes.Impressions);
                decimal[] spendData = Mapper.Map<decimal[]>(batchRes.Spend);
                decimal[] ctrData = Mapper.Map<decimal[]>(batchRes.Ctr);
                int[] clickData = Mapper.Map<int[]>(batchRes.Click);

                int[] linkClickData = Mapper.Map<int[]>(batchRes.LinkClick);
                decimal[] cplcData = Mapper.Map<decimal[]>(batchRes.CPLC);
                decimal[] cpcData = Mapper.Map<decimal[]>(batchRes.CPC);

                returnValue.ImpressionData = impressionData;
                returnValue.ReachData = reachData;
                returnValue.ClickData = clickData;
                returnValue.CpcData = cpcData;

                returnValue.SpendData = spendData;
                returnValue.LinkClickData = linkClickData;
                returnValue.CtrData = ctrData;
                returnValue.CplcData = cplcData;


                //Previous Date Calculation
                var prevDate = CalculatePreviousStartDateAndEndDate(startDate, endDate);
                var listOfPrevDates = await PrepareDateListForFacebook(prevDate.PreviousStartDate, prevDate.PreviousEndDate);
                var prevBatchRequests = new List<BatchDto>();

                for (var i = 0; i < listOfPrevDates.Count; i++)
                {
                    var relativeUrl = $"v22.0/" + facebookAdsAccountID + "/insights?level=adset&limit=2000&fields=cpc,inline_link_clicks,cost_per_unique_inline_link_click,account_currency,impressions,reach,clicks,ctr,spend,campaign_name,ad_name,adset_name,ad_id,campaign_id,adset_id,account_id&time_range={'since':" + "'" + listOfPrevDates[i].startDate + "'" + ",'until':" + "'" + listOfPrevDates[i].endDate + "'}";
                    prevBatchRequests.Add(new BatchDto
                    {
                        method = "GET",
                        relative_url = relativeUrl
                    });

                }

                var prevBatchRes = await BatchRequest(facebookAdsSetup.AccessToken, prevBatchRequests);


                int[] prevReachData = Mapper.Map<int[]>(prevBatchRes.Reach);
                int[] prevImpressionData = Mapper.Map<int[]>(prevBatchRes.Impressions);
                decimal[] prevSpendData = Mapper.Map<decimal[]>(prevBatchRes.Spend);
                decimal[] prevCtrData = Mapper.Map<decimal[]>(prevBatchRes.Ctr);
                int[] prevClickData = Mapper.Map<int[]>(prevBatchRes.Click);

                int[] prevLink = Mapper.Map<int[]>(prevBatchRes.LinkClick);
                decimal[] prevCplc = Mapper.Map<decimal[]>(prevBatchRes.CPLC);
                decimal[] prevCpcData = Mapper.Map<decimal[]>(prevBatchRes.CPC);


                //for previous data
                returnValue.PrevImpressionData = prevImpressionData;
                returnValue.PrevReachData = prevReachData;
                returnValue.PrevClickData = prevClickData;
                returnValue.PrevCpcData = prevCpcData;

                returnValue.PrevSpendData = prevSpendData;
                returnValue.PrevLinkClickData = prevLink;
                returnValue.PrevCtrData = prevCtrData;
                returnValue.PrevCplcData = prevCplc;

                var currency = fbAdsCampaigns.data.Select(x => x.account_currency).FirstOrDefault();

                var currencyCode = new List<Currency>();
                var restClient = new RestClient(_configuration.GetSection("BlobUrl").Value + "Json");
                var restRequest = new RestRequest("/currency_code.json", Method.Get);

                var responseCode = restClient.GetAsync(restRequest).Result;
                if (responseCode.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    currencyCode = JsonConvert.DeserializeObject<List<Currency>>(responseCode.Content);
                }

                var currency_symbol = currencyCode.Where(y => y.code == currency).Select(x => x.symbol).FirstOrDefault();

                returnValue.currency = currency_symbol;

                returnValue.shortDate = dateList.Select(date => date.startDate).ToList();

                returnValue.listInsights = fbAdsCampaigns.data.Count() > 0 ? fbAdsCampaigns.data : new List<InsightsField>();

                return returnValue;
            }

            return returnValue;
        }

        public async Task<FacebookAdsCampaignData> PrepareFacebookAdsCopiesView(CampaignFacebookAds facebookAdsSetup, DateTime startDate,
        DateTime endDate, List<FacebookReportDates> dateList)
        {
            var returnValue = new FacebookAdsCampaignData();

            List<InsightsField> listInsights = new List<InsightsField>();

            var facebookAdsAccountID = await GetFbAllAdsAccountDetails(facebookAdsSetup);

            if (!string.IsNullOrEmpty(facebookAdsAccountID))
            {
                var fbAdsCampaigns = await GetFBAdsCampaigns(facebookAdsSetup.AccessToken, facebookAdsAccountID, "ad", startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

                var currency = fbAdsCampaigns.data.Select(x => x.account_currency).FirstOrDefault();

                var currencyCode = new List<Currency>();
                var restClient = new RestClient(_configuration.GetSection("BlobUrl").Value + "Json");
                var restRequest = new RestRequest("/currency_code.json", Method.Get);

                var responseCode = restClient.GetAsync(restRequest).Result;
                if (responseCode.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    currencyCode = JsonConvert.DeserializeObject<List<Currency>>(responseCode.Content);
                }

                var currency_symbol = currencyCode.Where(y => y.code == currency).Select(x => x.symbol).FirstOrDefault();

                returnValue.currency = currency_symbol;
                returnValue.listInsights = fbAdsCampaigns.data.Count() > 0 ? fbAdsCampaigns.data : new List<InsightsField>();
                return returnValue;
            }

            return returnValue;
        }


        public async Task<List<InsightsField>> CalculateResult(List<InsightsField> listInsights, List<AdsetConfig> listConfig)
        {
            try
            {
                foreach (var element in listInsights)
                {
                    if (element != null && element.actions != null)
                    {


                        var adset_name = string.Empty;
                        var camp_name = string.Empty;

                        var getAdSetConfig = listConfig.Where(x => x.id == element.adset_id).ToList();

                        double resultTotal = 0;

                        foreach (var getads in getAdSetConfig)
                        {
                            adset_name = getads.name;


                            if (getads.optimization_goal == "AD_RECALL_LIFT")
                            {
                                resultTotal = +double.Parse(element.estimated_ad_recallers) > 0 ? double.Parse(element.estimated_ad_recallers) : 0;
                            }
                            else if (getads.optimization_goal == "THRUPLAY")
                            {
                                resultTotal = +double.Parse(element.video_15_sec_watched_actions[0].value);
                            }
                            else if (getads.optimization_goal == "LEAD_GENERATION" && getads.promoted_object != null)
                            {
                                var customActions = element.actions.Where(x => x.action_type == "leadgen_grouped").ToList();


                                resultTotal = +customActions.Count() > 0 && double.Parse(customActions[0].value) > 0 ? double.Parse(customActions[0].value) : 0;
                            }
                            else if (getads.optimization_goal == "OFFSITE_CONVERSIONS" && getads.promoted_object != null)
                            {

                                if (getads.promoted_object.custom_event_type == "OTHER" && !string.IsNullOrEmpty(getads.promoted_object.custom_conversion_id))
                                {
                                    var customActions = element.actions.Where(x => x.action_type == "offsite_conversion.custom." + getads.promoted_object.custom_conversion_id).ToList();
                                    resultTotal = +customActions.Count() > 0 && double.Parse(customActions[0].value) > 0 ? double.Parse(customActions[0].value) : 0;
                                }
                                else if (getads.promoted_object.custom_event_type == "CONTENT_VIEW")
                                {
                                    var customActions = element.actions.Where(x => x.action_type == "offsite_conversion.fb_pixel_view_content").ToList();
                                    resultTotal = +customActions.Count() > 0 && double.Parse(customActions[0].value) > 0 ? double.Parse(customActions[0].value) : 0;

                                }
                                else if (getads.promoted_object.custom_event_type == "ADD_TO_CART")
                                {
                                    var customActions = element.actions.Where(x => x.action_type == "offsite_conversion.fb_pixel_add_to_cart").ToList();
                                    resultTotal = +customActions.Count() > 0 && double.Parse(customActions[0].value) > 0 ? double.Parse(customActions[0].value) : 0;
                                }
                                else if (getads.promoted_object.custom_event_type == "LEAD")
                                {
                                    //if (element.actions.Where(x => x.action_type == "offsite_conversion.fb_pixel_lead").Count() > 0)
                                    //{
                                    //    var customActions = element.actions.Where(x => x.action_type == "offsite_conversion.fb_pixel_lead").ToList();
                                    //    resultTotal = +customActions.Count() > 0 && double.Parse(customActions[0].value) > 0 ? double.Parse(customActions[0].value) : 0;
                                    //}else 
                                    //{
                                    var customActions = element.actions.Where(x => x.action_type.Contains("offsite_conversion.custom.")).ToList();
                                    resultTotal = +customActions.Count() > 0 && double.Parse(customActions[0].value) > 0 ? double.Parse(customActions[0].value) : 0;
                                    //}
                                }

                            }
                            else if (getads.optimization_goal == "LANDING_PAGE_VIEWS")
                            {
                                var customActions = element.actions.Where(x => x.action_type == "landing_page_view").ToList();
                                resultTotal = +customActions.Count() > 0 && double.Parse(customActions[0].value) > 0 ? double.Parse(customActions[0].value) : 0;
                            }
                            else if (getads.optimization_goal == "LINK_CLICKS")
                            {
                                var customActions = element.actions.Where(x => x.action_type == "link_click").ToList();
                                resultTotal = +customActions.Count() > 0 && double.Parse(customActions[0].value) > 0 ? double.Parse(customActions[0].value) : 0;
                            }
                            else if (getads.optimization_goal == "REACH")
                            {
                                resultTotal = +double.Parse(element.reach) > 0 ? double.Parse(element.reach) : 0;
                            }
                            else if (getads.optimization_goal == "IMPRESSION")
                            {
                                resultTotal = +double.Parse(element.impressions) > 0 ? double.Parse(element.impressions) : 0;
                            }
                            else if (getads.optimization_goal == "POST_ENGAGEMENT")
                            {
                                var customActions = element.actions.Where(x => x.action_type == "post_engagement").ToList();
                                resultTotal = +customActions.Count() > 0 && double.Parse(customActions[0].value) > 0 ? double.Parse(customActions[0].value) : 0;
                            }
                            else if (getads.optimization_goal == "CONVERSATIONS")
                            {
                                var customActions = element.actions.Where(x => x.action_type == "").ToList();
                                resultTotal = +customActions.Count() > 0 && double.Parse(customActions[0].value) > 0 ? double.Parse(customActions[0].value) : 0;
                            }
                            else if (getads.optimization_goal == "LANDING_PAGE_VIEWS")
                            {
                                var customActions = element.actions.Where(x => x.action_type == "landing_page_view").ToList();

                                resultTotal = +customActions.Count() > 0 && double.Parse(customActions[0].value) > 0 ? double.Parse(customActions[0].value) : 0;
                            }
                        }

                        element.results = resultTotal.ToString();
                        var totalSpend = listInsights.Where(x => x.spend != null).Select(x => double.Parse(x.spend)).Sum();
                        element.cpr = Math.Round((double.Parse(element.results) / totalSpend), 2).ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                var test = ex;
            }



            return listInsights;

        }


        private PrepareGAdsData PrepareGoogleAdsReport(string htmlString, Guid campaignId, string startTime, string endTime, int reportType, string campaignLogo, string companyLogo, string headerText, string footerText, string headerBgColor, string headerTextColor, bool showFooter, string showPageNumberId, string showPageNumber, bool showHeader, int pageNumber)
        {
            PrepareGAdsData prepareGAdsData = new PrepareGAdsData();

            //google ads row data
            prepareGAdsData.GoogleAdsRawData = new GoogleAdsRawData();

            try
            {
                var gaAdsResponse = _campaignGoogleAdsService.GetGoogleAdsReports(campaignId.ToString(), startTime, endTime, reportType);

                decimal clicks = 0;
                decimal vtc = 0;
                decimal impressions = 0;
                decimal avg_cpc = 0;
                double total_avg_cpc = 0;
                decimal conv = 0;
                decimal conv_rate = 0;
                decimal cost = 0;
                decimal cost_conv = 0;

                for (var i = 0; i < gaAdsResponse.Count; i++)
                {
                    clicks = (int)(clicks + gaAdsResponse[i].Clicks);
                    vtc = (int)(vtc + gaAdsResponse[i].ViewThroughConversions);
                    impressions = (int)(impressions + gaAdsResponse[i].Impressions);
                    avg_cpc = (decimal)(avg_cpc + decimal.Parse(gaAdsResponse[i].Avg_CPC.ToString()));
                    conv = (int)(conv + decimal.Parse(gaAdsResponse[i].Conversation.ToString()));
                    conv_rate = (int)(conv_rate + decimal.Parse(gaAdsResponse[i].ConversationRate.ToString()));
                    cost = Math.Round((decimal)(cost + decimal.Parse(gaAdsResponse[i].Cost.ToString()) / 1000000), 2);
                    cost_conv = (int)(cost_conv + decimal.Parse(gaAdsResponse[i].Cost.ToString()) / 1000000);
                }


                if (gaAdsResponse.Count > 0)
                {
                    var totalCost = gaAdsResponse.Select(x => x.Cost).Sum();
                    var totalClicks = gaAdsResponse.Select(x => x.Clicks).Sum();

                    if (totalCost > 0 && totalClicks > 0)
                    {
                        total_avg_cpc = Math.Round((totalCost / 1000000) / totalClicks, 2);

                    }
                    else
                    {
                        total_avg_cpc = 0;
                    }


                }
                if (conv > 0)
                {
                    cost_conv = Math.Round((cost) / conv, 2);
                }
                if (clicks > 0)
                {
                    conv_rate = Math.Round((conv / clicks) * 100, 2);
                }

                List<GaAdsCampaigns> array1 = new List<GaAdsCampaigns>();
                List<GaAdsCampaigns> array2 = new List<GaAdsCampaigns>();

                List<GoogleAdsCampaignReport> unique = new List<GoogleAdsCampaignReport>();
                List<List<GoogleAdsCampaignReport>> array3 = new List<List<GoogleAdsCampaignReport>>();

                if ((ReportTypes)reportType == ReportTypes.GoogleAdsCampaign)
                {
                    unique = gaAdsResponse.GroupBy(x => x.CampaignId).Select(y => y.First()).ToList();
                    array3 = gaAdsResponse.GroupBy(x => x.CampaignId).Select(grp => grp.ToList()).ToList();
                }
                else if ((ReportTypes)reportType == ReportTypes.GoogleAdsGroups)
                {
                    unique = gaAdsResponse.GroupBy(x => x.AdGroupId).Select(y => y.First()).ToList();
                    array3 = gaAdsResponse.GroupBy(x => x.AdGroupId).Select(grp => grp.ToList()).ToList();
                }
                else if ((ReportTypes)reportType == ReportTypes.GoogleAdsCopies)
                {
                    unique = gaAdsResponse.GroupBy(x => x.AdId).Select(y => y.First()).ToList();
                    array3 = gaAdsResponse.GroupBy(x => x.AdId).Select(grp => grp.ToList()).ToList();
                }


                List<int> arra4 = new List<int> { };
                List<int> finalChartArray = new List<int> { };
                List<int> finalDoughnutChart = new List<int> { };
                List<string> finalDoughnutChartLabel = new List<string> { };
                List<GaAdsCampaigns> tableListArray = new List<GaAdsCampaigns> { };

                // for doughnut chart label
                for (var z = 0; z < unique.Count; z++)
                {
                    finalDoughnutChartLabel.Add(unique[z].Name);
                }

                // for line chart and table dataset
                for (var i = 0; i < unique.Count; i++)
                {
                    decimal t_clicks = 0;
                    decimal t_vtc = 0;
                    decimal t_impressions = 0;
                    decimal t_avg_cpc = 0;
                    decimal t_conv = 0;
                    decimal t_conv_rate = 0;
                    decimal t_cost = 0;
                    decimal t_cost_conv = 0;
                    var uname = unique[i].Name;
                    var adgroupId = unique[i].AdGroupId;
                    var campaign_id = unique[i].CampaignId;
                    var adId = unique[i].AdId;
                    List<GaAdsCampaigns> filterArray = new List<GaAdsCampaigns>();


                    if ((ReportTypes)reportType == ReportTypes.GoogleAdsCampaign)
                    {
                        filterArray = gaAdsResponse.Where(x => x.CampaignId == campaign_id).Select(x => new GaAdsCampaigns
                        {
                            campName = uname,
                            click = (int)(x.Clicks),
                            impression = (int)(x.Impressions),
                            cost = (int)(x.Cost),
                            conv = (int)(x.Conversation),
                            vtc = (int)(x.ViewThroughConversions),
                            avg_cpc = (int)(decimal.Parse(x.Avg_CPC.ToString()))

                        }).ToList();
                    }
                    else if ((ReportTypes)reportType == ReportTypes.GoogleAdsGroups)
                    {
                        filterArray = gaAdsResponse.Where(x => x.AdGroupId == adgroupId).Select(x => new GaAdsCampaigns
                        {
                            campName = uname,
                            click = (int)(x.Clicks),
                            impression = (int)(x.Impressions),
                            cost = (int)(x.Cost),
                            conv = (int)(x.Conversation),
                            vtc = (int)(x.ViewThroughConversions),
                            avg_cpc = (int)(decimal.Parse(x.Avg_CPC.ToString()))

                        }).ToList();
                    }
                    else if ((ReportTypes)reportType == ReportTypes.GoogleAdsCopies)
                    {
                        filterArray = gaAdsResponse.Where(x => x.AdId == adId).Select(x => new GaAdsCampaigns
                        {
                            campName = uname,
                            click = (int)(x.Clicks),
                            impression = (int)(x.Impressions),
                            cost = (int)(x.Cost),
                            conv = (int)(x.Conversation),
                            vtc = (int)(x.ViewThroughConversions),
                            avg_cpc = (int)(decimal.Parse(x.Avg_CPC.ToString()))

                        }).ToList();
                    }



                    // for doughnut chart
                    finalDoughnutChart.Add((int)filterArray.Sum(x => x.click));

                    // for table dataset
                    if (tableListArray.Count == 0)
                    {
                        for (var c = 0; c < filterArray.Count; c++)
                        {
                            t_clicks = (int)(t_clicks + filterArray[c].click);
                            t_vtc = (int)(t_vtc + filterArray[c].vtc);
                            t_impressions = (int)(t_impressions + filterArray[c].impression);
                            t_avg_cpc = (decimal)(t_avg_cpc + decimal.Parse(filterArray[c].avg_cpc.ToString()));
                            t_conv = (int)(t_conv + decimal.Parse(filterArray[c].conv.ToString()));
                            t_cost = Math.Round((decimal)(t_cost + decimal.Parse(filterArray[c].cost.ToString()) / 1000000), 2);
                        }

                        if (filterArray.Count > 0)
                        {
                            if (t_clicks > 0)
                            {
                                t_avg_cpc = Math.Round((t_cost / t_clicks), 2);
                            }
                            else
                            {
                                t_avg_cpc = 0;
                            }
                        }
                        if (t_conv > 0)
                        {
                            t_cost_conv = Math.Round((t_cost / 1000000) / t_conv, 2);
                        }
                        if (t_clicks > 0)
                        {
                            t_conv_rate = Math.Round((t_conv / t_clicks) * 100, 2);
                        }

                        try
                        {
                            tableListArray.Add(new GaAdsCampaigns
                            {
                                campName = uname,
                                click = int.Parse(t_clicks.ToString()),
                                conv = int.Parse(t_conv.ToString()),
                                impression = int.Parse(t_impressions.ToString()),
                                avg_cpc = t_avg_cpc,
                                cost = decimal.Parse(t_cost.ToString()),
                                cost_conv = t_cost_conv,
                                conv_rate = t_conv_rate,
                                vtc = int.Parse(t_vtc.ToString()),
                            });
                        }
                        catch (Exception e)
                        {
                            var error = e;
                        }
                    }
                    else
                    {
                        for (var c = 0; c < filterArray.Count; c++)
                        {
                            t_clicks = (int)(t_clicks + filterArray[c].click);
                            t_vtc = (int)(t_vtc + filterArray[c].vtc);
                            t_impressions = (int)(t_impressions + filterArray[c].impression);
                            t_avg_cpc = (decimal)(t_avg_cpc + decimal.Parse(filterArray[c].avg_cpc.ToString()));
                            t_conv = (int)(t_conv + decimal.Parse(filterArray[c].conv.ToString()));
                            t_cost = (int)(t_cost + decimal.Parse(filterArray[c].cost.ToString()) / 1000000);
                        }
                        if (filterArray.Count > 0)
                        {
                            if (t_clicks > 0)
                            {
                                t_avg_cpc = Math.Round((t_cost / t_clicks), 2);
                            }
                            else
                            {
                                t_avg_cpc = 0;
                            }
                        }
                        if (t_conv > 0)
                        {
                            t_cost_conv = t_conv > 0 ? Math.Round((t_cost / 1000000) / t_conv, 2) : Math.Round((t_cost / 1000000), 2);
                        }
                        if (t_clicks > 0)
                        {
                            t_conv_rate = Math.Round((t_conv / t_clicks) * 100, 2);
                        }

                        tableListArray.Add(new GaAdsCampaigns
                        {
                            campName = uname,
                            click = int.Parse(t_clicks.ToString()),
                            conv = int.Parse(t_conv.ToString()),
                            impression = int.Parse(t_impressions.ToString()),
                            avg_cpc = t_avg_cpc,
                            cost = int.Parse(t_cost.ToString()),
                            cost_conv = t_cost_conv,
                            conv_rate = t_conv_rate,
                            vtc = int.Parse(t_vtc.ToString()),
                        });
                    }

                    // for line chart
                    if (arra4.Count == 0)
                    {
                        for (var j = 0; j < filterArray.Count; j++)
                        {
                            arra4.Add((int)filterArray[j].click);
                        }
                    }
                    else
                    {
                        for (var j = 0; j < filterArray.Count; j++)
                        {
                            for (var k = 0; k < arra4.Count; k++)
                            {
                                var fValue = k;
                                if (j == k)
                                {
                                    int z = (int)(arra4[k] + filterArray[j].click);
                                    finalChartArray.Add(z);
                                }
                            }
                        }
                    }
                }

                // for date ranges
                DateTime DT = DateTime.ParseExact(startTime, "yyyy-MM-dd", new System.Globalization.CultureInfo("en-US"));
                DateTime DT1 = DateTime.ParseExact(endTime, "yyyy-MM-dd", new System.Globalization.CultureInfo("en-US"));

                var dates = new List<string>();

                for (var dt = DT; dt <= DT1; dt = dt.AddDays(1))
                {
                    String date = dt.ToString("MM-dd");
                    dates.Add(date);
                }

                var dateLabelStr = String.Join(",", dates.Select(x => "'" + x + "'"));
                var chartStr = String.Join(",", finalChartArray);
                var doughnutChartStr = String.Join(",", finalDoughnutChart);
                var doughnutChartLabelStr = String.Join(",", finalDoughnutChartLabel.Select(x => "'" + x + "'"));
                var tableListStr = JsonConvert.SerializeObject(tableListArray);


                //Previous data ========================

                var previousDate = CalculatePreviousStartDateAndEndDate(startTime, endTime);

                var prevGaAdsResponse = _campaignGoogleAdsService.GetGoogleAdsReports(campaignId.ToString(), previousDate.PreviousStartDate.ToString("yyyy-MM-dd"), previousDate.PreviousEndDate.ToString("yyyy-MM-dd"), reportType);

                decimal PrvClicks = 0;
                decimal PrevVtc = 0;
                decimal PrevImpressions = 0;
                decimal PrevAvg_cpc = 0;
                double  PrevTotal_avg_cpc = 0;
                decimal PrevConv = 0;
                decimal PrevConv_rate = 0;
                decimal PrevCost = 0;
                decimal PrevCost_conv = 0;

                for (var i = 0; i < prevGaAdsResponse.Count; i++)
                {
                    PrvClicks = (int)(PrvClicks + prevGaAdsResponse[i].Clicks);
                    PrevVtc = (int)(PrevVtc + prevGaAdsResponse[i].ViewThroughConversions);
                    PrevImpressions = (int)(PrevImpressions + prevGaAdsResponse[i].Impressions);
                    PrevAvg_cpc = (decimal)(PrevAvg_cpc + decimal.Parse(prevGaAdsResponse[i].Avg_CPC.ToString()));
                    PrevConv = (int)(PrevConv + decimal.Parse(prevGaAdsResponse[i].Conversation.ToString()));
                    PrevConv_rate = (int)(PrevConv_rate + decimal.Parse(prevGaAdsResponse[i].ConversationRate.ToString()));
                    PrevCost = Math.Round((decimal)(PrevCost + decimal.Parse(prevGaAdsResponse[i].Cost.ToString()) / 1000000), 2);
                    PrevCost_conv = (int)(PrevCost_conv + decimal.Parse(prevGaAdsResponse[i].Cost.ToString()) / 1000000);
                }

                if (prevGaAdsResponse.Count > 0)
                {
                    var totalCost = prevGaAdsResponse.Select(x => x.Cost).Sum();
                    var totalClicks = prevGaAdsResponse.Select(x => x.Clicks).Sum();

                    if (totalCost > 0 && totalClicks > 0)
                    {
                        PrevTotal_avg_cpc = Math.Round((totalCost / 1000000) / totalClicks, 2);

                    }
                    else
                    {
                        PrevTotal_avg_cpc = 0;
                    }
                }

                if (PrevConv > 0)
                {
                    PrevCost_conv = Math.Round((PrevCost) / PrevConv, 2);
                }
                if (PrvClicks > 0)
                {
                    PrevConv_rate = Math.Round((conv / PrvClicks) * 100, 2);
                }

                //Calculate tiles Difference
                var data = (clicks > 0 ?clicks.ToString() : "0") + "--" + (PrvClicks > 0 ? PrvClicks.ToString() : "0");
                var clickDataDifference = PrepareDataGa4(data);
                prepareGAdsData.GoogleAdsRawData.Clicks = clickDataDifference;

                var data1 = (vtc > 0 ? vtc.ToString() : "0") + "--" + (PrevVtc > 0 ? PrevVtc.ToString() : "0");
                var vtcDataDifference = PrepareDataGa4(data1);
                prepareGAdsData.GoogleAdsRawData.ViewThroughConversions = vtcDataDifference;

                var data2 = (impressions > 0 ? impressions.ToString() : "0") + "--" + (PrevImpressions > 0 ? PrevImpressions.ToString() : "0");
                var impDataDifference = PrepareDataGa4(data2);
                prepareGAdsData.GoogleAdsRawData.Impressions = impDataDifference;

                var data3 = (total_avg_cpc > 0 ? total_avg_cpc.ToString() : "0") + "--" + (PrevTotal_avg_cpc > 0 ? PrevTotal_avg_cpc.ToString() : "0");
                var cpcDataDifference = PrepareDataGa4(data3);
                prepareGAdsData.GoogleAdsRawData.Avg_CPC = cpcDataDifference;

                var data4 = (conv > 0 ? conv.ToString() : "0") + "--" + (PrevConv > 0 ? PrevConv.ToString() : "0");
                var convDataDifference = PrepareDataGa4(data4);
                prepareGAdsData.GoogleAdsRawData.Conversation = convDataDifference;

                var data5 = (conv_rate > 0 ? conv_rate.ToString() : "0") + "--" + (PrevConv_rate > 0 ? PrevConv_rate.ToString() : "0");
                var convRateDataDifference = PrepareDataGa4(data5);
                prepareGAdsData.GoogleAdsRawData.ConversationRate = convRateDataDifference;

                var data6 = (cost > 0 ? cost.ToString() : "0") + "--" + (PrevCost > 0 ? PrevCost.ToString() : "0");
                var costDataDifference = PrepareDataGa4(data6);
                prepareGAdsData.GoogleAdsRawData.Cost = costDataDifference;

                var data7 = (cost_conv > 0 ? cost_conv.ToString() : "0") + "--" + (PrevCost_conv > 0 ? PrevCost_conv.ToString() : "0");
                var prevCostDataDifference = PrepareDataGa4(data7);
                prepareGAdsData.GoogleAdsRawData.CostPerConversions = prevCostDataDifference;

                //Replace the string according to report type

                htmlString = htmlString.Replace("_gaAdsClicksData_", clicks.ToString());
                htmlString = htmlString.Replace("_gaAdsCurrencyData_", gaAdsResponse.Count() < 0 && !string.IsNullOrEmpty(gaAdsResponse[0].Currency) ? "'" + gaAdsResponse[0].Currency.ToString() + "'" : "");
                htmlString = htmlString.Replace("_gaAdsCurrencyDataTable_", gaAdsResponse.Count() < 0 && !string.IsNullOrEmpty(gaAdsResponse[0].Currency) ? "'" + gaAdsResponse[0].Currency.ToString() + "'" : "");
                htmlString = htmlString.Replace("_gaAdImpressionsData_", impressions.ToString());
                htmlString = htmlString.Replace("_gaAdVTCData_", vtc.ToString());
                htmlString = htmlString.Replace("_gaAdAVGCPCData_", total_avg_cpc.ToString());
                htmlString = htmlString.Replace("_gaAdConversionRateData_", conv_rate.ToString());
                htmlString = htmlString.Replace("_gaAdCONVData_", conv.ToString());
                htmlString = htmlString.Replace("_gaAdCOSTData_", cost.ToString());
                htmlString = htmlString.Replace("_gaAdCOSTCONVData_", cost_conv.ToString());

                htmlString = htmlString.Replace("_1gaAdsLineChartLabels_", dateLabelStr);
                htmlString = htmlString.Replace("_1gaAdsLinchartData_", chartStr);

                htmlString = htmlString.Replace("_2gaAdDoughnutChartLabels_", doughnutChartLabelStr);
                htmlString = htmlString.Replace("_2gaAdDoughnutChartData_", doughnutChartStr);

                htmlString = htmlString.Replace("_tableArrayList_", tableListStr);

                htmlString = htmlString.Replace("_headerImage1_", companyLogo);
                htmlString = htmlString.Replace("_headerImage_", campaignLogo);
                htmlString = htmlString.Replace("_headerText_", headerText);
                htmlString = htmlString.Replace("_footerText_", footerText);
                htmlString = htmlString.Replace("_headerTextColor_", headerTextColor);
                htmlString = htmlString.Replace("_headerBgColor_", headerBgColor);
                htmlString = htmlString.Replace("_showPageNumber_", showPageNumber);
                htmlString = htmlString.Replace("_pageNumberId_", showPageNumberId);
                htmlString = htmlString.Replace("_pageNumber_", "Page " + pageNumber.ToString());

                int intervalRes = dates.Count <= 31 ? 3 : (dates.Count <= 91 ? 7 : 31);
                htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

                if (showHeader)
                {
                    htmlString = htmlString.Replace("_showHeader_", "none");
                }
                else
                {
                    htmlString = htmlString.Replace("_showHeader_", "hidden");
                }
                if (!showFooter)
                {
                    htmlString = htmlString.Replace("_showFooter_", "hidden");
                }
                else
                {
                    htmlString = htmlString.Replace("_showFooter_", "none");
                }

                if (campaignLogo == "")
                {
                    htmlString = htmlString.Replace("_showImg2_", "hidden");
                }
                else
                {
                    htmlString = htmlString.Replace("_showImg2_", "none");
                }

                if (companyLogo == "")
                {
                    htmlString = htmlString.Replace("_showImg1_", "hidden");
                }
                else
                {
                    htmlString = htmlString.Replace("_showImg1_", "none");
                }
            }
            catch (Exception ex)
            {

                var testing = ex;
            }
            prepareGAdsData.Html = htmlString;

            return prepareGAdsData;
        }

        public async Task<GscIntegrationData> PrepareGoogleSearchConsole(string startDate, string endDate, string urlOrName, string accessToken, PreviousDate previousDate, List<string> subtypes)
        {
            var preparedUrl = string.Empty;
            GscIntegrationData gscIntegrationData = new GscIntegrationData();

            gscIntegrationData.GscRawData = new GscRawData();

            var htmlString = string.Empty;
            List<Dictionary<string, string>> listOfResult = new List<Dictionary<string, string>>();

            if (urlOrName.Contains("https://"))
            {
                var urlcamp = urlOrName.Replace("https://", "");
                urlcamp = urlcamp.Replace("/", "");
                preparedUrl = "https%3A%2F%2F" + urlcamp + "/searchAnalytics/query?key=" + _configuration.GetSection("GoogleApiKey").Value;

            }
            else if (urlOrName.Contains("http://"))
            {
                var urlcamp = urlOrName.Replace("http://", "");
                urlcamp = urlcamp.Replace("/", "");
                preparedUrl = "http%3A%2F%2F" + urlcamp + "/searchAnalytics/query?key=" + _configuration.GetSection("GoogleApiKey").Value;

            }
            else
            {
                var urlcamp = urlOrName.Replace("sc-domain:", "");
                urlcamp = urlcamp.Replace("/", "");
                preparedUrl = "sc-domain%3A" + urlcamp + "/searchAnalytics/query?key=" + _configuration.GetSection("GoogleApiKey").Value;

            }

            var gscData = await PrepareGSCReportsByPost(preparedUrl, startDate, endDate, accessToken);
            var previousGscData = await PrepareGSCReportsByPost(preparedUrl, previousDate.PreviousStartDate.ToString("yyyy-MM-dd"), previousDate.PreviousEndDate.ToString("yyyy-MM-dd"), accessToken);
            var gscChartData = await PrepareGSCReportsByPostWithDate(preparedUrl, startDate, endDate, accessToken);
            var previousGscChartData = await PrepareGSCReportsByPostWithDate(preparedUrl, previousDate.PreviousStartDate.ToString("yyyy-MM-dd"), previousDate.PreviousEndDate.ToString("yyyy-MM-dd"), accessToken);

            if (gscData.rows != null && previousGscData.rows != null)
            {

                foreach (var subtype in subtypes)
                {
                    var intsubtype = Convert.ToInt16(subtype);
                    if (intsubtype == (int)ReportTypes.GscClick)
                    {
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                        //string path1 = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/gscClick.html");
                        //htmlString = System.IO.File.ReadAllText(path1);

                        string path1 = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/gscClick.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(path1).Result;
                        }

                        var longDate = gscChartData.rows.Select(x => x.keys[0]).ToList();

                        List<DateTime> dates1 = longDate.Select(date => DateTime.Parse(date)).ToList();

                        var shortDate = dates1.Select(date => date.ToString("MM-dd")).ToArray();

                        var clicksChart = gscChartData.rows.Select(x => x.clicks).ToArray();

                        var clicks = String.Join(",", clicksChart);

                        var retdate = String.Join(",", shortDate.Select(x => "'" + x + "'"));

                        // gsc chart data current
                        htmlString = htmlString.Replace("_1gscChartClicks1_", clicks);

                        htmlString = htmlString.Replace("_gscChartLabels_", retdate);

                        int intervalRes = longDate.Count <= 31 ? 3 : (longDate.Count <= 91 ? 7 : 31);
                        htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

                        List<DateTime> PreLongDate = longDate.Select(date => DateTime.Parse(date)).ToList();

                        var PrevShortDate = PreLongDate.Select(date => date.ToString("MM-dd")).ToArray();

                        var PrevClicksChart = previousGscChartData.rows.Select(x => x.clicks).ToArray();

                        var pre_clicks = String.Join(",", PrevClicksChart);


                        // gsc chart data previous
                        htmlString = htmlString.Replace("_1gscDiffChartClicks1_", pre_clicks);

                        //different calculation
                        var data = (clicksChart.Sum(x => x) > 0 ? clicksChart.Sum(x => x).ToString() : "0") + "--" + (PrevClicksChart.Sum(x => x) > 0 ? PrevClicksChart.Sum(x => x).ToString() : "0");
                        var dataDifference = PrepareDataGa4(data);
                        htmlString = htmlString.Replace("_gscClickDifference_", dataDifference);

                        var str = dataDifference;
                        var hasPlusSign = str.Contains("+");
                        if (hasPlusSign)
                        {
                            htmlString = htmlString.Replace("_gscClickColor_", "green");
                        }
                        else
                        {
                            htmlString = htmlString.Replace("_gscClickColor_", "red");
                        }

                        string uniqueKey = $"{2}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);

                        gscIntegrationData.GscRawData.Clicks = dataDifference;
                    }
                    else if (intsubtype == (int)ReportTypes.GscImpression)
                    {
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                        //string path2 = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/gscImpression.html");
                        //htmlString = System.IO.File.ReadAllText(path2);

                        string path2 = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/gscImpression.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(path2).Result;
                        }

                        var longDate = gscChartData.rows.Select(x => x.keys[0]).ToList();

                        List<DateTime> dates1 = longDate.Select(date => DateTime.Parse(date)).ToList();

                        var shortDate = dates1.Select(date => date.ToString("MM-dd")).ToArray();
                        var impressionChart = gscChartData.rows.Select(x => x.impressions).ToArray();
                        var impression = String.Join(",", impressionChart);
                        var retdate = String.Join(",", shortDate.Select(x => "'" + x + "'"));

                        // gsc chart data current
                        htmlString = htmlString.Replace("_2gscChartImpressions2_", impression);
                        htmlString = htmlString.Replace("_gscChartLabels_", retdate);

                        int intervalRes = longDate.Count <= 31 ? 3 : (longDate.Count <= 91 ? 7 : 31);
                        htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

                        List<DateTime> PreLongDate = longDate.Select(date => DateTime.Parse(date)).ToList();
                        var PrevShortDate = PreLongDate.Select(date => date.ToString("MM-dd")).ToArray();
                        var PrevImpChart = previousGscChartData.rows.Select(x => x.impressions).ToArray();
                        var pre_impression = String.Join(",", PrevImpChart);
                        // gsc chart data previous
                        htmlString = htmlString.Replace("_2gscDiffChartImpressions2_", pre_impression);

                        //different calculation
                        var data = (impressionChart.Sum(x => x) > 0 ? impressionChart.Sum(x => x).ToString() : "0") + "--" + (PrevImpChart.Sum(x => x) > 0 ? PrevImpChart.Sum(x => x).ToString() : "0");
                        var dataDifference = PrepareDataGa4(data);
                        htmlString = htmlString.Replace("_gscImpDifference_", dataDifference);

                        var str = dataDifference;
                        var hasPlusSign = str.Contains("+");
                        if (hasPlusSign)
                        {
                            htmlString = htmlString.Replace("_gscImpColor_", "green");
                        }
                        else
                        {
                            htmlString = htmlString.Replace("_gscImpColor_", "red");
                        }

                        string uniqueKey = $"{2}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);

                        gscIntegrationData.GscRawData.Impressions = dataDifference;
                    }
                    else if (intsubtype == (int)ReportTypes.GscCtr)
                    {
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                        //string path3 = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/gscCtr.html");
                        //htmlString = System.IO.File.ReadAllText(path3);

                        string path3 = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/gscCtr.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(path3).Result;
                        }

                        var longDate = gscChartData.rows.Select(x => x.keys[0]).ToList();
                        List<DateTime> dates1 = longDate.Select(date => DateTime.Parse(date)).ToList();
                        var shortDate = dates1.Select(date => date.ToString("MM-dd")).ToArray();
                        var ctrChart = gscChartData.rows.Select(x => x.ctr * 100).ToArray();
                        var ctr = String.Join(",", ctrChart.Select(x => Math.Round(x, 2)));
                        var retdate = String.Join(",", shortDate.Select(x => "'" + x + "'"));
                        // gsc chart data current              
                        htmlString = htmlString.Replace("_3gscChartCTR3_", ctr);
                        htmlString = htmlString.Replace("_gscChartLabels_", retdate);

                        int intervalRes = longDate.Count <= 31 ? 3 : (longDate.Count <= 91 ? 7 : 31);
                        htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

                        List<DateTime> PreLongDate = longDate.Select(date => DateTime.Parse(date)).ToList();
                        var PrevShortDate = PreLongDate.Select(date => date.ToString("MM-dd")).ToArray();
                        var PrevCtrChart = previousGscChartData.rows.Select(x => x.ctr).ToArray();
                        var PrevPositionChart = previousGscChartData.rows.Select(x => x.position).ToArray();
                        var pre_ctr = String.Join(",", PrevCtrChart.Select(x => Math.Round(x, 2)));
                        var pre_retdate = String.Join(",", PrevPositionChart.Select(x => "'" + x + "'"));
                        // gsc chart data previous
                        htmlString = htmlString.Replace("_3gscDiffChartCTR3_", pre_ctr);

                        //different calculation
                        var data = (ctrChart.Sum(x => x) > 0 ? ctrChart.Sum(x => x).ToString() : "0") + "--" + (PrevCtrChart.Sum(x => x) > 0 ? PrevCtrChart.Sum(x => x).ToString() : "0");
                        var dataDifference = PrepareDataGa4(data);
                        htmlString = htmlString.Replace("_gscCtrDifference_", dataDifference);

                        var str = dataDifference;
                        var hasPlusSign = str.Contains("+");
                        if (hasPlusSign)
                        {
                            htmlString = htmlString.Replace("_gscCtrColor_", "green");
                        }
                        else
                        {
                            htmlString = htmlString.Replace("_gscCtrColor_", "red");
                        }

                        string uniqueKey = $"{2}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);

                        gscIntegrationData.GscRawData.CTR = dataDifference;
                    }
                    else if (intsubtype == (int)ReportTypes.GscPosition)
                    {
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                        //string path4 = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/gscPosition.html");
                        //htmlString = System.IO.File.ReadAllText(path4);

                        string path4 = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/gscPosition.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(path4).Result;
                        }

                        var longDate = gscChartData.rows.Select(x => x.keys[0]).ToList();

                        List<DateTime> dates1 = longDate.Select(date => DateTime.Parse(date)).ToList();

                        var shortDate = dates1.Select(date => date.ToString("MM-dd")).ToArray();



                        var positionChart = gscChartData.rows.Select(x => x.position).ToArray();


                        var position = String.Join(",", positionChart.Select(x => Math.Round(x, 2)));
                        var retdate = String.Join(",", shortDate.Select(x => "'" + x + "'"));

                        // gsc chart data current

                        htmlString = htmlString.Replace("_4gscChartPosition4_", position);
                        htmlString = htmlString.Replace("_gscChartLabels_", retdate);

                        int intervalRes = longDate.Count <= 31 ? 3 : (longDate.Count <= 91 ? 7 : 31);
                        htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

                        List<DateTime> PreLongDate = longDate.Select(date => DateTime.Parse(date)).ToList();
                        var PrevShortDate = PreLongDate.Select(date => date.ToString("MM-dd")).ToArray();
                        var PrevClicksChart = previousGscChartData.rows.Select(x => x.clicks).ToArray();
                        var PrevImpChart = previousGscChartData.rows.Select(x => x.impressions).ToArray();
                        var PrevCtrChart = previousGscChartData.rows.Select(x => x.ctr).ToArray();
                        var PrevPositionChart = previousGscChartData.rows.Select(x => x.position).ToArray();


                        var pre_position = String.Join(",", PrevPositionChart.Select(x => Math.Round(x, 2)));
                        var pre_retdate = String.Join(",", PrevPositionChart.Select(x => "'" + x + "'"));

                        // gsc chart data previous
                        htmlString = htmlString.Replace("_4gscDiffChartPosition4_", pre_position);

                        //different calculation
                        var data = (positionChart.Sum(x => x) > 0 ? positionChart.Sum(x => x).ToString() : "0") + "--" + (PrevPositionChart.Sum(x => x) > 0 ? PrevPositionChart.Sum(x => x).ToString() : "0");
                        var dataDifference = PrepareDataGa4(data);
                        htmlString = htmlString.Replace("_gscPositionDifference_", dataDifference);

                        var str = dataDifference;
                        var hasPlusSign = str.Contains("+");
                        if (hasPlusSign)
                        {
                            htmlString = htmlString.Replace("_gscPositionColor_", "green");
                        }
                        else
                        {
                            htmlString = htmlString.Replace("_gscPositionColor_", "red");
                        }

                        string uniqueKey = $"{2}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);

                        gscIntegrationData.GscRawData.Position = dataDifference;
                    }
                }

                gscIntegrationData.CurrentImpression = gscChartData.rows.Select(x => x.impressions).Sum().ToString();
                gscIntegrationData.PreviousImpression = previousGscChartData.rows.Select(x => x.impressions).Sum().ToString();
                gscIntegrationData.HtmlString = listOfResult;
                gscIntegrationData.GscRawData.CurrentDateRange = startDate +" "+ endDate;
                gscIntegrationData.GscRawData.PreviousDateRange = previousDate.PreviousStartDate.ToString("yyyy-MM-dd") + " " + previousDate.PreviousEndDate.ToString("yyyy-MM-dd");

            }
            gscIntegrationData.StatusCode = gscData.statusCode;

            return gscIntegrationData;
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

        private double GetDifference(double current, double previous)
        {
            var retVal = current > 0 ? ((current - previous) * 100) / current : (-previous);
            return Math.Round(retVal);
        }

        public async Task<GscData> PrepareGSCReportsByPost(string preparedUrl, string startDate, string endDate, string accessToken)
        {
            var gscResponse = new GscData();

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://www.googleapis.com/webmasters/v3/sites/"),
                DefaultRequestHeaders = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) }
            };

            var data = new
            {
                startRow = 0,
                startDate = startDate,
                endDate = endDate,
                dataState = "ALL"
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var resp = await httpClient.PostAsync(preparedUrl, stringContent);

            if (resp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                gscResponse.statusCode = System.Net.HttpStatusCode.OK;
                gscResponse = JsonConvert.DeserializeObject<GscData>(await resp.Content.ReadAsStringAsync());
                if (gscResponse.rows == null)
                {
                    gscResponse.rows = new List<Row> { new Row { clicks = 0, impressions = 0, ctr = 0, position = 0, keys = new List<String>() } };
                }
                else
                {
                    gscResponse.rows[0].ctr = Math.Round(((gscResponse.rows[0].ctr) * 100), 1);
                }
                return gscResponse;
            }
            else
            {
                gscResponse.statusCode = resp.StatusCode;
                return gscResponse;
            }

        }

        public async Task<GscChartResponse> PrepareGSCReportsByPostWithDate(string preparedUrl, string startDate, string endDate, string accessToken)
        {

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://www.googleapis.com/webmasters/v3/sites/"),
                DefaultRequestHeaders = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) }
            };

            var data = new
            {
                startRow = 0,
                startDate = startDate,
                endDate = endDate,
                dataState = "ALL",
                dimensions = new string[] { "DATE" }
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var resp = await httpClient.PostAsync(preparedUrl, stringContent);

            HttpContent requestContent = resp.Content;
            var notificationJson = new StreamReader(requestContent.ReadAsStreamAsync().Result).ReadToEnd();
            string serializedNotification = JsonConvert.SerializeObject(notificationJson);

            var gaResponse = JsonConvert.DeserializeObject<GscChartResponse>(await resp.Content.ReadAsStringAsync());

            if (gaResponse.rows == null)
            {
                gaResponse.rows = new List<Row> { new Row { clicks = 0, impressions = 0, ctr = 0, position = 0, keys = new List<String>() } };
            }
            return gaResponse;
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

        public async Task<GaIntegrationData> PrepareGaOrganicTrafficReports(string htmlString, string access_token, string startDate, string endDate, string url, Guid campaignId, string gaProfileId, PreviousDate previousDate, bool fromReportScheduling, string campaignLogo, string companyLogo, string headerText, string footerText, string headerBgColor, string headerTextColor, bool showFooter, string showPageNumberId, string showPageNumber, bool showHeader, int pageNumber)
        {

            GaIntegrationData gaIntegrationData = new GaIntegrationData();

            GoogleCredential cred = GoogleCredential.FromAccessToken(access_token);

            htmlString = htmlString.Replace("_headerImage1_", companyLogo);
            htmlString = htmlString.Replace("_headerImage_", campaignLogo);
            htmlString = htmlString.Replace("_headerText_", headerText);
            htmlString = htmlString.Replace("_footerText_", footerText);
            htmlString = htmlString.Replace("_headerTextColor_", headerTextColor);
            htmlString = htmlString.Replace("_headerBgColor_", headerBgColor);
            htmlString = htmlString.Replace("_showPageNumber_", showPageNumber);
            htmlString = htmlString.Replace("_pageNumberId_", showPageNumberId);
            htmlString = htmlString.Replace("_pageNumber_", "Page " + pageNumber.ToString());

            if (campaignLogo == "")
            {
                htmlString = htmlString.Replace("_showImg2_", "hidden");
            }
            else
            {
                htmlString = htmlString.Replace("_showImg2_", "none");
            }

            if (companyLogo == "")
            {
                htmlString = htmlString.Replace("_showImg1_", "hidden");
            }
            else
            {
                htmlString = htmlString.Replace("_showImg1_", "none");
            }

            if (showHeader)
            {
                htmlString = htmlString.Replace("_showHeader_", "none");
            }
            else
            {
                htmlString = htmlString.Replace("_showHeader_", "hidden");
            }

            if (!showFooter)
            {
                htmlString = htmlString.Replace("_showFooter_", "hidden");
            }
            else
            {
                htmlString = htmlString.Replace("_showFooter_", "none");
            }

            // Create the service
            AnalyticsService analyticsService = new AnalyticsService(
                new BaseClientService.Initializer()
                {
                    HttpClientInitializer = cred
                }
            );

            var act1 = analyticsService.Management.Accounts.List().Execute();

            var actSum = analyticsService.Management.AccountSummaries.List().Execute();


            if (fromReportScheduling)
            {
                if (!string.IsNullOrEmpty(gaProfileId))
                {
                    var gaData = await PrepareGaOrganicTrafficReportsByGet(htmlString, access_token, startDate, endDate, gaProfileId);

                    var gaPreviousData = await PrepareGaOrganicTrafficReportsByGet(gaData.HtmlString, access_token, previousDate.PreviousStartDate.ToString("yyyy-MM-dd"), previousDate.PreviousEndDate.ToString("yyyy-MM-dd"), gaProfileId, true);


                    //var pageSpeedDesktopData = await PrepareGetSiteSpeedDataDesktop(gaPreviousData.HtmlString, url);

                    //var pageSpeedMobileDataDto = await PrepareGetSiteSpeedDataMobile(pageSpeedDesktopData.HtmlString, url);

                    //htmlString = pageSpeedMobileDataDto.HtmlString;

                    gaIntegrationData.GoogleAnalyticsDataDto = gaData;
                    gaIntegrationData.PreviousGoogleAnalyticsDataDto = gaPreviousData;
                    gaIntegrationData.HtmlString = gaPreviousData.HtmlString;


                }
                //var keywordHtml = PrepareSerpKeyword(htmlString, startDate, endDate, campaignId);
                //gaIntegrationData.HtmlString = keywordHtml;
            }
            else
            {
                var gaData = await PrepareGaOrganicTrafficReportsByGet(htmlString, access_token, startDate, endDate, gaProfileId);

                var gaPreviousData = await PrepareGaOrganicTrafficReportsByGet(gaData.HtmlString, access_token, previousDate.PreviousStartDate.ToString("yyyy-MM-dd"), previousDate.PreviousEndDate.ToString("yyyy-MM-dd"), gaProfileId, true);

                gaIntegrationData.GoogleAnalyticsDataDto = gaData;
                gaIntegrationData.PreviousGoogleAnalyticsDataDto = gaPreviousData;
                gaIntegrationData.HtmlString = "";//No need to pass htmlstring because in dashboard we are not use
            }

            return gaIntegrationData;
        }

        public async Task<List<Dictionary<string, string>>> ReplaceData(ReportReplaceData reportReplaceData)
        {
            // var htmlArray = new List<string>();
            List<Dictionary<string, string>> listOfResult = new List<Dictionary<string, string>>();

            string htmlString = string.Empty;
            string companyLogo = reportReplaceData.ReportSetting.HeaderLableImg;
            string campaignLogo = reportReplaceData.ReportSetting.HeaderLableImgCamp;
            string headerText = reportReplaceData.ReportSetting.HeaderTextValue;
            string headerTextColor = reportReplaceData.ReportSetting.HeaderTextColor;
            string headerBgColor = reportReplaceData.ReportSetting.HeaderBgColor;
            string coverPageBgImage = reportReplaceData.ReportSetting.CoverPageBgImage;
            string coverPageBgColor = reportReplaceData.ReportSetting.CoverPageBgColor;
            string coverPageTextColor = reportReplaceData.ReportSetting.CoverPageTextColor;
            string coverPageTitle = reportReplaceData.ReportSetting.Name;

            if (reportReplaceData.Type == (int)ReportTypes.GoogleSearchConsole)
            {

                var GoogleSearchConsole = JsonConvert.DeserializeObject<RootGSCReportData>(reportReplaceData.RootReportData.GoogleSearchConsole);

                if (GoogleSearchConsole != null)
                {

                    foreach (var subtype in reportReplaceData.SubType)
                    {
                        var intsubtype = Convert.ToInt16(subtype);
                        if (intsubtype == (int)ReportTypes.GscClick)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                            //string path1 = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/gscClick.html");
                            //htmlString = System.IO.File.ReadAllText(path1);

                            string path1 = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/gscClick.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(path1).Result;
                            }

                            var clicks = String.Join(",", GoogleSearchConsole.Chart.Clicks.Current);

                            var dateListStr = String.Join(",", GoogleSearchConsole.Chart.Dates.Select(x => "'" + x + "'"));

                            // gsc chart data current
                            htmlString = htmlString.Replace("_1gscChartClicks1_", clicks);
                            htmlString = htmlString.Replace("_gscChartLabels_", dateListStr);

                            int intervalRes = GoogleSearchConsole.Chart.Dates.Length <= 31 ? 3 : (GoogleSearchConsole.Chart.Dates.Length <= 91 ? 7 : 31);
                            htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

                            var pre_clicks = String.Join(",", GoogleSearchConsole.Chart.Clicks.Previous);

                            // gsc chart data previous
                            htmlString = htmlString.Replace("_1gscDiffChartClicks1_", pre_clicks);

                            //different calculation
                            var data = (GoogleSearchConsole.Chart.Clicks.Current.Sum(x => x) > 0 ? GoogleSearchConsole.Chart.Clicks.Current.Sum(x => x).ToString() : "0") + "--" + (GoogleSearchConsole.Chart.Clicks.Previous.Sum(x => x) > 0 ? GoogleSearchConsole.Chart.Clicks.Previous.Sum(x => x).ToString() : "0");
                            var dataDifference = PrepareDataGa4(data);
                            htmlString = htmlString.Replace("_gscClickDifference_", dataDifference);

                            var str = dataDifference;
                            var hasPlusSign = str.Contains("+");
                            if (hasPlusSign)
                            {
                                htmlString = htmlString.Replace("_gscClickColor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_gscClickColor_", "red");
                            }

                            string uniqueKey = $"{reportReplaceData.Type}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                            //htmlArray.Add(htmlString);
                        }
                        else if (intsubtype == (int)ReportTypes.GscImpression)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                            //string path2 = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/gscImpression.html");
                            //htmlString = System.IO.File.ReadAllText(path2);

                            string path2 = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/gscImpression.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(path2).Result;
                            }


                            var impression = String.Join(",", GoogleSearchConsole.Chart.Impressions.Current);
                            var dateListStr = String.Join(",", GoogleSearchConsole.Chart.Dates.Select(x => "'" + x + "'"));

                            // gsc chart data current

                            htmlString = htmlString.Replace("_2gscChartImpressions2_", impression);
                            htmlString = htmlString.Replace("_gscChartLabels_", dateListStr);

                            int intervalRes = GoogleSearchConsole.Chart.Dates.Length <= 31 ? 3 : (GoogleSearchConsole.Chart.Dates.Length <= 91 ? 7 : 31);
                            htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

                            var pre_impression = String.Join(",", GoogleSearchConsole.Chart.Impressions.Previous);
                            // gsc chart data previous                           
                            htmlString = htmlString.Replace("_2gscDiffChartImpressions2_", pre_impression);

                            //different calculation
                            var data = (GoogleSearchConsole.Chart.Impressions.Current.Sum(x => x) > 0 ? GoogleSearchConsole.Chart.Impressions.Current.Sum(x => x).ToString() : "0") + "--" + (GoogleSearchConsole.Chart.Impressions.Previous.Sum(x => x) > 0 ? GoogleSearchConsole.Chart.Impressions.Previous.Sum(x => x).ToString() : "0");
                            var dataDifference = PrepareDataGa4(data);
                            htmlString = htmlString.Replace("_gscImpDifference_", dataDifference);

                            var str = dataDifference;
                            var hasPlusSign = str.Contains("+");
                            if (hasPlusSign)
                            {
                                htmlString = htmlString.Replace("_gscImpColor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_gscImpColor_", "red");
                            }

                            string uniqueKey = $"{reportReplaceData.Type}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                            //htmlArray.Add(htmlString);
                        }
                        else if (intsubtype == (int)ReportTypes.GscCtr)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                            //string path3 = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/gscCtr.html");
                            //htmlString = System.IO.File.ReadAllText(path3);

                            string path3 = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/gscCtr.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(path3).Result;
                            }

                            var ctr = String.Join(",", GoogleSearchConsole.Chart.Ctr.Current);
                            var dateListStr = String.Join(",", GoogleSearchConsole.Chart.Dates.Select(x => "'" + x + "'"));

                            htmlString = htmlString.Replace("_3gscChartCTR3_", ctr);
                            htmlString = htmlString.Replace("_gscChartLabels_", dateListStr);

                            int intervalRes = GoogleSearchConsole.Chart.Dates.Length <= 31 ? 3 : (GoogleSearchConsole.Chart.Dates.Length <= 91 ? 7 : 31);
                            htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

                            var pre_ctr = String.Join(",", GoogleSearchConsole.Chart.Ctr.Previous);
                            // gsc chart data previous
                            htmlString = htmlString.Replace("_3gscDiffChartCTR3_", pre_ctr);

                            //different calculation
                            var data = (GoogleSearchConsole.Chart.Ctr.Current.Sum(x => x) > 0 ? GoogleSearchConsole.Chart.Ctr.Current.Sum(x => x).ToString() : "0") + "--" + (GoogleSearchConsole.Chart.Ctr.Previous.Sum(x => x) > 0 ? GoogleSearchConsole.Chart.Ctr.Previous.Sum(x => x).ToString() : "0");
                            var dataDifference = PrepareDataGa4(data);
                            htmlString = htmlString.Replace("_gscCtrDifference_", dataDifference);

                            var str = dataDifference;
                            var hasPlusSign = str.Contains("+");
                            if (hasPlusSign)
                            {
                                htmlString = htmlString.Replace("_gscCtrColor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_gscCtrColor_", "red");
                            }

                            string uniqueKey = $"{reportReplaceData.Type}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                            //htmlArray.Add(htmlString);
                        }
                        else if (intsubtype == (int)ReportTypes.GscPosition)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                            //string path4 = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/gscPosition.html");
                            //htmlString = System.IO.File.ReadAllText(path4);

                            string path4 = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/gscPosition.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(path4).Result;
                            }

                            var position = String.Join(",", GoogleSearchConsole.Chart.Position.Current);
                            var dateListStr = String.Join(",", GoogleSearchConsole.Chart.Dates.Select(x => "'" + x + "'"));

                            // gsc chart data current
                            htmlString = htmlString.Replace("_4gscChartPosition4_", position);
                            htmlString = htmlString.Replace("_gscChartLabels_", dateListStr);

                            int intervalRes = GoogleSearchConsole.Chart.Dates.Length <= 31 ? 3 : (GoogleSearchConsole.Chart.Dates.Length <= 91 ? 7 : 31);
                            htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

                            var pre_position = String.Join(",", GoogleSearchConsole.Chart.Position.Previous);

                            // gsc chart data previous
                            htmlString = htmlString.Replace("_4gscDiffChartPosition4_", pre_position);

                            //different calculation
                            var data = (GoogleSearchConsole.Chart.Position.Current.Sum(x => x) > 0 ? GoogleSearchConsole.Chart.Position.Current.Sum(x => x).ToString() : "0") + "--" + (GoogleSearchConsole.Chart.Position.Previous.Sum(x => x) > 0 ? GoogleSearchConsole.Chart.Position.Previous.Sum(x => x).ToString() : "0");
                            var dataDifference = PrepareDataGa4(data);
                            htmlString = htmlString.Replace("_gscPositionDifference_", dataDifference);

                            var str = dataDifference;
                            var hasPlusSign = str.Contains("+");
                            if (hasPlusSign)
                            {
                                htmlString = htmlString.Replace("_gscPositionColor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_gscPositionColor_", "red");
                            }

                            string uniqueKey = $"{reportReplaceData.Type}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                            //htmlArray.Add(htmlString);
                        }
                    }
                }

            }
            else if (reportReplaceData.Type == (int)ReportTypes.GoogleAdsCampaign)
            {
                var googleAdsCampaign = JsonConvert.DeserializeObject<RootGoogleAdsReportData>(reportReplaceData.RootReportData.GoogleAdsCampaign);

                if (googleAdsCampaign != null)
                {
                    Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                    string path4 = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/gAds.html");

                    using (HttpClient httpclient = new HttpClient())
                    {
                        htmlString = httpclient.GetStringAsync(path4).Result;
                    }

                    //string path4 = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/gAds.html");
                    //htmlString = System.IO.File.ReadAllText(path4);

                    var leftLabel = String.Join(",", googleAdsCampaign.LeftChartLabels.Select(x => "'" + x + "'"));
                    var leftValue = String.Join(",", googleAdsCampaign.LeftChartValue);

                    var rightLabel = String.Join(",", googleAdsCampaign.RightChartLabels.Select(x => "'" + x + "'"));
                    var rightValue = String.Join(",", googleAdsCampaign.RightChartValue);

                    var tableListStr = JsonConvert.SerializeObject(googleAdsCampaign.TableList);


                    //Replace the string according to report type

                    htmlString = htmlString.Replace("_gaAdsClicksData_", googleAdsCampaign.Card.clicks);
                    htmlString = htmlString.Replace("_gaAdsCurrencyData_", googleAdsCampaign.Card.currency);
                    htmlString = htmlString.Replace("_gaAdImpressionsData_", googleAdsCampaign.Card.impressions);
                    htmlString = htmlString.Replace("_gaAdVTCData_", googleAdsCampaign.Card.vtc);
                    htmlString = htmlString.Replace("_gaAdAVGCPCData_", googleAdsCampaign.Card.avg_cpc == "NaN" ? "0" : googleAdsCampaign.Card.avg_cpc);
                    htmlString = htmlString.Replace("_gaAdConversionRateData_", googleAdsCampaign.Card.conv_rate == "NaN" ? "0" : googleAdsCampaign.Card.conv_rate);
                    htmlString = htmlString.Replace("_gaAdCONVData_", googleAdsCampaign.Card.conv);
                    htmlString = htmlString.Replace("_gaAdCOSTData_", googleAdsCampaign.Card.cost);
                    htmlString = htmlString.Replace("_gaAdCOSTCONVData_", googleAdsCampaign.Card.cost_conv);
                    htmlString = htmlString.Replace("_gaAdsCurrencyDataTable_", "'" + googleAdsCampaign.Card.currency + "'");

                    htmlString = htmlString.Replace("_1gaAdsLineChartLabels_", leftLabel);
                    htmlString = htmlString.Replace("_1gaAdsLinchartData_", leftValue);

                    htmlString = htmlString.Replace("_2gaAdDoughnutChartLabels_", rightLabel);
                    htmlString = htmlString.Replace("_2gaAdDoughnutChartData_", rightValue);

                    htmlString = htmlString.Replace("_tableArrayList_", tableListStr);

                    int intervalRes = googleAdsCampaign.LeftChartLabels.Count <= 31 ? 3 : (googleAdsCampaign.LeftChartLabels.Count <= 91 ? 7 : 31);
                    htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

                    string uniqueKey = $"{reportReplaceData.Type}";
                    uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                    listOfResult.Add(uniqueTypeSubtypeResults);
                    //htmlArray.Add(htmlString);
                }
            }
            else if (reportReplaceData.Type == (int)ReportTypes.GoogleAdsGroups)
            {
                Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                var googleAdsGroups = JsonConvert.DeserializeObject<RootGoogleAdsReportData>(reportReplaceData.RootReportData.GoogleAdsGroups);
                if (googleAdsGroups != null)
                {
                    string path4 = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/gAdsGroups.html");

                    using (HttpClient httpclient = new HttpClient())
                    {
                        htmlString = httpclient.GetStringAsync(path4).Result;
                    }

                    //string path4 = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/gAdsGroups.html");
                    //htmlString = System.IO.File.ReadAllText(path4);


                    var leftLabel = String.Join(",", googleAdsGroups.LeftChartLabels.Select(x => "'" + x + "'"));
                    var leftValue = String.Join(",", googleAdsGroups.LeftChartValue);

                    var rightLabel = String.Join(",", googleAdsGroups.RightChartLabels.Select(x => "'" + x + "'"));
                    var rightValue = String.Join(",", googleAdsGroups.RightChartValue);

                    var tableListStr = JsonConvert.SerializeObject(googleAdsGroups.TableList);

                    //Replace the string according to report type

                    htmlString = htmlString.Replace("_gaAdsClicksData_", googleAdsGroups.Card.clicks);
                    htmlString = htmlString.Replace("_gaAdsCurrencyData_", googleAdsGroups.Card.currency);
                    htmlString = htmlString.Replace("_gaAdImpressionsData_", googleAdsGroups.Card.impressions);
                    htmlString = htmlString.Replace("_gaAdVTCData_", googleAdsGroups.Card.vtc);
                    htmlString = htmlString.Replace("_gaAdAVGCPCData_", googleAdsGroups.Card.avg_cpc == "NaN" ? "0" : googleAdsGroups.Card.avg_cpc);
                    htmlString = htmlString.Replace("_gaAdConversionRateData_", googleAdsGroups.Card.conv_rate == "NaN" ? "0" : googleAdsGroups.Card.conv_rate);
                    htmlString = htmlString.Replace("_gaAdCONVData_", googleAdsGroups.Card.conv);
                    htmlString = htmlString.Replace("_gaAdCOSTData_", googleAdsGroups.Card.cost);
                    htmlString = htmlString.Replace("_gaAdCOSTCONVData_", googleAdsGroups.Card.cost_conv);
                    htmlString = htmlString.Replace("_gaAdsCurrencyDataTable_", "'" + googleAdsGroups.Card.currency + "'");

                    htmlString = htmlString.Replace("_1gaAdsLineChartLabels_", leftLabel);
                    htmlString = htmlString.Replace("_1gaAdsLinchartData_", leftValue);

                    htmlString = htmlString.Replace("_2gaAdDoughnutChartLabels_", rightLabel);
                    htmlString = htmlString.Replace("_2gaAdDoughnutChartData_", rightValue);

                    htmlString = htmlString.Replace("_tableArrayList_", tableListStr);

                    int intervalRes = googleAdsGroups.LeftChartLabels.Count <= 31 ? 3 : (googleAdsGroups.LeftChartLabels.Count <= 91 ? 7 : 31);
                    htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

                    string uniqueKey = $"{reportReplaceData.Type}";
                    uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                    listOfResult.Add(uniqueTypeSubtypeResults);
                    //htmlArray.Add(htmlString);

                }
            }
            else if (reportReplaceData.Type == (int)ReportTypes.GoogleAdsCopies)
            {
                var googleAdsCopies = JsonConvert.DeserializeObject<RootGoogleAdsReportData>(reportReplaceData.RootReportData.GoogleAdsCopies);

                if (googleAdsCopies != null)
                {
                    Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                    string path4 = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/gAdsCopies.html");

                    using (HttpClient httpclient = new HttpClient())
                    {
                        htmlString = httpclient.GetStringAsync(path4).Result;
                    }

                    //string path4 = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/gAdsCopies.html");
                    //htmlString = System.IO.File.ReadAllText(path4);

                    var leftValue = String.Join(",", googleAdsCopies.LeftChartValue);
                    var leftLabel = String.Join(",", googleAdsCopies.LeftChartLabels.Select(x => "'" + x + "'"));

                    var rightLabel = String.Join(",", googleAdsCopies.RightChartLabels);
                    var rightValue = String.Join(",", googleAdsCopies.RightChartValue);

                    var tableListStr = JsonConvert.SerializeObject(googleAdsCopies.TableList);

                    //Replace the string according to report type

                    htmlString = htmlString.Replace("_gaAdsClicksData_", googleAdsCopies.Card.clicks);
                    htmlString = htmlString.Replace("_gaAdsCurrencyData_", googleAdsCopies.Card.currency);
                    htmlString = htmlString.Replace("_gaAdImpressionsData_", googleAdsCopies.Card.impressions);
                    htmlString = htmlString.Replace("_gaAdVTCData_", googleAdsCopies.Card.vtc);
                    htmlString = htmlString.Replace("_gaAdAVGCPCData_", googleAdsCopies.Card.avg_cpc == "NaN" ? "0" : googleAdsCopies.Card.avg_cpc);
                    htmlString = htmlString.Replace("_gaAdConversionRateData_", googleAdsCopies.Card.conv_rate == "NaN" ? "0" : googleAdsCopies.Card.conv_rate);
                    htmlString = htmlString.Replace("_gaAdCONVData_", googleAdsCopies.Card.conv);
                    htmlString = htmlString.Replace("_gaAdCOSTData_", googleAdsCopies.Card.cost);
                    htmlString = htmlString.Replace("_gaAdCOSTCONVData_", googleAdsCopies.Card.cost_conv);
                    htmlString = htmlString.Replace("_gaAdChartName_", googleAdsCopies.ChartName);
                    htmlString = htmlString.Replace("_gaAdsCurrencyDataTable_", "'" + googleAdsCopies.Card.currency + "'");


                    htmlString = htmlString.Replace("_1gaAdsLineChartLabels_", leftLabel);
                    htmlString = htmlString.Replace("_1gaAdsLinchartData_", leftValue);

                    htmlString = htmlString.Replace("_2gaAdDoughnutChartLabels_", rightLabel);
                    htmlString = htmlString.Replace("_2gaAdDoughnutChartData_", rightValue);

                    htmlString = htmlString.Replace("_tableArrayList_", tableListStr);

                    int intervalRes = googleAdsCopies.LeftChartLabels.Count <= 31 ? 3 : (googleAdsCopies.LeftChartLabels.Count <= 91 ? 7 : 31);
                    htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

                    string uniqueKey = $"{reportReplaceData.Type}";
                    uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                    listOfResult.Add(uniqueTypeSubtypeResults);
                    //htmlArray.Add(htmlString);
                }

            }
            else if (reportReplaceData.Type == (int)ReportTypes.Facebook)
            {
                var facebook = JsonConvert.DeserializeObject<FacebookData>(reportReplaceData.RootReportData.Facebook);
                if (facebook != null)
                {
                    List<string> reachPieChartData = new List<string> { facebook.PercentOrganicReach.ToString(), facebook.PercentPaidReach.ToString() };
                    var reachPiechartDataStr = String.Join(",", reachPieChartData);
                    var countryLabelStr = String.Join(",", facebook.CountryLabelStr.Select(x => "'" + x + "'"));
                    var countryDataStr = String.Join(",", facebook.CountryDataStr);

                    List<string> likePieData = new List<string> { facebook.PercentOrganicLike.ToString(), facebook.PercentPaidLike.ToString() };
                    var likePieDataStr = String.Join(",", likePieData);


                    foreach (var subtype in reportReplaceData.SubType)
                    {
                        var intsubtype = Convert.ToInt16(subtype);
                        if (intsubtype == (int)ReportTypes.FbImpression)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                            //string path1 = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/fbImpression.html");
                            //htmlString = System.IO.File.ReadAllText(path1);

                            string path1 = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/fbImpression.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(path1).Result;
                            }

                            htmlString = htmlString.Replace("_1fbPageImpressionMainTotal1_", facebook.PageImpressionsTotal.ToString());
                            htmlString = htmlString.Replace("_1fbPageImpressionDiff1_", facebook.PercentPageImpression.ToString());
                            htmlString = htmlString.Replace("_1fbPageImpressionsAvgTotal1_", facebook.AvgPageImpression.ToString());

                            htmlString = htmlString.Replace("_1fbTopCountForCountries", facebook.TopCountForCity.ToString());

                            htmlString = htmlString.Replace("_1fbPageReachMainTotal1_", facebook.PageReachTotal.ToString());
                            htmlString = htmlString.Replace("_1fbPageReachDiff1_", facebook.PercentPageReach.ToString());
                            htmlString = htmlString.Replace("_1fbPageReachAvgTotal1_", facebook.AvgPageReach.ToString());

                            htmlString = htmlString.Replace("_1fbPieChartReachData1_", reachPiechartDataStr);

                            htmlString = htmlString.Replace("_1fbPieChartCountriesLabel1_", countryLabelStr);
                            htmlString = htmlString.Replace("_1fbPieChartCountriesData1_", countryDataStr);

                            string uniqueKey = $"{reportReplaceData.Type}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                            //htmlArray.Add(htmlString);
                        }
                        else if (intsubtype == (int)ReportTypes.FbPerformance)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                            //string path2 = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/fbPerformance.html");
                            //htmlString = System.IO.File.ReadAllText(path2);

                            string path2 = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/fbPerformance.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(path2).Result;
                            }

                            htmlString = htmlString.Replace("_1fbPageProfileMainTotal1_", facebook.ProfileViewTotal.ToString());
                            htmlString = htmlString.Replace("_1fbPageProfileDiff1_", facebook.PercentProfileView.ToString());
                            htmlString = htmlString.Replace("_1fbPageProfileAvgTotal1_", facebook.AvgPageProfileView.ToString());

                            htmlString = htmlString.Replace("_1fbPageLikesMainTotal1_", facebook.TotalPageLike.ToString());

                            htmlString = htmlString.Replace("_1fbPageNewLikesMainTotal1_", facebook.TotalNewLike.ToString());
                            htmlString = htmlString.Replace("_1fbPageNewLikesAvgTotal1_", facebook.AvgPerDayLike.ToString());

                            htmlString = htmlString.Replace("_1fbPieChartLikesData1_", likePieDataStr);

                            string uniqueKey = $"{reportReplaceData.Type}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                            //htmlArray.Add(htmlString);
                        }
                    }

                }

            }
            else if (reportReplaceData.Type == (int)ReportTypes.FacebookAdsCampaign)
            {
                var facebookAdsCampaign = JsonConvert.DeserializeObject<FacebookAdsCampaignData>(reportReplaceData.RootReportData.FacebookAdsCampaign);
                var facebookAdsGroup = JsonConvert.DeserializeObject<FacebookAdsCampaignData>(reportReplaceData.RootReportData.Facebook);
                var facebookAdsCopies = JsonConvert.DeserializeObject<FacebookAdsCampaignData>(reportReplaceData.RootReportData.FacebookAdsCopies);

                if (facebookAdsCampaign != null)
                {
                    Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                    string pathFbAds1 = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/facebookAdsCampaign.html");

                    using (HttpClient httpclient = new HttpClient())
                    {
                        htmlString = httpclient.GetStringAsync(pathFbAds1).Result;
                    }

                    //string pathFbAds1 = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/facebookAdsCampaign.html");
                    //htmlString = System.IO.File.ReadAllText(pathFbAds1);

                    var fbImp = facebookAdsCampaign.ImpressionData != null ? facebookAdsCampaign.ImpressionData.Select(x => "'" + x + "'") : Enumerable.Empty<string>();
                    var prevFbImp = facebookAdsCampaign.PrevImpressionData != null ? facebookAdsCampaign.PrevImpressionData.Select(x => "'" + x + "'") : Enumerable.Empty<string>();


                    var fbReach = facebookAdsCampaign.ReachData != null ? facebookAdsCampaign.ReachData.Select(x => "'" + x + "'") : Enumerable.Empty<string>();
                    var prevFbReach = facebookAdsCampaign.PrevReachData != null ? facebookAdsCampaign.PrevReachData.Select(x => "'" + x + "'") : Enumerable.Empty<string>();

                    var fbResult = facebookAdsCampaign.ClickData != null ? facebookAdsCampaign.ClickData.Select(x => "'" + x + "'") : Enumerable.Empty<string>();
                    var prevFbResult = facebookAdsCampaign.PrevClickData != null ? facebookAdsCampaign.PrevClickData.Select(x => "'" + x + "'") : Enumerable.Empty<string>();

                    var fbCpr = facebookAdsCampaign.CtrData != null ? facebookAdsCampaign.CtrData.Select(x => "'" + x + "'") : Enumerable.Empty<string>();
                    var prevFbCpr = facebookAdsCampaign.PrevCtrData != null ? facebookAdsCampaign.PrevCtrData.Select(x => "'" + x + "'") : Enumerable.Empty<string>();

                    var impressionCurrentChartStr = String.Join(",", fbImp);
                    var impressionPreviousChartStr = String.Join(",", prevFbImp);

                    var reachCurrentChartStr = String.Join(",", fbReach);
                    var reachPreviousChartStr = String.Join(",", prevFbReach);

                    var resultCurrentChartStr = String.Join(",", fbResult);
                    var resultPreviousChartStr = String.Join(",", prevFbResult);

                    var cprCurrentChartStr = String.Join(",", fbCpr);
                    var cprPreviousChartStr = String.Join(",", prevFbCpr);

                    var tableString = JsonConvert.SerializeObject(facebookAdsCampaign.listInsights);

                    var shortDate = facebookAdsCampaign.shortDate != null ? facebookAdsCampaign.shortDate.Select(date => date.ToString()).ToArray() : new string[] { };
                    var dateLabelStr = String.Join(",", shortDate.Select(x => "'" + x + "'"));

                    htmlString = htmlString.Replace("_1fbAdsImpressionsTotal1_", facebookAdsCampaign.totalImpressions.ToString());
                    htmlString = htmlString.Replace("_1fbAdsReachTotal1_", facebookAdsCampaign.totalReachs.ToString());
                    htmlString = htmlString.Replace("_1fbAdsResultTotal1_", facebookAdsCampaign.results.ToString());
                    htmlString = htmlString.Replace("_1fbAdsCPRTotal1_", facebookAdsCampaign.cpr.ToString());
                    htmlString = htmlString.Replace("_1fbAdsAmtSpentTotal1_", facebookAdsCampaign.spends.ToString());

                    int intervalRes = shortDate.Length <= 31 ? 3 : (shortDate.Length <= 91 ? 7 : 31);
                    htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

                    htmlString = htmlString.Replace("_fbAdsCurrency_", string.IsNullOrEmpty(facebookAdsCampaign.currency) ? "" : facebookAdsCampaign.currency);
                    htmlString = htmlString.Replace("_fbAdsCurrencyTable_", string.IsNullOrEmpty(facebookAdsCampaign.currency) ? "" : "'" + facebookAdsCampaign.currency + "'");

                    htmlString = htmlString.Replace("_fbadsImpressionChartLabels_", dateLabelStr);
                    htmlString = htmlString.Replace("_fbadsReachChartLabels_", dateLabelStr);
                    htmlString = htmlString.Replace("_fbadsResultChartLabels_", dateLabelStr);
                    htmlString = htmlString.Replace("_fbadsCPRChartLabels_", dateLabelStr);

                    htmlString = htmlString.Replace("_fbadsChartImpression_", impressionCurrentChartStr);
                    htmlString = htmlString.Replace("_fbadsDiffChartImpression_", impressionPreviousChartStr);

                    htmlString = htmlString.Replace("_1fbadsChartReach1_", reachCurrentChartStr);
                    htmlString = htmlString.Replace("_1fbadsDiffChartReach1_", reachPreviousChartStr);

                    htmlString = htmlString.Replace("_2fbadsChartResult2_", resultCurrentChartStr);
                    htmlString = htmlString.Replace("_2fbadsDiffChartResult2_", resultPreviousChartStr);

                    htmlString = htmlString.Replace("_3fbadsChartCPR3_", cprCurrentChartStr);
                    htmlString = htmlString.Replace("_3fbadsDiffChartCPR3_", cprPreviousChartStr);

                    htmlString = htmlString.Replace("_tableArrayList_", tableString);

                    // Convert the IEnumerable<string> to IEnumerable<int> using Select and parsing each string to an int
                    IEnumerable<int> fbImpInt = fbImp.Select(x => int.TryParse(x, out var parsedValue) ? parsedValue : 0);
                    IEnumerable<int> prevFbImpInt = prevFbImp.Select(x => int.TryParse(x, out var parsedValue) ? parsedValue : 0);

                    // Sum the elements in the IEnumerable<int>
                    int sumFbImp = fbImpInt.ToList().Any() ? fbImpInt.Sum() : 0;
                    int sumPrevFbImp = prevFbImpInt.ToList().Any() ? prevFbImpInt.Sum() : 0;

                    //different calculation
                    var data = (sumFbImp > 0 ? sumFbImp.ToString() : "0") + "--" + (sumPrevFbImp > 0 ? sumPrevFbImp.ToString() : "0");
                    var dataDifference = PrepareDataGa4(data);

                    //difference logic
                    htmlString = htmlString.Replace("_fbAdImpDifference_", dataDifference);

                    var str = dataDifference;
                    var hasPlusSign = str.Contains("+");
                    if (hasPlusSign)
                    {
                        htmlString = htmlString.Replace("_fbAdImpColor_", "green");
                    }
                    else
                    {
                        htmlString = htmlString.Replace("_fbAdImpColor_", "red");
                    }

                    //different calculation (Reach)
                    // Convert the IEnumerable<string> to IEnumerable<int> using Select and parsing each string to an int
                    IEnumerable<int> fbReachInt = fbReach.Select(x => int.TryParse(x, out var parsedValue) ? parsedValue : 0);
                    IEnumerable<int> prevFbReachInt = prevFbReach.Select(x => int.TryParse(x, out var parsedValue) ? parsedValue : 0);

                    // Sum the elements in the IEnumerable<int>
                    int sumFbReach = fbReachInt.Count() > 0 ? fbReachInt.Sum() : 0;
                    int sumPrevFbReach = prevFbReachInt.Count() > 0 ? prevFbReachInt.Sum() : 0;

                    var data1 = (sumFbReach > 0 ? sumFbReach.ToString() : "0") + "--" + (sumPrevFbReach > 0 ? sumPrevFbReach.ToString() : "0");
                    var dataDifference1 = PrepareDataGa4(data1);

                    //difference logic
                    htmlString = htmlString.Replace("_fbAdReachDifference_", dataDifference1);

                    var str1 = dataDifference1;
                    var hasPlusSign1 = str1.Contains("+");
                    if (hasPlusSign1)
                    {
                        htmlString = htmlString.Replace("_fbAdReachColor_", "green");
                    }
                    else
                    {
                        htmlString = htmlString.Replace("_fbAdReachColor_", "red");
                    }

                    //dataDifference calculation for results

                    // Convert the IEnumerable<string> to IEnumerable<int> using Select and parsing each string to an int
                    IEnumerable<int> fbResultInt = fbResult.Select(x => int.TryParse(x, out var parsedValue) ? parsedValue : 0);
                    IEnumerable<int> prevFbResultInt = prevFbResult.Select(x => int.TryParse(x, out var parsedValue) ? parsedValue : 0);

                    // Sum the elements in the IEnumerable<int>
                    int sumFbResult = fbResultInt.Count() > 0 ? fbResultInt.Sum() : 0;
                    int sumPrevFbResult = prevFbResultInt.Count() > 0 ? prevFbResultInt.Sum() : 0;

                    var data2 = (sumFbResult > 0 ? sumFbResult.ToString() : "0") + "--" + (sumPrevFbResult > 0 ? sumPrevFbResult.ToString() : "0");
                    var dataDifference2 = PrepareDataGa4(data2);

                    //difference logic
                    htmlString = htmlString.Replace("_fbAdResultDifference_", dataDifference2);

                    var str2 = dataDifference2;
                    var hasPlusSign2 = str2.Contains("+");
                    if (hasPlusSign2)
                    {
                        htmlString = htmlString.Replace("_fbAdResultColor_", "green");
                    }
                    else
                    {
                        htmlString = htmlString.Replace("_fbAdResultColor_", "red");
                    }

                    //calculation for cpr
                    // Convert the IEnumerable<string> to IEnumerable<double> using Select and parsing each string to a double
                    IEnumerable<double> fbCprDouble = fbCpr.Select(x => double.TryParse(x, out var parsedValue) ? parsedValue : 0);
                    IEnumerable<double> prevFbCprDouble = prevFbCpr.Select(x => double.TryParse(x, out var parsedValue) ? parsedValue : 0);

                    // Sum the elements in the IEnumerable<double>
                    double sumFbCpr = fbCprDouble.Count() > 0 ? fbCprDouble.Sum() : 0;
                    double sumPrevFbCpr = prevFbCprDouble.Count() > 0 ? prevFbCprDouble.Sum() : 0;

                    var data3 = (sumFbCpr > 0 ? sumFbCpr.ToString() : "0") + "--" + (sumPrevFbCpr > 0 ? sumPrevFbCpr.ToString() : "0");
                    var dataDifference3 = PrepareDataGa4(data3);

                    //difference logic
                    htmlString = htmlString.Replace("_fbCpcDifference_", dataDifference3);

                    var str3 = dataDifference3;
                    var hasPlusSign3 = str3.Contains("+");
                    if (hasPlusSign3)
                    {
                        htmlString = htmlString.Replace("_fbAdCpcColor_", "green");
                    }
                    else
                    {
                        htmlString = htmlString.Replace("_fbAdCpcColor_", "red");
                    }


                    string uniqueKey = $"{reportReplaceData.Type}";
                    uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                    listOfResult.Add(uniqueTypeSubtypeResults);
                    //htmlArray.Add(htmlString);
                }
            }
            else if (reportReplaceData.Type == (int)ReportTypes.FacebookAdsGroup)
            {
                var facebookAdsGroup = JsonConvert.DeserializeObject<FacebookAdsCampaignData>(reportReplaceData.RootReportData.FacebookAdsGroup);

                if (facebookAdsGroup != null)
                {
                    Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                    string pathFbAds1 = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/facebookAdsGroups.html");

                    using (HttpClient httpclient = new HttpClient())
                    {
                        htmlString = httpclient.GetStringAsync(pathFbAds1).Result;
                    }

                    //string pathFbAds1 = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/facebookAdsGroups.html");
                    //htmlString = System.IO.File.ReadAllText(pathFbAds1);


                    var tableString = JsonConvert.SerializeObject(facebookAdsGroup.listInsights);
                    var impressionChartStr = String.Join(",", facebookAdsGroup.ImpressionData);

                    var shortDate = facebookAdsGroup.shortDate.Select(date => date.ToString()).ToArray();
                    var dateLabelStr = String.Join(",", shortDate.Select(x => "'" + x + "'"));

                    htmlString = htmlString.Replace("_fbAdsImpressionData_", facebookAdsGroup.totalImpressions.ToString());
                    htmlString = htmlString.Replace("_fbAdReachData_", facebookAdsGroup.totalReachs.ToString());
                    htmlString = htmlString.Replace("_fbAdsResultData_", facebookAdsGroup.totalClicks.ToString());
                    htmlString = htmlString.Replace("_fbAdcostPerResultData_", facebookAdsGroup.cpc.ToString());
                    htmlString = htmlString.Replace("_fbAdsAmtSpentData_", facebookAdsGroup.spends.ToString());
                    htmlString = htmlString.Replace("_fbAdsLinkClickData_", facebookAdsGroup.totalLinkClick.ToString());
                    htmlString = htmlString.Replace("_fbAdsCtrData_", facebookAdsGroup.ctr.ToString("0.00"));
                    htmlString = htmlString.Replace("_fbAdsCostPerLinkData_", Math.Round(facebookAdsGroup.totalCplc, 2).ToString());

                    htmlString = htmlString.Replace("_fbAdsCurrency_", string.IsNullOrEmpty(facebookAdsGroup.currency) ? "" : facebookAdsGroup.currency);
                    htmlString = htmlString.Replace("_fbAdsCurrencyTable_", string.IsNullOrEmpty(facebookAdsGroup.currency) ? "" : "'" + facebookAdsGroup.currency + "'");

                    htmlString = htmlString.Replace("_1fbAdsLineChartLabels_", dateLabelStr);

                    int intervalRes = shortDate.Length <= 31 ? 3 : (shortDate.Length <= 91 ? 7 : 31);
                    htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

                    htmlString = htmlString.Replace("_1fbAdsLinchartData_", impressionChartStr);

                    htmlString = htmlString.Replace("_tableArrayList_", tableString);

                    string uniqueKey = $"{reportReplaceData.Type}";
                    uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                    listOfResult.Add(uniqueTypeSubtypeResults);
                    //htmlArray.Add(htmlString);
                }
            }
            else if (reportReplaceData.Type == (int)ReportTypes.FacebookAdsCopies)
            {
                var facebookAdsCopies = JsonConvert.DeserializeObject<FacebookAdsCampaignData>(reportReplaceData.RootReportData.FacebookAdsCopies);

                if (facebookAdsCopies != null)
                {
                    Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                    string pathFbAds1 = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/facebookAdsCopies.html");

                    using (HttpClient httpclient = new HttpClient())
                    {
                        htmlString = httpclient.GetStringAsync(pathFbAds1).Result;
                    }

                    //string pathFbAds1 = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/facebookAdsCopies.html");
                    //htmlString = System.IO.File.ReadAllText(pathFbAds1);

                    var tableString = JsonConvert.SerializeObject(facebookAdsCopies.listInsights);

                    htmlString = htmlString.Replace("_fbAdsCurrencyTable_", string.IsNullOrEmpty(facebookAdsCopies.currency) ? "" : "'" + facebookAdsCopies.currency + "'");

                    htmlString = htmlString.Replace("_tableArrayList_", tableString);

                    string uniqueKey = $"{reportReplaceData.Type}";
                    uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                    listOfResult.Add(uniqueTypeSubtypeResults);
                    //htmlArray.Add(htmlString);
                }

            }
            else if (reportReplaceData.Type == (int)ReportTypes.Instagram)
            {
                var instagram = JsonConvert.DeserializeObject<InstagramReportsData>(reportReplaceData.RootReportData.Instagram);

                if (instagram != null)
                {


                    var pieChartDataStr = String.Join(",", instagram.GenderDataChart);

                    List<ListOfInstaLocale> listOfLocale = new List<ListOfInstaLocale> { };
                    List<ListOfInstaLocale> listOfCountries = new List<ListOfInstaLocale> { };
                    List<ListOfInstaLocale> listOfCities = new List<ListOfInstaLocale> { };

                    foreach (var prop in instagram.ListOfLocale)
                    {
                        listOfLocale.Add(new ListOfInstaLocale { name = prop.name, value = prop.value });
                    }

                    foreach (var prop in instagram.ListOfCountries)
                    {
                        listOfCountries.Add(new ListOfInstaLocale { name = prop.name, value = prop.value });
                    }

                    foreach (var prop in instagram.ListOfCities)
                    {
                        listOfCities.Add(new ListOfInstaLocale { name = prop.name, value = prop.value });
                    }

                    var listCityStr = JsonConvert.SerializeObject(listOfCities);
                    var listCountryStr = JsonConvert.SerializeObject(listOfCountries);
                    var listLocaleStr = JsonConvert.SerializeObject(listOfLocale);


                    foreach (var subtype in reportReplaceData.SubType)
                    {
                        var intsubtype = Convert.ToInt16(subtype);
                        if (intsubtype == (int)ReportTypes.InstaPerformance)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                            //string path1 = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/instaPerformance.html");
                            //htmlString = System.IO.File.ReadAllText(path1);

                            string path1 = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/instaPerformance.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(path1).Result;
                            }

                            htmlString = htmlString.Replace("_1instaTotalPageProfileViews_1", instagram.ProfileViewTotal.ToString());
                            htmlString = htmlString.Replace("_1instaTotalNewFollowers_1", instagram.FollowersTotal.ToString());
                            htmlString = htmlString.Replace("_1instaTotalFollowers_1", instagram.InstaFollowersCountTotal.ToString());
                            htmlString = htmlString.Replace("_1instaAvgPageProfileViews_1", instagram.AvgProfileViews.ToString());
                            htmlString = htmlString.Replace("_1instaAvgNewFollowers_1", instagram.AvgFollowers.ToString());

                            string uniqueKey = $"{reportReplaceData.Type}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                            //htmlArray.Add(htmlString);
                        }
                        else if (intsubtype == (int)ReportTypes.InstaImpression)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                            //string path2 = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/instaImpression.html");
                            //htmlString = System.IO.File.ReadAllText(path2);

                            string path2 = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/instaImpression.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(path2).Result;
                            }

                            htmlString = htmlString.Replace("_1instaTotalImpressions_1", instagram.ImpressionsTotal.ToString());
                            htmlString = htmlString.Replace("_1instaTotalReach_1", instagram.ReachTotal.ToString());
                            htmlString = htmlString.Replace("_1instaTotalWebClicks_1", instagram.WebsiteClickTotal.ToString());
                            htmlString = htmlString.Replace("_1instaAvgMediaImpressions_1", instagram.AvgImpressions.ToString());
                            htmlString = htmlString.Replace("_1instaAvgMediaReach_1", instagram.AvgReachTotals.ToString());
                            htmlString = htmlString.Replace("_1instaAvgWebsiteClicksTotal_1", instagram.AvgWebSiteClickTotal.ToString());

                            string uniqueKey = $"{reportReplaceData.Type}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                            //htmlArray.Add(htmlString);
                        }
                        else if (intsubtype == (int)ReportTypes.InstAudiance)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                            string path3 = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/instaAudiance.html");
                            htmlString = System.IO.File.ReadAllText(path3);

                            //string path3 = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/instaAudiance.html");
                            //using (HttpClient httpclient = new HttpClient())
                            //{
                            //    htmlString = httpclient.GetStringAsync(path3).Result;
                            //}

                            htmlString = htmlString.Replace("_1instaGenderPieChartData_", pieChartDataStr);

                            htmlString = htmlString.Replace("_1cityListArray1_", listCityStr);
                            htmlString = htmlString.Replace("_1countryListArray1_", listCountryStr);
                            htmlString = htmlString.Replace("_1localeListArray1_", listLocaleStr);

                            string uniqueKey = $"{reportReplaceData.Type}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                            //htmlArray.Add(htmlString);
                        }
                    }


                }
            }
            else if (reportReplaceData.Type == (int)ReportTypes.LinkedInEngagement)
            {
                var linkedInEngagement = JsonConvert.DeserializeObject<RootLinkedInDataObject>(reportReplaceData.RootReportData.LinkedInEngagement);
                if (linkedInEngagement != null)
                {
                    Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                    string pathLinkedin = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/linkedIn.html");

                    using (HttpClient httpclient = new HttpClient())
                    {
                        htmlString = httpclient.GetStringAsync(pathLinkedin).Result;
                    }

                    //string pathLinkedin = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/linkedIn.html");
                    //htmlString = System.IO.File.ReadAllText(pathLinkedin);


                    // prepare follower gains data                 
                    int[] organic = Mapper.Map<int[]>(linkedInEngagement.FollowerGains.Where(e => e.organicFollowerGain >= 0).Select(x => x.organicFollowerGain).ToList());
                    int[] lost = Mapper.Map<int[]>(linkedInEngagement.FollowerGains.Where(e => e.organicFollowerGain < 0).Select(x => x.organicFollowerGain).ToList());
                    int[] paid = Mapper.Map<int[]>(linkedInEngagement.FollowerGains.Where(e => e.paidFollowerGain >= 0).Select(x => x.paidFollowerGain).ToList());

                    int[] clicks = Mapper.Map<int[]>(linkedInEngagement.ShareStatistics.Select(x => x.clickCount).ToList());
                    int[] impressions = Mapper.Map<int[]>(linkedInEngagement.ShareStatistics.Select(x => x.impressionCount).ToList());
                    int[] comments = Mapper.Map<int[]>(linkedInEngagement.ShareStatistics.Select(x => x.commentCount).ToList());
                    int[] likes = Mapper.Map<int[]>(linkedInEngagement.ShareStatistics.Select(x => x.likeCount).ToList());
                    int[] share = Mapper.Map<int[]>(linkedInEngagement.ShareStatistics.Select(x => x.shareCount).ToList());


                    var orgStr = String.Join(",", organic);
                    var lostStr = String.Join(",", lost);
                    var paidStr = String.Join(",", paid);

                    var commStr = String.Join(",", comments);
                    var likesStr = String.Join(",", likes);
                    var shareStr = String.Join(",", share);
                    var clicksStr = String.Join(",", clicks);
                    var impressionStr = String.Join(",", impressions);
                    var orgPieStr = organic.Sum();
                    var paidPieStr = paid.Sum();
                    var dateListStr = String.Join(",", linkedInEngagement.Dates.Select(x => "'" + x + "'"));

                    htmlString = htmlString.Replace("_1firstLineChartData1", orgStr);
                    htmlString = htmlString.Replace("_2FirstLineChartData2", lostStr);
                    htmlString = htmlString.Replace("_3FirstLineChartData3", paidStr);
                    htmlString = htmlString.Replace("_4firstLineChartData4", commStr);
                    htmlString = htmlString.Replace("_5firstLineChartData5", likesStr);
                    htmlString = htmlString.Replace("_6firstLineChartData6", shareStr);
                    htmlString = htmlString.Replace("_2secondLineChartData2", clicksStr);
                    htmlString = htmlString.Replace("_3thirdLineChartData3", impressionStr);
                    htmlString = htmlString.Replace("_7pieChartData7", orgPieStr.ToString());
                    htmlString = htmlString.Replace("_8pieChartData8", paidPieStr.ToString());
                    htmlString = htmlString.Replace("_1firstLineChartLabels1", dateListStr);

                    int intervalRes = linkedInEngagement.Dates.Count <= 31 ? 3 : (linkedInEngagement.Dates.Count <= 91 ? 7 : 31);
                    htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

                    string uniqueKey = $"{reportReplaceData.Type}";
                    uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                    listOfResult.Add(uniqueTypeSubtypeResults);
                    //htmlArray.Add(htmlString);
                }
            }
            else if (reportReplaceData.Type == (int)ReportTypes.LinkedInDemographic)
            {
                var linkedInDemographic = JsonConvert.DeserializeObject<LinkedInDemographicChart>(reportReplaceData.RootReportData.LinkedInDemographic);
                if (linkedInDemographic != null)
                {
                    Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                    string pathLinkedin = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/linkedInDemographics.html");

                    using (HttpClient httpclient = new HttpClient())
                    {
                        htmlString = httpclient.GetStringAsync(pathLinkedin).Result;
                    }

                    //string pathLinkedin = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/linkedInDemographics.html");
                    //htmlString = System.IO.File.ReadAllText(pathLinkedin);


                    var countryLabel = linkedInDemographic.CountryLabel.Select(date => date.ToString()).ToArray();
                    var countryLabelStr = String.Join(",", countryLabel.Select(x => '"' + x + '"'));

                    var industryLabel = linkedInDemographic.IndustryLabel.Select(date => date.ToString()).ToArray();
                    var industryLabelStr = String.Join(",", industryLabel.Select(x => '"' + x + '"'));

                    var seniorityLabel = linkedInDemographic.SeniorityLabel.Select(date => date.ToString()).ToArray();
                    var seniorityLabelStr = String.Join(",", seniorityLabel.Select(x => '"' + x + '"'));

                    var companySizeLabel = linkedInDemographic.CompanySizeLabel.Select(date => date.ToString()).ToArray();
                    var companySizeLabelStr = String.Join(",", companySizeLabel.Select(x => '"' + x + '"'));

                    var jobFunctionLabel = linkedInDemographic.JobFunctionLabel.Select(date => date.ToString()).ToArray();
                    var jobFunctionLabelStr = String.Join(",", jobFunctionLabel.Select(x => '"' + x + '"'));

                    var countryData = String.Join(",", linkedInDemographic.CountryData.Select(x => "'" + x + "'"));
                    var industryData = String.Join(",", linkedInDemographic.IndustryData.Select(x => "'" + x + "'"));
                    var seniorityData = String.Join(",", linkedInDemographic.SeniorityData.Select(x => "'" + x + "'"));
                    var companySizeData = String.Join(",", linkedInDemographic.CompanySizeData.Select(x => "'" + x + "'"));
                    var jobFunctionData = String.Join(",", linkedInDemographic.JobFunctionData.Select(x => "'" + x + "'"));

                    htmlString = htmlString.Replace("_countryChartLabels_", countryLabelStr);
                    htmlString = htmlString.Replace("_countryChartData_", countryData);

                    htmlString = htmlString.Replace("_seniorityChartLabels_", seniorityLabelStr);
                    htmlString = htmlString.Replace("_seniorityChartData_", seniorityData);

                    htmlString = htmlString.Replace("_industryChartLabels_", industryLabelStr);
                    htmlString = htmlString.Replace("_industryChartData_", industryData);

                    htmlString = htmlString.Replace("_jobFunctionChartLabels_", jobFunctionLabelStr);
                    htmlString = htmlString.Replace("_jobFunctionChartData_", jobFunctionData);

                    htmlString = htmlString.Replace("_companySizeChartLabels_", companySizeLabelStr);
                    htmlString = htmlString.Replace("_companySizeChartData_", companySizeData);

                    string uniqueKey = $"{reportReplaceData.Type}";
                    uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                    listOfResult.Add(uniqueTypeSubtypeResults);
                    //htmlArray.Add(htmlString);
                }
            }
            else if (reportReplaceData.Type == (int)ReportTypes.LinkedInAdsCampaign)
            {
                var linkedInDemographic = JsonConvert.DeserializeObject<FrontCampaignRoot>(reportReplaceData.RootReportData.FrontCampaignRoot);
                if (linkedInDemographic != null)
                {
                    Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                    string pathLinkedin = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/linkedinAdCampaign.html");

                    using (HttpClient httpclient = new HttpClient())
                    {
                        htmlString = httpclient.GetStringAsync(pathLinkedin).Result;
                    }

                    //string pathLinkedin = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/linkedinAdCampaign.html");
                    //htmlString = System.IO.File.ReadAllText(pathLinkedin);

                    var tableListStr = JsonConvert.SerializeObject(linkedInDemographic.campaignRoot.elements, Formatting.Indented);
                    htmlString = htmlString.Replace("_tableArrayList_", tableListStr);


                    //prepare chart data
                    var chartData = linkedInDemographic.linkedinStat.elements.Select(x => x.clicks).ToList();
                    var chartStr = String.Join(",", chartData);

                    var formattedDates = linkedInDemographic.linkedinStat.elements
                        .Select(x => DateTime.ParseExact(x.date, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("MM-dd"))
                        .ToList();

                    var dateLabelStr = String.Join(",", formattedDates.Select(x => "'" + x + "'"));


                    htmlString = htmlString.Replace("_1noOfCampaigns1_", linkedInDemographic.cardData.ad_count.ToString());

                    //currency
                    htmlString = htmlString.Replace("_1ladsCurrency1_", linkedInDemographic.campaignRoot.elements[0].currency);

                    // data current
                    htmlString = htmlString.Replace("_1ladsClicks1_", linkedInDemographic.cardData.total_clicks.ToString());
                    htmlString = htmlString.Replace("_1ladsSpent1_", linkedInDemographic.cardData.total_spent.ToString());
                    htmlString = htmlString.Replace("_1ladsLeads1_", linkedInDemographic.cardData.total_leads.ToString());
                    htmlString = htmlString.Replace("_1ladsCpl1_", linkedInDemographic.cardData.total_cpl.ToString());

                    // data previous
                    htmlString = htmlString.Replace("_1ladsSpendDiff1_", linkedInDemographic.cardData.percent_spent.ToString());
                    htmlString = htmlString.Replace("_1ladsClicksDiff1_", linkedInDemographic.cardData.percent_clicks.ToString());
                    htmlString = htmlString.Replace("_1ladsLeadsDiff1_", linkedInDemographic.cardData.percent_leads.ToString());
                    htmlString = htmlString.Replace("_1ladsCplDiff1_", linkedInDemographic.cardData.percent_cpl.ToString());

                    htmlString = htmlString.Replace("_1linkedinAdsLineChartLabels_", dateLabelStr);
                    htmlString = htmlString.Replace("_1linkedinAdsLinchartData_", chartStr);


                    int intervalRes = formattedDates.Count <= 31 ? 3 : (formattedDates.Count <= 91 ? 7 : 31);
                    htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());


                    if (linkedInDemographic.dempgraphicRoot != null && linkedInDemographic.dempgraphicRoot.elements != null)
                    {
                        foreach (var item in linkedInDemographic.dempgraphicRoot.elements)
                        {
                            //calculate percentage for demographic

                            double percentage = double.Parse(linkedInDemographic.cardData.total_impressions) > 0 ? (double)item.impressions / double.Parse(linkedInDemographic.cardData.total_impressions) * 100 : 0;
                            item.percent_impressions = Math.Round(percentage, 2).ToString();

                            double percentage_click = double.Parse(linkedInDemographic.cardData.total_clicks) > 0 ? (double)item.clicks / double.Parse(linkedInDemographic.cardData.total_clicks) * 100 : 0;
                            item.percent_clicks = Math.Round(percentage_click, 2).ToString();
                        }

                        var tableListStr1 = JsonConvert.SerializeObject(linkedInDemographic.dempgraphicRoot.elements, Formatting.Indented);
                        htmlString = htmlString.Replace("_tableArrayList1_", tableListStr1);

                    }

                    string uniqueKey = $"{reportReplaceData.Type}";
                    uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                    listOfResult.Add(uniqueTypeSubtypeResults);
                    // htmlArray.Add(htmlString);

                }
            }
            else if (reportReplaceData.Type == (int)ReportTypes.LinkedInAdsAdgroups)
            {
                var linkedInDemographic = JsonConvert.DeserializeObject<FrontAdGroupRoot>(reportReplaceData.RootReportData.FrontAdGroupRoot);
                if (linkedInDemographic != null)
                {
                    Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                    string pathLinkedin = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/linkedinAdAdgroup.html");

                    using (HttpClient httpclient = new HttpClient())
                    {
                        htmlString = httpclient.GetStringAsync(pathLinkedin).Result;
                    }

                    //string pathLinkedin = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/linkedinAdAdgroup.html");
                    //htmlString = System.IO.File.ReadAllText(pathLinkedin);


                    var tableListStr = JsonConvert.SerializeObject(linkedInDemographic.adGroupRoot.elements, Formatting.Indented);
                    htmlString = htmlString.Replace("_tableArrayList_", tableListStr);


                    //prepare chart data
                    var chartData = linkedInDemographic.linkedinStat.elements.Select(x => x.clicks).ToList();
                    var chartStr = String.Join(",", chartData);

                    var formattedDates = linkedInDemographic.linkedinStat.elements
                        .Select(x => DateTime.ParseExact(x.date, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("MM-dd"))
                        .ToList();

                    var dateLabelStr = String.Join(",", formattedDates.Select(x => "'" + x + "'"));


                    htmlString = htmlString.Replace("_1noOfCampaigns1_", linkedInDemographic.cardData.ad_count.ToString());

                    //currency
                    htmlString = htmlString.Replace("_1ladsCurrency1_", linkedInDemographic.adGroupRoot.elements[0].currency);

                    // data current
                    htmlString = htmlString.Replace("_1ladsClicks1_", linkedInDemographic.cardData.total_clicks.ToString());
                    htmlString = htmlString.Replace("_1ladsSpent1_", linkedInDemographic.cardData.total_spent.ToString());
                    htmlString = htmlString.Replace("_1ladsLeads1_", linkedInDemographic.cardData.total_leads.ToString());
                    htmlString = htmlString.Replace("_1ladsCpl1_", linkedInDemographic.cardData.total_cpl.ToString());

                    // data previous
                    htmlString = htmlString.Replace("_1ladsSpendDiff1_", linkedInDemographic.cardData.percent_spent.ToString());
                    htmlString = htmlString.Replace("_1ladsClicksDiff1_", linkedInDemographic.cardData.percent_clicks.ToString());
                    htmlString = htmlString.Replace("_1ladsLeadsDiff1_", linkedInDemographic.cardData.percent_leads.ToString());
                    htmlString = htmlString.Replace("_1ladsCplDiff1_", linkedInDemographic.cardData.percent_cpl.ToString());

                    htmlString = htmlString.Replace("_1linkedinAdsLineChartLabels_", dateLabelStr);
                    htmlString = htmlString.Replace("_1linkedinAdsLinchartData_", chartStr);

                    int intervalRes = formattedDates.Count <= 31 ? 3 : (formattedDates.Count <= 91 ? 7 : 31);
                    htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());


                    if (linkedInDemographic.dempgraphicRoot != null && linkedInDemographic.dempgraphicRoot.elements != null)
                    {
                        foreach (var item in linkedInDemographic.dempgraphicRoot.elements)
                        {
                            //calculate percentage for demographic

                            double percentage = double.Parse(linkedInDemographic.cardData.total_impressions) > 0 ? (double)item.impressions / double.Parse(linkedInDemographic.cardData.total_impressions) * 100 : 0;
                            item.percent_impressions = Math.Round(percentage, 2).ToString();

                            double percentage_click = double.Parse(linkedInDemographic.cardData.total_clicks) > 0 ? (double)item.clicks / double.Parse(linkedInDemographic.cardData.total_clicks) * 100 : 0;
                            item.percent_clicks = Math.Round(percentage_click, 2).ToString();
                        }

                        var tableListStr1 = JsonConvert.SerializeObject(linkedInDemographic.dempgraphicRoot.elements, Formatting.Indented);
                        htmlString = htmlString.Replace("_tableArrayList1_", tableListStr1);
                    }

                    string uniqueKey = $"{reportReplaceData.Type}";
                    uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                    listOfResult.Add(uniqueTypeSubtypeResults);
                    //htmlArray.Add(htmlString);

                }
            }
            else if (reportReplaceData.Type == (int)ReportTypes.LinkedInAdsCreative)
            {
                var linkedInDemographic = JsonConvert.DeserializeObject<FrontCreativeRoot>(reportReplaceData.RootReportData.FrontCreativeRoot);
                if (linkedInDemographic != null)
                {
                    Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                    string pathLinkedin = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/linkedinAdCreative.html");

                    using (HttpClient httpclient = new HttpClient())
                    {
                        htmlString = httpclient.GetStringAsync(pathLinkedin).Result;
                    }

                    //string pathLinkedin = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/linkedinAdCreative.html");
                    //htmlString = System.IO.File.ReadAllText(pathLinkedin);


                    var tableListStr = JsonConvert.SerializeObject(linkedInDemographic.creativeRoot.elements, Formatting.Indented);
                    htmlString = htmlString.Replace("_tableArrayList_", tableListStr);


                    //prepare chart data
                    var chartData = linkedInDemographic.linkedinStat.elements.Select(x => x.clicks).ToList();
                    var chartStr = String.Join(",", chartData);

                    var formattedDates = linkedInDemographic.linkedinStat.elements
                        .Select(x => DateTime.ParseExact(x.date, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("MM-dd"))
                        .ToList();

                    var dateLabelStr = String.Join(",", formattedDates.Select(x => "'" + x + "'"));


                    htmlString = htmlString.Replace("_1noOfCampaigns1_", linkedInDemographic.cardData.ad_count.ToString());

                    //currency
                    htmlString = htmlString.Replace("_1ladsCurrency1_", linkedInDemographic.creativeRoot.elements[0].currency);

                    // data current
                    htmlString = htmlString.Replace("_1ladsClicks1_", linkedInDemographic.cardData.total_clicks.ToString());
                    htmlString = htmlString.Replace("_1ladsSpent1_", linkedInDemographic.cardData.total_spent.ToString());
                    htmlString = htmlString.Replace("_1ladsLeads1_", linkedInDemographic.cardData.total_leads.ToString());
                    htmlString = htmlString.Replace("_1ladsCpl1_", linkedInDemographic.cardData.total_cpl.ToString());

                    // data previous
                    htmlString = htmlString.Replace("_1ladsSpendDiff1_", linkedInDemographic.cardData.percent_spent.ToString());
                    htmlString = htmlString.Replace("_1ladsClicksDiff1_", linkedInDemographic.cardData.percent_clicks.ToString());
                    htmlString = htmlString.Replace("_1ladsLeadsDiff1_", linkedInDemographic.cardData.percent_leads.ToString());
                    htmlString = htmlString.Replace("_1ladsCplDiff1_", linkedInDemographic.cardData.percent_cpl.ToString());

                    htmlString = htmlString.Replace("_1linkedinAdsLineChartLabels_", dateLabelStr);
                    htmlString = htmlString.Replace("_1linkedinAdsLinchartData_", chartStr);

                    int intervalRes = formattedDates.Count <= 31 ? 3 : (formattedDates.Count <= 91 ? 7 : 31);
                    htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());


                    if (linkedInDemographic.dempgraphicRoot != null && linkedInDemographic.dempgraphicRoot.elements != null)
                    {
                        foreach (var item in linkedInDemographic.dempgraphicRoot.elements)
                        {
                            //calculate percentage for demographic

                            double percentage = double.Parse(linkedInDemographic.cardData.total_impressions) > 0 ? (double)item.impressions / double.Parse(linkedInDemographic.cardData.total_impressions) * 100 : 0;
                            item.percent_impressions = Math.Round(percentage, 2).ToString();

                            double percentage_click = double.Parse(linkedInDemographic.cardData.total_clicks) > 0 ? (double)item.clicks / double.Parse(linkedInDemographic.cardData.total_clicks) * 100 : 0;
                            item.percent_clicks = Math.Round(percentage_click, 2).ToString();
                        }

                        var tableListStr1 = JsonConvert.SerializeObject(linkedInDemographic.dempgraphicRoot.elements, Formatting.Indented);
                        htmlString = htmlString.Replace("_tableArrayList1_", tableListStr1);
                    }

                    string uniqueKey = $"{reportReplaceData.Type}";
                    uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                    listOfResult.Add(uniqueTypeSubtypeResults);
                    //htmlArray.Add(htmlString);
                }
            }
            else if (reportReplaceData.Type == (int)ReportTypes.Keywords)
            {
                var keywords = JsonConvert.DeserializeObject<List<RootKeywordsReportData>>(reportReplaceData.RootReportData.Keywords);
                if (keywords != null)
                {
                    Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                    string pathKeyword = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/keywords.html");

                    using (HttpClient httpclient = new HttpClient())
                    {
                        htmlString = httpclient.GetStringAsync(pathKeyword).Result;
                    }
                    foreach (var k in keywords)
                    {
                        if (Convert.ToInt16(k.PrevLocalPackCount) > 0)
                        {
                            k.PreviousPosition = k.PreviousPosition + "&nbsp;&nbsp;<small><span><i class='fas fa-map-marker-alt'></i>&nbsp;&nbsp;" + k.PrevLocalPackCount + "</span></small>";
                        }

                        if (Convert.ToInt16(k.CurrentLocalPackCount) > 0)
                        {
                            k.CurrentPosition = k.CurrentPosition + "&nbsp;&nbsp;<small><span><i class='fas fa-map-marker-alt'></i>&nbsp;&nbsp;" + k.CurrentLocalPackCount + "</span></small>";
                        }
                        if (k.change.Contains("-"))
                        {
                            k.change = "<span class='text-danger'> <i class='fas fa-arrow-alt-square-down' ></i></span> " + k.change;
                        }
                        else
                        {
                            if (k.change == "0")
                            {
                                k.change = "-";
                            }
                            else
                            {
                                k.change = "<span class='text-success'><i class='fas fa-arrow-alt-square-up'></i></span> " + k.change;
                            }
                        }
                    }

                    var tableString = JsonConvert.SerializeObject(keywords);
                    htmlString = htmlString.Replace("_tableArrayList_", tableString);

                    string uniqueKey = $"{reportReplaceData.Type}";
                    uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                    listOfResult.Add(uniqueTypeSubtypeResults);
                    //htmlArray.Add(htmlString);
                }
            }
            else if (reportReplaceData.Type == (int)ReportTypes.GoogleAnalyticsFour)
            {
                var googleAnalyticsfour = JsonConvert.DeserializeObject<RootGAReportData>(reportReplaceData.RootReportData.GoogleAnalyticsFour);

                if (googleAnalyticsfour != null)
                {

                    var dateListStr = String.Join(",", googleAnalyticsfour.Dates.Select(x => "'" + x + "'"));

                    foreach (var subtype in reportReplaceData.SubType)
                    {
                        var intsubtype = Convert.ToInt16(subtype);
                        if (intsubtype == (int)ReportTypes.Ga4OrganicTaffic)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/ga4Organic.html");
                            //htmlString = System.IO.File.ReadAllText(pathGa);

                            string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/ga4Organic.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathGa).Result;
                            }


                            var trafficCurr = String.Join(",", googleAnalyticsfour.Traffic.Current.Select(x => "'" + x + "'"));


                            var trafficPre = String.Join(",", googleAnalyticsfour.Traffic.Previous.Select(x => "'" + x + "'"));
                            //var dateListStr = String.Join(",", googleAnalyticsfour.Dates.Select(x => "'" + x + "'"));


                            var trafficData = googleAnalyticsfour.Traffic.Current.Sum(x => x) + "--" + googleAnalyticsfour.Traffic.Previous.Sum(x => x);
                            var trafficDifference = PrepareDataGa4(trafficData);
                            htmlString = htmlString.Replace("_trafficDifference_", trafficDifference);

                            var str = trafficDifference;
                            var hasPlusSign = str.Contains("+");
                            if (hasPlusSign)
                            {
                                htmlString = htmlString.Replace("_trafficdiffcolor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_trafficdiffcolor_", "red");
                            }


                            htmlString = htmlString.Replace("_1gaOrganicTrafficLables1_", dateListStr);
                            htmlString = htmlString.Replace("_1gaOrganicTrafficData1_", trafficCurr);
                            htmlString = htmlString.Replace("_2gaOrganicTrafficData2_", trafficPre);

                            int intervalRes = googleAnalyticsfour.Dates.Count <= 31 ? 3 : (googleAnalyticsfour.Dates.Count <= 91 ? 7 : 31);
                            htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

                            string uniqueKey = $"{reportReplaceData.Type}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                            //htmlArray.Add(htmlString);
                        }
                        else if (intsubtype == (int)ReportTypes.Ga4OrganicConversion)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/ga4Conversion.html");
                            //htmlString = System.IO.File.ReadAllText(pathGa);

                            string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/ga4Conversion.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathGa).Result;
                            }

                            var conversionCurr = String.Join(",", googleAnalyticsfour.Conversions.Current.Select(x => "'" + x + "'"));
                            var conversionPre = String.Join(",", googleAnalyticsfour.Conversions.Previous.Select(x => "'" + x + "'"));

                            var conversionData = googleAnalyticsfour.Conversions.Current.Sum(x => x) + "--" + googleAnalyticsfour.Conversions.Previous.Sum(x => x);

                            var conDifference = PrepareDataGa4(conversionData);

                            htmlString = htmlString.Replace("_conDifference_", conDifference);

                            var str1 = conDifference;
                            var hasPlusSign1 = str1.Contains("+");
                            if (hasPlusSign1)
                            {
                                htmlString = htmlString.Replace("_condiffcolor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_condiffcolor_", "red");
                            }

                            htmlString = htmlString.Replace("_1gaConverionLabels1_", dateListStr);
                            htmlString = htmlString.Replace("_1gaConverionData1_", conversionCurr);
                            htmlString = htmlString.Replace("_2gaConverionData2_", conversionPre);

                            int intervalRes = googleAnalyticsfour.Dates.Count <= 31 ? 3 : (googleAnalyticsfour.Dates.Count <= 91 ? 7 : 31);
                            htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

                            string uniqueKey = $"{reportReplaceData.Type}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                            //htmlArray.Add(htmlString);
                        }
                        else if (intsubtype == (int)ReportTypes.Ga4UserAquasition)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/ga4UserAqa.html");
                            //htmlString = System.IO.File.ReadAllText(pathGa);

                            string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/ga4UserAqa.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathGa).Result;
                            }

                            //date labels
                            htmlString = htmlString.Replace("_1gaConverionLabels1_", dateListStr);


                            //User Aquisition line chart
                            //htmlString = htmlString.Replace("_1gaConverionLabels1_", labelStr);
                            htmlString = htmlString.Replace("_dataset1Data_", string.Join(",", googleAnalyticsfour.UserAquisition.LineChartData.Direct));
                            htmlString = htmlString.Replace("_dataset2Data_", string.Join(",", googleAnalyticsfour.UserAquisition.LineChartData.OrganicSearch));
                            htmlString = htmlString.Replace("_dataset3Data_", string.Join(",", googleAnalyticsfour.UserAquisition.LineChartData.OrganicSocial));
                            htmlString = htmlString.Replace("_dataset4Data_", string.Join(",", googleAnalyticsfour.UserAquisition.LineChartData.Referral));
                            htmlString = htmlString.Replace("_dataset5Data_", string.Join(",", googleAnalyticsfour.UserAquisition.LineChartData.Unassigned));

                            var totalUserAqBarCurrent = googleAnalyticsfour.UserAquisition.Current.Sum(x => x).ToString();
                            var totalUserAqBarPrev = googleAnalyticsfour.UserAquisition.Previous.Sum(x => x).ToString();

                            //User Aquisition bar chart
                            //htmlString = htmlString.Replace("_1gaConverionLabels1_", labelStr);
                            htmlString = htmlString.Replace("_userAquDataCurrent_", string.Join(",", googleAnalyticsfour.UserAquisition.Current));
                            htmlString = htmlString.Replace("_userAquDataPrev_", string.Join(",", googleAnalyticsfour.UserAquisition.Previous));

                            var userAquData = totalUserAqBarCurrent + "--" + totalUserAqBarPrev;

                            var userAquaDiff = PrepareDataGa4(userAquData);

                            htmlString = htmlString.Replace("_userAquaDiff_", userAquaDiff);

                            int intervalRes = googleAnalyticsfour.Dates.Count <= 31 ? 3 : (googleAnalyticsfour.Dates.Count <= 91 ? 7 : 31);
                            htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

                            var str2 = userAquaDiff;
                            var hasPlusSign2 = str2.Contains("+");
                            if (hasPlusSign2)
                            {
                                htmlString = htmlString.Replace("_userAquaDiffColor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_userAquaDiffColor_", "red");
                            }

                            string uniqueKey = $"{reportReplaceData.Type}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                            //htmlArray.Add(htmlString);
                        }
                        else if (intsubtype == (int)ReportTypes.Ga4TrafficAquasition)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                            Dictionary<string, string> uniqueTypeSubtypeResults1 = new Dictionary<string, string>();
                            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/ga4TrafficAqa.html");
                            //htmlString = System.IO.File.ReadAllText(pathGa);

                            string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/ga4TrafficAqa.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathGa).Result;
                            }

                            //date labels
                            htmlString = htmlString.Replace("_1gaConverionLabels1_", dateListStr);


                            //Traffic Aquasition
                            htmlString = htmlString.Replace("_dataset1TrafficData_", string.Join(",", googleAnalyticsfour.TrafficAquisition.LineChartData.Direct));
                            htmlString = htmlString.Replace("_dataset2TrafficData_", string.Join(",", googleAnalyticsfour.TrafficAquisition.LineChartData.OrganicSearch));
                            htmlString = htmlString.Replace("_dataset3TrafficData_", string.Join(",", googleAnalyticsfour.TrafficAquisition.LineChartData.OrganicSocial));
                            htmlString = htmlString.Replace("_dataset4TrafficData_", string.Join(",", googleAnalyticsfour.TrafficAquisition.LineChartData.Referral));
                            htmlString = htmlString.Replace("_dataset5TrafficData_", string.Join(",", googleAnalyticsfour.TrafficAquisition.LineChartData.Unassigned));

                            var totalTraAqBarCurrent = googleAnalyticsfour.TrafficAquisition.Current.Sum(x => x).ToString();
                            var totalTraAqBarPrev = googleAnalyticsfour.TrafficAquisition.Previous.Sum(x => x).ToString();

                            //Traffic Aquisition bar chart
                            //htmlString = htmlString.Replace("_1gaConverionLabels1_", labelStr);
                            htmlString = htmlString.Replace("_trafficAquDataCurrent_", string.Join(",", googleAnalyticsfour.TrafficAquisition.Current));
                            htmlString = htmlString.Replace("_trafficAquDataPrev_", string.Join(",", googleAnalyticsfour.TrafficAquisition.Previous));

                            var AquDataTra = totalTraAqBarCurrent + "--" + totalTraAqBarPrev;

                            var AquaDiffTra = PrepareDataGa4(AquDataTra);

                            htmlString = htmlString.Replace("_trafficAquaDiff_", AquaDiffTra);

                            int intervalRes = googleAnalyticsfour.Dates.Count <= 31 ? 3 : (googleAnalyticsfour.Dates.Count <= 91 ? 7 : 31);
                            htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

                            var str3 = AquaDiffTra;
                            var hasPlusSign1Tra = str3.Contains("+");
                            if (hasPlusSign1Tra)
                            {
                                htmlString = htmlString.Replace("_trafficAquaDiffColor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_trafficAquaDiffColor_", "red");
                            }
                            string uniqueKey = $"{reportReplaceData.Type}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                            //htmlArray.Add(htmlString);
                        }
                        else if (intsubtype == (int)ReportTypes.Ga4EcomPurchase)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/ga4EcomPurchase.html");
                            //htmlString = System.IO.File.ReadAllText(pathGa);

                            string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/ga4EcomPurchase.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathGa).Result;
                            }

                            //date labels
                            htmlString = htmlString.Replace("_1gaConverionLabels1_", dateListStr);

                            //Ecom Purchase
                            List<Dataset> datasets = new List<Dataset>();

                            if (googleAnalyticsfour.EcommercePurchases.Count > 4)
                            {
                                googleAnalyticsfour.EcommercePurchases = googleAnalyticsfour.EcommercePurchases.Take(5).ToList();
                            }
                            else
                            {
                                googleAnalyticsfour.EcommercePurchases = googleAnalyticsfour.EcommercePurchases.Take(googleAnalyticsfour.EcommercePurchases.Count).ToList();
                            }

                            for (int i = 0; i < googleAnalyticsfour.EcommercePurchases.Count; i++)
                            {
                                Dataset dataset = new Dataset
                                {
                                    fill = false,
                                    data = googleAnalyticsfour.EcommercePurchases[i].ItemPurchased,
                                    label = googleAnalyticsfour.EcommercePurchases[i].ItemName,
                                    backgroundColor = GetBackgroundColor(i),
                                    borderColor = GetBackgroundColor(i),
                                    borderWidth = 1,
                                    pointRadius = 2
                                };

                                datasets.Add(dataset);
                            }

                            // Convert to JSON
                            string jsonData = JsonConvert.SerializeObject(datasets, Formatting.Indented);

                            htmlString = htmlString.Replace("_ecomPurchaseData_", jsonData);

                            var tableListStr1 = JsonConvert.SerializeObject(googleAnalyticsfour.EcommercePurchases, Formatting.Indented);
                            htmlString = htmlString.Replace("_ecomtableArrayList_", tableListStr1);
                            int intervalRes = googleAnalyticsfour.Dates.Count <= 31 ? 3 : (googleAnalyticsfour.Dates.Count <= 91 ? 7 : 31);
                            htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());
                            string uniqueKey = $"{reportReplaceData.Type}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                            //htmlArray.Add(htmlString);
                        }
                        else if (intsubtype == (int)ReportTypes.Ga4PurchaseJourney)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/ga4Purchase.html");
                            //htmlString = System.IO.File.ReadAllText(pathGa);

                            string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/ga4Purchase.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathGa).Result;
                            }

                            //date labels
                            htmlString = htmlString.Replace("_1gaConverionLabels1_", dateListStr);
                            int intervalRes = googleAnalyticsfour.Dates.Count <= 31 ? 3 : (googleAnalyticsfour.Dates.Count <= 91 ? 7 : 31);
                            htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

                            //Purchase Journey
                            var pj = googleAnalyticsfour.PurchaseJourney.PurchaseTotalSessionStart + "," + googleAnalyticsfour.PurchaseJourney.PurchaseTotalViewItem + ","
                                + googleAnalyticsfour.PurchaseJourney.PurchaseTotalAddedCart + "," + googleAnalyticsfour.PurchaseJourney.PurchaseTotalCheckout + "," + googleAnalyticsfour.PurchaseJourney.PurchaseTotalPurchase;
                            htmlString = htmlString.Replace("_purchaseData_", pj);
                            string uniqueKey = $"{reportReplaceData.Type}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                            //htmlArray.Add(htmlString);
                        }
                    }
                }
            }
            else if (reportReplaceData.Type == (int)ReportTypes.LightHouseData)
            {
                var lightHouse = JsonConvert.DeserializeObject<RootPageSpeedReportData>(reportReplaceData.RootReportData.LightHouse);

                if (lightHouse != null)
                {
                    Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                    string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/pageSpeed.html");

                    using (HttpClient httpclient = new HttpClient())
                    {
                        htmlString = httpclient.GetStringAsync(pathGa).Result;
                    }

                    htmlString = htmlString.Replace("_mperformanceScoreMobile", lightHouse.Mobile);
                    htmlString = htmlString.Replace("_mperformanceScoreDesktop", lightHouse.Desktop);

                    string uniqueKey = $"{reportReplaceData.Type}";
                    uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                    listOfResult.Add(uniqueTypeSubtypeResults);
                    //htmlArray.Add(htmlString);
                }
            }
            else if (reportReplaceData.Type == (int)ReportTypes.GoogleBusinessProfile)
            {
                var gbpData = JsonConvert.DeserializeObject<RootGbpData>(reportReplaceData.RootReportData.GbpData);

                if (gbpData != null)
                {
                    foreach (var subtype in reportReplaceData.SubType)
                    {
                        var intsubtype = Convert.ToInt16(subtype);
                        if (intsubtype == (int)ReportTypes.GBPSearches)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/gbpSearches.html");
                            //htmlString = System.IO.File.ReadAllText(pathGa);

                            string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/gbpSearches.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathGa).Result;
                            }

                            // serialize object
                            var tableString = JsonConvert.SerializeObject(gbpData.KeywordData);
                            htmlString = htmlString.Replace("_gbpKeywordArrayList_", tableString);

                            int intervalRes = gbpData.DateLabels.Count <= 31 ? 3 : (gbpData.DateLabels.Count <= 91 ? 7 : 31);
                            htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());


                            //Difference

                            //Search keyword diff
                            htmlString = htmlString.Replace("_searchKeywordDiff_", gbpData.SearchKeywordDiff);

                            var hasPlusSign8 = gbpData.SearchKeywordDiff.Contains("+");
                            if (hasPlusSign8)
                            {
                                htmlString = htmlString.Replace("_searchKeywordDiffColor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_searchKeywordDiffColor_", "red");
                            }

                            htmlString = htmlString.Replace("_vsDateRange_", gbpData.VsDateRange.ToString());

                            string uniqueKey = $"{reportReplaceData.Type}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (intsubtype == (int)ReportTypes.GBPBookings)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/gbpBookings.html");
                            //htmlString = System.IO.File.ReadAllText(pathGa);

                            string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/gbpBookings.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathGa).Result;
                            }

                            var dateLabelStr = String.Join(",", gbpData.DateLabels.Select(x => "'" + x + "'"));

                            var bookingChart = String.Join(",", gbpData.BookingChartData);

                            htmlString = htmlString.Replace("_1gbpInterChartLabels_", dateLabelStr);

                            htmlString = htmlString.Replace("_1gbpBookingChartData_", bookingChart);

                            int intervalRes = gbpData.DateLabels.Count <= 31 ? 3 : (gbpData.DateLabels.Count <= 91 ? 7 : 31);
                            htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

                            //Booking
                            htmlString = htmlString.Replace("_bookingsDiff_", gbpData.BookingDiff);

                            var hasPlusSign5 = gbpData.BookingDiff.Contains("+");
                            if (hasPlusSign5)
                            {
                                htmlString = htmlString.Replace("_bookingsDiffColor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_bookingsDiffColor_", "red");
                            }

                            htmlString = htmlString.Replace("_vsDateRange_", gbpData.VsDateRange.ToString());

                            string uniqueKey = $"{reportReplaceData.Type}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (intsubtype == (int)ReportTypes.GBPCalls)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/gbpCalls.html");
                            //htmlString = System.IO.File.ReadAllText(pathGa);

                            string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/gbpCalls.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathGa).Result;
                            }

                            var dateLabelStr = String.Join(",", gbpData.DateLabels.Select(x => "'" + x + "'"));

                            var callChart = String.Join(",", gbpData.CallChartData);

                            htmlString = htmlString.Replace("_1gbpInterChartLabels_", dateLabelStr);
                            htmlString = htmlString.Replace("_1gbpCallChartData_", callChart);

                            int intervalRes = gbpData.DateLabels.Count <= 31 ? 3 : (gbpData.DateLabels.Count <= 91 ? 7 : 31);
                            htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

                            //Difference                            
                            //Call

                            htmlString = htmlString.Replace("_callDiff_", gbpData.CallDiff);

                            var hasPlusSign3 = gbpData.CallDiff.Contains("+");
                            if (hasPlusSign3)
                            {
                                htmlString = htmlString.Replace("_callDiffColor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_callDiffColor_", "red");
                            }

                            htmlString = htmlString.Replace("_vsDateRange_", gbpData.VsDateRange.ToString());

                            string uniqueKey = $"{reportReplaceData.Type}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (intsubtype == (int)ReportTypes.GBPDirections)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/gbpDirections.html");
                            //htmlString = System.IO.File.ReadAllText(pathGa);

                            string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/gbpDirections.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathGa).Result;
                            }

                            var dateLabelStr = String.Join(",", gbpData.DateLabels.Select(x => "'" + x + "'"));

                            var directionChart = String.Join(",", gbpData.DirectionChartData);

                            htmlString = htmlString.Replace("_1gbpInterChartLabels_", dateLabelStr);


                            htmlString = htmlString.Replace("_1gbpDirectionChartData_", directionChart);

                            int intervalRes = gbpData.DateLabels.Count <= 31 ? 3 : (gbpData.DateLabels.Count <= 91 ? 7 : 31);
                            htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());


                            //Difference                          
                            //Direction

                            htmlString = htmlString.Replace("_directionDiff_", gbpData.DirectionDiff);

                            var hasPlusSign6 = gbpData.DirectionDiff.Contains("+");
                            if (hasPlusSign6)
                            {
                                htmlString = htmlString.Replace("_directionDiffColor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_directionDiffColor_", "red");
                            }

                            htmlString = htmlString.Replace("_vsDateRange_", gbpData.VsDateRange.ToString());


                            string uniqueKey = $"{reportReplaceData.Type}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (intsubtype == (int)ReportTypes.GBPInteraction)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/gbpInteraction.html");
                            //htmlString = System.IO.File.ReadAllText(pathGa);

                            string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/gbpInteraction.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathGa).Result;
                            }

                            var dateLabelStr = String.Join(",", gbpData.DateLabels.Select(x => "'" + x + "'"));

                            var ineractionChart = String.Join(",", gbpData.InteractionChartData);

                            htmlString = htmlString.Replace("_1gbpInterChartLabels_", dateLabelStr);

                            htmlString = htmlString.Replace("_1gbpInterChartData_", ineractionChart);

                            int intervalRes = gbpData.DateLabels.Count <= 31 ? 3 : (gbpData.DateLabels.Count <= 91 ? 7 : 31);
                            htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());


                            //Difference                          
                            //Profile Interaction

                            htmlString = htmlString.Replace("_profileInterDiff_", gbpData.ProfileInteractionDiff);

                            var hasPlusSign2 = gbpData.ProfileInteractionDiff.Contains("+");
                            if (hasPlusSign2)
                            {
                                htmlString = htmlString.Replace("_profileInterDiffColor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_profileInterDiffColor_", "red");
                            }

                            htmlString = htmlString.Replace("_vsDateRange_", gbpData.VsDateRange.ToString());

                            string uniqueKey = $"{reportReplaceData.Type}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (intsubtype == (int)ReportTypes.GBPMessages)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/gbpMessages.html");
                            //htmlString = System.IO.File.ReadAllText(pathGa);

                            string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/gbpMessages.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathGa).Result;
                            }

                            var dateLabelStr = String.Join(",", gbpData.DateLabels.Select(x => "'" + x + "'"));


                            var messageChart = String.Join(",", gbpData.MessageChartData);


                            htmlString = htmlString.Replace("_1gbpInterChartLabels_", dateLabelStr);

                            htmlString = htmlString.Replace("_1gbpMessageChartData_", messageChart);

                            int intervalRes = gbpData.DateLabels.Count <= 31 ? 3 : (gbpData.DateLabels.Count <= 91 ? 7 : 31);
                            htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());


                            //Difference                            

                            //Message

                            htmlString = htmlString.Replace("_messageDiff_", gbpData.MessageDiff);

                            var hasPlusSign4 = gbpData.MessageDiff.Contains("+");
                            if (hasPlusSign4)
                            {
                                htmlString = htmlString.Replace("_messageColor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_messageColor_", "red");
                            }

                            htmlString = htmlString.Replace("_vsDateRange_", gbpData.VsDateRange.ToString());

                            string uniqueKey = $"{reportReplaceData.Type}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (intsubtype == (int)ReportTypes.GBPWebsiteClicks)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/gbpWebsiteClicks.html");
                            //htmlString = System.IO.File.ReadAllText(pathGa);

                            string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/gbpWebsiteClicks.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathGa).Result;
                            }

                            var dateLabelStr = String.Join(",", gbpData.DateLabels.Select(x => "'" + x + "'"));

                            var websiteChart = String.Join(",", gbpData.InteractionChartData);

                            htmlString = htmlString.Replace("_1gbpInterChartLabels_", dateLabelStr);

                            htmlString = htmlString.Replace("_1gbpWebsiteChartData_", websiteChart);

                            int intervalRes = gbpData.DateLabels.Count <= 31 ? 3 : (gbpData.DateLabels.Count <= 91 ? 7 : 31);
                            htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

                            //Difference                          
                            //Website Click

                            htmlString = htmlString.Replace("_websiteClicksDiff_", gbpData.CallDiff);

                            var hasPlusSign7 = gbpData.CallDiff.Contains("+");
                            if (hasPlusSign7)
                            {
                                htmlString = htmlString.Replace("_websiteDiffColor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_websiteDiffColor_", "red");
                            }

                            htmlString = htmlString.Replace("_vsDateRange_", gbpData.VsDateRange.ToString());

                            string uniqueKey = $"{reportReplaceData.Type}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (intsubtype == (int)ReportTypes.GBPViews)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/gbpViews.html");
                            //htmlString = System.IO.File.ReadAllText(pathGa);

                            string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/gbpViews.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathGa).Result;
                            }


                            htmlString = htmlString.Replace("_gSearchMobileData_", " " + gbpData.PercentGoogleSearchMobile.ToString() + "%");
                            htmlString = htmlString.Replace("_gMapMobileData_", " " + gbpData.PercentGoogleMapMobile.ToString() + "%");
                            htmlString = htmlString.Replace("_gSearchDesktopData_", " " + gbpData.PercentGoogleSearchDesktop.ToString() + "%");
                            htmlString = htmlString.Replace("_gMapDesktopData_", " " + gbpData.PercentGoogleMapDesktop.ToString() + "%");
                            htmlString = htmlString.Replace("_gMapDesktopData_", gbpData.PercentGoogleMapDesktop.ToString());

                            var dateLabelStr = String.Join(",", gbpData.DateLabels.Select(x => "'" + x + "'"));

                            var ineractionChart = String.Join(",", gbpData.InteractionChartData);
                            var callChart = String.Join(",", gbpData.CallChartData);
                            var messageChart = String.Join(",", gbpData.MessageChartData);
                            var bookingChart = String.Join(",", gbpData.BookingChartData);
                            var directionChart = String.Join(",", gbpData.DirectionChartData);
                            var websiteChart = String.Join(",", gbpData.InteractionChartData);

                            htmlString = htmlString.Replace("_1gbpInterChartLabels_", dateLabelStr);

                            htmlString = htmlString.Replace("_1gbpInterChartData_", ineractionChart);
                            htmlString = htmlString.Replace("_1gbpCallChartData_", callChart);
                            htmlString = htmlString.Replace("_1gbpMessageChartData_", messageChart);
                            htmlString = htmlString.Replace("_1gbpBookingChartData_", bookingChart);
                            htmlString = htmlString.Replace("_1gbpDirectionChartData_", directionChart);
                            htmlString = htmlString.Replace("_1gbpWebsiteChartData_", websiteChart);

                            htmlString = htmlString.Replace("_gbpPieChartData_", gbpData.TotalSearchMobile
                                + "," + gbpData.TotalMapMobile + "," + gbpData.TotalSearchDesktop +
                                "," + gbpData.TotalMapDesktop);

                            int intervalRes = gbpData.DateLabels.Count <= 31 ? 3 : (gbpData.DateLabels.Count <= 91 ? 7 : 31);
                            htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

                            //Difference
                            //Profile View
                            htmlString = htmlString.Replace("_profileViewDiff_", gbpData.ProfileViewDiff);

                            var hasPlusSign1 = gbpData.ProfileViewDiff.Contains("+");
                            if (hasPlusSign1)
                            {
                                htmlString = htmlString.Replace("_profileViewDiffColor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_profileViewDiffColor_", "red");
                            }

                            htmlString = htmlString.Replace("_vsDateRange_", gbpData.VsDateRange.ToString());

                            string uniqueKey = $"{reportReplaceData.Type}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                    }
                }
            }
            else if (reportReplaceData.Type == (int)ReportTypes.WooCommerce)
            {
                var wcData = JsonConvert.DeserializeObject<RootWcReportData>(reportReplaceData.RootReportData.WcReportData);

                if (wcData != null)
                {
                    foreach (var subtype in reportReplaceData.SubType)
                    {
                        var intsubtype = Convert.ToInt16(subtype);
                        if (intsubtype == (int)ReportTypes.WCOrders)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/wcOrders.html");
                            //htmlString = System.IO.File.ReadAllText(pathGa);

                            string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/wcOrders.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathGa).Result;
                            }

                            var dateLabelStr = String.Join(",", wcData.DateLabel.Select(x => "'" + x + "'"));

                            var orderChart = String.Join(",", wcData.OrdersChartData);

                            htmlString = htmlString.Replace("_1wcDateChartLabels_", dateLabelStr);

                            htmlString = htmlString.Replace("_1wcOrderChartData_", orderChart);

                            int intervalRes = wcData.DateLabel.Count <= 31 ? 3 : (wcData.DateLabel.Count <= 91 ? 7 : 31);
                            htmlString = htmlString.Replace("_wcLabelInterval_", intervalRes.ToString());

                            //Difference
                            //Order chart
                            htmlString = htmlString.Replace("_1wcOrderChartDiffData_", wcData.OrdersChartDiff);

                            var hasPlusSign1 = wcData.OrdersChartDiff.Contains("+");
                            if (hasPlusSign1)
                            {
                                htmlString = htmlString.Replace("_1wcOrderChartDiffColor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_1wcOrderChartDiffColor_", "red");
                            }

                            string uniqueKey = $"{reportReplaceData.Type}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (intsubtype == (int)ReportTypes.WCLocationChart)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/wcLocationChart.html");
                            //htmlString = System.IO.File.ReadAllText(pathGa);

                            string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/wcLocationChart.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathGa).Result;
                            }


                            List<string> originalColors = new List<string>
                            {
                                "rgb(242, 153, 0)",
                                "rgb(71, 71, 235)",
                                "rgb(217, 48, 37)",
                                "rgb(0, 100, 0)",
                                "rgb(255, 0, 127)"
                            };

                            if (wcData.LocationChartData != null && wcData.LocationChartData.Count() > 0)
                            {
                                // Limit the count to 5 if it's greater than 5
                                var bgColorCount = Math.Min(wcData.LocationChartData.Count(), 5);

                                // Create a new list by selecting elements from the original list based on the count
                                var selectedColors = string.Join(",", originalColors.GetRange(0, bgColorCount).Select(x => "'" + x + "'"));
                                htmlString = htmlString.Replace("_wcBgColorPieChart_", selectedColors);
                            }


                            var LocationChartLabel = String.Join(",", wcData.Locationdata.Select(x => "'" + x.Key + " " + x.Value + "'"));

                            var LocationChart = String.Join(",", wcData.LocationChartData != null ? wcData.LocationChartData : new double[] { });

                            htmlString = htmlString.Replace("_wcPieChartData_", LocationChart);
                            htmlString = htmlString.Replace("_wcPieChartLabel_", LocationChartLabel);

                            string uniqueKey = $"{reportReplaceData.Type}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (intsubtype == (int)ReportTypes.WCReturnCustomer)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/wcReturnCustomer.html");
                            //htmlString = System.IO.File.ReadAllText(pathGa);

                            string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/wcReturnCustomer.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathGa).Result;
                            }


                            var dateLabelStr = String.Join(",", wcData.DateLabel.Select(x => "'" + x + "'"));


                            var returnCustomerChart = String.Join(",", wcData.ReturningCustomerChartRate);

                            htmlString = htmlString.Replace("_1wcDateChartLabels_", dateLabelStr);

                            htmlString = htmlString.Replace("_1wcReturnChartData_", returnCustomerChart);

                            int intervalRes = wcData.DateLabel.Count <= 31 ? 3 : (wcData.DateLabel.Count <= 91 ? 7 : 31);
                            htmlString = htmlString.Replace("_wcLabelInterval_", intervalRes.ToString());


                            //Difference

                            //Returning Chart

                            htmlString = htmlString.Replace("_1wcReturnChartDiffData_", wcData.ReturningChartRateDiff);

                            var hasPlusSign2 = wcData.ReturningChartRateDiff.Contains("+");
                            if (hasPlusSign2)
                            {
                                htmlString = htmlString.Replace("_1wcReturnChartDiffColor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_1wcReturnChartDiffColor_", "red");
                            }

                            string uniqueKey = $"{reportReplaceData.Type}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (intsubtype == (int)ReportTypes.WCRevenueTable)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/wcRevenueTable.html");
                            //htmlString = System.IO.File.ReadAllText(pathGa);

                            string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/wcRevenueTable.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathGa).Result;
                            }

                            // serialize object
                            var tableString = JsonConvert.SerializeObject(wcData.ProductSold);

                            htmlString = htmlString.Replace("_wcProductDataTable1_", tableString);

                            htmlString = htmlString.Replace("_wcCurrency_", "'" + wcData.Currency + "'");

                            string uniqueKey = $"{reportReplaceData.Type}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (intsubtype == (int)ReportTypes.WCSales)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/wcSales.html");
                            //htmlString = System.IO.File.ReadAllText(pathGa);

                            string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/wcSales.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathGa).Result;
                            }

                            var dateLabelStr = String.Join(",", wcData.DateLabel.Select(x => "'" + x + "'"));


                            var salesChart = String.Join(",", wcData.SalesChartData);

                            htmlString = htmlString.Replace("_1wcDateChartLabels_", dateLabelStr);


                            htmlString = htmlString.Replace("_1wcSalesChartData_", salesChart);

                            int intervalRes = wcData.DateLabel.Count <= 31 ? 3 : (wcData.DateLabel.Count <= 91 ? 7 : 31);
                            htmlString = htmlString.Replace("_wcLabelInterval_", intervalRes.ToString());


                            //Difference

                            //Sales Chart
                            htmlString = htmlString.Replace("_1wcSalesChartDiffData_", wcData.Currency + wcData.SalesChartDiff);

                            var hasPlusSign8 = wcData.SalesChartDiff.Contains("+");
                            if (hasPlusSign8)
                            {
                                htmlString = htmlString.Replace("_1wcSalesChartDiffColor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_1wcSalesChartDiffColor_", "red");
                            }

                            string uniqueKey = $"{reportReplaceData.Type}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (intsubtype == (int)ReportTypes.WCSalesTable)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/wcSalesTable.html");
                            //htmlString = System.IO.File.ReadAllText(pathGa);

                            string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/wcSalesTable.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathGa).Result;
                            }


                            // serialize object
                            var tableString = JsonConvert.SerializeObject(wcData.ProductSold);

                            htmlString = htmlString.Replace("_wcProductDataTable1_", tableString);

                            htmlString = htmlString.Replace("_wcCurrency_", "'" + wcData.Currency + "'");

                            string uniqueKey = $"{reportReplaceData.Type}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                    }
                }
            }
            else if (reportReplaceData.Type == (int)ReportTypes.CallRail)
            {
                var callRailData = JsonConvert.DeserializeObject<CallRailReportData>(reportReplaceData.RootReportData.CallRailReportData);

                if (callRailData != null || reportReplaceData.RootReportData.CallRailReportTableData != null)
                {
                    foreach (var subtype in reportReplaceData.SubType)
                    {
                        var intsubtype = Convert.ToInt16(subtype);

                        if (intsubtype == (int)ReportTypes.CallRailPie)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            //string pathPie = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/crPie.html");
                            //htmlString = System.IO.File.ReadAllText(pathPie);

                            string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/crPie.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathPie).Result;
                            }

                            List<string> colorShades = new List<string>
                            {
                               "#ffa096",
                                "#9a55ff"
                            };

                            // Create a new list by selecting elements from the original list based on the count
                            var selectedColors = string.Join(",", colorShades.Select(x => "'" + x + "'"));
                            htmlString = htmlString.Replace("_gsBgColorPieChart_", selectedColors);

                            var myData = new List<int> { callRailData.CurrentPeriodData.TotalAnswered, callRailData.CurrentPeriodData.TotalMissed };

                            var myLabels = new List<string> { "Answered " + callRailData.CurrentPeriodData.TotalAnswered + " (" + callRailData.CurrentPeriodData.TotalAnsweredRateAvg + "%)", "Missed " + callRailData.CurrentPeriodData.TotalMissed + " (" + callRailData.CurrentPeriodData.TotalMissedRateAvg + "%)" };

                            var data = string.Join(",", myData);

                            var labels = string.Join(",", myLabels.Select(x => "'" + x + "'"));

                            htmlString = htmlString.Replace("_gsPieChartData_", data);
                            htmlString = htmlString.Replace("_gsPieChartLabel_", labels);
                            htmlString = htmlString.Replace("_gsTitle_", "Answered vs Missed Calls");
                            htmlString = htmlString.Replace("_gsAggeragatorValue_", callRailData.PieChartDiff);

                            var hasPlusSign1 = callRailData.PieChartDiff.Contains("+");
                            if (hasPlusSign1)
                            {
                                htmlString = htmlString.Replace("_condiffcolor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_condiffcolor_", "red");
                            }


                            string uniqueKey = $"{62}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (intsubtype == (int)ReportTypes.CallRailTopSources)
                        {

                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            //string pathPie = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/crSources.html");
                            //htmlString = System.IO.File.ReadAllText(pathPie);

                            string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/crSources.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathPie).Result;
                            }

                            var labels = callRailData.CurrentPeriodData.SourceCounts.Select(x => x.Key).Take(5).ToList();

                            var data = callRailData.CurrentPeriodData.SourceCounts.Select(x => x.Value).Take(5).ToList();
                            var total = data.Sum();

                            var percentages = total == 0 ? new List<int>() : data.Select(x => (int)Math.Round((double)x / total * 100)).ToList();

                            var sourceData = string.Join(",", labels.Select(x => "'" + x + "'"));

                            var valueData = string.Join(",", data);

                            var percentageData = string.Join(",", percentages);

                            // Create a new list by selecting elements from the original list based on the count
                            htmlString = htmlString.Replace("_crSourceList_", sourceData);
                            htmlString = htmlString.Replace("_crValueList_", valueData);
                            htmlString = htmlString.Replace("_crPercentageList_", percentageData);


                            string uniqueKey = $"{62}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }

                        else if (intsubtype == (int)ReportTypes.CallRailAnsweredLine)
                        {

                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/crLine.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathPie).Result;
                            }

                            htmlString = htmlString.Replace("_crChartId_", "crAnswerChart");

                            var labels = string.Join(",", callRailData.CurrentPeriodData.Dates.Select(x => "'" + x + "'"));

                            var currentData = string.Join(",", callRailData.CurrentPeriodData.AnsweredList);

                            var previousData = string.Join(",", callRailData.PreviousPeriodData.AnsweredList);

                            htmlString = htmlString.Replace("_crLineData1_", currentData);
                            htmlString = htmlString.Replace("_crLineData2_", previousData);
                            htmlString = htmlString.Replace("_crLineLabels_", labels);

                            htmlString = htmlString.Replace("_crLineAggeragatorValue_", callRailData.AnsweredDiff);

                            var hasPlusSign1 = callRailData.AnsweredDiff.Contains("+");
                            if (hasPlusSign1)
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "red");
                            }

                            htmlString = htmlString.Replace("_crLineChartTitle_", "Answered");

                            bool isDailyFormat = callRailData.CurrentPeriodData.Dates.FirstOrDefault()?.Length == 6;
                            if (isDailyFormat)
                            {
                                int intervalRes = callRailData.CurrentPeriodData.Dates.Count <= 31 ? 3 : (callRailData.CurrentPeriodData.Dates.Count <= 91 ? 7 : 31);
                                htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_labelInterval_", "1");
                            }

                            string uniqueKey = $"{62}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (intsubtype == (int)ReportTypes.CallRailCallLine)
                        {

                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/crLine.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathPie).Result;
                            }

                            htmlString = htmlString.Replace("_crChartId_", "crCallChart");

                            var labels = string.Join(",", callRailData.CurrentPeriodData.Dates.Select(x => "'" + x + "'"));

                            var currentData = string.Join(",", callRailData.CurrentPeriodData.CallsList);

                            var previousData = string.Join(",", callRailData.PreviousPeriodData.CallsList);

                            htmlString = htmlString.Replace("_crLineData1_", currentData);
                            htmlString = htmlString.Replace("_crLineData2_", previousData);
                            htmlString = htmlString.Replace("_crLineLabels_", labels);

                            htmlString = htmlString.Replace("_crLineAggeragatorValue_", callRailData.CallsDiff);

                            var hasPlusSign1 = callRailData.CallsDiff.Contains("+");
                            if (hasPlusSign1)
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "red");
                            }

                            htmlString = htmlString.Replace("_crLineChartTitle_", "Calls");

                            bool isDailyFormat = callRailData.CurrentPeriodData.Dates.FirstOrDefault()?.Length == 6;
                            if (isDailyFormat)
                            {
                                int intervalRes = callRailData.CurrentPeriodData.Dates.Count <= 31 ? 3 : (callRailData.CurrentPeriodData.Dates.Count <= 91 ? 7 : 31);
                                htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_labelInterval_", "1");
                            }

                            string uniqueKey = $"{62}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (intsubtype == (int)ReportTypes.CallRailMissedLine)
                        {

                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/crLine.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathPie).Result;
                            }

                            htmlString = htmlString.Replace("_crChartId_", "crMissedChart");

                            var labels = string.Join(",", callRailData.CurrentPeriodData.Dates.Select(x => "'" + x + "'"));

                            var currentData = string.Join(",", callRailData.CurrentPeriodData.MissedCallList);

                            var previousData = string.Join(",", callRailData.PreviousPeriodData.MissedCallList);

                            htmlString = htmlString.Replace("_crLineData1_", currentData);
                            htmlString = htmlString.Replace("_crLineData2_", previousData);
                            htmlString = htmlString.Replace("_crLineLabels_", labels);

                            htmlString = htmlString.Replace("_crLineAggeragatorValue_", callRailData.MissedDiff);

                            var hasPlusSign1 = callRailData.MissedDiff.Contains("+");
                            if (hasPlusSign1)
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "red");
                            }

                            htmlString = htmlString.Replace("_crLineChartTitle_", "Missed");

                            bool isDailyFormat = callRailData.CurrentPeriodData.Dates.FirstOrDefault()?.Length == 6;
                            if (isDailyFormat)
                            {
                                int intervalRes = callRailData.CurrentPeriodData.Dates.Count <= 31 ? 3 : (callRailData.CurrentPeriodData.Dates.Count <= 91 ? 7 : 31);
                                htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_labelInterval_", "1");
                            }

                            string uniqueKey = $"{62}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (intsubtype == (int)ReportTypes.CallRailFirstTimeCallLine)
                        {

                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/crLine.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathPie).Result;
                            }

                            htmlString = htmlString.Replace("_crChartId_", "crFTimeChart");

                            var labels = string.Join(",", callRailData.CurrentPeriodData.Dates.Select(x => "'" + x + "'"));

                            var currentData = string.Join(",", callRailData.CurrentPeriodData.FirstTimeList);

                            var previousData = string.Join(",", callRailData.PreviousPeriodData.FirstTimeList);

                            htmlString = htmlString.Replace("_crLineData1_", currentData);
                            htmlString = htmlString.Replace("_crLineData2_", previousData);
                            htmlString = htmlString.Replace("_crLineLabels_", labels);

                            htmlString = htmlString.Replace("_crLineAggeragatorValue_", callRailData.FirstTimeDiff);

                            var hasPlusSign1 = callRailData.FirstTimeDiff.Contains("+");
                            if (hasPlusSign1)
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "red");
                            }

                            htmlString = htmlString.Replace("_crLineChartTitle_", "First Time Calls");

                            bool isDailyFormat = callRailData.CurrentPeriodData.Dates.FirstOrDefault()?.Length == 6;
                            if (isDailyFormat)
                            {
                                int intervalRes = callRailData.CurrentPeriodData.Dates.Count <= 31 ? 3 : (callRailData.CurrentPeriodData.Dates.Count <= 91 ? 7 : 31);
                                htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_labelInterval_", "1");
                            }

                            string uniqueKey = $"{62}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (intsubtype == (int)ReportTypes.CallRailLeadLine)
                        {

                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/crLine.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathPie).Result;
                            }

                            htmlString = htmlString.Replace("_crChartId_", "crLeadChart");

                            var labels = string.Join(",", callRailData.CurrentPeriodData.Dates.Select(x => "'" + x + "'"));

                            var currentData = string.Join(",", callRailData.CurrentPeriodData.LeadsList);

                            var previousData = string.Join(",", callRailData.PreviousPeriodData.LeadsList);

                            htmlString = htmlString.Replace("_crLineData1_", currentData);
                            htmlString = htmlString.Replace("_crLineData2_", previousData);
                            htmlString = htmlString.Replace("_crLineLabels_", labels);

                            htmlString = htmlString.Replace("_crLineAggeragatorValue_", callRailData.LeadsDiff);

                            var hasPlusSign1 = callRailData.LeadsDiff.Contains("+");
                            if (hasPlusSign1)
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "red");
                            }

                            htmlString = htmlString.Replace("_crLineChartTitle_", "Leads");

                            bool isDailyFormat = callRailData.CurrentPeriodData.Dates.FirstOrDefault()?.Length == 6;
                            if (isDailyFormat)
                            {
                                int intervalRes = callRailData.CurrentPeriodData.Dates.Count <= 31 ? 3 : (callRailData.CurrentPeriodData.Dates.Count <= 91 ? 7 : 31);
                                htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_labelInterval_", "1");
                            }

                            string uniqueKey = $"{62}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }

                        else if (intsubtype == (int)ReportTypes.CallRailAvgDurationLine)
                        {

                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/dudrationChart.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathPie).Result;
                            }

                            var labels = string.Join(",", callRailData.CurrentPeriodData.Dates.Select(x => "'" + x + "'"));

                            var currentData = string.Join(",", callRailData.CurrentPeriodData.DurationListInt);

                            var previousData = string.Join(",", callRailData.PreviousPeriodData.DurationListInt);

                            htmlString = htmlString.Replace("_crLineData1_", currentData);
                            htmlString = htmlString.Replace("_crLineData2_", previousData);
                            htmlString = htmlString.Replace("_crLineLabels_", labels);

                            htmlString = htmlString.Replace("_crLineAggeragatorValue_", callRailData.AvgDurationDiff);

                            var hasPlusSign1 = callRailData.AvgDurationDiff.Contains("+");
                            if (hasPlusSign1)
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "red");
                            }

                            htmlString = htmlString.Replace("_crLineChartTitle_", "Avg Duration");

                            bool isDailyFormat = callRailData.CurrentPeriodData.Dates.FirstOrDefault()?.Length == 6;
                            if (isDailyFormat)
                            {
                                int intervalRes = callRailData.CurrentPeriodData.Dates.Count <= 31 ? 3 : (callRailData.CurrentPeriodData.Dates.Count <= 91 ? 7 : 31);
                                htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_labelInterval_", "1");
                            }

                            string uniqueKey = $"{62}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (intsubtype == (int)ReportTypes.CallRailAvgAnswerRateLine)
                        {

                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/crLine.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathPie).Result;
                            }

                            htmlString = htmlString.Replace("_crChartId_", "crAvgAnsRateChart");

                            var labels = string.Join(",", callRailData.CurrentPeriodData.Dates.Select(x => "'" + x + "'"));

                            var currentData = string.Join(",", callRailData.CurrentPeriodData.AvgAnswerRateList);

                            var previousData = string.Join(",", callRailData.PreviousPeriodData.AvgAnswerRateList);

                            htmlString = htmlString.Replace("_crLineData1_", currentData);
                            htmlString = htmlString.Replace("_crLineData2_", previousData);
                            htmlString = htmlString.Replace("_crLineLabels_", labels);

                            htmlString = htmlString.Replace("_crLineAggeragatorValue_", callRailData.AnsweredRateAvgDiff);

                            var hasPlusSign1 = callRailData.AnsweredRateAvgDiff.Contains("+");
                            if (hasPlusSign1)
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "red");
                            }

                            htmlString = htmlString.Replace("_crLineChartTitle_", "Avg Answer Rate");

                            bool isDailyFormat = callRailData.CurrentPeriodData.Dates.FirstOrDefault()?.Length == 6;
                            if (isDailyFormat)
                            {
                                int intervalRes = callRailData.CurrentPeriodData.Dates.Count <= 31 ? 3 : (callRailData.CurrentPeriodData.Dates.Count <= 91 ? 7 : 31);
                                htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_labelInterval_", "1");
                            }

                            string uniqueKey = $"{62}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (intsubtype == (int)ReportTypes.CallRailAvgMissedRateLine)
                        {

                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/crLine.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathPie).Result;
                            }

                            htmlString = htmlString.Replace("_crChartId_", "crAvgMissedRateChart");

                            var labels = string.Join(",", callRailData.CurrentPeriodData.Dates.Select(x => "'" + x + "'"));

                            var currentData = string.Join(",", callRailData.CurrentPeriodData.AvgMissedRateList);

                            var previousData = string.Join(",", callRailData.PreviousPeriodData.AvgMissedRateList);

                            htmlString = htmlString.Replace("_crLineData1_", currentData);
                            htmlString = htmlString.Replace("_crLineData2_", previousData);
                            htmlString = htmlString.Replace("_crLineLabels_", labels);

                            htmlString = htmlString.Replace("_crLineAggeragatorValue_", callRailData.MissedRateAvgDiff);

                            var hasPlusSign1 = callRailData.MissedRateAvgDiff.Contains("+");
                            if (hasPlusSign1)
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "red");
                            }

                            htmlString = htmlString.Replace("_crLineChartTitle_", "Avg Missed Rate");

                            bool isDailyFormat = callRailData.CurrentPeriodData.Dates.FirstOrDefault()?.Length == 6;
                            if (isDailyFormat)
                            {
                                int intervalRes = callRailData.CurrentPeriodData.Dates.Count <= 31 ? 3 : (callRailData.CurrentPeriodData.Dates.Count <= 91 ? 7 : 31);
                                htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_labelInterval_", "1");
                            }

                            string uniqueKey = $"{62}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (intsubtype == (int)ReportTypes.CallRailAvgFirstTimeRateLine)
                        {

                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/crLine.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathPie).Result;
                            }

                            htmlString = htmlString.Replace("_crChartId_", "crAvgFCallRateChart");

                            var labels = string.Join(",", callRailData.CurrentPeriodData.Dates.Select(x => "'" + x + "'"));

                            var currentData = string.Join(",", callRailData.CurrentPeriodData.AvgFirstTimeCallRateList);

                            var previousData = string.Join(",", callRailData.PreviousPeriodData.AvgFirstTimeCallRateList);

                            htmlString = htmlString.Replace("_crLineData1_", currentData);
                            htmlString = htmlString.Replace("_crLineData2_", previousData);
                            htmlString = htmlString.Replace("_crLineLabels_", labels);

                            htmlString = htmlString.Replace("_crLineAggeragatorValue_", callRailData.FirstRateAvgDiff);

                            var hasPlusSign1 = callRailData.FirstRateAvgDiff.Contains("+");
                            if (hasPlusSign1)
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "red");
                            }

                            htmlString = htmlString.Replace("_crLineChartTitle_", "Avg First Time Call Rate");

                            bool isDailyFormat = callRailData.CurrentPeriodData.Dates.FirstOrDefault()?.Length == 6;
                            if (isDailyFormat)
                            {
                                int intervalRes = callRailData.CurrentPeriodData.Dates.Count <= 31 ? 3 : (callRailData.CurrentPeriodData.Dates.Count <= 91 ? 7 : 31);
                                htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_labelInterval_", "1");
                            }

                            string uniqueKey = $"{62}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (intsubtype == (int)ReportTypes.CallRailAvgCallPerLead)
                        {

                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/crLine.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathPie).Result;
                            }

                            htmlString = htmlString.Replace("_crChartId_", "crAvgCallPerLeadChart");

                            var labels = string.Join(",", callRailData.CurrentPeriodData.Dates.Select(x => "'" + x + "'"));

                            var currentData = string.Join(",", callRailData.CurrentPeriodData.AvgCallsPerLeadList);

                            var previousData = string.Join(",", callRailData.PreviousPeriodData.AvgCallsPerLeadList);

                            htmlString = htmlString.Replace("_crLineData1_", currentData);
                            htmlString = htmlString.Replace("_crLineData2_", previousData);
                            htmlString = htmlString.Replace("_crLineLabels_", labels);

                            htmlString = htmlString.Replace("_crLineAggeragatorValue_", callRailData.AvgCallPerLeadsDiff);

                            var hasPlusSign1 = callRailData.AvgCallPerLeadsDiff.Contains("+");
                            if (hasPlusSign1)
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "red");
                            }

                            htmlString = htmlString.Replace("_crLineChartTitle_", "Avg Calls Per Lead");

                            bool isDailyFormat = callRailData.CurrentPeriodData.Dates.FirstOrDefault()?.Length == 6;
                            if (isDailyFormat)
                            {
                                int intervalRes = callRailData.CurrentPeriodData.Dates.Count <= 31 ? 3 : (callRailData.CurrentPeriodData.Dates.Count <= 91 ? 7 : 31);
                                htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_labelInterval_", "1");
                            }

                            string uniqueKey = $"{62}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (intsubtype == (int)ReportTypes.CallRailTable)
                        {
                            var tbData = JsonConvert.DeserializeObject<List<Call>>(reportReplaceData.RootReportData.CallRailReportTableData);

                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/crTable.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathPie).Result;
                            }

                            // serialize object
                            var tableString = JsonConvert.SerializeObject(tbData);

                            htmlString = htmlString.Replace("_crDataTable1_", tableString);

                            string uniqueKey = $"{62}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                    }
                }
            }
            else if (reportReplaceData.Type == (int)ReportTypes.Mailchimp)
            {
                var mailchimpData = JsonConvert.DeserializeObject<MailchimpPreviewData>(reportReplaceData.RootReportData.MailchimpPreviewData);

                if (mailchimpData != null || reportReplaceData.RootReportData.MailchimpPreviewData != null)
                {
                    int noOfSingleCampaign = 0;
                    int noOfSingleList = 0;

                    foreach (var subtype in reportReplaceData.SubType)
                    {
                        //put extra logic for mailchimp get dynamic id  // 76(82[8af9ff7617]) key: 82 Value: 8af9ff7617
                        var subtypeAndMcValue = ExtractNumber(subtype);

                        if (subtypeAndMcValue.Key == (int)ReportTypes.McCampaignsRecipients)
                        {

                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/campaignsRecipients.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathPie).Result;
                            }

                            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/campaignsRecipients.html");
                            //htmlString = System.IO.File.ReadAllText(pathGa);

                            htmlString = htmlString.Replace("_mcChartId_", "mcRecipientsChart");

                            var labels = string.Join(",", mailchimpData.McRootCampaignList.recipientsChartDates.Select(x => "'" + x + "'"));

                            var currentData = string.Join(",", mailchimpData.McRootCampaignList.recipientsChartValues);

                            htmlString = htmlString.Replace("_mcLineData1_", currentData);

                            htmlString = htmlString.Replace("_mcLineLabels_", labels);

                            htmlString = htmlString.Replace("_mcLineChartTotal_", mailchimpData.McRootCampaignList.recipientsChartTotal.ToString());

                            htmlString = htmlString.Replace("_mcLineChartTitle_", "Recipients (24-Hour Period)");

                            string uniqueKey = $"{76}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (subtypeAndMcValue.Key == (int)ReportTypes.McCampaignsOpens)
                        {

                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/campaignsOpens.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathPie).Result;
                            }

                            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/campaignsOpens.html");
                            //htmlString = System.IO.File.ReadAllText(pathGa);


                            htmlString = htmlString.Replace("_mcChartId_", "mcOpensChart");

                            var labels = string.Join(",", mailchimpData.McRootCampaignList.openChartDates.Select(x => "'" + x + "'"));

                            var currentData = string.Join(",", mailchimpData.McRootCampaignList.openChartValues);

                            htmlString = htmlString.Replace("_mcOpenData1_", currentData);

                            htmlString = htmlString.Replace("_mcOpenLabels_", labels);


                            htmlString = htmlString.Replace("_mcLineOpenTotal_", mailchimpData.McRootCampaignList.uniqueOpenChartTotal.ToString());

                            htmlString = htmlString.Replace("_mcOpenChartTitle_", "Opens (24-Hour Period)");

                            string uniqueKey = $"{76}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (subtypeAndMcValue.Key == (int)ReportTypes.McCampaignsClicks)
                        {

                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/campaignsClicks.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathPie).Result;
                            }

                            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/campaignsClicks.html");
                            //htmlString = System.IO.File.ReadAllText(pathGa);

                            htmlString = htmlString.Replace("_mcChartId_", "mcClickChart");

                            var labels = string.Join(",", mailchimpData.McRootCampaignList.clickChartDates.Select(x => "'" + x + "'"));

                            var currentData = string.Join(",", mailchimpData.McRootCampaignList.clickChartValues);

                            htmlString = htmlString.Replace("_mcClickData1_", currentData);

                            htmlString = htmlString.Replace("_mcClickLabels_", labels);

                            htmlString = htmlString.Replace("_mcClickChartTitle_", "Clicks (24-Hour Period)");

                            htmlString = htmlString.Replace("_mcLineClickTotal_", mailchimpData.McRootCampaignList.clickChartTotal.ToString());

                            string uniqueKey = $"{76}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (subtypeAndMcValue.Key == (int)ReportTypes.McCampaignsTiles && mailchimpData.McRootCampaignList.mcCampaignsSetting != null)
                        {

                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/campaignsTiles.html");
                            //htmlString = System.IO.File.ReadAllText(pathGa);


                            string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/campaignsTiles.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathPie).Result;
                            }

                            var listTilesData = new List<McTilesData>();

                            JObject jObject = JObject.Parse(mailchimpData.McRootCampaignList.mcCampaignsSettingString);

                            // Get properties for McCampaignsSetting
                            //JToken mcCampaignsSettingToken = jObject.SelectToken("mcCampaignsSetting");
                            //var orderedProperties = mcCampaignsSettingToken?.Children<JProperty>().ToList();

                            // Get the properties in the order specified in mailchimpSettings
                            var orderedProperties = jObject.Properties();
                            var orderedListTilesData = new List<McTilesData>();
                            var dataProperties = typeof(MCRootCampaignList).GetProperties();

                            foreach (var orderedProperty in orderedProperties)
                            {
                                var settingProperty = mailchimpData.McRootCampaignList.mcCampaignsSetting.GetType().GetProperty(orderedProperty.Name.Trim());
                                var dataProperty = dataProperties.FirstOrDefault(p => p.Name == orderedProperty.Name.Trim());

                                if (settingProperty != null && dataProperty != null && (bool)settingProperty.GetValue(mailchimpData.McRootCampaignList.mcCampaignsSetting))
                                {
                                    var singleTiles = new McTilesData
                                    {
                                        Display = "block",
                                        Status = (bool)settingProperty.GetValue(mailchimpData.McRootCampaignList.mcCampaignsSetting),
                                        Name = AddSpacesToCamelCase(settingProperty.Name),
                                        Value = AddPercentageIfRate((decimal)dataProperty.GetValue(mailchimpData.McRootCampaignList), settingProperty.Name)
                                    };

                                    orderedListTilesData.Add(singleTiles);
                                }
                            }

                            // Create a StringBuilder to store the generated HTML
                            var htmlBuilder = new StringBuilder();

                            // Start the HTML structure
                            htmlBuilder.AppendLine("<div class=\"row a4-page mt-3 d-flex justify-content-center gap-5 flex-wrap\">");

                            // Iterate over each McTilesData in the list
                            for (int i = 0; i < orderedListTilesData.Count; i++)
                            {
                                var tile = orderedListTilesData[i];

                                // Generate HTML for each tile
                                htmlBuilder.AppendLine($"<div class=\"width-18\" style=\"display: {tile.Display}\">");
                                htmlBuilder.AppendLine("    <div class=\"card\">");
                                htmlBuilder.AppendLine("        <div class=\"card-bg-1 card-body text-center card-border-radius\" id=\"clicks\">");
                                htmlBuilder.AppendLine($"            <h5 class=\"card-title card-title-color\">{tile.Name}</h5>");
                                htmlBuilder.AppendLine($"            <h1 class=\"card-text card-title-color fontxxlrge\">{tile.Value}</h1>");
                                htmlBuilder.AppendLine("        </div>");
                                htmlBuilder.AppendLine("    </div>");
                                htmlBuilder.AppendLine("</div>");
                            }

                            // End the HTML structure
                            htmlBuilder.AppendLine("</div>");

                            // Get the final HTML string
                            string generatedHtml = htmlBuilder.ToString();

                            // Now you can use the generatedHtml string where needed in your application


                            // Create a new list by selecting elements from the original list based on the count
                            htmlString = htmlString.Replace("_myMcTilesData_", generatedHtml);

                            string uniqueKey = $"{76}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (subtypeAndMcValue.Key == (int)ReportTypes.McCampaignsTable)
                        {

                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/campaignsTable.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathPie).Result;
                            }

                            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/campaignsTable.html");
                            //htmlString = System.IO.File.ReadAllText(pathGa);


                            // serialize object
                            var tableString = JsonConvert.SerializeObject(mailchimpData.McRootCampaignList.campaignListTable);

                            htmlString = htmlString.Replace("_mcCampaignsTable_", tableString);

                            string uniqueKey = $"{76}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (subtypeAndMcValue.Key == (int)ReportTypes.McListsAudianceGrowth)
                        {

                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/mcListGrowth.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathPie).Result;
                            }

                            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/mcListGrowth.html");
                            //htmlString = System.IO.File.ReadAllText(pathGa);


                            htmlString = htmlString.Replace("_mcChartId_", "mcListGrowthChart");

                            var labels = string.Join(",", mailchimpData.McRootList.audianceGrowthChartDates.Select(x => "'" + x + "'"));

                            var currentData = string.Join(",", mailchimpData.McRootList.audianceGrowthChartValues);

                            htmlString = htmlString.Replace("_mcGrowthData1_", currentData);

                            htmlString = htmlString.Replace("_mcGrowthLabels_", labels);

                            htmlString = htmlString.Replace("_mcGrowthChartTitle_", "Audience Growth");

                            htmlString = htmlString.Replace("_mcGrowthChartTotal_", mailchimpData.McRootList.growthChartTotal.ToString());

                            string uniqueKey = $"{76}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (subtypeAndMcValue.Key == (int)ReportTypes.McListsOpens)
                        {

                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/mcListOpens.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathPie).Result;
                            }

                            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/mcListOpens.html");
                            //htmlString = System.IO.File.ReadAllText(pathGa);


                            htmlString = htmlString.Replace("_mcChartId_", "mcListOpensChart");

                            var labels = string.Join(",", mailchimpData.McRootList.openChartDates.Select(x => "'" + x + "'"));

                            var currentData = string.Join(",", mailchimpData.McRootList.openChartValues);

                            htmlString = htmlString.Replace("_mcListOpenData1_", currentData);

                            htmlString = htmlString.Replace("_mcListOpenLabels_", labels);

                            htmlString = htmlString.Replace("_mcListOpenChartTotal_", mailchimpData.McRootList.opensChartTotal.ToString());

                            htmlString = htmlString.Replace("_mcListOpenChartTitle_", "Opens");

                            string uniqueKey = $"{76}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (subtypeAndMcValue.Key == (int)ReportTypes.McListsClicks)
                        {

                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/mcListClicks.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathPie).Result;
                            }

                            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/mcListClicks.html");
                            //htmlString = System.IO.File.ReadAllText(pathGa);


                            htmlString = htmlString.Replace("_mcChartId_", "mcListClickChart");

                            var labels = string.Join(",", mailchimpData.McRootList.openChartDates.Select(x => "'" + x + "'"));

                            var currentData = string.Join(",", mailchimpData.McRootList.clickChartValues);

                            htmlString = htmlString.Replace("_mcListClickData1_", currentData);

                            htmlString = htmlString.Replace("_mcListClickLabels_", labels);

                            htmlString = htmlString.Replace("_mcListClickChartTotal_", mailchimpData.McRootList.clickChartTotal.ToString());

                            htmlString = htmlString.Replace("_mcListClickChartTitle_", "Clicks");

                            string uniqueKey = $"{76}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (subtypeAndMcValue.Key == (int)ReportTypes.McListsTiles && mailchimpData.McRootList.mcListSetting != null)
                        {


                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/mcListTiles.html");
                            //htmlString = System.IO.File.ReadAllText(pathGa);


                            string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/mcListTiles.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathPie).Result;
                            }

                            var listTilesData = new List<McTilesData>();

                            JObject jObject = JObject.Parse(mailchimpData.McRootList.mcListSettingString);

                            // Get properties for McCampaignsSetting
                            //JToken mcListSettingToken = jObject.SelectToken("mcListSetting");
                            //var orderedProperties = mcListSettingToken?.Children<JProperty>().ToList();

                            // Get the properties in the order specified in mailchimpSettings
                            var orderedProperties = jObject.Properties();
                            var orderedListTilesData = new List<McTilesData>();
                            var dataProperties = typeof(MCRootList).GetProperties();

                            foreach (var orderedProperty in orderedProperties)
                            {
                                var settingProperty = mailchimpData.McRootList.mcListSetting.GetType().GetProperty(orderedProperty.Name);
                                var dataProperty = dataProperties.FirstOrDefault(p => p.Name == orderedProperty.Name);

                                if (settingProperty != null && dataProperty != null && (bool)settingProperty.GetValue(mailchimpData.McRootList.mcListSetting))
                                {
                                    var singleTiles = new McTilesData
                                    {
                                        Display = "block",
                                        Status = (bool)settingProperty.GetValue(mailchimpData.McRootList.mcListSetting),
                                        Name = AddSpacesToCamelCase(settingProperty.Name),
                                        Value = AddPercentageIfRate((decimal)dataProperty.GetValue(mailchimpData.McRootList), settingProperty.Name)
                                    };

                                    orderedListTilesData.Add(singleTiles);
                                }
                            }

                            // Create a StringBuilder to store the generated HTML
                            var htmlBuilder = new StringBuilder();

                            // Start the HTML structure
                            htmlBuilder.AppendLine("<div class=\"row a4-page mt-3 d-flex justify-content-center gap-5 flex-wrap\">");

                            // Iterate over each McTilesData in the list
                            for (int i = 0; i < orderedListTilesData.Count; i++)
                            {
                                var tile = orderedListTilesData[i];

                                // Generate HTML for each tile
                                htmlBuilder.AppendLine($"<div class=\"width-18\" style=\"display: {tile.Display}\">");
                                htmlBuilder.AppendLine("    <div class=\"card\">");
                                htmlBuilder.AppendLine("        <div class=\"card-bg-1 card-body text-center card-border-radius\" id=\"clicks\">");
                                htmlBuilder.AppendLine($"            <h5 class=\"card-title card-title-color\">{tile.Name}</h5>");
                                htmlBuilder.AppendLine($"            <h1 class=\"card-text card-title-color fontxxlrge\">{tile.Value}</h1>");
                                htmlBuilder.AppendLine("        </div>");
                                htmlBuilder.AppendLine("    </div>");
                                htmlBuilder.AppendLine("</div>");
                            }

                            // End the HTML structure
                            htmlBuilder.AppendLine("</div>");

                            // Get the final HTML string
                            string generatedHtml = htmlBuilder.ToString();

                            // Now you can use the generatedHtml string where needed in your application


                            // Create a new list by selecting elements from the original list based on the count
                            htmlString = htmlString.Replace("_myMcListTilesData_", generatedHtml);

                            string uniqueKey = $"{76}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (subtypeAndMcValue.Key == (int)ReportTypes.McListsTable)
                        {

                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/mcListTables.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathPie).Result;
                            }

                            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/mcListTables.html");
                            //htmlString = System.IO.File.ReadAllText(pathGa);


                            // serialize object
                            var tableString = JsonConvert.SerializeObject(mailchimpData.McRootList.listTable);

                            htmlString = htmlString.Replace("_mcListTable_", tableString);

                            string uniqueKey = $"{76}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (subtypeAndMcValue.Key == (int)ReportTypes.McSingleCampaign)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            if (mailchimpData.SingleCampaignReport != null && mailchimpData.SingleCampaignReport.Count() > 0)
                            {

                                string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/singleMcCampaign.html");
                                using (HttpClient httpclient = new HttpClient())
                                {
                                    htmlString = httpclient.GetStringAsync(pathPie).Result;
                                }

                                //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/singleMcCampaign.html");
                                //htmlString = System.IO.File.ReadAllText(pathGa);

                                //Bind Tiles

                                htmlString = htmlString.Replace("_singleMcName_", mailchimpData.SingleCampaignReport[noOfSingleCampaign].Name.ToString());

                                htmlString = htmlString.Replace("_mcsOpenRate_", mailchimpData.SingleCampaignReport[noOfSingleCampaign].OpenRate.ToString());

                                htmlString = htmlString.Replace("_mcsClickRate_", mailchimpData.SingleCampaignReport[noOfSingleCampaign].ClickRate.ToString());

                                htmlString = htmlString.Replace("_mcsUnsubscribeRate_", mailchimpData.SingleCampaignReport[noOfSingleCampaign].UnsubscribeRate.ToString());

                                htmlString = htmlString.Replace("_mcsBounceRate_", mailchimpData.SingleCampaignReport[noOfSingleCampaign].BounceRate.ToString());

                                htmlString = htmlString.Replace("_mcsClicks_", mailchimpData.SingleCampaignReport[noOfSingleCampaign].Click.ToString());

                                htmlString = htmlString.Replace("_mcsSubscriberClicks_", mailchimpData.SingleCampaignReport[noOfSingleCampaign].SubsciberClick.ToString());

                                htmlString = htmlString.Replace("_mcsUnsubscribes_", mailchimpData.SingleCampaignReport[noOfSingleCampaign].Unsubscribes.ToString());

                                htmlString = htmlString.Replace("_mcsOpens_", mailchimpData.SingleCampaignReport[noOfSingleCampaign].Opens.ToString());

                                htmlString = htmlString.Replace("_mcsOrders_", mailchimpData.SingleCampaignReport[noOfSingleCampaign].Orders.ToString());

                                htmlString = htmlString.Replace("_mcsAverageOrder_", mailchimpData.SingleCampaignReport[noOfSingleCampaign].AverageOrder.ToString());

                                htmlString = htmlString.Replace("_mcsTotalRevenue_", mailchimpData.SingleCampaignReport[noOfSingleCampaign].Revenue.ToString());

                                htmlString = htmlString.Replace("_mcsTotalSpent_", mailchimpData.SingleCampaignReport[noOfSingleCampaign].TotalSpent.ToString());

                                htmlString = htmlString.Replace("_mcsDeliveries_", mailchimpData.SingleCampaignReport[noOfSingleCampaign].Deliveries.ToString());

                                htmlString = htmlString.Replace("_mcsDeliveryRate_", mailchimpData.SingleCampaignReport[noOfSingleCampaign].DeliveryRate.ToString());

                                htmlString = htmlString.Replace("_mcsSpams_", mailchimpData.SingleCampaignReport[noOfSingleCampaign].Spams.ToString());

                                htmlString = htmlString.Replace("_mcsSpamRate_", mailchimpData.SingleCampaignReport[noOfSingleCampaign].SpamRate.ToString());


                                //Open and click chart

                                var labels = string.Join(",", mailchimpData.SingleCampaignReport[noOfSingleCampaign].OpenChartDates.Select(x => "'" + x + "'"));

                                var currentData = string.Join(",", mailchimpData.SingleCampaignReport[noOfSingleCampaign].OpenChartValues);

                                htmlString = htmlString.Replace("_mcsOpenChart_", "mcsOpen" + DateTime.UtcNow.Ticks.ToString());

                                htmlString = htmlString.Replace("_mcOpenData1_", currentData);

                                htmlString = htmlString.Replace("_mcOpenLabels_", labels);

                                htmlString = htmlString.Replace("_mcsOpenTotal_", mailchimpData.SingleCampaignReport[noOfSingleCampaign].UniqueOpenChartTotal.ToString());


                                var labels2 = string.Join(",", mailchimpData.SingleCampaignReport[noOfSingleCampaign].ClickChartDates.Select(x => "'" + x + "'"));

                                var currentData2 = string.Join(",", mailchimpData.SingleCampaignReport[noOfSingleCampaign].ClickChartValues);

                                htmlString = htmlString.Replace("_mcsClickChart_", "mcsClick" + DateTime.UtcNow.Ticks.ToString());

                                htmlString = htmlString.Replace("_mcsClickData1_", currentData2);

                                htmlString = htmlString.Replace("_mcsClickLabels_", labels2);

                                htmlString = htmlString.Replace("_mcsClicksTotal_", mailchimpData.SingleCampaignReport[noOfSingleCampaign].ClickChartTotal.ToString());


                                //pie chart

                                // Combine labels and data values
                                var combinedData = CombineLabelsAndValues(mailchimpData.SingleCampaignReport[noOfSingleCampaign].PieLabels, mailchimpData.SingleCampaignReport[noOfSingleCampaign].PieValues);

                                var data = string.Join(",", mailchimpData.SingleCampaignReport[noOfSingleCampaign].PieValues != null ? mailchimpData.SingleCampaignReport[noOfSingleCampaign].PieValues : new List<decimal>() { });

                                htmlString = htmlString.Replace("_mcsPieChart_", "mcsPie" + DateTime.UtcNow.Ticks.ToString());

                                htmlString = htmlString.Replace("_mcsPieChartData_", data);
                                htmlString = htmlString.Replace("_mcsPieChartLabel_", combinedData);

                                //location chart

                                var labels4 = string.Join(",", mailchimpData.SingleCampaignReport[noOfSingleCampaign].LocationLabels.Select(x => "'" + x + "'"));

                                var currentData4 = string.Join(",", mailchimpData.SingleCampaignReport[noOfSingleCampaign].LocationValues);

                                htmlString = htmlString.Replace("_mcsLocationChart_", "mcsLocation" + DateTime.UtcNow.Ticks.ToString());

                                htmlString = htmlString.Replace("_mcsLocationData1_", currentData4);

                                htmlString = htmlString.Replace("_mcsLocationLabels_", labels4);

                                htmlString = htmlString.Replace("_mcsLocationTotal_", mailchimpData.SingleCampaignReport[noOfSingleCampaign].LocationChartTotal.ToString());

                                //progess bar

                                if (mailchimpData.SingleCampaignReport[noOfSingleCampaign].TopUrlsClick != null)
                                {
                                    var label5 = mailchimpData.SingleCampaignReport[noOfSingleCampaign].TopUrlsClick.Select(x => x.url).ToList();

                                    var data5 = mailchimpData.SingleCampaignReport[noOfSingleCampaign].TopUrlsClick.Select(x => x.total_clicks).ToList();

                                    var totalNumberOfClicks = mailchimpData.SingleCampaignReport[noOfSingleCampaign].TopUrlsClick.Select(x => x.total_clicks).Sum();

                                    var percentages = totalNumberOfClicks == 0 ? new List<int>() : data5.Select(x => (int)Math.Round((double)x / totalNumberOfClicks * 100)).ToList();

                                    var sourceData = string.Join(",", label5.Select(x => "'" + x + "'"));

                                    var valueData = string.Join(",", data5);

                                    var percentageData = string.Join(",", percentages);

                                    htmlString = htmlString.Replace("_progressBarsContainerForMcId_", "mcsProgress" + DateTime.UtcNow.Ticks.ToString());

                                    // Create a new list by selecting elements from the original list based on the count
                                    htmlString = htmlString.Replace("_mcgSourceList_", sourceData);
                                    htmlString = htmlString.Replace("_mcgValueList_", valueData);
                                    htmlString = htmlString.Replace("_mcgPercentageList_", percentageData);
                                }

                                //table

                                // serialize object
                                var tableString = JsonConvert.SerializeObject(mailchimpData.SingleCampaignReport[noOfSingleCampaign].CampaignTableResponse);

                                htmlString = htmlString.Replace("_mcsCampaignTableId_", "mcsTable" + DateTime.UtcNow.Ticks.ToString());

                                htmlString = htmlString.Replace("_mcsCampaignTable_", tableString);

                                htmlString = htmlString.Replace("_mcLineChartTitle_", "Recipients (24-Hour Period)");

                                string uniqueKey = $"{76}({string.Join(",", subtype)})";
                                uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                                listOfResult.Add(uniqueTypeSubtypeResults);
                            }

                            noOfSingleCampaign++;
                        }
                        else if (subtypeAndMcValue.Key == (int)ReportTypes.McSingleList)
                        {
                            var singleListReport = new RootSingleList();
                            var listTable = new MailChimpMemberRoot();

                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            if (mailchimpData.RootSingleList != null && mailchimpData.RootSingleList.Count() > 0)
                            {

                                string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/singleMcList.html");
                                using (HttpClient httpclient = new HttpClient())
                                {
                                    htmlString = httpclient.GetStringAsync(pathPie).Result;
                                }

                                //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/singleMcList.html");
                                //htmlString = System.IO.File.ReadAllText(pathGa);

                                //Bind Tiles
                                htmlString = htmlString.Replace("_singleMcListName_", mailchimpData.RootSingleList[noOfSingleList].Name.ToString());

                                htmlString = htmlString.Replace("_mcsListSubscribers_", mailchimpData.RootSingleList[noOfSingleList].Subscribers.ToString());

                                htmlString = htmlString.Replace("_mcsListOpenRate_", mailchimpData.RootSingleList[noOfSingleList].OpenRate.ToString());

                                htmlString = htmlString.Replace("_mcsListClickRate_", mailchimpData.RootSingleList[noOfSingleList].ClickRate.ToString());

                                htmlString = htmlString.Replace("_mcsListCampaigns_", mailchimpData.RootSingleList[noOfSingleList].Campaigns.ToString());

                                htmlString = htmlString.Replace("_mcsListUnsubscribes_", mailchimpData.RootSingleList[noOfSingleList].Unsubscribes.ToString());

                                htmlString = htmlString.Replace("_mcsListAsr_", mailchimpData.RootSingleList[noOfSingleList].AvgSubscribeRate.ToString());

                                htmlString = htmlString.Replace("_mcsListUasr_", mailchimpData.RootSingleList[noOfSingleList].AvguNSubscribeRate.ToString());


                                //Open and click chart

                                var labels = string.Join(",", mailchimpData.RootSingleList[noOfSingleList].ChartDates.Select(x => "'" + x + "'"));

                                var currentData = string.Join(",", mailchimpData.RootSingleList[noOfSingleList].OpensChartValues);

                                htmlString = htmlString.Replace("_mcsListOpenChart_", "mcsListOpenChart" + mailchimpData.RootSingleList[noOfSingleList]);

                                htmlString = htmlString.Replace("_mcListOpenData1_", currentData);

                                htmlString = htmlString.Replace("_mcListOpenLabels_", labels);

                                htmlString = htmlString.Replace("_mcsListOpenTotal_", mailchimpData.RootSingleList[noOfSingleList].OpensChartTotal.ToString());


                                var labels2 = string.Join(",", mailchimpData.RootSingleList[noOfSingleList].ChartDates.Select(x => "'" + x + "'"));

                                var currentData2 = string.Join(",", mailchimpData.RootSingleList[noOfSingleList].ClicksChartValues);

                                htmlString = htmlString.Replace("_mcsListClickChart_", "mcsListClickChart" + DateTime.UtcNow.Ticks.ToString());

                                htmlString = htmlString.Replace("_mcsListClickData1_", currentData2);

                                htmlString = htmlString.Replace("_mcsListClickLabels_", labels2);

                                htmlString = htmlString.Replace("_mcsListClicksTotal_", mailchimpData.RootSingleList[noOfSingleList].ClickChartTotal.ToString());


                                //audience growth chart

                                var labels4 = string.Join(",", mailchimpData.RootSingleList[noOfSingleList].AudianceGrowthChartDates.Select(x => "'" + x + "'"));

                                var currentData4 = string.Join(",", mailchimpData.RootSingleList[noOfSingleList].AudianceGrowthChartValues);

                                htmlString = htmlString.Replace("_mcsListAudGrowthChart_", "mcsListAudGrowthChart" + DateTime.UtcNow.Ticks.ToString());

                                htmlString = htmlString.Replace("_mcsListAudienceData1_", currentData4);

                                htmlString = htmlString.Replace("_mcsListAudienceLabels_", labels4);


                                //top email clients

                                var labels5 = string.Join(",", mailchimpData.RootSingleList[noOfSingleList].Clients.Select(x => "'" + x + "'"));

                                var currentData5 = string.Join(",", mailchimpData.RootSingleList[noOfSingleList].Members);

                                htmlString = htmlString.Replace("_mcsListTopEmailChart_", "mcsListTopEmailChart" + DateTime.UtcNow.Ticks.ToString());

                                htmlString = htmlString.Replace("_mcsListEmailData1_", currentData5);

                                htmlString = htmlString.Replace("_mcsListEmailLabels_", labels5);

                                //table

                                // serialize object
                                var tableString = JsonConvert.SerializeObject(mailchimpData.RootSingleList[noOfSingleList].MailChimpMembers);

                                htmlString = htmlString.Replace("_mcsListTableId_", "mcsListTableId" + DateTime.UtcNow.Ticks.ToString());

                                htmlString = htmlString.Replace("_mcsListTable_", tableString);

                                string uniqueKey = $"{76}({string.Join(",", subtype)})";
                                uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                                listOfResult.Add(uniqueTypeSubtypeResults);
                            }

                            noOfSingleList++;
                        }
                    }
                }
            }
            else if (reportReplaceData.Type == (int)ReportTypes.MicrosoftAds)
            {
                var msAdsData = JsonConvert.DeserializeObject<MsAdsPreviewData>(reportReplaceData.RootReportData.MsAdsPreviewData);

                if (msAdsData != null || reportReplaceData.RootReportData.MailchimpPreviewData != null)
                {
                    int noOfSingleCampaign = 0;
                    int noOfSingleAdGroups = 0;
                    int noOfSingleKeywords = 0;
                    int noOfSingleConversion = 0;

                    foreach (var subtype in reportReplaceData.SubType)
                    {
                        //put extra logic for mailchimp get dynamic id  // 76(82[8af9ff7617]) key: 82 Value: 8af9ff7617
                        var subtypeAndMcValue = ExtractNumber(subtype);

                        if (subtypeAndMcValue.Key == (int)ReportTypes.MSCampaignLineChart)
                        {

                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/msAdsLineChart.html");
                            //using (HttpClient httpclient = new HttpClient())
                            //{
                            //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                            //}

                            string path = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/msAdsLineChart.html");
                            htmlString = System.IO.File.ReadAllText(path);

                            htmlString = htmlString.Replace("_msAdsLineChartId_", "msLine" + DateTime.UtcNow.Ticks.ToString());

                            int intervalRes = msAdsData.RootCampaignPerformace.dates.Count <= 31 ? 3 : (msAdsData.RootCampaignPerformace.dates.Count <= 91 ? 7 : 31);
                            htmlString = htmlString.Replace("_msAdsLabelInterval_", intervalRes.ToString());

                            var labels = string.Join(",", msAdsData.RootCampaignPerformace.dates.Select(x => "'" + x + "'"));

                            var currentData = string.Join(",", msAdsData.RootCampaignPerformace.clickChartValue);

                            htmlString = htmlString.Replace("_msLineChartData_", currentData);
                            htmlString = htmlString.Replace("_msAdsLineChartLabel_", labels);
                            htmlString = htmlString.Replace("_msAdsLineTotal_", msAdsData.RootCampaignPerformace.clicks);
                            htmlString = htmlString.Replace("_msAdsLineTitle_", "Clicks");

                            string uniqueKey = $"{91}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (subtypeAndMcValue.Key == (int)ReportTypes.MSCampaignBarChart)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/msAdsBarChart.html");
                            //using (HttpClient httpclient = new HttpClient())
                            //{
                            //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                            //}

                            string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/msAdsBarChart.html");
                            htmlString = System.IO.File.ReadAllText(pathGa);


                            htmlString = htmlString.Replace("_msAdsBarChartId_", "msBar" + DateTime.UtcNow.Ticks.ToString());

                            var labels = string.Join(",", msAdsData.RootCampaignPerformace.campaignsName.Select(x => "'" + x + "'"));

                            var currentData = string.Join(",", msAdsData.RootCampaignPerformace.clickBarChartValue);

                            htmlString = htmlString.Replace("_msAdsBarChartData_", currentData);

                            htmlString = htmlString.Replace("_msAdsBarChartLabel_", labels);

                            htmlString = htmlString.Replace("_msAdsBarChartTitle_", "Clicks");

                            htmlString = htmlString.Replace("_msAdsBarChartTotal", msAdsData.RootCampaignPerformace.clicks);

                            string uniqueKey = $"{91}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (subtypeAndMcValue.Key == (int)ReportTypes.MSCampaignTiles)
                        {

                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/msAdsCampaignTiles.html");
                            //using (HttpClient httpclient = new HttpClient())
                            //{
                            //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                            //}

                            string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/msAdsCampaignTiles.html");
                            htmlString = System.IO.File.ReadAllText(pathGa);


                            htmlString = htmlString.Replace("_msAdsClicksData_", msAdsData.RootCampaignPerformace.clicks);

                            htmlString = htmlString.Replace("_msAdImpressionsData_", msAdsData.RootCampaignPerformace.impressions);

                            htmlString = htmlString.Replace("_msAdCTRData_", msAdsData.RootCampaignPerformace.ctr);

                            htmlString = htmlString.Replace("_msAdAVGCPCData_", msAdsData.RootCampaignPerformace.averageCpc);

                            htmlString = htmlString.Replace("_msAdCostData_", msAdsData.RootCampaignPerformace.spend.ToString());

                            htmlString = htmlString.Replace("_msAdsConversions_", msAdsData.RootCampaignPerformace.conversions.ToString());

                            htmlString = htmlString.Replace("_msAdsCoversionRate_", msAdsData.RootCampaignPerformace.conversionRate);

                            htmlString = htmlString.Replace("_msAdsCostPerConversion_", msAdsData.RootCampaignPerformace.costPerConversion);

                            htmlString = htmlString.Replace("_msAdsImpressionShare_", msAdsData.RootCampaignPerformace.impressionSharePercent);

                            htmlString = htmlString.Replace("_msAdsLostImpressionShare_", msAdsData.RootCampaignPerformace.impressionLostToBudgetPercent);

                            htmlString = htmlString.Replace("_msAdsLostImpressionShareRank_", msAdsData.RootCampaignPerformace.impressionLostToRankAggPercent);

                            string uniqueKey = $"{91}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (subtypeAndMcValue.Key == (int)ReportTypes.MSCampaignTable)
                        {

                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/msAdsCampaignTable.html");
                            htmlString = System.IO.File.ReadAllText(pathGa);

                            //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/msAdsCampaignTable.html");
                            //using (HttpClient httpclient = new HttpClient())
                            //{
                            //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                            //}

                            // serialize object
                            var tableString = JsonConvert.SerializeObject(msAdsData.RootCampaignPerformace.campaignPerformanceDto);

                            htmlString = htmlString.Replace("_msAdsCampaignTableId_", "msads" + DateTime.UtcNow.Ticks.ToString());

                            htmlString = htmlString.Replace("_msAdsCampaignTableData_", tableString);

                            string uniqueKey = $"{91}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (subtypeAndMcValue.Key == (int)ReportTypes.MSAdsGroupLineChart)
                        {

                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/msAdsLineChart.html");
                            //using (HttpClient httpclient = new HttpClient())
                            //{
                            //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                            //}

                            string path = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/msAdsLineChart.html");
                            htmlString = System.IO.File.ReadAllText(path);

                            htmlString = htmlString.Replace("_msAdsLineChartId_", "msLine" + DateTime.UtcNow.Ticks.ToString());

                            var labels = string.Join(",", msAdsData.RootAdGroupPerformance.dates.Select(x => "'" + x + "'"));

                            int intervalRes = msAdsData.RootAdGroupPerformance.dates.Count <= 31 ? 3 : (msAdsData.RootAdGroupPerformance.dates.Count <= 91 ? 7 : 31);
                            htmlString = htmlString.Replace("_msAdsLabelInterval_", intervalRes.ToString());

                            var currentData = string.Join(",", msAdsData.RootAdGroupPerformance.clickChartValue);

                            htmlString = htmlString.Replace("_msLineChartData_", currentData);
                            htmlString = htmlString.Replace("_msAdsLineChartLabel_", labels);
                            htmlString = htmlString.Replace("_msAdsLineTotal_", msAdsData.RootAdGroupPerformance.clicks);
                            htmlString = htmlString.Replace("_msAdsLineTitle_", "Clicks");

                            string uniqueKey = $"{91}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (subtypeAndMcValue.Key == (int)ReportTypes.MSAdsGroupBarChart)
                        {

                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/msAdsBarChart.html");
                            //using (HttpClient httpclient = new HttpClient())
                            //{
                            //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                            //}

                            string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/msAdsBarChart.html");
                            htmlString = System.IO.File.ReadAllText(pathGa);


                            htmlString = htmlString.Replace("_msAdsBarChartId_", "msBar" + DateTime.UtcNow.Ticks.ToString());

                            var labels = string.Join(",", msAdsData.RootAdGroupPerformance.adGroupName.Select(x => "'" + x + "'"));

                            var currentData = string.Join(",", msAdsData.RootAdGroupPerformance.clickBarChartValue);

                            htmlString = htmlString.Replace("_msAdsBarChartData_", currentData);

                            htmlString = htmlString.Replace("_msAdsBarChartLabel_", labels);

                            htmlString = htmlString.Replace("_msAdsBarChartTitle_", "Clicks");

                            htmlString = htmlString.Replace("_msAdsBarChartTotal", msAdsData.RootAdGroupPerformance.clicks);

                            string uniqueKey = $"{91}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (subtypeAndMcValue.Key == (int)ReportTypes.MSAdsGroupTiles)
                        {

                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/msAdsGroupTiles.html");
                            //using (HttpClient httpclient = new HttpClient())
                            //{
                            //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                            //}

                            string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/msAdsGroupTiles.html");
                            htmlString = System.IO.File.ReadAllText(pathGa);


                            htmlString = htmlString.Replace("_msAdsClicksData_", msAdsData.RootAdGroupPerformance.clicks);

                            htmlString = htmlString.Replace("_msAdImpressionsData_", msAdsData.RootAdGroupPerformance.impressions);

                            htmlString = htmlString.Replace("_msAdCTRData_", msAdsData.RootAdGroupPerformance.ctr);

                            htmlString = htmlString.Replace("_msAdAVGCPCData_", msAdsData.RootAdGroupPerformance.averageCpc);

                            htmlString = htmlString.Replace("_msAdCostData_", msAdsData.RootAdGroupPerformance.spend.ToString());

                            htmlString = htmlString.Replace("_msAdsConversions_", msAdsData.RootAdGroupPerformance.conversions.ToString());

                            htmlString = htmlString.Replace("_msAdsCoversionRate_", msAdsData.RootAdGroupPerformance.conversionRate);

                            htmlString = htmlString.Replace("_msAdsCostPerConversion_", msAdsData.RootAdGroupPerformance.costPerConversion);

                            htmlString = htmlString.Replace("_msAdsImpressionShare_", msAdsData.RootAdGroupPerformance.impressionSharePercent);

                            htmlString = htmlString.Replace("_msAdsLostImpressionShare_", msAdsData.RootAdGroupPerformance.impressionLostToBudgetPercent);

                            htmlString = htmlString.Replace("_msAdsLostImpressionShareRank_", msAdsData.RootAdGroupPerformance.impressionLostToRankAggPercent);

                            string uniqueKey = $"{91}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (subtypeAndMcValue.Key == (int)ReportTypes.MSAdsGroupTable)
                        {

                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/msAdsGroupTable.html");
                            htmlString = System.IO.File.ReadAllText(pathGa);

                            //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/msAdsGroupTable.html");
                            //using (HttpClient httpclient = new HttpClient())
                            //{
                            //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                            //}

                            // serialize object
                            var tableString = JsonConvert.SerializeObject(msAdsData.RootAdGroupPerformance.adGroupPerformanceDto);

                            htmlString = htmlString.Replace("_msAdsGroupCampaignTableId_", "msads" + DateTime.UtcNow.Ticks.ToString());

                            htmlString = htmlString.Replace("_msAdsGroupsTableData_", tableString);

                            string uniqueKey = $"{91}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (subtypeAndMcValue.Key == (int)ReportTypes.MSKeywordsLineChart)
                        {


                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/msAdsLineChart.html");
                            //using (HttpClient httpclient = new HttpClient())
                            //{
                            //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                            //}

                            string path = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/msAdsLineChart.html");
                            htmlString = System.IO.File.ReadAllText(path);

                            htmlString = htmlString.Replace("_msAdsLineChartId_", "msLine" + DateTime.UtcNow.Ticks.ToString());

                            var labels = string.Join(",", msAdsData.RootKeywordPerformance.dates.Select(x => "'" + x + "'"));

                            int intervalRes = msAdsData.RootKeywordPerformance.dates.Count <= 31 ? 3 : (msAdsData.RootKeywordPerformance.dates.Count <= 91 ? 7 : 31);
                            htmlString = htmlString.Replace("_msAdsLabelInterval_", intervalRes.ToString());

                            var currentData = string.Join(",", msAdsData.RootKeywordPerformance.clickChartValue);

                            htmlString = htmlString.Replace("_msLineChartData_", currentData);
                            htmlString = htmlString.Replace("_msAdsLineChartLabel_", labels);
                            htmlString = htmlString.Replace("_msAdsLineTotal_", msAdsData.RootKeywordPerformance.clicks);
                            htmlString = htmlString.Replace("_msAdsLineTitle_", "Clicks");

                            string uniqueKey = $"{91}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (subtypeAndMcValue.Key == (int)ReportTypes.MSKeywordsBarChart)
                        {

                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/msAdsKeywordStatus.html");
                            //using (HttpClient httpclient = new HttpClient())
                            //{
                            //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                            //}

                            string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/msAdsKeywordStatus.html");
                            htmlString = System.IO.File.ReadAllText(pathGa);

                            htmlString = htmlString.Replace("_msAdsBarsContainerId_", "msAds" + DateTime.UtcNow.Ticks.ToString());

                            var data = msAdsData.RootKeywordPerformance.clickBarChartValue;
                            var total = data.Sum();

                            var percentages = total == 0 ? new List<int>() : data.Select(x => (int)Math.Round((double)x / total * 100)).ToList();

                            var sourceData = string.Join(",", msAdsData.RootKeywordPerformance.clickBarChartLabel.Select(x => "'" + x + "'"));

                            var valueData = string.Join(",", data);

                            var percentageData = string.Join(",", percentages);

                            // Create a new list by selecting elements from the original list based on the count
                            htmlString = htmlString.Replace("_msAdsSourceList_", sourceData);
                            htmlString = htmlString.Replace("_msAdsValueList_", valueData);
                            htmlString = htmlString.Replace("_msAdsPercentageList_", percentageData);

                            string uniqueKey = $"{91}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (subtypeAndMcValue.Key == (int)ReportTypes.MSKeywordsTiles)
                        {

                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/msAdsKeywordsTiles.html");
                            //using (HttpClient httpclient = new HttpClient())
                            //{
                            //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                            //}

                            string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/msAdsKeywordsTiles.html");
                            htmlString = System.IO.File.ReadAllText(pathGa);


                            htmlString = htmlString.Replace("_msAdsClicksData_", msAdsData.RootKeywordPerformance.clicks);

                            htmlString = htmlString.Replace("_msAdImpressionsData_", msAdsData.RootKeywordPerformance.impressions);

                            htmlString = htmlString.Replace("_msAdCTRData_", msAdsData.RootKeywordPerformance.ctr);

                            htmlString = htmlString.Replace("_msAdAVGCPCData_", msAdsData.RootKeywordPerformance.averageCpc);

                            htmlString = htmlString.Replace("_msAdCostData_", msAdsData.RootKeywordPerformance.spend.ToString());

                            htmlString = htmlString.Replace("_msAdsConversions_", msAdsData.RootKeywordPerformance.conversions.ToString());

                            htmlString = htmlString.Replace("_msAdsCoversionRate_", msAdsData.RootKeywordPerformance.conversionRate);

                            htmlString = htmlString.Replace("_msAdsCostPerConversion_", msAdsData.RootKeywordPerformance.costPerConversion);

                            string uniqueKey = $"{91}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (subtypeAndMcValue.Key == (int)ReportTypes.MSKeywordsTable)
                        {

                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/msAdsKeywordTable.html");
                            htmlString = System.IO.File.ReadAllText(pathGa);

                            //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/msAdsKeywordTable.html");
                            //using (HttpClient httpclient = new HttpClient())
                            //{
                            //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                            //}

                            // serialize object
                            var tableString = JsonConvert.SerializeObject(msAdsData.RootKeywordPerformance.keywordPerformanceDto);

                            htmlString = htmlString.Replace("_msAdsKeywordTableId_", "msads" + DateTime.UtcNow.Ticks.ToString());

                            htmlString = htmlString.Replace("_msAdsKeywordTableData_", tableString);

                            string uniqueKey = $"{91}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (subtypeAndMcValue.Key == (int)ReportTypes.MSConversionLineChart)
                        {

                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/msAdsLineChart.html");
                            //using (HttpClient httpclient = new HttpClient())
                            //{
                            //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                            //}

                            string path = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/msAdsLineChart.html");
                            htmlString = System.IO.File.ReadAllText(path);

                            htmlString = htmlString.Replace("_msAdsLineChartId_", "msLine" + DateTime.UtcNow.Ticks.ToString());

                            var labels = string.Join(",", msAdsData.RootConversionPerformance.dates.Select(x => "'" + x + "'"));

                            int intervalRes = msAdsData.RootConversionPerformance.dates.Count <= 31 ? 3 : (msAdsData.RootConversionPerformance.dates.Count <= 91 ? 7 : 31);
                            htmlString = htmlString.Replace("_msAdsLabelInterval_", intervalRes.ToString());

                            var currentData = string.Join(",", msAdsData.RootConversionPerformance.conversionLineChartValue);

                            htmlString = htmlString.Replace("_msLineChartData_", currentData);
                            htmlString = htmlString.Replace("_msAdsLineChartLabel_", labels);
                            htmlString = htmlString.Replace("_msAdsLineTotal_", msAdsData.RootConversionPerformance.conversions.ToString());
                            htmlString = htmlString.Replace("_msAdsLineTitle_", "Conversions");

                            string uniqueKey = $"{91}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (subtypeAndMcValue.Key == (int)ReportTypes.MSConversionBarChart)
                        {

                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/msAdsBarChart.html");
                            //using (HttpClient httpclient = new HttpClient())
                            //{
                            //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                            //}

                            string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/msAdsBarChart.html");
                            htmlString = System.IO.File.ReadAllText(pathGa);


                            htmlString = htmlString.Replace("_msAdsBarChartId_", "msBar" + DateTime.UtcNow.Ticks.ToString());

                            var labels = string.Join(",", msAdsData.RootConversionPerformance.campaignsName.Select(x => "'" + x + "'"));

                            var currentData = string.Join(",", msAdsData.RootConversionPerformance.conversionBarChartValue);

                            htmlString = htmlString.Replace("_msAdsBarChartData_", currentData);

                            htmlString = htmlString.Replace("_msAdsBarChartLabel_", labels);

                            htmlString = htmlString.Replace("_msAdsBarChartTitle_", "Conversions");

                            htmlString = htmlString.Replace("_msAdsBarChartTotal", msAdsData.RootConversionPerformance.conversions.ToString());

                            string uniqueKey = $"{91}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (subtypeAndMcValue.Key == (int)ReportTypes.MSConversionTiles)
                        {

                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/msAdsConversionTiles.html");
                            //using (HttpClient httpclient = new HttpClient())
                            //{
                            //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                            //}

                            string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/msAdsConversionTiles.html");
                            htmlString = System.IO.File.ReadAllText(pathGa);

                            htmlString = htmlString.Replace("_msAdsConversionsData_", msAdsData.RootConversionPerformance.conversions.ToString());

                            htmlString = htmlString.Replace("_msAdsRevenueData_", msAdsData.RootConversionPerformance.revenue.ToString());

                            string uniqueKey = $"{91}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (subtypeAndMcValue.Key == (int)ReportTypes.MSConversionTable)
                        {

                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/msAdsConversionTable.html");
                            htmlString = System.IO.File.ReadAllText(pathGa);

                            //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/msAdsConversionTable.html");
                            //using (HttpClient httpclient = new HttpClient())
                            //{
                            //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                            //}

                            // serialize object
                            var tableString = JsonConvert.SerializeObject(msAdsData.RootConversionPerformance.conversionPerformanceDto);

                            htmlString = htmlString.Replace("_msAdsConversionTableId_", "msads" + DateTime.UtcNow.Ticks.ToString());

                            htmlString = htmlString.Replace("_msAdsConvTableData_", tableString);

                            string uniqueKey = $"{91}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                        else if (subtypeAndMcValue.Key == (int)ReportTypes.MSSingleCampaign)
                        {
                            if (msAdsData.RootSingleCampaignPerformace != null && msAdsData.RootSingleCampaignPerformace.Count > 0)
                            {
                                Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                                //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/singleMsAdsCampaign.html");
                                //using (HttpClient httpclient = new HttpClient())
                                //{
                                //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                                //}

                                string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/singleMsAdsCampaign.html");
                                htmlString = System.IO.File.ReadAllText(pathGa);

                                htmlString = htmlString.Replace("_msAdsLineChartId_", "msLine" + DateTime.UtcNow.Ticks.ToString());

                                htmlString = htmlString.Replace("_singleMsAdsCampaignName_", msAdsData.RootSingleCampaignPerformace[noOfSingleCampaign].campaignsName.FirstOrDefault());

                                var labels = string.Join(",", msAdsData.RootSingleCampaignPerformace[noOfSingleCampaign].dates.Select(x => "'" + x + "'"));

                                int intervalRes = msAdsData.RootSingleCampaignPerformace[noOfSingleCampaign].dates.Count <= 31 ? 3 : (msAdsData.RootSingleCampaignPerformace[noOfSingleCampaign].dates.Count <= 91 ? 7 : 31);
                                htmlString = htmlString.Replace("_msAdsLabelInterval_", intervalRes.ToString());

                                var currentData = string.Join(",", msAdsData.RootSingleCampaignPerformace[noOfSingleCampaign].clickChartValue);

                                htmlString = htmlString.Replace("_msLineChartData_", currentData);
                                htmlString = htmlString.Replace("_msAdsLineChartLabel_", labels);
                                htmlString = htmlString.Replace("_msAdsLineTotal_", msAdsData.RootSingleCampaignPerformace[noOfSingleCampaign].clicks);
                                htmlString = htmlString.Replace("_msAdsLineTitle_", "Clicks");

                                //bar

                                htmlString = htmlString.Replace("_msAdsBarChartId_", "msBar" + DateTime.UtcNow.Ticks.ToString());

                                var labels1 = string.Join(",", msAdsData.RootSingleCampaignPerformace[noOfSingleCampaign].campaignsName.Select(x => "'" + x + "'"));

                                var currentData1 = string.Join(",", msAdsData.RootSingleCampaignPerformace[noOfSingleCampaign].clickBarChartValue);

                                htmlString = htmlString.Replace("_msAdsBarChartData_", currentData1);

                                htmlString = htmlString.Replace("_msAdsBarChartLabel_", labels1);

                                htmlString = htmlString.Replace("_msAdsBarChartTitle_", "Clicks");

                                htmlString = htmlString.Replace("_msAdsBarChartTotal", msAdsData.RootSingleCampaignPerformace[noOfSingleCampaign].clicks);

                                //tiles
                                htmlString = htmlString.Replace("_msAdsClicksData_", msAdsData.RootSingleCampaignPerformace[noOfSingleCampaign].clicks);

                                htmlString = htmlString.Replace("_msAdImpressionsData_", msAdsData.RootSingleCampaignPerformace[noOfSingleCampaign].impressions);

                                htmlString = htmlString.Replace("_msAdCTRData_", msAdsData.RootSingleCampaignPerformace[noOfSingleCampaign].ctr);

                                htmlString = htmlString.Replace("_msAdAVGCPCData_", msAdsData.RootSingleCampaignPerformace[noOfSingleCampaign].averageCpc);

                                htmlString = htmlString.Replace("_msAdCostData_", msAdsData.RootSingleCampaignPerformace[noOfSingleCampaign].spend.ToString());

                                htmlString = htmlString.Replace("_msAdsConversions_", msAdsData.RootSingleCampaignPerformace[noOfSingleCampaign].conversions.ToString());

                                htmlString = htmlString.Replace("_msAdsCoversionRate_", msAdsData.RootSingleCampaignPerformace[noOfSingleCampaign].conversionRate);

                                htmlString = htmlString.Replace("_msAdsCostPerConversion_", msAdsData.RootSingleCampaignPerformace[noOfSingleCampaign].costPerConversion);

                                htmlString = htmlString.Replace("_msAdsImpressionShare_", msAdsData.RootSingleCampaignPerformace[noOfSingleCampaign].impressionSharePercent);

                                htmlString = htmlString.Replace("_msAdsLostImpressionShare_", msAdsData.RootSingleCampaignPerformace[noOfSingleCampaign].impressionLostToBudgetPercent);

                                htmlString = htmlString.Replace("_msAdsLostImpressionShareRank_", msAdsData.RootSingleCampaignPerformace[noOfSingleCampaign].impressionLostToRankAggPercent);

                                //Table
                                // serialize object
                                var tableString = JsonConvert.SerializeObject(msAdsData.RootSingleCampaignPerformace[noOfSingleCampaign].campaignPerformanceDto);

                                htmlString = htmlString.Replace("_msAdsCampaignTableId_", "msads" + DateTime.UtcNow.Ticks.ToString());

                                htmlString = htmlString.Replace("_msAdsCampaignTableData_", tableString);

                                string uniqueKey = $"{91}({string.Join(",", subtype)})";
                                uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                                listOfResult.Add(uniqueTypeSubtypeResults);

                                noOfSingleCampaign++;
                            }
                        }
                        else if (subtypeAndMcValue.Key == (int)ReportTypes.MSSingleAdsGroup)
                        {
                            if (msAdsData.RootSingleAdGroupPerformance != null && msAdsData.RootSingleAdGroupPerformance.Count > 0)
                            {
                                Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                                //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/singleMsAdsGroups.html");
                                //using (HttpClient httpclient = new HttpClient())
                                //{
                                //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                                //}

                                string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/singleMsAdsGroups.html");
                                htmlString = System.IO.File.ReadAllText(pathGa);

                                htmlString = htmlString.Replace("_msAdsLineChartId_", "msLine" + DateTime.UtcNow.Ticks.ToString());

                                htmlString = htmlString.Replace("_singleMsAdsGroupName_", msAdsData.RootSingleAdGroupPerformance[noOfSingleAdGroups].adGroupPerformanceDto.FirstOrDefault().CampaignName);

                                var labels = string.Join(",", msAdsData.RootSingleAdGroupPerformance[noOfSingleAdGroups].dates.Select(x => "'" + x + "'"));

                                int intervalRes = msAdsData.RootSingleAdGroupPerformance[noOfSingleAdGroups].dates.Count <= 31 ? 3 : (msAdsData.RootSingleAdGroupPerformance[noOfSingleAdGroups].dates.Count <= 91 ? 7 : 31);
                                htmlString = htmlString.Replace("_msAdsLabelInterval_", intervalRes.ToString());

                                var currentData = string.Join(",", msAdsData.RootSingleAdGroupPerformance[noOfSingleAdGroups].clickChartValue);

                                htmlString = htmlString.Replace("_msLineChartData_", currentData);
                                htmlString = htmlString.Replace("_msAdsLineChartLabel_", labels);
                                htmlString = htmlString.Replace("_msAdsLineTotal_", msAdsData.RootSingleAdGroupPerformance[noOfSingleAdGroups].clicks);
                                htmlString = htmlString.Replace("_msAdsLineTitle_", "Clicks");

                                //bar

                                htmlString = htmlString.Replace("_msAdsBarChartId_", "msBar" + DateTime.UtcNow.Ticks.ToString());

                                var labels1 = string.Join(",", msAdsData.RootSingleAdGroupPerformance[noOfSingleAdGroups].adGroupName.Select(x => "'" + x + "'"));

                                var currentData1 = string.Join(",", msAdsData.RootSingleAdGroupPerformance[noOfSingleAdGroups].clickBarChartValue);

                                htmlString = htmlString.Replace("_msAdsBarChartData_", currentData1);

                                htmlString = htmlString.Replace("_msAdsBarChartLabel_", labels1);

                                htmlString = htmlString.Replace("_msAdsBarChartTitle_", "Clicks");


                                //Tiles

                                htmlString = htmlString.Replace("_msAdsBarChartTotal", msAdsData.RootSingleAdGroupPerformance[noOfSingleAdGroups].clicks);

                                htmlString = htmlString.Replace("_msAdsClicksData_", msAdsData.RootSingleAdGroupPerformance[noOfSingleAdGroups].clicks);

                                htmlString = htmlString.Replace("_msAdImpressionsData_", msAdsData.RootSingleAdGroupPerformance[noOfSingleAdGroups].impressions);

                                htmlString = htmlString.Replace("_msAdCTRData_", msAdsData.RootSingleAdGroupPerformance[noOfSingleAdGroups].ctr);

                                htmlString = htmlString.Replace("_msAdAVGCPCData_", msAdsData.RootSingleAdGroupPerformance[noOfSingleAdGroups].averageCpc);

                                htmlString = htmlString.Replace("_msAdCostData_", msAdsData.RootSingleAdGroupPerformance[noOfSingleAdGroups].spend.ToString());

                                htmlString = htmlString.Replace("_msAdsConversions_", msAdsData.RootSingleAdGroupPerformance[noOfSingleAdGroups].conversions.ToString());

                                htmlString = htmlString.Replace("_msAdsCoversionRate_", msAdsData.RootSingleAdGroupPerformance[noOfSingleAdGroups].conversionRate);

                                htmlString = htmlString.Replace("_msAdsCostPerConversion_", msAdsData.RootSingleAdGroupPerformance[noOfSingleAdGroups].costPerConversion);

                                htmlString = htmlString.Replace("_msAdsImpressionShare_", msAdsData.RootSingleAdGroupPerformance[noOfSingleAdGroups].impressionSharePercent);

                                htmlString = htmlString.Replace("_msAdsLostImpressionShare_", msAdsData.RootSingleAdGroupPerformance[noOfSingleAdGroups].impressionLostToBudgetPercent);

                                htmlString = htmlString.Replace("_msAdsLostImpressionShareRank_", msAdsData.RootSingleAdGroupPerformance[noOfSingleAdGroups].impressionLostToRankAggPercent);

                                // serialize object
                                var tableString = JsonConvert.SerializeObject(msAdsData.RootSingleAdGroupPerformance[noOfSingleAdGroups].adGroupPerformanceDto);

                                htmlString = htmlString.Replace("_msAdsGroupCampaignTableId_", "msads" + DateTime.UtcNow.Ticks.ToString());

                                htmlString = htmlString.Replace("_msAdsGroupsTableData_", tableString);

                                string uniqueKey = $"{91}({string.Join(",", subtype)})";
                                uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                                listOfResult.Add(uniqueTypeSubtypeResults);

                                noOfSingleAdGroups++;
                            }
                        }
                        else if (subtypeAndMcValue.Key == (int)ReportTypes.MSSingleKeywords)
                        {
                            if (msAdsData.RootSingleKeywordPerformance != null && msAdsData.RootSingleKeywordPerformance.Count > 0)
                            {
                                Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                                //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/singleMsAdsKeyword.html");
                                //using (HttpClient httpclient = new HttpClient())
                                //{
                                //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                                //}

                                string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/singleMsAdsKeyword.html");
                                htmlString = System.IO.File.ReadAllText(pathGa);

                                htmlString = htmlString.Replace("_msAdsLineChartId_", "msLine" + DateTime.UtcNow.Ticks.ToString());

                                htmlString = htmlString.Replace("_singleMsAdsKeywordName_", msAdsData.RootSingleKeywordPerformance[noOfSingleKeywords].keywordPerformanceDto.FirstOrDefault().CampaignName);

                                var labels = string.Join(",", msAdsData.RootSingleKeywordPerformance[noOfSingleKeywords].dates.Select(x => "'" + x + "'"));

                                int intervalRes = msAdsData.RootSingleKeywordPerformance[noOfSingleKeywords].dates.Count <= 31 ? 3 : (msAdsData.RootSingleKeywordPerformance[noOfSingleKeywords].dates.Count <= 91 ? 7 : 31);
                                htmlString = htmlString.Replace("_msAdsLabelInterval_", intervalRes.ToString());

                                var currentData = string.Join(",", msAdsData.RootSingleKeywordPerformance[noOfSingleKeywords].clickChartValue);

                                htmlString = htmlString.Replace("_msLineChartData_", currentData);
                                htmlString = htmlString.Replace("_msAdsLineChartLabel_", labels);
                                htmlString = htmlString.Replace("_msAdsLineTotal_", msAdsData.RootSingleKeywordPerformance[noOfSingleKeywords].clicks);
                                htmlString = htmlString.Replace("_msAdsLineTitle_", "Clicks");

                                //progress bar

                                htmlString = htmlString.Replace("_msAdsBarsContainerId_", "msAds" + DateTime.UtcNow.Ticks.ToString());

                                var data = msAdsData.RootSingleKeywordPerformance[noOfSingleKeywords].clickBarChartValue;
                                var total = data.Sum();

                                var percentages = total == 0 ? new List<int>() : data.Select(x => (int)Math.Round((double)x / total * 100)).ToList();

                                var sourceData = string.Join(",", msAdsData.RootSingleKeywordPerformance[noOfSingleKeywords].clickBarChartLabel.Select(x => "'" + x + "'"));

                                var valueData = string.Join(",", data);

                                var percentageData = string.Join(",", percentages);

                                // Create a new list by selecting elements from the original list based on the count
                                htmlString = htmlString.Replace("_msAdsSourceList_", sourceData);
                                htmlString = htmlString.Replace("_msAdsValueList_", valueData);
                                htmlString = htmlString.Replace("_msAdsPercentageList_", percentageData);



                                //Tiles
                                htmlString = htmlString.Replace("_msAdsClicksData_", msAdsData.RootSingleKeywordPerformance[noOfSingleKeywords].clicks);

                                htmlString = htmlString.Replace("_msAdImpressionsData_", msAdsData.RootSingleKeywordPerformance[noOfSingleKeywords].impressions);

                                htmlString = htmlString.Replace("_msAdCTRData_", msAdsData.RootSingleKeywordPerformance[noOfSingleKeywords].ctr);

                                htmlString = htmlString.Replace("_msAdAVGCPCData_", msAdsData.RootSingleKeywordPerformance[noOfSingleKeywords].averageCpc);

                                htmlString = htmlString.Replace("_msAdCostData_", msAdsData.RootSingleKeywordPerformance[noOfSingleKeywords].spend.ToString());

                                htmlString = htmlString.Replace("_msAdsConversions_", msAdsData.RootSingleKeywordPerformance[noOfSingleKeywords].conversions.ToString());

                                htmlString = htmlString.Replace("_msAdsCoversionRate_", msAdsData.RootSingleKeywordPerformance[noOfSingleKeywords].conversionRate);

                                htmlString = htmlString.Replace("_msAdsCostPerConversion_", msAdsData.RootSingleKeywordPerformance[noOfSingleKeywords].costPerConversion);

                                // serialize object
                                var tableString = JsonConvert.SerializeObject(msAdsData.RootSingleKeywordPerformance[noOfSingleKeywords].keywordPerformanceDto);

                                htmlString = htmlString.Replace("_msAdsKeywordTableId_", "msads" + DateTime.UtcNow.Ticks.ToString());

                                htmlString = htmlString.Replace("_msAdsKeywordTableData_", tableString);


                                string uniqueKey = $"{91}({string.Join(",", subtype)})";
                                uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                                listOfResult.Add(uniqueTypeSubtypeResults);
                                noOfSingleKeywords++;

                            }
                        }
                        else if (subtypeAndMcValue.Key == (int)ReportTypes.MSSingleConversion)
                        {
                            var singleConversionData = new RootConversionPerformance();

                            if (msAdsData.RootSingleConversionPerformance != null && msAdsData.RootSingleConversionPerformance.Count > 0)
                            {
                                Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                                //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/singleMsAdsConversion.html");
                                //using (HttpClient httpclient = new HttpClient())
                                //{
                                //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                                //}

                                string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/singleMsAdsConversion.html");
                                htmlString = System.IO.File.ReadAllText(pathGa);

                                htmlString = htmlString.Replace("_msAdsLineChartId_", "msLine" + DateTime.UtcNow.Ticks.ToString());

                                htmlString = htmlString.Replace("_singleMsAdsConversionName_", msAdsData.RootSingleConversionPerformance[noOfSingleConversion].conversionPerformanceDto.FirstOrDefault().CampaignName);

                                var labels = string.Join(",", msAdsData.RootSingleConversionPerformance[noOfSingleConversion].dates.Select(x => "'" + x + "'"));

                                int intervalRes = msAdsData.RootSingleConversionPerformance[noOfSingleConversion].dates.Count <= 31 ? 3 : (msAdsData.RootSingleConversionPerformance[noOfSingleConversion].dates.Count <= 91 ? 7 : 31);
                                htmlString = htmlString.Replace("_msAdsLabelInterval_", intervalRes.ToString());

                                var currentData = string.Join(",", msAdsData.RootSingleConversionPerformance[noOfSingleConversion].conversionLineChartValue);

                                htmlString = htmlString.Replace("_msLineChartData_", currentData);
                                htmlString = htmlString.Replace("_msAdsLineChartLabel_", labels);
                                htmlString = htmlString.Replace("_msAdsLineTotal_", msAdsData.RootSingleConversionPerformance[noOfSingleConversion].conversions.ToString());
                                htmlString = htmlString.Replace("_msAdsLineTitle_", "Clicks");

                                //bar

                                htmlString = htmlString.Replace("_msAdsBarChartTotal", msAdsData.RootSingleConversionPerformance[noOfSingleConversion].conversions.ToString());

                                htmlString = htmlString.Replace("_msAdsBarChartId_", "msBar" + DateTime.UtcNow.Ticks.ToString());

                                var labels1 = string.Join(",", msAdsData.RootSingleConversionPerformance[noOfSingleConversion].campaignsName.Select(x => "'" + x + "'"));

                                var currentData1 = string.Join(",", msAdsData.RootSingleConversionPerformance[noOfSingleConversion].conversionBarChartValue);

                                htmlString = htmlString.Replace("_msAdsBarChartData_", currentData1);

                                htmlString = htmlString.Replace("_msAdsBarChartLabel_", labels1);

                                htmlString = htmlString.Replace("_msAdsBarChartTitle_", "Clicks");

                                //Tiles
                                htmlString = htmlString.Replace("_msAdsConversionsData_", msAdsData.RootSingleConversionPerformance[noOfSingleConversion].conversions.ToString());

                                htmlString = htmlString.Replace("_msAdsRevenueData_", msAdsData.RootSingleConversionPerformance[noOfSingleConversion].revenue.ToString());


                                // serialize object
                                var tableString = JsonConvert.SerializeObject(msAdsData.RootSingleConversionPerformance[noOfSingleConversion].conversionPerformanceDto);

                                htmlString = htmlString.Replace("_msAdsConversionTableId_", "msads" + DateTime.UtcNow.Ticks.ToString());

                                htmlString = htmlString.Replace("_msAdsConvTableData_", tableString);

                                string uniqueKey = $"{91}({string.Join(",", subtype)})";
                                uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                                listOfResult.Add(uniqueTypeSubtypeResults);

                                noOfSingleConversion++;
                            }


                        }
                    }
                }
            }
            return listOfResult;
        }


        public string PrepareDataGa4(string data)
        {
            string[] parts = data.Split("--");
            double currentData = Math.Round(double.Parse(parts[0]), 2);
            double previousData = Math.Round(double.Parse(parts[1]), 2);

            double difference = currentData - previousData;
            string sign = (difference >= 0) ? "+" : "-";
            double absoluteDifference = Math.Round(Math.Abs(difference), 2);

            return $"{currentData}({sign}{absoluteDifference})";

        }

        static List<string> RemovePatternFromList(List<string> data)
        {
            var pattern = @"\[\w+\]";
            return data.Select(item => Regex.Replace(item, pattern, "")).ToList();
        }


        public async Task<string> PreparePdfReport(GenerateReportDto generateReportDto)
        {
            var a2pClient = new Api2Pdf.Api2Pdf(_configuration["Api2Pdf"]);
            var reportTypeList = generateReportDto.ReportTypeList;

            var AnalyticsOrganicTrafficReport = String.Empty;
            var googleSearchConsoleReport = String.Empty;
            var googleAdsReport = String.Empty;
            var linkedInReport = String.Empty;
            var instagramReport = String.Empty;
            var facebookAdsReport = String.Empty;
            var facebookReportHtml = String.Empty;
            int pageNumber = 0;
            var isCoverPageExist = generateReportDto.IsCoverPage;
            var isTocExist = generateReportDto.TableOfContent;

            string companyLogo = generateReportDto.ReportGenerateSetting.HeaderLableImg;
            string campaignLogo = generateReportDto.ReportGenerateSetting.HeaderLableImgCamp;
            string headerText = generateReportDto.ReportGenerateSetting.HeaderTextValue;
            string headerTextColor = generateReportDto.ReportGenerateSetting.HeaderTextColor;
            string headerBgColor = generateReportDto.ReportGenerateSetting.HeaderBgColor;
            string coverPageBgImage = generateReportDto.ReportGenerateSetting.CoverPageBgImage;
            string coverPageBgColor = generateReportDto.ReportGenerateSetting.CoverPageBgColor;
            string coverPageTextColor = generateReportDto.ReportGenerateSetting.CoverPageTextColor;
            string coverPageTitle = generateReportDto.ReportGenerateSetting.Name;

            string footerText = string.Empty;
            string showPageNumberId = string.Empty;
            string showPageNumber = string.Empty;

            if (generateReportDto.ReportGenerateSetting.ShowFooter)
            {
                footerText = generateReportDto.ReportGenerateSetting.FooterText;
            }
            if (generateReportDto.ReportGenerateSetting.ShowFooterPageNumber)
            {
                showPageNumberId = "pageFooter";
                showPageNumber = "none";
            }

            DateTime todayDate = DateTime.UtcNow;

            DateTime endDateForEmailReport = new DateTime();
            DateTime startDateForEmailReport = new DateTime();
            DateTime prevDateForEmailReport = new DateTime();

            //today
            if (generateReportDto.Frequency == 0)
            {
                endDateForEmailReport = todayDate.Date;
                startDateForEmailReport = todayDate.Date;
            }
            //7 days
            else if (generateReportDto.Frequency == 7)
            {
                endDateForEmailReport = todayDate.Date.AddDays(-1);
                startDateForEmailReport = todayDate.AddDays(-7).Date;
            }
            //30 days
            else if (generateReportDto.Frequency == 30)
            {
                endDateForEmailReport = todayDate.Date.AddDays(-1);
                startDateForEmailReport = todayDate.AddDays(-30).Date;
            }
            //60 days
            else if (generateReportDto.Frequency == 60)
            {
                endDateForEmailReport = todayDate.Date.AddDays(-1);
                startDateForEmailReport = todayDate.AddDays(-60).Date;
            }
            //Last Month
            else if (generateReportDto.Frequency == 1)
            {
                var month = new DateTime(todayDate.Year, todayDate.Month, 01);
                endDateForEmailReport = month.AddDays(-1);
                startDateForEmailReport = month.AddMonths(-1);
            }
            //last 3 months
            else if (generateReportDto.Frequency == 90)
            {
                var month = new DateTime(todayDate.Year, todayDate.Month, 01);
                endDateForEmailReport = month.AddDays(-1);
                startDateForEmailReport = month.AddMonths(-3);
            }
            //last year
            else if (generateReportDto.Frequency == 4)
            {
                prevDateForEmailReport = todayDate.AddYears(-1);
                startDateForEmailReport = new DateTime(prevDateForEmailReport.Year, 01, 01);
                endDateForEmailReport = new DateTime(prevDateForEmailReport.Year, 12, 31);
            }
            //This month 
            else if (generateReportDto.Frequency == 2)
            {
                startDateForEmailReport = new DateTime(todayDate.Year, todayDate.Month, 01);
                endDateForEmailReport = todayDate.Date;
            }
            //This year 
            else if (generateReportDto.Frequency == 3)
            {
                startDateForEmailReport = new DateTime(todayDate.Year, 01, 01);
                endDateForEmailReport = todayDate.Date;
            }
            //Custom Range
            else if (generateReportDto.Frequency == 5)
            {
                startDateForEmailReport = generateReportDto.StartDate;
                endDateForEmailReport = generateReportDto.EndDate;
            }

            List<string> htmlArray = new List<string>();

            List<Dictionary<string, string>> listOfResult = new List<Dictionary<string, string>>();

            //add head and style in list of html
            string configHtmlString = string.Empty;


            string configPath = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/config.html");
            using (HttpClient httpclient = new HttpClient())
            {
                configHtmlString = httpclient.GetStringAsync(configPath).Result;
            }

            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/config.html");
            //configHtmlString = System.IO.File.ReadAllText(pathGa);

            htmlArray.Add(configHtmlString);

            if (isCoverPageExist)
            {
                string htmlString2 = "";
                //string pathcoverpage = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/coverPAge.html");
                //htmlString2 = System.IO.File.ReadAllText(pathcoverpage);

                string pathcoverpage = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/coverPAge.html");
                using (HttpClient httpclient = new HttpClient())
                {
                    htmlString2 = httpclient.GetStringAsync(pathcoverpage).Result;
                }

                var reportName = generateReportDto.Name;

                htmlString2 = await PrepareCoverPageHtml(coverPageTitle, startDateForEmailReport.ToString("ddd, MMMM dd yyyy"), endDateForEmailReport.ToString("ddd, MMMM dd yyyy"), htmlString2, companyLogo, campaignLogo, generateReportDto.CompanyName, coverPageTextColor, coverPageBgColor, coverPageBgImage, generateReportDto.CampaignName);
                htmlArray.Insert(0, htmlString2);
            }
            if (generateReportDto.TableOfContent)
            {
                var modifiedTypeList = new List<string> { };
                bool oldIndexExist = false;
                string htmlString1 = "";

                // string pathToc = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/tableOfContent.html");
                pageNumber = pageNumber + 1;
                //string pathToc = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/tableOfContent.html");
                //htmlString1 = System.IO.File.ReadAllText(pathToc);

                string pathToc = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/tableOfContent.html");
                using (HttpClient httpclient = new HttpClient())
                {
                    htmlString1 = httpclient.GetStringAsync(pathToc).Result;
                }

                var indexContent = JsonConvert.DeserializeObject<IndexSettings>(generateReportDto.IndexSettings);

                if (indexContent == null)
                {
                    modifiedTypeList = RemovePatternFromList(reportTypeList);
                    oldIndexExist = true;

                }
                else
                {
                    modifiedTypeList = indexContent.IndexSettingsData;
                    oldIndexExist = false;
                }
                htmlString1 = await PrepareTableOfContentHtml(indexContent.IndexSettingsData, htmlString1, companyLogo, campaignLogo, headerText, footerText, headerTextColor, headerBgColor, generateReportDto.ReportGenerateSetting.ShowFooter, showPageNumberId, showPageNumber
                    , generateReportDto.ReportGenerateSetting.ShowHeader, pageNumber, oldIndexExist);
                htmlArray.Add(htmlString1);
            }

            // Create a dictionary to group subtypes by type
            Dictionary<int, List<string>> typeSubtypeDictionary = new Dictionary<int, List<string>>();

            // Traverse the reportTypeList to combine subtypes for each type
            foreach (var reportType in reportTypeList)
            {
                // Extract the type and subtypes using the provided method
                TypeSubType typeSubType = ExtractTypeAndSubtypes(reportType);

                // Check if the dictionary already contains an entry with the same type
                if (typeSubtypeDictionary.ContainsKey(typeSubType.Type))
                {
                    // If an entry with the same type exists, merge the subtypes
                    typeSubtypeDictionary[typeSubType.Type].AddRange(typeSubType.Subtype);
                }
                else
                {
                    // If there's no entry with the same type, add the current reportType's subtypes to the dictionary
                    typeSubtypeDictionary[typeSubType.Type] = typeSubType.Subtype;
                }
            }

            // Create the updatedReportTypeList by combining types and subtypes
            List<string> updatedReportTypeList = typeSubtypeDictionary
                .Select(kv => kv.Value.Count > 0 ? $"{kv.Key}({string.Join(",", kv.Value)})" : kv.Key.ToString())
                .ToList();

            // Now the updatedReportTypeList contains the modified report types
            foreach (var reportType in updatedReportTypeList)
            {
                if (!String.IsNullOrEmpty(reportType))
                {
                    //List<string> types = new List<string>();
                    List<string> subtypes = new List<string>();
                    int type = 0;

                    var res = ExtractTypeAndSubtypes(reportType);

                    type = res.Type;
                    subtypes = res.Subtype;

                    var replaceData = new ReportReplaceData();
                    replaceData.FooterText = footerText;
                    replaceData.ShowFooter = generateReportDto.ReportGenerateSetting.ShowFooter;
                    replaceData.ShowPageNumberId = showPageNumberId;
                    replaceData.ShowPageNumber = showPageNumber;
                    replaceData.ShowHeader = generateReportDto.ReportGenerateSetting.ShowHeader;
                    replaceData.ReportSetting = generateReportDto.ReportGenerateSetting;
                    replaceData.RootReportData = generateReportDto.ReportData;
                    replaceData.Type = type;
                    replaceData.PageNumber = pageNumber;
                    replaceData.SubType = subtypes;

                    if (type == (int)ReportTypes.GoogleSearchConsole)
                    {
                        string htmlString = "";

                        try
                        {
                            //add all subtypes in gsc sub type list
                            if (subtypes.Count == 0)
                            {
                                subtypes.AddRange(new List<string>
                                    {
                                        "30", "31","32","33"
                                    });
                            }
                            replaceData.SubType = subtypes;
                            var result = await ReplaceData(replaceData);
                            listOfResult.AddRange(result);
                            //htmlArray.AddRange(result);
                        }
                        catch (Exception ex)
                        {
                            htmlString = "<p><h3>Something went wrong in google search console.</h3></p>";
                            htmlArray.Add(htmlString);
                        }

                    }
                    else if (type == (int)ReportTypes.GoogleAdsCampaign)
                    {
                        string htmlString = "";

                        pageNumber = pageNumber + 1;
                        try
                        {
                            var result = await ReplaceData(replaceData);
                            listOfResult.AddRange(result);
                            //htmlArray.AddRange(result);
                        }
                        catch (Exception ex)
                        {
                            htmlString = "<p><h3>Something went wrong in google ads campaign.</h3></p>";
                            htmlArray.Add(htmlString);
                        }

                    }
                    else if (type == (int)ReportTypes.GoogleAdsGroups)
                    {
                        string htmlString = "";
                        pageNumber = pageNumber + 1;
                        try
                        {
                            var result = await ReplaceData(replaceData);
                            listOfResult.AddRange(result);
                            //htmlArray.AddRange(result);
                        }
                        catch (Exception)
                        {
                            htmlString = "<p><h3>Something went wrong in google ads groups.</h3></p>";
                            htmlArray.Add(htmlString);
                        }

                    }
                    else if (type == (int)ReportTypes.GoogleAdsCopies)
                    {
                        string htmlString = "";
                        pageNumber = pageNumber + 1;
                        try
                        {
                            var result = await ReplaceData(replaceData);
                            listOfResult.AddRange(result);
                            //htmlArray.AddRange(result);
                        }
                        catch (Exception ex)
                        {
                            htmlString = "<p><h3>Something went wrong in google ads copies.</h3></p>";
                            htmlArray.Add(htmlString);
                        }

                    }
                    else if (type == (int)ReportTypes.Facebook)
                    {
                        string htmlString = "";
                        pageNumber = pageNumber + 1;
                        try
                        {
                            //add all subtypes in ga4 sub type list
                            if (subtypes.Count == 0)
                            {
                                subtypes.AddRange(new List<string>
                                    {
                                        "34","35"
                                    });
                            };
                            replaceData.SubType = subtypes;
                            var result = await ReplaceData(replaceData);
                            listOfResult.AddRange(result);
                            //htmlArray.AddRange(result);
                        }
                        catch (Exception e)
                        {
                            htmlString = "<p><h3>Something went wrong in facebook.</h3></p>";
                            htmlArray.Add(htmlString);
                        }

                    }
                    else if (type == (int)ReportTypes.FacebookAdsCampaign)
                    {
                        string htmlString = "";
                        pageNumber = pageNumber + 1;
                        try
                        {
                            var result = await ReplaceData(replaceData);
                            listOfResult.AddRange(result);
                            //htmlArray.AddRange(result);
                        }
                        catch (Exception ex)
                        {
                            htmlString = "<p><h3>Something went wrong in facebook ads campaign.</h3></p>";
                            htmlArray.Add(htmlString);
                        }

                    }
                    else if (type == (int)ReportTypes.FacebookAdsGroup)
                    {
                        string htmlString = "";
                        pageNumber = pageNumber + 1;
                        try
                        {
                            var result = await ReplaceData(replaceData);
                            listOfResult.AddRange(result);
                            //htmlArray.AddRange(result);
                        }
                        catch (Exception)
                        {
                            htmlString = "<p><h3>Something went wrong in facebook ads groups.</h3></p>";
                            htmlArray.Add(htmlString);
                        }

                    }
                    else if (type == (int)ReportTypes.FacebookAdsCopies)
                    {
                        string htmlString = "";
                        pageNumber = pageNumber + 1;
                        try
                        {
                            var result = await ReplaceData(replaceData);
                            listOfResult.AddRange(result);
                            //htmlArray.AddRange(result);
                        }
                        catch (Exception)
                        {
                            htmlString = "<p><h3>Something went wrong in facebook ads copies.</h3></p>";
                            htmlArray.Add(htmlString);
                        }

                    }
                    else if (type == (int)ReportTypes.Instagram)
                    {
                        string htmlString = "";
                        pageNumber = pageNumber + 1;
                        try
                        {
                            //add all subtypes in ga4 sub type list
                            if (subtypes.Count == 0)
                            {
                                subtypes.AddRange(new List<string>
                                            {
                                                "36","37","38"
                                            });
                            };
                            replaceData.SubType = subtypes;
                            var result = await ReplaceData(replaceData);
                            listOfResult.AddRange(result);
                            //htmlArray.AddRange(result);
                        }
                        catch (Exception)
                        {
                            htmlString = "<p><h3>Something went wrong in instagram.</h3></p>";
                            htmlArray.Add(htmlString);
                        }

                    }
                    else if (type == (int)ReportTypes.LinkedInEngagement)
                    {
                        string htmlString = "";
                        pageNumber = pageNumber + 1;
                        try
                        {
                            var result = await ReplaceData(replaceData);
                            listOfResult.AddRange(result);
                            //htmlArray.AddRange(result);;
                        }
                        catch (Exception)
                        {
                            htmlString = "<p><h3>Something went wrong in linkedin engagement.</h3></p>";
                            htmlArray.Add(htmlString);
                        }

                    }
                    else if (type == (int)ReportTypes.LinkedInDemographic)
                    {
                        string htmlString = "";

                        pageNumber = pageNumber + 1;

                        try
                        {
                            var result = await ReplaceData(replaceData);
                            listOfResult.AddRange(result);
                            //htmlArray.AddRange(result);
                        }
                        catch (Exception ex)
                        {
                            htmlString = "<p><h3>Something went wrong in linkedin demographic.</h3></p>";
                            htmlArray.Add(htmlString);
                        }

                    }
                    else if (type == (int)ReportTypes.LinkedInAdsCampaign)
                    {
                        string htmlString = "";

                        pageNumber = pageNumber + 1;

                        try
                        {
                            var result = await ReplaceData(replaceData);
                            listOfResult.AddRange(result);
                            //htmlArray.AddRange(result);
                        }
                        catch (Exception ex)
                        {
                            htmlString = "<p><h3>Something went wrong in linkedin ad campaing.</h3></p>";
                            htmlArray.Add(htmlString);
                        }

                    }
                    else if (type == (int)ReportTypes.LinkedInAdsAdgroups)
                    {
                        string htmlString = "";

                        pageNumber = pageNumber + 1;

                        try
                        {
                            var result = await ReplaceData(replaceData);
                            listOfResult.AddRange(result);
                            //htmlArray.AddRange(result);
                        }
                        catch (Exception ex)
                        {
                            htmlString = "<p><h3>Something went wrong in linkedin ad group.</h3></p>";
                            htmlArray.Add(htmlString);
                        }

                    }
                    else if (type == (int)ReportTypes.LinkedInAdsCreative)
                    {
                        string htmlString = "";

                        pageNumber = pageNumber + 1;

                        try
                        {
                            var result = await ReplaceData(replaceData);
                            listOfResult.AddRange(result);
                            //htmlArray.AddRange(result);
                        }
                        catch (Exception ex)
                        {
                            htmlString = "<p><h3>Something went wrong in linkedin ad creative.</h3></p>";
                            htmlArray.Add(htmlString);
                        }

                    }
                    else if (type == (int)ReportTypes.Keywords)
                    {
                        string htmlString = "";
                        int offset = 40;
                        try
                        {
                            var keywordList = JsonConvert.DeserializeObject<List<RootKeywordsReportData>>(generateReportDto.ReportData.Keywords);
                            for (var i = 0; i < (float)keywordList.Count / offset; i++)
                            {
                                pageNumber = pageNumber + 1;
                                var newData = new List<RootKeywordsReportData>();
                                newData.AddRange(keywordList.Skip(i * offset).Take(offset));
                                generateReportDto.ReportData.Keywords = JsonConvert.SerializeObject(newData);

                                var result = await ReplaceData(replaceData);
                                listOfResult.AddRange(result);
                                //htmlArray.AddRange(result);
                            }
                        }
                        catch (Exception)
                        {
                            htmlString = "<p><h3>Something went wrong in keywords.</h3></p>";
                            htmlArray.Add(htmlString);
                        }
                    }
                    else if (type == (int)ReportTypes.GoogleAnalyticsFour)
                    {
                        string htmlString = "";
                        try
                        {
                            //add all subtypes in ga4 sub type list
                            if (subtypes.Count == 0)
                            {
                                subtypes.AddRange(new List<string>
                                    {
                                        "24", "25","26","27","28","29"
                                    });
                            }
                            replaceData.SubType = subtypes;
                            var result = await ReplaceData(replaceData);
                            listOfResult.AddRange(result);
                            //htmlArray.AddRange(result);
                        }
                        catch (Exception ex)
                        {
                            htmlString = "<p><h3>Something went wrong in google analytics 4.</h3></p>";
                            htmlArray.Add(htmlString);
                        }

                    }
                    else if (type == (int)ReportTypes.LightHouseData)
                    {
                        string htmlString = "";
                        pageNumber = pageNumber + 1;
                        try
                        {
                            var result = await ReplaceData(replaceData);
                            listOfResult.AddRange(result);
                            //htmlArray.AddRange(result);
                        }
                        catch (Exception ex)
                        {
                            htmlString = "<p><h3>Something went wrong in light house data.</h3></p>";
                            htmlArray.Add(htmlString);
                        }

                    }
                    else if (type == (int)ReportTypes.GoogleBusinessProfile)
                    {
                        string htmlString = "";
                        pageNumber = pageNumber + 1;
                        try
                        {
                            //add all subtypes in sub type list
                            if (subtypes.Count == 0)
                            {
                                subtypes.AddRange(new List<string>
                                    {
                                        "41", "42","43","44","45","46","47","48"
                                    });
                            }
                            var result = await ReplaceData(replaceData);
                            listOfResult.AddRange(result);
                            //htmlArray.AddRange(result);
                        }
                        catch (Exception ex)
                        {
                            htmlString = "<p><h3>Something went wrong in gbp data.</h3></p>";
                            htmlArray.Add(htmlString);
                        }

                    }
                    else if (type == (int)ReportTypes.WooCommerce)
                    {
                        string htmlString = "";
                        pageNumber = pageNumber + 1;
                        try
                        {
                            //add all subtypes in sub type list
                            if (subtypes.Count == 0)
                            {
                                subtypes.AddRange(new List<string>
                                    {
                                        "49", "50","51","52","53","54"
                                    });
                            }
                            replaceData.SubType = subtypes;
                            var result = await ReplaceData(replaceData);
                            listOfResult.AddRange(result);
                            //htmlArray.AddRange(result);
                        }
                        catch (Exception ex)
                        {
                            htmlString = "<p><h3>Something went wrong in woo commerce data.</h3></p>";
                            htmlArray.Add(htmlString);
                        }
                    }
                    else if (type == (int)ReportTypes.CallRail)
                    {
                        string htmlString = "";
                        pageNumber = pageNumber + 1;
                        try
                        {
                            //add all subtypes in sub type list
                            if (subtypes.Count == 0)
                            {
                                subtypes.AddRange(new List<string>
                                    {
                                        "63","64","65","66","67","68","69","70","71","72","73","74","75"
                                    });
                            }
                            replaceData.SubType = subtypes;
                            var result = await ReplaceData(replaceData);
                            listOfResult.AddRange(result);
                        }
                        catch (Exception ex)
                        {
                            htmlString = "<p><h3>Something went wrong in call rail.</h3></p>";
                            htmlArray.Add(htmlString);
                        }
                    }
                    else if (type == (int)ReportTypes.Mailchimp)
                    {
                        string htmlString = "";
                        pageNumber = pageNumber + 1;
                        try
                        {
                            //add all subtypes in sub type list
                            if (subtypes.Count == 0)
                            {
                                subtypes.AddRange(new List<string>
                                    {
                                         "77","78","79","80","81","83","84","85","86","87"
                                    });
                            }
                            replaceData.SubType = subtypes;
                            var result = await ReplaceData(replaceData);
                            listOfResult.AddRange(result);
                        }
                        catch (Exception ex)
                        {
                            htmlString = "<p><h3>Something went wrong in mail chimp.</h3></p>";
                            htmlArray.Add(htmlString);
                        }
                    }
                    else if (type == (int)ReportTypes.MicrosoftAds)
                    {
                        string htmlString = "";
                        pageNumber = pageNumber + 1;
                        try
                        {
                            //add all subtypes in sub type list
                            if (subtypes.Count == 0)
                            {
                                subtypes.AddRange(new List<string>
                                    {
                                          "93", "94", "95", "96", "98", "99", "100", "101", "103", "104", "105", "106" , "108", "109", "110", "111"
                                    });
                            }
                            replaceData.SubType = subtypes;
                            var result = await ReplaceData(replaceData);
                            listOfResult.AddRange(result);
                        }
                        catch (Exception ex)
                        {
                            htmlString = "<p><h3>Something went wrong in ms ads</h3></p>";
                            htmlArray.Add(htmlString);
                        }
                    }
                }
            }

            if (updatedReportTypeList.Where(x => x == "116").Any() && !string.IsNullOrEmpty(generateReportDto.ReportData.AiSummaryData))
            {
                string htmlString = string.Empty;
                Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/chatGpt.html");
                htmlString = System.IO.File.ReadAllText(pathGa);

                htmlString = htmlString.Replace("_innerChatGptData_", generateReportDto.ReportData.AiSummaryData);

                htmlArray.Add(htmlString);

            }

            int imageIndex = 0;

            int commentIndex = 0;

            foreach (var reportType in reportTypeList)
            {
                var res = ExtractTypeAndSubtypes(reportType);
                var uniqueKey = string.Empty;
                if (res.Subtype.Count > 0)
                {
                    uniqueKey = $"{res.Type}({string.Join(",", res.Subtype)})";
                }
                else
                {
                    uniqueKey = $"{res.Type}";
                }

                if (reportType != "19" && reportType != "20" && reportType != "55")
                {
                    // Check if the listOfResult list contains the uniqueKey
                    foreach (var dictionary in listOfResult)
                    {
                        if (dictionary.ContainsKey(uniqueKey))
                        {
                            // If there is a match, add the corresponding HTML data to the matchingHtmlArray
                            string htmlData = dictionary[uniqueKey];
                            htmlArray.Add(htmlData);
                        }
                    }
                }
                else
                {
                    if (reportType == "19")
                    {
                        string htmlString = "";
                        var commentList = generateReportDto?.Comments.Split("||");
                        //string pathText = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/text.html");
                        //htmlString =  System.IO.File.ReadAllText(pathText);

                        string pathText = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/text.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(pathText).Result;
                        }

                        htmlString = await PrepareTextAndImageHtml(commentList[commentIndex], htmlString, companyLogo, campaignLogo, headerText, footerText, headerTextColor, headerBgColor, generateReportDto.ReportGenerateSetting.ShowFooter, showPageNumberId, showPageNumber
                            , generateReportDto.ReportGenerateSetting.ShowHeader, pageNumber);

                        htmlArray.Add(htmlString);

                        commentIndex += 1;
                    }

                    if (reportType == "20")
                    {
                        string htmlString = "";
                        var imageList = generateReportDto?.Images.Split("||");
                        var imgObj = JsonConvert.DeserializeObject<ReportImageDto>(imageList[imageIndex]);

                        //string pathImage = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/image.html");
                        //htmlString =   System.IO.File.ReadAllText(pathImage) ;

                        string pathText = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/image.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(pathText).Result;
                        }
                        string data = "<div style=\"display: flex; justify-content: " + imgObj.align + ";\"><img src='" + imgObj.src + "' style=\"height:" + imgObj.height + "px; width:" + imgObj.width + "px;\"/></div>";
                        htmlString = await PrepareTextAndImageHtml(data, htmlString, companyLogo, campaignLogo, headerText, footerText, headerTextColor, headerBgColor, generateReportDto.ReportGenerateSetting.ShowFooter, showPageNumberId, showPageNumber
                            , generateReportDto.ReportGenerateSetting.ShowHeader, pageNumber);

                        htmlArray.Add(htmlString);
                        imageIndex += 1;
                    }

                    if (reportType == "55")
                    {
                        string htmlString = "";

                        var GoogleSheetData = JsonConvert.DeserializeObject<List<GoogleSheetData>>(generateReportDto.ReportData.GoogleSheetData);
                        if (generateReportDto.ReportData.GoogleSheetData != "null")
                        {


                            foreach (var chartData in GoogleSheetData)
                            {
                                switch (chartData.ReportSubType)
                                {
                                    //Pie Chart
                                    case 56:

                                        string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/googleSheetPie.html");
                                        using (HttpClient httpclient = new HttpClient())
                                        {
                                            htmlString = httpclient.GetStringAsync(pathPie).Result;
                                        }

                                        htmlString = htmlString.Replace("_gsChartId_", DateTimeOffset.Now.Ticks.ToString());


                                        List<string> randomColors = GenerateRandomColors(chartData.XAxis.Count);


                                        if (chartData.XAxis != null && chartData.XAxis.Count() > 0)
                                        {
                                            // Create a new list by selecting elements from the original list based on the count
                                            var selectedColors = string.Join(",", randomColors.Select(x => "'" + x + "'"));
                                            htmlString = htmlString.Replace("_gsBgColorPieChart_", selectedColors);
                                        }

                                        var labelsAndData = chartData.XAxis.Zip(chartData.YAxis, (label, data) => $"'{label}: {data}'");

                                        var labels = string.Join(",", labelsAndData);

                                        var data = string.Join(",", chartData.YAxis != null ? chartData.YAxis : new List<decimal?>() { });

                                        htmlString = htmlString.Replace("_gsPieChartData_", data);
                                        htmlString = htmlString.Replace("_gsPieChartLabel_", labels);
                                        htmlString = htmlString.Replace("_gsTitle_", !string.IsNullOrEmpty(chartData.Title) ? chartData.Title : "Pie Chart");

                                        if (chartData.Aggregator.ToUpper() == "SUM")
                                        {
                                            htmlString = htmlString.Replace("_gsAggeragatorValue_", "Sum: " + chartData.AggregationData);
                                        }
                                        else
                                        {
                                            htmlString = htmlString.Replace("_gsAggeragatorValue_", "Avg: " + chartData.AggregationData);
                                        }

                                        htmlArray.Add(htmlString);

                                        break;

                                    case 57:
                                        //bar charrt
                                        string pathBar = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/googleSheetBar.html");
                                        using (HttpClient httpclient = new HttpClient())
                                        {
                                            htmlString = httpclient.GetStringAsync(pathBar).Result;
                                        }

                                        htmlString = htmlString.Replace("_gsChartId_", DateTimeOffset.Now.Ticks.ToString());


                                        var barLabels = chartData.XAxis.Select(x => "'" + x + "'");

                                        var labels1 = string.Join(",", barLabels);

                                        var data1 = string.Join(",", chartData.YAxis != null ? chartData.YAxis : new List<decimal?>() { });

                                        htmlString = htmlString.Replace("_gsBarData1_", data1);
                                        htmlString = htmlString.Replace("_gsBarLabels_", labels1);
                                        htmlString = htmlString.Replace("_gsBarChartTitle_", !string.IsNullOrEmpty(chartData.Title) ? chartData.Title : "Bar Chart");

                                        if (chartData.IsComparePrevious)
                                        {

                                            var prevBarData = string.Join(",", chartData.PrevYAxis != null ? chartData.PrevYAxis : new List<decimal?>() { });

                                            htmlString = htmlString.Replace("_gsBarData2_", prevBarData);

                                            htmlString = htmlString.Replace("_gsBarData2_", "");
                                            if (chartData.Aggregator.ToUpper() == "SUM")
                                            {
                                                htmlString = htmlString.Replace("_gsBarAggeragatorValue_", "Sum: " + chartData.DiffAggregator);
                                            }
                                            else
                                            {
                                                htmlString = htmlString.Replace("_gsBarAggeragatorValue_", "Avg: " + chartData.DiffAggregator);
                                            }

                                            htmlString = htmlString.Replace("_IsComparePrevious_", "false");
                                        }
                                        else
                                        {
                                            htmlString = htmlString.Replace("_gsBarData2_", "");
                                            if (chartData.Aggregator.ToUpper() == "SUM")
                                            {
                                                htmlString = htmlString.Replace("_gsBarAggeragatorValue_", "Sum: " + chartData.AggregationData);
                                            }
                                            else
                                            {
                                                htmlString = htmlString.Replace("_gsBarAggeragatorValue_", "Avg: " + chartData.AggregationData);
                                            }

                                            htmlString = htmlString.Replace("_IsComparePrevious_", "true");
                                        }

                                        htmlArray.Add(htmlString);

                                        break;

                                    case 58:

                                        string pathLine = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/googleSheetLine.html");
                                        using (HttpClient httpclient = new HttpClient())
                                        {
                                            htmlString = httpclient.GetStringAsync(pathLine).Result;
                                        }

                                        htmlString = htmlString.Replace("_gsChartId_", DateTimeOffset.Now.Ticks.ToString());


                                        var lineLabels = chartData.XAxis.Select(x => "'" + x + "'").ToList();

                                        var labels2 = string.Join(",", lineLabels);

                                        var data2 = string.Join(",", chartData.YAxis != null ? chartData.YAxis : new List<decimal?>() { });

                                        htmlString = htmlString.Replace("_gsLineData1_", data2);
                                        htmlString = htmlString.Replace("_gsLineLabels_", labels2);
                                        htmlString = htmlString.Replace("_gsLineChartTitle_", !string.IsNullOrEmpty(chartData.Title) ? chartData.Title : "Line Chart");

                                        int intervalRes = lineLabels.Count() <= 31 ? 3 : (lineLabels.Count() <= 91 ? 7 : 31);
                                        htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

                                        if (chartData.IsComparePrevious)
                                        {

                                            var prevData = string.Join(",", chartData.PrevYAxis != null ? chartData.PrevYAxis : new List<decimal?>() { });

                                            htmlString = htmlString.Replace("_gsLineData2_", prevData);

                                            htmlString = htmlString.Replace("_gsLineData2_", "");
                                            if (chartData.Aggregator.ToUpper() == "SUM")
                                            {
                                                htmlString = htmlString.Replace("_gsLineAggeragatorValue_", "Sum: " + chartData.DiffAggregator);
                                            }
                                            else
                                            {
                                                htmlString = htmlString.Replace("_gsLineAggeragatorValue_", "Avg: " + chartData.DiffAggregator);
                                            }

                                            htmlString = htmlString.Replace("_IsComparePrevious_", "false");
                                        }
                                        else
                                        {
                                            htmlString = htmlString.Replace("_gsLineData2_", "");
                                            if (chartData.Aggregator.ToUpper() == "SUM")
                                            {
                                                htmlString = htmlString.Replace("_gsLineAggeragatorValue_", "Sum: " + chartData.AggregationData);
                                            }
                                            else
                                            {
                                                htmlString = htmlString.Replace("_gsLineAggeragatorValue_", "Avg: " + chartData.AggregationData);
                                            }

                                            htmlString = htmlString.Replace("_IsComparePrevious_", "true");
                                        }

                                        htmlArray.Add(htmlString);

                                        break;

                                    //Table
                                    case 59:

                                        string pathTable = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/googleSheetTable.html");
                                        using (HttpClient httpclient = new HttpClient())
                                        {
                                            htmlString = httpclient.GetStringAsync(pathTable).Result;
                                        }

                                        var tableData = chartData.TableData;

                                        var tableString = JsonConvert.SerializeObject(tableData);

                                        htmlString = htmlString.Replace("_gsDataTable1_", tableString);

                                        htmlString = htmlString.Replace("_gsTableTitle_", chartData.Title);

                                        htmlArray.Add(htmlString);
                                        break;

                                    //Stat Cell Value
                                    case 60:

                                        string pathCell = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/googleSheetCell.html");
                                        using (HttpClient httpclient = new HttpClient())
                                        {
                                            htmlString = httpclient.GetStringAsync(pathCell).Result;
                                        }

                                        var cellData = chartData.CellData;

                                        htmlString = htmlString.Replace("_gsCellData_", cellData);

                                        htmlString = htmlString.Replace("_gsCellTitle_", chartData.Title);

                                        htmlArray.Add(htmlString);

                                        break;

                                    //SparkLine Chart
                                    case 61:

                                        string pathSpark = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/googleSheetSpark.html");
                                        using (HttpClient httpclient = new HttpClient())
                                        {
                                            htmlString = httpclient.GetStringAsync(pathSpark).Result;
                                        }

                                        htmlString = htmlString.Replace("_gsChartId_", DateTimeOffset.Now.Ticks.ToString());


                                        var sparkLabels = chartData.XAxis.Select(x => "'" + x + "'");

                                        var sparkLabelsString = string.Join(",", sparkLabels);

                                        var sparkData = string.Join(",", chartData.YAxis != null ? chartData.YAxis : new List<decimal?>() { });

                                        htmlString = htmlString.Replace("_gsSparkData1_", sparkData);
                                        htmlString = htmlString.Replace("_gsSparkLabels_", sparkLabelsString);
                                        htmlString = htmlString.Replace("_gsSparkChartTitle_", !string.IsNullOrEmpty(chartData.Title) ? chartData.Title : "Spark Line Chart");

                                        if (chartData.IsComparePrevious)
                                        {

                                            var prevData = string.Join(",", chartData.PrevYAxis != null ? chartData.PrevYAxis : new List<decimal?>() { });

                                            htmlString = htmlString.Replace("_gsSparkData2_", prevData);

                                            htmlString = htmlString.Replace("_gsSparkData2_", "");
                                            if (chartData.Aggregator.ToUpper() == "SUM")
                                            {
                                                htmlString = htmlString.Replace("_gsSparkAggeragatorValue_", "Sum: " + chartData.DiffAggregator);
                                            }
                                            else
                                            {
                                                htmlString = htmlString.Replace("_gsSparkAggeragatorValue_", "Avg: " + chartData.DiffAggregator);
                                            }

                                            htmlString = htmlString.Replace("_IsComparePrevious_", "false");
                                        }
                                        else
                                        {
                                            htmlString = htmlString.Replace("_gsSparkData2_", "");
                                            if (chartData.Aggregator.ToUpper() == "SUM")
                                            {
                                                htmlString = htmlString.Replace("_gsSparkAggeragatorValue_", "Sum: " + chartData.AggregationData);
                                            }
                                            else
                                            {
                                                htmlString = htmlString.Replace("_gsSparkAggeragatorValue_", "Avg: " + chartData.AggregationData);
                                            }

                                            htmlString = htmlString.Replace("_IsComparePrevious_", "true");
                                        }

                                        htmlArray.Add(htmlString);
                                        break;

                                    default:


                                        break;
                                }
                            }
                        }
                    }
                }
            }

            var startDate = startDateForEmailReport.ToString("ddd, MMMM dd yyyy");
            var endDate = endDateForEmailReport.ToString("ddd, MMMM dd yyyy");
            var customFont = String.Empty;
            for (var i = 0; i < htmlArray.Count; i++)
            {
                htmlArray[i] = htmlArray[i].Replace("_companyName_", generateReportDto?.CompanyName);
                htmlArray[i] = htmlArray[i].Replace("_campaignName_", generateReportDto?.CampaignName);


                htmlArray[i] = htmlArray[i].Replace("_reportDateRange_", startDate + " - " + endDate);

                // theme setting
                string bgClr = generateReportDto.ReportGenerateSetting.ThemeBgColor;
                string txtClr = generateReportDto.ReportGenerateSetting.ThemeTextColor;
                string txtFont = generateReportDto.ReportGenerateSetting.Font;
                string themeBgColor = String.IsNullOrEmpty(bgClr) ? "#3445f6" : bgClr;
                string themeTextColor = String.IsNullOrEmpty(txtClr) ? "#ffffff" : txtClr;
                customFont = String.IsNullOrEmpty(txtFont) ? "Montserrat" : txtFont;

                htmlArray[i] = htmlArray[i].Replace("_themeBgColor_", themeBgColor);
                htmlArray[i] = htmlArray[i].Replace("_themeTextColor_", themeTextColor);
                htmlArray[i] = htmlArray[i].Replace("_font_", customFont);
            }

            var mergedHtml = string.Join(" ", htmlArray);

            //Prepare Header and Footer            
            var header = "<style>html { -webkit-print-color-adjust: exact;}</style><div style=\"margin-top: -15px;font-size:8px;height: 100%; width:100%; padding: 0px 20px; display: flex; flex-direction: row; align-items: center; justify-content: space-between;background-color: " + headerBgColor + "; color: " + headerTextColor + "; font-family: " + customFont + ";height: 25px;\"><span>" + generateReportDto?.CampaignName + "</span><span>" + startDate + " - " + endDate + "</span></div>";
            var footer = "<div style=\"margin-bottom: -15px;font-size: 8px; height: 25px; width: 100%; padding: 0px 20px; display: flex; align-items: center; justify-content: center; background-color: " + headerBgColor + "; color: " + headerTextColor + ";font-family:" + customFont + ";\"><span>Prepared by " + generateReportDto?.CompanyName + "</span></div>";

            var options1 = new ChromeHtmlToPdfOptions
            {
                Delay = 3000,
                DisplayHeaderFooter = true,
                Landscape = false,
                MarginBottom = ".6in",
                MarginTop = ".6in",
                Width = "8.27in",
                Height = "11.69in",
                HeaderTemplate = header,
                FooterTemplate = footer
            };

            var request1 = new ChromeHtmlToPdfRequest
            {
                Html = mergedHtml,
                FileName = "sample.pdf",
                Inline = true,
                Options = options1,
            };
            var apiResponse1 = a2pClient.Chrome.HtmlToPdf(request1);

            return apiResponse1.FileUrl;
        }

        // Helper method to extract type and subtypes from the reportType
        public TypeSubType ExtractTypeAndSubtypes(string reportType)
        {
            int type = 0;
            List<string> subtypes = new List<string>();
            TypeSubType typeSubType = new TypeSubType();

            if (reportType.Contains("(") && reportType.Contains(")"))
            {
                Match match = Regex.Match(reportType, @"(\d+)\(([^)]+)\)");
                if (match.Success)
                {
                    string report_type = match.Groups[1].Value;
                    string subTypeList = match.Groups[2].Value;

                    type = Convert.ToInt16(report_type);
                    string[] subTypes = subTypeList.Split(',');
                    subtypes.AddRange(subTypes);
                }
            }
            else
            {
                type = Convert.ToInt16(reportType);
            }

            typeSubType.Type = type;
            typeSubType.Subtype = subtypes;

            return typeSubType;
        }


        public async Task<PrepareGbpRootData> PrepareGbpReport(Guid campaignId, DateTime startDate, DateTime endDate, List<string> subtypes)
        {
            var retVal = new PrepareGbpRootData();

            List<Dictionary<string, string>> listOfResult = new List<Dictionary<string, string>>();

            if (campaignId != null)
            {
                var gbpData = await _campaignGBPService.GetGbpPerformanceData(campaignId, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

                foreach (var subtype in subtypes)
                {
                    var intsubtype = Convert.ToInt16(subtype);
                    if (intsubtype == (int)ReportTypes.GBPSearches)
                    {
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                        //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/gbpSearches.html");
                        //htmlString = System.IO.File.ReadAllText(pathGa);

                        string htmlString = string.Empty;
                        string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/gbpSearches.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(pathGa).Result;
                        }

                        var dateLabelStr = String.Join(",", gbpData.DateLabels.Select(x => "'" + x + "'"));

                        htmlString = htmlString.Replace("_1gbpInterChartLabels_", dateLabelStr);

                        // serialize object
                        var tableString = JsonConvert.SerializeObject(gbpData.KeywordData);
                        htmlString = htmlString.Replace("_gbpKeywordArrayList_", tableString);

                        int intervalRes = gbpData.DateLabels.Count <= 31 ? 3 : (gbpData.DateLabels.Count <= 91 ? 7 : 31);
                        htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

                        //Search keyword diff
                        htmlString = htmlString.Replace("_searchKeywordDiff_", gbpData.SearchKeywordDiff);

                        var hasPlusSign8 = gbpData.SearchKeywordDiff.Contains("+");
                        if (hasPlusSign8)
                        {
                            htmlString = htmlString.Replace("_searchKeywordDiffColor_", "green");
                        }
                        else
                        {
                            htmlString = htmlString.Replace("_searchKeywordDiffColor_", "red");
                        }

                        htmlString = htmlString.Replace("_vsDateRange_", gbpData.VsDateRange.ToString());

                        string uniqueKey = $"{39}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);
                    }
                    else if (intsubtype == (int)ReportTypes.GBPBookings)
                    {
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                        //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/gbpBookings.html");
                        //htmlString = System.IO.File.ReadAllText(pathGa);

                        string htmlString = string.Empty;
                        string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/gbpBookings.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(pathGa).Result;
                        }

                        var dateLabelStr = String.Join(",", gbpData.DateLabels.Select(x => "'" + x + "'"));

                        var bookingChart = String.Join(",", gbpData.BookingChartData);

                        htmlString = htmlString.Replace("_1gbpInterChartLabels_", dateLabelStr);

                        htmlString = htmlString.Replace("_1gbpBookingChartData_", bookingChart);

                        int intervalRes = gbpData.DateLabels.Count <= 31 ? 3 : (gbpData.DateLabels.Count <= 91 ? 7 : 31);
                        htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

                        //Difference
                        //Booking
                        htmlString = htmlString.Replace("_bookingsDiff_", gbpData.BookingDiff);

                        var hasPlusSign5 = gbpData.BookingDiff.Contains("+");
                        if (hasPlusSign5)
                        {
                            htmlString = htmlString.Replace("_bookingsDiffColor_", "green");
                        }
                        else
                        {
                            htmlString = htmlString.Replace("_bookingsDiffColor_", "red");
                        }

                        htmlString = htmlString.Replace("_vsDateRange_", gbpData.VsDateRange.ToString());

                        string uniqueKey = $"{39}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);
                    }
                    else if (intsubtype == (int)ReportTypes.GBPCalls)
                    {
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                        //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/gbpCalls.html");
                        //htmlString = System.IO.File.ReadAllText(pathGa);

                        string htmlString = string.Empty;
                        string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/gbpCalls.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(pathGa).Result;
                        }

                        var dateLabelStr = String.Join(",", gbpData.DateLabels.Select(x => "'" + x + "'"));

                        var callChart = String.Join(",", gbpData.CallChartData);

                        htmlString = htmlString.Replace("_1gbpInterChartLabels_", dateLabelStr);

                        htmlString = htmlString.Replace("_1gbpCallChartData_", callChart);

                        int intervalRes = gbpData.DateLabels.Count <= 31 ? 3 : (gbpData.DateLabels.Count <= 91 ? 7 : 31);
                        htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

                        //Difference                                      
                        //Call

                        htmlString = htmlString.Replace("_callDiff_", gbpData.CallDiff);

                        var hasPlusSign3 = gbpData.CallDiff.Contains("+");
                        if (hasPlusSign3)
                        {
                            htmlString = htmlString.Replace("_callDiffColor_", "green");
                        }
                        else
                        {
                            htmlString = htmlString.Replace("_callDiffColor_", "red");
                        }

                        htmlString = htmlString.Replace("_vsDateRange_", gbpData.VsDateRange.ToString());

                        string uniqueKey = $"{39}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);
                    }
                    else if (intsubtype == (int)ReportTypes.GBPDirections)
                    {
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                        //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/gbpDirections.html");
                        //htmlString = System.IO.File.ReadAllText(pathGa);

                        string htmlString = string.Empty;
                        string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/gbpDirections.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(pathGa).Result;
                        }

                        var dateLabelStr = String.Join(",", gbpData.DateLabels.Select(x => "'" + x + "'"));

                        var directionChart = String.Join(",", gbpData.DirectionChartData);

                        htmlString = htmlString.Replace("_1gbpInterChartLabels_", dateLabelStr);


                        htmlString = htmlString.Replace("_1gbpDirectionChartData_", directionChart);

                        int intervalRes = gbpData.DateLabels.Count <= 31 ? 3 : (gbpData.DateLabels.Count <= 91 ? 7 : 31);
                        htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

                        //Difference
                        //Direction

                        htmlString = htmlString.Replace("_directionDiff_", gbpData.DirectionDiff);

                        var hasPlusSign6 = gbpData.DirectionDiff.Contains("+");
                        if (hasPlusSign6)
                        {
                            htmlString = htmlString.Replace("_directionDiffColor_", "green");
                        }
                        else
                        {
                            htmlString = htmlString.Replace("_directionDiffColor_", "red");
                        }

                        htmlString = htmlString.Replace("_vsDateRange_", gbpData.VsDateRange.ToString());

                        string uniqueKey = $"{39}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);
                    }
                    else if (intsubtype == (int)ReportTypes.GBPInteraction)
                    {
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                        //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/gbpInteraction.html");
                        //htmlString = System.IO.File.ReadAllText(pathGa);

                        string htmlString = string.Empty;
                        string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/gbpInteraction.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(pathGa).Result;
                        }

                        var dateLabelStr = String.Join(",", gbpData.DateLabels.Select(x => "'" + x + "'"));

                        var ineractionChart = String.Join(",", gbpData.InteractionChartData);

                        htmlString = htmlString.Replace("_1gbpInterChartLabels_", dateLabelStr);

                        htmlString = htmlString.Replace("_1gbpInterChartData_", ineractionChart);

                        int intervalRes = gbpData.DateLabels.Count <= 31 ? 3 : (gbpData.DateLabels.Count <= 91 ? 7 : 31);
                        htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

                        //Difference
                        //Profile Interaction

                        htmlString = htmlString.Replace("_profileInterDiff_", gbpData.ProfileInteractionDiff);

                        var hasPlusSign2 = gbpData.ProfileInteractionDiff.Contains("+");
                        if (hasPlusSign2)
                        {
                            htmlString = htmlString.Replace("_profileInterDiffColor_", "green");
                        }
                        else
                        {
                            htmlString = htmlString.Replace("_profileInterDiffColor_", "red");
                        }

                        htmlString = htmlString.Replace("_vsDateRange_", gbpData.VsDateRange.ToString());

                        string uniqueKey = $"{39}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);
                    }
                    else if (intsubtype == (int)ReportTypes.GBPMessages)
                    {
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                        //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/gbpMessages.html");
                        //htmlString = System.IO.File.ReadAllText(pathGa);

                        string htmlString = string.Empty;
                        string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/gbpMessages.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(pathGa).Result;
                        }

                        var dateLabelStr = String.Join(",", gbpData.DateLabels.Select(x => "'" + x + "'"));

                        var messageChart = String.Join(",", gbpData.MessageChartData);

                        htmlString = htmlString.Replace("_1gbpInterChartLabels_", dateLabelStr);

                        htmlString = htmlString.Replace("_1gbpMessageChartData_", messageChart);

                        int intervalRes = gbpData.DateLabels.Count <= 31 ? 3 : (gbpData.DateLabels.Count <= 91 ? 7 : 31);
                        htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

                        //Difference

                        //Message

                        htmlString = htmlString.Replace("_messageDiff_", gbpData.MessageDiff);

                        var hasPlusSign4 = gbpData.MessageDiff.Contains("+");
                        if (hasPlusSign4)
                        {
                            htmlString = htmlString.Replace("_messageColor_", "green");
                        }
                        else
                        {
                            htmlString = htmlString.Replace("_messageColor_", "red");
                        }

                        htmlString = htmlString.Replace("_vsDateRange_", gbpData.VsDateRange.ToString());

                        string uniqueKey = $"{39}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);
                    }
                    else if (intsubtype == (int)ReportTypes.GBPWebsiteClicks)
                    {
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                        //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/gbpWebsiteClicks.html");
                        //htmlString = System.IO.File.ReadAllText(pathGa);

                        string htmlString = string.Empty;
                        string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/gbpWebsiteClicks.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(pathGa).Result;
                        }

                        var dateLabelStr = String.Join(",", gbpData.DateLabels.Select(x => "'" + x + "'"));


                        var websiteChart = String.Join(",", gbpData.InteractionChartData);

                        htmlString = htmlString.Replace("_1gbpInterChartLabels_", dateLabelStr);

                        htmlString = htmlString.Replace("_1gbpWebsiteChartData_", websiteChart);

                        int intervalRes = gbpData.DateLabels.Count <= 31 ? 3 : (gbpData.DateLabels.Count <= 91 ? 7 : 31);
                        htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

                        //Difference                        
                        //Website Click

                        htmlString = htmlString.Replace("_websiteClicksDiff_", gbpData.CallDiff);

                        var hasPlusSign7 = gbpData.CallDiff.Contains("+");
                        if (hasPlusSign7)
                        {
                            htmlString = htmlString.Replace("_websiteDiffColor_", "green");
                        }
                        else
                        {
                            htmlString = htmlString.Replace("_websiteDiffColor_", "red");
                        }

                        htmlString = htmlString.Replace("_vsDateRange_", gbpData.VsDateRange.ToString());

                        string uniqueKey = $"{39}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);
                    }
                    else if (intsubtype == (int)ReportTypes.GBPViews)
                    {
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                        //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/gbpViews.html");
                        //htmlString = System.IO.File.ReadAllText(pathGa);

                        string htmlString = string.Empty;
                        string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/gbpViews.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(pathGa).Result;
                        }

                        htmlString = htmlString.Replace("_gSearchMobileData_", " " + gbpData.PercentGoogleSearchMobile.ToString() + "%");
                        htmlString = htmlString.Replace("_gMapMobileData_", " " + gbpData.PercentGoogleMapMobile.ToString() + "%");
                        htmlString = htmlString.Replace("_gSearchDesktopData_", " " + gbpData.PercentGoogleSearchDesktop.ToString() + "%");
                        htmlString = htmlString.Replace("_gMapDesktopData_", " " + gbpData.PercentGoogleMapDesktop.ToString() + "%");

                        htmlString = htmlString.Replace("_gMapDesktopData_", gbpData.PercentGoogleMapDesktop.ToString());

                        var dateLabelStr = String.Join(",", gbpData.DateLabels.Select(x => "'" + x + "'"));

                        htmlString = htmlString.Replace("_1gbpInterChartLabels_", dateLabelStr);

                        htmlString = htmlString.Replace("_gbpPieChartData_", gbpData.TotalSearchMobile
                            + "," + gbpData.TotalMapMobile + "," + gbpData.TotalSearchDesktop +
                            "," + gbpData.TotalMapDesktop);

                        int intervalRes = gbpData.DateLabels.Count <= 31 ? 3 : (gbpData.DateLabels.Count <= 91 ? 7 : 31);
                        htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

                        //Difference
                        //Profile View
                        htmlString = htmlString.Replace("_profileViewDiff_", gbpData.ProfileViewDiff);

                        var hasPlusSign1 = gbpData.ProfileViewDiff.Contains("+");
                        if (hasPlusSign1)
                        {
                            htmlString = htmlString.Replace("_profileViewDiffColor_", "green");
                        }
                        else
                        {
                            htmlString = htmlString.Replace("_profileViewDiffColor_", "red");
                        }

                        htmlString = htmlString.Replace("_vsDateRange_", gbpData.VsDateRange.ToString());

                        string uniqueKey = $"{39}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);
                    }
                }

                retVal.HtmlList = listOfResult;

                retVal.RootGbpData = gbpData;

            }

            return retVal;
        }

        public async Task<PrepareWcData> PrepareWooCommerceReport(Guid campaignId, DateTime startDate, DateTime endDate, List<string> subtypes)
        {
            var retVal = new PrepareWcData();
            retVal.WcRawData = new WcRawData();

            List<Dictionary<string, string>> listOfResult = new List<Dictionary<string, string>>();

            if (campaignId != null)
            {
                // var wcData = await _campaignWooCommerceService.GetWcReports(campaignId, "2023-08-01", "2023-08-31");
                var wcData = await _campaignWooCommerceService.GetWcReports(campaignId, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

                if (wcData != null)
                {
                    foreach (var subtype in subtypes)
                    {
                        var intsubtype = Convert.ToInt16(subtype);
                        if (intsubtype == (int)ReportTypes.WCOrders)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/wcOrders.html");
                            //htmlString = System.IO.File.ReadAllText(pathGa);

                            string htmlString = string.Empty;
                            string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/wcOrders.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathGa).Result;
                            }

                            var dateLabelStr = String.Join(",", wcData.DateLabel.Select(x => "'" + x + "'"));

                            var orderChart = String.Join(",", wcData.OrdersChartData);

                            htmlString = htmlString.Replace("_1wcDateChartLabels_", dateLabelStr);

                            htmlString = htmlString.Replace("_1wcOrderChartData_", orderChart);

                            int intervalRes = wcData.DateLabel.Count <= 31 ? 3 : (wcData.DateLabel.Count <= 91 ? 7 : 31);
                            htmlString = htmlString.Replace("_wcLabelInterval_", intervalRes.ToString());


                            //Difference
                            //Order chart
                            htmlString = htmlString.Replace("_1wcOrderChartDiffData_", wcData.OrdersChartDiff);

                            var hasPlusSign1 = wcData.OrdersChartDiff.Contains("+");
                            if (hasPlusSign1)
                            {
                                htmlString = htmlString.Replace("_1wcOrderChartDiffColor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_1wcOrderChartDiffColor_", "red");
                            }

                            string uniqueKey = $"{40}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);

                            retVal.WcRawData.Orders = wcData.OrdersChartDiff;
                        }
                        else if (intsubtype == (int)ReportTypes.WCLocationChart)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/wcLocationChart.html");
                            //htmlString = System.IO.File.ReadAllText(pathGa);

                            string htmlString = string.Empty;
                            string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/wcLocationChart.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathGa).Result;
                            }

                            List<string> originalColors = new List<string>
                            {
                                "rgb(242, 153, 0)",
                                "rgb(71, 71, 235)",
                                "rgb(217, 48, 37)",
                                "rgb(0, 100, 0)",
                                "rgb(255, 0, 127)"
                            };

                            if (wcData.LocationChartData != null && wcData.LocationChartData.Count() > 0)
                            {
                                // Limit the count to 5 if it's greater than 5
                                var bgColorCount = Math.Min(wcData.LocationChartData.Count(), 5);

                                // Create a new list by selecting elements from the original list based on the count
                                var selectedColors = string.Join(",", originalColors.GetRange(0, bgColorCount).Select(x => "'" + x + "'"));
                                htmlString = htmlString.Replace("_wcBgColorPieChart_", selectedColors);

                            }

                            var dateLabelStr = String.Join(",", wcData.DateLabel.Select(x => "'" + x + "'"));

                            htmlString = htmlString.Replace("_1wcDateChartLabels_", dateLabelStr);

                            var LocationChartLabel = String.Join(",", wcData.Locationdata.Select(x => "'" + x.Key + " " + x.Value + "'"));

                            var LocationChart = String.Join(",", wcData.LocationChartData != null ? wcData.LocationChartData : new double[] { });

                            htmlString = htmlString.Replace("_wcPieChartData_", LocationChart);
                            htmlString = htmlString.Replace("_wcPieChartLabel_", LocationChartLabel);

                            int intervalRes = wcData.DateLabel.Count <= 31 ? 3 : (wcData.DateLabel.Count <= 91 ? 7 : 31);
                            htmlString = htmlString.Replace("_wcLabelInterval_", intervalRes.ToString());

                            string uniqueKey = $"{40}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);

                            retVal.WcRawData.HighestOrderLocation = wcData.Locationdata.Count > 0 ? wcData.Locationdata.FirstOrDefault().Key + " " + wcData.Locationdata.FirstOrDefault().Value : "";
                        }
                        else if (intsubtype == (int)ReportTypes.WCReturnCustomer)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/wcReturnCustomer.html");
                            //htmlString = System.IO.File.ReadAllText(pathGa);

                            string htmlString = string.Empty;
                            string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/wcReturnCustomer.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathGa).Result;
                            }

                            var dateLabelStr = String.Join(",", wcData.DateLabel.Select(x => "'" + x + "'"));

                            var returnCustomerChart = String.Join(",", wcData.ReturningCustomerChartRate);

                            htmlString = htmlString.Replace("_1wcDateChartLabels_", dateLabelStr);

                            htmlString = htmlString.Replace("_1wcReturnChartData_", returnCustomerChart);

                            htmlString = htmlString.Replace("_wcCurrency_", "'" + wcData.Currency + "'");

                            int intervalRes = wcData.DateLabel.Count <= 31 ? 3 : (wcData.DateLabel.Count <= 91 ? 7 : 31);
                            htmlString = htmlString.Replace("_wcLabelInterval_", intervalRes.ToString());


                            //Difference
                            //Returning Chart

                            htmlString = htmlString.Replace("_1wcReturnChartDiffData_", wcData.ReturningChartRateDiff);

                            var hasPlusSign2 = wcData.ReturningChartRateDiff.Contains("+");
                            if (hasPlusSign2)
                            {
                                htmlString = htmlString.Replace("_1wcReturnChartDiffColor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_1wcReturnChartDiffColor_", "red");
                            }

                            string uniqueKey = $"{40}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);

                            retVal.WcRawData.ReturningCustomerRate = wcData.ReturningChartRateDiff;
                        }
                        else if (intsubtype == (int)ReportTypes.WCRevenueTable)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/wcRevenueTable.html");
                            //htmlString = System.IO.File.ReadAllText(pathGa);

                            string htmlString = string.Empty;
                            string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/wcRevenueTable.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathGa).Result;
                            }

                            // serialize object
                            var tableString = JsonConvert.SerializeObject(wcData.ProductSold);

                            htmlString = htmlString.Replace("_wcProductDataTable1_", tableString);

                            htmlString = htmlString.Replace("_wcCurrency_", "'" + wcData.Currency + "'");

                            int intervalRes = wcData.DateLabel.Count <= 31 ? 3 : (wcData.DateLabel.Count <= 91 ? 7 : 31);
                            htmlString = htmlString.Replace("_wcLabelInterval_", intervalRes.ToString());

                            string uniqueKey = $"{40}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);

                            retVal.WcRawData.HighestRevenuePerProduct = wcData.ProductSold.Count > 0 ? wcData.ProductSold.Max(x => x.name).ToString()  +" "+ wcData.ProductSold.Max(x=>x.total_revenue_per_product).ToString() : "";
                        }
                        else if (intsubtype == (int)ReportTypes.WCSales)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/wcSales.html");
                            //htmlString = System.IO.File.ReadAllText(pathGa);

                            string htmlString = string.Empty;
                            string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/wcSales.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathGa).Result;
                            }

                            var dateLabelStr = String.Join(",", wcData.DateLabel.Select(x => "'" + x + "'"));

                            var salesChart = String.Join(",", wcData.SalesChartData);

                            htmlString = htmlString.Replace("_1wcDateChartLabels_", dateLabelStr);

                            htmlString = htmlString.Replace("_1wcSalesChartData_", salesChart);

                            int intervalRes = wcData.DateLabel.Count <= 31 ? 3 : (wcData.DateLabel.Count <= 91 ? 7 : 31);
                            htmlString = htmlString.Replace("_wcLabelInterval_", intervalRes.ToString());


                            //Difference                    
                            //Sales Chart
                            htmlString = htmlString.Replace("_1wcSalesChartDiffData_", wcData.Currency + wcData.SalesChartDiff);

                            var hasPlusSign8 = wcData.SalesChartDiff.Contains("+");
                            if (hasPlusSign8)
                            {
                                htmlString = htmlString.Replace("_1wcSalesChartDiffColor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_1wcSalesChartDiffColor_", "red");
                            }

                            string uniqueKey = $"{40}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);

                            retVal.WcRawData.Sales = wcData.SalesChartDiff;

                        }
                        else if (intsubtype == (int)ReportTypes.WCSalesTable)
                        {
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                            //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/wcSalesTable.html");
                            //htmlString = System.IO.File.ReadAllText(pathGa);

                            string htmlString = string.Empty;
                            string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/wcSalesTable.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathGa).Result;
                            }

                            // serialize object
                            var tableString = JsonConvert.SerializeObject(wcData.ProductSold);

                            htmlString = htmlString.Replace("_wcProductDataTable1_", tableString);

                            htmlString = htmlString.Replace("_wcCurrency_", "'" + wcData.Currency + "'");

                            int intervalRes = wcData.DateLabel.Count <= 31 ? 3 : (wcData.DateLabel.Count <= 91 ? 7 : 31);
                            htmlString = htmlString.Replace("_wcLabelInterval_", intervalRes.ToString());

                            string uniqueKey = $"{40}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);

                            retVal.WcRawData.HighestSalePerProduct = wcData.ProductSold.Count > 0  ? wcData.ProductSold.Max(x => x.name).ToString()  + " " +wcData.ProductSold.Max(x => x.quantity).ToString() :  "";
                        }
                    }
                    retVal.WcRawData.AvgOrderValue = wcData.AvgOrderValue > 0 ? wcData.AvgOrderValue.ToString() : "" ;
                    retVal.WcRawData.Inventory = wcData.TotalCardInventory > 0 ?  wcData.TotalCardInventory.ToString() : "";
                    retVal.WcRawData.RegisteredUsers = wcData.TotalCardCustomer > 0 ?  wcData.TotalCardCustomer.ToString() : "";
                    retVal.HtmlList = listOfResult;
                }
            }

            return retVal;
        }

        public async Task<PrepareCallRailData> PrepareCallRailReport(Guid campaignId, string startDate, string endDate, List<string> subtypes)
        {
            var retVal = new PrepareCallRailData();
            retVal.CrRawData = new CrRawData();

            List<Dictionary<string, string>> listOfResult = new List<Dictionary<string, string>>();

            var previousDate = CalculatePreviousStartDateAndEndDate(startDate, endDate);

            if (campaignId != null)
            {
                var callReportDto = new CallReportDTO
                {
                    CampaignId = campaignId,
                    StartDate = startDate,
                    EndDate = endDate,
                    PrevStartDate = previousDate.PreviousStartDate.ToString("yyyy-MM-dd"),
                    PrevEndDate = previousDate.PreviousEndDate.ToString("yyyy-MM-dd")

                };

                var callRailData = await _campaignCallRailService.GetCallRailReport(callReportDto);

                if (callRailData != null)
                {
                    foreach (var subtype in subtypes)
                    {
                        var intsubtype = Convert.ToInt16(subtype);
                        if (intsubtype == (int)ReportTypes.CallRailPie)
                        {
                            string htmlString = string.Empty;
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/crPie.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathPie).Result;
                            }

                            List<string> colorShades = new List<string>
                            {
                               "#ffa096",
                                "#9a55ff"
                            };

                            // Create a new list by selecting elements from the original list based on the count
                            var selectedColors = string.Join(",", colorShades.Select(x => "'" + x + "'"));
                            htmlString = htmlString.Replace("_gsBgColorPieChart_", selectedColors);

                            var myData = new List<int> { callRailData.CurrentPeriodData.TotalAnswered, callRailData.CurrentPeriodData.TotalMissed };

                            var myLabels = new List<string> { "Answered " + callRailData.CurrentPeriodData.TotalAnswered + " (" + callRailData.CurrentPeriodData.TotalAnsweredRateAvg + "%)", "Missed " + callRailData.CurrentPeriodData.TotalMissed + " (" + callRailData.CurrentPeriodData.TotalMissedRateAvg + "%)" };

                            var data = string.Join(",", myData);

                            var labels = string.Join(",", myLabels.Select(x => "'" + x + "'"));

                            htmlString = htmlString.Replace("_gsPieChartData_", data);
                            htmlString = htmlString.Replace("_gsPieChartLabel_", labels);
                            htmlString = htmlString.Replace("_gsTitle_", "Answered vs Missed Calls");
                            htmlString = htmlString.Replace("_gsAggeragatorValue_", callRailData.PieChartDiff);

                            var hasPlusSign1 = callRailData.PieChartDiff.Contains("+");
                            if (hasPlusSign1)
                            {
                                htmlString = htmlString.Replace("_condiffcolor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_condiffcolor_", "red");
                            }


                            string uniqueKey = $"{62}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);

                            retVal.CrRawData.AnswerVsMissed = "Total Answered :" + callRailData.CurrentPeriodData.TotalAnswered.ToString() + " Total Missed :" + callRailData.CurrentPeriodData.TotalMissed.ToString() + " (" + callRailData.PieChartDiff + ")";
                        }
                        else if (intsubtype == (int)ReportTypes.CallRailTopSources)
                        {
                            string htmlString = string.Empty;
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/crSources.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathPie).Result;
                            }

                            var labels = callRailData.CurrentPeriodData.SourceCounts.Select(x => x.Key).Take(5).ToList();

                            var data = callRailData.CurrentPeriodData.SourceCounts.Select(x => x.Value).Take(5).ToList();
                            var total = data.Sum();

                            var percentages = total == 0 ? new List<int>() : data.Select(x => (int)Math.Round((double)x / total * 100)).ToList();

                            var sourceData = string.Join(",", labels.Select(x => "'" + x + "'"));

                            var valueData = string.Join(",", data);

                            var percentageData = string.Join(",", percentages);

                            // Create a new list by selecting elements from the original list based on the count
                            htmlString = htmlString.Replace("_crSourceList_", sourceData);
                            htmlString = htmlString.Replace("_crValueList_", valueData);
                            htmlString = htmlString.Replace("_crPercentageList_", percentageData);


                            string uniqueKey = $"{62}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);

                            retVal.CrRawData.TopSourceData = data.Count > 0 && labels.Count > 0 ? labels[0] + " " + data[0] : "";
                        }

                        else if (intsubtype == (int)ReportTypes.CallRailAnsweredLine)
                        {
                            string htmlString = string.Empty;
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/crLine.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathPie).Result;
                            }

                            htmlString = htmlString.Replace("_crChartId_", "crAnswerChart");

                            var labels = string.Join(",", callRailData.CurrentPeriodData.Dates.Select(x => "'" + x + "'"));

                            var currentData = string.Join(",", callRailData.CurrentPeriodData.AnsweredList);

                            var previousData = string.Join(",", callRailData.PreviousPeriodData.AnsweredList);

                            htmlString = htmlString.Replace("_crLineData1_", currentData);
                            htmlString = htmlString.Replace("_crLineData2_", previousData);
                            htmlString = htmlString.Replace("_crLineLabels_", labels);

                            htmlString = htmlString.Replace("_crLineAggeragatorValue_", callRailData.AnsweredDiff);

                            var hasPlusSign1 = callRailData.AnsweredDiff.Contains("+");
                            if (hasPlusSign1)
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "red");
                            }

                            htmlString = htmlString.Replace("_crLineChartTitle_", "Answered");

                            bool isDailyFormat = callRailData.CurrentPeriodData.Dates.FirstOrDefault()?.Length == 6;
                            if (isDailyFormat)
                            {
                                int intervalRes = callRailData.CurrentPeriodData.Dates.Count <= 31 ? 3 : (callRailData.CurrentPeriodData.Dates.Count <= 91 ? 7 : 31);
                                htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_labelInterval_", "1");
                            }

                            string uniqueKey = $"{62}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);

                            retVal.CrRawData.Answered = callRailData.CurrentPeriodData.TotalAnswered.ToString() + " (" + callRailData.AnsweredDiff + ")";
                        }
                        else if (intsubtype == (int)ReportTypes.CallRailCallLine)
                        {
                            string htmlString = string.Empty;
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/crLine.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathPie).Result;
                            }

                            htmlString = htmlString.Replace("_crChartId_", "crCallChart");

                            var labels = string.Join(",", callRailData.CurrentPeriodData.Dates.Select(x => "'" + x + "'"));

                            var currentData = string.Join(",", callRailData.CurrentPeriodData.CallsList);

                            var previousData = string.Join(",", callRailData.PreviousPeriodData.CallsList);

                            htmlString = htmlString.Replace("_crLineData1_", currentData);
                            htmlString = htmlString.Replace("_crLineData2_", previousData);
                            htmlString = htmlString.Replace("_crLineLabels_", labels);

                            htmlString = htmlString.Replace("_crLineAggeragatorValue_", callRailData.CallsDiff);

                            var hasPlusSign1 = callRailData.CallsDiff.Contains("+");
                            if (hasPlusSign1)
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "red");
                            }

                            htmlString = htmlString.Replace("_crLineChartTitle_", "Calls");

                            bool isDailyFormat = callRailData.CurrentPeriodData.Dates.FirstOrDefault()?.Length == 6;
                            if (isDailyFormat)
                            {
                                int intervalRes = callRailData.CurrentPeriodData.Dates.Count <= 31 ? 3 : (callRailData.CurrentPeriodData.Dates.Count <= 91 ? 7 : 31);
                                htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_labelInterval_", "1");
                            }

                            string uniqueKey = $"{62}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                            retVal.CrRawData.Calls = callRailData.CurrentPeriodData.TotalCalls.ToString() + " (" + callRailData.CallsDiff + ")";

                        }
                        else if (intsubtype == (int)ReportTypes.CallRailMissedLine)
                        {
                            string htmlString = string.Empty;
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/crLine.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathPie).Result;
                            }

                            htmlString = htmlString.Replace("_crChartId_", "crMissedChart");

                            var labels = string.Join(",", callRailData.CurrentPeriodData.Dates.Select(x => "'" + x + "'"));

                            var currentData = string.Join(",", callRailData.CurrentPeriodData.MissedCallList);

                            var previousData = string.Join(",", callRailData.PreviousPeriodData.MissedCallList);

                            htmlString = htmlString.Replace("_crLineData1_", currentData);
                            htmlString = htmlString.Replace("_crLineData2_", previousData);
                            htmlString = htmlString.Replace("_crLineLabels_", labels);

                            htmlString = htmlString.Replace("_crLineAggeragatorValue_", callRailData.MissedDiff);

                            var hasPlusSign1 = callRailData.MissedDiff.Contains("+");
                            if (hasPlusSign1)
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "red");
                            }

                            htmlString = htmlString.Replace("_crLineChartTitle_", "Missed");

                            bool isDailyFormat = callRailData.CurrentPeriodData.Dates.FirstOrDefault()?.Length == 6;
                            if (isDailyFormat)
                            {
                                int intervalRes = callRailData.CurrentPeriodData.Dates.Count <= 31 ? 3 : (callRailData.CurrentPeriodData.Dates.Count <= 91 ? 7 : 31);
                                htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_labelInterval_", "1");
                            }

                            string uniqueKey = $"{62}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);

                            retVal.CrRawData.Missed = callRailData.CurrentPeriodData.TotalMissed.ToString() + " (" + callRailData.MissedDiff + ")";

                        }
                        else if (intsubtype == (int)ReportTypes.CallRailFirstTimeCallLine)
                        {
                            string htmlString = string.Empty;
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/crLine.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathPie).Result;
                            }

                            htmlString = htmlString.Replace("_crChartId_", "crFTimeChart");

                            var labels = string.Join(",", callRailData.CurrentPeriodData.Dates.Select(x => "'" + x + "'"));

                            var currentData = string.Join(",", callRailData.CurrentPeriodData.FirstTimeList);

                            var previousData = string.Join(",", callRailData.PreviousPeriodData.FirstTimeList);

                            htmlString = htmlString.Replace("_crLineData1_", currentData);
                            htmlString = htmlString.Replace("_crLineData2_", previousData);
                            htmlString = htmlString.Replace("_crLineLabels_", labels);

                            htmlString = htmlString.Replace("_crLineAggeragatorValue_", callRailData.FirstTimeDiff);

                            var hasPlusSign1 = callRailData.FirstTimeDiff.Contains("+");
                            if (hasPlusSign1)
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "red");
                            }

                            htmlString = htmlString.Replace("_crLineChartTitle_", "First Time Calls");

                            bool isDailyFormat = callRailData.CurrentPeriodData.Dates.FirstOrDefault()?.Length == 6;
                            if (isDailyFormat)
                            {
                                int intervalRes = callRailData.CurrentPeriodData.Dates.Count <= 31 ? 3 : (callRailData.CurrentPeriodData.Dates.Count <= 91 ? 7 : 31);
                                htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_labelInterval_", "1");
                            }

                            string uniqueKey = $"{62}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                            retVal.CrRawData.FirstTime = callRailData.CurrentPeriodData.TotalFirstTime.ToString() + " (" + callRailData.FirstTimeDiff + ")";

                        }
                        else if (intsubtype == (int)ReportTypes.CallRailLeadLine)
                        {
                            string htmlString = string.Empty;
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/crLine.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathPie).Result;
                            }

                            htmlString = htmlString.Replace("_crChartId_", "crLeadChart");

                            var labels = string.Join(",", callRailData.CurrentPeriodData.Dates.Select(x => "'" + x + "'"));

                            var currentData = string.Join(",", callRailData.CurrentPeriodData.LeadsList);

                            var previousData = string.Join(",", callRailData.PreviousPeriodData.LeadsList);

                            htmlString = htmlString.Replace("_crLineData1_", currentData);
                            htmlString = htmlString.Replace("_crLineData2_", previousData);
                            htmlString = htmlString.Replace("_crLineLabels_", labels);

                            htmlString = htmlString.Replace("_crLineAggeragatorValue_", callRailData.LeadsDiff);

                            var hasPlusSign1 = callRailData.LeadsDiff.Contains("+");
                            if (hasPlusSign1)
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "red");
                            }

                            htmlString = htmlString.Replace("_crLineChartTitle_", "Leads");

                            bool isDailyFormat = callRailData.CurrentPeriodData.Dates.FirstOrDefault()?.Length == 6;
                            if (isDailyFormat)
                            {
                                int intervalRes = callRailData.CurrentPeriodData.Dates.Count <= 31 ? 3 : (callRailData.CurrentPeriodData.Dates.Count <= 91 ? 7 : 31);
                                htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_labelInterval_", "1");
                            }

                            string uniqueKey = $"{62}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);

                            retVal.CrRawData.Leads = callRailData.CurrentPeriodData.TotalLeads.ToString() + " (" + callRailData.LeadsDiff + ")";

                        }

                        else if (intsubtype == (int)ReportTypes.CallRailAvgDurationLine)
                        {
                            string htmlString = string.Empty;
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/dudrationChart.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathPie).Result;
                            }

                            var labels = string.Join(",", callRailData.CurrentPeriodData.Dates.Select(x => "'" + x + "'"));

                            var currentData = string.Join(",", callRailData.CurrentPeriodData.DurationListInt);

                            var previousData = string.Join(",", callRailData.PreviousPeriodData.DurationListInt);

                            htmlString = htmlString.Replace("_crLineData1_", currentData);
                            htmlString = htmlString.Replace("_crLineData2_", previousData);
                            htmlString = htmlString.Replace("_crLineLabels_", labels);

                            htmlString = htmlString.Replace("_crLineAggeragatorValue_", callRailData.AvgDurationDiff);

                            var hasPlusSign1 = callRailData.AvgDurationDiff.Contains("+");
                            if (hasPlusSign1)
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "red");
                            }

                            htmlString = htmlString.Replace("_crLineChartTitle_", "Avg Duration");

                            bool isDailyFormat = callRailData.CurrentPeriodData.Dates.FirstOrDefault()?.Length == 6;
                            if (isDailyFormat)
                            {
                                int intervalRes = callRailData.CurrentPeriodData.Dates.Count <= 31 ? 3 : (callRailData.CurrentPeriodData.Dates.Count <= 91 ? 7 : 31);
                                htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_labelInterval_", "1");
                            }

                            string uniqueKey = $"{62}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);

                            retVal.CrRawData.AvgDuration = callRailData.CurrentPeriodData.AvgDuration.ToString() + " (" + callRailData.AvgDurationDiff + ")";

                        }
                        else if (intsubtype == (int)ReportTypes.CallRailAvgAnswerRateLine)
                        {
                            string htmlString = string.Empty;
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/crLine.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathPie).Result;
                            }

                            htmlString = htmlString.Replace("_crChartId_", "crAvgAnsRateChart");

                            var labels = string.Join(",", callRailData.CurrentPeriodData.Dates.Select(x => "'" + x + "'"));

                            var currentData = string.Join(",", callRailData.CurrentPeriodData.AvgAnswerRateList);

                            var previousData = string.Join(",", callRailData.PreviousPeriodData.AvgAnswerRateList);

                            htmlString = htmlString.Replace("_crLineData1_", currentData);
                            htmlString = htmlString.Replace("_crLineData2_", previousData);
                            htmlString = htmlString.Replace("_crLineLabels_", labels);

                            htmlString = htmlString.Replace("_crLineAggeragatorValue_", callRailData.AnsweredRateAvgDiff);

                            var hasPlusSign1 = callRailData.AnsweredRateAvgDiff.Contains("+");
                            if (hasPlusSign1)
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "red");
                            }

                            htmlString = htmlString.Replace("_crLineChartTitle_", "Avg Answer Rate");

                            bool isDailyFormat = callRailData.CurrentPeriodData.Dates.FirstOrDefault()?.Length == 6;
                            if (isDailyFormat)
                            {
                                int intervalRes = callRailData.CurrentPeriodData.Dates.Count <= 31 ? 3 : (callRailData.CurrentPeriodData.Dates.Count <= 91 ? 7 : 31);
                                htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_labelInterval_", "1");
                            }

                            string uniqueKey = $"{62}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                            retVal.CrRawData.AnsweredRateAvg = callRailData.CurrentPeriodData.TotalAnsweredRateAvg.ToString() + " (" + callRailData.AnsweredRateAvgDiff + ")";
                        }
                        else if (intsubtype == (int)ReportTypes.CallRailAvgMissedRateLine)
                        {
                            string htmlString = string.Empty;
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/crLine.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathPie).Result;
                            }

                            htmlString = htmlString.Replace("_crChartId_", "crAvgMissedRateChart");

                            var labels = string.Join(",", callRailData.CurrentPeriodData.Dates.Select(x => "'" + x + "'"));

                            var currentData = string.Join(",", callRailData.CurrentPeriodData.AvgMissedRateList);

                            var previousData = string.Join(",", callRailData.PreviousPeriodData.AvgMissedRateList);

                            htmlString = htmlString.Replace("_crLineData1_", currentData);
                            htmlString = htmlString.Replace("_crLineData2_", previousData);
                            htmlString = htmlString.Replace("_crLineLabels_", labels);

                            htmlString = htmlString.Replace("_crLineAggeragatorValue_", callRailData.MissedRateAvgDiff);

                            var hasPlusSign1 = callRailData.MissedRateAvgDiff.Contains("+");
                            if (hasPlusSign1)
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "red");
                            }

                            htmlString = htmlString.Replace("_crLineChartTitle_", "Avg Missed Rate");

                            bool isDailyFormat = callRailData.CurrentPeriodData.Dates.FirstOrDefault()?.Length == 6;
                            if (isDailyFormat)
                            {
                                int intervalRes = callRailData.CurrentPeriodData.Dates.Count <= 31 ? 3 : (callRailData.CurrentPeriodData.Dates.Count <= 91 ? 7 : 31);
                                htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_labelInterval_", "1");
                            }

                            string uniqueKey = $"{62}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);

                            retVal.CrRawData.MissedRateAvg = callRailData.CurrentPeriodData.TotalMissedRateAvg.ToString() + " (" + callRailData.MissedRateAvgDiff + ")";
                        }
                        else if (intsubtype == (int)ReportTypes.CallRailAvgFirstTimeRateLine)
                        {
                            string htmlString = string.Empty;
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/crLine.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathPie).Result;
                            }

                            htmlString = htmlString.Replace("_crChartId_", "crAvgFCallRateChart");

                            var labels = string.Join(",", callRailData.CurrentPeriodData.Dates.Select(x => "'" + x + "'"));

                            var currentData = string.Join(",", callRailData.CurrentPeriodData.AvgFirstTimeCallRateList);

                            var previousData = string.Join(",", callRailData.PreviousPeriodData.AvgFirstTimeCallRateList);

                            htmlString = htmlString.Replace("_crLineData1_", currentData);
                            htmlString = htmlString.Replace("_crLineData2_", previousData);
                            htmlString = htmlString.Replace("_crLineLabels_", labels);

                            htmlString = htmlString.Replace("_crLineAggeragatorValue_", callRailData.FirstRateAvgDiff);

                            var hasPlusSign1 = callRailData.FirstRateAvgDiff.Contains("+");
                            if (hasPlusSign1)
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "red");
                            }

                            htmlString = htmlString.Replace("_crLineChartTitle_", "Avg First Time Call Rate");

                            bool isDailyFormat = callRailData.CurrentPeriodData.Dates.FirstOrDefault()?.Length == 6;
                            if (isDailyFormat)
                            {
                                int intervalRes = callRailData.CurrentPeriodData.Dates.Count <= 31 ? 3 : (callRailData.CurrentPeriodData.Dates.Count <= 91 ? 7 : 31);
                                htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_labelInterval_", "1");
                            }

                            string uniqueKey = $"{62}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);

                            retVal.CrRawData.AvgFirstTimeCallRate = callRailData.CurrentPeriodData.TotalAvgFirstTimeCallRate.ToString() + " (" + callRailData.FirstRateAvgDiff + ")";
                        }
                        else if (intsubtype == (int)ReportTypes.CallRailAvgCallPerLead)
                        {
                            string htmlString = string.Empty;
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/crLine.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathPie).Result;
                            }

                            htmlString = htmlString.Replace("_crChartId_", "crAvgCallPerLeadChart");

                            var labels = string.Join(",", callRailData.CurrentPeriodData.Dates.Select(x => "'" + x + "'"));

                            var currentData = string.Join(",", callRailData.CurrentPeriodData.AvgCallsPerLeadList);

                            var previousData = string.Join(",", callRailData.PreviousPeriodData.AvgCallsPerLeadList);

                            htmlString = htmlString.Replace("_crLineData1_", currentData);
                            htmlString = htmlString.Replace("_crLineData2_", previousData);
                            htmlString = htmlString.Replace("_crLineLabels_", labels);

                            htmlString = htmlString.Replace("_crLineAggeragatorValue_", callRailData.AvgCallPerLeadsDiff);

                            var hasPlusSign1 = callRailData.AvgCallPerLeadsDiff.Contains("+");
                            if (hasPlusSign1)
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "green");
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_crDiffcolor_", "red");
                            }

                            htmlString = htmlString.Replace("_crLineChartTitle_", "Avg Calls Per Lead");

                            bool isDailyFormat = callRailData.CurrentPeriodData.Dates.FirstOrDefault()?.Length == 6;
                            if (isDailyFormat)
                            {
                                int intervalRes = callRailData.CurrentPeriodData.Dates.Count <= 31 ? 3 : (callRailData.CurrentPeriodData.Dates.Count <= 91 ? 7 : 31);
                                htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());
                            }
                            else
                            {
                                htmlString = htmlString.Replace("_labelInterval_", "1");
                            }

                            string uniqueKey = $"{62}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);

                            retVal.CrRawData.AvgCallsPerLead = callRailData.CurrentPeriodData.TotalAvgCallsPerLead.ToString() + " (" + callRailData.AvgCallPerLeadsDiff + ")";

                        }
                        else if (intsubtype == (int)ReportTypes.CallRailTable)
                        {
                            var crTableData = await _campaignCallRailService.GetAllCallRailCallsForPdf(callReportDto.CampaignId, callReportDto.StartDate, callReportDto.EndDate);

                            string htmlString = string.Empty;
                            Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                            string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/crTable.html");
                            using (HttpClient httpclient = new HttpClient())
                            {
                                htmlString = httpclient.GetStringAsync(pathPie).Result;
                            }


                            // serialize object
                            var tableString = JsonConvert.SerializeObject(crTableData.calls);

                            htmlString = htmlString.Replace("_crDataTable1_", tableString);

                            string uniqueKey = $"{62}({string.Join(",", subtype)})";
                            uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                            listOfResult.Add(uniqueTypeSubtypeResults);
                        }
                    }
                }
            }
            retVal.HtmlList = listOfResult;
            return retVal;
        }

        static string AddSpacesToCamelCase(string input)
        {
            // Use a regular expression to insert spaces before each capital letter
            string result = Regex.Replace(input, "(\\B[A-Z])", " $1");

            // Capitalize the first letter
            result = char.ToUpper(result[0]) + result.Substring(1);

            return result;
        }

        static string AddPercentageIfRate(decimal value, string propertyName)
        {
            // Check if the property name contains "Rate" and add "%" accordingly
            return propertyName.Contains("Rate") ? $"{value}%" : value.ToString();
        }
        static KeyValuePair<short, string> ExtractNumber(string input)
        {
            Regex regex = new Regex(@"^(\d+)(?:\[(.*?)])?$");

            Match match = regex.Match(input);

            if (match.Success)
            {
                short key = short.Parse(match.Groups[1].Value);
                string value = match.Groups[2].Success ? match.Groups[2].Value : string.Empty;

                return new KeyValuePair<short, string>(key, value);
            }

            // Return a default key-value pair or throw an exception based on your needs
            return new KeyValuePair<short, string>(0, string.Empty); // Default key-value pair, replace with appropriate handling
        }

        static string CombineLabelsAndValues(List<string> labels, List<decimal> values)
        {
            var combined = labels.Zip(values, (label, value) => $"'{label}: {value}'");

            return string.Join(", ", combined);
        }

        public async Task<PrepareMailChimpData> PrepareMailchimpReport(Guid campaignId, List<string> subtypes, MailchimpSettings mailchimpSettings, string mcString)
        {
            //raw data for chat gpt
            var retVal = new PrepareMailChimpData();
            retVal.MailchimpCampaignsRawData = new MCRootCampaignListRawData();
            retVal.MailchimpListRawData = new MCRootListRawData();
            retVal.SingleCampaignsRawData = new List<SingleCampaignReportRawData>();
            retVal.SingleListsRawData = new List<RootSingleListRawData>();
            //end

            List<Dictionary<string, string>> listOfResult = new List<Dictionary<string, string>>();

            MCRootCampaignList mailchimpData = new MCRootCampaignList();

            MCRootList mailchimpListData = new MCRootList();

            var singleCampaignReport = new SingleCampaignReport();

            var campaignTable = new CampaignTableRoot();

            var topLink = new ClickDetailsDto();

            if (campaignId != null)
            {
                var campaignsSubType = new List<string> { "77", "78", "79", "81", "80" };
                if (subtypes.Intersect(campaignsSubType).Any())
                {
                    mailchimpData = await _campaignMailchimpService.GetCampaignListReport(campaignId);
                }

                var listsSubType = new List<string> { "83", "84", "85", "86", "87" };
                if (subtypes.Intersect(listsSubType).Any())
                {
                    mailchimpListData = await _campaignMailchimpService.GetListReport(campaignId);
                }

                foreach (var subtype in subtypes)
                {
                    //put extra logic for mailchimp get dynamic id  // 76(82[8af9ff7617]) key: 82 Value: 8af9ff7617
                    var subtypeAndMcValue = ExtractNumber(subtype);

                    if (subtypeAndMcValue.Key == (int)ReportTypes.McCampaignsRecipients)
                    {
                        string htmlString = string.Empty;
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                        string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/campaignsRecipients.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(pathPie).Result;
                        }

                        //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/campaignsRecipients.html");
                        //htmlString = System.IO.File.ReadAllText(pathGa);


                        htmlString = htmlString.Replace("_mcChartId_", "mcRecipientsChart");

                        var labels = string.Join(",", mailchimpData.recipientsChartDates.Select(x => "'" + x + "'"));

                        var currentData = string.Join(",", mailchimpData.recipientsChartValues);

                        htmlString = htmlString.Replace("_mcLineData1_", currentData);

                        htmlString = htmlString.Replace("_mcLineLabels_", labels);
                        htmlString = htmlString.Replace("_mcLineChartTotal_", mailchimpData.recipientsChartTotal.ToString());


                        htmlString = htmlString.Replace("_mcLineChartTitle_", "Recipients (24-Hour Period)");

                        string uniqueKey = $"{76}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);

                        retVal.MailchimpCampaignsRawData.recipients24HourPeriod = mailchimpData.recipientsChartTotal;
                    }
                    else if (subtypeAndMcValue.Key == (int)ReportTypes.McCampaignsOpens)
                    {
                        string htmlString = string.Empty;
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                        string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/campaignsOpens.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(pathPie).Result;
                        }

                        //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/campaignsOpens.html");
                        //htmlString = System.IO.File.ReadAllText(pathGa);


                        htmlString = htmlString.Replace("_mcChartId_", "mcOpensChart");

                        var labels = string.Join(",", mailchimpData.openChartDates.Select(x => "'" + x + "'"));

                        var currentData = string.Join(",", mailchimpData.openChartValues);

                        htmlString = htmlString.Replace("_mcOpenData1_", currentData);

                        htmlString = htmlString.Replace("_mcOpenLabels_", labels);

                        htmlString = htmlString.Replace("_mcOpenChartTitle_", "Opens (24-Hour Period)");

                        htmlString = htmlString.Replace("_mcLineOpenTotal_", mailchimpData.uniqueOpenChartTotal.ToString());

                        string uniqueKey = $"{76}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);

                        retVal.MailchimpCampaignsRawData.open24HourPeriod = mailchimpData.uniqueOpenChartTotal;
                    }
                    else if (subtypeAndMcValue.Key == (int)ReportTypes.McCampaignsClicks)
                    {
                        string htmlString = string.Empty;
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                        string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/campaignsClicks.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(pathPie).Result;
                        }

                        //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/campaignsClicks.html");
                        //htmlString = System.IO.File.ReadAllText(pathGa);

                        htmlString = htmlString.Replace("_mcChartId_", "mcClickChart");

                        var labels = string.Join(",", mailchimpData.clickChartDates.Select(x => "'" + x + "'"));

                        var currentData = string.Join(",", mailchimpData.clickChartValues);

                        htmlString = htmlString.Replace("_mcClickData1_", currentData);

                        htmlString = htmlString.Replace("_mcClickLabels_", labels);

                        htmlString = htmlString.Replace("_mcLineClickTotal_", mailchimpData.clickChartTotal.ToString());

                        htmlString = htmlString.Replace("_mcClickChartTitle_", "Clicks (24-Hour Period)");

                        string uniqueKey = $"{76}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);

                        retVal.MailchimpCampaignsRawData.click24HourPeriod = mailchimpData.clickChartTotal;
                    }
                    else if (subtypeAndMcValue.Key == (int)ReportTypes.McCampaignsTiles && mailchimpSettings != null && mailchimpSettings.mcCampaignsSetting != null)
                    {

                        string htmlString = string.Empty;
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                        //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/campaignsTiles.html");
                        //htmlString = System.IO.File.ReadAllText(pathGa);

                        string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/campaignsTiles.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(pathPie).Result;
                        }

                        var listTilesData = new List<McTilesData>();

                        JObject jObject = JObject.Parse(mcString);

                        // Get properties for McCampaignsSetting
                        JToken mcCampaignsSettingToken = jObject.SelectToken("mcCampaignsSetting");
                        var orderedProperties = mcCampaignsSettingToken?.Children<JProperty>().ToList();


                        // Get the properties in the order specified in mailchimpSettings
                        //var orderedProperties = jObject.Properties();
                        var orderedListTilesData = new List<McTilesData>();
                        var dataProperties = typeof(MCRootCampaignList).GetProperties();

                        foreach (var orderedProperty in orderedProperties)
                        {
                            var settingProperty = mailchimpSettings.mcCampaignsSetting.GetType().GetProperty(orderedProperty.Name);
                            var dataProperty = dataProperties.FirstOrDefault(p => p.Name == orderedProperty.Name);

                            if (settingProperty != null && dataProperty != null && (bool)settingProperty.GetValue(mailchimpSettings.mcCampaignsSetting))
                            {
                                var singleTiles = new McTilesData
                                {
                                    Display = "block",
                                    Status = (bool)settingProperty.GetValue(mailchimpSettings.mcCampaignsSetting),
                                    Name = AddSpacesToCamelCase(settingProperty.Name),
                                    Value = AddPercentageIfRate((decimal)dataProperty.GetValue(mailchimpData), settingProperty.Name)
                                };

                                orderedListTilesData.Add(singleTiles);
                            }
                        }

                        // Create a StringBuilder to store the generated HTML
                        var htmlBuilder = new StringBuilder();

                        // Start the HTML structure
                        htmlBuilder.AppendLine("<div class=\"row a4-page mt-3 d-flex justify-content-center gap-5 flex-wrap\">");

                        // Iterate over each McTilesData in the list
                        for (int i = 0; i < orderedListTilesData.Count; i++)
                        {
                            var tile = orderedListTilesData[i];

                            // Generate HTML for each tile
                            htmlBuilder.AppendLine($"<div class=\"width-18\" style=\"display: {tile.Display}\">");
                            htmlBuilder.AppendLine("    <div class=\"card\">");
                            htmlBuilder.AppendLine("        <div class=\"card-bg-1 card-body text-center card-border-radius\" id=\"clicks\">");
                            htmlBuilder.AppendLine($"            <h5 class=\"card-title card-title-color\">{tile.Name}</h5>");
                            htmlBuilder.AppendLine($"            <h1 class=\"card-text card-title-color fontxxlrge\">{tile.Value}</h1>");
                            htmlBuilder.AppendLine("        </div>");
                            htmlBuilder.AppendLine("    </div>");
                            htmlBuilder.AppendLine("</div>");
                        }

                        // End the HTML structure
                        htmlBuilder.AppendLine("</div>");

                        // Get the final HTML string
                        string generatedHtml = htmlBuilder.ToString();

                        // Now you can use the generatedHtml string where needed in your application


                        // Create a new list by selecting elements from the original list based on the count
                        htmlString = htmlString.Replace("_myMcTilesData_", generatedHtml);

                        string uniqueKey = $"{76}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);

                        retVal.MailchimpCampaignsRawData.recipients = mailchimpData.recipients;
                        retVal.MailchimpCampaignsRawData.unopenedEmails = mailchimpData.unopenedEmails;
                        retVal.MailchimpCampaignsRawData.bouncedEmails = mailchimpData.bouncedEmails;
                        retVal.MailchimpCampaignsRawData.uniqueOpens = mailchimpData.uniqueOpens;
                        retVal.MailchimpCampaignsRawData.openRate = mailchimpData.openRate;
                        retVal.MailchimpCampaignsRawData.clickRate = mailchimpData.clickRate;
                        retVal.MailchimpCampaignsRawData.unsubscribeRate = mailchimpData.unsubscribeRate;
                        retVal.MailchimpCampaignsRawData.unsubscribes = mailchimpData.unsubscribes;
                        retVal.MailchimpCampaignsRawData.bounceRate = mailchimpData.bounceRate;
                        retVal.MailchimpCampaignsRawData.clicks = mailchimpData.clicks;
                        retVal.MailchimpCampaignsRawData.subsciberClick = mailchimpData.subsciberClick;
                        retVal.MailchimpCampaignsRawData.opens = mailchimpData.opens;
                        retVal.MailchimpCampaignsRawData.orders = mailchimpData.orders;
                        retVal.MailchimpCampaignsRawData.averageOrder = mailchimpData.averageOrder;
                        retVal.MailchimpCampaignsRawData.revenue = mailchimpData.revenue;
                        retVal.MailchimpCampaignsRawData.totalSpent = mailchimpData.totalSpent;
                        retVal.MailchimpCampaignsRawData.deliveries = mailchimpData.deliveries;
                        retVal.MailchimpCampaignsRawData.deliveryRate = mailchimpData.deliveryRate;
                        retVal.MailchimpCampaignsRawData.spams = mailchimpData.spams;
                        retVal.MailchimpCampaignsRawData.spamRate = mailchimpData.spamRate;

                    }
                    else if (subtypeAndMcValue.Key == (int)ReportTypes.McCampaignsTable)
                    {
                        string htmlString = string.Empty;
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                        string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/campaignsTable.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(pathPie).Result;
                        }

                        //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/campaignsTable.html");
                        //htmlString = System.IO.File.ReadAllText(pathGa);

                        // serialize object
                        var tableString = JsonConvert.SerializeObject(mailchimpData.campaignListTable);

                        htmlString = htmlString.Replace("_mcCampaignsTable_", tableString);

                        string uniqueKey = $"{76}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);
                    }

                    else if (subtypeAndMcValue.Key == (int)ReportTypes.McListsAudianceGrowth)
                    {
                        string htmlString = string.Empty;
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                        string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/mcListGrowth.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(pathPie).Result;
                        }

                        //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/mcListGrowth.html");
                        //htmlString = System.IO.File.ReadAllText(pathGa);


                        htmlString = htmlString.Replace("_mcChartId_", "mcListGrowthChart");

                        var labels = string.Join(",", mailchimpListData.audianceGrowthChartDates.Select(x => "'" + x + "'"));

                        var currentData = string.Join(",", mailchimpListData.audianceGrowthChartValues);

                        htmlString = htmlString.Replace("_mcGrowthData1_", currentData);

                        htmlString = htmlString.Replace("_mcGrowthLabels_", labels);

                        htmlString = htmlString.Replace("_mcGrowthChartTitle_", "Audience Growth");

                        htmlString = htmlString.Replace("_mcGrowthChartTotal_", mailchimpListData.growthChartTotal.ToString());

                        string uniqueKey = $"{76}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);

                        retVal.MailchimpListRawData.audienceGrowth = mailchimpListData.growthChartTotal;
                        retVal.MailchimpListRawData.campaigns = mailchimpListData.campaigns;

                    }
                    else if (subtypeAndMcValue.Key == (int)ReportTypes.McListsOpens)
                    {
                        string htmlString = string.Empty;
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                        string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/mcListOpens.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(pathPie).Result;
                        }

                        //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/mcListOpens.html");
                        //htmlString = System.IO.File.ReadAllText(pathGa);


                        htmlString = htmlString.Replace("_mcChartId_", "mcListOpensChart");

                        var labels = string.Join(",", mailchimpListData.openChartDates.Select(x => "'" + x + "'"));

                        var currentData = string.Join(",", mailchimpListData.openChartValues);

                        htmlString = htmlString.Replace("_mcListOpenData1_", currentData);

                        htmlString = htmlString.Replace("_mcListOpenLabels_", labels);

                        htmlString = htmlString.Replace("_mcListOpenChartTitle_", "Opens");

                        htmlString = htmlString.Replace("_mcListOpenChartTotal_", mailchimpListData.opensChartTotal.ToString());

                        string uniqueKey = $"{76}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);

                        retVal.MailchimpListRawData.opens = mailchimpListData.opensChartTotal;

                    }
                    else if (subtypeAndMcValue.Key == (int)ReportTypes.McListsClicks)
                    {
                        string htmlString = string.Empty;
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                        string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/mcListClicks.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(pathPie).Result;
                        }

                        //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/mcListClicks.html");
                        //htmlString = System.IO.File.ReadAllText(pathGa);


                        htmlString = htmlString.Replace("_mcChartId_", "mcListClickChart");

                        var labels = string.Join(",", mailchimpListData.openChartDates.Select(x => "'" + x + "'"));

                        var currentData = string.Join(",", mailchimpListData.clickChartValues);

                        htmlString = htmlString.Replace("_mcListClickData1_", currentData);

                        htmlString = htmlString.Replace("_mcListClickLabels_", labels);

                        htmlString = htmlString.Replace("_mcListClickChartTitle_", "Clicks");

                        htmlString = htmlString.Replace("_mcListClickChartTotal_", mailchimpListData.clickChartTotal.ToString());

                        string uniqueKey = $"{76}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);

                        retVal.MailchimpListRawData.clicks = mailchimpListData.clickChartTotal;
                    }
                    else if (subtypeAndMcValue.Key == (int)ReportTypes.McListsTiles && mailchimpSettings != null)
                    {

                        string htmlString = string.Empty;
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                        //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/mcListTiles.html");
                        //htmlString = System.IO.File.ReadAllText(pathGa);

                        string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/mcListTiles.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(pathPie).Result;
                        }

                        var listTilesData = new List<McTilesData>();

                        JObject jObject = JObject.Parse(mcString);

                        // Get properties for McCampaignsSetting
                        JToken mcListSettingToken = jObject.SelectToken("mcListSetting");
                        var orderedProperties = mcListSettingToken?.Children<JProperty>().ToList();

                        // Get the properties in the order specified in mailchimpSettings
                        // var orderedProperties = jObject.Properties();
                        var orderedListTilesData = new List<McTilesData>();
                        var dataProperties = typeof(MCRootList).GetProperties();

                        foreach (var orderedProperty in orderedProperties)
                        {
                            var settingProperty = mailchimpSettings.mcListSetting.GetType().GetProperty(orderedProperty.Name);
                            var dataProperty = dataProperties.FirstOrDefault(p => p.Name == orderedProperty.Name);

                            if (settingProperty != null && dataProperty != null && (bool)settingProperty.GetValue(mailchimpSettings.mcListSetting))
                            {
                                var singleTiles = new McTilesData
                                {
                                    Display = "block",
                                    Status = (bool)settingProperty.GetValue(mailchimpSettings.mcListSetting),
                                    Name = AddSpacesToCamelCase(settingProperty.Name),
                                    Value = AddPercentageIfRate((decimal)dataProperty.GetValue(mailchimpListData), settingProperty.Name)
                                };

                                orderedListTilesData.Add(singleTiles);
                            }
                        }

                        // Create a StringBuilder to store the generated HTML
                        var htmlBuilder = new StringBuilder();

                        // Start the HTML structure
                        htmlBuilder.AppendLine("<div class=\"row a4-page mt-3 d-flex justify-content-center gap-5 flex-wrap\">");

                        // Iterate over each McTilesData in the list
                        for (int i = 0; i < orderedListTilesData.Count; i++)
                        {
                            var tile = orderedListTilesData[i];

                            // Generate HTML for each tile
                            htmlBuilder.AppendLine($"<div class=\"width-18\" style=\"display: {tile.Display}\">");
                            htmlBuilder.AppendLine("    <div class=\"card\">");
                            htmlBuilder.AppendLine("        <div class=\"card-bg-1 card-body text-center card-border-radius\" id=\"clicks\">");
                            htmlBuilder.AppendLine($"            <h5 class=\"card-title card-title-color\">{tile.Name}</h5>");
                            htmlBuilder.AppendLine($"            <h1 class=\"card-text card-title-color fontxxlrge\">{tile.Value}</h1>");
                            htmlBuilder.AppendLine("        </div>");
                            htmlBuilder.AppendLine("    </div>");
                            htmlBuilder.AppendLine("</div>");
                        }

                        // End the HTML structure
                        htmlBuilder.AppendLine("</div>");

                        // Get the final HTML string
                        string generatedHtml = htmlBuilder.ToString();

                        // Now you can use the generatedHtml string where needed in your application


                        // Create a new list by selecting elements from the original list based on the count
                        htmlString = htmlString.Replace("_myMcListTilesData_", generatedHtml);

                        string uniqueKey = $"{76}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);

                        retVal.MailchimpListRawData.rating = mailchimpListData.rating;
                        retVal.MailchimpListRawData.subscribers = mailchimpListData.subscribers;
                        retVal.MailchimpListRawData.openRate = mailchimpListData.openRate;
                        retVal.MailchimpListRawData.clickRate = mailchimpListData.clickRate;
                        retVal.MailchimpListRawData.unsubscribes = mailchimpListData.unsubscribes;
                        retVal.MailchimpListRawData.avgSubscribeRate = mailchimpListData.avgSubscribeRate;
                        retVal.MailchimpListRawData.avgUnsubscribeRate = mailchimpListData.avgUnsubscribeRate;

                    }
                    else if (subtypeAndMcValue.Key == (int)ReportTypes.McListsTable)
                    {
                        string htmlString = string.Empty;
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                        string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/mcListTables.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(pathPie).Result;
                        }

                        //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/mcListTables.html");
                        //htmlString = System.IO.File.ReadAllText(pathGa);


                        // serialize object
                        var tableString = JsonConvert.SerializeObject(mailchimpListData.listTable);

                        htmlString = htmlString.Replace("_mcListTable_", tableString);

                        string uniqueKey = $"{76}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);
                    }
                    else if (subtypeAndMcValue.Key == (int)ReportTypes.McSingleCampaign)
                    {
                        string htmlString = string.Empty;
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                        if (!string.IsNullOrEmpty(subtypeAndMcValue.Value))
                        {
                            singleCampaignReport = await _campaignMailchimpService.GetSingleCampaignReport(campaignId, subtypeAndMcValue.Value);
                            campaignTable = await _campaignMailchimpService.GetCampaignTable(campaignId, subtypeAndMcValue.Value, 0, 1000);
                            topLink = await _campaignMailchimpService.GetTopLinksByCampaign(campaignId, subtypeAndMcValue.Value);
                        }

                        string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/singleMcCampaign.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(pathPie).Result;
                        }

                        //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/singleMcCampaign.html");
                        //htmlString = System.IO.File.ReadAllText(pathGa);

                        if (singleCampaignReport != null)
                        { //Bind Tiles



                            htmlString = htmlString.Replace("_singleMcName_", singleCampaignReport.Name.ToString());

                            htmlString = htmlString.Replace("_mcsOpenRate_", singleCampaignReport.OpenRate.ToString());

                            htmlString = htmlString.Replace("_mcsClickRate_", singleCampaignReport.ClickRate.ToString());

                            htmlString = htmlString.Replace("_mcsUnsubscribeRate_", singleCampaignReport.UnsubscribeRate.ToString());

                            htmlString = htmlString.Replace("_mcsBounceRate_", singleCampaignReport.BounceRate.ToString());

                            htmlString = htmlString.Replace("_mcsClicks_", singleCampaignReport.Click.ToString());

                            htmlString = htmlString.Replace("_mcsSubscriberClicks_", singleCampaignReport.SubsciberClick.ToString());

                            htmlString = htmlString.Replace("_mcsUnsubscribes_", singleCampaignReport.Unsubscribes.ToString());

                            htmlString = htmlString.Replace("_mcsOpens_", singleCampaignReport.Opens.ToString());

                            htmlString = htmlString.Replace("_mcsOrders_", singleCampaignReport.Orders.ToString());

                            htmlString = htmlString.Replace("_mcsAverageOrder_", singleCampaignReport.AverageOrder.ToString());

                            htmlString = htmlString.Replace("_mcsTotalRevenue_", singleCampaignReport.Revenue.ToString());

                            htmlString = htmlString.Replace("_mcsTotalSpent_", singleCampaignReport.TotalSpent.ToString());

                            htmlString = htmlString.Replace("_mcsDeliveries_", singleCampaignReport.Deliveries.ToString());

                            htmlString = htmlString.Replace("_mcsDeliveryRate_", singleCampaignReport.DeliveryRate.ToString());

                            htmlString = htmlString.Replace("_mcsSpams_", singleCampaignReport.Spams.ToString());

                            htmlString = htmlString.Replace("_mcsSpamRate_", singleCampaignReport.SpamRate.ToString());


                            //Open and click chart

                            var labels = string.Join(",", singleCampaignReport.OpenChartDates.Select(x => "'" + x + "'"));

                            var currentData = string.Join(",", singleCampaignReport.OpenChartValues);

                            htmlString = htmlString.Replace("_mcsOpenChart_", "mcsOpen" + DateTime.UtcNow.Ticks.ToString());

                            htmlString = htmlString.Replace("_mcOpenData1_", currentData);

                            htmlString = htmlString.Replace("_mcOpenLabels_", labels);

                            htmlString = htmlString.Replace("_mcsOpenTotal_", singleCampaignReport.UniqueOpenChartTotal.ToString());


                            var labels2 = string.Join(",", singleCampaignReport.ClickChartDates.Select(x => "'" + x + "'"));

                            var currentData2 = string.Join(",", singleCampaignReport.ClickChartValues);

                            htmlString = htmlString.Replace("_mcsClickChart_", "mcsClick" + DateTime.UtcNow.Ticks.ToString());

                            htmlString = htmlString.Replace("_mcsClickData1_", currentData2);

                            htmlString = htmlString.Replace("_mcsClickLabels_", labels2);

                            htmlString = htmlString.Replace("_mcsClicksTotal_", singleCampaignReport.ClickChartTotal.ToString());


                            //pie chart

                            // Combine labels and data values
                            var combinedData = CombineLabelsAndValues(singleCampaignReport.PieLabels, singleCampaignReport.PieValues);

                            var data = string.Join(",", singleCampaignReport.PieValues != null ? singleCampaignReport.PieValues : new List<decimal>() { });

                            htmlString = htmlString.Replace("_mcsPieChart_", "mcsPie" + DateTime.UtcNow.Ticks.ToString());
                            htmlString = htmlString.Replace("_mcsPieChartData_", data);
                            htmlString = htmlString.Replace("_mcsPieChartLabel_", combinedData);

                            //location chart

                            var labels4 = string.Join(",", singleCampaignReport.LocationLabels.Select(x => "'" + x + "'"));

                            var currentData4 = string.Join(",", singleCampaignReport.LocationValues);

                            htmlString = htmlString.Replace("_mcsLocationChart_", "mcsLocation" + DateTime.UtcNow.Ticks.ToString());

                            htmlString = htmlString.Replace("_mcsLocationData1_", currentData4);

                            htmlString = htmlString.Replace("_mcsLocationLabels_", labels4);

                            htmlString = htmlString.Replace("_mcsLocationTotal_", singleCampaignReport.LocationChartTotal.ToString());

                            //progess bar

                            if (topLink != null)
                            {
                                var label5 = topLink.urls_clicked.Select(x => x.url).ToList();

                                var data5 = topLink.urls_clicked.Select(x => x.total_clicks).ToList();

                                var totalNumberOfClicks = topLink.urls_clicked.Select(x => x.total_clicks).Sum();

                                var percentages = totalNumberOfClicks == 0 ? new List<int>() : data5.Select(x => (int)Math.Round((double)x / totalNumberOfClicks * 100)).ToList();

                                var sourceData = string.Join(",", label5.Select(x => "'" + x + "'"));

                                var valueData = string.Join(",", data5);

                                var percentageData = string.Join(",", percentages);

                                htmlString = htmlString.Replace("_progressBarsContainerForMcId_", "mcsProgress" + DateTime.UtcNow.Ticks.ToString());

                                // Create a new list by selecting elements from the original list based on the count
                                htmlString = htmlString.Replace("_mcgSourceList_", sourceData);
                                htmlString = htmlString.Replace("_mcgValueList_", valueData);
                                htmlString = htmlString.Replace("_mcgPercentageList_", percentageData);
                            }

                            //table

                            // serialize object
                            var tableString = JsonConvert.SerializeObject(campaignTable.CampaignTableResponse);

                            htmlString = htmlString.Replace("_mcsCampaignTableId_", "mcsTable" + DateTime.UtcNow.Ticks.ToString());

                            htmlString = htmlString.Replace("_mcsCampaignTable_", tableString);

                            htmlString = htmlString.Replace("_mcLineChartTitle_", "Recipients (24-Hour Period)");

                            //Raw data for chat gpt
                            var singleCampaignRawData = new SingleCampaignReportRawData();
                            singleCampaignRawData.Name = singleCampaignReport.Name;
                            singleCampaignRawData.OpenRate = singleCampaignReport.OpenRate;
                            singleCampaignRawData.ClickRate = singleCampaignReport.ClickRate;
                            singleCampaignRawData.UnsubscribeRate = singleCampaignReport.UnsubscribeRate;
                            singleCampaignRawData.Unsubscribes = singleCampaignReport.Unsubscribes;
                            singleCampaignRawData.BounceRate = singleCampaignReport.BounceRate;
                            singleCampaignRawData.Click24HoursPeriod = singleCampaignReport.Click;
                            singleCampaignRawData.SubsciberClick = singleCampaignReport.SubsciberClick;
                            singleCampaignRawData.Opens24HoursPeriod = singleCampaignReport.Opens;
                            singleCampaignRawData.Orders = singleCampaignReport.Orders;
                            singleCampaignRawData.AverageOrder = singleCampaignReport.AverageOrder;
                            singleCampaignRawData.Revenue = singleCampaignReport.Revenue;
                            singleCampaignRawData.TotalSpent = singleCampaignReport.TotalSpent;
                            singleCampaignRawData.Deliveries = singleCampaignReport.Deliveries;
                            singleCampaignRawData.DeliveryRate = singleCampaignReport.DeliveryRate;
                            singleCampaignRawData.Spams = singleCampaignReport.Spams;
                            singleCampaignRawData.SpamRate = singleCampaignReport.SpamRate;

                            singleCampaignRawData.TopLocations = singleCampaignReport.LocationLabels.FirstOrDefault() + " " + singleCampaignReport.LocationValues.FirstOrDefault();
                            singleCampaignRawData.TopSources =  topLink.urls_clicked.OrderByDescending(x => x.total_clicks).FirstOrDefault()?.url +" "+ topLink.urls_clicked.Max(x => x.total_clicks).ToString();


                            retVal.SingleCampaignsRawData.Add(singleCampaignRawData);
                        }

                        string uniqueKey = $"{76}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);
                    }
                    else if (subtypeAndMcValue.Key == (int)ReportTypes.McSingleList)
                    {
                        var singleListReport = new RootSingleList();
                        var listTable = new MailChimpMemberRoot();

                        string htmlString = string.Empty;
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                        if (!string.IsNullOrEmpty(subtypeAndMcValue.Value))
                        {
                            singleListReport = await _campaignMailchimpService.GetSingleListReport(campaignId, subtypeAndMcValue.Value);
                            listTable = await _campaignMailchimpService.GetMemberOfListApi(campaignId, subtypeAndMcValue.Value, 0, 1000, "");
                        }

                        string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/singleMcList.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(pathPie).Result;
                        }

                        //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/singleMcList.html");
                        //htmlString = System.IO.File.ReadAllText(pathGa);

                        if (singleListReport != null)
                        { //Bind Tiles
                            htmlString = htmlString.Replace("_singleMcListName_", singleListReport.Name.ToString());

                            htmlString = htmlString.Replace("_mcsListSubscribers_", singleListReport.Subscribers.ToString());

                            htmlString = htmlString.Replace("_mcsListOpenRate_", singleListReport.OpenRate.ToString());

                            htmlString = htmlString.Replace("_mcsListClickRate_", singleListReport.ClickRate.ToString());

                            htmlString = htmlString.Replace("_mcsListCampaigns_", singleListReport.Campaigns.ToString());

                            htmlString = htmlString.Replace("_mcsListUnsubscribes_", singleListReport.Unsubscribes.ToString());

                            htmlString = htmlString.Replace("_mcsListAsr_", singleListReport.AvgSubscribeRate.ToString());

                            htmlString = htmlString.Replace("_mcsListUasr_", singleListReport.AvguNSubscribeRate.ToString());


                            //Open and click chart

                            var labels = string.Join(",", singleListReport.ChartDates.Select(x => "'" + x + "'"));

                            var currentData = string.Join(",", singleListReport.OpensChartValues);

                            htmlString = htmlString.Replace("_mcsListOpenChart_", "mscOpen" + DateTime.UtcNow.Ticks.ToString());

                            htmlString = htmlString.Replace("_mcListOpenData1_", currentData);

                            htmlString = htmlString.Replace("_mcListOpenLabels_", labels);

                            htmlString = htmlString.Replace("_mcsListOpenTotal_", singleListReport.OpensChartTotal.ToString());


                            var labels2 = string.Join(",", singleListReport.ChartDates.Select(x => "'" + x + "'"));

                            var currentData2 = string.Join(",", singleListReport.ClicksChartValues);

                            htmlString = htmlString.Replace("_mcsListClickChart_", "mscClick" + DateTime.UtcNow.Ticks.ToString());


                            htmlString = htmlString.Replace("_mcsListClickData1_", currentData2);

                            htmlString = htmlString.Replace("_mcsListClickLabels_", labels2);

                            htmlString = htmlString.Replace("_mcsListClicksTotal_", singleListReport.ClickChartTotal.ToString());



                            //audience growth chart

                            var labels4 = string.Join(",", singleListReport.AudianceGrowthChartDates.Select(x => "'" + x + "'"));

                            var currentData4 = string.Join(",", singleListReport.AudianceGrowthChartValues);

                            htmlString = htmlString.Replace("_mcsListAudGrowthChart_", "mscGrowth" + DateTime.UtcNow.Ticks.ToString());


                            htmlString = htmlString.Replace("_mcsListAudienceData1_", currentData4);

                            htmlString = htmlString.Replace("_mcsListAudienceLabels_", labels4);


                            //top email clients

                            var labels5 = string.Join(",", singleListReport.Clients.Select(x => "'" + x + "'"));

                            var currentData5 = string.Join(",", singleListReport.Members);

                            htmlString = htmlString.Replace("_mcsListTopEmailChart_", "mscTop" + DateTime.UtcNow.Ticks.ToString());


                            htmlString = htmlString.Replace("_mcsListEmailData1_", currentData5);

                            htmlString = htmlString.Replace("_mcsListEmailLabels_", labels5);

                            //table

                            // serialize object
                            var tableString = JsonConvert.SerializeObject(listTable.MailChimpMembers);

                            htmlString = htmlString.Replace("_mcsListTableId_", "mcsListTableId" + DateTime.UtcNow.Ticks.ToString());

                            htmlString = htmlString.Replace("_mcsListTable_", tableString);


                            //Raw data for chat gpt
                            var singleCampaignListRawData = new RootSingleListRawData();
                            singleCampaignListRawData.Name = singleListReport.Name;
                            singleCampaignListRawData.OpenRate = singleListReport.OpenRate;
                            singleCampaignListRawData.ClickRate = singleListReport.ClickRate;
                            singleCampaignListRawData.Subscribers = singleListReport.Subscribers;
                            singleCampaignListRawData.Unsubscribes = singleListReport.Unsubscribes;
                            singleCampaignListRawData.Campaigns = singleListReport.Campaigns;
                            singleCampaignListRawData.AvgSubscribeRate = singleListReport.AvgSubscribeRate;
                            singleCampaignListRawData.AvgUnsubscribeRate = singleListReport.AvguNSubscribeRate;
                            singleCampaignListRawData.Clicks = singleListReport.ClickChartTotal;
                            singleCampaignListRawData.Opens = singleListReport.OpensChartTotal;

                            singleCampaignListRawData.AudienceGrowth = singleListReport.AudianceGrowthChartValues != null  ? singleListReport.AudianceGrowthChartValues.Sum(): 0;
                            singleCampaignListRawData.TopEmailClient = singleListReport.Members.Count > 0 && singleListReport.Clients.Count > 0 ? singleListReport.Clients[0] + singleListReport.Members[0].ToString() : "";

                            retVal.SingleListsRawData.Add(singleCampaignListRawData);
                        }

                        string uniqueKey = $"{76}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);
                    }
                }


            }
            retVal.HtmlList = listOfResult;
            return retVal;

        }

        public async Task<PrepareMicrosoftAdsData> PrepareMicrosoftAdsReport(Guid campaignId, List<string> subtypes, string startDate, string endDate)
        {
            //Prepare draw data for chat gpt

            var retVal = new PrepareMicrosoftAdsData();
            retVal.CampaignPerformace = new CampaignPerformace();
            retVal.AdGroupPerformance = new AdGroupPerformance();
            retVal.KeywordPerformance = new KeywordPerformance();
            retVal.ConversionPerformance = new ConversionPerformance();

            //for single list campaigns
            retVal.SingleCampaignPerformaceList = new List<CampaignPerformace>();
            retVal.SingleGroupPerformance = new List<AdGroupPerformance>();
            retVal.SingleKeywordPerformance = new List<KeywordPerformance>();
            retVal.SingleConversionPerformance = new List<ConversionPerformance>();

            List<Dictionary<string, string>> listOfResult = new List<Dictionary<string, string>>();

            var campaignsData = new RootCampaignPerformace();
            var adGroupData = new RootAdGroupPerformance();
            var keywordData = new RootKeywordPerformance();
            var conversionData = new RootConversionPerformance();

            if (campaignId != null)
            {
                var campaignsSubType = new List<string> { "93", "94", "95", "96" };
                if (subtypes.Intersect(campaignsSubType).Any())
                {
                    campaignsData = await _campaignMicrosoftAdService.GetCampaignPerformanceReport(campaignId, startDate, endDate);
                }

                var adGroupSubType = new List<string> { "98", "99", "100", "101" };
                if (subtypes.Intersect(adGroupSubType).Any())
                {
                    adGroupData = await _campaignMicrosoftAdService.GetAdGroupPerformanceReport(campaignId, startDate, endDate);
                }

                var keywordDataSubType = new List<string> { "103", "104", "105", "106" };
                if (subtypes.Intersect(keywordDataSubType).Any())
                {
                    keywordData = await _campaignMicrosoftAdService.GetKeywordPerformanceReport(campaignId, startDate, endDate);
                }

                var listsSubType = new List<string> { "108", "109", "110", "111" };
                if (subtypes.Intersect(listsSubType).Any())
                {
                    conversionData = await _campaignMicrosoftAdService.GetConversionPerformanceReport(campaignId, startDate, endDate);
                }

                foreach (var subtype in subtypes)
                {
                    //put extra logic for mailchimp get dynamic id  // 76(82[8af9ff7617]) key: 82 Value: 8af9ff7617
                    var subtypeAndMcValue = ExtractNumber(subtype);

                    if (subtypeAndMcValue.Key == (int)ReportTypes.MSCampaignLineChart)
                    {
                        string htmlString = string.Empty;
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                        //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/msAdsLineChart.html");
                        //using (HttpClient httpclient = new HttpClient())
                        //{
                        //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                        //}

                        string path = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/msAdsLineChart.html");
                        htmlString = System.IO.File.ReadAllText(path);

                        htmlString = htmlString.Replace("_msAdsLineChartId_", "msLine" + DateTime.UtcNow.Ticks.ToString());

                        int intervalRes = campaignsData.dates.Count <= 31 ? 3 : (campaignsData.dates.Count <= 91 ? 7 : 31);
                        htmlString = htmlString.Replace("_msAdsLabelInterval_", intervalRes.ToString());

                        var labels = string.Join(",", campaignsData.dates.Select(x => "'" + x + "'"));

                        var currentData = string.Join(",", campaignsData.clickChartValue);

                        htmlString = htmlString.Replace("_msLineChartData_", currentData);
                        htmlString = htmlString.Replace("_msAdsLineChartLabel_", labels);
                        htmlString = htmlString.Replace("_msAdsLineTotal_", campaignsData.clicks);
                        htmlString = htmlString.Replace("_msAdsLineTitle_", "Clicks");

                        string uniqueKey = $"{91}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);

                        retVal.CampaignPerformace.clicks = campaignsData.clicks + "(" + campaignsData.DiffClicks + ")";
                    }
                    else if (subtypeAndMcValue.Key == (int)ReportTypes.MSCampaignBarChart)
                    {
                        string htmlString = string.Empty;
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                        //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/msAdsBarChart.html");
                        //using (HttpClient httpclient = new HttpClient())
                        //{
                        //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                        //}

                        string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/msAdsBarChart.html");
                        htmlString = System.IO.File.ReadAllText(pathGa);


                        htmlString = htmlString.Replace("_msAdsBarChartId_", "msBar" + DateTime.UtcNow.Ticks.ToString());

                        var labels = string.Join(",", campaignsData.campaignsName.Select(x => "'" + x + "'"));

                        var currentData = string.Join(",", campaignsData.clickBarChartValue);

                        htmlString = htmlString.Replace("_msAdsBarChartData_", currentData);

                        htmlString = htmlString.Replace("_msAdsBarChartLabel_", labels);

                        htmlString = htmlString.Replace("_msAdsBarChartTitle_", "Clicks");

                        htmlString = htmlString.Replace("_msAdsBarChartTotal", campaignsData.clicks);

                        string uniqueKey = $"{91}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);

                        retVal.CampaignPerformace.clicks = campaignsData.clicks + "(" + campaignsData.DiffClicks + ")";
                    }
                    else if (subtypeAndMcValue.Key == (int)ReportTypes.MSCampaignTiles)
                    {
                        string htmlString = string.Empty;
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                        //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/msAdsCampaignTiles.html");
                        //using (HttpClient httpclient = new HttpClient())
                        //{
                        //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                        //}

                        string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/msAdsCampaignTiles.html");
                        htmlString = System.IO.File.ReadAllText(pathGa);


                        htmlString = htmlString.Replace("_msAdsClicksData_", campaignsData.clicks);

                        htmlString = htmlString.Replace("_msAdImpressionsData_", campaignsData.impressions);

                        htmlString = htmlString.Replace("_msAdCTRData_", campaignsData.ctr);

                        htmlString = htmlString.Replace("_msAdAVGCPCData_", campaignsData.averageCpc);

                        htmlString = htmlString.Replace("_msAdCostData_", campaignsData.spend.ToString());

                        htmlString = htmlString.Replace("_msAdsConversions_", campaignsData.conversions.ToString());

                        htmlString = htmlString.Replace("_msAdsCoversionRate_", campaignsData.conversionRate);

                        htmlString = htmlString.Replace("_msAdsCostPerConversion_", campaignsData.costPerConversion);

                        htmlString = htmlString.Replace("_msAdsImpressionShare_", campaignsData.impressionSharePercent);

                        htmlString = htmlString.Replace("_msAdsLostImpressionShare_", campaignsData.impressionLostToBudgetPercent);

                        htmlString = htmlString.Replace("_msAdsLostImpressionShareRank_", campaignsData.impressionLostToRankAggPercent);

                        string uniqueKey = $"{91}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);

                        retVal.CampaignPerformace.clicks = campaignsData.clicks + "(" + campaignsData.DiffClicks + ")";
                        retVal.CampaignPerformace.impressions = campaignsData.impressions + "(" + campaignsData.DiffImpressions + ")";
                        retVal.CampaignPerformace.ctr = campaignsData.ctr + "(" + campaignsData.DiffCtr + ")";
                        retVal.CampaignPerformace.averageCpc = campaignsData.averageCpc + "(" + campaignsData.DiffAverageCpc + ")";
                        retVal.CampaignPerformace.spend = campaignsData.spend.ToString() + "(" + campaignsData.DiffSpend + ")";
                        retVal.CampaignPerformace.conversions = campaignsData.conversions.ToString() + "(" + campaignsData.DiffConversions + ")";
                        retVal.CampaignPerformace.conversionRate = campaignsData.conversionRate + "(" + campaignsData.DiffConversionRate + ")";
                        retVal.CampaignPerformace.costPerConversion = campaignsData.costPerConversion + "(" + campaignsData.DiffCostPerConversion + ")";
                        retVal.CampaignPerformace.impressionSharePercent = campaignsData.impressionSharePercent + "(" + campaignsData.DiffImpressionSharePercent + ")";
                        retVal.CampaignPerformace.impressionLostToBudgetPercent = campaignsData.impressionLostToBudgetPercent + "(" + campaignsData.DiffImpressionLostToBudgetPercent + ")";
                        retVal.CampaignPerformace.impressionLostToRankAggPercent = campaignsData.impressionLostToRankAggPercent + "(" + campaignsData.DiffImpressionLostToRankAggPercent + ")";
                    }
                    else if (subtypeAndMcValue.Key == (int)ReportTypes.MSCampaignTable)
                    {

                        string htmlString = string.Empty;
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                        string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/msAdsCampaignTable.html");
                        htmlString = System.IO.File.ReadAllText(pathGa);

                        //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/msAdsCampaignTable.html");
                        //using (HttpClient httpclient = new HttpClient())
                        //{
                        //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                        //}

                        // serialize object
                        var tableString = JsonConvert.SerializeObject(campaignsData.campaignPerformanceDto);

                        htmlString = htmlString.Replace("_msAdsCampaignTableId_", "msads" + DateTime.UtcNow.Ticks.ToString());

                        htmlString = htmlString.Replace("_msAdsCampaignTableData_", tableString);

                        string uniqueKey = $"{91}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);
                    }
                    else if (subtypeAndMcValue.Key == (int)ReportTypes.MSAdsGroupLineChart)
                    {
                        string htmlString = string.Empty;
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                        //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/msAdsLineChart.html");
                        //using (HttpClient httpclient = new HttpClient())
                        //{
                        //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                        //}

                        string path = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/msAdsLineChart.html");
                        htmlString = System.IO.File.ReadAllText(path);

                        htmlString = htmlString.Replace("_msAdsLineChartId_", "msLine" + DateTime.UtcNow.Ticks.ToString());

                        var labels = string.Join(",", adGroupData.dates.Select(x => "'" + x + "'"));

                        int intervalRes = adGroupData.dates.Count <= 31 ? 3 : (adGroupData.dates.Count <= 91 ? 7 : 31);
                        htmlString = htmlString.Replace("_msAdsLabelInterval_", intervalRes.ToString());

                        var currentData = string.Join(",", adGroupData.clickChartValue);

                        htmlString = htmlString.Replace("_msLineChartData_", currentData);
                        htmlString = htmlString.Replace("_msAdsLineChartLabel_", labels);
                        htmlString = htmlString.Replace("_msAdsLineTotal_", adGroupData.clicks);
                        htmlString = htmlString.Replace("_msAdsLineTitle_", "Clicks");

                        string uniqueKey = $"{91}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);

                        retVal.AdGroupPerformance.clicks = adGroupData.clicks + "(" + adGroupData.DiffClicks + ")"; ;
                    }
                    else if (subtypeAndMcValue.Key == (int)ReportTypes.MSAdsGroupBarChart)
                    {
                        string htmlString = string.Empty;
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                        //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/msAdsBarChart.html");
                        //using (HttpClient httpclient = new HttpClient())
                        //{
                        //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                        //}

                        string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/msAdsBarChart.html");
                        htmlString = System.IO.File.ReadAllText(pathGa);


                        htmlString = htmlString.Replace("_msAdsBarChartId_", "msBar" + DateTime.UtcNow.Ticks.ToString());

                        var labels = string.Join(",", adGroupData.adGroupName.Select(x => "'" + x + "'"));

                        var currentData = string.Join(",", adGroupData.clickBarChartValue);

                        htmlString = htmlString.Replace("_msAdsBarChartData_", currentData);

                        htmlString = htmlString.Replace("_msAdsBarChartLabel_", labels);

                        htmlString = htmlString.Replace("_msAdsBarChartTitle_", "Clicks");

                        htmlString = htmlString.Replace("_msAdsBarChartTotal", adGroupData.clicks);

                        string uniqueKey = $"{91}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);

                        retVal.AdGroupPerformance.clicks = adGroupData.clicks + "(" + adGroupData.DiffClicks + ")";
                    }
                    else if (subtypeAndMcValue.Key == (int)ReportTypes.MSAdsGroupTiles)
                    {
                        string htmlString = string.Empty;
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                        //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/msAdsGroupTiles.html");
                        //using (HttpClient httpclient = new HttpClient())
                        //{
                        //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                        //}

                        string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/msAdsGroupTiles.html");
                        htmlString = System.IO.File.ReadAllText(pathGa);


                        htmlString = htmlString.Replace("_msAdsClicksData_", adGroupData.clicks);

                        htmlString = htmlString.Replace("_msAdImpressionsData_", adGroupData.impressions);

                        htmlString = htmlString.Replace("_msAdCTRData_", adGroupData.ctr);

                        htmlString = htmlString.Replace("_msAdAVGCPCData_", adGroupData.averageCpc);

                        htmlString = htmlString.Replace("_msAdCostData_", adGroupData.spend.ToString());

                        htmlString = htmlString.Replace("_msAdsConversions_", adGroupData.conversions.ToString());

                        htmlString = htmlString.Replace("_msAdsCoversionRate_", adGroupData.conversionRate);

                        htmlString = htmlString.Replace("_msAdsCostPerConversion_", adGroupData.costPerConversion);

                        htmlString = htmlString.Replace("_msAdsImpressionShare_", adGroupData.impressionSharePercent);

                        htmlString = htmlString.Replace("_msAdsLostImpressionShare_", adGroupData.impressionLostToBudgetPercent);

                        htmlString = htmlString.Replace("_msAdsLostImpressionShareRank_", adGroupData.impressionLostToRankAggPercent);

                        string uniqueKey = $"{91}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);

                        retVal.AdGroupPerformance.clicks = adGroupData.clicks + "(" + adGroupData.DiffClicks + ")";
                        retVal.AdGroupPerformance.impressions = adGroupData.impressions + "(" + adGroupData.DiffImpressions + ")";
                        retVal.AdGroupPerformance.ctr = adGroupData.ctr + "(" + adGroupData.DiffCtr + ")";
                        retVal.AdGroupPerformance.averageCpc = adGroupData.averageCpc + "(" + adGroupData.DiffAverageCpc + ")";
                        retVal.AdGroupPerformance.spend = adGroupData.spend.ToString() + "(" + adGroupData.DiffSpend + ")";
                        retVal.AdGroupPerformance.conversions = adGroupData.conversions.ToString() + "(" + adGroupData.DiffConversions + ")";
                        retVal.AdGroupPerformance.conversionRate = adGroupData.conversionRate + "(" + adGroupData.DiffConversionRate + ")";
                        retVal.AdGroupPerformance.costPerConversion = adGroupData.costPerConversion + "(" + adGroupData.DiffCostPerConversion + ")";
                        retVal.AdGroupPerformance.impressionSharePercent = adGroupData.impressionSharePercent + "(" + adGroupData.DiffImpressionSharePercent + ")";
                        retVal.AdGroupPerformance.impressionLostToBudgetPercent = adGroupData.impressionLostToBudgetPercent + "(" + adGroupData.DiffImpressionLostToBudgetPercent + ")";
                        retVal.AdGroupPerformance.impressionLostToRankAggPercent = adGroupData.impressionLostToRankAggPercent + "(" + adGroupData.DiffImpressionLostToRankAggPercent + ")";

                    }
                    else if (subtypeAndMcValue.Key == (int)ReportTypes.MSAdsGroupTable)
                    {
                        string htmlString = string.Empty;
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                        string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/msAdsGroupTable.html");
                        htmlString = System.IO.File.ReadAllText(pathGa);

                        //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/msAdsGroupTable.html");
                        //using (HttpClient httpclient = new HttpClient())
                        //{
                        //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                        //}

                        // serialize object
                        var tableString = JsonConvert.SerializeObject(adGroupData.adGroupPerformanceDto);

                        htmlString = htmlString.Replace("_msAdsGroupCampaignTableId_", "msads" + DateTime.UtcNow.Ticks.ToString());

                        htmlString = htmlString.Replace("_msAdsGroupsTableData_", tableString);

                        string uniqueKey = $"{91}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);
                    }
                    else if (subtypeAndMcValue.Key == (int)ReportTypes.MSKeywordsLineChart)
                    {

                        string htmlString = string.Empty;
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                        //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/msAdsLineChart.html");
                        //using (HttpClient httpclient = new HttpClient())
                        //{
                        //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                        //}

                        string path = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/msAdsLineChart.html");
                        htmlString = System.IO.File.ReadAllText(path);

                        htmlString = htmlString.Replace("_msAdsLineChartId_", "msLine" + DateTime.UtcNow.Ticks.ToString());

                        var labels = string.Join(",", keywordData.dates.Select(x => "'" + x + "'"));

                        int intervalRes = keywordData.dates.Count <= 31 ? 3 : (keywordData.dates.Count <= 91 ? 7 : 31);
                        htmlString = htmlString.Replace("_msAdsLabelInterval_", intervalRes.ToString());

                        var currentData = string.Join(",", keywordData.clickChartValue);

                        htmlString = htmlString.Replace("_msLineChartData_", currentData);
                        htmlString = htmlString.Replace("_msAdsLineChartLabel_", labels);
                        htmlString = htmlString.Replace("_msAdsLineTotal_", keywordData.clicks);
                        htmlString = htmlString.Replace("_msAdsLineTitle_", "Clicks");

                        string uniqueKey = $"{91}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);

                        retVal.KeywordPerformance.clicks = keywordData.clicks + "(" + keywordData.DiffClicks + ")";
                    }
                    else if (subtypeAndMcValue.Key == (int)ReportTypes.MSKeywordsBarChart)
                    {
                        string htmlString = string.Empty;
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                        //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/msAdsKeywordStatus.html");
                        //using (HttpClient httpclient = new HttpClient())
                        //{
                        //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                        //}

                        string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/msAdsKeywordStatus.html");
                        htmlString = System.IO.File.ReadAllText(pathGa);

                        htmlString = htmlString.Replace("_msAdsBarsContainerId_", "msAds" + DateTime.UtcNow.Ticks.ToString());

                        var data = keywordData.clickBarChartValue;
                        var total = data.Sum();

                        var percentages = total == 0 ? new List<int>() : data.Select(x => (int)Math.Round((double)x / total * 100)).ToList();

                        var sourceData = string.Join(",", keywordData.clickBarChartLabel.Select(x => "'" + x + "'"));

                        var valueData = string.Join(",", data);

                        var percentageData = string.Join(",", percentages);

                        // Create a new list by selecting elements from the original list based on the count
                        htmlString = htmlString.Replace("_msAdsSourceList_", sourceData);
                        htmlString = htmlString.Replace("_msAdsValueList_", valueData);
                        htmlString = htmlString.Replace("_msAdsPercentageList_", percentageData);

                        string uniqueKey = $"{91}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);

                        retVal.KeywordPerformance.clicks = keywordData.clicks + "(" + keywordData.DiffClicks + ")";
                        retVal.KeywordPerformance.highestClickKeyword = keywordData.clickBarChartLabel.Count > 0 && keywordData.clickBarChartValue.Count > 0 ? keywordData.clickBarChartLabel[0] + " " + keywordData.clickBarChartValue[0].ToString() : "";
                    }
                    else if (subtypeAndMcValue.Key == (int)ReportTypes.MSKeywordsTiles)
                    {
                        string htmlString = string.Empty;
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                        //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/msAdsKeywordsTiles.html");
                        //using (HttpClient httpclient = new HttpClient())
                        //{
                        //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                        //}

                        string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/msAdsKeywordsTiles.html");
                        htmlString = System.IO.File.ReadAllText(pathGa);


                        htmlString = htmlString.Replace("_msAdsClicksData_", keywordData.clicks);

                        htmlString = htmlString.Replace("_msAdImpressionsData_", keywordData.impressions);

                        htmlString = htmlString.Replace("_msAdCTRData_", keywordData.ctr);

                        htmlString = htmlString.Replace("_msAdAVGCPCData_", keywordData.averageCpc);

                        htmlString = htmlString.Replace("_msAdCostData_", keywordData.spend.ToString());

                        htmlString = htmlString.Replace("_msAdsConversions_", keywordData.conversions.ToString());

                        htmlString = htmlString.Replace("_msAdsCoversionRate_", keywordData.conversionRate);

                        htmlString = htmlString.Replace("_msAdsCostPerConversion_", keywordData.costPerConversion);

                        string uniqueKey = $"{91}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);

                        retVal.KeywordPerformance.clicks = keywordData.clicks + "(" + keywordData.DiffClicks + ")";
                        retVal.KeywordPerformance.impressions = keywordData.impressions + "(" + keywordData.DiffImpressions + ")";
                        retVal.KeywordPerformance.ctr = keywordData.ctr + "(" + keywordData.DiffCtr + ")";
                        retVal.KeywordPerformance.averageCpc = keywordData.averageCpc + "(" + keywordData.DiffAverageCpc + ")";
                        retVal.KeywordPerformance.spend = keywordData.spend.ToString() + "(" + keywordData.DiffSpend + ")";
                        retVal.KeywordPerformance.conversions = keywordData.conversions.ToString() + "(" + keywordData.DiffConversions + ")";
                        retVal.KeywordPerformance.conversionRate = keywordData.conversionRate + "(" + keywordData.DiffConversionRate + ")";
                        retVal.KeywordPerformance.costPerConversion = keywordData.costPerConversion + "(" + keywordData.DiffCostPerConversion + ")";

                    }
                    else if (subtypeAndMcValue.Key == (int)ReportTypes.MSKeywordsTable)
                    {
                        string htmlString = string.Empty;
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                        string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/msAdsKeywordTable.html");
                        htmlString = System.IO.File.ReadAllText(pathGa);

                        //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/msAdsKeywordTable.html");
                        //using (HttpClient httpclient = new HttpClient())
                        //{
                        //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                        //}

                        // serialize object
                        var tableString = JsonConvert.SerializeObject(keywordData.keywordPerformanceDto);

                        htmlString = htmlString.Replace("_msAdsKeywordTableId_", "msads" + DateTime.UtcNow.Ticks.ToString());

                        htmlString = htmlString.Replace("_msAdsKeywordTableData_", tableString);

                        string uniqueKey = $"{91}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);
                    }
                    else if (subtypeAndMcValue.Key == (int)ReportTypes.MSConversionLineChart)
                    {
                        string htmlString = string.Empty;
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                        //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/msAdsLineChart.html");
                        //using (HttpClient httpclient = new HttpClient())
                        //{
                        //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                        //}

                        string path = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/msAdsLineChart.html");
                        htmlString = System.IO.File.ReadAllText(path);

                        htmlString = htmlString.Replace("_msAdsLineChartId_", "msLine" + DateTime.UtcNow.Ticks.ToString());

                        var labels = string.Join(",", conversionData.dates.Select(x => "'" + x + "'"));

                        int intervalRes = conversionData.dates.Count <= 31 ? 3 : (conversionData.dates.Count <= 91 ? 7 : 31);
                        htmlString = htmlString.Replace("_msAdsLabelInterval_", intervalRes.ToString());

                        var currentData = string.Join(",", conversionData.conversionLineChartValue);

                        htmlString = htmlString.Replace("_msLineChartData_", currentData);
                        htmlString = htmlString.Replace("_msAdsLineChartLabel_", labels);
                        htmlString = htmlString.Replace("_msAdsLineTotal_", conversionData.conversions.ToString());
                        htmlString = htmlString.Replace("_msAdsLineTitle_", "Conversions");

                        string uniqueKey = $"{91}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);

                        retVal.ConversionPerformance.conversions = conversionData.conversions + "(" + conversionData.conversions + ")";
                    }
                    else if (subtypeAndMcValue.Key == (int)ReportTypes.MSConversionBarChart)
                    {
                        string htmlString = string.Empty;
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                        //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/msAdsBarChart.html");
                        //using (HttpClient httpclient = new HttpClient())
                        //{
                        //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                        //}

                        string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/msAdsBarChart.html");
                        htmlString = System.IO.File.ReadAllText(pathGa);


                        htmlString = htmlString.Replace("_msAdsBarChartId_", "msBar" + DateTime.UtcNow.Ticks.ToString());

                        var labels = string.Join(",", conversionData.campaignsName.Select(x => "'" + x + "'"));

                        var currentData = string.Join(",", conversionData.conversionBarChartValue);

                        htmlString = htmlString.Replace("_msAdsBarChartData_", currentData);

                        htmlString = htmlString.Replace("_msAdsBarChartLabel_", labels);

                        htmlString = htmlString.Replace("_msAdsBarChartTitle_", "Conversions");

                        htmlString = htmlString.Replace("_msAdsBarChartTotal", conversionData.conversions.ToString());

                        string uniqueKey = $"{91}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);

                        retVal.ConversionPerformance.conversions = conversionData.conversions + "(" + conversionData.conversions + ")";
                    }
                    else if (subtypeAndMcValue.Key == (int)ReportTypes.MSConversionTiles)
                    {
                        string htmlString = string.Empty;
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                        //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/msAdsConversionTiles.html");
                        //using (HttpClient httpclient = new HttpClient())
                        //{
                        //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                        //}

                        string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/msAdsConversionTiles.html");
                        htmlString = System.IO.File.ReadAllText(pathGa);

                        htmlString = htmlString.Replace("_msAdsConversionsData_", conversionData.conversions.ToString());

                        htmlString = htmlString.Replace("_msAdsRevenueData_", conversionData.revenue.ToString());

                        string uniqueKey = $"{91}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);

                        retVal.ConversionPerformance.conversions = conversionData.conversions + "(" + conversionData.diffConversions + ")";
                        retVal.ConversionPerformance.revenue = conversionData.revenue + "(" + conversionData.diffRevenue + ")";
                    }
                    else if (subtypeAndMcValue.Key == (int)ReportTypes.MSConversionTable)
                    {
                        string htmlString = string.Empty;
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                        string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/msAdsConversionTable.html");
                        htmlString = System.IO.File.ReadAllText(pathGa);

                        //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/msAdsConversionTable.html");
                        //using (HttpClient httpclient = new HttpClient())
                        //{
                        //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                        //}

                        // serialize object
                        var tableString = JsonConvert.SerializeObject(conversionData.conversionPerformanceDto);

                        htmlString = htmlString.Replace("_msAdsConversionTableId_", "msads" + DateTime.UtcNow.Ticks.ToString());

                        htmlString = htmlString.Replace("_msAdsConvTableData_", tableString);

                        string uniqueKey = $"{91}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);
                    }
                    else if (subtypeAndMcValue.Key == (int)ReportTypes.MSSingleCampaign)
                    {
                        var singleCampaignsData = new RootCampaignPerformace();

                        //var adGroupData = new RootAdGroupPerformance();
                        //var keywordData = new RootKeywordPerformance();
                        //var conversionData = new RootConversionPerformance();

                        string htmlString = string.Empty;
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                        if (!string.IsNullOrEmpty(subtypeAndMcValue.Value))
                        {
                            singleCampaignsData = await _campaignMicrosoftAdService.GetCampaignPerformanceReport(campaignId, startDate, endDate, long.Parse(subtypeAndMcValue.Value));
                        }

                        //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/singleMsAdsCampaign.html");
                        //using (HttpClient httpclient = new HttpClient())
                        //{
                        //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                        //}

                        string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/singleMsAdsCampaign.html");
                        htmlString = System.IO.File.ReadAllText(pathGa);

                        htmlString = htmlString.Replace("_msAdsLineChartId_", "msLine" + DateTime.UtcNow.Ticks.ToString());

                        htmlString = htmlString.Replace("_singleMsAdsCampaignName_", singleCampaignsData.campaignsName != null ? singleCampaignsData.campaignsName.FirstOrDefault() : "");

                        var labels = string.Join(",", singleCampaignsData.dates.Select(x => "'" + x + "'"));

                        int intervalRes = singleCampaignsData.dates.Count <= 31 ? 3 : (singleCampaignsData.dates.Count <= 91 ? 7 : 31);
                        htmlString = htmlString.Replace("_msAdsLabelInterval_", intervalRes.ToString());

                        var currentData = string.Join(",", singleCampaignsData.clickChartValue);

                        htmlString = htmlString.Replace("_msLineChartData_", currentData);
                        htmlString = htmlString.Replace("_msAdsLineChartLabel_", labels);
                        htmlString = htmlString.Replace("_msAdsLineTotal_", singleCampaignsData.clicks);
                        htmlString = htmlString.Replace("_msAdsLineTitle_", "Clicks");

                        //bar

                        htmlString = htmlString.Replace("_msAdsBarChartId_", "msBar" + DateTime.UtcNow.Ticks.ToString());

                        var labels1 = string.Join(",", singleCampaignsData.campaignsName.Select(x => "'" + x + "'"));

                        var currentData1 = string.Join(",", singleCampaignsData.clickBarChartValue);

                        htmlString = htmlString.Replace("_msAdsBarChartData_", currentData1);

                        htmlString = htmlString.Replace("_msAdsBarChartLabel_", labels1);

                        htmlString = htmlString.Replace("_msAdsBarChartTitle_", "Clicks");

                        htmlString = htmlString.Replace("_msAdsBarChartTotal", singleCampaignsData.clicks);

                        //tiles
                        htmlString = htmlString.Replace("_msAdsClicksData_", singleCampaignsData.clicks);

                        htmlString = htmlString.Replace("_msAdImpressionsData_", singleCampaignsData.impressions);

                        htmlString = htmlString.Replace("_msAdCTRData_", singleCampaignsData.ctr);

                        htmlString = htmlString.Replace("_msAdAVGCPCData_", singleCampaignsData.averageCpc);

                        htmlString = htmlString.Replace("_msAdCostData_", singleCampaignsData.spend.ToString());

                        htmlString = htmlString.Replace("_msAdsConversions_", singleCampaignsData.conversions.ToString());

                        htmlString = htmlString.Replace("_msAdsCoversionRate_", singleCampaignsData.conversionRate);

                        htmlString = htmlString.Replace("_msAdsCostPerConversion_", singleCampaignsData.costPerConversion);

                        htmlString = htmlString.Replace("_msAdsImpressionShare_", singleCampaignsData.impressionSharePercent);

                        htmlString = htmlString.Replace("_msAdsLostImpressionShare_", singleCampaignsData.impressionLostToBudgetPercent);

                        htmlString = htmlString.Replace("_msAdsLostImpressionShareRank_", singleCampaignsData.impressionLostToRankAggPercent);

                        //Table
                        // serialize object
                        var tableString = JsonConvert.SerializeObject(singleCampaignsData.campaignPerformanceDto);

                        htmlString = htmlString.Replace("_msAdsCampaignTableId_", "msads" + DateTime.UtcNow.Ticks.ToString());

                        htmlString = htmlString.Replace("_msAdsCampaignTableData_", tableString);

                        string uniqueKey = $"{91}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);

                        var singleCampaign = new CampaignPerformace();

                        singleCampaign.clicks = campaignsData.clicks + "(" + campaignsData.DiffClicks + ")";
                        singleCampaign.impressions = campaignsData.impressions + "(" + campaignsData.DiffImpressions + ")";
                        singleCampaign.ctr = campaignsData.ctr + "(" + campaignsData.DiffCtr + ")";
                        singleCampaign.averageCpc = campaignsData.averageCpc + "(" + campaignsData.DiffAverageCpc + ")";
                        singleCampaign.spend = campaignsData.spend.ToString() + "(" + campaignsData.DiffSpend + ")";
                        singleCampaign.conversions = campaignsData.conversions.ToString() + "(" + campaignsData.DiffConversions + ")";
                        singleCampaign.conversionRate = campaignsData.conversionRate + "(" + campaignsData.DiffConversionRate + ")";
                        singleCampaign.costPerConversion = campaignsData.costPerConversion + "(" + campaignsData.DiffCostPerConversion + ")";
                        singleCampaign.impressionSharePercent = campaignsData.impressionSharePercent + "(" + campaignsData.DiffImpressionSharePercent + ")";
                        singleCampaign.impressionLostToBudgetPercent = campaignsData.impressionLostToBudgetPercent + "(" + campaignsData.DiffImpressionLostToBudgetPercent + ")";
                        singleCampaign.impressionLostToRankAggPercent = campaignsData.impressionLostToRankAggPercent + "(" + campaignsData.DiffImpressionLostToRankAggPercent + ")";

                        retVal.SingleCampaignPerformaceList.Add(singleCampaign);

                    }
                    else if (subtypeAndMcValue.Key == (int)ReportTypes.MSSingleAdsGroup)
                    {
                        var singleAdGroupData = new RootAdGroupPerformance();

                        string htmlString = string.Empty;
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                        if (!string.IsNullOrEmpty(subtypeAndMcValue.Value))
                        {
                            singleAdGroupData = await _campaignMicrosoftAdService.GetAdGroupPerformanceReport(campaignId, startDate, endDate, long.Parse(subtypeAndMcValue.Value));

                        }
                        //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/singleMsAdsGroups.html");
                        //using (HttpClient httpclient = new HttpClient())
                        //{
                        //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                        //}

                        string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/singleMsAdsGroups.html");
                        htmlString = System.IO.File.ReadAllText(pathGa);

                        htmlString = htmlString.Replace("_msAdsLineChartId_", "msLine" + DateTime.UtcNow.Ticks.ToString());

                        htmlString = htmlString.Replace("_singleMsAdsGroupName_", singleAdGroupData.adGroupPerformanceDto.FirstOrDefault().CampaignName);

                        var labels = string.Join(",", singleAdGroupData.dates.Select(x => "'" + x + "'"));

                        int intervalRes = singleAdGroupData.dates.Count <= 31 ? 3 : (singleAdGroupData.dates.Count <= 91 ? 7 : 31);
                        htmlString = htmlString.Replace("_msAdsLabelInterval_", intervalRes.ToString());

                        var currentData = string.Join(",", singleAdGroupData.clickChartValue);

                        htmlString = htmlString.Replace("_msLineChartData_", currentData);
                        htmlString = htmlString.Replace("_msAdsLineChartLabel_", labels);
                        htmlString = htmlString.Replace("_msAdsLineTotal_", singleAdGroupData.clicks);
                        htmlString = htmlString.Replace("_msAdsLineTitle_", "Clicks");

                        //bar

                        htmlString = htmlString.Replace("_msAdsBarChartId_", "msBar" + DateTime.UtcNow.Ticks.ToString());

                        var labels1 = string.Join(",", singleAdGroupData.adGroupName.Select(x => "'" + x + "'"));

                        var currentData1 = string.Join(",", singleAdGroupData.clickBarChartValue);

                        htmlString = htmlString.Replace("_msAdsBarChartData_", currentData1);

                        htmlString = htmlString.Replace("_msAdsBarChartLabel_", labels1);

                        htmlString = htmlString.Replace("_msAdsBarChartTitle_", "Clicks");


                        //Tiles

                        htmlString = htmlString.Replace("_msAdsBarChartTotal", singleAdGroupData.clicks);

                        htmlString = htmlString.Replace("_msAdsClicksData_", singleAdGroupData.clicks);

                        htmlString = htmlString.Replace("_msAdImpressionsData_", singleAdGroupData.impressions);

                        htmlString = htmlString.Replace("_msAdCTRData_", singleAdGroupData.ctr);

                        htmlString = htmlString.Replace("_msAdAVGCPCData_", singleAdGroupData.averageCpc);

                        htmlString = htmlString.Replace("_msAdCostData_", singleAdGroupData.spend.ToString());

                        htmlString = htmlString.Replace("_msAdsConversions_", singleAdGroupData.conversions.ToString());

                        htmlString = htmlString.Replace("_msAdsCoversionRate_", singleAdGroupData.conversionRate);

                        htmlString = htmlString.Replace("_msAdsCostPerConversion_", singleAdGroupData.costPerConversion);

                        htmlString = htmlString.Replace("_msAdsImpressionShare_", singleAdGroupData.impressionSharePercent);

                        htmlString = htmlString.Replace("_msAdsLostImpressionShare_", singleAdGroupData.impressionLostToBudgetPercent);

                        htmlString = htmlString.Replace("_msAdsLostImpressionShareRank_", singleAdGroupData.impressionLostToRankAggPercent);

                        // serialize object
                        var tableString = JsonConvert.SerializeObject(singleAdGroupData.adGroupPerformanceDto);

                        htmlString = htmlString.Replace("_msAdsGroupCampaignTableId_", "msads" + DateTime.UtcNow.Ticks.ToString());

                        htmlString = htmlString.Replace("_msAdsGroupsTableData_", tableString);

                        string uniqueKey = $"{91}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);

                        var singleAdgroup = new AdGroupPerformance();

                        singleAdgroup.clicks = singleAdGroupData.clicks + "(" + singleAdGroupData.DiffClicks + ")";
                        singleAdgroup.name = singleAdGroupData.adGroupPerformanceDto != null ? singleAdGroupData.adGroupPerformanceDto.FirstOrDefault().CampaignName : "";
                        singleAdgroup.impressions = singleAdGroupData.impressions + "(" + singleAdGroupData.DiffImpressions + ")";
                        singleAdgroup.ctr = singleAdGroupData.ctr + "(" + singleAdGroupData.DiffCtr + ")";
                        singleAdgroup.averageCpc = singleAdGroupData.averageCpc + "(" + singleAdGroupData.DiffAverageCpc + ")";
                        singleAdgroup.spend = singleAdGroupData.spend.ToString() + "(" + singleAdGroupData.DiffSpend + ")";
                        singleAdgroup.conversions = singleAdGroupData.conversions.ToString() + "(" + singleAdGroupData.DiffConversions + ")";
                        singleAdgroup.conversionRate = singleAdGroupData.conversionRate + "(" + singleAdGroupData.DiffConversionRate + ")";
                        singleAdgroup.costPerConversion = singleAdGroupData.costPerConversion + "(" + singleAdGroupData.DiffCostPerConversion + ")";
                        singleAdgroup.impressionSharePercent = singleAdGroupData.impressionSharePercent + "(" + singleAdGroupData.DiffImpressionSharePercent + ")";
                        singleAdgroup.impressionLostToBudgetPercent = singleAdGroupData.impressionLostToBudgetPercent + "(" + singleAdGroupData.DiffImpressionLostToBudgetPercent + ")";
                        singleAdgroup.impressionLostToRankAggPercent = singleAdGroupData.impressionLostToRankAggPercent + "(" + singleAdGroupData.DiffImpressionLostToRankAggPercent + ")";

                        retVal.SingleGroupPerformance.Add(singleAdgroup);
                    }
                    else if (subtypeAndMcValue.Key == (int)ReportTypes.MSSingleKeywords)
                    {
                        var singleKeywordData = new RootKeywordPerformance();

                        string htmlString = string.Empty;
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                        if (!string.IsNullOrEmpty(subtypeAndMcValue.Value))
                        {
                            singleKeywordData = await _campaignMicrosoftAdService.GetKeywordPerformanceReport(campaignId, startDate, endDate, long.Parse(subtypeAndMcValue.Value));
                        }

                        //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/singleMsAdsKeyword.html");
                        //using (HttpClient httpclient = new HttpClient())
                        //{
                        //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                        //}

                        string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/singleMsAdsKeyword.html");
                        htmlString = System.IO.File.ReadAllText(pathGa);

                        htmlString = htmlString.Replace("_singleMsAdsKeywordName_", singleKeywordData.keywordPerformanceDto.FirstOrDefault().CampaignName);

                        htmlString = htmlString.Replace("_msAdsLineChartId_", "msLine" + DateTime.UtcNow.Ticks.ToString());

                        var labels = string.Join(",", singleKeywordData.dates.Select(x => "'" + x + "'"));

                        int intervalRes = singleKeywordData.dates.Count <= 31 ? 3 : (singleKeywordData.dates.Count <= 91 ? 7 : 31);
                        htmlString = htmlString.Replace("_msAdsLabelInterval_", intervalRes.ToString());

                        var currentData = string.Join(",", singleKeywordData.clickChartValue);

                        htmlString = htmlString.Replace("_msLineChartData_", currentData);
                        htmlString = htmlString.Replace("_msAdsLineChartLabel_", labels);
                        htmlString = htmlString.Replace("_msAdsLineTotal_", singleKeywordData.clicks);
                        htmlString = htmlString.Replace("_msAdsLineTitle_", "Clicks");

                        ////progress bar

                        htmlString = htmlString.Replace("_msAdsBarsContainerId_", "msAds" + DateTime.UtcNow.Ticks.ToString());
                        var data = singleKeywordData.clickBarChartValue;
                        var total = data.Sum();

                        var percentages = total == 0 ? new List<int>() : data.Select(x => (int)Math.Round((double)x / total * 100)).ToList();

                        var sourceData = string.Join(",", singleKeywordData.clickBarChartLabel.Select(x => "'" + x + "'"));

                        var valueData = string.Join(",", data);

                        var percentageData = string.Join(",", percentages);

                        // Create a new list by selecting elements from the original list based on the count
                        htmlString = htmlString.Replace("_msAdsSourceList_", sourceData);
                        htmlString = htmlString.Replace("_msAdsValueList_", valueData);
                        htmlString = htmlString.Replace("_msAdsPercentageList_", percentageData);


                        //Tiles
                        htmlString = htmlString.Replace("_msAdsClicksData_", singleKeywordData.clicks);

                        htmlString = htmlString.Replace("_msAdImpressionsData_", singleKeywordData.impressions);

                        htmlString = htmlString.Replace("_msAdCTRData_", singleKeywordData.ctr);

                        htmlString = htmlString.Replace("_msAdAVGCPCData_", keywordData.averageCpc);

                        htmlString = htmlString.Replace("_msAdCostData_", singleKeywordData.spend.ToString());

                        htmlString = htmlString.Replace("_msAdsConversions_", singleKeywordData.conversions.ToString());

                        htmlString = htmlString.Replace("_msAdsCoversionRate_", singleKeywordData.conversionRate);

                        htmlString = htmlString.Replace("_msAdsCostPerConversion_", singleKeywordData.costPerConversion);

                        // serialize object
                        var tableString = JsonConvert.SerializeObject(singleKeywordData.keywordPerformanceDto);

                        htmlString = htmlString.Replace("_msAdsKeywordTableId_", "msads" + DateTime.UtcNow.Ticks.ToString());

                        htmlString = htmlString.Replace("_msAdsKeywordTableData_", tableString);


                        string uniqueKey = $"{91}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);

                        var singleKeyword = new KeywordPerformance();

                        singleKeyword.clicks = singleKeywordData.clicks + "(" + singleKeywordData.DiffClicks + ")";
                        singleKeyword.name = singleKeywordData.keywordPerformanceDto != null ? singleKeywordData.keywordPerformanceDto.FirstOrDefault().CampaignName : "";
                        singleKeyword.impressions = singleKeywordData.impressions + "(" + singleKeywordData.DiffClicks + ")";
                        singleKeyword.ctr = singleKeywordData.ctr + "(" + singleKeywordData.DiffClicks + ")";
                        singleKeyword.averageCpc = singleKeywordData.averageCpc + "(" + singleKeywordData.DiffClicks + ")";
                        singleKeyword.spend = singleKeywordData.spend.ToString() + "(" + singleKeywordData.DiffClicks + ")";
                        singleKeyword.conversions = singleKeywordData.conversions.ToString() + "(" + singleKeywordData.DiffClicks + ")";
                        singleKeyword.conversionRate = singleKeywordData.conversionRate + "(" + singleKeywordData.DiffClicks + ")";
                        singleKeyword.costPerConversion = singleKeywordData.costPerConversion + "(" + singleKeywordData.DiffClicks + ")";
                        singleKeyword.highestClickKeyword = singleKeywordData.clickBarChartLabel.Count > 0 && singleKeywordData.clickBarChartValue.Count > 0 ? singleKeywordData.clickBarChartLabel[0] + " " + singleKeywordData.clickBarChartValue[0].ToString() : " ";

                        retVal.SingleKeywordPerformance.Add(singleKeyword);
                    }
                    else if (subtypeAndMcValue.Key == (int)ReportTypes.MSSingleConversion)
                    {
                        var singleConversionData = new RootConversionPerformance();

                        string htmlString = string.Empty;
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();

                        if (!string.IsNullOrEmpty(subtypeAndMcValue.Value))
                        {
                            singleConversionData = await _campaignMicrosoftAdService.GetConversionPerformanceReport(campaignId, startDate, endDate, long.Parse(subtypeAndMcValue.Value));
                        }

                        //string pathPie = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/singleMsAdsConversion.html");
                        //using (HttpClient httpclient = new HttpClient())
                        //{
                        //    htmlString = httpclient.GetStringAsync(pathPie).Result;
                        //}

                        string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/singleMsAdsConversion.html");
                        htmlString = System.IO.File.ReadAllText(pathGa);

                        htmlString = htmlString.Replace("_msAdsLineChartId_", "msLine" + DateTime.UtcNow.Ticks.ToString());

                        htmlString = htmlString.Replace("_singleMsAdsConversionName_", singleConversionData.conversionPerformanceDto.FirstOrDefault().CampaignName);

                        var labels = string.Join(",", singleConversionData.dates.Select(x => "'" + x + "'"));

                        int intervalRes = singleConversionData.dates.Count <= 31 ? 3 : (singleConversionData.dates.Count <= 91 ? 7 : 31);
                        htmlString = htmlString.Replace("_msAdsLabelInterval_", intervalRes.ToString());

                        var currentData = string.Join(",", singleConversionData.conversionLineChartValue);

                        htmlString = htmlString.Replace("_msLineChartData_", currentData);
                        htmlString = htmlString.Replace("_msAdsLineChartLabel_", labels);
                        htmlString = htmlString.Replace("_msAdsLineTotal_", singleConversionData.conversions.ToString());
                        htmlString = htmlString.Replace("_msAdsLineTitle_", "Clicks");

                        //bar

                        htmlString = htmlString.Replace("_msAdsBarChartTotal", singleConversionData.conversions.ToString());

                        htmlString = htmlString.Replace("_msAdsBarChartId_", "msBar" + DateTime.UtcNow.Ticks.ToString());

                        var labels1 = string.Join(",", singleConversionData.campaignsName.Select(x => "'" + x + "'"));

                        var currentData1 = string.Join(",", singleConversionData.conversionBarChartValue);

                        htmlString = htmlString.Replace("_msAdsBarChartData_", currentData1);

                        htmlString = htmlString.Replace("_msAdsBarChartLabel_", labels1);

                        htmlString = htmlString.Replace("_msAdsBarChartTitle_", "Clicks");

                        //Tiles
                        htmlString = htmlString.Replace("_msAdsConversionsData_", singleConversionData.conversions.ToString());

                        htmlString = htmlString.Replace("_msAdsRevenueData_", singleConversionData.revenue.ToString());


                        // serialize object
                        var tableString = JsonConvert.SerializeObject(singleConversionData.conversionPerformanceDto);

                        htmlString = htmlString.Replace("_msAdsConversionTableId_", "msads" + DateTime.UtcNow.Ticks.ToString());

                        htmlString = htmlString.Replace("_msAdsConvTableData_", tableString);

                        string uniqueKey = $"{91}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);

                        var singleConversion = new ConversionPerformance();

                        singleConversion.name = singleConversionData.conversionPerformanceDto != null ? singleConversionData.conversionPerformanceDto.FirstOrDefault().CampaignName : "";
                        singleConversion.conversions = singleConversionData.conversions.ToString() + "(" + singleConversionData.diffConversions + ")"; ;
                        singleConversion.revenue = singleConversionData.revenue.ToString() + "(" + singleConversionData.diffRevenue + ")"; ;

                        retVal.SingleConversionPerformance.Add(singleConversion);
                    }
                }
            }

            retVal.HtmlList = listOfResult;

            return retVal;
        }
        public async Task<HtmlAndRawData> PrepareGa4Reports(CampaignGoogleAnalytics ga4Setup, DateTime startDate, DateTime endDate, List<string> subtypes)
        {
            var htmlAndRawData = new HtmlAndRawData();

            htmlAndRawData.Ga4RawData = new Ga4RawData();

            List<Dictionary<string, string>> listOfResult = new List<Dictionary<string, string>>();

            if (!string.IsNullOrEmpty(ga4Setup.UrlOrName))
            {
                var ga4Data = await PrepareGa4OrganicTrafficReportsByGet(ga4Setup.AccessToken, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"), ga4Setup.UrlOrName);
                var ecomData = await PrepareGa4EcomReportsByGet(ga4Setup.AccessToken, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"), ga4Setup.ProfileId);
                var purchaseJourney = await PrepareGa4PurchaseJourneyReports(ga4Setup.AccessToken, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"), ga4Setup.ProfileId);

                if (ga4Data.statusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var accessToken = await GetAccessTokenUsingRefreshToken(ga4Setup.RefreshToken);

                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        ga4Setup.AccessToken = accessToken;
                        ga4Setup.UpdatedOn = DateTime.UtcNow;
                        _campaignGoogleAnalyticsRepository.UpdateEntity(ga4Setup);
                        _campaignGoogleAnalyticsRepository.SaveChanges();

                        ga4Data = await PrepareGa4OrganicTrafficReportsByGet(ga4Setup.AccessToken, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"), ga4Setup.ProfileId);
                        ecomData = await PrepareGa4EcomReportsByGet(ga4Setup.AccessToken, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"), ga4Setup.ProfileId);
                        purchaseJourney = await PrepareGa4PurchaseJourneyReports(ga4Setup.AccessToken, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"), ga4Setup.ProfileId);
                    }
                }

                var result = await PrepareGa4ChartData(ga4Data, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

                var previousDate = CalculatePreviousStartDateAndEndDate(startDate, endDate);

                var ga4PreviousData = await PrepareGa4OrganicTrafficReportsByGet(ga4Setup.AccessToken, previousDate.PreviousStartDate.ToString("yyyy-MM-dd"), previousDate.PreviousEndDate.ToString("yyyy-MM-dd"), ga4Setup.ProfileId);
                var result1 = await PrepareGa4ChartData(ga4PreviousData, previousDate.PreviousStartDate.ToString("yyyy-MM-dd"), previousDate.PreviousEndDate.ToString("yyyy-MM-dd"));

                //labels
                var date = result.dates.ToList();
                var shortDate = date.Select(date => date.ToString()).ToArray();
                var labelStr = String.Join(",", shortDate.Select(x => "'" + x + "'"));

                foreach (var subtype in subtypes)
                {
                    var intsubtype = Convert.ToInt16(subtype);
                    if (intsubtype == (int)ReportTypes.Ga4OrganicTaffic)
                    {
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                        string htmlString = string.Empty;
                        //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/ga4Organic.html");
                        //htmlString = System.IO.File.ReadAllText(pathGa);

                        string pathGa4Organic = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/ga4Organic.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(pathGa4Organic).Result;
                        }

                        //organic previous data
                        var listOrganicPrev = result1.organicData.ToList();
                        var prevData = new List<int>();
                        listOrganicPrev.ForEach(item =>
                        {
                            prevData.Add(Convert.ToInt16(item.date));
                        });
                        var prevChartStr = String.Join(",", prevData);

                        //organic current data
                        var listOrganicCurt = result.organicData.ToList();
                        var curtData = new List<int>();
                        listOrganicCurt.ForEach(item =>
                        {
                            curtData.Add(Convert.ToInt16(item.date));
                        });
                        var curtChartStr = String.Join(",", curtData);


                        var trafficData = curtData.Sum(x => x) + "--" + prevData.Sum(x => x);
                        var trafficDifference = PrepareDataGa4(trafficData);

                        htmlString = htmlString.Replace("_trafficDifference_", trafficDifference);

                        htmlString = htmlString.Replace("_1gaOrganicTrafficLables1_", labelStr);
                        htmlString = htmlString.Replace("_1gaOrganicTrafficData1_", curtChartStr);
                        htmlString = htmlString.Replace("_2gaOrganicTrafficData2_", prevChartStr);

                        int intervalRes = date.Count <= 31 ? 3 : (date.Count <= 91 ? 7 : 31);
                        htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());

                        string uniqueKey = $"{14}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);

                        //Added raw data for chat gpt
                        htmlAndRawData.Ga4RawData.OrganicSessions = trafficDifference;
                    }
                    else if (intsubtype == (int)ReportTypes.Ga4OrganicConversion)
                    {
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                        string htmlString = string.Empty;
                        //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/ga4Conversion.html");
                        //htmlString = System.IO.File.ReadAllText(pathGa);

                        string pathGa4Conversion = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/ga4Conversion.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(pathGa4Conversion).Result;
                        }

                        //conversion previous data
                        var listConversionPrev = result1.conversionData.ToList();
                        var prevConvData = new List<int>();
                        listConversionPrev.ForEach(item =>
                        {
                            prevConvData.Add(Convert.ToInt16(item.date));
                        });
                        var prevConvChartStr = String.Join(",", prevConvData);

                        //conversion current data
                        var listConversionCurt = result.conversionData.ToList();
                        var curtConvData = new List<int>();
                        listConversionCurt.ForEach(item =>
                        {
                            curtConvData.Add(Convert.ToInt16(item.date));
                        });
                        var curtConvChartStr = String.Join(",", curtConvData);



                        var conversionData = curtConvData.Sum(x => x) + "--" + prevConvData.Sum(x => x);

                        var conDifference = PrepareDataGa4(conversionData);

                        htmlString = htmlString.Replace("_conDifference_", conDifference);

                        htmlString = htmlString.Replace("_1gaConverionLabels1_", labelStr);
                        htmlString = htmlString.Replace("_1gaConverionData1_", curtConvChartStr);
                        htmlString = htmlString.Replace("_2gaConverionData2_", prevConvChartStr);
                        int intervalRes = date.Count <= 31 ? 3 : (date.Count <= 91 ? 7 : 31);
                        htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());


                        string uniqueKey = $"{14}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);
                        //Added raw data for chat gpt
                        htmlAndRawData.Ga4RawData.OrganicConversions = conDifference;
                    }
                    else if (intsubtype == (int)ReportTypes.Ga4UserAquasition)
                    {
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                        string htmlString = string.Empty;
                        //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/ga4UserAqa.html");
                        //htmlString = System.IO.File.ReadAllText(pathGa);

                        string pathGa4UserAqua = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/ga4UserAqa.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(pathGa4UserAqua).Result;
                        }

                        //User Aquisition line chart

                        htmlString = htmlString.Replace("_dataset1Data_", string.Join(",", result.userAcquisition.Direct));
                        htmlString = htmlString.Replace("_dataset2Data_", string.Join(",", result.userAcquisition.OrganicSearch));
                        htmlString = htmlString.Replace("_dataset3Data_", string.Join(",", result.userAcquisition.OrganicSocial));
                        htmlString = htmlString.Replace("_dataset4Data_", string.Join(",", result.userAcquisition.Referral));
                        htmlString = htmlString.Replace("_dataset5Data_", string.Join(",", result.userAcquisition.Unassigned));

                        // Current Data
                        var totalDirect = result.userAcquisition?.Direct?.Count > 0
                            ? result.userAcquisition.Direct.Select(int.Parse).Sum().ToString()
                            : "0";

                        var totalOrganic = result.userAcquisition?.OrganicSearch?.Count > 0
                            ? result.userAcquisition.OrganicSearch.Select(int.Parse).Sum().ToString()
                            : "0";

                        var totalOrganicSocial = result.userAcquisition?.OrganicSocial?.Count > 0
                            ? result.userAcquisition.OrganicSocial.Select(int.Parse).Sum().ToString()
                            : "0";

                        var totalReff = result.userAcquisition?.Referral?.Count > 0
                            ? result.userAcquisition.Referral.Select(int.Parse).Sum().ToString()
                            : "0";

                        var totalUnass = result.userAcquisition?.Unassigned?.Count > 0
                            ? result.userAcquisition.Unassigned.Select(int.Parse).Sum().ToString()
                            : "0";

                        // Previous Data
                        var totalDirectPrev = result1.userAcquisition?.Direct?.Count > 0
                            ? result1.userAcquisition.Direct.Select(int.Parse).Sum().ToString()
                            : "0";

                        var totalOrganicPrev = result1.userAcquisition?.OrganicSearch?.Count > 0
                            ? result1.userAcquisition.OrganicSearch.Select(int.Parse).Sum().ToString()
                            : "0";

                        var totalOrganicSocialPrev = result1.userAcquisition?.OrganicSocial?.Count > 0
                            ? result1.userAcquisition.OrganicSocial.Select(int.Parse).Sum().ToString()
                            : "0";

                        var totalReffPrev = result1.userAcquisition?.Referral?.Count > 0
                            ? result1.userAcquisition.Referral.Select(int.Parse).Sum().ToString()
                            : "0";

                        var totalUnassPrev = result1.userAcquisition?.Unassigned?.Count > 0
                            ? result1.userAcquisition.Unassigned.Select(int.Parse).Sum().ToString()
                            : "0";

                        //User Aquisition bar chart

                        htmlString = htmlString.Replace("_1gaConverionLabels1_", labelStr);


                        htmlString = htmlString.Replace("_userAquDataCurrent_", totalDirect + "," + totalOrganic + "," + totalOrganicSocial + "," + totalReff + "," + totalUnass);
                        htmlString = htmlString.Replace("_userAquDataPrev_", totalDirectPrev + "," + totalOrganicPrev + "," + totalOrganicSocialPrev + "," + totalReffPrev + "," + totalUnassPrev);

                        var totalAqua = Int32.Parse(totalDirect) + Int32.Parse(totalOrganic) + Int32.Parse(totalOrganicSocial) + Int32.Parse(totalReff) + Int32.Parse(totalUnass);
                        var totalAquaPrev = Int32.Parse(totalDirectPrev) + Int32.Parse(totalOrganicPrev) + Int32.Parse(totalOrganicSocialPrev) + Int32.Parse(totalReffPrev) + Int32.Parse(totalUnassPrev);

                        var userAquData = totalAqua.ToString() + "--" + totalAquaPrev.ToString();

                        var userAquaDiff = PrepareDataGa4(userAquData);

                        htmlString = htmlString.Replace("_userAquaDiff_", userAquaDiff);

                        var str1 = userAquaDiff;
                        var hasPlusSign1 = str1.Contains("+");
                        if (hasPlusSign1)
                        {
                            htmlString = htmlString.Replace("_userAquaDiffColor_", "green");
                        }
                        else
                        {
                            htmlString = htmlString.Replace("_userAquaDiffColor_", "red");
                        }

                        int intervalRes = date.Count <= 31 ? 3 : (date.Count <= 91 ? 7 : 31);
                        htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());


                        string uniqueKey = $"{14}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);
                        //Added raw data for chat gpt
                        htmlAndRawData.Ga4RawData.NewUserAcquisition = userAquaDiff;
                    }
                    else if (intsubtype == (int)ReportTypes.Ga4TrafficAquasition)
                    {
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                        string htmlString = string.Empty;
                        //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/ga4TrafficAqa.html");
                        //htmlString = System.IO.File.ReadAllText(pathGa);

                        string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/ga4TrafficAqa.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(pathGa).Result;
                        }

                        htmlString = htmlString.Replace("_1gaConverionLabels1_", labelStr);

                        //Traffic Aquasition
                        htmlString = htmlString.Replace("_dataset1TrafficData_", string.Join(",", result.trafficAcquisition.Direct));
                        htmlString = htmlString.Replace("_dataset2TrafficData_", string.Join(",", result.trafficAcquisition.OrganicSearch));
                        htmlString = htmlString.Replace("_dataset3TrafficData_", string.Join(",", result.trafficAcquisition.OrganicSocial));
                        htmlString = htmlString.Replace("_dataset4TrafficData_", string.Join(",", result.trafficAcquisition.Referral));
                        htmlString = htmlString.Replace("_dataset5TrafficData_", string.Join(",", result.trafficAcquisition.Unassigned));

                        // Current Data
                        var totalDirectTra = result.trafficAcquisition?.Direct?.Count > 0
                            ? result.trafficAcquisition.Direct.Select(int.Parse).Sum().ToString()
                            : "0";

                        var totalOrganicTra = result.trafficAcquisition?.OrganicSearch?.Count > 0
                            ? result.trafficAcquisition.OrganicSearch.Select(int.Parse).Sum().ToString()
                            : "0";

                        var totalOrganicSocialTra = result.trafficAcquisition?.OrganicSocial?.Count > 0
                            ? result.trafficAcquisition.OrganicSocial.Select(int.Parse).Sum().ToString()
                            : "0";

                        var totalReffTra = result.trafficAcquisition?.Referral?.Count > 0
                            ? result.trafficAcquisition.Referral.Select(int.Parse).Sum().ToString()
                            : "0";

                        var totalUnassTra = result.trafficAcquisition?.Unassigned?.Count > 0
                            ? result.trafficAcquisition.Unassigned.Select(int.Parse).Sum().ToString()
                            : "0";

                        // Previous Data
                        var totalDirectPrevTra = result1.trafficAcquisition?.Direct?.Count > 0
                            ? result1.trafficAcquisition.Direct.Select(int.Parse).Sum().ToString()
                            : "0";

                        var totalOrganicPrevTra = result1.trafficAcquisition?.OrganicSearch?.Count > 0
                            ? result1.trafficAcquisition.OrganicSearch.Select(int.Parse).Sum().ToString()
                            : "0";

                        var totalOrganicSocialPrevTra = result1.trafficAcquisition?.OrganicSocial?.Count > 0
                            ? result1.trafficAcquisition.OrganicSocial.Select(int.Parse).Sum().ToString()
                            : "0";

                        var totalReffPrevTra = result1.trafficAcquisition?.Referral?.Count > 0
                            ? result1.trafficAcquisition.Referral.Select(int.Parse).Sum().ToString()
                            : "0";

                        var totalUnassPrevTra = result1.trafficAcquisition?.Unassigned?.Count > 0
                            ? result1.trafficAcquisition.Unassigned.Select(int.Parse).Sum().ToString()
                            : "0";

                        //Traffic Aquisition bar chart
                        //htmlString = htmlString.Replace("_1gaConverionLabels1_", labelStr);
                        htmlString = htmlString.Replace("_trafficAquDataCurrent_", totalDirectTra + "," + totalOrganicTra + "," + totalOrganicSocialTra + "," + totalReffTra + "," + totalUnassTra);
                        htmlString = htmlString.Replace("_trafficAquDataPrev_", totalDirectPrevTra + "," + totalOrganicPrevTra + "," + totalOrganicSocialPrevTra + "," + totalReffPrevTra + "," + totalUnassPrevTra);

                        var totalAquaTra = Int32.Parse(totalDirectTra) + Int32.Parse(totalOrganicTra) + Int32.Parse(totalOrganicSocialTra) + Int32.Parse(totalReffTra) + Int32.Parse(totalUnassTra);
                        var totalAquaPrevTra = Int32.Parse(totalDirectPrevTra) + Int32.Parse(totalOrganicPrevTra) + Int32.Parse(totalOrganicSocialPrevTra) + Int32.Parse(totalReffPrevTra) + Int32.Parse(totalUnassPrevTra);

                        var AquDataTra = totalAquaTra.ToString() + "--" + totalAquaPrevTra.ToString();

                        var AquaDiffTra = PrepareDataGa4(AquDataTra);

                        htmlString = htmlString.Replace("_trafficAquaDiff_", AquaDiffTra);

                        var str2 = AquaDiffTra;
                        var hasPlusSign1Tra = str2.Contains("+");
                        if (hasPlusSign1Tra)
                        {
                            htmlString = htmlString.Replace("_trafficAquaDiffColor_", "green");
                        }
                        else
                        {
                            htmlString = htmlString.Replace("_trafficAquaDiffColor_", "red");
                        }
                        int intervalRes = date.Count <= 31 ? 3 : (date.Count <= 91 ? 7 : 31);
                        htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());


                        string uniqueKey = $"{14}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);
                        htmlAndRawData.Ga4RawData.TrafficAcquisition = AquaDiffTra;
                    }
                    else if (intsubtype == (int)ReportTypes.Ga4EcomPurchase)
                    {
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                        string htmlString = string.Empty;
                        //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/ga4EcomPurchase.html");
                        //htmlString = System.IO.File.ReadAllText(pathGa);

                        string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/ga4EcomPurchase.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(pathGa).Result;
                        }

                        htmlString = htmlString.Replace("_1gaConverionLabels1_", labelStr);

                        //Ecom Purchase
                        List<Dataset> datasets = new List<Dataset>();

                        if (ecomData.Count > 4)
                        {
                            ecomData = ecomData.Take(5).ToList();
                        }
                        else
                        {
                            ecomData = ecomData.Take(ecomData.Count).ToList();
                        }

                        for (int i = 0; i < ecomData.Count; i++)
                        {
                            Dataset dataset = new Dataset
                            {
                                fill = false,
                                data = ecomData[i].ItemPurchased,
                                label = ecomData[i].ItemName,
                                backgroundColor = GetBackgroundColor(i),
                                borderColor = GetBackgroundColor(i)
                            };

                            datasets.Add(dataset);
                        }

                        // Convert to JSON
                        string jsonData = JsonConvert.SerializeObject(datasets, Formatting.Indented);

                        htmlString = htmlString.Replace("_ecomPurchaseData_", jsonData);

                        var tableListStr1 = JsonConvert.SerializeObject(ecomData, Formatting.Indented);
                        htmlString = htmlString.Replace("_ecomtableArrayList_", tableListStr1);

                        int intervalRes = date.Count <= 31 ? 3 : (date.Count <= 91 ? 7 : 31);
                        htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());


                        string uniqueKey = $"{14}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);

                        htmlAndRawData.Ga4RawData.EcommerceTotalRevenue = ecomData.Sum(x => Convert.ToDecimal(x.TotalRevenue));
                    }
                    else if (intsubtype == (int)ReportTypes.Ga4PurchaseJourney)
                    {
                        Dictionary<string, string> uniqueTypeSubtypeResults = new Dictionary<string, string>();
                        string htmlString = string.Empty;
                        //string pathGa = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf/ga4Purchase.html");
                        //htmlString = System.IO.File.ReadAllText(pathGa);


                        string pathGa = Path.Combine(_configuration.GetSection("BlobUrl").Value, "HtmlToPdf/ga4Purchase.html");
                        using (HttpClient httpclient = new HttpClient())
                        {
                            htmlString = httpclient.GetStringAsync(pathGa).Result;
                        }

                        htmlString = htmlString.Replace("_1gaConverionLabels1_", labelStr);

                        //Purchase Journey
                        var pj = purchaseJourney.PurchaseTotalSessionStart + "," + purchaseJourney.PurchaseTotalViewItem + ","
                            + purchaseJourney.PurchaseTotalAddedCart + "," + purchaseJourney.PurchaseTotalCheckout + "," + purchaseJourney.PurchaseTotalPurchase;
                        htmlString = htmlString.Replace("_purchaseData_", pj);

                        int intervalRes = date.Count <= 31 ? 3 : (date.Count <= 91 ? 7 : 31);
                        htmlString = htmlString.Replace("_labelInterval_", intervalRes.ToString());


                        string uniqueKey = $"{14}({string.Join(",", subtype)})";
                        uniqueTypeSubtypeResults[uniqueKey] = htmlString;
                        listOfResult.Add(uniqueTypeSubtypeResults);
                    }
                }
            }

            htmlAndRawData.htmlString = listOfResult;

            return htmlAndRawData;
        }

        private static string GetBackgroundColor(int index)
        {
            string[] colors = { "rgb(8, 120, 179)", "rgb(71, 71, 235)", "rgb(24, 107, 216)", "rgb(107, 47, 190)", "rgb(107, 11, 142)" };
            return index < colors.Length ? colors[index] : "rgb(0, 0, 0)";
        }




        private async Task<PageSpeedDesktopDataDto> PrepareGetSiteSpeedDataDesktop(string htmlString, string url)
        {
            PageSpeedDesktopDataDto pageSpeedDesktopDataDto = new PageSpeedDesktopDataDto();
            var urlCamp = String.Empty;
            var addString = "https://";

            if (url.Contains("https://"))
            {
                url = url.Replace("https://", "");
                url = url.Replace("/", "");
            }
            else if (url.Contains("http://"))
            {
                url = url.Replace("http://", "");
                url = url.Replace("/", "");
            }

            urlCamp = addString + url;

            //&strategy=DESKTOP

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://content-pagespeedonline.googleapis.com/")
            };

            var prepareUrl = "pagespeedonline/v5/runPagespeed?category=PERFORMANCE&url=" + urlCamp + "&strategy=DESKTOP&key=" + _configuration.GetSection("GoogleApiKey").Value;
            ////Get task status

            try
            {

                var response = await httpClient.GetAsync(prepareUrl);
                var data = await response.Content.ReadAsStringAsync();

                var res = JsonConvert.DeserializeObject<dynamic>(data);

                var lightHouse = res["lighthouseResult"];

                var first_contentful_paint_Desktop = lightHouse.audits["first-contentful-paint"].displayValue;
                var speed_index_Desktop = lightHouse.audits["speed-index"].displayValue;
                var largest_contentful_paint_Desktop = lightHouse.audits["largest-contentful-paint"].displayValue;
                var interactive_Desktop = lightHouse.audits["interactive"].displayValue;
                var total_blocking_time_Desktop = lightHouse.audits["total-blocking-time"].displayValue;
                var cumulative_layout_shift_Desktop = lightHouse.audits["cumulative-layout-shift"].displayValue;


                var msFcpDesktop = lightHouse.audits["first-contentful-paint"].score * 100;
                var msSiDesktop = lightHouse.audits["speed-index"].score * 100;
                var msLcpDesktop = lightHouse.audits["largest-contentful-paint"].score * 100;
                var msToiDesktop = lightHouse.audits["interactive"].score * 100;
                var msTbtDesktop = lightHouse.audits["total-blocking-time"].score * 100;
                var msClsDesktop = lightHouse.audits["cumulative-layout-shift"].score * 100;

                var performanceScoreDesktop = ((msFcpDesktop * 0.10) + (msSiDesktop * 0.10) + (msLcpDesktop * 0.25) + (msToiDesktop * 0.10) + (msTbtDesktop * 0.30) + (msClsDesktop * 0.15));

                performanceScoreDesktop = Math.Round(Convert.ToDecimal(performanceScoreDesktop));

                if (performanceScoreDesktop >= 0 && performanceScoreDesktop <= 49)
                {

                    htmlString = htmlString.Replace("performanceScoreDesktop", performanceScoreDesktop);
                    htmlString = htmlString.Replace("PieColorDesktop", "#dc3545");

                }
                else if (performanceScoreDesktop >= 50 && performanceScoreDesktop <= 89)
                {
                    htmlString = htmlString.Replace("performanceScoreDesktop", performanceScoreDesktop.ToString());
                    htmlString = htmlString.Replace("PieColorDesktop", "#ffc107");

                }
                else
                {
                    htmlString = htmlString.Replace("performanceScoreDesktop", performanceScoreDesktop.ToString());
                    htmlString = htmlString.Replace("PieColorDesktop", "#28a745");

                }

                pageSpeedDesktopDataDto.DesktopPerformanceScore = performanceScoreDesktop.ToString();

                string first_contentful_paint_Desktop_String = first_contentful_paint_Desktop;
                first_contentful_paint_Desktop_String = first_contentful_paint_Desktop_String.Replace("s", "").TrimEnd();
                decimal first_contentful_paint_Desktop_Decimal = Decimal.Parse(first_contentful_paint_Desktop_String.ToString());

                string speed_index_Desktop_String = speed_index_Desktop;
                speed_index_Desktop_String = speed_index_Desktop_String.Replace("s", "").TrimEnd();
                decimal speed_index_Desktop_Decimal = Decimal.Parse(speed_index_Desktop_String.ToString());

                string largest_contentful_paint_Desktop_String = largest_contentful_paint_Desktop;
                largest_contentful_paint_Desktop_String = largest_contentful_paint_Desktop_String.Replace("s", "").TrimEnd();
                decimal largest_contentful_paint_Desktop_Decimal = Decimal.Parse(largest_contentful_paint_Desktop_String.ToString());

                string interactive_Desktop_String = interactive_Desktop;
                interactive_Desktop_String = interactive_Desktop_String.Replace("s", "").TrimEnd();
                decimal interactive_Desktop_Decimal = Decimal.Parse(largest_contentful_paint_Desktop_String.ToString());

                var fcp = (first_contentful_paint_Desktop_Decimal * 1000);

                var si = (speed_index_Desktop_Decimal * 1000);

                var lcp = (largest_contentful_paint_Desktop_Decimal * 1000);

                var tot = (interactive_Desktop_Decimal * 1000);

                string total_blocking_time_Desktop_String = total_blocking_time_Desktop;
                total_blocking_time_Desktop_String = total_blocking_time_Desktop_String.Replace("ms", "").TrimEnd();
                decimal tbtDeci = Decimal.Parse(total_blocking_time_Desktop_String.ToString());

                string greenFile = "https://abhisiblob.blob.core.windows.net/abhisi/green-circle-icon.png";
                string redFile = "https://abhisiblob.blob.core.windows.net/abhisi/triangle.png";
                string orangeFile = "https://abhisiblob.blob.core.windows.net/abhisi/orange.png";

                if (fcp < 930)
                {
                    htmlString = htmlString.Replace("_dataShowZoneFCP_Desktop", fcp.ToString());
                    htmlString = htmlString.Replace("_showZoneFCP_Desktop_fileName", greenFile);

                }
                else if ((fcp < 1590) && (fcp > 930))
                {
                    htmlString = htmlString.Replace("_dataShowZoneFCP_Desktop", fcp.ToString());
                    htmlString = htmlString.Replace("_showZoneFCP_Desktop_fileName", orangeFile);

                }
                else
                {
                    htmlString = htmlString.Replace("_dataShowZoneFCP_Desktop", fcp.ToString());
                    htmlString = htmlString.Replace("_showZoneFCP_Desktop_fileName", redFile);

                }

                if ((si <= 1290))
                {
                    htmlString = htmlString.Replace("_1dataShowZoneSI_Desktop", si.ToString());
                    htmlString = htmlString.Replace("_1showZoneSI_Desktop_fileName", greenFile);

                }
                else if ((si > 1290) && (si < 2300))
                {
                    htmlString = htmlString.Replace("_1dataShowZoneSI_Desktop", si.ToString());
                    htmlString = htmlString.Replace("_1showZoneSI_Desktop_fileName", orangeFile);
                }
                else
                {
                    htmlString = htmlString.Replace("_1dataShowZoneSI_Desktop", si.ToString());
                    htmlString = htmlString.Replace("_1showZoneSI_Desktop_fileName", redFile);

                }

                if ((lcp <= 1200))
                {
                    htmlString = htmlString.Replace("_2dataShowZoneLCP_Desktop", lcp.ToString());
                    htmlString = htmlString.Replace("_2showZoneLCP_Desktop", greenFile);

                }
                else if ((lcp > 1200) && (lcp < 2390))
                {
                    htmlString = htmlString.Replace("_2dataShowZoneLCP_Desktop", lcp.ToString());
                    htmlString = htmlString.Replace("_2showZoneLCP_Desktop", orangeFile);
                }
                else
                {
                    htmlString = htmlString.Replace("_2dataShowZoneLCP_Desktop", lcp.ToString());
                    htmlString = htmlString.Replace("_2showZoneLCP_Desktop", redFile);

                }

                if ((tot <= 2470))
                {
                    htmlString = htmlString.Replace("_3dataShowZoneTOT_Desktop", tot.ToString());
                    htmlString = htmlString.Replace("_3showZoneTOT_Desktop", greenFile);

                }
                else if ((tot > 2470) && (tot < 4510))
                {
                    htmlString = htmlString.Replace("_3dataShowZoneTOT_Desktop", tot.ToString());
                    htmlString = htmlString.Replace("_3showZoneTOT_Desktop", orangeFile);


                }
                else
                {
                    htmlString = htmlString.Replace("_3dataShowZoneTOT_Desktop", tot.ToString());
                    htmlString = htmlString.Replace("_3showZoneTOT_Desktop", redFile);

                }

                if ((tbtDeci <= 150))
                {
                    htmlString = htmlString.Replace("_4dataShowZoneTBT_Desktop", tbtDeci.ToString());
                    htmlString = htmlString.Replace("_4showZoneTBT_Desktop", greenFile);


                }
                else if ((tbtDeci > 150) && (tbtDeci < 350))
                {
                    htmlString = htmlString.Replace("_4dataShowZoneTBT_Desktop", tbtDeci.ToString());
                    htmlString = htmlString.Replace("_4showZoneTBT_Desktop", orangeFile);
                }
                else
                {
                    htmlString = htmlString.Replace("_4dataShowZoneTBT_Desktop", tbtDeci.ToString());
                    htmlString = htmlString.Replace("_4showZoneTBT_Desktop", redFile);

                }

                if ((cumulative_layout_shift_Desktop <= 0.10))
                {
                    htmlString = htmlString.Replace("_5dataShowZoneCLS_Desktop", cumulative_layout_shift_Desktop.ToString());
                    htmlString = htmlString.Replace("_5showZoneCLS_Desktop", greenFile);

                }
                else if ((cumulative_layout_shift_Desktop > 0.10) && (cumulative_layout_shift_Desktop < 0.25))
                {
                    htmlString = htmlString.Replace("_5dataShowZoneCLS_Desktop", cumulative_layout_shift_Desktop.ToString());
                    htmlString = htmlString.Replace("_5showZoneCLS_Desktop", orangeFile);

                }
                else
                {
                    htmlString = htmlString.Replace("_5dataShowZoneCLS_Desktop", cumulative_layout_shift_Desktop.ToString());
                    htmlString = htmlString.Replace("_5showZoneCLS_Desktop", redFile);

                }

                pageSpeedDesktopDataDto.HtmlString = htmlString;
            }

            catch (Exception e)
            {

                var error = e;
                return pageSpeedDesktopDataDto;
            }

            return pageSpeedDesktopDataDto;
        }

        private async Task<PageSpeedMobileDataDto> PrepareGetSiteSpeedDataMobile(string htmlString, string url)
        {
            PageSpeedMobileDataDto pageSpeedMobileDataDto = new PageSpeedMobileDataDto();
            var urlCamp = String.Empty;
            var addString = "https://";

            if (url.Contains("https://"))
            {
                url = url.Replace("https://", "");
                url = url.Replace("/", "");
            }
            else if (url.Contains("http://"))
            {
                url = url.Replace("http://", "");
                url = url.Replace("/", "");
            }

            urlCamp = addString + url;

            //&strategy=MOBILE

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://content-pagespeedonline.googleapis.com/")
            };

            var prepareUrl = "pagespeedonline/v5/runPagespeed?category=PERFORMANCE&url=" + urlCamp + "&strategy=MOBILE&key=" + _configuration.GetSection("GoogleApiKey").Value;
            ////Get task status

            try
            {

                var response = await httpClient.GetAsync(prepareUrl);
                var data = await response.Content.ReadAsStringAsync();

                var res = JsonConvert.DeserializeObject<dynamic>(data);

                var lightHouse = res["lighthouseResult"];

                var first_contentful_paint = lightHouse.audits["first-contentful-paint"].displayValue;
                var speed_index_Mobile = lightHouse.audits["speed-index"].displayValue;
                var largest_contentful_paint = lightHouse.audits["largest-contentful-paint"].displayValue;
                var interactive = lightHouse.audits["interactive"].displayValue;
                var total_blocking_time = lightHouse.audits["total-blocking-time"].displayValue;
                var cumulative_layout_shift = lightHouse.audits["cumulative-layout-shift"].displayValue;

                var msFcpMobile = lightHouse.audits["first-contentful-paint"].score * 100;
                var msSiMobile = lightHouse.audits["speed-index"].score * 100;
                var msLcpMobile = lightHouse.audits["largest-contentful-paint"].score * 100;
                var msToiMobile = lightHouse.audits["interactive"].score * 100;
                var msTbtMobile = lightHouse.audits["total-blocking-time"].score * 100;
                var msClsMobile = lightHouse.audits["cumulative-layout-shift"].score * 100;

                var performanceScoreMobile = ((msFcpMobile * 0.15) + (msSiMobile * 0.15) + (msLcpMobile * 0.25) + (msToiMobile * 0.15) + (msTbtMobile * 0.25) + (msClsMobile * 0.05));

                performanceScoreMobile = Math.Round(Convert.ToDecimal(performanceScoreMobile));


                if (performanceScoreMobile >= 0 && performanceScoreMobile <= 49)
                {
                    htmlString = htmlString.Replace("_mperformanceScoreMobile", performanceScoreMobile.ToString());
                    htmlString = htmlString.Replace("mPieColorMobile", "#dc3545");

                }
                else if (performanceScoreMobile >= 50 && performanceScoreMobile <= 89)
                {
                    htmlString = htmlString.Replace("_mperformanceScoreMobile", performanceScoreMobile.ToString());
                    htmlString = htmlString.Replace("mPieColorMobile", "#ffc107");

                }
                else
                {
                    htmlString = htmlString.Replace("_mperformanceScoreMobile", performanceScoreMobile.ToString());
                    htmlString = htmlString.Replace("mPieColorMobile", "#28a745");

                }

                pageSpeedMobileDataDto.MobilePerformanceScore = performanceScoreMobile.ToString();

                string first_contentful_paint_Mobile_String = first_contentful_paint;
                first_contentful_paint_Mobile_String = first_contentful_paint_Mobile_String.Replace("s", "").TrimEnd();
                decimal first_contentful_paint_Mobile_Decimal = Decimal.Parse(first_contentful_paint_Mobile_String.ToString());

                string speed_index_Mobile_String = speed_index_Mobile;
                speed_index_Mobile_String = speed_index_Mobile_String.Replace("s", "").TrimEnd();
                decimal speed_index_Mobile_Decimal = Decimal.Parse(speed_index_Mobile_String.ToString());

                string largest_contentful_paint_Mobile_String = largest_contentful_paint;
                largest_contentful_paint_Mobile_String = largest_contentful_paint_Mobile_String.Replace("s", "").TrimEnd();
                decimal largest_contentful_paint_Mobile_Decimal = Decimal.Parse(largest_contentful_paint_Mobile_String.ToString());

                string interactive_Mobile_String = interactive;
                interactive_Mobile_String = interactive_Mobile_String.Replace("s", "").TrimEnd();
                decimal interactive_Mobile_Decimal = Decimal.Parse(largest_contentful_paint_Mobile_String.ToString());

                var fcp = (first_contentful_paint_Mobile_Decimal * 1000);


                var si = (speed_index_Mobile_Decimal * 1000);

                var lcp = (largest_contentful_paint_Mobile_Decimal * 1000);


                var tot = (interactive_Mobile_Decimal * 1000);


                string total_blocking_time_Mobile_String = total_blocking_time;
                total_blocking_time_Mobile_String = total_blocking_time_Mobile_String.Replace("ms", "").TrimEnd();
                decimal tbtDeci = Decimal.Parse(total_blocking_time_Mobile_String.ToString());


                string greenFile = "https://abhisiblob.blob.core.windows.net/abhisi/green-circle-icon.png";
                string redFile = "https://abhisiblob.blob.core.windows.net/abhisi/triangle.png";
                string orangeFile = "https://abhisiblob.blob.core.windows.net/abhisi/orange.png";

                if (fcp < 2350)
                {

                    htmlString = htmlString.Replace("_mdataShowZoneFCP_MOBILE", fcp.ToString());
                    htmlString = htmlString.Replace("_mshowZoneFCP_Mobile_fileName", greenFile);

                }
                else if ((fcp < 4020) && (fcp > 2350))
                {
                    htmlString = htmlString.Replace("_mdataShowZoneFCP_MOBILE", fcp.ToString());
                    htmlString = htmlString.Replace("_mshowZoneFCP_Mobile_fileName", orangeFile);

                }
                else
                {
                    htmlString = htmlString.Replace("_mdataShowZoneFCP_MOBILE", fcp.ToString());
                    htmlString = htmlString.Replace("_mshowZoneFCP_Mobile_fileName", redFile);

                }

                if ((si <= 3340))
                {

                    htmlString = htmlString.Replace("_1mdataShowZoneSI_MOBILE", si.ToString());
                    htmlString = htmlString.Replace("_1mshowZoneSI_Mobile_fileName", greenFile);
                }
                else if ((si > 3340) && (si < 5790))
                {
                    htmlString = htmlString.Replace("_1mdataShowZoneSI_MOBILE", si.ToString());
                    htmlString = htmlString.Replace("_1mshowZoneSI_Mobile_fileName", orangeFile);

                }
                else
                {
                    htmlString = htmlString.Replace("_1mdataShowZoneSI_MOBILE", si.ToString());
                    htmlString = htmlString.Replace("_1mshowZoneSI_Mobile_fileName", redFile);

                }

                if ((lcp <= 2520))
                {

                    htmlString = htmlString.Replace("_2mdataShowZoneLCP_MOBILE", lcp.ToString());
                    htmlString = htmlString.Replace("_2mshowZoneLCP_Mobile_fileName", greenFile);
                }
                else if ((lcp > 2520) && (lcp < 3990))
                {
                    htmlString = htmlString.Replace("_2mdataShowZoneLCP_MOBILE", lcp.ToString());
                    htmlString = htmlString.Replace("_2mshowZoneLCP_Mobile_fileName", orangeFile);

                }
                else
                {
                    htmlString = htmlString.Replace("_2mdataShowZoneLCP_MOBILE", lcp.ToString());
                    htmlString = htmlString.Replace("_2mshowZoneLCP_Mobile_fileName", redFile);

                }

                if ((tot <= 3810))
                {

                    htmlString = htmlString.Replace("_3mdataShowZoneTOT_MOBILE", tot.ToString());
                    htmlString = htmlString.Replace("_3mshowZoneTOT_Mobile_fileName", greenFile);

                }
                else if ((tot > 3810) && (tot < 7310))
                {
                    htmlString = htmlString.Replace("_3mdataShowZoneTOT_MOBILE", tot.ToString());
                    htmlString = htmlString.Replace("_3mshowZoneTOT_Mobile_fileName", orangeFile);

                }
                else
                {
                    htmlString = htmlString.Replace("_3mdataShowZoneTOT_MOBILE", tot.ToString());
                    htmlString = htmlString.Replace("_3mshowZoneTOT_Mobile_fileName", redFile);

                }

                if ((tbtDeci <= 290))
                {

                    htmlString = htmlString.Replace("_4mdataShowZoneTBT_MOBILE", tot.ToString());
                    htmlString = htmlString.Replace("_4mshowZoneTOT_Mobile_fileName", greenFile);

                }
                else if ((tbtDeci > 290) && (tbtDeci < 600))
                {
                    htmlString = htmlString.Replace("_4mdataShowZoneTBT_MOBILE", tot.ToString());
                    htmlString = htmlString.Replace("_4mshowZoneTOT_Mobile_fileName", orangeFile);

                }
                else
                {
                    htmlString = htmlString.Replace("_4mdataShowZoneTBT_MOBILE", tot.ToString());
                    htmlString = htmlString.Replace("_4mshowZoneTOT_Mobile_fileName", redFile);

                }

                if ((cumulative_layout_shift <= 0.10))
                {

                    htmlString = htmlString.Replace("_5mdataShowZoneCLS_MOBILE", cumulative_layout_shift.ToString());
                    htmlString = htmlString.Replace("_5mshowZoneCLS_Mobile_fileName", greenFile);

                }
                else if ((cumulative_layout_shift > 0.10) && (cumulative_layout_shift < 0.25))
                {
                    htmlString = htmlString.Replace("_5mdataShowZoneCLS_MOBILE", cumulative_layout_shift.ToString());
                    htmlString = htmlString.Replace("_5mshowZoneCLS_Mobile_fileName", orangeFile);
                }
                else
                {
                    htmlString = htmlString.Replace("_5mdataShowZoneCLS_MOBILE", cumulative_layout_shift.ToString());
                    htmlString = htmlString.Replace("_5mshowZoneCLS_Mobile_fileName", redFile);
                }

                pageSpeedMobileDataDto.HtmlString = htmlString;
            }
            catch (Exception e)
            {

                var error = e;
                return pageSpeedMobileDataDto;
            }

            return pageSpeedMobileDataDto;
        }

        public async Task<string> PreparePageSpeedLighthouseByStrategy(string url, string strategy)
        {
            string pageSpeed = "0";

            var urlCamp = String.Empty;
            var addString = "https://";

            if (url.Contains("https://"))
            {
                url = url.Replace("https://", "");
                url = url.Replace("/", "");
            }
            else if (url.Contains("http://"))
            {
                url = url.Replace("http://", "");
                url = url.Replace("/", "");
            }

            urlCamp = addString + url;

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://content-pagespeedonline.googleapis.com/")
            };

            var prepareUrl = "pagespeedonline/v5/runPagespeed?category=PERFORMANCE&url=" + urlCamp + "&strategy=" + strategy + "&key=" + _configuration.GetSection("GoogleApiKey").Value;

            var response = await httpClient.GetAsync(prepareUrl);
            var data = await response.Content.ReadAsStringAsync();

            var res = JsonConvert.DeserializeObject<dynamic>(data);

            var lightHouse = res["lighthouseResult"];

            if (lightHouse != null)
            {
                var score = lightHouse.categories["performance"].score * 100;
                pageSpeed = Math.Round(Convert.ToDecimal(score)).ToString();
            }
            return pageSpeed;
        }

        private List<SerpKeywordDataDto> GetSerpKewordList(string startDate, string endDate, Guid campaignId)
        {
            var fromDate = DateTime.Parse(startDate);
            var toDate = DateTime.Parse(endDate).AddHours(23).AddMinutes(59).AddSeconds(59);

            // get keyowrd from current period
            var latestKeywordListData = _serpRepository.GetAllEntities(true).Where(x => x.CampaignID == campaignId && x.CreatedOn >= fromDate.ToUniversalTime() && x.CreatedOn <= toDate.ToUniversalTime()).Select(y => new SerpDto
            {
                Id = y.Id,
                CampaignID = y.CampaignID.ToString(),
                Position = y.Position,
                LocalPackCount = y.LocalPackCount,
                Keywords = y.Keywords,
                CreatedOn = y.CreatedOn,
                LocationName = y.LocationName,
                UpdatedOn = y.UpdatedOn
            })
            .ToList();

            // sort data - latest created keyword first
            var latestKeywordList = latestKeywordListData.OrderByDescending(x => x.UpdatedOn).ToList();
            var previousKeywordList = latestKeywordList.OrderBy(x => x.UpdatedOn).ToList();

            // remove duplicate keywords
            latestKeywordList = latestKeywordList.Distinct(new KeywordEqualityComparer()).ToList();
            previousKeywordList = previousKeywordList.Distinct(new KeywordEqualityComparer()).ToList();

            // add current period keywords by default into filterd data list
            List<SerpKeywordDataDto> returnData = new List<SerpKeywordDataDto>();
            returnData = latestKeywordList.Select(y => new SerpKeywordDataDto
            {
                Keyword = y.Keywords,
                CurrentPosition = y.Position.ToString(),
                PreviousPosition = y.Position.ToString(),
                change = "-",
                CurrentDate = y.UpdatedOn.ToString("dd-MMMM"),
                Location = y.LocationName,
                PreDate = y.CreatedOn.ToString("dd-MMMM"),
                LocalPackCount = y.LocalPackCount.ToString()

            }).ToList();

            foreach (var keyword in returnData)
            {
                // check is keyword exist in previous array
                // if exist then update it's PreviousPosition and Change value
                // if not exist then do nothing
                var isExist = previousKeywordList.Where(x => x.Keywords == keyword.Keyword).FirstOrDefault();
                if (isExist != null)
                {
                    var diff = Convert.ToInt64(keyword.PreviousPosition) - Convert.ToInt64(keyword.CurrentPosition);
                    keyword.PreDate = isExist.UpdatedOn.ToString("dd-MMMM");
                    if (diff > 0)
                    {
                        keyword.change = diff.ToString();
                    }
                    else if (diff < 0)
                    {
                        keyword.change = diff.ToString();
                    }

                    if (isExist.LocalPackCount > 0)
                    {
                        keyword.PreviousPosition = isExist.Position.ToString() + "&nbsp;&nbsp;<small><span><i class='fas fa-map-marker-alt'></i>&nbsp;&nbsp;" + isExist.LocalPackCount.ToString() + "</span></small>";
                    }
                    else
                    {
                        keyword.PreviousPosition = isExist.Position.ToString();
                    }


                    if (Int32.Parse(keyword.LocalPackCount) > 0)
                    {
                        keyword.CurrentPosition = keyword.CurrentPosition.ToString() + "&nbsp;&nbsp;<small><span><i class='fas fa-map-marker-alt'></i>&nbsp;&nbsp;" + keyword.LocalPackCount.ToString() + "</span></small>";
                    }


                }
            }

            var withoutDesk = returnData.Where(x => x.change != "-").ToList();
            var withDesk = returnData.Where(x => x.change == "-").ToList();
            var negPos = withoutDesk.OrderByDescending(x => Convert.ToInt64(x.change)).ToList();
            var negative = negPos.Where(x => Convert.ToInt64(x.change) < 1).ToList();
            var positive = negPos.Where(x => Convert.ToInt64(x.change) >= 1).ToList();
            foreach (var keyword in positive)
            {
                keyword.change = "<span class='text-success'><i class='fas fa-arrow-alt-circle-up' ></i></span> " + keyword.change;
            }

            foreach (var keyword in negative)
            {
                keyword.change = "<span class='text-danger'> <i class='fas fa-arrow-alt-circle-down' ></i></span> " + Math.Abs(Convert.ToDecimal(keyword.change));
            }

            var finalList = new List<SerpKeywordDataDto>();
            if (positive.Count > 0)
            {
                finalList.AddRange(positive);
            }

            if (withDesk.Count > 0)
            {
                finalList.AddRange(withDesk);
            }

            if (negative.Count > 0)
            {
                finalList.AddRange(negative);
            }

            // if keyword not get in current period then add previous period keywords by default into filterd data list
            if (finalList.Count == 0)
            {
                finalList = previousKeywordList.Select(y => new SerpKeywordDataDto
                {
                    Keyword = y.Keywords,
                    CurrentPosition = y.Position.ToString(),
                    PreviousPosition = y.Position.ToString(),
                    change = "-",
                    CurrentDate = y.UpdatedOn.ToString("MMMM-dd"),
                    Location = y.LocationName,
                    PreDate = y.CreatedOn.ToString("MMMM-dd")
                }).ToList();
            }
            return finalList;
            //// serialize object
            //var tableString = JsonConvert.SerializeObject(finalList);
            //htmlString = htmlString.Replace("_tableArrayList_", tableString);
            //return htmlString;
        }

        private string PrepareSerpKeywordData(string htmlString, string startDate, string endDate, Guid campaignId, string companyLogo, string campaignLogo, string headerText, string footerText, string headerBgColor, string headerTextColor, bool showFooter, string showPageNumberId, string showPageNumber, bool showHeader, int pageNumber)
        {

            var fromDate = DateTime.Parse(startDate);
            var toDate = DateTime.Parse(endDate).AddHours(23).AddMinutes(59).AddSeconds(59);

            // get keyowrd from current period
            var latestKeywordListData = _serpRepository.GetAllEntities(true).Where(x => x.CampaignID == campaignId && x.CreatedOn >= fromDate.ToUniversalTime() && x.CreatedOn <= toDate.ToUniversalTime()).Select(y => new SerpDto
            {
                Id = y.Id,
                CampaignID = y.CampaignID.ToString(),
                Position = y.Position,
                LocalPackCount = y.LocalPackCount,
                Keywords = y.Keywords,
                CreatedOn = y.CreatedOn,
                LocationName = y.LocationName,
                UpdatedOn = y.UpdatedOn
            })
            .ToList();

            // sort data - latest created keyword first
            var latestKeywordList = latestKeywordListData.OrderByDescending(x => x.UpdatedOn).ToList();
            var previousKeywordList = latestKeywordList.OrderBy(x => x.UpdatedOn).ToList();

            // remove duplicate keywords
            latestKeywordList = latestKeywordList.Distinct(new KeywordEqualityComparer()).ToList();
            previousKeywordList = previousKeywordList.Distinct(new KeywordEqualityComparer()).ToList();

            // add current period keywords by default into filterd data list
            List<SerpKeywordDataDto> returnData = new List<SerpKeywordDataDto>();
            returnData = latestKeywordList.Select(y => new SerpKeywordDataDto
            {
                Keyword = y.Keywords,
                CurrentPosition = y.Position.ToString(),
                PreviousPosition = y.Position.ToString(),
                change = "-",
                CurrentDate = y.UpdatedOn.ToString("dd-MMMM"),
                Location = y.LocationName,
                PreDate = y.CreatedOn.ToString("dd-MMMM"),
                LocalPackCount = y.LocalPackCount.ToString()

            }).ToList();

            foreach (var keyword in returnData)
            {
                // check is keyword exist in previous array
                // if exist then update it's PreviousPosition and Change value
                // if not exist then do nothing
                var isExist = previousKeywordList.Where(x => x.Keywords == keyword.Keyword).FirstOrDefault();
                if (isExist != null)
                {
                    var diff = Convert.ToInt64(keyword.PreviousPosition) - Convert.ToInt64(keyword.CurrentPosition);
                    keyword.PreDate = isExist.UpdatedOn.ToString("dd-MMMM");
                    if (diff > 0)
                    {
                        keyword.change = diff.ToString();
                    }
                    else if (diff < 0)
                    {
                        keyword.change = diff.ToString();
                    }

                    if (isExist.LocalPackCount > 0)
                    {
                        keyword.PreviousPosition = isExist.Position.ToString() + "&nbsp;&nbsp;<small><span><i class='fas fa-map-marker-alt'></i>&nbsp;&nbsp;" + isExist.LocalPackCount.ToString() + "</span></small>";
                    }
                    else
                    {
                        keyword.PreviousPosition = isExist.Position.ToString();
                    }


                    if (Int32.Parse(keyword.LocalPackCount) > 0)
                    {
                        keyword.CurrentPosition = keyword.CurrentPosition.ToString() + "&nbsp;&nbsp;<small><span><i class='fas fa-map-marker-alt'></i>&nbsp;&nbsp;" + keyword.LocalPackCount.ToString() + "</span></small>";
                    }


                }
            }

            var withoutDesk = returnData.Where(x => x.change != "-").ToList();
            var withDesk = returnData.Where(x => x.change == "-").ToList();
            var negPos = withoutDesk.OrderByDescending(x => Convert.ToInt64(x.change)).ToList();
            var negative = negPos.Where(x => Convert.ToInt64(x.change) < 1).ToList();
            var positive = negPos.Where(x => Convert.ToInt64(x.change) >= 1).ToList();
            foreach (var keyword in positive)
            {
                keyword.change = "<span class='text-success'><i class='fas fa-arrow-alt-circle-up' ></i></span> " + keyword.change;
            }

            foreach (var keyword in negative)
            {
                keyword.change = "<span class='text-danger'> <i class='fas fa-arrow-alt-circle-down' ></i></span> " + Math.Abs(Convert.ToDecimal(keyword.change));
            }

            var finalList = new List<SerpKeywordDataDto>();
            if (positive.Count > 0)
            {
                finalList.AddRange(positive);
            }

            if (withDesk.Count > 0)
            {
                finalList.AddRange(withDesk);
            }

            if (negative.Count > 0)
            {
                finalList.AddRange(negative);
            }

            // if keyword not get in current period then add previous period keywords by default into filterd data list
            if (finalList.Count == 0)
            {
                finalList = previousKeywordList.Select(y => new SerpKeywordDataDto
                {
                    Keyword = y.Keywords,
                    CurrentPosition = y.Position.ToString(),
                    PreviousPosition = y.Position.ToString(),
                    change = "-",
                    CurrentDate = y.UpdatedOn.ToString("MMMM-dd"),
                    Location = y.LocationName,
                    PreDate = y.CreatedOn.ToString("MMMM-dd")
                }).ToList();
            }

            // serialize object
            var tableString = JsonConvert.SerializeObject(finalList);
            htmlString = htmlString.Replace("_tableArrayList_", tableString);
            return htmlString;
        }


        public List<ReportSchedulingDto> GetReportScheduleNotificationList()
        {
            List<ReportSchedulingDto> returnData = new List<ReportSchedulingDto>();

            var reportScheduleList = _reportschedulingRepository.GetAllEntities(true).Where(x => x.Status).Select(x => new ReportSchedulingDto
            {
                Id = x.Id,
                ReportSetting = Mapper.Map<ReportSetting>(x.ReportSetting),
                ReportId = x.ReportId,
                Scheduled = x.Scheduled,
                Day = x.Day,
                ScheduleDateAndTime = x.ScheduleDateAndTime,
                EmaildIds = x.EmaildIds,
                Subject = x.Subject,
                Status = x.Status,
                HtmlHeader = x.HtmlHeader,
                HtmlFooter = x.HtmlFooter
            }).ToList();


            foreach (var report in reportScheduleList)
            {
                //if (report.Id == new Guid("26432496-d702-4126-b61d-b209f2236248"))
                //{
                // schedule is daily and schedule time is equal to current time
                if (report.Scheduled == ReportScheduleType.Daily && report.ScheduleDateAndTime.Hour == DateTime.UtcNow.Hour)
                {
                    // add to list
                    returnData.Add(report);
                }
                // schedule is weekly
                else if (report.Scheduled == ReportScheduleType.Weekly)
                {
                    // selected day is today and schedule time is equal to current time
                    if ((int)DateTime.UtcNow.DayOfWeek == report.Day && report.ScheduleDateAndTime.Hour == DateTime.UtcNow.Hour)
                    {
                        // add to list
                        returnData.Add(report);
                    }
                }
                // schedule is monthly
                else if (report.Scheduled == ReportScheduleType.Monthly)
                {
                    // selected date is today's date and schedule time is equal to current time
                    if (DateTime.UtcNow.Day == report.Day && report.ScheduleDateAndTime.Hour == DateTime.UtcNow.Hour)
                    {
                        // add to list
                        returnData.Add(report);
                    }
                }

                //}
            }

            return returnData;
        }

        public async Task<Ga4Details> PrepareGa4ChartData(GA4Root gA4Root, string startDate, string endDate)
        {
            var organic = 0;
            var conversions = 0;
            var organicList = new List<Ga4OrganicData>();
            var conversionList = new List<Ga4ConversionData>();
            var organicHolder = new List<Ga4OrganicData>();
            var conversionHolder = new List<Ga4ConversionData>();
            var ga4Details = new Ga4Details();
            var userAcquisition = new Acquisition();
            var trafficAcquisition = new Acquisition();

            if (gA4Root.rows != null && gA4Root.rows.Count > 0)
            {
                for (int i = 0; i < gA4Root.rows.Count; i++)
                {
                    if (gA4Root.rows[i].dimensionValues[1].value == "Organic Search")
                    {
                        var ga4OrganicData = new Ga4OrganicData();
                        var ga4ConversionData = new Ga4ConversionData();

                        organic = organic + Convert.ToInt16(gA4Root.rows[i].metricValues[1].value); //organic search
                        ga4OrganicData.date = gA4Root.rows[i].dimensionValues[0].value;
                        ga4OrganicData.value = Convert.ToInt32(gA4Root.rows[i].metricValues[1].value);
                        organicList.Add(ga4OrganicData);

                        conversions = conversions + Convert.ToInt16(gA4Root.rows[i].metricValues[0].value); // organic conversion
                        ga4ConversionData.date = gA4Root.rows[i].dimensionValues[0].value;
                        ga4ConversionData.value = Convert.ToInt32(gA4Root.rows[i].metricValues[0].value);
                        conversionList.Add(ga4ConversionData);
                    }
                }

                //var dates1 = gA4Root.rows.Select(row => DateTime.ParseExact(row.dimensionValues[0].value, "yyyyMMdd", null).ToString("MM-dd")).ToList();
                userAcquisition.Direct = gA4Root.rows.Where(row => row.dimensionValues.Count > 1 && row.dimensionValues[1].value == "Direct").Select(row => row.metricValues[2].value).ToList();
                userAcquisition.OrganicSearch = gA4Root.rows.Where(row => row.dimensionValues.Count > 1 && row.dimensionValues[1].value == "Organic Search").Select(row => row.metricValues[2].value).ToList();
                userAcquisition.OrganicSocial = gA4Root.rows.Where(row => row.dimensionValues.Count > 1 && row.dimensionValues[1].value == "Organic Social").Select(row => row.metricValues[2].value).ToList();
                userAcquisition.Referral = gA4Root.rows.Where(row => row.dimensionValues.Count > 1 && row.dimensionValues[1].value == "Referral").Select(row => row.metricValues[2].value).ToList();
                userAcquisition.Unassigned = gA4Root.rows.Where(row => row.dimensionValues.Count > 1 && row.dimensionValues[1].value == "Unassigned").Select(row => row.metricValues[2].value).ToList();

                trafficAcquisition.Direct = gA4Root.rows.Where(row => row.dimensionValues.Count > 1 && row.dimensionValues[1].value == "Direct").Select(row => row.metricValues[1].value).ToList();
                trafficAcquisition.OrganicSearch = gA4Root.rows.Where(row => row.dimensionValues.Count > 1 && row.dimensionValues[1].value == "Organic Search").Select(row => row.metricValues[1].value).ToList();
                trafficAcquisition.OrganicSocial = gA4Root.rows.Where(row => row.dimensionValues.Count > 1 && row.dimensionValues[1].value == "Organic Social").Select(row => row.metricValues[1].value).ToList();
                trafficAcquisition.Referral = gA4Root.rows.Where(row => row.dimensionValues.Count > 1 && row.dimensionValues[1].value == "Referral").Select(row => row.metricValues[1].value).ToList();
                trafficAcquisition.Unassigned = gA4Root.rows.Where(row => row.dimensionValues.Count > 1 && row.dimensionValues[1].value == "Unassigned").Select(row => row.metricValues[1].value).ToList();

            }

            //var gaConversions = conversionList.GroupBy(d => d.date)
            //            .Select(g => new
            //            {
            //                Value = g.Sum(s => s.value)
            //            });
            //var orgTrafficData = organicList.GroupBy(d => d.date)
            //    .Select(g => new
            //    {
            //        Value = g.Sum(s => s.value)
            //    });

            DateTime DT = DateTime.ParseExact(startDate, "yyyy-MM-dd", new System.Globalization.CultureInfo("en-US"));
            DateTime DT1 = DateTime.ParseExact(endDate, "yyyy-MM-dd", new System.Globalization.CultureInfo("en-US"));

            var dates = new List<string>();

            for (var dt = DT; dt <= DT1; dt = dt.AddDays(1))
            {
                String date = dt.ToString("MM-dd");
                dates.Add(date);
            }


            conversionList.ForEach(item =>
            {
                var type = conversionHolder.GetType();
                var prop = type.GetProperty(item.date);
                if (prop != null)
                {
                    item.date = item.date + item.value;
                    conversionHolder.Add(item);
                }
                else
                {
                    item.date = item.value.ToString();
                    conversionHolder.Add(item);
                }
            });

            organicList.ForEach(item =>
            {
                var type = organicHolder.GetType();
                var prop = type.GetProperty(item.date);
                if (prop != null)
                {
                    item.date = item.date + item.value;
                    organicHolder.Add(item);
                }
                else
                {
                    item.date = item.value.ToString();
                    organicHolder.Add(item);
                }
            });

            ga4Details.conversionData = conversionHolder;
            ga4Details.organicData = organicHolder;
            ga4Details.userAcquisition = userAcquisition;
            ga4Details.trafficAcquisition = trafficAcquisition;

            ga4Details.dates = dates;

            return ga4Details;
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
