using System;

namespace Eventually.Portal.UI.Areas.Identity.EventHandlers.IAAA.Users
{
    public class PortalUserLogin
    {
        public string LoginProvider { get; set; }
        
        public string LoginHash { get; set; }
        
        public string LoginToken { get; set; }
        
        public Guid UserId { get; set; }
    }
}