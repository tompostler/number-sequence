using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TcpWtf.NumberSequence.Client
{
    /// <summary>
    /// Random. Get random numbers or data.
    /// </summary>
    public sealed class RandomOperations
    {
        private readonly NsTcpWtfClient nsTcpWtfClient;

        internal RandomOperations(NsTcpWtfClient nsTcpWtfClient)
        {
            this.nsTcpWtfClient = nsTcpWtfClient;
        }

        /// <summary>
        /// Gets a random 8-ball response.
        /// </summary>
        public async Task<string> Get8BallAsync(CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "random/8ball"),
                cancellationToken,
                needsPreparation: false);
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        /// <summary>
        /// Gets a random value from a user-provided list of values.
        /// </summary>
        public async Task<string> GetFromAsync(CancellationToken cancellationToken = default, params string[] values)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "random/from?" + string.Join(';', values)),
                cancellationToken,
                needsPreparation: false);
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        /// <summary>
        /// Returns the shuffled user-provided list of values.
        /// </summary>
        public async Task<string[]> GetFromListAsync(CancellationToken cancellationToken = default, params string[] values)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "random/fromlist?" + string.Join(';', values)),
                cancellationToken,
                needsPreparation: false);
            return await response.Content.ReadJsonAsAsync<string[]>(cancellationToken);
        }

        /// <summary>
        /// Gets a random guid.
        /// </summary>
        public async Task<Guid> GetGuidAsync(CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "random/guid"),
                cancellationToken,
                needsPreparation: false);
            return Guid.ParseExact(await response.Content.ReadAsStringAsync(cancellationToken), "D");
        }

        /// <summary>
        /// Gets a random name based on the docker container naming generator.
        /// </summary>
        public async Task<string> GetNameAsync(CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "random/name"),
                cancellationToken,
                needsPreparation: false);
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        /// <summary>
        /// Gets a random long in the range [0,100]. Optionally adjust the max/min limits.
        /// </summary>
        /// <param name="min">Minimum ulong. Inclusive, and default is 0.</param>
        /// <param name="max">Maximum ulong. Exclusive, and default is 100.</param>
        /// <param name="cancellationToken"></param>
        public async Task<ulong> GetULongAsync(
            ulong min = 0,
            ulong max = 100,
            CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"random?min={min}&max={max}"),
                cancellationToken,
                needsPreparation: false);
            return await response.Content.ReadJsonAsAsync<ulong>(cancellationToken);
        }

        /// <summary>
        /// Gets a random bit (1 bit).
        /// </summary>
        public async Task<ulong> GetULong01Async(CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "random/bit"),
                cancellationToken,
                needsPreparation: false);
            return await response.Content.ReadJsonAsAsync<ulong>(cancellationToken);
        }

        /// <summary>
        /// Gets a random crumb (2 bits).
        /// </summary>
        public async Task<ulong> GetULong02Async(CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "random/crumb"),
                cancellationToken,
                needsPreparation: false);
            return await response.Content.ReadJsonAsAsync<ulong>(cancellationToken);
        }

        /// <summary>
        /// Gets a random nibble (4 bits).
        /// </summary>
        public async Task<ulong> GetULong04Async(CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "random/nibble"),
                cancellationToken,
                needsPreparation: false);
            return await response.Content.ReadJsonAsAsync<ulong>(cancellationToken);
        }

        /// <summary>
        /// Gets a random byte (8 bits, 1 byte).
        /// </summary>
        public async Task<ulong> GetULong08Async(CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "random/byte"),
                cancellationToken,
                needsPreparation: false);
            return await response.Content.ReadJsonAsAsync<ulong>(cancellationToken);
        }

        /// <summary>
        /// Gets a random short (16 bits, 2 bytes).
        /// </summary>
        public async Task<ulong> GetULong16Async(CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "random/short"),
                cancellationToken,
                needsPreparation: false);
            return await response.Content.ReadJsonAsAsync<ulong>(cancellationToken);
        }

        /// <summary>
        /// Gets a random int (32 bits, 4 bytes).
        /// </summary>
        public async Task<ulong> GetULong32Async(CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "random/int"),
                cancellationToken,
                needsPreparation: false);
            return await response.Content.ReadJsonAsAsync<ulong>(cancellationToken);
        }

        /// <summary>
        /// Gets a random long (64 bits, 8 bytes).
        /// </summary>
        public async Task<ulong> GetULong64Async(CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "random/long"),
                cancellationToken,
                needsPreparation: false);
            return await response.Content.ReadJsonAsAsync<ulong>(cancellationToken);
        }

        /// <summary>
        /// Gets a specified number of bits.
        /// </summary>
        public async Task<ulong> GetULongBitsAsync(byte numberOfBits, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"random/bits/{numberOfBits}"),
                cancellationToken,
                needsPreparation: false);
            return await response.Content.ReadJsonAsAsync<ulong>(cancellationToken);
        }
    }
}
