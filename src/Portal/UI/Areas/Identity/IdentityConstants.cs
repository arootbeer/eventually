namespace Eventually.Portal.UI.Areas.Identity
{
    /// <summary>
    /// Because ASP.Net Identity uses strings for role identification, we need to treat certain string values specially
    /// </summary>
    public class IdentityConstants : Microsoft.AspNetCore.Identity.IdentityConstants
    {
        public const string AccountCreationUser = "AccountCreationUser";
        
        public const string ManageUsers = nameof(ManageUsers);
        public const string ManageRoles = nameof(ManageRoles);
    }
}