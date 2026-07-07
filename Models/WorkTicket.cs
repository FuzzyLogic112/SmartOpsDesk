namespace SmartOpsDesk.Models;

public sealed class WorkTicket
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Requester { get; set; } = "";
    public string Department { get; set; } = "";
    public string Description { get; set; } = "";
    public string Category { get; set; } = "未分类";
    public string Priority { get; set; } = "低";
    public string SuggestedOwner { get; set; } = "待分派";
    public string Status { get; set; } = "待处理";
    public string AiReason { get; set; } = "";
    public string HandlingAdvice { get; set; } = "";
    public double Confidence { get; set; }
    public List<ApprovalRecord> ApprovalHistory { get; set; } = [];
    public List<TicketAttachment> Attachments { get; set; } = [];
    public List<DeploymentLog> DeploymentLogs { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public DateTime DueAt { get; set; } = DateTime.Now.AddDays(3);

    public string SlaStatus
    {
        get
        {
            if (Status == "已完成")
            {
                return "已完成";
            }

            if (DateTime.Now > DueAt)
            {
                return "已超时";
            }

            var remaining = DueAt - DateTime.Now;
            return remaining.TotalHours < 8 ? "即将超时" : "正常";
        }
    }
}

public sealed class ApprovalRecord
{
    public DateTime Time { get; set; } = DateTime.Now;
    public string Operator { get; set; } = "";
    public string Role { get; set; } = "";
    public string Action { get; set; } = "";
    public string Comment { get; set; } = "";
}

public sealed class TicketAttachment
{
    public string FileName { get; set; } = "";
    public string StoredPath { get; set; } = "";
    public string UploadedBy { get; set; } = "";
    public DateTime UploadedAt { get; set; } = DateTime.Now;
}

public sealed class DeploymentLog
{
    public DateTime Time { get; set; } = DateTime.Now;
    public string Operator { get; set; } = "";
    public string Environment { get; set; } = "";
    public string Content { get; set; } = "";
}
