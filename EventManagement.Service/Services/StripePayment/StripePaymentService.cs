using EventManagement.Domain;
using EventManagement.Domain.Entities;
using EventManagement.Dto;
using IdentityModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Threading.Tasks;

namespace EventManagement.Service
{
    public class StripePaymentService : ServiceBase<StripePayment, Guid>, IStripePaymentService
    {


        #region PRIVATE MEMBERS

        private readonly IStripePaymentRepository _stripepaymentRepository;
        private readonly IConfiguration _configuration;
        private readonly IPlanService _planService;
        public readonly ICompanyPlanRepository _companyPlanRepository;
        public readonly IDefaultPlanService _defaultPlanService;
        public readonly IStripeCouponService _stripeCouponService;

        #endregion


        #region CONSTRUCTOR

        public StripePaymentService(IStripePaymentRepository stripepaymentRepository,
            ILogger<StripePaymentService> logger,
            IConfiguration configuration, IPlanService planService,
            ICompanyPlanRepository companyPlanRepository, IDefaultPlanService defaultPlanService, IStripeCouponService stripeCouponService) : base(stripepaymentRepository, logger)
        {
            _stripepaymentRepository = stripepaymentRepository;
            _configuration = configuration;
            _planService = planService;
            _companyPlanRepository = companyPlanRepository;
            _defaultPlanService = defaultPlanService;
            _stripeCouponService = stripeCouponService;
        }

        #endregion


        #region PUBLIC MEMBERS   
        public EventManagement.Service.CreateCheckoutSessionResponse PaymentWithStripe()
        {
            // Use configuration value instead of hardcoded key
            var stripeSecret = _configuration["StripeSecret"];
            StripeConfiguration.ApiKey = stripeSecret;

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                    {
                      "card",
                    },
                LineItems = new List<SessionLineItemOptions>
        {
          new SessionLineItemOptions
          {
            PriceData = new SessionLineItemPriceDataOptions
            {
              UnitAmount = 2000,
              Currency = "usd",
              ProductData = new SessionLineItemPriceDataProductDataOptions
              {
                Name = "T-shirt",
              },

            },
            Quantity = 1,
          },
        },
                Mode = "payment",
                SuccessUrl = "https://example.com/success",
                CancelUrl = "https://example.com/cancel",
            };

            var service = new SessionService();
            Session session = service.Create(options);
            // StripeConfiguration.ApiKey should be set from configuration

            //var options = new CustomerCreateOptions
            //{
            //    Name = "Jenny Rosen",
            //    Address = new AddressOptions
            //    {
            //        Line1 = "510 Townsend St",
            //        PostalCode = "98140",
            //        City = "San Francisco",
            //        State = "CA",
            //        Country = "US",
            //    },
            //};
            //var service = new CustomerService();
            //var customer = service.Create(options);
            // StripeConfiguration.ApiKey should be set from configuration

            //// `source` is obtained with Stripe.js; see https://stripe.com/docs/payments/accept-a-payment-charges#web-create-token
            //var options1 = new ChargeCreateOptions
            //{
            //    Amount = 2000,
            //    Currency = "inr",
            //    Source = "tok_visa",
            //    Description = "My First Test Charge (created for API docs)",
            //};
            //var service1 = new ChargeService();
            //service1.Create(options1);
            return new CreateCheckoutSessionResponse
            {
                SessionId = session.Id,
            };
        }

        public Stripe.Price CreateStripePriceService(EventManagement.Service.StripeProduct myObj)
        {
            var stripeSecretKey = new StripeClient(_configuration["StripeSecret"]);

            var options = new PriceCreateOptions();
            if (myObj.Type == "payment")
            {
                options = new PriceCreateOptions
                {
                    UnitAmount = Convert.ToInt32(myObj.UnitAmount.ToString()),
                    Currency = myObj.Currency,
                    Product = _configuration["MarketPlaceProductId"]
                };
            }
            else if (myObj.Type == "recurring")
            {
                options = new PriceCreateOptions
                {
                    UnitAmount = Convert.ToInt32(myObj.UnitAmount.ToString()),
                    Currency = myObj.Currency,
                    Product = _configuration["MarketPlaceProductId"],
                    Recurring = new PriceRecurringOptions
                    {
                        Interval = myObj.Interval,
                    },
                };
            }
            var service = new PriceService(stripeSecretKey);
            var price = service.Create(options);
            return price;
        }
        public Stripe.Product DeleteStripeProductService(string id)
        {
            var StripeKey = _configuration["StripeSecret"];
            StripeConfiguration.ApiKey = StripeKey;
            var service = new Stripe.ProductService();
            var productId = _configuration["StripeProductId"] ?? id;
            var price = service.Delete(productId);
            return price;
        }

        public PlanDto GetPlanById(Guid id)
        {
            return _planService.GetPlansById(id);
        }

        public bool IsMonthlyPlan(string subscriptionId)
        {
            try
            {
                var stripeSecretKey = new StripeClient(_configuration["StripeSecret"]);

                var subscriptionService = new SubscriptionService(stripeSecretKey);
                var subscription = subscriptionService.Get(subscriptionId);

                var stripeRes = JsonConvert.DeserializeObject<Subscription>(subscription.StripeResponse.Content);

                if (!string.IsNullOrEmpty(stripeRes.Items.Data[0].Plan.Interval) &&
                    stripeRes.Items.Data[0].Plan.Interval == "year")
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                return true;
            }
        }

        public async Task<SubscriptionResponse> CreateAndUpdateSubscription(string priceId, Guid companyId, string defaultPlanId, string baseUrl, string subscriptionId)
        {
            //subscriptionId = "sub_1Nt3gDDlhoqIyFUOrpAwX3pv";
            var retVal = new SubscriptionResponse();
            try
            {
                //subscriptionId = "sub_1NsP5UDlhoqIyFUOA8P3cdik";

                int projectCount = 0;
                int projectCountForCustomPlan = 1;
                var _baseUrl = baseUrl;
                var stripeSecretKey = new StripeClient(_configuration["StripeSecret"]);

                var agencyMonthly = _configuration.GetSection("StripeSubscriptionPriceId:AgencyMonthly").Value;
                var agencyYearly = _configuration.GetSection("StripeSubscriptionPriceId:AgencyYearly").Value;

                var startupMonthly = _configuration.GetSection("StripeSubscriptionPriceId:StartupMonthly").Value;
                var startupYearly = _configuration.GetSection("StripeSubscriptionPriceId:StartupYearly").Value;

                var professionalMonthly = _configuration.GetSection("StripeSubscriptionPriceId:ProfessionalMonthly").Value;
                var professionalYearly = _configuration.GetSection("StripeSubscriptionPriceId:ProfessionalYearly").Value;

                if (priceId.Equals(agencyYearly) || priceId.Equals(agencyMonthly))
                {
                    projectCount = 20;
                }
                else if (priceId.Equals(startupYearly) || priceId.Equals(startupMonthly))
                {
                    projectCount = 5;
                }
                else if (priceId.Equals(professionalMonthly) || priceId.Equals(professionalYearly))
                {
                    projectCount = 50;
                }

                //Update Subscription

                //we are going to update subscription plan if sub id exist
                if (!string.IsNullOrEmpty(subscriptionId) && subscriptionId.Contains("sub_"))
                {
                    var subscriptionService = new SubscriptionService(stripeSecretKey);
                    var subscription = await subscriptionService.GetAsync(subscriptionId);

                    //Update plan if subscription is active
                    if (subscription.Status == "active")
                    {
                        var updateoptions = new SubscriptionUpdateOptions
                        {
                            Items = new List<SubscriptionItemOptions>
                        {
                            new SubscriptionItemOptions
                            {
                                Id = subscription.Items.Data[0].Id,
                                Price = priceId,
                                Quantity = projectCountForCustomPlan,

                            },
                        },

                            ProrationBehavior = "always_invoice",
                            ProrationDate = DateTime.UtcNow

                        };
                        var service = new SubscriptionService(stripeSecretKey);
                        var update_sub = service.Update(subscriptionId, updateoptions);

                        //Create new plan if payment status is paid
                        if (update_sub.Status == "active")
                        {
                            var currentPlan = _companyPlanRepository.GetFilteredEntities().Where(x => x.CompanyId == companyId && x.Active == true).FirstOrDefault();
                            currentPlan.Active = false;
                            _companyPlanRepository.UpdateEntity(currentPlan);

                            var newPlanDetails = _defaultPlanService.GetDefaultPlanById(defaultPlanId);

                            // add new plan entry
                            var entity = new CompanyPlan();
                            entity.CompanyId = companyId;
                            entity.DefaultPlanId = new Guid(defaultPlanId);
                            entity.Active = true;
                            entity.ExpiredOn = subscription.CurrentPeriodEnd;
                            entity.PaymentProfileId = subscription.Id;


                            entity.MaxKeywordsPerProject = newPlanDetails.MaxKeywordsPerProject;
                            entity.MaxProjects = newPlanDetails.MaxProjects;
                            entity.MaxTeamUsers = newPlanDetails.MaxTeamUsers;
                            entity.MaxClientUsers = newPlanDetails.MaxClientUsers;
                            entity.CreatedBy = "system";
                            entity.CreatedOn = DateTime.UtcNow;

                            _companyPlanRepository.CreateEntity(entity);
                            _companyPlanRepository.SaveChanges();
                        }

                        retVal.StripSubscription = update_sub;

                    }
                    else
                    {
                        retVal.ErrorMessage = "Can not upgrade or downgrade cancelled subscription";
                    }

                    return retVal;
                }
                else
                {
                    //Create new subcription
                    var options1 = new SessionCreateOptions
                    {
                        PaymentMethodTypes = new List<string> {
                            "card",
                        },
                        Mode = "subscription",

                        SuccessUrl = _baseUrl + "/company/" + companyId + "/subscription?session_id={CHECKOUT_SESSION_ID}",
                        CancelUrl = _baseUrl + "/company/" + companyId + "/subscription",
                        AllowPromotionCodes = true,

                        LineItems = new List<SessionLineItemOptions>
                          {
                                new SessionLineItemOptions
                                {
                                  Price = priceId,
                                  Quantity = projectCountForCustomPlan,
                                  DynamicTaxRates = new List<string>
                                  {
                                    _configuration["StripeTaxId"]
                                  }

                                }
                          },
                        Metadata = new Dictionary<string, string>(){
                                {"ProjectCount", projectCount.ToString()},
                                {"PlanId", defaultPlanId },
                            }
                    };

                    var sessionService = new SessionService(stripeSecretKey);
                    Session session = sessionService.Create(options1);
                    retVal.Url = session.Url;
                    return retVal;
                }
            }
            catch (StripeException e)
            {
                retVal.ErrorMessage = e.StripeError.Message;
                retVal.StatusCode = e.HttpStatusCode;
                return retVal;
            }
            catch (System.Exception e)
            {
                retVal.ErrorMessage = e.Message;
                retVal.StatusCode = System.Net.HttpStatusCode.BadRequest;
                return retVal;
            }

        }

        public async Task<SubscriptionResponse> CancelSubscription(string subscriptionId, Guid companyId)
        {
            var retVal = new SubscriptionResponse();
            try
            {
                var stripeSecretKey = new StripeClient(_configuration["StripeSecret"]);

                var subOption = new SubscriptionCancelOptions
                {
                    InvoiceNow = true,
                    Prorate = true
                };

                var service = new SubscriptionService(stripeSecretKey);

                var subscriptionRes = await service.GetAsync(subscriptionId);

                if (subscriptionRes.Status == "active")
                {
                    var response = service.Cancel(subscriptionId, subOption);

                    if (response.StripeResponse.StatusCode == HttpStatusCode.OK)
                    {
                        var currentPlan = _companyPlanRepository.GetFilteredEntities().Where(x => x.PaymentProfileId == subscriptionId && x.Active == true).FirstOrDefault();
                        currentPlan.ExpiredOn = DateTime.UtcNow;
                        _companyPlanRepository.UpdateEntity(currentPlan);

                        //Remove 20% coupon from db
                        await _stripeCouponService.DeleteBulkEntityAsync(x => x.CompanyId == companyId);
                    }

                    retVal.StripSubscription =  response;

                    return retVal;
                }
                else
                {
                    retVal.ErrorMessage = "Current Subscription is not active";

                    return retVal;
                }
            }
            catch (StripeException e)
            {
                retVal.ErrorMessage = e.StripeError.Message;
                retVal.StatusCode = e.HttpStatusCode;
                return retVal;
            }
            catch (System.Exception e)
            {
                retVal.ErrorMessage = e.Message;
                retVal.StatusCode = System.Net.HttpStatusCode.BadRequest;
                return retVal;
            }

        }

        public async Task<SubscriptionResponse> RefundCustomerBalance(string subscriptionId)
        {
            var retVal = new SubscriptionResponse();

            try
            {
                var stripeSecretKey = new StripeClient(_configuration["StripeSecret"]);

                var subOption = new SubscriptionCancelOptions
                {
                    InvoiceNow = true,
                    Prorate = true
                };

                var service = new SubscriptionService(stripeSecretKey);

                var subscriptionRes = await service.GetAsync(subscriptionId);

                if (subscriptionRes.Status == "canceled")
                {
                    var customerService = new CustomerService(stripeSecretKey);
                    var customerInfo = customerService.Get(subscriptionRes.CustomerId);

                    var options = new InvoiceListOptions
                    {
                        Limit = 100,
                        Subscription = subscriptionId,
                        Expand = new List<string>
                        {
                            "data.charge"
                        }
                    };

                    var invoiceService = new InvoiceService(stripeSecretKey);
                    StripeList<Invoice> invoices = invoiceService.List(
                      options);

                    //get charge id which has highest amount of payment we have got                    
                    var chargeDetails = invoices.Where(x => x.Charge != null).OrderByDescending(invoice => invoice.Charge.Amount)
                                              .Select(c => c.Charge)
                                              .ToList();

                    if (invoices.StripeResponse.StatusCode == HttpStatusCode.OK &&
                       customerInfo.StripeResponse.StatusCode == HttpStatusCode.OK &&
                       customerInfo.Balance < 0)
                    {
                        // Loop through the charges to calculate and process refunds
                        var remainingRefundAmount = Math.Abs(customerInfo.Balance);

                        retVal.Refund = new List<Refund>();

                        foreach (var charge in chargeDetails)
                        {
                            if (remainingRefundAmount > 0)
                            {
                                var refundAmountForCharge = Math.Min(remainingRefundAmount, charge.Amount);

                                var refundOptions = new RefundCreateOptions
                                {
                                    Charge = charge.Id,
                                    Amount = refundAmountForCharge
                                };

                                var refundService = new RefundService(stripeSecretKey);
                                var refundResponse = refundService.Create(refundOptions);

                                remainingRefundAmount -= refundAmountForCharge;

                                // Reset customer balance after refunding
                                if (refundResponse.Status == "succeeded" && remainingRefundAmount <= 0)
                                {
                                    var cusOptions = new CustomerBalanceTransactionCreateOptions
                                    {
                                        Amount = Math.Abs(customerInfo.Balance),
                                        Currency = customerInfo.Currency,
                                    };
                                    var cusService = new CustomerBalanceTransactionService(stripeSecretKey);
                                    cusService.Create(customerInfo.Id, cusOptions);

                                    retVal.Refund.Add(refundResponse);
                                }
                            }
                            else
                            {
                                // No more refund needed
                                break;
                            }
                        }
                    }
                  
                    return retVal;
                }
                else
                {
                    retVal.ErrorMessage = "Current Subscription is not active.";
                    return retVal;
                }
            }
            catch (StripeException e)
            {
                retVal.ErrorMessage = e.StripeError.Message;
                retVal.StatusCode = e.HttpStatusCode;
                return retVal;
            }
            catch (System.Exception e)
            {
                retVal.ErrorMessage = e.Message;
                retVal.StatusCode = System.Net.HttpStatusCode.BadRequest;
                return retVal;
            }
        }


        public async Task<InvoiceAndRefundList> GetAllInvoice(string subscriptionId, Guid companyId)
        {
            var retVal = new InvoiceAndRefundList();

            var listOfInvoices = new List<StripeInvoiceDto>();

            var listOfRefunds = new List<Refund>();

            var stripeSecretKey = new StripeClient(_configuration["StripeSecret"]);

            try
            {
                var subsciptionIds = _companyPlanRepository.GetAllEntities(false).Where(x => x.CompanyId == companyId && x.PaymentProfileId.Contains("sub_")).Select(x => x.PaymentProfileId).Distinct().ToList();

                //Get product list
                var product_options = new ProductListOptions
                {
                    Limit = 40,
                };
                var product_service = new Stripe.ProductService(stripeSecretKey);
                StripeList<Stripe.Product> products = product_service.List(product_options);

                foreach (var sub in subsciptionIds)
                {                    
                    var options = new InvoiceListOptions
                    {
                        Limit = 100,
                        Subscription = sub,
                        Expand = new List<string>
                          {
                              "data.charge.refunds",
                              "data.subscription"
                          }
                    };

                    var invoiceService = new InvoiceService(stripeSecretKey);
                    StripeList<Invoice> invoices = await invoiceService.ListAsync(options);

           
                    if (invoices.Count() > 0)
                    {
                        var refunds = invoices.Where(x => x.Charge != null).SelectMany(x => x.Charge.Refunds).ToList();

                        var myInvoice = invoices.Select(x => new StripeInvoiceDto
                        {
                            AmountPaid = x.AmountPaid,
                            Id = x.Id,
                            Status = x.Status,
                            InvoicePdf = x.InvoicePdf,
                            SubscriptionId = x.SubscriptionId,
                            ProductId = x.Lines.Data.Where(x => x.Price != null).Select(x => x.Price.ProductId).LastOrDefault(),
                            PlanName = products.Where(y => y.Id == x.Lines.Data.Where(x => x.Price != null).Select(x => x.Price.ProductId).LastOrDefault()).Select(y => y.Name).FirstOrDefault(),
                            InvoiceDate = x.Created,
                            Currency = x.Currency

                        }).ToList();

                        listOfRefunds.AddRange(refunds);
                        listOfInvoices.AddRange(myInvoice);

                    }
                    else
                    {
                        retVal.ErrorMessage = "Invoice Not Found";
                        return retVal;
                    }
                }

                listOfInvoices = listOfInvoices.OrderByDescending(x => x.InvoiceDate).ToList();
                listOfRefunds = listOfRefunds.OrderByDescending(x => x.Created).ToList();

                retVal.StripeRefunds = listOfRefunds;
                retVal.StripeInvoices = listOfInvoices;

                return retVal;
            }
            catch (StripeException e)
            {
                retVal.ErrorMessage = e.StripeError.Message;
                return retVal;
            }
            catch (System.Exception ex)
            {
                retVal.ErrorMessage = ex.Message;
                return retVal;                
            }
        }
        #endregion


        #region OVERRIDDEN IMPLEMENTATION

        public override Dictionary<string, PropertyMappingValue> GetPropertyMapping()
        {
            return new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "Id", new PropertyMappingValue(new List<string>() { "Id" } ) },
                        { "UpdatedOn", new PropertyMappingValue(new List<string>() { "UpdatedOn" } )},
                    };
        }

        public override string GetDefaultOrderByColumn()
        {
            return "UpdatedOn";
        }

        public override string GetDefaultFieldsToSelect()
        {
            return "Id,UpdatedOn,CreatedOn,PlanId,UserId,PaymentCycle,Amount,CampaignId,IsActive,StripePaymentId,StripeSubscriptionId,Plan,AspNetUsers";
        }

        #endregion
    }

}
