namespace TcpWtf.NumberSequence.UI
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            this.InitializeComponent();

            Routing.RegisterRoute("homepage", typeof(Views.HomePage));
            Routing.RegisterRoute("loginpage", typeof(Views.LoginPage));
        }
    }
}
