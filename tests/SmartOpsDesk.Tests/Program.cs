using SmartOpsDesk;
using SmartOpsDesk.Services;

var tests = new List<(string Name, Action Test)>
{
    ("AI should classify production login failure as high-priority deployment issue", TestHighPriorityDeployment),
    ("SLA should be 4 hours for high priority and 1 day for medium priority", TestSlaRules),
    ("Demo login should accept project manager account", TestLogin)
};

var failed = 0;
foreach (var (name, test) in tests)
{
    try
    {
        test();
        Console.WriteLine($"PASS {name}");
    }
    catch (Exception ex)
    {
        failed++;
        Console.WriteLine($"FAIL {name}: {ex.Message}");
    }
}

if (failed > 0)
{
    Environment.Exit(1);
}

static void TestHighPriorityDeployment()
{
    var agent = new TicketAiAgent();
    var result = agent.Analyze("OA 系统生产环境无法登录", "今天早上全员反馈打不开，疑似生产服务异常。", "办公室");
    Assert(result.Category == "系统部署", $"Expected 系统部署, got {result.Category}");
    Assert(result.Priority == "高", $"Expected 高, got {result.Priority}");
}

static void TestSlaRules()
{
    var createdAt = new DateTime(2026, 7, 7, 10, 0, 0);
    Assert(MainWindow.CalculateDueAt("高", createdAt) == createdAt.AddHours(4), "High priority SLA should be 4 hours.");
    Assert(MainWindow.CalculateDueAt("中", createdAt) == createdAt.AddDays(1), "Medium priority SLA should be 1 day.");
}

static void TestLogin()
{
    var auth = new AuthService();
    var user = auth.Login("pm", "123456");
    Assert(user is not null, "Project manager login should succeed.");
    Assert(user!.Role == "项目经理", $"Expected 项目经理, got {user.Role}");
}

static void Assert(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}
