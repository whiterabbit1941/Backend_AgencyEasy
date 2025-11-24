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
    /// ClientPostLogoutRedirectUri endpoint
    /// </summary>
    [Route("api/clientpostlogoutredirecturis")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
    public class ClientPostLogoutRedirectUriController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly IClientPostLogoutRedirectUriService _clientpostlogoutredirecturiService;
        private ILogger<ClientPostLogoutRedirectUriController> _logger;
        private readonly IUrlHelper _urlHelper;

        #endregion


        #region CONSTRUCTOR

        public ClientPostLogoutRedirectUriController(IClientPostLogoutRedirectUriService clientpostlogoutredirecturiService, ILogger<ClientPostLogoutRedirectUriController> logger, IUrlHelper urlHelper) 
        {
            _logger = logger;
            _clientpostlogoutredirecturiService = clientpostlogoutredirecturiService;
            _urlHelper = urlHelper;
        }

        #endregion


        #region HTTPGET

        [HttpGet(Name = "GetFilteredClientPostLogoutRedirectUris")]
        [Produces("application/vnd.tourmanagement.clientpostlogoutredirecturis.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ClientPostLogoutRedirectUriDto>>> GetFilteredClientPostLogoutRedirectUris([FromQuery]FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_clientpostlogoutredirecturiService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<ClientPostLogoutRedirectUriDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var clientpostlogoutredirecturisFromRepo = await _clientpostlogoutredirecturiService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.clientpostlogoutredirecturis.hateoas+json")
            {
                //create HATEOAS links for each show.
                clientpostlogoutredirecturisFromRepo.ForEach(clientpostlogoutredirecturi =>
                {
                    var entityLinks = CreateLinksForClientPostLogoutRedirectUri(clientpostlogoutredirecturi.Id, filterOptionsModel.Fields);
                    clientpostlogoutredirecturi.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = clientpostlogoutredirecturisFromRepo.TotalCount,
                    pageSize = clientpostlogoutredirecturisFromRepo.PageSize,
                    currentPage = clientpostlogoutredirecturisFromRepo.CurrentPage,
                    totalPages = clientpostlogoutredirecturisFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForClientPostLogoutRedirectUris(filterOptionsModel, clientpostlogoutredirecturisFromRepo.HasNext, clientpostlogoutredirecturisFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = clientpostlogoutredirecturisFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = clientpostlogoutredirecturisFromRepo.HasPrevious ?
                    CreateClientPostLogoutRedirectUrisResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = clientpostlogoutredirecturisFromRepo.HasNext ?
                    CreateClientPostLogoutRedirectUrisResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = clientpostlogoutredirecturisFromRepo.TotalCount,
                    pageSize = clientpostlogoutredirecturisFromRepo.PageSize,
                    currentPage = clientpostlogoutredirecturisFromRepo.CurrentPage,
                    totalPages = clientpostlogoutredirecturisFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(clientpostlogoutredirecturisFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.clientpostlogoutredirecturis.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetClientPostLogoutRedirectUri")]
        public async Task<ActionResult<ClientPostLogoutRedirectUris>> GetClientPostLogoutRedirectUri(int id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object clientpostlogoutredirecturiEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetClientPostLogoutRedirectUri called");

                //then get the whole entity and map it to the Dto.
                clientpostlogoutredirecturiEntity = Mapper.Map<ClientPostLogoutRedirectUriDto>(await _clientpostlogoutredirecturiService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                clientpostlogoutredirecturiEntity = await _clientpostlogoutredirecturiService.GetPartialEntityAsync(id, fields);
            }

            //if clientpostlogoutredirecturi not found.
            if (clientpostlogoutredirecturiEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.clientpostlogoutredirecturis.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForClientPostLogoutRedirectUri(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((ClientPostLogoutRedirectUriDto)clientpostlogoutredirecturiEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = clientpostlogoutredirecturiEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = clientpostlogoutredirecturiEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }

        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateClientPostLogoutRedirectUri")]
        public async Task<IActionResult> UpdateClientPostLogoutRedirectUri(int id, [FromBody]ClientPostLogoutRedirectUriForUpdate ClientPostLogoutRedirectUriForUpdate)
        {

            //if show not found
            if (!await _clientpostlogoutredirecturiService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _clientpostlogoutredirecturiService.UpdateEntityAsync(id, ClientPostLogoutRedirectUriForUpdate);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateClientPostLogoutRedirectUri(int id, [FromBody] JsonPatchDocument<ClientPostLogoutRedirectUriForUpdate> jsonPatchDocument)
        {
            ClientPostLogoutRedirectUriForUpdate dto = new ClientPostLogoutRedirectUriForUpdate();
            ClientPostLogoutRedirectUris clientpostlogoutredirecturi = new ClientPostLogoutRedirectUris();

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
            Mapper.Map(dto, clientpostlogoutredirecturi);

            //set the Id for the show model.
            clientpostlogoutredirecturi.Id = id;

            //partially update the chnages to the db. 
            await _clientpostlogoutredirecturiService.UpdatePartialEntityAsync(clientpostlogoutredirecturi, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateClientPostLogoutRedirectUri")]
        public async Task<ActionResult<ClientPostLogoutRedirectUriDto>> CreateClientPostLogoutRedirectUri([FromBody]ClientPostLogoutRedirectUriForCreation clientpostlogoutredirecturi)
        {
            //create a show in db.
            var clientpostlogoutredirecturiToReturn = await _clientpostlogoutredirecturiService.CreateEntityAsync<ClientPostLogoutRedirectUriDto, ClientPostLogoutRedirectUriForCreation>(clientpostlogoutredirecturi);

            //return the show created response.
            return CreatedAtRoute("GetClientPostLogoutRedirectUri", new { id = clientpostlogoutredirecturiToReturn.Id }, clientpostlogoutredirecturiToReturn);
        }

        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteClientPostLogoutRedirectUriById")]
        public async Task<IActionResult> DeleteClientPostLogoutRedirectUriById(int id)
        {
            //if the clientpostlogoutredirecturi exists
            if (await _clientpostlogoutredirecturiService.ExistAsync(x => x.Id == id))
            {
                //delete the clientpostlogoutredirecturi from the db.
                await _clientpostlogoutredirecturiService.DeleteEntityAsync(id);
            }
            else
            {
                //if clientpostlogoutredirecturi doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForClientPostLogoutRedirectUri(int id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetClientPostLogoutRedirectUri", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetClientPostLogoutRedirectUri", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteClientPostLogoutRedirectUriById", new { id = id }),
              "delete_clientpostlogoutredirecturi",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateClientPostLogoutRedirectUri", new { id = id }),
             "update_clientpostlogoutredirecturi",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateClientPostLogoutRedirectUri", new { }),
              "create_clientpostlogoutredirecturi",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForClientPostLogoutRedirectUris(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateClientPostLogoutRedirectUrisResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateClientPostLogoutRedirectUrisResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateClientPostLogoutRedirectUrisResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateClientPostLogoutRedirectUrisResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredClientPostLogoutRedirectUris",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredClientPostLogoutRedirectUris",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredClientPostLogoutRedirectUris",
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
