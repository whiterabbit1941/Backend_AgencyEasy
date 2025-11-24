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
    /// GoogleAdsSummary endpoint
    /// </summary>
    [Route("api/googleadssummarys")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]

    public class GoogleAdsSummaryController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly IGoogleAdsSummaryService _googleadssummaryService;
        private ILogger<GoogleAdsSummaryController> _logger;
        private readonly IUrlHelper _urlHelper;

        #endregion


        #region CONSTRUCTOR

        public GoogleAdsSummaryController(IGoogleAdsSummaryService googleadssummaryService, ILogger<GoogleAdsSummaryController> logger, IUrlHelper urlHelper) 
        {
            _logger = logger;
            _googleadssummaryService = googleadssummaryService;
            _urlHelper = urlHelper;
        }

        #endregion


        #region HTTPGET

        [HttpGet(Name = "GetFilteredGoogleAdsSummarys")]
        [Produces("application/vnd.tourmanagement.googleadssummarys.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<GoogleAdsSummaryDto>>> GetFilteredGoogleAdsSummarys([FromQuery]FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_googleadssummaryService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<GoogleAdsSummaryDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var googleadssummarysFromRepo = await _googleadssummaryService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.googleadssummarys.hateoas+json")
            {
                //create HATEOAS links for each show.
                googleadssummarysFromRepo.ForEach(googleadssummary =>
                {
                    var entityLinks = CreateLinksForGoogleAdsSummary(googleadssummary.Id, filterOptionsModel.Fields);
                    googleadssummary.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = googleadssummarysFromRepo.TotalCount,
                    pageSize = googleadssummarysFromRepo.PageSize,
                    currentPage = googleadssummarysFromRepo.CurrentPage,
                    totalPages = googleadssummarysFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForGoogleAdsSummarys(filterOptionsModel, googleadssummarysFromRepo.HasNext, googleadssummarysFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = googleadssummarysFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = googleadssummarysFromRepo.HasPrevious ?
                    CreateGoogleAdsSummarysResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = googleadssummarysFromRepo.HasNext ?
                    CreateGoogleAdsSummarysResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = googleadssummarysFromRepo.TotalCount,
                    pageSize = googleadssummarysFromRepo.PageSize,
                    currentPage = googleadssummarysFromRepo.CurrentPage,
                    totalPages = googleadssummarysFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(googleadssummarysFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.googleadssummarys.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetGoogleAdsSummary")]
        public async Task<ActionResult<GoogleAdsSummary>> GetGoogleAdsSummary(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object googleadssummaryEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetGoogleAdsSummary called");

                //then get the whole entity and map it to the Dto.
                googleadssummaryEntity = Mapper.Map<GoogleAdsSummaryDto>(await _googleadssummaryService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                googleadssummaryEntity = await _googleadssummaryService.GetPartialEntityAsync(id, fields);
            }

            //if googleadssummary not found.
            if (googleadssummaryEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.googleadssummarys.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForGoogleAdsSummary(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((GoogleAdsSummaryDto)googleadssummaryEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = googleadssummaryEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = googleadssummaryEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }

        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateGoogleAdsSummary")]
        public async Task<IActionResult> UpdateGoogleAdsSummary(Guid id, [FromBody]GoogleAdsSummaryForUpdate GoogleAdsSummaryForUpdate)
        {

            //if show not found
            if (!await _googleadssummaryService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _googleadssummaryService.UpdateEntityAsync(id, GoogleAdsSummaryForUpdate);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateGoogleAdsSummary(Guid id, [FromBody] JsonPatchDocument<GoogleAdsSummaryForUpdate> jsonPatchDocument)
        {
            GoogleAdsSummaryForUpdate dto = new GoogleAdsSummaryForUpdate();
            GoogleAdsSummary googleadssummary = new GoogleAdsSummary();

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
            Mapper.Map(dto, googleadssummary);

            //set the Id for the show model.
            googleadssummary.Id = id;

            //partially update the chnages to the db. 
            await _googleadssummaryService.UpdatePartialEntityAsync(googleadssummary, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateGoogleAdsSummary")]
        public async Task<ActionResult<GoogleAdsSummaryDto>> CreateGoogleAdsSummary([FromBody]GoogleAdsSummaryForCreation googleadssummary)
        {
            //create a show in db.
            var googleadssummaryToReturn = await _googleadssummaryService.CreateEntityAsync<GoogleAdsSummaryDto, GoogleAdsSummaryForCreation>(googleadssummary);

            //return the show created response.
            return CreatedAtRoute("GetGoogleAdsSummary", new { id = googleadssummaryToReturn.Id }, googleadssummaryToReturn);
        }

        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteGoogleAdsSummaryById")]
        public async Task<IActionResult> DeleteGoogleAdsSummaryById(Guid id)
        {
            //if the googleadssummary exists
            if (await _googleadssummaryService.ExistAsync(x => x.Id == id))
            {
                //delete the googleadssummary from the db.
                await _googleadssummaryService.DeleteEntityAsync(id);
            }
            else
            {
                //if googleadssummary doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForGoogleAdsSummary(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetGoogleAdsSummary", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetGoogleAdsSummary", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteGoogleAdsSummaryById", new { id = id }),
              "delete_googleadssummary",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateGoogleAdsSummary", new { id = id }),
             "update_googleadssummary",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateGoogleAdsSummary", new { }),
              "create_googleadssummary",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForGoogleAdsSummarys(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateGoogleAdsSummarysResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateGoogleAdsSummarysResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateGoogleAdsSummarysResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateGoogleAdsSummarysResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredGoogleAdsSummarys",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredGoogleAdsSummarys",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredGoogleAdsSummarys",
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
