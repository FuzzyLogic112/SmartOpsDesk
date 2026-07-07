using System.IO;
using System.Text;
using System.Text.Json;
using SmartOpsDesk.Models;

namespace SmartOpsDesk.Services;

public sealed class TicketStore : ITicketRepository
{
    private readonly string _dataFile;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public TicketStore()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var directory = Path.Combine(appData, "SmartOpsDesk");
        Directory.CreateDirectory(directory);
        _dataFile = Path.Combine(directory, "tickets.json");
    }

    public List<WorkTicket> Load()
    {
        if (!File.Exists(_dataFile))
        {
            var seed = BuildSeedTickets();
            Save(seed);
            return seed;
        }

        var json = File.ReadAllText(_dataFile, Encoding.UTF8);
        return JsonSerializer.Deserialize<List<WorkTicket>>(json) ?? [];
    }

    public void Save(IEnumerable<WorkTicket> tickets)
    {
        var json = JsonSerializer.Serialize(tickets, JsonOptions);
        File.WriteAllText(_dataFile, json, Encoding.UTF8);
    }

    private static List<WorkTicket> BuildSeedTickets()
    {
        return
        [
            new WorkTicket
            {
                Id = 1,
                Title = "人事系统导出员工报表失败",
                Requester = "刘老师",
                Department = "人事处",
                Description = "筛选入职日期后点击导出报错，影响月底统计。",
                Category = "数据报表",
                Priority = "中",
                SuggestedOwner = "数据报表组",
                Status = "处理中",
                AiReason = "命中报表、导出、报错等关键词。",
                HandlingAdvice = "复现筛选条件，检查 SQL 过滤、导出字段和异常日志。",
                ApprovalHistory =
                [
                    new ApprovalRecord
                    {
                        Operator = "项目经理",
                        Role = "项目经理",
                        Action = "受理",
                        Comment = "已确认影响月底统计，转数据报表组排查。"
                    }
                ]
            },
            new WorkTicket
            {
                Id = 2,
                Title = "教务系统审批节点卡住",
                Requester = "王老师",
                Department = "教务处",
                Description = "课程调整申请提交后一直停留在学院审批节点。",
                Category = "流程审批",
                Priority = "中",
                SuggestedOwner = "业务流程组",
                Status = "待处理",
                AiReason = "命中审批、节点、提交等流程关键词。",
                HandlingAdvice = "检查流程配置、审批人范围和状态流转日志。",
                ApprovalHistory =
                [
                    new ApprovalRecord
                    {
                        Operator = "实施顾问",
                        Role = "实施",
                        Action = "登记",
                        Comment = "待补充审批人账号和流程编号。"
                    }
                ]
            },
            new WorkTicket
            {
                Id = 3,
                Title = "OA 系统生产环境无法登录",
                Requester = "陈主任",
                Department = "办公室",
                Description = "今天早上多个用户反馈 OA 打不开，疑似生产服务异常。",
                Category = "系统部署",
                Priority = "高",
                SuggestedOwner = "运维交付组",
                Status = "待处理",
                AiReason = "命中无法登录、生产等高优先级关键词。",
                HandlingAdvice = "先确认影响范围，查看服务日志、端口、数据库连接和反向代理。",
                DeploymentLogs =
                [
                    new DeploymentLog
                    {
                        Operator = "运维交付",
                        Environment = "生产",
                        Content = "待检查服务端口、Nginx 反向代理和数据库连接。"
                    }
                ]
            }
        ];
    }
}
