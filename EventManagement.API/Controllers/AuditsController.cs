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
    /// Audits endpoint
    /// </summary>
    [Route("api/audits")]
    [Produces("application/json")]
    [ApiController]
    public class AuditsController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly IAuditsService _auditsService;
        private ILogger<AuditsController> _logger;
        private readonly IUrlHelper _urlHelper;

        #endregion


        #region CONSTRUCTOR

        public AuditsController(IAuditsService auditsService, ILogger<AuditsController> logger, IUrlHelper urlHelper) 
        {
            _logger = logger;
            _auditsService = auditsService;
            _urlHelper = urlHelper;
        }

        #endregion


        #region HTTPGET

        [HttpGet(Name = "GetFilteredAudits")]
        [Produces("application/vnd.tourmanagement.auditss.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<AuditsDto>>> GetFilteredAudits([FromQuery]FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_auditsService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<AuditsDto>(filterOptionsModel.Fields))
            {
                //then return bad request.

                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var auditssFromRepo = await _auditsService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.auditss.hateoas+json")
            {
                //create HATEOAS links for each show.
                auditssFromRepo.ForEach(audits =>
                {
                    var entityLinks = CreateLinksForAudits(audits.Id, filterOptionsModel.Fields);
                    audits.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = auditssFromRepo.TotalCount,
                    pageSize = auditssFromRepo.PageSize,
                    currentPage = auditssFromRepo.CurrentPage,
                    totalPages = auditssFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForAuditss(filterOptionsModel, auditssFromRepo.HasNext, auditssFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = auditssFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = auditssFromRepo.HasPrevious ?
                    CreateAuditssResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = auditssFromRepo.HasNext ?
                    CreateAuditssResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = auditssFromRepo.TotalCount,
                    pageSize = auditssFromRepo.PageSize,
                    currentPage = auditssFromRepo.CurrentPage,
                    totalPages = auditssFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(auditssFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.auditss.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetAudits")]
        public async Task<ActionResult<Domain.Entities.Audits>> GetAudits(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object auditsEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetAudits called");

                //then get the whole entity and map it to the Dto.
                auditsEntity = Mapper.Map<AuditsDto>(await _auditsService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                auditsEntity = await _auditsService.GetPartialEntityAsync(id, fields);
            }

            //if audits not found.
            if (auditsEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.auditss.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForAudits(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((AuditsDto)auditsEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = auditsEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = auditsEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }

        //[ProducesResponseType(StatusCodes.Status201Created)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]       
        //[HttpGet]
        // [AllowAnonymous]
        //[HttpGet("AuditStatusUpdate", Name = "AuditStatusUpdate")]
        
        //public async Task<IActionResult>AuditStatusUpdate(long taskId)
        //{
        //    await _auditsService.AuditStatusUpdate(taskId);

        //    return Ok();
        //}
        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateAudits")]
        public async Task<IActionResult> UpdateAudits(Guid id, [FromBody]AuditsForUpdate AuditsForUpdate)
        {

            //if show not found
            if (!await _auditsService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _auditsService.UpdateEntityAsync(id, AuditsForUpdate);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateAudits(Guid id, [FromBody] JsonPatchDocument<AuditsForUpdate> jsonPatchDocument)
        {
            AuditsForUpdate dto = new AuditsForUpdate();
            Domain.Entities.Audits audits = new Domain.Entities.Audits();

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
            Mapper.Map(dto, audits);

            //set the Id for the show model.
            audits.Id = id;

            //partially update the chnages to the db. 
            await _auditsService.UpdatePartialEntityAsync(audits, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateAudits")]
        public async Task<ActionResult<AuditsDto>> CreateAudits([FromBody]AuditsForCreation audits)
        {
            //create a show in db.
            var auditsToReturn = await _auditsService.CreateEntityAsync<AuditsDto, AuditsForCreation>(audits);

            //return the show created response.
            return CreatedAtRoute("GetAudits", new { id = auditsToReturn.Id }, auditsToReturn);
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("SettingTasks", Name = "SettingTasks")]

        public async Task<bool> SettingTasksWebsite(string websiteUrl,string CompanyID)
        {
            //create a show in db.
           return  await _auditsService.SettingTaskOnDataForSeo(websiteUrl, CompanyID);
           
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("GetOnPageByTaskId", Name = "GetOnPageByTaskId")]

        public async Task<AuditData> GetOnPageByTaskId(long taskID)
        {
            //create a show in db.
            return await _auditsService.GetOnPageByTaskId(taskID);

        }
        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteAuditsById")]
        public async Task<IActionResult> DeleteAuditsById(Guid id)
        {
            //if the audits exists
            if (await _auditsService.ExistAsync(x => x.Id == id))
            {
                //delete the audits from the db.
                await _auditsService.DeleteEntityAsync(id);
            }
            else
            {
                //if audits doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForAudits(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetAudits", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetAudits", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteAuditsById", new { id = id }),
              "delete_audits",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateAudits", new { id = id }),
             "update_audits",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateAudits", new { }),
              "create_audits",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForAuditss(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateAuditssResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateAuditssResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateAuditssResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateAuditssResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredAuditss",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredAuditss",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredAuditss",
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
