using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.Contracts
{
    /// <summary>
    /// A special override of <see cref="Count"/> that includes additional interpretations.
    /// </summary>
    public sealed class CountWithBases
    {
        /// <summary>
        /// See <see cref="Count.Name"/>
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// See <see cref="Count.Value"/>
        /// </summary>
        public ulong Value { get; set; }

        /// <summary>
        /// The baseX representation of the count value, where X is in (1,64) and is converted according to the
        /// <see cref="Unlimitedinf.Utilities.Extensions.NumericExtensions.ToBaseX(long, byte)"/> implementation.
        /// The array index corresponds to the X of the baseX.
        /// </summary>
        public string[] Bases { get; set; }

        /// <summary>
        /// Convert.
        /// </summary>
        public static CountWithBases From(Count count)
        {
            CountWithBases result = new()
            {
                Name = count.Name,
                Value = count.Value,
                Bases = new string[64],
            };

            for (byte i = 2; i < 64; i++)
            {
                result.Bases[i] = result.Value.ToBaseX(i);
            }

            return result;
        }
    }
}
