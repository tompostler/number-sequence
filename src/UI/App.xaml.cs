namespace TcpWtf.NumberSequence.UI
{
    public partial class App : Application
    {
        public App()
        {
            Current.UserAppTheme = AppTheme.Light;
            this.InitializeComponent();

            this.MainPage = new AppShell();
        }
    }
}
