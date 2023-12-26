using System;
using System.Collections.Generic;
using Eventually.Portal.UI.ViewModel;
using Microsoft.AspNetCore.Identity;

namespace Eventually.Portal.UI.Areas.Identity.Data
{
    public class PortalRole : IdentityRole<Guid>, IViewModel
    {
        public override string NormalizedName => Name.ToUpperInvariant();

        public List<Guid> UserIds { get; set; } = new();
        
        public long Version { get; set; }

        public override string ConcurrencyStamp => Version.ToString();
    }
}