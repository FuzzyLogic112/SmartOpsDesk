namespace SmartOpsDesk.Services;

public sealed class AppSettings
{
    public string StorageMode { get; set; } = "JSON";
    public string SupabaseConnection { get; set; } = "";
    public string PostgresConnection { get; set; } = "";
    public string SqlServerConnection { get; set; } = "";
    public string LlmEndpoint { get; set; } = "https://api.openai.com/v1/chat/completions";
    public string LlmApiKey { get; set; } = "";
    public string LlmModel { get; set; } = "gpt-4o-mini";
}
