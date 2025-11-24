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
using Microsoft.ApplicationInsights.AspNetCore.TelemetryInitializers;
using IdentityServer4.AccessTokenValidation;

namespace EventManagement.API.Controllers
{
    /// <summary>
    /// CampaignFacebookAds endpoint
    /// </summary>
    [Route("api/campaignfacebookadss")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
    public class CampaignFacebookAdsController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly ICampaignFacebookAdsService _campaignfacebookadsService;
        private ILogger<CampaignFacebookAdsController> _logger;
        private readonly IUrlHelper _urlHelper;

        #endregion


        #region CONSTRUCTOR

        public CampaignFacebookAdsController(ICampaignFacebookAdsService campaignfacebookadsService, ILogger<CampaignFacebookAdsController> logger, IUrlHelper urlHelper) 
        {
            _logger = logger;
            _campaignfacebookadsService = campaignfacebookadsService;
            _urlHelper = urlHelper;
        }

        #endregion


        #region HTTPGET

        [HttpGet(Name = "GetFilteredCampaignFacebookAdss")]
        [Produces("application/vnd.tourmanagement.campaignfacebookadss.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CampaignFacebookAdsDto>>> GetFilteredCampaignFacebookAdss([FromQuery]FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_campaignfacebookadsService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<CampaignFacebookAdsDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var campaignfacebookadssFromRepo = await _campaignfacebookadsService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.campaignfacebookadss.hateoas+json")
            {
                //create HATEOAS links for each show.
                campaignfacebookadssFromRepo.ForEach(campaignfacebookads =>
                {
                    var entityLinks = CreateLinksForCampaignFacebookAds(campaignfacebookads.Id, filterOptionsModel.Fields);
                    campaignfacebookads.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = campaignfacebookadssFromRepo.TotalCount,
                    pageSize = campaignfacebookadssFromRepo.PageSize,
                    currentPage = campaignfacebookadssFromRepo.CurrentPage,
                    totalPages = campaignfacebookadssFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForCampaignFacebookAdss(filterOptionsModel, campaignfacebookadssFromRepo.HasNext, campaignfacebookadssFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = campaignfacebookadssFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = campaignfacebookadssFromRepo.HasPrevious ?
                    CreateCampaignFacebookAdssResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = campaignfacebookadssFromRepo.HasNext ?
                    CreateCampaignFacebookAdssResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = campaignfacebookadssFromRepo.TotalCount,
                    pageSize = campaignfacebookadssFromRepo.PageSize,
                    currentPage = campaignfacebookadssFromRepo.CurrentPage,
                    totalPages = campaignfacebookadssFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(campaignfacebookadssFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.campaignfacebookadss.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetCampaignFacebookAds")]
        public async Task<ActionResult<CampaignFacebookAds>> GetCampaignFacebookAds(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object campaignfacebookadsEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetCampaignFacebookAds called");

                //then get the whole entity and map it to the Dto.
                campaignfacebookadsEntity = Mapper.Map<CampaignFacebookAdsDto>(await _campaignfacebookadsService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                campaignfacebookadsEntity = await _campaignfacebookadsService.GetPartialEntityAsync(id, fields);
            }

            //if campaignfacebookads not found.
            if (campaignfacebookadsEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.campaignfacebookadss.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForCampaignFacebookAds(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((CampaignFacebookAdsDto)campaignfacebookadsEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = campaignfacebookadsEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = campaignfacebookadsEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.campaignfacebookadss.hateoas+json", "application/json")]
        [HttpGet("get-fbads-campaigns", Name = "get-fbads-campaigns")]
        public async Task<ActionResult<FacebookGetData>> GetFbAdsData(Guid campignId,int type,DateTime startDate,DateTime endDate)
        {


            //startDate = new DateTime(2022, 10, 01);
            //endDate = new DateTime(2022, 10, 30);

            return Ok(await _campaignfacebookadsService.GetFbAdsData(campignId, type, startDate, endDate));
        }

        

        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateCampaignFacebookAds")]
        public async Task<IActionResult> UpdateCampaignFacebookAds(Guid id, [FromBody]CampaignFacebookAdsForUpdate CampaignFacebookAdsForUpdate)
        {

            //if show not found
            if (!await _campaignfacebookadsService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _campaignfacebookadsService.UpdateEntityAsync(id, CampaignFacebookAdsForUpdate);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateCampaignFacebookAds(Guid id, [FromBody] JsonPatchDocument<CampaignFacebookAdsForUpdate> jsonPatchDocument)
        {
            CampaignFacebookAdsForUpdate dto = new CampaignFacebookAdsForUpdate();
            CampaignFacebookAds campaignfacebookads = new CampaignFacebookAds();

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
            Mapper.Map(dto, campaignfacebookads);

            //set the Id for the show model.
            campaignfacebookads.Id = id;

            //partially update the chnages to the db. 
            await _campaignfacebookadsService.UpdatePartialEntityAsync(campaignfacebookads, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateCampaignFacebookAds")]
        public async Task<ActionResult<CampaignFacebookAdsDto>> CreateCampaignFacebookAds([FromBody]CampaignFacebookAdsForCreation campaignfacebookads)
        {
            //create a show in db.
            var campaignfacebookadsToReturn = await _campaignfacebookadsService.CreateEntityAsync<CampaignFacebookAdsDto, CampaignFacebookAdsForCreation>(campaignfacebookads);

            //return the show created response.
            return CreatedAtRoute("GetCampaignFacebookAds", new { id = campaignfacebookadsToReturn.Id }, campaignfacebookadsToReturn);
        }

        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteCampaignFacebookAdsById")]
        public async Task<IActionResult> DeleteCampaignFacebookAdsById(Guid id)
        {
            //if the campaignfacebookads exists
            if (await _campaignfacebookadsService.ExistAsync(x => x.Id == id))
            {
                //delete the campaignfacebookads from the db.
                await _campaignfacebookadsService.DeleteEntityAsync(id);
            }
            else
            {
                //if campaignfacebookads doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForCampaignFacebookAds(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCampaignFacebookAds", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCampaignFacebookAds", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteCampaignFacebookAdsById", new { id = id }),
              "delete_campaignfacebookads",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateCampaignFacebookAds", new { id = id }),
             "update_campaignfacebookads",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateCampaignFacebookAds", new { }),
              "create_campaignfacebookads",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForCampaignFacebookAdss(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateCampaignFacebookAdssResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateCampaignFacebookAdssResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateCampaignFacebookAdssResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateCampaignFacebookAdssResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredCampaignFacebookAdss",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredCampaignFacebookAdss",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredCampaignFacebookAdss",
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
