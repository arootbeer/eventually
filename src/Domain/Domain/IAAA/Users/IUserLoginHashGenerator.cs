namespace Eventually.Domain.IAAA.Users
{
    public interface IUserLoginHashGenerator
    {
        string Hash(string username);
        
        string Hash(string loginProvider, string providerKey);
    }
}