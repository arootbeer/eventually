using System;

namespace Eventually.Interfaces.DomainCommands
{
    public class DomainCommandResponseError
    {
        private DomainCommandResponseError() { }

        private DomainCommandResponseError(string reason)
        {
            Reason = reason;
        }

        public string Reason { get; }

        public static DomainCommandResponseError From(Exception exception)
        {
            return new DomainCommandResponseError(exception.ToString());
        }
    }
}