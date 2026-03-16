using Microsoft.AspNetCore.Mvc;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Controllers
{
    public sealed partial class RandomController
    {
        // Initialized 2025-03-20
        #region Razors

        private static readonly Razor[] razors =
        [
            new()
            {
                Name = "Alder's razor",
                Alias = "Newton's flaming laser sword",
                Value = "If something cannot be settled by experiment or observation, then it is not worthy of debate.",
                Reference = "https://en.wikipedia.org/wiki/Mike_Alder#Newton's_flaming_laser_sword",
            },
            new()
            {
                Name = "Grice's razor",
                Alias = "Guillaume's razor",
                Value = "As a principle of parsimony, conversational implicatures are to be preferred over semantic context for linguistic explanations.",
                Reference = "https://en.wikipedia.org/wiki/Paul_Grice",
            },
            new()
            {
                Name = "Hanlon's razor",
                Value = "Never attribute to malice that which is adequately explained by stupidity.",
                Reference = "https://en.wikipedia.org/wiki/Hanlon%27s_razor",
            },
            new()
            {
                Name = "Hitchens's razor",
                Value = "That which can be asserted without evidence can be dismissed without evidence.",
                Reference = "https://en.wikipedia.org/wiki/Hitchens%27s_razor",
            },
            new()
            {
                Name = "Hume's razor",
                Value = "If the cause, assigned for any effect, be not sufficient to produce it, we must either reject that cause, or add to it such qualities as will give it a just proportion to the effect.",
                Reference = "https://en.wikipedia.org/wiki/Hume%27s_guillotine",
            },
            new()
            {
                Name = "Occam's razor",
                Value = "Explanations which require fewer unjustified assumptions are more likely to be correct; avoid unnecessary or improbable assumptions.",
                Reference = "https://en.wikipedia.org/wiki/Occam%27s_razor",
            },
            new()
            {
                Name = "Popper's falsifiability criterion",
                Value = "For a theory to be considered scientific, it must be falsifiable.",
                Reference = "https://en.wikipedia.org/wiki/Karl_Popper",
            },
            new()
            {
                Name = "Sagan standard",
                Value = "Positive claims require positive evidence, extraordinary claims require extraordinary evidence.",
                Reference = "https://en.wikipedia.org/wiki/Sagan_standard",
            },
        ];
        private static readonly Dictionary<string, Razor> razorsIndex = razors.ToDictionary(r => r.Name.Split("'")[0].ToLowerInvariant());

        #endregion // Razors

        [HttpGet("razor")]
        public IActionResult Razor([FromQuery] string name = default)
        {
            if (name != default)
            {
                if (razorsIndex.TryGetValue(name.ToLowerInvariant(), out Razor razor))
                {
                    return this.Ok(razor);
                }
                else
                {
                    return this.BadRequest($"Unknown razor '{name}'.");
                }
            }

            return this.Ok(razors[Random.Shared.Next(razors.Length)]);
        }
    }
}
