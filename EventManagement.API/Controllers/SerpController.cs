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
using System.Net.Http;
using Newtonsoft.Json.Linq;
using SendGrid;
using SendGrid.Helpers.Mail;
using IdentityServer4.AccessTokenValidation;
using EventManagement.Domain;
using Amazon.Lambda.Core;
using Google.Apis.Pagespeedonline.v1.Data;
using System.IO;
using System.Text;
using System.Net.Http.Headers;
using AutoMapper.Configuration;
using EventManagement.Domain.Migrations;
using Serp = EventManagement.Domain.Entities.Serp;

namespace EventManagement.API.Controllers
{
    /// <summary>
    /// Serp endpoint
    /// </summary>
    [Route("api/serps")]
    //[Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]

    public class SerpController : Controller
    {

        #region PRIVATE MEMBERS
        private readonly ISerpService _serpService;
        private ILogger<SerpController> _logger;
        private readonly IUrlHelper _urlHelper;
        private readonly ISerpRepository _serpRepository;

        #endregion


        #region CONSTRUCTOR

        public SerpController(ISerpRepository serpRepository, ISerpService serpService,
            ILogger<SerpController> logger, IUrlHelper urlHelper)
        {
            _logger = logger;
            _serpService = serpService;
            _urlHelper = urlHelper;
            _serpRepository = serpRepository;
        }

        #endregion


        #region HTTPGET

        [HttpGet(Name = "GetFilteredSerps")]
        [Produces("application/vnd.tourmanagement.serps.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<SerpDto>>> GetFilteredSerps([FromQuery] FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_serpService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<SerpDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }
            //get the paged/filtered show from db. 
            var serpsFromRepo = await _serpService.GetFilteredEntities(filterOptionsModel);

            return Ok(serpsFromRepo);
        }

        [HttpGet("GetAllSerps", Name = "GetAllSerps")]
        [Produces("application/vnd.tourmanagement.serps.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<SerpDto>>> GetAllSerps([FromQuery] FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType, string fromDate, string toDate)
        {

            //if order by fields are not valid.
            if (!_serpService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<SerpDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }
            var CampaignId = Guid.Parse(filterOptionsModel.SearchQuery.Split('"')[1]);
            //get the paged/filtered show from db. 
            //var serpsFromRepo = await _serpService.GetFilteredEntities(filterOptionsModel);
            var serpsFromRepo1 = _serpRepository.GetAllEntities(true).Where(x => x.CampaignID == CampaignId && x.CreatedOn >= DateTime.Parse(fromDate).ToUniversalTime() && x.CreatedOn <= DateTime.Parse(toDate).ToUniversalTime()).Select(y => new SerpDto
            {
                Id = y.Id,
                CampaignID = y.CampaignID.ToString(),
                Position = y.Position,
                LocalPackCount = y.LocalPackCount,
                Searches = y.Searches,
                Location = y.Location,
                Keywords = y.Keywords,
                UpdatedOn = y.UpdatedOn,
                CreatedOn = y.CreatedOn,
                LocationName = y.LocationName,
                LocalPacksStatus = y.LocalPacksStatus,
            })
            .ToList();

            return Ok(serpsFromRepo1);
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("get-keyword-count", Name = "get-keyword-count")]

        public ActionResult<int> GetKeywordCount(string campaignID)
        {
            return _serpService.GetKeywordsCount(campaignID);
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("get-company-keyword-count", Name = "get-company-keyword-count")]

        public ActionResult<int> GetKeywordCountByCompanyId(Guid companyId)
        {
            return _serpService.GetCompanyKeywordsCount(companyId);
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.serps.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetSerp")]
        public async Task<ActionResult<Serp>> GetSerp(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType, string startDate, string endDate)
        {
            object serpEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetSerp called");

                //then get the whole entity and map it to the Dto.
                serpEntity = Mapper.Map<SerpDto>(await _serpService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                serpEntity = await _serpService.GetPartialEntityAsync(id, fields);
            }

            //if serp not found.
            if (serpEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            //return the Ok response.
            return Ok(serpEntity);
        }


        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("get-unique-keyword", Name = "get-unique-keyword")]
        public ActionResult<List<SerpDto>> GetUniqueKeywordByProject(string campaignID)
        {
            return _serpService.GetUniqueKeywordByProject(campaignID);
        }

        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateSerp")]
        public async Task<IActionResult> UpdateSerp(Guid id, [FromBody] SerpForUpdate SerpForUpdate)
        {

            //if show not found
            if (!await _serpService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _serpService.UpdateEntityAsync(id, SerpForUpdate);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateSerp(Guid id, [FromBody] JsonPatchDocument<SerpForUpdate> jsonPatchDocument)
        {
            SerpForUpdate dto = new SerpForUpdate();
            Serp serp = new Serp();

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
            Mapper.Map(dto, serp);

            //set the Id for the show model.
            serp.Id = id;

            //partially update the chnages to the db. 
            await _serpService.UpdatePartialEntityAsync(serp, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateSerp")]
        public async Task<ActionResult<SerpDto>> CreateSerp([FromBody] SerpForCreation serp)
        {
            //create a show in db.
            var serpToReturn = await _serpService.CreateEntityAsync<SerpDto, SerpForCreation>(serp);

            //return the show created response.
            return CreatedAtRoute("GetSerp", new { id = serpToReturn.Id }, serpToReturn);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="campaignID">  campaignID </param>
        /// <param name="keywords">  list of keywords </param>
        /// <param name="location">location</param>
        /// <param name="locationName">locationName </param>
        /// <param name=""></param>
        /// <returns> ExistNotExistKeyword </returns>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("GetKeywordExistOrNot", Name = "GetKeywordExistOrNot")]

        public async Task<ExistNotExistKeyword> GetKeywordExistOrNot(string campaignID, [FromQuery] List<string> keywords, string location, string locationName)
        {
            //create a show in db.
            var joinKeywords = string.Join(',', keywords);
            List<string> listOfKeywords = joinKeywords.Split(',').ToList();

            //List<string> List = keywords.
            return await _serpService.GetExistNotExistKeyword(campaignID, location, locationName, listOfKeywords);
        }




        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("GetSerpData", Name = "GetSerpData")]

        public async Task<bool> GetSerpData(string campaignID, string location, string locationName, [FromQuery] List<string> keywords, string businessName, bool localPacksStatus, string searchParam)
        {
            //create a show in db.
            var str = string.Join(',', keywords);
            List<string> listOfKeywords = str.Split(',').ToList();

            foreach (var keyword in listOfKeywords)
            {
                await _serpService.GetSerpData(campaignID, location, locationName, keyword, businessName, localPacksStatus, searchParam);
            }

            return true;
            //return await _serpService.GetExistKeyword(campaignID,location, locationName, listOfKeywords);
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("GetSerpLocationData", Name = "GetSerpLocationData")]

        public async Task<JArray> GetSerpLocationData(string location)
        {
            return await _serpService.GetSerpLocationData(location);
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("GetUpdateKeywordsStatus", Name = "GetUpdateKeywordsStatus")]

        public async Task<string> GetUpdateKeywordsStatus()
        {
            var response = await _serpService.UpdateKeywordsStatus();
            LambdaLogger.Log(response);
            return response;
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("SerpWebhook", Name = "SerpWebhook")]
        public async Task<ActionResult> SerpWebhook(string id, string tag)
        {
            try
            {
                var new_id = new Guid(id);
                if (!string.IsNullOrEmpty(tag))
                {

                    var tagData = tag.Split("***");

                    var webUrl = tagData[1];

                    var businessName = tagData[0];

                    long localPackCount = 0;

                    var httpClient = _serpService.GetDataForSeoClient();

                    var getSerpResult = await httpClient.GetAsync("https://api.dataforseo.com/v3/serp/google/organic/task_get/advanced/" + id);

                    var strRequestBody = await getSerpResult.Content.ReadAsStringAsync();
                    var desrializeResult = JsonConvert.DeserializeObject<SerpRes>(strRequestBody);

                    if (desrializeResult.StatusCode == 20000 && desrializeResult.SerpTasks[0].StatusCode == 20000)
                    {

                        if (!string.IsNullOrEmpty(webUrl) && webUrl.Contains("https://"))
                        {
                            webUrl = webUrl.Replace("https://", "");
                        }

                        if (!string.IsNullOrEmpty(webUrl) && webUrl.Contains("http://"))
                        {
                            webUrl = webUrl.Replace("https://", "");
                        }

                        if (!string.IsNullOrEmpty(webUrl) && webUrl.Contains("/"))
                        {
                            webUrl = webUrl.Replace("/", "");
                        }

                        var position = (long)desrializeResult.SerpTasks[0].Result[0].Items.Where(y => (y.Type == "organic" || y.Type == "paid") && y.Url.AbsoluteUri.Contains(webUrl)).Select(x => x.RankAbsolute).FirstOrDefault();

                        //If localpacks is not include
                        if (!string.IsNullOrWhiteSpace(businessName))
                        {
                            localPackCount = (long)desrializeResult.SerpTasks[0].Result[0].Items.Where(y => y.Type == "local_pack" && y.Title.ToLower() == businessName.Trim().ToLower()).Select(x => x.RankGroup).FirstOrDefault();
                        }

                        var searches = desrializeResult.SerpTasks[0].Result[0].Searches;
                        // you can find the full list of the response codes here https://docs.dataforseo.com/v3/appendix/errors

                        var serpDetails = new SerpDetails();
                        serpDetails.Searchs = desrializeResult.SerpTasks[0].Result[0].Searches;
                        serpDetails.Position = position;
                        //serpDetails.CampaignId = campaignId;
                        serpDetails.TaskId = new_id;
                        serpDetails.LocalPacksCount = localPackCount;

                        await _serpService.UpdateSerpData(serpDetails);
                    }
                    else
                    {
                        await _serpService.UpdateBulkEntityAsync(x => new Serp { LambdaLogger = desrializeResult.StatusCode.ToString(), UpdatedOn = DateTime.UtcNow }, y => y.TaskId == new_id);
                    }

                    return StatusCode(200);
                }
                else
                {
                    return StatusCode(404);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong: " + ex.Message);
            }

        }


        //method are used for deleting the multiple keywords

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("DeleteSerpByMultipleId", Name = "DeleteSerpByMultipleId")]
        public async Task<int> DeleteSerpByMultipleId(List<Guid> ids)
        {
            int returnData = -1;
            //if the serp exists
            if (await _serpService.ExistAsync(x => ids.Contains(x.Id)))
            {
                //delete the serp from the db.  
                returnData = await _serpService.DeleteBulkEntityAsync(x => ids.Contains(x.Id));
            }
            //return the response.
            return returnData;
        }

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("delete-serp-keywords", Name = "delete-serp-keywords")]
        public async Task<IActionResult> DeleteSerpKeywordsForDowngrade(List<Guid> ids)
        {
            try
            {
                foreach (var id in ids)
                {
                    var serp = await _serpService.GetEntityByIdAsync(id);
                    if (serp != null)
                    {
                        //delete the serp from the db.
                        await _serpService.DeleteBulkEntityAsync(x => x.Keywords.Contains(serp.Keywords) && x.CampaignID.Equals(serp.CampaignID) && x.Location.Equals(x.Location));
                    }
                }

                //return the response.
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong: " + ex.Message);
            }
        }

        #endregion
        //method that will be called bu web job for keywords insert


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteSerpById")]
        public async Task<IActionResult> DeleteSerpById(Guid id)
        {
            //if the serp exists
            if (await _serpService.ExistAsync(x => x.Id == id))
            {
                //delete the serp from the db.
                await _serpService.DeleteEntityAsync(id);
            }
            else
            {
                //if serp doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }


        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForSerp(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetSerp", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetSerp", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteSerpById", new { id = id }),
              "delete_serp",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateSerp", new { id = id }),
             "update_serp",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateSerp", new { }),
              "create_serp",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForSerps(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateSerpsResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateSerpsResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateSerpsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateSerpsResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredSerps",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredSerps",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredSerps",
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
