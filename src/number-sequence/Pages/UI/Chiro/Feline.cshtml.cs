using Microsoft.Extensions.Options;
using number_sequence.Filters;
using number_sequence.Services;
using number_sequence.Utilities;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Pages.UI.Chiro
{
    [RequiresToken(AccountRoles.Chiro)]
    public sealed class FelineModel : SmallAnimalChiroFormModel
    {
        public FelineModel(NsTcpWtfClient nsClient, IOptions<Options.Email> emailOptions, ChiroDictationParser parser)
            : base(nsClient, emailOptions, parser)
        {
        }

        private static readonly ChiroVocabulary FelineVocabulary = BuildVocabulary(ChiroSpecies.Feline);

        public override ChiroVocabulary Vocabulary => FelineVocabulary;
    }
}
