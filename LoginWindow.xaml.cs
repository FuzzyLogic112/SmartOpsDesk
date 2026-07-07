using System.Windows;
using System.Windows.Controls;
using SmartOpsDesk.Services;

namespace SmartOpsDesk;

public partial class LoginWindow : Window
{
    private readonly AuthService _authService = new();

    public AppUser? LoggedInUser { get; private set; }

    public LoginWindow()
    {
        InitializeComponent();
    }

    private void Login_Click(object sender, RoutedEventArgs e)
    {
        var userName = (UserNameBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
        var user = _authService.Login(userName, PasswordBox.Password);
        if (user is null)
        {
            MessageBox.Show("账号或密码错误。", "登录失败", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        LoggedInUser = user;
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
