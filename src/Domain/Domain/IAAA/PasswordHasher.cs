using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace Eventually.Domain.IAAA
{
    public class PasswordHasher
    {
        private readonly int _memorySize;

        public PasswordHasher()
        {
            _memorySize = 512 * 1024; //512 MB
        }

        public PasswordHasher(int memorySize)
        {
            _memorySize = memorySize;
        }

        public (byte[] saltedPasswordHash, byte[] passwordSalt) Hash(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[128];
            rng.GetBytes(salt);

            return (Hash(password, salt), salt);
        }

        public byte[] Hash(string password, byte[] salt)
        {
            using var argon2 = new Argon2id(Encoding.Unicode.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = 8,
                Iterations = 5,
                MemorySize = _memorySize
            };

            // get the hash
            return argon2.GetBytes(128);
        }
    }
}