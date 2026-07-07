using System.Globalization;
using System.IO;
using System.Text;
using SmartOpsDesk.Models;

namespace SmartOpsDesk.Services;

public sealed class CsvExporter
{
    public string Export(IEnumerable<WorkTicket> tickets)
    {
        var exportFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
            $"SmartOpsDeskTickets_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

        var builder = new StringBuilder();
        builder.AppendLine("Id,标题,部门,提交人,分类,优先级,建议负责人,状态,SLA截止时间,SLA状态,创建时间,AI原因,处理建议");

        foreach (var ticket in tickets)
        {
            builder.AppendLine(string.Join(",", new[]
            {
                ticket.Id.ToString(CultureInfo.InvariantCulture),
                Csv(ticket.Title),
                Csv(ticket.Department),
                Csv(ticket.Requester),
                Csv(ticket.Category),
                Csv(ticket.Priority),
                Csv(ticket.SuggestedOwner),
                Csv(ticket.Status),
                Csv(ticket.DueAt.ToString("yyyy-MM-dd HH:mm")),
                Csv(ticket.SlaStatus),
                Csv(ticket.CreatedAt.ToString("yyyy-MM-dd HH:mm")),
                Csv(ticket.AiReason),
                Csv(ticket.HandlingAdvice)
            }));
        }

        File.WriteAllText(exportFile, "\uFEFF" + builder, Encoding.UTF8);
        return exportFile;
    }

    private static string Csv(string value)
    {
        var escaped = value.Replace("\"", "\"\"");
        return $"\"{escaped}\"";
    }
}
