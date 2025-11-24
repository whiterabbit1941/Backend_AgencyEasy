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
using Microsoft.Extensions.Configuration;
using System.Web;
using AutoMapper.Configuration;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using System.Linq;
using RestSharp;
using Microsoft.BingAds;
using Microsoft.BingAds.V13.CustomerManagement;
using Paging = Microsoft.BingAds.V13.CustomerManagement.Paging;
using System.Net.Http.Headers;
using System.ServiceModel;
using IdentityServer4.AccessTokenValidation;


namespace EventManagement.API.Controllers
{
    /// <summary>
    /// CampaignMicrosoftAd endpoint
    /// </summary>
    [Route("api/campaignmicrosoftads")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
    public class CampaignMicrosoftAdController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly ICampaignMicrosoftAdService _campaignmicrosoftadService;
        private ILogger<CampaignMicrosoftAdController> _logger;
        private readonly IUrlHelper _urlHelper;
        static string _companyId;
        static string _baseUrl;
        static string _campaignId;
        private IConfiguration _configuration;
        //private static AuthorizationData _authorizationData;
        //private static ServiceClient<ICustomerManagementService> _customerManagementService;
        //private static string ClientState = "ClientStateGoesHere";
        //private static string _output = "";

        private static AuthorizationData _authorizationData;
        private static ServiceClient<ICustomerManagementService> _customerManagementService;
        private static string ClientState = "ClientStateGoesHere";
        private static string _output = "";
        private static string _refreshToken = "";



        #endregion


        #region CONSTRUCTOR

        public CampaignMicrosoftAdController(ICampaignMicrosoftAdService campaignmicrosoftadService,
            ILogger<CampaignMicrosoftAdController> logger, IUrlHelper urlHelper, IConfiguration configuration)
        {
            _logger = logger;
            _campaignmicrosoftadService = campaignmicrosoftadService;
            _urlHelper = urlHelper;
            _configuration = configuration;
        }

        #endregion


        #region HTTPGET

        [HttpGet(Name = "GetFilteredCampaignMicrosoftAds")]
        [Produces("application/vnd.tourmanagement.campaignmicrosoftads.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CampaignMicrosoftAdDto>>> GetFilteredCampaignMicrosoftAds([FromQuery] FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_campaignmicrosoftadService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<CampaignMicrosoftAdDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var campaignmicrosoftadsFromRepo = await _campaignmicrosoftadService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.campaignmicrosoftads.hateoas+json")
            {
                //create HATEOAS links for each show.
                campaignmicrosoftadsFromRepo.ForEach(campaignmicrosoftad =>
                {
                    var entityLinks = CreateLinksForCampaignMicrosoftAd(campaignmicrosoftad.Id, filterOptionsModel.Fields);
                    campaignmicrosoftad.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = campaignmicrosoftadsFromRepo.TotalCount,
                    pageSize = campaignmicrosoftadsFromRepo.PageSize,
                    currentPage = campaignmicrosoftadsFromRepo.CurrentPage,
                    totalPages = campaignmicrosoftadsFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForCampaignMicrosoftAds(filterOptionsModel, campaignmicrosoftadsFromRepo.HasNext, campaignmicrosoftadsFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = campaignmicrosoftadsFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = campaignmicrosoftadsFromRepo.HasPrevious ?
                    CreateCampaignMicrosoftAdsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = campaignmicrosoftadsFromRepo.HasNext ?
                    CreateCampaignMicrosoftAdsResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = campaignmicrosoftadsFromRepo.TotalCount,
                    pageSize = campaignmicrosoftadsFromRepo.PageSize,
                    currentPage = campaignmicrosoftadsFromRepo.CurrentPage,
                    totalPages = campaignmicrosoftadsFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(campaignmicrosoftadsFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.campaignmicrosoftads.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetCampaignMicrosoftAd")]
        public async Task<ActionResult<CampaignMicrosoftAd>> GetCampaignMicrosoftAd(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object campaignmicrosoftadEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetCampaignMicrosoftAd called");

                //then get the whole entity and map it to the Dto.
                campaignmicrosoftadEntity = Mapper.Map<CampaignMicrosoftAdDto>(await _campaignmicrosoftadService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                campaignmicrosoftadEntity = await _campaignmicrosoftadService.GetPartialEntityAsync(id, fields);
            }

            //if campaignmicrosoftad not found.
            if (campaignmicrosoftadEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.campaignmicrosoftads.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForCampaignMicrosoftAd(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((CampaignMicrosoftAdDto)campaignmicrosoftadEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = campaignmicrosoftadEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = campaignmicrosoftadEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }


        //[AllowAnonymous]
        [HttpGet("microsoft-login", Name = "microsoft-login")]
        public string MicrosoftLogin([FromQuery] string companyId, [FromQuery] string campaignId, [FromQuery] string baseUrl)
        {
            _companyId = companyId;
            _campaignId = campaignId;
            _baseUrl = baseUrl;

            string redirectUri = _configuration.GetSection("HostedUrl").Value + "api/campaignmicrosoftads/MicrosoftAdsCode";

            var myAuthDto = new MsAuthDto()
            {
                campaignId = campaignId,
                companyId = companyId
            };

            string state = JsonConvert.SerializeObject(myAuthDto);

            // Construct the URL with proper encoding
            string authorizationUrl = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize?" +
                "client_id=" + HttpUtility.UrlEncode(_configuration.GetSection("MicrosoftClientId").Value) +
                "&response_type=code" +
                "&redirect_uri=" + HttpUtility.UrlEncode(redirectUri) +
                "&response_mode=query" +
                "&scope=" + HttpUtility.UrlEncode("openid profile https://ads.microsoft.com/msads.manage https://ads.microsoft.com/ads.manage offline_access") +
                "&state=" + HttpUtility.UrlEncode(state) + "&prompt=consent";

            return authorizationUrl;
        }

        [AllowAnonymous]
        [HttpGet("MicrosoftAdsCode", Name = "MicrosoftAdsCode")]
        public async Task<IActionResult> MicrosoftAdsCode([FromQuery] string code)
        {
            var data = Request.Query["state"].ToString();

            var muAuthDetail = JsonConvert.DeserializeObject<MsAuthDto>(data);

            var redirectUrl = string.Empty;

            bool forbidden = false;

            // Replace the values with your actual client ID, scope, code, redirect URI, and client secret
            string clientId = _configuration.GetSection("MicrosoftClientId").Value;
            string clientSecret = _configuration.GetSection("MicrosoftClientSeceret").Value;
            //string tenantId = _configuration.GetSection("TenantId").Value;

            string scope = "openid profile https://ads.microsoft.com/msads.manage https://ads.microsoft.com/ads.manage offline_access";

            //string code = "OAAABAAAAiL9Kn2Z27UubvWFPbm0gLWQJVzCTE9UkP3pSx1aXxUjq3n8b2JRLk4OxVXr...";
            string redirectUriFrontEnd = _baseUrl + "/company/" + muAuthDetail.companyId + "/projects/" + muAuthDetail.campaignId + "/update" + "?integration=microsoftads&forbidden=" + forbidden;

            string redirectUri = _configuration.GetSection("HostedUrl").Value + "api/campaignmicrosoftads/MicrosoftAdsCode";

            string grantType = "authorization_code";

            // Create a RestSharp client
            var client = new RestClient("https://login.microsoftonline.com/");

            // Create a RestSharp request
            var request = new RestRequest("common/oauth2/v2.0/token", Method.Post);

            // Add headers
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            // Add parameters
            request.AddParameter("client_id", clientId);
            request.AddParameter("scope", scope);
            request.AddParameter("code", code);
            request.AddParameter("redirect_uri", redirectUri);
            request.AddParameter("grant_type", grantType);
            request.AddParameter("client_secret", clientSecret);

            // Execute the request
            var response = await client.ExecuteAsync(request);

            var msAdsResponse = JsonConvert.DeserializeObject<TokenResponseDTO>(response.Content);

            if (msAdsResponse != null && msAdsResponse.scope != null)
            {
                string[] targetScopes = { "https://ads.microsoft.com/msads.manage", "https://ads.microsoft.com/ads.manage" };

                string[] grantedScopes = msAdsResponse.scope.Split(' ');

                bool areAllScopesGranted = targetScopes.All(targetScope => grantedScopes.Contains(targetScope));

                if (areAllScopesGranted)
                {
                    CampaignMicrosoftAdForCreation msCreation = new CampaignMicrosoftAdForCreation();
                    msCreation.AccountName = "";
                    msCreation.AccessToken = msAdsResponse.access_token;
                    msCreation.RefreshToken = msAdsResponse.refresh_token;
                    msCreation.CampaignID = Guid.Parse(muAuthDetail.campaignId);
                    msCreation.AccessExpire = msAdsResponse.expires_in;

                    var saveMsAds = await _campaignmicrosoftadService.CreateEntityAsync<CampaignMicrosoftAdDto, CampaignMicrosoftAdForCreation>(msCreation);
                    await _campaignmicrosoftadService.SaveChangesAsync();
                }
            }
            else
            {
                forbidden = true;
            }

            return Redirect(redirectUriFrontEnd);
        }

        //[AllowAnonymous]
        [HttpGet("list-accounts", Name = "list-accounts")]
        public async Task<List<MsAdAccountListDto>> ListAccountsAsync(Guid campaignId)
        {
            try
            {
                var listOfAccount = await _campaignmicrosoftadService.GetMsAdAccountList(campaignId);

                return listOfAccount;
            }
            // Catch authentication exceptions
            catch (OAuthTokenRequestException ex)
            {
                //OutputStatusMessage(string.Format("Couldn't get OAuth tokens. Error: {0}. Description: {1}", ex.Details.Error, ex.Details.Description));
            }
            // Catch Customer Management service exceptions
            catch (FaultException<Microsoft.BingAds.V13.CustomerManagement.AdApiFaultDetail> ex)
            {
                //OutputStatusMessage(string.Join("; ", ex.Detail.Errors.Select(error => string.Format("{0}: {1}", error.Code, error.Message))));
            }
            catch (FaultException<Microsoft.BingAds.V13.CustomerManagement.ApiFault> ex)
            {
                //OutputStatusMessage(string.Join("; ", ex.Detail.OperationErrors.Select(error => string.Format("{0}: {1}", error.Code, error.Message))));
            }
            catch (Exception ex)
            {
                var test = ex.InnerException;
            }

            return new List<MsAdAccountListDto>() { };
        }

        
        [HttpGet("get-campaign-performance", Name = "get-campaign-performance")]
        public async Task<RootCampaignPerformace> GetCampaignPerformanceReport(Guid campaignId, string startDate, string endDate, long adCampaignId)
        {
            var myreport = await _campaignmicrosoftadService.GetCampaignPerformanceReport(campaignId, startDate, endDate, adCampaignId);

            return myreport;
        }

        
        [HttpGet("get-adgroup-performance", Name = "get-adgroup-performance")]
        public async Task<RootAdGroupPerformance> GetAdGroupPerformanceReport(Guid campaignId, string startDate, string endDate, long adCampaignId)
        {
            var myreport = await _campaignmicrosoftadService.GetAdGroupPerformanceReport(campaignId, startDate, endDate, adCampaignId);

            return myreport;
        }

        [AllowAnonymous]
        [HttpGet("get-keyword-performance", Name = "get-keyword-performance")]
        public async Task<RootKeywordPerformance> GetKeywordPerformanceReport(Guid campaignId, string startDate, string endDate, long adCampaignId)
        {
            var myreport = await _campaignmicrosoftadService.GetKeywordPerformanceReport(campaignId, startDate, endDate, adCampaignId);

            return myreport;
        }

        
        [HttpGet("get-conversion-performance", Name = "get-conversion-performance")]
        public async Task<RootConversionPerformance> GetConversionPerformanceReport(Guid campaignId, string startDate, string endDate, long adCampaignId)
        {
            var myreport = await _campaignmicrosoftadService.GetConversionPerformanceReport(campaignId, startDate, endDate, adCampaignId);

            return myreport;
        }

        [HttpGet("get-campaign-list", Name = "get-campaign-list")]
        public async Task<List<MsAdCampaignList>> GetCampaignList(Guid campaignId)
        {
            var myreport = await _campaignmicrosoftadService.GetCampaignList(campaignId);

            return myreport;
        }
        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateCampaignMicrosoftAd")]
        public async Task<IActionResult> UpdateCampaignMicrosoftAd(Guid id, [FromBody] CampaignMicrosoftAdForUpdate CampaignMicrosoftAdForUpdate)
        {

            //if show not found
            if (!await _campaignmicrosoftadService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _campaignmicrosoftadService.UpdateEntityAsync(id, CampaignMicrosoftAdForUpdate);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateCampaignMicrosoftAd(Guid id, [FromBody] JsonPatchDocument<CampaignMicrosoftAdForUpdate> jsonPatchDocument)
        {
            CampaignMicrosoftAdForUpdate dto = new CampaignMicrosoftAdForUpdate();
            CampaignMicrosoftAd campaignmicrosoftad = new CampaignMicrosoftAd();

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
            Mapper.Map(dto, campaignmicrosoftad);

            //set the Id for the show model.
            campaignmicrosoftad.Id = id;

            //partially update the chnages to the db. 
            await _campaignmicrosoftadService.UpdatePartialEntityAsync(campaignmicrosoftad, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateCampaignMicrosoftAd")]
        public async Task<ActionResult<CampaignMicrosoftAdDto>> CreateCampaignMicrosoftAd([FromBody] CampaignMicrosoftAdForCreation campaignmicrosoftad)
        {
            //create a show in db.
            var campaignmicrosoftadToReturn = await _campaignmicrosoftadService.CreateEntityAsync<CampaignMicrosoftAdDto, CampaignMicrosoftAdForCreation>(campaignmicrosoftad);

            //return the show created response.
            return CreatedAtRoute("GetCampaignMicrosoftAd", new { id = campaignmicrosoftadToReturn.Id }, campaignmicrosoftadToReturn);
        }

        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteCampaignMicrosoftAdById")]
        public async Task<IActionResult> DeleteCampaignMicrosoftAdById(Guid id)
        {
            //if the campaignmicrosoftad exists
            if (await _campaignmicrosoftadService.ExistAsync(x => x.Id == id))
            {
                //delete the campaignmicrosoftad from the db.
                await _campaignmicrosoftadService.DeleteEntityAsync(id);
            }
            else
            {
                //if campaignmicrosoftad doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForCampaignMicrosoftAd(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCampaignMicrosoftAd", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCampaignMicrosoftAd", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteCampaignMicrosoftAdById", new { id = id }),
              "delete_campaignmicrosoftad",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateCampaignMicrosoftAd", new { id = id }),
             "update_campaignmicrosoftad",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateCampaignMicrosoftAd", new { }),
              "create_campaignmicrosoftad",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForCampaignMicrosoftAds(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateCampaignMicrosoftAdsResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateCampaignMicrosoftAdsResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateCampaignMicrosoftAdsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateCampaignMicrosoftAdsResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredCampaignMicrosoftAds",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredCampaignMicrosoftAds",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredCampaignMicrosoftAds",
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
