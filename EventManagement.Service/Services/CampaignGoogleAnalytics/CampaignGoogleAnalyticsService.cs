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
using System.Net;
using System.Net.Http;
using System.Text;
using Google.Rpc;
using RestSharp.Extensions;
using RestSharp.Authenticators;
using static Google.Rpc.Context.AttributeContext.Types;
using RestSharp.Authenticators.OAuth2;
using Google.Protobuf.WellKnownTypes;
using Method = RestSharp.Method;

namespace EventManagement.Service
{
    public class CampaignGoogleAnalyticsService : ServiceBase<CampaignGoogleAnalytics, Guid>, ICampaignGoogleAnalyticsService
    {

        #region PRIVATE MEMBERS

        private readonly ICampaignGoogleAnalyticsRepository _campaigngoogleanalyticsRepository;
        private readonly IConfiguration _configuration;
        private readonly ICampaignRepository _campaignRepository;
        private readonly IReportSchedulingService _reportSchedulingService;
        private readonly ICampaignGSCRepository _campaigngscRepository;
        private readonly ICampaignGoogleAdsRepository _campaignGoogleAdsRepository;
        private readonly ICampaignGBPRepository _campaignGBPRepository;
        #endregion


        #region CONSTRUCTOR

        public CampaignGoogleAnalyticsService(ICampaignGoogleAdsRepository campaignGoogleAdsRepository, 
            ICampaignGSCRepository campaigngscRepository, 
            IReportSchedulingService reportSchedulingService,
            ICampaignGoogleAnalyticsRepository campaigngoogleanalyticsRepository, 
            ILogger<CampaignGoogleAnalyticsService> logger, IConfiguration configuration, 
            ICampaignRepository campaignRepository,ICampaignGBPRepository campaignGBPRepository) : base(campaigngoogleanalyticsRepository, logger)
        {
            _campaigngoogleanalyticsRepository = campaigngoogleanalyticsRepository;
            _configuration = configuration;
            _campaignRepository = campaignRepository;
            _reportSchedulingService = reportSchedulingService;
            _campaigngscRepository = campaigngscRepository;
            _campaignGoogleAdsRepository = campaignGoogleAdsRepository;
            _reportSchedulingService = reportSchedulingService;
            _campaignGBPRepository = campaignGBPRepository;
        }

        #endregion


        #region PUBLIC MEMBERS   

        public async Task<Ga4Details> GetCampaignGa4DataById(Guid campaignId, string startDate, string endDate)
        {
            var returnData = new Ga4Details();

            var campaign = _campaigngoogleanalyticsRepository.GetAllEntities(true).Where(x => x.CampaignID == campaignId && x.IsGa4).FirstOrDefault();
            if (campaign != null)
            {
                var response = await _reportSchedulingService.PrepareGa4OrganicTrafficReportsByGet(campaign.AccessToken, startDate, endDate, campaign.ProfileId);
                var ecomData = await _reportSchedulingService.PrepareGa4EcomReportsByGet(campaign.AccessToken, startDate, endDate, campaign.ProfileId);
                var purchaseJourney = await _reportSchedulingService.PrepareGa4PurchaseJourneyReports(campaign.AccessToken, startDate, endDate, campaign.ProfileId);
                if (response.statusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var accessToken = await _reportSchedulingService.GetAccessTokenUsingRefreshToken(campaign.RefreshToken);

                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        campaign.AccessToken = accessToken;
                        campaign.UpdatedOn = DateTime.UtcNow;
                        _campaigngoogleanalyticsRepository.UpdateEntity(campaign);
                        _campaigngoogleanalyticsRepository.SaveChanges();

                        response = await _reportSchedulingService.PrepareGa4OrganicTrafficReportsByGet(campaign.AccessToken, startDate, endDate, campaign.ProfileId);
                        ecomData = await _reportSchedulingService.PrepareGa4EcomReportsByGet(campaign.AccessToken, startDate, endDate, campaign.ProfileId);
                        purchaseJourney = await _reportSchedulingService.PrepareGa4PurchaseJourneyReports(campaign.AccessToken, startDate, endDate, campaign.ProfileId);

                    }
                }

                returnData = await _reportSchedulingService.PrepareGa4ChartData(response, startDate, endDate);
                returnData.EcomPurchases = ecomData;
                returnData.Ga4PurchaseJourney = purchaseJourney;
                
            }
            return returnData;

        }

        public async Task<GoogleAnalyticsResponseDto> GetCampaignGaDataById(Guid campaignId, string startDate, string endDate)
        {
            var returnData = new GoogleAnalyticsResponseDto();

            var campaign = _campaigngoogleanalyticsRepository.GetAllEntities(true).Where(x => x.CampaignID == campaignId && !x.IsGa4).FirstOrDefault();
            if (campaign != null)
            {
                returnData = await _reportSchedulingService.PrepareGAReportsByPost(campaign.AccessToken, startDate, endDate, campaign.ProfileId);
                if (returnData.statusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var accessToken = await _reportSchedulingService.GetAccessTokenUsingRefreshToken(campaign.RefreshToken);

                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        campaign.AccessToken = accessToken;
                        campaign.UpdatedOn = DateTime.UtcNow;
                        _campaigngoogleanalyticsRepository.UpdateEntity(campaign);
                        _campaigngoogleanalyticsRepository.SaveChanges();

                        returnData = await _reportSchedulingService.PrepareGAReportsByPost(campaign.AccessToken, startDate, endDate, campaign.ProfileId);
                    }                   
                }
            }

            return returnData;
        }

        public async Task<string> GetLighthouseDataByStrategy(Guid campaignId, string strategy)
        {
            string returnData = "";

            var campaign = _campaignRepository.GetAllEntities(true).Where(x => x.Id == campaignId).FirstOrDefault();
            if (campaign != null)
            {
                returnData = await _reportSchedulingService.PreparePageSpeedLighthouseByStrategy(campaign.WebUrl, strategy);
            }

            return returnData;
        }

        public string GoogleAnalyticsSetup(string type, string source)
        {
            if (source == "tab")
            {
                string _clientId = _configuration.GetSection("ClientIdForGoogleAds").Value;
                string _clientSecret = _configuration.GetSection("ClientSecretForGoogleAds").Value;
                //var client = new RestClient("https://accounts.google.com/o/oauth2/v2");

                string _callbackUrl = _configuration.GetSection("HostedUrl").Value + "api/campaigngoogleanalyticss/GoogleExchangeCode";
                string loadUrl = String.Empty;

                if (type == "GA" || type == "GA4")
                {
                    loadUrl = "https://accounts.google.com/o/oauth2/v2/auth?client_id=" + _clientId + "&scope=https://www.googleapis.com/auth/analytics.readonly https://www.googleapis.com/auth/analytics+openid https://www.googleapis.com/auth/userinfo.email&access_type=offline&prompt=consent&include_granted_scopes=false&redirect_uri=" + _callbackUrl + "&response_type=code";
                }
                else if (type == "GSC")
                {
                    loadUrl = "https://accounts.google.com/o/oauth2/v2/auth?client_id=" + _clientId + "&scope=https://www.googleapis.com/auth/webmasters.readonly https://www.googleapis.com/auth/webmasters https://www.googleapis.com/auth/userinfo.email&access_type=offline&prompt=consent&include_granted_scopes=false&redirect_uri=" + _callbackUrl + "&response_type=code";
                }
                else if (type == "GBP")
                {
                    loadUrl = "https://accounts.google.com/o/oauth2/v2/auth?client_id=" + _clientId + "&scope=https://www.googleapis.com/auth/userinfo.email https://www.googleapis.com/auth/business.manage&access_type=offline&prompt=consent&include_granted_scopes=false&redirect_uri=" + _callbackUrl + "&response_type=code";
                }
                else if (type == "GSHEET")
                {
                    loadUrl = "https://accounts.google.com/o/oauth2/v2/auth?client_id=" + _clientId + "&scope=https://www.googleapis.com/auth/userinfo.profile https://www.googleapis.com/auth/drive.readonly https://www.googleapis.com/auth/spreadsheets.readonly&access_type=offline&prompt=consent&redirect_uri=" + _callbackUrl + "&response_type=code";
                }
                else
                {
                    loadUrl = "https://accounts.google.com/o/oauth2/v2/auth?client_id=" + _clientId + "&scope=https://www.googleapis.com/auth/adwords+openid https://www.googleapis.com/auth/userinfo.email&access_type=offline&prompt=consent&include_granted_scopes=false&redirect_uri=" + _callbackUrl + "&response_type=code";
                }

                return loadUrl;
            }
            else
            {
                string _clientId = _configuration.GetSection("ClientIdForGoogleAds").Value;
                string _clientSecret = _configuration.GetSection("ClientSecretForGoogleAds").Value;
                //var client = new RestClient("https://accounts.google.com/o/oauth2/v2");

                string _callbackUrl = _configuration.GetSection("HostedUrl").Value + "api/campaigngoogleanalyticss/GoogleExchangeCodeForPopup";
                string loadUrl = String.Empty;

                if (type == "GA" || type == "GA4")
                {
                    loadUrl = "https://accounts.google.com/o/oauth2/v2/auth?client_id=" + _clientId + "&scope=https://www.googleapis.com/auth/analytics.readonly https://www.googleapis.com/auth/analytics+openid https://www.googleapis.com/auth/userinfo.email&access_type=offline&prompt=consent&include_granted_scopes=true&redirect_uri=" + _callbackUrl + "&response_type=code";
                }
                else if (type == "GSC")
                {
                    loadUrl = "https://accounts.google.com/o/oauth2/v2/auth?client_id=" + _clientId + "&scope=https://www.googleapis.com/auth/webmasters.readonly https://www.googleapis.com/auth/webmasters https://www.googleapis.com/auth/userinfo.email&access_type=offline&prompt=consent&include_granted_scopes=true&redirect_uri=" + _callbackUrl + "&response_type=code";
                }
                else if (type == "GBP")
                {
                    loadUrl = "https://accounts.google.com/o/oauth2/v2/auth?client_id=" + _clientId + "&scope=https://www.googleapis.com/auth/business.manage&access_type=offline&prompt=consent&include_granted_scopes=true&redirect_uri=" + _callbackUrl + "&response_type=code";
                }
                else if (type == "GSHEET")
                {
                    loadUrl = "https://accounts.google.com/o/oauth2/v2/auth?client_id=" + _clientId + "&scope=https://www.googleapis.com/auth/userinfo.profile https://www.googleapis.com/auth/drive.readonly https://www.googleapis.com/auth/spreadsheets.readonly&access_type=offline&prompt=consent&include_granted_scopes=true&redirect_uri=" + _callbackUrl + "&response_type=code";
                }
                else
                {
                    loadUrl = "https://accounts.google.com/o/oauth2/v2/auth?client_id=" + _clientId + "&scope=https://www.googleapis.com/auth/adwords+openid https://www.googleapis.com/auth/userinfo.email&access_type=offline&prompt=consent&include_granted_scopes=true&redirect_uri=" + _callbackUrl + "&response_type=code";
                }

                return loadUrl;
            }

        }

        public async Task<GaToken> GetAccessTokenUsingCode(string code)
        {

            var clientId = _configuration["ClientIdForGoogleAds"];
            var clientSecret = _configuration["ClientSecretForGoogleAds"];
            string _callbackUrl = _configuration.GetSection("HostedUrl").Value + "api/campaigngoogleanalyticss/GoogleExchangeCode";
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://oauth2.googleapis.com"),
            };

            var data = new
            {
                client_id = clientId,
                client_secret = clientSecret,
                code = code,
                grant_type = "authorization_code",
                redirect_uri = _callbackUrl
                //access_type = "offline"
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var resp = await httpClient.PostAsync("/token", stringContent);
            var resp1 = await resp.Content.ReadAsStringAsync();
            var gaResponse = JsonConvert.DeserializeObject<GaToken>(await resp.Content.ReadAsStringAsync());

            return gaResponse;

        }

        public async Task<GaToken> GetAccessTokenUsingCodeForPopup(string code)
        {

            var clientId = _configuration["ClientIdForGoogleAds"];
            var clientSecret = _configuration["ClientSecretForGoogleAds"];
            string _callbackUrl = _configuration.GetSection("HostedUrl").Value + "api/campaigngoogleanalyticss/GoogleExchangeCodeForPopup";
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://oauth2.googleapis.com"),
            };

            var data = new
            {
                client_id = clientId,
                client_secret = clientSecret,
                code = code,
                grant_type = "authorization_code",
                redirect_uri = _callbackUrl
                //access_type = "offline"
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var resp = await httpClient.PostAsync("/token", stringContent);
            var resp1 = await resp.Content.ReadAsStringAsync();
            var gaResponse = JsonConvert.DeserializeObject<GaToken>(await resp.Content.ReadAsStringAsync());

            return gaResponse;

        }

        public async Task<GaToken> GetRefreshTokenUsingAccessToken(string code)
        {

            var clientId = _configuration["ClientIdForGoogleAds"];
            var clientSecret = _configuration["ClientSecretForGoogleAds"];
            string _callbackUrl = _configuration.GetSection("HostedUrl").Value + "api/campaigngoogleanalyticss/GoogleExchangeCodeForPopup";
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://oauth2.googleapis.com"),
            };

            var data = new
            {
                client_id = clientId,
                client_secret = clientSecret,
                code = code,
                grant_type = "authorization_code",
                redirect_uri = _callbackUrl
                //access_type = "offline"
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var resp = await httpClient.PostAsync("/token", stringContent);
            var resp1 = await resp.Content.ReadAsStringAsync();
            var gaResponse = JsonConvert.DeserializeObject<GaToken>(await resp.Content.ReadAsStringAsync());

            return gaResponse;

        }

        /// <summary>
        /// Update previously access token, refresh token if email id same in same company 
        /// </summary>
        /// <param name="campaigngoogleanalytics">campaigngoogleanalytics</param>
        /// <param name="companyId">companyId</param>
        /// <returns>bool</returns>
        public async Task<bool> UpdateRefreshTokenAndEmail(CampaignGoogleAnalyticsForCreation campaigngoogleanalytics, string companyId)
        {
            var campaignIds = _campaignRepository.GetFilteredEntities(true).Where(x => x.CompanyID == new Guid(companyId)).Select(x => x.Id).ToList();
            //campaignIds.Remove(campaigngoogleanalytics.CampaignID);

            foreach (var campaignId in campaignIds)
            {
                var campaignGaSetup = _campaigngoogleanalyticsRepository.GetFilteredEntities(true).Where(x => x.EmailId == campaigngoogleanalytics.EmailId && x.CampaignID == campaignId).ToList();

                if (campaignGaSetup.Count > 0)
                {
                    foreach (var cga in campaignGaSetup)
                    {
                        cga.RefreshToken = campaigngoogleanalytics.RefreshToken;
                        cga.AccessToken = campaigngoogleanalytics.AccessToken;
                        _campaigngoogleanalyticsRepository.UpdateEntity(cga);
                        var response = await _campaigngoogleanalyticsRepository.SaveChangesAsync();

                    }

                }
            }
            return true;
        }

        public async Task<RootObjectGoogleAnayltics> GetAnalyticsProfileIds(Guid campaignId)
        {
            RootObjectGoogleAnayltics list = new RootObjectGoogleAnayltics();
            var campaign = new CampaignGoogleAnalytics();
            try
            {
                campaign = _campaigngoogleanalyticsRepository.GetAllEntities(true).Where(x => x.CampaignID == campaignId && !x.IsGa4).FirstOrDefault();
                if (campaign != null)
                {
                    string _clientId = _configuration.GetSection("ClientIdForGoogleAds").Value;
                    string _clientSecret = _configuration.GetSection("ClientSecretForGoogleAds").Value;

                    var uri = _configuration.GetSection("IdentityServerUrl").Value;


                    var options = new RestClientOptions("https://www.googleapis.com/analytics/v3/")
                    {
                        Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(campaign.AccessToken, "Bearer")
                    };

                    var requestForData = new RestRequest("management/accountSummaries", Method.Get);

                    var clientForData = new RestClient(options);
                    requestForData.AddHeader("Content-Type", "application/json");


                    var response = await clientForData.GetAsync(requestForData);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        list = JsonConvert.DeserializeObject<RootObjectGoogleAnayltics>(response.Content);
                    }

                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "Request failed with status code Unauthorized" || ex.InnerException.Message == "Request failed with status code Unauthorized")
                {
                    var accessToken = await _reportSchedulingService.GetAccessTokenUsingRefreshToken(campaign.RefreshToken);

                    campaign.AccessToken = accessToken;
                    campaign.UpdatedOn = DateTime.UtcNow;
                    _campaigngoogleanalyticsRepository.UpdateEntity(campaign);
                    _campaigngoogleanalyticsRepository.SaveChanges();

                    var options = new RestClientOptions("https://www.googleapis.com/analytics/v3/")
                    {
                        Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(accessToken, "Bearer")
                    };

                    var requestForData = new RestRequest("management/accountSummaries", Method.Get);

                    var clientForData = new RestClient(options);
                    requestForData.AddHeader("Content-Type", "application/json");


                    var response = await clientForData.GetAsync(requestForData);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        list = JsonConvert.DeserializeObject<RootObjectGoogleAnayltics>(response.Content);
                    }
                    return list;
                }
            }
            return list;
        }

        public async Task<RootObjectOfGoogleEmail> GetEmailAddress(Guid campaignId, string type)
        {
            var emailId = new RootObjectOfGoogleEmail();

            var campaignGAds = new CampaignGoogleAds();
            var campaignGA4 = new CampaignGoogleAnalytics();
            var campaignGsc = new CampaignGSC();
            var responseGads = new RestResponse();
            var campaignGbp = new CampaignGBP();

            try
            {
                if (type == "GA4")
                {
                    campaignGA4 = _campaigngoogleanalyticsRepository.GetAllEntities().Where(x => x.CampaignID == campaignId && x.IsGa4).FirstOrDefault();
                    if (campaignGA4 != null)
                    {
                        var response = GetEmailData(campaignGA4.AccessToken);
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            emailId = JsonConvert.DeserializeObject<RootObjectOfGoogleEmail>(response.Content);
                        }
                    }
                }
                else if (type == "GSC")
                {
                    campaignGsc = _campaigngscRepository.GetAllEntities().Where(x => x.CampaignID == campaignId).FirstOrDefault();
                    if (campaignGsc != null)
                    {
                        var response = GetEmailData(campaignGsc.AccessToken);
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            emailId = JsonConvert.DeserializeObject<RootObjectOfGoogleEmail>(response.Content);
                        }

                    }
                }else if (type == "GBP")
                {
                    campaignGbp = _campaignGBPRepository.GetAllEntities().Where(x => x.CampaignID == campaignId).FirstOrDefault();
                    if (campaignGbp != null)
                    {
                        var response = GetEmailData(campaignGbp.AccessToken);
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            emailId = JsonConvert.DeserializeObject<RootObjectOfGoogleEmail>(response.Content);
                        }
                        

                    }
                }
                else if (type == "GADS")
                {
                    campaignGAds = _campaignGoogleAdsRepository.GetAllEntities().Where(x => x.CampaignID == campaignId).FirstOrDefault();
                    if (campaignGAds != null)
                    {
                        //var response = GetEmailData(campaign.AccessToken);
                        var uri = _configuration.GetSection("IdentityServerUrl").Value;

                        //var options = new RestClientOptions("https://www.googleapis.com/oauth2/v2/")
                        //{
                        //    Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(accessToken, "Bearer")            
                        //};
                        var clientForData = new RestClient("https://www.googleapis.com/oauth2/v2/");
                        var requestForData = new RestRequest("userinfo", Method.Get);
                        requestForData.AddHeader("Content-Type", "application/json");
                        requestForData.AddHeader("X-Restli-Protocol-Version", "2.0.0");
                        requestForData.AddHeader("Bearer", campaignGAds.AccessToken);

                        responseGads = clientForData.GetAsync(requestForData).Result;

                        if (responseGads.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            emailId = JsonConvert.DeserializeObject<RootObjectOfGoogleEmail>(responseGads.Content);
                        }
                        else if (responseGads.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            var accessToken = await _reportSchedulingService.GetAccessTokenUsingRefreshToken(campaignGAds.RefreshToken);

                            if (!string.IsNullOrEmpty(accessToken))
                            {
                                campaignGAds.AccessToken = accessToken;
                                campaignGAds.UpdatedOn = DateTime.UtcNow;
                                _campaignGoogleAdsRepository.UpdateEntity(campaignGAds);
                                _campaignGoogleAdsRepository.SaveChanges();

                                responseGads = GetEmailData(campaignGAds.AccessToken);
                            }

                            if (responseGads.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                emailId = JsonConvert.DeserializeObject<RootObjectOfGoogleEmail>(responseGads.Content);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "Request failed with status code Unauthorized" || ex.InnerException.Message == "Request failed with status code Unauthorized")
                {
                    if (type == "GA4")
                    {
                        var accessToken = await _reportSchedulingService.GetAccessTokenUsingRefreshToken(campaignGA4.RefreshToken);

                        campaignGA4.AccessToken = accessToken;
                        campaignGA4.UpdatedOn = DateTime.UtcNow;
                        _campaigngoogleanalyticsRepository.UpdateEntity(campaignGA4);
                        _campaigngoogleanalyticsRepository.SaveChanges();

                        var response = GetEmailData(accessToken);
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            emailId = JsonConvert.DeserializeObject<RootObjectOfGoogleEmail>(response.Content);
                            return emailId;
                        }
                    }
                    else if (type == "GSC")
                    {
                        var accessToken = await _reportSchedulingService.GetAccessTokenUsingRefreshToken(campaignGsc.RefreshToken);

                        campaignGsc.AccessToken = accessToken;
                        campaignGsc.UpdatedOn = DateTime.UtcNow;
                        _campaigngscRepository.UpdateEntity(campaignGsc);
                        _campaigngscRepository.SaveChanges();

                        var response = GetEmailData(accessToken);
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            emailId = JsonConvert.DeserializeObject<RootObjectOfGoogleEmail>(response.Content);
                            return emailId;
                        }
                    }
                    else if (type == "GADS")
                    {
                        var accessToken = await _reportSchedulingService.GetAccessTokenUsingRefreshToken(campaignGAds.RefreshToken);

                        campaignGAds.AccessToken = accessToken;
                        campaignGAds.UpdatedOn = DateTime.UtcNow;
                        _campaignGoogleAdsRepository.UpdateEntity(campaignGAds);
                        _campaignGoogleAdsRepository.SaveChanges();

                        var response = GetEmailData(accessToken);
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            emailId = JsonConvert.DeserializeObject<RootObjectOfGoogleEmail>(response.Content);
                            return emailId;
                        }
                    }
                    else if (type == "GBP")
                    {
                        var accessToken = await _reportSchedulingService.GetAccessTokenUsingRefreshToken(campaignGbp.RefreshToken);

                        campaignGbp.AccessToken = accessToken;
                        campaignGbp.UpdatedOn = DateTime.UtcNow;
                        _campaignGBPRepository.UpdateEntity(campaignGbp);
                        _campaignGBPRepository.SaveChanges();

                        var response = GetEmailData(accessToken);
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            emailId = JsonConvert.DeserializeObject<RootObjectOfGoogleEmail>(response.Content);
                            return emailId;
                        }
                    }
                }

            }
            return emailId;
        }

        private RestResponse GetEmailData(string accessToken)
        {
            var uri = _configuration.GetSection("IdentityServerUrl").Value;


            //// create client
            //var client = new RestClient("https://www.googleapis.com/oauth2/v2/");
            //var request = new RestRequest("userinfo", Method.Get);

            //var auth = new OAuth2AuthorizationRequestHeaderAuthenticator(accessToken, "Bearer");
            //// client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(accessToken, "Bearer");
            //// add header
            //request.AddHeader("Content-Type", "application/json");
            //request.AddHeader("Bearer", accessToken);

            //var response = client.Execute(request);

            var requestForData = new RestRequest("userinfo", Method.Post);
            requestForData.AddHeader("X-Restli-Protocol-Version", "2.0.0");
            requestForData.AddHeader("Content-Type", "application/json");
            requestForData.AddHeader("Bearer", accessToken);

            var options = new RestClientOptions("https://www.googleapis.com/oauth2/v2/")
            {
                Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(accessToken, "Bearer")
            };

            var clientForData = new RestClient(options);

            var response = clientForData.GetAsync(requestForData).Result;
   
            return response;
        }

        public async Task<bool> IsPropertiesExists(string access_token)
        {
            try
            {

                //string[] scopes = new string[] { "https://www.googleapis.com/auth/plus.business.manage" };

                //var userCredential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                //    new ClientSecrets
                //    {
                //        ClientId = ClientId,
                //        ClientSecret = ClientSecret,
                //    },
                //     scopes,
                //     "user",
                //     CancellationToken.None).Result;

                //GoogleCredential cred = GoogleCredential.FromAccessToken(access_token);

                //var accountService = new MyBusinessAccountManagementService(new BaseClientService.Initializer()
                //{
                //    HttpClientInitializer = cred,
                //    ApplicationName = "Account Service"
                //});


                //var businessService = new MyBusinessBusinessInformationService(new BaseClientService.Initializer()
                //{
                //    HttpClientInitializer = cred,
                //    ApplicationName = "Google my profile"
                //});

                //var accountList = accountService.Accounts.List().Execute();
                //foreach (var accountName in accountList.Accounts)
                //{
                //    var locations =  businessService.Accounts.Locations.List(accountName.Name).Execute();
                //}

                string _clientId = _configuration.GetSection("ClientIdForGoogleAds").Value;
                string _clientSecret = _configuration.GetSection("ClientSecretForGoogleAds").Value;
                var uri = _configuration.GetSection("IdentityServerUrl").Value;

                var options = new RestClientOptions("https://analyticsadmin.googleapis.com/v1alpha")
                {
                    Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(access_token, "Bearer")
                };
                var requestForData = new RestRequest("/accountSummaries", Method.Get);
                requestForData.AddHeader("Content-Type", "application/json");
                requestForData.AddParameter("pageSize", 2000);


                var clientForData = new RestClient(options);
                var response = clientForData.GetAsync(requestForData).Result;
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    return false;
                }                
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public async Task<Ga4RootList> GetAnalytics4ProfileIds(Guid campaignId)
        {
            Ga4RootList list = new Ga4RootList();

            var campaign = _campaigngoogleanalyticsRepository.GetAllEntities(true).Where(x => x.CampaignID == campaignId && x.IsGa4).FirstOrDefault();

            try
            {
                campaign = _campaigngoogleanalyticsRepository.GetAllEntities(true).Where(x => x.CampaignID == campaignId && x.IsGa4).FirstOrDefault();
                if (campaign != null)
                {
                    string _clientId = _configuration.GetSection("ClientIdForGoogleAds").Value;
                    string _clientSecret = _configuration.GetSection("ClientSecretForGoogleAds").Value;
                    var uri = _configuration.GetSection("IdentityServerUrl").Value;

                    var options = new RestClientOptions("https://analyticsadmin.googleapis.com/v1alpha")
                    {
                        Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(campaign.AccessToken, "Bearer")
                    };
                    var requestForData = new RestRequest("/accountSummaries", Method.Get);
                    requestForData.AddHeader("Content-Type", "application/json");
                    requestForData.AddParameter("pageSize", 2000);


                    var clientForData = new RestClient(options);
                    var response = clientForData.GetAsync(requestForData).Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        list = JsonConvert.DeserializeObject<Ga4RootList>(response.Content);
                    }

                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "Request failed with status code Unauthorized" || ex.InnerException.Message == "Request failed with status code Unauthorized")
                {
                    var accessToken = await _reportSchedulingService.GetAccessTokenUsingRefreshToken(campaign.RefreshToken);

                    campaign.AccessToken = accessToken;
                    campaign.UpdatedOn = DateTime.UtcNow;
                    _campaigngoogleanalyticsRepository.UpdateEntity(campaign);
                    _campaigngoogleanalyticsRepository.SaveChanges();

                    var options = new RestClientOptions("https://analyticsadmin.googleapis.com/v1alpha")
                    {
                        Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(campaign.AccessToken, "Bearer")
                    };


                    var clientForData = new RestClient(options);

                    var requestForData = new RestRequest("/accountSummaries", Method.Get);
                    requestForData.AddHeader("Content-Type", "application/json");
                    requestForData.AddParameter("pageSize", 2000);

                    var response = clientForData.GetAsync(requestForData).Result;

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        list = JsonConvert.DeserializeObject<Ga4RootList>(response.Content);
                        return list;
                    }
                }
                throw ex;
            }

            return list;
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
            return "Id, UrlOrName, CampaignID, IsActive, Campaign, AccessToken ,EmailId,ProfileId,IsGa4";
        }

        #endregion
    }
}
