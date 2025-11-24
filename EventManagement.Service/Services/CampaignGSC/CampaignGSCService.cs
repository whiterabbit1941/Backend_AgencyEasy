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
using System.Net.Http;
using System.Text;
using RestSharp.Authenticators;

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using RestSharp.Authenticators.OAuth2;

namespace EventManagement.Service
{
    public class CampaignGSCService : ServiceBase<CampaignGSC, Guid>, ICampaignGSCService
    {

        #region PRIVATE MEMBERS

        private readonly ICampaignGSCRepository _campaigngscRepository;
        private readonly IConfiguration _configuration;
        private readonly ICampaignRepository _campaignRepository;
        private readonly IReportSchedulingService _reportSchedulingService;

        #endregion


        #region CONSTRUCTOR

        public CampaignGSCService(IReportSchedulingService reportSchedulingService, ICampaignGSCRepository campaigngscRepository, ILogger<CampaignGSCService> logger, IConfiguration configuration, ICampaignRepository campaignRepository) : base(campaigngscRepository, logger)
        {
            _campaigngscRepository = campaigngscRepository;
            _configuration = configuration;
            _campaignRepository = campaignRepository;
            _reportSchedulingService = reportSchedulingService;
        }

        #endregion


        #region PUBLIC MEMBERS   


        public async Task<GscData> GetCampaignGscDataById(Guid campaignId, string startDate, string endDate)
        {
            var returnData = new GscData();
            string preparedUrl = "";
            var campaign = _campaigngscRepository.GetAllEntities(true).Where(x => x.CampaignID == campaignId).FirstOrDefault();

            if (campaign != null)
            {
                if (campaign.UrlOrName.Contains("https://"))
                {
                    var urlcamp = campaign.UrlOrName.Replace("https://", "");
                    urlcamp = urlcamp.Replace("/", "");
                    preparedUrl = "https%3A%2F%2F" + urlcamp + "/searchAnalytics/query?key=" + _configuration.GetSection("GoogleApiKey").Value;
                }
                else if (campaign.UrlOrName.Contains("http://"))
                {
                    var urlcamp = campaign.UrlOrName.Replace("http://", "");
                    urlcamp = urlcamp.Replace("/", "");
                    preparedUrl = "http%3A%2F%2F" + urlcamp + "/searchAnalytics/query?key=" + _configuration.GetSection("GoogleApiKey").Value;
                }
                else
                {
                    var urlcamp = campaign.UrlOrName.Replace("sc-domain:", "");
                    urlcamp = urlcamp.Replace("/", "");
                    preparedUrl = "sc-domain%3A" + urlcamp + "/searchAnalytics/query?key=" + _configuration.GetSection("GoogleApiKey").Value;
                }
                returnData = await _reportSchedulingService.PrepareGSCReportsByPost(preparedUrl, startDate, endDate, campaign.AccessToken);
                if (returnData.statusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var accessToken = await _reportSchedulingService.GetAccessTokenUsingRefreshToken(campaign.RefreshToken);

                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        campaign.AccessToken = accessToken;
                        campaign.UpdatedOn = DateTime.UtcNow;
                        _campaigngscRepository.UpdateEntity(campaign);
                        _campaigngscRepository.SaveChanges();

                        returnData = await _reportSchedulingService.PrepareGSCReportsByPost(preparedUrl, startDate, endDate, campaign.AccessToken);
                    }                    
                }
            }

            return returnData;
        }

        public async Task<GscChartResponse> GetCampaignGscChartDataWithDateById(Guid campaignId, string startDate, string endDate)
        {
            var returnData = new GscChartResponse();
            string preparedUrl = "";
            var campaign = _campaigngscRepository.GetAllEntities(true).Where(x => x.CampaignID == campaignId).FirstOrDefault();

            if (campaign != null)
            {
                if (campaign.UrlOrName.Contains("https://"))
                {
                    var urlcamp = campaign.UrlOrName.Replace("https://", "");
                    urlcamp = urlcamp.Replace("/", "");
                    preparedUrl = "https%3A%2F%2F" + urlcamp + "/searchAnalytics/query?key=" + _configuration.GetSection("GoogleApiKey").Value;
                }
                else if (campaign.UrlOrName.Contains("http://"))
                {
                    var urlcamp = campaign.UrlOrName.Replace("http://", "");
                    urlcamp = urlcamp.Replace("/", "");
                    preparedUrl = "http%3A%2F%2F" + urlcamp + "/searchAnalytics/query?key=" + _configuration.GetSection("GoogleApiKey").Value;
                }
                else
                {
                    var urlcamp = campaign.UrlOrName.Replace("sc-domain:", "");
                    urlcamp = urlcamp.Replace("/", "");
                    preparedUrl = "sc-domain%3A" + urlcamp + "/searchAnalytics/query?key=" + _configuration.GetSection("GoogleApiKey").Value;
                }

                returnData = await _reportSchedulingService.PrepareGSCReportsByPostWithDate(preparedUrl, startDate, endDate, campaign.AccessToken);
                if (String.IsNullOrEmpty(returnData.responseAggregationType))
                {
                    var accessToken = await _reportSchedulingService.GetAccessTokenUsingRefreshToken(campaign.RefreshToken);

                    campaign.AccessToken = accessToken;
                    campaign.UpdatedOn = DateTime.UtcNow;
                    _campaigngscRepository.UpdateEntity(campaign);
                    _campaigngscRepository.SaveChanges();

                    returnData = await _reportSchedulingService.PrepareGSCReportsByPostWithDate(preparedUrl, startDate, endDate, campaign.AccessToken);
                }
            }

            return returnData;
        }

        /// <summary>
        /// Update previously access token, refresh token if email id same in same company 
        /// </summary>
        /// <param name="campaigngsc">campaigngsc</param>
        /// <param name="companyId">companyId</param>
        /// <returns>bool</returns>
        public async Task<bool> UpdateRefreshTokenAndEmail(CampaignGSCForCreation campaigngsc, string companyId)
        {
            var campaignIds = _campaignRepository.GetFilteredEntities(true).Where(x => x.CompanyID == new Guid(companyId)).Select(x => x.Id).ToList();
            campaignIds.Remove(campaigngsc.CampaignID);

            foreach (var campaignId in campaignIds)
            {
                var campaignGa = _campaigngscRepository.GetFilteredEntities(true).Where(x => x.EmailId == campaigngsc.EmailId && x.CampaignID == campaignId).FirstOrDefault();
                if (campaignGa != null)
                {
                    campaignGa.RefreshToken = campaigngsc.RefreshToken;
                    campaignGa.AccessToken = campaigngsc.AccessToken;
                    _campaigngscRepository.UpdateEntity(campaignGa);
                    var response = await _campaigngscRepository.SaveChangesAsync();
                }
            }
            return true;
        }

        public async Task<GaToken> RefreshGoogleGSCToken(string accessToken)
        {
            try
            {
                var clientId = _configuration["ClientIdForGoogleAds"];
                var clientSecret = _configuration["ClientSecretForGoogleAds"];
                string _callbackUrl = _configuration.GetSection("HostedUrl").Value + "api/campaigngoogleanalyticss/GoogleExchangeCodeForPopup";
                var httpClient = new HttpClient
                {
                    BaseAddress = new Uri("https://www.googleapis.com/oauth2/v4"),
                };

                var data = new
                {
                    client_id = clientId,
                    client_secret = clientSecret,
                    code = accessToken,
                    grant_type = "refresh_token",
                    //redirect_uri = _callbackUrl
                    access_type = "offline"
                };

                var stringContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                var resp = await httpClient.PostAsync("/token", stringContent);
                var resp1 = await resp.Content.ReadAsStringAsync();
                var gscResponse = JsonConvert.DeserializeObject<GaToken>(await resp.Content.ReadAsStringAsync());

                return gscResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<bool> IsPropertiesExists(string access_token)
        {
            var googleApiKey = _configuration.GetSection("GoogleApiKey").Value;

            try
            {
                string _clientId = _configuration.GetSection("ClientIdForGoogleAds").Value;
                string _clientSecret = _configuration.GetSection("ClientSecretForGoogleAds").Value;


                var options = new RestClientOptions("https://searchconsole.googleapis.com/webmasters/v3/")
                {
                    Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(access_token, "Bearer")
                };

                var clientForData = new RestClient(options);

                var requestForData = new RestRequest("sites", Method.Get);
                requestForData.AddHeader("Content-Type", "application/json");
                requestForData.AddHeader("key", googleApiKey);

                var response = await clientForData.GetAsync(requestForData);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return false;
        }


        public async Task<RootObjectOfGSCList> GetGSCList(Guid campaignId)
        {
            RootObjectOfGSCList list = new RootObjectOfGSCList();
            var campaign = new CampaignGSC();
            var googleApiKey = _configuration.GetSection("GoogleApiKey").Value;

            try
            {
                campaign = _campaigngscRepository.GetAllEntities(true).Where(x => x.CampaignID == campaignId).FirstOrDefault();
                if (campaign != null)
                {
                    string _clientId = _configuration.GetSection("ClientIdForGoogleAds").Value;
                    string _clientSecret = _configuration.GetSection("ClientSecretForGoogleAds").Value;


                    var options = new RestClientOptions("https://searchconsole.googleapis.com/webmasters/v3/")
                    {
                        Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(campaign.AccessToken, "Bearer")
                    };

                    var clientForData = new RestClient(options);

                    var requestForData = new RestRequest("sites", Method.Get);
                    requestForData.AddHeader("Content-Type", "application/json");
                    requestForData.AddHeader("key", googleApiKey);

                    var response = await clientForData.GetAsync(requestForData);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        list = JsonConvert.DeserializeObject<RootObjectOfGSCList>(response.Content);
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        var accessToken = await _reportSchedulingService.GetAccessTokenUsingRefreshToken(campaign.RefreshToken);

                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            campaign.AccessToken = accessToken;
                            campaign.UpdatedOn = DateTime.UtcNow;
                            _campaigngscRepository.UpdateEntity(campaign);
                            _campaigngscRepository.SaveChanges();

                            options = new RestClientOptions("https://searchconsole.googleapis.com/webmasters/v3/")
                            {
                                Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(campaign.AccessToken, "Bearer")
                            };

                            response = await clientForData.GetAsync(requestForData);
                        }

                           

                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            list = JsonConvert.DeserializeObject<RootObjectOfGSCList>(response.Content);
                        }
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
                    _campaigngscRepository.UpdateEntity(campaign);
                    _campaigngscRepository.SaveChanges();

                    var options = new RestClientOptions("https://searchconsole.googleapis.com/webmasters/v3/")
                    {
                        Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(accessToken, "Bearer")
                    };

                    var clientForData = new RestClient(options);

                    var requestForData = new RestRequest("sites", Method.Get);
                    requestForData.AddHeader("Content-Type", "application/json");
                    requestForData.AddHeader("key", googleApiKey);
                    var response = await clientForData.GetAsync(requestForData);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        list = JsonConvert.DeserializeObject<RootObjectOfGSCList>(response.Content);
                        return list;
                    }
                    throw ex;
                }
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
            return "Id, UrlOrName, CampaignID, IsActive, Campaign, AccessToken,EmailId";
        }

        #endregion
    }
}
