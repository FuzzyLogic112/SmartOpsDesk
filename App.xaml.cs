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
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        try
        {
            var login = new LoginWindow();
            if (login.ShowDialog() != true || login.LoggedInUser is null)
            {
                Shutdown();
                return;
            }

            var main = new MainWindow(login.LoggedInUser.DisplayName, login.LoggedInUser.Role);
            MainWindow = main;
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            main.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"程序启动失败：{ex.Message}\n\n请检查数据库连接字符串或先取消云数据库环境变量后再运行。",
                "SmartOpsDesk 启动失败",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
        }
    }
}

