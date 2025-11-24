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
    /// TrafficSummary endpoint
    /// </summary>
    [Route("api/trafficsummarys")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]

    public class TrafficSummaryController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly ITrafficSummaryService _trafficsummaryService;
        private ILogger<TrafficSummaryController> _logger;
        private readonly IUrlHelper _urlHelper;

        #endregion


        #region CONSTRUCTOR

        public TrafficSummaryController(ITrafficSummaryService trafficsummaryService, ILogger<TrafficSummaryController> logger, IUrlHelper urlHelper) 
        {
            _logger = logger;
            _trafficsummaryService = trafficsummaryService;
            _urlHelper = urlHelper;
        }

        #endregion


        #region HTTPGET

        [HttpGet(Name = "GetFilteredTrafficSummarys")]
        [Produces("application/vnd.tourmanagement.trafficsummarys.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<TrafficSummaryDto>>> GetFilteredTrafficSummarys([FromQuery]FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_trafficsummaryService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<TrafficSummaryDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var trafficsummarysFromRepo = await _trafficsummaryService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.trafficsummarys.hateoas+json")
            {
                //create HATEOAS links for each show.
                trafficsummarysFromRepo.ForEach(trafficsummary =>
                {
                    var entityLinks = CreateLinksForTrafficSummary(trafficsummary.Id, filterOptionsModel.Fields);
                    trafficsummary.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = trafficsummarysFromRepo.TotalCount,
                    pageSize = trafficsummarysFromRepo.PageSize,
                    currentPage = trafficsummarysFromRepo.CurrentPage,
                    totalPages = trafficsummarysFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForTrafficSummarys(filterOptionsModel, trafficsummarysFromRepo.HasNext, trafficsummarysFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = trafficsummarysFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = trafficsummarysFromRepo.HasPrevious ?
                    CreateTrafficSummarysResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = trafficsummarysFromRepo.HasNext ?
                    CreateTrafficSummarysResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = trafficsummarysFromRepo.TotalCount,
                    pageSize = trafficsummarysFromRepo.PageSize,
                    currentPage = trafficsummarysFromRepo.CurrentPage,
                    totalPages = trafficsummarysFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(trafficsummarysFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.trafficsummarys.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetTrafficSummary")]
        public async Task<ActionResult<TrafficSummary>> GetTrafficSummary(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object trafficsummaryEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetTrafficSummary called");

                //then get the whole entity and map it to the Dto.
                trafficsummaryEntity = Mapper.Map<TrafficSummaryDto>(await _trafficsummaryService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                trafficsummaryEntity = await _trafficsummaryService.GetPartialEntityAsync(id, fields);
            }

            //if trafficsummary not found.
            if (trafficsummaryEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.trafficsummarys.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForTrafficSummary(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((TrafficSummaryDto)trafficsummaryEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = trafficsummaryEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = trafficsummaryEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }

        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateTrafficSummary")]
        public async Task<IActionResult> UpdateTrafficSummary(Guid id, [FromBody]TrafficSummaryForUpdate TrafficSummaryForUpdate)
        {

            //if show not found
            if (!await _trafficsummaryService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _trafficsummaryService.UpdateEntityAsync(id, TrafficSummaryForUpdate);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateTrafficSummary(Guid id, [FromBody] JsonPatchDocument<TrafficSummaryForUpdate> jsonPatchDocument)
        {
            TrafficSummaryForUpdate dto = new TrafficSummaryForUpdate();
            TrafficSummary trafficsummary = new TrafficSummary();

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
            Mapper.Map(dto, trafficsummary);

            //set the Id for the show model.
            trafficsummary.Id = id;

            //partially update the chnages to the db. 
            await _trafficsummaryService.UpdatePartialEntityAsync(trafficsummary, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateTrafficSummary")]
        public async Task<ActionResult<TrafficSummaryDto>> CreateTrafficSummary([FromBody]TrafficSummaryForCreation trafficsummary)
        {
            //create a show in db.
            var trafficsummaryToReturn = await _trafficsummaryService.CreateEntityAsync<TrafficSummaryDto, TrafficSummaryForCreation>(trafficsummary);

            //return the show created response.
            return CreatedAtRoute("GetTrafficSummary", new { id = trafficsummaryToReturn.Id }, trafficsummaryToReturn);
        }

        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteTrafficSummaryById")]
        public async Task<IActionResult> DeleteTrafficSummaryById(Guid id)
        {
            //if the trafficsummary exists
            if (await _trafficsummaryService.ExistAsync(x => x.Id == id))
            {
                //delete the trafficsummary from the db.
                await _trafficsummaryService.DeleteEntityAsync(id);
            }
            else
            {
                //if trafficsummary doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForTrafficSummary(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetTrafficSummary", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetTrafficSummary", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteTrafficSummaryById", new { id = id }),
              "delete_trafficsummary",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateTrafficSummary", new { id = id }),
             "update_trafficsummary",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateTrafficSummary", new { }),
              "create_trafficsummary",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForTrafficSummarys(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateTrafficSummarysResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateTrafficSummarysResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateTrafficSummarysResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateTrafficSummarysResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredTrafficSummarys",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredTrafficSummarys",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredTrafficSummarys",
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
