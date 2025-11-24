using System;
using System.Collections.Generic;
using EventManagement.Domain.Entities;

namespace EventManagement.Domain   
{
    public static class EventManagementContextExtensions
    {
        public static void EnsureSeedDataForContext(this EventManagementContext context)
        {
            // first, clear the database.  This ensures we can always start 
            // fresh with each demo.  Not advised for production environments, obviously :-)

            context.Events.RemoveRange(context.Events);
            context.SaveChanges();

            var events = new List<Event>()
            {
                new Event()
                {
                    Id = new Guid("25320c5e-f58a-4b1f-b63a-8ee07a840bdf"),
                    Name = "Queens of the Stone Age",
                    CreatedBy = "system",
                    CreatedOn = DateTime.UtcNow
                },
                new Event()
                {
                    Id = new Guid("83b126b9-d7bf-4f50-96dc-860884155f8b"),
                    Name = "Nick Cave and the Bad Seeds",
                    CreatedBy = "system",
                    CreatedOn = DateTime.UtcNow
                }
            };
      
            context.Events.AddRange(events);
            context.SaveChanges();
        }
    }
}
