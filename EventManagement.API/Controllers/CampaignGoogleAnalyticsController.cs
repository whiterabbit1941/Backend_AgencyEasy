using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
//using EventManagement.API.Helpers;
using EventManagement.Dto;
using EventManagement.Service;
using EventManagement.Domain.Entities;
using EventManagement.Utility;
using Microsoft.Extensions.Logging;
using IdentityServer4.AccessTokenValidation;
using Microsoft.Extensions.Configuration;
using EventManagement.Utility.Enums;
using System.Linq;
using Microsoft.VisualBasic;

namespace EventManagement.API.Controllers
{
    /// <summary>
    /// CampaignGoogleAnalytics endpoint
    /// </summary>
    [Route("api/campaigngoogleanalyticss")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]

    public class CampaignGoogleAnalyticsController : Controller
    {

        #region PRIVATE MEMBERS
        private readonly IConfiguration _configuration;
        private readonly ICampaignGoogleAnalyticsService _campaigngoogleanalyticsService;
        private readonly ICampaignGSCService _campaigngscService;
        private readonly ICampaignGoogleAdsService _campaignGoogleAdsService;
        private readonly ICampaignService _campaignService;
        private readonly ICampaignGBPService _campaignGBPService;
        private readonly ICampaignGoogleSheetService _campaignGoogleSheetService;


        private ILogger<CampaignGoogleAnalyticsController> _logger;
        private readonly IUrlHelper _urlHelper;
        static string _companyId;
        static string _baseUrl;
        static string _accessToken;
        static string _campaignId;
        static string _type;
        #endregion


        #region CONSTRUCTOR

        public CampaignGoogleAnalyticsController(ICampaignGoogleAdsService campaignGoogleAdsService,
            ICampaignGSCService campaigngscService, ICampaignService campaignService, 
            IConfiguration configuration, ICampaignGoogleAnalyticsService campaigngoogleanalyticsService,
            ILogger<CampaignGoogleAnalyticsController> logger, IUrlHelper urlHelper,
            ICampaignGBPService campaignGBPService, ICampaignGoogleSheetService campaignGoogleSheetService)
        {
            _logger = logger;
            _campaigngoogleanalyticsService = campaigngoogleanalyticsService;
            _urlHelper = urlHelper;
            _configuration = configuration;
            _campaignService = campaignService;
            _campaigngscService = campaigngscService;
            _campaignGoogleAdsService = campaignGoogleAdsService;
            _campaignGBPService = campaignGBPService;
            _campaignGoogleSheetService = campaignGoogleSheetService;
        }

        #endregion


        #region HTTPGET

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("GetCampaignGa4DataById", Name = "GetCampaignGa4DataById")]
        public async Task<ActionResult<Ga4Details>> GetCampaignGa4DataById(Guid campaignId, string startDate, string endDate)
        {
            var ga4Data = await _campaigngoogleanalyticsService.GetCampaignGa4DataById(campaignId, startDate, endDate);

            return Ok(ga4Data);
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("GetCampaignGaDataById", Name = "GetCampaignGaDataById")]
        public async Task<ActionResult<GoogleAnalyticsResponseDto>> GetCampaignGaDataById(Guid campaignId, string startDate, string endDate)
        {
            var gaData = await _campaigngoogleanalyticsService.GetCampaignGaDataById(campaignId, startDate, endDate);

            return Ok(gaData);
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("GetLighthouseDataByStrategy", Name = "GetLighthouseDataByStrategy")]
        public async Task<ActionResult<PageSpeedLightHouseDto>> GetLighthouseDataByStrategy(Guid campaignId)
        {
            var returnData = new PageSpeedLightHouseDto();

            var desktop = await _campaigngoogleanalyticsService.GetLighthouseDataByStrategy(campaignId, "DESKTOP");
            var mobile = await _campaigngoogleanalyticsService.GetLighthouseDataByStrategy(campaignId, "MOBILE");

            returnData.Desktop = desktop;
            returnData.Mobile = mobile;

            return Ok(returnData);
        }

        [HttpGet(Name = "GetFilteredCampaignGoogleAnalyticss")]
        [Produces("application/vnd.tourmanagement.campaigngoogleanalyticss.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CampaignGoogleAnalyticsDto>>> GetFilteredCampaignGoogleAnalyticss([FromQuery] FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_campaigngoogleanalyticsService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<CampaignGoogleAnalyticsDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var campaigngoogleanalyticssFromRepo = await _campaigngoogleanalyticsService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.campaigngoogleanalyticss.hateoas+json")
            {
                //create HATEOAS links for each show.
                campaigngoogleanalyticssFromRepo.ForEach(campaigngoogleanalytics =>
                {
                    var entityLinks = CreateLinksForCampaignGoogleAnalytics(campaigngoogleanalytics.Id, filterOptionsModel.Fields);
                    campaigngoogleanalytics.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = campaigngoogleanalyticssFromRepo.TotalCount,
                    pageSize = campaigngoogleanalyticssFromRepo.PageSize,
                    currentPage = campaigngoogleanalyticssFromRepo.CurrentPage,
                    totalPages = campaigngoogleanalyticssFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForCampaignGoogleAnalyticss(filterOptionsModel, campaigngoogleanalyticssFromRepo.HasNext, campaigngoogleanalyticssFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = campaigngoogleanalyticssFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = campaigngoogleanalyticssFromRepo.HasPrevious ?
                    CreateCampaignGoogleAnalyticssResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = campaigngoogleanalyticssFromRepo.HasNext ?
                    CreateCampaignGoogleAnalyticssResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = campaigngoogleanalyticssFromRepo.TotalCount,
                    pageSize = campaigngoogleanalyticssFromRepo.PageSize,
                    currentPage = campaigngoogleanalyticssFromRepo.CurrentPage,
                    totalPages = campaigngoogleanalyticssFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(campaigngoogleanalyticssFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.campaigngoogleanalyticss.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetCampaignGoogleAnalytics")]
        public async Task<ActionResult<CampaignGoogleAnalytics>> GetCampaignGoogleAnalytics(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object campaigngoogleanalyticsEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetCampaignGoogleAnalytics called");

                //then get the whole entity and map it to the Dto.
                campaigngoogleanalyticsEntity = Mapper.Map<CampaignGoogleAnalyticsDto>(await _campaigngoogleanalyticsService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                campaigngoogleanalyticsEntity = await _campaigngoogleanalyticsService.GetPartialEntityAsync(id, fields);
            }

            //if campaigngoogleanalytics not found.
            if (campaigngoogleanalyticsEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.campaigngoogleanalyticss.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForCampaignGoogleAnalytics(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((CampaignGoogleAnalyticsDto)campaigngoogleanalyticsEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = campaigngoogleanalyticsEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = campaigngoogleanalyticsEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }

        //[ProducesResponseType(StatusCodes.Status201Created)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("GoogleAnalyticsSetup", Name = "GoogleAnalyticsSetup")]
        public string GoogleAnalyticsSetup([FromQuery] string type, [FromQuery] string companyId, [FromQuery] string source, [FromQuery] string campaignId, [FromQuery] string baseUrl)
        {
            _companyId = companyId;
            _campaignId = campaignId;
            _type = type;
            _baseUrl = baseUrl;
            string url = _campaigngoogleanalyticsService.GoogleAnalyticsSetup(type, source);

            return url;
        }

        [AllowAnonymous]
        [HttpGet("GoogleExchangeCode", Name = "GoogleExchangeCode")]
        public async Task<IActionResult> GoogleExchangeCode([FromQuery] string code)
        {
            var redirectUrl = string.Empty;
            bool forbidden = false; 
            if (!string.IsNullOrEmpty(code))
            {
                GaToken res = await _campaigngoogleanalyticsService.GetAccessTokenUsingCode(code);

                if (_type == "GA")
                {
                    CampaignGoogleAnalyticsForCreation storeGADetail = new CampaignGoogleAnalyticsForCreation();
                    storeGADetail.UrlOrName = "";
                    storeGADetail.AccessToken = res.access_token;
                    storeGADetail.RefreshToken = res.refresh_token;
                    storeGADetail.IsActive = true;
                    storeGADetail.CampaignID = Guid.Parse(_campaignId);
                    storeGADetail.IsGa4 = false;
                    var response = await _campaigngoogleanalyticsService.CreateEntityAsync<CampaignGoogleAnalyticsDto, CampaignGoogleAnalyticsForCreation>(storeGADetail);
                    await _campaigngoogleanalyticsService.SaveChangesAsync();
                }
                else if (_type == "GA4")
                {
                    string[] targetScopes = { "https://www.googleapis.com/auth/analytics.readonly", "https://www.googleapis.com/auth/analytics", "openid", "https://www.googleapis.com/auth/userinfo.email" };

                    string[] grantedScopes = res.scope.Split(' ');

                    bool areAllScopesGranted = targetScopes.All(targetScope => grantedScopes.Contains(targetScope));

                    //var result = await _campaigngoogleanalyticsService.IsPropertiesExists(res.access_token);

                    if (areAllScopesGranted)
                    {
                        CampaignGoogleAnalyticsForCreation storeGADetail = new CampaignGoogleAnalyticsForCreation();
                        storeGADetail.UrlOrName = "";
                        storeGADetail.AccessToken = res.access_token;
                        storeGADetail.RefreshToken = res.refresh_token;
                        storeGADetail.IsActive = true;
                        storeGADetail.CampaignID = Guid.Parse(_campaignId);
                        storeGADetail.IsGa4 = true;
                        var response = await _campaigngoogleanalyticsService.CreateEntityAsync<CampaignGoogleAnalyticsDto, CampaignGoogleAnalyticsForCreation>(storeGADetail);
                        await _campaigngoogleanalyticsService.SaveChangesAsync();
                    }else
                    {
                        forbidden = true;
                    }                  
                }
                else if (_type == "GSC")
                {
                    string[] targetScopes = { "https://www.googleapis.com/auth/userinfo.email", "https://www.googleapis.com/auth/webmasters", "https://www.googleapis.com/auth/webmasters.readonly"};

                    string[] grantedScopes = res.scope.Split(' ');

                    bool areAllScopesGranted = targetScopes.All(targetScope => grantedScopes.Contains(targetScope));

                    //var result = await _campaigngscService.IsPropertiesExists(res.access_token);

                    if (areAllScopesGranted)
                    {
                        CampaignGSCForCreation storeGSCDetail = new CampaignGSCForCreation();
                        storeGSCDetail.UrlOrName = "";
                        storeGSCDetail.AccessToken = res.access_token;
                        storeGSCDetail.RefreshToken = res.refresh_token;
                        storeGSCDetail.IsActive = true;
                        storeGSCDetail.CampaignID = Guid.Parse(_campaignId);
                        var responseGSC = await _campaigngscService.CreateEntityAsync<CampaignGSCDto, CampaignGSCForCreation>(storeGSCDetail);
                        await _campaigngscService.SaveChangesAsync();
                    }
                    else
                    {
                        forbidden = true;
                    }

                }
                else if (_type == "GADS")
                {
                    string[] targetScopes = { "https://www.googleapis.com/auth/userinfo.email", "openid", "https://www.googleapis.com/auth/adwords" };

                    string[] grantedScopes = res.scope.Split(' ');

                    bool areAllScopesGranted = targetScopes.All(targetScope => grantedScopes.Contains(targetScope));

                    //var result =  _campaignGoogleAdsService.IsPropertiesExists(res.access_token);

                    if (areAllScopesGranted)
                    {
                        CampaignGoogleAdsForCreation storeGAdsDetail = new CampaignGoogleAdsForCreation();
                        storeGAdsDetail.AccessToken = res.access_token;
                        storeGAdsDetail.RefreshToken = res.refresh_token;
                        storeGAdsDetail.IsActive = true;
                        storeGAdsDetail.Name = "";
                        storeGAdsDetail.CampaignID = Guid.Parse(_campaignId);
                        var responseGAds = await _campaignGoogleAdsService.CreateEntityAsync<CampaignGoogleAdsDto, CampaignGoogleAdsForCreation>(storeGAdsDetail);
                        await _campaignGoogleAdsService.SaveChangesAsync();
                    }
                    else
                    {
                        forbidden = true;
                    }
                }
                else if (_type == "GBP")
                {
                    string[] targetScopes = { "https://www.googleapis.com/auth/userinfo.email", "https://www.googleapis.com/auth/business.manage" };

                    string[] grantedScopes = res.scope.Split(' ');

                    bool areAllScopesGranted = targetScopes.All(targetScope => grantedScopes.Contains(targetScope));

                    //var result =  _campaignGoogleAdsService.IsPropertiesExists(res.access_token);

                    if (areAllScopesGranted)
                    {
                        CampaignGBPForCreation storeGbpDetail = new CampaignGBPForCreation();
                        storeGbpDetail.AccessToken = res.access_token;
                        storeGbpDetail.RefreshToken = res.refresh_token;
                        storeGbpDetail.Name = "";
                        storeGbpDetail.AccountId = "";
                        storeGbpDetail.EmailId = "";
                        storeGbpDetail.CampaignID = Guid.Parse(_campaignId);
                        var responseGAds = await _campaignGBPService.CreateEntityAsync<CampaignGBPDto, CampaignGBPForCreation>(storeGbpDetail);
                        await _campaignGBPService.SaveChangesAsync();
                    }
                    else
                    {
                        forbidden = true;
                    }
                }
                else if (_type == "GSHEET")
                {
                    string[] targetScopes = { "https://www.googleapis.com/auth/drive.readonly", "https://www.googleapis.com/auth/spreadsheets.readonly", "https://www.googleapis.com/auth/userinfo.profile" };

                    string[] grantedScopes = res.scope.Split(' ');

                    bool areAllScopesGranted = targetScopes.All(targetScope => grantedScopes.Contains(targetScope));

                    //var result = await _campaigngoogleanalyticsService.IsPropertiesExists(res.access_token);

                    if (areAllScopesGranted)
                    {
                        CampaignGoogleSheetForCreation googleAccount = new CampaignGoogleSheetForCreation();
                        googleAccount.Name = res.name;
                        googleAccount.AccessToken = res.access_token;
                        googleAccount.RefreshToken = res.refresh_token;
                        googleAccount.CampaignID = Guid.Parse(_campaignId);                        
                        var response = await _campaignGoogleSheetService.CreateEntityAsync<CampaignGoogleSheetDto, CampaignGoogleSheetForCreation>(googleAccount);
                        await _campaignGoogleSheetService.SaveChangesAsync();
                    }
                    else
                    {
                        forbidden = true;
                    }
                }
                redirectUrl = _baseUrl + "/company/" + _companyId + "/projects/" + _campaignId + "/update" + "?integration=" + _type + "&forbidden=" + forbidden;
            }
            else
            {
                redirectUrl = _baseUrl + "/company/" + _companyId + "/projects/" + _campaignId + "/update" + "?integration=" + _type + "&forbidden=" + forbidden;
            }                      

            return Redirect(redirectUrl);
        }

        [AllowAnonymous]
        [HttpGet("GoogleExchangeCodeForPopup", Name = "GoogleExchangeCodeForPopup")]
        public async Task<IActionResult> GoogleExchangeCodeForPopup([FromQuery] string code)
        {
            var redirectUrl = string.Empty;
            var activePopup = 2;
            var integration = _type;
            bool forbidden = false;

            if (!string.IsNullOrEmpty(code))
            {
                GaToken res = await _campaigngoogleanalyticsService.GetAccessTokenUsingCodeForPopup(code);
           
                if (_type == "GA")
                {
                    CampaignGoogleAnalyticsForCreation storeGADetail = new CampaignGoogleAnalyticsForCreation();
                    storeGADetail.UrlOrName = "";
                    storeGADetail.AccessToken = res.access_token;
                    storeGADetail.RefreshToken = res.refresh_token;
                    storeGADetail.IsActive = true;
                    storeGADetail.CampaignID = Guid.Parse(_campaignId);
                    storeGADetail.IsGa4 = false;
                    var response = await _campaigngoogleanalyticsService.CreateEntityAsync<CampaignGoogleAnalyticsDto, CampaignGoogleAnalyticsForCreation>(storeGADetail);
                    await _campaigngoogleanalyticsService.SaveChangesAsync();
                }
                else if (_type == "GA4")
                {
                    string[] targetScopes = { "https://www.googleapis.com/auth/business.manage" };

                    string[] grantedScopes = res.scope.Split(' ');

                    bool areAllScopesGranted = targetScopes.All(targetScope => grantedScopes.Contains(targetScope));

                    //var result = await _campaigngoogleanalyticsService.IsPropertiesExists(res.access_token);

                    if (areAllScopesGranted)
                    {
                        CampaignGoogleAnalyticsForCreation storeGADetail = new CampaignGoogleAnalyticsForCreation();
                        storeGADetail.UrlOrName = "";
                        storeGADetail.AccessToken = res.access_token;
                        storeGADetail.RefreshToken = res.refresh_token;
                        storeGADetail.IsActive = true;
                        storeGADetail.CampaignID = Guid.Parse(_campaignId);
                        storeGADetail.IsGa4 = true;
                        var response = await _campaigngoogleanalyticsService.CreateEntityAsync<CampaignGoogleAnalyticsDto, CampaignGoogleAnalyticsForCreation>(storeGADetail);
                        await _campaigngoogleanalyticsService.SaveChangesAsync();
                    }
                    else
                    {
                        forbidden = true;
                    }

                }
                else if (_type == "GSC")
                {
                    string[] targetScopes = { "https://www.googleapis.com/auth/userinfo.email", "https://www.googleapis.com/auth/webmasters", "https://www.googleapis.com/auth/webmasters.readonly" };

                    string[] grantedScopes = res.scope.Split(' ');

                    bool areAllScopesGranted = targetScopes.All(targetScope => grantedScopes.Contains(targetScope));

                    //var result = await _campaigngscService.IsPropertiesExists(res.access_token);

                    if (areAllScopesGranted)
                    {
                        CampaignGSCForCreation storeGSCDetail = new CampaignGSCForCreation();
                        storeGSCDetail.UrlOrName = "";
                        storeGSCDetail.AccessToken = res.access_token;
                        storeGSCDetail.RefreshToken = res.refresh_token;
                        storeGSCDetail.IsActive = true;
                        storeGSCDetail.CampaignID = Guid.Parse(_campaignId);
                        var responseGSC = await _campaigngscService.CreateEntityAsync<CampaignGSCDto, CampaignGSCForCreation>(storeGSCDetail);
                        await _campaigngscService.SaveChangesAsync();
                    }
                    else
                    {
                        forbidden = true;
                    }

                }
                else if (_type == "GADS")
                {
                    string[] targetScopes = { "https://www.googleapis.com/auth/userinfo.email", "openid", "https://www.googleapis.com/auth/adwords" };

                    string[] grantedScopes = res.scope.Split(' ');

                    bool areAllScopesGranted = targetScopes.All(targetScope => grantedScopes.Contains(targetScope));

                    //var result =  _campaignGoogleAdsService.IsPropertiesExists(res.access_token);

                    if (areAllScopesGranted)
                    {
                        CampaignGoogleAdsForCreation storeGAdsDetail = new CampaignGoogleAdsForCreation();
                        storeGAdsDetail.AccessToken = res.access_token;
                        storeGAdsDetail.RefreshToken = res.refresh_token;
                        storeGAdsDetail.IsActive = true;
                        storeGAdsDetail.Name = "";
                        storeGAdsDetail.CampaignID = Guid.Parse(_campaignId);
                        var responseGAds = await _campaignGoogleAdsService.CreateEntityAsync<CampaignGoogleAdsDto, CampaignGoogleAdsForCreation>(storeGAdsDetail);
                        await _campaignGoogleAdsService.SaveChangesAsync();
                    }
                    else
                    {
                        forbidden = true;
                    }

                }
                else if (_type == "GBP")
                {
                    string[] targetScopes = { "https://www.googleapis.com/auth/userinfo.email", "https://www.googleapis.com/auth/business.manage" };

                    string[] grantedScopes = res.scope.Split(' ');

                    bool areAllScopesGranted = targetScopes.All(targetScope => grantedScopes.Contains(targetScope));

                    //var result =  _campaignGoogleAdsService.IsPropertiesExists(res.access_token);

                    if (areAllScopesGranted)
                    {
                        CampaignGBPForCreation storeGbpDetail = new CampaignGBPForCreation();
                        storeGbpDetail.AccessToken = res.access_token;
                        storeGbpDetail.RefreshToken = res.refresh_token;
                        storeGbpDetail.Name = "";
                        storeGbpDetail.AccountId = "";
                        storeGbpDetail.EmailId = "";
                        storeGbpDetail.CampaignID = Guid.Parse(_campaignId);
                        var responseGAds = await _campaignGBPService.CreateEntityAsync<CampaignGBPDto, CampaignGBPForCreation>(storeGbpDetail);
                        await _campaignGBPService.SaveChangesAsync();
                    }
                    else
                    {
                        forbidden = true;
                    }
                }
                else if (_type == "GSHEET")
                {
                    string[] targetScopes = { "https://www.googleapis.com/auth/drive.readonly", "https://www.googleapis.com/auth/spreadsheets.readonly", "openid", "https://www.googleapis.com/auth/userinfo.profile" };

                    string[] grantedScopes = res.scope.Split(' ');

                    bool areAllScopesGranted = targetScopes.All(targetScope => grantedScopes.Contains(targetScope));

                    //var result = await _campaigngoogleanalyticsService.IsPropertiesExists(res.access_token);

                    if (areAllScopesGranted)
                    {
                        CampaignGoogleSheetForCreation googleAccount = new CampaignGoogleSheetForCreation();
                        googleAccount.Name = res.name;
                        googleAccount.AccessToken = res.access_token;
                        googleAccount.RefreshToken = res.refresh_token;
                        googleAccount.CampaignID = Guid.Parse(_campaignId);
                        var response = await _campaignGoogleSheetService.CreateEntityAsync<CampaignGoogleSheetDto, CampaignGoogleSheetForCreation>(googleAccount);
                        await _campaignGoogleSheetService.SaveChangesAsync();
                    }
                    else
                    {
                        forbidden = true;
                    }
                }

                redirectUrl = _baseUrl + "/company/" + _companyId + "/home" + "?integration=" + integration + "&activePopup=" + activePopup + "&forbidden=" + forbidden;
            }
            else
            {
                 redirectUrl = _baseUrl + "/company/" + _companyId + "/home" + "?integration=" + integration + "&activePopup=" + activePopup + "&iscodenull="+ true + "&forbidden=" + forbidden;
            }
                return Redirect(redirectUrl);
        }

        [HttpGet("CheckGAIntegratedByCampaignId", Name = "CheckGAIntegratedByCampaignId")]
        public async Task<bool> CheckGAIntegratedByCampaignId(string campaignId)
        {
            var isExists = _campaigngoogleanalyticsService.GetAllEntities().Where(x => x.CampaignID.ToString() == campaignId).Any();
            if (isExists)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [HttpGet("GetGoogleAnalyticsList", Name = "GetGoogleAnalyticsList")]
        public async Task<RootObjectGoogleAnayltics> GetGoogleAnalyticsList(Guid campaignId)
        {
            var list = new RootObjectGoogleAnayltics();
            try
            {
                list = await _campaigngoogleanalyticsService.GetAnalyticsProfileIds(campaignId);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return list;
        }

        [HttpGet("GetGoogleAnalytics4List", Name = "GetGoogleAnalytics4List")]
        public async Task<Ga4RootList> GetGoogleAnalytics4List(Guid campaignId)
        {
            var list = new Ga4RootList();
            try
            {
                list = await _campaigngoogleanalyticsService.GetAnalytics4ProfileIds(campaignId);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return list;
        }

        [HttpGet("GetGoogleAnalyticsEmails", Name = "GetGoogleAnalyticsEmails")]
        public async Task<RootObjectOfGoogleEmail> GetGoogleAnalyticsEmails(Guid campaignId, string type)
        {
            var list = new RootObjectOfGoogleEmail();
            try
            {
                list = await _campaigngoogleanalyticsService.GetEmailAddress(campaignId, type);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return list;
        }

        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateCampaignGoogleAnalytics")]
        public async Task<IActionResult> UpdateCampaignGoogleAnalytics(Guid id, [FromBody] CampaignGoogleAnalyticsForUpdate CampaignGoogleAnalyticsForUpdate)
        {

            //if show not found
            if (!await _campaigngoogleanalyticsService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _campaigngoogleanalyticsService.UpdateEntityAsync(id, CampaignGoogleAnalyticsForUpdate);

            //return the response.
            return NoContent();
        }

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("UpdateCampaignGoogleAnalyticsByCampaignId", Name = "UpdateCampaignGoogleAnalyticsByCampaignId")]
        public async Task<IActionResult> UpdateCampaignGoogleAnalyticsByCampaignId([FromBody] CampaignGoogleAnalyticsForUpdate CampaignGoogleAnalyticsForUpdate)
        {
            try
            {
                var temp = await _campaigngoogleanalyticsService.UpdateBulkEntityAsync(x => new CampaignGoogleAnalytics
                {
                    UrlOrName = CampaignGoogleAnalyticsForUpdate.UrlOrName,
                    ProfileId = CampaignGoogleAnalyticsForUpdate.ProfileId,
                    IsGa4 = CampaignGoogleAnalyticsForUpdate.IsGa4,
                    EmailId = CampaignGoogleAnalyticsForUpdate.EmailId
                }, y => y.CampaignID == CampaignGoogleAnalyticsForUpdate.CampaignID && y.IsGa4 == CampaignGoogleAnalyticsForUpdate.IsGa4);

                //return the response.
                return Ok(true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateCampaignGoogleAnalytics(Guid id, [FromBody] JsonPatchDocument<CampaignGoogleAnalyticsForUpdate> jsonPatchDocument)
        {
            CampaignGoogleAnalyticsForUpdate dto = new CampaignGoogleAnalyticsForUpdate();
            CampaignGoogleAnalytics campaigngoogleanalytics = new CampaignGoogleAnalytics();

            //apply the patch changes to the dto. 
            jsonPatchDocument.ApplyTo(dto, ModelState);

            //if the jsonPatchDocument is not valid.
            if (!ModelState.IsValid)
            {
                //then return unprocessableEntity response.
                return new UnprocessableEntityObjectResult(ModelState);
            }

            //if the dto model is not valid after applying changes.
            if (!TryValidateModel(dto))
            {
                //then return unprocessableEntity response.
                return new UnprocessableEntityObjectResult(ModelState);
            }

            //map the chnages from dto to entity.
            Mapper.Map(dto, campaigngoogleanalytics);

            //set the Id for the show model.
            campaigngoogleanalytics.Id = id;

            //partially update the chnages to the db. 
            await _campaigngoogleanalyticsService.UpdatePartialEntityAsync(campaigngoogleanalytics, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateCampaignGoogleAnalytics")]
        public async Task<ActionResult<CampaignGoogleAnalyticsDto>> CreateCampaignGoogleAnalytics([FromBody] CampaignGoogleAnalyticsForCreation campaigngoogleanalytics)
        {
            //create a show in db.
            var campaigngoogleanalyticsToReturn = await _campaigngoogleanalyticsService.CreateEntityAsync<CampaignGoogleAnalyticsDto, CampaignGoogleAnalyticsForCreation>(campaigngoogleanalytics);

            //return the show created response.
            return campaigngoogleanalyticsToReturn; // CreatedAtRoute("GetCampaignGoogleAnalytics", new { id = campaigngoogleanalyticsToReturn.Id }, campaigngoogleanalyticsToReturn);
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("UpdateRefreshTokenAndEmail", Name = "UpdateRefreshTokenAndEmail")]
        public async Task<ActionResult<bool>> UpdateRefreshTokenAndEmail([FromBody] CampaignGoogleAnalyticsForCreation campaigngoogleanalytics)
        {

            var isCompanyIdExist = Request.Headers.ContainsKey("SelectedCompanyId");
            if (isCompanyIdExist)
            {
                Request.Headers.TryGetValue("SelectedCompanyId", out Microsoft.Extensions.Primitives.StringValues companyId);

                return await _campaigngoogleanalyticsService.UpdateRefreshTokenAndEmail(campaigngoogleanalytics, companyId);
            }

            //return the show created response.
            return false;
        }


        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteCampaignGoogleAnalyticsById")]
        public async Task<IActionResult> DeleteCampaignGoogleAnalyticsById(Guid id)
        {
            //if the campaigngoogleanalytics exists
            if (await _campaigngoogleanalyticsService.ExistAsync(x => x.Id == id))
            {
                //Update dashboard table 
                await _campaignService.UpdateDashboardTable(id, (int)ReportTypes.GoogleAnalyticsFour);
                //delete the campaigngoogleanalytics from the db.
                await _campaigngoogleanalyticsService.DeleteEntityAsync(id);
            }
            else
            {
                //if campaigngoogleanalytics doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForCampaignGoogleAnalytics(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCampaignGoogleAnalytics", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCampaignGoogleAnalytics", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteCampaignGoogleAnalyticsById", new { id = id }),
              "delete_campaigngoogleanalytics",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateCampaignGoogleAnalytics", new { id = id }),
             "update_campaigngoogleanalytics",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateCampaignGoogleAnalytics", new { }),
              "create_campaigngoogleanalytics",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForCampaignGoogleAnalyticss(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateCampaignGoogleAnalyticssResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateCampaignGoogleAnalyticssResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateCampaignGoogleAnalyticssResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateCampaignGoogleAnalyticssResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredCampaignGoogleAnalyticss",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredCampaignGoogleAnalyticss",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredCampaignGoogleAnalyticss",
                    new
                    {
                        fields = filterOptionsModel.Fields,
                        orderBy = filterOptionsModel.OrderBy,
                        searchQuery = filterOptionsModel.SearchQuery,
                        pageNumber = filterOptionsModel.PageNumber,
                        pageSize = filterOptionsModel.PageSize
                    });
            }
        }

        #endregion

    }
}
