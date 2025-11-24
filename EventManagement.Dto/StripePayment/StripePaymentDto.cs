using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using Stripe;
using System.Collections.Generic;

namespace EventManagement.Dto
{
    /// <summary>
    /// StripePayment Model
    /// </summary>
    public class StripePaymentDto : StripePaymentAbstractBase
    {

    }

    public class PaymentIntentDto
    {
        public string Status { get; set; }

        public long Amount { get; set; }
    }

    public class StripeInvoiceDto
    {
        public string Id { get; set; }

        public string Status { get; set; }

        public string InvoicePdf { get; set; }

        public long AmountPaid { get; set; }
         
        public string PlanName { get; set; }

        public string SubscriptionId { get; set; }

        public string ProductId { get; set; }

        public string Currency { get; set; }
        public DateTime InvoiceDate { get; set; }
    }

    public class InvoiceAndRefundList
    {
        public List<Refund> StripeRefunds { get; set; }

        public List<StripeInvoiceDto> StripeInvoices { get; set; }

        public string ErrorMessage { get; set; }

    }


    public class SubscriptionResponse
    {
        public string Url { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public string ErrorMessage { get; set; }

        public Subscription StripSubscription { get; set; }

        public List<Refund> Refund { get; set; }

    }

}
 