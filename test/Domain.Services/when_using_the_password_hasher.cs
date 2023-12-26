using Eventually.Portal.Domain.IAAA;
using Machine.Specifications;

namespace Eventually.Tests.Portal.Domain
{
    #pragma warning disable CS0169 // Field is never used
    // ReSharper disable MemberHidesStaticFromOuterClass
    
    [Subject("When using the password service")]
    public class when_using_the_password_hasher
    {
        const string password = "ThisIsAFixedPassword";
        static PasswordHasher subject;

        Establish context = () =>
        {
            subject = new PasswordHasher(1024 * 512); // 512MB
        };

        public class to_generate_an_initial_salt_and_hash_combo_from_a_password
        {
            static byte[] generated_salt;
            protected static byte[] generated_hash;

            Because of = () =>
            {
                (generated_hash, generated_salt) = subject.Hash(password);
            };

            Behaves_like<hash_bytes_generated_from_password> hash_bytes_generated_from_password;

            It should_have_generated_a_128_byte_salt = () => generated_salt.Length.ShouldEqual(128);
        }

        public class to_validate_a_password_against_an_existing_salt_and_hash_combination
        {
            static readonly byte[] existing_password_salt = {124, 17, 37, 198, 23, 64, 9, 212};
            static readonly byte[] existing_password_hash =
            {
                142, 27, 229, 223, 48, 10, 87, 105, 
                7, 106, 174, 56, 88, 212, 118, 48, 
                153, 168, 125, 122, 240, 195, 164, 69, 
                98, 180, 199, 118, 128, 17, 254, 209, 
                15, 171, 117, 255, 81, 106, 104, 101, 
                177, 60, 100, 194, 228, 171, 179, 85, 
                10, 75, 23, 58, 23, 57, 115, 206, 
                136, 78, 54, 130, 236, 211, 59, 0, 
                137, 236, 22, 167, 62, 174, 196, 163, 
                236, 18, 195, 226, 85, 186, 83, 81, 
                130, 29, 160, 129, 14, 40, 29, 239, 
                67, 219, 91, 243, 186, 4, 3, 35,
                12, 134, 160, 238, 165, 26, 146, 155, 
                201, 212, 10, 38, 188, 242, 174, 187, 
                154, 219, 219, 254, 115, 51, 13, 115, 
                75, 170, 255, 62, 167, 151, 111, 150
            };

            protected static byte[] generated_hash;

            Because of = () =>
            {
                generated_hash = subject.Hash(password, existing_password_salt);
            };

            Behaves_like<hash_bytes_generated_from_password> hash_bytes_generated_from_password;

            It should_match_the_expected_password_hash = () => generated_hash.ShouldContainOnly(existing_password_hash);
        }

        [Behaviors]
        public class hash_bytes_generated_from_password
        {
            protected static byte[] generated_hash;

            It should_be_128_bytes_long = () => generated_hash.Length.ShouldEqual(128);
        }
    }
}
