namespace Eventually.Portal.Infrastructure.Configuration
{
    public interface ISocketAddress
    {
        string Protocol { get; }

        string Address { get; }

        int Port { get; }
    }
}