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
    /// EmailSetting endpoint
    /// </summary>
    [Route("api/emailsettings")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]

    public class EmailSettingController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly IEmailSettingService _emailsettingService;
        private ILogger<EmailSettingController> _logger;
        private readonly IUrlHelper _urlHelper;

        #endregion


        #region CONSTRUCTOR

        public EmailSettingController(IEmailSettingService emailsettingService, ILogger<EmailSettingController> logger, IUrlHelper urlHelper) 
        {
            _logger = logger;
            _emailsettingService = emailsettingService;
            _urlHelper = urlHelper;
        }

        #endregion


        #region HTTPGET

        [HttpGet(Name = "GetFilteredEmailSettings")]
        [Produces("application/vnd.tourmanagement.emailsettings.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<EmailSettingDto>>> GetFilteredEmailSettings([FromQuery]FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_emailsettingService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<EmailSettingDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var emailsettingsFromRepo = await _emailsettingService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.emailsettings.hateoas+json")
            {
                //create HATEOAS links for each show.
                emailsettingsFromRepo.ForEach(emailsetting =>
                {
                    var entityLinks = CreateLinksForEmailSetting(emailsetting.Id, filterOptionsModel.Fields);
                    emailsetting.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = emailsettingsFromRepo.TotalCount,
                    pageSize = emailsettingsFromRepo.PageSize,
                    currentPage = emailsettingsFromRepo.CurrentPage,
                    totalPages = emailsettingsFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForEmailSettings(filterOptionsModel, emailsettingsFromRepo.HasNext, emailsettingsFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = emailsettingsFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = emailsettingsFromRepo.HasPrevious ?
                    CreateEmailSettingsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = emailsettingsFromRepo.HasNext ?
                    CreateEmailSettingsResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = emailsettingsFromRepo.TotalCount,
                    pageSize = emailsettingsFromRepo.PageSize,
                    currentPage = emailsettingsFromRepo.CurrentPage,
                    totalPages = emailsettingsFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(emailsettingsFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.emailsettings.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetEmailSetting")]
        public async Task<ActionResult<EmailSetting>> GetEmailSetting(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object emailsettingEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetEmailSetting called");

                //then get the whole entity and map it to the Dto.
                emailsettingEntity = Mapper.Map<EmailSettingDto>(await _emailsettingService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                emailsettingEntity = await _emailsettingService.GetPartialEntityAsync(id, fields);
            }

            //if emailsetting not found.
            if (emailsettingEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.emailsettings.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForEmailSetting(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((EmailSettingDto)emailsettingEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = emailsettingEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = emailsettingEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }

        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateEmailSetting")]
        public async Task<IActionResult> UpdateEmailSetting(Guid id, [FromBody]EmailSettingForUpdate EmailSettingForUpdate)
        {

            //if show not found
            if (!await _emailsettingService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _emailsettingService.UpdateEntityAsync(id, EmailSettingForUpdate);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateEmailSetting(Guid id, [FromBody] JsonPatchDocument<EmailSettingForUpdate> jsonPatchDocument)
        {
            EmailSettingForUpdate dto = new EmailSettingForUpdate();
            EmailSetting emailsetting = new EmailSetting();

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
            Mapper.Map(dto, emailsetting);

            //set the Id for the show model.
            emailsetting.Id = id;

            //partially update the chnages to the db. 
            await _emailsettingService.UpdatePartialEntityAsync(emailsetting, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateEmailSetting")]
        public async Task<ActionResult<EmailSettingDto>> CreateEmailSetting([FromBody]EmailSettingForCreation emailsetting)
        {
            //create a show in db.
            var emailsettingToReturn = await _emailsettingService.CreateEntityAsync<EmailSettingDto, EmailSettingForCreation>(emailsetting);

            //return the show created response.
            return CreatedAtRoute("GetEmailSetting", new { id = emailsettingToReturn.Id }, emailsettingToReturn);
        }

        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteEmailSettingById")]
        public async Task<IActionResult> DeleteEmailSettingById(Guid id)
        {
            //if the emailsetting exists
            if (await _emailsettingService.ExistAsync(x => x.Id == id))
            {
                //delete the emailsetting from the db.
                await _emailsettingService.DeleteEntityAsync(id);
            }
            else
            {
                //if emailsetting doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForEmailSetting(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetEmailSetting", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetEmailSetting", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteEmailSettingById", new { id = id }),
              "delete_emailsetting",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateEmailSetting", new { id = id }),
             "update_emailsetting",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateEmailSetting", new { }),
              "create_emailsetting",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForEmailSettings(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateEmailSettingsResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateEmailSettingsResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateEmailSettingsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateEmailSettingsResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredEmailSettings",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredEmailSettings",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredEmailSettings",
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
