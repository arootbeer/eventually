﻿using Eventually.Interfaces.DomainCommands;

namespace Eventually.Portal.Domain.IAAA.Commands.Users
{
    public record ChangePasswordCommand : ChangeEntityCommand
    {
        public string Password { get; init; }
        
        // TODO: De-naive-ify
        // https://stackoverflow.com/questions/66015437
        public override string ToString()
        {
            return base.ToString()
                .Replace(Password, "********");
        }
    }
}