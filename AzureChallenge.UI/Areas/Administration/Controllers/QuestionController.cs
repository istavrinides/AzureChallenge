using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AzureChallenge.Interfaces.Providers.Data;
using ACM = AzureChallenge.Models;
using ACMQ = AzureChallenge.Models.Questions;
using AzureChallenge.UI.Areas.Administration.Models.Questions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using AzureChallenge.Interfaces.Providers.Questions;

namespace AzureChallenge.UI.Areas.Administration.Controllers
{
    [Area("Administration")]
    public class QuestionController : Controller
    {
        private IQuestionProvider<ACM.AzureChallengeResult, ACMQ.Question> questionProvider;
        private IMapper mapper;

        public QuestionController(IQuestionProvider<ACM.AzureChallengeResult, ACMQ.Question> questionProvider, IMapper mapper)
        {
            this.questionProvider = questionProvider;
            this.mapper = mapper;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> AddNew()
        {
            return View(new AddNewQuestionViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNew(AddNewQuestionViewModel model)
        {
            var answers = new Dictionary<string, string>();
            var textParams = new Dictionary<string, string>();
            var uriParams = new Dictionary<string, string>();

            answers.Add("One", "OneVal");
            answers.Add("Two", "TwoVal");
            answers.Add("Three", "ThreeVal");
            textParams.Add("Four", "OneVal");
            textParams.Add("Five", "TwoVal");
            textParams.Add("Six", "ThreeVal");
            uriParams.Add("Seven", "OneVal");
            uriParams.Add("Eight", "TwoVal");
            uriParams.Add("Nine", "ThreeVal");

            var toInsert = new AddNewQuestionViewModel()
            {
                Answers = answers,
                Description = "Description",
                Name = "Name",
                Text = "Text",
                TextParameters = textParams,
                Uri = "https://google.com",
                UriParameters = uriParams,
            };

            await questionProvider.AddItemAsync(mapper.Map<ACMQ.Question>(toInsert));

            return View(model);
        }
    }
}