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
    public sealed class AccountDataAccess : BaseDataAccess
    {
        private readonly PartitionKey pk;
        private readonly ILogger<AccountDataAccess> logger;

        public AccountDataAccess(IOptions<Options.CosmosDB> cosmosOptions, ILogger<AccountDataAccess> logger)
            : base(cosmosOptions)
        {
            this.pk = new PartitionKey(nameof(Account));
            this.logger = logger;
        }

        public async Task<Account> GetAsync(string name)
        {
            try
            {
                var response = await this.Container.ReadItemAsync<AccountModel>(name?.ToLower(), this.pk);
                this.logger.LogInformation($"Cost: {response.RequestCharge} ({nameof(AccountDataAccess)}.{nameof(GetAsync)})");
                return response.Resource;
            }
            catch (CosmosException ce) when (ce.StatusCode == HttpStatusCode.NotFound)
            {
                return default;
            }
        }

        private async Task<AccountTier?> GetMaxTierByCreatedFromAsync(string createdFrom)
        {
            var query = new QueryDefinition("SELECT TOP 1 VALUE a.Tier FROM a WHERE a.CreatedFrom = @CreatedFrom ORDER BY a.Tier ASC")
                        .WithParameter("@CreatedFrom", createdFrom?.ToLower());
            using var streamResultSet = this.Container.GetItemQueryStreamIterator(
                                            query,
                                            requestOptions: new QueryRequestOptions
                                            {
                                                MaxItemCount = 1,
                                                PartitionKey = this.pk
                                            });

            var response = await streamResultSet.ReadNextAsync();
            this.logger.LogInformation($"Cost: {response.Headers.RequestCharge} ({nameof(AccountDataAccess)}.{nameof(GetMaxTierByCreatedFromAsync)})");

            return response.IsSuccessStatusCode
                ? (await response.Content.ReadResultsAsync<AccountTier>()).FirstOrDefault()
                : default;
        }

        private async Task<int> GetCountByCreatedFromAsync(string createdFrom)
        {
            var query = new QueryDefinition("SELECT VALUE COUNT(1) FROM a WHERE a.CreatedFrom = @CreatedFrom")
                        .WithParameter("@CreatedFrom", createdFrom?.ToLower());
            using var streamResultSet = this.Container.GetItemQueryStreamIterator(
                                            query,
                                            requestOptions: new QueryRequestOptions
                                            {
                                                MaxItemCount = 1,
                                                PartitionKey = this.pk
                                            });

            var response = await streamResultSet.ReadNextAsync();
            this.logger.LogInformation($"Cost: {response.Headers.RequestCharge} ({nameof(AccountDataAccess)}.{nameof(GetCountByCreatedFromAsync)})");

            return response.IsSuccessStatusCode
                ? (await response.Content.ReadResultsAsync<int>()).FirstOrDefault()
                : default;
        }

        public async Task<Account> CreateAsync(Account account)
        {
            if (await this.GetAsync(account.Name) != default) throw new ConflictException($"Account with name [{account.Name}] already exists.");
            if (await this.GetCountByCreatedFromAsync(account.CreatedFrom) >= TierLimits.AccountsPerCreatedFrom[await this.GetMaxTierByCreatedFromAsync(account.CreatedFrom) ?? AccountTier.Small]) throw new ConflictException($"Too many accounts already created from [{account.CreatedFrom}].");

            var accountModel = new AccountModel
            {
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedFrom = account.CreatedFrom.ToLower(),
                ModifiedAt = DateTimeOffset.UtcNow,
                Key = account.Key.ComputeSHA256(),
                Name = account.Name.ToLower(),
                Tier = AccountTier.Small
            };
            this.logger.LogInformation($"Creating account: {accountModel.ToJsonString()}");

            var response = await this.Container.CreateItemAsync(accountModel, this.pk);
            this.logger.LogInformation($"Cost: {response.RequestCharge} ({nameof(AccountDataAccess)}.{nameof(CreateAsync)})");
            return response.Resource;
        }
    }
}
