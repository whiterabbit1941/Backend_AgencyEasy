using System;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.Dto
{
    public class StripePaymentForCreation : StripePaymentAbstractBase
    {
        public Guid Id { get; set; }
        public double? Amount { get; set; }
        public string UserId { get; set; }
        public Guid PlanId { get; set; }
        public string PaymentCycle { get; set; }
        public Guid CampaignId { get; set; }
        public string PaymentMode { get; set; }
        public bool IsActive { get; set; }
        public string StripePaymentId { get; set; }
        public string StripeSubscriptionId { get; set; }
    }
}
