using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AzureChallenge.Interfaces.Providers.Questions;
using AzureChallenge.Interfaces.Providers.Tournaments;
using AzureChallenge.Interfaces.Providers.Parameters;
using VM = AzureChallenge.UI.Areas.Administration.Models.Tournaments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ACM = AzureChallenge.Models;
using ACMT = AzureChallenge.Models.Tournaments;
using ACMQ = AzureChallenge.Models.Questions;
using ACMP = AzureChallenge.Models.Parameters;
using Microsoft.CodeAnalysis.Differencing;
using AzureChallenge.UI.Areas.Administration.Models.Tournaments;
using Microsoft.AspNetCore.Identity;
using AzureChallenge.UI.Areas.Identity.Data;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Azure.Cosmos.Linq;

namespace AzureChallenge.UI.Areas.Administration.Controllers
{
    [Area("Administration")]
    [Authorize]
    [Authorize(Roles = "Administrator")]
    public class TournamentController : Controller
    {
        private readonly ITournamentProvider<ACM.AzureChallengeResult, ACMT.TournamentDetails> tournamentProvider;
        private readonly IAssignedQuestionProvider<ACM.AzureChallengeResult, ACMQ.AssignedQuestion> assignedQuestionProvider;
        private readonly IQuestionProvider<ACM.AzureChallengeResult, ACMQ.Question> questionProvider;
        private readonly IParameterProvider<ACM.AzureChallengeResult, ACMP.GlobalTournamentParameters> globalParameterProvider;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;
        private readonly UserManager<AzureChallengeUIUser> userManager;

        public TournamentController(ITournamentProvider<ACM.AzureChallengeResult, ACMT.TournamentDetails> tournamentProvider,
                                    IAssignedQuestionProvider<ACM.AzureChallengeResult, ACMQ.AssignedQuestion> assignedQuestionProvider,
                                    IQuestionProvider<ACM.AzureChallengeResult, ACMQ.Question> questionProvider,
                                    IParameterProvider<ACM.AzureChallengeResult, ACMP.GlobalTournamentParameters> globalParameterProvider,
                                    IMapper mapper,
                                    IConfiguration configuration,
                                    UserManager<AzureChallengeUIUser> userManager)
        {
            this.tournamentProvider = tournamentProvider;
            this.assignedQuestionProvider = assignedQuestionProvider;
            this.questionProvider = questionProvider;
            this.globalParameterProvider = globalParameterProvider;
            this.mapper = mapper;
            this.configuration = configuration;
            this.userManager = userManager;
        }

        [Route("Administration/Tournament/Index")]
        [Route("Administration/Tournament")]
        public async Task<IActionResult> Index()
        {
            var result = await tournamentProvider.GetAllItemsAsync();

            var model = mapper.Map<IList<VM.IndexTournamentViewModel>>(result.Item2);

            return View(model);
        }

        [Route("Administration/Tournament/{tournamentId}/Edit")]
        public async Task<IActionResult> Edit(string tournamentId)
        {
            // Get the tournament details
            var tournament = await tournamentProvider.GetItemAsync(tournamentId);

            // Get the list of available questions
            var questions = await questionProvider.GetAllItemsAsync();

            var userProfile = await userManager.GetUserAsync(User);

            var model = new VM.EditTournamentViewModel()
            {
                Questions = new List<VM.Question>(),
                Id = tournamentId,
                Description = tournament.Item2.Description,
                Name = tournament.Item2.Name,
                TournamentQuestions = tournament.Item2.Questions.OrderBy(p => p.Index)
                                            .Select(p => new VM.TournamentQuestion
                                            {
                                                Description = p.Description,
                                                Difficulty = p.Difficulty,
                                                Id = p.Id,
                                                Index = p.Index,
                                                Name = p.Name,
                                                NextQuestionId = p.NextQuestionId,
                                                AssociatedQuestionId = p.AssociatedQuestionId
                                            }).ToList(),
                CurrentUserProfile = new EditTournamentViewModel.UserProfile
                {
                    SubscriptionId = userProfile.SubscriptionId,
                    TenantId = userProfile.TenantId,
                    UserNameHashed = userProfile.UserNameHashed()
                }
            };

            foreach (var q in questions.Item2)
            {
                if (!model.TournamentQuestions.Exists(p => p.AssociatedQuestionId == q.Id))
                {
                    model.Questions.Add(new VM.Question()
                    {
                        AzureService = q.TargettedAzureService,
                        Id = q.Id,
                        Name = $"{q.Name} - {q.Description} - (Level: {q.DifficultyString})",
                        Selected = false
                    });
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAssignedQuestion(EditTournamentViewModel inputModel)
        {
            if (ModelState.IsValid)
            {
                // Translate the view model to our backend model
                var assignedQuestion = new ACMQ.AssignedQuestion
                {
                    Answers = inputModel.QuestionToAdd.Answers
                                        .Select(p => new ACMQ.AssignedQuestion.AnswerList
                                        {
                                            AnswerParameters = p.AnswerParameters?.Select(a => new ACMQ.AssignedQuestion.AnswerParameterItem() { ErrorMessage = a.ErrorMessage, Key = a.Key, Value = a.Value }).ToList(),
                                            AssociatedQuestionId = p.AssociatedQuestionId,
                                            ResponseType = p.ResponseType
                                        }
                                        ).ToList(),
                    AssociatedQuestionId = inputModel.QuestionToAdd.AssociatedQuestionId,
                    Description = inputModel.QuestionToAdd.Description,
                    Difficulty = inputModel.QuestionToAdd.Difficulty,
                    Name = inputModel.QuestionToAdd.Name,
                    QuestionId = string.IsNullOrWhiteSpace(inputModel.QuestionToAdd.Id) ? Guid.NewGuid().ToString() : inputModel.QuestionToAdd.Id,
                    TargettedAzureService = inputModel.QuestionToAdd.TargettedAzureService,
                    Text = inputModel.QuestionToAdd.Text,
                    TextParameters = inputModel.QuestionToAdd.TextParameters.ToDictionary(p => p.Key, p => p.Value),
                    TournamentId = inputModel.QuestionToAdd.TournamentId,
                    Uris = inputModel.QuestionToAdd.Uris
                                    .Select(p => new ACMQ.AssignedQuestion.UriList
                                    {
                                        CallType = p.CallType,
                                        Id = p.Id,
                                        Uri = p.Uri,
                                        UriParameters = p.UriParameters.ToDictionary(q => q.Key, q => q.Value)
                                    }
                                    ).ToList(),
                    Justification = inputModel.QuestionToAdd.Justification,
                    UsefulLinks = inputModel.QuestionToAdd.UsefulLinks
                };

                List<ACMQ.QuestionLite> tournamentQuestions = null;
                if (inputModel.TournamentQuestions != null)
                    tournamentQuestions = inputModel.TournamentQuestions
                                                      .Select(p => new ACMQ.QuestionLite
                                                      {
                                                          Description = p.Description,
                                                          Difficulty = p.Difficulty,
                                                          Id = p.Id,
                                                          Index = p.Index,
                                                          Name = p.Name,
                                                          AssociatedQuestionId = p.AssociatedQuestionId,
                                                          NextQuestionId = p.NextQuestionId
                                                      }).ToList();
                else
                    tournamentQuestions = new List<ACMQ.QuestionLite>();

                // Get the existing tournament details
                var tournament = new ACMT.TournamentDetails()
                {
                    Description = inputModel.Description,
                    Id = inputModel.Id,
                    Name = inputModel.Name,
                    Questions = tournamentQuestions
                };

                // Check if this is an update to an existing tournament question
                if (tournament.Questions.Exists(p => p.Id == assignedQuestion.QuestionId))
                {
                    // We only need to update the assigned question, not the tournament
                    var updateQuestionResult = await assignedQuestionProvider.AddItemAsync(assignedQuestion);
                    if (!updateQuestionResult.Success)
                    {
                        return StatusCode(500);
                    }
                }
                else
                {
                    // Create a new Tournament Question
                    var newTournamentQuestion = new ACMQ.QuestionLite
                    {
                        Name = assignedQuestion.Name,
                        Description = assignedQuestion.Description,
                        Difficulty = assignedQuestion.Difficulty,
                        Id = assignedQuestion.QuestionId,
                        AssociatedQuestionId = assignedQuestion.AssociatedQuestionId,
                        Index = tournament.Questions.Count()
                    };
                    // Assign the next question value of the last question in the tournament to this one
                    if (tournament.Questions.Count > 0)
                    {
                        tournament.Questions[tournament.Questions.Count - 1].NextQuestionId = assignedQuestion.QuestionId;
                    }
                    tournament.Questions.Add(newTournamentQuestion);

                    // Get the global parameters for the tournament
                    var globalParamsResponse = await globalParameterProvider.GetItemAsync(assignedQuestion.TournamentId);
                    var globalParams = globalParamsResponse.Item2;

                    // Check if the parameter is global and it exists in the global parameters list
                    // If yes, increment the count. If no, add it
                    foreach (var parameter in assignedQuestion.TextParameters.ToList())
                    {
                        if (parameter.Key.StartsWith("Global."))
                        {
                            var globalParameter = globalParams.Parameters?.Where(p => p.Key == parameter.Key.Replace("Global.", "")).FirstOrDefault();

                            if (globalParameter == null)
                            {
                                if (globalParams.Parameters == null)
                                    globalParams.Parameters = new List<ACMP.GlobalTournamentParameters.ParameterDefinition>();

                                globalParams.Parameters.Add(new ACMP.GlobalTournamentParameters.ParameterDefinition
                                {
                                    Key = parameter.Key.Replace("Global.", ""),
                                    Value = parameter.Value,
                                    AssignedToQuestion = 1
                                });
                            }
                            else
                            {
                                globalParameter.AssignedToQuestion += 1;
                            }
                        }
                    }

                    await globalParameterProvider.AddItemAsync(globalParams);

                    var addQuestionResult = await assignedQuestionProvider.AddItemAsync(assignedQuestion);
                    if (addQuestionResult.Success)
                    {
                        var updateTournamentResult = await tournamentProvider.AddItemAsync(tournament);

                        if (!updateTournamentResult.Success)
                        {
                            await assignedQuestionProvider.DeleteItemAsync(assignedQuestion.QuestionId);
                            return StatusCode(500);
                        }
                    }
                }
            }

            return RedirectToAction("Edit", new
            {
                tournamentId = inputModel.QuestionToAdd.TournamentId
            });
        }

        [Route("Administration/Tournament/{tournamentId}/AssignedQuestion/{assignedQuestionId}")]
        public async Task<IActionResult> GetAssignedQuestionAsync(string tournamentId, string assignedQuestionId)
        {
            var result = await assignedQuestionProvider.GetItemAsync(assignedQuestionId);

            if (result.Item1.Success)
            {
                var question = new VM.AssignedQuestion()
                {
                    Answers = result.Item2.Answers
                                        .Select(p => new VM.AssignedQuestion.AnswerList()
                                        {
                                            AnswerParameters = p.AnswerParameters == null ? new List<VM.AssignedQuestion.KVPair>() : p.AnswerParameters.Select(q => new VM.AssignedQuestion.KVPair() { Key = q.Key, Value = q.Value, ErrorMessage = q.ErrorMessage }).ToList(),
                                            AssociatedQuestionId = p.AssociatedQuestionId,
                                            ResponseType = p.ResponseType
                                        }).ToList(),
                    AssociatedQuestionId = result.Item2.AssociatedQuestionId,
                    Description = result.Item2.Description,
                    Difficulty = result.Item2.Difficulty,
                    Id = result.Item2.QuestionId,
                    Name = result.Item2.Name,
                    TargettedAzureService = result.Item2.TargettedAzureService,
                    Text = result.Item2.Text,
                    TextParameters = result.Item2.TextParameters.Select(p => new VM.AssignedQuestion.KVPair { Key = p.Key, Value = p.Value }).ToList(),
                    TournamentId = result.Item2.TournamentId,
                    Uris = result.Item2.Uris
                             .Select(p => new VM.AssignedQuestion.UriList
                             {
                                 CallType = p.CallType,
                                 Id = p.Id,
                                 Uri = p.Uri,
                                 UriParameters = p.UriParameters.Select(q => new VM.AssignedQuestion.KVPair { Key = q.Key, Value = q.Value }).ToList()
                             }).ToList(),
                    Justification = result.Item2.Justification,
                    UsefulLinks = result.Item2.UsefulLinks ?? new List<string>()
                };

                return Ok(question);
            }
            else
            {
                return StatusCode(500);
            }
        }

        public async Task<IActionResult> RemoveQuestion(string tournamentId, string questionId)
        {
            // Get the tournament
            var tournamentResponse = await tournamentProvider.GetItemAsync(tournamentId);

            if (tournamentResponse.Item1.Success)
            {
                // Create a new object, in case we need to revert
                var tournament = new ACMT.TournamentDetails
                {
                    Description = tournamentResponse.Item2.Description,
                    Id = tournamentResponse.Item2.Id,
                    Name = tournamentResponse.Item2.Name,
                    Questions = tournamentResponse.Item2.Questions
                };

                var tournamentQuestions = tournament.Questions;

                // Remove the question from the list, re-index the next questions and change the pointers also
                // First find the question's index
                var questionIndex = tournamentQuestions.IndexOf(tournamentQuestions.Where(p => p.Id == questionId).FirstOrDefault());
                // Starting backwards until the question index - 1, re-index
                for (int i = tournamentQuestions.Count - 1; i > questionIndex - 1; i--)
                {
                    tournamentQuestions[i].Index = tournamentQuestions[i].Index - 1;
                }
                // Point the previous question to the next one
                // If the question to delete is the last one, point the previous question to null
                if (questionIndex == tournamentQuestions.Count - 1)
                {
                    // We only need to do something if it's not the only question
                    if (questionIndex > 0)
                        tournamentQuestions[questionIndex - 1].NextQuestionId = null;
                }
                else
                {
                    // We only need to do something if it's not the first question
                    if (questionIndex > 0)
                        tournamentQuestions[questionIndex - 1].NextQuestionId = tournamentQuestions[questionIndex + 1].Id;
                }

                // Get the assigned question
                var assignedQuestionResponse = await assignedQuestionProvider.GetItemAsync(questionId);
                var assignedQuestion = assignedQuestionResponse.Item2;
                // Get the global parameters for the tournament
                var globalParamsResponse = await globalParameterProvider.GetItemAsync(tournamentId);
                var globalParams = globalParamsResponse.Item2;

                // Check if the parameter is global and it exists in the global parameters list
                // If yes, increment the count. If no, add it
                foreach (var parameter in assignedQuestion.TextParameters.ToList())
                {
                    if (parameter.Key.StartsWith("Global."))
                    {
                        var globalParameter = globalParams.Parameters.Where(p => p.Key == parameter.Key.Replace("Global.", "")).FirstOrDefault();

                        if (globalParameter != null)
                        {
                            globalParameter.AssignedToQuestion -= 1;
                        }
                    }
                }

                await globalParameterProvider.AddItemAsync(globalParams);

                // Now remove the question
                tournamentQuestions.RemoveAt(questionIndex);

                // Update the tournament
                var updateResult = await tournamentProvider.AddItemAsync(tournament);

                if (updateResult.Success)
                {
                    // Delete the question also
                    var questionDeleteResult = await assignedQuestionProvider.DeleteItemAsync(questionId);

                    if (questionDeleteResult.Success)
                    {
                        return Ok();
                    }
                    else
                    {
                        // Try to re-add the previous state, hopefully it works
                        await tournamentProvider.AddItemAsync(tournamentResponse.Item2);
                        return StatusCode(500);
                    }
                }

                return StatusCode(500);
            }
            else
            {
                return StatusCode(500);
            }
        }

        public async Task<IActionResult> ValidateQuestion(string questionId)
        {
            var user = await userManager.GetUserAsync(User);

            var results = await assignedQuestionProvider.ValidateQuestion(questionId, mapper.Map<AzureChallenge.Models.Profile.UserProfile>(user));

            return Ok(results);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTournament(EditTournamentViewModel inputModel)
        {
            // We only care about updating the tournament values
            var tournament = new ACMT.TournamentDetails
            {
                Description = inputModel.Description,
                Id = inputModel.Id,
                Name = inputModel.Name,
                Questions = inputModel.TournamentQuestions
                                      .Select(p => new ACMQ.QuestionLite()
                                      {
                                          AssociatedQuestionId = p.AssociatedQuestionId,
                                          Description = p.Description,
                                          Difficulty = p.Difficulty,
                                          Id = p.Id,
                                          Index = p.Index,
                                          Name = p.Name,
                                          NextQuestionId = p.NextQuestionId
                                      }).ToList()
            };

            var updateResult = await tournamentProvider.AddItemAsync(tournament);

            if (updateResult.Success)
                return Ok();
            else
                return StatusCode(500);
        }

        [HttpPost]
        public async Task<IActionResult> AddNewTournament(IndexTournamentViewModel inputModel)
        {
            var tournament = new ACMT.TournamentDetails
            {
                Description = inputModel.Description,
                Id = Guid.NewGuid().ToString(),
                Name = inputModel.Name,
                Questions = new List<ACMQ.QuestionLite>()
            };

            var response = await tournamentProvider.AddItemAsync(tournament);

            if (response.Success)
            {
                // Add a new global parameter object for the Tournament
                await globalParameterProvider.AddItemAsync(new ACMP.GlobalTournamentParameters() { TournamentId = tournament.Id });
                return Ok();
            }

            return StatusCode(500);
        }
    }
}