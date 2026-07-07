using System.Windows;
using System.Windows.Controls;

namespace SmartOpsDesk;

public sealed class PromptDialog : Window
{
    private readonly TextBox _inputBox = new();

    private PromptDialog(string title, string label, string defaultValue)
    {
        Title = title;
        Width = 460;
        Height = 240;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;

        var panel = new StackPanel { Margin = new Thickness(16) };
        panel.Children.Add(new TextBlock { Text = label, Margin = new Thickness(0, 0, 0, 8) });
        _inputBox.Text = defaultValue;
        _inputBox.AcceptsReturn = true;
        _inputBox.TextWrapping = TextWrapping.Wrap;
        _inputBox.Height = 90;
        panel.Children.Add(_inputBox);

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 14, 0, 0)
        };

        var ok = new Button { Content = "确定", Width = 80, Margin = new Thickness(0, 0, 8, 0) };
        ok.Click += (_, _) => { DialogResult = true; Close(); };
        var cancel = new Button { Content = "取消", Width = 80 };
        cancel.Click += (_, _) => { DialogResult = false; Close(); };
        buttons.Children.Add(ok);
        buttons.Children.Add(cancel);
        panel.Children.Add(buttons);

        Content = panel;
    }

    public static string? Show(Window owner, string title, string label, string defaultValue = "")
    {
        var dialog = new PromptDialog(title, label, defaultValue) { Owner = owner };
        return dialog.ShowDialog() == true ? dialog._inputBox.Text.Trim() : null;
    }
}
