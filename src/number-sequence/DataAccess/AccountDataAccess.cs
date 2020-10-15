using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using System.Net;
using System.Threading.Tasks;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.DataAccess
{
    public sealed class AccountDataAccess : BaseDataAccess
    {
        private readonly PartitionKey pk;

        public AccountDataAccess(IOptions<Options.CosmosDB> cosmosOptions)
            : base(cosmosOptions)
        {
            this.pk = new PartitionKey(nameof(Account));
        }

        public async Task<Account> GetAsync(string name)
        {
            try
            {
                return await this.Container.ReadItemAsync<Account>(name, this.pk);
            }
            catch (CosmosException ce) when (ce.StatusCode == HttpStatusCode.NotFound)
            {
                return default;
            }
        }

        public async Task CreateAsync(Account account)
        {

        }
    }
}
