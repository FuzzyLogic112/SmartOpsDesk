using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Win32;
using SmartOpsDesk.Models;
using SmartOpsDesk.Services;

namespace SmartOpsDesk;

public partial class MainWindow : Window
{
    private readonly ITicketRepository _repository = RepositoryFactory.Create();
    private readonly CsvExporter _csvExporter = new();
    private readonly TicketAiAgent _agent = new();
    private readonly AttachmentService _attachmentService = new();
    private readonly LargeModelAnalyzer _largeModelAnalyzer = new();
    private readonly ObservableCollection<WorkTicket> _tickets;
    private readonly ICollectionView _view;

    public MainWindow(string userName = "王文涛", string role = "开发")
    {
        InitializeComponent();
        _tickets = new ObservableCollection<WorkTicket>(_repository.Load().OrderByDescending(ticket => ticket.CreatedAt));
        _view = CollectionViewSource.GetDefaultView(_tickets);
        _view.Filter = FilterTicket;
        TicketGrid.ItemsSource = _view;
        DepartmentBox.SelectedIndex = 0;
        StatusBox.SelectedIndex = 0;
        SelectComboValue(UserBox, userName);
        SelectComboValue(RoleBox, role);
        UpdateMetrics();
        SetStatus($"已登录：{userName} / {role}；当前存储：{RepositoryFactory.CurrentStorageName}");
    }

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateForm())
        {
            return;
        }

        var ticket = new WorkTicket
        {
            Id = _tickets.Any() ? _tickets.Max(item => item.Id) + 1 : 1,
            Title = TitleBox.Text.Trim(),
            Requester = RequesterBox.Text.Trim(),
            Department = SelectedComboText(DepartmentBox),
            Description = DescriptionBox.Text.Trim(),
            Status = SelectedComboText(StatusBox),
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        ApplyAi(ticket);
        ticket.ApprovalHistory.Add(new ApprovalRecord
        {
            Operator = CurrentUser(),
            Role = CurrentRole(),
            Action = "创建",
            Comment = "新建工单并完成规则 AI 初步分派。"
        });
        _tickets.Insert(0, ticket);
        Persist("已新增并自动完成 AI 分析。");
        ClearForm();
        TicketGrid.SelectedItem = ticket;
    }

    private void UpdateSelected_Click(object sender, RoutedEventArgs e)
    {
        if (TicketGrid.SelectedItem is not WorkTicket ticket)
        {
            SetStatus("请先选择一条工单。");
            return;
        }

        if (!ValidateForm())
        {
            return;
        }

        ticket.Title = TitleBox.Text.Trim();
        ticket.Requester = RequesterBox.Text.Trim();
        ticket.Department = SelectedComboText(DepartmentBox);
        ticket.Description = DescriptionBox.Text.Trim();
        ticket.Status = SelectedComboText(StatusBox);
        ticket.UpdatedAt = DateTime.Now;
        ApplyAi(ticket);
        _view.Refresh();
        Persist("已更新选中工单。");
        ShowAnalysis(ticket);
    }

    private void AnalyzeSelected_Click(object sender, RoutedEventArgs e)
    {
        if (TicketGrid.SelectedItem is not WorkTicket ticket)
        {
            SetStatus("请先选择一条工单。");
            return;
        }

        ApplyAi(ticket);
        _view.Refresh();
        Persist("AI 分析已刷新。");
        ShowAnalysis(ticket);
        ShowDetails(ticket);
    }

    private async void LargeModelAnalyze_Click(object sender, RoutedEventArgs e)
    {
        if (TicketGrid.SelectedItem is not WorkTicket ticket)
        {
            SetStatus("请先选择一条工单。");
            return;
        }

        SetStatus("正在调用大模型分析接口...");
        var result = await _largeModelAnalyzer.AnalyzeAsync(ticket);
        ticket.ApprovalHistory.Add(new ApprovalRecord
        {
            Operator = CurrentUser(),
            Role = CurrentRole(),
            Action = _largeModelAnalyzer.IsConfigured ? "大模型分析" : "大模型未配置",
            Comment = result
        });
        Persist(_largeModelAnalyzer.IsConfigured ? "大模型分析结果已写入审批记录。" : "大模型接口未配置，已记录配置提示。");
        ShowDetails(ticket);
    }

    private void MarkProcessing_Click(object sender, RoutedEventArgs e)
    {
        UpdateSelectedStatus("处理中");
    }

    private void MarkDone_Click(object sender, RoutedEventArgs e)
    {
        UpdateSelectedStatus("已完成");
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (TicketGrid.SelectedItem is not WorkTicket ticket)
        {
            SetStatus("请先选择一条工单。");
            return;
        }

        var result = MessageBox.Show($"确认删除工单 #{ticket.Id}？", "删除确认", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        _tickets.Remove(ticket);
        Persist("工单已删除。");
        ClearForm();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        Persist("已保存到本地 JSON。");
    }

    private void Export_Click(object sender, RoutedEventArgs e)
    {
        var file = _csvExporter.Export(_view.Cast<WorkTicket>());
        SetStatus($"已导出 CSV：{file}");
    }

    private void ClearForm_Click(object sender, RoutedEventArgs e)
    {
        ClearForm();
    }

    private void Approve_Click(object sender, RoutedEventArgs e)
    {
        if (TicketGrid.SelectedItem is not WorkTicket ticket)
        {
            SetStatus("请先选择一条工单。");
            return;
        }

        if (!CanApprove())
        {
            MessageBox.Show("当前角色没有审批权限。请使用“项目经理”角色登录或切换角色。", "权限不足", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var comment = PromptDialog.Show(this, "审批通过", "请输入审批意见：", "确认受理，进入处理流程。");
        if (comment is null)
        {
            return;
        }

        ticket.ApprovalHistory.Add(new ApprovalRecord
        {
            Operator = CurrentUser(),
            Role = CurrentRole(),
            Action = "审批通过",
            Comment = comment
        });
        ticket.Status = "处理中";
        ticket.UpdatedAt = DateTime.Now;
        SelectComboValue(StatusBox, ticket.Status);
        Persist("审批通过，工单已进入处理中。");
        ShowDetails(ticket);
    }

    private void Reject_Click(object sender, RoutedEventArgs e)
    {
        if (TicketGrid.SelectedItem is not WorkTicket ticket)
        {
            SetStatus("请先选择一条工单。");
            return;
        }

        if (!CanApprove())
        {
            MessageBox.Show("当前角色没有退回权限。请使用“项目经理”角色登录或切换角色。", "权限不足", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var comment = PromptDialog.Show(this, "退回工单", "请输入退回原因：", "信息不完整，请补充账号、截图和操作路径。");
        if (comment is null)
        {
            return;
        }

        ticket.ApprovalHistory.Add(new ApprovalRecord
        {
            Operator = CurrentUser(),
            Role = CurrentRole(),
            Action = "退回",
            Comment = comment
        });
        ticket.Status = "已退回";
        ticket.UpdatedAt = DateTime.Now;
        SelectComboValue(StatusBox, ticket.Status);
        Persist("工单已退回。");
        ShowDetails(ticket);
    }

    private void Attach_Click(object sender, RoutedEventArgs e)
    {
        if (TicketGrid.SelectedItem is not WorkTicket ticket)
        {
            SetStatus("请先选择一条工单。");
            return;
        }

        if (!CanAttach())
        {
            MessageBox.Show("当前角色没有上传附件权限。开发、实施、项目经理可以上传附件。", "权限不足", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dialog = new OpenFileDialog
        {
            Title = "选择附件或截图",
            Filter = "常用附件|*.png;*.jpg;*.jpeg;*.pdf;*.doc;*.docx;*.xlsx;*.txt;*.log|所有文件|*.*"
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        var attachment = _attachmentService.CopyIntoStore(dialog.FileName, ticket.Id, CurrentUser());
        ticket.Attachments.Add(attachment);
        ticket.UpdatedAt = DateTime.Now;
        Persist($"已添加附件：{attachment.FileName}");
        ShowDetails(ticket);
    }

    private void AddDeployLog_Click(object sender, RoutedEventArgs e)
    {
        if (TicketGrid.SelectedItem is not WorkTicket ticket)
        {
            SetStatus("请先选择一条工单。");
            return;
        }

        if (!CanWriteDeploymentLog())
        {
            MessageBox.Show("当前角色没有写部署日志权限。请使用“运维”或“项目经理”角色。", "权限不足", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var content = PromptDialog.Show(this, "新增部署/现场日志", "请输入部署、排查或客户现场处理记录：", "生产环境已检查服务状态，待继续查看应用日志。");
        if (content is null)
        {
            return;
        }

        ticket.DeploymentLogs.Add(new DeploymentLog
        {
            Operator = CurrentUser(),
            Environment = ticket.Priority == "高" ? "生产" : "测试/客户现场",
            Content = content
        });
        ticket.UpdatedAt = DateTime.Now;
        Persist("部署/现场日志已记录。");
        ShowDetails(ticket);
    }

    private void TicketGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TicketGrid.SelectedItem is not WorkTicket ticket)
        {
            return;
        }

        TitleBox.Text = ticket.Title;
        RequesterBox.Text = ticket.Requester;
        DescriptionBox.Text = ticket.Description;
        SelectComboValue(DepartmentBox, ticket.Department);
        SelectComboValue(StatusBox, ticket.Status);
        ShowAnalysis(ticket);
        ShowDetails(ticket);
    }

    private void Filter_Changed(object sender, EventArgs e)
    {
        if (_view is null)
        {
            return;
        }

        _view?.Refresh();
        UpdateMetrics();
    }

    private bool FilterTicket(object item)
    {
        if (item is not WorkTicket ticket)
        {
            return false;
        }

        var query = SearchBox?.Text.Trim() ?? "";
        var priority = SelectedComboText(PriorityFilter);
        var status = SelectedComboText(StatusFilter);
        var queryMatched = string.IsNullOrWhiteSpace(query)
            || ticket.Title.Contains(query, StringComparison.OrdinalIgnoreCase)
            || ticket.Department.Contains(query, StringComparison.OrdinalIgnoreCase)
            || ticket.Description.Contains(query, StringComparison.OrdinalIgnoreCase)
            || ticket.Category.Contains(query, StringComparison.OrdinalIgnoreCase);

        var priorityMatched = priority is "" or "全部优先级" || ticket.Priority == priority;
        var statusMatched = status is "" or "全部状态" || ticket.Status == status;
        return queryMatched && priorityMatched && statusMatched;
    }

    private void ApplyAi(WorkTicket ticket)
    {
        var analysis = _agent.Analyze(ticket.Title, ticket.Description, ticket.Department);
        ticket.Category = analysis.Category;
        ticket.Priority = analysis.Priority;
        ticket.SuggestedOwner = analysis.SuggestedOwner;
        ticket.AiReason = analysis.Reason;
        ticket.HandlingAdvice = analysis.Advice;
        ticket.Confidence = analysis.Confidence;
        ticket.DueAt = CalculateDueAt(ticket.Priority, ticket.CreatedAt);
        ticket.UpdatedAt = DateTime.Now;
    }

    private void UpdateSelectedStatus(string status)
    {
        if (TicketGrid.SelectedItem is not WorkTicket ticket)
        {
            SetStatus("请先选择一条工单。");
            return;
        }

        ticket.Status = status;
        ticket.UpdatedAt = DateTime.Now;
        ticket.ApprovalHistory.Add(new ApprovalRecord
        {
            Operator = CurrentUser(),
            Role = CurrentRole(),
            Action = $"状态变更为{status}",
            Comment = "通过快捷状态按钮更新。"
        });
        SelectComboValue(StatusBox, status);
        _view.Refresh();
        Persist($"工单已标记为：{status}");
        ShowDetails(ticket);
    }

    private bool ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(TitleBox.Text) || string.IsNullOrWhiteSpace(DescriptionBox.Text))
        {
            MessageBox.Show("标题和问题描述不能为空。", "输入不完整", MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }

        return true;
    }

    private void Persist(string message)
    {
        _repository.Save(_tickets.OrderBy(ticket => ticket.Id));
        _view.Refresh();
        UpdateMetrics();
        SetStatus(message);
    }

    private void UpdateMetrics()
    {
        if (_tickets is null)
        {
            return;
        }

        TotalCountText.Text = _tickets.Count.ToString();
        HighCountText.Text = _tickets.Count(ticket => ticket.Priority == "高").ToString();
        TodoCountText.Text = _tickets.Count(ticket => ticket.Status == "待处理").ToString();
        DoneCountText.Text = _tickets.Count(ticket => ticket.Status == "已完成").ToString();
    }

    private void ShowAnalysis(WorkTicket ticket)
    {
        AiResultText.Text =
            $"分类：{ticket.Category}\n" +
            $"优先级：{ticket.Priority}\n" +
            $"建议负责人：{ticket.SuggestedOwner}\n" +
            $"置信度：{ticket.Confidence:P0}\n\n" +
            $"SLA截止：{ticket.DueAt:yyyy-MM-dd HH:mm}（{ticket.SlaStatus}）\n\n" +
            $"原因：{ticket.AiReason}\n\n" +
            $"建议：{ticket.HandlingAdvice}";
    }

    private void ShowDetails(WorkTicket ticket)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"工单 #{ticket.Id} 追踪详情");
        builder.AppendLine($"最后更新：{ticket.UpdatedAt:yyyy-MM-dd HH:mm:ss}");
        builder.AppendLine($"SLA：{ticket.DueAt:yyyy-MM-dd HH:mm} / {ticket.SlaStatus}");
        builder.AppendLine();

        builder.AppendLine("审批/流转记录：");
        if (ticket.ApprovalHistory.Count == 0)
        {
            builder.AppendLine("- 暂无审批记录");
        }
        else
        {
            foreach (var record in ticket.ApprovalHistory.OrderByDescending(item => item.Time).Take(6))
            {
                builder.AppendLine($"- {record.Time:MM-dd HH:mm} [{record.Role}] {record.Operator}：{record.Action}；{record.Comment}");
            }
        }

        builder.AppendLine();
        builder.AppendLine("附件/截图：");
        if (ticket.Attachments.Count == 0)
        {
            builder.AppendLine("- 暂无附件");
        }
        else
        {
            foreach (var attachment in ticket.Attachments.OrderByDescending(item => item.UploadedAt).Take(4))
            {
                builder.AppendLine($"- {attachment.UploadedAt:MM-dd HH:mm} {attachment.FileName}（{attachment.UploadedBy}）");
            }
        }

        builder.AppendLine();
        builder.AppendLine("部署/客户现场日志：");
        if (ticket.DeploymentLogs.Count == 0)
        {
            builder.AppendLine("- 暂无部署日志");
        }
        else
        {
            foreach (var log in ticket.DeploymentLogs.OrderByDescending(item => item.Time).Take(4))
            {
                builder.AppendLine($"- {log.Time:MM-dd HH:mm} [{log.Environment}] {log.Operator}：{log.Content}");
            }
        }

        DetailText.Text = builder.ToString();
    }

    private void ClearForm()
    {
        TitleBox.Clear();
        RequesterBox.Clear();
        DescriptionBox.Clear();
        DepartmentBox.SelectedIndex = 0;
        StatusBox.SelectedIndex = 0;
        AiResultText.Text = "选择工单后点击 AI 分析。";
        DetailText.Text = "选择工单后查看审批记录、附件和部署日志。";
    }

    private void SetStatus(string message)
    {
        StatusText.Text = $"{DateTime.Now:HH:mm:ss}  {message}";
    }

    private static string SelectedComboText(ComboBox comboBox)
    {
        return (comboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
    }

    private string CurrentUser()
    {
        return SelectedComboText(UserBox) is { Length: > 0 } value ? value : "当前用户";
    }

    private string CurrentRole()
    {
        return SelectedComboText(RoleBox) is { Length: > 0 } value ? value : "开发";
    }

    private bool CanApprove()
    {
        return CurrentRole() == "项目经理";
    }

    private bool CanAttach()
    {
        return CurrentRole() is "开发" or "实施" or "项目经理";
    }

    private bool CanWriteDeploymentLog()
    {
        return CurrentRole() is "运维" or "项目经理";
    }

    public static DateTime CalculateDueAt(string priority, DateTime createdAt)
    {
        return priority switch
        {
            "高" => createdAt.AddHours(4),
            "中" => createdAt.AddDays(1),
            _ => createdAt.AddDays(3)
        };
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
    }
}
