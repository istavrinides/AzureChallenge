using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AzureChallenge.Interfaces.Providers.Aggregates;
using AzureChallenge.Interfaces.Providers.Challenges;
using AzureChallenge.Interfaces.Providers.Parameters;
using AzureChallenge.Interfaces.Providers.Questions;
using AzureChallenge.Interfaces.Providers.Users;
using AzureChallenge.Models;
using AzureChallenge.Models.Challenges;
using AzureChallenge.Models.Parameters;
using AzureChallenge.Models.Questions;
using AzureChallenge.Models.Users;
using AzureChallenge.UI.Areas.Identity.Data;
using AzureChallenge.UI.Models.ChallengeViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AzureChallenge.UI.Controllers
{
    [Authorize]
    public class ChallengeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AzureChallengeUIUser> _userManager;
        private readonly IUserChallengesProvider<AzureChallengeResult, UserChallenges> userChallengesProvider;
        private readonly IAssignedQuestionProvider<AzureChallengeResult, AssignedQuestion> assignedQuestionProvider;
        private readonly IChallengeProvider<AzureChallengeResult, ChallengeDetails> challengesProvider;
        private readonly IParameterProvider<AzureChallengeResult, GlobalChallengeParameters> parametersProvider;
        private readonly IMapper mapper;

        public ChallengeController(ILogger<HomeController> logger,
                              IUserChallengesProvider<AzureChallengeResult, UserChallenges> userChallengesProvider,
                              IAssignedQuestionProvider<AzureChallengeResult, AssignedQuestion> assignedQuestionProvider,
                              IChallengeProvider<AzureChallengeResult, ChallengeDetails> challengesProvider,
                              IParameterProvider<AzureChallengeResult, GlobalChallengeParameters> parametersProvider,
                              IMapper mapper,
                              UserManager<AzureChallengeUIUser> userManager)
        {
            this.userChallengesProvider = userChallengesProvider;
            this.assignedQuestionProvider = assignedQuestionProvider;
            this.challengesProvider = challengesProvider;
            this.parametersProvider = parametersProvider;
            this.mapper = mapper;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var model = new IndexViewModel { Challenges = new List<Challenge>() };

            var challengesResponse = await challengesProvider.GetAllItemsAsync();

            if (challengesResponse.Item1.Success)
            {
                model.Challenges = challengesResponse.Item2
                                    .Select(c => new Models.ChallengeViewModels.Challenge
                                    {
                                        Description = c.Description,
                                        Id = c.Id,
                                        Name = c.Name,
                                        CurrentQuestionId = c.Questions.Where(q => q.Index == 0).FirstOrDefault()?.Id,
                                        IsComplete = false,
                                        IsUnderway = false
                                    })
                                    .ToList();
            }

            var user = await _userManager.GetUserAsync(User);
            var userChallengesResponse = await userChallengesProvider.GetItemAsync(user.Id);

            if (userChallengesResponse.Item1.Success)
            {
                if (userChallengesResponse.Item2 != null)
                {
                    foreach (var challenge in userChallengesResponse.Item2.Challenges)
                    {
                        // If a user is enrolled in a challenge, check if not finished and set the continuation question
                        if (!challenge.Completed)
                        {
                            var challengeInModel = model.Challenges.Where(p => p.Id == challenge.ChallengeId).FirstOrDefault();

                            if (challengeInModel != null)
                            {
                                challengeInModel.CurrentQuestionId = challenge.CurrentQuestion;
                                challengeInModel.IsUnderway = true;
                            }
                        }
                        else
                        {
                            var challengeInModel = model.Challenges.Where(p => p.Id == challenge.ChallengeId).FirstOrDefault();

                            if (challengeInModel != null)
                            {
                                challengeInModel.IsComplete = true;
                            }
                        }
                    }
                }
            }

            return View(model);
        }

        [Route("Challenge/{challengeId}/Question/{questionId}")]
        public async Task<IActionResult> ShowQuestion(string challengeId, string questionId)
        {
            var model = new QuestionViewModel();
            var user = await _userManager.GetUserAsync(User);

            // Get the challenge
            var challengeResponse = await challengesProvider.GetItemAsync(challengeId);
            // Get the associated question
            var assignedQuestionRespnose = await assignedQuestionProvider.GetItemAsync(questionId);
            // Get the global challenge parameters
            var globalChallengeParametersResponse = await parametersProvider.GetItemAsync(challengeId);
            // Get the profile parameters
            var profileParameters = mapper.Map<AzureChallenge.Models.Profile.UserProfile>(user).GetKeyValuePairs().ToDictionary(p => p.Key, p => p.Value);

            var parameters = new Dictionary<string, string>();
            foreach (var gp in globalChallengeParametersResponse.Item2.Parameters)
            {
                parameters.Add($"Global_{gp.Key}", gp.Value);
            }
            parameters = parameters.Concat(profileParameters).ToDictionary(p => p.Key, p => p.Value);


            if (assignedQuestionRespnose.Item1.Success)
            {
                // Find the question id
                var question = assignedQuestionRespnose.Item2;

                if (question == null) return StatusCode(500);

                parameters = parameters.Concat(question.TextParameters.Where(p => !p.Key.StartsWith("Profile.") && !p.Key.StartsWith("Global.")).ToDictionary(p => p.Key, p => p.Value)).ToDictionary(p => p.Key, p => p.Value);
                var challengeQuestion = challengeResponse.Item2.Questions.Where(q => q.Id == questionId).FirstOrDefault();

                model.Difficulty = question.Difficulty;
                model.QuestionIndex = challengeQuestion.Index;
                model.HelpfulLinks = question.UsefulLinks;
                model.Justification = question.Justification;
                model.NextQuestionId = challengeQuestion.NextQuestionId;
                model.QuestionId = questionId;
                model.QuestionText = SmartFormat.Smart.Format(question.Text.Replace("{Global.", "{Global_").Replace("{Profile.", "{Profile_"), parameters);
                model.ChallengeId = challengeId;
            }
            else
            {
                return StatusCode(404);
            }

            return View(model);
        }
    }
}
