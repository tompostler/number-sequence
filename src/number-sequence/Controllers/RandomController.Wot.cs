using Microsoft.AspNetCore.Mvc;

namespace number_sequence.Controllers
{
    public sealed partial class RandomController
    {
        // Fetched 2023-06-09
        #region Wot Quotes

        private static readonly string[] wotQuotes =
        [
            "The Wheel of Time turns, and Ages come and pass, leaving memories that become legend. Legend fades to myth, and even myth is long forgotten when the Age that gave it birth comes again. " +
            "In one Age, called the Third Age by some, an Age yet to come, an Age long past, a wind rose in the Mountains of Mist. " +
            "The wind was not the beginning. There are neither beginnings nor endings to the turning of the Wheel of Time. But it was A beginning.\n" +
            "~ Robert Jordan, The Eye of the World, The Wheel of Time #1",

            "The Wheel of Time turns, and Ages come and pass leaving memories that become legend, then fade to myth, and are long forgot when that Age comes again. " +
            "In one Age, called the Third Age by some, an Age yet to come, an Age long past, a wind rose in the Mountains of Dhoom. " +
            "The wind was not the beginning. There are neither beginnings nor endings to the turning of the Wheel of Time. But it was a beginning.\n" +
            "~ Robert Jordan, The Great Hunt, The Wheel of Time #2",

            "The Wheel of Time turns, and Ages come and pass, leaving memories that become legend. Legend fades to myth, and even myth is long forgotten when the Age that gave it birth comes again. " +
            "In one Age, called the Third Age by some, an Age yet to come, an Age long past, a wind rose in the Mountains of Mist. " +
            "The wind was not the beginning. There are neither beginnings nor endings to the turning of the Wheel of Time. But it was A beginning.\n" +
            "~ Robert Jordan, The Dragon Reborn, The Wheel of Time #3",

            "The Wheel of Time turns, and Ages come and pass, leaving memories that become legend. Legend fades to myth, and even myth is long forgotten when the Age that gave it birth comes again. " +
            "In one Age, called the Third Age by some, an Age yet to come, an Age long past, a wind rose on the great plain called the Caralain Grass. " +
            "The wind was not the beginning. There are neither beginnings nor endings to the turning of the Wheel of Time. But it was A beginning.\n" +
            "~ Robert Jordan, The Shadow Rising, The Wheel of Time #4",

            "The Wheel of Time turns, and Ages come and pass, leaving memories that become legend. Legend fades to myth, and even myth is long forgotten when the Age that gave it birth comes again. " +
            "In one Age, called the Third Age by some, an Age yet to come, an Age long past, a wind rose in the great forest called Braem Wood. " +
            "The wind was not the beginning. There are neither beginnings nor endings to the turning of the Wheel of Time. But it was A beginning.\n" +
            "~ Robert Jordan, The Fires of Heaven, The Wheel of Time #5",

            "The Wheel of Time turns, and Ages come and pass, leaving memories that become legend. Legend fades to myth, and even myth is long forgotten when the Age that gave it birth comes again. " +
            "In one Age, called the Third Age by some, an Age yet to come, an Age long past, a wind rose among brown-thicketed hills in Cairhien. " +
            "The wind was not the beginning. There are neither beginnings nor endings to the turning of the Wheel of Time. But it was A beginning.\n" +
            "~ Robert Jordan, Lord of Chaos, The Wheel of Time #6",

            "The Wheel of Time turns, and Ages come and pass, leaving memories that become legend. Legend fades to myth, and even myth is long forgotten when the Age that gave it birth comes again. " +
            "In one Age, called the Third Age by some, an Age yet to come, an Age long past, a wind rose in the great forest called Braem Wood. " +
            "The wind was not the beginning. There are neither beginnings nor endings to the turning of the Wheel of Time. But it was A beginning.\n" +
            "~ Robert Jordan, A Crown of Swords, The Wheel of Time #7",

            "The Wheel of Time turns, and Ages come and pass, leaving memories that become legend. Legend fades to myth, and even myth is long forgotten when the Age that gave it birth comes again. " +
            "In one Age, called the Third Age by some, an Age yet to come, an Age long past, a wind rose above the great mountainous island of Tremalking. " +
            "The wind was not the beginning. There are neither beginnings nor endings to the turning of the Wheel of Time. But it was A beginning.\n" +
            "~ Robert Jordan, The Path of Daggers, The Wheel of Time #8",

            "The Wheel of Time turns, and Ages come and pass, leaving memories that become legend. Legend fades to myth, and even myth is long forgotten when the Age that gave it birth comes again. " +
            "In one Age, called the Third Age by some, an Age yet to come, an Age long past, a wind rose above the Aryth Ocean. " +
            "The wind was not the beginning. There are neither beginnings nor endings to the turning of the Wheel of Time. But it was A beginning.\n" +
            "~ Robert Jordan, Winter's Heart, The Wheel of Time #9",

            "The Wheel of Time turns, and Ages come and pass, leaving memories that become legend. Legend fades to myth, and even myth is long forgotten when the Age that gave it birth comes again. " +
            "In one Age, called the Third Age by some, an Age yet to come, an Age long past, a wind rose in the Rhannon Hills. " +
            "The wind was not the beginning. There are neither beginnings nor endings to the turning of the Wheel of Time. But it was A beginning.\n" +
            "~ Robert Jordan, Crossroads of Twilight, The Wheel of Time #10",

            "The Wheel of Time turns, and Ages come and pass, leaving memories that become legend. Legend fades to myth, and even myth is long forgotten when the Age that gave it birth comes again. " +
            "In one Age, called the Third Age by some, an Age yet to come, an Age long past, a wind rose above the broken mountain named Dragonmount. " +
            "The wind was not the beginning. There are neither beginnings nor endings to the turning of the Wheel of Time. But it was A beginning.\n" +
            "~ Robert Jordan, Knife of Dreams, The Wheel of Time #11",

            "The Wheel of Time turns, and Ages come and pass, leaving memories that become legend. Legend fades to myth, and even myth is long forgotten when the Age that gave it birth comes again. " +
            "In one Age, called the Third Age by some, an Age yet to come, an Age long past, a wind rose around the alabaster spire known as the White Tower. " +
            "The wind was not the beginning. There are neither beginnings nor endings to the turning of the Wheel of Time. But it was A beginning.\n" +
            "~ Robert Jordan & Brandon Sanderson, The Gathering Storm, The Wheel of Time #12",

            "The Wheel of Time turns, and Ages come and pass, leaving memories that become legend. Legend fades to myth, and even myth is long forgotten when the Age that gave it birth comes again. " +
            "In one Age, called the Third Age by some, an Age yet to come, an Age long past, a wind rose above the misty peaks of Imfaral. " +
            "The wind was not the beginning. There are neither beginnings nor endings to the turning of the Wheel of Time. But it was A beginning.\n" +
            "~ Robert Jordan & Brandon Sanderson, Towers of Midnight, The Wheel of Time #13",

            "The Wheel of Time turns, and Ages come and pass, leaving memories that become legend. Legend fades to myth, and even myth is long forgotten when the Age that gave it birth comes again. " +
            "In one Age, called the Third Age by some, an Age yet to come, an Age long past, a wind rose in the Mountains of Mist. " +
            "The wind was not the beginning. There are neither beginnings nor endings to the turning of the Wheel of Time. But it was A beginning.\n" +
            "~ Robert Jordan & Brandon Sanderson, A Memory of Light, The Wheel of Time #14"
        ];

        #endregion // Wot Quotes

        [HttpGet("wot")]
        public IActionResult Wot([FromQuery] int? book = default)
        {
            if (book.HasValue && (book <= 0 || book > wotQuotes.Length))
            {
                return this.BadRequest($"If supplying book number, it must be in [1,{wotQuotes.Length}]. You supplied '{book}'.");
            }
            else if (!book.HasValue)
            {
                book = Random.Shared.Next(wotQuotes.Length) + 1;
            }

            return this.Ok(wotQuotes[book.Value - 1]);
        }
    }
}
