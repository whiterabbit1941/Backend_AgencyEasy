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
    /// GoogleAccountSetup endpoint
    /// </summary>
    [Route("api/googleaccountsetups")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]

    public class GoogleAccountSetupController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly IGoogleAccountSetupService _googleaccountsetupService;
        private ILogger<GoogleAccountSetupController> _logger;
        private readonly IUrlHelper _urlHelper;

        #endregion


        #region CONSTRUCTOR

        public GoogleAccountSetupController(IGoogleAccountSetupService googleaccountsetupService, ILogger<GoogleAccountSetupController> logger, IUrlHelper urlHelper) 
        {
            _logger = logger;
            _googleaccountsetupService = googleaccountsetupService;
            _urlHelper = urlHelper;
        }

        #endregion


        #region HTTPGET

        [HttpGet(Name = "GetFilteredGoogleAccountSetups")]
        [Produces("application/vnd.tourmanagement.googleaccountsetups.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<GoogleAccountSetupDto>>> GetFilteredGoogleAccountSetups([FromQuery]FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_googleaccountsetupService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<GoogleAccountSetupDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var googleaccountsetupsFromRepo = await _googleaccountsetupService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.googleaccountsetups.hateoas+json")
            {
                //create HATEOAS links for each show.
                googleaccountsetupsFromRepo.ForEach(googleaccountsetup =>
                {
                    var entityLinks = CreateLinksForGoogleAccountSetup(googleaccountsetup.Id, filterOptionsModel.Fields);
                    googleaccountsetup.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = googleaccountsetupsFromRepo.TotalCount,
                    pageSize = googleaccountsetupsFromRepo.PageSize,
                    currentPage = googleaccountsetupsFromRepo.CurrentPage,
                    totalPages = googleaccountsetupsFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForGoogleAccountSetups(filterOptionsModel, googleaccountsetupsFromRepo.HasNext, googleaccountsetupsFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = googleaccountsetupsFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = googleaccountsetupsFromRepo.HasPrevious ?
                    CreateGoogleAccountSetupsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = googleaccountsetupsFromRepo.HasNext ?
                    CreateGoogleAccountSetupsResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = googleaccountsetupsFromRepo.TotalCount,
                    pageSize = googleaccountsetupsFromRepo.PageSize,
                    currentPage = googleaccountsetupsFromRepo.CurrentPage,
                    totalPages = googleaccountsetupsFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(googleaccountsetupsFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.googleaccountsetups.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetGoogleAccountSetup")]
        public async Task<ActionResult<GoogleAccountSetup>> GetGoogleAccountSetup(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object googleaccountsetupEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetGoogleAccountSetup called");

                //then get the whole entity and map it to the Dto.
                googleaccountsetupEntity = Mapper.Map<GoogleAccountSetupDto>(await _googleaccountsetupService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                googleaccountsetupEntity = await _googleaccountsetupService.GetPartialEntityAsync(id, fields);
            }

            //if googleaccountsetup not found.
            if (googleaccountsetupEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.googleaccountsetups.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForGoogleAccountSetup(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((GoogleAccountSetupDto)googleaccountsetupEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = googleaccountsetupEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = googleaccountsetupEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }

        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateGoogleAccountSetup")]
        public async Task<IActionResult> UpdateGoogleAccountSetup(Guid id, [FromBody]GoogleAccountSetupForUpdate GoogleAccountSetupForUpdate)
        {

            //if show not found
            if (!await _googleaccountsetupService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _googleaccountsetupService.UpdateEntityAsync(id, GoogleAccountSetupForUpdate);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateGoogleAccountSetup(Guid id, [FromBody] JsonPatchDocument<GoogleAccountSetupForUpdate> jsonPatchDocument)
        {
            GoogleAccountSetupForUpdate dto = new GoogleAccountSetupForUpdate();
            GoogleAccountSetup googleaccountsetup = new GoogleAccountSetup();

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
            Mapper.Map(dto, googleaccountsetup);

            //set the Id for the show model.
            googleaccountsetup.Id = id;

            //partially update the chnages to the db. 
            await _googleaccountsetupService.UpdatePartialEntityAsync(googleaccountsetup, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateGoogleAccountSetup")]
        public async Task<ActionResult<GoogleAccountSetupDto>> CreateGoogleAccountSetup([FromBody]GoogleAccountSetupForCreation googleaccountsetup)
        {
            //create a show in db.
            var googleaccountsetupToReturn = await _googleaccountsetupService.CreateEntityAsync<GoogleAccountSetupDto, GoogleAccountSetupForCreation>(googleaccountsetup);

            //return the show created response.
            return CreatedAtRoute("GetGoogleAccountSetup", new { id = googleaccountsetupToReturn.Id }, googleaccountsetupToReturn);
        }

        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteGoogleAccountSetupById")]
        public async Task<IActionResult> DeleteGoogleAccountSetupById(Guid id)
        {
            //if the googleaccountsetup exists
            if (await _googleaccountsetupService.ExistAsync(x => x.Id == id))
            {
                //delete the googleaccountsetup from the db.
                await _googleaccountsetupService.DeleteEntityAsync(id);
            }
            else
            {
                //if googleaccountsetup doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForGoogleAccountSetup(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetGoogleAccountSetup", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetGoogleAccountSetup", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteGoogleAccountSetupById", new { id = id }),
              "delete_googleaccountsetup",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateGoogleAccountSetup", new { id = id }),
             "update_googleaccountsetup",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateGoogleAccountSetup", new { }),
              "create_googleaccountsetup",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForGoogleAccountSetups(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateGoogleAccountSetupsResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateGoogleAccountSetupsResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateGoogleAccountSetupsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateGoogleAccountSetupsResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredGoogleAccountSetups",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredGoogleAccountSetups",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredGoogleAccountSetups",
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
