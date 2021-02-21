using System;
using System.Collections.Generic;
using Eventually.Domain.EventBuilders;
using Eventually.Interfaces.DomainCommands.IAAA.Users;
using Eventually.Interfaces.DomainEvents.IAAA.Users;
using Eventually.Utilities.Collections;
using Eventually.Utilities.Extensions;
using NodaTime;
using Enumerable = System.Linq.Enumerable;

namespace Eventually.Domain.IAAA.Users
{
    public class User : AggregateBase<User, CreateUserCommand, IUserEvent>
    {
        public const string DefaultProviderName = "Eventually";
        public const string DefaultProviderId = "F33037E9F5B04D3B981258D9744F62CE";

        private static readonly PasswordHasher PasswordHasher = new();
        public static readonly ConcurrentHashSet<string> ExistingUserLogins = new();
        internal static readonly UserLoginHashGenerator HashGenerator = new(); 
        
        // object state fields
#pragma warning disable 649
        private string _username;
        private byte[] _saltedPasswordHash;
        private byte[] _passwordSalt;
        private readonly List<LoginHistoryEntry> _loginHistory = new();
        private bool _active;
        private bool _locked;
        private readonly HashSet<string> _logins = new();
        private readonly Dictionary<string, byte[]> _tokens = new();
#pragma warning restore 649

        private User(Guid id) : base(id) { }

        protected override void ConsiderCreateCommand(CreateUserCommand command)
        {
            var loginHash = HashGenerator.Hash(command.Username);
            ValidateUserName(command.Username, loginHash);
            ValidatePassword(command.Password);
        }

        protected override void InitializeFromCreateCommand(CreateUserCommand command)
        {
            var (saltedPasswordHash, passwordSalt) = PasswordHasher.Hash(command.Password);

            Raise(
                EntityCreatedEventBuilder<UserCreated>
                    .For(this)
                    .From(command)
                    .With(uc => uc.Username, command.Username)
                    .With(uc => uc.SaltedPasswordHash, saltedPasswordHash)
                    .With(uc => uc.PasswordSalt, passwordSalt)
                    .Build()
                );

            Raise(
                EntityChangedEventBuilder<LoginAddedToUser>.For(this)
                    .From(command)
                    .With(latu => latu.LoginHash, HashGenerator.Hash(command.Username))
                    .With(latu => latu.LoginProvider, DefaultProviderName)
                    .With(latu => latu.ProviderDisplayName, command.Username)
                    .Build()
            );
        }

        public void Apply(UserCreated uc)
        {
            _username = uc.Username;
            SetPasswordParams(uc.SaltedPasswordHash, uc.PasswordSalt);
        }

        public void ChangeUserName(ChangeUserNameCommand command)
        {
            var oldLoginHash = HashGenerator.Hash(_username);
            var newLoginHash = HashGenerator.Hash(command.Username);
            ValidateUserName(command.Username, newLoginHash);

            Raise(
                EntityChangedEventBuilder<LoginRemovedFromUser>
                    .For(this)
                    .From(command)
                    .With(lrfu => lrfu.LoginProvider, command.Username)
                    .With(lrfu => lrfu.LoginHash, oldLoginHash)
                    .Build()
            );
            Raise(
                EntityChangedEventBuilder<LoginAddedToUser>.For(this)
                    .From(command)
                    .With(latu => latu.LoginHash, newLoginHash)
                    .With(latu => latu.LoginProvider, DefaultProviderName)
                    .With(latu => latu.ProviderDisplayName, command.Username)
                    .Build()
            );
        }

        private void ValidateUserName(string userName, string loginHash)
        {
            if (userName.IsNullOrWhitespace())
            {
                throw new Exception("The user name may not be empty");
            }

            lock (ExistingUserLogins)
            {
                if (ExistingUserLogins.Contains(loginHash))
                {
                    throw new Exception("The user name must be unique.");
                }
            }
        }

        public void ChangePassword(ChangePasswordCommand command)
        {
            ValidatePassword(command.Password);
            var (saltedPasswordHash, passwordSalt) = PasswordHasher.Hash(command.Password);
            Raise(
                EntityChangedEventBuilder<UserPasswordChanged>
                    .For(this)
                    .From(command)
                    .With(uc => uc.SaltedPasswordHash, saltedPasswordHash)
                    .With(uc => uc.PasswordSalt, passwordSalt)
                    .Build()
            );
        }

        private void ValidatePassword(string password)
        {
            //todo
        }

        public void Apply(UserPasswordChanged upc)
        {
            SetPasswordParams(upc.SaltedPasswordHash, upc.PasswordSalt);
        }

        private void SetPasswordParams(byte[] saltedPasswordHash, byte[] passwordSalt)
        {
            _saltedPasswordHash = saltedPasswordHash;
            _passwordSalt = passwordSalt;
        }

        public void AttemptLogin(LoginUserCommand command)
        {
            var loginResult = ValidateLogin(command);

            lock (_loginHistory)
            {
                var userLoginAttempted = EntityChangedEventBuilder<UserLoginAttempted>
                    .For(this)
                    .From(command)
                    .With(ula => ula.Result, loginResult)
                    .With(ula => ula.IPAddress, command.IPAddress)
                    .Build();
                
                Raise(userLoginAttempted);
            }
        }

        private string ValidateLogin(LoginUserCommand command)
        {
            //always perform all validation steps to combat timing attacks
            var hash = PasswordHasher.Hash(command.Password, _passwordSalt);
            //todo: if (!(validate IP Address)) {

            if (!_active && !_locked && Enumerable.SequenceEqual(hash, _saltedPasswordHash))
            {
                return UserLoginAttempted.LoginAttemptResult.Succeeded;
            }

            if (LoginFailureShouldResultInLockout)
            {
                return UserLoginAttempted.LoginAttemptResult.Lockout;
            }

            return UserLoginAttempted.LoginAttemptResult.Failed;
        }

        public bool LoginFailureShouldResultInLockout =>
            //non-short-circuiting: always perform the history validation step to combat timing attacks
            !_locked &
            Enumerable.All(Enumerable.TakeLast(Enumerable.OrderBy(_loginHistory, lhe => lhe.Timestamp), 5), lhe => !lhe.Allowed);

        private void UpdateLoginHistory(Guid eventId, Instant timestamp)
        {
        }

        private class LoginHistoryEntry
        {
            public Guid EventId { get; set; }
            
            public Instant Timestamp { get; set; }

            public bool Allowed { get; set; }
        }

        public void Apply(UserLoginAttempted uli)
        {
            _loginHistory.Add(
                new LoginHistoryEntry
                {
                    EventId = uli.Identity,
                    Timestamp = uli.Timestamp,
                    Allowed = true
                }
            );

            if (uli.Result == UserLoginAttempted.LoginAttemptResult.Lockout)
            {
                _locked = true;
            }
        }
        
        public void AddLogin(AddLoginToUserCommand command)
        {
            if (_logins.Contains(command.LoginHash))
            {
                throw new Exception("This login is already associated with this user");
            }

            lock (ExistingUserLogins)
            {
                if (ExistingUserLogins.Contains(command.LoginHash))
                {
                    throw new Exception("This login is already associated with another user");
                }
            }

            var loginAddedToUser = EntityChangedEventBuilder<LoginAddedToUser>.For(this)
                .From(command)
                .With(latu => latu.LoginHash, command.LoginHash)
                .With(latu => latu.LoginProvider, command.LoginProvider)
                .With(latu => latu.ProviderDisplayName, command.ProviderDisplayName)
                .Build();
            
            Raise(loginAddedToUser);
        }

        public void Apply(LoginAddedToUser latu)
        {
            _logins.Add(latu.LoginHash);
        }

        public void SetLoginToken(SetUserLoginTokenCommand command)
        {
            var userLoginTokenSet = EntityChangedEventBuilder<UserLoginTokenSet>.For(this)
                .From(command)
                .With(ults => ults.LoginProvider, command.LoginProvider)
                .With(ults => ults.TokenName, command.TokenName)
                .With(ults => ults.TokenValue, command.TokenValue)
                .Build();
            
            Raise(userLoginTokenSet);
        }
    }
}