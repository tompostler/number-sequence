using TcpWtf.NumberSequence.Contracts;

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
        /// Gets a random name of the union of moby and ubuntu naming conventions normalized to lower case with no spaces.
        /// </summary>
        public async Task<string> GetNameAsync(int? seed = default, CancellationToken cancellationToken = default)
        {
            string queryString = seed.HasValue ? $"?seed={seed}" : string.Empty;

            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "random/name"+ queryString),
                cancellationToken,
                needsPreparation: false);
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        /// <summary>
        /// Gets a random name from the moby (docker) naming conventions.
        /// </summary>
        public async Task<string> GetNameMobyAsync(int? seed = default, CancellationToken cancellationToken = default)
        {
            string queryString = seed.HasValue ? $"?seed={seed}" : string.Empty;

            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "random/name/moby" + queryString),
                cancellationToken,
                needsPreparation: false);
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        /// <summary>
        /// Gets a random name from the ubuntu naming conventions.
        /// </summary>
        public async Task<string> GetNameUbuntuAsync(int? seed = default, CancellationToken cancellationToken = default)
        {
            string queryString = seed.HasValue ? $"?seed={seed}" : string.Empty;

            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "random/name/ubuntu" + queryString),
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

        /// <summary>
        /// Gets a random Wheel of Time book intro.
        /// </summary>
        public async Task<string> GetWheelOfTimeIntroAsync(int? book = default, CancellationToken cancellationToken = default)
        {
            string queryString = book.HasValue ? $"?book={book}" : string.Empty;

            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "random/wot" + queryString),
                cancellationToken,
                needsPreparation: false);
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        /// <summary>
        /// Gets Heads or Tails.
        /// </summary>
        public async Task<string> GetCoinFlipAsync(CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "random/coin"),
                cancellationToken,
                needsPreparation: false);
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        /// <summary>
        /// Gets a random philosophical razor.
        /// </summary>
        public async Task<Razor> GetPhilosophicalRazorAsync(string name = default, CancellationToken cancellationToken = default)
        {
            string queryString = !string.IsNullOrEmpty(name) ? $"?name={name}" : string.Empty;

            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "random/razor" + queryString),
                cancellationToken,
                needsPreparation: false);
            return await response.Content.ReadJsonAsAsync<Razor>(cancellationToken);
        }

        /// <summary>
        /// Gets a random Gibbs rule (from the show NCIS).
        /// </summary>
        public async Task<string> GetGibbsRuleAsync(string rule = default, CancellationToken cancellationToken = default)
        {
            string queryString = !string.IsNullOrEmpty(rule) ? $"?rule={rule}" : string.Empty;

            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "random/gibbs" + queryString),
                cancellationToken,
                needsPreparation: false);
            return await response.Content.ReadJsonAsAsync<string>(cancellationToken);
        }

        /// <summary>
        /// Gets a random number according to xkcd comic 221.
        /// </summary>
        public async Task<int> GetXkcdAsync(CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "random/xkcd"),
                cancellationToken,
                needsPreparation: false);
            return await response.Content.ReadJsonAsAsync<int>(cancellationToken);
        }

        /// <summary>
        /// Gets a random 'no' excuse.
        /// </summary>
        public async Task<string> GetNoAsync(int? index = default, CancellationToken cancellationToken = default)
        {
            string queryString = index.HasValue ? $"?index={index}" : string.Empty;

            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "random/no" + queryString),
                cancellationToken,
                needsPreparation: false);
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
    }
}
