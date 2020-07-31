using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureChallenge.UI.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace AzureChallenge.UI.Areas.Identity.Pages.Roles
{
    [Authorize]
    [Authorize(Roles = "Administrator")]
    public class ManageModel : PageModel
    {
        private readonly UserManager<AzureChallengeUIUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ManageModel(
            UserManager<AzureChallengeUIUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public List<UserRole> UserRoles { get; set; }

        public class UserRole
        {
            public string Email { get; set; }
            public string Id { get; set; }
            public string Role { get; set; }
        }

        public async Task OnGetAsync()
        {
            await PopulateUserRoleAssociation();
        }

        public async Task<IActionResult> OnPostRemoveRoleAsync(string userId, string role)
        {
            var userToRemove = await _userManager.FindByIdAsync(userId);
            await _userManager.RemoveFromRoleAsync(userToRemove, role);

            await PopulateUserRoleAssociation();

            return Page();
        }

        public async Task<IActionResult> OnPostAddAsAdminAsync(string userId)
        {
            var userToAdd = await _userManager.FindByIdAsync(userId);
            await _userManager.AddToRoleAsync(userToAdd, "Administrator");

            // Check if the user was a content moderator. If yes, remove
            if (await _userManager.IsInRoleAsync(userToAdd, "ContentEditor"))
                await _userManager.RemoveFromRoleAsync(userToAdd, "ContentEditor");

            await PopulateUserRoleAssociation();

            return Page();
        }

        public async Task<IActionResult> OnPostAddAsContentAsync(string userId)
        {
            var userToAdd = await _userManager.FindByIdAsync(userId);
            await _userManager.AddToRoleAsync(userToAdd, "ContentEditor");

            // Check if the user was an administrator. If yes, remove
            if (await _userManager.IsInRoleAsync(userToAdd, "Administrator"))
                await _userManager.RemoveFromRoleAsync(userToAdd, "Administrator");

            await PopulateUserRoleAssociation();

            return Page();
        }

        private async Task PopulateUserRoleAssociation()
        {
            UserRoles = new List<UserRole>();

            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            foreach (var u in await _userManager.Users.ToListAsync())
            {
                if (u.Id != currentUser.Id)
                {
                    var roles = await _userManager.GetRolesAsync(u);

                    UserRoles.Add(new UserRole()
                    {
                        Email = u.Email,
                        Role = roles.Count == 0 ? "User" : roles[0],
                        Id = u.Id
                    });
                }
            }
        }
    }
}
