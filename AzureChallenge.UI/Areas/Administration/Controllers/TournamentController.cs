using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AzureChallenge.Interfaces.Providers.Questions;
using AzureChallenge.Interfaces.Providers.Tournaments;
using VM = AzureChallenge.UI.Areas.Administration.Models.Tournaments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ACM = AzureChallenge.Models;
using ACMT = AzureChallenge.Models.Tournaments;
using ACMQ = AzureChallenge.Models.Questions;
using Microsoft.CodeAnalysis.Differencing;
using AzureChallenge.UI.Areas.Administration.Models.Tournaments;

namespace AzureChallenge.UI.Areas.Administration.Controllers
{
    [Area("Administration")]
    public class TournamentController : Controller
    {
        private ITournamentProvider<ACM.AzureChallengeResult, ACMT.TournamentDetails> tournamentProvider;
        private IAssignedQuestionProvider<ACM.AzureChallengeResult, ACMQ.AssignedQuestion> assignedQuestionProvider;
        private IQuestionProvider<ACM.AzureChallengeResult, ACMQ.Question> questionProvider;
        private IMapper mapper;
        private IConfiguration configuration;

        public TournamentController(ITournamentProvider<ACM.AzureChallengeResult, ACMT.TournamentDetails> tournamentProvider,
                                    IAssignedQuestionProvider<ACM.AzureChallengeResult, ACMQ.AssignedQuestion> assignedQuestionProvider,
                                    IQuestionProvider<ACM.AzureChallengeResult, ACMQ.Question> questionProvider,
                                    IMapper mapper,
                                    IConfiguration configuration)
        {
            this.tournamentProvider = tournamentProvider;
            this.assignedQuestionProvider = assignedQuestionProvider;
            this.questionProvider = questionProvider;
            this.mapper = mapper;
            this.configuration = configuration;
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
            // Get the questions assigned to this tournament
            var assignedQuestions = await assignedQuestionProvider.GetAllItemsAsync();

            // Get the list of available questions
            var questions = await questionProvider.GetAllItemsAsync();

            var model = new VM.EditTournamentViewModel() { AssignedQuestions = new List<VM.AssignedQuestion>(), Id = tournamentId, Name = tournament.Item2.Name, Questions = new List<VM.Question>() };

            foreach (var aq in assignedQuestions.Item2)
            {
                model.AssignedQuestions.Add(new VM.AssignedQuestion()
                {
                    Answers = aq.Answers
                                 .Select(p => new VM.AssignedQuestion.AnswerList()
                                 {
                                     AnswerParameters = p.AnswerParameters.Select(q => new VM.AssignedQuestion.KVPair() { Key = q.Key, Value = q.Value }).ToList(),
                                     AssociatedQuestionId = p.AssociatedQuestionId,
                                     ResponseType = p.ResponseType
                                 }).ToList(),
                    AssociatedQuestionId = aq.AssociatedQuestionId,
                    Description = aq.Description,
                    Difficulty = aq.Difficulty,
                    Id = aq.QuestionId,
                    Name = aq.Name,
                    TargettedAzureService = aq.TargettedAzureService,
                    Text = aq.Text,
                    TextParameters = aq.TextParameters.Select(p => new VM.AssignedQuestion.KVPair { Key = p.Key, Value = p.Value }).ToList(),
                    TournamentId = aq.TournamentId,
                    Uris = aq.Uris
                             .Select(p => new VM.AssignedQuestion.UriList
                             {
                                 CallType = p.CallType,
                                 Id = p.Id,
                                 Uri = p.Uri,
                                 UriParameters = p.UriParameters.Select(q => new VM.AssignedQuestion.KVPair { Key = q.Key, Value = q.Value }).ToList()
                             }).ToList()
                });
            }
            foreach (var q in questions.Item2)
            {
                model.Questions.Add(new VM.Question()
                {
                    AzureService = q.TargettedAzureService,
                    Id = q.Id,
                    Name = $"{q.Name} - {q.Description} - (Level: {q.DifficultyString})",
                    Selected = model.AssignedQuestions.Exists(p => p.AssociatedQuestionId == q.Id)
                });
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAssignedQuestion(EditTournamentViewModel inputModel)
        {
            if (ModelState.IsValid)
            {
                // Translate the view model to our backed model
                var assignedQuestion = new ACMQ.AssignedQuestion
                {
                    Answers = inputModel.QuestionToAdd.Answers
                                        .Select(p => new ACMQ.AssignedQuestion.AnswerList
                                        {
                                            AnswerParameters = p.AnswerParameters.ToDictionary(q => q.Key, q => q.Value),
                                            AssociatedQuestionId = p.AssociatedQuestionId,
                                            ResponseType = p.ResponseType
                                        }
                                        ).ToList(),
                    AssociatedQuestionId = inputModel.QuestionToAdd.AssociatedQuestionId,
                    Description = inputModel.QuestionToAdd.Description,
                    Difficulty = inputModel.QuestionToAdd.Difficulty,
                    Name = inputModel.QuestionToAdd.Name,
                    QuestionId = Guid.NewGuid().ToString(),
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
                };

                await assignedQuestionProvider.AddItemAsync(assignedQuestion);
            }

            return RedirectToAction("Edit", new { tournamentId = inputModel.QuestionToAdd.TournamentId });
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
                                            AnswerParameters = p.AnswerParameters.Select(q => new VM.AssignedQuestion.KVPair() { Key = q.Key, Value = q.Value }).ToList(),
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
                             }).ToList()
                };

                return Ok(question);
            }
            else
            {
                return StatusCode(500);
            }
        }
    }
}