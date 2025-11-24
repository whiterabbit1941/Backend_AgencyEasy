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
    /// SocialMediaSUmmmary endpoint
    /// </summary>
    [Route("api/socialmediasummmarys")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]

    public class SocialMediaSUmmmaryController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly ISocialMediaSUmmmaryService _socialmediasummmaryService;
        private ILogger<SocialMediaSUmmmaryController> _logger;
        private readonly IUrlHelper _urlHelper;

        #endregion


        #region CONSTRUCTOR

        public SocialMediaSUmmmaryController(ISocialMediaSUmmmaryService socialmediasummmaryService, ILogger<SocialMediaSUmmmaryController> logger, IUrlHelper urlHelper) 
        {
            _logger = logger;
            _socialmediasummmaryService = socialmediasummmaryService;
            _urlHelper = urlHelper;
        }

        #endregion


        #region HTTPGET

        [HttpGet(Name = "GetFilteredSocialMediaSUmmmarys")]
        [Produces("application/vnd.tourmanagement.socialmediasummmarys.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<SocialMediaSUmmmaryDto>>> GetFilteredSocialMediaSUmmmarys([FromQuery]FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_socialmediasummmaryService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<SocialMediaSUmmmaryDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var socialmediasummmarysFromRepo = await _socialmediasummmaryService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.socialmediasummmarys.hateoas+json")
            {
                //create HATEOAS links for each show.
                socialmediasummmarysFromRepo.ForEach(socialmediasummmary =>
                {
                    var entityLinks = CreateLinksForSocialMediaSUmmmary(socialmediasummmary.Id, filterOptionsModel.Fields);
                    socialmediasummmary.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = socialmediasummmarysFromRepo.TotalCount,
                    pageSize = socialmediasummmarysFromRepo.PageSize,
                    currentPage = socialmediasummmarysFromRepo.CurrentPage,
                    totalPages = socialmediasummmarysFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForSocialMediaSUmmmarys(filterOptionsModel, socialmediasummmarysFromRepo.HasNext, socialmediasummmarysFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = socialmediasummmarysFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = socialmediasummmarysFromRepo.HasPrevious ?
                    CreateSocialMediaSUmmmarysResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = socialmediasummmarysFromRepo.HasNext ?
                    CreateSocialMediaSUmmmarysResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = socialmediasummmarysFromRepo.TotalCount,
                    pageSize = socialmediasummmarysFromRepo.PageSize,
                    currentPage = socialmediasummmarysFromRepo.CurrentPage,
                    totalPages = socialmediasummmarysFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(socialmediasummmarysFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.socialmediasummmarys.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetSocialMediaSUmmmary")]
        public async Task<ActionResult<SocialMediaSUmmmary>> GetSocialMediaSUmmmary(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object socialmediasummmaryEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetSocialMediaSUmmmary called");

                //then get the whole entity and map it to the Dto.
                socialmediasummmaryEntity = Mapper.Map<SocialMediaSUmmmaryDto>(await _socialmediasummmaryService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                socialmediasummmaryEntity = await _socialmediasummmaryService.GetPartialEntityAsync(id, fields);
            }

            //if socialmediasummmary not found.
            if (socialmediasummmaryEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.socialmediasummmarys.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForSocialMediaSUmmmary(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((SocialMediaSUmmmaryDto)socialmediasummmaryEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = socialmediasummmaryEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = socialmediasummmaryEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }

        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateSocialMediaSUmmmary")]
        public async Task<IActionResult> UpdateSocialMediaSUmmmary(Guid id, [FromBody]SocialMediaSUmmmaryForUpdate SocialMediaSUmmmaryForUpdate)
        {

            //if show not found
            if (!await _socialmediasummmaryService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _socialmediasummmaryService.UpdateEntityAsync(id, SocialMediaSUmmmaryForUpdate);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateSocialMediaSUmmmary(Guid id, [FromBody] JsonPatchDocument<SocialMediaSUmmmaryForUpdate> jsonPatchDocument)
        {
            SocialMediaSUmmmaryForUpdate dto = new SocialMediaSUmmmaryForUpdate();
            SocialMediaSUmmmary socialmediasummmary = new SocialMediaSUmmmary();

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
            Mapper.Map(dto, socialmediasummmary);

            //set the Id for the show model.
            socialmediasummmary.Id = id;

            //partially update the chnages to the db. 
            await _socialmediasummmaryService.UpdatePartialEntityAsync(socialmediasummmary, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateSocialMediaSUmmmary")]
        public async Task<ActionResult<SocialMediaSUmmmaryDto>> CreateSocialMediaSUmmmary([FromBody]SocialMediaSUmmmaryForCreation socialmediasummmary)
        {
            //create a show in db.
            var socialmediasummmaryToReturn = await _socialmediasummmaryService.CreateEntityAsync<SocialMediaSUmmmaryDto, SocialMediaSUmmmaryForCreation>(socialmediasummmary);

            //return the show created response.
            return CreatedAtRoute("GetSocialMediaSUmmmary", new { id = socialmediasummmaryToReturn.Id }, socialmediasummmaryToReturn);
        }

        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteSocialMediaSUmmmaryById")]
        public async Task<IActionResult> DeleteSocialMediaSUmmmaryById(Guid id)
        {
            //if the socialmediasummmary exists
            if (await _socialmediasummmaryService.ExistAsync(x => x.Id == id))
            {
                //delete the socialmediasummmary from the db.
                await _socialmediasummmaryService.DeleteEntityAsync(id);
            }
            else
            {
                //if socialmediasummmary doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForSocialMediaSUmmmary(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetSocialMediaSUmmmary", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetSocialMediaSUmmmary", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteSocialMediaSUmmmaryById", new { id = id }),
              "delete_socialmediasummmary",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateSocialMediaSUmmmary", new { id = id }),
             "update_socialmediasummmary",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateSocialMediaSUmmmary", new { }),
              "create_socialmediasummmary",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForSocialMediaSUmmmarys(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateSocialMediaSUmmmarysResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateSocialMediaSUmmmarysResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateSocialMediaSUmmmarysResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateSocialMediaSUmmmarysResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredSocialMediaSUmmmarys",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredSocialMediaSUmmmarys",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredSocialMediaSUmmmarys",
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
