﻿using Eventually.Interfaces.DomainEvents;

namespace Eventually.Portal.Domain.IAAA.Events.Users
{
    public class LoginRemovedFromUser : DomainEntityChangedEventBase, IUserEvent
    {
        public string LoginProvider { get; }
        
        public string LoginHash { get; }
    }
}