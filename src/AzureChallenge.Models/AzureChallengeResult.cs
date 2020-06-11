using System;
using System.Collections.Generic;
using System.Text;

namespace AzureChallenge.Models
{
    public class AzureChallengeResult
    {
        public bool IsError { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
