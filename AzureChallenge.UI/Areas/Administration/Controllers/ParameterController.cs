using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using AzureChallenge.Interfaces.Providers.Parameters;
using AzureChallenge.Models.Parameters;
using AzureChallenge.UI.Areas.Administration.Models.Tournaments;
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
        private IParameterProvider<ACM.AzureChallengeResult, ACMP.GlobalTournamentParameters> parametersProvider;
        private IMapper mapper;
        private IConfiguration configuration;

        public ParameterController(IParameterProvider<ACM.AzureChallengeResult, ACMP.GlobalTournamentParameters> parametersProvider,
                                    IMapper mapper,
                                    IConfiguration configuration)
        {
            this.parametersProvider = parametersProvider;
            this.mapper = mapper;
            this.configuration = configuration;
        }

        [Route("Administration/Tournament/{tournamentId}/GlobalParameters")]
        public async Task<IActionResult> Index(string tournamentId)
        {
            var result = await parametersProvider.GetItemAsync(tournamentId);

            if (!result.Item1.Success)
                return StatusCode(500);

            var model = new IndexParameterViewModel() { TournamentId = tournamentId, ParameterList = new List<IndexParameterViewModel.ParameterItem>() };

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
            var tournamentParameters = new ACMP.GlobalTournamentParameters() { Parameters = new List<ACMP.GlobalTournamentParameters.ParameterDefinition>(), TournamentId = inputModel.TournamentId };

            if (inputModel.ParameterList != null)
            {
                foreach (var p in inputModel.ParameterList)
                {
                    tournamentParameters.Parameters.Add(new ACMP.GlobalTournamentParameters.ParameterDefinition() { Key = p.Name, Value = p.Value, AssignedToQuestion = p.AssignedToQuestion });
                }
            }

            var result = await parametersProvider.AddItemAsync(tournamentParameters);

            if (!result.Success)
                return StatusCode(500);

            return Ok();
        }

        [Route("Administration/Tournament/{tournamentId}/GlobalParameters/Get")]
        public async Task<IActionResult> GetQuestionByIdAsync(string tournamentId)
        {
            var result = await parametersProvider.GetItemAsync(tournamentId);

            if (result.Item1.Success)
            {
                var globalParameters = new GlobalTournamentParameters
                {
                    Parameters = result.Item2.Parameters.Select(p => new GlobalTournamentParameters.ParameterDefinition() { AssignedToQuestion = p.AssignedToQuestion, Key = $"Global.{p.Key}", Value = p.Value }).ToList(),
                    TournamentId = result.Item2.TournamentId
                };
                return Ok(globalParameters);
            }

            return StatusCode(500);
        }
    }
}