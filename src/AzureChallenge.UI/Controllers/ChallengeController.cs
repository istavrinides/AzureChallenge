﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Configuration.Conventions;
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
using ACMA = AzureChallenge.Models.Aggregates;
using AzureChallenge.Providers;
using AzureChallenge.UI.Areas.Identity.Data;
using AzureChallenge.UI.Models.ChallengeViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Spatial;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Web.CodeGeneration;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.AspNetCore.Http;

namespace AzureChallenge.UI.Controllers
{
    [Authorize]
    public class ChallengeController : Controller
    {
        private readonly ILogger<ChallengeController> _logger;
        private readonly UserManager<AzureChallengeUIUser> _userManager;
        private readonly IUserChallengesProvider<AzureChallengeResult, UserChallenges> userChallengesProvider;
        private readonly IAssignedQuestionProvider<AzureChallengeResult, AssignedQuestion> assignedQuestionProvider;
        private readonly IChallengeProvider<AzureChallengeResult, ChallengeDetails> challengesProvider;
        private readonly IParameterProvider<AzureChallengeResult, GlobalChallengeParameters> parametersProvider;
        private readonly IAggregateProvider<AzureChallengeResult, ACMA.Aggregate> aggregateProvider;
        private readonly IMapper mapper;

        public ChallengeController(ILogger<ChallengeController> logger,
                              IUserChallengesProvider<AzureChallengeResult, UserChallenges> userChallengesProvider,
                              IAssignedQuestionProvider<AzureChallengeResult, AssignedQuestion> assignedQuestionProvider,
                              IChallengeProvider<AzureChallengeResult, ChallengeDetails> challengesProvider,
                              IParameterProvider<AzureChallengeResult, GlobalChallengeParameters> parametersProvider,
                              IAggregateProvider<AzureChallengeResult, ACMA.Aggregate> aggregateProvider,
                              IMapper mapper,
                              UserManager<AzureChallengeUIUser> userManager)
        {
            this.userChallengesProvider = userChallengesProvider;
            this.assignedQuestionProvider = assignedQuestionProvider;
            this.challengesProvider = challengesProvider;
            this.parametersProvider = parametersProvider;
            this.aggregateProvider = aggregateProvider;
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
                                    .Where(c => c.IsPublic)
                                    .Select(c => new Models.ChallengeViewModels.Challenge
                                    {
                                        Description = c.Description,
                                        Id = c.Id,
                                        Name = c.Name,
                                        CurrentQuestionId = c.Questions.Where(q => q.Index == 0).FirstOrDefault()?.Id,
                                        IsComplete = false,
                                        IsUnderway = false,
                                        AzureCategory = c.AzureServiceCategory,
                                        WelcomeMessage = c.WelcomeMessage,
                                        PrereqLinks = c.PrereqLinks,
                                        TrackAndDeductPoints = c.TrackAndDeductPoints
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

        [Route("MyChallenges")]
        public async Task<IActionResult> UserChallenges()
        {
            var model = new IndexViewModel { Challenges = new List<Challenge>() };

            var user = await _userManager.GetUserAsync(User);
            var challengesResponse = await challengesProvider.GetAllItemsAsync();
            var userChallengesResponse = await userChallengesProvider.GetItemAsync(user.Id);

            if (challengesResponse.Item1.Success && userChallengesResponse.Item1.Success)
            {
                var challenges = challengesResponse.Item2;

                model.Challenges = userChallengesResponse.Item2.Challenges
                                    .Select(c => new Models.ChallengeViewModels.Challenge
                                    {
                                        Description = challenges.Where(p => p.Id == c.ChallengeId).FirstOrDefault().Description,
                                        Id = c.ChallengeId,
                                        Name = challenges.Where(p => p.Id == c.ChallengeId).FirstOrDefault().Name,
                                        CurrentQuestionId = c.CurrentQuestion,
                                        IsComplete = c.Completed,
                                        IsUnderway = !c.Completed,
                                        CurrentQuestionIndex = c.CurrentIndex,
                                        TotalQuestions = challenges.Where(p => p.Id == c.ChallengeId).FirstOrDefault().Questions.Count()
                                    })
                                    .ToList();
            }

            return View(model);
        }

        [HttpGet]
        [Route("Challenge/Join")]
        public IActionResult JoinPrivateChallenge()
        {
            var model = new JoinPrivateChallengeViewModel();

            return View(model);
        }

        [HttpPost]
        [Route("Challenge/Join")]
        public async Task<IActionResult> JoinPrivateChallenge(JoinPrivateChallengeViewModel inputModel)
        {
            if (ModelState.IsValid)
            {
                var challengeResponse = await challengesProvider.GetItemAsync(inputModel.ChallengeId);
                var aggregateReponse = await aggregateProvider.GetItemAsync(inputModel.ChallengeId);

                // Check if the challenge has questions
                if (challengeResponse.Item2.Questions.Count > 0)
                {
                    // Get the id of the first
                    var firstQuestion = challengeResponse.Item2.Questions.Where(q => q.Index == 0).FirstOrDefault();

                    return RedirectToAction("StartChallenge", new { challengeId = challengeResponse.Item2.Id, questionId = firstQuestion.Id });
                }
                else
                {
                    return StatusCode(404);
                }
            }

            return View(inputModel);
        }


        [Route("Challenge/{challengeId}/Start/{questionId}")]
        public async Task<IActionResult> StartChallenge(string challengeId, string questionId)
        {
            var user = await _userManager.GetUserAsync(User);
            var userChallengeResponse = await userChallengesProvider.GetItemAsync(user.Id);
            var challengeResponse = await challengesProvider.GetItemAsync(challengeId);
            var aggregateReponse = await aggregateProvider.GetItemAsync(challengeId);

            if (!userChallengeResponse.Item1.IsError)
            {
                // If the challenge is not locked, lock it since the first user has started it
                if (!challengeResponse.Item2.IsLocked)
                {
                    challengeResponse.Item2.IsLocked = true;
                    await challengesProvider.AddItemAsync(challengeResponse.Item2);
                }

                // If this is a new challenge, we first need to show the introduction
                var showIntroduction = false;

                // New tournament
                if (!userChallengeResponse.Item1.Success)
                {
                    var userChallenge = new UserChallenges { Id = user.Id, Challenges = new List<UserChallengeItem>() };
                    userChallenge.Challenges
                        .Add(new UserChallengeItem()
                        {
                            AccumulatedXP = 0,
                            ChallengeId = challengeId,
                            Completed = false,
                            CurrentQuestion = questionId,
                            StartTimeUTC = DateTime.Now.ToUniversalTime(),
                            CurrentIndex = 0,
                            NumOfQuestions = challengeResponse.Item2.Questions.Count(),
                            NumOfEfforts = 0
                        });

                    await userChallengesProvider.AddItemAsync(userChallenge);

                    if (aggregateReponse.Item1.Success)
                    {
                        var agg =
                           aggregateReponse.Item2 ??
                           new ACMA.Aggregate()
                           {
                               Id = challengeId,
                               ChallengeUsers = new ACMA.ChallengeAggregateUsers() { Finished = 0, Started = 0 }
                           };

                        agg.ChallengeUsers.Started += 1;
                        await aggregateProvider.AddItemAsync(agg);
                    }
                    else
                    {
                        // Doesn't exist
                        var agg = new ACMA.Aggregate()
                        {
                            Id = challengeId,
                            ChallengeUsers = new ACMA.ChallengeAggregateUsers() { Finished = 0, Started = 0 }
                        };

                        agg.ChallengeUsers.Started += 1;
                        await aggregateProvider.AddItemAsync(agg);
                    }

                    showIntroduction = true;
                }
                else
                {
                    // The user already has done some challenges
                    // If this is new, then add the userChallenge and update the Aggregate
                    if (!userChallengeResponse.Item2.Challenges.Any(p => p.ChallengeId == challengeId))
                    {
                        userChallengeResponse.Item2.Challenges
                           .Add(new UserChallengeItem()
                           {
                               AccumulatedXP = 0,
                               ChallengeId = challengeId,
                               Completed = false,
                               CurrentQuestion = questionId,
                               StartTimeUTC = DateTime.Now.ToUniversalTime(),
                               CurrentIndex = 0,
                               NumOfQuestions = challengeResponse.Item2.Questions.Count(),
                               NumOfEfforts = 0
                           });
                        await userChallengesProvider.AddItemAsync(userChallengeResponse.Item2);

                        if (aggregateReponse.Item1.Success)
                        {
                            var agg =
                               aggregateReponse.Item2 ??
                               new ACMA.Aggregate()
                               {
                                   Id = challengeId,
                                   ChallengeUsers = new ACMA.ChallengeAggregateUsers() { Finished = 0, Started = 0 }
                               };

                            agg.ChallengeUsers.Started += 1;
                            await aggregateProvider.AddItemAsync(agg);
                        }
                        else
                        {
                            // Doesn't exist
                            var agg = new ACMA.Aggregate()
                            {
                                Id = challengeId,
                                ChallengeUsers = new ACMA.ChallengeAggregateUsers() { Finished = 0, Started = 0 }
                            };

                            agg.ChallengeUsers.Started += 1;
                            await aggregateProvider.AddItemAsync(agg);
                        }

                        showIntroduction = true;
                    }
                    else if (userChallengeResponse.Item2.Challenges.Where(p => p.ChallengeId == challengeId).FirstOrDefault()?.CurrentIndex == 0)
                    {
                        // If the user already registered for the challenge but never started it
                        showIntroduction = true;
                    }
                }

                if (showIntroduction)
                    return RedirectToAction("Introduction", new { challengeId = challengeId, questionId = questionId });
                else
                    return RedirectToAction("ShowQuestion", new { challengeId = challengeId, questionId = questionId });
            }


            return StatusCode(500);
        }

        [Route("Challenge/{challengeId}/Introduction")]
        public async Task<IActionResult> Introduction(string challengeId, string questionId)
        {
            if (string.IsNullOrEmpty(challengeId) || string.IsNullOrEmpty(questionId))
                return View("Index");

            var challenge = await challengesProvider.GetItemAsync(challengeId);

            var model = new IntroductionViewModel()
            {
                Id = challenge.Item2.Id,
                Duration = challenge.Item2.Duration,
                Name = challenge.Item2.Name,
                PrereqLinks = challenge.Item2.PrereqLinks,
                WelcomeMessage = challenge.Item2.WelcomeMessage,
                FirstQuestion = questionId
            };

            return View(model);
        }

        [Route("Challenge/{challengeId}/Question/{questionId}")]
        public async Task<IActionResult> ShowQuestion(string challengeId, string questionId)
        {
            var model = new QuestionViewModel() { Choices = new List<(string Text, bool Value, bool Selected)>() };
            var user = await _userManager.GetUserAsync(User);

            var userChallengeResponse = await userChallengesProvider.GetItemAsync(user.Id);
            // Get the challenge
            var challengeResponse = await challengesProvider.GetItemAsync(challengeId);

            // If the challenge is not registered for the user (never registered)
            if (!userChallengeResponse.Item1.Success) return RedirectToAction("Index");

            var userChallenge = userChallengeResponse.Item2.Challenges.Where(p => p.ChallengeId == challengeId).FirstOrDefault();

            // If the user is not registered for this challenge
            if (userChallenge == null) return RedirectToAction("Index");
            else if (userChallenge.CurrentQuestion != questionId && challengeResponse.Item2.Questions.Where(p => p.Id == questionId).Select(p => p.Index).FirstOrDefault() > userChallenge.CurrentIndex)
            {
                // Redirect the user to the current question (don't allow skipping)
                return RedirectToAction("ShowQuestion", new { challengeId = challengeId, questionId = userChallenge.CurrentQuestion });
            }
            else if (userChallenge.CurrentIndex == 0)
            {
                var startTime = DateTime.Now.ToUniversalTime();
                userChallenge.StartTimeUTC = startTime;
                // Before doing anything, if the user is on the first question, always reset their start time (in case we are in a timed Challenge)
                userChallengeResponse.Item2.Challenges[userChallengeResponse.Item2.Challenges.IndexOf(userChallenge)].StartTimeUTC = startTime;
                await userChallengesProvider.AddItemAsync(userChallengeResponse.Item2);
            }
            else if (challengeResponse.Item2.Duration > 0 && userChallenge.StartTimeUTC > userChallenge.StartTimeUTC.AddMinutes(challengeResponse.Item2.Duration))
            {
                // If the user exceeded the time limit
                return RedirectToAction("TimeLimitReached", new { challengeId = challengeId });
            }


            // Get the associated question
            var assignedQuestionRespnose = await assignedQuestionProvider.GetItemAsync(questionId);
            // Get the global challenge parameters
            var globalChallengeParametersResponse = await parametersProvider.GetItemAsync(challengeId);
            // Get the profile parameters
            var profileParameters = mapper.Map<AzureChallenge.Models.Profile.UserProfile>(user).GetKeyValuePairs().ToDictionary(p => p.Key, p => p.Value);

            var parameters = new Dictionary<string, string>();
            foreach (var gp in globalChallengeParametersResponse.Item2.Parameters ?? Enumerable.Empty<GlobalChallengeParameters.ParameterDefinition>())
            {
                parameters.Add($"Global_{gp.Key}", gp.Value);
            }
            parameters = parameters.Concat(profileParameters).ToDictionary(p => p.Key, p => p.Value);


            if (assignedQuestionRespnose.Item1.Success)
            {
                // Find the question id
                var question = assignedQuestionRespnose.Item2;

                if (question == null) return StatusCode(500);

                parameters = parameters.Concat(question.TextParameters.Where(p => !p.Key.StartsWith("Profile.") && !p.Key.StartsWith("Global.")).ToDictionary(p => p.Key, p => p.Value == "null" ? "" : p.Value)).ToDictionary(p => p.Key, p => p.Value);
                var challengeQuestion = challengeResponse.Item2.Questions.Where(q => q.Id == questionId).FirstOrDefault();
                var previousChallengeQuestion = challengeResponse.Item2.Questions.Where(q => q.NextQuestionId == questionId).FirstOrDefault();

                model.QuestionType = question.QuestionType;
                model.Difficulty = question.Difficulty;
                model.QuestionIndex = challengeQuestion.Index;
                model.TotalNumOfQuestions = challengeResponse.Item2.Questions.Count();
                model.ThisQuestionDone = userChallenge.CurrentIndex > challengeQuestion.Index;
                model.HelpfulLinks = question.UsefulLinks;
                model.Justification = question.Justification;
                model.PreviousQuestionId = previousChallengeQuestion?.Id;
                model.NextQuestionId = challengeQuestion.NextQuestionId;
                model.QuestionId = questionId;
                model.QuestionText = SmartFormat.Smart.Format(question.Text.Replace("{Global.", "{Global_").Replace("{Profile.", "{Profile_"), parameters);
                model.QuestionName = challengeQuestion.Name;
                model.TournamentName = challengeResponse.Item2.Name;
                model.ChallengeId = challengeId;
                model.TimeLeftInSeconds = challengeResponse.Item2.Duration == 0 ? 0 : challengeResponse.Item2.Duration * 60 - (int)((DateTime.Now.ToUniversalTime() - userChallenge.StartTimeUTC).TotalSeconds);
                if (question.QuestionType == "MultiChoice")
                {
                    model.Choices = question.Answers[0].AnswerParameters.Select(a => (a.Key, bool.Parse(a.Value), false)).ToList();
                }

                if (question.Uris != null && question.Uris.Any(u => u.RequiresContributorAccess))
                {
                    model.ShowWarning = true;
                    model.WarningMessage = "This question requires that the Service Principal you have created has Contributor access to the below Resource(s).";
                }
            }
            else
            {
                return StatusCode(404);
            }

            return View(model);
        }

        [Route("Challenge/{challengeId}/Congratulations")]
        public IActionResult Completed(string challengeId)
        {
            return View();
        }

        [Route("Challenge/{challengeId}/TimeLimitReached")]
        public IActionResult TimeLimitReached(string challengeId)
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ValidateQuestion(ValidateQuestionViewModel inputModel)
        {
            var user = await _userManager.GetUserAsync(User);

            // We need to get the selected choice(s) in case it's a multiple choice question
            var selectedChoices = new List<string>();

            if (!string.IsNullOrWhiteSpace(inputModel.SelectedRBChoice))
            {
                selectedChoices.Add(inputModel.SelectedRBChoice);
            }
            else if (inputModel.Choices?.Count > 0)
            {
                foreach (var choice in inputModel.Choices)
                    selectedChoices.Add(choice);
            }

            var results = await assignedQuestionProvider.ValidateQuestion(inputModel.QuestionId, mapper.Map<AzureChallenge.Models.Profile.UserProfile>(user), selectedChoices);

            if (results.All(p => p.Value))
            {
                // User has successfully responded to the question, so update the database
                var userChallengeResponse = await userChallengesProvider.GetItemAsync(user.Id);

                // Get the current challenge
                var challenge = userChallengeResponse.Item2.Challenges.Where(p => p.ChallengeId == inputModel.ChallengeId).FirstOrDefault();
                // Get the current challenge definition
                var challengeDefinition = await challengesProvider.GetItemAsync(inputModel.ChallengeId);

                if (inputModel.QuestionIndex == challenge.CurrentIndex)
                {
                    // If the challenge has penalties
                    if (challengeDefinition.Item2.TrackAndDeductPoints)
                    {
                        // User can only get (1 - NumOfEfforts * 0.25) * Difficult points
                        var pointsToAdd = (int)((1 - challenge.NumOfEfforts * 0.25) * inputModel.Difficulty);
                        user.AccumulatedPoint += pointsToAdd;
                        challenge.AccumulatedXP += pointsToAdd;
                    }
                    else
                    {
                        user.AccumulatedPoint += inputModel.Difficulty;
                        challenge.AccumulatedXP += inputModel.Difficulty;
                    }

                    challenge.CurrentIndex += 1;
                    
                    // Reset number of efforts
                    challenge.NumOfEfforts = 0;

                    // End of challenge
                    if (inputModel.NextQuestionId == null)
                    {
                        var aggregateReponse = await aggregateProvider.GetItemAsync(inputModel.ChallengeId);

                        aggregateReponse.Item2.ChallengeUsers.Finished += 1;
                        aggregateReponse.Item2.ChallengeUsers.Started -= 1;
                        await aggregateProvider.AddItemAsync(aggregateReponse.Item2);

                        challenge.Completed = true;
                        challenge.endTimeUTC = DateTime.Now.ToUniversalTime();
                    }
                    else
                    {
                        challenge.CurrentQuestion = inputModel.NextQuestionId;
                    }

                    await userChallengesProvider.AddItemAsync(userChallengeResponse.Item2);
                    await _userManager.UpdateAsync(user);
                }
            }
            else
            {
                // User hasn't responded correctly
                // Check to see if the challenge penalizes incorrect answers
                var challengeDefinition = await challengesProvider.GetItemAsync(inputModel.ChallengeId);

                if (!challengeDefinition.Item1.IsError && challengeDefinition.Item2.TrackAndDeductPoints)
                {
                    // Challenge penalizes, so we need to update the user record
                    var userChallengeResponse = await userChallengesProvider.GetItemAsync(user.Id);
                    // Get the current challenge
                    var challenge = userChallengeResponse.Item2.Challenges.Where(p => p.ChallengeId == inputModel.ChallengeId).FirstOrDefault();

                    // Check if the number of efforts is less than 5
                    if (challenge.NumOfEfforts < 5)
                    {
                        // Increase the number of efforts
                        challenge.NumOfEfforts += 1;

                        // Update the record
                        await userChallengesProvider.AddItemAsync(userChallengeResponse.Item2);
                    }
                }

            }

            return Ok(results);
        }
    }
}
