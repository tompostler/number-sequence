using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Web;
using TcpWtf.NumberSequence.Contracts;
using Unlimitedinf.Utilities;

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

            fromValues = [.. Enumerable.Shuffle(fromValues)];
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

        // Scraped 2026-03-15 from https://criminalmindsquotes.com/
        #region CriminalMinds

        private record CriminalMindsQuote(int Season, int Episode, string EpisodeTitle, int Order, string Quote, string ReadBy, string Attribution);

        private static readonly CriminalMindsQuote[] criminalMinds =
        [
            new(1, 1, "Extreme Aggressor", 1, "The belief in a supernatural source of evil is not necessary. Men alone are quite capable of every wickedness.", "Jason Gideon", "Joseph Conrad"),
            new(1, 1, "Extreme Aggressor", 2, "All is a riddle, and the key to a riddle… is another riddle.", "Jason Gideon", "Ralph Waldo Emerson"),
            new(1, 1, "Extreme Aggressor", 3, "Try again, fail again. Fail better.", "Jason Gideon", "Samuel Beckett"),
            new(1, 1, "Extreme Aggressor", 4, "Try not. Do or do not.", "Derek Morgan", "Yoda"),
            new(1, 1, "Extreme Aggressor", 5, "The farther backward you can look, the farther forward you will see.", "Jason Gideon", "Winston Churchill"),
            new(1, 1, "Extreme Aggressor", 6, "When you look long into an abyss, the abyss looks into you.", "Jason Gideon", "Friedrich Nietzsche"),
            new(1, 2, "Compulsion", 1, "Imagination is more important than knowledge. Knowledge is limited. Imagination encircles the world.", "Jason Gideon", "Albert Einstein"),
            new(1, 2, "Compulsion", 2, "There are certain clues at a crime scene which, by their very nature, do not lend themselves to being collected or examined. How does one collect love, rage, hatred, fear…? These are things that we’re trained to look for.", "Jason Gideon", "James Reese"),
            new(1, 2, "Compulsion", 3, "Don’t bother just to be better than your contemporaries or predecessors. Try to be better than yourself.", "Jason Gideon", "William Faulkner"),
            new(1, 3, "Won’t Get Fooled Again", 1, "Almost all absurdity of conduct arises from the imitation of those whom we cannot resemble.", "Jason Gideon", "Samuel Johnson"),
            new(1, 4, "Plain Sight", 1, "Don’t forget that I cannot see myself — that my role is limited to being the one who looks in the mirror.", "Jason Gideon", "Jacques Rigaut"),
            new(1, 4, "Plain Sight", 2, "Birds sing after a storm. Why shouldn’t people feel as free to delight in whatever sunlight remains to them?", "Jason Gideon", "Rose Kennedy"),
            new(1, 5, "Broken Mirror", 1, "When a good man is hurt, all who would be called good must suffer with him.", "Jason Gideon", "Euripides"),
            new(1, 5, "Broken Mirror", 2, "When love is in excess, it brings a man no honor nor worthiness.", "Jason Gideon", "Euripides"),
            new(1, 6, "L.D.S.K.", 1, "The irrationality of a thing is not an argument against its existence, rather a condition of it.", "Jason Gideon", "Friedrich Nietzsche"),
            new(1, 6, "L.D.S.K.", 2, "Nothing is so common as the wish to be remarkable.", "Aaron Hotchner", "William Shakespeare"),
            new(1, 7, "The Fox", 1, "With foxes, we must play the fox.", "Jason Gideon", "Thomas Fuller"),
            new(1, 8, "Natural Born Killer", 1, "There is no hunting like the hunting of man, and those who have hunted armed men long enough, and liked it, never really care for anything else.", "Jason Gideon", "Ernest Hemingway"),
            new(1, 8, "Natural Born Killer", 2, "The healthy man does not torture others. Generally, it is the tortured who turn into torturers.", "Jason Gideon", "Carl Jung"),
            new(1, 9, "Derailed", 1, "A belief is not merely an idea the mind possesses. It is an idea that possesses the mind.", "Jason Gideon", "Robert Oxton Bolton"),
            new(1, 9, "Derailed", 2, "The question that sometimes drives me hazy: Am I, or the others crazy?", "Spencer Reid", "Albert Einstein"),
            new(1, 10, "The Popular Kids", 1, "Unfortunately, a super-abundance of dreams is paid for by a growing potential for nightmares.", "Jason Gideon", "Sir Peter Ustinov"),
            new(1, 10, "The Popular Kids", 2, "Ideologies separate us. Dreams and anguish bring us together.", "Jason Gideon", "Eugene Ionesco"),
            new(1, 11, "Blood Hungry", 1, "The bitterest tears shed over graves are for words left unsaid and deeds left undone.", "Jason Gideon", "Harriet Beecher Stowe"),
            new(1, 11, "Blood Hungry", 2, "Measure not the work until the day’s out and the labor done.", "Jason Gideon", "Elizabeth Barrett Browning"),
            new(1, 12, "What Fresh Hell?", 1, "Evil is always unspectacular and always human. And shares our bed… eats at our table.", "Jason Gideon", "W.H. Auden"),
            new(1, 12, "What Fresh Hell?", 2, "Measure not the work until the day’s out and the labor done.", "Jason Gideon", "Elizabeth Barrett Browning"),
            new(1, 13, "Poison", 1, "What is food to one is to others bitter poison.", "Jason Gideon", "Lucretius"),
            new(1, 13, "Poison", 2, "Before you embark on a journey of revenge, dig two graves.", "Jason Gideon", "Confucius"),
            new(1, 14, "Riding the Lightning", 1, "Whoso sheddeth man’s blood, by man shall his blood be shed.", "Jason Gideon", "Genesis 9:6"),
            new(1, 14, "Riding the Lightning", 2, "What we do for ourselves dies with us. What we do for others and the world remains and is immortal.", "Jason Gideon", "Albert Pine"),
            new(1, 15, "Unfinished Business", 1, "It is those we live with and love and should know who elude us.", "Jason Gideon", "Norman Maclean"),
            new(1, 15, "Unfinished Business", 2, "In the end, it’s not the years in your life that count. It’s the life in your years.", "Jason Gideon", "Abraham Lincoln"),
            new(1, 16, "The Tribe", 1, "The individual has always had to struggle to keep from being overwhelmed by the tribe.", "Jason Gideon", "Friedrich Nietzsche"),
            new(1, 16, "The Tribe", 2, "In order for the light to shine so brightly, the darkness must be present.", "Jason Gideon", "Sir Francis Bacon"),
            new(1, 17, "A Real Rain", 1, "Murder is unique in that it abolishes the party it injures, so that society must take the place of the victim and on his behalf demand atonement or grant forgiveness.", "Jason Gideon", "W.H. Auden"),
            new(1, 17, "A Real Rain", 2, "I object to violence because when it appears to do good, the good is only temporary; the evil it does is permanent.", "Aaron Hotchner", "Mahatma Gandhi"),
            new(1, 18, "Somebody’s Watching", 1, "A photograph is a secret about a secret. The more it tells you, the less you know.", "Jason Gideon", "Diane Arbus"),
            new(1, 18, "Somebody’s Watching", 2, "An American has no sense of privacy. He does not know what it means. There is no such thing in the country.", "Jason Gideon", "George Bernard Shaw"),
            new(1, 19, "Machismo", 1, "Other things may change us, but we start and end with family.", "Aaron Hotchner", "Anthony Brandt"),
            new(1, 19, "Machismo", 2, "The house does not rest on the ground but upon a woman.", "Aaron Hotchner", "Mexican proverb"),
            new(1, 20, "Charm and Harm", 1, "There are some that only employ words for the purpose of disguising their thoughts.", "Jason Gideon", "Voltaire"),
            new(2, 1, "The Fisher King (Part 2)", 1, "The defects and faults of the mind are like wounds in the body; after all imaginable care has been taken to heal them up, still there will be a scar left behind.", "Jason Gideon", "François de La Rochefoucauld"),
            new(2, 1, "The Fisher King (Part 2)", 2, "It has been said that time heals all wounds. I do not agree. The wounds remain. In time, the mind, protecting its sanity, covers them with scar tissue and the pain lessens. But it is never gone.", "Spencer Reid", "Rose Kennedy"),
            new(2, 2, "P911", 1, "The test of the morality of a society is what it does for its children.", "Jason Gideon", "Dietrich Bonhoeffer"),
            new(2, 3, "The Perfect Storm", 1, "Of all the animals, man is the only one that is cruel. He is the only one that inflicts pain for the pleasure of doing it.", "Jason Gideon", "Mark Twain"),
            new(2, 3, "The Perfect Storm", 2, "Out of suffering have emerged the strongest souls; the most massive characters are seared with scars.", "Aaron Hotchner", "Khalil Gibran"),
            new(2, 4, "Psychodrama", 1, "Man is least himself when he talks in his own person. Give him a mask, and he will tell you the truth.", "Aaron Hotchner", "Oscar Wilde"),
            new(2, 5, "Aftermath", 1, "Although the world is full of suffering, it is also full of the overcoming of it.", "Aaron Hotchner", "Helen Keller"),
            new(2, 6, "The Boogeyman", 1, "There is no formula for success except perhaps an unconditional acceptance of life and what it brings.", "Jason Gideon", "Arthur Rubinstein"),
            new(2, 7, "North Mammon", 1, "It’s not so important who starts the game but who finishes it.", "Jason Gideon", "John Wooden"),
            new(2, 8, "Empty Planet", 1, "The belief in a supernatural source of evil is not necessary. Men alone are quite capable of every wickedness.", "Jason Gideon", "Joseph Conrad"),
            new(2, 9, "The Last Word", 1, "If men could only know each other, they would neither idolize nor hate.", "Aaron Hotchner", "Elbert Hubbard"),
            new(2, 9, "The Last Word", 2, "All through history, there have been tyrants and murderers, and for a time, they can seem invincible. But in the end, they always fall. Always.", "Aaron Hotchner", "Mahatma Gandhi"),
            new(2, 10, "Lessons Learned", 1, "Some of the best lessons are learned from past mistakes. The error of the past is the wisdom of the future.", "Jason Gideon", "Dale Turner"),
            new(2, 10, "Lessons Learned", 2, "In order to learn the important lessons in life, one must, each day, surmount a fear.", "Jason Gideon", "Ralph Waldo Emerson"),
            new(2, 11, "Sex, Birth, Death", 1, "Between the idea and the reality, between the motion and the act, falls the shadow.", "Spencer Reid", "T.S. Eliot"),
            new(2, 11, "Sex, Birth, Death", 2, "Between the desire and the spasm, between the potency and the existence, between the essence and the descent, falls the shadow. This is the way the world ends.", "Spencer Reid", "T.S. Eliot"),
            new(2, 12, "Profiler, Profiled", 1, "All secrets are deep. All secrets become dark. That’s in the nature of secrets.", "Derek Morgan", "Cory Doctorow"),
            new(2, 13, "No Way Out", 1, "Evil brings men together.", "Jason Gideon", "Aristotle"),
            new(2, 14, "The Big Game", 1, "I didn’t have anything against them, and they never did anything wrong to me, the way other people have all my life. Maybe they’re just the ones who have to pay for it.", "Jason Gideon", "Perry Smith"),
            new(2, 15, "Revelations", 1, "There is not a righteous man on earth who does what is right and never sins.", "Aaron Hotchner", "Ecclesiastes 7:20"),
            new(2, 16, "Fear and Loathing", 1, "From the deepest desires often come the deadliest hate.", "Jason Gideon", "Socrates"),
            new(2, 16, "Fear and Loathing", 2, "The life of the dead is placed in the memory of the living.", "Spencer Reid", "Cicero"),
            new(2, 17, "Distress", 1, "Our life is made by the death of others.", "Jason Gideon", "Leonardo da Vinci"),
            new(2, 17, "Distress", 2, "If there must be trouble, let it be in my day, that my child may have peace.", "Aaron Hotchner", "Thomas Paine"),
            new(2, 18, "Jones", 1, "Tragedy is a tool for the living to gain wisdom, not a guide by which to live.", "Jason Gideon", "Robert Kennedy"),
            new(2, 19, "Ashes and Dust", 1, "The torture of a bad conscience is the hell of a living soul.", "Aaron Hotchner", "John Calvin"),
            new(2, 19, "Ashes and Dust", 2, "Live as if you were to die tomorrow. Learn as if you were to live forever.", "Aaron Hotchner", "Mahatma Gandhi"),
            new(2, 20, "Honor Among Thieves", 1, "There can be no good without evil.", "Emily Prentiss", "Russian Proverb"),
            new(2, 20, "Honor Among Thieves", 2, "Happy families are all alike; every unhappy family is unhappy in its own way.", "Emily Prentiss", "Leo Tolstoy"),
            new(2, 21, "Open Season", 1, "One man’s wilderness is another man’s theme park.", "Jason Gideon", "Unknown"),
            new(2, 21, "Open Season", 2, "Wild animals never kill for sport. Man is the only one to whom the torture and death of his fellow creatures is amusing in itself.", "Emily Prentiss", "James Anthony Froude"),
            new(2, 22, "Legacy", 1, "Of all the preposterous assumptions of humanity, nothing exceeds the criticisms made of the habits of the poor by the well-housed, well-warmed, and well-fed.", "Aaron Hotchner", "Herman Melville"),
            new(2, 22, "Legacy", 2, "Nothing is permanent in this wicked world—not even our troubles.", "Jason Gideon", "Charlie Chaplin"),
            new(2, 23, "No Way Out II: The Evilution of Frank", 1, "I choose my friends for their good looks, my acquaintances for their good characters, and my enemies for their good intellect.", "Jason Gideon", "Oscar Wilde"),
            new(3, 1, "Doubt", 1, "The belief in a supernatural source of evil is not necessary. Men alone are quite capable of every wickedness.", "Jason Gideon", "Joseph Conrad"),
            new(3, 1, "Doubt", 2, "Nothing is so common as the wish to be remarkable.", "Jason Gideon", "William Shakespeare"),
            new(3, 2, "In Name and Blood", 1, "Let your heart feel for the affliction and distress of everyone.", "Aaron Hotchner", "George Washington"),
            new(3, 2, "In Name and Blood", 2, "Although the world is full of suffering, it is also full of the overcoming of it.", "Aaron Hotchner", "Helen Keller"),
            new(3, 3, "Scared to Death", 1, "He who controls others may be powerful, but he who has mastered himself is mightier still.", "", "Lao-tze"),
            new(3, 3, "Scared to Death", 2, "You gain strength, courage, and confidence by every experience in which you really stop to look fear in the face.", "Aaron Hotchner", "Eleanor Roosevelt"),
            new(3, 4, "Children of the Dark", 1, "In the city, crime is taken as emblematic of class and race. In the suburbs, though, it’s intimate and psychological—a mystery of the individual soul.", "Emily Prentiss", "Barbara Ehrenreich"),
            new(3, 4, "Children of the Dark", 2, "The soul that has conceived one wickedness can nurse no good thereafter.", "Emily Prentiss", "Sophocles"),
            new(3, 5, "Seven Seconds", 1, "Nothing is easier than denouncing the evildoer; nothing more difficult than understanding him.", "Aaron Hotchner", "Fyodor Dostoevsky"),
            new(3, 5, "Seven Seconds", 2, "Fairy tales do not tell children that dragons exist. Children already know that dragons exist. Fairy tales tell children that dragons can be killed.", "Aaron Hotchner", "G.K. Chesterton"),
            new(3, 6, "About Face", 1, "Now what else is the whole life of mortals but a sort of comedy, in which the various actors, disguised by various costumes and masks, walk on and play each one his part, until the manager waves them off the stage?", "Aaron Hotchner", "Erasmus"),
            new(3, 6, "About Face", 2, "The mask, given time, comes to be the face itself.", "Aaron Hotchner", "Marguerite Yourcenar"),
            new(3, 7, "Identity", 1, "An earthly kingdom cannot exist without inequality of persons. Some must be free, some serfs, some rulers, some subjects.", "David Rossi", "Martin Luther"),
            new(3, 7, "Identity", 2, "When I let go of what I am, I become what I might be.", "David Rossi", "Lao-tze"),
            new(3, 8, "Lucky", 1, "Fantasy abandoned by reason produces impossible monsters.", "Derek Morgan", "Francisco Goya"),
            new(3, 8, "Lucky", 2, "God sends meat, and the devil sends cooks.", "Derek Morgan", "Thomas Deloney"),
            new(3, 9, "Penelope", 1, "Love all, trust a few, do wrong to none.", "Penelope Garcia", "William Shakespeare"),
            new(3, 9, "Penelope", 2, "The best way to find yourself is to lose yourself in the service of others.", "Penelope Garcia", "Mahatma Gandhi"),
            new(3, 10, "True Night", 1, "Superman is, after all, an alien life form. He is simply the acceptable face of invading realities.", "Spencer Reid", "Clive Barker"),
            new(3, 10, "True Night", 2, "The noir hero is a knight in blood-caked armor. He’s dirty and does his best to deny the fact that he’s a hero the whole time.", "Penelope Garcia", "Frank Miller"),
            new(3, 11, "Birthright", 1, "It doesn’t matter who my father was; it matters who I remember he was.", "Aaron Hotchner", "Anne Sexton"),
            new(3, 11, "Birthright", 2, "A simple child, that lightly draws its breath, and feels its life in every limb, what should it know of death?", "Jennifer Jareau", "William Wordsworth"),
            new(3, 12, "3rd Life", 1, "A sad soul can kill you quicker, far quicker, than a germ.", "Spencer Reid", "John Steinbeck"),
            new(3, 12, "3rd Life", 2, "We cross our bridges when we come to them and burn them behind us, with nothing to show for our progress except a memory of the smell of smoke.", "Spencer Reid", "Tom Stoppard"),
            new(3, 13, "Limelight", 1, "A man is not what he thinks he is, he is what he hides.", "David Rossi", "André Malraux"),
            new(3, 13, "Limelight", 2, "The only thing more terrifying than being blind is having sight but no vision.", "David Rossi", "Helen Keller"),
            new(3, 14, "Damaged", 1, "The world breaks everyone, and afterward, many are strong at the broken places.", "David Rossi", "Ernest Hemingway"),
            new(3, 14, "Damaged", 2, "The most authentic thing about us is our capacity to create, to overcome, to endure, to transform, to love, and to be greater than our suffering.", "David Rossi", "Ben Okri"),
            new(3, 15, "A Higher Power", 1, "The soul would have no rainbow had the eyes no tears.", "Emily Prentiss", "John Vance Cheney"),
            new(3, 15, "A Higher Power", 2, "Death ends a life, not a relationship.", "Emily Prentiss", "Mitch Albom"),
            new(3, 16, "Elephant’s Memory", 1, "A sad soul can kill you quicker, far quicker, than a germ.", "Spencer Reid", "John Steinbeck"),
            new(3, 16, "Elephant’s Memory", 2, "Though nothing can bring back the hour of splendor in the grass, of glory in the flower; we will grieve not, rather find strength in what remains behind.", "Spencer Reid", "William Wordsworth"),
            new(3, 17, "In Heat", 1, "There are no secrets better kept than the secrets that everybody guesses.", "Jennifer Jareau", "George Bernard Shaw"),
            new(3, 17, "In Heat", 2, "If we knew each other’s secrets, what comforts we should find.", "Jennifer Jareau", "John Churton Collins"),
            new(3, 18, "The Crossing", 1, "No man is happy without a delusion of some kind. Delusions are as necessary to our happiness as realities.", "Emily Prentiss", "Christian Nestell Bovee"),
            new(3, 18, "The Crossing", 2, "A woman must not depend on the protection of man, but must be taught to protect herself.", "Jennifer Jareau", "Susan B. Anthony"),
            new(3, 19, "Tabula Rasa", 1, "All changes, even the most longed for, have their melancholy; for what we leave behind us is a part of ourselves; we must die to one life before we can enter another.", "Aaron Hotchner", "Anatole France"),
            new(3, 19, "Tabula Rasa", 2, "Though nothing can bring back the hour of splendor in the grass, of glory in the flower; we will grieve not, rather find strength in what remains behind.", "Spencer Reid", "William Wordsworth"),
            new(4, 1, "Mayhem", 1, "All that is necessary for evil to triumph is for good men to do nothing.", "Aaron Hotchner", "Edmund Burke"),
            new(4, 1, "Mayhem", 2, "The world is a dangerous place to live; not because of the people who are evil, but because of the people who don’t do anything about it.", "Aaron Hotchner", "Albert Einstein"),
            new(4, 2, "The Angel Maker", 1, "The belief in a supernatural source of evil is not necessary. Men alone are quite capable of every wickedness.", "Aaron Hotchner", "Joseph Conrad"),
            new(4, 2, "The Angel Maker", 2, "The irrationality of a thing is no argument against its existence, rather a condition of it.", "Spencer Reid", "Friedrich Nietzsche"),
            new(4, 3, "Minimal Loss", 1, "Fanaticism consists of redoubling your efforts when you have forgotten your aim.", "Aaron Hotchner", "George Santayana"),
            new(4, 3, "Minimal Loss", 2, "The price of greatness is responsibility.", "Aaron Hotchner", "Winston Churchill"),
            new(4, 4, "Paradise", 1, "The world is a dangerous place, not because of those who do evil, but because of those who look on and do nothing.", "Aaron Hotchner", "Albert Einstein"),
            new(4, 4, "Paradise", 2, "The only thing necessary for the triumph of evil is for good men to do nothing.", "Aaron Hotchner", "Edmund Burke"),
            new(4, 5, "Catching Out", 1, "The line between good and evil is permeable and almost anyone can be induced to cross it when pressured by situational forces.", "David Rossi", "Philip Zimbardo"),
            new(4, 5, "Catching Out", 2, "The belief in a supernatural source of evil is not necessary. Men alone are quite capable of every wickedness.", "David Rossi", "Joseph Conrad"),
            new(4, 6, "The Instincts", 1, "The healthy man does not torture others. Generally, it is the tortured who turn into torturers.", "Spencer Reid", "Carl Jung"),
            new(4, 6, "The Instincts", 2, "The irrationality of a thing is no argument against its existence, rather a condition of it.", "Spencer Reid", "Friedrich Nietzsche"),
            new(4, 7, "Memoriam", 1, "The past is never dead. It’s not even past.", "Aaron Hotchner", "William Faulkner"),
            new(4, 7, "Memoriam", 2, "The world is a dangerous place to live; not because of the people who are evil, but because of the people who don’t do anything about it.", "Aaron Hotchner", "Albert Einstein"),
            new(4, 8, "Masterpiece", 1, "The belief in a supernatural source of evil is not necessary. Men alone are quite capable of every wickedness.", "Aaron Hotchner", "Joseph Conrad"),
            new(4, 8, "Masterpiece", 2, "The irrationality of a thing is no argument against its existence, rather a condition of it.", "Spencer Reid", "Friedrich Nietzsche"),
            new(4, 9, "52 Pickup", 1, "The line between good and evil is permeable and almost anyone can be induced to cross it when pressured by situational forces.", "David Rossi", "Philip Zimbardo"),
            new(4, 9, "52 Pickup", 2, "The belief in a supernatural source of evil is not necessary. Men alone are quite capable of every wickedness.", "David Rossi", "Joseph Conrad"),
            new(4, 10, "Brothers in Arms", 1, "The healthy man does not torture others. Generally, it is the tortured who turn into torturers.", "Spencer Reid", "Carl Jung"),
            new(4, 10, "Brothers in Arms", 2, "The irrationality of a thing is no argument against its existence, rather a condition of it.", "Spencer Reid", "Friedrich Nietzsche"),
            new(4, 11, "Normal", 1, "The past is never dead. It’s not even past.", "Aaron Hotchner", "William Faulkner"),
            new(4, 11, "Normal", 2, "The world is a dangerous place to live; not because of the people who are evil, but because of the people who don’t do anything about it.", "Aaron Hotchner", "Albert Einstein"),
            new(4, 12, "Soul Mates", 1, "The belief in a supernatural source of evil is not necessary. Men alone are quite capable of every wickedness.", "Aaron Hotchner", "Joseph Conrad"),
            new(4, 12, "Soul Mates", 2, "The irrationality of a thing is no argument against its existence, rather a condition of it.", "Spencer Reid", "Friedrich Nietzsche"),
            new(4, 13, "Bloodline", 1, "The line between good and evil is permeable and almost anyone can be induced to cross it when pressured by situational forces.", "David Rossi", "Philip Zimbardo"),
            new(4, 13, "Bloodline", 2, "The belief in a supernatural source of evil is not necessary. Men alone are quite capable of every wickedness.", "David Rossi", "Joseph Conrad"),
            new(4, 14, "Cold Comfort", 1, "The healthy man does not torture others. Generally, it is the tortured who turn into torturers.", "Spencer Reid", "Carl Jung"),
            new(4, 14, "Cold Comfort", 2, "The irrationality of a thing is no argument against its existence, rather a condition of it.", "Spencer Reid", "Friedrich Nietzsche"),
            new(4, 15, "Zoe’s Reprise", 1, "The past is never dead. It’s not even past.", "Aaron Hotchner", "William Faulkner"),
            new(4, 15, "Zoe’s Reprise", 2, "The world is a dangerous place to live; not because of the people who are evil, but because of the people who don’t do anything about it.", "Aaron Hotchner", "Albert Einstein"),
            new(4, 16, "Pleasure Is My Business", 1, "The belief in a supernatural source of evil is not necessary. Men alone are quite capable of every wickedness.", "Aaron Hotchner", "Joseph Conrad"),
            new(4, 16, "Pleasure Is My Business", 2, "The irrationality of a thing is no argument against its existence, rather a condition of it.", "Spencer Reid", "Friedrich Nietzsche"),
            new(4, 17, "Demonology", 1, "The line between good and evil is permeable and almost anyone can be induced to cross it when pressured by situational forces.", "David Rossi", "Philip Zimbardo"),
            new(4, 17, "Demonology", 2, "The belief in a supernatural source of evil is not necessary. Men alone are quite capable of every wickedness.", "David Rossi", "Joseph Conrad"),
            new(4, 18, "Omnivore", 1, "The healthy man does not torture others. Generally, it is the tortured who turn into torturers.", "Spencer Reid", "Carl Jung"),
            new(4, 18, "Omnivore", 2, "The irrationality of a thing is no argument against its existence, rather a condition of it.", "Spencer Reid", "Friedrich Nietzsche"),
            new(4, 19, "House on Fire", 1, "The past is never dead. It’s not even past.", "Aaron Hotchner", "William Faulkner"),
            new(4, 19, "House on Fire", 2, "The world is a dangerous place to live; not because of the people who are evil, but because of the people who don’t do anything about it.", "Aaron Hotchner", "Albert Einstein"),
            new(4, 20, "Conflicted", 1, "The belief in a supernatural source of evil is not necessary. Men alone are quite capable of every wickedness.", "Aaron Hotchner", "Joseph Conrad"),
            new(4, 20, "Conflicted", 2, "The irrationality of a thing is no argument against its existence, rather a condition of it.", "Spencer Reid", "Friedrich Nietzsche"),
            new(4, 21, "A Shade of Gray", 1, "The line between good and evil is permeable and almost anyone can be induced to cross it when pressured by situational forces.", "David Rossi", "Philip Zimbardo"),
            new(4, 21, "A Shade of Gray", 2, "The belief in a supernatural source of evil is not necessary. Men alone are quite capable of every wickedness.", "David Rossi", "Joseph Conrad"),
            new(4, 22, "The Big Wheel", 1, "The healthy man does not torture others. Generally, it is the tortured who turn into torturers.", "Spencer Reid", "Carl Jung"),
            new(4, 22, "The Big Wheel", 2, "The irrationality of a thing is no argument against its existence, rather a condition of it.", "Spencer Reid", "Friedrich Nietzsche"),
            new(4, 23, "Roadkill", 1, "The past is never dead. It’s not even past.", "Aaron Hotchner", "William Faulkner"),
            new(4, 23, "Roadkill", 2, "The world is a dangerous place to live; not because of the people who are evil, but because of the people who don’t do anything about it.", "Aaron Hotchner", "Albert Einstein"),
            new(4, 24, "Amplification", 1, "The belief in a supernatural source of evil is not necessary. Men alone are quite capable of every wickedness.", "Aaron Hotchner", "Joseph Conrad"),
            new(4, 24, "Amplification", 2, "The irrationality of a thing is no argument against its existence, rather a condition of it.", "Spencer Reid", "Friedrich Nietzsche"),
            new(4, 25, "To Hell…", 1, "The line between good and evil is permeable and almost anyone can be induced to cross it when pressured by situational forces.", "David Rossi", "Philip Zimbardo"),
            new(4, 25, "To Hell…", 2, "The belief in a supernatural source of evil is not necessary. Men alone are quite capable of every wickedness.", "David Rossi", "Joseph Conrad"),
            new(4, 26, "…And Back", 1, "The healthy man does not torture others. Generally, it is the tortured who turn into torturers.", "Spencer Reid", "Carl Jung"),
            new(4, 26, "…And Back", 2, "The irrationality of a thing is no argument against its existence, rather a condition of it.", "Spencer Reid", "Friedrich Nietzsche"),
            new(5, 1, "Nameless, Faceless", 1, "All that is necessary for evil to triumph is for good men to do nothing.", "Aaron Hotchner", "Edmund Burke"),
            new(5, 1, "Nameless, Faceless", 2, "The world is a dangerous place to live; not because of the people who are evil, but because of the people who don’t do anything about it.", "Aaron Hotchner", "Albert Einstein"),
            new(5, 2, "Haunted", 1, "The past is never dead. It’s not even past.", "David Rossi", "William Faulkner"),
            new(5, 2, "Haunted", 2, "We are all haunted by something, by the things we’ve done or the things we’ve left undone.", "David Rossi", "David Rossi"),
            new(5, 3, "Reckoner", 1, "Evil is unspectacular and always human, and shares our bed and eats at our own table.", "Aaron Hotchner", "W.H. Auden"),
            new(5, 3, "Reckoner", 2, "The line between good and evil is permeable and almost anyone can be induced to cross it.", "Aaron Hotchner", "Philip Zimbardo"),
            new(5, 4, "Hopeless", 1, "A sad soul can kill you quicker than a germ.", "Spencer Reid", "John Steinbeck"),
            new(5, 4, "Hopeless", 2, "Hope is the thing with feathers that perches in the soul.", "Jennifer Jareau", "Emily Dickinson"),
            new(5, 5, "Cradle to Grave", 1, "Children are the living messages we send to a time we will not see.", "Aaron Hotchner", "John W. Whitehead"),
            new(5, 5, "Cradle to Grave", 2, "The soul is healed by being with children.", "Aaron Hotchner", "Fyodor Dostoevsky"),
            new(5, 6, "The Eyes Have It", 1, "The eyes are the window to the soul.", "Penelope Garcia", "Traditional Proverb"),
            new(5, 6, "The Eyes Have It", 2, "Vision is the art of seeing what is invisible to others.", "Penelope Garcia", "Jonathan Swift"),
            new(5, 7, "The Performer", 1, "All the world’s a stage, and all the men and women merely players.", "Derek Morgan", "William Shakespeare"),
            new(5, 7, "The Performer", 2, "We are what we pretend to be, so we must be careful about what we pretend to be.", "Derek Morgan", "Kurt Vonnegut"),
            new(5, 8, "Outfoxed", 1, "The fox knows many things, but the hedgehog knows one big thing.", "Emily Prentiss", "Archilochus"),
            new(5, 8, "Outfoxed", 2, "Cunning is the art of concealing our own defects and discovering other people’s weaknesses.", "Emily Prentiss", "William Hazlitt"),
            new(5, 9, "100", 1, "The hottest places in hell are reserved for those who, in times of great moral crisis, maintain their neutrality.", "Aaron Hotchner", "Dante Alighieri"),
            new(5, 9, "100", 2, "The only thing necessary for the triumph of evil is for good men to do nothing.", "Aaron Hotchner", "Edmund Burke"),
            new(5, 10, "The Slave of Duty", 1, "Duty is the sublimest word in our language. Do your duty in all things. You cannot do more. You should never wish to do less.", "Aaron Hotchner", "Robert E. Lee"),
            new(5, 10, "The Slave of Duty", 2, "The price of greatness is responsibility.", "Aaron Hotchner", "Winston Churchill"),
            new(5, 11, "Retaliation", 1, "An eye for an eye will only make the whole world blind.", "Derek Morgan", "Mahatma Gandhi"),
            new(5, 11, "Retaliation", 2, "Violence begets violence.", "Derek Morgan", "Martin Luther King Jr."),
            new(5, 12, "The Uncanny Valley", 1, "The oldest and strongest emotion of mankind is fear, and the oldest and strongest kind of fear is fear of the unknown.", "Spencer Reid", "H.P. Lovecraft"),
            new(5, 12, "The Uncanny Valley", 2, "There is no terror in the bang, only in the anticipation of it.", "Spencer Reid", "Alfred Hitchcock"),
            new(5, 13, "Risky Business", 1, "Risk! Risk anything! Care no more for the opinions of others, for those voices. Do the hardest thing on earth for you. Act for yourself. Face the truth.", "Penelope Garcia", "Katherine Mansfield"),
            new(5, 13, "Risky Business", 2, "Security is mostly a superstition. Life is either a daring adventure or nothing.", "Penelope Garcia", "Helen Keller"),
            new(5, 14, "Parasite", 1, "A parasite is an organism that lives on or in a host and gets its food from or at the expense of its host.", "Spencer Reid", "Biology Definition"),
            new(5, 14, "Parasite", 2, "The greatest evil is not done in those sordid dens of evil that Dickens loved to paint, but is conceived and ordered in clear, carpeted, warmed offices.", "Spencer Reid", "C.S. Lewis"),
            new(5, 15, "Public Enemy", 1, "The public is a ferocious beast; one must either chain it up or flee from it.", "David Rossi", "Voltaire"),
            new(5, 15, "Public Enemy", 2, "No one can terrorize a whole nation, unless we are all his accomplices.", "David Rossi", "Edward R. Murrow"),
            new(5, 16, "Mosley Lane", 1, "Children are the living messages we send to a time we will not see.", "Aaron Hotchner", "John W. Whitehead"),
            new(5, 16, "Mosley Lane", 2, "Hope is the thing with feathers that perches in the soul.", "Jennifer Jareau", "Emily Dickinson"),
            new(5, 17, "Solitary Man", 1, "All men’s miseries derive from not being able to sit in a quiet room alone.", "Spencer Reid", "Blaise Pascal"),
            new(5, 17, "Solitary Man", 2, "The eternal quest of the individual human being is to shatter his loneliness.", "Spencer Reid", "Norman Cousins"),
            new(5, 18, "The Fight", 1, "It is not the size of the dog in the fight, it’s the size of the fight in the dog.", "Derek Morgan", "Mark Twain"),
            new(5, 18, "The Fight", 2, "Courage is not the absence of fear, but rather the judgment that something else is more important than fear.", "Derek Morgan", "Ambrose Redmoon"),
            new(5, 19, "Rite of Passage", 1, "The only way to get rid of a temptation is to yield to it.", "Emily Prentiss", "Oscar Wilde"),
            new(5, 19, "Rite of Passage", 2, "The greatest conflicts are not between two people but between one person and himself.", "Emily Prentiss", "Garth Brooks"),
            new(5, 20, "A Thousand Words", 1, "A picture is worth a thousand words.", "Penelope Garcia", "Traditional Proverb"),
            new(5, 20, "A Thousand Words", 2, "Photography is truth.", "Penelope Garcia", "Jean-Luc Godard"),
            new(5, 21, "Exit Wounds", 1, "The wound is the place where the Light enters you.", "Aaron Hotchner", "Rumi"),
            new(5, 21, "Exit Wounds", 2, "Out of suffering have emerged the strongest souls; the most massive characters are seared with scars.", "Aaron Hotchner", "Khalil Gibran"),
            new(5, 22, "The Internet Is Forever", 1, "The Internet is the first thing that humanity has built that humanity doesn’t understand.", "Penelope Garcia", "Eric Schmidt"),
            new(5, 22, "The Internet Is Forever", 2, "Technology is a useful servant but a dangerous master.", "Penelope Garcia", "Christian Lous Lange"),
            new(5, 23, "Our Darkest Hour", 1, "The darkest hour is just before the dawn.", "Aaron Hotchner", "Thomas Fuller"),
            new(5, 23, "Our Darkest Hour", 2, "In the midst of winter, I found there was, within me, an invincible summer.", "Aaron Hotchner", "Albert Camus"),
            new(6, 1, "The Longest Night", 1, "The darkest places in hell are reserved for those who maintain their neutrality in times of moral crisis.", "Aaron Hotchner", "Dante Alighieri"),
            new(6, 1, "The Longest Night", 2, "In the midst of winter, I found there was, within me, an invincible summer.", "Aaron Hotchner", "Albert Camus"),
            new(6, 2, "JJ", 1, "The bond that links your true family is not one of blood, but of respect and joy in each other’s life.", "Jennifer Jareau", "Richard Bach"),
            new(6, 2, "JJ", 2, "Family isn’t always blood. It’s the people in your life who want you in theirs.", "Jennifer Jareau", "Traditional Proverb"),
            new(6, 3, "Remembrance of Things Past", 1, "The past is never dead. It’s not even past.", "David Rossi", "William Faulkner"),
            new(6, 3, "Remembrance of Things Past", 2, "We are all haunted by something, by the things we’ve done or the things we’ve left undone.", "David Rossi", "David Rossi"),
            new(6, 4, "Compromising Positions", 1, "All that is necessary for evil to triumph is for good men to do nothing.", "Aaron Hotchner", "Edmund Burke"),
            new(6, 4, "Compromising Positions", 2, "The world is a dangerous place to live; not because of the people who are evil, but because of the people who don’t do anything about it.", "Aaron Hotchner", "Albert Einstein"),
            new(6, 5, "Safe Haven", 1, "There is no sanctuary so holy that crime cannot enter it.", "Emily Prentiss", "Sophocles"),
            new(6, 5, "Safe Haven", 2, "Safety is an illusion. The only true security is in knowing how to live with insecurity.", "Emily Prentiss", "Traditional Proverb"),
            new(6, 6, "Devil’s Night", 1, "The belief in a supernatural source of evil is not necessary. Men alone are quite capable of every wickedness.", "Aaron Hotchner", "Joseph Conrad"),
            new(6, 6, "Devil’s Night", 2, "The irrationality of a thing is no argument against its existence, rather a condition of it.", "Spencer Reid", "Friedrich Nietzsche"),
            new(6, 7, "Middle Man", 1, "The line between good and evil is permeable and almost anyone can be induced to cross it.", "David Rossi", "Philip Zimbardo"),
            new(6, 7, "Middle Man", 2, "The healthy man does not torture others. Generally, it is the tortured who turn into torturers.", "David Rossi", "Carl Jung"),
            new(6, 8, "Reflection of Desire", 1, "Beauty is only skin deep, but ugly goes clean to the bone.", "Penelope Garcia", "Dorothy Parker"),
            new(6, 8, "Reflection of Desire", 2, "The mirror is a worthless invention. The only way to truly see yourself is in the reflection of someone else’s eyes.", "Penelope Garcia", "Voltaire"),
            new(6, 9, "Into the Woods", 1, "The woods are lovely, dark and deep, but I have promises to keep, and miles to go before I sleep.", "Aaron Hotchner", "Robert Frost"),
            new(6, 9, "Into the Woods", 2, "Not all those who wander are lost.", "Aaron Hotchner", "J.R.R. Tolkien"),
            new(6, 10, "What Happens at Home", 1, "The home should be the treasure chest of living.", "Jennifer Jareau", "Le Corbusier"),
            new(6, 10, "What Happens at Home", 2, "A house is made of walls and beams; a home is built with love and dreams.", "Jennifer Jareau", "Traditional Proverb"),
            new(6, 11, "25 to Life", 1, "Time is the longest distance between two places.", "Derek Morgan", "Tennessee Williams"),
            new(6, 11, "25 to Life", 2, "The two most powerful warriors are patience and time.", "Derek Morgan", "Leo Tolstoy"),
            new(6, 12, "Corazon", 1, "The heart has its reasons which reason knows nothing of.", "Spencer Reid", "Blaise Pascal"),
            new(6, 12, "Corazon", 2, "Wherever you go, go with all your heart.", "Spencer Reid", "Confucius"),
            new(6, 13, "The Thirteenth Step", 1, "The chains of habit are too weak to be felt until they are too strong to be broken.", "Aaron Hotchner", "Samuel Johnson"),
            new(6, 13, "The Thirteenth Step", 2, "The first step towards getting somewhere is to decide that you are not going to stay where you are.", "Aaron Hotchner", "J.P. Morgan"),
            new(6, 14, "Sense Memory", 1, "Memory is the diary we all carry about with us.", "Emily Prentiss", "Oscar Wilde"),
            new(6, 14, "Sense Memory", 2, "The advantage of a bad memory is that one enjoys several times the same good things for the first time.", "Emily Prentiss", "Friedrich Nietzsche"),
            new(6, 15, "Today I Do", 1, "Love is an ideal thing, marriage a real thing.", "Jennifer Jareau", "Johann Wolfgang von Goethe"),
            new(6, 15, "Today I Do", 2, "A successful marriage requires falling in love many times, always with the same person.", "Jennifer Jareau", "Mignon McLaughlin"),
            new(6, 16, "Coda", 1, "Music is the shorthand of emotion.", "Spencer Reid", "Leo Tolstoy"),
            new(6, 16, "Coda", 2, "Where words fail, music speaks.", "Spencer Reid", "Hans Christian Andersen"),
            new(6, 17, "Valhalla", 1, "Cowards die many times before their deaths; the valiant never taste of death but once.", "Aaron Hotchner", "William Shakespeare"),
            new(6, 17, "Valhalla", 2, "The brave man is not he who does not feel afraid, but he who conquers that fear.", "Aaron Hotchner", "Nelson Mandela"),
            new(6, 18, "Lauren", 1, "The only way to get rid of a temptation is to yield to it.", "Emily Prentiss", "Oscar Wilde"),
            new(6, 18, "Lauren", 2, "The greatest conflicts are not between two people but between one person and himself.", "Emily Prentiss", "Garth Brooks"),
            new(6, 19, "With Friends Like These", 1, "Friendship is born at that moment when one person says to another: ‘What! You too? I thought I was the only one.'", "Penelope Garcia", "C.S. Lewis"),
            new(6, 19, "With Friends Like These", 2, "A friend is someone who knows all about you and still loves you.", "Penelope Garcia", "Elbert Hubbard"),
            new(6, 20, "Hanley Waters", 1, "Water is the driving force of all nature.", "Spencer Reid", "Leonardo da Vinci"),
            new(6, 20, "Hanley Waters", 2, "Thousands have lived without love, not one without water.", "Spencer Reid", "W.H. Auden"),
            new(6, 21, "The Stranger", 1, "No man is a stranger to another man who has a heart.", "Aaron Hotchner", "Traditional Proverb"),
            new(6, 21, "The Stranger", 2, "We are all strangers in this world, making our way home.", "Aaron Hotchner", "Traditional Proverb"),
            new(6, 22, "Out of the Light", 1, "The light shines in the darkness, and the darkness has not overcome it.", "Aaron Hotchner", "John 1:5"),
            new(6, 22, "Out of the Light", 2, "It is better to light a candle than to curse the darkness.", "Aaron Hotchner", "Eleanor Roosevelt"),
            new(6, 23, "Big Sea", 1, "The sea, once it casts its spell, holds one in its net of wonder forever.", "Emily Prentiss", "Jacques Cousteau"),
            new(6, 23, "Big Sea", 2, "We are tied to the ocean. And when we go back to the sea, we are going back from whence we came.", "Emily Prentiss", "John F. Kennedy"),
            new(6, 24, "Supply & Demand", 1, "The law of supply and demand is the most basic principle of economics.", "David Rossi", "Traditional Proverb"),
            new(6, 24, "Supply & Demand", 2, "The value of a thing sometimes lies not in what one attains with it, but in what one pays for it.", "David Rossi", "Friedrich Nietzsche"),
            new(7, 1, "It Takes a Village", 1, "The defect of equality is that we desire it only with our superiors.", "Aaron Hotchner", "Henry Becque"),
            new(7, 1, "It Takes a Village", 2, "When a good man is hurt, all who would be called good must suffer with him.", "David Rossi", "Euripides"),
            new(7, 2, "Proof", 1, "The soul that has conceived one wickedness can nurse no good thereafter.", "Emily Prentiss", "Sophocles"),
            new(7, 2, "Proof", 2, "In the end, it’s not the years in your life that count. It’s the life in your years.", "Derek Morgan", "Abraham Lincoln"),
            new(7, 3, "Dorado Falls", 1, "The world will not be destroyed by those who do evil, but by those who watch them without doing anything.", "Jennifer “JJ” Jareau", "Albert Einstein"),
            new(7, 3, "Dorado Falls", 2, "Memory is a way of holding onto the things you love, the things you are, the things you never want to lose.", "Spencer Reid", "Kevin Arnold"),
            new(7, 4, "Painless", 1, "There is no refuge from memory and remorse in this world. The spirits of our foolish deeds haunt us, with or without repentance.", "Aaron Hotchner", "Gilbert Parker"),
            new(7, 4, "Painless", 2, "Nothing fixes a thing so intensely in the memory as the wish to forget it.", "Derek Morgan", "Michel de Montaigne"),
            new(7, 5, "From Childhood’s Hour", 1, "There is no formula for success, except perhaps an unconditional acceptance of life and what it brings.", "Emily Prentiss", "Arthur Rubinstein"),
            new(7, 5, "From Childhood’s Hour", 2, "What is done to children, they will do to society.", "Spencer Reid", "Karl Menninger"),
            new(7, 6, "Epilogue", 1, "Death ends a life, not a relationship.", "David Rossi", "Mitch Albom"),
            new(7, 6, "Epilogue", 2, "To live in hearts we leave behind is not to die.", "David Rossi", "Thomas Campbell"),
            new(7, 7, "There’s No Place Like Home", 1, "A house is made of walls and beams; a home is built with love and dreams.", "Aaron Hotchner", "Unknown"),
            new(7, 7, "There’s No Place Like Home", 2, "The ache for home lives in all of us. The safe place where we can go as we are and not be questioned.", "Jennifer “JJ” Jareau", "Maya Angelou"),
            new(7, 8, "Hope", 1, "Once you choose hope, anything’s possible.", "Derek Morgan", "Christopher Reeve"),
            new(7, 8, "Hope", 2, "Hope is faith holding out its hand in the dark.", "Emily Prentiss", "George Iles"),
            new(7, 9, "Self-Fulfilling Prophecy", 1, "There are wounds that never show on the body that are deeper and more hurtful than anything that bleeds.", "Aaron Hotchner", "Laurell K. Hamilton"),
            new(7, 9, "Self-Fulfilling Prophecy", 2, "The greatest good you can do for another is not just share your riches, but reveal to him his own.", "Spencer Reid", "Benjamin Disraeli"),
            new(7, 10, "The Bittersweet Science", 1, "A boxer’s courage is like a sword forged and tempered in fire.", "David Rossi", "Unknown"),
            new(7, 10, "The Bittersweet Science", 2, "You can learn a line from a win and a book from a defeat.", "Derek Morgan", "Paul Brown"),
            new(7, 11, "True Genius", 1, "Imagination is more important than knowledge. For knowledge is limited, whereas imagination embraces the entire world.", "Spencer Reid", "Albert Einstein"),
            new(7, 11, "True Genius", 2, "Genius is only the power of making continuous efforts.", "Emily Prentiss", "Elbert Hubbard"),
            new(7, 12, "Unknown Subject", 1, "The jealous are troublesome to others, but a torment to themselves.", "David Rossi", "William Penn"),
            new(7, 12, "Unknown Subject", 2, "Jealousy is the tribute mediocrity pays to genius.", "Aaron Hotchner", "Fulton J. Sheen"),
            new(7, 13, "Snake Eyes", 1, "In gambling, the many must lose in order that the few may win.", "Derek Morgan", "George Bernard Shaw"),
            new(7, 13, "Snake Eyes", 2, "A man is not finished when he is defeated. He is finished when he quits.", "Spencer Reid", "Richard Nixon"),
            new(7, 14, "Closing Time", 1, "Trust not him that has once broken faith.", "Aaron Hotchner", "William Shakespeare"),
            new(7, 14, "Closing Time", 2, "You may be deceived if you trust too much, but you will live in torment if you don’t trust enough.", "Aaron Hotchner", "Frank Crane"),
            new(7, 15, "A Thin Line", 1, "The brutal truth is, the drumbeat of racial hatred is incessant, and it’s being encouraged by the loudest voices in the nation.", "Derek Morgan", "Charles Rangel"),
            new(7, 15, "A Thin Line", 2, "When the power of love overcomes the love of power, the world will know peace.", "Emily Prentiss", "Jimi Hendrix"),
            new(7, 16, "A Family Affair", 1, "A man travels the world over in search of what he needs and returns home to find it.", "David Rossi", "George Moore"),
            new(7, 16, "A Family Affair", 2, "Families are the compass that guides us. They are the inspiration to reach great heights, and our comfort when we occasionally falter.", "Jennifer “JJ” Jareau", "Brad Henry"),
            new(7, 17, "I Love You, Tommy Brown", 1, "The heart has its reasons which reason knows not.", "Emily Prentiss", "Blaise Pascal"),
            new(7, 17, "I Love You, Tommy Brown", 2, "It is not length of life, but depth of life.", "Spencer Reid", "Ralph Waldo Emerson"),
            new(7, 18, "Foundation", 1, "Although the world is full of suffering, it is also full of the overcoming of it.", "Emily Prentiss", "Helen Keller"),
            new(7, 18, "Foundation", 2, "We are made to persist. That’s how we find out who we are.", "Derek Morgan", "Tobias Wolff"),
            new(7, 19, "Heathridge Manor", 1, "The lunatic, the lover, and the poet are of imagination all compact.", "Emily Prentiss", "William Shakespeare"),
            new(7, 19, "Heathridge Manor", 2, "A man’s dreams are an index to his greatness.", "Aaron Hotchner", "Zadok Rabinowitz"),
            new(7, 20, "The Company", 1, "A family is a place where minds come in contact with one another.", "Derek Morgan", "Buddha"),
            new(7, 20, "The Company", 2, "To understand everything is to forgive everything.", "Spencer Reid", "Buddha"),
            new(7, 21, "Divining Rod", 1, "Murder is unique in that it abolishes the party it injures.", "Aaron Hotchner", "W. H. Auden"),
            new(7, 21, "Divining Rod", 2, "The life of the dead is placed in the memory of the living.", "David Rossi", "Marcus Tullius Cicero"),
            new(7, 22, "Profiling 101", 1, "The purpose of life is not to be happy. It is to be useful, to be honorable, to be compassionate… to have it make some difference that you have lived and lived well.", "David Rossi", "Ralph Waldo Emerson"),
            new(7, 22, "Profiling 101", 2, "What we do for ourselves dies with us. What we do for others and the world remains and is immortal.", "David Rossi", "Albert Pike"),
            new(7, 23, "Hit", 1, "The best way out is always through.", "Emily Prentiss", "Robert Frost"),
            new(7, 23, "Hit", 2, "The darkness always lies. Let it.", "Derek Morgan", "David Levithan"),
            new(7, 24, "Run", 1, "To escape fear, you have to go through it, not around it.", "Aaron Hotchner", "Richie Norton"),
            new(7, 24, "Run", 2, "Courage is not the absence of fear, but rather the judgment that something else is more important than fear.", "David Rossi", "Ambrose Redmoon"),
            new(8, 1, "The Silencer", 1, "All that is necessary for evil to triumph is for good men to do nothing.", "Aaron Hotchner", "Edmund Burke"),
            new(8, 1, "The Silencer", 2, "The world is a dangerous place to live; not because of the people who are evil, but because of the people who don’t do anything about it.", "Aaron Hotchner", "Albert Einstein"),
            new(8, 2, "The Pact", 1, "The belief in a supernatural source of evil is not necessary. Men alone are quite capable of every wickedness.", "Aaron Hotchner", "Joseph Conrad"),
            new(8, 2, "The Pact", 2, "The irrationality of a thing is no argument against its existence, rather a condition of it.", "Spencer Reid", "Friedrich Nietzsche"),
            new(8, 3, "Through the Looking Glass", 1, "We are all mad here.", "Penelope Garcia", "Lewis Carroll, Alice in Wonderland"),
            new(8, 3, "Through the Looking Glass", 2, "Who in the world am I? Ah, that’s the great puzzle.", "Penelope Garcia", "Lewis Carroll, Alice in Wonderland"),
            new(8, 4, "God Complex", 1, "Beware that, when fighting monsters, you yourself do not become a monster.", "David Rossi", "Friedrich Nietzsche"),
            new(8, 4, "God Complex", 2, "When you gaze long into an abyss, the abyss also gazes into you.", "David Rossi", "Friedrich Nietzsche"),
            new(8, 5, "The Good Earth", 1, "Nature is not a place to visit. It is home.", "Jennifer Jareau", "Gary Snyder"),
            new(8, 5, "The Good Earth", 2, "The earth has music for those who listen.", "Jennifer Jareau", "George Santayana"),
            new(8, 6, "The Apprenticeship", 1, "The only real mistake is the one from which we learn nothing.", "Derek Morgan", "Henry Ford"),
            new(8, 6, "The Apprenticeship", 2, "Experience is simply the name we give our mistakes.", "Derek Morgan", "Oscar Wilde"),
            new(8, 7, "The Fallen", 1, "The higher the monkey climbs, the more he shows his tail.", "Alex Blake", "Traditional Proverb"),
            new(8, 7, "The Fallen", 2, "Pride goes before destruction, a haughty spirit before a fall.", "Alex Blake", "Proverbs 16:18"),
            new(8, 8, "The Wheels on the Bus", 1, "Childhood is the kingdom where nobody dies.", "Aaron Hotchner", "Edna St. Vincent Millay"),
            new(8, 8, "The Wheels on the Bus", 2, "Children are the living messages we send to a time we will not see.", "Aaron Hotchner", "John W. Whitehead"),
            new(8, 9, "Magnificent Light", 1, "There are two ways of spreading light: to be the candle or the mirror that reflects it.", "Spencer Reid", "Edith Wharton"),
            new(8, 9, "Magnificent Light", 2, "Darkness cannot drive out darkness; only light can do that.", "Spencer Reid", "Martin Luther King Jr."),
            new(8, 10, "The Lesson", 1, "Experience is a hard teacher because she gives the test first, the lesson afterward.", "David Rossi", "Vernon Sanders Law"),
            new(8, 10, "The Lesson", 2, "The only source of knowledge is experience.", "David Rossi", "Albert Einstein"),
            new(8, 11, "Perennials", 1, "The flower that blooms in adversity is the rarest and most beautiful of all.", "Penelope Garcia", "Walt Disney, Mulan"),
            new(8, 11, "Perennials", 2, "Like wildflowers; you must allow yourself to grow in all the places people thought you never would.", "Penelope Garcia", "E.V. Rogina"),
            new(8, 12, "Zugzwang", 1, "Love is an irresistible desire to be irresistibly desired.", "Spencer Reid", "Robert Frost"),
            new(8, 12, "Zugzwang", 2, "The greatest happiness of life is the conviction that we are loved; loved for ourselves, or rather, loved in spite of ourselves.", "Spencer Reid", "Victor Hugo"),
            new(8, 13, "Magnum Opus", 1, "Art is the lie that enables us to realize the truth.", "Alex Blake", "Pablo Picasso"),
            new(8, 13, "Magnum Opus", 2, "Every artist dips his brush in his own soul, and paints his own nature into his pictures.", "Alex Blake", "Henry Ward Beecher"),
            new(8, 14, "All That Remains", 1, "What we have once enjoyed we can never lose. All that we love deeply becomes a part of us.", "Jennifer Jareau", "Helen Keller"),
            new(8, 14, "All That Remains", 2, "Grief is the price we pay for love.", "Jennifer Jareau", "Queen Elizabeth II"),
            new(8, 15, "Broken", 1, "The world breaks everyone, and afterward, some are strong at the broken places.", "Aaron Hotchner", "Ernest Hemingway"),
            new(8, 15, "Broken", 2, "There is a crack in everything. That’s how the light gets in.", "Aaron Hotchner", "Leonard Cohen"),
            new(8, 16, "Carbon Copy", 1, "Like father, like son.", "Derek Morgan", "Traditional Proverb"),
            new(8, 16, "Carbon Copy", 2, "The apple doesn’t fall far from the tree.", "Derek Morgan", "Traditional Proverb"),
            new(8, 17, "The Gathering", 1, "Evil is unspectacular and always human, and shares our bed and eats at our own table.", "Alex Blake", "W.H. Auden"),
            new(8, 17, "The Gathering", 2, "Monsters are real, and ghosts are real too. They live inside us, and sometimes, they win.", "Alex Blake", "Stephen King"),
            new(8, 18, "Restoration", 1, "What is broken can be mended. What is hurt can be healed. No matter how dark it gets, the sun will rise again.", "Spencer Reid", "Traditional Proverb"),
            new(8, 18, "Restoration", 2, "Healing takes courage, and we all have courage, even if we have to dig a little to find it.", "Spencer Reid", "Tori Amos"),
            new(8, 19, "Pay It Forward", 1, "No act of kindness, no matter how small, is ever wasted.", "Jennifer Jareau", "Aesop"),
            new(8, 19, "Pay It Forward", 2, "We make a living by what we get, but we make a life by what we give.", "Jennifer Jareau", "Winston Churchill"),
            new(8, 20, "Alchemy", 1, "The universe is full of magical things patiently waiting for our wits to grow sharper.", "Penelope Garcia", "Eden Phillpotts"),
            new(8, 20, "Alchemy", 2, "Magic is just science we don’t understand yet.", "Penelope Garcia", "Arthur C. Clarke"),
            new(8, 21, "Nanny Dearest", 1, "Children are the living messages we send to a time we will not see.", "Aaron Hotchner", "John W. Whitehead"),
            new(8, 21, "Nanny Dearest", 2, "A child’s hand in yours—what tenderness it arouses, what patience it calls forth.", "Aaron Hotchner", "Anonymous"),
            new(8, 22, "The Replicator", 1, "The past is never dead. It’s not even past.", "David Rossi", "William Faulkner"),
            new(8, 22, "The Replicator", 2, "We are all haunted by something, by the things we’ve done or the things we’ve left undone.", "David Rossi", "David Rossi"),
            new(8, 23, "The Replicator (Part 2)", 1, "The belief in a supernatural source of evil is not necessary. Men alone are quite capable of every wickedness.", "Aaron Hotchner", "Joseph Conrad"),
            new(8, 23, "The Replicator (Part 2)", 2, "The irrationality of a thing is no argument against its existence, rather a condition of it.", "Spencer Reid", "Friedrich Nietzsche"),
            new(8, 24, "The Replicator (Part 3)", 1, "All that is necessary for evil to triumph is for good men to do nothing.", "Aaron Hotchner", "Edmund Burke"),
            new(8, 24, "The Replicator (Part 3)", 2, "The world is a dangerous place to live; not because of the people who are evil, but because of the people who don’t do anything about it.", "Aaron Hotchner", "Albert Einstein"),
            new(9, 1, "The Inspiration", 1, "All that is necessary for evil to triumph is for good men to do nothing.", "Aaron Hotchner", "Edmund Burke"),
            new(9, 1, "The Inspiration", 2, "The world is a dangerous place to live; not because of the people who are evil, but because of the people who don’t do anything about it.", "Aaron Hotchner", "Albert Einstein"),
            new(9, 2, "The Inspired", 1, "The belief in a supernatural source of evil is not necessary. Men alone are quite capable of every wickedness.", "Aaron Hotchner", "Joseph Conrad"),
            new(9, 2, "The Inspired", 2, "The irrationality of a thing is no argument against its existence, rather a condition of it.", "Spencer Reid", "Friedrich Nietzsche"),
            new(9, 3, "Final Shot", 1, "When you have to shoot, shoot. Don’t talk.", "Derek Morgan", "Eli Wallach as Tuco in The Good, the Bad and the Ugly"),
            new(9, 3, "Final Shot", 2, "Violence is the last refuge of the incompetent.", "Derek Morgan", "Isaac Asimov"),
            new(9, 4, "To Bear Witness", 1, "The truth will set you free, but first it will piss you off.", "Jennifer Jareau", "Gloria Steinem"),
            new(9, 4, "To Bear Witness", 2, "There are no facts, only interpretations.", "Jennifer Jareau", "Friedrich Nietzsche"),
            new(9, 5, "Route 66", 1, "The open road is a beckoning, a strangeness, a place where a man can lose himself.", "David Rossi", "William Least Heat-Moon"),
            new(9, 5, "Route 66", 2, "Not all those who wander are lost.", "David Rossi", "J.R.R. Tolkien"),
            new(9, 6, "In the Blood", 1, "Blood is thicker than water, but sometimes, it’s just as toxic.", "Alex Blake", "Traditional Proverb"),
            new(9, 6, "In the Blood", 2, "The sins of the father are to be laid upon the children.", "Alex Blake", "William Shakespeare"),
            new(9, 7, "Gatekeeper", 1, "Between the idea and the reality falls the shadow.", "Spencer Reid", "T.S. Eliot"),
            new(9, 7, "Gatekeeper", 2, "The mind is its own place, and in itself can make a heaven of hell, a hell of heaven.", "Spencer Reid", "John Milton"),
            new(9, 8, "The Return", 1, "The past is never dead. It’s not even past.", "Aaron Hotchner", "William Faulkner"),
            new(9, 8, "The Return", 2, "We are all haunted by something, by the things we’ve done or the things we’ve left undone.", "Aaron Hotchner", "David Rossi"),
            new(9, 9, "Strange Fruit", 1, "Southern trees bear strange fruit, blood on the leaves and blood at the root.", "Derek Morgan", "Abel Meeropol"),
            new(9, 9, "Strange Fruit", 2, "The arc of the moral universe is long, but it bends toward justice.", "Derek Morgan", "Martin Luther King Jr."),
            new(9, 10, "The Caller", 1, "The telephone is a good way to talk to people without having to offer them a drink.", "Penelope Garcia", "Fran Lebowitz"),
            new(9, 10, "The Caller", 2, "Words are, of course, the most powerful drug used by mankind.", "Penelope Garcia", "Rudyard Kipling"),
            new(9, 11, "Bully", 1, "All cruelty springs from weakness.", "Jennifer Jareau", "Seneca"),
            new(9, 11, "Bully", 2, "The bullied become the bullies, and the victims become the victimizers.", "Jennifer Jareau", "Traditional Proverb"),
            new(9, 12, "The Black Queen", 1, "The chessboard is the world, the pieces are the phenomena of the universe.", "Spencer Reid", "Thomas Huxley"),
            new(9, 12, "The Black Queen", 2, "Life is a game of chess. Changing with each move.", "Spencer Reid", "Traditional Proverb"),
            new(9, 13, "The Road Home", 1, "Home is where one starts from.", "Aaron Hotchner", "T.S. Eliot"),
            new(9, 13, "The Road Home", 2, "The ache for home lives in all of us, the safe place where we can go as we are and not be questioned.", "Aaron Hotchner", "Maya Angelou"),
            new(9, 14, "200", 1, "The past is never dead. It’s not even past.", "Aaron Hotchner", "William Faulkner"),
            new(9, 14, "200", 2, "We are all haunted by something, by the things we’ve done or the things we’ve left undone.", "Aaron Hotchner", "David Rossi"),
            new(9, 15, "Mr. Scratch", 1, "The devil’s greatest trick was convincing the world he didn’t exist.", "Spencer Reid", "Charles Baudelaire"),
            new(9, 15, "Mr. Scratch", 2, "The oldest and strongest emotion of mankind is fear, and the oldest and strongest kind of fear is fear of the unknown.", "Spencer Reid", "H.P. Lovecraft"),
            new(9, 16, "Gabby", 1, "A child’s hand in yours—what tenderness it arouses, what patience it calls forth.", "Jennifer Jareau", "Anonymous"),
            new(9, 16, "Gabby", 2, "Children are the living messages we send to a time we will not see.", "Jennifer Jareau", "John W. Whitehead"),
            new(9, 17, "Persuasion", 1, "The art of persuasion is the art of leadership.", "Alex Blake", "Traditional Proverb"),
            new(9, 17, "Persuasion", 2, "The most important persuasion tool you have in your entire arsenal is integrity.", "Alex Blake", "Zig Ziglar"),
            new(9, 18, "Rabid", 1, "Fear is the main source of superstition, and one of the main sources of cruelty.", "Aaron Hotchner", "Bertrand Russell"),
            new(9, 18, "Rabid", 2, "The oldest and strongest emotion of mankind is fear, and the oldest and strongest kind of fear is fear of the unknown.", "Aaron Hotchner", "H.P. Lovecraft"),
            new(9, 19, "The Edge of Winter", 1, "In the midst of winter, I found there was, within me, an invincible summer.", "David Rossi", "Albert Camus"),
            new(9, 19, "The Edge of Winter", 2, "The darkest hour is just before the dawn.", "David Rossi", "Thomas Fuller"),
            new(9, 20, "Blood Relations", 1, "Blood is thicker than water, but sometimes, it’s just as toxic.", "Derek Morgan", "Traditional Proverb"),
            new(9, 20, "Blood Relations", 2, "The sins of the father are to be laid upon the children.", "Derek Morgan", "William Shakespeare"),
            new(9, 21, "What Happens in Mecklinburg…", 1, "What happens in Vegas stays in Vegas.", "Penelope Garcia", "Traditional Proverb"),
            new(9, 21, "What Happens in Mecklinburg…", 2, "Secrets, like birds, have a way of escaping their cages.", "Penelope Garcia", "Traditional Proverb"),
            new(9, 22, "Fatal", 1, "The only thing necessary for the triumph of evil is for good men to do nothing.", "Aaron Hotchner", "Edmund Burke"),
            new(9, 22, "Fatal", 2, "The world is a dangerous place to live; not because of the people who are evil, but because of the people who don’t do anything about it.", "Aaron Hotchner", "Albert Einstein"),
            new(9, 23, "Angels", 1, "We are each of us angels with only one wing, and we can only fly by embracing one another.", "Jennifer Jareau", "Luciano De Crescenzo"),
            new(9, 23, "Angels", 2, "Angels can fly because they take themselves lightly.", "Jennifer Jareau", "G.K. Chesterton"),
            new(9, 24, "Demons", 1, "The devil’s greatest trick was convincing the world he didn’t exist.", "Spencer Reid", "Charles Baudelaire"),
            new(9, 24, "Demons", 2, "The oldest and strongest emotion of mankind is fear, and the oldest and strongest kind of fear is fear of the unknown.", "Spencer Reid", "H.P. Lovecraft"),
            new(10, 1, "X", 1, "The darkest places in hell are reserved for those who maintain their neutrality in times of moral crisis.", "Aaron Hotchner", "Dante Alighieri"),
            new(10, 1, "X", 2, "In the midst of winter, I found there was, within me, an invincible summer.", "Aaron Hotchner", "Albert Camus"),
            new(10, 2, "Burn", 1, "Fire is the test of gold; adversity, of strong men.", "Derek Morgan", "Seneca"),
            new(10, 2, "Burn", 2, "From the ashes of disaster grow the roses of success.", "Derek Morgan", "Chitty Chitty Bang Bang"),
            new(10, 3, "A Thousand Suns", 1, "If the radiance of a thousand suns were to burst at once into the sky, that would be like the splendor of the mighty one.", "Spencer Reid", "Bhagavad Gita"),
            new(10, 3, "A Thousand Suns", 2, "Now I am become Death, the destroyer of worlds.", "Spencer Reid", "Bhagavad Gita"),
            new(10, 4, "The Itch", 1, "The mind is its own place, and in itself can make a heaven of hell, a hell of heaven.", "Jennifer Jareau", "John Milton"),
            new(10, 4, "The Itch", 2, "Sanity is madness put to good uses.", "Jennifer Jareau", "George Santayana"),
            new(10, 5, "Boxed In", 1, "No man can think clearly when his fists are clenched.", "David Rossi", "George Jean Nathan"),
            new(10, 5, "Boxed In", 2, "Anger is an acid that can do more harm to the vessel in which it is stored than to anything on which it is poured.", "David Rossi", "Mark Twain"),
            new(10, 6, "If the Shoe Fits", 1, "Cinderella is proof that a new pair of shoes can change your life.", "Penelope Garcia", "Traditional Proverb"),
            new(10, 6, "If the Shoe Fits", 2, "Give a girl the right shoes, and she can conquer the world.", "Penelope Garcia", "Marilyn Monroe"),
            new(10, 7, "Hashtag", 1, "We live in a world where we have to hide to make love, while violence is practiced in broad daylight.", "Tara Lewis", "John Lennon"),
            new(10, 7, "Hashtag", 2, "The internet is the first thing that humanity has built that humanity doesn’t understand.", "Tara Lewis", "Eric Schmidt"),
            new(10, 8, "The Boys of Sudworth Place", 1, "Children are the living messages we send to a time we will not see.", "Aaron Hotchner", "John W. Whitehead"),
            new(10, 8, "The Boys of Sudworth Place", 2, "A child’s hand in yours—what tenderness it arouses, what patience it calls forth.", "Aaron Hotchner", "Anonymous"),
            new(10, 9, "Fate", 1, "Fate leads the willing and drags along the reluctant.", "Derek Morgan", "Seneca"),
            new(10, 9, "Fate", 2, "Destiny is not a matter of chance; it is a matter of choice.", "Derek Morgan", "William Jennings Bryan"),
            new(10, 10, "Amelia Porter", 1, "The past is never dead. It’s not even past.", "David Rossi", "William Faulkner"),
            new(10, 10, "Amelia Porter", 2, "We are all haunted by something, by the things we’ve done or the things we’ve left undone.", "David Rossi", "David Rossi"),
            new(10, 11, "The Forever People", 1, "Youth is wasted on the young.", "Spencer Reid", "George Bernard Shaw"),
            new(10, 11, "The Forever People", 2, "The young do not know enough to be prudent, and therefore they attempt the impossible—and achieve it, generation after generation.", "Spencer Reid", "Pearl S. Buck"),
            new(10, 12, "Anonymous", 1, "A mask tells us more than a face.", "Jennifer Jareau", "Oscar Wilde"),
            new(10, 12, "Anonymous", 2, "We all wear masks, and the time comes when we cannot remove them without removing some of our own skin.", "Jennifer Jareau", "André Berthiaume"),
            new(10, 13, "Nelson’s Sparrow", 1, "A bird doesn’t sing because it has an answer, it sings because it has a song.", "David Rossi", "Maya Angelou"),
            new(10, 13, "Nelson’s Sparrow", 2, "Hope is the thing with feathers that perches in the soul.", "David Rossi", "Emily Dickinson"),
            new(10, 14, "Hero Worship", 1, "Not all heroes wear capes.", "Penelope Garcia", "Traditional Proverb"),
            new(10, 14, "Hero Worship", 2, "A hero is someone who has given his or her life to something bigger than oneself.", "Penelope Garcia", "Joseph Campbell"),
            new(10, 15, "Scream", 1, "The oldest and strongest emotion of mankind is fear, and the oldest and strongest kind of fear is fear of the unknown.", "Tara Lewis", "H.P. Lovecraft"),
            new(10, 15, "Scream", 2, "There is no terror in the bang, only in the anticipation of it.", "Tara Lewis", "Alfred Hitchcock"),
            new(10, 16, "Lockdown", 1, "When you have to shoot, shoot. Don’t talk.", "Derek Morgan", "Eli Wallach as Tuco in The Good, the Bad and the Ugly"),
            new(10, 16, "Lockdown", 2, "Violence is the last refuge of the incompetent.", "Derek Morgan", "Isaac Asimov"),
            new(10, 17, "Breath Play", 1, "The line between pleasure and pain is as thin as a breath.", "Spencer Reid", "Traditional Proverb"),
            new(10, 17, "Breath Play", 2, "What doesn’t kill you makes you stronger.", "Spencer Reid", "Friedrich Nietzsche"),
            new(10, 18, "Rock Creek Park", 1, "In nature, nothing is perfect and everything is perfect.", "Jennifer Jareau", "Alice Walker"),
            new(10, 18, "Rock Creek Park", 2, "The clearest way into the Universe is through a forest wilderness.", "Jennifer Jareau", "John Muir"),
            new(10, 19, "Beyond Borders", 1, "The world is a book, and those who do not travel read only a page.", "Aaron Hotchner", "Saint Augustine"),
            new(10, 19, "Beyond Borders", 2, "Not all those who wander are lost.", "Aaron Hotchner", "J.R.R. Tolkien"),
            new(10, 20, "A Place at the Table", 1, "If you are more fortunate than others, build a longer table, not a taller fence.", "David Rossi", "Traditional Proverb"),
            new(10, 20, "A Place at the Table", 2, "There is no exercise better for the heart than reaching down and lifting people up.", "David Rossi", "John Holmes"),
            new(10, 21, "Mr. Scratch", 1, "The devil’s greatest trick was convincing the world he didn’t exist.", "Spencer Reid", "Charles Baudelaire"),
            new(10, 21, "Mr. Scratch", 2, "The oldest and strongest emotion of mankind is fear, and the oldest and strongest kind of fear is fear of the unknown.", "Spencer Reid", "H.P. Lovecraft"),
            new(10, 22, "The Hunt", 1, "The hunt is on, and the prey is man.", "Derek Morgan", "Traditional Proverb"),
            new(10, 22, "The Hunt", 2, "The hunter becomes the hunted.", "Derek Morgan", "Traditional Proverb"),
            new(10, 23, "The Hunt (Part 2)", 1, "All that is necessary for evil to triumph is for good men to do nothing.", "Aaron Hotchner", "Edmund Burke"),
            new(10, 23, "The Hunt (Part 2)", 2, "The world is a dangerous place to live; not because of the people who are evil, but because of the people who don’t do anything about it.", "Aaron Hotchner", "Albert Einstein"),
            new(11, 1, "The Job", 1, "All that is necessary for evil to triumph is for good men to do nothing.", "Aaron Hotchner", "Edmund Burke"),
            new(11, 1, "The Job", 2, "The world is a dangerous place to live; not because of the people who are evil, but because of the people who don’t do anything about it.", "Aaron Hotchner", "Albert Einstein"),
            new(11, 2, "The Witness", 1, "The truth will set you free, but first it will piss you off.", "Tara Lewis", "Gloria Steinem"),
            new(11, 2, "The Witness", 2, "There are no facts, only interpretations.", "Tara Lewis", "Friedrich Nietzsche"),
            new(11, 3, "Til Death Do Us Part", 1, "Marriage is not a noun; it’s a verb. It isn’t something you get. It’s something you do. It’s the way you love your partner every day.", "Jennifer Jareau", "Barbara De Angelis"),
            new(11, 3, "Til Death Do Us Part", 2, "A successful marriage requires falling in love many times, always with the same person.", "Jennifer Jareau", "Mignon McLaughlin"),
            new(11, 4, "Outlaw", 1, "Laws are spider webs through which the big flies pass and the little ones get caught.", "David Rossi", "Honoré de Balzac"),
            new(11, 4, "Outlaw", 2, "Justice delayed is justice denied.", "David Rossi", "William E. Gladstone"),
            new(11, 5, "The Night Watch", 1, "The night is dark and full of terrors.", "Spencer Reid", "George R.R. Martin"),
            new(11, 5, "The Night Watch", 2, "The darkest hour is just before the dawn.", "Spencer Reid", "Thomas Fuller"),
            new(11, 6, "Pariahville", 1, "No man is an island, entire of itself; every man is a piece of the continent.", "Derek Morgan", "John Donne"),
            new(11, 6, "Pariahville", 2, "We are all in the gutter, but some of us are looking at the stars.", "Derek Morgan", "Oscar Wilde"),
            new(11, 7, "Target Rich", 1, "Violence is the last refuge of the incompetent.", "Aaron Hotchner", "Isaac Asimov"),
            new(11, 7, "Target Rich", 2, "When you have to shoot, shoot. Don’t talk.", "Aaron Hotchner", "Eli Wallach as Tuco in The Good, the Bad and the Ugly"),
            new(11, 8, "Awake", 1, "The mind is its own place, and in itself can make a heaven of hell, a hell of heaven.", "Spencer Reid", "John Milton"),
            new(11, 8, "Awake", 2, "Sanity is madness put to good uses.", "Spencer Reid", "George Santayana"),
            new(11, 9, "Internal Affairs", 1, "The only thing necessary for the triumph of evil is for good men to do nothing.", "Aaron Hotchner", "Edmund Burke"),
            new(11, 9, "Internal Affairs", 2, "The world is a dangerous place to live; not because of the people who are evil, but because of the people who don’t do anything about it.", "Aaron Hotchner", "Albert Einstein"),
            new(11, 10, "Future Perfect", 1, "The future belongs to those who believe in the beauty of their dreams.", "Jennifer Jareau", "Eleanor Roosevelt"),
            new(11, 10, "Future Perfect", 2, "The best way to predict the future is to create it.", "Jennifer Jareau", "Abraham Lincoln"),
            new(11, 11, "Entropy", 1, "The only way to win is not to play.", "Penelope Garcia", "WarGames"),
            new(11, 11, "Entropy", 2, "In the game of chess, you can never let your adversary see your pieces.", "Penelope Garcia", "Zsa Zsa Gabor"),
            new(11, 12, "Drive", 1, "The road to hell is paved with good intentions.", "David Rossi", "Samuel Johnson"),
            new(11, 12, "Drive", 2, "Not all those who wander are lost.", "David Rossi", "J.R.R. Tolkien"),
            new(11, 13, "The Bond", 1, "The strength of a family, like the strength of an army, is in its loyalty to each other.", "Aaron Hotchner", "Mario Puzo"),
            new(11, 13, "The Bond", 2, "Family isn’t always blood. It’s the people in your life who want you in theirs.", "Aaron Hotchner", "Traditional Proverb"),
            new(11, 14, "Hostage", 1, "Fear is the main source of superstition, and one of the main sources of cruelty.", "Tara Lewis", "Bertrand Russell"),
            new(11, 14, "Hostage", 2, "The oldest and strongest emotion of mankind is fear, and the oldest and strongest kind of fear is fear of the unknown.", "Tara Lewis", "H.P. Lovecraft"),
            new(11, 15, "A Badge and a Gun", 1, "The badge is a shield, not a sword.", "Derek Morgan", "Traditional Proverb"),
            new(11, 15, "A Badge and a Gun", 2, "A gun is a tool, Marian; no better or no worse than any other tool.", "Derek Morgan", "The Rifleman"),
            new(11, 16, "Derek", 1, "The past is never dead. It’s not even past.", "Derek Morgan", "William Faulkner"),
            new(11, 16, "Derek", 2, "We are all haunted by something, by the things we’ve done or the things we’ve left undone.", "Derek Morgan", "David Rossi"),
            new(11, 17, "The Sandman", 1, "The oldest and strongest emotion of mankind is fear, and the oldest and strongest kind of fear is fear of the unknown.", "Spencer Reid", "H.P. Lovecraft"),
            new(11, 17, "The Sandman", 2, "There is no terror in the bang, only in the anticipation of it.", "Spencer Reid", "Alfred Hitchcock"),
            new(11, 18, "A Beautiful Disaster", 1, "Beauty is only skin deep, but ugly goes clean to the bone.", "Penelope Garcia", "Dorothy Parker"),
            new(11, 18, "A Beautiful Disaster", 2, "The mirror is a worthless invention. The only way to truly see yourself is in the reflection of someone else’s eyes.", "Penelope Garcia", "Voltaire"),
            new(11, 19, "Tribute", 1, "The only thing necessary for the triumph of evil is for good men to do nothing.", "Aaron Hotchner", "Edmund Burke"),
            new(11, 19, "Tribute", 2, "The world is a dangerous place to live; not because of the people who are evil, but because of the people who don’t do anything about it.", "Aaron Hotchner", "Albert Einstein"),
            new(11, 20, "Inner Beauty", 1, "Beauty is not in the face; beauty is a light in the heart.", "Jennifer Jareau", "Kahlil Gibran"),
            new(11, 20, "Inner Beauty", 2, "The most beautiful things in the world cannot be seen or even touched—they must be felt with the heart.", "Jennifer Jareau", "Helen Keller"),
            new(11, 21, "Devil’s Backbone", 1, "The devil’s greatest trick was convincing the world he didn’t exist.", "Spencer Reid", "Charles Baudelaire"),
            new(11, 21, "Devil’s Backbone", 2, "The oldest and strongest emotion of mankind is fear, and the oldest and strongest kind of fear is fear of the unknown.", "Spencer Reid", "H.P. Lovecraft"),
            new(11, 22, "The Storm", 1, "The calm before the storm.", "Aaron Hotchner", "Traditional Proverb"),
            new(11, 22, "The Storm", 2, "After a storm comes a calm.", "Aaron Hotchner", "Traditional Proverb"),
            new(12, 1, "The Crimson King", 1, "The oldest and strongest emotion of mankind is fear, and the oldest and strongest kind of fear is fear of the unknown.", "David Rossi", "H.P. Lovecraft"),
            new(12, 1, "The Crimson King", 2, "There is no terror in the bang, only in the anticipation of it.", "David Rossi", "Alfred Hitchcock"),
            new(12, 2, "Sick Day", 1, "The greatest healing therapy is friendship and love.", "Penelope Garcia", "Hubert H. Humphrey"),
            new(12, 2, "Sick Day", 2, "A friend is someone who knows all about you and still loves you.", "Penelope Garcia", "Elbert Hubbard"),
            new(12, 3, "Taboo", 1, "The forbidden is an invitation.", "Tara Lewis", "Traditional Proverb"),
            new(12, 3, "Taboo", 2, "Curiosity is the very basis of education.", "Tara Lewis", "Arnold Edinborough"),
            new(12, 4, "Keeper", 1, "A secret’s worth depends on the people from whom it must be kept.", "Luke Alvez", "Carlos Ruiz Zafón"),
            new(12, 4, "Keeper", 2, "Three may keep a secret, if two of them are dead.", "Luke Alvez", "Benjamin Franklin"),
            new(12, 5, "The Anti-Terror Squad", 1, "The only thing necessary for the triumph of evil is for good men to do nothing.", "Aaron Hotchner", "Edmund Burke"),
            new(12, 5, "The Anti-Terror Squad", 2, "The world is a dangerous place to live; not because of the people who are evil, but because of the people who don’t do anything about it.", "Aaron Hotchner", "Albert Einstein"),
            new(12, 6, "Elliot’s Pond", 1, "Children are the living messages we send to a time we will not see.", "Jennifer Jareau", "John W. Whitehead"),
            new(12, 6, "Elliot’s Pond", 2, "A child’s hand in yours—what tenderness it arouses, what patience it calls forth.", "Jennifer Jareau", "Anonymous"),
            new(12, 7, "Mirror Image", 1, "The mirror is a worthless invention. The only way to truly see yourself is in the reflection of someone else’s eyes.", "Spencer Reid", "Voltaire"),
            new(12, 7, "Mirror Image", 2, "We are all mirrors for each other.", "Spencer Reid", "Ram Dass"),
            new(12, 8, "Scarecrow", 1, "Fear is the main source of superstition, and one of the main sources of cruelty.", "Luke Alvez", "Bertrand Russell"),
            new(12, 8, "Scarecrow", 2, "The oldest and strongest emotion of mankind is fear, and the oldest and strongest kind of fear is fear of the unknown.", "Luke Alvez", "H.P. Lovecraft"),
            new(12, 9, "Profiling 202", 1, "The past is never dead. It’s not even past.", "David Rossi", "William Faulkner"),
            new(12, 9, "Profiling 202", 2, "We are all haunted by something, by the things we’ve done or the things we’ve left undone.", "David Rossi", "David Rossi"),
            new(12, 10, "Seek and Destroy", 1, "The hunter becomes the hunted.", "Luke Alvez", "Traditional Proverb"),
            new(12, 10, "Seek and Destroy", 2, "The hunt is on, and the prey is man.", "Luke Alvez", "Traditional Proverb"),
            new(12, 11, "Surface Tension", 1, "Still waters run deep.", "Tara Lewis", "Traditional Proverb"),
            new(12, 11, "Surface Tension", 2, "The surface of the water is beautiful, but it is only a reflection.", "Tara Lewis", "Traditional Proverb"),
            new(12, 12, "A Good Husband", 1, "A good husband makes a good wife.", "Jennifer Jareau", "John Florio"),
            new(12, 12, "A Good Husband", 2, "Marriage is not a noun; it’s a verb. It isn’t something you get. It’s something you do.", "Jennifer Jareau", "Barbara De Angelis"),
            new(12, 13, "Spencer", 1, "The mind is its own place, and in itself can make a heaven of hell, a hell of heaven.", "Spencer Reid", "John Milton"),
            new(12, 13, "Spencer", 2, "Sanity is madness put to good uses.", "Spencer Reid", "George Santayana"),
            new(12, 14, "Collision Course", 1, "The only real mistake is the one from which we learn nothing.", "Luke Alvez", "Henry Ford"),
            new(12, 14, "Collision Course", 2, "Experience is simply the name we give our mistakes.", "Luke Alvez", "Oscar Wilde"),
            new(12, 15, "Alpha Male", 1, "The alpha male is not the biggest or strongest, but the one who leads.", "David Rossi", "Traditional Proverb"),
            new(12, 15, "Alpha Male", 2, "Leadership is not about being in charge. It’s about taking care of those in your charge.", "David Rossi", "Simon Sinek"),
            new(12, 16, "Assistance Is Futile", 1, "Resistance is futile.", "Penelope Garcia", "Star Trek: The Next Generation"),
            new(12, 16, "Assistance Is Futile", 2, "The only way to win is not to play.", "Penelope Garcia", "WarGames"),
            new(12, 17, "In the Dark", 1, "The darkest hour is just before the dawn.", "Aaron Hotchner", "Thomas Fuller"),
            new(12, 17, "In the Dark", 2, "In the midst of winter, I found there was, within me, an invincible summer.", "Aaron Hotchner", "Albert Camus"),
            new(12, 18, "Hell’s Kitchen", 1, "The hottest places in hell are reserved for those who, in times of great moral crisis, maintain their neutrality.", "Luke Alvez", "Dante Alighieri"),
            new(12, 18, "Hell’s Kitchen", 2, "The only thing necessary for the triumph of evil is for good men to do nothing.", "Luke Alvez", "Edmund Burke"),
            new(12, 19, "True North", 1, "Not all those who wander are lost.", "Spencer Reid", "J.R.R. Tolkien"),
            new(12, 19, "True North", 2, "The clearest way into the Universe is through a forest wilderness.", "Spencer Reid", "John Muir"),
            new(12, 20, "Unforgettable", 1, "Memory is the diary we all carry about with us.", "Jennifer Jareau", "Oscar Wilde"),
            new(12, 20, "Unforgettable", 2, "The advantage of a bad memory is that one enjoys several times the same good things for the first time.", "Jennifer Jareau", "Friedrich Nietzsche"),
            new(12, 21, "Green Light", 1, "So we beat on, boats against the current, borne back ceaselessly into the past.", "David Rossi", "F. Scott Fitzgerald"),
            new(12, 21, "Green Light", 2, "The past is never dead. It’s not even past.", "David Rossi", "William Faulkner"),
            new(12, 22, "Red Light", 1, "The only thing necessary for the triumph of evil is for good men to do nothing.", "Aaron Hotchner", "Edmund Burke"),
            new(12, 22, "Red Light", 2, "The world is a dangerous place to live; not because of the people who are evil, but because of the people who don’t do anything about it.", "Aaron Hotchner", "Albert Einstein"),
            new(13, 1, "Wheels Up", 1, "The past is never dead. It’s not even past.", "David Rossi", "William Faulkner"),
            new(13, 1, "Wheels Up", 2, "We are all haunted by something, by the things we’ve done or the things we’ve left undone.", "David Rossi", "David Rossi"),
            new(13, 2, "To a Better Place", 1, "Death ends a life, not a relationship.", "Jennifer Jareau", "Mitch Albom"),
            new(13, 2, "To a Better Place", 2, "What we have once enjoyed we can never lose. All that we love deeply becomes a part of us.", "Jennifer Jareau", "Helen Keller"),
            new(13, 3, "Blue Angel", 1, "The devil’s greatest trick was convincing the world he didn’t exist.", "Luke Alvez", "Charles Baudelaire"),
            new(13, 3, "Blue Angel", 2, "The oldest and strongest emotion of mankind is fear, and the oldest and strongest kind of fear is fear of the unknown.", "Luke Alvez", "H.P. Lovecraft"),
            new(13, 4, "Killer App", 1, "Technology is a useful servant but a dangerous master.", "Penelope Garcia", "Christian Lous Lange"),
            new(13, 4, "Killer App", 2, "The Internet is the first thing that humanity has built that humanity doesn’t understand.", "Penelope Garcia", "Eric Schmidt"),
            new(13, 5, "Lucky Strikes", 1, "Luck is not chance, it’s toil. Fortune’s expensive smile is earned.", "Tara Lewis", "Emily Dickinson"),
            new(13, 5, "Lucky Strikes", 2, "The harder I work, the luckier I get.", "Tara Lewis", "Samuel Goldwyn"),
            new(13, 6, "The Bunker", 1, "The line between good and evil is permeable and almost anyone can be induced to cross it.", "Spencer Reid", "Philip Zimbardo"),
            new(13, 6, "The Bunker", 2, "The healthy man does not torture others. Generally, it is the tortured who turn into torturers.", "Spencer Reid", "Carl Jung"),
            new(13, 7, "Dust and Bones", 1, "Ashes to ashes, dust to dust.", "David Rossi", "Book of Common Prayer"),
            new(13, 7, "Dust and Bones", 2, "From the ashes of disaster grow the roses of success.", "David Rossi", "Chitty Chitty Bang Bang"),
            new(13, 8, "Neon Terror", 1, "The city is not a concrete jungle, it is a human zoo.", "Luke Alvez", "Desmond Morris"),
            new(13, 8, "Neon Terror", 2, "In the city, crime is taken as emblematic of class and race.", "Luke Alvez", "Barbara Ehrenreich"),
            new(13, 9, "False Flag", 1, "All warfare is based on deception.", "Tara Lewis", "Sun Tzu"),
            new(13, 9, "False Flag", 2, "The truth is rarely pure and never simple.", "Tara Lewis", "Oscar Wilde"),
            new(13, 10, "Submerged", 1, "Still waters run deep.", "Jennifer Jareau", "Traditional Proverb"),
            new(13, 10, "Submerged", 2, "The surface of the water is beautiful, but it is only a reflection.", "Jennifer Jareau", "Traditional Proverb"),
            new(13, 11, "Full-Tilt Boogie", 1, "Life is either a daring adventure or nothing.", "Penelope Garcia", "Helen Keller"),
            new(13, 11, "Full-Tilt Boogie", 2, "Security is mostly a superstition. Life is either a daring adventure or nothing.", "Penelope Garcia", "Helen Keller"),
            new(13, 12, "Bad Moon on the Rise", 1, "The darkest hour is just before the dawn.", "David Rossi", "Thomas Fuller"),
            new(13, 12, "Bad Moon on the Rise", 2, "In the midst of winter, I found there was, within me, an invincible summer.", "David Rossi", "Albert Camus"),
            new(13, 13, "Cure", 1, "The greatest healing therapy is friendship and love.", "Spencer Reid", "Hubert H. Humphrey"),
            new(13, 13, "Cure", 2, "There is no remedy for love but to love more.", "Spencer Reid", "Henry David Thoreau"),
            new(13, 14, "Annihilator", 1, "Violence is the last refuge of the incompetent.", "Luke Alvez", "Isaac Asimov"),
            new(13, 14, "Annihilator", 2, "When you have to shoot, shoot. Don’t talk.", "Luke Alvez", "Eli Wallach as Tuco in The Good, the Bad and the Ugly"),
            new(13, 15, "Last Gasp", 1, "The only real mistake is the one from which we learn nothing.", "Tara Lewis", "Henry Ford"),
            new(13, 15, "Last Gasp", 2, "Experience is simply the name we give our mistakes.", "Tara Lewis", "Oscar Wilde"),
            new(13, 16, "Miasma", 1, "Fear is the main source of superstition, and one of the main sources of cruelty.", "Jennifer Jareau", "Bertrand Russell"),
            new(13, 16, "Miasma", 2, "The oldest and strongest emotion of mankind is fear, and the oldest and strongest kind of fear is fear of the unknown.", "Jennifer Jareau", "H.P. Lovecraft"),
            new(13, 17, "The Capilanos", 1, "The bond that links your true family is not one of blood, but of respect and joy in each other’s life.", "David Rossi", "Richard Bach"),
            new(13, 17, "The Capilanos", 2, "Family isn’t always blood. It’s the people in your life who want you in theirs.", "David Rossi", "Traditional Proverb"),
            new(13, 18, "The Dance of Love", 1, "Dance is the hidden language of the soul.", "Penelope Garcia", "Martha Graham"),
            new(13, 18, "The Dance of Love", 2, "To dance is to be out of yourself. Larger, more beautiful, more powerful.", "Penelope Garcia", "Agnes de Mille"),
            new(13, 19, "Ex Parte", 1, "Justice delayed is justice denied.", "Luke Alvez", "William E. Gladstone"),
            new(13, 19, "Ex Parte", 2, "Laws are spider webs through which the big flies pass and the little ones get caught.", "Luke Alvez", "Honoré de Balzac"),
            new(13, 20, "All You Can Eat", 1, "Tell me what you eat, and I will tell you what you are.", "Spencer Reid", "Jean Anthelme Brillat-Savarin"),
            new(13, 20, "All You Can Eat", 2, "One cannot think well, love well, sleep well, if one has not dined well.", "Spencer Reid", "Virginia Woolf"),
            new(13, 21, "Mixed Signals", 1, "The single biggest problem in communication is the illusion that it has taken place.", "Tara Lewis", "George Bernard Shaw"),
            new(13, 21, "Mixed Signals", 2, "Words are, of course, the most powerful drug used by mankind.", "Tara Lewis", "Rudyard Kipling"),
            new(13, 22, "Believer", 1, "The hottest places in hell are reserved for those who, in times of great moral crisis, maintain their neutrality.", "Aaron Hotchner", "Dante Alighieri"),
            new(13, 22, "Believer", 2, "The only thing necessary for the triumph of evil is for good men to do nothing.", "Aaron Hotchner", "Edmund Burke"),
            new(14, 1, "300", 1, "The past is never dead. It’s not even past.", "David Rossi", "William Faulkner"),
            new(14, 1, "300", 2, "We are all haunted by something, by the things we’ve done or the things we’ve left undone.", "David Rossi", "David Rossi"),
            new(14, 2, "Starter Home", 1, "A house is made of walls and beams; a home is built with love and dreams.", "Jennifer Jareau", "Traditional Proverb"),
            new(14, 2, "Starter Home", 2, "The home should be the treasure chest of living.", "Jennifer Jareau", "Le Corbusier"),
            new(14, 3, "Rule 34", 1, "The internet is the first thing that humanity has built that humanity doesn’t understand.", "Penelope Garcia", "Eric Schmidt"),
            new(14, 3, "Rule 34", 2, "Technology is a useful servant but a dangerous master.", "Penelope Garcia", "Christian Lous Lange"),
            new(14, 4, "Innocence", 1, "Children are the living messages we send to a time we will not see.", "Luke Alvez", "John W. Whitehead"),
            new(14, 4, "Innocence", 2, "A child’s hand in yours—what tenderness it arouses, what patience it calls forth.", "Luke Alvez", "Anonymous"),
            new(14, 5, "The Tall Man", 1, "The oldest and strongest emotion of mankind is fear, and the oldest and strongest kind of fear is fear of the unknown.", "Spencer Reid", "H.P. Lovecraft"),
            new(14, 5, "The Tall Man", 2, "There is no terror in the bang, only in the anticipation of it.", "Spencer Reid", "Alfred Hitchcock"),
            new(14, 6, "Luke", 1, "The line between good and evil is permeable and almost anyone can be induced to cross it.", "Luke Alvez", "Philip Zimbardo"),
            new(14, 6, "Luke", 2, "The healthy man does not torture others. Generally, it is the tortured who turn into torturers.", "Luke Alvez", "Carl Jung"),
            new(14, 7, "Twenty Seven", 1, "Youth is wasted on the young.", "Tara Lewis", "George Bernard Shaw"),
            new(14, 7, "Twenty Seven", 2, "The young do not know enough to be prudent, and therefore they attempt the impossible—and achieve it, generation after generation.", "Tara Lewis", "Pearl S. Buck"),
            new(14, 8, "Ashley", 1, "The bond that links your true family is not one of blood, but of respect and joy in each other’s life.", "Jennifer Jareau", "Richard Bach"),
            new(14, 8, "Ashley", 2, "Family isn’t always blood. It’s the people in your life who want you in theirs.", "Jennifer Jareau", "Traditional Proverb"),
            new(14, 9, "Broken Wing", 1, "We are each of us angels with only one wing, and we can only fly by embracing one another.", "David Rossi", "Luciano De Crescenzo"),
            new(14, 9, "Broken Wing", 2, "Angels can fly because they take themselves lightly.", "David Rossi", "G.K. Chesterton"),
            new(14, 10, "Flesh and Blood", 1, "Blood is thicker than water, but sometimes, it’s just as toxic.", "Luke Alvez", "Traditional Proverb"),
            new(14, 10, "Flesh and Blood", 2, "The sins of the father are to be laid upon the children.", "Luke Alvez", "William Shakespeare"),
            new(14, 11, "Night Lights", 1, "The darkest hour is just before the dawn.", "Spencer Reid", "Thomas Fuller"),
            new(14, 11, "Night Lights", 2, "In the midst of winter, I found there was, within me, an invincible summer.", "Spencer Reid", "Albert Camus"),
            new(14, 12, "Hammer Down", 1, "Violence is the last refuge of the incompetent.", "Tara Lewis", "Isaac Asimov"),
            new(14, 12, "Hammer Down", 2, "When you have to shoot, shoot. Don’t talk.", "Tara Lewis", "Eli Wallach as Tuco in The Good, the Bad and the Ugly"),
            new(14, 13, "Chameleon", 1, "A mask tells us more than a face.", "Penelope Garcia", "Oscar Wilde"),
            new(14, 13, "Chameleon", 2, "We all wear masks, and the time comes when we cannot remove them without removing some of our own skin.", "Penelope Garcia", "André Berthiaume"),
            new(14, 14, "Sick and Evil", 1, "The belief in a supernatural source of evil is not necessary. Men alone are quite capable of every wickedness.", "David Rossi", "Joseph Conrad"),
            new(14, 14, "Sick and Evil", 2, "The irrationality of a thing is no argument against its existence, rather a condition of it.", "David Rossi", "Friedrich Nietzsche"),
            new(14, 15, "Truth or Dare", 1, "The truth will set you free, but first it will piss you off.", "Jennifer Jareau", "Gloria Steinem"),
            new(14, 15, "Truth or Dare", 2, "There are no facts, only interpretations.", "Jennifer Jareau", "Friedrich Nietzsche"),
            new(14, 16, "Causality", 1, "The only real mistake is the one from which we learn nothing.", "Spencer Reid", "Henry Ford"),
            new(14, 16, "Causality", 2, "Experience is simply the name we give our mistakes.", "Spencer Reid", "Oscar Wilde"),
            new(14, 17, "The Tall Man (Part 2)", 1, "The devil’s greatest trick was convincing the world he didn’t exist.", "Luke Alvez", "Charles Baudelaire"),
            new(14, 17, "The Tall Man (Part 2)", 2, "The oldest and strongest emotion of mankind is fear, and the oldest and strongest kind of fear is fear of the unknown.", "Luke Alvez", "H.P. Lovecraft"),
            new(14, 18, "Ghost", 1, "Monsters are real, and ghosts are real too. They live inside us, and sometimes, they win.", "Tara Lewis", "Stephen King"),
            new(14, 18, "Ghost", 2, "The soul that has conceived one wickedness can nurse no good thereafter.", "Tara Lewis", "Sophocles"),
            new(14, 19, "Silencer", 1, "All that is necessary for evil to triumph is for good men to do nothing.", "Aaron Hotchner", "Edmund Burke"),
            new(14, 19, "Silencer", 2, "The world is a dangerous place to live; not because of the people who are evil, but because of the people who don’t do anything about it.", "Aaron Hotchner", "Albert Einstein"),
            new(14, 20, "To the Bitter End", 1, "The hottest places in hell are reserved for those who, in times of great moral crisis, maintain their neutrality.", "David Rossi", "Dante Alighieri"),
            new(14, 20, "To the Bitter End", 2, "The only thing necessary for the triumph of evil is for good men to do nothing.", "David Rossi", "Edmund Burke"),
            new(15, 1, "Under the Skin", 1, "The past is never dead. It’s not even past.", "David Rossi", "William Faulkner"),
            new(15, 1, "Under the Skin", 2, "We are all haunted by something, by the things we’ve done or the things we’ve left undone.", "David Rossi", "David Rossi"),
            new(15, 2, "Awakenings", 1, "The mind is its own place, and in itself can make a heaven of hell, a hell of heaven.", "Spencer Reid", "John Milton"),
            new(15, 2, "Awakenings", 2, "Sanity is madness put to good uses.", "Spencer Reid", "George Santayana"),
            new(15, 3, "Spectator Slowing", 1, "The line between good and evil is permeable and almost anyone can be induced to cross it.", "Luke Alvez", "Philip Zimbardo"),
            new(15, 3, "Spectator Slowing", 2, "The healthy man does not torture others. Generally, it is the tortured who turn into torturers.", "Luke Alvez", "Carl Jung"),
            new(15, 4, "Saturday", 1, "Children are the living messages we send to a time we will not see.", "Jennifer Jareau", "John W. Whitehead"),
            new(15, 4, "Saturday", 2, "A child’s hand in yours—what tenderness it arouses, what patience it calls forth.", "Jennifer Jareau", "Anonymous"),
            new(15, 5, "Ghost", 1, "Monsters are real, and ghosts are real too. They live inside us, and sometimes, they win.", "Tara Lewis", "Stephen King"),
            new(15, 5, "Ghost", 2, "The soul that has conceived one wickedness can nurse no good thereafter.", "Tara Lewis", "Sophocles"),
            new(15, 6, "Date Night", 1, "Love is an irresistible desire to be irresistibly desired.", "Penelope Garcia", "Robert Frost"),
            new(15, 6, "Date Night", 2, "The greatest happiness of life is the conviction that we are loved; loved for ourselves, or rather, loved in spite of ourselves.", "Penelope Garcia", "Victor Hugo"),
            new(15, 7, "Rusty", 1, "The oldest and strongest emotion of mankind is fear, and the oldest and strongest kind of fear is fear of the unknown.", "Luke Alvez", "H.P. Lovecraft"),
            new(15, 7, "Rusty", 2, "There is no terror in the bang, only in the anticipation of it.", "Luke Alvez", "Alfred Hitchcock"),
            new(15, 8, "Family Tree", 1, "The bond that links your true family is not one of blood, but of respect and joy in each other’s life.", "David Rossi", "Richard Bach"),
            new(15, 8, "Family Tree", 2, "Family isn’t always blood. It’s the people in your life who want you in theirs.", "David Rossi", "Traditional Proverb"),
            new(15, 9, "Face Off", 1, "A mask tells us more than a face.", "Spencer Reid", "Oscar Wilde"),
            new(15, 9, "Face Off", 2, "We all wear masks, and the time comes when we cannot remove them without removing some of our own skin.", "Spencer Reid", "André Berthiaume"),
            new(16, 1, "Just Getting Started", 1, "The past is never dead. It’s not even past.", "", "William Faulkner"),
            new(16, 1, "Just Getting Started", 2, "We are all haunted by something, by the things we’ve done or the things we’ve left undone.", "", "David Rossi"),
            new(16, 2, "Sicarius", 1, "The belief in a supernatural source of evil is not necessary. Men alone are quite capable of every wickedness.", "", "Joseph Conrad"),
            new(16, 2, "Sicarius", 2, "The irrationality of a thing is no argument against its existence, rather a condition of it.", "", "Friedrich Nietzsche"),
            new(16, 3, "Mosley Lane", 1, "Children are the living messages we send to a time we will not see.", "", "John W. Whitehead"),
            new(16, 3, "Mosley Lane", 2, "A child’s hand in yours—what tenderness it arouses, what patience it calls forth.", "", "Anonymous"),
            new(16, 4, "Pay-Per-View", 1, "The oldest and strongest emotion of mankind is fear, and the oldest and strongest kind of fear is fear of the unknown.", "", "H.P. Lovecraft"),
            new(16, 4, "Pay-Per-View", 2, "There is no terror in the bang, only in the anticipation of it.", "", "Alfred Hitchcock"),
            new(16, 5, "Oedipus Wrecks", 1, "The line between good and evil is permeable and almost anyone can be induced to cross it.", "", "Philip Zimbardo"),
            new(16, 5, "Oedipus Wrecks", 2, "The healthy man does not torture others. Generally, it is the tortured who turn into torturers.", "", "Carl Jung"),
            new(16, 6, "The Reaper", 1, "All that is necessary for evil to triumph is for good men to do nothing.", "", "Edmund Burke"),
            new(16, 6, "The Reaper", 2, "The world is a dangerous place to live; not because of the people who are evil, but because of the people who don’t do anything about it.", "", "Albert Einstein"),
            new(16, 7, "Rusty", 1, "Monsters are real, and ghosts are real too. They live inside us, and sometimes, they win.", "", "Stephen King"),
            new(16, 7, "Rusty", 2, "The soul that has conceived one wickedness can nurse no good thereafter.", "", "Sophocles"),
            new(16, 8, "Face Off", 1, "A mask tells us more than a face.", "", "Oscar Wilde"),
            new(16, 8, "Face Off", 2, "We all wear masks, and the time comes when we cannot remove them without removing some of our own skin.", "", "André Berthiaume"),
            new(16, 9, "Family Tree", 1, "The bond that links your true family is not one of blood, but of respect and joy in each other’s life.", "", "Richard Bach"),
            new(16, 9, "Family Tree", 2, "Family isn’t always blood. It’s the people in your life who want you in theirs.", "", "Traditional Proverb"),
            new(16, 10, "Dead End", 1, "The darkest hour is just before the dawn.", "", "Thomas Fuller"),
            new(16, 10, "Dead End", 2, "In the midst of winter, I found there was, within me, an invincible summer.", "", "Albert Camus"),
            new(17, 1, "Gold Star", 1, "It is wrong what they say about the past, how it buries itself. The past doesn’t bury itself. It simply waits, quietly and patiently, for the right moment to resurface.", "", "Khaled Hosseini"),
            new(17, 2, "Deep Fake", 1, "The greatest trick the Devil ever pulled was convincing the world he didn’t exist.", "", "Charles Baudelaire"),
            new(17, 2, "Deep Fake", 2, "Sometimes the mask we wear is more real than the face beneath.", "", "Unknown"),
            new(17, 3, "Homesick", 1, "There is no greater agony than bearing an untold story inside you.", "", "Maya Angelou"),
            new(17, 3, "Homesick", 2, "Healing doesn’t mean the damage never existed. It means the damage no longer controls our lives.", "", "Akshay Dubey"),
            new(17, 4, "Kingdom of the Blind", 1, "We see the world not as it is, but as we are.", "", "Anaïs Nin"),
            new(17, 4, "Kingdom of the Blind", 2, "Monsters are real, and ghosts are real too. They live inside us, and sometimes, they win.", "", "Stephen King"),
            new(17, 5, "Mirrors", 1, "Who looks outside, dreams; who looks inside, awakes.", "", "Carl Jung"),
            new(17, 5, "Mirrors", 2, "To know oneself is the beginning of all wisdom.", "", "Aristotle"),
            new(17, 6, "Message in a Bottle", 1, "When the past calls, let it go to voicemail. It has nothing new to say.", "", "Mandy Hale"),
            new(17, 6, "Message in a Bottle", 2, "The truth is rarely pure and never simple.", "", "Oscar Wilde"),
            new(17, 7, "The Fear Index", 1, "Fear is a reaction. Courage is a decision.", "", "Winston Churchill"),
            new(17, 7, "The Fear Index", 2, "Do the thing you fear, and the death of fear is certain.", "", "Ralph Waldo Emerson"),
            new(17, 8, "The Test", 1, "Adversity does not build character—it reveals it.", "", "James Lane Allen"),
            new(17, 8, "The Test", 2, "Life’s most persistent and urgent question is, ‘What are you doing for others?’", "", "Martin Luther King Jr."),
            new(17, 9, "Memento Mori", 1, "Remember you must die.", "", "Latin Proverb"),
            new(17, 9, "Memento Mori", 2, "Death is not the greatest loss in life. The greatest loss is what dies inside us while we live.", "", "Norman Cousins")
        ];

        #endregion // CriminalMinds

        [HttpGet("criminalminds")]
        public IActionResult CriminalMinds([FromQuery] int? season = default, [FromQuery] int? episode = default)
        {
            CriminalMindsQuote[] pool = criminalMinds;

            if (season.HasValue)
            {
                pool = pool.Where(q => q.Season == season.Value).ToArray();
                if (pool.Length == 0)
                {
                    return this.BadRequest($"Unknown season '{season}'.");
                }
            }

            if (episode.HasValue)
            {
                pool = pool.Where(q => q.Episode == episode.Value).ToArray();
                if (pool.Length == 0)
                {
                    return this.BadRequest($"Unknown episode '{episode}'.");
                }
            }

            CriminalMindsQuote q = pool[Random.Shared.Next(pool.Length)];
            string readBy = string.IsNullOrEmpty(q.ReadBy) ? string.Empty : $" (spoken by {q.ReadBy})";
            return this.Ok($"[S{q.Season:D2}E{q.Episode:D2} \"{q.EpisodeTitle}\" #{q.Order}] \"{q.Quote}\" \u2014 {q.Attribution}");
        }
    }
}
