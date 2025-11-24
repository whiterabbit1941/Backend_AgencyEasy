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
using System.Net;
using SendGrid;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace EventManagement.API.Controllers
{
    /// <summary>
    /// EmailWhitelabel endpoint
    /// </summary>
    [Route("api/emailwhitelabels")]
    [Produces("application/json")]
    [ApiController]
    public class EmailWhitelabelController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly IEmailWhitelabelService _emailwhitelabelService;
        private ILogger<EmailWhitelabelController> _logger;
        private readonly IUrlHelper _urlHelper;
        private IConfiguration _configuration;

        #endregion


        #region CONSTRUCTOR

        public EmailWhitelabelController(IConfiguration configuration, IEmailWhitelabelService emailwhitelabelService, ILogger<EmailWhitelabelController> logger, IUrlHelper urlHelper)
        {
            _logger = logger;
            _emailwhitelabelService = emailwhitelabelService;
            _urlHelper = urlHelper;
            _configuration = configuration;
        }

        #endregion


        #region HTTPGET

        private SendGridClient GetTransportMechanism()
        {
            return new SendGridClient(_configuration.GetSection("Client").Value);
        }


        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("VerifyDomainsInSendgrid", Name = "VerifyDomainsInSendgrid")]
        public async Task<ActionResult<DomainWhiteLabelDTO>> VerifyDomainsInSendgrid(string domainID)
        {
            var domainId = long.Parse(domainID);
            //verify domain
            var transportInstance = GetTransportMechanism();
            var VerifyDomainResponse = await transportInstance.RequestAsync(method: SendGridClient.Method.POST, urlPath: "whitelabel/domains/" + domainId + "/validate");
            var DomainWhiteLabelDTO = JsonConvert.DeserializeObject<DomainWhiteLabelDTO>(VerifyDomainResponse.Body.ReadAsStringAsync().Result);
            if (VerifyDomainResponse.StatusCode == HttpStatusCode.OK)
            {
                if (DomainWhiteLabelDTO.valid &&
                DomainWhiteLabelDTO.validation_results.mail_cname.valid &&
                DomainWhiteLabelDTO.validation_results.dkim1.valid &&
                DomainWhiteLabelDTO.validation_results.dkim2.valid
                )
                {
                    //verified
                    await _emailwhitelabelService.UpdateBulkEntityAsync(x => new EmailWhitelabel { IsVerify = true, UpdatedOn = DateTime.UtcNow }, y => y.DomainID == domainId);

                    return DomainWhiteLabelDTO;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("GetDomainByCompanyID", Name = "GetDomainByCompanyID")]
        public async Task<ActionResult<List<EmailWhitelabel>>> GetDomainByCompanyID(Guid CompanyID)
        {
            var domainInfo = _emailwhitelabelService.GetAllEntities().Where(x => x.CompanyID == CompanyID).ToList();
            return Ok(domainInfo);
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("GetVerifyDomainByCompanyID", Name = "GetVerifyDomainByCompanyID")]
        [AllowAnonymous]
        public async Task<ActionResult<List<EmailWhitelabel>>> GetVerifyDomainByCompanyID(Guid CompanyID)
        {
            var domainInfo = _emailwhitelabelService.GetAllEntities().Where(x => x.CompanyID == CompanyID && x.IsVerify == true).ToList();
            return Ok(domainInfo);
        }

        [HttpGet(Name = "GetFilteredEmailWhitelabels")]
        [Produces("application/vnd.tourmanagement.emailwhitelabels.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<EmailWhitelabelDto>>> GetFilteredEmailWhitelabels([FromQuery] FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_emailwhitelabelService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<EmailWhitelabelDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var emailwhitelabelsFromRepo = await _emailwhitelabelService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.emailwhitelabels.hateoas+json")
            {
                //create HATEOAS links for each show.
                emailwhitelabelsFromRepo.ForEach(emailwhitelabel =>
                {
                    var entityLinks = CreateLinksForEmailWhitelabel(emailwhitelabel.Id, filterOptionsModel.Fields);
                    emailwhitelabel.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = emailwhitelabelsFromRepo.TotalCount,
                    pageSize = emailwhitelabelsFromRepo.PageSize,
                    currentPage = emailwhitelabelsFromRepo.CurrentPage,
                    totalPages = emailwhitelabelsFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForEmailWhitelabels(filterOptionsModel, emailwhitelabelsFromRepo.HasNext, emailwhitelabelsFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = emailwhitelabelsFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = emailwhitelabelsFromRepo.HasPrevious ?
                    CreateEmailWhitelabelsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = emailwhitelabelsFromRepo.HasNext ?
                    CreateEmailWhitelabelsResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = emailwhitelabelsFromRepo.TotalCount,
                    pageSize = emailwhitelabelsFromRepo.PageSize,
                    currentPage = emailwhitelabelsFromRepo.CurrentPage,
                    totalPages = emailwhitelabelsFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(emailwhitelabelsFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.emailwhitelabels.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetEmailWhitelabel")]
        public async Task<ActionResult<EmailWhitelabel>> GetEmailWhitelabel(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object emailwhitelabelEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetEmailWhitelabel called");

                //then get the whole entity and map it to the Dto.
                emailwhitelabelEntity = Mapper.Map<EmailWhitelabelDto>(await _emailwhitelabelService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                emailwhitelabelEntity = await _emailwhitelabelService.GetPartialEntityAsync(id, fields);
            }

            //if emailwhitelabel not found.
            if (emailwhitelabelEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.emailwhitelabels.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForEmailWhitelabel(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((EmailWhitelabelDto)emailwhitelabelEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = emailwhitelabelEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = emailwhitelabelEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }

        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateEmailWhitelabel")]
        public async Task<IActionResult> UpdateEmailWhitelabel(Guid id, [FromBody] EmailWhitelabelForUpdate EmailWhitelabelForUpdate)
        {

            //if show not found
            if (!await _emailwhitelabelService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _emailwhitelabelService.UpdateEntityAsync(id, EmailWhitelabelForUpdate);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateEmailWhitelabel(Guid id, [FromBody] JsonPatchDocument<EmailWhitelabelForUpdate> jsonPatchDocument)
        {
            EmailWhitelabelForUpdate dto = new EmailWhitelabelForUpdate();
            EmailWhitelabel emailwhitelabel = new EmailWhitelabel();

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
            Mapper.Map(dto, emailwhitelabel);

            //set the Id for the show model.
            emailwhitelabel.Id = id;

            //partially update the chnages to the db. 
            await _emailwhitelabelService.UpdatePartialEntityAsync(emailwhitelabel, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("CreateDomainInSendgrid", Name = "CreateDomainInSendgrid")]
        public async Task<int> CreateDomainInSendgrid(DomainWhiteLabelDTO domainsDTO, Guid companyID)
        {
            var retval = 1;

            //create an instance of the SMTP transport mechanism
            var transportInstance = GetTransportMechanism();

            foreach (var domain in domainsDTO.domains)
            {
                var data = "{'domain':'" + domain + "'}";
                Object json = JsonConvert.DeserializeObject<Object>(data);
                data = json.ToString();

                //Prevent Rate limiting
                await Task.Delay(2000);

                //create domain
                var CreateDomainResponse = await transportInstance.RequestAsync(method: SendGridClient.Method.POST, urlPath: "whitelabel/domains", requestBody: data);
                var DomainWhiteLabelDTO = JsonConvert.DeserializeObject<DomainWhiteLabelDTO>(CreateDomainResponse.Body.ReadAsStringAsync().Result);

                if (CreateDomainResponse.StatusCode == HttpStatusCode.Created && DomainWhiteLabelDTO != null)
                {
                    string CnameHost = DomainWhiteLabelDTO.dns.mail_cname.host.Substring(0, DomainWhiteLabelDTO.dns.mail_cname.host.IndexOf('.'));

                    var DomainWhitelabel = new EmailWhitelabel
                    {
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = "system",
                        UpdatedOn = DateTime.UtcNow,
                        UpdatedBy = "system",
                        CompanyID = companyID,
                        DomainID = DomainWhiteLabelDTO.id,
                        DomainName = DomainWhiteLabelDTO.domain,
                        CnameHost = CnameHost,
                        CnamePointsTo = DomainWhiteLabelDTO.dns.mail_cname.data,
                        DomainKey1PointsTo = DomainWhiteLabelDTO.dns.dkim1.data,
                        DomainKey2PointsTo = DomainWhiteLabelDTO.dns.dkim2.data,
                        CnameType = DomainWhiteLabelDTO.dns.mail_cname.type,
                        DomainKey1Type = DomainWhiteLabelDTO.dns.dkim1.type,
                        DomainKey2Type = DomainWhiteLabelDTO.dns.dkim2.type,
                        IsVerify = false
                    };
                    _emailwhitelabelService.CreateEntity(DomainWhitelabel);
                    _emailwhitelabelService.SaveChanges();
                }
                else
                {
                    return 0;
                }
            }
            return retval;
        }

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateEmailWhitelabel")]
        public async Task<ActionResult<EmailWhitelabelDto>> CreateEmailWhitelabel([FromBody] EmailWhitelabelForCreation emailwhitelabel)
        {
            //create a show in db.
            var emailwhitelabelToReturn = await _emailwhitelabelService.CreateEntityAsync<EmailWhitelabelDto, EmailWhitelabelForCreation>(emailwhitelabel);

            //return the show created response.
            return CreatedAtRoute("GetEmailWhitelabel", new { id = emailwhitelabelToReturn.Id }, emailwhitelabelToReturn);
        }

        #endregion


        #region HTTPDELETE

        /// <summary>
        /// Delete the domain from sendgrid as well as DB
        /// </summary>
        /// <param name="id">domainID</param>
        /// <returns>status of the operations</returns>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("DeleteDomainByID/{domainID}", Name = "DeleteDomainByID")]
        public async Task<int> DeleteDomainByID(long domainID)
        {
            var transportInstance = GetTransportMechanism();
            var DeleteResponse = await transportInstance.RequestAsync(method: SendGridClient.Method.DELETE, urlPath: "whitelabel/domains/" + domainID);
            if (DeleteResponse.StatusCode == HttpStatusCode.NoContent)
            {
                await _emailwhitelabelService.DeleteBulkEntityAsync(x => x.DomainID == domainID);
                return 1;
            }
            else
            {
                return 0;
            }
        }

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("delete-email-by-companyid", Name = "delete-email-by-companyid")]
        public async Task<int> DeleteDomainByCompanyID(Guid companyId)
        {
            var ret = await _emailwhitelabelService.DeleteEmailByCompanyID(companyId);
            return ret;
        }

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteEmailWhitelabelById")]
        public async Task<IActionResult> DeleteEmailWhitelabelById(Guid id)
        {
            //if the emailwhitelabel exists
            if (await _emailwhitelabelService.ExistAsync(x => x.Id == id))
            {
                //delete the emailwhitelabel from the db.
                await _emailwhitelabelService.DeleteEntityAsync(id);
            }
            else
            {
                //if emailwhitelabel doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForEmailWhitelabel(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetEmailWhitelabel", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetEmailWhitelabel", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteEmailWhitelabelById", new { id = id }),
              "delete_emailwhitelabel",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateEmailWhitelabel", new { id = id }),
             "update_emailwhitelabel",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateEmailWhitelabel", new { }),
              "create_emailwhitelabel",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForEmailWhitelabels(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateEmailWhitelabelsResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateEmailWhitelabelsResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateEmailWhitelabelsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateEmailWhitelabelsResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredEmailWhitelabels",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredEmailWhitelabels",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredEmailWhitelabels",
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
