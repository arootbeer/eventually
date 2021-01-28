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
    [Authorize(Roles = IdentityConstants.ManageUsers)]
    public class ManageUsersModel : PageModel
    {
        private readonly UserManager<PortalUser> _userManager;
        private readonly RoleManager<ServerUIRole> _roleManager;
        private readonly IMongoCollection<PortalUser> _users;
        private readonly IMongoCollection<ServerUIRole> _roles;

        public ManageUsersModel(
            IMongoDatabase database,
            UserManager<PortalUser> userManager,
            RoleManager<ServerUIRole> roleManager
            )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _users = database.GetCollection<PortalUser>(nameof(PortalUser));
            _roles = database.GetCollection<ServerUIRole>(nameof(ServerUIRole));
        }

        public bool ShowRemoveButton { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public List<PortalUser> Users { get; set; }

        public Dictionary<string, ServerUIRole> Roles { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Users = _users.Find(user => true).ToList();
            Roles = _roles.Find(role => true).ToEnumerable()
                .ToDictionary(role => $"{role.Id}");
            return Page();
        }

        public async Task<IActionResult> OnPostCreateUserAsync(string username, string password)
        {
            try
            {
                var result = await _userManager.CreateAsync(
                    new PortalUser
                    {
                        UserName = username,
                        Password = password
                    }
                );
                if (result.Succeeded)
                {
                    StatusMessage = "User created successfully";
                }
                else
                {
                    StatusMessage = $"Unable to create user: {string.Join(",", result.Errors.Select(error => error.Description))}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Unable to create user: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveUserRoleAsync(string userId, string roleId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID `{userId}`");
            }

            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return NotFound($"Unable to load role with ID `{roleId}`");
            }

            await _userManager.RemoveFromRoleAsync(user, role.NormalizedName);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAssignUserRoleAsync(string userId, string roleId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID `{userId}`");
            }

            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return NotFound($"Unable to load role with ID `{roleId}`");
            }

            await _userManager.AddToRoleAsync(user, role.NormalizedName);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeactivateUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID `{userId}`");
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                StatusMessage = $"The user was not deactivated: {result}";
                return RedirectToPage();
            }

            StatusMessage = "The user was deactivated";
            return RedirectToPage();
        }
        //public async Task<IActionResult> OnGetLinkLoginCallbackAsync()
        //{
        //    var user = await _userManager.GetUserAsync(User);
        //    if (user == null)
        //    {
        //        return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        //    }

        //    var info = await _signInManager.GetExternalLoginInfoAsync(await _userManager.GetUserIdAsync(user));
        //    if (info == null)
        //    {
        //        throw new InvalidOperationException($"Unexpected error occurred loading external login info for user with ID '{user.Id}'.");
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
