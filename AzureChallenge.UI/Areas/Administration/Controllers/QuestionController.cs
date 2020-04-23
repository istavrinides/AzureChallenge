﻿using System;
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
using System.Net;
using System.IO;
using System.IO.Pipes;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Http;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AzureChallenge.UI.Areas.Administration.Controllers
{
    [Area("Administration")]
    public class QuestionController : Controller
    {
        private IQuestionProvider<ACM.AzureChallengeResult, ACMQ.Question> questionProvider;
        private IMapper mapper;
        private IConfiguration configuration;

        public QuestionController(IQuestionProvider<ACM.AzureChallengeResult,
                                    ACMQ.Question> questionProvider,
                                    IMapper mapper,
                                    IConfiguration configuration)
        {
            this.questionProvider = questionProvider;
            this.mapper = mapper;
            this.configuration = configuration;
        }

        public class AzureServiceListReponse
        {
            public AzureServiceClass[] Classes { get; set; }
        }

        public class AzureServiceClass
        {
            public string name { get; set; }
            public string scope { get; set; }
            public Service[] services { get; set; }
        }

        public class Service
        {
            public string name { get; set; }
            public string url { get; set; }
            public string description { get; set; }
        }


        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> AddNew()
        {
            var client = new HttpClient();
            var response = await client.GetAsync(configuration["Endpoints:AzureServicesEnpoint"]);
            var azureServiceList = new List<string>();

            if (response != null)
            {
                var serviceListStr = await response.Content.ReadAsStringAsync();
                var serviceList = JsonConvert.DeserializeObject<List<AzureServiceClass>>(serviceListStr);
                var azureServices = serviceList.Where(p => p.name == "Azure").FirstOrDefault();
                
                if (azureServices != null)
                {
                    foreach (var service in azureServices.services)
                    {
                        azureServiceList.Add(service.name);
                    }
                }
            }

            var model = new AddNewQuestionViewModel()
            {
                Uris = new List<AddNewQuestionViewModel.UriList>(),
                Answers = new List<AddNewQuestionViewModel.AnswerList>(),
                AzureServicesList = azureServiceList
            };

            return View(model);
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