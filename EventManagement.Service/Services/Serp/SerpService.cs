using Amazon.Lambda;
using AutoMapper;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using EventManagement.Dto;
using Google.Api.Ads.AdWords.v201809;
using Google.Api.Gax.ResourceNames;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Campaign = EventManagement.Domain.Entities.Campaign;

namespace EventManagement.Service
{
    public class SerpService : ServiceBase<Serp, Guid>, ISerpService
    {

        #region PRIVATE MEMBERS

        private readonly ISerpRepository _serpRepository;
        private readonly IConfiguration _configuration;
        private readonly ICampaignService _campaignService;
        #endregion


        #region CONSTRUCTOR

        public SerpService(ISerpRepository serpRepository, ILogger<SerpService> logger, IConfiguration configuration, ICampaignService campaignService) : base(serpRepository, logger)
        {
            _serpRepository = serpRepository;
            _configuration = configuration;
            _campaignService = campaignService;
        }

        #endregion


        #region PUBLIC MEMBERS   
        public async Task<string> UpdateKeywordsStatus()
        {
            var campaignId = new Guid();
            var webUrl = string.Empty;
            var businessName = string.Empty;

            string res = "UpdateKeywordsStatus Lambda has been executed successfully...";

            try
            {
                bool resp = false;           
                var campaignRepo = await _campaignService.GetAllEntitiesAsync();

                var currentDateTime = DateTime.UtcNow.AddDays(-2);

                campaignRepo = campaignRepo.Where(x => x.LastUpdateSerpDate < currentDateTime).ToList();

                if (campaignRepo.Count > 0)
                {
                    for (int i = 0; i < campaignRepo.Count; i++)
                    {
                        campaignId = campaignRepo[i].Id;

                        webUrl = campaignRepo[i].WebUrl;

                        var serpsFromRepo = _serpRepository.GetAllEntities().Where(x => x.CampaignID == campaignRepo[i].Id).Select(y => new SerpDto
                        {
                            Id = y.Id,
                            CampaignID = y.CampaignID.ToString(),
                            Position = y.Position,
                            Searches = y.Searches,
                            Location = y.Location,
                            Keywords = y.Keywords,
                            UpdatedOn = y.UpdatedOn,
                            LocationName = y.LocationName,
                            LocalPackCount = y.LocalPackCount,
                            BusinessName = y.BusinessName,
                            LocalPacksStatus = y.LocalPacksStatus
                        }).ToList();

                        //var serpsFromRepo = Mapper.Map<List<SerpDto>>(serps);

                        if (serpsFromRepo.Count > 0)
                        {
                            serpsFromRepo = serpsFromRepo.Distinct(new SerpKeywordEqualityComparer()).ToList();

                            int keywordsPerRequest = 100;

                            while (serpsFromRepo.Any())
                            {
                                var serpList = new List<SerpDto>();

                                if (serpsFromRepo.Count >= keywordsPerRequest)
                                {
                                    //100 Conversations in batches
                                    serpList = serpsFromRepo.Take(keywordsPerRequest).ToList();
                                }
                                else
                                {
                                    //no. of Conversations in batches if less than 100
                                    serpList = serpsFromRepo.Take(serpsFromRepo.Count()).ToList();
                                }

                                var allRequest = new List<SerpPost>();
                                var createDto = new List<Serp>();

                                for (int k = 0; k < serpList.Count; k++)
                                {
                                    var perRequest = new SerpPost();

                                    serpList[k].BusinessName = String.IsNullOrEmpty(serpList[k].BusinessName) ? " " : serpList[k].BusinessName;
                                    webUrl = String.IsNullOrEmpty(webUrl) ? " " : webUrl;

                                    perRequest.Keywords = serpList[k].Keywords;
                                    perRequest.LocationName = serpList[k].LocationName;
                                    perRequest.Tag = HttpUtility.UrlEncode(serpList[k].BusinessName + "***" + webUrl);

                                    //prepare dto for create record

                                    Serp serpForCreation = new Serp();
                                    serpForCreation.IsWebhookRecieved = false;
                                    serpForCreation.Position = 0;
                                    serpForCreation.LocalPackCount = 0;
                                    serpForCreation.Searches = 0;

                                    serpForCreation.CampaignID = campaignId;
                                    serpForCreation.Location = serpList[k].Location;
                                    serpForCreation.LocationName = serpList[k].LocationName;
                                    serpForCreation.Keywords = serpList[k].Keywords;
                                    serpForCreation.BusinessName = serpList[k].BusinessName;
                                    serpForCreation.CreatedOn = DateTime.UtcNow;
                                    serpForCreation.LocalPacksStatus = serpList[k].LocalPacksStatus;
                                    serpForCreation.CreatedBy = "lambda";

                                    createDto.Add(serpForCreation);

                                    allRequest.Add(perRequest);
                                }

                                var httpClient = GetDataForSeoClient();
                                var allPostData = new StringContent(JsonConvert.SerializeObject(allRequest), Encoding.UTF8, "application/json");
                                var taskPostResponse = await httpClient.PostAsync("/v3/serp/google/organic/task_post", allPostData);
                                var SerpRes = JsonConvert.DeserializeObject<SerpRes>(await taskPostResponse.Content.ReadAsStringAsync());

                                //find and set task id by keywords
                                for (int j = 0; j < createDto.Count; j++)
                                {
                                    createDto[j].TaskId = SerpRes.SerpTasks.Where(y => y.Data.Keyword.Equals(createDto[j].Keywords) && y.Data.Location.Equals(createDto[j].LocationName)).Select(p => p.Id).FirstOrDefault();
                                }

                                _serpRepository.CreateBulkEntity(createDto);
                                _serpRepository.SaveChanges();

                                if (serpsFromRepo.Count >= keywordsPerRequest)
                                {
                                    //100 serps records will remove from the list after successfully created
                                    serpsFromRepo.RemoveRange(0, keywordsPerRequest);
                                }
                                else
                                {
                                    //if... less than 100 keyword will remove from the list after successfully deleted
                                    serpsFromRepo.RemoveRange(0, serpsFromRepo.Count());
                                }
                            }
                        }

                        await _campaignService.UpdateBulkEntityAsync(y => new Campaign { LastUpdateSerpDate = DateTime.UtcNow }, x => x.Id == campaignId);
                    }
                }
            }
            catch (Exception ex)
            {
                res = "StackTrace Error in UpdateKeywordsStatus-- " + ex.StackTrace
                    + "Error messasge: " + ex.Message +
                    "CampaignId " + campaignId;
            }

            return res;
        }




        public class SerpKeywordEqualityComparer : IEqualityComparer<SerpDto>

        {


            public bool Equals(SerpDto x, SerpDto y)
            {
                // Two items are equal if their keys are equal.
                return x.Keywords == y.Keywords && x.LocationName == y.LocationName;
            }



            public int GetHashCode(SerpDto obj)
            {
                return obj.Keywords.GetHashCode();
            }

        }

        //public async Task<List<SerpDto>> GetSerpMostRecentData(DateTime fromdate, DateTime todate, Guid campaignid) {
        //	var serpsFromRepo = _serpRepository.GetAllEntities(true).Where(x => x.CampaignID == campaignid).Select(y => new SerpDto {
        //		Id = y.Id,
        //		CampaignID = y.CampaignID.ToString(),
        //		Position = y.Position,
        //		Searches = y.Searches,
        //		Location = y.Location,
        //		Keywords = y.Keywords,
        //		UpdatedOn = y.UpdatedOn,
        //	}
        //		).ToList();
        //	if (serpsFromRepo.Count > 0) {
        //		serpsFromRepo = serpsFromRepo.Where(x => x.UpdatedOn.Date <= todate.Date && x.UpdatedOn.Date >= fromdate.Date).ToList();
        //	}
        //	if (serpsFromRepo.Count > 0) {
        //		var maxValue = serpsFromRepo.OrderByDescending(x => x.UpdatedOn).First();
        //		serpsFromRepo = serpsFromRepo.Where(x => x.UpdatedOn.Date == maxValue.UpdatedOn.Date).ToList();
        //	}
        //	return serpsFromRepo;
        //}
        public async Task<JArray> GetSerpLocationData(string location)
        {
            HttpResponseMessage resp = new HttpResponseMessage();
            string responseBody = "";
            //try {
            var login = _configuration["DataForSeoLoginV3"];
            var pass = _configuration["DataForSeoPasswordV3"];

            var httpClient = GetDataForSeoClient();
            resp = httpClient.GetAsync("/v3/serp/google/locations/" + location).Result;
            JObject json = JObject.Parse(JObject.Parse(JsonConvert.SerializeObject(resp.Content.ReadAsStringAsync())).SelectToken("Result").ToString());
            JArray result = (JArray)json["tasks"][0]["result"];

            //} catch (Exception ex) {
            //	return categories;
            //}

            return result;
        }

        public async Task UpdateSerpData(SerpDetails serpDetails)
        {
            var getData = _serpRepository.GetFilteredEntities().Where(x => x.TaskId == serpDetails.TaskId).FirstOrDefault();

            try
            {
                if (getData != null)
                {
                    getData.Position = serpDetails.Position;
                    getData.LocalPackCount = serpDetails.LocalPacksCount;
                    getData.Searches = serpDetails.Searchs;
                    getData.IsWebhookRecieved = true;
                    getData.UpdatedOn = DateTime.UtcNow;

                    _serpRepository.UpdateEntity(getData);
                    _serpRepository.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                await UpdateBulkEntityAsync(x => new Serp
                {
                    LambdaLogger = "StackTrace Error in UpdateKeywordsStatus-- " + ex.StackTrace
                 + "Error messasge: " + ex.Message,
                    UpdatedOn = DateTime.UtcNow
                }, y => y.TaskId == serpDetails.TaskId);
            }
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="campaignID"> campaignID</param>
        /// <param name="location"> location code </param>
        /// <param name="locationname"> locationname </param>
        /// <param name="listKeyword"> list of keywords</param>
        /// <returns> objKeyword </returns>
        public async Task<ExistNotExistKeyword> GetExistNotExistKeyword(string campaignID, string location, string locationname, List<string> listKeyword)
        {
            ExistNotExistKeyword objKeyword = new ExistNotExistKeyword();
            objKeyword.ExistKeywords = new List<string>();
            objKeyword.NotExistKeywords = new List<string>();

            for (int i = 0; i < listKeyword.Count; i++)
            {
                var exist = await _serpRepository.ExistAsync(x => x.Keywords == listKeyword[i] && x.LocationName == locationname && x.CampaignID == new Guid(campaignID));
                if (exist == true)
                {
                    objKeyword.ExistKeywords.Add(listKeyword[i]);
                }
                else
                {
                    objKeyword.NotExistKeywords.Add(listKeyword[i]);
                }
            }
            return objKeyword;
        }

        public int GetKeywordsCount(string campaignID)
        {

            var latestKeywordListData = _serpRepository.GetAllEntities(true).Where(x => x.CampaignID == new Guid(campaignID)).Select(y => new SerpDto
            {
                Id = y.Id,
                Keywords = y.Keywords,
                LocationName = y.LocationName

            }).ToList();

            var count = latestKeywordListData.Distinct(new KeywordEqualityComparer()).Count();

            return count;
        }

        public int GetCompanyKeywordsCount(Guid companyId)
        {

            var campaignIds = _campaignService.GetAllEntities().Where(x => x.CompanyID == companyId).Select(x => x.Id).ToList();

            if (campaignIds.Count > 0)
            {
                var latestKeywordListData = _serpRepository.GetAllEntities(true).Where(x => campaignIds.Contains(x.CampaignID)).Select(y => new SerpDto
                {
                    Id = y.Id,
                    Keywords = y.Keywords,
                    LocationName = y.LocationName,

                }).ToList();

                var count = latestKeywordListData.Distinct(new KeywordEqualityComparer()).Count();
                return count;
            }
            else
            {
                return 0;
            }                        
        }


        public List<SerpDto> GetUniqueKeywordByProject(string campaignID)
        {
            var latestKeywordListData = _serpRepository.GetAllEntities(true).Where(x => x.CampaignID == new Guid(campaignID)).Select(y => new SerpDto
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
            var latestKeywordList = latestKeywordListData.Distinct(new KeywordEqualityComparer()).ToList();
            return latestKeywordList;
        }

        /// <summary>
        /// Prepare Avg Ranking For Dashboard
        /// </summary>
        /// <param name="campaignID">campaignID</param>
        /// <param name="fromDate">fromDate</param>
        /// <param name="toDate">toDate</param>
        /// <param name="previousDate">previousDate</param>
        /// <returns>current and previous ranking position</returns>
        public List<long> PrepareAvgRanking(Guid campaignID, string fromDate, string toDate, PreviousDate previousDate)
        {
            decimal avgRankingPosition;
            decimal avgPreviousRankingPosition;
            var preFromDate = previousDate.PreviousStartDate.ToString("yyyy-MM-dd");
            var preToDate = previousDate.PreviousEndDate.ToString("yyyy-MM-dd");


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

            var avgRankings = new List<long>() { (long)avgRankingPosition, (long)avgPreviousRankingPosition };

            return avgRankings;
        }

        /// <summary>
        /// GetSerpData
        /// </summary>
        /// <param name="campaignID">campaignID</param>
        /// <param name="location">location</param>
        /// <param name="keywords">keywords</param>
        /// <param name="tag">tag</param>
        /// <returns>true or false</returns>
        public async Task<bool> GetSerpData(string campaignID, string location, string locationname, string keyword, string businessName, bool localPacksStatus, string searchParam)
        {
            var keywordCall = 0;
            var keywordCopy = string.Empty;

            var id = new Guid(campaignID);
            var campaign = await _campaignService.GetEntityByIdAsync(id);

            try
            {
                if (campaign != null && !string.IsNullOrEmpty(campaign.WebUrl) && campaign.WebUrl.Contains("https://"))
                {
                    campaign.WebUrl = campaign.WebUrl.Replace("https://", "");
                }

                if (campaign != null && !string.IsNullOrEmpty(campaign.WebUrl) && campaign.WebUrl.Contains("http://"))
                {
                    campaign.WebUrl = campaign.WebUrl.Replace("http://", "");
                }

                if (campaign != null && !string.IsNullOrEmpty(campaign.WebUrl) && campaign.WebUrl.Contains("/"))
                {
                    campaign.WebUrl = campaign.WebUrl.Replace("/", "");
                }

                if (campaign != null && !string.IsNullOrEmpty(campaign.WebUrl) && campaign.WebUrl.Contains("www."))
                {
                    campaign.WebUrl = campaign.WebUrl.Replace("www.", "");
                }


                try
                {
                    keywordCopy = keyword;
                    keywordCall++;
                    var postData = new List<object>();
                    postData.Add(new
                    {
                        language_code = "en",
                        location_name = locationname,
                        keyword
                    });
                    // POST /v3/serp/google/organic/live/regular
                    // in addition to 'google' and 'organic' you can also set other search engine and type parameters
                    // the full list of possible parameters is available in documentation
                    var httpClient = GetDataForSeoClient();
                    var taskPostResponse = await httpClient.PostAsync("/v3/serp/google/organic/live/advanced", new StringContent(JsonConvert.SerializeObject(postData)));
                    var SerpRes = JsonConvert.DeserializeObject<SerpRes>(await taskPostResponse.Content.ReadAsStringAsync());

                    if (SerpRes.StatusCode == 20000 && SerpRes.SerpTasks[0].StatusCode == 20000)
                    {
                        SerpForCreation serpForCreation = new SerpForCreation();
                        serpForCreation.Position = (long)SerpRes.SerpTasks[0].Result[0].Items.Where(y => (y.Type == "organic" || y.Type == "paid") && y.Url.AbsoluteUri.Contains(campaign.WebUrl)).Select(x => x.RankAbsolute).FirstOrDefault();

                        //If localpacks is not include
                        if (businessName != null)
                        {
                            serpForCreation.LocalPackCount = (long)SerpRes.SerpTasks[0].Result[0].Items.Where(y => y.Type == "local_pack" && y.Title.ToLower() == businessName.Trim().ToLower()).Select(x => x.RankGroup).FirstOrDefault();
                        }
                        serpForCreation.Searches = SerpRes.SerpTasks[0].Result[0].Searches;

                        serpForCreation.CampaignID = id;
                        serpForCreation.Keywords = keyword;
                        serpForCreation.Location = location;
                        serpForCreation.LocationName = locationname;
                        serpForCreation.BusinessName = businessName;
                        serpForCreation.LocalPacksStatus = localPacksStatus;

                        await CreateEntityAsync<SerpDto, SerpForCreation>(serpForCreation);

                        // you can find the full list of the response codes here https://docs.dataforseo.com/v3/appendix/errors
                    }
                    else
                    {
                        SerpForCreation serpForCreation = new SerpForCreation();
                        serpForCreation.Position = 0;
                        serpForCreation.LocalPackCount = 0;

                        serpForCreation.Searches = 0;
                        serpForCreation.CampaignID = id;
                        serpForCreation.Keywords = keyword;
                        serpForCreation.Location = location;
                        serpForCreation.LocationName = locationname;
                        serpForCreation.BusinessName = businessName;
                        serpForCreation.LocalPacksStatus = localPacksStatus;
                        serpForCreation.LambdaLogger = "Error status code: " + SerpRes.StatusCode;
                        await CreateEntityAsync<SerpDto, SerpForCreation>(serpForCreation);

                    }

                }
                catch (Exception ex)
                {
                    SerpForCreation serpForCreation = new SerpForCreation();
                    serpForCreation.Position = 0;
                    serpForCreation.LocalPackCount = 0;
                    serpForCreation.Searches = 0;
                    serpForCreation.CampaignID = id;
                    serpForCreation.Keywords = keywordCopy;
                    serpForCreation.Location = location;
                    serpForCreation.LocationName = locationname;
                    serpForCreation.LambdaLogger = "Error : " + JsonConvert.SerializeObject(ex);
                    await CreateEntityAsync<SerpDto, SerpForCreation>(serpForCreation);

                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }


        public HttpClient GetDataForSeoClient()
        {
            var login = _configuration["DataForSeoLoginV3"];
            var pass = _configuration["DataForSeoPasswordV3"];

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.dataforseo.com/"),
                // Instead of 'login' and 'password' use your credentials from https://app.dataforseo.com/api-dashboard
                DefaultRequestHeaders = { Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(login + ":" + pass))) }
            };

            return httpClient;
        }
        #endregion


        #region OVERRIDDEN IMPLEMENTATION

        public override Dictionary<string, PropertyMappingValue> GetPropertyMapping()
        {
            return new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "Id", new PropertyMappingValue(new List<string>() { "Id" } ) },
                        { "CampaignID", new PropertyMappingValue(new List<string>() { "CampaignID" } ) },
                        { "CreatedOn", new PropertyMappingValue(new List<string>() { "CreatedOn" } ) },
                        { "Position", new PropertyMappingValue(new List<string>() { "Position" } ) },
                        { "Searches", new PropertyMappingValue(new List<string>() { "Searches" } ) },
                        { "Location", new PropertyMappingValue(new List<string>() { "Location" } ) },
                        { "Keywords", new PropertyMappingValue(new List<string>() { "Keywords" } ) },
                        { "LocationName", new PropertyMappingValue(new List<string>() { "LocationName" } ) },
                         { "LocalPackCount", new PropertyMappingValue(new List<string>() { "LocalPackCount" } ) }

                    };
        }


        public override string GetDefaultFieldsToSelect()
        {
            return "Id,CampaignID,CreatedOn,Position,Searches,Location,Keywords,LocationName,UpdatedOn,LocalPackCount,LocalPacksStatus";
        }

        #endregion
    }
}
