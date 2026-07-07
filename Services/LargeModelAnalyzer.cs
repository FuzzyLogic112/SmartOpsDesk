using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SmartOpsDesk.Models;

namespace SmartOpsDesk.Services;

public sealed class LargeModelAnalyzer
{
    private readonly HttpClient _httpClient = new();

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(GetEndpoint())
        && !string.IsNullOrWhiteSpace(GetApiKey());

    public async Task<string> AnalyzeAsync(WorkTicket ticket)
    {
        var endpoint = GetEndpoint();
        var apiKey = GetApiKey();
        var model = GetModel();

        if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(apiKey))
        {
            return "未配置真实大模型接口。请登录后点击“连接设置”，填写 AI Endpoint、AI API Key 和模型名。";
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var payload = new
        {
            model,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = "你是企业OA系统的实施与运维工单分析助手。请用中文输出分类、优先级、建议负责人、排查步骤。"
                },
                new
                {
                    role = "user",
                    content = $"标题：{ticket.Title}\n部门：{ticket.Department}\n提交人：{ticket.Requester}\n描述：{ticket.Description}"
                }
            },
            temperature = 0.2
        };

        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        using var response = await _httpClient.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return $"大模型接口调用失败：{(int)response.StatusCode} {response.ReasonPhrase}\n{json}";
        }

        using var document = JsonDocument.Parse(json);
        var content = document.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return string.IsNullOrWhiteSpace(content) ? "大模型返回为空。" : content;
    }

    private static string GetEndpoint()
    {
        var settings = AppSettingsService.Load();
        if (AppSettingsService.Exists)
        {
            return string.IsNullOrWhiteSpace(settings.LlmEndpoint)
                ? "https://api.openai.com/v1/chat/completions"
                : settings.LlmEndpoint;
        }

        return !string.IsNullOrWhiteSpace(settings.LlmEndpoint)
            ? Environment.GetEnvironmentVariable("SMARTOPSDESK_LLM_ENDPOINT") ?? settings.LlmEndpoint
            : Environment.GetEnvironmentVariable("SMARTOPSDESK_LLM_ENDPOINT") ?? "https://api.openai.com/v1/chat/completions";
    }

    private static string GetApiKey()
    {
        var settings = AppSettingsService.Load();
        if (AppSettingsService.Exists)
        {
            return settings.LlmApiKey;
        }

        return !string.IsNullOrWhiteSpace(settings.LlmApiKey)
            ? settings.LlmApiKey
            : Environment.GetEnvironmentVariable("SMARTOPSDESK_LLM_API_KEY") ?? "";
    }

    private static string GetModel()
    {
        var settings = AppSettingsService.Load();
        if (AppSettingsService.Exists)
        {
            return string.IsNullOrWhiteSpace(settings.LlmModel)
                ? "gpt-4o-mini"
                : settings.LlmModel;
        }

        return !string.IsNullOrWhiteSpace(settings.LlmModel)
            ? Environment.GetEnvironmentVariable("SMARTOPSDESK_LLM_MODEL") ?? settings.LlmModel
            : Environment.GetEnvironmentVariable("SMARTOPSDESK_LLM_MODEL") ?? "gpt-4o-mini";
    }
}
