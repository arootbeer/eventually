using System;
using Eventually.Domain;
using Eventually.Domain.EventBuilders;
using Eventually.Portal.Domain.IAAA.Commands.Roles;
using Eventually.Portal.Domain.IAAA.Events.Roles;
using Eventually.Utilities.Collections;

namespace Eventually.Portal.Domain.IAAA.Roles
{
    public class Role : AggregateBase<Role, CreateRoleCommand, IRoleEvent>
    {
        public static ConcurrentHashSet<string> ExistingRoleNames { get; set; }

        private string _name;
        private readonly ConcurrentHashSet<Guid> _assignedUsers = new();

        private Role(Guid id) : base(id) { }

        protected override void InitializeFromCreateCommand(CreateRoleCommand command)
        {
            Raise(
                EntityCreatedEventBuilder<RoleCreated>
                    .For(this)
                    .From(command)
                    .With(rc => rc.RoleName, command.RoleName)
                    .Build()
                );
        }

        public void Apply(RoleCreated rc)
        {
            _name = rc.RoleName;
        }

        public void ChangeRoleName(ChangeRoleNameCommand command)
        {
            Raise(
                EntityChangedEventBuilder<RoleNameChanged>
                    .For(this)
                    .From(command)
                    .With(rc => rc.RoleName, command.RoleName)
                    .Build()
            );
        }

        public void Apply(RoleNameChanged rnc)
        {
            _name = rnc.RoleName;
        }

        public void AddUser(AssignRoleToUserCommand command)
        {
            if (_assignedUsers.Contains(command.UserId))
            {
                throw new Exception($"The user with id `{command.UserId}` already has role `{_name}`.");
            }

            Raise(
                EntityChangedEventBuilder<RoleAssignedToUser>
                    .For(this)
                    .From(command)
                    .With(ratu => ratu.UserId, command.UserId)
                    .Build()
            );
        }

        public void Apply(RoleAssignedToUser ratu)
        {
            _assignedUsers.Add(ratu.UserId);
        }

        public void RemoveUser(RemoveRoleFromUserCommand command)
        {
            if (!_assignedUsers.Contains(command.UserId))
            {
                throw new Exception($"The user with id `{command.UserId}` does not have role `{_name}`.");
            }

            Raise(
                EntityChangedEventBuilder<RoleRemovedFromUser>
                    .For(this)
                    .From(command)
                    .With(uatr => uatr.UserId, command.UserId)
                    .Build()
            );
        }

        public void Apply(RoleRemovedFromUser rrfu)
        {
            _assignedUsers.Remove(rrfu.UserId);
        }
    }
}