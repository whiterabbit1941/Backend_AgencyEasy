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
using System.Globalization;
using System.Net.Http;
using System.Text;

namespace EventManagement.Service
{
    public class CampaignInstagramService : ServiceBase<CampaignInstagram, Guid>, ICampaignInstagramService
    {

        #region PRIVATE MEMBERS

        private readonly ICampaignInstagramRepository _campaigninstagramRepository;
        private readonly IConfiguration _configuration;
        private readonly ICampaignFacebookService _campaignFacebookService;
        #endregion


        #region CONSTRUCTOR

        public CampaignInstagramService(ICampaignFacebookService campaignFacebookService, ICampaignInstagramRepository campaigninstagramRepository, ILogger<CampaignInstagramService> logger, IConfiguration configuration) : base(campaigninstagramRepository, logger)
        {
            _campaigninstagramRepository = campaigninstagramRepository;
            _configuration = configuration;
            _campaignFacebookService = campaignFacebookService;
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
                                                 (permissionDto.permission == "instagram_basic" ||
                                                 permissionDto.permission == "pages_read_engagement" ||
                                                 permissionDto.permission == "pages_show_list" ||
                                                 permissionDto.permission == "instagram_manage_insights" ||
                                                 permissionDto.permission == "read_insights")) ?? false;

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


        public async Task<RootObjectFBData> GetFaceBookPageList(Guid campaignId)
        {
            var returnData = new RootObjectFBData();
            returnData.data = new List<FacebookList>();

            var campaign = _campaigninstagramRepository.GetAllEntities(true).Where(x => x.CampaignID == campaignId).FirstOrDefault();
            if (campaign != null)
            {
                var isTokenValid = await _campaignFacebookService.GetAccessTokenDetails(campaign.AccessToken);

                if (isTokenValid.data.is_valid == true)
                {
                    // create client
                    var client = new RestClient("https://graph.facebook.com");

                    var request = new RestRequest("/me/accounts?limit=1000", Method.Get);

                    // add header
                    request.AddHeader("Content-Type", "application/json");

                    // add params
                    request.AddParameter("access_token", campaign.AccessToken);

                    var response = client.GetAsync(request).Result;

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        returnData = JsonConvert.DeserializeObject<RootObjectFBData>(response.Content);
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

        public async Task<RootObjectInstaData> GetInstaIds(List<FacebookList> facebookPage, Guid campaignId)
        {
            var returnData = new RootObjectInstaData();
            returnData.data = new List<InstaList>();

            var campaign = _campaigninstagramRepository.GetAllEntities(true).Where(x => x.CampaignID == campaignId).FirstOrDefault();
            if (campaign != null)
            {
                var isTokenValid = await _campaignFacebookService.GetAccessTokenDetails(campaign.AccessToken);

                if (isTokenValid.data.is_valid == true)
                {
                    // create client
                    var client = new RestClient("https://graph.facebook.com/");

                    foreach (var temp in facebookPage)
                    {
                        var request = new RestRequest(temp.id, Method.Get);

                        // add header
                        request.AddHeader("Content-Type", "application/json");

                        // add params
                        request.AddParameter("fields", "instagram_business_account,access_token");
                        request.AddParameter("access_token", campaign.AccessToken);

                        var response = client.GetAsync(request).Result;

                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var res = JsonConvert.DeserializeObject<dynamic>(response.Content);
                            if (!String.IsNullOrEmpty(Convert.ToString(res.instagram_business_account)))
                            {
                                string instaId = res.instagram_business_account.id;
                                string accessToken1 = res.access_token;
                                returnData.data.Add(new InstaList { id = instaId, name = accessToken1 });
                            }
                        }
                    }
                }
                else
                {
                    if (isTokenValid.data.error.subcode == 460)
                    {
                        returnData.errror_msg = "Your Instagram password has been changed. Please re-integrate it again.";
                    }
                    else
                    {
                        returnData.errror_msg = "Something went wrong";
                    }
                }
            }
            return returnData;
        }

        public async Task<RootObjectInstaData> GetInstagramPageLists(List<InstaList> instaids, Guid campaignId)
        {
            var returnData = new RootObjectInstaData();
            returnData.data = new List<InstaList>();

            var campaign = _campaigninstagramRepository.GetAllEntities(true).Where(x => x.CampaignID == campaignId).FirstOrDefault();
            if (campaign != null)
            {
                var isTokenValid = await _campaignFacebookService.GetAccessTokenDetails(campaign.AccessToken);

                if (isTokenValid.data.is_valid == true)
                {
                    // create client
                    var client = new RestClient("https://graph.facebook.com/");

                    foreach (var temp in instaids)
                    {
                        var request = new RestRequest(temp.id, Method.Get);

                        // add header
                        request.AddHeader("Content-Type", "application/json");

                        // add params
                        request.AddParameter("fields", "name,username,ig_id,id");
                        request.AddParameter("access_token", campaign.AccessToken);

                        var response = client.GetAsync(request).Result;

                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var responseData = JsonConvert.DeserializeObject<InstaList>(response.Content);

                            responseData.name= string.IsNullOrEmpty(responseData.name) ? responseData.username : responseData.name;

                            returnData.data.Add(responseData);
                        }
                    }
                }
                else
                {
                    if (isTokenValid.data.error.subcode == 460)
                    {
                        returnData.errror_msg = "Your Instagram password has been changed. Please re-integrate it again.";
                    }
                    else
                    {
                        returnData.errror_msg = "Something went wrong";
                    }
                }
            }
            return returnData;
        }

        public async Task<InstagramReportsData> GetInstagramReportDataById(Guid campaignId, string fromDate, string toDate)
        {
           
            var returnData = new InstagramReportsData();
            returnData.GenderDataChart = new List<int>();
            returnData.ListOfLocale = new List<ListOfInstaLocale>();
            returnData.ListOfCountries = new List<ListOfInstaLocale>();
            returnData.ListOfCities = new List<ListOfInstaLocale>();

            try
            {
                var campaign = _campaigninstagramRepository.GetAllEntities(true).Where(x => x.CampaignID == campaignId).FirstOrDefault();
                if (campaign != null)
                {
                    var isTokenValid = await _campaignFacebookService.GetAccessTokenDetails(campaign.AccessToken);

                    if (isTokenValid.data.is_valid == true)
                    {
                        var tempsdt = string.Empty;
                        var tempedt = string.Empty;
                        var ListOfDates = new List<FacebookReportDates>();
                        double d = 0;
                        double tempDiff = 0;
                        var startDate = Convert.ToDateTime(fromDate);
                        var endDate = Convert.ToDateTime(toDate);
                        var diff = CalculateDateSlabDiff(startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

                        if (diff > 30)
                        {
                            d = diff / 30;
                            if (d > 0)
                            {
                                d = Math.Floor(d);

                                for (var i = 0; i <= d; i++)
                                {
                                    if (i == 0)
                                    {
                                        tempsdt = startDate.ToString("yyyy-MM-dd");
                                        tempedt = startDate.AddDays(30).ToString("yyyy-MM-dd");
                                        tempDiff = (int)diff - 30;

                                        var fbDates = new FacebookReportDates();
                                        fbDates.startDate = tempsdt;
                                        fbDates.endDate = tempedt;
                                        ListOfDates.Add(fbDates);
                                    }
                                    else
                                    {
                                        tempsdt = Convert.ToDateTime(tempedt).ToString("yyyy-MM-dd");
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
                                        ListOfDates.Add(fbDates);
                                    }
                                }
                            }
                        }
                        else
                        {
                            var fbDates = new FacebookReportDates();
                            fbDates.startDate = Convert.ToDateTime(startDate).ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture);
                            fbDates.endDate = Convert.ToDateTime(endDate).AddDays(1).ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture);
                            ListOfDates.Add(fbDates);
                        }


                        var httpClient = new HttpClient
                        {
                            BaseAddress = new Uri("https://graph.facebook.com/"),
                            //DefaultRequestHeaders = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) }
                        };

                        // get facebook page data
                        var prepareUrl = "me/accounts?limit=1000&access_token=" + campaign.AccessToken;
                        ////Get task status
                        var response = await httpClient.GetAsync(prepareUrl);
                        var data = await response.Content.ReadAsStringAsync();
                        var res = JsonConvert.DeserializeObject<dynamic>(data);

                        List<InstagramIdModel> instaList = new List<InstagramIdModel> { };
                        List<InstagramPageModel> instaPageList = new List<InstagramPageModel> { };
                        var rows = res.data;
                        // get insta page list
                        for (var i = 0; i < rows.Count; i++)
                        {
                            string id = rows[i].id;
                            var prepareUrl1 = id + "?fields=instagram_business_account,access_token&access_token=" + campaign.AccessToken;
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

                        // get insta page list data
                        for (var j = 0; j < instaList.Count; j++)
                        {
                            var prepareUrl2 = instaList[j].instgramid + "?fields=name,username,ig_id,id&access_token=" + campaign.AccessToken;
                            var response2 = await httpClient.GetAsync(prepareUrl2);
                            var data2 = await response2.Content.ReadAsStringAsync();
                            var res2 = JsonConvert.DeserializeObject<dynamic>(data2);
                            instaPageList.Add(new InstagramPageModel { instgramid = instaList[j].instgramid, accessToken = instaList[j].accessToken, pageName = res2.name, username = res2.username });

                        }

                        var currPage = instaPageList
                                    .Where(x => (x.pageName != null && x.pageName.ToLower() == campaign.UrlOrName.ToLower()) ||
                                                (x.pageName == null && x.username.ToLower() == campaign.UrlOrName.ToLower()))
                                    .FirstOrDefault();

                        //var currPage = instaPageList.Where(x => x.pageName.ToLower() == campaign.UrlOrName.ToLower()).FirstOrDefault();
                        var pageId = currPage.instgramid;
                        var pageToken = currPage.accessToken;

                        int totalFemaleFollowers = 0;
                        int totalMaleFollowers = 0;
                        int fTotalFemaleFollowers = 0;
                        int fTotalMaleFollowers = 0;

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
                                            returnData.ProfileViewTotal = returnData.ProfileViewTotal + (int)(l[k].value);
                                        }
                                        returnData.AvgProfileViews = returnData.ProfileViewTotal / l.Count;
                                    }

                                    if (p.name == "impressions")
                                    {
                                        var l = p.values;
                                        for (var k = 0; k < l.Count; k++)
                                        {
                                            returnData.ImpressionsTotal = returnData.ImpressionsTotal + (int)(l[k].value);
                                        }
                                        returnData.AvgImpressions = returnData.ImpressionsTotal / l.Count;
                                    }
                                    if (p.name == "reach")
                                    {
                                        var l = p.values;
                                        for (var k = 0; k < l.Count; k++)
                                        {
                                            returnData.ReachTotal = returnData.ReachTotal + (int)(l[k].value);
                                        }
                                        returnData.AvgReachTotals = returnData.ReachTotal / l.Count;
                                    }
                                    if (p.name == "website_clicks")
                                    {
                                        var l = p.values;
                                        for (var k = 0; k < l.Count; k++)
                                        {
                                            returnData.WebsiteClickTotal = returnData.WebsiteClickTotal + (int)(l[k].value);
                                        }
                                        returnData.AvgWebSiteClickTotal = returnData.WebsiteClickTotal / l.Count;
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
                                prepareUrl3 = pageId + "/insights/impressions,reach,website_clicks,profile_views?access_token=" + pageToken + "&since=" + ListOfDates[0].startDate + "&until=" + ListOfDates[0].endDate + "&period=day";
                            }
                            else
                            {
                                prepareUrl3 = pageId + "/insights/impressions,reach,website_clicks,profile_views?access_token=" + pageToken + "&since=" + ListOfDates[0].startDate + "&until=" + ListOfDates[0].endDate + "&period=day";
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
                                        returnData.ProfileViewTotal = returnData.ProfileViewTotal + (int)(l[k].value);
                                    }
                                    returnData.AvgProfileViews = returnData.ProfileViewTotal / l.Count;
                                }
                                if (p.name == "follower_count")
                                {
                                    var l = p.values;
                                    for (var k = 0; k < l.Count; k++)
                                    {
                                        returnData.FollowersTotal = returnData.FollowersTotal + (int)(l[k].value);
                                    }

                                    returnData.AvgFollowers = returnData.FollowersTotal / l.Count;
                                }
                                if (p.name == "impressions")
                                {
                                    var l = p.values;
                                    for (var k = 0; k < l.Count; k++)
                                    {
                                        returnData.ImpressionsTotal = returnData.ImpressionsTotal + (int)(l[k].value);
                                    }
                                    returnData.AvgImpressions = returnData.ImpressionsTotal / l.Count;
                                }
                                if (p.name == "reach")
                                {
                                    var l = p.values;
                                    for (var k = 0; k < l.Count; k++)
                                    {
                                        returnData.ReachTotal = returnData.ReachTotal + (int)(l[k].value);
                                    }
                                    returnData.AvgReachTotals = returnData.ReachTotal / l.Count;
                                }
                                if (p.name == "website_clicks")
                                {
                                    var l = p.values;
                                    for (var k = 0; k < l.Count; k++)
                                    {
                                        returnData.WebsiteClickTotal = returnData.WebsiteClickTotal + (int)(l[k].value);
                                    }
                                    returnData.AvgWebSiteClickTotal = returnData.WebsiteClickTotal / l.Count;
                                }
                            }

                        }

                        // get insta followers
                        var prepareUrl5 = pageId + "?access_token=" + pageToken + "&fields=followers_count";
                        var response5 = await httpClient.GetAsync(prepareUrl5);
                        var data5 = await response5.Content.ReadAsStringAsync();
                        var res5 = JsonConvert.DeserializeObject<dynamic>(data5);
                        returnData.InstaFollowersCountTotal = res5.followers_count;

                        // get insta audiences

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
                                returnData.ListOfCountries.Add(result.ToListOfInstaLocale());
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
                                returnData.ListOfCities.Add(result.ToListOfInstaLocale());
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

                            returnData.ListOfLocale = groupedData;

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
                            returnData.GenderDataChart.Add(fTotalFemaleFollowers);
                            returnData.GenderDataChart.Add(fTotalMaleFollowers);

                        }

                    }
                    else
                    {
                        if (isTokenValid.data.error.subcode == 460)
                        {
                            returnData.errror_msg = "Your Instagram password has been changed. Please re-integrate it again.";
                        }
                        else
                        {
                            returnData.errror_msg = "Something went wrong";
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                var test = ex;
            }
          
            return returnData;
        }

        private double CalculateDateSlabDiff(string startDate, string endDate)
        {
            var difference = (Convert.ToDateTime(endDate) - Convert.ToDateTime(startDate)).TotalDays; 
            return difference + 1;
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
            return "Id, UrlOrName, CampaignID, IsActive, Campaign, AccessToken, PageToken";
        }

        #endregion
    }
}
