namespace Eventually.Portal.UI.Configuration
{
    public class ServerUIConfiguration : IServerUIConfiguration
    {
        public string CertificateFilePath { get; private set; }
        string IServerUIConfiguration.CertificateFilePath => CertificateFilePath;

        public MongoSettings ViewModelDatabase { get; private set; }
        IMongoSettings IServerUIConfiguration.ViewModelDatabase => ViewModelDatabase;
    }
}