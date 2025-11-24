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
using System.Linq;
using IdentityServer4.AccessTokenValidation;

namespace EventManagement.API.Controllers
{
    /// <summary>
    /// ReportScheduling endpoint
    /// </summary>
    [Route("api/reportschedulings")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
    public class ReportSchedulingController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly IReportSchedulingService _reportschedulingService;
        private ILogger<ReportSchedulingController> _logger;
        private readonly IUrlHelper _urlHelper;

        #endregion


        #region CONSTRUCTOR

        public ReportSchedulingController(IReportSchedulingService reportschedulingService, ILogger<ReportSchedulingController> logger, IUrlHelper urlHelper)
        {
            _logger = logger;
            _reportschedulingService = reportschedulingService;
            _urlHelper = urlHelper;
        }

        #endregion


        #region HTTPGET

        [HttpGet("GetReportScheduleByReportId", Name = "GetReportScheduleByReportId")]
        [Produces("application/vnd.tourmanagement.reportsettings.hateoasjson", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ReportSchedulingDto>>> GetReportScheduleByReportId(Guid reportId)
        {
            var reportData = _reportschedulingService.GetAllEntities().Where(x => x.ReportId == reportId).FirstOrDefault();
            if (reportData != null)
            {
                return Ok(reportData);
            }
            else
            {
                return null;
            }
        }

        [HttpGet(Name = "GetFilteredReportSchedulings")]
        [Produces("application/vnd.tourmanagement.reportschedulings.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ReportSchedulingDto>>> GetFilteredReportSchedulings([FromQuery] FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_reportschedulingService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<ReportSchedulingDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var reportschedulingsFromRepo = await _reportschedulingService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.reportschedulings.hateoas+json")
            {
                //create HATEOAS links for each show.
                reportschedulingsFromRepo.ForEach(reportscheduling =>
                {
                    var entityLinks = CreateLinksForReportScheduling(reportscheduling.Id, filterOptionsModel.Fields);
                    reportscheduling.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = reportschedulingsFromRepo.TotalCount,
                    pageSize = reportschedulingsFromRepo.PageSize,
                    currentPage = reportschedulingsFromRepo.CurrentPage,
                    totalPages = reportschedulingsFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForReportSchedulings(filterOptionsModel, reportschedulingsFromRepo.HasNext, reportschedulingsFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = reportschedulingsFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = reportschedulingsFromRepo.HasPrevious ?
                    CreateReportSchedulingsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = reportschedulingsFromRepo.HasNext ?
                    CreateReportSchedulingsResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = reportschedulingsFromRepo.TotalCount,
                    pageSize = reportschedulingsFromRepo.PageSize,
                    currentPage = reportschedulingsFromRepo.CurrentPage,
                    totalPages = reportschedulingsFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(reportschedulingsFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.reportschedulings.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetReportScheduling")]
        public async Task<ActionResult<ReportScheduling>> GetReportScheduling(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object reportschedulingEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetReportScheduling called");

                //then get the whole entity and map it to the Dto.
                reportschedulingEntity = Mapper.Map<ReportSchedulingDto>(await _reportschedulingService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                reportschedulingEntity = await _reportschedulingService.GetPartialEntityAsync(id, fields);
            }

            //if reportscheduling not found.
            if (reportschedulingEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.reportschedulings.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForReportScheduling(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((ReportSchedulingDto)reportschedulingEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = reportschedulingEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = reportschedulingEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }


        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("EmailReportSchedule", Name = "EmailReportSchedule")]
        [AllowAnonymous]
        public async Task<bool> EmailReportSchedule()
        {
            return await _reportschedulingService.EmailReportSchedule();
        }

        //[ProducesResponseType(StatusCodes.Status201Created)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[HttpPost("generate-report", Name = "generate-report")]
        //public async Task<string> GenerateReportAndDownload(GenerateReportDto generateReportDto)
        //{
        //    return await _reportschedulingService.GenerateReportAndDownload(generateReportDto);
        //}

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("generate-report-pdf", Name = "generate-report-pdf")]
        public async Task<string> GenerateReportAndDownloadPdf(GenerateReportDto demo)
        {
            try
            {
                var response = await _reportschedulingService.PreparePdfReport(demo);
                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

  
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("share-report-pdf", Name = "share-report-pdf")]
        public async Task<bool> ShareReportInEmail(ShareReportPdfEmail shareReportPdfEmail)
        {
            try
            {
                var response = await _reportschedulingService.SendReportInEmailFronEnd(shareReportPdfEmail);
                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateReportScheduling")]
        public async Task<IActionResult> UpdateReportScheduling(Guid id, [FromBody] ReportSchedulingForUpdate ReportSchedulingForUpdate)
        {

            //if show not found
            if (!await _reportschedulingService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _reportschedulingService.UpdateEntityAsync(id, ReportSchedulingForUpdate);

            //return the response.
            return CreatedAtRoute("GetReportScheduling", new { id = ReportSchedulingForUpdate.Id }, ReportSchedulingForUpdate);
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateReportScheduling(Guid id, [FromBody] JsonPatchDocument<ReportSchedulingForUpdate> jsonPatchDocument)
        {
            ReportSchedulingForUpdate dto = new ReportSchedulingForUpdate();
            ReportScheduling reportscheduling = new ReportScheduling();

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
            Mapper.Map(dto, reportscheduling);

            //set the Id for the show model.
            reportscheduling.Id = id;

            //partially update the chnages to the db. 
            await _reportschedulingService.UpdatePartialEntityAsync(reportscheduling, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateReportScheduling")]
        public async Task<ActionResult<ReportSchedulingDto>> CreateReportScheduling([FromBody] ReportSchedulingForCreation reportscheduling)
        {
            //create a show in db.
            var reportschedulingToReturn = await _reportschedulingService.CreateEntityAsync<ReportSchedulingDto, ReportSchedulingForCreation>(reportscheduling);

            //return the show created response.
            return CreatedAtRoute("GetReportScheduling", new { id = reportschedulingToReturn.Id }, reportschedulingToReturn);
        }

        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteReportSchedulingById")]
        public async Task<IActionResult> DeleteReportSchedulingById(Guid id)
        {
            //if the reportscheduling exists
            if (await _reportschedulingService.ExistAsync(x => x.Id == id))
            {
                //delete the reportscheduling from the db.
                await _reportschedulingService.DeleteEntityAsync(id);
            }
            else
            {
                //if reportscheduling doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForReportScheduling(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetReportScheduling", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetReportScheduling", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteReportSchedulingById", new { id = id }),
              "delete_reportscheduling",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateReportScheduling", new { id = id }),
             "update_reportscheduling",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateReportScheduling", new { }),
              "create_reportscheduling",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForReportSchedulings(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateReportSchedulingsResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateReportSchedulingsResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateReportSchedulingsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateReportSchedulingsResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredReportSchedulings",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredReportSchedulings",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredReportSchedulings",
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
