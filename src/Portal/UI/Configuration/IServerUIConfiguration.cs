namespace Eventually.Portal.UI.Configuration
{
    public interface IServerUIConfiguration
    {
        string CertificateFilePath { get; }
        
        IMongoSettings ViewModelDatabase { get; }
    }
}