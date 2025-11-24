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
    /// PlanDetail endpoint
    /// </summary>
    [Route("api/plandetails")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
    public class PlanDetailController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly IPlanDetailService _plandetailService;
        private ILogger<PlanDetailController> _logger;
        private readonly IUrlHelper _urlHelper;

        #endregion


        #region CONSTRUCTOR

        public PlanDetailController(IPlanDetailService plandetailService, ILogger<PlanDetailController> logger, IUrlHelper urlHelper) 
        {
            _logger = logger;
            _plandetailService = plandetailService;
            _urlHelper = urlHelper;
        }

        #endregion


        #region HTTPGET

        [HttpGet(Name = "GetFilteredPlanDetails")]
        [Produces("application/vnd.tourmanagement.plandetails.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<PlanDetailDto>>> GetFilteredPlanDetails([FromQuery]FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_plandetailService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<PlanDetailDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var plandetailsFromRepo = _plandetailService.getAllPlanDetails();

            return Ok(plandetailsFromRepo);
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.plandetails.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetPlanDetail")]
        public async Task<ActionResult<PlanDetail>> GetPlanDetail(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object plandetailEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetPlanDetail called");

                //then get the whole entity and map it to the Dto.
                plandetailEntity = Mapper.Map<PlanDetailDto>(await _plandetailService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                plandetailEntity = await _plandetailService.GetPartialEntityAsync(id, fields);
            }

            //if plandetail not found.
            if (plandetailEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.plandetails.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForPlanDetail(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((PlanDetailDto)plandetailEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = plandetailEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = plandetailEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }

        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdatePlanDetail")]
        public async Task<IActionResult> UpdatePlanDetail(Guid id, [FromBody]PlanDetailForUpdate PlanDetailForUpdate)
        {

            //if show not found
            if (!await _plandetailService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _plandetailService.UpdateEntityAsync(id, PlanDetailForUpdate);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdatePlanDetail(Guid id, [FromBody] JsonPatchDocument<PlanDetailForUpdate> jsonPatchDocument)
        {
            PlanDetailForUpdate dto = new PlanDetailForUpdate();
            PlanDetail plandetail = new PlanDetail();

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
            Mapper.Map(dto, plandetail);

            //set the Id for the show model.
            plandetail.Id = id;

            //partially update the chnages to the db. 
            await _plandetailService.UpdatePartialEntityAsync(plandetail, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreatePlanDetail")]
        public async Task<ActionResult<PlanDetailDto>> CreatePlanDetail([FromBody]PlanDetailForCreation plandetail)
        {
            //create a show in db.
            var plandetailToReturn = await _plandetailService.CreateEntityAsync<PlanDetailDto, PlanDetailForCreation>(plandetail);

            //return the show created response.
            return CreatedAtRoute("GetPlanDetail", new { id = plandetailToReturn.Id }, plandetailToReturn);
        }

        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeletePlanDetailById")]
        public async Task<IActionResult> DeletePlanDetailById(Guid id)
        {
            //if the plandetail exists
            if (await _plandetailService.ExistAsync(x => x.Id == id))
            {
                //delete the plandetail from the db.
                await _plandetailService.DeleteEntityAsync(id);
            }
            else
            {
                //if plandetail doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForPlanDetail(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetPlanDetail", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetPlanDetail", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeletePlanDetailById", new { id = id }),
              "delete_plandetail",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdatePlanDetail", new { id = id }),
             "update_plandetail",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreatePlanDetail", new { }),
              "create_plandetail",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForPlanDetails(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreatePlanDetailsResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreatePlanDetailsResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreatePlanDetailsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreatePlanDetailsResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredPlanDetails",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredPlanDetails",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredPlanDetails",
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
