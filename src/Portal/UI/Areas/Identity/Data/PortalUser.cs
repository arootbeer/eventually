using System;
using System.Collections.Generic;
using Eventually.Portal.UI.ViewModel;
using Microsoft.AspNetCore.Identity;

namespace Eventually.Portal.UI.Areas.Identity.Data
{
    // Add profile data for application users by adding properties to the ServerUIUser class
    public class PortalUser : IdentityUser<Guid>, IViewModel
    {
        public string Password { get; set; }  //populated only when this class is used for user creation
        
        public bool Active { get; set; }

        public override string NormalizedUserName => UserName.ToUpperInvariant();

        public override string NormalizedEmail => Email?.ToUpperInvariant();

        public Dictionary<string, string> Roles { get; } = new Dictionary<string, string>();

        public int Version { get; set; }

        public override string ConcurrencyStamp => Version.ToString();
    }
}
