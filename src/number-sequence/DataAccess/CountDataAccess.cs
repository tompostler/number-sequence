using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using number_sequence.Exceptions;
using number_sequence.Extensions;
using number_sequence.Models;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.DataAccess
{
    public sealed class CountDataAccess : BaseDataAccess
    {
        private readonly AccountDataAccess accountDataAccess;
        private readonly ILogger<CountDataAccess> logger;

        public CountDataAccess(AccountDataAccess accountDataAccess, IOptions<Options.CosmosDB> cosmosOptions, ILogger<CountDataAccess> logger)
            : base(cosmosOptions)
        {
            this.accountDataAccess = accountDataAccess;
            this.logger = logger;
        }

        private PartitionKey MakePK(string account) => new PartitionKey($"{nameof(Count)}|{account}".ToLower());

        public async Task<Count> TryGetAsync(string account, string name)
        {
            try
            {
                var response = await this.Container.ReadItemAsync<CountModel>(name?.ToLower(), this.MakePK(account));
                this.logger.LogInformation($"Cost: {response.RequestCharge} ({nameof(CountDataAccess)}.{nameof(TryGetAsync)})");
                return response.Resource;
            }
            catch (CosmosException ce) when (ce.StatusCode == HttpStatusCode.NotFound)
            {
                return default;
            }
        }

        private async Task<int> GetCountByAccountAsync(string account)
        {
            var query = new QueryDefinition("SELECT VALUE COUNT(1) FROM c");
            using var streamResultSet = this.Container.GetItemQueryStreamIterator(
                                            query,
                                            requestOptions: new QueryRequestOptions
                                            {
                                                MaxItemCount = 1,
                                                PartitionKey = this.MakePK(account)
                                            });

            var response = await streamResultSet.ReadNextAsync();
            this.logger.LogInformation($"Cost: {response.Headers.RequestCharge} ({nameof(CountDataAccess)}.{nameof(GetCountByAccountAsync)})");

            return response.IsSuccessStatusCode
                ? (await response.Content.ReadResultsAsync<int>()).FirstOrDefault()
                : default;
        }

        public async Task<Count> CreateAsync(Count count)
        {
            if (await this.TryGetAsync(count.Account, count.Name) != default) throw new ConflictException($"Count with name [{count.Name}] already exists.");
            if (await this.GetCountByAccountAsync(count.Account) >= TierLimits.CountsPerAccount[(await this.accountDataAccess.TryGetAsync(count.Account)).Tier]) throw new ConflictException($"Too many counts already created for account with name [{count.Account}].");

            var countModel = new CountModel
            {
                Account = count.Account.ToLower(),
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow,
                Name = count.Name.ToLower(),
                Value = count.Value
            };
            this.logger.LogInformation($"Creating count: {countModel.ToJsonString()}");

            var response = await this.Container.CreateItemAsync(countModel, this.MakePK(count.Account));
            this.logger.LogInformation($"Cost: {response.RequestCharge} ({nameof(CountDataAccess)}.{nameof(CreateAsync)})");
            return response.Resource;
        }

        public async Task<Count> IncrementAsync(string account, string name)
        {
            var response = await this.Container.ReadItemAsync<CountModel>(name?.ToLower(), this.MakePK(account));
            this.logger.LogInformation($"Cost: {response.RequestCharge} ({nameof(CountDataAccess)}.{nameof(IncrementAsync)}.IncrementAsync)");
            var count = response.Resource;
            count.Value += 1;
            try
            {
                response = await this.Container.ReplaceItemAsync(
                                                    count,
                                                    count.Name,
                                                    this.MakePK(account),
                                                    new ItemRequestOptions { IfMatchEtag = response.ETag });
                return response.Resource;
            }
            catch (CosmosException ce) when (ce.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                throw new ConflictException($"Count [{name}] was updated elsewhere.");
            }
        }

        public async Task<Count> IncrementByAmountAsync(string account, string name, ulong amount)
        {
            var response = await this.Container.ReadItemAsync<CountModel>(name?.ToLower(), this.MakePK(account));
            this.logger.LogInformation($"Cost: {response.RequestCharge} ({nameof(CountDataAccess)}.{nameof(IncrementAsync)}.IncrementByAmountAsync)");
            var count = response.Resource;
            count.Value += amount;
            try
            {
                response = await this.Container.ReplaceItemAsync(
                                                    count,
                                                    count.Name,
                                                    this.MakePK(account),
                                                    new ItemRequestOptions { IfMatchEtag = response.ETag });
                return response.Resource;
            }
            catch (CosmosException ce) when (ce.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                throw new ConflictException($"Count [{name}] was updated elsewhere.");
            }
        }
    }
}
