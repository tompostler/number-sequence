﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Cryptography;

namespace number_sequence.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class RandomController : ControllerBase
    {
        private static readonly RNGCryptoServiceProvider _rngCsp = new RNGCryptoServiceProvider();

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

            ulong val = Generate(64);

            val %= max - min + 1;
            val += min;

            return this.Ok(val);
        }

        [HttpGet("bit")]
        public IActionResult Bit()
        {
            return this.Ok(Generate(1));
        }

        [HttpGet("crumb")]
        public IActionResult Crumb()
        {
            return this.Ok(Generate(2));
        }

        [HttpGet("nibble")]
        public IActionResult Nibble()
        {
            return this.Ok(Generate(4));
        }

        [HttpGet("byte")]
        public IActionResult Byte()
        {
            return this.Ok(Generate(8));
        }

        [HttpGet("short")]
        public IActionResult Short()
        {
            return this.Ok(Generate(16));
        }

        [HttpGet("int")]
        public IActionResult Int()
        {
            return this.Ok(Generate(32));
        }

        [HttpGet("long")]
        public IActionResult Long()
        {
            return this.Ok(Generate(64));
        }

        public static readonly string[] EightBallResponses = new[]
        {
            // A standard Magic 8 Ball is capable of 10 affirmative answers (✔), 5 non-committal answers (〰), and 5 negative answers (❌).
            "✔ It is certain.",         "✔ As I see it, yes.",      "〰 Reply hazy, try again.",         "❌ Don't count on it.",
            "✔ It is decidedly so.",    "✔ Most likely.",           "〰 Ask again later.",               "❌ My reply is no.",
            "✔ Without a doubt.",       "✔ Outlook good.",          "〰 Better not tell you now.",       "❌ My sources say no.",
            "✔ Yes - definitely.",      "✔ Yes.",                   "〰 Cannot predict now.",            "❌ Outlook not so good.",
            "✔ You may rely on it.",    "✔ Signs point to yes.",    "〰 Concentrate and ask again.",     "❌ Very doubtful.",
        };
        [HttpGet("8ball")]
        public IActionResult EightBall()
        {
            return this.Ok(EightBallResponses[(int)Generate(8) % EightBallResponses.Length]);
        }

        [HttpGet("bits/{num}")]
        public IActionResult Bits(byte num)
        {
            if (num <= 0 || num > 64)
            {
                return this.BadRequest("num must be in (0,64]");
            }

            return this.Ok(Generate(num));
        }

        [HttpGet("guid")]
        public IActionResult GuidMethod()
        {
            return this.Ok(Guid.NewGuid().ToString("D"));
        }

        private static readonly ulong[] bitmaps = new ulong[]
        {
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
        };

        private static ulong Generate(byte bits)
        {
            var bytes = new byte[8];
            _rngCsp.GetBytes(bytes);
            ulong val = 0;
            for (int i = 0; i < 8; i++)
            {
                val += (ulong)(bytes[i] << (i * 8));
            }

            return val & bitmaps[bits];
        }
    }
}