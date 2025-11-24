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
using System.IdentityModel.Tokens.Jwt;
using RestSharp.Authenticators;
using System.Net.Http;
using System.Text;
using Stripe;
using Google.Rpc;
using static Google.Rpc.Context.AttributeContext.Types;
using AutoMapper;
using System.IO;
using File = System.IO.File;
using Microsoft.AspNetCore.Hosting;
using static System.Net.Mime.MediaTypeNames;
using RestSharp.Authenticators.OAuth2;
using System.Linq.Dynamic.Core;
using IdentityModel.Client;
using Microsoft.Extensions.Options;
using Google.Protobuf.WellKnownTypes;
using SendGrid;
using Method = RestSharp.Method;

namespace EventManagement.Service
{
    public class CampaignLinkedinService : ServiceBase<CampaignLinkedin, Guid>, ICampaignLinkedinService
    {

        #region PRIVATE MEMBERS

        private readonly ICampaignLinkedinRepository _campaignlinkedinRepository;
        private readonly IConfiguration _configuration;
        // private readonly ICampaignLinkedinService _campaignLinkedinService;
        private readonly IHostingEnvironment _hostingEnvironment;
        #endregion


        #region CONSTRUCTOR

        public CampaignLinkedinService(IUserInfoService userinfoService,
            ICampaignLinkedinRepository campaignlinkedinRepository,
            ILogger<CampaignLinkedinService> logger,
            IConfiguration configuration, IHostingEnvironment hostingEnvironment) : base(campaignlinkedinRepository, logger)
        {
            _campaignlinkedinRepository = campaignlinkedinRepository;
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
            //_campaignLinkedinService = campaignLinkedinService;

        }

        #endregion


        #region PUBLIC MEMBERS   
        public string LinkedinSetup(string source,string type)
        {
            string _clientId = _configuration["LinkedinClientID"];
            string _clientSecret = _configuration["LinkedinSeceretId"];
            string _callbackUrl;
            var client = new RestClient("https://www.linkedin.com/oauth/v2");

            if (source == "tab")
            {
                _callbackUrl = _configuration["HostedUrl"] + "api/campaignlinkedins/LinkedInCallbackWithCode";
            }
            else
            {
                _callbackUrl = _configuration["HostedUrl"] + "api/campaignlinkedins/LinkedInCallbackWithCodeForPopUp";
            }

            var request = new RestRequest("/authorization", Method.Post);
            request.AddHeader("Linkedin-Version", "202305");
            request.AddHeader("X-Restli-Protocol-Version", "2.0.0");

            request.AddParameter("client_id", _clientId, ParameterType.GetOrPost);
            request.AddParameter("client_secret", _clientSecret, ParameterType.GetOrPost);
            request.AddParameter("redirect_uri", _callbackUrl);
            request.AddParameter("response_type", "code");

            if (type == "LinkedIn")
            {
                request.AddParameter("scope", "r_organization_social%2Crw_organization_admin%2Cr_ads%2Crw_ads%2Cr_ads_reporting", ParameterType.GetOrPost);
            }
            else if(type == "LinkedIn_Ads")
            {

                request.AddParameter("scope", "r_organization_social%2Crw_organization_admin%2Cr_ads%2Crw_ads%2Cr_ads_reporting", ParameterType.GetOrPost);
            }

            var response = client.GetAsync(request).Result;

            //Remove %2025

            var decodeUrl = WebUtility.UrlDecode(response.ResponseUri.ToString());

            return decodeUrl;
        }

        public async Task<LinkedinToken> GetAccessTokenUsingCode(string code, string source, string type )
        {
            string _clientId = _configuration["LinkedinClientID"];
            string _clientSecret = _configuration["LinkedinSeceretId"];
            string _callbackUrl;

            var client = new RestClient("https://www.linkedin.com/oauth/v2");
            if (source == "tab")
            {
                _callbackUrl = _configuration["HostedUrl"] + "api/campaignlinkedins/LinkedInCallbackWithCode";
            }
            else
            {
                _callbackUrl = _configuration["HostedUrl"] + "api/campaignlinkedins/LinkedInCallbackWithCodeForPopUp";
            }

            var request = new RestRequest("/accessToken", Method.Post);
            request.AddHeader("Linkedin-Version", "202305");
            request.AddHeader("X-Restli-Protocol-Version", "2.0.0");

            if (type == "LinkedIn")
            {
                request.AddParameter("scope", "r_organization_social%2Crw_organization_admin", ParameterType.GetOrPost);
            }
            else if (type == "LinkedIn_Ads")
            {
                request.AddParameter("scope", "r_ads%2Crw_ads%2Cr_ads_reporting", ParameterType.GetOrPost);
            }

            request.AddParameter("grant_type", "authorization_code");
            request.AddParameter("code", code);
            request.AddParameter("client_id", _clientId);
            request.AddParameter("client_secret", _clientSecret);
            request.AddParameter("redirect_uri", _callbackUrl);
            var response = await client.GetAsync(request);
            var data = JsonConvert.DeserializeObject<LinkedinToken>(response.Content);

            return data;

        }

        public async Task<LinkedinRoot> GetLinkedInPages(Guid campaignId)
        {
            var returnData = new LinkedinRoot();
            var campaign = _campaignlinkedinRepository.GetAllEntities().Where(x => x.CampaignID == campaignId).FirstOrDefault();
            var options = new RestClientOptions("https://api.linkedin.com/rest/")
            {
                Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(campaign.AccessToken, "Bearer")
            };

            var clientForData = new RestClient(options);

            var requestForData = new RestRequest("/organizationAcls?q=roleAssignee&role=ADMINISTRATOR&projection=(elements*(*,roleAssignee~(localizedFirstName,localizedLastName),organization~(localizedName)))", Method.Post);

            requestForData.AddHeader("Linkedin-Version", "202303");
            requestForData.AddHeader("X-Restli-Protocol-Version", "2.0.0");
            if (campaign != null)
            {
                try
                {                  
                    var response = await clientForData.GetAsync(requestForData);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        returnData = JsonConvert.DeserializeObject<LinkedinRoot>(response.Content);
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        var res = await GetAccessTokenUsingRefreshToken(campaign.RefreshToken);
                        if (res != null && !string.IsNullOrEmpty(res.access_token))
                        {
                            campaign.AccessToken = res.access_token;
                            campaign.AccessTokenExpiresIn = res.expires_in;
                            campaign.RefreshToken = res.refresh_token;
                            campaign.RefreshTokenExpiresIn = res.refresh_token_expires_in;
                            campaign.UpdatedOn = DateTime.UtcNow;

                            _campaignlinkedinRepository.UpdateEntity(campaign);
                            _campaignlinkedinRepository.SaveChanges();

                            options = new RestClientOptions("https://api.linkedin.com/rest/")
                            {
                                Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(campaign.AccessToken, "Bearer")
                            };

                            clientForData = new RestClient(options);

                            response = clientForData.GetAsync(requestForData).Result;
                            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                returnData = JsonConvert.DeserializeObject<LinkedinRoot>(response.Content);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if(ex.Message == "Request failed with status code Unauthorized" && campaign != null)
                    {
                        var res = await GetAccessTokenUsingRefreshToken(campaign.RefreshToken);

                        if (res != null)
                        {
                            campaign.AccessToken = res.access_token;
                            campaign.AccessTokenExpiresIn = res.expires_in;
                            campaign.RefreshToken = res.refresh_token;
                            campaign.RefreshTokenExpiresIn = res.refresh_token_expires_in;
                            campaign.UpdatedOn = DateTime.UtcNow;

                            _campaignlinkedinRepository.UpdateEntity(campaign);
                            _campaignlinkedinRepository.SaveChanges();

                        }

                        options = new RestClientOptions("https://api.linkedin.com/rest/")
                        {
                            Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(campaign.AccessToken, "Bearer")
                        };
                        clientForData = new RestClient(options);

                        var response = clientForData.GetAsync(requestForData).Result;
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            returnData = JsonConvert.DeserializeObject<LinkedinRoot>(response.Content);
                        }
                    }
                    return returnData;
                }

            }
            return returnData;
        }
        /// <summary>
        /// GetLinkedinPageFollowers
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <returns>followers data</returns>
        public string GetLinkedinPageFollowers(string campaignId)
        {
            var campaign = _campaignlinkedinRepository.GetAllEntities().Where(x => x.CampaignID == new Guid(campaignId)).FirstOrDefault();
            if (campaign != null)
            {
                var options = new RestClientOptions("https://api.linkedin.com/rest/")
                {
                    Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(campaign.AccessToken, "Bearer")
                };

                var clientForData = new RestClient(options);

                var requestForData = new RestRequest("/networkSizes/urn:li:organization:" + campaign.OrganizationalEntity + "?edgeType=CompanyFollowedByMember", Method.Post);

                requestForData.AddHeader("Linkedin-Version", "202303");
                requestForData.AddHeader("X-Restli-Protocol-Version", "2.0.0");

                var response = clientForData.GetAsync(requestForData).Result;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return response.Content;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var res = GetAccessTokenUsingRefreshToken(campaign.RefreshToken).Result;

                    if(res != null && !string.IsNullOrEmpty(res.access_token))
                    {
                        campaign.AccessToken = res.access_token;
                        campaign.AccessTokenExpiresIn = res.expires_in;
                        campaign.RefreshToken = res.refresh_token;
                        campaign.RefreshTokenExpiresIn = res.refresh_token_expires_in;
                        campaign.UpdatedOn = DateTime.UtcNow;

                        _campaignlinkedinRepository.UpdateEntity(campaign);
                        _campaignlinkedinRepository.SaveChanges();

                        options = new RestClientOptions("https://api.linkedin.com/rest/")
                        {
                            Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(campaign.AccessToken, "Bearer")
                        };

                        clientForData = new RestClient(options);
                        response = clientForData.GetAsync(requestForData).Result;
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            return response.Content;
                        }
                    }
                    
                }

            }
            return "";
        }

        /// <summary>
        /// GetLinkedinTotalShareStatistics
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <param name="startTime">startTime</param>
        /// <param name="endTime">endTime</param>
        /// <returns>data</returns>
        public string GetLinkedinTotalShareStatistics(string campaignId, string startTime, string endTime)
        {
            var campaign = _campaignlinkedinRepository.GetAllEntities().Where(x => x.CampaignID == new Guid(campaignId)).FirstOrDefault();
            if (campaign != null)
            {
                var timeInterval = "(timeRange:(start:" + startTime + ",end:" + endTime + "),timeGranularityType:DAY)";


                var requestForData = new RestRequest("organizationalEntityShareStatistics", Method.Post);
                requestForData.AddHeader("Linkedin-Version", "202304");
                requestForData.AddHeader("X-Restli-Protocol-Version", "2.0.0");

                requestForData.AddParameter("q", "organizationalEntity");
                requestForData.AddParameter("organizationalEntity", "urn:li:organization:" + campaign.OrganizationalEntity);

                requestForData.AddQueryParameter("timeIntervals", timeInterval, false);


                var options = new RestClientOptions("https://api.linkedin.com/rest/")
                {
                    Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(campaign.AccessToken, "Bearer")
                };
                var clientForData = new RestClient(options);

                var response = clientForData.ExecuteGetAsync(requestForData).Result;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return response.Content;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var res = GetAccessTokenUsingRefreshToken(campaign.RefreshToken).Result;
                    if (res != null && !string.IsNullOrEmpty(res.access_token))
                    {
                        campaign.AccessToken = res.access_token;
                        campaign.AccessTokenExpiresIn = res.expires_in;
                        campaign.RefreshToken = res.refresh_token;
                        campaign.RefreshTokenExpiresIn = res.refresh_token_expires_in;
                        campaign.UpdatedOn = DateTime.UtcNow;

                        _campaignlinkedinRepository.UpdateEntity(campaign);
                        _campaignlinkedinRepository.SaveChanges();

                        options = new RestClientOptions("https://api.linkedin.com/rest/")
                        {
                            Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(campaign.AccessToken, "Bearer")
                        };

                        clientForData = new RestClient(options);

                        response = clientForData.ExecuteGetAsync(requestForData).Result;


                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            return response.Content;
                        }
                    }
                }
            }
            return "";
        }

        /// <summary>
        /// GetLinkedinTotalOrganicPaidFollowerStatistics
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <param name="startTime">startTime</param>
        /// <param name="endTime">endTime</param>
        /// <returns></returns>
        public string GetLinkedinTotalOrganicPaidFollowerStatistics(string campaignId, string startTime, string endTime)
        {
            var campaign = _campaignlinkedinRepository.GetAllEntities().Where(x => x.CampaignID == new Guid(campaignId)).FirstOrDefault();
            var options = new RestClientOptions("https://api.linkedin.com/rest/")
            {
                Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(campaign.AccessToken, "Bearer")
            };

            var clientForData = new RestClient(options);

            var timeInterval = "(timeRange:(start:" + startTime + ",end:" + endTime + "),timeGranularityType:DAY)";

            var requestForData = new RestRequest("organizationalEntityFollowerStatistics", Method.Post);
            requestForData.AddParameter("q", "organizationalEntity");
            requestForData.AddParameter("organizationalEntity", "urn:li:organization:" + campaign.OrganizationalEntity);
            requestForData.AddQueryParameter("timeIntervals", timeInterval, false);

            requestForData.AddHeader("Linkedin-Version", "202304");
            requestForData.AddHeader("X-Restli-Protocol-Version", "2.0.0");

            try
            {                
                if (campaign != null)
                {                  
                    var response = clientForData.GetAsync(requestForData).Result;

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return response.Content;
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        var res = GetAccessTokenUsingRefreshToken(campaign.RefreshToken).Result;
                        if (res != null && !string.IsNullOrEmpty(res.access_token))
                        {
                            campaign.AccessToken = res.access_token;
                            campaign.AccessTokenExpiresIn = res.expires_in;
                            campaign.RefreshToken = res.refresh_token;
                            campaign.RefreshTokenExpiresIn = res.refresh_token_expires_in;
                            campaign.UpdatedOn = DateTime.UtcNow;

                            _campaignlinkedinRepository.UpdateEntity(campaign);
                            _campaignlinkedinRepository.SaveChanges();

                            options = new RestClientOptions("https://api.linkedin.com/rest/")
                            {
                                Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(campaign.AccessToken, "Bearer")
                            };

                            clientForData = new RestClient(options);

                            response = clientForData.GetAsync(requestForData).Result;

                            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                return response.Content;
                            }
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                if (ex.Message == "Request failed with status code Unauthorized" && campaign != null)
                {
                    var res = GetAccessTokenUsingRefreshToken(campaign.RefreshToken).Result;
                    if (res != null)
                    {
                        campaign.AccessToken = res.access_token;
                        campaign.AccessTokenExpiresIn = res.expires_in;
                        campaign.RefreshToken = res.refresh_token;
                        campaign.RefreshTokenExpiresIn = res.refresh_token_expires_in;
                        campaign.UpdatedOn = DateTime.UtcNow;

                        _campaignlinkedinRepository.UpdateEntity(campaign);
                        _campaignlinkedinRepository.SaveChanges();
                    }
                   
                    options = new RestClientOptions("https://api.linkedin.com/rest/")
                    {
                        Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(campaign.AccessToken, "Bearer")
                    };

                    clientForData = new RestClient(options);

                    var response = clientForData.GetAsync(requestForData).Result;

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return response.Content;
                    }
                }
                else
                {
                    return "";
                }
            }
            return "";
        }

        public RootLinkedInDataObject PrepareLinkedinEngagement(string campaignId, string startTime, string endTime)
        {
            var rootObj = new RootLinkedInDataObject();

            // prepare follower gains data
            var TotalOrganicPaidFollowerStatistics = GetLinkedinTotalOrganicPaidFollowerStatistics(campaignId.ToString(), startTime, endTime);
            var followerGainsData = JsonConvert.DeserializeObject<RootFollowerGains>(TotalOrganicPaidFollowerStatistics);
            rootObj.FollowerGains = followerGainsData == null ? new
                List<FollowerGains>() : followerGainsData.elements.Select(x => x.followerGains).ToList();


            // prepare share statistics data
            var TotalShareStatistics = GetLinkedinTotalShareStatistics(campaignId.ToString(), startTime, endTime);
            var shareStatisticsData = JsonConvert.DeserializeObject<RootTotalShareStatistics>(TotalShareStatistics);

            rootObj.ShareStatistics = shareStatisticsData == null ? new List<TotalShareStatistics>() : shareStatisticsData.elements.Select(x => x.totalShareStatistics).ToList();

            rootObj.Dates = new List<string>(){ startTime, endTime };
            return rootObj;
        }

        public LinkedInDemographicChart GetLinkedinDemographicData(Guid campaignId)
        {
            return CalculateLinkedinDemographic(campaignId);
        }

        private LinkedInDemographicChart CalculateLinkedinDemographic(Guid campaignId)
        {
            var retVal = new LinkedInDemographicChart();

            // prepare follower gains data
            var demographicData = GetLinkedinTotalDemographicStatistics(campaignId.ToString());

            var linkedinDemographicDto = JsonConvert.DeserializeObject<LinkedinDemographic>(demographicData);

            var demographicCode = new LinkedInDemographicCode();
            var countryLocation = new CountryLocation();
            var restClient = new RestClient(_configuration["BlobUrl"] + "Json");
            var restRequest = new RestRequest("/countries.json", Method.Get);

            var response = restClient.GetAsync(restRequest).Result;
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                demographicCode = JsonConvert.DeserializeObject<LinkedInDemographicCode>(response.Content);
            }

            //string path = Path.GetFullPath(Path.Combine(_hostingEnvironment.ContentRootPath, @"..\\EventManagement.Utility\\Json\\countries.json"));
            //string text = File.ReadAllText(path);
            //var demographicCode = JsonConvert.DeserializeObject<LinkedInDemographicCode>(text);
            if (linkedinDemographicDto != null && linkedinDemographicDto.elements != null && linkedinDemographicDto.elements.Count > 0)
            {
                var topFiveCoutriesFollower = linkedinDemographicDto.elements[0].followerCountsByGeoCountry != null ? linkedinDemographicDto.elements[0].followerCountsByGeoCountry.OrderByDescending(x => x.followerCounts.organicFollowerCount).Take(5).ToList() : new List<FollowerCountsByCountry>();

                var topFiveSeniority = linkedinDemographicDto.elements[0].followerCountsBySeniority.OrderByDescending(x => x.followerCounts.organicFollowerCount).Take(5).ToList();

                var topFiveIndustries = linkedinDemographicDto.elements[0].followerCountsByIndustry.OrderByDescending(x => x.followerCounts.organicFollowerCount).Take(5).ToList();

                var topFiveJobFunction = linkedinDemographicDto.elements[0].followerCountsByFunction.OrderByDescending(x => x.followerCounts.organicFollowerCount).Take(5).ToList();

                var topFiveCompanySize = linkedinDemographicDto.elements[0].followerCountsByStaffCountRange.OrderByDescending(x => x.followerCounts.organicFollowerCount).Take(5).ToList();

                topFiveCoutriesFollower = PrepareCityInListOfFollowerCountry(topFiveCoutriesFollower, demographicCode, campaignId);
                
                foreach (var con in topFiveSeniority)
                {
                    var code = con.seniority;
                    var seniorityName = demographicCode.seniority.Where(y => y.code.Equals(code)).Select(x => x.name).FirstOrDefault();
                    con.name = !String.IsNullOrEmpty(seniorityName) ? seniorityName : "Others";
                }

                foreach (var con in topFiveIndustries)
                {
                    var code = con.industry;
                    var industriesName = demographicCode.industries.Where(y => y.URN.Equals(code)).Select(x => x.name.localized.en_US).FirstOrDefault();
                    con.name = !String.IsNullOrEmpty(industriesName) ? industriesName : "Others";
                }

                foreach (var con in topFiveJobFunction)
                {
                    var code = con.function;
                    var jobFunctionName = demographicCode.job_function.Where(y => y.URN.Equals(code)).Select(x => x.name.localized.en_US).FirstOrDefault();
                    con.name = !String.IsNullOrEmpty(jobFunctionName) ? jobFunctionName : "Others";
                }

                foreach (var con in topFiveCompanySize)
                {
                    var code = con.staffCountRange;
                    var companySizeName = demographicCode.company_size.Where(y => y.code.Equals(code)).Select(x => x.name).FirstOrDefault();
                    con.name = !String.IsNullOrEmpty(companySizeName) ? companySizeName : "Others";
                }

                var topFiveCoutriesFollowerData = topFiveCoutriesFollower.Select(x => x.followerCounts.organicFollowerCount).ToList();

                var topFiveCoutriesFollowerLabel = topFiveCoutriesFollower.Select(x => x.countryName).ToList();


                var topFiveSeniorityData = topFiveSeniority.Select(x => x.followerCounts.organicFollowerCount).ToList();

                var topFiveSeniorityLabel = topFiveSeniority.Select(x => x.name).ToList();


                var topFiveIndustriesData = topFiveIndustries.Select(x => x.followerCounts.organicFollowerCount).ToList();

                var topFiveIndustriesLabel = topFiveIndustries.Select(x => x.name).ToList();


                var topFiveJobData = topFiveJobFunction.Select(x => x.followerCounts.organicFollowerCount).ToList();

                var topFiveJobLabel = topFiveJobFunction.Select(x => x.name).ToList();


                var topFiveCompanySizeData = topFiveCompanySize.Select(x => x.followerCounts.organicFollowerCount).ToList();

                var topFiveCompanySizeLabel = topFiveCompanySize.Select(x => x.name).ToList();

                retVal.CountryLabel = topFiveCoutriesFollowerLabel;
                retVal.CountryData = topFiveCoutriesFollowerData;

                retVal.SeniorityLabel = topFiveSeniorityLabel;
                retVal.SeniorityData = topFiveSeniorityData;

                retVal.IndustryData = topFiveIndustriesData;
                retVal.IndustryLabel = topFiveIndustriesLabel;

                retVal.JobFunctionData = topFiveJobData;
                retVal.JobFunctionLabel = topFiveJobLabel;

                retVal.CompanySizeData = topFiveCompanySizeData;
                retVal.CompanySizeLabel = topFiveCompanySizeLabel;

                return retVal;
            }
            else
            {
                return retVal;
            }
        }

        public List<FollowerCountsByCountry> PrepareCityInListOfFollowerCountry(List<FollowerCountsByCountry> listOfFollowers, LinkedInDemographicCode demographicCode, Guid campaignId)
        {
            try
            {
                var countryLocation = new CountryLocation();

                var campaign = _campaignlinkedinRepository.GetAllEntities().Where(x => x.CampaignID == campaignId).FirstOrDefault();
                if (campaign != null)
                {
                    foreach (var con in listOfFollowers)
                    {
                        var countrycode = con.geo.Split(":");

                        var options = new RestClientOptions("https://api.linkedin.com/rest/")
                        {
                            Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(campaign.AccessToken, "Bearer")
                        };

                        var clientForData = new RestClient(options);

                        var restRequest1 = new RestRequest("geo/" + countrycode[3], Method.Get);

                        restRequest1.AddHeader("Linkedin-Version", "202305");
                        restRequest1.AddHeader("X-Restli-Protocol-Version", "2.0.0");

                        restRequest1.AddHeader("Content-Type", "application/json");

                        var res = clientForData.GetAsync(restRequest1).Result;

                        if (res.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            countryLocation = JsonConvert.DeserializeObject<CountryLocation>(res.Content);
                            if (!string.IsNullOrEmpty(countryLocation.defaultLocalizedName.value))
                            {
                                var country = countryLocation.defaultLocalizedName.value;

                                con.countryName = !String.IsNullOrEmpty(country) ? country : "Others";
                            }
                        }
                    }                    
                }
            }
            catch(Exception ex)
            {
                return new List<FollowerCountsByCountry>();
            }

            return listOfFollowers;
        }


        /// <summary>
        /// GetLinkedinTotalDemographicStatistics
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <returns>data</returns>
        public string GetLinkedinTotalDemographicStatistics(string campaignId)
        {
            var campaign = _campaignlinkedinRepository.GetAllEntities().Where(x => x.CampaignID == new Guid(campaignId)).FirstOrDefault();
            if (campaign != null)
            {
                var requestForData = new RestRequest("organizationalEntityFollowerStatistics", Method.Post);
                requestForData.AddParameter("q", "organizationalEntity");
                requestForData.AddParameter("organizationalEntity", "urn:li:organization:" + campaign.OrganizationalEntity);
                requestForData.AddHeader("Linkedin-Version", "202303");
                requestForData.AddHeader("X-Restli-Protocol-Version", "2.0.0");

              
                var options = new RestClientOptions("https://api.linkedin.com/rest/")
                {
                    Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(campaign.AccessToken, "Bearer")
                };

                var clientForData = new RestClient(options);

                var response = clientForData.ExecuteGetAsync(requestForData).Result;
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return response.Content;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var res = GetAccessTokenUsingRefreshToken(campaign.RefreshToken).Result;
                    if (res != null && !string.IsNullOrEmpty(res.access_token))
                    {
                        campaign.AccessToken = res.access_token;
                        campaign.AccessTokenExpiresIn = res.expires_in;
                        campaign.RefreshToken = res.refresh_token;
                        campaign.RefreshTokenExpiresIn = res.refresh_token_expires_in;
                        campaign.UpdatedOn = DateTime.UtcNow;

                        _campaignlinkedinRepository.UpdateEntity(campaign);
                        _campaignlinkedinRepository.SaveChanges();

                        options = new RestClientOptions("https://api.linkedin.com/rest/")
                        {
                            Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(campaign.AccessToken, "Bearer")
                        };

                        clientForData = new RestClient(options);

                        response = clientForData.ExecuteGetAsync(requestForData).Result;

                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            return response.Content;
                        }
                    }
                }
            }
            return "";
        }

        public List<CampaignLinkedinDto> GetCampaignLinkedinByCampaignId(string campaignId)
        {
            var linkedinSetup = _campaignlinkedinRepository.GetAllEntities(true).Where(x => x.CampaignID == new Guid(campaignId))
               .Select(linkedin => new CampaignLinkedinDto
               {
                   Id = linkedin.Id,
                   AccessToken = linkedin.AccessToken,
                   RefreshToken = linkedin.RefreshToken,
                   RefreshTokenExpiresIn = linkedin.RefreshTokenExpiresIn,
                   CampaignID = linkedin.CampaignID,
                   OrganizationalEntity = linkedin.OrganizationalEntity
               }).ToList();

            return linkedinSetup;
        }

        public async Task<LinkedinToken> GetAccessTokenUsingRefreshToken(string refresh_token)
         {
            try
            {
                string _clientId = _configuration["LinkedinClientID"];
                string _clientSecret = _configuration["LinkedinSeceretId"];

                var client = new RestClient("https://www.linkedin.com/oauth/v2");

                var request = new RestRequest("/accessToken", Method.Post);

                request.AddParameter("grant_type", "refresh_token");
                request.AddParameter("refresh_token", refresh_token);
                request.AddParameter("client_id", _clientId);
                request.AddParameter("client_secret", _clientSecret);

                var response = client.GetAsync(request).Result;
                var data = JsonConvert.DeserializeObject<LinkedinToken>(response.Content);

                return data;
            }
            catch(Exception ex)
            {
                var test = ex;
            }

            return new LinkedinToken();

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
            return "Id,PageName,OrganizationalEntity,CampaignID";
        }

        #endregion
    }
}
