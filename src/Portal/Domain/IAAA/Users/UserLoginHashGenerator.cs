using System;
using System.Security.Cryptography;
using System.Text;

namespace Eventually.Portal.Domain.IAAA.Users
{
    public class UserLoginHashGenerator : IUserLoginHashGenerator
    {
        public string Hash(string username)
        {
            return Hash(User.DefaultProviderId, username);
        }

        public string Hash(string loginProvider, string providerKey)
        {
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes($"{loginProvider}-+-{providerKey}"));
                var result = Convert.ToBase64String(hash);
                return result;
            }
        }
    }
}