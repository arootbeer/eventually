using System;
using Eventually.Portal.Infrastructure.Configuration;

namespace Eventually.Portal.UI.Configuration
{
    public class ServerUIConfiguration : IServerUIConfiguration
    {
        public Guid ServerIdentity { get; set; }

        public SocketAddress ServerAddress { get; set; }
        ISocketAddress IServerUIConfiguration.ServerAddress => ServerAddress;
        
        public MongoSettings ViewModelDatabase { get; set; }
        IMongoSettings IServerUIConfiguration.ViewModelDatabase => ViewModelDatabase;
    }
}