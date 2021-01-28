﻿using Eventually.Interfaces.DomainEvents.IAAA.Roles;
using Eventually.Portal.UI.Areas.Identity.Data;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Eventually.Portal.UI.Areas.Identity.EventHandlers.IAAA.Roles
{
    public class RoleCreatedEventHandler : MongoVersionedDomainEventHandler<RoleCreated, ServerUIRole>
    {
        public RoleCreatedEventHandler(IMongoDatabase database, ILoggerFactory loggerFactory) : base(database, loggerFactory) { }

        protected override void HandleInternal(RoleCreated domainEvent)
        {
            var role = new ServerUIRole
            {
                Id = domainEvent.EntityId,
                Name = domainEvent.RoleName
            };

            Collection.InsertOne(role);
        }
    }
}