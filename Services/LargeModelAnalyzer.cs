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
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SMARTOPSDESK_LLM_ENDPOINT"))
        && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SMARTOPSDESK_LLM_API_KEY"));

    public async Task<string> AnalyzeAsync(WorkTicket ticket)
    {
        var endpoint = Environment.GetEnvironmentVariable("SMARTOPSDESK_LLM_ENDPOINT");
        var apiKey = Environment.GetEnvironmentVariable("SMARTOPSDESK_LLM_API_KEY");
        var model = Environment.GetEnvironmentVariable("SMARTOPSDESK_LLM_MODEL") ?? "gpt-4o-mini";

        if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(apiKey))
        {
            return "未配置真实大模型接口。请设置 SMARTOPSDESK_LLM_ENDPOINT、SMARTOPSDESK_LLM_API_KEY 和可选 SMARTOPSDESK_LLM_MODEL。";
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
}
