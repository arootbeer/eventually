using System.ComponentModel.Composition;

namespace Eventually.Portal.Domain
{
    [Export(typeof(Eventually.Interfaces.Common.MessageTypeLookupStrategy))]
    public class MessageTypeLookupStrategy : Eventually.Interfaces.Common.MessageTypeLookupStrategy { }
}