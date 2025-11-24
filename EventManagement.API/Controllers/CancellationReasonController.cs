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
    /// CancellationReason endpoint
    /// </summary>
    [Route("api/cancellationreasons")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
    public class CancellationReasonController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly ICancellationReasonService _cancellationreasonService;
        private ILogger<CancellationReasonController> _logger;
        private readonly IUrlHelper _urlHelper;

        #endregion


        #region CONSTRUCTOR

        public CancellationReasonController(ICancellationReasonService cancellationreasonService, ILogger<CancellationReasonController> logger, IUrlHelper urlHelper) 
        {
            _logger = logger;
            _cancellationreasonService = cancellationreasonService;
            _urlHelper = urlHelper;
        }

        #endregion


        #region HTTPGET

        [HttpGet(Name = "GetFilteredCancellationReasons")]
        [Produces("application/vnd.tourmanagement.cancellationreasons.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CancellationReasonDto>>> GetFilteredCancellationReasons([FromQuery]FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_cancellationreasonService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<CancellationReasonDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var cancellationreasonsFromRepo = await _cancellationreasonService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.cancellationreasons.hateoas+json")
            {
                //create HATEOAS links for each show.
                cancellationreasonsFromRepo.ForEach(cancellationreason =>
                {
                    var entityLinks = CreateLinksForCancellationReason(cancellationreason.Id, filterOptionsModel.Fields);
                    cancellationreason.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = cancellationreasonsFromRepo.TotalCount,
                    pageSize = cancellationreasonsFromRepo.PageSize,
                    currentPage = cancellationreasonsFromRepo.CurrentPage,
                    totalPages = cancellationreasonsFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForCancellationReasons(filterOptionsModel, cancellationreasonsFromRepo.HasNext, cancellationreasonsFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = cancellationreasonsFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = cancellationreasonsFromRepo.HasPrevious ?
                    CreateCancellationReasonsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = cancellationreasonsFromRepo.HasNext ?
                    CreateCancellationReasonsResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = cancellationreasonsFromRepo.TotalCount,
                    pageSize = cancellationreasonsFromRepo.PageSize,
                    currentPage = cancellationreasonsFromRepo.CurrentPage,
                    totalPages = cancellationreasonsFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(cancellationreasonsFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.cancellationreasons.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetCancellationReason")]
        public async Task<ActionResult<CancellationReason>> GetCancellationReason(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object cancellationreasonEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetCancellationReason called");

                //then get the whole entity and map it to the Dto.
                cancellationreasonEntity = Mapper.Map<CancellationReasonDto>(await _cancellationreasonService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                cancellationreasonEntity = await _cancellationreasonService.GetPartialEntityAsync(id, fields);
            }

            //if cancellationreason not found.
            if (cancellationreasonEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.cancellationreasons.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForCancellationReason(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((CancellationReasonDto)cancellationreasonEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = cancellationreasonEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = cancellationreasonEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }

        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateCancellationReason")]
        public async Task<IActionResult> UpdateCancellationReason(Guid id, [FromBody]CancellationReasonForUpdate CancellationReasonForUpdate)
        {

            //if show not found
            if (!await _cancellationreasonService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _cancellationreasonService.UpdateEntityAsync(id, CancellationReasonForUpdate);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateCancellationReason(Guid id, [FromBody] JsonPatchDocument<CancellationReasonForUpdate> jsonPatchDocument)
        {
            CancellationReasonForUpdate dto = new CancellationReasonForUpdate();
            CancellationReason cancellationreason = new CancellationReason();

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
            Mapper.Map(dto, cancellationreason);

            //set the Id for the show model.
            cancellationreason.Id = id;

            //partially update the chnages to the db. 
            await _cancellationreasonService.UpdatePartialEntityAsync(cancellationreason, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateCancellationReason")]
        public async Task<ActionResult<CancellationReasonDto>> CreateCancellationReason([FromBody]CancellationReasonForCreation cancellationreason)
        {
            //create a show in db.
            var cancellationreasonToReturn = await _cancellationreasonService.CreateEntityAsync<CancellationReasonDto, CancellationReasonForCreation>(cancellationreason);

            //return the show created response.
            return CreatedAtRoute("GetCancellationReason", new { id = cancellationreasonToReturn.Id }, cancellationreasonToReturn);
        }

        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteCancellationReasonById")]
        public async Task<IActionResult> DeleteCancellationReasonById(Guid id)
        {
            //if the cancellationreason exists
            if (await _cancellationreasonService.ExistAsync(x => x.Id == id))
            {
                //delete the cancellationreason from the db.
                await _cancellationreasonService.DeleteEntityAsync(id);
            }
            else
            {
                //if cancellationreason doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForCancellationReason(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCancellationReason", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCancellationReason", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteCancellationReasonById", new { id = id }),
              "delete_cancellationreason",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateCancellationReason", new { id = id }),
             "update_cancellationreason",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateCancellationReason", new { }),
              "create_cancellationreason",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForCancellationReasons(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateCancellationReasonsResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateCancellationReasonsResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateCancellationReasonsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateCancellationReasonsResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredCancellationReasons",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredCancellationReasons",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredCancellationReasons",
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
