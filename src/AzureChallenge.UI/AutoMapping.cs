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
            CreateMap<ACModels.Questions.Question, ACAreaViewModels.Questions.IndexQuestionViewModel>().ReverseMap();

            CreateMap<ACModels.Profile.UserProfile, AzureChallenge.UI.Areas.Identity.Data.AzureChallengeUIUser>().ReverseMap();

            CreateMap<ACModels.Challenges.ChallengeDetails, ACAreaViewModels.Challenges.ChallengeList>().ReverseMap();           
        }
    }
}
