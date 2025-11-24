using System;

namespace EventManagement.Dto
{
    public abstract class ChatGptReportAbstractBase
    {
        /// <summary>
        /// ChatGptReport Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ChatGptReport Name.
        /// </summary>
        public string Name { get; set; }

    }
}
