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
using Facebook;
using Microsoft.Extensions.Configuration;
using RestSharp;
using RestSharp.Extensions;
using System.Linq;
using Google.Rpc;
using Microsoft.VisualBasic;

namespace EventManagement.API.Controllers
{
    /// <summary>
    /// CampaignFacebook endpoint
    /// </summary>
    [Route("api/campaignfacebooks")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]

    public class CampaignFacebookController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly ICampaignFacebookService _campaignfacebookService;
        private readonly ICampaignInstagramService _campaignInstagramService;
        private readonly ICampaignFacebookAdsService _campaignFacebookAdsService;
        private ILogger<CampaignFacebookController> _logger;
        private readonly IUrlHelper _urlHelper;
        private readonly IConfiguration _configuration;
        static string _baseUrl;
        static string _companyId;
        static string _integration;
        static string _accessToken;
        static string _campaignId;

        #endregion


        #region CONSTRUCTOR

        public CampaignFacebookController(ICampaignFacebookAdsService campaignFacebookAdsService, ICampaignInstagramService campaignInstagramService, IConfiguration configuration, ICampaignFacebookService campaignfacebookService, ILogger<CampaignFacebookController> logger, IUrlHelper urlHelper)
        {
            _logger = logger;
            _campaignfacebookService = campaignfacebookService;
            _urlHelper = urlHelper;
            _configuration = configuration;
            _campaignInstagramService = campaignInstagramService;
            _campaignFacebookAdsService = campaignFacebookAdsService;
        }

        #endregion


        #region HTTPGET

        /// <summary>
        /// Setup Facebook, Instagram & Facebook-Ads Account
        /// </summary>
        /// <returns>Callback Url</returns>
        [HttpGet("SetupGraphApi", Name = "SetupGraphApi")]
        
        public IActionResult SetupGraphApi(string companyId, string integration, string source, string campaignId, string baseUrl)
        {
            _baseUrl = baseUrl;
            _companyId = companyId;
            _integration = integration;
            _campaignId = campaignId;

            var fb = new FacebookClient();
            // Add other permissions as needed
            string scope = "", redirectUri = string.Empty;
            if (source == "tab")
            {
                redirectUri = _configuration.GetSection("HostedUrl").Value + "api/campaignfacebooks/GraphApiCallback";
            }
            else
            {
                redirectUri = _configuration.GetSection("HostedUrl").Value + "api/campaignfacebooks/GraphApiCallbackForPop";
            }
            if (integration.ToLower() == "facebook")
            {
                scope = "pages_show_list,read_insights,pages_read_engagement,business_management";
            }
            else if (integration.ToLower() == "instagram")
            {
                scope = "instagram_basic,pages_show_list,read_insights,pages_read_engagement,instagram_manage_insights,business_management";
            }
            else if (integration.ToLower() == "facebook-ads")
            {
                scope = "pages_show_list,ads_management,pages_read_engagement,business_management";
            }
            var loginUrl = fb.GetLoginUrl(new
            {
                client_id = _configuration.GetSection("FacebookAppId").Value,
                redirect_uri = redirectUri,
                response_type = "code",
                scope = scope
            });
            return Ok(loginUrl.AbsoluteUri);
        }

        /// <summary>
        ///     Get Facebook, Instagram & Facebook-Ads Token
        /// </summary>
        /// <returns>Acess Token</returns>
        [HttpGet("GraphApiCallback", Name = "GraphApiCallback")]
        [AllowAnonymous]
        public ActionResult GraphApiCallback(string code)
        {
            var redirectUrl = string.Empty;
            if (!string.IsNullOrEmpty(code))
            {
                var fb = new FacebookClient();
                try
                {
                    string redirectUri = _configuration.GetSection("HostedUrl").Value + "api/campaignfacebooks/GraphApiCallback";
                    dynamic result = fb.Post("oauth/access_token", new
                    {
                        client_id = _configuration.GetSection("FacebookAppId").Value,
                        client_secret = _configuration.GetSection("FacebookAppSecret").Value,
                        redirect_uri = redirectUri,
                        code = code
                    });

                    _accessToken = result.access_token;

                    string endpoint = "";
                    bool forbidden = false;
                    if (_integration.ToLower() == "facebook")
                    {

                        var hasDeclinedStatus =  _campaignfacebookService.IsPermissionGranted(_accessToken);

                        if (!hasDeclinedStatus)
                        {
                            CampaignFacebookForCreation storeTokenInDB = new CampaignFacebookForCreation();
                            storeTokenInDB.UrlOrName = "";
                            storeTokenInDB.AccessToken = _accessToken;
                            storeTokenInDB.RefreshToken = result.refresh_token;
                            storeTokenInDB.IsActive = true;
                            storeTokenInDB.CampaignID = Guid.Parse(_campaignId);
                            var response = _campaignfacebookService.CreateEntityAsync<CampaignFacebookDto, CampaignFacebookForCreation>(storeTokenInDB);
                            _campaignfacebookService.SaveChangesAsync();
                        }
                        else
                        {
                            forbidden = true;
                        }

                        _integration = "facebook";
                    }
                    else if (_integration.ToLower() == "instagram")
                    {

                        var hasDeclinedStatus = _campaignInstagramService.IsPermissionGranted(_accessToken);

                        if (!hasDeclinedStatus)
                        {
                            CampaignInstagramForCreation storeTokenInDB = new CampaignInstagramForCreation();
                            storeTokenInDB.UrlOrName = "";
                            storeTokenInDB.AccessToken = _accessToken;
                            storeTokenInDB.RefreshToken = result.refresh_token;
                            storeTokenInDB.IsActive = true;
                            storeTokenInDB.CampaignID = Guid.Parse(_campaignId);
                            var response = _campaignInstagramService.CreateEntityAsync<CampaignInstagramDto, CampaignInstagramForCreation>(storeTokenInDB);
                            _campaignInstagramService.SaveChangesAsync();
                        }
                        else
                        {
                            forbidden = true;
                        }                 

                        _integration = "instagram";
                    }
                    else if (_integration.ToLower() == "facebook-ads")
                    {

                        var hasDeclinedStatus = _campaignFacebookAdsService.IsPermissionGranted(_accessToken);

                        if (!hasDeclinedStatus)
                        {
                            CampaignFacebookAdsForCreation storeTokenInDB = new CampaignFacebookAdsForCreation();
                            storeTokenInDB.AdAccountName = "";
                            storeTokenInDB.AccessToken = _accessToken;
                            storeTokenInDB.RefreshToken = result.refresh_token;
                            storeTokenInDB.IsActive = true;
                            storeTokenInDB.CampaignID = Guid.Parse(_campaignId);
                            var response = _campaignFacebookAdsService.CreateEntityAsync<CampaignFacebookAdsDto, CampaignFacebookAdsForCreation>(storeTokenInDB);
                            _campaignFacebookAdsService.SaveChangesAsync();
                        }
                        else
                        {
                            forbidden = true;
                        }

                        _integration = "facebook-ads";
                    }

                     redirectUrl = _baseUrl + "/company/" + _companyId + "/projects/" + _campaignId + "/update" + "?integration=" + _integration + "&forbidden=" + forbidden;
                    
                }
                catch (Exception ex)
                {
                    return Redirect(_baseUrl + "/home/campaign");
                }
            }
            else
            {
                redirectUrl = _baseUrl + "/company/" + _companyId + "/projects/" + _campaignId + "/update" + "?integration=" + _integration + "&forbidden=" + false; 
            }

            return Redirect(redirectUrl);
        }

       

        /// <summary>
        ///     Get Facebook, Instagram & Facebook-Ads Token
        /// </summary>
        /// <returns>Acess Token</returns>
        [HttpGet("GraphApiCallbackForPop", Name = "GraphApiCallbackForPop")]
        [AllowAnonymous]
        public ActionResult GraphApiCallbackForPop(string code)
        {
            var fb = new FacebookClient();
            var fblist = new List<FacebookList>();
            var instaList = new List<InstaList>();
            var instaPageList = new List<InstaList>();

            var redirectUrl = string.Empty;

            var queryparam = "";
            var integration = _integration;
            var activePopup = 2;
            string endpoint = "/home";
            bool forbidden = false;

            if (!string.IsNullOrEmpty(code))
            {
                try
                {
                    string redirectUri = _configuration.GetSection("HostedUrl").Value + "api/campaignfacebooks/GraphApiCallbackForPop";
                    dynamic result = fb.Post("oauth/access_token", new
                    {
                        client_id = _configuration.GetSection("FacebookAppId").Value,
                        client_secret = _configuration.GetSection("FacebookAppSecret").Value,
                        redirect_uri = redirectUri,
                        code = code
                    });

                    _accessToken = result.access_token;

               
                    if (_integration.ToLower() == "facebook")
                    {
                        var hasDeclinedStatus = _campaignfacebookService.IsPermissionGranted(_accessToken);

                        if (!hasDeclinedStatus)
                        {
                            CampaignFacebookForCreation storeTokenInDB = new CampaignFacebookForCreation();
                            storeTokenInDB.UrlOrName = "";
                            storeTokenInDB.AccessToken = _accessToken;
                            storeTokenInDB.RefreshToken = result.refresh_token;
                            storeTokenInDB.IsActive = true;
                            storeTokenInDB.CampaignID = Guid.Parse(_campaignId);
                            var response = _campaignfacebookService.CreateEntityAsync<CampaignFacebookDto, CampaignFacebookForCreation>(storeTokenInDB);
                            _campaignfacebookService.SaveChangesAsync();
                        }
                        else
                        {
                            forbidden = true;
                        }

                        endpoint = "/home";
                        integration = "facebook";
                    }
                    else if (_integration.ToLower() == "instagram")
                    {
                        var hasDeclinedStatus = _campaignInstagramService.IsPermissionGranted(_accessToken);

                        if (!hasDeclinedStatus)
                        {
                            CampaignInstagramForCreation storeTokenInDB = new CampaignInstagramForCreation();
                            storeTokenInDB.UrlOrName = "";
                            storeTokenInDB.AccessToken = _accessToken;
                            storeTokenInDB.RefreshToken = result.refresh_token;
                            storeTokenInDB.IsActive = true;
                            storeTokenInDB.CampaignID = Guid.Parse(_campaignId);
                            var response = _campaignInstagramService.CreateEntityAsync<CampaignInstagramDto, CampaignInstagramForCreation>(storeTokenInDB);
                            _campaignInstagramService.SaveChangesAsync();
                        }
                        else
                        {
                            forbidden = true;
                        }

                        endpoint = "/home";
                        integration = "instagram";
                    }
                    else if (_integration.ToLower() == "facebook-ads")
                    {

                        var hasDeclinedStatus = _campaignFacebookAdsService.IsPermissionGranted(_accessToken);

                        if (!hasDeclinedStatus)
                        {
                            CampaignFacebookAdsForCreation storeTokenInDB = new CampaignFacebookAdsForCreation();
                            storeTokenInDB.AdAccountName = "";
                            storeTokenInDB.AccessToken = _accessToken;
                            storeTokenInDB.RefreshToken = result.refresh_token;
                            storeTokenInDB.IsActive = true;
                            storeTokenInDB.CampaignID = Guid.Parse(_campaignId);
                            var response = _campaignFacebookAdsService.CreateEntityAsync<CampaignFacebookAdsDto, CampaignFacebookAdsForCreation>(storeTokenInDB);
                            _campaignFacebookAdsService.SaveChangesAsync();
                        }
                        else
                        {
                            forbidden = true;
                        }

                        endpoint = "home";
                        integration = "facebook-ads";
                    }

                    redirectUrl = _baseUrl + "/company/" + _companyId + endpoint + "?integration=" + integration + "&activePopup=" + activePopup + "&forbidden=" + forbidden;
                    
                }
                catch (Exception ex)
                {
                    return Redirect(_baseUrl + "/company/" + _companyId);
                }
            }
            else
            {
                redirectUrl = _baseUrl + "/company/" + _companyId + endpoint + "?integration=" + integration + "&activePopup=" + activePopup + "&iscodenull=" + true + "&forbidden=" + forbidden;
            }
            return Redirect(redirectUrl);
        }

        [HttpGet("GetFacebookList", Name = "GetFacebookList")]
        public async Task<RootObjectFBData> GetFacebookList(Guid campaignId)
        {
            var returnData = new RootObjectFBData();
            try
            {
                returnData = await _campaignfacebookService.GetFaceBookPageList(campaignId);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return returnData;
        }

        [HttpGet("GetInstagramList", Name = "GetInstagramList")]
        public async Task<RootObjectInstaData> GetInstagramList(Guid campaignId)
        {
            var returnData = new RootObjectInstaData();
            try
            {
                var fbPageData = await _campaignInstagramService.GetFaceBookPageList(campaignId);
                var instaIds = await _campaignInstagramService.GetInstaIds(fbPageData.data, campaignId);
                returnData = await _campaignInstagramService.GetInstagramPageLists(instaIds.data, campaignId);

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return returnData;
        }

        [HttpGet("GetFacebookAdsAccountList", Name = "GetFacebookAdsAccountList")]
        public async Task<RootObjectFBAdsData> GetFacebookAdsAccountList(Guid campaignId)
        {
            var returnData = new RootObjectFBAdsData();
            try
            {
                returnData = await _campaignFacebookAdsService.GetFbAllAdsAccountDetails(campaignId);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return returnData;
        }

        [HttpGet("CheckFacebookIntegratedByCampaignId", Name = "CheckFacebookIntegratedByCampaignId")]
        public async Task<bool> CheckFacebookIntegratedByCampaignId(string campaignId)
        {
            var isExists = _campaignfacebookService.GetAllEntities().Where(x => x.CampaignID.ToString() == campaignId).Any();
            if (isExists)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [HttpGet(Name = "GetFilteredCampaignFacebooks")]
        [Produces("application/vnd.tourmanagement.campaignfacebooks.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CampaignFacebookDto>>> GetFilteredCampaignFacebooks([FromQuery] FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_campaignfacebookService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<CampaignFacebookDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var campaignfacebooksFromRepo = await _campaignfacebookService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.campaignfacebooks.hateoas+json")
            {
                //create HATEOAS links for each show.
                campaignfacebooksFromRepo.ForEach(campaignfacebook =>
                {
                    var entityLinks = CreateLinksForCampaignFacebook(campaignfacebook.Id, filterOptionsModel.Fields);
                    campaignfacebook.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = campaignfacebooksFromRepo.TotalCount,
                    pageSize = campaignfacebooksFromRepo.PageSize,
                    currentPage = campaignfacebooksFromRepo.CurrentPage,
                    totalPages = campaignfacebooksFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForCampaignFacebooks(filterOptionsModel, campaignfacebooksFromRepo.HasNext, campaignfacebooksFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = campaignfacebooksFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = campaignfacebooksFromRepo.HasPrevious ?
                    CreateCampaignFacebooksResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = campaignfacebooksFromRepo.HasNext ?
                    CreateCampaignFacebooksResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = campaignfacebooksFromRepo.TotalCount,
                    pageSize = campaignfacebooksFromRepo.PageSize,
                    currentPage = campaignfacebooksFromRepo.CurrentPage,
                    totalPages = campaignfacebooksFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(campaignfacebooksFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.campaignfacebooks.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetCampaignFacebook")]
        public async Task<ActionResult<CampaignFacebook>> GetCampaignFacebook(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object campaignfacebookEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetCampaignFacebook called");

                //then get the whole entity and map it to the Dto.
                campaignfacebookEntity = Mapper.Map<CampaignFacebookDto>(await _campaignfacebookService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                campaignfacebookEntity = await _campaignfacebookService.GetPartialEntityAsync(id, fields);
            }

            //if campaignfacebook not found.
            if (campaignfacebookEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.campaignfacebooks.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForCampaignFacebook(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((CampaignFacebookDto)campaignfacebookEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = campaignfacebookEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = campaignfacebookEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }

        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateCampaignFacebook")]
        public async Task<IActionResult> UpdateCampaignFacebook(Guid id, [FromBody] CampaignFacebookForUpdate CampaignFacebookForUpdate)
        {

            //if show not found
            if (!await _campaignfacebookService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _campaignfacebookService.UpdateEntityAsync(id, CampaignFacebookForUpdate);

            //return the response.
            return NoContent();
        }

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("UpdateCampaignFacebookByCampaignId", Name = "UpdateCampaignFacebookByCampaignId")]
        public async Task<IActionResult> UpdateCampaignFacebookByCampaignId([FromBody] CampaignFacebookForUpdate CampaignFacebookForUpdate)
        {
            try
            {
                var temp = await _campaignfacebookService.UpdateBulkEntityAsync(x => new CampaignFacebook { UrlOrName = CampaignFacebookForUpdate.UrlOrName }, y => y.CampaignID == CampaignFacebookForUpdate.CampaignID);

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
        public async Task<IActionResult> PartiallyUpdateCampaignFacebook(Guid id, [FromBody] JsonPatchDocument<CampaignFacebookForUpdate> jsonPatchDocument)
        {
            CampaignFacebookForUpdate dto = new CampaignFacebookForUpdate();
            CampaignFacebook campaignfacebook = new CampaignFacebook();

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
            Mapper.Map(dto, campaignfacebook);

            //set the Id for the show model.
            campaignfacebook.Id = id;

            //partially update the chnages to the db. 
            await _campaignfacebookService.UpdatePartialEntityAsync(campaignfacebook, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateCampaignFacebook")]
        public async Task<ActionResult<CampaignFacebookDto>> CreateCampaignFacebook([FromBody] CampaignFacebookForCreation campaignfacebook)
        {
            campaignfacebook.AccessToken = _accessToken;
            //create a show in db.
            var campaignfacebookToReturn = await _campaignfacebookService.CreateEntityAsync<CampaignFacebookDto, CampaignFacebookForCreation>(campaignfacebook);

            //return the show created response.
            return Ok(campaignfacebookToReturn); //CreatedAtRoute("GetCampaignFacebook", new { id = campaignfacebookToReturn.Id }, campaignfacebookToReturn);
        }

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("GetFacebookReport", Name = "GetFacebookReport")]
        public async Task<ActionResult<FacebookData>> GetFacebookReport(string campaignId, string startTime, string endTime)
        {
            //create a show in db.
            var campaignfacebookToReturn = await _campaignfacebookService.GetFacebookReport(new Guid(campaignId), Convert.ToDateTime(startTime), Convert.ToDateTime(endTime));

            return Ok(campaignfacebookToReturn);
        }

        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteCampaignFacebookById")]
        public async Task<IActionResult> DeleteCampaignFacebookById(Guid id)
        {
            //if the campaignfacebook exists
            if (await _campaignfacebookService.ExistAsync(x => x.Id == id))
            {
                //delete the campaignfacebook from the db.
                await _campaignfacebookService.DeleteEntityAsync(id);
            }
            else
            {
                //if campaignfacebook doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForCampaignFacebook(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCampaignFacebook", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCampaignFacebook", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteCampaignFacebookById", new { id = id }),
              "delete_campaignfacebook",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateCampaignFacebook", new { id = id }),
             "update_campaignfacebook",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateCampaignFacebook", new { }),
              "create_campaignfacebook",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForCampaignFacebooks(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateCampaignFacebooksResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateCampaignFacebooksResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateCampaignFacebooksResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateCampaignFacebooksResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredCampaignFacebooks",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredCampaignFacebooks",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredCampaignFacebooks",
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
