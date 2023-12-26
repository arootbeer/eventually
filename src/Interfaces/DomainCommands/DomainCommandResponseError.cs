using System;

namespace Eventually.Interfaces.DomainCommands
{
    public class DomainCommandResponseError
    {
        private DomainCommandResponseError(string reason)
        {
            Reason = reason;
        }

        public string Reason { get; }
        
        public Exception Exception { get; private init; }

        public static DomainCommandResponseError For(string reason)
        {
            return new DomainCommandResponseError(reason);
        }
        
        public static DomainCommandResponseError From(Exception exception)
        {
            return new DomainCommandResponseError(exception.Message)
            {
                Exception = exception
            };
        }
    }
}