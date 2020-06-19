using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AzureChallenge.UI.Models;
using AzureChallenge.Interfaces.Providers.Aggregates;
using AzureChallenge.Models;
using AzureChallenge.Models.Aggregates;
using AzureChallenge.UI.Models.HomeViewModels;
using System.ServiceModel.Security;
using Microsoft.AspNetCore.Identity;
using AzureChallenge.UI.Areas.Identity.Data;
using AzureChallenge.Interfaces.Providers.Users;
using AzureChallenge.Models.Users;
using Microsoft.AspNetCore.Diagnostics;

namespace AzureChallenge.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AzureChallengeUIUser> _userManager;
        private readonly IAggregateProvider<AzureChallengeResult, Aggregate> aggregateProvider;
        private readonly IUserChallengesProvider<AzureChallengeResult, UserChallenges> userChallengesProvider;

        public HomeController(ILogger<HomeController> logger,
                              IAggregateProvider<AzureChallengeResult, Aggregate> aggregateProvider,
                              IUserChallengesProvider<AzureChallengeResult, UserChallenges> userChallengesProvider,
                              UserManager<AzureChallengeUIUser> userManager)
        {
            this.aggregateProvider = aggregateProvider;
            this.userChallengesProvider = userChallengesProvider;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var model = new IndexViewModel { AvailableChallenges = 0, UnfinishedChallenges = 0 };

            throw new Exception("Test");

            var user = await _userManager.GetUserAsync(User);
            var aggregateResponse = await aggregateProvider.GetItemAsync("00000000-0000-0000-0000-000000000000");

            if (aggregateResponse.Item1.Success)
            {
                if (aggregateResponse.Item2 != null)
                {
                    model.AvailableChallenges = aggregateResponse.Item2.ChallengeTotals.TotalPublic;
                }
            }
            if (user != null)
            {
                var userChallengesResponse = await userChallengesProvider.GetItemAsync(user.Id);
                if (userChallengesResponse.Item1.Success)
                {
                    if (userChallengesResponse.Item2 != null)
                    {
                        model.UnfinishedChallenges = userChallengesResponse.Item2.Challenges.Where(c => !c.Completed).Count();
                    }
                }
            }

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if(exceptionHandlerPathFeature != null)
            {
                _logger.LogError(exceptionHandlerPathFeature.Error.ToString());
            }
            
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
