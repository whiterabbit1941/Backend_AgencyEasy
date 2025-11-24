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
    /// DefaultPlan endpoint
    /// </summary>
    [Route("api/defaultplans")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
    public class DefaultPlanController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly IDefaultPlanService _defaultplanService;
        private ILogger<DefaultPlanController> _logger;
        private readonly IUrlHelper _urlHelper;

        #endregion


        #region CONSTRUCTOR

        public DefaultPlanController(IDefaultPlanService defaultplanService, ILogger<DefaultPlanController> logger, IUrlHelper urlHelper) 
        {
            _logger = logger;
            _defaultplanService = defaultplanService;
            _urlHelper = urlHelper;
        }

        #endregion


        #region HTTPGET

        [HttpGet(Name = "GetFilteredDefaultPlans")]
        [Produces("application/vnd.tourmanagement.defaultplans.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<DefaultPlanDto>>> GetFilteredDefaultPlans([FromQuery]FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_defaultplanService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<DefaultPlanDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var defaultplansFromRepo = await _defaultplanService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.defaultplans.hateoas+json")
            {
                //create HATEOAS links for each show.
                defaultplansFromRepo.ForEach(defaultplan =>
                {
                    var entityLinks = CreateLinksForDefaultPlan(defaultplan.Id, filterOptionsModel.Fields);
                    defaultplan.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = defaultplansFromRepo.TotalCount,
                    pageSize = defaultplansFromRepo.PageSize,
                    currentPage = defaultplansFromRepo.CurrentPage,
                    totalPages = defaultplansFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForDefaultPlans(filterOptionsModel, defaultplansFromRepo.HasNext, defaultplansFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = defaultplansFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = defaultplansFromRepo.HasPrevious ?
                    CreateDefaultPlansResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = defaultplansFromRepo.HasNext ?
                    CreateDefaultPlansResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = defaultplansFromRepo.TotalCount,
                    pageSize = defaultplansFromRepo.PageSize,
                    currentPage = defaultplansFromRepo.CurrentPage,
                    totalPages = defaultplansFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(defaultplansFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.defaultplans.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetDefaultPlan")]
        public async Task<ActionResult<DefaultPlan>> GetDefaultPlan(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object defaultplanEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetDefaultPlan called");

                //then get the whole entity and map it to the Dto.
                defaultplanEntity = Mapper.Map<DefaultPlanDto>(await _defaultplanService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                defaultplanEntity = await _defaultplanService.GetPartialEntityAsync(id, fields);
            }

            //if defaultplan not found.
            if (defaultplanEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.defaultplans.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForDefaultPlan(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((DefaultPlanDto)defaultplanEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = defaultplanEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = defaultplanEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }

        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateDefaultPlan")]
        public async Task<IActionResult> UpdateDefaultPlan(Guid id, [FromBody]DefaultPlanForUpdate DefaultPlanForUpdate)
        {

            //if show not found
            if (!await _defaultplanService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _defaultplanService.UpdateEntityAsync(id, DefaultPlanForUpdate);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateDefaultPlan(Guid id, [FromBody] JsonPatchDocument<DefaultPlanForUpdate> jsonPatchDocument)
        {
            DefaultPlanForUpdate dto = new DefaultPlanForUpdate();
            DefaultPlan defaultplan = new DefaultPlan();

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
            Mapper.Map(dto, defaultplan);

            //set the Id for the show model.
            defaultplan.Id = id;

            //partially update the chnages to the db. 
            await _defaultplanService.UpdatePartialEntityAsync(defaultplan, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateDefaultPlan")]
        public async Task<ActionResult<DefaultPlanDto>> CreateDefaultPlan([FromBody]DefaultPlanForCreation defaultplan)
        {
            //create a show in db.
            var defaultplanToReturn = await _defaultplanService.CreateEntityAsync<DefaultPlanDto, DefaultPlanForCreation>(defaultplan);

            //return the show created response.
            return CreatedAtRoute("GetDefaultPlan", new { id = defaultplanToReturn.Id }, defaultplanToReturn);
        }

        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteDefaultPlanById")]
        public async Task<IActionResult> DeleteDefaultPlanById(Guid id)
        {
            //if the defaultplan exists
            if (await _defaultplanService.ExistAsync(x => x.Id == id))
            {
                //delete the defaultplan from the db.
                await _defaultplanService.DeleteEntityAsync(id);
            }
            else
            {
                //if defaultplan doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForDefaultPlan(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetDefaultPlan", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetDefaultPlan", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteDefaultPlanById", new { id = id }),
              "delete_defaultplan",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateDefaultPlan", new { id = id }),
             "update_defaultplan",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateDefaultPlan", new { }),
              "create_defaultplan",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForDefaultPlans(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateDefaultPlansResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateDefaultPlansResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateDefaultPlansResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateDefaultPlansResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredDefaultPlans",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredDefaultPlans",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredDefaultPlans",
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
