namespace TcpWtf.NumberSequence.UI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            MauiAppBuilder builder = MauiApp.CreateBuilder();

            _ = builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    _ = fonts.AddFont("Hack-Bold.ttf");
                    _ = fonts.AddFont("Hack-BoldItalic.ttf");
                    _ = fonts.AddFont("Hack-Italic.ttf");
                    _ = fonts.AddFont("Hack-Regular.ttf", "Hack");
                    _ = fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    _ = fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            return builder.Build();
        }
    }
}
