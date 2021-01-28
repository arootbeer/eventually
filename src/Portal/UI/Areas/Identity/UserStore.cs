using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Eventually.Domain.IAAA.Users;
using Eventually.Interfaces.DomainCommands;
using Eventually.Interfaces.DomainCommands.IAAA.Roles;
using Eventually.Interfaces.DomainCommands.IAAA.Users;
using Eventually.Interfaces.DomainCommands.MessageBuilders.Commands;
using Eventually.Portal.UI.Areas.Identity.Data;
using Eventually.Portal.UI.Areas.Identity.EventHandlers.IAAA.Users;
using Eventually.Portal.UI.Domain;
using Eventually.Utilities.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Eventually.Portal.UI.Areas.Identity
{
    public class UserStore :
        IUserPasswordStore<PortalUser>,
        IUserClaimStore<PortalUser>,
        IRoleStore<ServerUIRole>,
        IUserSecurityStampStore<PortalUser>,
        IUserRoleStore<PortalUser>,
        IUserEmailStore<PortalUser>,
        IUserLoginStore<PortalUser>,
        IUserAuthenticationTokenStore<PortalUser>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserStore> _logger;
        private readonly IDomainCommandBus _commandBus;
        private readonly IUserLoginHashGenerator _userLoginHashGenerator;
        private readonly IMongoDatabase _mongo;
        private readonly IMongoCollection<ServerUIRole> _roles;
        private readonly IMongoCollection<PortalUser> _users;

        public UserStore(
            IHttpContextAccessor httpContextAccessor,
            IMongoDatabase mongo,
            IDomainCommandBus commandBus,
            IUserLoginHashGenerator userLoginHashGenerator,
            ILogger<UserStore> logger
        )
        {
            _httpContextAccessor = httpContextAccessor;
            _commandBus = commandBus;
            _userLoginHashGenerator = userLoginHashGenerator;
            _logger = logger;
            _mongo = mongo;
            _users = mongo.GetCollection<PortalUser>(nameof(PortalUser));
            _roles = mongo.GetCollection<ServerUIRole>(nameof(ServerUIRole));
        }

        private PortalUser _currentUser;

        internal static readonly Guid UserIdForUserCreation = Guid.Parse("2F1193E1-E3A3-4F20-B0B2-A4C315DB6995");

        private PortalUser CurrentUser
        {
            get
            {
                if (_currentUser == null)
                {
                    var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    _currentUser = ((IUserStore<PortalUser>) this).FindByIdAsync(userId, CancellationToken.None).Result;
                }

                return _currentUser;
            }
        }

        private async Task<IdentityResult> ExecuteCommand(DomainCommand command, CancellationToken cancellationToken)
        {
            return await _commandBus.ExecuteCommand(command, MapCommandResponseToIdentityResult, cancellationToken);
        }

        private IdentityResult MapCommandResponseToIdentityResult(DomainCommandResponse commandResponse, CancellationToken cancellationToken)
        {
            return commandResponse.Succeeded
                ? IdentityResult.Success
                : IdentityResult.Failed(commandResponse.Errors
                    .Select(m => new IdentityError { Description = m.Reason })
                    .ToArray());
        }

        private async Task<ServerUIRole> FindRoleMatching(Expression<Func<ServerUIRole, bool>> predicate, CancellationToken cancellationToken)
        {
            return await Task.Factory.StartNew(
                () =>
                {
                    var user = _roles.Find(predicate).SingleOrDefault();
                    return user;
                },
                cancellationToken
            );
        }

        private async Task<PortalUser> FindUserMatching(Expression<Func<PortalUser, bool>> predicate, CancellationToken cancellationToken)
        {
            return await FindUsersMatching(predicate, cancellationToken)
                .ContinueWith(task => task.Result.SingleOrDefault(), cancellationToken);
        }

        private async Task<IEnumerable<PortalUser>> FindUsersMatching(Expression<Func<PortalUser, bool>> predicate, CancellationToken cancellationToken)
        {
            return await Task.Factory.StartNew(
                () => _users.Find(predicate).ToEnumerable(),
                cancellationToken
            );
        }

        public async Task AddClaimsAsync(PortalUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task AddLoginAsync(PortalUser user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            var command = ChangeEntityCommandBuilder.For<AddLoginToUserCommand>(user.Id)
                .IssuedBy(CurrentUser?.Id ?? user.Id)
                .With(cmd => cmd.LoginProvider, login.LoginProvider)
                .With(cmd => cmd.LoginHash, _userLoginHashGenerator.Hash(login.LoginProvider, login.ProviderKey))
                .With(cmd => cmd.ProviderDisplayName, login.ProviderDisplayName)
                .Build();

            await ExecuteCommand(command, cancellationToken);
        }

        public async Task AddToRoleAsync(PortalUser user, string roleName, CancellationToken cancellationToken)
        {
            var roleId = ((IRoleStore<ServerUIRole>) this).FindByNameAsync(roleName, cancellationToken)
                .Result
                .Id;
            
            var command = ChangeEntityCommandBuilder.For<AssignRoleToUserCommand>(roleId)
                .IssuedBy(CurrentUser.Id)
                .With(cmd => cmd.UserId, user.Id)
                .Build();

            await ExecuteCommand(command, cancellationToken);
        }

        public async Task AttemptLoginAsync(PortalUser user, string password, CancellationToken cancellationToken)
        {
            var command = ChangeEntityCommandBuilder.For<LoginUserCommand>(user.Id)
                .IssuedBy(Guid.Empty) //anonymous
                .With(cmd => cmd.Password, password)
                .Build();

            await ExecuteCommand(command, cancellationToken)
                .ContinueWith(
                    t => Task.Delay(250, cancellationToken), //await event handler processing
                    cancellationToken
                );
        }

        public async Task<IdentityResult> CreateAsync(PortalUser user, CancellationToken cancellationToken)
        {
            var command = CreateEntityCommandBuilder.For<CreateUserCommand>()
                .IssuedBy(CurrentUser?.Id ?? UserIdForUserCreation)
                .With(cmd => cmd.Username, user.UserName)
                .With(cmd => cmd.Password, user.Password)
                .Build();

            user.SecurityStamp = $"{command.Identity}";
            
            return await _commandBus.ExecuteCommand(
                command,
                (response, token) =>
                {
                    user.Id = ((DomainCreateCommandResponse) response).CreatedEntityId;
                    return MapCommandResponseToIdentityResult(response, token);
                },
                cancellationToken);
        }

        public async Task<IdentityResult> CreateAsync(ServerUIRole role, CancellationToken cancellationToken)
        {
            var command = CreateEntityCommandBuilder.For<CreateRoleCommand>()
                .IssuedBy(CurrentUser.Id)
                .With(cmd => cmd.RoleName, role.Name)
                .Build();

            return await ExecuteCommand(command, cancellationToken);
        }

        public async Task<IdentityResult> DeleteAsync(PortalUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<IdentityResult> DeleteAsync(ServerUIRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<PortalUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            return await FindUserMatching(u => u.NormalizedEmail == normalizedEmail, cancellationToken);
        }

        async Task<ServerUIRole> IRoleStore<ServerUIRole>.FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            return await FindRoleMatching(r => r.Id == Guid.Parse(roleId), cancellationToken);
        }

        async Task<PortalUser> IUserStore<PortalUser>.FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            if (userId == null)
            {
                return await Task.FromResult<PortalUser>(null);
            }
            return await FindUserMatching(u => u.Id == Guid.Parse(userId), cancellationToken);
        }

        public async Task<PortalUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            if (new[] {loginProvider, providerKey}.Contains(null))
            {
                return await Task.FromResult<PortalUser>(null);
            }

            var loginHash = _userLoginHashGenerator.Hash(loginProvider, providerKey);
            var logins = _mongo.GetCollection<PortalUserLogin>(nameof(PortalUserLogin));
            var userId = logins
                .Find(login => login.LoginProvider == loginProvider && login.LoginHash == loginHash)
                .SingleOrDefaultAsync(cancellationToken)
                .ContinueWith(login => login.Result?.UserId, cancellationToken)
                .Result;

            if (!userId.HasValue)
            {
                return await Task.FromResult<PortalUser>(null);
            }
            return await FindUserMatching(user => user.Id == userId.Value, cancellationToken);
        }

        async Task<ServerUIRole> IRoleStore<ServerUIRole>.FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            return await FindRoleMatching(r => r.NormalizedName == normalizedRoleName, cancellationToken);
        }

        async Task<PortalUser> IUserStore<PortalUser>.FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return await FindUserMatching(u => u.NormalizedUserName == normalizedUserName, cancellationToken);
        }

        public async Task<IList<Claim>> GetClaimsAsync(PortalUser user, CancellationToken cancellationToken)
        {
            return await Task.FromResult(new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName)
                }
            );
        }

        public async Task<string> GetEmailAsync(PortalUser user, CancellationToken cancellationToken)
        {
            return await Task.FromResult(user.Email);
        }

        public async Task<bool> GetEmailConfirmedAsync(PortalUser user, CancellationToken cancellationToken)
        {
            return await Task.FromResult(user.EmailConfirmed);
        }

        public async Task<IList<UserLoginInfo>> GetLoginsAsync(PortalUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetNormalizedEmailAsync(PortalUser user, CancellationToken cancellationToken)
        {
            return await Task.FromResult(user.NormalizedEmail);
        }

        public async Task<string> GetNormalizedRoleNameAsync(ServerUIRole role, CancellationToken cancellationToken)
        {
            return await Task.FromResult(role.NormalizedName);
        }

        public async Task<string> GetNormalizedUserNameAsync(PortalUser user, CancellationToken cancellationToken)
        {
            return await Task.FromResult(user.NormalizedUserName);
        }

        public async Task<string> GetPasswordHashAsync(PortalUser user, CancellationToken cancellationToken)
        {
            return await Task.FromResult("");
        }

        public async Task<string> GetRoleIdAsync(ServerUIRole role, CancellationToken cancellationToken)
        {
            return await Task.FromResult(role.Id.ToString());
        }

        public async Task<string> GetRoleNameAsync(ServerUIRole role, CancellationToken cancellationToken)
        {
            return await Task.FromResult(role.Name);
        }

        public async Task<IList<string>> GetRolesAsync(PortalUser user, CancellationToken cancellationToken)
        {
            return await Task.FromResult(user.Roles.Values.ToList());
        }

        public async Task<string> GetSecurityStampAsync(PortalUser user, CancellationToken cancellationToken)
        {
            return await Task.FromResult(user.SecurityStamp);
        }

        public Task<string> GetTokenAsync(PortalUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetUserIdAsync(PortalUser user, CancellationToken cancellationToken)
        {
            return await Task.FromResult(user.Id.ToString());
        }

        public async Task<string> GetUserNameAsync(PortalUser user, CancellationToken cancellationToken)
        {
            return await Task.FromResult(user.UserName);
        }

        public async Task<IList<PortalUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<IList<PortalUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            return await FindUsersMatching(u => u.Roles.ContainsValue(roleName), cancellationToken)
                .ContinueWith(task => task.Result.ToList(), cancellationToken);
        }

        public async Task<bool> HasPasswordAsync(PortalUser user, CancellationToken cancellationToken)
        {
            return await Task.FromResult(true);
        }

        public async Task<bool> IsInRoleAsync(PortalUser user, string roleName, CancellationToken cancellationToken)
        {
            return await Task.FromResult(
                user.Roles.Values
                    .Select(name => name.ToLowerInvariant())
                    .Contains(roleName.ToLowerInvariant())
            );
        }

        public async Task RemoveClaimsAsync(PortalUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task RemoveFromRoleAsync(PortalUser user, string roleName, CancellationToken cancellationToken)
        {
            var roleId = ((IRoleStore<ServerUIRole>)this).FindByNameAsync(roleName, cancellationToken)
                .Result
                .Id;

            var command = ChangeEntityCommandBuilder.For<RemoveRoleFromUserCommand>(roleId)
                .IssuedBy(CurrentUser.Id)
                .With(cmd => cmd.UserId, user.Id)
                .Build();

            await ExecuteCommand(command, cancellationToken);
        }

        public async Task RemoveLoginAsync(PortalUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RemoveTokenAsync(PortalUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task ReplaceClaimAsync(PortalUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task SetEmailAsync(PortalUser user, string email, CancellationToken cancellationToken)
        {
            // no-op - this is taken care of by event handlers
        }

        public async Task SetEmailConfirmedAsync(PortalUser user, bool confirmed, CancellationToken cancellationToken)
        {
            // no-op - this is taken care of by event handlers
        }

        public async Task SetNormalizedEmailAsync(PortalUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            // no-op - this is taken care of by event handlers
        }

        public async Task SetNormalizedRoleNameAsync(ServerUIRole role, string normalizedName, CancellationToken cancellationToken)
        {
            // no-op - this is taken care of by event handlers
        }

        public async Task SetNormalizedUserNameAsync(PortalUser user, string normalizedName, CancellationToken cancellationToken)
        {
            // no-op - this is taken care of by event handlers
        }

        public async Task SetRoleNameAsync(ServerUIRole role, string roleName, CancellationToken cancellationToken)
        {
            var command = ChangeEntityCommandBuilder.For<ChangeRoleNameCommand>(role.Id)
                .IssuedBy(CurrentUser.Id)
                .With(cmd => cmd.RoleName, roleName)
                .Build();

            await ExecuteCommand(command, cancellationToken);
        }

        public async Task SetSecurityStampAsync(PortalUser user, string stamp, CancellationToken cancellationToken)
        {
            // no-op - this is taken care of by event handlers
        }

        public async Task SetTokenAsync(PortalUser user, string loginProvider, string name, string value, CancellationToken cancellationToken)
        {
            var command = ChangeEntityCommandBuilder.For<SetUserLoginTokenCommand>(user.Id)
                .With(cmd => cmd.LoginProvider, loginProvider)
                .With(cmd => cmd.TokenName, name)
                .With(cmd => cmd.TokenValue, _userLoginHashGenerator.Hash(loginProvider, value))
                .Build();

            await ExecuteCommand(command, cancellationToken);
        }

        public async Task SetUserNameAsync(PortalUser user, string userName, CancellationToken cancellationToken)
        {
            var command = ChangeEntityCommandBuilder.For<ChangeUserNameCommand>(user.Id)
                .IssuedBy(CurrentUser.Id)
                .With(cmd => cmd.Username, user.UserName)
                .Build();

            await ExecuteCommand(command, cancellationToken);
        }

        public async Task SetPasswordHashAsync(PortalUser user, string passwordHash, CancellationToken cancellationToken)
        {
            var command = ChangeEntityCommandBuilder.For<ChangePasswordCommand>(user.Id)
                .IssuedBy(CurrentUser.Id)
                .With(cmd => cmd.Password, user.Password)
                .Build();

            await ExecuteCommand(command, cancellationToken);
        }

        public async Task<IdentityResult> UpdateAsync(ServerUIRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<IdentityResult> UpdateAsync(PortalUser user, CancellationToken cancellationToken)
        {
            var existing = _users.Find(u => u.Id == user.Id).SingleOrDefault();

            return await Task.FromResult(IdentityResult.Success);
        }

        private bool _disposed;
        private IUserLoginStore<PortalUser> _userLoginStoreImplementation;
        
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _logger.LogDebug($"{nameof(UserStore)} is being disposed.");
            GC.SuppressFinalize(this);

            _commandBus.Dispose();

            _logger.LogInformation($"{nameof(UserStore)} has been disposed.");
        }
    }
}