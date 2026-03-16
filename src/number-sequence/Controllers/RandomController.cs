using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Web;
using Unlimitedinf.Utilities;

namespace number_sequence.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed partial class RandomController : ControllerBase
    {
        [HttpGet]
        public IActionResult Default(ulong min = 0, ulong max = 100)
        {
            // Standard inclusive lower and upper bounds. Defaults to [0,100]
            if (min > max)
            {
                return this.BadRequest("max must be greater than min");
            }
            else if (min == max)
            {
                return this.Ok(min);
            }

            ulong val = (ulong)Random.Shared.NextInt64();

            val %= max - min + 1;
            val += min;

            return this.Ok(val);
        }

        [HttpGet("bit")]
        public IActionResult Bit() => this.Ok(Random.Shared.NextInt64() % (1 << 1));

        [HttpGet("crumb")]
        public IActionResult Crumb() => this.Ok(Random.Shared.NextInt64() % (1 << 2));

        [HttpGet("nibble")]
        public IActionResult Nibble() => this.Ok(Random.Shared.NextInt64() % (1 << 4));

        [HttpGet("byte")]
        public IActionResult Byte() => this.Ok(Random.Shared.NextInt64() % byte.MaxValue);

        [HttpGet("short")]
        public IActionResult Short() => this.Ok(Random.Shared.NextInt64() % ushort.MaxValue);

        [HttpGet("int")]
        public IActionResult Int() => this.Ok(Random.Shared.NextInt64() % uint.MaxValue);

        [HttpGet("long")]
        public IActionResult Long() => this.Ok((ulong)Random.Shared.NextInt64());

        public static readonly string[] EightBallResponses =
        [
            // A standard Magic 8 Ball is capable of 10 affirmative answers (Y), 5 non-committal answers (~), and 5 negative answers (X).
            "[Y] It is certain.",         "[Y] As I see it, yes.",      "[~] Reply hazy, try again.",         "[X] Don't count on it.",
            "[Y] It is decidedly so.",    "[Y] Most likely.",           "[~] Ask again later.",               "[X] My reply is no.",
            "[Y] Without a doubt.",       "[Y] Outlook good.",          "[~] Better not tell you now.",       "[X] My sources say no.",
            "[Y] Yes - definitely.",      "[Y] Yes.",                   "[~] Cannot predict now.",            "[X] Outlook not so good.",
            "[Y] You may rely on it.",    "[Y] Signs point to yes.",    "[~] Concentrate and ask again.",     "[X] Very doubtful.",
        ];
        [HttpGet("8ball")]
        public IActionResult EightBall() => this.Ok(EightBallResponses[Random.Shared.Next(EightBallResponses.Length)]);

        private static readonly ulong[] bitmaps =
        [
            0x0000000000000000,
            0x0000000000000001, 0x0000000000000003, 0x0000000000000007, 0x000000000000000F,
            0x000000000000001F, 0x000000000000003F, 0x000000000000007F, 0x00000000000000FF,
            0x00000000000001FF, 0x00000000000003FF, 0x00000000000007FF, 0x0000000000000FFF,
            0x0000000000001FFF, 0x0000000000003FFF, 0x0000000000007FFF, 0x000000000000FFFF,
            0x000000000001FFFF, 0x000000000003FFFF, 0x000000000007FFFF, 0x00000000000FFFFF,
            0x00000000001FFFFF, 0x00000000003FFFFF, 0x00000000007FFFFF, 0x0000000000FFFFFF,
            0x0000000001FFFFFF, 0x0000000003FFFFFF, 0x0000000007FFFFFF, 0x000000000FFFFFFF,
            0x000000001FFFFFFF, 0x000000003FFFFFFF, 0x000000007FFFFFFF, 0x00000000FFFFFFFF,
            0x00000001FFFFFFFF, 0x00000003FFFFFFFF, 0x00000007FFFFFFFF, 0x0000000FFFFFFFFF,
            0x0000001FFFFFFFFF, 0x0000003FFFFFFFFF, 0x0000007FFFFFFFFF, 0x000000FFFFFFFFFF,
            0x000001FFFFFFFFFF, 0x000003FFFFFFFFFF, 0x000007FFFFFFFFFF, 0x00000FFFFFFFFFFF,
            0x00001FFFFFFFFFFF, 0x00003FFFFFFFFFFF, 0x00007FFFFFFFFFFF, 0x0000FFFFFFFFFFFF,
            0x0001FFFFFFFFFFFF, 0x0003FFFFFFFFFFFF, 0x0007FFFFFFFFFFFF, 0x000FFFFFFFFFFFFF,
            0x001FFFFFFFFFFFFF, 0x003FFFFFFFFFFFFF, 0x007FFFFFFFFFFFFF, 0x00FFFFFFFFFFFFFF,
            0x01FFFFFFFFFFFFFF, 0x03FFFFFFFFFFFFFF, 0x07FFFFFFFFFFFFFF, 0x0FFFFFFFFFFFFFFF,
            0x1FFFFFFFFFFFFFFF, 0x3FFFFFFFFFFFFFFF, 0x7FFFFFFFFFFFFFFF, 0xFFFFFFFFFFFFFFFF
        ];

        [HttpGet("bits/{num}")]
        public IActionResult Bits(byte num)
        {
            if (num <= 0 || num > 64)
            {
                return this.BadRequest("num must be in (0,64]");
            }

            return this.Ok((ulong)Random.Shared.NextInt64() & bitmaps[num]);
        }

        [HttpGet("guid")]
        public IActionResult GuidMethod() => this.Ok(Guid.NewGuid().ToString("D"));

        private static readonly IEnumerable<string> nameAdjectives = mobyNameAdjectives.Union(ubuntuNameAdjectives);
        private static readonly IEnumerable<string> nameNames = mobyNameNames.Union(ubuntuNameAnimals);

        [HttpGet("name")]
        public IActionResult Name([FromQuery] int? seed = default)
        {
            if (seed < 0)
            {
                seed = Math.Abs(seed.Value);
            }
            else if (seed == default)
            {
                seed = Random.Shared.Next();
            }

            return this.Ok((nameAdjectives.ElementAt(seed.Value % nameAdjectives.Count()) + '_' + nameNames.ElementAt(seed.Value % nameNames.Count())).Replace(' ', '-').Replace("'", string.Empty).ToLower());
        }

        private static readonly char[] fromSeparators = new[] { ';', ',' };

        [HttpGet("from")]
        public IActionResult From()
        {
            string[] fromValues = this.HttpContext.Request.QueryString.Value?.TrimStart('?').Split(fromSeparators, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

            // If there's no values provided, give it a docker name
            if (fromValues.Length == 0)
            {
                return this.Name();
            }

            return this.Ok(HttpUtility.UrlDecode(fromValues[Random.Shared.Next(fromValues.Length)]));
        }

        [HttpGet("fromlist")]
        public IActionResult FromList()
        {
            string[] fromValues = this.HttpContext.Request.QueryString.Value?.TrimStart('?').Split(fromSeparators, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

            for (int i = 0; i < fromValues.Length; i++)
            {
                fromValues[i] = HttpUtility.UrlDecode(fromValues[i]);
            }

            fromValues = [.. Enumerable.Shuffle(fromValues)];
            return this.Ok(fromValues);
        }

        [HttpGet("coin")]
        public IActionResult Coin()
        {
            return this.Ok(Random.Shared.Next() % 2 == 0
                            ? "Heads"
                            : "Tails");
        }

        // https://xkcd.com/221/
        [HttpGet("xkcd")]
        public IActionResult Xkcd() => this.Ok(4);

        private static readonly string[] nos = JsonSerializer.Deserialize<string[]>(Resources.NoAsAServiceJson);


        [HttpGet("no")]
        public IActionResult No([FromQuery] int? index = default)
        {
            if (index.HasValue && (index <= 0 || index > nos.Length))
            {
                return this.BadRequest($"If supplying index, it must be in [1,{nos.Length}]. You supplied '{index}'.");
            }
            else if (!index.HasValue)
            {
                index = Random.Shared.Next(nos.Length) + 1;
            }

            return this.Ok(nos[index.Value - 1]);
        }

        [HttpGet("identicon")]
        public IActionResult Identicon([FromQuery] string value = default, [FromQuery] string format = "png", [FromQuery] int size = 128)
        {
            if (format != "png" && format != "svg")
            {
                return this.BadRequest("format must be 'png' or 'svg'.");
            }
            if (format == "png" && size > 256)
            {
                return this.BadRequest("size must be at most 256 for png.");
            }
            value ??= Guid.NewGuid().ToString();
            IdenticonGenerator generator = new();
            return format == "svg"
                ? this.Content(generator.GenerateSvg(value, size), "image/svg+xml")
                : this.File(generator.GeneratePng(value, size), "image/png");
        }
    }
}
