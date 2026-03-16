using Microsoft.AspNetCore.Mvc;

namespace number_sequence.Controllers
{
    public sealed partial class RandomController
    {
        // Fetched 2025-03-26 from https://discover.hubpages.com/entertainment/ncis-jethro-gibbs-rules
        #region Gibbs

        private static readonly IReadOnlyDictionary<string, string> gibbs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["1"] = "Rule #1: \"Never let suspects stay together.\"",
            ["1a"] = "Alternate Rule #1: \"Never screw over your partner.\"",

            ["2"] = "Rule #2: \"Always wear gloves at a crime scene.\"",

            ["3"] = "Rule #3: \"Don't believe what you are told; double-check.\"",
            ["3a"] = "Alternate Rule #3: \"Never be unreachable.\"",

            ["4"] = "Rule #4: \"The best way to keep a secret, keep it to yourself.\"",
            ["4c1"] = "Corollary 1 to Rule #4: \"The 2nd best way to keep a secret, tell one other person, if you must.\"",
            ["4c2"] = "Corollary 2 to Rule #4: \"There is no third best.\"",

            ["5"] = "Rule #5: \"You don't waste good.\"",
            ["6"] = "Rule #6: \"Never say you're sorry.\"",
            ["7"] = "Rule #7: \"Always be specific when you lie.\"",
            ["8"] = "Rule #8: \"Never take anything for granted.\"",
            ["9"] = "Rule #9: \"Never go anywhere without a knife.\"",
            ["10"] = "Rule #10: \"Never get personally involved in a case.\"",
            ["11"] = "Rule #11: \"When the job is done, walk away.\"",
            ["12"] = "Rule #12: \"Never date a co-worker.\"",
            ["13"] = "Rule #13: \"Never ever involve lawyers.\"",
            ["14"] = "Rule #14: \"Bend the line, don't break it.\"",
            ["15"] = "Rule #15: \"Always work as a team.\"",
            ["16"] = "Rule #16: \"If someone thinks they have the upper hand, break it.\"",

            ["18"] = "Rule #18: \"It's better to seek forgiveness than ask permission.\"",

            ["20"] = "Rule #20: \"Always look under.\"",

            ["22"] = "Rule #22: \"Never ever bother Gibbs in interrogation.\"",
            ["23"] = "Rule #23: \"Never mess with a marine's coffee if you want to live.\"",

            ["27"] = "Rule #27: \"Two ways to follow: First way they never notice you, the second way, they ONLY notice you.\"",
            ["28"] = "Rule #28: \"When you need help, ask.\"",
            ["29"] = "Rule #29: \"Learn to Obey before you Command.\"",

            ["35"] = "Rule #35: \"Always watch the watchers.\"",
            ["36"] = "Rule #36: \"If you feel like you're being played, you probably are.\"",

            ["38"] = "Rule #38: \"Your case, your lead\"",

            ["39"] = "Rule #39: \"There is no such thing as coincidences.\"",
            ["39a"] = "Alternate Rule #39: \"There is no such thing as a small world.\"",

            ["40"] = "Rule #40: \"If it seems like someone is out to get you, they are.\"",

            ["42"] = "Rule #42: \"Never accept an apology from someone who has just sucker punched you.\"",

            ["44"] = "Rule #44: \"First things first, hide the women and children.\"",
            ["45"] = "Rule #45: \"Clean up your own messes.\"",

            ["51"] = "Rule #51: \"Sometimes you're wrong.\"",

            ["62"] = "Rule #62: \"Always give people space when they get off the elevator.\"",

            ["69"] = "Rule #69: \"Never trust a woman who doesn't trust her man.\"",

            ["73"] = "Rule #73: \"Never meet your heroes.\"",

            ["91"] = "Rule #91: \"When you decide to walk away, don't look back.\"",
        };
        private static readonly string[] gibbsKeys = gibbs.Keys.ToArray();

        #endregion // Gibbs

        [HttpGet("gibbs")]
        public IActionResult Gibbs([FromQuery] string rule = default)
        {
            if (rule != default)
            {
                if (gibbs.TryGetValue(rule, out string ruleValue))
                {
                    return this.Ok(ruleValue);
                }
                else
                {
                    return this.BadRequest($"Unknown rule '{rule}'.");
                }
            }

            return this.Ok(gibbs[gibbsKeys[Random.Shared.Next(gibbsKeys.Length)]]);
        }
    }
}
