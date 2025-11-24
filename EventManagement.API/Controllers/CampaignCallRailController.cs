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
using static EventManagement.Dto.CampaignCallRailDto;
using IdentityServer4.AccessTokenValidation;

namespace EventManagement.API.Controllers
{
    /// <summary>
    /// CampaignCallRail endpoint
    /// </summary>
    [Route("api/campaigncallrails")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]

    public class CampaignCallRailController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly ICampaignCallRailService _campaigncallrailService;
        private ILogger<CampaignCallRailController> _logger;
        private readonly IUrlHelper _urlHelper;

        #endregion


        #region CONSTRUCTOR

        public CampaignCallRailController(ICampaignCallRailService campaigncallrailService, ILogger<CampaignCallRailController> logger, IUrlHelper urlHelper) 
        {
            _logger = logger;
            _campaigncallrailService = campaigncallrailService;
            _urlHelper = urlHelper;
        }

        #endregion


        #region HTTPGET

        [HttpGet(Name = "GetFilteredCampaignCallRails")]
        [Produces("application/vnd.tourmanagement.campaigncallrails.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CampaignCallRailDto>>> GetFilteredCampaignCallRails([FromQuery]FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_campaigncallrailService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<CampaignCallRailDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var campaigncallrailsFromRepo = await _campaigncallrailService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.campaigncallrails.hateoas+json")
            {
                //create HATEOAS links for each show.
                campaigncallrailsFromRepo.ForEach(campaigncallrail =>
                {
                    var entityLinks = CreateLinksForCampaignCallRail(campaigncallrail.Id, filterOptionsModel.Fields);
                    campaigncallrail.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = campaigncallrailsFromRepo.TotalCount,
                    pageSize = campaigncallrailsFromRepo.PageSize,
                    currentPage = campaigncallrailsFromRepo.CurrentPage,
                    totalPages = campaigncallrailsFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForCampaignCallRails(filterOptionsModel, campaigncallrailsFromRepo.HasNext, campaigncallrailsFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = campaigncallrailsFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = campaigncallrailsFromRepo.HasPrevious ?
                    CreateCampaignCallRailsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = campaigncallrailsFromRepo.HasNext ?
                    CreateCampaignCallRailsResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = campaigncallrailsFromRepo.TotalCount,
                    pageSize = campaigncallrailsFromRepo.PageSize,
                    currentPage = campaigncallrailsFromRepo.CurrentPage,
                    totalPages = campaigncallrailsFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(campaigncallrailsFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.campaigncallrails.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetCampaignCallRail")]
        public async Task<ActionResult<CampaignCallRail>> GetCampaignCallRail(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object campaigncallrailEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetCampaignCallRail called");

                //then get the whole entity and map it to the Dto.
                campaigncallrailEntity = Mapper.Map<CampaignCallRailDto>(await _campaigncallrailService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                campaigncallrailEntity = await _campaigncallrailService.GetPartialEntityAsync(id, fields);
            }

            //if campaigncallrail not found.
            if (campaigncallrailEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.campaigncallrails.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForCampaignCallRail(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((CampaignCallRailDto)campaigncallrailEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = campaigncallrailEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = campaigncallrailEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }


        [HttpGet("call-rail-setup", Name = "call-rail-setup")]
        public async Task<CampaignCallRailDto> ValidateShop(string apiKey,Guid campaignId)
        {            
            var retVal = new CampaignCallRailDto();

            try
            {
                retVal = await _campaigncallrailService.VaidateApiKeyAndSetup(apiKey, campaignId);
                return  retVal;
            }
            catch (Exception ex)
            {
              
                return retVal;
            }
        }

        [HttpGet("call-rail-account-list", Name = "call-rail-account-list")]
        public async Task<AccountResponse> GetAccountList(Guid campaignId)
        {
            var retVal = new AccountResponse();

            try
            {
                retVal = await _campaigncallrailService.GetAccountList(campaignId);
                return retVal;
            }
            catch (Exception ex)
            {

                return retVal;
            }
        }

        [HttpGet("get-call-rail-report", Name = "get-call-rail-report")]
        public async Task<CallRailReportData> GetCallRailReport([FromQuery] CallReportDTO callReportDTO )
        {
            var retVal = new CallRailReportData();

            try
            {
                retVal = await _campaigncallrailService.GetCallRailReport(callReportDTO);
                return retVal;
            }
            catch (Exception ex)
            {
                retVal.ErrorMessage = ex.Message;
                retVal.StatusCodes = System.Net.HttpStatusCode.BadRequest;
                return retVal;
            }
        }

        [HttpGet("get-call-rail-table", Name = "get-call-rail-table")]
        public async Task<CallResponse> GetCallRailTableReport(Guid campaignId, string startTime, string endTime, int pageNumber)
        {
            var retVal = new CallResponse();

            try
            {
                retVal = await _campaigncallrailService.GetCallRailTableReport(campaignId, startTime, endTime, pageNumber);
                return retVal;
            }
            catch (Exception ex)
            {
                retVal.ErrorMessage = ex.Message;
                retVal.StatusCodes = System.Net.HttpStatusCode.BadRequest;
                return retVal;
            }
        }


        [HttpGet("get-call-recording", Name = "get-call-recording")]
        public async Task<Recording> GetCallRecording(Guid campaignId, string url)
        {
                var retVal = await _campaigncallrailService.GetRecording(campaignId, url);
                return retVal;                       
        }

        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateCampaignCallRail")]
        public async Task<IActionResult> UpdateCampaignCallRail(Guid id, [FromBody]CampaignCallRailForUpdate CampaignCallRailForUpdate)
        {

            //if show not found
            if (!await _campaigncallrailService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _campaigncallrailService.UpdateEntityAsync(id, CampaignCallRailForUpdate);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateCampaignCallRail(Guid id, [FromBody] JsonPatchDocument<CampaignCallRailForUpdate> jsonPatchDocument)
        {
            CampaignCallRailForUpdate dto = new CampaignCallRailForUpdate();
            CampaignCallRail campaigncallrail = new CampaignCallRail();

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
            Mapper.Map(dto, campaigncallrail);

            //set the Id for the show model.
            campaigncallrail.Id = id;

            //partially update the chnages to the db. 
            await _campaigncallrailService.UpdatePartialEntityAsync(campaigncallrail, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateCampaignCallRail")]
        public async Task<ActionResult<CampaignCallRailDto>> CreateCampaignCallRail([FromBody]CampaignCallRailForCreation campaigncallrail)
        {
            //create a show in db.
            var campaigncallrailToReturn = await _campaigncallrailService.CreateEntityAsync<CampaignCallRailDto, CampaignCallRailForCreation>(campaigncallrail);

            //return the show created response.
            return CreatedAtRoute("GetCampaignCallRail", new { id = campaigncallrailToReturn.Id }, campaigncallrailToReturn);
        }

        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteCampaignCallRailById")]
        public async Task<IActionResult> DeleteCampaignCallRailById(Guid id)
        {
            //if the campaigncallrail exists
            if (await _campaigncallrailService.ExistAsync(x => x.Id == id))
            {
                //delete the campaigncallrail from the db.
                await _campaigncallrailService.DeleteEntityAsync(id);
            }
            else
            {
                //if campaigncallrail doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForCampaignCallRail(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCampaignCallRail", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCampaignCallRail", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteCampaignCallRailById", new { id = id }),
              "delete_campaigncallrail",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateCampaignCallRail", new { id = id }),
             "update_campaigncallrail",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateCampaignCallRail", new { }),
              "create_campaigncallrail",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForCampaignCallRails(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateCampaignCallRailsResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateCampaignCallRailsResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateCampaignCallRailsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateCampaignCallRailsResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredCampaignCallRails",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredCampaignCallRails",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredCampaignCallRails",
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
