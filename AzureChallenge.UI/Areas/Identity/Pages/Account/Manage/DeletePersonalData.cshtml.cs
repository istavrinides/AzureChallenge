using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AzureChallenge.Interfaces.Providers.Users;
using AzureChallenge.Models;
using AzureChallenge.Models.Users;
using AzureChallenge.UI.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace AzureChallenge.UI.Areas.Identity.Pages.Account.Manage
{
    public class DeletePersonalDataModel : PageModel
    {
        private readonly UserManager<AzureChallengeUIUser> _userManager;
        private readonly SignInManager<AzureChallengeUIUser> _signInManager;
        private readonly ILogger<DeletePersonalDataModel> _logger;
        private readonly IUserChallengesProvider<AzureChallengeResult, UserChallenges> userChallengesProvider;

        public DeletePersonalDataModel(
            UserManager<AzureChallengeUIUser> userManager,
            SignInManager<AzureChallengeUIUser> signInManager,
            IUserChallengesProvider<AzureChallengeResult, UserChallenges> userChallengesProvider,
            ILogger<DeletePersonalDataModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            this.userChallengesProvider = userChallengesProvider;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public bool RequirePassword { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            if (RequirePassword)
            {
                if (!await _userManager.CheckPasswordAsync(user, Input.Password))
                {
                    ModelState.AddModelError(string.Empty, "Incorrect password.");
                    return Page();
                }
            }

            var result = await _userManager.DeleteAsync(user);
            var userId = await _userManager.GetUserIdAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred deleting user with ID '{userId}'.");
            }

            await userChallengesProvider.DeleteItemAsync(userId);

            await _signInManager.SignOutAsync();

            _logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);

            return Redirect("~/");
        }
    }
}
