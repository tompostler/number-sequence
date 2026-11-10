using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Utilities
{
    /// <summary>
    /// Parses and rolls standard dice notation: terms of <c>NdS</c> (N defaults to 1) or plain integers, joined by
    /// <c>+</c> or <c>-</c>. Whitespace between tokens is ignored. See docs/random-dice.md.
    /// </summary>
    public static class DiceNotation
    {
        public const int MaxExpressionLength = 200;
        public const int MaxTerms = 20;
        public const int MaxTotalDice = 1_000;
        public const int MaxSides = 1_000_000;
        public const int MaxConstant = 1_000_000;

        private sealed record Term(bool Subtract, int? Count, int? Sides, int Constant);

        /// <summary>
        /// Parses and rolls <paramref name="expression"/>. Returns false with a user-facing <paramref name="error"/> when
        /// the expression is malformed or exceeds a limit; nothing is rolled in that case.
        /// </summary>
        public static bool TryRoll(string expression, Random random, out DiceRoll roll, out string error)
        {
            roll = default;
            if (!TryParse(expression, out List<Term> terms, out error))
            {
                return false;
            }

            List<DiceRollTerm> rolled = new(terms.Count);
            long total = 0;
            foreach (Term term in terms)
            {
                DiceRollTerm result;
                if (term.Sides.HasValue)
                {
                    int[] rolls = new int[term.Count.Value];
                    for (int i = 0; i < rolls.Length; i++)
                    {
                        rolls[i] = random.Next(1, term.Sides.Value + 1);
                    }

                    long sum = rolls.Sum(r => (long)r);
                    result = new()
                    {
                        Notation = $"{term.Count}d{term.Sides}",
                        Subtract = term.Subtract,
                        Count = term.Count,
                        Sides = term.Sides,
                        Rolls = rolls,
                        Value = term.Subtract ? -sum : sum,
                    };
                }
                else
                {
                    result = new()
                    {
                        Notation = term.Constant.ToString(),
                        Subtract = term.Subtract,
                        Rolls = [],
                        Value = term.Subtract ? -term.Constant : term.Constant,
                    };
                }

                rolled.Add(result);
                total += result.Value;
            }

            roll = new()
            {
                Expression = string.Concat(rolled.Select((t, i) => (t.Subtract ? "-" : i > 0 ? "+" : string.Empty) + t.Notation)),
                Total = total,
                Terms = rolled,
            };
            return true;
        }

        private static bool TryParse(string expression, out List<Term> terms, out string error)
        {
            terms = [];
            error = default;

            if (string.IsNullOrWhiteSpace(expression))
            {
                error = "Expression is empty.";
                return false;
            }
            if (expression.Length > MaxExpressionLength)
            {
                error = $"Expression must be at most {MaxExpressionLength} characters.";
                return false;
            }

            int pos = 0;
            int totalDice = 0;
            while (true)
            {
                SkipWhitespace(expression, ref pos);

                // A sign is required between terms and optional before the first.
                bool subtract = false;
                if (pos < expression.Length && (expression[pos] == '+' || expression[pos] == '-'))
                {
                    subtract = expression[pos] == '-';
                    pos++;
                    SkipWhitespace(expression, ref pos);
                }
                else if (terms.Count > 0)
                {
                    error = $"Expected '+' or '-' at position {pos + 1}.";
                    return false;
                }

                int termStart = pos;
                if (!TryReadNumber(expression, ref pos, out int? leading, out error))
                {
                    return false;
                }

                if (pos < expression.Length && (expression[pos] == 'd' || expression[pos] == 'D'))
                {
                    pos++;
                    if (!TryReadNumber(expression, ref pos, out int? sides, out error))
                    {
                        return false;
                    }
                    if (!sides.HasValue)
                    {
                        error = $"Expected the number of sides after 'd' at position {pos + 1}.";
                        return false;
                    }

                    int count = leading ?? 1;
                    if (count < 1)
                    {
                        error = $"'{expression[termStart..pos]}' must roll at least one die.";
                        return false;
                    }
                    if (sides < 1 || sides > MaxSides)
                    {
                        error = $"'{expression[termStart..pos]}' must have between 1 and {MaxSides:N0} sides.";
                        return false;
                    }

                    totalDice += count;
                    if (totalDice > MaxTotalDice)
                    {
                        error = $"At most {MaxTotalDice:N0} dice can be rolled at once.";
                        return false;
                    }

                    terms.Add(new(subtract, count, sides, 0));
                }
                else if (leading.HasValue)
                {
                    if (leading > MaxConstant)
                    {
                        error = $"Constant '{leading}' must be at most {MaxConstant:N0}.";
                        return false;
                    }

                    terms.Add(new(subtract, null, null, leading.Value));
                }
                else
                {
                    error = pos < expression.Length
                        ? $"Unexpected '{expression[pos]}' at position {pos + 1}."
                        : "Expression ends with an operator.";
                    return false;
                }

                if (terms.Count > MaxTerms)
                {
                    error = $"Expression must have at most {MaxTerms} terms.";
                    return false;
                }

                SkipWhitespace(expression, ref pos);
                if (pos >= expression.Length)
                {
                    return true;
                }
            }
        }

        private static void SkipWhitespace(string s, ref int pos)
        {
            while (pos < s.Length && char.IsWhiteSpace(s[pos]))
            {
                pos++;
            }
        }

        private static bool TryReadNumber(string s, ref int pos, out int? value, out string error)
        {
            value = default;
            error = default;

            int start = pos;
            while (pos < s.Length && char.IsAsciiDigit(s[pos]))
            {
                pos++;
            }
            if (pos == start)
            {
                return true;
            }

            // Anything past seven digits is over every limit; reject it here rather than overflow an int.
            if (pos - start > 7 || !int.TryParse(s.AsSpan(start, pos - start), out int parsed))
            {
                error = $"Number '{s[start..pos]}' is too large.";
                return false;
            }

            value = parsed;
            return true;
        }
    }
}
