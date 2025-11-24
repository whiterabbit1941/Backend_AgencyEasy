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
    /// TemplateSetting endpoint
    /// </summary>
    [Route("api/templatesettings")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
    public class TemplateSettingController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly ITemplateSettingService _templatesettingService;
        private ILogger<TemplateSettingController> _logger;
        private readonly IUrlHelper _urlHelper;

        #endregion


        #region CONSTRUCTOR

        public TemplateSettingController(ITemplateSettingService templatesettingService, ILogger<TemplateSettingController> logger, IUrlHelper urlHelper) 
        {
            _logger = logger;
            _templatesettingService = templatesettingService;
            _urlHelper = urlHelper;
        }

        #endregion


        #region HTTPGET

        [HttpGet(Name = "GetFilteredTemplateSettings")]
        [Produces("application/vnd.tourmanagement.templatesettings.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<TemplateSettingDto>>> GetFilteredTemplateSettings([FromQuery]FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_templatesettingService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<TemplateSettingDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var templatesettingsFromRepo = await _templatesettingService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            //if (mediaType == "application/vnd.tourmanagement.templatesettings.hateoas+json")
            //{
            //    //create HATEOAS links for each show.
            //    templatesettingsFromRepo.ForEach(templatesetting =>
            //    {
            //        var entityLinks = CreateLinksForTemplateSetting(templatesetting.Id, filterOptionsModel.Fields);
            //        templatesetting.links = entityLinks;
            //    });

            //    //prepare pagination metadata.
            //    var paginationMetadata = new
            //    {
            //        totalCount = templatesettingsFromRepo.TotalCount,
            //        pageSize = templatesettingsFromRepo.PageSize,
            //        currentPage = templatesettingsFromRepo.CurrentPage,
            //        totalPages = templatesettingsFromRepo.TotalPages,
            //    };

            //    //add pagination meta data to response header.
            //    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

            //    //create links for shows.
            //    var links = CreateLinksForTemplateSettings(filterOptionsModel, templatesettingsFromRepo.HasNext, templatesettingsFromRepo.HasPrevious);

            //    //prepare model with data and HATEOAS links.
            //    var linkedCollectionResource = new
            //    {
            //        value = templatesettingsFromRepo,
            //        links = links
            //    };

            //    //return the data with Ok response.
            //    return Ok(linkedCollectionResource);
            //}
            //else
            //{
            //    var previousPageLink = templatesettingsFromRepo.HasPrevious ?
            //        CreateTemplateSettingsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

            //    var nextPageLink = templatesettingsFromRepo.HasNext ?
            //        CreateTemplateSettingsResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

            //    //prepare the pagination metadata.
            //    var paginationMetadata = new
            //    {
            //        previousPageLink = previousPageLink,
            //        nextPageLink = nextPageLink,
            //        totalCount = templatesettingsFromRepo.TotalCount,
            //        pageSize = templatesettingsFromRepo.PageSize,
            //        currentPage = templatesettingsFromRepo.CurrentPage,
            //        totalPages = templatesettingsFromRepo.TotalPages
            //    };

            //    //add pagination meta data to response header.
            //    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

            //    //return the data with Ok response.
            //    return Ok(templatesettingsFromRepo);
            //}

            //return the data with Ok response.
            return Ok(templatesettingsFromRepo);
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.templatesettings.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetTemplateSetting")]
        public async Task<ActionResult<TemplateSetting>> GetTemplateSetting(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object templatesettingEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetTemplateSetting called");

                //then get the whole entity and map it to the Dto.
                templatesettingEntity = Mapper.Map<TemplateSettingDto>(await _templatesettingService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                templatesettingEntity = await _templatesettingService.GetPartialEntityAsync(id, fields);
            }

            //if templatesetting not found.
            if (templatesettingEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            //if (mediaType == "application/vnd.tourmanagement.templatesettings.hateoas+json")
            //{
            //    //create HATEOS links
            //    var links = CreateLinksForTemplateSetting(id, fields);

            //    //if fields are not passed.
            //    if (string.IsNullOrEmpty(fields))
            //    {
            //        //convert the typed object to expando object.
            //        linkedResourceToReturn = ((TemplateSettingDto)templatesettingEntity).ShapeData("") as IDictionary<string, object>;

            //        //add the HATEOAS links to the model.
            //        ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
            //    }
            //    else
            //    {
            //        linkedResourceToReturn = templatesettingEntity;

            //        //add the HATEOAS links to the model.
            //        ((dynamic)linkedResourceToReturn).links = links;

            //    }
            //}
            //else
            //{
            //    linkedResourceToReturn = templatesettingEntity;
            //}

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }

        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateTemplateSetting")]
        public async Task<IActionResult> UpdateTemplateSetting(Guid id, [FromBody]TemplateSettingForUpdate TemplateSettingForUpdate)
        {

            //if show not found
            if (!await _templatesettingService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _templatesettingService.UpdateEntityAsync(id, TemplateSettingForUpdate);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateTemplateSetting(Guid id, [FromBody] JsonPatchDocument<TemplateSettingForUpdate> jsonPatchDocument)
        {
            TemplateSettingForUpdate dto = new TemplateSettingForUpdate();
            TemplateSetting templatesetting = new TemplateSetting();

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
            Mapper.Map(dto, templatesetting);

            //set the Id for the show model.
            templatesetting.Id = id;

            //partially update the chnages to the db. 
            await _templatesettingService.UpdatePartialEntityAsync(templatesetting, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateTemplateSetting")]
        public async Task<ActionResult<TemplateSettingDto>> CreateTemplateSetting([FromBody]TemplateSettingForCreation templatesetting)
        {
            //create a show in db.
            var templatesettingToReturn = await _templatesettingService.CreateEntityAsync<TemplateSettingDto, TemplateSettingForCreation>(templatesetting);

            //return the show created response.
            return CreatedAtRoute("GetTemplateSetting", new { id = templatesettingToReturn.Id }, templatesettingToReturn);
        }

        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteTemplateSettingById")]
        public async Task<IActionResult> DeleteTemplateSettingById(Guid id)
        {
            //if the templatesetting exists
            if (await _templatesettingService.ExistAsync(x => x.Id == id))
            {
                //delete the templatesetting from the db.
                await _templatesettingService.DeleteEntityAsync(id);
            }
            else
            {
                //if templatesetting doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForTemplateSetting(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetTemplateSetting", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetTemplateSetting", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteTemplateSettingById", new { id = id }),
              "delete_templatesetting",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateTemplateSetting", new { id = id }),
             "update_templatesetting",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateTemplateSetting", new { }),
              "create_templatesetting",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForTemplateSettings(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateTemplateSettingsResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateTemplateSettingsResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateTemplateSettingsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateTemplateSettingsResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredTemplateSettings",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredTemplateSettings",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredTemplateSettings",
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
