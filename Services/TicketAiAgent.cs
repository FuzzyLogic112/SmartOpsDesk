using SmartOpsDesk.Models;

namespace SmartOpsDesk.Services;

public sealed record TicketAnalysis(
    string Category,
    string Priority,
    string SuggestedOwner,
    string Reason,
    string Advice,
    double Confidence);

public sealed class TicketAiAgent
{
    private static readonly Dictionary<string, string[]> CategoryKeywords = new()
    {
        ["账号权限"] = ["登录", "密码", "账号", "权限", "角色", "菜单", "无法访问"],
        ["流程审批"] = ["审批", "流程", "驳回", "提交", "节点", "待办", "卡住"],
        ["数据报表"] = ["数据", "报表", "导出", "统计", "查询", "金额", "记录", "字段"],
        ["系统部署"] = ["服务器", "部署", "上线", "Nginx", "数据库连接", "接口超时", "证书"],
        ["页面体验"] = ["页面", "按钮", "样式", "显示", "移动端", "兼容", "卡顿"]
    };

    private static readonly Dictionary<string, string> CategoryOwners = new()
    {
        ["账号权限"] = "权限/平台组",
        ["流程审批"] = "业务流程组",
        ["数据报表"] = "数据报表组",
        ["系统部署"] = "运维交付组",
        ["页面体验"] = "前端体验组"
    };

    public TicketAnalysis Analyze(string title, string description, string department)
    {
        var text = $"{title} {description} {department}".ToLowerInvariant();
        var scoredCategories = CategoryKeywords
            .Select(pair => new
            {
                Category = pair.Key,
                Score = pair.Value.Count(keyword => text.Contains(keyword.ToLowerInvariant()))
            })
            .OrderByDescending(item => item.Score)
            .ToList();

        var best = scoredCategories.First();
        var category = best.Score > 0 ? best.Category : "综合咨询";
        var priority = DetectPriority(text);
        if (priority == "高" && (text.Contains("生产") || text.Contains("全员") || text.Contains("打不开") || text.Contains("上线失败")))
        {
            category = "系统部署";
        }

        var owner = CategoryOwners.GetValueOrDefault(category, "项目经理/需求负责人");
        var confidence = Math.Min(0.95, 0.45 + best.Score * 0.15 + (priority == "高" ? 0.1 : 0));
        var reason = BuildReason(category, priority, best.Score);
        var advice = BuildAdvice(category, priority);

        return new TicketAnalysis(category, priority, owner, reason, advice, Math.Round(confidence, 2));
    }

    private static string DetectPriority(string text)
    {
        string[] high = ["崩溃", "无法登录", "打不开", "数据丢失", "紧急", "生产", "全员", "上线失败"];
        string[] medium = ["报错", "异常", "卡住", "很慢", "超时", "影响", "审批不了", "导出失败"];

        if (high.Any(text.Contains))
        {
            return "高";
        }

        if (medium.Any(text.Contains))
        {
            return "中";
        }

        return "低";
    }

    private static string BuildReason(string category, string priority, int score)
    {
        var hitInfo = score > 0 ? $"命中 {score} 个业务关键词" : "未命中强关键词，按综合咨询处理";
        return $"{hitInfo}；分类为“{category}”；结合影响范围判断优先级为“{priority}”。";
    }

    private static string BuildAdvice(string category, string priority)
    {
        var prefix = priority == "高"
            ? "先确认影响范围并拉起负责人，必要时先做临时恢复。"
            : "先复现问题并补充截图、账号、时间点和操作路径。";

        return category switch
        {
            "账号权限" => $"{prefix} 核对用户、角色、菜单权限和组织范围，避免只改前端入口。",
            "流程审批" => $"{prefix} 检查流程节点、审批人配置、状态流转和历史日志。",
            "数据报表" => $"{prefix} 对照原始数据和查询条件，重点检查 SQL 过滤、关联和导出字段。",
            "系统部署" => $"{prefix} 查看服务日志、端口、数据库连接、反向代理和证书配置。",
            "页面体验" => $"{prefix} 记录浏览器版本和分辨率，检查样式、脚本异常和接口耗时。",
            _ => $"{prefix} 先明确需求边界，再决定由开发、实施或产品跟进。"
        };
    }
}
