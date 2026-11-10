using System.Text;

namespace TcpWtf.NumberSequence.Contracts
{
    /// <summary>
    /// The result of rolling a dice expression such as <c>4d6+2d4-1</c>.
    /// </summary>
    public sealed class DiceRoll
    {
        /// <summary>
        /// The expression as it was understood, normalized (e.g. <c>d20</c> becomes <c>1d20</c>, whitespace removed).
        /// </summary>
        public string Expression { get; set; }

        /// <summary>
        /// The sum of every term.
        /// </summary>
        public long Total { get; set; }

        /// <summary>
        /// Each term of the expression, in order.
        /// </summary>
        public List<DiceRollTerm> Terms { get; set; }

        /// <summary>
        /// A one-line breakdown, e.g. <c>4d6 [3, 5, 1, 6] + 2d4 [2, 4] - 1 = 20</c>.
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new();
            for (int i = 0; i < this.Terms.Count; i++)
            {
                DiceRollTerm term = this.Terms[i];
                if (i > 0)
                {
                    _ = sb.Append(term.Subtract ? " - " : " + ");
                }
                else if (term.Subtract)
                {
                    _ = sb.Append('-');
                }

                _ = sb.Append(term.Notation);
                if (term.Sides.HasValue)
                {
                    _ = sb.Append(" [").AppendJoin(", ", term.Rolls).Append(']');
                }
            }

            return sb.Append(" = ").Append(this.Total).ToString();
        }
    }

    /// <summary>
    /// One term of a <see cref="DiceRoll"/>: either a group of identical dice (<c>4d6</c>) or a constant (<c>2</c>).
    /// </summary>
    public sealed class DiceRollTerm
    {
        /// <summary>
        /// The term without its sign, normalized (<c>4d6</c>, <c>1d20</c>, <c>2</c>).
        /// </summary>
        public string Notation { get; set; }

        /// <summary>
        /// Whether the term is subtracted from the total rather than added.
        /// </summary>
        public bool Subtract { get; set; }

        /// <summary>
        /// How many dice were rolled. Null for a constant.
        /// </summary>
        public int? Count { get; set; }

        /// <summary>
        /// How many sides each die has. Null for a constant.
        /// </summary>
        public int? Sides { get; set; }

        /// <summary>
        /// Each die's result, in the order rolled. Empty for a constant.
        /// </summary>
        public int[] Rolls { get; set; }

        /// <summary>
        /// The term's contribution to the total, with its sign applied.
        /// </summary>
        public long Value { get; set; }
    }
}
