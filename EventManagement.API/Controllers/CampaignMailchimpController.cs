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
using RestSharp;
using IdentityServer4.AccessTokenValidation;
using static EventManagement.Dto.CampaignCallRailDto;

namespace EventManagement.API.Controllers
{
    /// <summary>
    /// CampaignMailchimp endpoint
    /// </summary>
    [Route("api/campaignmailchimps")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
    public class CampaignMailchimpController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly ICampaignMailchimpService _campaignmailchimpService;
        private ILogger<CampaignMailchimpController> _logger;
        private readonly IUrlHelper _urlHelper;
        static string _baseUrl;
        static string _campaignID;
        static string _companyID;

        #endregion


        #region CONSTRUCTOR

        public CampaignMailchimpController(ICampaignMailchimpService campaignmailchimpService,
            ILogger<CampaignMailchimpController> logger, IUrlHelper urlHelper)
        {
            _logger = logger;
            _campaignmailchimpService = campaignmailchimpService;
            _urlHelper = urlHelper;
        }

        #endregion


        #region HTTPGET

        [HttpGet(Name = "GetFilteredCampaignMailchimps")]
        [Produces("application/vnd.tourmanagement.campaignmailchimps.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CampaignMailchimpDto>>> GetFilteredCampaignMailchimps([FromQuery] FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_campaignmailchimpService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<CampaignMailchimpDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var campaignmailchimpsFromRepo = await _campaignmailchimpService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.campaignmailchimps.hateoas+json")
            {
                //create HATEOAS links for each show.
                campaignmailchimpsFromRepo.ForEach(campaignmailchimp =>
                {
                    var entityLinks = CreateLinksForCampaignMailchimp(campaignmailchimp.Id, filterOptionsModel.Fields);
                    campaignmailchimp.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = campaignmailchimpsFromRepo.TotalCount,
                    pageSize = campaignmailchimpsFromRepo.PageSize,
                    currentPage = campaignmailchimpsFromRepo.CurrentPage,
                    totalPages = campaignmailchimpsFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForCampaignMailchimps(filterOptionsModel, campaignmailchimpsFromRepo.HasNext, campaignmailchimpsFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = campaignmailchimpsFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = campaignmailchimpsFromRepo.HasPrevious ?
                    CreateCampaignMailchimpsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = campaignmailchimpsFromRepo.HasNext ?
                    CreateCampaignMailchimpsResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = campaignmailchimpsFromRepo.TotalCount,
                    pageSize = campaignmailchimpsFromRepo.PageSize,
                    currentPage = campaignmailchimpsFromRepo.CurrentPage,
                    totalPages = campaignmailchimpsFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(campaignmailchimpsFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.campaignmailchimps.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetCampaignMailchimp")]
        public async Task<ActionResult<CampaignMailchimp>> GetCampaignMailchimp(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object campaignmailchimpEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetCampaignMailchimp called");

                //then get the whole entity and map it to the Dto.
                campaignmailchimpEntity = Mapper.Map<CampaignMailchimpDto>(await _campaignmailchimpService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                campaignmailchimpEntity = await _campaignmailchimpService.GetPartialEntityAsync(id, fields);
            }

            //if campaignmailchimp not found.
            if (campaignmailchimpEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.campaignmailchimps.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForCampaignMailchimp(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((CampaignMailchimpDto)campaignmailchimpEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = campaignmailchimpEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = campaignmailchimpEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("maichimp-setup", Name = "maichimp-setup")]
        public string MailchimpSetup(string campaignId, string companyId, string baseUrl)
        {
            _baseUrl = baseUrl;
            _campaignID = campaignId;
            _companyID = companyId;

            var auth_url = _campaignmailchimpService.MailchimpAuth();

            return auth_url;
        }

        [HttpGet("mailchimp-callback", Name = "mailchimp-callback")]
        [AllowAnonymous]
        public async Task<CampaignMailchimpDto> MailchimpCallback(string code)
        {
            var redirectUrl = string.Empty;


            var accessToken = await _campaignmailchimpService.GetAccessTokenUsingCode(code);

            var res = await _campaignmailchimpService.GetMailchimpAccount(accessToken);

            if (res != null)
            {
                CampaignMailchimpForCreation campaignMailchimp = new CampaignMailchimpForCreation();
                campaignMailchimp.AccountId = res.login.login_email;
                campaignMailchimp.AccountName = res.accountname;
                campaignMailchimp.AccessToken = accessToken;
                campaignMailchimp.ApiEndpoint = res.api_endpoint;
                campaignMailchimp.CampaignID = Guid.Parse(_campaignID);

                var response = await _campaignmailchimpService.CreateEntityAsync<CampaignMailchimpDto, CampaignMailchimpForCreation>(campaignMailchimp);
                await _campaignmailchimpService.SaveChangesAsync();
                return response;

            }
            else
            {
                return null;
            }
        }

        [HttpGet("campaign-list-report", Name = "campaign-list-report")]
        public async Task<MCRootCampaignList> GetCampaignListReport(Guid campaignId)
        {
            var retVal = new MCRootCampaignList();

            try
            {
                retVal = await _campaignmailchimpService.GetCampaignListReport(campaignId);
                return retVal;
            }
            catch (Exception ex)
            {
                retVal.ErrorMsg = ex.Message;
                return retVal;
            }
        }

        [HttpGet("list-report", Name = "list-report")]
        public async Task<MCRootList> GetListReport(Guid campaignId)
        {
            var retVal = new MCRootList();

            try
            {
                retVal = await _campaignmailchimpService.GetListReport(campaignId);
                return retVal;
            }
            catch (Exception ex)
            {
                retVal.ErrorMsg = ex.Message;
                return retVal;
            }
        }


        [HttpGet("get-single-campaign-report", Name = "get-single-campaign-report")]
        public async Task<SingleCampaignReport> GetSingleCampaignReport(Guid campaignId, string mcCampaignId)
        {
            var retVal = new SingleCampaignReport();

            try
            {
                retVal = await _campaignmailchimpService.GetSingleCampaignReport(campaignId, mcCampaignId);
                return retVal;
            }
            catch (Exception ex)
            {
                retVal.ErrorMsg = ex.Message;
                return retVal;
            }
        }

        [HttpGet("get-single-list-report", Name = "get-single-list-report")]
        public async Task<RootSingleList> GetSingleListReport(Guid campaignId, string id)
        {
            var retVal = new RootSingleList();

            try
            {
                retVal = await _campaignmailchimpService.GetSingleListReport(campaignId, id);
                return retVal;
            }
            catch (Exception ex)
            {
                retVal.ErrorMsg = ex.Message;
                return retVal;
            }
        }


        [HttpGet("get-campaign-table", Name = "get-campaign-table")]
        public async Task<CampaignTableRoot> GetCampaignTable(Guid campaignId, string mcCampaignId,int offset,int count)
        {
            var retVal = new CampaignTableRoot();

            try
            {
                retVal = await _campaignmailchimpService.GetCampaignTable(campaignId, mcCampaignId, offset,count);
                return retVal;
            }
            catch (Exception ex)
            {
                retVal.ErrorMsg = ex.Message;
                return retVal;
            }
        }

        [HttpGet("get-member-table", Name = "get-member-table")]
        public async Task<MailChimpMemberRoot> GetMemberTable(Guid campaignId, string id, int offset, int count,string status)
        {
            var retVal = new MailChimpMemberRoot();

            try
            {
                retVal = await _campaignmailchimpService.GetMemberOfListApi(campaignId, id, offset, count,status);
                return retVal;
            }
            catch (Exception ex)
            {
                retVal.ErrorMsg = ex.Message;
                return retVal;
            }
        }

      

        [HttpGet("campaign-list", Name = "campaign-list")]
        public async Task<MCCampaignList> GetCampaignListt(Guid campaignId)
        {
            var retVal = new MCCampaignList();

            try
            {
                retVal = await _campaignmailchimpService.GetCampaignList(campaignId);
                return retVal;
            }
            catch (Exception ex)
            {
                retVal.ErrorMsg = ex.Message;
                return retVal;
            }
        }

        [HttpGet("mc-list", Name = "mc-list")]
        public async Task<McListRoot> GetMcList(Guid campaignId)
        {
            var retVal = new McListRoot();

            try
            {
                retVal = await _campaignmailchimpService.GetMcList(campaignId);
                return retVal;
            }
            catch (Exception ex)
            {
                retVal.ErrorMsg = ex.Message;
                return retVal;
            }
        }


        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateCampaignMailchimp")]
        public async Task<IActionResult> UpdateCampaignMailchimp(Guid id, [FromBody] CampaignMailchimpForUpdate CampaignMailchimpForUpdate)
        {

            //if show not found
            if (!await _campaignmailchimpService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _campaignmailchimpService.UpdateEntityAsync(id, CampaignMailchimpForUpdate);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateCampaignMailchimp(Guid id, [FromBody] JsonPatchDocument<CampaignMailchimpForUpdate> jsonPatchDocument)
        {
            CampaignMailchimpForUpdate dto = new CampaignMailchimpForUpdate();
            CampaignMailchimp campaignmailchimp = new CampaignMailchimp();

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
            Mapper.Map(dto, campaignmailchimp);

            //set the Id for the show model.
            campaignmailchimp.Id = id;

            //partially update the chnages to the db. 
            await _campaignmailchimpService.UpdatePartialEntityAsync(campaignmailchimp, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateCampaignMailchimp")]
        public async Task<ActionResult<CampaignMailchimpDto>> CreateCampaignMailchimp([FromBody] CampaignMailchimpForCreation campaignmailchimp)
        {
            //create a show in db.
            var campaignmailchimpToReturn = await _campaignmailchimpService.CreateEntityAsync<CampaignMailchimpDto, CampaignMailchimpForCreation>(campaignmailchimp);

            //return the show created response.
            return CreatedAtRoute("GetCampaignMailchimp", new { id = campaignmailchimpToReturn.Id }, campaignmailchimpToReturn);
        }

        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteCampaignMailchimpById")]
        public async Task<IActionResult> DeleteCampaignMailchimpById(Guid id)
        {
            //if the campaignmailchimp exists
            if (await _campaignmailchimpService.ExistAsync(x => x.Id == id))
            {
                //delete the campaignmailchimp from the db.
                await _campaignmailchimpService.DeleteEntityAsync(id);
            }
            else
            {
                //if campaignmailchimp doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForCampaignMailchimp(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCampaignMailchimp", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCampaignMailchimp", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteCampaignMailchimpById", new { id = id }),
              "delete_campaignmailchimp",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateCampaignMailchimp", new { id = id }),
             "update_campaignmailchimp",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateCampaignMailchimp", new { }),
              "create_campaignmailchimp",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForCampaignMailchimps(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateCampaignMailchimpsResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateCampaignMailchimpsResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateCampaignMailchimpsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateCampaignMailchimpsResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredCampaignMailchimps",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredCampaignMailchimps",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredCampaignMailchimps",
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
