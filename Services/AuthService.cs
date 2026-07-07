namespace SmartOpsDesk.Services;

public sealed record AppUser(string UserName, string Password, string DisplayName, string Role);

public sealed class AuthService
{
    private readonly List<AppUser> _users =
    [
        new("dev", "123456", "王文涛", "开发"),
        new("pm", "123456", "项目经理", "项目经理"),
        new("impl", "123456", "实施顾问", "实施"),
        new("ops", "123456", "运维交付", "运维")
    ];

    public IReadOnlyList<AppUser> Users => _users;

    public AppUser? Login(string userName, string password)
    {
        return _users.FirstOrDefault(user =>
            user.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)
            && user.Password == password);
    }
}
