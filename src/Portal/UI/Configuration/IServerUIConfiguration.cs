using System;
using Eventually.Portal.Infrastructure.Configuration;

namespace Eventually.Portal.UI.Configuration
{
    public interface IServerUIConfiguration
    {
        Guid ServerIdentity { get; }

        ISocketAddress ServerAddress { get; }

        IMongoSettings ViewModelDatabase { get; }
    }
}