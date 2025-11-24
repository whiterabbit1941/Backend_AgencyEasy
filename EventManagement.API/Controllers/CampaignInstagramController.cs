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
    /// CampaignInstagram endpoint
    /// </summary>
    [Route("api/campaigninstagrams")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
    public class CampaignInstagramController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly ICampaignInstagramService _campaigninstagramService;
        private ILogger<CampaignInstagramController> _logger;
        private readonly IUrlHelper _urlHelper;
        static string _accessToken;

        #endregion


        #region CONSTRUCTOR

        public CampaignInstagramController(ICampaignInstagramService campaigninstagramService, ILogger<CampaignInstagramController> logger, IUrlHelper urlHelper) 
        {
            _logger = logger;
            _campaigninstagramService = campaigninstagramService;
            _urlHelper = urlHelper;
        }

        #endregion


        #region HTTPGET

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("GetCampaignInstagramDataById", Name = "GetCampaignInstagramDataById")]
        public async Task<ActionResult<InstagramReportsData>> GetCampaignInstagramDataById(Guid campaignId, string startDate, string endDate)
        {
            var instaData = await _campaigninstagramService.GetInstagramReportDataById(campaignId, startDate, endDate);

            return Ok(instaData);
        }

        [HttpGet(Name = "GetFilteredCampaignInstagrams")]
        [Produces("application/vnd.tourmanagement.campaigninstagrams.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CampaignInstagramDto>>> GetFilteredCampaignInstagrams([FromQuery]FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_campaigninstagramService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<CampaignInstagramDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }
            try
            {
                //get the paged/filtered show from db. 
                var campaigninstagramsFromRepo = await _campaigninstagramService.GetFilteredEntities(filterOptionsModel);
                //if HATEOAS links are required.
                if (mediaType == "application/vnd.tourmanagement.campaigninstagrams.hateoas+json")
                {
                    //create HATEOAS links for each show.
                    campaigninstagramsFromRepo.ForEach(campaigninstagram =>
                    {
                        var entityLinks = CreateLinksForCampaignInstagram(campaigninstagram.Id, filterOptionsModel.Fields);
                        campaigninstagram.links = entityLinks;
                    });

                    //prepare pagination metadata.
                    var paginationMetadata = new
                    {
                        totalCount = campaigninstagramsFromRepo.TotalCount,
                        pageSize = campaigninstagramsFromRepo.PageSize,
                        currentPage = campaigninstagramsFromRepo.CurrentPage,
                        totalPages = campaigninstagramsFromRepo.TotalPages,
                    };

                    //add pagination meta data to response header.
                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                    //create links for shows.
                    var links = CreateLinksForCampaignInstagrams(filterOptionsModel, campaigninstagramsFromRepo.HasNext, campaigninstagramsFromRepo.HasPrevious);

                    //prepare model with data and HATEOAS links.
                    var linkedCollectionResource = new
                    {
                        value = campaigninstagramsFromRepo,
                        links = links
                    };

                    //return the data with Ok response.
                    return Ok(linkedCollectionResource);
                }
                else
                {
                    var previousPageLink = campaigninstagramsFromRepo.HasPrevious ?
                        CreateCampaignInstagramsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                    var nextPageLink = campaigninstagramsFromRepo.HasNext ?
                        CreateCampaignInstagramsResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                    //prepare the pagination metadata.
                    var paginationMetadata = new
                    {
                        previousPageLink = previousPageLink,
                        nextPageLink = nextPageLink,
                        totalCount = campaigninstagramsFromRepo.TotalCount,
                        pageSize = campaigninstagramsFromRepo.PageSize,
                        currentPage = campaigninstagramsFromRepo.CurrentPage,
                        totalPages = campaigninstagramsFromRepo.TotalPages
                    };

                    //add pagination meta data to response header.
                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                    //return the data with Ok response.
                    return Ok(campaigninstagramsFromRepo);
                }
                
            }
            catch (Exception ex)
            {
                var test = ex;
            }
            return Ok(new CampaignInstagramDto());

        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.campaigninstagrams.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetCampaignInstagram")]
        public async Task<ActionResult<CampaignInstagram>> GetCampaignInstagram(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object campaigninstagramEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetCampaignInstagram called");

                //then get the whole entity and map it to the Dto.
                campaigninstagramEntity = Mapper.Map<CampaignInstagramDto>(await _campaigninstagramService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                campaigninstagramEntity = await _campaigninstagramService.GetPartialEntityAsync(id, fields);
            }

            //if campaigninstagram not found.
            if (campaigninstagramEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.campaigninstagrams.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForCampaignInstagram(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((CampaignInstagramDto)campaigninstagramEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = campaigninstagramEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = campaigninstagramEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }


        [HttpGet("CheckInstagramIntegratedByCampaignId", Name = "CheckInstagramIntegratedByCampaignId")]
        public async Task<bool> CheckInstagramIntegratedByCampaignId(string campaignId)
        {
            var isExists = _campaigninstagramService.GetAllEntities().Where(x => x.CampaignID.ToString() == campaignId).Any();
            if (isExists)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateCampaignInstagram")]
        public async Task<IActionResult> UpdateCampaignInstagram(Guid id, [FromBody]CampaignInstagramForUpdate CampaignInstagramForUpdate)
        {

            //if show not found
            if (!await _campaigninstagramService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _campaigninstagramService.UpdateEntityAsync(id, CampaignInstagramForUpdate);

            //return the response.
            return NoContent();
        }

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("UpdateCampaignInstagramByCampaignId", Name = "UpdateCampaignInstagramByCampaignId")]
        public async Task<IActionResult> UpdateCampaignInstagramByCampaignId([FromBody] CampaignInstagramForUpdate CampaignInstagramForUpdate)
        {
            var returnValue = "";
            try
            {
                var temp = await _campaigninstagramService.UpdateBulkEntityAsync(x => new CampaignInstagram { UrlOrName = CampaignInstagramForUpdate.UrlOrName }, y => y.CampaignID == CampaignInstagramForUpdate.CampaignID);

                if(temp == 1)
                {
                    returnValue = temp.ToString();
                }
                else
                {
                    returnValue = temp.ToString();
                }
              
            }
            catch (Exception ex)
            {
                throw ex;
            }

            //return the response.
            return Ok(returnValue);
        }
        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateCampaignInstagram(Guid id, [FromBody] JsonPatchDocument<CampaignInstagramForUpdate> jsonPatchDocument)
        {
            CampaignInstagramForUpdate dto = new CampaignInstagramForUpdate();
            CampaignInstagram campaigninstagram = new CampaignInstagram();

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
            Mapper.Map(dto, campaigninstagram);

            //set the Id for the show model.
            campaigninstagram.Id = id;

            //partially update the chnages to the db. 
            await _campaigninstagramService.UpdatePartialEntityAsync(campaigninstagram, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateCampaignInstagram")]
        public async Task<ActionResult<CampaignInstagramDto>> CreateCampaignInstagram([FromBody]CampaignInstagramForCreation campaigninstagram)
        {
            campaigninstagram.AccessToken = _accessToken;
            //create a show in db.
            var campaigninstagramToReturn = await _campaigninstagramService.CreateEntityAsync<CampaignInstagramDto, CampaignInstagramForCreation>(campaigninstagram);

            //return the show created response.
            return Ok(campaigninstagramToReturn);//CreatedAtRoute("GetCampaignInstagram", new { id = campaigninstagramToReturn.Id }, campaigninstagramToReturn);
        }

        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteCampaignInstagramById")]
        public async Task<IActionResult> DeleteCampaignInstagramById(Guid id)
        {
            //if the campaigninstagram exists
            if (await _campaigninstagramService.ExistAsync(x => x.Id == id))
            {
                //delete the campaigninstagram from the db.
                await _campaigninstagramService.DeleteEntityAsync(id);
            }
            else
            {
                //if campaigninstagram doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForCampaignInstagram(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCampaignInstagram", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCampaignInstagram", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteCampaignInstagramById", new { id = id }),
              "delete_campaigninstagram",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateCampaignInstagram", new { id = id }),
             "update_campaigninstagram",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateCampaignInstagram", new { }),
              "create_campaigninstagram",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForCampaignInstagrams(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateCampaignInstagramsResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateCampaignInstagramsResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateCampaignInstagramsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateCampaignInstagramsResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredCampaignInstagrams",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredCampaignInstagrams",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredCampaignInstagrams",
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
