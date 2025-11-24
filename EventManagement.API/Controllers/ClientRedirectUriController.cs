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
using FinanaceManagement.API.Models;
using IdentityServer4.AccessTokenValidation;

namespace EventManagement.API.Controllers
{
    /// <summary>
    /// ClientRedirectUri endpoint
    /// </summary>
    [Route("api/clientredirecturis")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
    public class ClientRedirectUriController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly IClientRedirectUriService _clientredirecturiService;
        private ILogger<ClientRedirectUriController> _logger;
        private readonly IUrlHelper _urlHelper;

        #endregion


        #region CONSTRUCTOR

        public ClientRedirectUriController(IClientRedirectUriService clientredirecturiService, ILogger<ClientRedirectUriController> logger, IUrlHelper urlHelper) 
        {
            _logger = logger;
            _clientredirecturiService = clientredirecturiService;
            _urlHelper = urlHelper;
        }

        #endregion


        #region HTTPGET

        [HttpGet(Name = "GetFilteredClientRedirectUris")]
        [Produces("application/vnd.tourmanagement.clientredirecturis.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ClientRedirectUriDto>>> GetFilteredClientRedirectUris([FromQuery]FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_clientredirecturiService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<ClientRedirectUriDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var clientredirecturisFromRepo = await _clientredirecturiService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.clientredirecturis.hateoas+json")
            {
                //create HATEOAS links for each show.
                clientredirecturisFromRepo.ForEach(clientredirecturi =>
                {
                    var entityLinks = CreateLinksForClientRedirectUri(clientredirecturi.Id, filterOptionsModel.Fields);
                    clientredirecturi.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = clientredirecturisFromRepo.TotalCount,
                    pageSize = clientredirecturisFromRepo.PageSize,
                    currentPage = clientredirecturisFromRepo.CurrentPage,
                    totalPages = clientredirecturisFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForClientRedirectUris(filterOptionsModel, clientredirecturisFromRepo.HasNext, clientredirecturisFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = clientredirecturisFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = clientredirecturisFromRepo.HasPrevious ?
                    CreateClientRedirectUrisResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = clientredirecturisFromRepo.HasNext ?
                    CreateClientRedirectUrisResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = clientredirecturisFromRepo.TotalCount,
                    pageSize = clientredirecturisFromRepo.PageSize,
                    currentPage = clientredirecturisFromRepo.CurrentPage,
                    totalPages = clientredirecturisFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(clientredirecturisFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.clientredirecturis.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetClientRedirectUri")]
        public async Task<ActionResult<ClientRedirectUris>> GetClientRedirectUri(int id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object clientredirecturiEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetClientRedirectUri called");

                //then get the whole entity and map it to the Dto.
                clientredirecturiEntity = Mapper.Map<ClientRedirectUriDto>(await _clientredirecturiService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                clientredirecturiEntity = await _clientredirecturiService.GetPartialEntityAsync(id, fields);
            }

            //if clientredirecturi not found.
            if (clientredirecturiEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.clientredirecturis.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForClientRedirectUri(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((ClientRedirectUriDto)clientredirecturiEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = clientredirecturiEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = clientredirecturiEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }

        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateClientRedirectUri")]
        public async Task<IActionResult> UpdateClientRedirectUri(int id, [FromBody]ClientRedirectUriForUpdate ClientRedirectUriForUpdate)
        {

            //if show not found
            if (!await _clientredirecturiService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _clientredirecturiService.UpdateEntityAsync(id, ClientRedirectUriForUpdate);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateClientRedirectUri(int id, [FromBody] JsonPatchDocument<ClientRedirectUriForUpdate> jsonPatchDocument)
        {
            ClientRedirectUriForUpdate dto = new ClientRedirectUriForUpdate();
            ClientRedirectUris clientredirecturi = new ClientRedirectUris();

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
            Mapper.Map(dto, clientredirecturi);

            //set the Id for the show model.
            clientredirecturi.Id = id;

            //partially update the chnages to the db. 
            await _clientredirecturiService.UpdatePartialEntityAsync(clientredirecturi, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateClientRedirectUri")]
        public async Task<ActionResult<ClientRedirectUriDto>> CreateClientRedirectUri([FromBody]ClientRedirectUriForCreation clientredirecturi)
        {
            //create a show in db.
            var clientredirecturiToReturn = await _clientredirecturiService.CreateEntityAsync<ClientRedirectUriDto, ClientRedirectUriForCreation>(clientredirecturi);

            //return the show created response.
            return CreatedAtRoute("GetClientRedirectUri", new { id = clientredirecturiToReturn.Id }, clientredirecturiToReturn);
        }

        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteClientRedirectUriById")]
        public async Task<IActionResult> DeleteClientRedirectUriById(int id)
        {
            //if the clientredirecturi exists
            if (await _clientredirecturiService.ExistAsync(x => x.Id == id))
            {
                //delete the clientredirecturi from the db.
                await _clientredirecturiService.DeleteEntityAsync(id);
            }
            else
            {
                //if clientredirecturi doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForClientRedirectUri(int id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetClientRedirectUri", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetClientRedirectUri", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteClientRedirectUriById", new { id = id }),
              "delete_clientredirecturi",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateClientRedirectUri", new { id = id }),
             "update_clientredirecturi",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateClientRedirectUri", new { }),
              "create_clientredirecturi",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForClientRedirectUris(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateClientRedirectUrisResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateClientRedirectUrisResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateClientRedirectUrisResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateClientRedirectUrisResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredClientRedirectUris",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredClientRedirectUris",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredClientRedirectUris",
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
