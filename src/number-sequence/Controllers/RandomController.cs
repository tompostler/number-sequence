﻿using Microsoft.AspNetCore.Mvc;
using System.Web;
using TcpWtf.NumberSequence.Contracts;
using Unlimitedinf.Utilities.Extensions;

namespace number_sequence.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class RandomController : ControllerBase
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

        public static readonly string[] EightBallResponses = new[]
        {
            // A standard Magic 8 Ball is capable of 10 affirmative answers (Y), 5 non-committal answers (~), and 5 negative answers (X).
            "[Y] It is certain.",         "[Y] As I see it, yes.",      "[~] Reply hazy, try again.",         "[X] Don't count on it.",
            "[Y] It is decidedly so.",    "[Y] Most likely.",           "[~] Ask again later.",               "[X] My reply is no.",
            "[Y] Without a doubt.",       "[Y] Outlook good.",          "[~] Better not tell you now.",       "[X] My sources say no.",
            "[Y] Yes - definitely.",      "[Y] Yes.",                   "[~] Cannot predict now.",            "[X] Outlook not so good.",
            "[Y] You may rely on it.",    "[Y] Signs point to yes.",    "[~] Concentrate and ask again.",     "[X] Very doubtful.",
        };
        [HttpGet("8ball")]
        public IActionResult EightBall() => this.Ok(EightBallResponses[Random.Shared.Next(EightBallResponses.Length)]);

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

        // Fetched 2021-06-03 from https://github.com/moby/moby/blob/master/pkg/namesgenerator/names-generator.go
        #region Moby name constants

        private static readonly string[] mobyNameAdjectives = new string[]
        {
            "admiring",
            "adoring",
            "affectionate",
            "agitated",
            "amazing",
            "angry",
            "awesome",
            "beautiful",
            "blissful",
            "bold",
            "boring",
            "brave",
            "busy",
            "charming",
            "clever",
            "cool",
            "compassionate",
            "competent",
            "condescending",
            "confident",
            "cranky",
            "crazy",
            "dazzling",
            "determined",
            "distracted",
            "dreamy",
            "eager",
            "ecstatic",
            "elastic",
            "elated",
            "elegant",
            "eloquent",
            "epic",
            "exciting",
            "fervent",
            "festive",
            "flamboyant",
            "focused",
            "friendly",
            "frosty",
            "funny",
            "gallant",
            "gifted",
            "goofy",
            "gracious",
            "great",
            "happy",
            "hardcore",
            "heuristic",
            "hopeful",
            "hungry",
            "infallible",
            "inspiring",
            "interesting",
            "intelligent",
            "jolly",
            "jovial",
            "keen",
            "kind",
            "laughing",
            "loving",
            "lucid",
            "magical",
            "mystifying",
            "modest",
            "musing",
            "naughty",
            "nervous",
            "nice",
            "nifty",
            "nostalgic",
            "objective",
            "optimistic",
            "peaceful",
            "pedantic",
            "pensive",
            "practical",
            "priceless",
            "quirky",
            "quizzical",
            "recursing",
            "relaxed",
            "reverent",
            "romantic",
            "sad",
            "serene",
            "sharp",
            "silly",
            "sleepy",
            "stoic",
            "strange",
            "stupefied",
            "suspicious",
            "sweet",
            "tender",
            "thirsty",
            "trusting",
            "unruffled",
            "upbeat",
            "vibrant",
            "vigilant",
            "vigorous",
            "wizardly",
            "wonderful",
            "xenodochial",
            "youthful",
            "zealous",
            "zen",
        };

        private static readonly string[] mobyNameNames = new string[]
        {
            // Muhammad ibn Jābir al-Ḥarrānī al-Battānī was a founding father of astronomy. https://en.wikipedia.org/wiki/Mu%E1%B8%A5ammad_ibn_J%C4%81bir_al-%E1%B8%A4arr%C4%81n%C4%AB_al-Batt%C4%81n%C4%AB
			"albattani",

			// Frances E. Allen, became the first female IBM Fellow in 1989. In 2006, she became the first female recipient of the ACM's Turing Award. https://en.wikipedia.org/wiki/Frances_E._Allen
			"allen",

			// June Almeida - Scottish virologist who took the first pictures of the rubella virus - https://en.wikipedia.org/wiki/June_Almeida
			"almeida",

			// Kathleen Antonelli, American computer programmer and one of the six original programmers of the ENIAC - https://en.wikipedia.org/wiki/Kathleen_Antonelli
			"antonelli",

			// Maria Gaetana Agnesi - Italian mathematician, philosopher, theologian and humanitarian. She was the first woman to write a mathematics handbook and the first woman appointed as a Mathematics Professor at a University. https://en.wikipedia.org/wiki/Maria_Gaetana_Agnesi
			"agnesi",

			// Archimedes was a physicist, engineer and mathematician who invented too many things to list them here. https://en.wikipedia.org/wiki/Archimedes
			"archimedes",

			// Maria Ardinghelli - Italian translator, mathematician and physicist - https://en.wikipedia.org/wiki/Maria_Ardinghelli
			"ardinghelli",

			// Aryabhata - Ancient Indian mathematician-astronomer during 476-550 CE https://en.wikipedia.org/wiki/Aryabhata
			"aryabhata",

			// Wanda Austin - Wanda Austin is the President and CEO of The Aerospace Corporation, a leading architect for the US security space programs. https://en.wikipedia.org/wiki/Wanda_Austin
			"austin",

			// Charles Babbage invented the concept of a programmable computer. https://en.wikipedia.org/wiki/Charles_Babbage.
			"babbage",

			// Stefan Banach - Polish mathematician, was one of the founders of modern functional analysis. https://en.wikipedia.org/wiki/Stefan_Banach
			"banach",

			// Buckaroo Banzai and his mentor Dr. Hikita perfected the "oscillation overthruster", a device that allows one to pass through solid matter. - https://en.wikipedia.org/wiki/The_Adventures_of_Buckaroo_Banzai_Across_the_8th_Dimension
			"banzai",

			// John Bardeen co-invented the transistor - https://en.wikipedia.org/wiki/John_Bardeen
			"bardeen",

			// Jean Bartik, born Betty Jean Jennings, was one of the original programmers for the ENIAC computer. https://en.wikipedia.org/wiki/Jean_Bartik
			"bartik",

			// Laura Bassi, the world's first female professor https://en.wikipedia.org/wiki/Laura_Bassi
			"bassi",

			// Hugh Beaver, British engineer, founder of the Guinness Book of World Records https://en.wikipedia.org/wiki/Hugh_Beaver
			"beaver",

			// Alexander Graham Bell - an eminent Scottish-born scientist, inventor, engineer and innovator who is credited with inventing the first practical telephone - https://en.wikipedia.org/wiki/Alexander_Graham_Bell
			"bell",

			// Karl Friedrich Benz - a German automobile engineer. Inventor of the first practical motorcar. https://en.wikipedia.org/wiki/Karl_Benz
			"benz",

			// Homi J Bhabha - was an Indian nuclear physicist, founding director, and professor of physics at the Tata Institute of Fundamental Research. Colloquially known as "father of Indian nuclear programme"- https://en.wikipedia.org/wiki/Homi_J._Bhabha
			"bhabha",

			// Bhaskara II - Ancient Indian mathematician-astronomer whose work on calculus predates Newton and Leibniz by over half a millennium - https://en.wikipedia.org/wiki/Bh%C4%81skara_II#Calculus
			"bhaskara",

			// Sue Black - British computer scientist and campaigner. She has been instrumental in saving Bletchley Park, the site of World War II codebreaking - https://en.wikipedia.org/wiki/Sue_Black_(computer_scientist)
			"black",

			// Elizabeth Helen Blackburn - Australian-American Nobel laureate; best known for co-discovering telomerase. https://en.wikipedia.org/wiki/Elizabeth_Blackburn
			"blackburn",

			// Elizabeth Blackwell - American doctor and first American woman to receive a medical degree - https://en.wikipedia.org/wiki/Elizabeth_Blackwell
			"blackwell",

			// Niels Bohr is the father of quantum theory. https://en.wikipedia.org/wiki/Niels_Bohr.
			"bohr",

			// Kathleen Booth, she's credited with writing the first assembly language. https://en.wikipedia.org/wiki/Kathleen_Booth
			"booth",

			// Anita Borg - Anita Borg was the founding director of the Institute for Women and Technology (IWT). https://en.wikipedia.org/wiki/Anita_Borg
			"borg",

			// Satyendra Nath Bose - He provided the foundation for Bose–Einstein statistics and the theory of the Bose–Einstein condensate. - https://en.wikipedia.org/wiki/Satyendra_Nath_Bose
			"bose",

			// Katherine Louise Bouman is an imaging scientist and Assistant Professor of Computer Science at the California Institute of Technology. She researches computational methods for imaging, and developed an algorithm that made possible the picture first visualization of a black hole using the Event Horizon Telescope. - https://en.wikipedia.org/wiki/Katie_Bouman
			"bouman",

			// Evelyn Boyd Granville - She was one of the first African-American woman to receive a Ph.D. in mathematics; she earned it in 1949 from Yale University. https://en.wikipedia.org/wiki/Evelyn_Boyd_Granville
			"boyd",

			// Brahmagupta - Ancient Indian mathematician during 598-670 CE who gave rules to compute with zero - https://en.wikipedia.org/wiki/Brahmagupta#Zero
			"brahmagupta",

			// Walter Houser Brattain co-invented the transistor - https://en.wikipedia.org/wiki/Walter_Houser_Brattain
			"brattain",

			// Emmett Brown invented time travel. https://en.wikipedia.org/wiki/Emmett_Brown (thanks Brian Goff)
			"brown",

			// Linda Brown Buck - American biologist and Nobel laureate best known for her genetic and molecular analyses of the mechanisms of smell. https://en.wikipedia.org/wiki/Linda_B._Buck
			"buck",

			// Dame Susan Jocelyn Bell Burnell - Northern Irish astrophysicist who discovered radio pulsars and was the first to analyse them. https://en.wikipedia.org/wiki/Jocelyn_Bell_Burnell
			"burnell",

			// Annie Jump Cannon - pioneering female astronomer who classified hundreds of thousands of stars and created the system we use to understand stars today. https://en.wikipedia.org/wiki/Annie_Jump_Cannon
			"cannon",

			// Rachel Carson - American marine biologist and conservationist, her book Silent Spring and other writings are credited with advancing the global environmental movement. https://en.wikipedia.org/wiki/Rachel_Carson
			"carson",

			// Dame Mary Lucy Cartwright - British mathematician who was one of the first to study what is now known as chaos theory. Also known for Cartwright's theorem which finds applications in signal processing. https://en.wikipedia.org/wiki/Mary_Cartwright
			"cartwright",

			// George Washington Carver - American agricultural scientist and inventor. He was the most prominent black scientist of the early 20th century. https://en.wikipedia.org/wiki/George_Washington_Carver
			"carver",

			// Vinton Gray Cerf - American Internet pioneer, recognised as one of "the fathers of the Internet". With Robert Elliot Kahn, he designed TCP and IP, the primary data communication protocols of the Internet and other computer networks. https://en.wikipedia.org/wiki/Vint_Cerf
			"cerf",

			// Subrahmanyan Chandrasekhar - Astrophysicist known for his mathematical theory on different stages and evolution in structures of the stars. He has won nobel prize for physics - https://en.wikipedia.org/wiki/Subrahmanyan_Chandrasekhar
			"chandrasekhar",

			// Sergey Alexeyevich Chaplygin (Russian: Серге́й Алексе́евич Чаплы́гин; April 5, 1869 – October 8, 1942) was a Russian and Soviet physicist, mathematician, and mechanical engineer. He is known for mathematical formulas such as Chaplygin's equation and for a hypothetical substance in cosmology called Chaplygin gas, named after him. https://en.wikipedia.org/wiki/Sergey_Chaplygin
			"chaplygin",

			// Émilie du Châtelet - French natural philosopher, mathematician, physicist, and author during the early 1730s, known for her translation of and commentary on Isaac Newton's book Principia containing basic laws of physics. https://en.wikipedia.org/wiki/%C3%89milie_du_Ch%C3%A2telet
			"chatelet",

			// Asima Chatterjee was an Indian organic chemist noted for her research on vinca alkaloids, development of drugs for treatment of epilepsy and malaria - https://en.wikipedia.org/wiki/Asima_Chatterjee
			"chatterjee",

			// Pafnuty Chebyshev - Russian mathematician. He is known fo his works on probability, statistics, mechanics, analytical geometry and number theory https://en.wikipedia.org/wiki/Pafnuty_Chebyshev
			"chebyshev",

			// Bram Cohen - American computer programmer and author of the BitTorrent peer-to-peer protocol. https://en.wikipedia.org/wiki/Bram_Cohen
			"cohen",

			// David Lee Chaum - American computer scientist and cryptographer. Known for his seminal contributions in the field of anonymous communication. https://en.wikipedia.org/wiki/David_Chaum
			"chaum",

			// Joan Clarke - Bletchley Park code breaker during the Second World War who pioneered techniques that remained top secret for decades. Also an accomplished numismatist https://en.wikipedia.org/wiki/Joan_Clarke
			"clarke",

			// Jane Colden - American botanist widely considered the first female American botanist - https://en.wikipedia.org/wiki/Jane_Colden
			"colden",

			// Gerty Theresa Cori - American biochemist who became the third woman—and first American woman—to win a Nobel Prize in science, and the first woman to be awarded the Nobel Prize in Physiology or Medicine. Cori was born in Prague. https://en.wikipedia.org/wiki/Gerty_Cori
			"cori",

			// Seymour Roger Cray was an American electrical engineer and supercomputer architect who designed a series of computers that were the fastest in the world for decades. https://en.wikipedia.org/wiki/Seymour_Cray
			"cray",

			// This entry reflects a husband and wife team who worked together:
			// Joan Curran was a Welsh scientist who developed radar and invented chaff, a radar countermeasure. https://en.wikipedia.org/wiki/Joan_Curran
			// Samuel Curran was an Irish physicist who worked alongside his wife during WWII and invented the proximity fuse. https://en.wikipedia.org/wiki/Samuel_Curran
			"curran",

			// Marie Curie discovered radioactivity. https://en.wikipedia.org/wiki/Marie_Curie.
			"curie",

			// Charles Darwin established the principles of natural evolution. https://en.wikipedia.org/wiki/Charles_Darwin.
			"darwin",

			// Leonardo Da Vinci invented too many things to list here. https://en.wikipedia.org/wiki/Leonardo_da_Vinci.
			"davinci",

			// A. K. (Alexander Keewatin) Dewdney, Canadian mathematician, computer scientist, author and filmmaker. Contributor to Scientific American's "Computer Recreations" from 1984 to 1991. Author of Core War (program), The Planiverse, The Armchair Universe, The Magic Machine, The New Turing Omnibus, and more. https://en.wikipedia.org/wiki/Alexander_Dewdney
			"dewdney",

			// Satish Dhawan - Indian mathematician and aerospace engineer, known for leading the successful and indigenous development of the Indian space programme. https://en.wikipedia.org/wiki/Satish_Dhawan
			"dhawan",

			// Bailey Whitfield Diffie - American cryptographer and one of the pioneers of public-key cryptography. https://en.wikipedia.org/wiki/Whitfield_Diffie
			"diffie",

			// Edsger Wybe Dijkstra was a Dutch computer scientist and mathematical scientist. https://en.wikipedia.org/wiki/Edsger_W._Dijkstra.
			"dijkstra",

			// Paul Adrien Maurice Dirac - English theoretical physicist who made fundamental contributions to the early development of both quantum mechanics and quantum electrodynamics. https://en.wikipedia.org/wiki/Paul_Dirac
			"dirac",

			// Agnes Meyer Driscoll - American cryptanalyst during World Wars I and II who successfully cryptanalysed a number of Japanese ciphers. She was also the co-developer of one of the cipher machines of the US Navy, the CM. https://en.wikipedia.org/wiki/Agnes_Meyer_Driscoll
			"driscoll",

			// Donna Dubinsky - played an integral role in the development of personal digital assistants (PDAs) serving as CEO of Palm, Inc. and co-founding Handspring. https://en.wikipedia.org/wiki/Donna_Dubinsky
			"dubinsky",

			// Annie Easley - She was a leading member of the team which developed software for the Centaur rocket stage and one of the first African-Americans in her field. https://en.wikipedia.org/wiki/Annie_Easley
			"easley",

			// Thomas Alva Edison, prolific inventor https://en.wikipedia.org/wiki/Thomas_Edison
			"edison",

			// Albert Einstein invented the general theory of relativity. https://en.wikipedia.org/wiki/Albert_Einstein
			"einstein",

			// Alexandra Asanovna Elbakyan (Russian: Алекса́ндра Аса́новна Элбакя́н) is a Kazakhstani graduate student, computer programmer, internet pirate in hiding, and the creator of the site Sci-Hub. Nature has listed her in 2016 in the top ten people that mattered in science, and Ars Technica has compared her to Aaron Swartz. - https://en.wikipedia.org/wiki/Alexandra_Elbakyan
			"elbakyan",

			// Taher A. ElGamal - Egyptian cryptographer best known for the ElGamal discrete log cryptosystem and the ElGamal digital signature scheme. https://en.wikipedia.org/wiki/Taher_Elgamal
			"elgamal",

			// Gertrude Elion - American biochemist, pharmacologist and the 1988 recipient of the Nobel Prize in Medicine - https://en.wikipedia.org/wiki/Gertrude_Elion
			"elion",

			// James Henry Ellis - British engineer and cryptographer employed by the GCHQ. Best known for conceiving for the first time, the idea of public-key cryptography. https://en.wikipedia.org/wiki/James_H._Ellis
			"ellis",

			// Douglas Engelbart gave the mother of all demos: https://en.wikipedia.org/wiki/Douglas_Engelbart
			"engelbart",

			// Euclid invented geometry. https://en.wikipedia.org/wiki/Euclid
			"euclid",

			// Leonhard Euler invented large parts of modern mathematics. https://de.wikipedia.org/wiki/Leonhard_Euler
			"euler",

			// Michael Faraday - British scientist who contributed to the study of electromagnetism and electrochemistry. https://en.wikipedia.org/wiki/Michael_Faraday
			"faraday",

			// Horst Feistel - German-born American cryptographer who was one of the earliest non-government researchers to study the design and theory of block ciphers. Co-developer of DES and Lucifer. Feistel networks, a symmetric structure used in the construction of block ciphers are named after him. https://en.wikipedia.org/wiki/Horst_Feistel
			"feistel",

			// Pierre de Fermat pioneered several aspects of modern mathematics. https://en.wikipedia.org/wiki/Pierre_de_Fermat
			"fermat",

			// Enrico Fermi invented the first nuclear reactor. https://en.wikipedia.org/wiki/Enrico_Fermi.
			"fermi",

			// Richard Feynman was a key contributor to quantum mechanics and particle physics. https://en.wikipedia.org/wiki/Richard_Feynman
			"feynman",

			// Benjamin Franklin is famous for his experiments in electricity and the invention of the lightning rod.
			"franklin",

			// Yuri Alekseyevich Gagarin - Soviet pilot and cosmonaut, best known as the first human to journey into outer space. https://en.wikipedia.org/wiki/Yuri_Gagarin
			"gagarin",

			// Galileo was a founding father of modern astronomy, and faced politics and obscurantism to establish scientific truth.  https://en.wikipedia.org/wiki/Galileo_Galilei
			"galileo",

			// Évariste Galois - French mathematician whose work laid the foundations of Galois theory and group theory, two major branches of abstract algebra, and the subfield of Galois connections, all while still in his late teens. https://en.wikipedia.org/wiki/%C3%89variste_Galois
			"galois",

			// Kadambini Ganguly - Indian physician, known for being the first South Asian female physician, trained in western medicine, to graduate in South Asia. https://en.wikipedia.org/wiki/Kadambini_Ganguly
			"ganguly",

			// William Henry "Bill" Gates III is an American business magnate, philanthropist, investor, computer programmer, and inventor. https://en.wikipedia.org/wiki/Bill_Gates
			"gates",

			// Johann Carl Friedrich Gauss - German mathematician who made significant contributions to many fields, including number theory, algebra, statistics, analysis, differential geometry, geodesy, geophysics, mechanics, electrostatics, magnetic fields, astronomy, matrix theory, and optics. https://en.wikipedia.org/wiki/Carl_Friedrich_Gauss
			"gauss",

			// Marie-Sophie Germain - French mathematician, physicist and philosopher. Known for her work on elasticity theory, number theory and philosophy. https://en.wikipedia.org/wiki/Sophie_Germain
			"germain",

			// Adele Goldberg, was one of the designers and developers of the Smalltalk language. https://en.wikipedia.org/wiki/Adele_Goldberg_(computer_scientist)
			"goldberg",

			// Adele Goldstine, born Adele Katz, wrote the complete technical description for the first electronic digital computer, ENIAC. https://en.wikipedia.org/wiki/Adele_Goldstine
			"goldstine",

			// Shafi Goldwasser is a computer scientist known for creating theoretical foundations of modern cryptography. Winner of 2012 ACM Turing Award. https://en.wikipedia.org/wiki/Shafi_Goldwasser
			"goldwasser",

			// James Golick, all around gangster.
			"golick",

			// Jane Goodall - British primatologist, ethologist, and anthropologist who is considered to be the world's foremost expert on chimpanzees - https://en.wikipedia.org/wiki/Jane_Goodall
			"goodall",

			// Stephen Jay Gould was was an American paleontologist, evolutionary biologist, and historian of science. He is most famous for the theory of punctuated equilibrium - https://en.wikipedia.org/wiki/Stephen_Jay_Gould
			"gould",

			// Carolyn Widney Greider - American molecular biologist and joint winner of the 2009 Nobel Prize for Physiology or Medicine for the discovery of telomerase. https://en.wikipedia.org/wiki/Carol_W._Greider
			"greider",

			// Alexander Grothendieck - German-born French mathematician who became a leading figure in the creation of modern algebraic geometry. https://en.wikipedia.org/wiki/Alexander_Grothendieck
			"grothendieck",

			// Lois Haibt - American computer scientist, part of the team at IBM that developed FORTRAN - https://en.wikipedia.org/wiki/Lois_Haibt
			"haibt",

			// Margaret Hamilton - Director of the Software Engineering Division of the MIT Instrumentation Laboratory, which developed on-board flight software for the Apollo space program. https://en.wikipedia.org/wiki/Margaret_Hamilton_(scientist)
			"hamilton",

			// Caroline Harriet Haslett - English electrical engineer, electricity industry administrator and champion of women's rights. Co-author of British Standard 1363 that specifies AC power plugs and sockets used across the United Kingdom (which is widely considered as one of the safest designs). https://en.wikipedia.org/wiki/Caroline_Haslett
			"haslett",

			// Stephen Hawking pioneered the field of cosmology by combining general relativity and quantum mechanics. https://en.wikipedia.org/wiki/Stephen_Hawking
			"hawking",

			// Martin Edward Hellman - American cryptologist, best known for his invention of public-key cryptography in co-operation with Whitfield Diffie and Ralph Merkle. https://en.wikipedia.org/wiki/Martin_Hellman
			"hellman",

			// Werner Heisenberg was a founding father of quantum mechanics. https://en.wikipedia.org/wiki/Werner_Heisenberg
			"heisenberg",

			// Grete Hermann was a German philosopher noted for her philosophical work on the foundations of quantum mechanics. https://en.wikipedia.org/wiki/Grete_Hermann
			"hermann",

			// Caroline Lucretia Herschel - German astronomer and discoverer of several comets. https://en.wikipedia.org/wiki/Caroline_Herschel
			"herschel",

			// Heinrich Rudolf Hertz - German physicist who first conclusively proved the existence of the electromagnetic waves. https://en.wikipedia.org/wiki/Heinrich_Hertz
			"hertz",

			// Jaroslav Heyrovský was the inventor of the polarographic method, father of the electroanalytical method, and recipient of the Nobel Prize in 1959. His main field of work was polarography. https://en.wikipedia.org/wiki/Jaroslav_Heyrovsk%C3%BD
			"heyrovsky",

			// Dorothy Hodgkin was a British biochemist, credited with the development of protein crystallography. She was awarded the Nobel Prize in Chemistry in 1964. https://en.wikipedia.org/wiki/Dorothy_Hodgkin
			"hodgkin",

			// Douglas R. Hofstadter is an American professor of cognitive science and author of the Pulitzer Prize and American Book Award-winning work Goedel, Escher, Bach: An Eternal Golden Braid in 1979. A mind-bending work which coined Hofstadter's Law: "It always takes longer than you expect, even when you take into account Hofstadter's Law." https://en.wikipedia.org/wiki/Douglas_Hofstadter
			"hofstadter",

			// Erna Schneider Hoover revolutionized modern communication by inventing a computerized telephone switching method. https://en.wikipedia.org/wiki/Erna_Schneider_Hoover
			"hoover",

			// Grace Hopper developed the first compiler for a computer programming language and  is credited with popularizing the term "debugging" for fixing computer glitches. https://en.wikipedia.org/wiki/Grace_Hopper
			"hopper",

			// Frances Hugle, she was an American scientist, engineer, and inventor who contributed to the understanding of semiconductors, integrated circuitry, and the unique electrical principles of microscopic materials. https://en.wikipedia.org/wiki/Frances_Hugle
			"hugle",

			// Hypatia - Greek Alexandrine Neoplatonist philosopher in Egypt who was one of the earliest mothers of mathematics - https://en.wikipedia.org/wiki/Hypatia
			"hypatia",

			// Teruko Ishizaka - Japanese scientist and immunologist who co-discovered the antibody class Immunoglobulin E. https://en.wikipedia.org/wiki/Teruko_Ishizaka
			"ishizaka",

			// Mary Jackson, American mathematician and aerospace engineer who earned the highest title within NASA's engineering department - https://en.wikipedia.org/wiki/Mary_Jackson_(engineer)
			"jackson",

			// Yeong-Sil Jang was a Korean scientist and astronomer during the Joseon Dynasty; he invented the first metal printing press and water gauge. https://en.wikipedia.org/wiki/Jang_Yeong-sil
			"jang",

			// Mae Carol Jemison -  is an American engineer, physician, and former NASA astronaut. She became the first black woman to travel in space when she served as a mission specialist aboard the Space Shuttle Endeavour - https://en.wikipedia.org/wiki/Mae_Jemison
			"jemison",

			// Betty Jennings - one of the original programmers of the ENIAC. https://en.wikipedia.org/wiki/ENIAC - https://en.wikipedia.org/wiki/Jean_Bartik
			"jennings",

			// Mary Lou Jepsen, was the founder and chief technology officer of One Laptop Per Child (OLPC), and the founder of Pixel Qi. https://en.wikipedia.org/wiki/Mary_Lou_Jepsen
			"jepsen",

			// Katherine Coleman Goble Johnson - American physicist and mathematician contributed to the NASA. https://en.wikipedia.org/wiki/Katherine_Johnson
			"johnson",

			// Irène Joliot-Curie - French scientist who was awarded the Nobel Prize for Chemistry in 1935. Daughter of Marie and Pierre Curie. https://en.wikipedia.org/wiki/Ir%C3%A8ne_Joliot-Curie
			"joliot",

			// Karen Spärck Jones came up with the concept of inverse document frequency, which is used in most search engines today. https://en.wikipedia.org/wiki/Karen_Sp%C3%A4rck_Jones
			"jones",

			// A. P. J. Abdul Kalam - is an Indian scientist aka Missile Man of India for his work on the development of ballistic missile and launch vehicle technology - https://en.wikipedia.org/wiki/A._P._J._Abdul_Kalam
			"kalam",

			// Sergey Petrovich Kapitsa (Russian: Серге́й Петро́вич Капи́ца; 14 February 1928 – 14 August 2012) was a Russian physicist and demographer. He was best known as host of the popular and long-running Russian scientific TV show, Evident, but Incredible. His father was the Nobel laureate Soviet-era physicist Pyotr Kapitsa, and his brother was the geographer and Antarctic explorer Andrey Kapitsa. - https://en.wikipedia.org/wiki/Sergey_Kapitsa
			"kapitsa",

			// Susan Kare, created the icons and many of the interface elements for the original Apple Macintosh in the 1980s, and was an original employee of NeXT, working as the Creative Director. https://en.wikipedia.org/wiki/Susan_Kare
			"kare",

			// Mstislav Keldysh - a Soviet scientist in the field of mathematics and mechanics, academician of the USSR Academy of Sciences (1946), President of the USSR Academy of Sciences (1961–1975), three times Hero of Socialist Labor (1956, 1961, 1971), fellow of the Royal Society of Edinburgh (1968). https://en.wikipedia.org/wiki/Mstislav_Keldysh
			"keldysh",

			// Mary Kenneth Keller, Sister Mary Kenneth Keller became the first American woman to earn a PhD in Computer Science in 1965. https://en.wikipedia.org/wiki/Mary_Kenneth_Keller
			"keller",

			// Johannes Kepler, German astronomer known for his three laws of planetary motion - https://en.wikipedia.org/wiki/Johannes_Kepler
			"kepler",

			// Omar Khayyam - Persian mathematician, astronomer and poet. Known for his work on the classification and solution of cubic equations, for his contribution to the understanding of Euclid's fifth postulate and for computing the length of a year very accurately. https://en.wikipedia.org/wiki/Omar_Khayyam
			"khayyam",

			// Har Gobind Khorana - Indian-American biochemist who shared the 1968 Nobel Prize for Physiology - https://en.wikipedia.org/wiki/Har_Gobind_Khorana
			"khorana",

			// Jack Kilby invented silicon integrated circuits and gave Silicon Valley its name. - https://en.wikipedia.org/wiki/Jack_Kilby
			"kilby",

			// Maria Kirch - German astronomer and first woman to discover a comet - https://en.wikipedia.org/wiki/Maria_Margarethe_Kirch
			"kirch",

			// Donald Knuth - American computer scientist, author of "The Art of Computer Programming" and creator of the TeX typesetting system. https://en.wikipedia.org/wiki/Donald_Knuth
			"knuth",

			// Sophie Kowalevski - Russian mathematician responsible for important original contributions to analysis, differential equations and mechanics - https://en.wikipedia.org/wiki/Sofia_Kovalevskaya
			"kowalevski",

			// Marie-Jeanne de Lalande - French astronomer, mathematician and cataloguer of stars - https://en.wikipedia.org/wiki/Marie-Jeanne_de_Lalande
			"lalande",

			// Hedy Lamarr - Actress and inventor. The principles of her work are now incorporated into modern Wi-Fi, CDMA and Bluetooth technology. https://en.wikipedia.org/wiki/Hedy_Lamarr
			"lamarr",

			// Leslie B. Lamport - American computer scientist. Lamport is best known for his seminal work in distributed systems and was the winner of the 2013 Turing Award. https://en.wikipedia.org/wiki/Leslie_Lamport
			"lamport",

			// Mary Leakey - British paleoanthropologist who discovered the first fossilized Proconsul skull - https://en.wikipedia.org/wiki/Mary_Leakey
			"leakey",

			// Henrietta Swan Leavitt - she was an American astronomer who discovered the relation between the luminosity and the period of Cepheid variable stars. https://en.wikipedia.org/wiki/Henrietta_Swan_Leavitt
			"leavitt",

			// Esther Miriam Zimmer Lederberg - American microbiologist and a pioneer of bacterial genetics. https://en.wikipedia.org/wiki/Esther_Lederberg
			"lederberg",

			// Inge Lehmann - Danish seismologist and geophysicist. Known for discovering in 1936 that the Earth has a solid inner core inside a molten outer core. https://en.wikipedia.org/wiki/Inge_Lehmann
			"lehmann",

			// Daniel Lewin - Mathematician, Akamai co-founder, soldier, 9/11 victim-- Developed optimization techniques for routing traffic on the internet. Died attempting to stop the 9-11 hijackers. https://en.wikipedia.org/wiki/Daniel_Lewin
			"lewin",

			// Ruth Lichterman - one of the original programmers of the ENIAC. https://en.wikipedia.org/wiki/ENIAC - https://en.wikipedia.org/wiki/Ruth_Teitelbaum
			"lichterman",

			// Barbara Liskov - co-developed the Liskov substitution principle. Liskov was also the winner of the Turing Prize in 2008. - https://en.wikipedia.org/wiki/Barbara_Liskov
			"liskov",

			// Ada Lovelace invented the first algorithm. https://en.wikipedia.org/wiki/Ada_Lovelace (thanks James Turnbull)
			"lovelace",

			// Auguste and Louis Lumière - the first filmmakers in history - https://en.wikipedia.org/wiki/Auguste_and_Louis_Lumi%C3%A8re
			"lumiere",

			// Mahavira - Ancient Indian mathematician during 9th century AD who discovered basic algebraic identities - https://en.wikipedia.org/wiki/Mah%C4%81v%C4%ABra_(mathematician)
			"mahavira",

			// Lynn Margulis (b. Lynn Petra Alexander) - an American evolutionary theorist and biologist, science author, educator, and popularizer, and was the primary modern proponent for the significance of symbiosis in evolution. - https://en.wikipedia.org/wiki/Lynn_Margulis
			"margulis",

			// Yukihiro Matsumoto - Japanese computer scientist and software programmer best known as the chief designer of the Ruby programming language. https://en.wikipedia.org/wiki/Yukihiro_Matsumoto
			"matsumoto",

			// James Clerk Maxwell - Scottish physicist, best known for his formulation of electromagnetic theory. https://en.wikipedia.org/wiki/James_Clerk_Maxwell
			"maxwell",

			// Maria Mayer - American theoretical physicist and Nobel laureate in Physics for proposing the nuclear shell model of the atomic nucleus - https://en.wikipedia.org/wiki/Maria_Mayer
			"mayer",

			// John McCarthy invented LISP: https://en.wikipedia.org/wiki/John_McCarthy_(computer_scientist)
			"mccarthy",

			// Barbara McClintock - a distinguished American cytogeneticist, 1983 Nobel Laureate in Physiology or Medicine for discovering transposons. https://en.wikipedia.org/wiki/Barbara_McClintock
			"mcclintock",

			// Anne Laura Dorinthea McLaren - British developmental biologist whose work helped lead to human in-vitro fertilisation. https://en.wikipedia.org/wiki/Anne_McLaren
			"mclaren",

			// Malcolm McLean invented the modern shipping container: https://en.wikipedia.org/wiki/Malcom_McLean
			"mclean",

			// Kay McNulty - one of the original programmers of the ENIAC. https://en.wikipedia.org/wiki/ENIAC - https://en.wikipedia.org/wiki/Kathleen_Antonelli
			"mcnulty",

			// Gregor Johann Mendel - Czech scientist and founder of genetics. https://en.wikipedia.org/wiki/Gregor_Mendel
			"mendel",

			// Dmitri Mendeleev - a chemist and inventor. He formulated the Periodic Law, created a farsighted version of the periodic table of elements, and used it to correct the properties of some already discovered elements and also to predict the properties of eight elements yet to be discovered. https://en.wikipedia.org/wiki/Dmitri_Mendeleev
			"mendeleev",

			// Lise Meitner - Austrian/Swedish physicist who was involved in the discovery of nuclear fission. The element meitnerium is named after her - https://en.wikipedia.org/wiki/Lise_Meitner
			"meitner",

			// Carla Meninsky, was the game designer and programmer for Atari 2600 games Dodge 'Em and Warlords. https://en.wikipedia.org/wiki/Carla_Meninsky
			"meninsky",

			// Ralph C. Merkle - American computer scientist, known for devising Merkle's puzzles - one of the very first schemes for public-key cryptography. Also, inventor of Merkle trees and co-inventor of the Merkle-Damgård construction for building collision-resistant cryptographic hash functions and the Merkle-Hellman knapsack cryptosystem. https://en.wikipedia.org/wiki/Ralph_Merkle
			"merkle",

			// Johanna Mestorf - German prehistoric archaeologist and first female museum director in Germany - https://en.wikipedia.org/wiki/Johanna_Mestorf
			"mestorf",

			// Maryam Mirzakhani - an Iranian mathematician and the first woman to win the Fields Medal. https://en.wikipedia.org/wiki/Maryam_Mirzakhani
			"mirzakhani",

			// Rita Levi-Montalcini - Won Nobel Prize in Physiology or Medicine jointly with colleague Stanley Cohen for the discovery of nerve growth factor (https://en.wikipedia.org/wiki/Rita_Levi-Montalcini)
			"montalcini",

			// Gordon Earle Moore - American engineer, Silicon Valley founding father, author of Moore's law. https://en.wikipedia.org/wiki/Gordon_Moore
			"moore",

			// Samuel Morse - contributed to the invention of a single-wire telegraph system based on European telegraphs and was a co-developer of the Morse code - https://en.wikipedia.org/wiki/Samuel_Morse
			"morse",

			// Ian Murdock - founder of the Debian project - https://en.wikipedia.org/wiki/Ian_Murdock
			"murdock",

			// May-Britt Moser - Nobel prize winner neuroscientist who contributed to the discovery of grid cells in the brain. https://en.wikipedia.org/wiki/May-Britt_Moser
			"moser",

			// John Napier of Merchiston - Scottish landowner known as an astronomer, mathematician and physicist. Best known for his discovery of logarithms. https://en.wikipedia.org/wiki/John_Napier
			"napier",

			// John Forbes Nash, Jr. - American mathematician who made fundamental contributions to game theory, differential geometry, and the study of partial differential equations. https://en.wikipedia.org/wiki/John_Forbes_Nash_Jr.
			"nash",

			// John von Neumann - todays computer architectures are based on the von Neumann architecture. https://en.wikipedia.org/wiki/Von_Neumann_architecture
			"neumann",

			// Isaac Newton invented classic mechanics and modern optics. https://en.wikipedia.org/wiki/Isaac_Newton
			"newton",

			// Florence Nightingale, more prominently known as a nurse, was also the first female member of the Royal Statistical Society and a pioneer in statistical graphics https://en.wikipedia.org/wiki/Florence_Nightingale#Statistics_and_sanitary_reform
			"nightingale",

			// Alfred Nobel - a Swedish chemist, engineer, innovator, and armaments manufacturer (inventor of dynamite) - https://en.wikipedia.org/wiki/Alfred_Nobel
			"nobel",

			// Emmy Noether, German mathematician. Noether's Theorem is named after her. https://en.wikipedia.org/wiki/Emmy_Noether
			"noether",

			// Poppy Northcutt. Poppy Northcutt was the first woman to work as part of NASA’s Mission Control. http://www.businessinsider.com/poppy-northcutt-helped-apollo-astronauts-2014-12?op=1
			"northcutt",

			// Robert Noyce invented silicon integrated circuits and gave Silicon Valley its name. - https://en.wikipedia.org/wiki/Robert_Noyce
			"noyce",

			// Panini - Ancient Indian linguist and grammarian from 4th century CE who worked on the world's first formal system - https://en.wikipedia.org/wiki/P%C4%81%E1%B9%87ini#Comparison_with_modern_formal_systems
			"panini",

			// Ambroise Pare invented modern surgery. https://en.wikipedia.org/wiki/Ambroise_Par%C3%A9
			"pare",

			// Blaise Pascal, French mathematician, physicist, and inventor - https://en.wikipedia.org/wiki/Blaise_Pascal
			"pascal",

			// Louis Pasteur discovered vaccination, fermentation and pasteurization. https://en.wikipedia.org/wiki/Louis_Pasteur.
			"pasteur",

			// Cecilia Payne-Gaposchkin was an astronomer and astrophysicist who, in 1925, proposed in her Ph.D. thesis an explanation for the composition of stars in terms of the relative abundances of hydrogen and helium. https://en.wikipedia.org/wiki/Cecilia_Payne-Gaposchkin
			"payne",

			// Radia Perlman is a software designer and network engineer and most famous for her invention of the spanning-tree protocol (STP). https://en.wikipedia.org/wiki/Radia_Perlman
			"perlman",

			// Rob Pike was a key contributor to Unix, Plan 9, the X graphic system, utf-8, and the Go programming language. https://en.wikipedia.org/wiki/Rob_Pike
			"pike",

			// Henri Poincaré made fundamental contributions in several fields of mathematics. https://en.wikipedia.org/wiki/Henri_Poincar%C3%A9
			"poincare",

			// Laura Poitras is a director and producer whose work, made possible by open source crypto tools, advances the causes of truth and freedom of information by reporting disclosures by whistleblowers such as Edward Snowden. https://en.wikipedia.org/wiki/Laura_Poitras
			"poitras",

			// Tat’yana Avenirovna Proskuriakova (Russian: Татья́на Авени́ровна Проскуряко́ва) (January 23 [O.S. January 10] 1909 – August 30, 1985) was a Russian-American Mayanist scholar and archaeologist who contributed significantly to the deciphering of Maya hieroglyphs, the writing system of the pre-Columbian Maya civilization of Mesoamerica. https://en.wikipedia.org/wiki/Tatiana_Proskouriakoff
			"proskuriakova",

			// Claudius Ptolemy - a Greco-Egyptian writer of Alexandria, known as a mathematician, astronomer, geographer, astrologer, and poet of a single epigram in the Greek Anthology - https://en.wikipedia.org/wiki/Ptolemy
			"ptolemy",

			// C. V. Raman - Indian physicist who won the Nobel Prize in 1930 for proposing the Raman effect. - https://en.wikipedia.org/wiki/C._V._Raman
			"raman",

			// Srinivasa Ramanujan - Indian mathematician and autodidact who made extraordinary contributions to mathematical analysis, number theory, infinite series, and continued fractions. - https://en.wikipedia.org/wiki/Srinivasa_Ramanujan
			"ramanujan",

			// Sally Kristen Ride was an American physicist and astronaut. She was the first American woman in space, and the youngest American astronaut. https://en.wikipedia.org/wiki/Sally_Ride
			"ride",

			// Dennis Ritchie - co-creator of UNIX and the C programming language. - https://en.wikipedia.org/wiki/Dennis_Ritchie
			"ritchie",

			// Ida Rhodes - American pioneer in computer programming, designed the first computer used for Social Security. https://en.wikipedia.org/wiki/Ida_Rhodes
			"rhodes",

			// Julia Hall Bowman Robinson - American mathematician renowned for her contributions to the fields of computability theory and computational complexity theory. https://en.wikipedia.org/wiki/Julia_Robinson
			"robinson",

			// Wilhelm Conrad Röntgen - German physicist who was awarded the first Nobel Prize in Physics in 1901 for the discovery of X-rays (Röntgen rays). https://en.wikipedia.org/wiki/Wilhelm_R%C3%B6ntgen
			"roentgen",

			// Rosalind Franklin - British biophysicist and X-ray crystallographer whose research was critical to the understanding of DNA - https://en.wikipedia.org/wiki/Rosalind_Franklin
			"rosalind",

			// Vera Rubin - American astronomer who pioneered work on galaxy rotation rates. https://en.wikipedia.org/wiki/Vera_Rubin
			"rubin",

			// Meghnad Saha - Indian astrophysicist best known for his development of the Saha equation, used to describe chemical and physical conditions in stars - https://en.wikipedia.org/wiki/Meghnad_Saha
			"saha",

			// Jean E. Sammet developed FORMAC, the first widely used computer language for symbolic manipulation of mathematical formulas. https://en.wikipedia.org/wiki/Jean_E._Sammet
			"sammet",

			// Mildred Sanderson - American mathematician best known for Sanderson's theorem concerning modular invariants. https://en.wikipedia.org/wiki/Mildred_Sanderson
			"sanderson",

			// Satoshi Nakamoto is the name used by the unknown person or group of people who developed bitcoin, authored the bitcoin white paper, and created and deployed bitcoin's original reference implementation. https://en.wikipedia.org/wiki/Satoshi_Nakamoto
			"satoshi",

			// Adi Shamir - Israeli cryptographer whose numerous inventions and contributions to cryptography include the Ferge Fiat Shamir identification scheme, the Rivest Shamir Adleman (RSA) public-key cryptosystem, the Shamir's secret sharing scheme, the breaking of the Merkle-Hellman cryptosystem, the TWINKLE and TWIRL factoring devices and the discovery of differential cryptanalysis (with Eli Biham). https://en.wikipedia.org/wiki/Adi_Shamir
			"shamir",

			// Claude Shannon - The father of information theory and founder of digital circuit design theory. (https://en.wikipedia.org/wiki/Claude_Shannon)
			"shannon",

			// Carol Shaw - Originally an Atari employee, Carol Shaw is said to be the first female video game designer. https://en.wikipedia.org/wiki/Carol_Shaw_(video_game_designer)
			"shaw",

			// Dame Stephanie "Steve" Shirley - Founded a software company in 1962 employing women working from home. https://en.wikipedia.org/wiki/Steve_Shirley
			"shirley",

			// William Shockley co-invented the transistor - https://en.wikipedia.org/wiki/William_Shockley
			"shockley",

			// Lina Solomonovna Stern (or Shtern; Russian: Лина Соломоновна Штерн; 26 August 1878 – 7 March 1968) was a Soviet biochemist, physiologist and humanist whose medical discoveries saved thousands of lives at the fronts of World War II. She is best known for her pioneering work on blood–brain barrier, which she described as hemato-encephalic barrier in 1921. https://en.wikipedia.org/wiki/Lina_Stern
			"shtern",

			// Françoise Barré-Sinoussi - French virologist and Nobel Prize Laureate in Physiology or Medicine; her work was fundamental in identifying HIV as the cause of AIDS. https://en.wikipedia.org/wiki/Fran%C3%A7oise_Barr%C3%A9-Sinoussi
			"sinoussi",

			// Betty Snyder - one of the original programmers of the ENIAC. https://en.wikipedia.org/wiki/ENIAC - https://en.wikipedia.org/wiki/Betty_Holberton
			"snyder",

			// Cynthia Solomon - Pioneer in the fields of artificial intelligence, computer science and educational computing. Known for creation of Logo, an educational programming language.  https://en.wikipedia.org/wiki/Cynthia_Solomon
			"solomon",

			// Frances Spence - one of the original programmers of the ENIAC. https://en.wikipedia.org/wiki/ENIAC - https://en.wikipedia.org/wiki/Frances_Spence
			"spence",

			// Michael Stonebraker is a database research pioneer and architect of Ingres, Postgres, VoltDB and SciDB. Winner of 2014 ACM Turing Award. https://en.wikipedia.org/wiki/Michael_Stonebraker
			"stonebraker",

			// Ivan Edward Sutherland - American computer scientist and Internet pioneer, widely regarded as the father of computer graphics. https://en.wikipedia.org/wiki/Ivan_Sutherland
			"sutherland",

			// Janese Swanson (with others) developed the first of the Carmen Sandiego games. She went on to found Girl Tech. https://en.wikipedia.org/wiki/Janese_Swanson
			"swanson",

			// Aaron Swartz was influential in creating RSS, Markdown, Creative Commons, Reddit, and much of the internet as we know it today. He was devoted to freedom of information on the web. https://en.wikiquote.org/wiki/Aaron_Swartz
			"swartz",

			// Bertha Swirles was a theoretical physicist who made a number of contributions to early quantum theory. https://en.wikipedia.org/wiki/Bertha_Swirles
			"swirles",

			// Helen Brooke Taussig - American cardiologist and founder of the field of paediatric cardiology. https://en.wikipedia.org/wiki/Helen_B._Taussig
			"taussig",

			// Valentina Tereshkova is a Russian engineer, cosmonaut and politician. She was the first woman to fly to space in 1963. In 2013, at the age of 76, she offered to go on a one-way mission to Mars. https://en.wikipedia.org/wiki/Valentina_Tereshkova
			"tereshkova",

			// Nikola Tesla invented the AC electric system and every gadget ever used by a James Bond villain. https://en.wikipedia.org/wiki/Nikola_Tesla
			"tesla",

			// Marie Tharp - American geologist and oceanic cartographer who co-created the first scientific map of the Atlantic Ocean floor. Her work led to the acceptance of the theories of plate tectonics and continental drift. https://en.wikipedia.org/wiki/Marie_Tharp
			"tharp",

			// Ken Thompson - co-creator of UNIX and the C programming language - https://en.wikipedia.org/wiki/Ken_Thompson
			"thompson",

			// Linus Torvalds invented Linux and Git. https://en.wikipedia.org/wiki/Linus_Torvalds
			"torvalds",

			// Youyou Tu - Chinese pharmaceutical chemist and educator known for discovering artemisinin and dihydroartemisinin, used to treat malaria, which has saved millions of lives. Joint winner of the 2015 Nobel Prize in Physiology or Medicine. https://en.wikipedia.org/wiki/Tu_Youyou
			"tu",

			// Alan Turing was a founding father of computer science. https://en.wikipedia.org/wiki/Alan_Turing.
			"turing",

			// Varahamihira - Ancient Indian mathematician who discovered trigonometric formulae during 505-587 CE - https://en.wikipedia.org/wiki/Var%C4%81hamihira#Contributions
			"varahamihira",

			// Dorothy Vaughan was a NASA mathematician and computer programmer on the SCOUT launch vehicle program that put America's first satellites into space - https://en.wikipedia.org/wiki/Dorothy_Vaughan
			"vaughan",

			// Sir Mokshagundam Visvesvaraya - is a notable Indian engineer.  He is a recipient of the Indian Republic's highest honour, the Bharat Ratna, in 1955. On his birthday, 15 September is celebrated as Engineer's Day in India in his memory - https://en.wikipedia.org/wiki/Visvesvaraya
			"visvesvaraya",

			// Christiane Nüsslein-Volhard - German biologist, won Nobel Prize in Physiology or Medicine in 1995 for research on the genetic control of embryonic development. https://en.wikipedia.org/wiki/Christiane_N%C3%BCsslein-Volhard
			"volhard",

			// Cédric Villani - French mathematician, won Fields Medal, Fermat Prize and Poincaré Price for his work in differential geometry and statistical mechanics. https://en.wikipedia.org/wiki/C%C3%A9dric_Villani
			"villani",

			// Marlyn Wescoff - one of the original programmers of the ENIAC. https://en.wikipedia.org/wiki/ENIAC - https://en.wikipedia.org/wiki/Marlyn_Meltzer
			"wescoff",

			// Sylvia B. Wilbur - British computer scientist who helped develop the ARPANET, was one of the first to exchange email in the UK and a leading researcher in computer-supported collaborative work. https://en.wikipedia.org/wiki/Sylvia_Wilbur
			"wilbur",

			// Andrew Wiles - Notable British mathematician who proved the enigmatic Fermat's Last Theorem - https://en.wikipedia.org/wiki/Andrew_Wiles
			"wiles",

			// Roberta Williams, did pioneering work in graphical adventure games for personal computers, particularly the King's Quest series. https://en.wikipedia.org/wiki/Roberta_Williams
			"williams",

			// Malcolm John Williamson - British mathematician and cryptographer employed by the GCHQ. Developed in 1974 what is now known as Diffie-Hellman key exchange (Diffie and Hellman first published the scheme in 1976). https://en.wikipedia.org/wiki/Malcolm_J._Williamson
			"williamson",

			// Sophie Wilson designed the first Acorn Micro-Computer and the instruction set for ARM processors. https://en.wikipedia.org/wiki/Sophie_Wilson
			"wilson",

			// Jeannette Wing - co-developed the Liskov substitution principle. - https://en.wikipedia.org/wiki/Jeannette_Wing
			"wing",

			// Steve Wozniak invented the Apple I and Apple II. https://en.wikipedia.org/wiki/Steve_Wozniak
			"wozniak",

			// The Wright brothers, Orville and Wilbur - credited with inventing and building the world's first successful airplane and making the first controlled, powered and sustained heavier-than-air human flight - https://en.wikipedia.org/wiki/Wright_brothers
			"wright",

			// Chien-Shiung Wu - Chinese-American experimental physicist who made significant contributions to nuclear physics. https://en.wikipedia.org/wiki/Chien-Shiung_Wu
			"wu",

			// Rosalyn Sussman Yalow - Rosalyn Sussman Yalow was an American medical physicist, and a co-winner of the 1977 Nobel Prize in Physiology or Medicine for development of the radioimmunoassay technique. https://en.wikipedia.org/wiki/Rosalyn_Sussman_Yalow
			"yalow",

			// Ada Yonath - an Israeli crystallographer, the first woman from the Middle East to win a Nobel prize in the sciences. https://en.wikipedia.org/wiki/Ada_Yonath
			"yonath",

			// Nikolay Yegorovich Zhukovsky (Russian: Никола́й Его́рович Жуко́вский, January 17 1847 – March 17, 1921) was a Russian scientist, mathematician and engineer, and a founding father of modern aero- and hydrodynamics. Whereas contemporary scientists scoffed at the idea of human flight, Zhukovsky was the first to undertake the study of airflow. He is often called the Father of Russian Aviation. https://en.wikipedia.org/wiki/Nikolay_Yegorovich_Zhukovsky
			"zhukovsky",
        };

        #endregion Moby name constants

        [HttpGet("name/moby")]
        public IActionResult MobyName([FromQuery] int? seed = default)
        {
            if (seed < 0)
            {
                seed = Math.Abs(seed.Value);
            }
            else if (seed == default)
            {
                seed = Random.Shared.Next();
            }

            return this.Ok(mobyNameAdjectives[seed.Value % mobyNameAdjectives.Length] + '_' + mobyNameNames[seed.Value % mobyNameNames.Length]);
        }

        // Fetched 2025-05-27 from https://wiki.ubuntu.com/DevelopmentCodeNames#List_of_Adjectives_and_Animals
        // Parentheses expanded into multiple terms.
        #region Ubuntu name constants

        private static readonly string[] ubuntuNameAdjectives = new string[]
        {
            "Able",
            "Acute",
            "Adventurous",
            "Aggravated",
            "Agile",
            "Alcoholic",
            "Alliterating",
            "Amazing",
            "Ambling",
            "Amenable",
            "Amicable",
            "Amok",
            "Anarchic",
            "Androgynous",
            "Angelic",
            "Annoying",
            "Antidisestablishmentarian",
            "Antlered",
            "Antsy",
            "Archaic",
            "Arty",
            "Atomic",
            "Atrophying",
            "Aware",
            "Awesome",
            "Aztec",
            "Ballsy",
            "Barking",
            "Batty",
            "Belching",
            "Bellicose",
            "Bewildered",
            "Blustering",
            "Boggling",
            "Boisterous",
            "Bonkers",
            "Bounding",
            "Brainy",
            "Brave",
            "Brazen",
            "Breezy",
            "Bright",
            "Brilliant",
            "Bumpy",
            "Busy",
            "Calculating",
            "Callous",
            "Campy",
            "Canny",
            "Canonical",
            "Careful",
            "Cavalier",
            "Celebrating",
            "Charming",
            "Cheeky",
            "Cheerful",
            "Cheesy",
            "Chirping",
            "Chocolate",
            "Chronic",
            "Chummy",
            "Clever",
            "Clumsy",
            "Colonel",
            "Constipated",
            "Cool",
            "Courageous",
            "Crabby",
            "Crafty",
            "Cranky",
            "Crazy",
            "Crooked",
            "Cruising",
            "Cuddly",
            "Culpable",
            "Cunning",
            "Curt",
            "Cute",
            "Dangerous",
            "Dapper",
            "Daring",
            "Dastardly",
            "Daunting",
            "Dazzling",
            "Defiant",
            "Diligent",
            "Disaffected",
            "Disorderly",
            "Dogmatic",
            "Dreamy",
            "Drowsy",
            "Durable",
            "Dutiful",
            "Eager",
            "Early",
            "Earnest",
            "Easy",
            "Eccentric",
            "Eclectic",
            "Edgy",
            "Eel",
            "Eerie",
            "Effervescent",
            "Eked",
            "Electric",
            "Elegant",
            "Elite",
            "Eloquent",
            "Embraceable",
            "Eminent",
            "Enlightened",
            "Enoumous",
            "Ergonomic",
            "Erratic",
            "Euphoric",
            "Evangelizing",
            "Exquisite",
            "Extrovert",
            "Fair",
            "Fairies",
            "Fanatical",
            "Fantastic",
            "Farsighted",
            "Fast",
            "Fatuous",
            "Fecund",
            "Feisty",
            "Ferocious",
            "Festive",
            "Fierce",
            "Fiery",
            "Fishy",
            "Fitchew",
            "Flakey",
            "Flamboyant",
            "Flashy",
            "Flippant",
            "Flirty",
            "Fluent",
            "Flying",
            "Foxy",
            "Freaky",
            "Friendly",
            "Frisky",
            "Froody",
            "Fruity",
            "Functional",
            "Funky",
            "Furious",
            "Furry",
            "Fuzzy",
            "Gaia",
            "Gallant",
            "Galloping",
            "Gallus",
            "Gargantuan",
            "Gelatinous",
            "Genderqueer",
            "Giddy",
            "Giggling",
            "Glad",
            "Gleaming",
            "Glorious",
            "Gnathal",
            "Gnathonic",
            "Gnomic",
            "Gnostic",
            "Gol",
            "Gooey",
            "Goofy",
            "Gorgeous",
            "Graceful",
            "Gracious",
            "Great",
            "Greedy",
            "Gregarious",
            "Grinning",
            "Gritty",
            "Grizzly",
            "Groovy",
            "Grumpy",
            "Gutsy",
            "Hairy",
            "Happy",
            "Hardy",
            "Haughty",
            "Hazardous",
            "Helpful",
            "Helpless",
            "Hilarious",
            "Hippy",
            "Hoary",
            "Holy",
            "Hoopy",
            "Hopping",
            "Horned",
            "Horny",
            "Horrible",
            "Hot",
            "Howling",
            "Huge",
            "Humble",
            "Humourous",
            "Humpin",
            "Hungry",
            "Hyperactive",
            "Icky",
            "Icy",
            "Idyllic",
            "Iffy",
            "Igneous",
            "Ignited",
            "Illuminating",
            "Illustrious",
            "Immodest",
            "Immortal",
            "Impish",
            "Impressive",
            "In one's way",
            "In the way",
            "Incendiary",
            "Incomparable",
            "Inconceivable!",
            "Incontinent",
            "Incredible",
            "Incriminatory",
            "Indefatigable",
            "Indelible",
            "Industrious",
            "Inebriated",
            "Ineffable",
            "Inescapable",
            "Inestimable",
            "Inevitable",
            "Inexorable",
            "Infallible",
            "Inflammatory",
            "Inflationary",
            "Ingenious",
            "Ingratiating",
            "Initiatory",
            "Inky",
            "Inner-city",
            "Innocent",
            "Innovatory",
            "Inquisitive",
            "Insanitary",
            "Insatiable",
            "Inscrutable",
            "Insightful",
            "Insolent",
            "Insouciant",
            "Inspirational",
            "Inspired",
            "Insurgent",
            "Intelligent",
            "Interagency",
            "Intercalary",
            "Intercessory",
            "Intercity",
            "Intermediary",
            "Interstellar",
            "Intervarsity",
            "Intimate",
            "Intimidated",
            "Intrepid",
            "Inverted-snobbery",
            "Investigatory",
            "Irate",
            "Iridescent",
            "Irie",
            "Irksome",
            "Irrefutable",
            "Itchy",
            "Itsy",
            "Ivory",
            "Jabbering",
            "Jaded",
            "Jaundice",
            "Jaunty",
            "Jazzy",
            "Jealous",
            "Jiggly",
            "Jinchira",
            "Jittery",
            "Jiving",
            "Jocular",
            "Jocund",
            "Jodeling",
            "Jogging",
            "Jolly",
            "Jolted",
            "Jolty",
            "Jousting",
            "Jovial",
            "Joyous",
            "Jubilant",
            "Judicious",
            "Juggling",
            "Juicy",
            "Jumbled",
            "Jumping",
            "Jumpity",
            "Jumpy",
            "Jungle",
            "Jungly",
            "Jurassic",
            "Juvenile",
            "Kafkaesque",
            "Kaleidoscopic",
            "Kamikaze",
            "Kanny",
            "Karmic",
            "Katie",
            "Kayoed",
            "Keeking",
            "Keen",
            "Keepable",
            "Kempy",
            "Khaki",
            "Kick-ass",
            "Kicking",
            "Kicky",
            "Killer",
            "Kilted",
            "Kind",
            "Kindhearted",
            "Kinetic",
            "King-sized",
            "Kingly",
            "Kinky",
            "Kissable",
            "Kissy",
            "Kitschy",
            "Kleptomaniacal",
            "Klutzy",
            "Knavish",
            "Knightly",
            "Knobby",
            "Knotty",
            "Kooky",
            "Kurt",
            "Kvetching",
            "Lampooning",
            "Languid",
            "Lascivious",
            "Laudable",
            "Laughable",
            "Laughing",
            "Lazy",
            "Leaky",
            "Leal",
            "Leaping",
            "Leapy",
            "Leftist",
            "Lethal",
            "Liberal",
            "Limp",
            "Lingual",
            "Listless",
            "Lithe",
            "Litigious",
            "Lively",
            "Lofty",
            "Lonely",
            "Longhorn",
            "Loopy",
            "Loquacious",
            "Lordly",
            "Lovely",
            "Lucid",
            "Lucky",
            "Lugubrious",
            "Luminiferous",
            "Luminous",
            "Lurid",
            "Luscious",
            "Lusty",
            "Macular",
            "Mad",
            "Magical",
            "Magistical",
            "Magnanimous",
            "Magnetic",
            "Magnificent",
            "Maieutic",
            "Majestic",
            "Majim",
            "Majuscul",
            "Malapert",
            "Malicious",
            "Malodorous",
            "Malty",
            "Mammoth",
            "Mangy",
            "Manic",
            "Marauding",
            "Marvelous",
            "Massive",
            "Masterful",
            "Maudlin",
            "Maverick",
            "Mazarine",
            "Meandering",
            "Measured",
            "Meaty",
            "Melancholic",
            "Melic",
            "Melliferous",
            "Mellifluous",
            "Mellow",
            "Melodic",
            "Melodious",
            "Memorable",
            "Menacing",
            "Menstrual",
            "Mental",
            "Mercantile",
            "Mercenary",
            "Merciful",
            "Merciless",
            "Mercurial",
            "Meridian",
            "Merry",
            "Mesmeric",
            "Metallic",
            "Methodic",
            "Methodical",
            "Meticulous",
            "Metric",
            "Mettled",
            "Mickle",
            "Mighty",
            "Mild",
            "Militant",
            "Mindful",
            "Mini",
            "Minimalist",
            "Minty",
            "Miraculous",
            "Mischievous",
            "Misty",
            "Mobile",
            "Modest",
            "Modish",
            "Momentous",
            "Monadic",
            "Monastic",
            "Monty",
            "Moonlight",
            "Moral",
            "Mordant",
            "Moxious",
            "Murphy",
            "Muscled",
            "Mushy",
            "Musical",
            "Musky",
            "Muttering",
            "Myriad",
            "Mysterious",
            "Mystic",
            "Mystical",
            "Mythical",
            "Nano",
            "Nasty",
            "Natty",
            "Naughty",
            "Nauseous",
            "Necrotising",
            "Nefarious",
            "Nerdy",
            "Nested",
            "Neurotic",
            "Nifty",
            "Nimble",
            "Nippy",
            "Noble",
            "Nocturnal",
            "Nomadic",
            "Normal",
            "Normandic",
            "Nostalgic",
            "Novel",
            "Null",
            "Nutritious",
            "Obedient",
            "Oblivious",
            "Obnoxious",
            "Obsequious",
            "Observing",
            "Obstreperous",
            "Obtuse",
            "Obverse",
            "Obvious",
            "Occult",
            "Octal",
            "Octennial",
            "Odd",
            "Oily",
            "Okay",
            "Omnific",
            "Omniscient",
            "Oneiric",
            "Opportune",
            "Opportunistic",
            "Opulent",
            "Oral",
            "Orderly",
            "Ordinary",
            "Orgasmic",
            "Orgiastic",
            "Origamist",
            "Ornery",
            "Orthodox",
            "Orthogenic",
            "Oscillating",
            "Ostentatious",
            "Pacific",
            "Panacean",
            "Parabolic",
            "Paradisaic",
            "Paramount",
            "Passive",
            "Peachy",
            "Pedantic",
            "Perceptive",
            "Perennial",
            "Perky",
            "Persistent",
            "Persnickety",
            "Perverse",
            "Phantastic",
            "Phantom",
            "Phat",
            "Philantropic",
            "Philosophical",
            "Phlegmatic",
            "Phlegmatic",
            "Phobic",
            "Phooka",
            "Photogenic",
            "Pickled",
            "Pimping",
            "Pink Panther",
            "Plastered",
            "Plastic",
            "Playful",
            "Plonky",
            "Plucky",
            "Plutonic",
            "Poetic",
            "Pointy",
            "Pokey",
            "Polished",
            "Polyplastic",
            "Posh",
            "Powerful",
            "Powersaver",
            "Pragmatic",
            "Prancing",
            "Precise",
            "Precocious",
            "Predatory",
            "Predictive",
            "Predominate",
            "Prestidigious",
            "Pretty",
            "Priapic",
            "Prickly",
            "Prideful",
            "Prissy",
            "Pristine",
            "Proactive",
            "Prolific",
            "Proud",
            "Prowly",
            "Prudent",
            "Psychedelic",
            "Psychic",
            "Pulchritudinous",
            "Purring",
            "Pyroclastic",
            "Pyromaniac",
            "Pyrotechnic",
            "Quaint",
            "Quaking",
            "Quantal",
            "Quantum",
            "Quarter",
            "Queer",
            "Querulous",
            "Quick",
            "Quiet",
            "Quintessential",
            "Quirky",
            "Quivering",
            "Quixotic",
            "Rabid",
            "Racey",
            "Racing",
            "Radiant",
            "Radical",
            "Rambunctious",
            "Rampant",
            "Rancid",
            "Randy",
            "Rapid",
            "Raring",
            "Rational",
            "Raucous",
            "Raunchy",
            "Reasonable",
            "Rebel",
            "Rebellious",
            "Rednosed",
            "Reminiscent",
            "Resonant",
            "Rhapsy",
            "Ribald",
            "Rich",
            "Risky",
            "Roasted",
            "Robust",
            "Rolly",
            "Rowdy",
            "Rugged",
            "Runcible",
            "Ruthless",
            "Saccate",
            "Saccharine",
            "Sagacious",
            "Sage",
            "Salivating",
            "Salubrious",
            "Sanguine",
            "Sarcastic",
            "Sardonic",
            "Sassy",
            "Saucy",
            "Scary",
            "Scrawny",
            "Screwy",
            "Secretive",
            "Selenographic",
            "Selenomorphic",
            "Sensual",
            "Sentimental",
            "Serendipitous",
            "Serious",
            "Servile",
            "Severe",
            "Sexy",
            "Sharp",
            "Shiny",
            "Shrewd",
            "Silly",
            "Skinny",
            "Slick",
            "Slim",
            "Slimmy",
            "Slinky",
            "Slippery",
            "Slippy",
            "Smelly",
            "Smiling",
            "Smitten",
            "Smooth",
            "Snappy",
            "Snazzy",
            "Sneaky",
            "Soaring",
            "Solid",
            "Sophisticated",
            "Sound",
            "Spastic",
            "Special",
            "Spectacular",
            "Speedy",
            "Spicy",
            "Spooky",
            "Spotty",
            "Sprightly",
            "Spunky",
            "Squidgy",
            "Squirly",
            "Stalwart",
            "Stealthy",
            "Stelene",
            "Steward",
            "Stocky",
            "Strapping",
            "Streetwise",
            "Stressful",
            "Striking",
            "Stunning",
            "Stupendous",
            "Sturdy",
            "Suave",
            "Suctorial",
            "Sudden",
            "Sultry",
            "Superstitious",
            "Surreptitious",
            "Swarthy",
            "Tabby",
            "Tactful",
            "Talented",
            "Tame",
            "Tangible",
            "Tangled",
            "Tanked",
            "Tantric",
            "Tasteful",
            "Tasty",
            "Taught",
            "Teachable",
            "Tedious",
            "Teeming",
            "Tempean",
            "Temperamental",
            "Temperate",
            "Tenable",
            "Tenacious",
            "Tender",
            "Terrific",
            "Thankful",
            "Therapeutic",
            "Thorough",
            "Thoughtful",
            "Thrashing",
            "Thrifty",
            "Thrilled",
            "Thrilling",
            "Thriving",
            "Ticklish",
            "Tidy",
            "Tight",
            "Timeless",
            "Timely",
            "Tiny",
            "Tipsy",
            "Tireless",
            "Tolerant",
            "Touching",
            "Tough",
            "Touted",
            "Trailblazing",
            "Tranquil",
            "Transcendent",
            "Transcendental",
            "Transgendered",
            "Transparent",
            "Transpicuous",
            "Transsexual",
            "Traumatized",
            "Traveled",
            "Treasured",
            "Tremendous",
            "Tretis",
            "Tricky",
            "Trim",
            "Trippy",
            "Triumphant",
            "Truculent",
            "True",
            "Trusted",
            "Trustful",
            "Trustworthy",
            "Trusty",
            "Truthful",
            "Tweaky",
            "Twirling",
            "Twitchy",
            "Tympanic",
            "Ubiquitous",
            "Ubuntu",
            "Ugly",
            "Ultimate",
            "Ultrarelativistic",
            "Unadulterated",
            "Unapologetic",
            "Unassailable",
            "Unassuming",
            "Unbelievable",
            "Unctuous",
            "Undead",
            "Underrated",
            "Underrepresented",
            "Understanding",
            "Undulating",
            "Unplugged",
            "Unpretentious",
            "Unrated",
            "Unrepentant",
            "Unruly",
            "Untamed",
            "Untapped",
            "Uppity",
            "Uproarious",
            "Utopic",
            "Valiant",
            "Vast",
            "Veracious",
            "Versatile",
            "Veteran",
            "Vibrant",
            "Violent",
            "Viral",
            "Virtuoso",
            "Vitriolic",
            "Vivacious",
            "Vivid",
            "Vociferous",
            "Volatile",
            "Voluptuous",
            "Vomiting",
            "Voracious",
            "Wandering",
            "Warm",
            "Warty",
            "Western Tragopan",
            "Wild",
            "Wily",
            "Wimpy",
            "Windy",
            "Witty",
            "Wobbly",
            "Wonderful",
            "Woolly",
            "Xanthic",
            "Xenial",
            "Xenodochial",
            "Xenolithic",
            "Xenophobic",
            "Xeric",
            "Xintillating",
            "Xiphoid",
            "Xylographic",
            "Xylophonic",
            "Yakkity",
            "Yammering",
            "Yelping",
            "Yiffy",
            "Yippity",
            "Yodeling",
            "Youthful",
            "Yummy",
            "Zany",
            "Zarking",
            "Zealous",
            "Zen",
            "Zesty",
            "Zippy",
            "Zooming",
        };

        private static readonly string[] ubuntuNameAnimals = new string[]
        {
            "Aal",
            "Aardvark",
            "Affenpinscher",
            "Agama",
            "Albatross",
            "Alebrije",
            "Alien",
            "Alligator",
            "Alliteration",
            "Alpaca",
            "Amoeba",
            "Anemone",
            "Angelfish",
            "Anoa",
            "Ant",
            "Anteater",
            "Antelope",
            "Ape",
            "Arachnid",
            "Armadillo",
            "Asp",
            "Axolotl",
            "Baboon",
            "Badger",
            "Barnacle",
            "Barracuda",
            "Bat",
            "Beagle",
            "Bear",
            "Beaver",
            "Bee",
            "Beetle",
            "Bison",
            "Bitch",
            "Bittern",
            "Blackbird",
            "Blowfish",
            "Bluejay",
            "Bobcat",
            "Bonobo",
            "Bothrop",
            "Bowerbird",
            "Buck",
            "Bugblatter Beast of Traal",
            "Bull",
            "Bunny",
            "Camel",
            "Capybara",
            "Caracal",
            "Cardinal",
            "Caribou",
            "Cassowary",
            "Cat",
            "Caterpillar",
            "Centipede",
            "Chameleon",
            "Chapulín",
            "Cheetah",
            "Chickadee",
            "Chicken",
            "Chimp",
            "Chinchilla",
            "Chipmunk",
            "Chupacabra",
            "Coati",
            "Cobra",
            "Cockatoo",
            "Condor",
            "Cony",
            "Coot",
            "Cougar",
            "Cow",
            "Coyote",
            "Crab",
            "Crane",
            "Crocodile",
            "Crow",
            "Deer",
            "Devil",
            "Dingo",
            "Diplodocus",
            "Dodo",
            "Doe",
            "Dog",
            "Dolphin",
            "Donkey",
            "Dormouse",
            "Dove",
            "Dragon",
            "Drake",
            "Drop Bear",
            "Dryad",
            "Duck",
            "Dugong",
            "Dungbeetle",
            "Dzo",
            "Eagle",
            "Eagle",
            "Earthworm",
            "Earwig",
            "Echidna",
            "Eel",
            "Eft",
            "Eland",
            "Elephant",
            "Elf",
            "Elk",
            "Emu",
            "Ermine",
            "Escargot",
            "Euglena",
            "Ewe",
            "Falcon",
            "Fawn",
            "Feline",
            "Fenestrator",
            "Fenix",
            "Fennec",
            "Ferret",
            "Fieldmouse",
            "Finch",
            "Finfoot",
            "Firefly",
            "Fish",
            "Flamingo",
            "Flight",
            "Fly",
            "Foundling",
            "Fox",
            "Frog",
            "Fruitbat",
            "Fugu",
            "Furbolg",
            "Galah",
            "Gannet",
            "Gappu",
            "Gator",
            "Gazelle",
            "Gecko",
            "Gelding",
            "Gerbil",
            "Ghost",
            "Gibbon",
            "Giraffe",
            "Glider",
            "Gnarwhal",
            "Gnat",
            "Gnu",
            "Goat",
            "Gobbler",
            "Goblin",
            "Goldfinch",
            "Goose",
            "Gopher",
            "Gorilla",
            "Goshawk",
            "Grebe",
            "Groundhog",
            "Grue",
            "Gryphon",
            "Guanaco",
            "Gull",
            "Guppy",
            "Gurk",
            "Hamster",
            "Hare",
            "Harrier",
            "Hartebeest",
            "Hawk",
            "Hedgehog",
            "Heffalump",
            "Hen",
            "Heron",
            "Hippo",
            "Hippopotamus",
            "Hoopoe",
            "Hornet",
            "Horse",
            "Horus",
            "Hounddog",
            "Human",
            "Hummingbird",
            "Humpback",
            "Husky",
            "Hyena",
            "Hypnotoad",
            "Ibex",
            "Ibis",
            "Icebear",
            "Iceweasel",
            "Ichabodcraniosaurus",
            "Ichthyosaur",
            "Ichtiosaurus",
            "Ifrit",
            "Iguana",
            "Iguana",
            "Iguanodon",
            "Iguanoides",
            "Iguanosaurus",
            "Iliosuchus",
            "Ilokelesia",
            "Imp",
            "Impala",
            "Incisivosaurus",
            "Indosaurus",
            "Indosuchus",
            "Indri",
            "Indri",
            "Ingenia",
            "Inkanyamba",
            "Inosaurus",
            "Insect",
            "Irritator",
            "Irukandji",
            "Isanosaurus",
            "Ischisaurus",
            "Ischyrosaurus",
            "Isisaurus",
            "Isopoda",
            "Issasaurus",
            "Itemirus",
            "Iuticosaurus",
            "Ivory",
            "Jabberwock",
            "Jacana",
            "Jackal",
            "Jackalope",
            "Jackass",
            "Jackrabbit",
            "Jackrabbit",
            "Jaguar",
            "Javelina",
            "Jay",
            "Jaybird",
            "Jellyfish",
            "Jentink's",
            "Jerboa",
            "Joey",
            "Jubata",
            "Junco",
            "Junebug",
            "Kakapo",
            "Kalong",
            "Kangaroo",
            "Katydid",
            "Kea",
            "Kelpie",
            "Kestrel",
            "Kid",
            "Killdeer",
            "Kingfisher",
            "Kinkajou",
            "Kite",
            "Kitten",
            "Kittie",
            "Kittiwake",
            "Kittyhawk",
            "Kiwi",
            "Knight",
            "Koala",
            "Kodiak",
            "Kodkod",
            "Koi",
            "Kolibri",
            "Komodo",
            "Konqui",
            "Kookaburra",
            "Kouprey",
            "Kowari",
            "Kraut",
            "Krocodile",
            "Kudu",
            "Labrador",
            "Ladybug",
            "Lagomorph",
            "Lamantin",
            "Lamb",
            "Lamprey",
            "Lapwing",
            "Lark",
            "Leafhopper",
            "Leech",
            "Lemming",
            "Lemur",
            "Leopard",
            "Leopon",
            "Liger",
            "Limpet",
            "Lion",
            "Lizard",
            "Llama",
            "Lobster",
            "Longhorn",
            "Loompa",
            "Loon",
            "Lorax",
            "Loris",
            "Louse",
            "Lynx",
            "Macaque",
            "Macaw",
            "Macropod",
            "Maggots",
            "Magpie",
            "Mallard",
            "Mammoth",
            "Man",
            "Manatee",
            "Mandrill",
            "Manta",
            "Mantis",
            "Mara",
            "Markhor",
            "Marlin",
            "Marmot",
            "Marsupilami",
            "Marten",
            "Mastiff",
            "Meadowlark",
            "Meerkat",
            "Millipede",
            "Mink",
            "Minnow",
            "Mobutu",
            "Mockingbird",
            "Mole",
            "Mollusk",
            "Molly",
            "Monal",
            "Monca",
            "Mongoose",
            "Monitor",
            "Monkey",
            "Moose",
            "Mosquito",
            "Moth",
            "Motmot",
            "Mouflon",
            "Mouse",
            "Mule",
            "Muskox",
            "Muskrat",
            "Mustang",
            "Mutt",
            "Myna",
            "Naiad",
            "Narwhal",
            "Newt",
            "Nicholine",
            "Nightelf",
            "Nightingale",
            "Nightjar",
            "Ninf",
            "Numbat",
            "Nutria",
            "Nymph",
            "Ocelot",
            "Octopus",
            "Okapi",
            "Oompa-loompa",
            "Opossum",
            "Orangutan",
            "Orc",
            "Orca",
            "Oriole",
            "Oryx",
            "Ostrich",
            "Otter",
            "Owl",
            "Ox",
            "Oyster",
            "Pademelon",
            "Panda",
            "Pangolin",
            "Panther",
            "Parakeet",
            "Parrot",
            "Partridge",
            "Peacock",
            "Peafowl",
            "Peccary",
            "Pegasus",
            "Pelican",
            "Penguin",
            "Pheasant",
            "Phoenix",
            "Pichi",
            "Pig",
            "Pigeon",
            "Piglet",
            "Pika",
            "Piranha",
            "Piraña",
            "Pitbull",
            "Pixi",
            "Platypus",
            "Polecat",
            "Pony",
            "Porcupine",
            "Porpoise",
            "Possum",
            "Primate",
            "Pronghorn",
            "Protozoa",
            "Puffin",
            "Puma",
            "Python",
            "Qantassaurus",
            "Quagga",
            "Quahog",
            "Quail",
            "Quetzal",
            "Quetzalcoatl",
            "Quokka",
            "Quoll",
            "Rabbit",
            "Raccoon",
            "Raptor",
            "Raptor",
            "Rat",
            "Ratel",
            "Rattlesnake",
            "Raven",
            "Redbird",
            "Reindeer",
            "Rhea",
            "Rhino",
            "Ringtail",
            "Roach",
            "Roadrunner",
            "Robin",
            "Rook",
            "Rooster",
            "Rottweiler",
            "Sabretooth",
            "Salamander",
            "Salmon",
            "Saola",
            "Sasquatch",
            "Satyr",
            "Scallop",
            "Scorpion",
            "Scorpionfish",
            "Scottie",
            "Seadonkey",
            "Seagull",
            "Seahorse",
            "Seahorse",
            "Seal",
            "Serval",
            "Shark",
            "Sheep",
            "Shih Tzu",
            "Shrew",
            "Silverfish",
            "Sitatunga",
            "Skunk",
            "Slippershell",
            "Sloth",
            "Snail",
            "Snake",
            "Snake",
            "Snapper",
            "Snapping Turtle",
            "Snow Leopard",
            "Spaniel",
            "Sparrow",
            "Spider",
            "Springbok",
            "Squid",
            "Squirrel",
            "Stallion",
            "Starfish",
            "Stingray",
            "Sukko",
            "Suricata",
            "Suricate",
            "Swallow",
            "Swan",
            "Swordfish",
            "Tahr",
            "Taipan",
            "Takahe",
            "Tamandua",
            "Tapir",
            "Tarantula",
            "Tarsier",
            "Tasmanian Devil",
            "Tassie Thylacine",
            "Tazzy Thylacine",
            "Tenrec",
            "Termite",
            "Terrapin",
            "Terrier",
            "Tesselated",
            "Tiger",
            "Tigerfish",
            "Tigon",
            "Toad",
            "Tortoise",
            "Toucan",
            "Trogon",
            "Troll",
            "Trout",
            "Tuatara",
            "Turkey",
            "Turtle",
            "Uakari",
            "Ubuntu",
            "Uglybird",
            "Uguisu",
            "Uintathere",
            "Umbrian",
            "Ungulate",
            "Unicorn",
            "Urchin",
            "Urial",
            "Urubu",
            "Urukai",
            "Utonagan",
            "Veery",
            "Velociraptor",
            "Vervet",
            "Vicuña",
            "Viper",
            "Vixen",
            "Vizcacha",
            "Vole",
            "Vulture",
            "Wallaby",
            "Walrus",
            "Warthog",
            "Wasp",
            "Weasel",
            "Weevil",
            "Weka",
            "Werewolf",
            "Whale",
            "Wildebeest",
            "Wolf",
            "Wolverine",
            "Wombat",
            "Woodchuck",
            "Woodpeck",
            "Worm",
            "Wren",
            "X-ray fish",
            "Xebu",
            "Xenarthra",
            "Xenomorph",
            "Xenops",
            "Xenoturbella",
            "Xerus",
            "Xiphias",
            "Xipho",
            "Xoloitzcuintle",
            "Xue Bao",
            "Xólotl",
            "Yaffle",
            "Yak",
            "Yellowjacket",
            "Yeti",
            "Zealot",
            "Zebra",
            "Zebrafish",
            "Zebu",
            "Zorilla",
            "Zorse",
            "lycaon",
        };

        #endregion Ubuntu name constants

        [HttpGet("name/ubuntu")]
        public IActionResult UbuntuName([FromQuery] int? seed = default)
        {
            if (seed < 0)
            {
                seed = Math.Abs(seed.Value);
            }
            else if (seed == default)
            {
                seed = Random.Shared.Next();
            }

            return this.Ok(ubuntuNameAdjectives[seed.Value % ubuntuNameAdjectives.Length] + ' ' + ubuntuNameAnimals[seed.Value % ubuntuNameAnimals.Length]);
        }

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

            fromValues.Shuffle();
            return this.Ok(fromValues);
        }

        // Fetched 2023-06-09
        #region Wot Quotes

        private static readonly string[] wotQuotes = new string[]
        {
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
        };

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

        [HttpGet("coin")]
        public IActionResult Coin()
        {
            return this.Ok(Random.Shared.Next() % 2 == 0
                            ? "Heads"
                            : "Tails");
        }

        // Initialized 2025-03-20
        #region Razors

        private static readonly Razor[] razors = new Razor[]
        {
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
        };
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

        // https://xkcd.com/221/
        [HttpGet("xkcd")]
        public IActionResult Xkcd() => this.Ok(4);
    }
}
