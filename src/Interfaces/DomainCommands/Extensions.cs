using System;
using System.Linq;

namespace Eventually.Interfaces.DomainCommands
{
    public static class Extensions
    {
        public static Exception ToException(this DomainCommandResponse response)
        {
            if (response.Succeeded)
            {
                return null;
            }

            var errors = response.Errors.ToList();
            return errors.Count switch
            {
                0 => new Exception("The command did not complete successfully, but no further information was provided."),
                1 => new Exception(errors[0].Reason),
                _ => new AggregateException("The command did not complete successfully", errors.Select(error => new Exception(error.Reason)))
            };
        }
    }
}