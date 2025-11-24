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
    /// RankingGraph endpoint
    /// </summary>
    [Route("api/rankinggraphs")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]

    public class RankingGraphController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly IRankingGraphService _rankinggraphService;
        private ILogger<RankingGraphController> _logger;
        private readonly IUrlHelper _urlHelper;

        #endregion


        #region CONSTRUCTOR

        public RankingGraphController(IRankingGraphService rankinggraphService, ILogger<RankingGraphController> logger, IUrlHelper urlHelper) 
        {
            _logger = logger;
            _rankinggraphService = rankinggraphService;
            _urlHelper = urlHelper;
        }

        #endregion


        #region HTTPGET

        [HttpGet(Name = "GetFilteredRankingGraphs")]
        [Produces("application/vnd.tourmanagement.rankinggraphs.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<RankingGraphDto>>> GetFilteredRankingGraphs([FromQuery]FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_rankinggraphService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<RankingGraphDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var rankinggraphsFromRepo = await _rankinggraphService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.rankinggraphs.hateoas+json")
            {
                //create HATEOAS links for each show.
                rankinggraphsFromRepo.ForEach(rankinggraph =>
                {
                    var entityLinks = CreateLinksForRankingGraph(rankinggraph.Id, filterOptionsModel.Fields);
                    rankinggraph.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = rankinggraphsFromRepo.TotalCount,
                    pageSize = rankinggraphsFromRepo.PageSize,
                    currentPage = rankinggraphsFromRepo.CurrentPage,
                    totalPages = rankinggraphsFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForRankingGraphs(filterOptionsModel, rankinggraphsFromRepo.HasNext, rankinggraphsFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = rankinggraphsFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = rankinggraphsFromRepo.HasPrevious ?
                    CreateRankingGraphsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = rankinggraphsFromRepo.HasNext ?
                    CreateRankingGraphsResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = rankinggraphsFromRepo.TotalCount,
                    pageSize = rankinggraphsFromRepo.PageSize,
                    currentPage = rankinggraphsFromRepo.CurrentPage,
                    totalPages = rankinggraphsFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(rankinggraphsFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.rankinggraphs.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetRankingGraph")]
        public async Task<ActionResult<RankingGraph>> GetRankingGraph(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object rankinggraphEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetRankingGraph called");

                //then get the whole entity and map it to the Dto.
                rankinggraphEntity = Mapper.Map<RankingGraphDto>(await _rankinggraphService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                rankinggraphEntity = await _rankinggraphService.GetPartialEntityAsync(id, fields);
            }

            //if rankinggraph not found.
            if (rankinggraphEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.rankinggraphs.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForRankingGraph(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((RankingGraphDto)rankinggraphEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = rankinggraphEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = rankinggraphEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }

        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateRankingGraph")]
        public async Task<IActionResult> UpdateRankingGraph(Guid id, [FromBody]RankingGraphForUpdate RankingGraphForUpdate)
        {

            //if show not found
            if (!await _rankinggraphService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _rankinggraphService.UpdateEntityAsync(id, RankingGraphForUpdate);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateRankingGraph(Guid id, [FromBody] JsonPatchDocument<RankingGraphForUpdate> jsonPatchDocument)
        {
            RankingGraphForUpdate dto = new RankingGraphForUpdate();
            RankingGraph rankinggraph = new RankingGraph();

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
            Mapper.Map(dto, rankinggraph);

            //set the Id for the show model.
            rankinggraph.Id = id;

            //partially update the chnages to the db. 
            await _rankinggraphService.UpdatePartialEntityAsync(rankinggraph, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateRankingGraph")]
        public async Task<ActionResult<RankingGraphDto>> CreateRankingGraph([FromBody]RankingGraphForCreation rankinggraph)
        {
            //create a show in db.
            var rankinggraphToReturn = await _rankinggraphService.CreateEntityAsync<RankingGraphDto, RankingGraphForCreation>(rankinggraph);

            //return the show created response.
            return CreatedAtRoute("GetRankingGraph", new { id = rankinggraphToReturn.Id }, rankinggraphToReturn);
        }

        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteRankingGraphById")]
        public async Task<IActionResult> DeleteRankingGraphById(Guid id)
        {
            //if the rankinggraph exists
            if (await _rankinggraphService.ExistAsync(x => x.Id == id))
            {
                //delete the rankinggraph from the db.
                await _rankinggraphService.DeleteEntityAsync(id);
            }
            else
            {
                //if rankinggraph doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForRankingGraph(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetRankingGraph", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetRankingGraph", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteRankingGraphById", new { id = id }),
              "delete_rankinggraph",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateRankingGraph", new { id = id }),
             "update_rankinggraph",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateRankingGraph", new { }),
              "create_rankinggraph",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForRankingGraphs(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateRankingGraphsResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateRankingGraphsResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateRankingGraphsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateRankingGraphsResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredRankingGraphs",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredRankingGraphs",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredRankingGraphs",
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
