using System;

namespace EventManagement.Dto
{
    public abstract class CompanyUserAbstractBase
    {
        /// <summary>
        /// CompanyUser Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// CompanyUser Name.
        /// </summary>
        public string Name { get; set; }

    }
}
