namespace Eventually.Portal.UI.Configuration
{
    public class MongoSettings : IMongoSettings
    {
        public string Address { get; set; }

        public int Port { get; set; }

        public string DatabaseName { get; set; }
        
        public string User { get; set; }
        
        public string Password { get; set; }
    }
}