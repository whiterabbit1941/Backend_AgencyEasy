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
    /// GscSummary endpoint
    /// </summary>
    [Route("api/gscsummarys")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]

    public class GscSummaryController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly IGscSummaryService _gscsummaryService;
        private ILogger<GscSummaryController> _logger;
        private readonly IUrlHelper _urlHelper;

        #endregion


        #region CONSTRUCTOR

        public GscSummaryController(IGscSummaryService gscsummaryService, ILogger<GscSummaryController> logger, IUrlHelper urlHelper) 
        {
            _logger = logger;
            _gscsummaryService = gscsummaryService;
            _urlHelper = urlHelper;
        }

        #endregion


        #region HTTPGET

        [HttpGet(Name = "GetFilteredGscSummarys")]
        [Produces("application/vnd.tourmanagement.gscsummarys.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<GscSummaryDto>>> GetFilteredGscSummarys([FromQuery]FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_gscsummaryService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<GscSummaryDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var gscsummarysFromRepo = await _gscsummaryService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.gscsummarys.hateoas+json")
            {
                //create HATEOAS links for each show.
                gscsummarysFromRepo.ForEach(gscsummary =>
                {
                    var entityLinks = CreateLinksForGscSummary(gscsummary.Id, filterOptionsModel.Fields);
                    gscsummary.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = gscsummarysFromRepo.TotalCount,
                    pageSize = gscsummarysFromRepo.PageSize,
                    currentPage = gscsummarysFromRepo.CurrentPage,
                    totalPages = gscsummarysFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForGscSummarys(filterOptionsModel, gscsummarysFromRepo.HasNext, gscsummarysFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = gscsummarysFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = gscsummarysFromRepo.HasPrevious ?
                    CreateGscSummarysResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = gscsummarysFromRepo.HasNext ?
                    CreateGscSummarysResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = gscsummarysFromRepo.TotalCount,
                    pageSize = gscsummarysFromRepo.PageSize,
                    currentPage = gscsummarysFromRepo.CurrentPage,
                    totalPages = gscsummarysFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(gscsummarysFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.gscsummarys.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetGscSummary")]
        public async Task<ActionResult<GscSummary>> GetGscSummary(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object gscsummaryEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetGscSummary called");

                //then get the whole entity and map it to the Dto.
                gscsummaryEntity = Mapper.Map<GscSummaryDto>(await _gscsummaryService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                gscsummaryEntity = await _gscsummaryService.GetPartialEntityAsync(id, fields);
            }

            //if gscsummary not found.
            if (gscsummaryEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.gscsummarys.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForGscSummary(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((GscSummaryDto)gscsummaryEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = gscsummaryEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = gscsummaryEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }

        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateGscSummary")]
        public async Task<IActionResult> UpdateGscSummary(Guid id, [FromBody]GscSummaryForUpdate GscSummaryForUpdate)
        {

            //if show not found
            if (!await _gscsummaryService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _gscsummaryService.UpdateEntityAsync(id, GscSummaryForUpdate);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateGscSummary(Guid id, [FromBody] JsonPatchDocument<GscSummaryForUpdate> jsonPatchDocument)
        {
            GscSummaryForUpdate dto = new GscSummaryForUpdate();
            GscSummary gscsummary = new GscSummary();

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
            Mapper.Map(dto, gscsummary);

            //set the Id for the show model.
            gscsummary.Id = id;

            //partially update the chnages to the db. 
            await _gscsummaryService.UpdatePartialEntityAsync(gscsummary, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateGscSummary")]
        public async Task<ActionResult<GscSummaryDto>> CreateGscSummary([FromBody]GscSummaryForCreation gscsummary)
        {
            //create a show in db.
            var gscsummaryToReturn = await _gscsummaryService.CreateEntityAsync<GscSummaryDto, GscSummaryForCreation>(gscsummary);

            //return the show created response.
            return CreatedAtRoute("GetGscSummary", new { id = gscsummaryToReturn.Id }, gscsummaryToReturn);
        }

        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteGscSummaryById")]
        public async Task<IActionResult> DeleteGscSummaryById(Guid id)
        {
            //if the gscsummary exists
            if (await _gscsummaryService.ExistAsync(x => x.Id == id))
            {
                //delete the gscsummary from the db.
                await _gscsummaryService.DeleteEntityAsync(id);
            }
            else
            {
                //if gscsummary doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForGscSummary(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetGscSummary", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetGscSummary", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteGscSummaryById", new { id = id }),
              "delete_gscsummary",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateGscSummary", new { id = id }),
             "update_gscsummary",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateGscSummary", new { }),
              "create_gscsummary",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForGscSummarys(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateGscSummarysResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateGscSummarysResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateGscSummarysResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateGscSummarysResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredGscSummarys",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredGscSummarys",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredGscSummarys",
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
