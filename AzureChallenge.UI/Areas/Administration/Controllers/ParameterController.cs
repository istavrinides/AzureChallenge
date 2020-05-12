using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using AzureChallenge.Interfaces.Providers.Parameters;
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
        private IParameterProvider<ACM.AzureChallengeResult, ACMP.GlobalParameters> parametersProvider;
        private IMapper mapper;
        private IConfiguration configuration;

        public ParameterController(IParameterProvider<ACM.AzureChallengeResult, ACMP.GlobalParameters> parametersProvider,
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
            var tournamentParameters = new ACMP.GlobalParameters() { Parameters = new List<ACMP.GlobalParameters.ParameterDefinition>(), TournamentId = inputModel.TournamentId };

            if (inputModel.ParameterList != null)
            {
                foreach (var p in inputModel.ParameterList)
                {
                    tournamentParameters.Parameters.Add(new ACMP.GlobalParameters.ParameterDefinition() { Key = p.Name, Value = p.Value, AssignedToQuestion = p.AssignedToQuestion });
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
                return Ok(result.Item2);
            }

            return StatusCode(500);
        }
    }
}