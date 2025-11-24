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
using System.Net.Http;
using EventManagement.Utility.Enums;
using IdentityServer4.Hosting;
using System.Xml.Linq;
using System.IO;
using EventManagement.Domain.Migrations;
using CampaignFacebookAds = EventManagement.Domain.Entities.CampaignFacebookAds;
using System.Globalization;
using RestSharp;

namespace EventManagement.Service
{
    public class CampaignFacebookAdsService : ServiceBase<CampaignFacebookAds, Guid>, ICampaignFacebookAdsService
    {

        #region PRIVATE MEMBERS

        private readonly ICampaignFacebookAdsRepository _campaignfacebookadsRepository;
        private readonly IConfiguration _configuration;
        private readonly ICampaignFacebookService _campaignFacebookService;
        private readonly IReportSchedulingService _reportSchedulingService;

        private HttpClient _httpClient = new HttpClient();
        private string _facebookBaseUrl = "https://graph.facebook.com/v22.0/";

        #endregion


        #region CONSTRUCTOR

        public CampaignFacebookAdsService(ICampaignFacebookService campaignFacebookService,
            ICampaignFacebookAdsRepository campaignfacebookadsRepository,
            ILogger<CampaignFacebookAdsService> logger,
            IConfiguration configuration, IReportSchedulingService reportSchedulingService) : base(campaignfacebookadsRepository, logger)
        {
            _campaignfacebookadsRepository = campaignfacebookadsRepository;
            _configuration = configuration;
            _campaignFacebookService = campaignFacebookService;
            _reportSchedulingService = reportSchedulingService;
        }

        #endregion


        #region PUBLIC MEMBERS   

        public bool IsPermissionGranted(string accessToken)
        {
            try
            {
                // create client
                var client = new RestClient("https://graph.facebook.com");

                var request = new RestRequest("/me/permissions", Method.Get);

                // add header
                request.AddHeader("Content-Type", "application/json");

                // add params
                request.AddParameter("access_token", accessToken);

                var response = client.GetAsync(request).Result;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var permissionData = JsonConvert.DeserializeObject<PermissionDataDto>(response.Content);
                    if (permissionData != null && permissionData.data != null)
                    {
                        bool hasDeclinedStatus = permissionData?.data?.Any(permissionDto =>
                                                 permissionDto.status == "declined" &&
                                                 (permissionDto.permission == "pages_show_list" ||
                                                 permissionDto.permission == "ads_management" ||                                                                                                 
                                                 permissionDto.permission == "pages_read_engagement")) ?? false;                        

                        return hasDeclinedStatus;
                    }
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                var test = ex;
            }

            return true;
        }



        public async Task<RootObjectFBAdsData> GetFbAllAdsAccountDetails(Guid campaignId)
        {
            var returnData = new RootObjectFBAdsData();
            try
            {
                var campaign = _campaignfacebookadsRepository.GetAllEntities(true).Where(x => x.CampaignID == campaignId).FirstOrDefault();
                if (campaign != null)
                {
                    var isTokenValid = await _campaignFacebookService.GetAccessTokenDetails(campaign.AccessToken);

                    if (isTokenValid.data.is_valid == true)
                    {
                        var prepareUrl = _facebookBaseUrl + "me?fields=id,name,adaccounts.limit(1000){account_id,business_name,id,name}&access_token=" + campaign.AccessToken;

                        var response = await _httpClient.GetAsync(prepareUrl);

                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var data = await response.Content.ReadAsStringAsync();

                            var fbAdsdata = JsonConvert.DeserializeObject<FacebookAdsAccount>(data);
                            returnData.data = fbAdsdata;
                            returnData.data.access_token = campaign.AccessToken;
                            returnData.data.ad_account_name = campaign.AdAccountName;
                        }
                    }
                    else
                    {
                        if (isTokenValid.data.error.subcode == 460)
                        {
                            returnData.errror_msg = "Your Facebook password has been changed. Please re-integrate it again.";
                        }
                        else
                        {
                            returnData.errror_msg = "Something went wrong";
                        }
                    }
                }
                return returnData;
            }
            catch (Exception)
            {
                return returnData;
            }
        }

        public async Task<FacebookGetData> GetFbAdsData(Guid campignId, int type, DateTime startDate, DateTime endDate)
        {
            var listOfDates = await _reportSchedulingService.PrepareDateListForFacebook(startDate, endDate);

            var facebookAdsCampaignDto = new FacebookGetData();
            facebookAdsCampaignDto.facebookAdsCampaignData = new FacebookAdsCampaignData();

            try
            {
                var facebookAdsSetup = _campaignfacebookadsRepository.GetFilteredEntities(true).Where(x => x.CampaignID == campignId).FirstOrDefault();

                if (facebookAdsSetup != null)
                {
                    var isTokenValid = await _campaignFacebookService.GetAccessTokenDetails(facebookAdsSetup.AccessToken);
                    if (isTokenValid.data.is_valid)
                    {
                        //Campaign 0, AdGroups 1, AdCopies 2
                        if (type == (int)ReportTypes.FacebookAdsCampaign)
                        {
                            facebookAdsCampaignDto.facebookAdsCampaignData = await _reportSchedulingService.PrepareFacebookAdsCampaignsView(facebookAdsSetup, startDate, endDate, listOfDates);
                        }
                        else if (type == (int)ReportTypes.FacebookAdsGroup)
                        {
                            facebookAdsCampaignDto.facebookAdsCampaignData = await _reportSchedulingService.PrepareFacebookAdsAdsGroupView(facebookAdsSetup, startDate, endDate, listOfDates);
                        }
                        else if (type == (int)ReportTypes.FacebookAdsCopies)
                        {
                            facebookAdsCampaignDto.facebookAdsCampaignData = await _reportSchedulingService.PrepareFacebookAdsCopiesView(facebookAdsSetup, startDate, endDate, listOfDates);
                        }
                    }
                    else
                    {
                        facebookAdsCampaignDto.Error = "Facebook ads token is not valid.";
                    }
                }
                else
                {

                    facebookAdsCampaignDto.Error = "Facebook ads setup not found.";
                }

                return facebookAdsCampaignDto;
            }
            catch (Exception ex)
            {
                facebookAdsCampaignDto.Error = "Facebook Ads Error Message: " + ex.Message;
                return facebookAdsCampaignDto;
            }
        }

        private double CalculateDateSlabDiff(string startDate, string endDate)
        {
            var difference = (Convert.ToDateTime(endDate) - Convert.ToDateTime(startDate)).TotalDays; return difference;
        }
        #endregion

        #region PRIVATE METHODS

        public async Task<object> getFBAdsCampaigns(RootObjectFBAdsData fBData)
        {
            var fbAccountId = fBData.data.adaccounts.data.Find(fn => fn.name == fBData.data.ad_account_name).id;

            var prepareUrl = _facebookBaseUrl + fbAccountId + "?fields=campaigns{name},currency&access_token=" + fBData.data.access_token;

            var response = await _httpClient.GetAsync(prepareUrl);

            return response;
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
            return "Id, AdAccountName, CampaignID, IsActive, AccessToken";
        }

        #endregion
    }
}
