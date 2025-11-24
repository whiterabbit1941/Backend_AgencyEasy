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
using AutoMapper;
using System.Net;
using EventManagement.Utility.Enums;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace EventManagement.Service
{
    public class CampaignService : ServiceBase<Campaign, Guid>, ICampaignService
    {

        #region PRIVATE MEMBERS

        private readonly ICampaignRepository _campaignRepository;
        private readonly IConfiguration _configuration;
        private readonly IAspUserRepository _aspuserRepository;
        private readonly ICampaignUserRepository _campaignUserRepository;

        private readonly IReportSchedulingService _reportSchedulingService;
        private readonly ICampaignGSCRepository _campaignGSCRepository;
        private readonly ICampaignGoogleAnalyticsRepository _campaignGoogleAnalyticsRepository;
        private readonly ICampaignGoogleAdsRepository _campaignGoogleAdsRepository;
        private readonly ICampaignFacebookRepository _campaignFacebookRepository;
        private readonly ICampaignFacebookAdsRepository _campaignFacebookAdsRepository;
        private readonly ICampaignInstagramRepository _campaignInstagramRepository;
        private readonly ICampaignLinkedinRepository _campaignLinkedinRepository;
        private readonly ISerpRepository _serpRepository;

        #endregion


        #region CONSTRUCTOR

        public CampaignService(ICampaignUserRepository campaignUserRepository,
            ICampaignRepository campaignRepository,
            ILogger<CampaignService> logger,
            IConfiguration configuration,
            IAspUserRepository aspuserRepository,
            IReportSchedulingService reportSchedulingService,
            ICampaignGSCRepository campaignGSCRepository,
            ICampaignGoogleAnalyticsRepository campaignGoogleAnalyticsRepository,
            ICampaignGoogleAdsRepository campaignGoogleAdsRepository,
            ICampaignFacebookRepository campaignFacebookRepository,
            ICampaignFacebookAdsRepository campaignFacebookAdsRepository,
            ICampaignInstagramRepository campaignInstagramRepository,
            ICampaignLinkedinRepository campaignLinkedinRepository,
            ISerpRepository serpRepository
            ) : base(campaignRepository, logger)
        {
            _campaignRepository = campaignRepository;
            _configuration = configuration;
            _aspuserRepository = aspuserRepository;
            _campaignUserRepository = campaignUserRepository;
            _reportSchedulingService = reportSchedulingService;
            _campaignGSCRepository = campaignGSCRepository;
            _campaignGoogleAnalyticsRepository = campaignGoogleAnalyticsRepository;
            _campaignGoogleAdsRepository = campaignGoogleAdsRepository;
            _campaignFacebookRepository = campaignFacebookRepository;
            _campaignFacebookAdsRepository = campaignFacebookAdsRepository;
            _campaignInstagramRepository = campaignInstagramRepository;
            _campaignLinkedinRepository = campaignLinkedinRepository;
            _serpRepository = serpRepository;
        }

        #endregion


        #region PUBLIC MEMBERS   

        public async Task<CampaignIntegraionDto> GetCampaignIntegrationStatus(Guid campaignId)
        {
            var returnData = new CampaignIntegraionDto();

            var gsc = await _campaignGSCRepository.ExistAsync(x => x.CampaignID == campaignId && !String.IsNullOrEmpty(x.UrlOrName));
            var ga = await _campaignGoogleAnalyticsRepository.ExistAsync(x => x.CampaignID == campaignId && !String.IsNullOrEmpty(x.UrlOrName) && !x.IsGa4);
            var ga4 = await _campaignGoogleAnalyticsRepository.ExistAsync(x => x.CampaignID == campaignId && !String.IsNullOrEmpty(x.ProfileId) && x.IsGa4);
            var gads = await _campaignGoogleAdsRepository.ExistAsync(x => x.CampaignID == campaignId && !String.IsNullOrEmpty(x.Name));
            var fb = await _campaignFacebookRepository.ExistAsync(x => x.CampaignID == campaignId && !String.IsNullOrEmpty(x.UrlOrName));
            var fbads = await _campaignFacebookAdsRepository.ExistAsync(x => x.CampaignID == campaignId && !String.IsNullOrEmpty(x.AdAccountName));
            var insta = await _campaignInstagramRepository.ExistAsync(x => x.CampaignID == campaignId && !String.IsNullOrEmpty(x.UrlOrName));
            var link = await _campaignLinkedinRepository.ExistAsync(x => x.CampaignID == campaignId && !String.IsNullOrEmpty(x.OrganizationalEntity));
            var serp = await _serpRepository.ExistAsync(x => x.CampaignID == campaignId);

            returnData.GoogleSearchConsole = gsc;
            returnData.GoogleAnalytics = ga;
            returnData.GoogleAnalytics4 = ga4;
            returnData.GoogleAds = gads;
            returnData.Facebook = fb;
            returnData.FacebookAds = fbads;
            returnData.Instagram = insta;
            returnData.LinkedIn = link;
            returnData.Keyword = serp;

            return returnData;
        }

        public List<CampaignDto> GetCampaignByUserId(string userId)
        {
            //then get the whole entity and map it to the Dto.
            var aspuserEntity = _aspuserRepository.GetAllEntities(true).Where(x => x.Id == userId).Select(user => new AspUserDto { CompanyID = user.CompanyID }).FirstOrDefault();


            //then get the whole entity and map it to the Dto.
            var campaign = _campaignRepository.GetAllEntities(true).Where(x => x.CompanyID == aspuserEntity.CompanyID).Select(y => new CampaignDto
            {
                Id = y.Id,
                Name = y.Name,
                CompanyID = y.CompanyID,
                LeadGeneration = y.LeadGeneration,
                MoreTraffic = y.MoreTraffic,
                Sales = y.Sales,
                WebUrl = y.WebUrl

            }
            ).ToList();

            return campaign;
        }

        public async Task<CampaignUserDto> AddUserToCampaign(string userId, string campaignId, string companyId)
        {
            // create CampaignUser Entity and stored in DB
            CampaignUser campaignEntity = new CampaignUser();
            campaignEntity.CreatedOn = DateTime.UtcNow;
            campaignEntity.CreatedBy = "system";
            campaignEntity.UpdatedOn = DateTime.UtcNow;
            campaignEntity.UpdatedBy = "system";
            campaignEntity.Id = new Guid();
            campaignEntity.CompanyId = new Guid(companyId);
            campaignEntity.UserId = userId;
            campaignEntity.CampaignId = new Guid(campaignId);

            _campaignUserRepository.CreateEntity(campaignEntity);
            _campaignUserRepository.SaveChanges();

            var returnData = Mapper.Map<CampaignUserDto>(campaignEntity);

            return returnData;
        }

        /// <summary>
        /// Get Update Dashboard data using webjob
        /// </summary>
        /// <returns>error or success </returns>
        public async Task<string> GetUpdateDashboard()
        {        
            var campaignRepo = _campaignRepository.GetAllEntities(true).ToList();
            var endDate = DateTime.UtcNow.Date.AddDays(-1);
            var startDate = DateTime.UtcNow.Date.AddDays(-30);

            var response = await UpdateDashboardData(campaignRepo, startDate, endDate);

            if (response != null)
            {
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

                var fromEmail = _configuration.GetSection("MailFrom").Value ?? "support@whitelabelboard.com";
                var msg = MailHelper.CreateSingleEmailToMultipleRecipients(new EmailAddress(fromEmail), notificationEmailsList,
                "Successfully: Lambda Execution For Dashboard Data", "", "Execution Successfuly For Lambda Execution For Dashboard Data");

                var res = client.SendEmailAsync(msg);

                return "Dashboard data updated sucessfully.";
            }
            else
            {
                return "Something went wrong in data update.";
            }
        }

        /// <summary>
        /// To update dashboard table when GA or GSC integrations are deleted
        /// </summary>
        /// <param name="id">Campaign id</param>
        /// <param name="type">type in int to identify GA or GSC setup</param>
        /// <returns></returns>
        public async Task UpdateDashboardTable(Guid id, int type)
        {
            if (type == (int)ReportTypes.GoogleAnalyticsFour)
            {
                var campaignDetails = _campaignGoogleAnalyticsRepository.GetEntityById(id);

                await _campaignRepository.UpdateBulkEntityAsync(y => new Campaign
                {
                    Traffic = "NA",
                    Conversions = "NA"
                }, x => x.Id == campaignDetails.CampaignID);
            }
            else if (type == (int)ReportTypes.GoogleSearchConsole)
            {
                var campaignDetailsForGsc = _campaignGSCRepository.GetEntityById(id);

                await _campaignRepository.UpdateBulkEntityAsync(y => new Campaign
                {
                    Gsc = "NA"
                }, x => x.Id == campaignDetailsForGsc.CampaignID);

            }
        }

        /// <summary>
        /// Update dashboard data
        /// </summary>
        /// <param name="campaignRepo"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<List<Campaign>> UpdateDashboardData(List<Campaign> campaignRepo, DateTime startDate, DateTime endDate)
        {
            List<Campaign> campaignList = new List<Campaign>();

            string retVal = String.Empty;

            var campaignId = new Guid();

            //var endDate = DateTime.UtcNow.Date;
            //var startDate = DateTime.UtcNow.Date.AddDays(-30);

            foreach (var campaign in campaignRepo)
            {
                Campaign camp = new Campaign();
                campaignId = campaign.Id;
                var previousDate = _reportSchedulingService.CalculatePreviousStartDateAndEndDate(startDate, endDate);

                try
                {
                    string traffic = string.Empty;
                    string conversions = string.Empty;

                    //GA4 SETUP

                    var ga4setup = _campaignGoogleAnalyticsRepository.GetAllEntities(true).Where(x => x.CampaignID == campaignId && x.IsGa4).FirstOrDefault();

                    if (ga4setup != null)
                    {
                        //Current
                        var response = await _reportSchedulingService.PrepareGa4OrganicTrafficReportsByGet(ga4setup.AccessToken, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"), ga4setup.ProfileId);
                        if (response.statusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            var accessToken = await _reportSchedulingService.GetAccessTokenUsingRefreshToken(ga4setup.RefreshToken);

                            if (!string.IsNullOrEmpty(accessToken))
                            {
                                ga4setup.AccessToken = accessToken;
                                ga4setup.UpdatedOn = DateTime.UtcNow;
                                _campaignGoogleAnalyticsRepository.UpdateEntity(ga4setup);
                                _campaignGoogleAnalyticsRepository.SaveChanges();

                                response = await _reportSchedulingService.PrepareGa4OrganicTrafficReportsByGet(ga4setup.AccessToken, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"), ga4setup.ProfileId);
                            }                            
                        }
                        var returnData = await _reportSchedulingService.PrepareGa4ChartData(response, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

                        //Previous

                        var responsePrev = await _reportSchedulingService.PrepareGa4OrganicTrafficReportsByGet(ga4setup.AccessToken, previousDate.PreviousStartDate.ToString("yyyy-MM-dd"), previousDate.PreviousEndDate.ToString("yyyy-MM-dd"), ga4setup.ProfileId);
                        if (responsePrev.statusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            var accessToken = await _reportSchedulingService.GetAccessTokenUsingRefreshToken(ga4setup.RefreshToken);

                            if (!string.IsNullOrEmpty(accessToken))
                            {
                                ga4setup.AccessToken = accessToken;
                                ga4setup.UpdatedOn = DateTime.UtcNow;
                                _campaignGoogleAnalyticsRepository.UpdateEntity(ga4setup);
                                _campaignGoogleAnalyticsRepository.SaveChanges();

                                responsePrev = await _reportSchedulingService.PrepareGa4OrganicTrafficReportsByGet(ga4setup.AccessToken, previousDate.PreviousStartDate.ToString("yyyy-MM-dd"), previousDate.PreviousEndDate.ToString("yyyy-MM-dd"), ga4setup.ProfileId);
                            }                            
                        }
                        var returnDataPrev = await _reportSchedulingService.PrepareGa4ChartData(responsePrev, previousDate.PreviousStartDate.ToString("yyyy-MM-dd"), previousDate.PreviousEndDate.ToString("yyyy-MM-dd"));


                        if (returnData.organicData.Count > 0 || returnDataPrev.organicData.Count > 0)
                        {
                            var totalTraffic = returnData.organicData.Sum(x => x.value);

                            var pretotalTraffic = returnDataPrev.organicData.Sum(x => x.value);

                            int[] oganicTraffic = new int[] { totalTraffic, pretotalTraffic };

                            traffic = string.Join("--", oganicTraffic);
                        }
                        else
                        {
                            traffic = "0--0";
                        }

                        if (returnData.conversionData.Count > 0 || returnDataPrev.conversionData.Count > 0)
                        {

                            var totalConversion = returnData.conversionData.Sum(x => x.value);

                            var prevtotalConversion = returnDataPrev.conversionData.Sum(x => x.value);

                            int[] conversionData = new int[] { totalConversion, prevtotalConversion };

                            conversions = string.Join("--", conversionData);
                        }
                        else
                        {
                            conversions = "0--0";
                        }
                    }
                    else
                    {
                        traffic = "NA";
                        conversions = "NA";
                        //NA
                    }
                    retVal = "---GA4 Succeed---";

                    await _campaignRepository.UpdateBulkEntityAsync(y => new Campaign
                    {
                        LastUpdateDashboardDate = DateTime.UtcNow,
                        Traffic = traffic,
                        Conversions = conversions,
                        ExceptionDashboardLambda = retVal
                    }, x => x.Id == campaign.Id);

                    camp.Traffic = traffic;
                    camp.Conversions = conversions;

                }
                catch (Exception ex)
                {
                    retVal = "Error in GA4---" + ex.StackTrace
                   + "Error messasge: " + ex.Message +
                   "CampaignId " + campaignId;

                    await _campaignRepository.UpdateBulkEntityAsync(y => new Campaign
                    {
                        LastUpdateDashboardDate = DateTime.UtcNow,
                        ExceptionDashboardLambda = retVal
                    }, x => x.Id == campaign.Id);
                }

                try
                {
                    HttpStatusCode gscStatusCode = HttpStatusCode.OK;
                    string gsc = string.Empty;


                    var gscSetup = _campaignGSCRepository.GetFilteredEntities(true).Where(x => x.CampaignID == campaign.Id).FirstOrDefault();

                    if (gscSetup != null)
                    {
                        var accessToken = await _reportSchedulingService.GetAccessTokenUsingRefreshToken(gscSetup.RefreshToken);

                        var googleSearchConsoleReport = await _reportSchedulingService.PrepareGoogleSearchConsole(startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"), gscSetup.UrlOrName, accessToken, previousDate, new List<string> { });

                        if (googleSearchConsoleReport != null & googleSearchConsoleReport.CurrentImpression != null & googleSearchConsoleReport.PreviousImpression != null)
                        {
                            int[] myGscImpression = new int[] { Int32.Parse(googleSearchConsoleReport.CurrentImpression), Int32.Parse(googleSearchConsoleReport.PreviousImpression) };

                            gsc = string.Join("--", myGscImpression);
                        }
                        else
                        {
                            if (googleSearchConsoleReport.StatusCode != System.Net.HttpStatusCode.OK)
                            {
                                gscStatusCode = googleSearchConsoleReport.StatusCode;
                                retVal += "---Error in Gsc---StatusCode" + googleSearchConsoleReport.StatusCode.ToString();
                                await _campaignRepository.UpdateBulkEntityAsync(y => new Campaign
                                {
                                    LastUpdateDashboardDate = DateTime.UtcNow,
                                    ExceptionDashboardLambda = retVal
                                }, x => x.Id == campaign.Id);

                            }
                            else
                            {
                                gsc = "0--0";
                            }
                        }
                    }
                    else
                    {
                        gsc = "NA";
                        //NA
                    }

                    if (gscStatusCode == HttpStatusCode.OK || gsc == "NA")
                    {
                        retVal += "---GSC Succeed---";

                        await _campaignRepository.UpdateBulkEntityAsync(y => new Campaign
                        {
                            LastUpdateDashboardDate = DateTime.UtcNow,
                            Gsc = gsc,
                            ExceptionDashboardLambda = retVal
                        }, x => x.Id == campaign.Id);

                        camp.Gsc = gsc;
                    }


                }
                catch (Exception ex)
                {
                    retVal += "---Error in Gsc---" + ex.StackTrace
                                         + "Error messasge: " + ex.Message +
                                         "CampaignId " + campaignId;

                    await _campaignRepository.UpdateBulkEntityAsync(y => new Campaign
                    {
                        LastUpdateDashboardDate = DateTime.UtcNow,
                        ExceptionDashboardLambda = retVal
                    }, x => x.Id == campaign.Id);
                }

                try
                {
                    string ranking = string.Empty;

                    var avgRankings = PrepareAvgRanking(campaign.Id, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"), previousDate);

                    if (avgRankings.Count > 0)
                    {
                        ranking = String.Join("--", avgRankings);
                    }
                    else
                    {
                        ranking = "NA";
                    }

                    retVal += "---Ranking Succeed---";
                    await _campaignRepository.UpdateBulkEntityAsync(y => new Campaign
                    {
                        LastUpdateDashboardDate = DateTime.UtcNow,
                        Ranking = ranking,
                        ExceptionDashboardLambda = retVal
                    }, x => x.Id == campaign.Id);

                    camp.Ranking = ranking;
                }
                catch (Exception ex)
                {
                    retVal += "---Error in Keywords--- " + ex.StackTrace
                    + "Error messasge: " + ex.Message +
                    "CampaignId " + campaignId;

                    await _campaignRepository.UpdateBulkEntityAsync(y => new Campaign
                    {
                        LastUpdateDashboardDate = DateTime.UtcNow,
                        ExceptionDashboardLambda = retVal
                    }, x => x.Id == campaign.Id);
                }

                camp.Id = campaign.Id;
                camp.WebUrl = campaign.WebUrl;
                camp.Name = campaign.Name;
                campaignList.Add(camp);
            }
            return campaignList;
        }

        /// <summary>
        /// Update dashboard data after anyone do setup integration
        /// </summary>
        /// <returns></returns>
        public async Task<bool> UpdateDashboardDataAfterIntegration(Guid campaignId)
        {
            string retVal = String.Empty;
            var endDate = DateTime.UtcNow.Date.AddDays(-1);
            var startDate = DateTime.UtcNow.Date.AddDays(-30);
            Campaign camp = new Campaign();            
            var previousDate = _reportSchedulingService.CalculatePreviousStartDateAndEndDate(startDate, endDate);

            try
            {
                string traffic = string.Empty;
                string conversions = string.Empty;
                //GA4 SETUP

                var ga4setup = _campaignGoogleAnalyticsRepository.GetAllEntities(true).Where(x => x.CampaignID == campaignId && x.IsGa4).FirstOrDefault();

                if (ga4setup != null)
                {
                    //Current
                    var response = await _reportSchedulingService.PrepareGa4OrganicTrafficReportsByGet(ga4setup.AccessToken, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"), ga4setup.ProfileId);
                    if (response.statusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        var accessToken = await _reportSchedulingService.GetAccessTokenUsingRefreshToken(ga4setup.RefreshToken);

                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            ga4setup.AccessToken = accessToken;
                            ga4setup.UpdatedOn = DateTime.UtcNow;
                            _campaignGoogleAnalyticsRepository.UpdateEntity(ga4setup);
                            _campaignGoogleAnalyticsRepository.SaveChanges();

                            response = await _reportSchedulingService.PrepareGa4OrganicTrafficReportsByGet(ga4setup.AccessToken, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"), ga4setup.ProfileId);
                        }                       
                    }
                    var returnData = await _reportSchedulingService.PrepareGa4ChartData(response, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

                    //Previous

                    var responsePrev = await _reportSchedulingService.PrepareGa4OrganicTrafficReportsByGet(ga4setup.AccessToken, previousDate.PreviousStartDate.ToString("yyyy-MM-dd"), previousDate.PreviousEndDate.ToString("yyyy-MM-dd"), ga4setup.ProfileId);
                    if (responsePrev.statusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        var accessToken = await _reportSchedulingService.GetAccessTokenUsingRefreshToken(ga4setup.RefreshToken);

                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            ga4setup.AccessToken = accessToken;
                            ga4setup.UpdatedOn = DateTime.UtcNow;
                            _campaignGoogleAnalyticsRepository.UpdateEntity(ga4setup);
                            _campaignGoogleAnalyticsRepository.SaveChanges();

                            responsePrev = await _reportSchedulingService.PrepareGa4OrganicTrafficReportsByGet(ga4setup.AccessToken, previousDate.PreviousStartDate.ToString("yyyy-MM-dd"), previousDate.PreviousEndDate.ToString("yyyy-MM-dd"), ga4setup.ProfileId);
                        }                       
                    }
                    var returnDataPrev = await _reportSchedulingService.PrepareGa4ChartData(responsePrev, previousDate.PreviousStartDate.ToString("yyyy-MM-dd"), previousDate.PreviousEndDate.ToString("yyyy-MM-dd"));


                    if (returnData.organicData.Count > 0 || returnDataPrev.organicData.Count > 0)
                    {
                        var totalTraffic = returnData.organicData.Sum(x => x.value);

                        var pretotalTraffic = returnDataPrev.organicData.Sum(x => x.value);

                        int[] oganicTraffic = new int[] { totalTraffic, pretotalTraffic };

                        traffic = string.Join("--", oganicTraffic);
                    }
                    else
                    {
                        traffic = "0--0";
                    }

                    if (returnData.conversionData.Count > 0 || returnDataPrev.conversionData.Count > 0)
                    {

                        var totalConversion = returnData.conversionData.Sum(x => x.value);

                        var prevtotalConversion = returnDataPrev.conversionData.Sum(x => x.value);

                        int[] conversionData = new int[] { totalConversion, prevtotalConversion };

                        conversions = string.Join("--", conversionData);
                    }
                    else
                    {
                        conversions = "0--0";
                    }
                }
                else
                {
                    traffic = "NA";
                    conversions = "NA";
                    //NA
                }

                retVal = "---GA4 Succeed---";

                await _campaignRepository.UpdateBulkEntityAsync(y => new Campaign
                {
                    LastUpdateDashboardDate = DateTime.UtcNow,
                    Traffic = traffic,
                    Conversions = conversions,
                    ExceptionDashboardLambda = retVal
                }, x => x.Id == campaignId);

                camp.Traffic = traffic;
                camp.Conversions = conversions;

            }
            catch (Exception ex)
            {
                retVal = "Error in GA4---" + ex.StackTrace
               + "Error messasge: " + ex.Message +
               "CampaignId " + campaignId;

                await _campaignRepository.UpdateBulkEntityAsync(y => new Campaign
                {
                    LastUpdateDashboardDate = DateTime.UtcNow,
                    ExceptionDashboardLambda = retVal
                }, x => x.Id == campaignId);
            }

            try
            {
                HttpStatusCode gscStatusCode = HttpStatusCode.OK;
                string gsc = string.Empty;


                var gscSetup = _campaignGSCRepository.GetFilteredEntities(true).Where(x => x.CampaignID == campaignId).FirstOrDefault();

                if (gscSetup != null)
                {
                    var accessToken = await _reportSchedulingService.GetAccessTokenUsingRefreshToken(gscSetup.RefreshToken);

                    var googleSearchConsoleReport = await _reportSchedulingService.PrepareGoogleSearchConsole( startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"), gscSetup.UrlOrName, accessToken, previousDate, new List<string>() { });

                    if (googleSearchConsoleReport != null & googleSearchConsoleReport.CurrentImpression != null & googleSearchConsoleReport.PreviousImpression != null)
                    {
                        int[] myGscImpression = new int[] { Int32.Parse(googleSearchConsoleReport.CurrentImpression), Int32.Parse(googleSearchConsoleReport.PreviousImpression) };

                        gsc = string.Join("--", myGscImpression);
                    }
                    else
                    {
                        if (googleSearchConsoleReport.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            gscStatusCode = googleSearchConsoleReport.StatusCode;
                            retVal += "---Error in Gsc---StatusCode" + googleSearchConsoleReport.StatusCode.ToString();
                            await _campaignRepository.UpdateBulkEntityAsync(y => new Campaign
                            {
                                LastUpdateDashboardDate = DateTime.UtcNow,
                                ExceptionDashboardLambda = retVal
                            }, x => x.Id == campaignId);

                        }
                        else
                        {
                            gsc = "0--0";
                        }
                    }
                }
                else
                {
                    gsc = "NA";
                    //NA
                }

                if (gscStatusCode == HttpStatusCode.OK || gsc == "NA")
                {
                    retVal += "---GSC Succeed---";

                    await _campaignRepository.UpdateBulkEntityAsync(y => new Campaign
                    {
                        LastUpdateDashboardDate = DateTime.UtcNow,
                        Gsc = gsc,
                        ExceptionDashboardLambda = retVal
                    }, x => x.Id == campaignId);

                    camp.Gsc = gsc;
                }


            }
            catch (Exception ex)
            {
                retVal += "---Error in Gsc---" + ex.StackTrace
                                     + "Error messasge: " + ex.Message +
                                     "CampaignId " + campaignId;

                await _campaignRepository.UpdateBulkEntityAsync(y => new Campaign
                {
                    LastUpdateDashboardDate = DateTime.UtcNow,
                    ExceptionDashboardLambda = retVal
                }, x => x.Id == campaignId);
            }

            try
            {
                string ranking = string.Empty;

                var avgRankings = PrepareAvgRanking(campaignId, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"), previousDate);

                if (avgRankings.Count > 0)
                {
                    ranking = String.Join("--", avgRankings);
                }
                else
                {
                    ranking = "NA";
                }

                retVal += "---Ranking Succeed---";
                await _campaignRepository.UpdateBulkEntityAsync(y => new Campaign
                {
                    LastUpdateDashboardDate = DateTime.UtcNow,
                    Ranking = ranking,
                    ExceptionDashboardLambda = retVal
                }, x => x.Id == campaignId);

                camp.Ranking = ranking;
            }
            catch (Exception ex)
            {
                retVal += "---Error in Keywords--- " + ex.StackTrace
                + "Error messasge: " + ex.Message +
                "CampaignId " + campaignId;

                await _campaignRepository.UpdateBulkEntityAsync(y => new Campaign
                {
                    LastUpdateDashboardDate = DateTime.UtcNow,
                    ExceptionDashboardLambda = retVal
                }, x => x.Id == campaignId);
            }

            if (retVal == "---GA4 Succeed------GSC Succeed------Ranking Succeed---")
            {
                return true;
            }
            else
            {
                return false;
            }

        }


        /// <summary>
        /// Update dashboard data
        /// </summary>
        /// <param name="campaignRepo"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<List<Campaign>> GetDashboardData(List<Campaign> campaignRepo, string startDate, string endDate)
        {
            List<Campaign> campaignList = new List<Campaign>();

            string retVal = String.Empty;

            var campaignId = new Guid();

            var convertedEndDate = DateTime.Parse(endDate);
            var convertedStartDate = DateTime.Parse(startDate);
            var previousDate = _reportSchedulingService.CalculatePreviousStartDateAndEndDate(convertedStartDate, convertedEndDate);

            foreach (var campaign in campaignRepo)
            {
                Campaign camp = new Campaign();
                campaignId = campaign.Id;

                try
                {
                    string traffic = string.Empty;
                    string conversions = string.Empty;

                    //GA4 SETUP

                    var ga4setup = _campaignGoogleAnalyticsRepository.GetAllEntities(true).Where(x => x.CampaignID == campaignId && x.IsGa4).FirstOrDefault();
                 
                    if (ga4setup != null)
                    {
                            //Current
                            var response = await _reportSchedulingService.PrepareGa4OrganicTrafficReportsByGet(ga4setup.AccessToken, startDate, endDate, ga4setup.ProfileId);
                            if (response.statusCode == System.Net.HttpStatusCode.Unauthorized)
                            {
                                var accessToken = await _reportSchedulingService.GetAccessTokenUsingRefreshToken(ga4setup.RefreshToken);

                                if (!string.IsNullOrEmpty(accessToken))
                                {
                                    ga4setup.AccessToken = accessToken;
                                    ga4setup.UpdatedOn = DateTime.UtcNow;
                                    _campaignGoogleAnalyticsRepository.UpdateEntity(ga4setup);
                                    _campaignGoogleAnalyticsRepository.SaveChanges();

                                    response = await _reportSchedulingService.PrepareGa4OrganicTrafficReportsByGet(ga4setup.AccessToken, startDate, endDate, ga4setup.ProfileId);
                                }                                
                            }
                            var returnData = await _reportSchedulingService.PrepareGa4ChartData(response, startDate, endDate);

                        //Previous

                        var responsePrev = await _reportSchedulingService.PrepareGa4OrganicTrafficReportsByGet(ga4setup.AccessToken, previousDate.PreviousStartDate.ToString("yyyy-MM-dd"), previousDate.PreviousEndDate.ToString("yyyy-MM-dd"), ga4setup.ProfileId);
                        if (responsePrev.statusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            var accessToken = await _reportSchedulingService.GetAccessTokenUsingRefreshToken(ga4setup.RefreshToken);

                            if (!string.IsNullOrEmpty(accessToken))
                            {
                                ga4setup.AccessToken = accessToken;
                                ga4setup.UpdatedOn = DateTime.UtcNow;
                                _campaignGoogleAnalyticsRepository.UpdateEntity(ga4setup);
                                _campaignGoogleAnalyticsRepository.SaveChanges();

                                responsePrev = await _reportSchedulingService.PrepareGa4OrganicTrafficReportsByGet(ga4setup.AccessToken, previousDate.PreviousStartDate.ToString("yyyy-MM-dd"), previousDate.PreviousEndDate.ToString("yyyy-MM-dd"), ga4setup.ProfileId);
                            }                                                       
                        }
                        var returnDataPrev = await _reportSchedulingService.PrepareGa4ChartData(responsePrev, previousDate.PreviousStartDate.ToString("yyyy-MM-dd"), previousDate.PreviousEndDate.ToString("yyyy-MM-dd"));

                       
                        if (returnData.organicData.Count > 0 ||returnDataPrev.organicData.Count > 0)
                        {
                            var totalTraffic = returnData.organicData.Sum(x => x.value);

                            var pretotalTraffic = returnDataPrev.organicData.Sum(x => x.value);

                            int[] oganicTraffic = new int[] { totalTraffic , pretotalTraffic };

                            traffic = string.Join("--", oganicTraffic);
                        }
                        else
                        {
                            traffic = "0--0";
                        }

                        if (returnData.conversionData.Count > 0 || returnDataPrev.conversionData.Count > 0)
                        {

                            var totalConversion = returnData.conversionData.Sum(x => x.value);

                            var prevtotalConversion = returnDataPrev.conversionData.Sum(x => x.value);

                            int[] conversionData = new int[] { totalConversion , prevtotalConversion };

                            conversions = string.Join("--", conversionData);
                        }
                        else
                        {
                            conversions = "0--0";
                        }
                    }
                    else
                    {
                        traffic = "NA";
                        conversions = "NA";
                        //NA
                    }

                    retVal = "---GA4 Succeed---";

                    camp.Traffic = traffic;
                    camp.Conversions = conversions;

                }
                catch (Exception ex)
                {
                    retVal = "Error in GA4---" + ex.StackTrace
                   + "Error messasge: " + ex.Message +
                   "CampaignId " + campaignId;

                }

                try
                {
                    HttpStatusCode gscStatusCode = HttpStatusCode.OK;
                    string gsc = string.Empty;

                    var gscSetup = _campaignGSCRepository.GetFilteredEntities(true).Where(x => x.CampaignID == campaign.Id).FirstOrDefault();

                    if (gscSetup != null)
                    {
                        var accessToken = await _reportSchedulingService.GetAccessTokenUsingRefreshToken(gscSetup.RefreshToken);

                        var googleSearchConsoleReport = await _reportSchedulingService.PrepareGoogleSearchConsole(startDate, endDate, gscSetup.UrlOrName, accessToken, previousDate, new List<string>() { });

                        if (googleSearchConsoleReport != null & googleSearchConsoleReport.CurrentImpression != null & googleSearchConsoleReport.PreviousImpression != null)
                        {
                            int[] myGscImpression = new int[] { Int32.Parse(googleSearchConsoleReport.CurrentImpression), Int32.Parse(googleSearchConsoleReport.PreviousImpression) };

                            gsc = string.Join("--", myGscImpression);
                        }
                        else
                        {
                            if (googleSearchConsoleReport.StatusCode != System.Net.HttpStatusCode.OK)
                            {
                                gscStatusCode = googleSearchConsoleReport.StatusCode;
                                retVal += "---Error in Gsc---StatusCode" + googleSearchConsoleReport.StatusCode.ToString();
                            }
                            else
                            {
                                gsc = "0--0";
                            }
                        }
                    }
                    else
                    {
                        gsc = "NA";
                        //NA
                    }

                    if (gscStatusCode == HttpStatusCode.OK || gsc == "NA")
                    {
                        retVal += "---GSC Succeed---";

                        camp.Gsc = gsc;
                    }


                }
                catch (Exception ex)
                {
                    retVal += "---Error in Gsc---" + ex.StackTrace
                                         + "Error messasge: " + ex.Message +
                                         "CampaignId " + campaignId;

                }

                try
                {
                    string ranking = string.Empty;

                    var avgRankings = PrepareAvgRanking(campaign.Id, startDate, endDate, previousDate);

                    if (avgRankings.Count > 0)
                    {
                        ranking = String.Join("--", avgRankings);
                    }
                    else
                    {
                        ranking = "NA";
                    }

                    retVal += "---Ranking Succeed---";


                    camp.Ranking = ranking;
                }
                catch (Exception ex)
                {
                    retVal += "---Error in Keywords--- " + ex.StackTrace
                    + "Error messasge: " + ex.Message +
                    "CampaignId " + campaignId;
                }

                camp.Id = campaign.Id;
                camp.WebUrl = campaign.WebUrl;
                camp.Name = campaign.Name;
                camp.CompanyID = campaign.CompanyID;
                camp.CreatedBy = campaign.CreatedBy;
                camp.CreatedOn = campaign.CreatedOn;
                campaignList.Add(camp);
            }
            return campaignList;
        }


        public List<long> PrepareAvgRanking(Guid campaignID, string fromDate, string toDate, PreviousDate previousDate)
        {
            var avgRankings = new List<long>() { };
            decimal avgRankingPosition;
            decimal avgPreviousRankingPosition;
            var preFromDate = previousDate.PreviousStartDate.ToString("yyyy-MM-dd");
            var preToDate = previousDate.PreviousEndDate.ToString("yyyy-MM-dd");


            var hasKeyword = _serpRepository.GetAllEntities(true).Where(x => x.CampaignID == campaignID).Any();

            if (hasKeyword)
            {
                var latestKeywordListData = _serpRepository.GetAllEntities(true).Where(x => x.CampaignID == campaignID && x.CreatedOn >= DateTime.Parse(fromDate).ToUniversalTime() && x.CreatedOn <= DateTime.Parse(toDate).ToUniversalTime()).Select(y => new SerpDto
                {
                    Id = y.Id,
                    CampaignID = y.CampaignID.ToString(),
                    Position = y.Position,
                    LocalPackCount = y.LocalPackCount,
                    Searches = y.Searches,
                    Location = y.Location,
                    Keywords = y.Keywords,
                    UpdatedOn = y.UpdatedOn,
                    CreatedOn = y.CreatedOn,
                    LocationName = y.LocationName
                }).ToList();

                // sort data - latest created keyword first
                var latestKeywordList = latestKeywordListData.OrderByDescending(x => x.UpdatedOn).ToList();

                latestKeywordList = latestKeywordList.Distinct(new KeywordEqualityComparer()).ToList();
                if (latestKeywordList.Count > 0)
                {
                    var totalSum = latestKeywordList.Distinct(new KeywordEqualityComparer()).ToList().Sum(x => x.Position);
                    var avg = Decimal.Divide(totalSum, latestKeywordList.Count);
                    avgRankingPosition = totalSum == 0 ? 0 : Math.Round(avg);
                }
                else
                {
                    avgRankingPosition = 0;
                }

                var previousKeywordListData = _serpRepository.GetAllEntities(true).Where(x => x.CampaignID == campaignID && x.CreatedOn >= DateTime.Parse(preFromDate).ToUniversalTime() && x.CreatedOn <= DateTime.Parse(preToDate).ToUniversalTime()).Select(y => new SerpDto
                {
                    Id = y.Id,
                    CampaignID = y.CampaignID.ToString(),
                    Position = y.Position,
                    LocalPackCount = y.LocalPackCount,
                    Searches = y.Searches,
                    Location = y.Location,
                    Keywords = y.Keywords,
                    UpdatedOn = y.UpdatedOn,
                    CreatedOn = y.CreatedOn,
                    LocationName = y.LocationName
                }).ToList();

                // sort data - latest created keyword first
                var previousKeywordList = previousKeywordListData.OrderByDescending(x => x.UpdatedOn).ToList();
                previousKeywordList = previousKeywordList.Distinct(new KeywordEqualityComparer()).ToList();
                if (previousKeywordList.Count > 0)
                {
                    var totalSum = previousKeywordList.Distinct(new KeywordEqualityComparer()).ToList().Sum(x => x.Position);
                    var avg = Decimal.Divide(totalSum, previousKeywordList.Count);
                    avgPreviousRankingPosition = totalSum == 0 ? 0 : Math.Round(avg);
                }
                else
                {
                    avgPreviousRankingPosition = 0;
                }

                //var avgRankings = new long[] { avgRankingPosition, avgPreviousRankingPosition };

                avgRankings = new List<long>() { (long)avgRankingPosition, (long)avgPreviousRankingPosition };
            }


            return avgRankings;
        }

        /// <summary>
        /// Calculate PreviousStartDate And EndDate
        /// </summary>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <returns>PreviousDate</returns>
        private PreviousDate CalculatePreviousStartDateAndEndDate(DateTime startDate, DateTime endDate)
        {
            var previousDate = new PreviousDate();
            var diff = (endDate - startDate).TotalDays;
            diff = Math.Round(diff);

            previousDate.PreviousEndDate = startDate.AddDays(-1);
            previousDate.PreviousStartDate = startDate.AddDays(-diff);

            return previousDate;
        }

        public async Task<int> DeleteCampaignsFromAppsumo(List<Guid> ids)
        {
            //delete the campaign from the db.
            return await _campaignRepository.DeleteBulkEntityAsync(x => ids.Contains(x.Id));                       
        }

        #endregion


        #region OVERRIDDEN IMPLEMENTATION

        public override Dictionary<string, PropertyMappingValue> GetPropertyMapping()
        {
            return new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "Id", new PropertyMappingValue(new List<string>() { "Id" } ) },
                        { "Name", new PropertyMappingValue(new List<string>() { "Name" } )},
                        { "WebUrl", new PropertyMappingValue(new List<string>() { "WebUrl" } )},
                        { "MoreTraffic", new PropertyMappingValue(new List<string>() { "MoreTraffic" } )},
                        { "Sales", new PropertyMappingValue(new List<string>() { "Sales" } )},
                        { "LeadGeneration", new PropertyMappingValue(new List<string>() { "LeadGeneration" } )},
                    };
        }

        public override string GetDefaultOrderByColumn()
        {
            return "Id";
        }

        public override string GetDefaultFieldsToSelect()
        {
            return "Id,Name,WebUrl,MoreTraffic,Sales,LeadGeneration,Traffic,Ranking,Gsc,Conversions";

        }

        #endregion
    }
}
