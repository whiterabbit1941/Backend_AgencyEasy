using System;

namespace EventManagement.Dto
{
    public abstract class PlanAbstractBase
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Subtitle { get; set; }
        public Guid? ProductId { get; set; }
        public string Description { get; set; }
        public string Features { get; set; }
        public double Price { get; set; }
        public double RecommendedAgencyPrice { get; set; }
        public string Currency { get; set; }
        public string PaymentType { get; set; }
        public string PaymentCycle { get; set; }
        public string stripeProductId { get; set; }
        public string priceId { get; set; }

    }
}
