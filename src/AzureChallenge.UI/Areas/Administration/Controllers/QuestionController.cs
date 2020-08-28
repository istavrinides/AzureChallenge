using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AzureChallenge.Interfaces.Providers.Data;
using ACM = AzureChallenge.Models;
using ACMQ = AzureChallenge.Models.Questions;
using ACMP = AzureChallenge.Models.Parameters;
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
using AzureChallenge.Interfaces.Providers.Parameters;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SmartFormat.Utilities;
using AzureChallenge.UI.Models;

namespace AzureChallenge.UI.Areas.Administration.Controllers
{
    [Area("Administration")]
    [Authorize(Roles = "Administrator, ContentEditor")]
    public class QuestionController : Controller
    {
        private IQuestionProvider<ACM.AzureChallengeResult, ACMQ.Question> questionProvider;
        private IParameterProvider<ACM.AzureChallengeResult, ACMP.GlobalParameters> globalParameterProvider;
        private IMapper mapper;
        private IConfiguration configuration;

        public QuestionController(IQuestionProvider<ACM.AzureChallengeResult, ACMQ.Question> questionProvider,
                                    IParameterProvider<ACM.AzureChallengeResult, ACMP.GlobalParameters> globalParameterProvider,
                                    IMapper mapper,
                                    IConfiguration configuration)
        {
            this.questionProvider = questionProvider;
            this.globalParameterProvider = globalParameterProvider;
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

            if (!result.Item1.Success || result.Item1.IsError)
                return StatusCode(500);

            var model = new List<IndexQuestionViewModel>();

            foreach (var q in result.Item2)
            {
                model.Add(new IndexQuestionViewModel
                {
                    CanDelete = q.Owner == User.Identity.Name,
                    Name = q.Name,
                    Description = q.Description,
                    Id = q.Id,
                    TargettedAzureService = q.TargettedAzureService,
                    QuestionType = q.QuestionType
                });
            }

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

                if (question.QuestionType == "API")
                {
                    foreach (var u in question.Uris)
                    {
                        uriParams.Add(new ViewQuestionModel.UriList()
                        {
                            UriParameters = u.UriParameters,
                            CallType = u.CallType,
                            Id = u.Id,
                            Uri = u.Uri,
                            RequiresContributorAccess = u.RequiresContributorAccess
                        });
                    }
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
                    Uris = uriParams,
                    Justification = question.Justification,
                    UsefulLinks = question.UsefulLinks ?? new List<string>(),
                    QuestionType = question.QuestionType
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
                if (result.Item2.Owner != User.Identity.Name)
                    return RedirectToAction("Index");

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

                        azureServiceList.AddRange(AzureServicesCategoryMapping.TargettedServiceAdditions);
                    }
                }

                var question = result.Item2;

                var uriParams = new List<EditQuestionViewModel.UriList>();

                if (question.QuestionType == "API")
                {
                    foreach (var u in question.Uris)
                    {
                        uriParams.Add(new EditQuestionViewModel.UriList()
                        {
                            UriParameters = u.UriParameters,
                            CallType = u.CallType,
                            Id = u.Id,
                            Uri = u.Uri,
                            RequiresContributorAccess = u.RequiresContributorAccess
                        });
                    }
                }

                var model = new EditQuestionViewModel()
                {
                    Description = question.Description,
                    Difficulty = question.Difficulty,
                    Id = question.Id,
                    Name = question.Name,
                    TargettedAzureService = question.TargettedAzureService,
                    Text = question.Text,
                    TextParameters = question.TextParameters ?? new List<string>(),
                    Uris = uriParams,
                    AzureServicesList = azureServiceList.OrderBy(p => p).ToList(),
                    AvailableParameters = new List<string>(),
                    Justification = question.Justification,
                    UsefulLinks = question.UsefulLinks ?? new List<string>(),
                    Owner = question.Owner,
                    QuestionType = question.QuestionType
                };

                // Get the list of global parameters (if exist)
                var globalParams = await globalParameterProvider.GetAllItemsAsync();

                if (globalParams.Item1.Success)
                {
                    if (globalParams.Item2.Count > 0)
                    {
                        // There is only one
                        model.AvailableParameters.AddRange(globalParams.Item2[0].Parameters);
                    }
                }


                return View(model);
            }

            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Administration/Questions/{id}/Edit")]
        public async Task<IActionResult> Edit(string id, AddNewQuestionViewModel model)
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

                    azureServiceList.AddRange(AzureServicesCategoryMapping.TargettedServiceAdditions);
                }
            }
            model.AzureServicesList = azureServiceList.OrderBy(p => p).ToList();
            model.AvailableParameters = new List<string>();

            // Get the list of global parameters (if exist)
            var globalParams = await globalParameterProvider.GetAllItemsAsync();

            if (globalParams.Item1.Success)
            {
                if (globalParams.Item2.Count > 0)
                {
                    // There is only one
                    model.AvailableParameters.AddRange(globalParams.Item2[0].Parameters);
                }
            }

            if (ModelState.IsValid)
            {
                var success = await AddUpdateQuestionAsync(model);

                if (success)
                    return RedirectToAction("Index");

            }

            return View(model);
        }

        [HttpGet]
        [Route("Administration/Questions/Add")]
        public async Task<IActionResult> Add(string type)
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

                    azureServiceList.AddRange(AzureServicesCategoryMapping.TargettedServiceAdditions);
                }
            }

            var model = new AddNewQuestionViewModel()
            {
                Id = Guid.NewGuid().ToString(),
                QuestionType = type,
                AzureServicesList = azureServiceList.OrderBy(p => p).ToList(),
                Difficulty = 1,
                TextParameters = new List<string>(),
                Uris = new List<AddNewQuestionViewModel.UriList>(),
                AvailableParameters = new List<string>(),
                UsefulLinks = new List<string>()
            };

            // Get the list of global parameters (if exist)
            var globalParams = await globalParameterProvider.GetAllItemsAsync();

            if (globalParams.Item1.Success)
            {
                if (globalParams.Item2.Count > 0)
                {
                    // There is only one
                    model.AvailableParameters.AddRange(globalParams.Item2[0].Parameters);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Administration/Questions/Add")]
        public async Task<IActionResult> Add(AddNewQuestionViewModel model)
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

                    azureServiceList.AddRange(AzureServicesCategoryMapping.TargettedServiceAdditions);
                }
            }
            model.AzureServicesList = azureServiceList.OrderBy(p => p).ToList();
            model.AvailableParameters = new List<string>();
            model.Owner = User.Identity.Name;


            // Get the list of global parameters (if exist)
            var globalParams = await globalParameterProvider.GetAllItemsAsync();

            if (globalParams.Item1.Success)
            {
                if (globalParams.Item2.Count > 0)
                {
                    // There is only one
                    model.AvailableParameters.AddRange(globalParams.Item2[0].Parameters);
                }
            }

            if (ModelState.IsValid)
            {
                var success = await AddUpdateQuestionAsync(model);

                if (success)
                    return RedirectToAction("Index");

            }

            if (model.TextParameters == null) model.TextParameters = new List<string>();
            if (model.UsefulLinks == null) model.UsefulLinks = new List<string>();

            return View(model);
        }

        [Route("Administration/Questions/{id}/Get")]
        public async Task<IActionResult> GetQuestionByIdAsync(string id)
        {
            var result = await questionProvider.GetItemAsync(id);

            return Ok(result.Item2);
        }

        public async Task<IActionResult> RemoveQuestion(string id)
        {
            var result = await questionProvider.DeleteItemAsync(id);

            if (result.Success)
            {
                return Ok();
            }
            else
            {
                return StatusCode(500);
            }
        }

        private async Task<bool> AddUpdateQuestionAsync(AddNewQuestionViewModel model)
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

                    azureServiceList.AddRange(AzureServicesCategoryMapping.TargettedServiceAdditions);
                }
            }
            model.AzureServicesList = azureServiceList.OrderBy(p => p).ToList();

            var uriList = new List<ACMQ.Question.UriList>();
            var paramList = new List<string>();

            if (model.QuestionType == "API")
            {

                foreach (var u in model.Uris)
                {
                    uriList.Add(new ACMQ.Question.UriList
                    {
                        CallType = u.CallType,
                        Id = u.Id,
                        Uri = u.Uri,
                        UriParameters = u.UriParameters,
                        RequiresContributorAccess = u.RequiresContributorAccess
                    });

                    if (u.UriParameters != null)
                        paramList.AddRange(u.UriParameters);
                }
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
                Uris = uriList,
                Justification = model.Justification,
                UsefulLinks = model.UsefulLinks,
                Owner = User.Identity.Name,
                QuestionType = model.QuestionType
            };

            if (model.TextParameters != null)
                paramList.AddRange(model.TextParameters);

            // Get the global parameter list
            var globalParamsExisting = await globalParameterProvider.GetAllItemsAsync();
            var globalParamToAdd = new ACMP.GlobalParameters { Parameters = new List<string>() };

            if (globalParamsExisting.Item1.Success)
            {
                // if not exists, create
                if (globalParamsExisting.Item2.Count == 0)
                {
                    globalParamToAdd.Id = Guid.NewGuid().ToString();

                    // Add everything we found
                    globalParamToAdd.Parameters.AddRange(paramList.Where(p => p.StartsWith("Global.")).Distinct());
                }
                else
                {
                    // It's only one
                    globalParamToAdd.Id = globalParamsExisting.Item2[0].Id;
                    globalParamToAdd.Parameters.AddRange(globalParamsExisting.Item2[0].Parameters);

                    // Add eveything we found that doesn't exist already
                    globalParamToAdd.Parameters.AddRange(paramList.Where(p => p.StartsWith("Global.")).Distinct().Where(p => !globalParamToAdd.Parameters.Any(g => g == p)));
                }

                await globalParameterProvider.AddItemAsync(globalParamToAdd);
            }

            var responseAdd = await questionProvider.AddItemAsync(mapped);

            return responseAdd.Success;
        }
    }
}