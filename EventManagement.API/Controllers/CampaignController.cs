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

namespace EventManagement.API.Controllers
{
    /// <summary>
    /// Campaign endpoint
    /// </summary>
    [Route("api/campaigns")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
    public class CampaignController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly ICampaignService _campaignService;
        private ILogger<CampaignController> _logger;
        private readonly IUrlHelper _urlHelper;
        private readonly ICampaignUserService _campaignUserService;

        #endregion


        #region CONSTRUCTOR

        public CampaignController(ICampaignUserService campaignUserService, ICampaignService campaignService, ILogger<CampaignController> logger, IUrlHelper urlHelper)
        {
            _logger = logger;
            _campaignService = campaignService;
            _urlHelper = urlHelper;
            _campaignUserService = campaignUserService;
        }

        #endregion


        #region HTTPGET

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("GetCampaignIntegrationStatus", Name = "GetCampaignIntegrationStatus")]
        public async Task<ActionResult<List<CampaignIntegraionDto>>> GetCampaignIntegrationStatus([FromQuery] Guid campaignId)
        {
            var returnData = await _campaignService.GetCampaignIntegrationStatus(campaignId);

            return Ok(returnData);
        }

        [HttpGet(Name = "GetFilteredCampaigns")]
        [Produces("application/vnd.tourmanagement.campaigns.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CampaignDto>>> GetFilteredCampaigns([FromQuery] FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_campaignService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<CampaignDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var campaignsFromRepo = await _campaignService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.campaigns.hateoas+json")
            {
                //create HATEOAS links for each show.
                campaignsFromRepo.ForEach(campaign =>
                {
                    var entityLinks = CreateLinksForCampaign(campaign.Id, filterOptionsModel.Fields);
                    campaign.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = campaignsFromRepo.TotalCount,
                    pageSize = campaignsFromRepo.PageSize,
                    currentPage = campaignsFromRepo.CurrentPage,
                    totalPages = campaignsFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForCampaigns(filterOptionsModel, campaignsFromRepo.HasNext, campaignsFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = campaignsFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = campaignsFromRepo.HasPrevious ?
                    CreateCampaignsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = campaignsFromRepo.HasNext ?
                    CreateCampaignsResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = campaignsFromRepo.TotalCount,
                    pageSize = campaignsFromRepo.PageSize,
                    currentPage = campaignsFromRepo.CurrentPage,
                    totalPages = campaignsFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(campaignsFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.campaigns.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetCampaign")]
        public async Task<ActionResult<Campaign>> GetCampaign(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object campaignEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetCampaign called");

                //then get the whole entity and map it to the Dto.
                campaignEntity = Mapper.Map<CampaignDto>(await _campaignService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                campaignEntity = await _campaignService.GetPartialEntityAsync(id, fields);
            }

            //if campaign not found.
            if (campaignEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.campaigns.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForCampaign(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((CampaignDto)campaignEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = campaignEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = campaignEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("GetCampaignByUserId", Name = "GetCampaignByUserId")]
        public async Task<ActionResult<List<CampaignDto>>> GetCampaignByUserId([FromQuery] string userId)
        {
            var CampaignInfo = _campaignService.GetCampaignByUserId(userId);

            return Ok(CampaignInfo);
        }


        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("GetUpdateDashboard", Name = "GetUpdateDashboard")]
        [AllowAnonymous]
        public async Task<string> GetUpdateDashboard()
        {
            var response = await _campaignService.GetUpdateDashboard();
            return response;
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("UpdateDataAfterIntegration", Name = "UpdateDataAfterIntegration")]
        public async Task<bool> UpdateDashboardDataAfterIntegration(Guid campaignId)
        {
            return await _campaignService.UpdateDashboardDataAfterIntegration(campaignId);
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("GetDashboardData", Name = "GetDashboardData")]
        public async Task<List<Campaign>> GetDashboardDataFromFrontEnd(List<Campaign> campaigns, string startDate, string endDate)
        {
            var response = await _campaignService.GetDashboardData(campaigns, startDate, endDate);
            return response;
        }

        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateCampaign")]
        public async Task<IActionResult> UpdateCampaign(Guid id, [FromBody] CampaignForUpdate CampaignForUpdate)
        {

            //if show not found
            if (!await _campaignService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _campaignService.UpdateEntityAsync(id, CampaignForUpdate);

            //return the response.
            return Ok(CampaignForUpdate);
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateCampaign(Guid id, [FromBody] JsonPatchDocument<CampaignForUpdate> jsonPatchDocument)
        {
            CampaignForUpdate dto = new CampaignForUpdate();
            Campaign campaign = new Campaign();

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
            Mapper.Map(dto, campaign);

            //set the Id for the show model.
            campaign.Id = id;

            //partially update the chnages to the db. 
            await _campaignService.UpdatePartialEntityAsync(campaign, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateCampaign")]
        public async Task<ActionResult<CampaignDto>> CreateCampaign([FromBody] CampaignForCreation campaign)
        {
            //create a show in db.
            var campaignToReturn = await _campaignService.CreateEntityAsync<CampaignDto, CampaignForCreation>(campaign);

            var campaignUserData = _campaignService.AddUserToCampaign(campaign.UserId, campaignToReturn.Id.ToString(), campaignToReturn.CompanyID.ToString()).Result;

            //return the show created response.
            return Ok(campaignToReturn); // CreatedAtRoute("GetCampaign", new { id = campaignToReturn.Id }, campaignToReturn);
        }



        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("DeleteCampaignsFromAppsumo", Name = "DeleteCampaignsFromAppsumo")]
        public async Task<IActionResult> DeleteCampaignsFromAppsumo(List<Guid> ids)
        {
            var response = await _campaignService.DeleteCampaignsFromAppsumo(ids);
            //return the response.
            return Ok(response);
        }

        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteCampaignById")]
        public async Task<IActionResult> DeleteCampaignById(Guid id)
        {
            //if the campaign exists
            if (await _campaignService.ExistAsync(x => x.Id == id))
            {
                //delete the campaign from the db.
                await _campaignService.DeleteEntityAsync(id);
            }
            else
            {
                //if campaign doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }


        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForCampaign(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCampaign", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCampaign", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteCampaignById", new { id = id }),
              "delete_campaign",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateCampaign", new { id = id }),
             "update_campaign",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateCampaign", new { }),
              "create_campaign",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForCampaigns(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateCampaignsResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateCampaignsResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateCampaignsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateCampaignsResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredCampaigns",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredCampaigns",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredCampaigns",
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
