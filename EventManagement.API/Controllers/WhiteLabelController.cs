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
    /// WhiteLabel endpoint
    /// </summary>
    [Route("api/whitelabels")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]

    public class WhiteLabelController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly IWhiteLabelService _whitelabelService;
        private ILogger<WhiteLabelController> _logger;
        private readonly IUrlHelper _urlHelper;

        #endregion


        #region CONSTRUCTOR

        public WhiteLabelController(IWhiteLabelService whitelabelService, ILogger<WhiteLabelController> logger, IUrlHelper urlHelper) 
        {
            _logger = logger;
            _whitelabelService = whitelabelService;
            _urlHelper = urlHelper;
        }

        #endregion


        #region HTTPGET

        [HttpGet(Name = "GetFilteredWhiteLabels")]
        [Produces("application/vnd.tourmanagement.whitelabels.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<WhiteLabelDto>>> GetFilteredWhiteLabels([FromQuery]FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_whitelabelService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<WhiteLabelDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var whitelabelsFromRepo = await _whitelabelService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.whitelabels.hateoas+json")
            {
                //create HATEOAS links for each show.
                whitelabelsFromRepo.ForEach(whitelabel =>
                {
                    var entityLinks = CreateLinksForWhiteLabel(whitelabel.Id, filterOptionsModel.Fields);
                    whitelabel.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = whitelabelsFromRepo.TotalCount,
                    pageSize = whitelabelsFromRepo.PageSize,
                    currentPage = whitelabelsFromRepo.CurrentPage,
                    totalPages = whitelabelsFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForWhiteLabels(filterOptionsModel, whitelabelsFromRepo.HasNext, whitelabelsFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = whitelabelsFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = whitelabelsFromRepo.HasPrevious ?
                    CreateWhiteLabelsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = whitelabelsFromRepo.HasNext ?
                    CreateWhiteLabelsResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = whitelabelsFromRepo.TotalCount,
                    pageSize = whitelabelsFromRepo.PageSize,
                    currentPage = whitelabelsFromRepo.CurrentPage,
                    totalPages = whitelabelsFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(whitelabelsFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.whitelabels.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetWhiteLabel")]
        public async Task<ActionResult<WhiteLabel>> GetWhiteLabel(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object whitelabelEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetWhiteLabel called");

                //then get the whole entity and map it to the Dto.
                whitelabelEntity = Mapper.Map<WhiteLabelDto>(await _whitelabelService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                whitelabelEntity = await _whitelabelService.GetPartialEntityAsync(id, fields);
            }

            //if whitelabel not found.
            if (whitelabelEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.whitelabels.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForWhiteLabel(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((WhiteLabelDto)whitelabelEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = whitelabelEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = whitelabelEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }

        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateWhiteLabel")]
        public async Task<IActionResult> UpdateWhiteLabel(Guid id, [FromBody]WhiteLabelForUpdate WhiteLabelForUpdate)
        {

            //if show not found
            if (!await _whitelabelService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _whitelabelService.UpdateEntityAsync(id, WhiteLabelForUpdate);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateWhiteLabel(Guid id, [FromBody] JsonPatchDocument<WhiteLabelForUpdate> jsonPatchDocument)
        {
            WhiteLabelForUpdate dto = new WhiteLabelForUpdate();
            WhiteLabel whitelabel = new WhiteLabel();

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
            Mapper.Map(dto, whitelabel);

            //set the Id for the show model.
            whitelabel.Id = id;

            //partially update the chnages to the db. 
            await _whitelabelService.UpdatePartialEntityAsync(whitelabel, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateWhiteLabel")]
        public async Task<ActionResult<WhiteLabelDto>> CreateWhiteLabel([FromBody]WhiteLabelForCreation whitelabel)
        {
            //create a show in db.
            var whitelabelToReturn = await _whitelabelService.CreateEntityAsync<WhiteLabelDto, WhiteLabelForCreation>(whitelabel);

            //return the show created response.
            return CreatedAtRoute("GetWhiteLabel", new { id = whitelabelToReturn.Id }, whitelabelToReturn);
        }

        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteWhiteLabelById")]
        public async Task<IActionResult> DeleteWhiteLabelById(Guid id)
        {
            //if the whitelabel exists
            if (await _whitelabelService.ExistAsync(x => x.Id == id))
            {
                //delete the whitelabel from the db.
                await _whitelabelService.DeleteEntityAsync(id);
            }
            else
            {
                //if whitelabel doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForWhiteLabel(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetWhiteLabel", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetWhiteLabel", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteWhiteLabelById", new { id = id }),
              "delete_whitelabel",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateWhiteLabel", new { id = id }),
             "update_whitelabel",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateWhiteLabel", new { }),
              "create_whitelabel",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForWhiteLabels(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateWhiteLabelsResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateWhiteLabelsResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateWhiteLabelsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateWhiteLabelsResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredWhiteLabels",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredWhiteLabels",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredWhiteLabels",
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
