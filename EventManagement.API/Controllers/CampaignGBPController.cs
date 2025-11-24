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
    /// CampaignGBP endpoint
    /// </summary>
    [Route("api/campaigngbps")]
    [Produces("application/json")]
    [ApiController]
    public class CampaignGBPController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly ICampaignGBPService _campaigngbpService;
        private ILogger<CampaignGBPController> _logger;
        private readonly IUrlHelper _urlHelper;

        #endregion


        #region CONSTRUCTOR

        public CampaignGBPController(ICampaignGBPService campaigngbpService, ILogger<CampaignGBPController> logger, IUrlHelper urlHelper) 
        {
            _logger = logger;
            _campaigngbpService = campaigngbpService;
            _urlHelper = urlHelper;
        }

        #endregion


        #region HTTPGET

        [HttpGet(Name = "GetFilteredCampaignGBPs")]
        [Produces("application/vnd.tourmanagement.campaigngbps.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CampaignGBPDto>>> GetFilteredCampaignGBPs([FromQuery]FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_campaigngbpService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<CampaignGBPDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var campaigngbpsFromRepo = await _campaigngbpService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.campaigngbps.hateoas+json")
            {
                //create HATEOAS links for each show.
                campaigngbpsFromRepo.ForEach(campaigngbp =>
                {
                    var entityLinks = CreateLinksForCampaignGBP(campaigngbp.Id, filterOptionsModel.Fields);
                    campaigngbp.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = campaigngbpsFromRepo.TotalCount,
                    pageSize = campaigngbpsFromRepo.PageSize,
                    currentPage = campaigngbpsFromRepo.CurrentPage,
                    totalPages = campaigngbpsFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForCampaignGBPs(filterOptionsModel, campaigngbpsFromRepo.HasNext, campaigngbpsFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = campaigngbpsFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = campaigngbpsFromRepo.HasPrevious ?
                    CreateCampaignGBPsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = campaigngbpsFromRepo.HasNext ?
                    CreateCampaignGBPsResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = campaigngbpsFromRepo.TotalCount,
                    pageSize = campaigngbpsFromRepo.PageSize,
                    currentPage = campaigngbpsFromRepo.CurrentPage,
                    totalPages = campaigngbpsFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(campaigngbpsFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.campaigngbps.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetCampaignGBP")]
        public async Task<ActionResult<CampaignGBP>> GetCampaignGBP(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object campaigngbpEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetCampaignGBP called");

                //then get the whole entity and map it to the Dto.
                campaigngbpEntity = Mapper.Map<CampaignGBPDto>(await _campaigngbpService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                campaigngbpEntity = await _campaigngbpService.GetPartialEntityAsync(id, fields);
            }

            //if campaigngbp not found.
            if (campaigngbpEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.campaigngbps.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForCampaignGBP(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((CampaignGBPDto)campaigngbpEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = campaigngbpEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = campaigngbpEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }


        [HttpGet("GetGbpLocationList", Name = "GetGbpLocationList")]        
        public async Task<List<GbpLocation>> GetGSCList(Guid campaignId)
        {
            var list = new RootObjectOfGSCList();
            try
            {
                var res = await _campaigngbpService.GetLocationList(campaignId);
                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet("GetGbpPerformance", Name = "GetGbpPerformance")]
        public async Task<RootGbpData> GetGbpPerformance(Guid campaignId,string startTime,string endTime)
        {
            var list = new RootObjectOfGSCList();
            try
            {
                var res = await _campaigngbpService.GetGbpPerformanceData(campaignId, startTime, endTime);
                return res;
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
        [HttpPut("{id}", Name = "UpdateCampaignGBP")]
        public async Task<IActionResult> UpdateCampaignGBP(Guid id, [FromBody]CampaignGBPForUpdate CampaignGBPForUpdate)
        {

            //if show not found
            if (!await _campaigngbpService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _campaigngbpService.UpdateEntityAsync(id, CampaignGBPForUpdate);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateCampaignGBP(Guid id, [FromBody] JsonPatchDocument<CampaignGBPForUpdate> jsonPatchDocument)
        {
            CampaignGBPForUpdate dto = new CampaignGBPForUpdate();
            CampaignGBP campaigngbp = new CampaignGBP();

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
            Mapper.Map(dto, campaigngbp);

            //set the Id for the show model.
            campaigngbp.Id = id;

            //partially update the chnages to the db. 
            await _campaigngbpService.UpdatePartialEntityAsync(campaigngbp, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateCampaignGBP")]
        public async Task<ActionResult<CampaignGBPDto>> CreateCampaignGBP([FromBody]CampaignGBPForCreation campaigngbp)
        {
            //create a show in db.
            var campaigngbpToReturn = await _campaigngbpService.CreateEntityAsync<CampaignGBPDto, CampaignGBPForCreation>(campaigngbp);

            //return the show created response.
            return CreatedAtRoute("GetCampaignGBP", new { id = campaigngbpToReturn.Id }, campaigngbpToReturn);
        }

        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteCampaignGBPById")]
        public async Task<IActionResult> DeleteCampaignGBPById(Guid id)
        {
            //if the campaigngbp exists
            if (await _campaigngbpService.ExistAsync(x => x.Id == id))
            {
                //delete the campaigngbp from the db.
                await _campaigngbpService.DeleteEntityAsync(id);
            }
            else
            {
                //if campaigngbp doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForCampaignGBP(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCampaignGBP", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCampaignGBP", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteCampaignGBPById", new { id = id }),
              "delete_campaigngbp",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateCampaignGBP", new { id = id }),
             "update_campaigngbp",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateCampaignGBP", new { }),
              "create_campaigngbp",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForCampaignGBPs(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateCampaignGBPsResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateCampaignGBPsResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateCampaignGBPsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateCampaignGBPsResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredCampaignGBPs",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredCampaignGBPs",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredCampaignGBPs",
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
