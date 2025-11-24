using FinanaceManagement.API.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagement.Domain.Entities
{
    public class StripePayment : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
		[ForeignKey("PlanId")]
		public virtual Plan Plan { get; set; }
		[ForeignKey("CampaignId")]
		public virtual Campaign Campaign { get; set; }
		[ForeignKey("UserId")]
		public virtual AspNetUsers AspNetUsers { get; set; }

	}
}
