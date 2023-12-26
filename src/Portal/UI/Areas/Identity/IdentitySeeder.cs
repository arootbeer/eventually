using System;
using System.Threading;
using System.Threading.Tasks;
using Eventually.Infrastructure.Transport.CommandBus;
using Eventually.Interfaces.DomainCommands;
using Eventually.Interfaces.DomainCommands.MessageBuilders.Commands;
using Eventually.Portal.Domain.IAAA.Commands.Roles;
using Eventually.Portal.Domain.IAAA.Commands.Users;
using Eventually.Portal.UI.Areas.Identity.Data;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Eventually.Portal.UI.Areas.Identity
{
    public class IdentitySeeder
    {
        private readonly IDomainCommandBus _commandBus;
        private readonly ILogger<IdentitySeeder> _logger;
        private readonly IMongoCollection<PortalUser> _users;
        private readonly IMongoCollection<PortalRole> _roles;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public IdentitySeeder(
            IMongoDatabase database,
            IDomainCommandBus commandBus,
            ILogger<IdentitySeeder> logger
        )
        {
            _commandBus = commandBus;

            _users = database.GetCollection<PortalUser>(nameof(PortalUser));
            _roles = database.GetCollection<PortalRole>(nameof(PortalRole));
            _cancellationTokenSource = new CancellationTokenSource();

            _logger = logger;
        }

        public async Task Seed()
        {
            var cancellationToken = _cancellationTokenSource.Token;

            var manageRolesName = IdentityConstants.ManageRoles;
            var manageRolesRole = _roles.Find(r => r.Name == manageRolesName).SingleOrDefault();
            
            var manageUsersName = IdentityConstants.ManageUsers;
            var manageUsersRole = _roles.Find(r => r.Name == manageUsersName).SingleOrDefault();

            const string username = IdentityConstants.AccountCreationUser;
            string password = Guid.NewGuid().ToString();
            var user = _users.Find(u => u.UserName == username).SingleOrDefault();

            if (manageRolesRole != null && manageUsersRole != null && user != null)
            {
                _logger.LogDebug($"Roles and User `{username}` already exist. No seeding will be performed.");
                return;
            }

            _logger.LogInformation($"Roles or User `{username}` do not exist. Creating...");

            var manageRolesRoleId = manageRolesRole?.Id ?? Guid.Empty;
            try
            {
                if (manageRolesRoleId == Guid.Empty)
                {
                    var command = CreateEntityCommandBuilder.For<CreateRoleCommand>()
                        .IssuedBy(Guid.Empty)
                        .With(cmd => cmd.RoleName, manageRolesName)
                        .Build();

                    manageRolesRoleId = _commandBus.ExecuteCommand(command, ValidateCreateWasSuccessful, cancellationToken).Result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to create role `{manageRolesName}`.");
            }

            var manageUsersRoleId = manageUsersRole?.Id ?? Guid.Empty;
            try
            {
                if (manageUsersRoleId == Guid.Empty)
                {
                    var command = CreateEntityCommandBuilder.For<CreateRoleCommand>()
                        .IssuedBy(Guid.Empty)
                        .With(cmd => cmd.RoleName, manageUsersName)
                        .Build();

                    manageUsersRoleId = _commandBus.ExecuteCommand(command, ValidateCreateWasSuccessful, cancellationToken).Result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to create role `{manageUsersName}`.");
            }

            if (manageRolesRoleId == Guid.Empty || manageUsersRoleId == Guid.Empty)
            {
                throw new Exception($"Either or both of the `{manageRolesName}` and `{manageUsersName}` roles could not be created. Seeding cannot continue.");
            }

            var userId = user?.Id ?? Guid.Empty;
            try
            {
                if (userId == Guid.Empty)
                {
                    var command = CreateEntityCommandBuilder.For<CreateUserCommand>()
                        .IssuedBy(Guid.Empty)
                        .With(cmd => cmd.Username, username)
                        .With(cmd => cmd.Password, password)
                        .Build();

                    userId = _commandBus.ExecuteCommand(command, ValidateCreateWasSuccessful, cancellationToken).Result;
                }
            }
            catch (Exception ex)
            {
                var message = $"Unable to create user `{username}`. Seeding cannot continue.";
                _logger.LogError(ex, message);
                throw new Exception(message, ex);
            }

            try
            {
                var assignManageRolesCommand = ChangeEntityCommandBuilder.For<AssignRoleToUserCommand>(manageRolesRoleId)
                    .IssuedBy(UserStore.UserIdForUserCreation)
                    .With(cmd => cmd.UserId, userId)
                    .Build();

                await _commandBus.ExecuteCommand(assignManageRolesCommand, cancellationToken);
                
                var assignManageUsersCommand = ChangeEntityCommandBuilder.For<AssignRoleToUserCommand>(manageUsersRoleId)
                    .IssuedBy(UserStore.UserIdForUserCreation)
                    .With(cmd => cmd.UserId, userId)
                    .Build();

                await _commandBus.ExecuteCommand(assignManageUsersCommand, cancellationToken);
            }
            catch (Exception ex)
            {
                var message = $"Unable to assign either or both of `{manageRolesName}` or `{manageUsersName}` to user `{username}`. Seeding was not successful.";
                _logger.LogError(ex, message);
                throw new Exception(message, ex);
            }
        }

        private Guid ValidateCreateWasSuccessful(DomainCommandResponse response, CancellationToken cancellationToken)
        {
            if (!(response is DomainCreateCommandResponse createResponse))
            {
                throw response.ToException() ?? new Exception("Received an unexpected response");
            }

            return createResponse.CreatedEntityId;
        }
    }
}