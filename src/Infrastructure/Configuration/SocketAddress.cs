namespace Eventually.Portal.Infrastructure.Configuration
{
    public class SocketAddress : ISocketAddress
    {
        public string Protocol { get; set; }

        public string Address { get; set; }

        public int Port { get; set; }
    }
}