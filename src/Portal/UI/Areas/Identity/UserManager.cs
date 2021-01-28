using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eventually.Portal.UI.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Eventually.Portal.UI.Areas.Identity
{
    public class UserManager : UserManager<PortalUser>
    {
        private readonly UserStore _userStore;

        public UserManager(
            IUserStore<PortalUser> userStore,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<PortalUser> passwordHasher,
            IEnumerable<IUserValidator<PortalUser>> userValidators,
            IEnumerable<IPasswordValidator<PortalUser>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<PortalUser>> logger
        ) : base(userStore, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            _userStore = (UserStore) userStore;
        }

        public override async Task<IdentityResult> CreateAsync(PortalUser user)
        {
            ThrowIfDisposed();

            return await Store.CreateAsync(user, CancellationToken);
        }

        protected override async Task<PasswordVerificationResult> VerifyPasswordAsync(IUserPasswordStore<PortalUser> store, PortalUser user, string password)
        {
            ThrowIfDisposed();

            try
            {
                await _userStore.AttemptLoginAsync(user, password, CancellationToken);
                return await Task.FromResult(PasswordVerificationResult.Success);
            }
            catch
            {
                return await Task.FromResult(PasswordVerificationResult.Failed);
            }
        }
    }
}