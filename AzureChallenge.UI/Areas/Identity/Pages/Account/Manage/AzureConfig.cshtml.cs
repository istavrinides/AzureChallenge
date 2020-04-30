using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AzureChallenge.UI.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AzureChallenge.UI.Areas.Identity.Pages.Account.Manage
{
    public class AzureConfigModel : PageModel
    {
        private readonly UserManager<AzureChallengeUIUser> _userManager;
        private readonly SignInManager<AzureChallengeUIUser> _signInManager;

        public AzureConfigModel(
            UserManager<AzureChallengeUIUser> userManager,
            SignInManager<AzureChallengeUIUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }
        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Subscription Id")]
            public string SubscriptionId { get; set; }

            [Required]
            [Display(Name = "Client Id")]
            public string ClientId { get; set; }

            [Required]
            [Display(Name = "Client Secret")]
            public string ClientSecret { get; set; }

            [Required]
            [Display(Name = "Tenant Id")]
            public string TenantId { get; set; }

            [Display(Name = "Subscription Name")]
            public string SubscriptionName { get; set; }
        }

        private async Task LoadAsync(AzureChallengeUIUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                SubscriptionId = user.SubscriptionId,
                ClientId = user.ClientId,
                ClientSecret = user.ClientSecret,
                SubscriptionName = user.SubscriptionName,
                TenantId = user.TenantId
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            user.SubscriptionId = Input.SubscriptionId;
            //user.SubscriptionName 
            user.ClientId = Input.ClientId;
            user.ClientSecret = Input.ClientSecret;
            user.TenantId = Input.TenantId;

            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);
            
            StatusMessage = "Your profile has been updated";

            return RedirectToPage();
        }
    }
}
