using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using AzureChallenge.Interfaces.Providers.Parameters;
using AzureChallenge.Models.Parameters;
using AzureChallenge.UI.Areas.Administration.Models.Challenges;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ACM = AzureChallenge.Models;
using ACMP = AzureChallenge.Models.Parameters;

namespace AzureChallenge.UI.Areas.Administration.Controllers
{
    [Area("Administration")]
    [Authorize(Roles = "Administrator")]
    public class ParameterController : Controller
    {
        private IParameterProvider<ACM.AzureChallengeResult, ACMP.GlobalChallengeParameters> parametersProvider;
        private IMapper mapper;
        private IConfiguration configuration;

        public ParameterController(IParameterProvider<ACM.AzureChallengeResult, ACMP.GlobalChallengeParameters> parametersProvider,
                                    IMapper mapper,
                                    IConfiguration configuration)
        {
            this.parametersProvider = parametersProvider;
            this.mapper = mapper;
            this.configuration = configuration;
        }

        [Route("Administration/Challenge/{challengeId}/GlobalParameters")]
        public async Task<IActionResult> Index(string challengeId)
        {
            var result = await parametersProvider.GetItemAsync(challengeId);
            var model = new IndexParameterViewModel() { ChallengeId = challengeId, ParameterList = new List<IndexParameterViewModel.ParameterItem>() };

            if (result.Item1.IsError)
                return StatusCode(500);
            else if (!result.Item1.Success)
                return View(model);

            if (result.Item2.Parameters != null)
            {
                foreach (var d in result.Item2.Parameters)
                {
                    model.ParameterList.Add(new IndexParameterViewModel.ParameterItem() { Name = d.Key, Value = d.Value, AssignedToQuestion = d.AssignedToQuestion });
                }
            }

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateParameters(IndexParameterViewModel inputModel)
        {
            var challengeParameters = new ACMP.GlobalChallengeParameters() { Parameters = new List<ACMP.GlobalChallengeParameters.ParameterDefinition>(), ChallengeId = inputModel.ChallengeId };

            if (inputModel.ParameterList != null)
            {
                foreach (var p in inputModel.ParameterList)
                {
                    challengeParameters.Parameters.Add(new ACMP.GlobalChallengeParameters.ParameterDefinition() { Key = p.Name, Value = p.Value, AssignedToQuestion = p.AssignedToQuestion });
                }
            }

            var result = await parametersProvider.AddItemAsync(challengeParameters);

            if (!result.Success)
                return StatusCode(500);

            return Ok();
        }

        [Route("Administration/Challenge/{challengeId}/GlobalParameters/Get")]
        public async Task<IActionResult> GetQuestionByIdAsync(string challengeId)
        {
            var result = await parametersProvider.GetItemAsync(challengeId);

            if (result.Item1.Success)
            {
                var globalParameters = new GlobalChallengeParameters
                {
                    Parameters = result.Item2.Parameters == null ? new List<GlobalChallengeParameters.ParameterDefinition>() : result.Item2.Parameters.Select(p => new GlobalChallengeParameters.ParameterDefinition() { AssignedToQuestion = p.AssignedToQuestion, Key = $"Global.{p.Key}", Value = p.Value }).ToList(),
                    ChallengeId = result.Item2.ChallengeId
                };
                return Ok(globalParameters);
            }
            else if (!result.Item1.IsError)
            {
                // No parameters yet
                var globalParameters = new GlobalChallengeParameters
                {
                    Parameters = new List<GlobalChallengeParameters.ParameterDefinition>(),
                    ChallengeId = challengeId
                };

                return Ok(globalParameters);
            }

            return StatusCode(500);
        }
    }
}