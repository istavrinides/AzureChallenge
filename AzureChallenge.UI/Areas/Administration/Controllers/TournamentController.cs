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

        //[HttpGet]
        //public async Task<IActionResult> AddNew()
        //{
        //    var client = new HttpClient();
        //    var response = await client.GetAsync(configuration["Endpoints:AzureServicesEnpoint"]);
        //    var azureServiceList = new List<string>();

        //    if (response != null)
        //    {
        //        var serviceListStr = await response.Content.ReadAsStringAsync();
        //        var serviceList = JsonConvert.DeserializeObject<List<AzureServiceClass>>(serviceListStr);
        //        var azureServices = serviceList.Where(p => p.name == "Azure").FirstOrDefault();

        //        if (azureServices != null)
        //        {
        //            foreach (var service in azureServices.services)
        //            {
        //                azureServiceList.Add(service.name);
        //            }
        //        }
        //    }

        //    var model = new AddNewQuestionViewModel()
        //    {
        //        Id = Guid.NewGuid().ToString(),
        //        AzureServicesList = azureServiceList,
        //        Difficulty = 1
        //    };

        //    return View(model);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> AddNew(AddNewQuestionViewModel model)
        //{
        //    var client = new HttpClient();
        //    var response = await client.GetAsync(configuration["Endpoints:AzureServicesEnpoint"]);
        //    var azureServiceList = new List<string>();

        //    if (response != null)
        //    {
        //        var serviceListStr = await response.Content.ReadAsStringAsync();
        //        var serviceList = JsonConvert.DeserializeObject<List<AzureServiceClass>>(serviceListStr);
        //        var azureServices = serviceList.Where(p => p.name == "Azure").FirstOrDefault();

        //        if (azureServices != null)
        //        {
        //            foreach (var service in azureServices.services)
        //            {
        //                azureServiceList.Add(service.name);
        //            }
        //        }
        //    }
        //    model.AzureServicesList = azureServiceList;

        //    if (ModelState.IsValid)
        //    {
        //        var answerList = new List<ACMQ.Question.AnswerList>();
        //        var uriList = new List<ACMQ.Question.UriList>();

        //        foreach (var a in model.Answers)
        //        {
        //            answerList.Add(new ACMQ.Question.AnswerList
        //            {
        //                AnswerParameters = a.AnswerParameters.ToDictionary(x => x.Key, x => x.Value),
        //                AssociatedQuestionId = a.AssociatedQuestionId,
        //                ResponseType = a.ResponseType
        //            });
        //        }
        //        foreach (var u in model.Uris)
        //        {
        //            uriList.Add(new ACMQ.Question.UriList
        //            {
        //                CallType = u.CallType,
        //                Id = u.Id,
        //                Uri = u.Uri,
        //                UriParameters = u.UriParameters.ToDictionary(x => x.Key, x => x.Value)
        //            });
        //        }

        //        var mapped = new ACMQ.Question()
        //        {
        //            Answers = answerList,
        //            Description = model.Description,
        //            Difficulty = model.Difficulty,
        //            Id = model.Id,
        //            Name = model.Name,
        //            TargettedAzureService = model.TargettedAzureService,
        //            Text = model.Text,
        //            TextParameters = model.TextParameters.ToDictionary(x => x.Key, x => x.Value),
        //            Uris = uriList
        //        };

        //        await questionProvider.AddItemAsync(mapped);
        //    }

        //    return View(model);
        //}
    }
}