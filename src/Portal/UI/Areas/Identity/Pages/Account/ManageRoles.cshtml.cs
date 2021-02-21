using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eventually.Portal.UI.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;

namespace Eventually.Portal.UI.Areas.Identity.Pages.Account
{
    [Authorize(Roles = IdentityConstants.ManageRoles)]
    public class ManageRolesModel : PageModel
    {
        private readonly UserManager<PortalUser> _userManager;
        private readonly RoleManager<PortalRole> _roleManager;
        private readonly IMongoCollection<PortalUser> _users;
        private readonly IMongoCollection<PortalRole> _roles;

        public ManageRolesModel(
            IMongoDatabase database,
            UserManager<PortalUser> userManager,
            RoleManager<PortalRole> roleManager
            )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _users = database.GetCollection<PortalUser>(nameof(PortalUser));
            _roles = database.GetCollection<PortalRole>(nameof(PortalRole));
        }

        public bool ShowRemoveButton { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public List<PortalUser> Users { get; set; }

        public Dictionary<string, PortalRole> Roles { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Users = _users.Find(user => true).ToList();
            Roles = _roles.Find(role => true).ToEnumerable()
                .ToDictionary(role => role.NormalizedName);
            return Page();
        }

        public async Task<IActionResult> OnPostCreateRoleAsync(string name)
        {
            var result = await _roleManager.FindByNameAsync(name);
            if (result != null)
            {
                StatusMessage = $"A role named `{name}` already exists";
                return RedirectToPage();
            }

            try
            {
                await _roleManager.CreateAsync(new PortalRole {Name = name});
                StatusMessage = $"Role `{name}` was created successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"The role could not be created: {ex}";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRenameRoleAsync(string roleId, string newName)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return NotFound($"Unable to load role with id `{roleId}`");
            }

            await _roleManager.SetRoleNameAsync(role, newName);
            return RedirectToPage();
        }

        //public async Task<IActionResult> OnGetLinkLoginCallbackAsync()
        //{
        //    var user = await _userManager.GetUserAsync(User);
        //    if (user == null)
        //    {
        //        return NotFound($"Unable to load user with ID `{_userManager.GetUserId(User)}`.");
        //    }

        //    var info = await _signInManager.GetExternalLoginInfoAsync(await _userManager.GetUserIdAsync(user));
        //    if (info == null)
        //    {
        //        throw new InvalidOperationException($"Unexpected error occurred loading external login info for user with ID `{user.Id}`.");
        //    }

        //    var result = await _userManager.AddLoginAsync(user, info);
        //    if (!result.Succeeded)
        //    {
        //        StatusMessage = "The external login was not added. External logins can only be associated with one account.";
        //        return RedirectToPage();
        //    }

        //    // Clear the existing external cookie to ensure a clean login process
        //    await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        //    StatusMessage = "The external login was added.";
        //    return RedirectToPage();
        //}
    }
}
