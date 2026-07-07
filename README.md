# SmartOpsDesk - OA智能工单分派助手

SmartOpsDesk 是一个用 .NET/C# 写的 Windows 桌面软件。它模拟企业里常见的 OA、教务、人事、资产管理系统的“客户问题处理流程”。

简单说：客户或老师反馈一个问题，系统把问题登记成工单，然后自动判断这个问题属于哪一类、急不急、应该交给谁处理，还能记录审批、附件截图、部署日志和处理状态。

## 这个项目解决什么问题

在真实公司里，客户经常会反馈类似问题：

- OA 系统打不开
- 教务审批流程卡住
- 人事报表导出失败
- 资产管理系统查询很慢
- 服务器上线后接口超时

如果只靠人工记录，很容易漏信息、分派慢、追踪不清楚。  
这个项目就是做一个小型“工单分派 + 项目交付跟进”工具。

## 技术栈，用小白也能懂的话解释

### .NET 8

.NET 是微软的开发平台，可以用来写桌面软件、网站、接口服务、后台程序。  
这个项目用的是 .NET 8，属于比较新的长期支持版本。

### C#

C# 是 .NET 里最常用的编程语言。  
这个项目的业务逻辑，比如新增工单、AI 判断、保存数据、导出 CSV，都是用 C# 写的。

### WPF

WPF 是 .NET 里的 Windows 桌面界面技术。  
它可以做出真正的 Windows 软件窗口，而不是网页。这个项目的登录页、表格、按钮、输入框、弹窗都是 WPF 做的。

### SQL Server

SQL Server 是微软的关系型数据库，很多 .NET 公司都会用它保存业务数据。  
这个项目已经支持 SQL Server：只要配置连接字符串，程序会自动建表并把工单保存到 SQL Server。

如果没有 SQL Server，程序会自动使用本地 JSON 文件保存数据，方便演示。

### AI Agent

这里的 AI Agent 可以理解成“智能分派助手”。  
它会根据工单标题和描述里的关键词，自动判断：

- 问题分类：账号权限、流程审批、数据报表、系统部署、页面体验
- 优先级：高、中、低
- 建议负责人：权限组、流程组、报表组、运维组等
- 处理建议：下一步该查什么、补什么信息

项目还预留了真实大模型接口。如果配置 API 地址和 Key，就可以调用真正的大模型来分析工单。

### CI/CD

CI/CD 可以理解成“代码上传后自动检查和打包”。  
这个项目配置了 GitHub Actions：代码推到 GitHub 后，会自动：

1. 下载 .NET 环境
2. 还原依赖包
3. 编译项目
4. 运行测试
5. 打包 Windows 版本
6. 上传打包结果

这能说明项目不是只在本机能跑，而是有版本控制和自动化构建意识。

## 已实现功能

- 登录窗口
- 演示账号和角色权限
- 工单新增、编辑、删除
- 工单搜索和筛选
- AI 自动分类
- AI 自动判断优先级
- AI 推荐负责人
- AI 生成处理建议
- SLA 超时提醒
- 审批通过、退回
- 附件/截图上传
- 部署日志和客户现场记录
- 状态流转：待处理、处理中、已完成、已挂起、已退回
- 本地 JSON 存储
- SQL Server 数据库存储
- CSV 导出
- 单元测试
- GitHub Actions 自动构建和发布

## 演示账号

密码都是：

```text
123456
```

| 账号 | 角色 | 可以演示什么 |
|---|---|---|
| dev | 开发 | 新增工单、上传附件、处理问题 |
| pm | 项目经理 | 审批通过、退回、写项目跟进记录 |
| impl | 实施 | 上传客户截图、补充现场信息 |
| ops | 运维 | 写部署日志、记录服务器排查过程 |

## 运行方式

进入项目目录：

```powershell
cd "C:\Users\Administrator\Documents\面试项目复盘\SmartOpsDesk"
```

运行：

```powershell
dotnet run
```

也可以双击：

```text
启动SmartOpsDesk.bat
```

## 使用 SQL Server 存储

如果你有 SQL Server 或免费的云数据库，只需要配置一个环境变量：

```powershell
$env:SMARTOPSDESK_SQLSERVER_CONNECTION = "Server=你的服务器;Database=SmartOpsDesk;User Id=用户名;Password=密码;TrustServerCertificate=True"
dotnet run
```

配置后，程序会自动创建这些表：

- `Tickets`：工单主表
- `ApprovalRecords`：审批流转记录
- `TicketAttachments`：附件记录
- `DeploymentLogs`：部署和现场日志

如果不配置这个环境变量，程序会自动使用本地 JSON 文件，不影响演示。

## 使用真实大模型分析

默认情况下，项目用本地规则分析，不需要联网。  
如果想接真实大模型，可以配置：

```powershell
$env:SMARTOPSDESK_LLM_ENDPOINT = "https://api.example.com/v1/chat/completions"
$env:SMARTOPSDESK_LLM_API_KEY = "你的API Key"
$env:SMARTOPSDESK_LLM_MODEL = "gpt-4o-mini"
dotnet run
```

未配置时点击“大模型分析”不会崩溃，会提示尚未配置接口。

## 测试和打包

运行测试：

```powershell
dotnet run --project tests\SmartOpsDesk.Tests\SmartOpsDesk.Tests.csproj
```

打包 Windows 版本：

```powershell
.\publish.ps1
```

打包结果会生成到：

```text
publish/
```

## 项目结构

```text
SmartOpsDesk/
  App.xaml                         程序入口
  LoginWindow.xaml                 登录窗口
  MainWindow.xaml                  主界面
  Models/
    WorkTicket.cs                  工单、审批、附件、部署日志实体
  Services/
    AuthService.cs                 登录账号和角色
    TicketAiAgent.cs               本地规则 AI 分析
    LargeModelAnalyzer.cs          真实大模型接口预留
    TicketStore.cs                 JSON 存储
    SqlServerTicketRepository.cs   SQL Server 存储
    CsvExporter.cs                 CSV 导出
  tests/
    SmartOpsDesk.Tests             冒烟测试
  .github/workflows/
    ci.yml                         GitHub 自动构建流程
```

## 面试时怎么介绍

可以这样说：

> 我知道贵公司做 OA、人事、教务、资产管理等管理信息化系统，所以我做了一个 .NET WPF 桌面端的智能工单分派助手。它模拟客户现场反馈问题后的处理流程：登录、登记工单、AI 分类、判断优先级、SLA 截止时间、审批流转、附件截图、部署日志、状态跟踪和导出。
>
> 这个项目不是单纯页面展示，而是覆盖了管理系统里常见的业务链路。存储层支持 SQL Server，也保留 JSON 兜底；AI 部分先用规则 Agent 跑通流程，也预留了真实大模型 API；GitHub Actions 可以自动构建、测试和打包。

## 被问到技术点时怎么答

### 为什么用 WPF？

WPF 可以做 Windows 桌面软件。很多企业内部工具、实施工具、运维工具不一定都是网页，所以我想用 WPF 补一个桌面端项目，体现 C#、事件处理、数据绑定和本地文件处理能力。

### 为什么支持 SQL Server？

.NET 公司里 SQL Server 很常见。这个项目把存储层抽成接口，既能用 JSON 演示，也能切到 SQL Server，后续更接近真实企业项目。

### AI 是怎么做的？

当前默认是规则型 AI Agent。比如标题里有“生产、打不开、全员”，优先级会更高，分类会偏系统部署。后续可以把 `LargeModelAnalyzer` 接到真实大模型，让模型返回更智能的分析结果。

### CI/CD 有什么用？

GitHub Actions 会在代码上传后自动编译、测试、打包。这样能减少“我本机能跑，别人电脑跑不了”的问题，也体现工程化意识。

