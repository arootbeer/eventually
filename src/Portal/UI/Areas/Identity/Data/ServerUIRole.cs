using System;
using System.Collections.Generic;
using Eventually.Portal.UI.ViewModel;
using Microsoft.AspNetCore.Identity;

namespace Eventually.Portal.UI.Areas.Identity.Data
{
    public class ServerUIRole : IdentityRole<Guid>, IViewModel
    {
        public override string NormalizedName => Name.ToUpperInvariant();

        public List<Guid> UserIds { get; set; } = new List<Guid>();
        
        public int Version { get; set; }

        public override string ConcurrencyStamp => Version.ToString();
    }
}