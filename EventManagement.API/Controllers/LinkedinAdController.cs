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
    /// LinkedinAd endpoint
    /// </summary>
    [Route("api/linkedinads")]
    [Produces("application/json")]
    [ApiController]
    //[Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
    public class LinkedinAdController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly ILinkedinAdService _linkedinadService;
        private ILogger<LinkedinAdController> _logger;
        private readonly IUrlHelper _urlHelper;

        #endregion


        #region CONSTRUCTOR

        public LinkedinAdController(ILinkedinAdService linkedinadService, ILogger<LinkedinAdController> logger, IUrlHelper urlHelper) 
        {
            _logger = logger;
            _linkedinadService = linkedinadService;
            _urlHelper = urlHelper;
        }

        #endregion


        #region HTTPGET

        [HttpGet(Name = "GetFilteredLinkedinAds")]
        [Produces("application/vnd.tourmanagement.linkedinads.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<LinkedinAdDto>>> GetFilteredLinkedinAds([FromQuery]FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_linkedinadService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<LinkedinAdDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var linkedinadsFromRepo = await _linkedinadService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.linkedinads.hateoas+json")
            {
                //create HATEOAS links for each show.
                linkedinadsFromRepo.ForEach(linkedinad =>
                {
                    var entityLinks = CreateLinksForLinkedinAd(linkedinad.Id, filterOptionsModel.Fields);
                    linkedinad.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = linkedinadsFromRepo.TotalCount,
                    pageSize = linkedinadsFromRepo.PageSize,
                    currentPage = linkedinadsFromRepo.CurrentPage,
                    totalPages = linkedinadsFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForLinkedinAds(filterOptionsModel, linkedinadsFromRepo.HasNext, linkedinadsFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = linkedinadsFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = linkedinadsFromRepo.HasPrevious ?
                    CreateLinkedinAdsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = linkedinadsFromRepo.HasNext ?
                    CreateLinkedinAdsResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = linkedinadsFromRepo.TotalCount,
                    pageSize = linkedinadsFromRepo.PageSize,
                    currentPage = linkedinadsFromRepo.CurrentPage,
                    totalPages = linkedinadsFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(linkedinadsFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.linkedinads.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetLinkedinAd")]
        public async Task<ActionResult<LinkedinAd>> GetLinkedinAd(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object linkedinadEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetLinkedinAd called");

                //then get the whole entity and map it to the Dto.
                linkedinadEntity = Mapper.Map<LinkedinAdDto>(await _linkedinadService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                linkedinadEntity = await _linkedinadService.GetPartialEntityAsync(id, fields);
            }

            //if linkedinad not found.
            if (linkedinadEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.linkedinads.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForLinkedinAd(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((LinkedinAdDto)linkedinadEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = linkedinadEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = linkedinadEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }


        [HttpGet("GetLinkedinAdsAnalytics", Name = "GetLinkedinAdsAnalytics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<AnalyticsRoot> GetLinkedinTotalDemographicStatistics(string campaignId,string type,string startTime,string endTime)
        {
            var data = await _linkedinadService.GetPreparedLinkedinAdData(campaignId,type, startTime,endTime);
            return data;
        }


        [HttpGet("GetLinkedinAdsDemographic", Name = "GetLinkedinAdsDemographic")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<DempgraphicRoot> GetLinkedinAdsDemographic(string campaignId, string type, string startTime, string endTime)
        {
            var data = await _linkedinadService.GetLinkedinAdsDemographic(campaignId, type, startTime, endTime);
            return data;
        }

        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateLinkedinAd")]
        public async Task<IActionResult> UpdateLinkedinAd(Guid id, [FromBody]LinkedinAdForUpdate LinkedinAdForUpdate)
        {

            //if show not found
            if (!await _linkedinadService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _linkedinadService.UpdateEntityAsync(id, LinkedinAdForUpdate);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateLinkedinAd(Guid id, [FromBody] JsonPatchDocument<LinkedinAdForUpdate> jsonPatchDocument)
        {
            LinkedinAdForUpdate dto = new LinkedinAdForUpdate();
            LinkedinAd linkedinad = new LinkedinAd();

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
            Mapper.Map(dto, linkedinad);

            //set the Id for the show model.
            linkedinad.Id = id;

            //partially update the chnages to the db. 
            await _linkedinadService.UpdatePartialEntityAsync(linkedinad, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateLinkedinAd")]
        public async Task<ActionResult<LinkedinAdDto>> CreateLinkedinAd([FromBody]LinkedinAdForCreation linkedinad)
        {
            //create a show in db.
            var linkedinadToReturn = await _linkedinadService.CreateEntityAsync<LinkedinAdDto, LinkedinAdForCreation>(linkedinad);

            //return the show created response.
            return CreatedAtRoute("GetLinkedinAd", new { id = linkedinadToReturn.Id }, linkedinadToReturn);
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("LinkedAdsInCallback", Name = "LinkedAdsInCallback")]
        public async Task<LinkedinAdRoot> LinkedInCallback(Guid campaignId)
        {
            var linkedinPage = await _linkedinadService.GetLinkedInPages(campaignId);
            return linkedinPage;
        }
        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteLinkedinAdById")]
        public async Task<IActionResult> DeleteLinkedinAdById(Guid id)
        {
            //if the linkedinad exists
            if (await _linkedinadService.ExistAsync(x => x.Id == id))
            {
                //delete the linkedinad from the db.
                await _linkedinadService.DeleteEntityAsync(id);
            }
            else
            {
                //if linkedinad doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForLinkedinAd(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetLinkedinAd", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetLinkedinAd", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteLinkedinAdById", new { id = id }),
              "delete_linkedinad",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateLinkedinAd", new { id = id }),
             "update_linkedinad",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateLinkedinAd", new { }),
              "create_linkedinad",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForLinkedinAds(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateLinkedinAdsResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateLinkedinAdsResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateLinkedinAdsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateLinkedinAdsResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredLinkedinAds",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredLinkedinAds",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredLinkedinAds",
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
