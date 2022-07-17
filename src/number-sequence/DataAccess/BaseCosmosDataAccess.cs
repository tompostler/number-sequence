using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace number_sequence.DataAccess
{
    public abstract class BaseCosmosDataAccess
    {
        private CosmosClient CosmosClient { get; set; }
        protected Container Container { get; private set; }

        public BaseCosmosDataAccess(IOptions<Options.CosmosDB> cosmosOptions)
        {
            this.CosmosClient = new CosmosClient(cosmosOptions.Value.Endpoint, cosmosOptions.Value.Key);
            this.Container = this.CosmosClient.GetContainer(cosmosOptions.Value.DatabaseId, cosmosOptions.Value.ContainerId);
        }
    }
}
