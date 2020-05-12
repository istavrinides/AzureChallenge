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
using System.Net;
using System.IO;
using System.IO.Pipes;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Http;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AzureChallenge.Models;
using Microsoft.AspNetCore.Authorization;
using System.Runtime.InteropServices.WindowsRuntime;

namespace AzureChallenge.UI.Areas.Administration.Controllers
{
    [Area("Administration")]
    [Authorize(Roles = "Administrator")]
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

        [Route("Administration/Questions/Index")]
        [Route("Administration/Questions")]
        public async Task<IActionResult> Index()
        {
            var result = await questionProvider.GetAllItemsAsync();

            (AzureChallengeResult, IList<IndexQuestionViewModel>) model = (result.Item1, mapper.Map<IList<IndexQuestionViewModel>>(result.Item2));

            return View(model);
        }

        [Route("Administration/Questions/{id}/Details")]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var result = await questionProvider.GetItemAsync(id);

            if (result.Item1.Success)
            {
                var question = result.Item2;

                var uriParams = new List<ViewQuestionModel.UriList>();

                foreach (var u in question.Uris)
                {
                    uriParams.Add(new ViewQuestionModel.UriList()
                    {
                        UriParameters = u.UriParameters,
                        CallType = u.CallType,
                        Id = u.Id,
                        Uri = u.Uri
                    });
                }

                var model = new ViewQuestionModel()
                {
                    Description = question.Description,
                    Difficulty = question.Difficulty,
                    Id = question.Id,
                    Name = question.Name,
                    TargettedAzureService = question.TargettedAzureService,
                    Text = question.Text,
                    TextParameters = question.TextParameters,
                    Uris = uriParams
                };


                return View(model);
            }

            return NotFound();
        }

        [Route("Administration/Questions/{id}/Edit")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var result = await questionProvider.GetItemAsync(id);

            if (result.Item1.Success)
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

                var question = result.Item2;

                var uriParams = new List<EditQuestionViewModel.UriList>();

                foreach (var u in question.Uris)
                {
                    uriParams.Add(new EditQuestionViewModel.UriList()
                    {
                        UriParameters = u.UriParameters,
                        CallType = u.CallType,
                        Id = u.Id,
                        Uri = u.Uri
                    });
                }

                var model = new EditQuestionViewModel()
                {
                    Description = question.Description,
                    Difficulty = question.Difficulty,
                    Id = question.Id,
                    Name = question.Name,
                    TargettedAzureService = question.TargettedAzureService,
                    Text = question.Text,
                    TextParameters = question.TextParameters,
                    Uris = uriParams,
                    AzureServicesList = azureServiceList
                };


                return View(model);
            }

            return NotFound();
        }

        [HttpGet]
        [Route("Administration/Questions/AddNew")]
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
                Id = Guid.NewGuid().ToString(),
                AzureServicesList = azureServiceList,
                Difficulty = 1,
                TextParameters = new List<string>(),
                Uris = new List<AddNewQuestionViewModel.UriList>()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Administration/Questions/AddNew")]
        public async Task<IActionResult> AddNew(AddNewQuestionViewModel model)
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
            model.AzureServicesList = azureServiceList;

            if (ModelState.IsValid)
            {
                var uriList = new List<ACMQ.Question.UriList>();

                foreach (var u in model.Uris)
                {
                    uriList.Add(new ACMQ.Question.UriList
                    {
                        CallType = u.CallType,
                        Id = u.Id,
                        Uri = u.Uri,
                        UriParameters = u.UriParameters
                    });
                }

                var mapped = new ACMQ.Question()
                {
                    Description = model.Description,
                    Difficulty = model.Difficulty,
                    Id = model.Id,
                    Name = model.Name,
                    TargettedAzureService = model.TargettedAzureService,
                    Text = model.Text,
                    TextParameters = model.TextParameters,
                    Uris = uriList
                };

                var responseAdd = await questionProvider.AddItemAsync(mapped);

                if (responseAdd.Success)
                {
                    return RedirectToAction("Index");
                }

            }

            return View(model);
        }

        [Route("Administration/Questions/{id}/Get")]
        public async Task<IActionResult> GetQuestionByIdAsync(string id)
        {
            var result = await questionProvider.GetItemAsync(id);

            return Ok(result.Item2);
        }
    }
}