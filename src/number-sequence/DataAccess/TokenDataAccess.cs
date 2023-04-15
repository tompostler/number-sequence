﻿using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
using Unlimitedinf.Utilities.Extensions;

namespace number_sequence.DataAccess
{
    public sealed class TokenDataAccess : BaseCosmosDataAccess
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<TokenDataAccess> logger;

        public TokenDataAccess(
            IServiceProvider serviceProvider,
            IOptions<Options.CosmosDB> cosmosOptions,
            ILogger<TokenDataAccess> logger)
            : base(cosmosOptions)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        private PartitionKey MakePK(string account) => new($"{nameof(Token)}|{account}".ToLower());

        public async Task<Token> TryGetAsync(string account, string name)
        {
            try
            {
                ItemResponse<TokenModel> response = await this.Container.ReadItemAsync<TokenModel>(name?.ToLower(), this.MakePK(account));
                this.logger.LogInformation($"Cost: {response.RequestCharge} ({nameof(TokenDataAccess)}.{nameof(TryGetAsync)})");
                return response.Resource;
            }
            catch (CosmosException ce) when (ce.StatusCode == HttpStatusCode.NotFound)
            {
                return default;
            }
        }

        private async Task<int> GetCountByAccountAsync(string account)
        {
            var query = new QueryDefinition("SELECT VALUE COUNT(1) FROM t");
            using FeedIterator streamResultSet = this.Container.GetItemQueryStreamIterator(
                                            query,
                                            requestOptions: new QueryRequestOptions
                                            {
                                                MaxItemCount = 1,
                                                PartitionKey = this.MakePK(account)
                                            });

            ResponseMessage response = await streamResultSet.ReadNextAsync();
            this.logger.LogInformation($"Cost: {response.Headers.RequestCharge} ({nameof(TokenDataAccess)}.{nameof(GetCountByAccountAsync)})");

            return response.IsSuccessStatusCode
                ? (await response.Content.ReadResultsAsync<int>()).FirstOrDefault()
                : default;
        }

        public async Task<Token> CreateAsync(Token token)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            if (await this.TryGetAsync(token.Account, token.Name) != default)
            {
                throw new ConflictException($"Token with name [{token.Name}] already exists.");
            }

            Account account = await nsContext.Accounts.SingleAsync(x => x.Name == token.Account);
            if (await this.GetCountByAccountAsync(token.Account) >= TierLimits.TokensPerAccount[account.Tier])
            {
                throw new ConflictException($"Too many tokens already created for account with name [{token.Account}].");
            }

            var tokenModel = new TokenModel
            {
                Account = token.Account.ToLower(),
                AccountTier = account.Tier,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = token.ExpiresAt.ToUniversalTime(),
                ModifiedAt = DateTimeOffset.UtcNow,
                Name = token.Name.ToLower(),
                Value = default
            };
            this.logger.LogInformation($"Creating token: {tokenModel.ToJsonString()}");
            tokenModel.Value = TokenValue.CreateFrom(tokenModel).ToBase64JsonString();

            ItemResponse<TokenModel> response = await this.Container.CreateItemAsync(tokenModel, this.MakePK(token.Account));
            this.logger.LogInformation($"Cost: {response.RequestCharge} ({nameof(TokenDataAccess)}.{nameof(CreateAsync)})");
            return response.Resource;
        }
    }
}
