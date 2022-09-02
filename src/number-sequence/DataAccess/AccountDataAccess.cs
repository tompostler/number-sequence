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
    public sealed class AccountDataAccess : BaseCosmosDataAccess
    {
        private readonly PartitionKey pk;
        private readonly ILogger<AccountDataAccess> logger;

        public AccountDataAccess(IOptions<Options.CosmosDB> cosmosOptions, ILogger<AccountDataAccess> logger)
            : base(cosmosOptions)
        {
            this.pk = new PartitionKey(nameof(Account));
            this.logger = logger;
        }

        public async Task<Account> TryGetAsync(string name)
        {
            try
            {
                ItemResponse<AccountModel> response = await this.Container.ReadItemAsync<AccountModel>(name?.ToLower(), this.pk);
                this.logger.LogInformation($"Cost: {response.RequestCharge} ({nameof(AccountDataAccess)}.{nameof(TryGetAsync)})");
                return response.Resource;
            }
            catch (CosmosException ce) when (ce.StatusCode == HttpStatusCode.NotFound)
            {
                return default;
            }
        }

        private async Task<AccountTier?> GetMaxTierByCreatedFromAsync(string createdFrom)
        {
            QueryDefinition query = new QueryDefinition("SELECT TOP 1 VALUE a.Tier FROM a WHERE a.CreatedFrom = @CreatedFrom ORDER BY a.Tier ASC")
                        .WithParameter("@CreatedFrom", createdFrom?.ToLower());
            using FeedIterator streamResultSet = this.Container.GetItemQueryStreamIterator(
                                            query,
                                            requestOptions: new QueryRequestOptions
                                            {
                                                MaxItemCount = 1,
                                                PartitionKey = this.pk
                                            });

            ResponseMessage response = await streamResultSet.ReadNextAsync();
            this.logger.LogInformation($"Cost: {response.Headers.RequestCharge} ({nameof(AccountDataAccess)}.{nameof(GetMaxTierByCreatedFromAsync)})");

            return response.IsSuccessStatusCode
                ? (await response.Content.ReadResultsAsync<AccountTier>()).FirstOrDefault()
                : default;
        }

        private async Task<int> GetCountByCreatedFromAsync(string createdFrom)
        {
            QueryDefinition query = new QueryDefinition("SELECT VALUE COUNT(1) FROM a WHERE a.CreatedFrom = @CreatedFrom")
                        .WithParameter("@CreatedFrom", createdFrom?.ToLower());
            using FeedIterator streamResultSet = this.Container.GetItemQueryStreamIterator(
                                            query,
                                            requestOptions: new QueryRequestOptions
                                            {
                                                MaxItemCount = 1,
                                                PartitionKey = this.pk
                                            });

            ResponseMessage response = await streamResultSet.ReadNextAsync();
            this.logger.LogInformation($"Cost: {response.Headers.RequestCharge} ({nameof(AccountDataAccess)}.{nameof(GetCountByCreatedFromAsync)})");

            return response.IsSuccessStatusCode
                ? (await response.Content.ReadResultsAsync<int>()).FirstOrDefault()
                : default;
        }

        public async Task<Account> CreateAsync(Account account)
        {
            if (await this.TryGetAsync(account.Name) != default) throw new ConflictException($"Account with name [{account.Name}] already exists.");
            if (await this.GetCountByCreatedFromAsync(account.CreatedFrom) >= TierLimits.AccountsPerCreatedFrom[await this.GetMaxTierByCreatedFromAsync(account.CreatedFrom) ?? AccountTier.Small]) throw new ConflictException($"Too many accounts already created from [{account.CreatedFrom}].");

            var accountModel = new AccountModel
            {
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedFrom = account.CreatedFrom.ToLower(),
                ModifiedAt = DateTimeOffset.UtcNow,
                Key = account.Key.ComputeSHA256(),
                Name = account.Name.ToLower(),
                Tier = AccountTier.Small,
                Roles = new()
            };
            this.logger.LogInformation($"Creating account: {accountModel.ToJsonString()}");

            ItemResponse<AccountModel> response = await this.Container.CreateItemAsync(accountModel, this.pk);
            this.logger.LogInformation($"Cost: {response.RequestCharge} ({nameof(AccountDataAccess)}.{nameof(CreateAsync)})");
            return response.Resource;
        }

        public async Task ValidateAsync(string name, string key)
        {
            Account account = await this.TryGetAsync(name);
            if (account == default) throw new BadRequestException($"Account with name [{name}] does not exist.");
            if (key?.ComputeSHA256() != account.Key) throw new UnauthorizedException($"Provided key did not match for account with name [{name}].");
        }
    }
}
