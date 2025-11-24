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
using RestSharp;
using static Google.Apis.Pagespeedonline.v1.Data.Result.FormattedResultsData.RuleResultsDataElement.UrlBlocksData.UrlsData;

namespace EventManagement.Service
{
    public class CampaignFacebookService : ServiceBase<CampaignFacebook, Guid>, ICampaignFacebookService
    {

        #region PRIVATE MEMBERS

        private readonly ICampaignFacebookRepository _campaignfacebookRepository;
        private readonly IConfiguration _configuration;

        #endregion


        #region CONSTRUCTOR

        public CampaignFacebookService(ICampaignFacebookRepository campaignfacebookRepository, ILogger<CampaignFacebookService> logger, IConfiguration configuration) : base(campaignfacebookRepository, logger)
        {
            _campaignfacebookRepository = campaignfacebookRepository;
            _configuration = configuration;
        }

        #endregion


        #region PUBLIC MEMBERS   

        /// <summary>
        /// Get Facebook Report from facebook API
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <param name="startTime">startTime</param>
        /// <param name="endTime">endTime</param>
        /// <returns>FacebookData DTO</returns>
        public async Task<FacebookData> GetFacebookReport(Guid campaignId, DateTime startTime, DateTime endTime)
        {
            var FacebookData = new FacebookData();
            var previousDate = CalculatePreviousStartDateAndEndDate(startTime, endTime);

            var facebookSetup = _campaignfacebookRepository.GetFilteredEntities(true).Where(x => x.CampaignID == campaignId).FirstOrDefault();

            if (facebookSetup != null)
            {
                var isTokenValid = await GetAccessTokenDetails(facebookSetup.AccessToken);

                if (isTokenValid.data.is_valid == true)
                {
                    var getPageDetail = await GetPageDetail(facebookSetup.AccessToken, facebookSetup);

                    FacebookData = await PrepareFacebookReport(startTime, endTime, previousDate, getPageDetail);
                    return FacebookData;
                }
                else
                {
                    if (isTokenValid.data.error.subcode == 460)
                    {
                        FacebookData.ErrorMsg = "Your Facebook password has been changed. Please re-integrate it again.";
                        return FacebookData;
                    }
                    else
                    {
                        return new FacebookData { };
                    }
                }
            }
            else
            {
                return new FacebookData { };
            }
        }

        /// <summary>
        /// Convert StartDate To UTC Format
        /// </summary>
        /// <param name="startDate">startDate</param>
        /// <param name="type">type</param>
        /// <returns>Utc time</returns>
        public DateTime ConvertStartDateToUTCFormat(DateTime startDate, string type)
        {
            var newStartDate = startDate; newStartDate = !string.IsNullOrEmpty(type) ? newStartDate.AddDays(-2) : newStartDate.AddDays(-1);
            newStartDate = newStartDate.AddHours(12).AddMinutes(30);
            var newDate = newStartDate.ToUniversalTime();
            return newDate;
        }

        /// <summary>
        /// Convert EndDate To UTC Format
        /// </summary>
        /// <param name="endDate">endDate</param>
        /// <param name="type">type</param>
        /// <returns>Utc time</returns>
        public DateTime ConvertEndDateToUTCFormat(DateTime endDate, string type)
        {
            var newStartDate = endDate; newStartDate = !string.IsNullOrEmpty(type) ? endDate : newStartDate.AddDays(1);
            newStartDate = newStartDate.AddHours(12).AddMinutes(30);
            var newDate = newStartDate.ToUniversalTime();
            return newDate;
        }

        /// <summary>
        /// Get Percentage
        /// </summary>
        /// <param name="number">number</param>
        /// <param name="totalFeedback">totalFeedback</param>
        /// <returns>result</returns>
        public double GetPercentage(int number, int totalFeedback)
        {
            if (totalFeedback > 0)
            {
                double p = ((100 * (double)number) / (double)totalFeedback);
                return Math.Round(p, 2);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Calculate of data which is return from facebook
        /// </summary>
        /// <param name="facebookInsightsDatas">facebookInsightsDatas</param>
        /// <returns>FacebookReportOneCall</returns>
        public FacebookReportOneCall CalculateThisSlab(List<FacebookInsightsData> facebookInsightsDatas)
        {
            FacebookReportOneCall facebookReportOneCall = new FacebookReportOneCall();
            facebookReportOneCall.CountryImpression = new List<CountryImpression>();
            int pageReachTotal = 0;
            int pageClicksTotal = 0;
            int profileViewTotal = 0;
            int pageImpressionsTotal = 0;
            int organicReach = 0;
            int paidReach = 0;
            int lostLikes = 0;
            int newLikes = 0;
            for (var i = 0; i < facebookInsightsDatas.Count; i++)
            {
                var p = facebookInsightsDatas[i];
                if (p.name == "page_impressions_unique")
                {
                    var l = p.values;
                    pageReachTotal = p.values.Select(x => Int32.Parse(x.value.ToString())).Sum();
                    facebookReportOneCall.pageReachTotal = pageReachTotal;
                }
                if (p.name == "page_total_actions")
                {
                    pageClicksTotal = p.values.Select(x => Int32.Parse(x.value.ToString())).Sum();
                    facebookReportOneCall.pageClicksTotal = pageClicksTotal;
                }
                if (p.name == "page_views_total")
                {
                    profileViewTotal = p.values.Select(x => Int32.Parse(x.value.ToString())).Sum();
                    facebookReportOneCall.profileViewTotal = profileViewTotal;
                }
                if (p.name == "page_impressions")
                {
                    pageImpressionsTotal = p.values.Select(x => Int32.Parse(x.value.ToString())).Sum();
                    facebookReportOneCall.pageImpressionsTotal = pageImpressionsTotal;
                }
                //if (p.name == "page_views_external_referrals")
                //{
                //    var l2 = new List<dynamic> { p.values.ToList() };
                //    var listOfExternalReferrerList = new List<ExternalReferrerList>(); foreach (var data in l2[0])
                //    {
                //        foreach (var prop in data.value)
                //        {
                //            var name = prop.Name;
                //            var nameValue = prop.Value.Value; listOfExternalReferrerList.Add(new ExternalReferrerList { url = name, count = nameValue, percent = "0" });
                //        }
                //    }
                //}
                //if (p.name == "page_fans_by_unlike_source_unique")
                //{
                //    var l3 = new List<dynamic> { p.values.ToList() };
                //    var listOfPageUnlikeList = new List<ExternalReferrerList>(); foreach (var data in l3[0])
                //    {
                //        foreach (var prop in data.value)
                //        {
                //            var name = prop.Name;
                //            var nameValue = prop.Value.Value; listOfPageUnlikeList.Add(new ExternalReferrerList { url = name, count = nameValue, percent = "0" });
                //        }
                //    }
                //    facebookReportOneCall.ExternalReferrerList = listOfPageUnlikeList;
                //}
                //if (p.name == "page_impressions_organic_unique_v2")
                //{
                //    organicReach = p.values.Select(x => Int32.Parse(x.value.ToString())).Sum();
                //    facebookReportOneCall.organicReach = organicReach;
                //}
                if (p.name == "page_impressions_paid_unique")
                {
                    paidReach = p.values.Select(x => Int32.Parse(x.value.ToString())).Sum();
                    facebookReportOneCall.paidReach = paidReach;
                }
                if (p.name == "page_fan_removes_unique")
                {
                    lostLikes = p.values.Select(x => Int32.Parse(x.value.ToString())).Sum();
                    facebookReportOneCall.lostLikes = lostLikes;
                }
                //if (p.name == "page_fans_by_like_source_unique")
                //{
                //    var result = JsonConvert.SerializeObject(p.values);
                //    int paidLikes = 0;
                //    int newsFeedLikes = 0;
                //    int pageSuggestionsLikes = 0;
                //    int reactivatedLikes = 0;
                //    int searchLikes = 0;
                //    int pageLikes = 0;
                //    int otherLike = 0;


                //    if (result.Contains("Ads"))
                //    {
                //        var res = JsonConvert.DeserializeObject<List<LikeTypeDto>>(result);
                //        paidLikes = res.Select(x => x.value.Ads).Sum().Value;
                //    }
                //    if (result.Contains("News Feed"))
                //    {
                //        var res = JsonConvert.DeserializeObject<List<LikeTypeDto>>(result);
                //        newsFeedLikes = res.Select(x => x.value.NewsFeed).Sum().Value;
                //    }

                //    if (result.Contains("Page Suggestions"))
                //    {
                //        var res = JsonConvert.DeserializeObject<List<LikeTypeDto>>(result);
                //        pageSuggestionsLikes = res.Select(x => x.value.PageSuggestions).Sum().Value;
                //    }
                //    if (result.Contains("Restored Likes from Reactivated Accounts"))
                //    {
                //        var res = JsonConvert.DeserializeObject<List<LikeTypeDto>>(result);
                //        reactivatedLikes = res.Select(x => x.value.RestoredLikesFromReactivatedAccounts).Sum().Value;
                //    }
                //    if (result.Contains("Search"))
                //    {
                //        var res = JsonConvert.DeserializeObject<List<LikeTypeDto>>(result);
                //        searchLikes = res.Select(x => x.value.Search).Sum().Value;
                //    }
                //    if (result.Contains("Your Page"))
                //    {
                //        var res = JsonConvert.DeserializeObject<List<LikeTypeDto>>(result);
                //        pageLikes = res.Select(x => x.value.YourPage).Sum().Value;
                //    }

                //    if (result.Contains("Other"))
                //    {
                //        var res = JsonConvert.DeserializeObject<List<LikeTypeDto>>(result);
                //        otherLike = res.Select(x => x.value.Other).Sum().Value;
                //    }

                //    var organicLike = newsFeedLikes + pageSuggestionsLikes + reactivatedLikes + searchLikes + pageLikes + otherLike;

                //    facebookReportOneCall.LikeType = new LikesType { OrganicLike = organicLike, PaidLike = paidLikes };
                //}
                //if (p.name == "page_impressions_by_country_unique")
                //{
                //    facebookReportOneCall.CountryImpression = new List<CountryImpression>();
                //    var l4 = new List<dynamic> { p.values.ToList() };
                //    var listOfCountry = new List<CountryImpression>();
                //    foreach (var data in l4[0])
                //    {
                //        foreach (var prop in data.value)
                //        {
                //            var name = prop.Name;
                //            var nameValue = prop.Value.Value; listOfCountry.Add(new CountryImpression { country = name, count = nameValue, percent = "0" });
                //        }
                //    }

                //    var countryImpression = (from c in listOfCountry
                //                             group c by c.country into g
                //                             select new CountryImpression
                //                             {
                //                                 count = g.Sum(x => x.count),
                //                                 country = g.Key
                //                             }).ToList();

                //    facebookReportOneCall.CountryImpression = countryImpression;

                //}
                if (p.name == "page_fan_adds_unique")
                {
                    newLikes = p.values.Select(x => Int32.Parse(x.value.ToString())).Sum();
                    facebookReportOneCall.newLikes = newLikes;
                }

            }
            return facebookReportOneCall;
        }

        /// <summary>
        /// Grpah API access token is valid or not
        /// </summary>
        /// <param name="accessToken">accessToken</param>
        /// <returns>bool</returns>
        public async Task<GraphApiTokenValid> GetAccessTokenDetails(string accessToken)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://graph.facebook.com/")
            };

            var url = "debug_token?input_token=" + accessToken + "&access_token=" + _configuration.GetSection("FacebookAppId").Value + "|" + _configuration.GetSection("FacebookAppSecret").Value;

            var response = await httpClient.GetAsync(url);

            var data = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<GraphApiTokenValid>(data);

            return result;
        }

        public async Task<RootObjectFBData> GetFaceBookPageList(Guid campaignId)
        {
            var returnData = new RootObjectFBData();
            returnData.data = new List<FacebookList>();
         
                var campaign = _campaignfacebookRepository.GetAllEntities(true).Where(x => x.CampaignID == campaignId).FirstOrDefault();
                if (campaign != null)
                {
                    var isTokenValid = await GetAccessTokenDetails(campaign.AccessToken);

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
                            foreach(var obj in returnData.data)
                            {
                                obj.access_token = "";
                            }
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
                                                 permissionDto.permission == "read_insights" ||
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

        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// Calculate Previous StartDate And EndDate
        /// </summary>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <returns>PreviousDate</returns>
        private PreviousDate CalculatePreviousStartDateAndEndDate(DateTime startDate, DateTime endDate)
        {
            var previousDate = new PreviousDate();
            var diff = (endDate - startDate).TotalDays;

            previousDate.PreviousEndDate = startDate.AddDays(-1);

            previousDate.PreviousStartDate = previousDate.PreviousEndDate.AddDays(-diff);

            return previousDate;
        }

        /// <summary>
        /// Call FaceBook Api for insights
        /// </summary>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <param name="fbPage">fbPage</param>
        /// <returns>List<FacebookInsightsData></returns>
        private async Task<List<FacebookInsightsData>> CallFaceBookApi1(string startDate, string endDate, FbPage fbPage)
        {
            var url = fbPage.id + "/insights/page_impressions_unique,page_total_actions,page_views_total,page_impressions,page_fan_removes_unique,page_impressions_paid_unique,page_fan_adds_unique?access_token=" + fbPage.access_token + "&since=" + startDate + "&until=" + endDate + "&period=day";
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://graph.facebook.com/")
            }; 
            var response = await httpClient.GetAsync(url);
            var data = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<FacebookInsights>(data);
            return result.data;
        }

        /// <summary>
        /// Call FaceBook Api For NewLikes
        /// </summary>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <param name="fbPage">fbPage</param>
        /// <returns>Total new like</returns>
        private async Task<int> CallFaceBookApiForNewLikes(string startDate, string endDate, FbPage fbPage)
        {
            var url = fbPage.id + "?access_token=" + fbPage.access_token + "&fields=new_like_count&since=" + startDate + "&until=" + endDate + "&period=day"; var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://graph.facebook.com/")
            }; var response = await httpClient.GetAsync(url); var data = await response.Content.ReadAsStringAsync(); var result = JsonConvert.DeserializeObject<FacebookNewLike>(data); return result.new_like_count;
        }

        /// <summary>
        /// Call FaceBookApi For PageLike
        /// </summary>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <param name="fbPage">fbPage</param>
        /// <returns>Total page like</returns>
        private async Task<int> CallFaceBookApiForPageLike(string startDate, string endDate, FbPage fbPage)
        {
            var url = fbPage.id + "/insights/page_fans?access_token=" + fbPage.access_token ;
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://graph.facebook.com/")
            }; var response = await httpClient.GetAsync(url);
            var data = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<PageLiked>(data);
            if (result.data != null && result.data[0].values.Count > 0 && result.data[0].values[0].value > 0)
            {
                return result.data[0].values[0].value;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Calculate total difference of days
        /// </summary>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <returns>Status of the operation</returns>
        private double CalculateDateSlabDiff(DateTime startDate, DateTime endDate)
        {
            var difference = (endDate - startDate).TotalDays; return difference;
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

        /// <summary>
        /// Calculate average
        /// </summary>
        /// <param name="totalNumbers">totalNumbers</param>
        /// <param name="totaldays">totaldays</param>
        /// <returns>average</returns>
        private double GetAverage(int totalNumbers, double totaldays)
        {
            if (totaldays == 0)
            {
                return totalNumbers;
            }
            else
            {
                var p = totalNumbers / totaldays; return Math.Round(p, 2);
            }
        }

        /// <summary>
        /// Apply Negative 
        /// </summary>
        /// <param name="number">number</param>
        /// <returns>negative number</returns>
        private double ApplyNegative(double number)
        {
            return -Math.Abs(number);
        }

        /// <summary>
        /// Get Percentage Change
        /// </summary>
        /// <param name="oldNumber">oldNumber</param>
        /// <param name="newNumber">newNumber</param>
        /// <returns>Status of the operation</returns>
        private double GetPercentageChange(int oldNumber, double newNumber)
        {
            if (oldNumber == 0 && newNumber == 0)
            {
                return 0;
            }
            else
            if (oldNumber == 0 && newNumber > 0)
            {
                return ((newNumber) * 100);
            }
            else
            {
                var decreaseValue = Math.Abs(oldNumber - newNumber);
                return Math.Round(((decreaseValue / oldNumber) * 100), 2);
            }
        }

        /// <summary>
        /// Calculate data for facebook
        /// </summary>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <param name="getPageDetail">getPageDetail</param>
        /// <returns>Status of the operation</returns>
        private async Task<FacebookAllCallData> TestDateSlab(DateTime startDate, DateTime endDate, FbPage getPageDetail)
        {
            FacebookAllCallData facebookAllCallData = new FacebookAllCallData();
            List<List<FacebookInsightsData>> facebookInsightsDatas = new List<List<FacebookInsightsData>>();
            List<int> newLikes = new List<int>();
            List<int> pageLikes = new List<int>();
            var tempsdt = string.Empty;
            var tempedt = string.Empty;
            var ListOfDates = new List<FacebookReportDates>();
            var listOfMultipleFbCallData = new List<FacebookReportOneCall>();

            double d = 0;
            var tempDiff = 0;
            var diff = CalculateDateSlabDiff(startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

            if (diff > 92)
            {
                d = diff / 92;
                if (d > 0)
                {
                    d = Math.Round(d);

                    for (var i = 0; i <= d; i++)
                    {
                        if (i == 0)
                        {
                            tempsdt = startDate.AddDays(-1).ToString("yyyy-MM-dd");
                            tempedt = startDate.AddDays(92).ToString("yyyy-MM-dd");
                            tempDiff = (int)diff - 92;

                            var fbDates = new FacebookReportDates();
                            fbDates.startDate = tempsdt;
                            fbDates.endDate = tempedt;
                            ListOfDates.Add(fbDates);
                        }
                        else
                        {
                            tempsdt = Convert.ToDateTime(tempedt).ToString("yyyy-MM-dd");
                            if (tempDiff >= 92)
                            {
                                tempedt = Convert.ToDateTime(tempedt).AddDays(92).ToString("yyyy-MM-dd");
                                tempDiff = tempDiff - 92;
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
                fbDates.startDate = Convert.ToDateTime(startDate).AddDays(-1).ToString("yyyy-MM-dd");
                fbDates.endDate = Convert.ToDateTime(endDate).ToString("yyyy-MM-dd");
                ListOfDates.Add(fbDates);
            }

            for (var i = 0; i < ListOfDates.Count; i++)
            {
                List<FacebookInsightsData> all = new List<FacebookInsightsData>();
                int newLike = 0;
                int pageLike = 0;

                all = await CallFaceBookApi1(ListOfDates[i].startDate, ListOfDates[i].endDate, getPageDetail);
                newLike = await CallFaceBookApiForNewLikes(ListOfDates[i].startDate, ListOfDates[i].endDate, getPageDetail);
                pageLike = await CallFaceBookApiForPageLike(ListOfDates[i].startDate, ListOfDates[i].endDate, getPageDetail);

                if (all != null)
                {
                    facebookInsightsDatas.Add(all);
                }
                if (newLike > 0)
                {
                    newLikes.Add(newLike);
                }
                if (pageLike > 0)
                {
                    pageLikes.Add(pageLike);
                }
            }

            for (var i = 0; i < facebookInsightsDatas.Count; i++)
            {
                var facebookSingleCallData = CalculateThisSlab(facebookInsightsDatas[i]);
                listOfMultipleFbCallData.Add(facebookSingleCallData);
            }
            facebookAllCallData.FacebookReportMultipleCall = listOfMultipleFbCallData;
            facebookAllCallData.newLikes = newLikes;
            facebookAllCallData.pageLikes = pageLikes;

            return facebookAllCallData;
        }

        /// <summary>
        /// PrepareFacebookReport
        /// </summary>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <param name="previousDate">previousDate</param>
        /// <param name="fbPage">fbPage</param>
        /// <returns>Status of the operation</returns>
        private async Task<FacebookData> PrepareFacebookReport(DateTime startDate, DateTime endDate, PreviousDate previousDate, FbPage fbPage)
        {
            var facebookData = new FacebookData();
            List<List<FacebookInsightsData>> facebookInsightsDatas = new List<List<FacebookInsightsData>>();
            List<int> newLikes = new List<int>();
            List<int> pageLikes = new List<int>();

            int profileViewDiff = 0;
            int profileViewTotal = 0;
            int profileViewTotalPrev = 0;
            double percentProfileView = 0;
            int pageImpressionsTotal = 0;
            int pageImpressionsTotalPrev = 0;
            int pageImpressionsDiff = 0;
            double percentPageImpression = 0;
            int organicReach = 0;
            int paidReach = 0;
            int pageReachTotal = 0;
            int pageReachTotalPrev = 0;
            double percentPageReach = 0;
            long topCountForCity = 0;

            long totalimpressionByCountry = 0;

            try
            {
                var startDateUtc = ConvertStartDateToUTCFormat(startDate, "");
                var endDateUtc = ConvertEndDateToUTCFormat(endDate, "");

                var prevStartDate = ConvertStartDateToUTCFormat(previousDate.PreviousStartDate, "prev");
                var prevEndDate = ConvertEndDateToUTCFormat(previousDate.PreviousEndDate, "prev");

                var multipleCallData = await TestDateSlab(startDate, endDate, fbPage);

                var preMultipleCallData = await TestDateSlab(previousDate.PreviousStartDate, previousDate.PreviousEndDate, fbPage);

                //Page Impression
                pageImpressionsTotal = multipleCallData.FacebookReportMultipleCall.Select(x => x.pageImpressionsTotal).Sum();
                pageImpressionsTotalPrev = preMultipleCallData.FacebookReportMultipleCall.Select(x => x.pageImpressionsTotal).Sum();
                facebookData.PageImpressionsTotal = pageImpressionsTotal;

                var totalDays = CalculateDateSlabDiff(startDate, endDate);

                facebookData.AvgPageImpression = GetAverage(pageImpressionsTotal, totalDays);

                pageImpressionsDiff = pageImpressionsTotalPrev - pageImpressionsTotal;
                percentPageImpression = GetPercentageChange(pageImpressionsTotalPrev, pageImpressionsTotal);
                if (pageImpressionsDiff < 0 && pageImpressionsTotalPrev > pageImpressionsTotal)
                {
                    percentPageImpression = ApplyNegative(percentPageImpression);
                }

                facebookData.PercentPageImpression = percentPageImpression;

                //Page Reach
                pageReachTotal = multipleCallData.FacebookReportMultipleCall.Select(x => x.pageReachTotal).Sum();
                pageReachTotalPrev = preMultipleCallData.FacebookReportMultipleCall.Select(x => x.pageReachTotal).Sum();
                facebookData.PageReachTotal = pageReachTotal;

                facebookData.AvgPageReach = GetAverage(pageReachTotal, totalDays);

                var pageReachTotalDiff = pageReachTotal - pageReachTotal;
                percentPageReach = GetPercentageChange(pageReachTotalPrev, pageReachTotal);
                if (pageReachTotalDiff < 0 && pageReachTotalPrev > pageReachTotal)
                {
                    percentPageReach = ApplyNegative(percentPageReach);
                }

                facebookData.PercentPageReach = percentPageReach;

                //Organic
                organicReach = multipleCallData.FacebookReportMultipleCall.Select(x => x.organicReach).Sum();
                facebookData.OrganicReach = organicReach;

                //Paid
                paidReach = multipleCallData.FacebookReportMultipleCall.Select(x => x.paidReach).Sum();
                facebookData.PaidReach = paidReach;

                var paidOrganicTotal = paidReach + organicReach;

                facebookData.PercentOrganicReach = GetPercentage(organicReach, paidOrganicTotal);
                facebookData.PercentPaidReach = GetPercentage(paidReach, paidOrganicTotal);



                //Page Profile View

                profileViewTotal = multipleCallData.FacebookReportMultipleCall.Select(x => x.profileViewTotal).Sum();
                profileViewTotalPrev = preMultipleCallData.FacebookReportMultipleCall.Select(x => x.profileViewTotal).Sum();
                facebookData.ProfileViewTotal = profileViewTotal;

                profileViewDiff = profileViewTotal - profileViewTotalPrev;
                facebookData.PercentProfileView = GetPercentageChange(profileViewTotalPrev, profileViewTotal);
                if (profileViewDiff < 0)
                {
                    percentProfileView = ApplyNegative(percentProfileView);
                }

                facebookData.AvgPageProfileView = GetAverage(profileViewTotal, totalDays);

                //Total New Like
                //var totalNewLike = multipleCallData.newLikes[0];
                //facebookData.TotalNewLike = totalNewLike;

                //var avgPerDayLike = GetAverage(totalNewLike, totalDays);
                //facebookData.AvgPerDayLike = avgPerDayLike;

                //Total Page Like
                if (multipleCallData.pageLikes.Count > 0)
                {
                    var totalPageLike = multipleCallData.pageLikes[0];
                    facebookData.TotalPageLike = totalPageLike;
                }

                var CountryImpressions = multipleCallData.FacebookReportMultipleCall.Select(y => y.CountryImpression).ToList();

                var mergeMultipleCallData = new List<CountryImpression>();
                var percentOfCount = new List<double>();
                var label = new List<string>();

                foreach (var countryImp in CountryImpressions)
                {
                    foreach (var value in countryImp)
                    {
                        mergeMultipleCallData.Add(value);
                    }
                }

                var countryImpression = (from c in mergeMultipleCallData
                                         group c by c.country into g
                                         //where g.Count() > 1

                                         select new CountryImpression
                                         {
                                             count = g.Sum(x => x.count),
                                             country = g.Key
                                         }).ToList();

                totalimpressionByCountry = countryImpression.Select(x => x.count).Sum();

                countryImpression = countryImpression.OrderByDescending(x => x.count).Take(5).ToList();

                if (countryImpression.Count > 0)
                {
                    topCountForCity = countryImpression.Select(x => x.count).FirstOrDefault();
                }

                facebookData.TopCountForCity = topCountForCity;


                foreach (var value in countryImpression)
                {
                    label.Add(value.country);
                    double percent = GetPercentage((int)value.count, (int)totalimpressionByCountry);
                    percentOfCount.Add(Math.Round(percent, 2));
                }

                facebookData.CountryDataStr = percentOfCount;
                facebookData.CountryLabelStr = label;

                //var paidLike = multipleCallData.FacebookReportMultipleCall.Select(x => x.LikeType).Sum(x => x.PaidLike);

                //var oranicLike = multipleCallData.FacebookReportMultipleCall.Select(x => x.LikeType).Sum(x => x.OrganicLike);

                //New Like
                //var totalNewLike = multipleCallData.FacebookReportMultipleCall.Select(x => x.newLikes).Sum();
                //facebookData.TotalNewLike = totalNewLike;

                //var avgPerDayLike = GetAverage(totalNewLike, totalDays);
                //facebookData.AvgPerDayLike = avgPerDayLike;

                //var percentagePaidLike = GetPercentage(paidLike, paidLike + oranicLike);

                //var percentageOrganicLike = GetPercentage(oranicLike, paidLike + oranicLike);

                //facebookData.PercentOrganicLike = percentageOrganicLike;
                //facebookData.PercentPaidLike = percentagePaidLike;
            }
            catch (Exception e)
            {
                throw e;
            }

            return facebookData;
        }

        /// <summary>
        /// Get Page Detail
        /// </summary>
        /// <param name="accessToken">accessToken</param>
        /// <param name="facebookSetup">facebookSetup</param>
        /// <returns>Status of the operation</returns>
        private async Task<FbPage> GetPageDetail(string accessToken, CampaignFacebook facebookSetup)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://graph.facebook.com/")
            };

            var url = "me/accounts?limit=1000&access_token=" + accessToken;
            var response = await httpClient.GetAsync(url);
            var data = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<FbPageList>(data);
            var setupPage = result.data.Where(y => y.name == facebookSetup.UrlOrName).FirstOrDefault();
            return setupPage;
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
            return "Id, UrlOrName, CampaignID, IsActive, Campaign, AccessToken, PageToken";
        }

        #endregion
    }
}
