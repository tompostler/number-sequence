using TcpWtf.NumberSequence.Contracts;

namespace number_sequence
{
    public static class Resources
    {
        private static Stream GetFromResrouce(string resourceName)
            => typeof(Resources).Assembly.GetManifestResourceStream("number_sequence.Resources." + resourceName);

        public static Stream ChiroDiagram(ChiroSpecies species)
            => GetFromResrouce($"chiro-{species}-diagram.png".ToLowerInvariant());

        public static Stream ChiroLogo => GetFromResrouce("chiro-logo.png");

        public static Stream ComputerModernRomanFont => GetFromResrouce("cmunrm.ttf");

        public static Stream Favicon => GetFromResrouce("unlimitedinf-favicon.ico");
        public static Stream NoAsAServiceJson => GetFromResrouce("no-as-a-service.json");
    }
}
