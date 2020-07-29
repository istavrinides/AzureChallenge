using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AzureChallenge.Interfaces.Providers.Questions;
using AzureChallenge.Interfaces.Providers.Challenges;
using AzureChallenge.Interfaces.Providers.Parameters;
using VM = AzureChallenge.UI.Areas.Administration.Models.Challenges;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ACM = AzureChallenge.Models;
using ACMT = AzureChallenge.Models.Challenges;
using ACMQ = AzureChallenge.Models.Questions;
using ACMP = AzureChallenge.Models.Parameters;
using ACMA = AzureChallenge.Models.Aggregates;
using Microsoft.CodeAnalysis.Differencing;
using AzureChallenge.UI.Areas.Administration.Models.Challenges;
using Microsoft.AspNetCore.Identity;
using AzureChallenge.UI.Areas.Identity.Data;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Azure.Cosmos.Linq;
using AzureChallenge.Interfaces.Providers.Aggregates;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Net.Mime;
using System.Net.Http;
using System.Net.Cache;
using System.Threading;
using AutoMapper.Configuration.Annotations;
using System.Net;
using Microsoft.Extensions.Logging;

namespace AzureChallenge.UI.Areas.Administration.Controllers
{
    [Area("Administration")]
    [Authorize]
    [Authorize(Roles = "Administrator")]
    public class ChallengeController : Controller
    {
        private readonly ILogger<ChallengeController> _logger;
        private readonly IChallengeProvider<ACM.AzureChallengeResult, ACMT.ChallengeDetails> challengeProvider;
        private readonly IAssignedQuestionProvider<ACM.AzureChallengeResult, ACMQ.AssignedQuestion> assignedQuestionProvider;
        private readonly IQuestionProvider<ACM.AzureChallengeResult, ACMQ.Question> questionProvider;
        private readonly IParameterProvider<ACM.AzureChallengeResult, ACMP.GlobalChallengeParameters> challengeParameterProvider;
        private readonly IParameterProvider<ACM.AzureChallengeResult, ACMP.GlobalParameters> globalParameterProvider;
        private readonly IAggregateProvider<ACM.AzureChallengeResult, ACMA.Aggregate> aggregateProvider;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;
        private readonly UserManager<AzureChallengeUIUser> userManager;

        public ChallengeController(ILogger<ChallengeController> logger,
                                    IChallengeProvider<ACM.AzureChallengeResult, ACMT.ChallengeDetails> challengeProvider,
                                    IAssignedQuestionProvider<ACM.AzureChallengeResult, ACMQ.AssignedQuestion> assignedQuestionProvider,
                                    IQuestionProvider<ACM.AzureChallengeResult, ACMQ.Question> questionProvider,
                                    IParameterProvider<ACM.AzureChallengeResult, ACMP.GlobalChallengeParameters> challengeParameterProvider,
                                    IParameterProvider<ACM.AzureChallengeResult, ACMP.GlobalParameters> globalParameterProvider,
                                    IAggregateProvider<ACM.AzureChallengeResult, ACMA.Aggregate> aggregateProvider,
                                    IMapper mapper,
                                    IConfiguration configuration,
                                    UserManager<AzureChallengeUIUser> userManager)
        {
            this._logger = logger;
            this.challengeProvider = challengeProvider;
            this.assignedQuestionProvider = assignedQuestionProvider;
            this.questionProvider = questionProvider;
            this.challengeParameterProvider = challengeParameterProvider;
            this.globalParameterProvider = globalParameterProvider;
            this.aggregateProvider = aggregateProvider;
            this.mapper = mapper;
            this.configuration = configuration;
            this.userManager = userManager;
        }

        [Route("Administration/Challenge/Index")]
        [Route("Administration/Challenge")]
        public async Task<IActionResult> Index()
        {
            var result = await challengeProvider.GetAllItemsAsync();

            var model = new IndexChallengeViewModel
            {
                Challenges = mapper.Map<IList<VM.ChallengeList>>(result.Item2)
            };

            model.AzureServiceCategories = AzureChallenge.UI.Models.AzureServicesCategoryMapping.CategoryName;
            model.AzureServiceCategory = AzureChallenge.UI.Models.AzureServicesCategoryMapping.CategoryName[0];
            model.FilesAvailableForImport = new List<KeyValuePair<string, string>>();

            // Get the list of available import files from the public GitHub repo
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/repos/istavrinides/AzureChallenge/contents/Exports");
                request.Headers.Add("User-Agent", "AzChallenge");
                var response = await client.SendAsync(request);
                List<GitHubFiles> filesInRepo = JsonConvert.DeserializeObject<List<GitHubFiles>>(await response.Content.ReadAsStringAsync());

                foreach (var file in filesInRepo)
                {
                    if (file.name.EndsWith(".zip"))
                        model.FilesAvailableForImport.Add(new KeyValuePair<string, string>(file.download_url, file.name));
                }
            }

            return View(model);
        }

        [Route("Administration/Challenge/{challengeId}/Edit")]
        public async Task<IActionResult> Edit(string challengeId)
        {
            // Get the challenge details
            var challenge = await challengeProvider.GetItemAsync(challengeId);

            // Get the list of available questions
            var questions = await questionProvider.GetAllItemsAsync();

            var userProfile = await userManager.GetUserAsync(User);

            var model = new VM.EditChallengeViewModel()
            {
                Questions = new List<VM.Question>(),
                Id = challengeId,
                Description = challenge.Item2.Description,
                IsPublic = challenge.Item2.IsPublic,
                IsLocked = challenge.Item2.IsLocked,
                OldIsPublic = challenge.Item2.IsPublic,
                Name = challenge.Item2.Name,
                ChallengeQuestions = challenge.Item2.Questions.OrderBy(p => p.Index)
                                            .Select(p => new VM.ChallengeQuestion
                                            {
                                                Description = p.Description,
                                                Difficulty = p.Difficulty,
                                                Id = p.Id,
                                                Index = p.Index,
                                                Name = p.Name,
                                                NextQuestionId = p.NextQuestionId,
                                                AssociatedQuestionId = p.AssociatedQuestionId
                                            }).ToList(),
                CurrentUserProfile = new EditChallengeViewModel.UserProfile
                {
                    SubscriptionId = userProfile.SubscriptionId,
                    TenantId = userProfile.TenantId,
                    UserNameHashed = userProfile.UserNameHashed()
                },
                AzureServiceCategory = challenge.Item2.AzureServiceCategory
            };

            foreach (var q in questions.Item2)
            {
                if (!model.ChallengeQuestions.Exists(p => p.AssociatedQuestionId == q.Id))
                {
                    model.Questions.Add(new VM.Question()
                    {
                        AzureService = q.TargettedAzureService,
                        Id = q.Id,
                        Name = $"{q.Name} - (Level: {q.DifficultyString}) - {q.QuestionType}",
                        Selected = false
                    });
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAssignedQuestion(EditChallengeViewModel inputModel)
        {
            if (ModelState.IsValid)
            {
                // Translate the view model to our backend model
                var assignedQuestion = new ACMQ.AssignedQuestion
                {
                    Answers = inputModel.QuestionToAdd.Answers
                                        .Select(p => new ACMQ.AssignedQuestion.AnswerList
                                        {
                                            AnswerParameters = p.AnswerParameters?.Select(a => new ACMQ.AssignedQuestion.AnswerParameterItem() { ErrorMessage = a.ErrorMessage, Key = a.Key, Value = a.Value ?? (inputModel.QuestionToAdd.QuestionType == "MultiChoice" ? "false" : "") }).ToList(),
                                            AssociatedQuestionId = p.AssociatedQuestionId,
                                            ResponseType = p.ResponseType
                                        }
                                        ).ToList(),
                    QuestionType = inputModel.QuestionToAdd.QuestionType,
                    AssociatedQuestionId = inputModel.QuestionToAdd.AssociatedQuestionId,
                    Description = inputModel.QuestionToAdd.Description,
                    Difficulty = inputModel.QuestionToAdd.Difficulty,
                    Name = inputModel.QuestionToAdd.Name,
                    QuestionId = string.IsNullOrWhiteSpace(inputModel.QuestionToAdd.Id) ? Guid.NewGuid().ToString() : inputModel.QuestionToAdd.Id,
                    TargettedAzureService = inputModel.QuestionToAdd.TargettedAzureService,
                    Text = inputModel.QuestionToAdd.Text,
                    TextParameters = inputModel.QuestionToAdd.TextParameters?.ToDictionary(p => p.Key, p => p.Value) ?? new Dictionary<string, string>(),
                    ChallengeId = inputModel.QuestionToAdd.ChallengeId,
                    Uris = inputModel.QuestionToAdd.Uris?
                                    .Select(p => new ACMQ.AssignedQuestion.UriList
                                    {
                                        CallType = p.CallType,
                                        Id = p.Id,
                                        Uri = p.Uri,
                                        UriParameters = p.UriParameters?.ToDictionary(q => q.Key, q => q.Value) ?? new Dictionary<string, string>(),
                                        RequiresContributorAccess = p.RequiresContributorAccess
                                    }
                                    ).ToList(),
                    Justification = inputModel.QuestionToAdd.Justification,
                    UsefulLinks = inputModel.QuestionToAdd.UsefulLinks ?? new List<string>()
                };

                List<ACMQ.QuestionLite> challengeQuestions = null;
                if (inputModel.ChallengeQuestions != null)
                    challengeQuestions = inputModel.ChallengeQuestions
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
                    challengeQuestions = new List<ACMQ.QuestionLite>();

                // Get the existing challenge details
                var challenge = new ACMT.ChallengeDetails()
                {
                    Description = inputModel.Description,
                    Id = inputModel.Id,
                    Name = inputModel.Name,
                    Questions = challengeQuestions,
                    AzureServiceCategory = inputModel.AzureServiceCategory
                };

                // Check if this is an update to an existing challenge question
                if (challenge.Questions.Exists(p => p.Id == assignedQuestion.QuestionId))
                {
                    // We only need to update the assigned question, not the challenge
                    var updateQuestionResult = await assignedQuestionProvider.AddItemAsync(assignedQuestion);
                    if (!updateQuestionResult.Success)
                    {
                        return StatusCode(500);
                    }
                }
                else
                {
                    // Create a new Challenge Question
                    var newChallengeQuestion = new ACMQ.QuestionLite
                    {
                        Name = assignedQuestion.Name,
                        Description = assignedQuestion.Description,
                        Difficulty = assignedQuestion.Difficulty,
                        Id = assignedQuestion.QuestionId,
                        AssociatedQuestionId = assignedQuestion.AssociatedQuestionId,
                        Index = challenge.Questions.Count()
                    };
                    // Assign the next question value of the last question in the challenge to this one
                    if (challenge.Questions.Count > 0)
                    {
                        challenge.Questions[challenge.Questions.Count - 1].NextQuestionId = assignedQuestion.QuestionId;
                    }
                    challenge.Questions.Add(newChallengeQuestion);

                    // Get the global parameters for the challenge
                    var globalParamsResponse = await challengeParameterProvider.GetItemAsync(assignedQuestion.ChallengeId);
                    var globalParams = globalParamsResponse.Item2;

                    // Check if the parameter is global and it exists in the global parameters list
                    // If yes, increment the count. If no, add it
                    foreach (var parameter in assignedQuestion.TextParameters.ToList())
                    {
                        if (parameter.Key.StartsWith("Global."))
                        {
                            var globalParameter = globalParams?.Parameters?.Where(p => p.Key == parameter.Key.Replace("Global.", "")).FirstOrDefault();

                            if (globalParameter == null)
                            {
                                if (globalParams == null)
                                {
                                    globalParams = new ACMP.GlobalChallengeParameters() { ChallengeId = assignedQuestion.ChallengeId, Parameters = new List<ACMP.GlobalChallengeParameters.ParameterDefinition>() };
                                }
                                else if (globalParams.Parameters == null)
                                    globalParams.Parameters = new List<ACMP.GlobalChallengeParameters.ParameterDefinition>();

                                globalParams.Parameters.Add(new ACMP.GlobalChallengeParameters.ParameterDefinition
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

                    await challengeParameterProvider.AddItemAsync(globalParams);

                    var addQuestionResult = await assignedQuestionProvider.AddItemAsync(assignedQuestion);
                    if (addQuestionResult.Success)
                    {
                        var updateChallengeResult = await challengeProvider.AddItemAsync(challenge);

                        if (!updateChallengeResult.Success)
                        {
                            await assignedQuestionProvider.DeleteItemAsync(assignedQuestion.QuestionId);
                            return StatusCode(500);
                        }
                    }
                }
            }

            return RedirectToAction("Edit", new
            {
                challengeId = inputModel.QuestionToAdd.ChallengeId
            });
        }

        [Route("Administration/Challenge/{challengeId}/AssignedQuestion/{assignedQuestionId}")]
        public async Task<IActionResult> GetAssignedQuestionAsync(string challengeId, string assignedQuestionId)
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
                    QuestionType = result.Item2.QuestionType,
                    Description = result.Item2.Description,
                    Difficulty = result.Item2.Difficulty,
                    Id = result.Item2.QuestionId,
                    Name = result.Item2.Name,
                    TargettedAzureService = result.Item2.TargettedAzureService,
                    Text = result.Item2.Text,
                    TextParameters = result.Item2.TextParameters.Select(p => new VM.AssignedQuestion.KVPair { Key = p.Key, Value = p.Value }).ToList(),
                    ChallengeId = result.Item2.ChallengeId,
                    Uris = result.Item2.Uris?
                             .Select(p => new VM.AssignedQuestion.UriList
                             {
                                 CallType = p.CallType,
                                 Id = p.Id,
                                 Uri = p.Uri,
                                 UriParameters = p.UriParameters.Select(q => new VM.AssignedQuestion.KVPair { Key = q.Key, Value = q.Value }).ToList(),
                                 RequiresContributorAccess = p.RequiresContributorAccess
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

        public async Task<IActionResult> RemoveQuestion(string challengeId, string questionId)
        {
            // Get the challenge
            var challengeResponse = await challengeProvider.GetItemAsync(challengeId);

            if (challengeResponse.Item1.Success)
            {
                // Create a new object, in case we need to revert
                var challenge = new ACMT.ChallengeDetails
                {
                    Description = challengeResponse.Item2.Description,
                    Id = challengeResponse.Item2.Id,
                    Name = challengeResponse.Item2.Name,
                    Questions = challengeResponse.Item2.Questions,
                    AzureServiceCategory = challengeResponse.Item2.AzureServiceCategory
                };

                var challengeQuestions = challenge.Questions;

                // Remove the question from the list, re-index the next questions and change the pointers also
                // First find the question's index
                var questionIndex = challengeQuestions.IndexOf(challengeQuestions.Where(p => p.Id == questionId).FirstOrDefault());
                // Starting backwards until the question index - 1, re-index
                for (int i = challengeQuestions.Count - 1; i > questionIndex - 1; i--)
                {
                    challengeQuestions[i].Index = challengeQuestions[i].Index - 1;
                }
                // Point the previous question to the next one
                // If the question to delete is the last one, point the previous question to null
                if (questionIndex == challengeQuestions.Count - 1)
                {
                    // We only need to do something if it's not the only question
                    if (questionIndex > 0)
                        challengeQuestions[questionIndex - 1].NextQuestionId = null;
                }
                else
                {
                    // We only need to do something if it's not the first question
                    if (questionIndex > 0)
                        challengeQuestions[questionIndex - 1].NextQuestionId = challengeQuestions[questionIndex + 1].Id;
                }

                // Get the assigned question
                var assignedQuestionResponse = await assignedQuestionProvider.GetItemAsync(questionId);
                var assignedQuestion = assignedQuestionResponse.Item2;
                // Get the global parameters for the challenge
                var globalParamsResponse = await challengeParameterProvider.GetItemAsync(challengeId);
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

                await challengeParameterProvider.AddItemAsync(globalParams);

                // Now remove the question
                challengeQuestions.RemoveAt(questionIndex);

                // Update the challenge
                var updateResult = await challengeProvider.AddItemAsync(challenge);

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
                        await challengeProvider.AddItemAsync(challengeResponse.Item2);
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

            var results = await assignedQuestionProvider.ValidateQuestion(questionId, mapper.Map<AzureChallenge.Models.Profile.UserProfile>(user), new List<string>());

            return Ok(results);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateChallenge(EditChallengeViewModel inputModel)
        {
            // We only care about updating the challenge values
            var challenge = new ACMT.ChallengeDetails
            {
                Description = inputModel.Description,
                Id = inputModel.Id,
                Name = inputModel.Name,
                IsPublic = inputModel.IsPublic,
                Questions = inputModel.ChallengeQuestions == null ? new List<ACMQ.QuestionLite>() :
                            inputModel.ChallengeQuestions?
                                      .Select(p => new ACMQ.QuestionLite()
                                      {
                                          AssociatedQuestionId = p.AssociatedQuestionId,
                                          Description = p.Description,
                                          Difficulty = p.Difficulty,
                                          Id = p.Id,
                                          Index = p.Index,
                                          Name = p.Name,
                                          NextQuestionId = p.NextQuestionId
                                      }).ToList(),
                AzureServiceCategory = inputModel.AzureServiceCategory
            };

            var updateResult = await challengeProvider.AddItemAsync(challenge);

            // Check the IsPublic property. Depending on the change, we need to add or remove the challenge from the aggregate
            if (inputModel.IsPublic != inputModel.OldIsPublic)
            {
                // We only have one, so just get via the Partition search
                var aggregatesReponse = await aggregateProvider.GetItemAsync("00000000-0000-0000-0000-000000000000");

                if (aggregatesReponse.Item1.Success)
                {
                    var agg =
                        aggregatesReponse.Item2 ??
                        new ACMA.Aggregate()
                        {
                            Id = "00000000-0000-0000-0000-000000000000",
                            ChallengeTotals = new ACMA.ChallengeAggregateTotals() { TotalPublic = 0 }
                        };

                    agg.ChallengeTotals.TotalPublic += inputModel.IsPublic ? 1 : agg.ChallengeTotals.TotalPublic > 0 ? -1 : 0;
                    await aggregateProvider.AddItemAsync(agg);
                }
                else
                {
                    // Doesn't exists 
                    var agg = new ACMA.Aggregate()
                    {
                        Id = "00000000-0000-0000-0000-000000000000",
                        ChallengeTotals = new ACMA.ChallengeAggregateTotals() { TotalPublic = 0 }
                    };

                    agg.ChallengeTotals.TotalPublic += inputModel.IsPublic ? 1 : agg.ChallengeTotals.TotalPublic > 0 ? -1 : 0;
                    await aggregateProvider.AddItemAsync(agg);
                }
            }

            if (updateResult.Success)
                return Ok();
            else
                return StatusCode(500);
        }

        [HttpPost]
        public async Task<IActionResult> AddNewChallenge(IndexChallengeViewModelFromPost inputModel)
        {
            var challenge = new ACMT.ChallengeDetails
            {
                Description = inputModel.Description,
                Id = Guid.NewGuid().ToString(),
                Name = inputModel.Name,
                Questions = new List<ACMQ.QuestionLite>(),
                AzureServiceCategory = inputModel.AzureServiceCategory
            };

            var response = await challengeProvider.AddItemAsync(challenge);

            if (response.Success)
            {
                // Add a new global parameter object for the Challenge
                await challengeParameterProvider.AddItemAsync(new ACMP.GlobalChallengeParameters() { ChallengeId = challenge.Id });
                return Ok();
            }

            return StatusCode(500);
        }

        [HttpPost]
        public async Task<IActionResult> CopyChallenge(IndexChallengeViewModelFromPost inputModel)
        {
            var newChallengeId = Guid.NewGuid().ToString();

            // Get the copy-from challenge, assigned questions and global parameters
            var challegeOriginalResponse = await challengeProvider.GetItemAsync(inputModel.Id);
            var assignedQuestionOriginalResponse = await assignedQuestionProvider.GetItemsOfChallengeAsync(inputModel.Id);
            var globalParametersOriginalReponse = await challengeParameterProvider.GetItemAsync(inputModel.Id);

            // Change the challenge name, description and id
            var challenge = challegeOriginalResponse.Item2;
            challenge.Name = inputModel.Name;
            challenge.Description = inputModel.Description;
            challenge.Id = newChallengeId;
            challenge.IsLocked = false;
            challenge.IsPublic = false;
            challenge.AzureServiceCategory = inputModel.AzureServiceCategory;

            // For each assigned question, change the id and associated id in the 
            foreach (var q in assignedQuestionOriginalResponse.Item2)
            {
                var newQuestionId = Guid.NewGuid().ToString();

                var challengeQuestion = challenge.Questions.Where(p => p.Id == q.QuestionId).FirstOrDefault();
                challengeQuestion.Id = newQuestionId;
                q.QuestionId = newQuestionId;
                q.ChallengeId = newChallengeId;
            }

            // We need to set the correct new nextQuestionIds
            for (var i = 0; i < challenge.Questions.Count(); i++)
            {
                if (i + 1 != challenge.Questions.Count())
                {
                    challenge.Questions[i].NextQuestionId = challenge.Questions[i + 1].Id;
                }
            }

            globalParametersOriginalReponse.Item2.ChallengeId = newChallengeId;

            // Add the challenge
            await challengeProvider.AddItemAsync(challenge);
            await challengeParameterProvider.AddItemAsync(globalParametersOriginalReponse.Item2);

            // For each question, add it
            foreach (var q in assignedQuestionOriginalResponse.Item2)
            {
                await assignedQuestionProvider.AddItemAsync(q);
            }

            // Add some delay so that the database can commit the copy. Otherwise, doesn't appear on the refresh...
            Task.Delay(1000).Wait();

            return Ok();

        }

        [HttpPost]
        public async Task<IActionResult> DeleteChallenge(string challengeId)
        {
            var challengeResponse = await challengeProvider.GetItemAsync(challengeId);

            await challengeProvider.DeleteItemAsync(challengeId);
            await challengeParameterProvider.DeleteItemAsync(challengeId);
            await assignedQuestionProvider.DeleteAllItemsOfChallenge(challengeId);

            if (challengeResponse.Item2.IsPublic)
            {
                // We only have one, so just get via the Partition search
                var aggregatesReponse = await aggregateProvider.GetItemAsync("00000000-0000-0000-0000-000000000000");

                if (aggregatesReponse.Item1.Success)
                {
                    aggregatesReponse.Item2.ChallengeTotals.TotalPublic -= 1;

                    await aggregateProvider.AddItemAsync(aggregatesReponse.Item2);
                }
            }

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> ExportChallenge(string challengeId)
        {
            // Get the challenge
            var challenge = await challengeProvider.GetItemAsync(challengeId);
            var challengeParameters = await challengeParameterProvider.GetItemAsync(challengeId);
            var globalParameters = await globalParameterProvider.GetAllItemsAsync();
            var assignedQuestions = await assignedQuestionProvider.GetItemsOfChallengeAsync(challengeId);

            if (challenge.Item1.Success && challenge.Item2 != null
                && challengeParameters.Item1.Success && challengeParameters.Item2 != null && challengeParameters.Item2.Parameters != null && challengeParameters.Item2.Parameters.Count > 0
                && globalParameters.Item1.Success && globalParameters.Item2 != null && globalParameters.Item2.Count == 1
                && assignedQuestions.Item1.Success && assignedQuestions.Item2 != null && assignedQuestions.Item2.Count > 0)
            {

                using (var zipMS = new MemoryStream())
                {
                    using (var archive = new ZipArchive(zipMS, ZipArchiveMode.Create, true))
                    {

                        // Write the challenge json definition
                        var challengeEntry = archive.CreateEntry("challenge.json");
                        using (var stream = challengeEntry.Open())
                        using (var sr = new StreamWriter(stream))
                        {
                            sr.Write(JsonConvert.SerializeObject(challenge.Item2));
                        }

                        // Write the challenge parameters
                        var challengeParamEntry = archive.CreateEntry("challengeParams.json");
                        using (var stream = challengeParamEntry.Open())
                        using (var sr = new StreamWriter(stream))
                        {
                            sr.Write(JsonConvert.SerializeObject(challengeParameters.Item2));
                        }

                        // Write the global parameters
                        var globalParamEntry = archive.CreateEntry("globalParams.json");
                        using (var stream = globalParamEntry.Open())
                        using (var sr = new StreamWriter(stream))
                        {
                            // There is only one
                            sr.Write(JsonConvert.SerializeObject(globalParameters.Item2[0]));
                        }

                        foreach (var aq in assignedQuestions.Item2)
                        {
                            // Write the assigned question
                            var assignedQuestionEntry = archive.CreateEntry($"aq-{aq.QuestionId}.json");
                            using (var stream = assignedQuestionEntry.Open())
                            using (var sr = new StreamWriter(stream))
                            {
                                sr.Write(JsonConvert.SerializeObject(aq));
                            }

                            // Get and write the original question template
                            var question = await questionProvider.GetItemAsync(aq.AssociatedQuestionId);
                            var questionEntry = archive.CreateEntry($"q-{question.Item2.Id}.json");
                            using (var stream = questionEntry.Open())
                            using (var sr = new StreamWriter(stream))
                            {
                                question.Item2.Owner = null;
                                sr.Write(JsonConvert.SerializeObject(question.Item2));
                            }
                        }
                    }


                    return File(zipMS.ToArray(), "application/octet-stream", $"{challenge.Item2.Name}.zip");
                }
            }
            else
            {
                return StatusCode(500);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ImportChallenge(string uri)
        {
            try
            {
                using (var webClient = new WebClient())
                {
                    using (var zipMS = new MemoryStream(webClient.DownloadData(uri)))
                    {
                        using (var archive = new ZipArchive(zipMS, ZipArchiveMode.Read))
                        {
                            // Add the challenge
                            var challengeEntry = archive.Entries.Where(p => p.Name == "challenge.json").FirstOrDefault();
                            using (var challengeStream = challengeEntry.Open())
                            {
                                using (var challengeSR = new StreamReader(challengeStream))
                                {
                                    var challengeDoc = JsonConvert.DeserializeObject<ACMT.ChallengeDetails>(await challengeSR.ReadToEndAsync());

                                    // Check if we already have this one defined (same id). if so, don't allow import
                                    var doesChallengeExist = await challengeProvider.GetItemAsync(challengeDoc.Id);

                                    if (doesChallengeExist.Item1.Success && doesChallengeExist.Item2 != null)
                                    {
                                        return StatusCode(409);
                                    }

                                    // Challenge should be unlocked and private to start with
                                    challengeDoc.IsLocked = false;
                                    challengeDoc.IsPublic = false;

                                    await challengeProvider.AddItemAsync(challengeDoc);
                                }
                            }

                            // Add the challenge parameters
                            var challengeParamsEntry = archive.Entries.Where(p => p.Name == "challengeParams.json").FirstOrDefault();
                            using (var challengeParamsStream = challengeParamsEntry.Open())
                            {
                                using (var challengeParamsSR = new StreamReader(challengeParamsStream))
                                {
                                    var paramDoc = JsonConvert.DeserializeObject<ACMP.GlobalChallengeParameters>(await challengeParamsSR.ReadToEndAsync());
                                    await challengeParameterProvider.AddItemAsync(paramDoc);
                                }
                            }

                            // Append the global parameters
                            var globalParamsEntry = archive.Entries.Where(p => p.Name == "globalParams.json").FirstOrDefault();
                            using (var globalParamsStream = globalParamsEntry.Open())
                            {
                                using (var globalParamsSR = new StreamReader(globalParamsStream))
                                {
                                    var paramDoc = JsonConvert.DeserializeObject<ACMP.GlobalParameters>(await globalParamsSR.ReadToEndAsync());

                                    // Get the current global parameter entry
                                    var globalParameters = await globalParameterProvider.GetAllItemsAsync();

                                    // Check if it exists
                                    if (globalParameters.Item1.Success)
                                    {
                                        if (globalParameters.Item2.Count > 0)
                                        {
                                            // There is only one, check if it has parameters
                                            if (globalParameters.Item2[0].Parameters != null)
                                            {
                                                globalParameters.Item2[0].Parameters.AddRange(paramDoc.Parameters);
                                            }
                                            else
                                            {
                                                globalParameters.Item2[0].Parameters = new List<string>();
                                                globalParameters.Item2[0].Parameters.AddRange(paramDoc.Parameters);
                                            }
                                        }
                                        else
                                        {
                                            globalParameters.Item2.Add(paramDoc);
                                        }

                                        await globalParameterProvider.AddItemAsync(globalParameters.Item2[0]);
                                    }
                                    else
                                    {
                                        await globalParameterProvider.AddItemAsync(paramDoc);
                                    }

                                }
                            }

                            // Add the assigned questions
                            var assignedQuestionEntries = archive.Entries.Where(p => p.Name.StartsWith("aq-")).ToList();
                            foreach (var aq in assignedQuestionEntries)
                            {
                                using (var aqStream = aq.Open())
                                {
                                    using (var aqSR = new StreamReader(aqStream))
                                    {
                                        var aqDoc = JsonConvert.DeserializeObject<ACMQ.AssignedQuestion>(await aqSR.ReadToEndAsync());
                                        await assignedQuestionProvider.AddItemAsync(aqDoc);
                                    }
                                }
                            }

                            // Add the question templates
                            var questionEntries = archive.Entries.Where(p => p.Name.StartsWith("q-")).ToList();
                            foreach (var q in questionEntries)
                            {
                                using (var qStream = q.Open())
                                {
                                    using (var qSR = new StreamReader(qStream))
                                    {
                                        var qDoc = JsonConvert.DeserializeObject<ACMQ.Question>(await qSR.ReadToEndAsync());
                                        // Need to assigned owner
                                        qDoc.Owner = User.Identity.Name;
                                        await questionProvider.AddItemAsync(qDoc);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return StatusCode(500);
            }

            return Ok();
        }

        [HttpGet]
        [Route("Administration/Challenge/{challengeId}/Analytics")]
        public async Task<IActionResult> Analytics(string challengeId)
        {
            var aggregatesReponse = await aggregateProvider.GetItemAsync(challengeId);
            var model = new AnalyticsTournamentViewModel() { ChallengeId = challengeId };

            if (aggregatesReponse.Item1.Success)
            {
                var agg =
                    aggregatesReponse.Item2 ??
                    new ACMA.Aggregate()
                    {
                        Id = challengeId,
                        ChallengeUsers = new ACMA.ChallengeAggregateUsers() { Finished = 0, Started = 0, ChallengeProgress = new List<string>() }
                    };

                if (aggregatesReponse.Item2 == null)
                    await aggregateProvider.AddItemAsync(agg);
                else
                {
                    model.Finished = aggregatesReponse.Item2.ChallengeUsers.Finished;
                    model.Started = aggregatesReponse.Item2.ChallengeUsers.Started;
                    model.ChallengeProgress = aggregatesReponse.Item2.ChallengeUsers.ChallengeProgress;
                }

            }
            else
            {
                // No aggregate defined
                var agg = new ACMA.Aggregate
                {
                    Id = challengeId,
                    ChallengeUsers = new ACMA.ChallengeAggregateUsers() { Finished = 0, Started = 0, ChallengeProgress = new List<string>() }
                };


                await aggregateProvider.AddItemAsync(agg);
            }

            return View(model);
        }
    }
}