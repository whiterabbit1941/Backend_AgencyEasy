using System;
using System.Threading.Tasks;
using EventManagement.Dto;
using EventManagement.Domain.Entities;
using Newtonsoft.Json;
using Stripe;
using System.Collections.Generic;

namespace EventManagement.Service
{
    public interface IStripePaymentService : IService<StripePayment, Guid>
    {
        CreateCheckoutSessionResponse PaymentWithStripe();
        Stripe.Price CreateStripePriceService(EventManagement.Service.StripeProduct myObj);

        //Stripe.PaymentMethod CreateStripeSubscriptionService(EventManagement.Service.StripePaymentMethod myObj);
        Stripe.Product DeleteStripeProductService(string stripeProductId);

        PlanDto GetPlanById(Guid id);
        public bool IsMonthlyPlan(string subscriptionId);
        Task<SubscriptionResponse> CreateAndUpdateSubscription(string priceId, Guid companyId, string defaultPlanId, string baseUrl, string subscriptionId);

        Task<SubscriptionResponse> CancelSubscription(string subscriptionId, Guid companyId);

        Task<SubscriptionResponse> RefundCustomerBalance(string subscriptionId);

        Task<InvoiceAndRefundList> GetAllInvoice(string subscriptionId, Guid companyId);
    }
    public class CreateCheckoutSessionResponse
    {
        [JsonProperty("sessionId")]
        public string SessionId { get; set; }
    }
    public class StripeProduct
    {
        public string PriceId { get; set; }
        public string UnitAmount { get; set; }
        public string Currency { get; set; }
        public string Interval { get; set; }
        public string ProductId { get; set; }
        public string Type { get; set; }
    }
    public class StripeProductDelete
    {
        public string id { get; set; }
        public string ob { get; set; }
        public string deleted { get; set; }
    }
    public class StripePaymentMethod
    {
        public string type { get; set; }
        public string number { get; set; }
        public string expmonth { get; set; }
        public string expyear { get; set; }
        public string cvc { get; set; }
    }
}
