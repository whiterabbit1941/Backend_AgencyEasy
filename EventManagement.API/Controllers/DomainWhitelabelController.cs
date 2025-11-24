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
using Amazon.CertificateManager;
using System.Net;
using FinanaceManagement.API.Models;
using Microsoft.Extensions.Configuration;
using Amazon.CertificateManager.Model;
using IdentityServer4.AccessTokenValidation;

namespace EventManagement.API.Controllers
{
    /// <summary>
    /// DomainWhitelabel endpoint
    /// </summary>
    [Route("api/domainwhitelabels")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
    public class DomainWhitelabelController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly IDomainWhitelabelService _domainwhitelabelService;
        private ILogger<DomainWhitelabelController> _logger;
        private readonly IUrlHelper _urlHelper;
        private readonly ICompanyService _companyService;
        private readonly IClientRedirectUriService _clientRedirectUriService;
        private readonly IClientPostLogoutRedirectUriService _clientPostLogoutRedirectUriService;
        private readonly IConfiguration _configuration;

        #endregion


        #region CONSTRUCTOR

        public DomainWhitelabelController(IConfiguration configuration, IClientPostLogoutRedirectUriService clientPostLogoutRedirectUriService, IClientRedirectUriService clientRedirectUriService, ICompanyService companyService, IDomainWhitelabelService domainwhitelabelService, ILogger<DomainWhitelabelController> logger, IUrlHelper urlHelper)
        {
            _logger = logger;
            _domainwhitelabelService = domainwhitelabelService;
            _urlHelper = urlHelper;
            _companyService = companyService;
            _clientRedirectUriService = clientRedirectUriService;
            _clientPostLogoutRedirectUriService = clientPostLogoutRedirectUriService;
            _configuration = configuration;
        }

        #endregion


        #region HTTPGET

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("TestDomainWhitelabel", Name = "TestDomainWhitelabel")]
        public async Task<ActionResult> TestDomainWhitelabel(string alternateDomainName, string certificateARN, string distributionId)
        {
            //string alternateDomainName = "agencytest.e-intelligence.co";
            //string certificateARN = "arn:aws:acm:us-east-1:213387745654:certificate/28b34c70-c38e-40c9-a5dc-3f871b0c8fe6";
            //string distributionId = "E7UALMPOTRHJO";

            #region CreateCertificate
            //var response = _domainwhitelabelService.CreateCertificate(alternateDomainName);
            //var CertificateARN = response.CertificateArn;
            #endregion

            #region GetCertificate
            var response = await _domainwhitelabelService.GetCertificate(certificateARN);
            var CnameType = response.Certificate.DomainValidationOptions[0].ResourceRecord.Type.Value;
            var CnameHost = response.Certificate.DomainValidationOptions[0].ResourceRecord.Name;
            var CnamePointsTo = response.Certificate.DomainValidationOptions[0].ResourceRecord.Value;
            var Status = response.Certificate.Status.Value;
            #endregion

            #region CreateDistributionRequestFromExisiting
            //var response = _domainwhitelabelService.CreateDistributionRequestFromExisiting(alternateDomainName, "Trial Hemang");
            //var Origin = response.Distribution.DistributionConfig.Origins.Items[0].DomainName;
            //var DomainName = response.Distribution.DomainName;
            //var DistributionId = response.Distribution.Id;
            #endregion

            #region UpdateCloudfrontDistribution
            //var response = _domainwhitelabelService.UpdateCloudfrontDistribution(alternateDomainName, certificateARN, distributionId);
            #endregion

            return Ok(response);
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("GetDomainWhitelabelByCompanyID", Name = "GetDomainWhitelabelByCompanyID")]
        public async Task<ActionResult<List<DomainWhitelabel>>> GetDomainWhitelabelByCompanyID(Guid CompanyID)
        {
            var domainInfo = _domainwhitelabelService.GetAllEntities().Where(x => x.CompanyID == CompanyID).ToList();
            return Ok(domainInfo);
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("CheckValidationStatusOfCertificate", Name = "CheckValidationStatusOfCertificate")]
        public async Task<ActionResult<DomainWhitelabel>> CheckValidationStatusOfCertificate(Guid Id)
        {
            var domainInfo = _domainwhitelabelService.GetEntityById(Id);
            if (domainInfo != null)
            {
                // Get Certificate
                var getResponse = await _domainwhitelabelService.GetCertificate(domainInfo.CertificateARN);
                // set value
                domainInfo.Status = getResponse.Certificate.Status.Value;

                if (getResponse.Certificate.Status == CertificateStatus.ISSUED)
                {
                    // Get Company Detail
                    var companyData = _companyService.GetEntityById(domainInfo.CompanyID);

                    // CreateDistributionRequestFromExisiting
                    var createResponse = await _domainwhitelabelService.CreateDistributionRequestFromExisiting(domainInfo.AlternateDomainName, companyData.Name);
                    // set value
                    domainInfo.Origin = createResponse.Distribution.DistributionConfig.Origins.Items[0].DomainName;
                    domainInfo.DomainName = createResponse.Distribution.DomainName;
                    domainInfo.DistributionId = createResponse.Distribution.Id;

                    // UpdateCloudfrontDistribution
                    var updateResponse = await _domainwhitelabelService.UpdateCloudfrontDistribution(domainInfo.AlternateDomainName, domainInfo.CertificateARN, domainInfo.DistributionId);
                    if (updateResponse.HttpStatusCode == HttpStatusCode.OK)
                    {
                        domainInfo.Certificate = true;
                        domainInfo.UpdatedOn = DateTime.UtcNow;

                        // update data
                        _domainwhitelabelService.UpdateEntity(domainInfo);
                        _domainwhitelabelService.SaveChanges();

                        // update ClientRedirectUris table
                        List<ClientRedirectUris> entities = new List<ClientRedirectUris>();

                        var entity1 = new ClientRedirectUris();
                        entity1.ClientId = Convert.ToInt32(_configuration.GetSection("ClientId").Value);
                        entity1.RedirectUri = "https://" + domainInfo.AlternateDomainName + "/signin-oidc";
                        entities.Add(entity1);

                        var entity2 = new ClientRedirectUris();
                        entity2.ClientId = Convert.ToInt32(_configuration.GetSection("ClientId").Value);
                        entity2.RedirectUri = "https://" + domainInfo.AlternateDomainName + "/redirect-silentrenew";
                        entities.Add(entity2);

                        _clientRedirectUriService.CreateBulkEntity(entities);
                        _clientRedirectUriService.SaveChanges();

                        // update ClientPostLogoutRedirectUris table
                        ClientPostLogoutRedirectUris entity = new ClientPostLogoutRedirectUris();
                        entity.ClientId = Convert.ToInt32(_configuration.GetSection("ClientId").Value);
                        entity.PostLogoutRedirectUri = "https://" + domainInfo.AlternateDomainName + "/";

                        _clientPostLogoutRedirectUriService.CreateEntity(entity);
                        _clientPostLogoutRedirectUriService.SaveChanges();
                    }
                }
            }
            return Ok(domainInfo);
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("UploadCertificate", Name = "UploadCertificate")]
        public async Task<ActionResult<bool>> UploadCertificate(Guid Id)
        {
            var domainInfo = _domainwhitelabelService.GetEntityById(Id);

            if (domainInfo != null)
            {
                if (!String.IsNullOrEmpty(domainInfo.DistributionId))
                {
                    // UpdateCloudfrontDistribution
                    var updateResponse = await _domainwhitelabelService.UpdateCloudfrontDistribution(domainInfo.AlternateDomainName, domainInfo.CertificateARN, domainInfo.DistributionId);
                    if (updateResponse.HttpStatusCode == HttpStatusCode.OK)
                    {
                        domainInfo.Certificate = true;
                        domainInfo.UpdatedOn = DateTime.UtcNow;

                        // update data
                        _domainwhitelabelService.UpdateEntity(domainInfo);
                        _domainwhitelabelService.SaveChanges();

                        // update ClientRedirectUris table
                        List<ClientRedirectUris> entities = new List<ClientRedirectUris>();

                        var entity1 = new ClientRedirectUris();
                        entity1.ClientId = Convert.ToInt32(_configuration.GetSection("ClientId").Value);
                        entity1.RedirectUri = "https://" + domainInfo.AlternateDomainName + "/signin-oidc";
                        entities.Add(entity1);



                        var entity2 = new ClientRedirectUris();
                        entity2.ClientId = Convert.ToInt32(_configuration.GetSection("ClientId").Value);
                        entity2.RedirectUri = "https://" + domainInfo.AlternateDomainName + "/redirect-silentrenew";
                        entities.Add(entity2);

                        _clientRedirectUriService.CreateBulkEntity(entities);
                        _clientRedirectUriService.SaveChanges();

                        // update ClientPostLogoutRedirectUris table
                        ClientPostLogoutRedirectUris entity = new ClientPostLogoutRedirectUris();
                        entity.ClientId = Convert.ToInt32(_configuration.GetSection("ClientId").Value);
                        entity.PostLogoutRedirectUri = "https://" + domainInfo.AlternateDomainName + "/";

                        _clientPostLogoutRedirectUriService.CreateEntity(entity);
                        _clientPostLogoutRedirectUriService.SaveChanges();
                    }
                }
            }
            return domainInfo.Certificate;
        }

        [HttpGet(Name = "GetFilteredDomainWhitelabels")]
        [Produces("application/vnd.tourmanagement.domainwhitelabels.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<DomainWhitelabelDto>>> GetFilteredDomainWhitelabels([FromQuery] FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_domainwhitelabelService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<DomainWhitelabelDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var domainwhitelabelsFromRepo = await _domainwhitelabelService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.domainwhitelabels.hateoas+json")
            {
                //create HATEOAS links for each show.
                domainwhitelabelsFromRepo.ForEach(domainwhitelabel =>
                {
                    var entityLinks = CreateLinksForDomainWhitelabel(domainwhitelabel.Id, filterOptionsModel.Fields);
                    domainwhitelabel.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = domainwhitelabelsFromRepo.TotalCount,
                    pageSize = domainwhitelabelsFromRepo.PageSize,
                    currentPage = domainwhitelabelsFromRepo.CurrentPage,
                    totalPages = domainwhitelabelsFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForDomainWhitelabels(filterOptionsModel, domainwhitelabelsFromRepo.HasNext, domainwhitelabelsFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = domainwhitelabelsFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = domainwhitelabelsFromRepo.HasPrevious ?
                    CreateDomainWhitelabelsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = domainwhitelabelsFromRepo.HasNext ?
                    CreateDomainWhitelabelsResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = domainwhitelabelsFromRepo.TotalCount,
                    pageSize = domainwhitelabelsFromRepo.PageSize,
                    currentPage = domainwhitelabelsFromRepo.CurrentPage,
                    totalPages = domainwhitelabelsFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(domainwhitelabelsFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.domainwhitelabels.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetDomainWhitelabel")]
        public async Task<ActionResult<DomainWhitelabel>> GetDomainWhitelabel(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object domainwhitelabelEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetDomainWhitelabel called");

                //then get the whole entity and map it to the Dto.
                domainwhitelabelEntity = Mapper.Map<DomainWhitelabelDto>(await _domainwhitelabelService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                domainwhitelabelEntity = await _domainwhitelabelService.GetPartialEntityAsync(id, fields);
            }

            //if domainwhitelabel not found.
            if (domainwhitelabelEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.domainwhitelabels.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForDomainWhitelabel(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((DomainWhitelabelDto)domainwhitelabelEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = domainwhitelabelEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = domainwhitelabelEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("GetCompanyDetailsByCustomDomain", Name = "GetCompanyDetailsByCustomDomain")]
        [AllowAnonymous]
        public async Task<ActionResult<List<CompanyDto>>> GetCompanyDetailsByCustomDomain([FromQuery] string domain)
        {
            var companyInfo = _domainwhitelabelService.GetCompanyDetailsByDomain(domain);

            return Ok(companyInfo);
        }
        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateDomainWhitelabel")]
        public async Task<IActionResult> UpdateDomainWhitelabel(Guid id, [FromBody] DomainWhitelabelForUpdate DomainWhitelabelForUpdate)
        {

            //if show not found
            if (!await _domainwhitelabelService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _domainwhitelabelService.UpdateEntityAsync(id, DomainWhitelabelForUpdate);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateDomainWhitelabel(Guid id, [FromBody] JsonPatchDocument<DomainWhitelabelForUpdate> jsonPatchDocument)
        {
            DomainWhitelabelForUpdate dto = new DomainWhitelabelForUpdate();
            DomainWhitelabel domainwhitelabel = new DomainWhitelabel();

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
            Mapper.Map(dto, domainwhitelabel);

            //set the Id for the show model.
            domainwhitelabel.Id = id;

            //partially update the chnages to the db. 
            await _domainwhitelabelService.UpdatePartialEntityAsync(domainwhitelabel, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("CreateDomainWhitelabelInAWS", Name = "CreateDomainWhitelabelInAWS")]
        public async Task<ActionResult<DomainWhitelabelDto>> CreateDomainWhitelabelInAWS(DomainWhitelabelDto domainsDTO, Guid companyID)
        {
            // create entity
            DomainWhitelabelForCreation entity = new DomainWhitelabelForCreation();
            entity.CompanyID = companyID;
            entity.AlternateDomainName = domainsDTO.AlternateDomainName;

            // Create Certificate
            var createResponse = await _domainwhitelabelService.CreateCertificate(domainsDTO.AlternateDomainName);
            // set value
            entity.CertificateARN = createResponse.CertificateArn;

            // Getting ResourceRecord null in first call
            DescribeCertificateResponse getResponse;
            do
            {
                // Get Certificate
                getResponse = await _domainwhitelabelService.GetCertificate(createResponse.CertificateArn);

            }
            while (getResponse.Certificate.DomainValidationOptions[0].ResourceRecord == null);


            // set value
            entity.CnameType = getResponse.Certificate.DomainValidationOptions[0].ResourceRecord.Type.Value;
            entity.CnameHost = getResponse.Certificate.DomainValidationOptions[0].ResourceRecord.Name;
            entity.CnamePointsTo = getResponse.Certificate.DomainValidationOptions[0].ResourceRecord.Value;
            entity.Status = getResponse.Certificate.Status.Value;

            //create a show in db.
            var domainwhitelabelToReturn = await _domainwhitelabelService.CreateEntityAsync<DomainWhitelabelDto, DomainWhitelabelForCreation>(entity);

            //return the show created response.
            return CreatedAtRoute("GetDomainWhitelabel", new { id = domainwhitelabelToReturn.Id }, domainwhitelabelToReturn);
        }

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("CreateDomainWhitelabelInAWSForAmplify", Name = "CreateDomainWhitelabelInAWSForAmplify")]
        public async Task<ActionResult<DomainWhitelabelDto>> CreateDomainWhitelabelInAWSForAmplify(Guid companyId, string customDomain)
        {
            var createResponse = await _domainwhitelabelService.CreateCustomDomain(companyId, customDomain);

            return Ok(createResponse); //CreatedAtRoute("GetDomainWhitelabel", new { id = createResponse.Id }, createResponse);
        }


        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("GetDomainDns", Name = "GetDomainDns")]
        public async Task<ActionResult<DomainWhitelabel>> GetDomainDns(Guid companyId, string customDomain)
        {

            var createResponse = await _domainwhitelabelService.GetDomainDns(companyId, customDomain);

            return Ok(createResponse);
        }

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateDomainWhitelabel")]
        public async Task<ActionResult<DomainWhitelabelDto>> CreateDomainWhitelabel([FromBody] DomainWhitelabelForCreation domainwhitelabel)
        {
            //create a show in db.
            var domainwhitelabelToReturn = await _domainwhitelabelService.CreateEntityAsync<DomainWhitelabelDto, DomainWhitelabelForCreation>(domainwhitelabel);

            //return the show created response.
            return CreatedAtRoute("GetDomainWhitelabel", new { id = domainwhitelabelToReturn.Id }, domainwhitelabelToReturn);
        }

        #endregion


        #region HTTPDELETE

        //[ProducesResponseType(StatusCodes.Status204NoContent)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[HttpDelete("{id}", Name = "DeleteDomainWhitelabelById")]
        //public async Task<int> DeleteDomainWhitelabelById(Guid id)
        //{
        //    //if the domainwhitelabel exists
        //    if (await _domainwhitelabelService.ExistAsync(x => x.Id == id))
        //    {
        //        //delete the domainwhitelabel from the db.
        //        return await _domainwhitelabelService.DeleteEntityAsync(id);
        //    }
        //    else
        //    {
        //        //if domainwhitelabel doesn't exists then returns not found.
        //        return 0;
        //    }
        //}

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("DeleteDomainAssociation/{id}", Name = "DeleteDomainAssociation")]
        public async Task<int> DeleteDomainAssociation(Guid id)
        {
            //delete the domainwhitelabel from the db.
            return await _domainwhitelabelService.DeleteDomainAssociation(id);
        }

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("delete-domain-by-companyid", Name = "delete-domain-by-companyid")]
        public async Task<int> DeleteDomainAssociationByCompanyId(Guid companyId)
        {
            //delete the domainwhitelabel from the db.
            return await _domainwhitelabelService.DeleteDomainAssociationByCompanyId(companyId);
        }


        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForDomainWhitelabel(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetDomainWhitelabel", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetDomainWhitelabel", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteDomainWhitelabelById", new { id = id }),
              "delete_domainwhitelabel",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateDomainWhitelabel", new { id = id }),
             "update_domainwhitelabel",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateDomainWhitelabel", new { }),
              "create_domainwhitelabel",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForDomainWhitelabels(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateDomainWhitelabelsResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateDomainWhitelabelsResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateDomainWhitelabelsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateDomainWhitelabelsResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredDomainWhitelabels",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredDomainWhitelabels",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredDomainWhitelabels",
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
