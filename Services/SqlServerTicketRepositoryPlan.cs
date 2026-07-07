namespace SmartOpsDesk.Services;

public static class SqlServerTicketRepositoryPlan
{
    public const string MigrationNote = """
        后续替换 SQL Server 的建议：
        1. 新建 Tickets、ApprovalRecords、TicketAttachments、DeploymentLogs 四张表。
        2. 让 SqlServerTicketRepository 实现 ITicketRepository。
        3. UI 层只依赖 ITicketRepository，不直接依赖 JSON 文件。
        4. 查询筛选可下推到 SQL WHERE / ORDER BY，CSV 导出复用当前视图模型。
        """;
}
