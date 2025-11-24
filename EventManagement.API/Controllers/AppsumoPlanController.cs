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

namespace EventManagement.API.Controllers
{
    /// <summary>
    /// AppsumoPlan endpoint
    /// </summary>
    [Route("api/appsumoplans")]
    [Produces("application/json")]
    [ApiController]
    public class AppsumoPlanController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly IAppsumoPlanService _appsumoplanService;
        private ILogger<AppsumoPlanController> _logger;
        private readonly IUrlHelper _urlHelper;

        #endregion


        #region CONSTRUCTOR

        public AppsumoPlanController(IAppsumoPlanService appsumoplanService, ILogger<AppsumoPlanController> logger, IUrlHelper urlHelper) 
        {
            _logger = logger;
            _appsumoplanService = appsumoplanService;
            _urlHelper = urlHelper;
        }

        #endregion


        #region HTTPGET

        [HttpGet(Name = "GetFilteredAppsumoPlans")]
        [Produces("application/vnd.tourmanagement.appsumoplans.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<AppsumoPlanDto>>> GetFilteredAppsumoPlans([FromQuery]FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_appsumoplanService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<AppsumoPlanDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var appsumoplansFromRepo = await _appsumoplanService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.appsumoplans.hateoas+json")
            {
                //create HATEOAS links for each show.
                appsumoplansFromRepo.ForEach(appsumoplan =>
                {
                    var entityLinks = CreateLinksForAppsumoPlan(appsumoplan.Id, filterOptionsModel.Fields);
                    appsumoplan.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = appsumoplansFromRepo.TotalCount,
                    pageSize = appsumoplansFromRepo.PageSize,
                    currentPage = appsumoplansFromRepo.CurrentPage,
                    totalPages = appsumoplansFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForAppsumoPlans(filterOptionsModel, appsumoplansFromRepo.HasNext, appsumoplansFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = appsumoplansFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = appsumoplansFromRepo.HasPrevious ?
                    CreateAppsumoPlansResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = appsumoplansFromRepo.HasNext ?
                    CreateAppsumoPlansResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = appsumoplansFromRepo.TotalCount,
                    pageSize = appsumoplansFromRepo.PageSize,
                    currentPage = appsumoplansFromRepo.CurrentPage,
                    totalPages = appsumoplansFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(appsumoplansFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.appsumoplans.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetAppsumoPlan")]
        public async Task<ActionResult<AppsumoPlan>> GetAppsumoPlan(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object appsumoplanEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetAppsumoPlan called");

                //then get the whole entity and map it to the Dto.
                appsumoplanEntity = Mapper.Map<AppsumoPlanDto>(await _appsumoplanService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                appsumoplanEntity = await _appsumoplanService.GetPartialEntityAsync(id, fields);
            }

            //if appsumoplan not found.
            if (appsumoplanEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.appsumoplans.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForAppsumoPlan(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((AppsumoPlanDto)appsumoplanEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = appsumoplanEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = appsumoplanEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }

        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateAppsumoPlan")]
        public async Task<IActionResult> UpdateAppsumoPlan(Guid id, [FromBody]AppsumoPlanForUpdate AppsumoPlanForUpdate)
        {

            //if show not found
            if (!await _appsumoplanService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _appsumoplanService.UpdateEntityAsync(id, AppsumoPlanForUpdate);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateAppsumoPlan(Guid id, [FromBody] JsonPatchDocument<AppsumoPlanForUpdate> jsonPatchDocument)
        {
            AppsumoPlanForUpdate dto = new AppsumoPlanForUpdate();
            AppsumoPlan appsumoplan = new AppsumoPlan();

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
            Mapper.Map(dto, appsumoplan);

            //set the Id for the show model.
            appsumoplan.Id = id;

            //partially update the chnages to the db. 
            await _appsumoplanService.UpdatePartialEntityAsync(appsumoplan, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateAppsumoPlan")]
        public async Task<ActionResult<AppsumoPlanDto>> CreateAppsumoPlan([FromBody]AppsumoPlanForCreation appsumoplan)
        {
            //create a show in db.
            var appsumoplanToReturn = await _appsumoplanService.CreateEntityAsync<AppsumoPlanDto, AppsumoPlanForCreation>(appsumoplan);

            //return the show created response.
            return CreatedAtRoute("GetAppsumoPlan", new { id = appsumoplanToReturn.Id }, appsumoplanToReturn);
        }

        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteAppsumoPlanById")]
        public async Task<IActionResult> DeleteAppsumoPlanById(Guid id)
        {
            //if the appsumoplan exists
            if (await _appsumoplanService.ExistAsync(x => x.Id == id))
            {
                //delete the appsumoplan from the db.
                await _appsumoplanService.DeleteEntityAsync(id);
            }
            else
            {
                //if appsumoplan doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForAppsumoPlan(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetAppsumoPlan", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetAppsumoPlan", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteAppsumoPlanById", new { id = id }),
              "delete_appsumoplan",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateAppsumoPlan", new { id = id }),
             "update_appsumoplan",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateAppsumoPlan", new { }),
              "create_appsumoplan",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForAppsumoPlans(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateAppsumoPlansResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateAppsumoPlansResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateAppsumoPlansResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateAppsumoPlansResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredAppsumoPlans",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredAppsumoPlans",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredAppsumoPlans",
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
