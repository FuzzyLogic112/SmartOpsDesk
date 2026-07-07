using System.Windows;
using System.Windows.Controls;
using SmartOpsDesk.Services;

namespace SmartOpsDesk;

public partial class SettingsWindow : Window
{
    private AppSettings _settings;

    public AppSettings Settings => _settings;

    public SettingsWindow(AppSettings settings)
    {
        InitializeComponent();
        _settings = settings;
        LoadToForm();
    }

    private void LoadToForm()
    {
        SelectComboValue(StorageModeBox, _settings.StorageMode);
        SupabaseBox.Text = string.IsNullOrWhiteSpace(_settings.SupabaseConnection) ? _settings.PostgresConnection : _settings.SupabaseConnection;
        SqlServerBox.Text = _settings.SqlServerConnection;
        LlmEndpointBox.Text = _settings.LlmEndpoint;
        LlmApiKeyBox.Password = _settings.LlmApiKey;
        LlmModelBox.Text = _settings.LlmModel;
        StatusText.Text = $"配置文件：{AppSettingsService.SettingsFile}";
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        _settings = ReadFromForm();
        AppSettingsService.Save(_settings);
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void TestDatabase_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var repository = RepositoryFactory.Create(ReadFromForm());
            _ = repository.Load();
            StatusText.Text = "数据库连接成功。";
            MessageBox.Show("数据库连接成功，表结构也已检查/创建。", "连接成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            StatusText.Text = "数据库连接失败。";
            MessageBox.Show($"数据库连接失败：{ex.Message}", "连接失败", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private AppSettings ReadFromForm()
    {
        var storageMode = SelectedComboText(StorageModeBox);
        return new AppSettings
        {
            StorageMode = string.IsNullOrWhiteSpace(storageMode) ? "JSON" : storageMode,
            SupabaseConnection = SupabaseBox.Text.Trim(),
            PostgresConnection = SupabaseBox.Text.Trim(),
            SqlServerConnection = SqlServerBox.Text.Trim(),
            LlmEndpoint = string.IsNullOrWhiteSpace(LlmEndpointBox.Text) ? "https://api.openai.com/v1/chat/completions" : LlmEndpointBox.Text.Trim(),
            LlmApiKey = LlmApiKeyBox.Password.Trim(),
            LlmModel = string.IsNullOrWhiteSpace(LlmModelBox.Text) ? "gpt-4o-mini" : LlmModelBox.Text.Trim()
        };
    }

    private static string SelectedComboText(ComboBox comboBox)
    {
        return (comboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
    }

    private static void SelectComboValue(ComboBox comboBox, string value)
    {
        foreach (var item in comboBox.Items.OfType<ComboBoxItem>())
        {
            if (item.Content?.ToString() == value)
            {
                comboBox.SelectedItem = item;
                return;
            }
        }

        comboBox.SelectedIndex = 0;
    }
}
