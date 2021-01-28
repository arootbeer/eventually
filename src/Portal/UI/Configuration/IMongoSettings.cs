namespace Eventually.Portal.UI.Configuration
{
    public interface IMongoSettings
    {
        string Address { get; }

        int Port { get; }

        string DatabaseName { get; }
        
        string User { get; }
        
        string Password { get; }
    }
}