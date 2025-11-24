using System;

namespace EventManagement.Dto
{
    public abstract class StripePaymentAbstractBase
    {
        /// <summary>
        /// StripePayment Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// StripePayment Name.
        /// </summary>
        public string Name { get; set; }

    }
}
