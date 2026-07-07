using System.Configuration;
using System.Data;
using System.Windows;

namespace SmartOpsDesk;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private void App_Startup(object sender, StartupEventArgs e)
    {
        var login = new LoginWindow();
        if (login.ShowDialog() != true || login.LoggedInUser is null)
        {
            Shutdown();
            return;
        }

        var main = new MainWindow(login.LoggedInUser.DisplayName, login.LoggedInUser.Role);
        MainWindow = main;
        main.Show();
    }
}

