namespace number_sequence
{
    public static class Resources
    {
        private static Stream GetFromResrouce(string resourceName)
            => typeof(Resources).Assembly.GetManifestResourceStream("number_sequence.Resources." + resourceName);

        public static Stream ChiroCanineDiagram => GetFromResrouce("chiro-canine-diagram.png");
        public static Stream ChiroEquineDiagram => GetFromResrouce("chiro-equine-diagram.png");
        public static Stream ChiroLogo => GetFromResrouce("chiro-logo.png");
        public static Stream ComputerModernRomanFont => GetFromResrouce("cmunrm.ttf");
    }
}
