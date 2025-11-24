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
using System.Linq;
using IdentityServer4.AccessTokenValidation;
using Microsoft.VisualBasic;
using RestSharp;

namespace EventManagement.API.Controllers
{
    /// <summary>
    /// CampaignLinkedin endpoint
    /// </summary>
    [Route("api/campaignlinkedins")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
    public class CampaignLinkedinController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly ICampaignLinkedinService _campaignlinkedinService;
        private readonly ILinkedinAdService _linkedinAdService;
        private ILogger<CampaignLinkedinController> _logger;
        private readonly IUrlHelper _urlHelper;
        static string _baseUrl;
        static string _campaignID;
        static string _companyID;
        static string _code;
        private readonly IConfiguration _configuration;
        static string _type;

        #endregion


        #region CONSTRUCTOR

        public CampaignLinkedinController(IConfiguration configuration, ICampaignLinkedinService campaignlinkedinService,
            ILogger<CampaignLinkedinController> logger, IUrlHelper urlHelper, ILinkedinAdService linkedinAdService)
        {
            _logger = logger;
            _campaignlinkedinService = campaignlinkedinService;
            _linkedinAdService = linkedinAdService;
            _urlHelper = urlHelper;
            _configuration = configuration;
            _linkedinAdService = linkedinAdService;
        }

        #endregion


        #region HTTPGET

        [HttpGet(Name = "GetFilteredCampaignLinkedins")]
        [Produces("application/vnd.tourmanagement.campaignlinkedins.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CampaignLinkedinDto>>> GetFilteredCampaignLinkedins([FromQuery] FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_campaignlinkedinService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<CampaignLinkedinDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var campaignlinkedinsFromRepo = await _campaignlinkedinService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.campaignlinkedins.hateoas+json")
            {
                //create HATEOAS links for each show.
                campaignlinkedinsFromRepo.ForEach(campaignlinkedin =>
                {
                    var entityLinks = CreateLinksForCampaignLinkedin(campaignlinkedin.Id, filterOptionsModel.Fields);
                    campaignlinkedin.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = campaignlinkedinsFromRepo.TotalCount,
                    pageSize = campaignlinkedinsFromRepo.PageSize,
                    currentPage = campaignlinkedinsFromRepo.CurrentPage,
                    totalPages = campaignlinkedinsFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForCampaignLinkedins(filterOptionsModel, campaignlinkedinsFromRepo.HasNext, campaignlinkedinsFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = campaignlinkedinsFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = campaignlinkedinsFromRepo.HasPrevious ?
                    CreateCampaignLinkedinsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = campaignlinkedinsFromRepo.HasNext ?
                    CreateCampaignLinkedinsResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = campaignlinkedinsFromRepo.TotalCount,
                    pageSize = campaignlinkedinsFromRepo.PageSize,
                    currentPage = campaignlinkedinsFromRepo.CurrentPage,
                    totalPages = campaignlinkedinsFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(campaignlinkedinsFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.campaignlinkedins.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetCampaignLinkedin")]
        public async Task<ActionResult<CampaignLinkedin>> GetCampaignLinkedin(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object campaignlinkedinEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetCampaignLinkedin called");

                //then get the whole entity and map it to the Dto.
                campaignlinkedinEntity = Mapper.Map<CampaignLinkedinDto>(await _campaignlinkedinService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                campaignlinkedinEntity = await _campaignlinkedinService.GetPartialEntityAsync(id, fields);
            }

            //if campaignlinkedin not found.
            if (campaignlinkedinEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.campaignlinkedins.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForCampaignLinkedin(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((CampaignLinkedinDto)campaignlinkedinEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = campaignlinkedinEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = campaignlinkedinEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("LinkedinSetup", Name = "LinkedinSetup")]
        public string LinkedinSetup(string source, string campaignId, string companyId, string baseUrl, string type)
        {
            _baseUrl = baseUrl;
            _campaignID = campaignId;
            _companyID = companyId;
            _type = type;
            var url = _campaignlinkedinService.LinkedinSetup(source, type);
            return url;
        }

        [HttpGet("LinkedInCallbackWithCode", Name = "LinkedInCallbackWithCode")]
        [AllowAnonymous]
        public async Task<ActionResult> LinkedInCallbackWithCode(string code)
        {
            var redirectUrl = string.Empty;

            if (!string.IsNullOrEmpty(code))
            {
                if (_type == "LinkedIn")
                {

                    LinkedinToken res = await _campaignlinkedinService.GetAccessTokenUsingCode(code, "tab", _type);

                    CampaignLinkedinForCreation storeLinkedInDetail = new CampaignLinkedinForCreation();
                    storeLinkedInDetail.PageName = "";
                    storeLinkedInDetail.OrganizationalEntity = "";
                    storeLinkedInDetail.AccessToken = res.access_token;
                    storeLinkedInDetail.AccessTokenExpiresIn = res.expires_in;
                    storeLinkedInDetail.RefreshToken = res.refresh_token;
                    storeLinkedInDetail.RefreshTokenExpiresIn = res.refresh_token_expires_in;
                    storeLinkedInDetail.CampaignID = Guid.Parse(_campaignID);

                    var response = await _campaignlinkedinService.CreateEntityAsync<CampaignLinkedinDto, CampaignLinkedinForCreation>(storeLinkedInDetail);
                    await _campaignlinkedinService.SaveChangesAsync();


                }
                else if (_type == "LinkedIn_Ads")
                {

                    LinkedinToken res = await _campaignlinkedinService.GetAccessTokenUsingCode(code, "tab", _type);

                    LinkedinAdForCreation storeLinkedInDetail = new LinkedinAdForCreation();
                    storeLinkedInDetail.PageName = "";
                    storeLinkedInDetail.OrganizationalEntity = "";
                    storeLinkedInDetail.AccessToken = res.access_token;
                    storeLinkedInDetail.AccessTokenExpiresIn = res.expires_in;
                    storeLinkedInDetail.RefreshToken = res.refresh_token;
                    storeLinkedInDetail.RefreshTokenExpiresIn = res.refresh_token_expires_in;
                    storeLinkedInDetail.CampaignID = Guid.Parse(_campaignID);

                    var response = await _linkedinAdService.CreateEntityAsync<LinkedinAdDto, LinkedinAdForCreation>(storeLinkedInDetail);
                    await _linkedinAdService.SaveChangesAsync();

                }
            }

            redirectUrl = _baseUrl + "/company/" + _companyID + "/projects/" + _campaignID + "/update" + "?integration=" + _type;

            return Redirect(redirectUrl);
        }

        [HttpGet("LinkedInCallbackWithCodeForPopUp", Name = "LinkedInCallbackWithCodeForPopUp")]
        [AllowAnonymous]
        public async Task<ActionResult> LinkedInCallbackWithCodeForPopUp(string code)
        {
            var redirectUrl = string.Empty;

            var activePopup = 2;

            if (!string.IsNullOrEmpty(code))
            {

                if (_type == "LinkedIn")
                {

                    LinkedinToken res = await _campaignlinkedinService.GetAccessTokenUsingCode(code, "popup", _type);

                    CampaignLinkedinForCreation storeLinkedInDetail = new CampaignLinkedinForCreation();
                    storeLinkedInDetail.PageName = "";
                    storeLinkedInDetail.OrganizationalEntity = "";
                    storeLinkedInDetail.AccessToken = res.access_token;
                    storeLinkedInDetail.AccessTokenExpiresIn = res.expires_in;
                    storeLinkedInDetail.RefreshToken = res.refresh_token;
                    storeLinkedInDetail.RefreshTokenExpiresIn = res.refresh_token_expires_in;
                    storeLinkedInDetail.CampaignID = Guid.Parse(_campaignID);

                    var response = await _campaignlinkedinService.CreateEntityAsync<CampaignLinkedinDto, CampaignLinkedinForCreation>(storeLinkedInDetail);
                    await _campaignlinkedinService.SaveChangesAsync();
                   
                }
                else if (_type == "LinkedIn_Ads")
                {

                    LinkedinToken res = await _campaignlinkedinService.GetAccessTokenUsingCode(code, "popup", _type);

                    LinkedinAdForCreation storeLinkedInDetail = new LinkedinAdForCreation();
                    storeLinkedInDetail.PageName = "";
                    storeLinkedInDetail.OrganizationalEntity = "";
                    storeLinkedInDetail.AccessToken = res.access_token;
                    storeLinkedInDetail.AccessTokenExpiresIn = res.expires_in;
                    storeLinkedInDetail.RefreshToken = res.refresh_token;
                    storeLinkedInDetail.RefreshTokenExpiresIn = res.refresh_token_expires_in;
                    storeLinkedInDetail.CampaignID = Guid.Parse(_campaignID);

                    var response = await _linkedinAdService.CreateEntityAsync<LinkedinAdDto, LinkedinAdForCreation>(storeLinkedInDetail);
                    await _linkedinAdService.SaveChangesAsync();
                    
                }

                redirectUrl = _baseUrl + "/company/" + _companyID + "/home?integration=" + _type + "&activePopup=" + activePopup;

            }
            else
            {
                redirectUrl = _baseUrl + "/company/" + _companyID + "/home?integration=" + _type + "&activePopup=" + activePopup + "&iscodenull=" + true;
            }

            return Redirect(redirectUrl);
        }

        [HttpGet("GetLinkedinPageFollowers", Name = "GetLinkedinPageFollowers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public string GetLinkedinPageFollowers(string campaignId)
        {
            var followers = _campaignlinkedinService.GetLinkedinPageFollowers(campaignId);
            return followers;
        }

        [HttpGet("GetLinkedinTotalShareStatistics", Name = "GetLinkedinTotalShareStatistics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public string GetLinkedinTotalShareStatistics(string campaignId, string startDate, string endDate)
        {
            var TotalShareStatistics = _campaignlinkedinService.GetLinkedinTotalShareStatistics(campaignId, startDate, endDate);
            return TotalShareStatistics;
        }

        [HttpGet("GetLinkedinTotalOrganicPaidFollowerStatistics", Name = "GetLinkedinTotalOrganicPaidFollowerStatistics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public string GetLinkedinTotalOrganicPaidFollowerStatistics(string campaignId, string startDate, string endDate)
        {
            var TotalShareStatistics = _campaignlinkedinService.GetLinkedinTotalOrganicPaidFollowerStatistics(campaignId, startDate, endDate);
            return TotalShareStatistics;
        }

        [HttpGet("GetLinkedinEngagementData", Name = "GetLinkedinEngagementData")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public RootLinkedInDataObject GetLinkedinEngagement(string campaignId, string startDate, string endDate)
        {
            var TotalShareStatistics = _campaignlinkedinService.PrepareLinkedinEngagement(campaignId, startDate, endDate);
            return TotalShareStatistics;
        }

        [HttpGet("GetLinkedinDemographicData", Name = "GetLinkedinDemographicData")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public LinkedInDemographicChart GetLinkedinDemographicData(Guid campaignId)
        {
            return _campaignlinkedinService.GetLinkedinDemographicData(campaignId);
        }

        //GetLinkedinTotalDemographicStatistics

        [HttpGet("GetLinkedinTotalDemographicStatistics", Name = "GetLinkedinTotalDemographicStatistics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public string GetLinkedinTotalDemographicStatistics(string campaignId)
        {
            var TotalShareStatistics = _campaignlinkedinService.GetLinkedinTotalDemographicStatistics(campaignId);
            return TotalShareStatistics;
        }

        [HttpGet("CheckLinkedInIntegratedByCampaignId", Name = "CheckLinkedInIntegratedByCampaignId")]
        public async Task<bool> CheckLinkedInIntegratedByCampaignId(string campaignId)
        {
            var isExists = _campaignlinkedinService.GetAllEntities().Where(x => x.CampaignID.ToString() == campaignId).Any();
            if (isExists)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateCampaignLinkedin")]
        public async Task<IActionResult> UpdateCampaignLinkedin(Guid id, [FromBody] CampaignLinkedinForUpdate CampaignLinkedinForUpdate)
        {

            //if show not found
            if (!await _campaignlinkedinService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _campaignlinkedinService.UpdateEntityAsync(id, CampaignLinkedinForUpdate);

            //return the response.
            return NoContent();
        }

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("UpdateCampaignLinkedInByCampaignId", Name = "UpdateCampaignLinkedInByCampaignId")]
        public async Task<IActionResult> UpdateCampaignLinkedInByCampaignId([FromBody] CampaignLinkedinForUpdate CampaignLinkedinForUpdate)
        {
            try
            {
                var temp = await _campaignlinkedinService.UpdateBulkEntityAsync(x => new CampaignLinkedin
                {
                    PageName = CampaignLinkedinForUpdate.PageName,
                    OrganizationalEntity = CampaignLinkedinForUpdate.OrganizationalEntity,

                }, y => y.CampaignID == CampaignLinkedinForUpdate.CampaignID);

                //return the response.
                return Ok(temp);
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
        public async Task<IActionResult> PartiallyUpdateCampaignLinkedin(Guid id, [FromBody] JsonPatchDocument<CampaignLinkedinForUpdate> jsonPatchDocument)
        {
            CampaignLinkedinForUpdate dto = new CampaignLinkedinForUpdate();
            CampaignLinkedin campaignlinkedin = new CampaignLinkedin();

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
            Mapper.Map(dto, campaignlinkedin);

            //set the Id for the show model.
            campaignlinkedin.Id = id;

            //partially update the chnages to the db. 
            await _campaignlinkedinService.UpdatePartialEntityAsync(campaignlinkedin, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateCampaignLinkedin")]
        public async Task<ActionResult<CampaignLinkedinDto>> CreateCampaignLinkedin([FromBody] CampaignLinkedinForCreation campaignlinkedin)
        {
            //create a show in db.
            var campaignlinkedinToReturn = await _campaignlinkedinService.CreateEntityAsync<CampaignLinkedinDto, CampaignLinkedinForCreation>(campaignlinkedin);

            //return the show created response.
            return CreatedAtRoute("GetCampaignLinkedin", new { id = campaignlinkedinToReturn.Id }, campaignlinkedinToReturn);
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("LinkedInCallback", Name = "LinkedInCallback")]
        public async Task<LinkedinRoot> LinkedInCallback(Guid campaignId)
        {
            var linkedinPage = await _campaignlinkedinService.GetLinkedInPages(campaignId);
            return linkedinPage;
        }

        #endregion

        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteCampaignLinkedinById")]
        public async Task<IActionResult> DeleteCampaignLinkedinById(Guid id)
        {
            //if the campaignlinkedin exists
            if (await _campaignlinkedinService.ExistAsync(x => x.Id == id))
            {
                //delete the campaignlinkedin from the db.
                await _campaignlinkedinService.DeleteEntityAsync(id);
            }
            else
            {
                //if campaignlinkedin doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForCampaignLinkedin(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCampaignLinkedin", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCampaignLinkedin", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteCampaignLinkedinById", new { id = id }),
              "delete_campaignlinkedin",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateCampaignLinkedin", new { id = id }),
             "update_campaignlinkedin",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateCampaignLinkedin", new { }),
              "create_campaignlinkedin",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForCampaignLinkedins(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateCampaignLinkedinsResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateCampaignLinkedinsResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateCampaignLinkedinsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateCampaignLinkedinsResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredCampaignLinkedins",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredCampaignLinkedins",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredCampaignLinkedins",
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
