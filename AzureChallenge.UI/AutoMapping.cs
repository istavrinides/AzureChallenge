using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ACModels = AzureChallenge.Models;
using ACAreaViewModels = AzureChallenge.UI.Areas.Administration.Models;

namespace AzureChallenge.UI
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            CreateMap<ACModels.Questions.Question, ACAreaViewModels.Questions.AddNewQuestionViewModel>().ReverseMap();
        }
    }
}
