﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureChallenge.Interfaces.Providers.Aggregates
{
    public interface IAggregateProvider<T, Q>
    {
        public Task<(T, Q)> GetItemAsync(string id);

        public Task<(T, IList<Q>)> GetAllItemsAsync();

        public Task<T> AddItemAsync(Q item);
    }
}
