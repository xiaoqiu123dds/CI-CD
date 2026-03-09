# B 方案：Prism + ReactiveUI 深度集成研究仓库 / Option B: Prism + ReactiveUI Deep Integration Study Repository

## 仓库目标 / Repository Goal

- 本仓库当前用于沉淀 `Prism + ReactiveUI` 在 **单项目模块化结构** 下的深度集成设计，并提供可编译运行的示例项目。 This repository captures the deep integration design of `Prism + ReactiveUI` under a **single-project modular structure** and now also provides a compilable sample project.
- 当前目标是解决“继承关系像拼凑、生命周期边界不清、导航与对话职责混杂、事件总线使用失衡”等问题。 The current goal is to solve issues such as “inheritance that feels stitched together, unclear lifecycle boundaries, mixed responsibilities between navigation and dialogs, and unbalanced event bus usage”.
- 本次输出以 `B` 方案为准：**ReactiveUI + Prism**、**无数据持久化**、**无额外扩展功能**。 This output follows option `B`: **ReactiveUI + Prism**, **no persistence**, and **no extra extensions**.

## 开发环境 / Development Environment

- 操作系统：`Windows 10/11`。 Operating system: `Windows 10/11`.
- SDK：`.NET 8 SDK` 或更高的 Windows Desktop SDK。 SDK: `.NET 8 SDK` or a newer Windows Desktop SDK.
- IDE：`Visual Studio 2022`（建议 17.10 或更高版本）。 IDE: `Visual Studio 2022` (17.10 or later is recommended).
- UI 技术栈：`WPF`。 UI stack: `WPF`.
- 目标 NuGet 包：`Prism.Wpf`、`Prism.DryIoc`、`ReactiveUI`、`ReactiveUI.WPF`、`ReactiveUI.Fody`。 Target NuGet packages: `Prism.Wpf`, `Prism.DryIoc`, `ReactiveUI`, `ReactiveUI.WPF`, and `ReactiveUI.Fody`.

## 目标架构 / Target Architecture

- `Prism` 负责 **模块化、区域、导航、对话服务、应用组合边界**。 `Prism` owns **modularity, regions, navigation, dialog service, and application composition boundaries**.
- `ReactiveUI` 负责 **状态流、响应式命令、激活、强绑定、交互、调度**。 `ReactiveUI` owns **state flow, reactive commands, activation, strong bindings, interactions, and scheduling**.
- 本方案以 `PrismApplication` 作为唯一组合根，避免容器职责和应用生命周期管理出现双轨。 This solution uses `PrismApplication` as the single composition root, avoiding split responsibilities across containers and application lifetime management.
- 导航以 `Prism Region Navigation` 为唯一主线，不与 `ReactiveUI RoutingState` 双轨并存。 Navigation uses `Prism Region Navigation` as the single authoritative model and does not run in parallel with `ReactiveUI RoutingState`.

## 文档索引 / Documentation Index

- 深度说明：`docs/prism-reactiveui-deep-integration.md`。 Deep explanation: `docs/prism-reactiveui-deep-integration.md`.
- 构建计划：`docs/implementation-plan-b.md`。 Build plan: `docs/implementation-plan-b.md`.
- 示例覆盖映射：`docs/example-project-coverage.md`。 Example coverage map: `docs/example-project-coverage.md`.
- 任务记录：`Issues.md`。 Task log: `Issues.md`.

## 示例项目 / Sample Project

- 解决方案：`PrismReactiveDemo.Sample.sln`。 Solution: `PrismReactiveDemo.Sample.sln`.
- 项目：`src/PrismReactiveDemo/PrismReactiveDemo.csproj`。 Project: `src/PrismReactiveDemo/PrismReactiveDemo.csproj`.
- 代码结构严格对齐主文档，包含 `Shell`、`Dashboard`、`Editor`、对话框、事件桥接、异常出口、基于命令执行流的忙碌态聚合，以及设计时数据示例。 The code structure follows the main guide closely and includes `Shell`, `Dashboard`, `Editor`, dialogs, event bridging, exception exits, busy-state aggregation based on command execution streams, and design-time data examples.

## 调试与运行指南 / Debug and Run Guide

- 命令行构建：`dotnet build PrismReactiveDemo.Sample.sln`。 Command-line build: `dotnet build PrismReactiveDemo.Sample.sln`.
- 命令行测试：`dotnet test PrismReactiveDemo.Sample.sln -c Release`。 Command-line test: `dotnet test PrismReactiveDemo.Sample.sln -c Release`.
- 命令行运行：`dotnet run --project src/PrismReactiveDemo/PrismReactiveDemo.csproj`。 Command-line run: `dotnet run --project src/PrismReactiveDemo/PrismReactiveDemo.csproj`.
- 命令行打包：`powershell.exe -ExecutionPolicy Bypass -File scripts/package_release.ps1 -Version v0.1.0-local`。 Command-line packaging: `powershell.exe -ExecutionPolicy Bypass -File scripts/package_release.ps1 -Version v0.1.0-local`.
- 推荐演示顺序：先进入 `Dashboard`，再进入 `Editor`，修改文本后切回 `Dashboard`，最后再尝试输入 `error` 后点击保存。 Recommended demo flow: open `Dashboard`, then `Editor`, modify text and navigate back to `Dashboard`, then enter `error` and click save.
- 详细覆盖项请参考 `docs/example-project-coverage.md`。 For detailed coverage, see `docs/example-project-coverage.md`.

## CI/CD / CI/CD

- 默认平台：`GitHub Actions`。 Default platform: `GitHub Actions`.
- CI 工作流：`.github/workflows/ci.yml`，在 `Push / Pull Request / 手动触发` 时执行编码检查、`Release` 构建、单元测试和桌面包产出。 CI workflow: `.github/workflows/ci.yml`, which runs encoding checks, `Release` builds, unit tests, and desktop package generation on `push`, `pull request`, and manual runs.
- CD 工作流：`.github/workflows/release.yml`，在 `v*` Tag 或手动输入版本号时执行质量门禁、打包 `win-x64` 发布物，并自动附加到 GitHub Release。 CD workflow: `.github/workflows/release.yml`, which runs quality gates, packages the `win-x64` release artifact, and attaches it to a GitHub Release on `v*` tags or manual version input.
- 打包脚本：`scripts/package_release.ps1`，统一本地与 CI 的发布目录、压缩包和 `SHA256` 文件生成方式。 Packaging script: `scripts/package_release.ps1`, which standardizes local and CI release output, archive creation, and `SHA256` generation.
- 详细实施顺序：`docs/ci-cd-plan.md`。 Detailed execution plan: `docs/ci-cd-plan.md`.

## 编码检查 / Encoding Gate

- 提交前执行：`powershell.exe -ExecutionPolicy Bypass -File scripts/check_encoding.ps1 -Root .`。 Run before commit: `powershell.exe -ExecutionPolicy Bypass -File scripts/check_encoding.ps1 -Root .`.
- 检查范围：`.cs/.csproj/.sln/.config/.json/.xml/.xaml/.md/.txt/.ps1`。 Checked extensions: `.cs/.csproj/.sln/.config/.json/.xml/.xaml/.md/.txt/.ps1`.
- 失败条件：非 UTF-8、检测到 `U+FFFD`、或检测到疑似二进制/UTF-16 文本痕迹。 Failure conditions: non-UTF-8 content, detected `U+FFFD`, or suspicious binary/UTF-16 text traces.
