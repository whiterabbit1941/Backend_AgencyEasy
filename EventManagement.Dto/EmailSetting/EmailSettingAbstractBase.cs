using System;

namespace EventManagement.Dto
{
    public abstract class EmailSettingAbstractBase
    {
        /// <summary>
        /// EmailSetting Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// EmailSetting Name.
        /// </summary>
        public string Name { get; set; }

    }
}
