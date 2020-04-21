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
            await questionProvider.AddItemAsync(mapper.Map<ACMQ.Question>(model));

            return View(model);
        }
    }
}