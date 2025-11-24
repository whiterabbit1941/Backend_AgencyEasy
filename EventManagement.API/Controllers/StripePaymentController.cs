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
using Stripe.Checkout;
using Stripe;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.Design;
using System.Linq;

namespace EventManagement.API.Controllers
{
    /// <summary>
    /// StripePayment endpoint
    /// </summary>
    [Route("api/stripepayments")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]

    public class StripePaymentController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly IStripePaymentService _stripepaymentService;
        private ILogger<StripePaymentController> _logger;
        private readonly IUrlHelper _urlHelper;
        private readonly IConfiguration _configuration;

        #endregion


        #region CONSTRUCTOR

        public StripePaymentController(IStripePaymentService stripepaymentService,
            ILogger<StripePaymentController> logger,
            IUrlHelper urlHelper, IConfiguration configuration)
        {
            _logger = logger;
            _stripepaymentService = stripepaymentService;
            _urlHelper = urlHelper;
            _configuration = configuration;
        }

        #endregion


        #region HTTPGET

        [HttpGet(Name = "GetFilteredStripePayments")]
        [Produces("application/vnd.tourmanagement.stripepayments.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<StripePaymentDto>>> GetFilteredStripePayments([FromQuery] FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_stripepaymentService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<StripePaymentDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var stripepaymentsFromRepo = await _stripepaymentService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.stripepayments.hateoas+json")
            {
                //create HATEOAS links for each show.
                stripepaymentsFromRepo.ForEach(stripepayment =>
                {
                    var entityLinks = CreateLinksForStripePayment(stripepayment.Id, filterOptionsModel.Fields);
                    stripepayment.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = stripepaymentsFromRepo.TotalCount,
                    pageSize = stripepaymentsFromRepo.PageSize,
                    currentPage = stripepaymentsFromRepo.CurrentPage,
                    totalPages = stripepaymentsFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForStripePayments(filterOptionsModel, stripepaymentsFromRepo.HasNext, stripepaymentsFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = stripepaymentsFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = stripepaymentsFromRepo.HasPrevious ?
                    CreateStripePaymentsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = stripepaymentsFromRepo.HasNext ?
                    CreateStripePaymentsResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = stripepaymentsFromRepo.TotalCount,
                    pageSize = stripepaymentsFromRepo.PageSize,
                    currentPage = stripepaymentsFromRepo.CurrentPage,
                    totalPages = stripepaymentsFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(stripepaymentsFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.stripepayments.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetStripePayment")]
        public async Task<ActionResult<StripePayment>> GetStripePayment(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object stripepaymentEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetStripePayment called");

                //then get the whole entity and map it to the Dto.
                stripepaymentEntity = Mapper.Map<StripePaymentDto>(await _stripepaymentService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                stripepaymentEntity = await _stripepaymentService.GetPartialEntityAsync(id, fields);
            }

            //if stripepayment not found.
            if (stripepaymentEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.stripepayments.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForStripePayment(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((StripePaymentDto)stripepaymentEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = stripepaymentEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = stripepaymentEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("get-invoice", Name = "get-invoice")]
        public async Task<IActionResult> GetInvoice(string subscriptionId)
        {
            var stripeSecretKey = new StripeClient(_configuration["StripeSecret"]);

            try
            {
                if (subscriptionId.Contains("sub_"))
                {
                    var subscriptionService = new SubscriptionService(stripeSecretKey);
                    var subscription = await subscriptionService.GetAsync(subscriptionId);

                    var service = new InvoiceService(stripeSecretKey);
                    var invoice = await service.GetAsync(subscription.LatestInvoiceId);
                    return Ok(invoice.InvoicePdf);

                }
                else
                {
                    NotFound();
                }
            }
            catch (StripeException e)
            {
                return BadRequest(new { error = new { message = e.StripeError.Message } });
            }
            catch (System.Exception)
            {
                return BadRequest(new { error = new { message = "unknown failure: 500" } });
            }
            return NotFound();
        }

        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateStripePayment")]
        public async Task<IActionResult> UpdateStripePayment(Guid id, [FromBody] StripePaymentForUpdate StripePaymentForUpdate)
        {

            //if show not found
            if (!await _stripepaymentService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _stripepaymentService.UpdateEntityAsync(id, StripePaymentForUpdate);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateStripePayment(Guid id, [FromBody] JsonPatchDocument<StripePaymentForUpdate> jsonPatchDocument)
        {
            StripePaymentForUpdate dto = new StripePaymentForUpdate();
            StripePayment stripepayment = new StripePayment();

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
            Mapper.Map(dto, stripepayment);

            //set the Id for the show model.
            stripepayment.Id = id;

            //partially update the chnages to the db. 
            await _stripepaymentService.UpdatePartialEntityAsync(stripepayment, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateStripePayment")]
        public async Task<ActionResult<StripePaymentDto>> CreateStripePayment([FromBody] StripePaymentForCreation stripepayment)
        {
            var p = _stripepaymentService.PaymentWithStripe();
            //create a show in db.
            var stripepaymentToReturn = await _stripepaymentService.CreateEntityAsync<StripePaymentDto, StripePaymentForCreation>(stripepayment);

            //return the show created response.
            return CreatedAtRoute("GetStripePayment", new { id = stripepaymentToReturn.Id }, stripepaymentToReturn);
        }

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("CreateStripePaymentCheckout", Name = "CreateStripePaymentCheckout")]
        public EventManagement.Service.CreateCheckoutSessionResponse CreateStripePaymentCheckout()
        {
            EventManagement.Service.CreateCheckoutSessionResponse p = _stripepaymentService.PaymentWithStripe();
            return p;
        }
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("CreateStripePrice", Name = "CreateStripePrice")]
        public Stripe.Price CreateStripePrice(EventManagement.Service.StripeProduct myObj)
        {
            Stripe.Price p = _stripepaymentService.CreateStripePriceService(myObj);
            return p;
        }
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("DeleteStripeProduct", Name = "DeleteStripeProduct")]
        public Stripe.Product DeleteStripeProduct(EventManagement.Service.StripeProduct myObj)
        {
            Stripe.Product p = _stripepaymentService.DeleteStripeProductService(myObj.ProductId);
            return p;
        }

        [HttpGet("create-subscription", Name = "create-subscription")]
        public async Task<IActionResult> CreateSubscription(string priceId, int noOfProject, Guid companyId, string defaultPlanId, string baseUrl, string subscriptionId)
        {

            var res = await _stripepaymentService.CreateAndUpdateSubscription(priceId, companyId, defaultPlanId, baseUrl, subscriptionId);

            return Ok(res);

        }

        [AllowAnonymous]
        [HttpGet("get-stripe-session", Name = "get-stripe-session")]
        public async Task<IActionResult> GetStripeSession(string sessionId)
        {
            try
            {

                var stripeSecretKey = new StripeClient(_configuration["StripeSecret"]);

                // subscribed new plan
                var sessionService = new SessionService(stripeSecretKey);
                Session session = await sessionService.GetAsync(sessionId);
                var subscriptionService = new SubscriptionService(stripeSecretKey);
                var subscription =await  subscriptionService.GetAsync(session.SubscriptionId);

                if (subscription.StripeResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(subscription);
                }
                else
                {
                    return BadRequest(new { error = new { message = "Customer subscription not found" } });
                }
            }
            catch (StripeException e)
            {
                return BadRequest(new { error = new { message = e.StripeError.Message } });
            }
            catch (System.Exception)
            {
                return BadRequest(new { error = new { message = "unknown failure: 500" } });
            }
        }



        [HttpGet("get-stripe-subscription", Name = "get-stripe-subscription")]
        public async Task<IActionResult> GetStripeSubscription(string subscriptionId)
        {
            try
            {
                var stripeSecretKey = new StripeClient(_configuration["StripeSecret"]);

                var subOption = new SubscriptionCancelOptions
                {
                    InvoiceNow = true,
                    Expand = new List<string> { "discounts" }
                };

                var service = new SubscriptionService(stripeSecretKey);

                var subscriptionRes = await service.GetAsync(subscriptionId);

                if (subscriptionRes.StripeResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(subscriptionRes);
                }
                else
                {
                    return BadRequest(new { error = new { message = "Customer subscription not found" } });
                }
            }
            catch (StripeException e)
            {
                return BadRequest(new { error = new { message = e.StripeError.Message } });
            }
            catch (System.Exception)
            {
                return BadRequest(new { error = new { message = "unknown failure: 500" } });
            }
        }

        [HttpGet("get-upcoming-invoice", Name = "get-upcoming-invoice")]
        public async Task<IActionResult> GetUpcomingInvoice(string priceId, string subscriptionId)
        {
            try
            {
                //subscriptionId = "sub_1NsP5UDlhoqIyFUOA8P3cdik";

                int projectCount = 0;
                int projectCountForCustomPlan = 1;

                var stripeSecretKey = new StripeClient(_configuration["StripeSecret"]);

                //var customMonthly = _configuration.GetSection("StripeSubscriptionPriceId:CustomMonthly").Value;
                //var customYearly = _configuration.GetSection("StripeSubscriptionPriceId:CustomYearly").Value;

                var agencyMonthly = _configuration.GetSection("StripeSubscriptionPriceId:AgencyMonthly").Value;
                var agencyYearly = _configuration.GetSection("StripeSubscriptionPriceId:AgencyYearly").Value;

                var startupMonthly = _configuration.GetSection("StripeSubscriptionPriceId:StartupMonthly").Value;
                var startupYearly = _configuration.GetSection("StripeSubscriptionPriceId:StartupYearly").Value;

                var professionalMonthly = _configuration.GetSection("StripeSubscriptionPriceId:ProfessionalMonthly").Value;
                var professionalYearly = _configuration.GetSection("StripeSubscriptionPriceId:ProfessionalYearly").Value;

                //if (priceId.Equals(customMonthly) || priceId.Equals(customYearly))
                //{
                //    if (noOfProject < 11)
                //    {
                //        projectCount = 11;
                //        projectCountForCustomPlan = 11;
                //    }
                //    else
                //    {
                //        projectCount = noOfProject;
                //        projectCountForCustomPlan = noOfProject;
                //    }
                //}
                //else

                if (priceId.Equals(agencyYearly) || priceId.Equals(agencyMonthly))
                {
                    projectCount = 20;
                }
                else if (priceId.Equals(startupYearly) || priceId.Equals(startupMonthly))
                {
                    projectCount = 5;
                }
                else if (priceId.Equals(professionalYearly) || priceId.Equals(professionalMonthly))
                {
                    projectCount = 50;
                }

                //Preview Proration Preview
                //Retrive Upcoming Invoice

                var subscriptionService = new SubscriptionService(stripeSecretKey);
                var subscription = await subscriptionService.GetAsync(subscriptionId);

                // Set the proration date to this moment:
                DateTimeOffset prorationDate = DateTimeOffset.UtcNow;

                // See what the next invoice would look like with a price switch
                // and proration set:
                var items = new List<InvoiceSubscriptionItemOptions>
                {
                  new InvoiceSubscriptionItemOptions
                  {
                    Id = subscription.Items.Data[0].Id,
                    Price = priceId, // switch to new price                    
                  },
                };

                var options = new UpcomingInvoiceOptions
                {
                    Customer = subscription.CustomerId,
                    Subscription = subscriptionId,
                    SubscriptionItems = items,
                    SubscriptionProrationDate = prorationDate.UtcDateTime,
                    Expand = new List<string> { "discounts" }
                };

                var invService = new InvoiceService(stripeSecretKey);
                Invoice invoice = invService.Upcoming(options);

                return Ok(invoice);
            }
            catch (StripeException e)
            {
                return BadRequest(new { error = new { message = e.StripeError.Message } });
            }
            catch (System.Exception)
            {
                return BadRequest(new { error = new { message = "unknown failure: 500" } });
            }
        }

        [HttpGet("update-payment-method", Name = "update-payment-method")]
        public async Task<IActionResult> UpdatePaymentMethod(string subscriptionId, string _baseUrl, string successUrl)
        {
            try
            {
                var stripeSecretKey = new StripeClient(_configuration["StripeSecret"]);

                var subscriptionService = new SubscriptionService(stripeSecretKey);
                var subscription = await subscriptionService.GetAsync(subscriptionId);
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> {
                   "card",
                },
                    Mode = "setup",

                    Customer = subscription.CustomerId,
                    SuccessUrl = _baseUrl + successUrl + "?session_id={CHECKOUT_SESSION_ID}",
                    CancelUrl = _baseUrl + successUrl,


                };

                var service = new SessionService(stripeSecretKey);
                var session = service.Create(options);

                return Ok(session.Url);
            }
            catch (StripeException e)
            {
                return BadRequest(new { error = new { message = e.StripeError.Message } });
            }
            catch (System.Exception)
            {
                return BadRequest(new { error = new { message = "unknown failure: 500" } });
            }
        }

        [HttpGet("cancel-subscription", Name = "cancel-subscription")]
        public async Task<IActionResult> CancelSubscription(string subscriptionId, Guid companyId)
        {
            var res = await _stripepaymentService.CancelSubscription(subscriptionId, companyId);
            return Ok(res);
        }

        [HttpGet("refund-balance", Name = "refund-balance")]
        public async Task<IActionResult> RefundCustomerBalance(string subscriptionId)
        {
            var res = await _stripepaymentService.RefundCustomerBalance(subscriptionId);
            return Ok(res);
        }

        [HttpGet("get-stripe-customer", Name = "get-stripe-customer")]
        public async Task<IActionResult> GetStripeCustomer(string customerId)
        {
            try
            {
                var stripeSecretKey = new StripeClient(_configuration["StripeSecret"]);



                var customerService = new CustomerService(stripeSecretKey);
                var customerInfo = customerService.Get(customerId);
                return Ok(customerInfo);
            }
            catch (StripeException e)
            {
                return BadRequest(new { error = new { message = e.StripeError.Message } });
            }
            catch (System.Exception)
            {
                return BadRequest(new { error = new { message = "unknown failure: 500" } });
            }
        }

        [HttpGet("get-invoices-and-refunds", Name = "get-invoices-and-refunds")]
        public async Task<IActionResult> GetAllInvoice(string subscriptionId, Guid companyId)
        {
            var res = await _stripepaymentService.GetAllInvoice(subscriptionId, companyId);
            return Ok(res);
        }

        [HttpGet("create-payment-for-marketplace", Name = "create-payment-for-marketplace")]
        public async Task<IActionResult> CreatePayment(string planId, string successUrl, string baseUrl)
        {
            try
            {
                var planDetail = _stripepaymentService.GetPlanById(new Guid(planId));
                var _baseUrl = baseUrl;
                var stripeSecretKey = new StripeClient(_configuration["StripeSecret"]);

                var paymentMode = planDetail.PaymentType == "recurring" ? "subscription" : "payment";

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> {
                        "card",
                    },

                    Mode = paymentMode,
                    SuccessUrl = _baseUrl + successUrl + "?session_id={CHECKOUT_SESSION_ID}",
                    CancelUrl = _baseUrl + successUrl,
                    Currency = planDetail.Currency,

                    LineItems = new List<SessionLineItemOptions>
                      {
                        new SessionLineItemOptions
                        {
                          Price = planDetail.priceId,
                          Quantity = 1,
                          DynamicTaxRates = new List<string>
                          {
                            _configuration["StripeTaxId"]
                          }
                        },
                      }
                };

                var sessionService = new SessionService(stripeSecretKey);
                Session session = sessionService.Create(options);

                return Ok(session.Url);
            }
            catch (StripeException e)
            {
                return BadRequest(new { error = new { message = e.StripeError.Message } });
            }
            catch (System.Exception)
            {
                return BadRequest(new { error = new { message = "unknown failure: 500" } });
            }
        }

        /// <summary>
        /// Get plan interval type
        /// </summary>
        /// <param name="subscriptionId"></param>
        /// <returns>true or false</returns>
        [HttpGet("is-monthly", Name = "is-monthly")]
        public async Task<IActionResult> IsMonthly(string subscriptionId)
        {
            try
            {
                bool isMOnthly = _stripepaymentService.IsMonthlyPlan(subscriptionId);
                return Ok(isMOnthly);
            }
            catch (StripeException e)
            {
                return BadRequest(new { error = new { message = e.StripeError.Message } });
            }
            catch (System.Exception)
            {
                return BadRequest(new { error = new { message = "unknown failure: 500" } });
            }
        }

        [AllowAnonymous]
        [HttpGet("get-payment-intent", Name = "get-payment-intent")]
        public async Task<IActionResult> GetPaymentIntent(string id)
        {
            try
            {
                PaymentIntentDto returnVal = new PaymentIntentDto();
                var stripeSecretKey = new StripeClient(_configuration["StripeSecret"]);

                var piService = new PaymentIntentService(stripeSecretKey);
                PaymentIntent piResponse = await piService.GetAsync(id);

                returnVal.Amount = piResponse.AmountReceived;
                returnVal.Status = piResponse.Status;

                return Ok(returnVal);
            }
            catch (StripeException e)
            {
                return BadRequest(new { error = new { message = e.StripeError.Message } });
            }
            catch (System.Exception)
            {
                return BadRequest(new { error = new { message = "unknown failure: 500" } });
            }
        }


        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteStripePaymentById")]
        public async Task<IActionResult> DeleteStripePaymentById(Guid id)
        {
            //if the stripepayment exists
            if (await _stripepaymentService.ExistAsync(x => x.Id == id))
            {
                //delete the stripepayment from the db.
                await _stripepaymentService.DeleteEntityAsync(id);
            }
            else
            {
                //if stripepayment doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForStripePayment(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetStripePayment", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetStripePayment", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteStripePaymentById", new { id = id }),
              "delete_stripepayment",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateStripePayment", new { id = id }),
             "update_stripepayment",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateStripePayment", new { }),
              "create_stripepayment",
              "POST"));

            links.Add(
            new LinkDto(_urlHelper.Link("CreateStripePaymentCheckout", new { }),
            "create_stripepayment",
            "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForStripePayments(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateStripePaymentsResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateStripePaymentsResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateStripePaymentsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateStripePaymentsResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredStripePayments",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredStripePayments",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredStripePayments",
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

